# AGENTS.md — DevContext Desktop UI worktree

You are in `C:\Code\DevContext2-ui` on branch `feat/ui-iteration`.
**Mission:** Build the live console, synced lens, facet views, and release polish for the desktop app.
You work ONLY in `src/DevContext.App/` — zero C# changes needed.

## Start here (every session)
1. Read this file — the work items below are ordered.
2. `docs/dev/PLAN-DESKTOP-V3.md` — the spec for P2 Live Console and P3 Synced Lens.
3. `docs/dev/go-to-program/UI-UX-GUIDELINES.md` — design contract for all UI work.
4. `docs/dev/go-to-program/ITERATION-I5-facet-menu.md` — facet specs (for U3).
5. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.

## Work items (do NOT skip order — each builds on the prior)

### U1 — Live Console (V3 P2) ✅ DONE
Stream engine `ProgressEvent`s as a scrolling boot-log. Settle into the RunReport on completion.
- Done: `workspace.store.ts` — `LogLine` type + `consoleLog` in `TabSessionSlice`. `session.store.ts` — appends progress events to `consoleLog` signal. New `section-console.ts` (boot-log + RunReport). Wired into `narrative-canvas.ts`.
- Gate: analyzing a repo shows live streaming log; after completion = readable report with funnel.

### U2 — Synced Lens (V3 P3) ✅ DONE
Single selection → Human pane + LLM pane side-by-side. Auto-render on selection via existing `Render` RPC.
- Done: `section-lens.ts` — persistent 50/50 split. Human pane (node detail + trace tree). LLM pane (auto-rendered markdown). Debounced render on `TraceStore.focus` change. Copy button + Ctrl+C shortcut. Wired into `narrative-canvas.ts` after trace section.
- Gate: pick any entry — both panes show it. Copy is one keystroke. Zero navigation.

### U3 — Facet views (E4 facets, UI-side only — engine must deliver E4 first) ⬜ BLOCKED
- F1 auth surface: add Auth column to Entries table (data comes from engine facet)
- F3 message matrix: producers→consumers table
- F4 data map, F5 talks-to, F8 DI health cards
- Gate: each facet renders real data from engine RPCs.

### U4 — Release UI polish (I9 UI side) ✅ DONE
- About section: version from ConnectionStore, server status dot, GitHub + issues + releases links, privacy note.
- Error telemetry hardening: audited all 34 catch sites. Fixed `palette.ts` swallow comment, added toast to `node.store.ts` failures, added toast to `node-card.ts` clipboard failures. Audit confirms no truly swallowed user-facing errors remain.
- Updates panel: "Check updates" link to GitHub Releases in About section.
- Gate: about shows real version; force an RPC error = toast appears.

### U5 — Workspace navigation + polish ✅ DONE
Navigation rail + routed views replacing single-page scroll. Entries table sorting/keyboard nav. Palette entry/node results. Keyboard shortcuts.
- Done: `shell/navigation-rail.ts` — left sidebar with icon+label navigation. `shell/workspace-shell.ts` — header + rail + router-outlet + footer, `g+key` view nav, `?` help overlay. `app.config.ts` — 8 lazy-loaded routes. `features/pages/` — overview, entries, trace, graph, insights, export page wrappers. `section-entries.ts` — sortable columns, arrow-key nav, row actions. `palette.ts` — entry search top 10, stale routes removed.
- Gate: clicking rail items navigates views; sort headers work; Ctrl+K palette searches entries; `?` shows shortcuts.

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + 7/7 vitest tests + build — must be GREEN
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server → http://localhost:4200
```

## Hard rules
- **No C# changes** — you don't need the engine worktree. If a face needs data the kernel can't answer, document it and move on. The engine agent fills the gap.
- **TypeScript only:** `src/DevContext.App/src/app/**`
- **pnpm check green** before every commit.
- Append `PROGRESS-LOG.md` after every session.

## Resume protocol (cold start)
```
git -C C:/Code/DevContext2-ui checkout feat/ui-iteration
git -C C:/Code/DevContext2-ui pull

# Verify baseline
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check

# Pick the first work item whose Status != DONE in this file
# Do Step 0 (reproduce) first, then execute
```

---

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

### I10.3 — Server MaxLiveSessions + LRU + rehydrate  **DONE**
- Locus: `src/DevContext.Server/Sessions/EngineRunner.cs` — cache-hit rehydration before analysis.
- Gate: server checks I8 snapshot cache before analysis; cache hit → instant EngineResult from cached snapshot.

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

### I9 — Release readiness (engine side)  **DONE** (CLI exit codes + --quiet)
CLI polish: exit codes, `--quiet`, stdout/stderr separation, completions.
- Locus: `src/DevContext.Cli/Settings/AnalyzeSettings.cs`, `src/DevContext.Cli/Commands/AnalyzeCommand.cs`
- Gate: `--strict` returns exit code 2 on invariant fail; `--quiet` prints nothing on success.

## Verify loop
```powershell
# From C:/Code/DevContext2-engine
dotnet build DevContext.slnx                             # 0 warnings (analyzer warnings = errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"   # must be green (383/0 as of I9)
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
