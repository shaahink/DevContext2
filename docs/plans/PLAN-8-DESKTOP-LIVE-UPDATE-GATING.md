# Plan 8 — Desktop live-update gating + snapshot cache + stale state

> Builds on PLAN-1 (`AnalysisSnapshot` = expensive analyze; cheap render-many) and PLAN-6
> (the `MainViewModel` decomposition into `OutputViewModel` / `SectionSelectionModel` /
> `CancellableOperation` / `Debouncer`). Read both before starting.
>
> **Problem.** The desktop "live update" auto-runs the *full pipeline* on analysis-affecting
> input changes. The worst offender is the focus field: `OnAroundChanged` →
> `OnAnalysisInputChanged` → `AnalyzeCommand.Execute(null)` (`MainViewModel.cs:213, 220-226`),
> and `Around` is bound `@bind:event="oninput"` (`ConfigPanel.razor:108`) — so **every
> keystroke cancels and restarts a full analysis**. Scenario / no-Roslyn / dry-run /
> anti-patterns toggles do the same. This is net-negative UX.
>
> **Goal.** Auto-update only the *cheap, snapshot-based* render tier. Make the *expensive*
> analysis tier explicit: on an analysis-affecting change, either serve a cached snapshot
> instantly (in-memory LRU, session-scoped) or mark the output **stale** and show an explicit
> **Re-analyze** button. No pipeline run ever fires without an explicit click (or a cache hit).
>
> **Decisions already made (do not relitigate):** trigger model = *stale banner + explicit
> run*; caching = *in-memory LRU, session only* (no disk persistence, no source-change
> invalidation).

---

## Ground rules for the executing agent

- Branch off `develop`: `git switch develop && git switch -c feature/desktop-live-update-gating`.
- Build after every phase: `dotnet build DevContext.sln`. Run desktop tests after phases 1, 3,
  4, 6: `dotnet test tests/DevContext.Desktop.Tests`.
- **Hosting model that matters for every change:** the UI is Blazor inside a `BlazorWebView`
  (`MainWindow.xaml`), *not* WPF XAML bindings. Components subscribe to `VM.PropertyChanged`
  and call `InvokeAsync(StateHasChanged)` — they ignore `e.PropertyName` (see
  `OutputPanel.razor:23-24`). The VM is a **singleton** (`MainWindow.xaml.cs:43`) constructed
  via its parameterless ctor (`IAnalysisService` is not registered), so it and its single
  `AnalysisService` live for the whole session — **a cache field on the VM is automatically
  session-scoped.**
- Razor comments are `@* *@`, never `<!-- -->`.
- The `__source__` sentinel in section checkboxes is not a real `SectionNames` entry; leave it
  alone.
- Before renaming/removing any public VM member, grep the solution **including `.razor`** files
  (they reference members as `VM.X`).
- Do not change CLI behavior, golden output, or the analyze→render split from PLAN-1.

### Precondition check (abort if it fails)

This plan assumes PLAN-6 has landed. Confirm `MainViewModel.cs` is the ~487-line version with
fields `_output` (`OutputViewModel`), `_sections` (`SectionSelectionModel`), `_analyzeOp` /
`_renderOp` / `_validateOp` (`CancellableOperation`), and `_tokenDebouncer` (`Debouncer`). If you
instead find a ~750-line god class with raw `CancellationTokenSource` fields, **stop** — PLAN-6
must land first.

---

## Phase 0 — Recon (read, don't edit)

Read in full:

- `src/DevContext.Desktop/ViewModels/MainViewModel.cs` — especially the change handlers
  (`:201-242`), `AnalyzeAsync` (`:253-375`), `RerenderAsync` (`:377-426`), and `Dispose` (`:479`).
- `src/DevContext.Desktop/Services/AnalysisService.cs` — `AnalyzeAsync` (`:57`), the section
  filtering on the resolved scenario (`:89-97`), `RenderAsync` (`:171`), and the `AnalysisOptions`
  record (`:296-310`).
- `src/DevContext.Desktop/ViewModels/SectionSelectionModel.cs` — `ApplyScenarioSectionDefaults`,
  `ResetToDefaults`, `GetActiveSections`.
- `src/DevContext.Desktop/Components/OutputPanel.razor` and `ConfigPanel.razor` — every binding
  and the `VM.PropertyChanged → StateHasChanged` subscription.
- `tests/DevContext.Desktop.Tests` — how the VM is tested with a fake `IAnalysisService`.

**Input-tier classification (this is the contract — wire every handler to match):**

| Input | Current handler (`MainViewModel.cs`) | Tier | Target routing |
|---|---|---|---|
| Format | `OnSelectedFormatChanged` → render (`:210`) | **Render** | unchanged |
| Section toggle | `SetSectionEnabled` → render (`:162-166`) | **Render** | unchanged |
| Max tokens | `OnMaxTokensChanged` → debounced render (`:212`) | **Render** | unchanged (optional: rename `DebouncedReanalyze`→`DebouncedRerender`) |
| Provenance | `OnIncludeProvenanceChanged` → render (`:214`) | **Render** | unchanged |
| Diagnostics | `OnIncludeDiagnosticsChanged` → render (`:215`) | **Render** | unchanged |
| **Focus / Around** | `OnAroundChanged` → **analyze** (`:213`) | **Analysis** | → `MarkAnalysisInputsChanged()` |
| **Scenario** | `OnSelectedScenarioChanged` → reset + **analyze** (`:202-209`) | **Analysis** | reset defaults, then → `MarkAnalysisInputsChanged()` |
| **No-Roslyn** | `OnNoRoslynChanged` → **analyze** (`:216`) | **Analysis** | → `MarkAnalysisInputsChanged()` |
| **Dry-run** | `OnDryRunChanged` → **analyze** (`:217`) | **Analysis** | → `MarkAnalysisInputsChanged()` |
| **Anti-patterns** | `OnIncludeAntiPatternsChanged` → **analyze** (`:218`) | **Analysis** | → `MarkAnalysisInputsChanged()` |

The five "Analysis" rows are exactly today's `OnAnalysisInputChanged` callers. The whole job is:
**delete the auto-`AnalyzeCommand.Execute` for those, and replace with cache-probe-or-stale.**

---

## Phase 1 — `SnapshotCache` + `AnalysisKey`

**Intent:** a bounded, session-scoped LRU mapping analysis inputs → produced snapshot.

New file `src/DevContext.Desktop/ViewModels/SnapshotCache.cs`:

```csharp
using DevContext.Core.Models;

namespace DevContext.Desktop.ViewModels;

/// <summary>Identity of an analysis run — the inputs that change the produced snapshot.
/// Excludes render-only params (max tokens, format, sections, provenance, diagnostics).</summary>
public sealed record AnalysisKey(
    string ProjectPath, string Scenario, string Focus,
    bool NoRoslyn, bool DryRun, bool IncludeAntiPatterns);

/// <summary>In-memory LRU of analysis snapshots, keyed by AnalysisKey. Session-scoped
/// (the owning MainViewModel is a DI singleton). Not thread-safe; call from the UI thread.</summary>
public sealed class SnapshotCache(int capacity = 8)
{
    private readonly LinkedList<(AnalysisKey Key, AnalysisSnapshot Snap)> _lru = new();
    private readonly Dictionary<AnalysisKey, LinkedListNode<(AnalysisKey, AnalysisSnapshot)>> _map = new();

    public bool TryGet(AnalysisKey key, out AnalysisSnapshot snapshot)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _lru.Remove(node);
            _lru.AddFirst(node);          // move-to-front (most-recently-used)
            snapshot = node.Value.Item2;
            return true;
        }
        snapshot = null!;
        return false;
    }

    public void Set(AnalysisKey key, AnalysisSnapshot snapshot)
    {
        if (_map.TryGetValue(key, out var existing)) { _lru.Remove(existing); _map.Remove(key); }
        var node = _lru.AddFirst((key, snapshot));
        _map[key] = node;
        while (_map.Count > capacity)
        {
            var last = _lru.Last!;
            _lru.RemoveLast();
            _map.Remove(last.Value.Item1);
        }
    }

    public void Clear() { _lru.Clear(); _map.Clear(); }
}
```

Notes:
- The key uses the **raw `ProjectPath`** (URL or local path as typed), *not* the resolved clone
  dir, so the cache can be probed **before** cloning a GitHub repo.
- `MaxTokens`, format, sections, provenance, diagnostics are **excluded** — they are render
  params and must not split the cache.

**Acceptance:** unit tests in `tests/DevContext.Desktop.Tests` covering miss→set→hit,
move-to-front recency, and eviction of the least-recently-used entry at `capacity + 1`.

---

## Phase 2 — VM state: analysis key + `IsStale`

**Intent:** the VM tracks which snapshot is currently displayed and whether the form has drifted.

In `MainViewModel`:

```csharp
private readonly SnapshotCache _cache = new(capacity: 8);
private AnalysisKey? _displayedKey;   // key of the snapshot currently shown (null when no output)

private AnalysisKey BuildAnalysisKey() =>
    new(ProjectPath, SelectedScenario.Value, Around, NoRoslyn, DryRun, IncludeAntiPatterns);

public bool IsStale =>
    HasOutput && _displayedKey is not null && _displayedKey != BuildAnalysisKey();
```

Add `_cache.Clear();` to `Dispose()` (`MainViewModel.cs:479`).

---

## Phase 3 — Replace auto-analyze with cache-probe-or-stale

**Intent:** an analysis-affecting change applies a cached snapshot instantly, or goes stale —
never auto-runs the pipeline.

```csharp
private void MarkAnalysisInputsChanged()
{
    if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
        return;

    var key = BuildAnalysisKey();
    if (_cache.TryGet(key, out var snapshot))   // already analyzed → instant
    {
        _snapshot = snapshot;
        _displayedKey = key;
        _ = RerenderAsync();                     // cheap; also reflects new section defaults
    }
    // miss → leave _displayedKey unchanged so IsStale evaluates true

    OnPropertyChanged(nameof(IsStale));
    OnPropertyChanged(nameof(AnalyzeButtonText));
}
```

Rewire the five analysis handlers (`MainViewModel.cs:202-218`) to call this instead of
`OnAnalysisInputChanged`. For scenario, keep the existing `ResetToScenarioDefaults()` /
section-default reset first, then call `MarkAnalysisInputsChanged()`:

```csharp
partial void OnSelectedScenarioChanged(ScenarioItem value)
{
    if (_isInitializing) return;
    _sections.SelectedScenarioValue = value.Value;
    OnPropertyChanged(nameof(IsTraceMode));
    ResetToScenarioDefaults();
    MarkAnalysisInputsChanged();   // was OnAnalysisInputChanged()
}
```

Delete the now-unused `OnAnalysisInputChanged()` (`:220-226`) once no caller remains (grep
first). Leave `OnRenderInputChanged()` and `DebouncedReanalyze()` untouched — render tier is
unchanged (optionally rename `DebouncedReanalyze`→`DebouncedRerender`; it never analyzed).

---

## Phase 4 — Make `AnalyzeAsync` cache-aware (the explicit run)

**Intent:** the Analyze button (and the stale banner's Re-analyze button) is the only thing that
runs the pipeline — and even it skips the pipeline + clone on a cache hit.

In `AnalyzeAsync` (`MainViewModel.cs:253`):

1. Capture the key **before** cloning: `var key = BuildAnalysisKey();`.
2. **Cache-hit fast path** (insert near the top, after setting `IsAnalyzing`): if
   `_cache.TryGet(key, out var cached)`, set `_snapshot = cached`, `_displayedKey = key`,
   `await RerenderAsync(ct)`, `_output.HasOutput = true`, clear `IsAnalyzing` /
   `IsProgressVisible`, `OnPropertyChanged(nameof(IsStale))`, and `return`. **Do not clone, do
   not call `_svc.AnalyzeAsync`.** (`RerenderAsync` already restores `StatsHtml`/`StatsText`
   from `_snapshot.Report`.)
3. **Miss path** = the existing clone + `_svc.AnalyzeAsync` body, unchanged, plus on success
   (`:322`): `_cache.Set(key, _snapshot); _displayedKey = key;` and, after the stats block,
   `OnPropertyChanged(nameof(IsStale));`.

The Razor stale banner invokes `VM.AnalyzeCommand` (same command), so first-run and re-analyze
share one code path.

---

## Phase 5 — UI: stale banner + Re-analyze button

**Intent:** make staleness visible and the re-run explicit.

In `Components/OutputPanel.razor`, immediately above the output toolbar (`:101`):

```razor
@if (VM.IsStale)
{
    <div class="stale-banner">
        <span>Inputs changed — output is out of date.</span>
        <button class="btn-text" @onclick="() => VM.AnalyzeCommand.Execute(null)">Re-analyze</button>
    </div>
}
```

Components already re-render on any `VM.PropertyChanged`, and Phases 3–4 raise `IsStale`, so the
banner toggles with no extra wiring. Add a `.stale-banner` style to the project stylesheet
(locate it under `wwwroot`; match the existing toolbar/banner look — a thin amber strip).

**Optional (recommended) calmer focus UX:** change `ConfigPanel.razor:108` from
`@bind:event="oninput"` to the default (`onchange`, fires on blur / Enter), so `IsStale` flips
once per committed edit instead of per keystroke. Low risk; defer if it complicates section/test
expectations.

---

## Phase 6 — Tests + per-mode verification

**Unit (VM with a fake `IAnalysisService` that counts calls):**
- Changing an **analysis** input (e.g. `Around`) when output exists and the key is uncached
  does **not** call `AnalyzeAsync`, and `IsStale == true`.
- Changing an analysis input to a **previously-cached** key applies instantly: `RenderAsync`
  called, `AnalyzeAsync` **not** called again, `IsStale == false`.
- Changing a **render** input (format / provenance / max tokens) calls `RenderAsync`, never
  `AnalyzeAsync`, and `IsStale` stays `false`.
- `AnalyzeCommand` on a cached key serves from cache (no `AnalyzeAsync` call); on a miss it
  calls `AnalyzeAsync` exactly once and caches the result.

**Per-mode behavior matrix (document + verify):**

| Mode (`SelectedScenario.Value`) | Focus role | Default sections (from `SectionSelectionModel`) | Expected output emphasis |
|---|---|---|---|
| `overview` | broad / optional | architecture, endpoints, data model, related types | architecture-first overview |
| `deep-dive` ("Trace") | central entry point | call graph **on**, architecture **off** | focused entry + call graph |

**Manual gate (G-desktop)** — `dotnet run --project src/DevContext.Desktop`, watch the log for
pipeline stage text (`"Discovering files..."`, `"Extracting structure..."`):

1. Analyze a known project (e.g. `eval-repos/eShop`) in Overview → renders once.
2. Toggle a section / change format / move the token slider → output updates instantly, **no**
   pipeline stage in the log, **no** stale banner.
3. Type in the focus field → stale banner appears, **no** pipeline run while typing.
4. Click **Re-analyze** → exactly **one** pipeline run; banner clears; output reflects focus.
5. Switch Overview → Trace (first time) → stale banner (cache miss); click → runs once; output
   matches the Trace row above (call graph present, architecture absent).
6. Switch Trace → Overview → **instant** from cache: no pipeline run, no banner, the prior
   Overview output is restored.
7. Re-enter a focus string you already analyzed → instant from cache (no pipeline run).

Pass = steps 2/6/7 never reprint a pipeline stage; steps 4/5 print exactly one; the banner
appears only when stale.

---

## Out of scope / assumptions

- **Render-tier classification is taken as correct per the current code.** Provenance,
  diagnostics, and max-tokens flow through `RenderRequest` in `RerenderAsync`, so toggling them
  re-renders without re-analysis. Gate step 2 confirms this; if toggling one of them actually
  requires re-analysis, that's a pre-existing defect — **flag it, don't fix it here.**
- **No disk persistence and no source-change invalidation.** Re-running the same key serves the
  cached snapshot even if files changed on disk. A "force refresh" affordance is a deliberate
  follow-up, not part of this plan.
- Cache `capacity = 8` is a starting value; snapshots can be large, so tune if memory is a
  concern. Do not make it unbounded.
- No CLI changes. The analyze→render split from PLAN-1 is the foundation, not a target.
