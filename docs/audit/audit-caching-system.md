# Audit: Caching System

> v1.0.5-preview, commit `2c40662`. Based on source code review + live runs against eShop & DntSite.

## Cache Architecture — Three Tiers

```
┌─────────────────────────────────────────────────────────────────┐
│ Tier 3: SharedAnalysisContext.FileSyntaxNodes (in-memory)        │
│   ConcurrentDictionary<string, Lazy<Task<FileSyntaxNodes>>>      │
│   Per-analysis, shared across extractors                         │
│   NOT persisted cross-run                                        │
├─────────────────────────────────────────────────────────────────┤
│ Tier 2: PersistentAnalysisCache (Desktop cross-run)             │
│   ConcurrentDictionary<string, Entry> with mtime validation      │
│   One instance reused across desktop analyses                    │
│   Invalidates single entry on file change                        │
├─────────────────────────────────────────────────────────────────┤
│ Tier 1: AnalysisCache (per-run, CLI)                            │
│   Three ConcurrentDictionary<string, Lazy<Task<T>>> caches       │
│   New instance per run, destroyed after analysis completes       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tier 1: AnalysisCache (Per-Run)

**Used by**: CLI, Benchmarks, Tests

### What It Caches

| Cache | Key | Value | Thread-Safety |
|---|---|---|---|
| `_textCache` | file path | `Lazy<Task<string>>` | `ConcurrentDictionary.GetOrAdd` + `Lazy` |
| `_syntaxCache` | file path | `Lazy<Task<SyntaxTree>>` | Same, depends on `_textCache` |
| `_xmlCache` | file path | `Lazy<Task<XDocument>>` | Same, depends on `_textCache` |

### Design Properties

- **Parse-once guarantee**: `Lazy<Task<T>>` ensures that even under concurrent access from multiple extractors, each file is read and parsed exactly once. The first caller triggers the lazy; all subsequent callers await the same task.
- **Lock-free**: `ConcurrentDictionary.GetOrAdd` is lock-free for reads and uses fine-grained locking for writes. `Lazy<Task<T>>` with `ExecutionAndPublication` mode internally handles concurrent initialization with a single lock acquisition.
- **No invalidation**: Once a file is cached, it's never re-read within the same run. If a file changes on disk during analysis (unlikely), stale data is served.
- **Cancellation isolation**: `CancellationToken.None` is deliberately used inside the `Lazy` to prevent one caller's cancellation from poisoning the shared task.

### Hit/Miss Tracking

```csharp
// GetTextAsync
if (_textCache.TryGetValue(filePath, out var existing)) {
    Interlocked.Increment(ref _textHits);      // Hit
} else {
    Interlocked.Increment(ref _textMisses);    // Miss → create new Lazy
}
```

Stats reported via `ICacheStatsSource.GetStats()` → `CacheStats(TextHits, TextMisses, SyntaxHits, SyntaxMisses)`.

### Live Data: DntSite (first run, cold)

```
cache 0% hit · 1342 files · 0 projects
```
- 1,342 text reads: 0 hits, 1,342 misses (cold cache by definition)
- 1,342 syntax tree parses: 0 hits, 1,342 misses
- XML cache not used (no XML files in DntSite)

On a second run against the same clone, the numbers would flip to near-100% due to the OS filesystem cache, but the `AnalysisCache` instance itself starts empty each run.

---

## Tier 2: PersistentAnalysisCache (Cross-Run)

**Used by**: Desktop (`AnalysisService`)

### What Makes It Persistent

Instead of a simple `ConcurrentDictionary<string, Lazy<Task<T>>>`, each entry stores the file's **last-write time**:

```csharp
private sealed class Entry {
    public required DateTime Mtime { get; init; }
    public required Lazy<Task<string>> Text { get; init; }
    public required Lazy<Task<SyntaxTree>> Syntax { get; init; }
    public required Lazy<Task<XDocument>> Xml { get; init; }
}
```

### Invalidation Logic (mtime-based)

```csharp
private Entry GetEntry(string filePath) {
    var mtime = _fs.GetLastWriteTimeUtc(filePath);
    return _entries.AddOrUpdate(
        filePath,
        // Add: create new entry
        (path, state) => NewEntry(state.fs, path, state.mtime),
        // Update: keep existing entry if mtime unchanged, else rebuild
        (path, existing, state) =>
            existing.Mtime == state.mtime ? existing : NewEntry(state.fs, path, state.mtime),
        (fs: _fs, mtime));
}
```

**Key property**: When the user changes focus, depth, or any render-time option, unchanged files are NOT re-read or re-parsed. Only files that were actually edited on disk trigger a re-parse.

### Desktop Integration

`AnalysisService` creates **one** `PersistentAnalysisCache` instance for the lifetime of the desktop session:

```csharp
// AnalysisService constructor
_cache = new PersistentAnalysisCache(_fs);
```

This cache is reused across all analyses within a session. Combined with `AnalysisSnapshot` (analyze-once, render-many), the desktop user experience is:
1. **First analysis**: Full cost (read + parse all files)
2. **Option change** (focus, depth, detail, format): Near-instant re-render (milliseconds) — no re-analysis
3. **Re-analysis** (project path change, NoRoslyn toggle): Only re-parses files changed on disk

---

## Tier 3: SharedAnalysisContext (Per-Analysis Syntax Nodes)

### What It Caches

```csharp
public sealed class SharedAnalysisContext {
    public ConcurrentDictionary<string, Lazy<Task<FileSyntaxNodes>>> SyntaxCache { get; } = new();
}
```

`FileSyntaxNodes` contains pre-parsed syntax node collections:
- Class declarations
- Method declarations  
- Property declarations
- Field declarations
- Constructor declarations

### Who Uses It

`SyntaxStructureExtractor` and `DiRegistrationExtractor` share this cache during Stage 2. Since both extractors iterate all source files and need parsed class/method structures, sharing avoids double-parsing at the syntax level.

### Thread Safety

Same `Lazy<Task<>>` pattern as Tier 1 with `ConcurrentDictionary`. The shared cache is safe for parallel access during `Parallel.ForEachAsync` in Stage 2.

### Cross-Run Gap

This cache is NOT persisted across runs. The iteration-8 HANDOVER notes: "Next lever would be caching `FileSyntaxNodes` cross-run too (today only text+SyntaxTree persist on the desktop)."

---

## The "Analyze Once, Render Many" Contract

The cache system is one half of the story. The other half is `AnalysisSnapshot`:

```
CLI flow:
  AnalyzeAsync() → AnalysisSnapshot (immutable)
    ├─ DiscoveryModel (types, detections, call edges)
    ├─ CodeGraph (nodes, edges)
    ├─ MapModel (topology, entries, packages)
    ├─ EntryPoint[] (catalogued entry points)
    └─ RunReport (stage timings, cache stats)
  
  RenderAsync(snapshot, request) → RenderedContext
    ├─ Entry set? → TraceBuilder traversal → TraceRenderer
    ├─ No entry? → MapRenderer or LibrarySurfaceRenderer
    └─ (cheap: graph walk + string formatting, milliseconds)
```

### Desktop Interactive Re-render

```
User changes focus → VM.Focus = "POST /api/orders/"
  → Debouncer (500ms) fires
  → RerenderAsync(snapshot, request) 
  → ~0 parsing, ~0 IO, pure graph traversal
  → Output updates in the UI
```

No re-analysis, no re-reading of files, no re-parsing.

### Desktop Re-analysis Triggers

Only these options trigger a full re-analysis:
- `ProjectPath` change
- `NoRoslyn` toggle
- `DryRun` toggle
- `IncludeAntiPatterns` toggle

All other options (focus, depth, detail, format, max-tokens, sections, include-provenance, include-diagnostics) trigger only a re-render.

---

## Cache Hit Rates: Live Measurement

| Run | Type | Text Hit | Syntax Hit | Notes |
|---|---|---|---|---|
| DntSite Map (cold) | First run | 0% | 0% | New clone, fresh AnalysisCache |
| DntSite Trace (2nd) | Subsequent | 0% | 0% | New AnalysisCache per CLI run |
| eShop Map (cold) | First run | 0% | 0% | Same |
| eShop Trace (2nd) | Subsequent | 0% | 0% | Same |

**On CLI, cache is always 0% hit** because `AnalysisCache` is per-run. The Desktop's `PersistentAnalysisCache` would show near-100% on subsequent analyses with unchanged files.

---

## What Is NOT Cached

| Item | Status | Impact |
|---|---|---|
| `FileSyntaxNodes` cross-run | Not cached | Stage 2 re-parses even on Desktop re-analysis |
| `CodeGraph` (serialized) | Designed, not built | Full graph rebuild on every analysis |
| Roslyn `SemanticModel` | Created per-call, not cached | CallGraph bind recompiles every analysis |
| `Compilation` object | Not reused | Bind cost is per-run |
| Token estimates | Recalculated on every render | Cheap, not a bottleneck |

---

## Cache Performance Characteristics

### Where Time Is Spent (DntSite Map, 41.5s total)

| Phase | Time | Cache-Dependent? |
|---|---|---|
| Discovery & Cache Warmup | 167ms | File walk, register paths |
| Generic Extraction (Stage 2) | 8,218ms | **Heavily** — 1,342 text reads + syntax parses |
| Specific Extraction (Stage 3) | 32,342ms | Partially — CallGraph bind dominates (30,490ms) |
| Scoring | 35ms | No |
| Compression | 25ms | No |

**The Stage 2 floor (~8s for 1,342 files) is the dominant cache-dependent cost.** With PersistentAnalysisCache on Desktop, this drops to near-zero for re-analyses.

### Where Time Is Spent (eShop Map, 2.8s total)

| Phase | Time |
|---|---|
| Discovery | 75ms |
| Generic Extraction | 209ms |
| Specific Extraction | 2,233ms |
| Scoring + Compression | 65ms |

eShop is small (140 files) so Stage 2 is negligible. The CallGraphExtractor (2,121ms) dominates but benefits less from caching.

---

## Design Decisions & Trade-offs

### Decision: CancellationToken.None in Lazy
The shared `Lazy<Task<T>>` uses `CancellationToken.None` because the entry is shared across callers. A single caller's cancellation must not poison the cached task for everyone else. This is correct for the cache pattern — the alternative (per-caller cancellation tokens) would break the single-task guarantee.

### Decision: mtime-based invalidation vs. content hashing
`PersistentAnalysisCache` uses file mtime (last write time) for invalidation. Content hashing (SHA of file contents) would be more precise but requires reading the entire file to compute the hash, which defeats the purpose of a cache. Mtime is a single stat call and is sufficient assuming well-behaved editors that update it on save.

### Decision: CLI keeps per-run cache
The CLI intentionally uses `AnalysisCache` (not `PersistentAnalysisCache`) because there's no benefit to cross-run caching in a one-shot CLI invocation. The Desktop uses `PersistentAnalysisCache` because it benefits from retaining parsed data across user interactions.
