# AGENTS.md — DevContext Engine worktree

You are in `C:\Code\DevContext2-engine` on branch `feat/engine-iteration`.
**Mission:** Deliver E2 (pattern-zoo), E5 (benchmark expansion), I8 (snapshot cache), I10.3 (server LRU),
A-F14/F15 (harder repos), and I9 (release readiness engine side).
You work in `src/DevContext.Core/**`, `tests/**`, `src/DevContext.Cli/**`, `src/DevContext.Server/**` —
**zero TypeScript changes needed.**

## Start here (every session)
1. Read this file — work items below are ordered by dependency.
2. `docs/dev/go-to-program/HANDOVER.md` — round-1/2/3 delivery summary + engine state section.
3. `docs/dev/go-to-program/PROGRESS-LOG.md` — last entry has unified-iteration-1 summary.
4. `docs/dev/go-to-program/ITERATION-I1-trust.md` — §I1.5 for E2 pattern-zoo.
5. `docs/dev/go-to-program/ITERATION-I7-benchmark-audit.md` — E5 benchmark expansion.
6. `docs/dev/go-to-program/ITERATION-I8-caching-storage.md` — I8 snapshot cache.
7. `docs/dev/go-to-program/ITERATION-I10-workspace-tabs.md` — I10.3 server LRU (depends on I8).
8. `docs/dev/go-to-program/ADDENDUM-A-harder-repos.md` — A-F14/F15 harder repos.
9. `docs/dev/go-to-program/ITERATION-I9-release-readiness.md` — I9 release readiness.

## Work items (ordered — each builds on the prior; I10.3 is blocked until I8 done)

### E2 — Pattern-zoo corpus  **DONE**
- Locus: `tests/fixtures/PatternZoo/PatternZoo/` (9 fixture files) + `tests/DevContext.Core.Tests/PatternZooTests.cs` (13 tests).
- Also shipped I1.3 (conjunction gate for bare-verb fallback) + I1.5 (string literal stripping in GraphBuilder).
- Gate: `PatternZooTests` green (13/0); all existing seam tests still green (369/0).

### E5 — Benchmark expansion
Clone 8 missing-archetype repos → register in `eval-repos.json` + `eval/expectations/` → capture Map+Trace.
- Locus: `eval-repos.json`, `eval/expectations/*.json`
- Archetypes: CLI, Worker, gRPC, Blazor, MAUI/Avalonia, classic MVC, serverless, 2nd library.
- Gate: `dotnet test --filter Category=Eval` green with new expectations.

### I8 — Caching & storage
Repo-hash snapshot cache → instant re-opens. Settings→Storage backend.
- Locus: new cache service in `src/DevContext.Core/Analysis/`, wire through DI.
- Gate: analyze same repo twice → second run near-instant from cache.

### I10.3 — Server MaxLiveSessions + LRU + rehydrate  **BLOCKED until I8 done**
- Locus: `src/DevContext.Server/Sessions/` — LRU eviction, rehydrate from I8 cache.
- Gate: open 7 tabs → oldest evicted; rehydrate from cache is instant.

### A-F14 — EF depth tracking  **DONE**
Entity relationship depth analysis (entity→aggregate root distance).
- Locus: `src/DevContext.Core/Graph/GraphBuilder.cs` — `AddEntityNavigationEdges()` + `ExtractInnerEntityNameWithDir()`
  `src/DevContext.Core/Graph/TraceBuilder.cs` — `AnnotateEntityDepths()`
  `src/DevContext.Core/Graph/CodeGraph.cs` — `EdgeKind.EntityRelation`
- Gate: entity navigation relationship tests green; TOUCHES annotated with chain depth.

### A-F15 — Build intelligence  **DONE**
CPM detection + Directory.Build.props fix (bug-grade: CPM packages not detected).
- Locus: `src/DevContext.Core/Resolvers/CsprojReader.cs` — `ResolveCpmVersions()`, `ParsePackageReferencesCpmAware()`, `ResolveOutputType()`, `ResolveTargetFrameworks()`, `ResolveIsPackable()` with ancestor-chain walking.
- Gate: CPM fixture project; `CsprojReaderCpmTests` (12 tests) green; all existing tests green (381/0).

### I9 — Release readiness (engine side)
CLI polish: exit codes, `--quiet`, stdout/stderr separation, completions.
- Locus: `src/DevContext.Cli/Program.cs`, `AnalyzeCommand.cs`
- Gate: `--strict` returns exit code 2 on invariant fail; `--quiet` prints nothing on success.

## Verify loop
```powershell
# From C:/Code/DevContext2-engine
dotnet build DevContext.slnx                             # 0 warnings (analyzer warnings = errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"   # must be green
powershell -File eval/gates.ps1                          # full gate (needs populated eval-repos/)
```

## Hard rules
- **No TypeScript changes** — you work in the engine. The UI agent handles desktop.
- **Reform in place; never rewrite extractors.**
- **Do-not-regress anchors:** `BudgetIndependenceTests`, `TraceQualityTests` sibling-divergence Facts,
  `GraphBuilderSpanTests`, `NoiseFilterTests`, `ArchetypeDetectorTests`.
- **Docs move with code, same commit.** Update `docs/product/cli-reference.md` for CLI changes.
- **One wire contract:** anything a face shows must exist as `GraphQuery` op / kernel JSON first.

### Before running eval/gates.ps1
`eval-repos/` in this worktree may be empty. Junction to the populated copy:
```powershell
New-Item -ItemType Junction -Path C:\Code\DevContext2-engine\eval-repos -Target C:\Code\DevContext2\eval-repos
```

## Resume protocol (cold start)
```
git -C C:/Code/DevContext2-engine checkout feat/engine-iteration
git -C C:/Code/DevContext2-engine pull

# Pre-flight
dotnet build C:/Code/DevContext2-engine/DevContext.slnx
dotnet test C:/Code/DevContext2-engine/DevContext.slnx --filter "Category!=Eval"

# Ensure eval-repos junction exists (see above)

# Pick the first work item whose Status != DONE in this file
# Do Step 0 (reproduce) first, then execute. Commit per item.
```
