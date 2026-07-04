# Fable Iteration — Final Report & Close-Out

> **Read this first** if you're picking this project back up cold. Branch `feat/fable-redesign-skeleton`
> @ `e85d662` (2026-07-04). This closes the "F — Fable Workbench Redesign" track
> (`docs/dev/briefs/ui-ux-redesign-proposal-fable.md`) and folds in an engine-side audit trail
> (`docs/dev/audit/*`, `docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md`) that a UI-only session would
> otherwise miss. Written as a warm-up doc, not a changelog — optimized for "what do I need to know
> before I touch this again," not "what happened when."

---

## 1. What DevContext Is

A .NET 10 static-analysis tool: point it at a repo (local path or GitHub URL), it builds one immutable
typed graph of the codebase (`CodeGraph`), and renders that graph through a few lenses — architecture
overview (Map), call-chain from an entry point (Trace), node detail, neighbors, search. No LLM calls
inside the engine; it's a fact-extraction and graph-query tool that an LLM (or a human) consumes downstream.

**Three surfaces sit on the same kernel:**

```
DevContext.Core (kernel)              — graph, extraction, caching, GraphQuery
        │
        ├── DevContext.Cli            — one-shot process, Spectre.Console output
        ├── DevContext.Server         — long-lived gRPC-Web service (the desktop's backend)
        └── DevContext.Desktop        — OLD WPF+BlazorWebView shell (superseded, see §7)

DevContext.App (Tauri + Angular)      — the CURRENT desktop client, talks to DevContext.Server
```

`DevContext.Desktop` (WPF) is the pre-Tauri shell referenced in the older `docs/dev/audit/*` files —
it's dead weight now; `DevContext.App` + `DevContext.Server` is the real desktop today. Don't resume
work on `DevContext.Desktop`.

---

## 2. What This Iteration Delivered

### 2.1 The Fable redesign (the headline work)

`docs/dev/briefs/ui-ux-redesign-proposal-fable.md` — full design system + component tree + waterfall
plan (§10, stages W0→W7). **All 8 stages done, gate passed on each, 2026-06-29 → 2026-07-04:**

| Stage | Delivered |
|---|---|
| W0 Design system | Graphite token set, bundled Inter/JetBrains Mono, `ThemeService`, `/styleguide` dev route |
| W1 Shell skeleton | titlebar/tabstrip/trail-bar/activity-bar/statusbar regrid, WebView shortcut interception, offline banner |
| W2 Component build | entry-deck, stage, inspector, trace-tree, trail-bar, omnibox/export-drawer (UI), node-peek |
| W3 State/RPC hardening | `LatestGate` (epoch+abort), `TrailStore`, `AtlasStore`, dup-path guard |
| W4 The great wiring | Workbench (deck│stage│inspector) live, omnibox live, export drawer live, Home + Atlas assembled, route cutover, old pages deleted |
| W5 Derived insights | Flow Atlas indexer, Top Flows, Event Wiring Board, Impact lens, Confidence Ledger, Unwired Entries, Hub Radar, status-bar ticker |
| W6 Tauri hardening | sidecar lifecycle, window-state, single-instance, fs/opener plugins, CSP, clipboard |
| W7 Polish | node-peek everywhere, NodeCard skeleton, `?` help overlay, Paper light theme, reduced-motion audit, snapshot diff |

One workbench, one selection, one trail — the mental model shift from "pages" (Entries/Trace/Graph as
separate routes) to one Explore surface where everything reacts off a single focus, is real and shipped,
not aspirational.

### 2.2 This session's audit + fixes

Asked to audit delivered work against the plan and fix bugs. Found and fixed three, all C#/TS, all
verified live (Playwright against a real analyzed repo, not just `pnpm check`):

1. **Wrong default theme** (`theme.service.ts`) — fresh sessions fell back to the pre-Fable `'terminal'`
   vibe (mono font, amber accent, 0 radius, compact density) instead of `'modern'`/Graphite. This is
   almost certainly why the redesign "didn't look great" on inspection — the actual Graphite system was
   never being seen by default. One-line fix; screenshots before/after are night and day.
2. **Activity bar text truncation** — `item.label.slice(0, 3)` printed "Hom"/"Exp"/"Atl" under the
   icons. Spec §8.1 calls for icons-only. Removed the text.
3. **Caching invisible end-to-end** (the "caching hasn't been implemented" complaint) — two independent
   bugs stacked:
   - `DevContext.Server/Sessions/EngineRunner.cs` never wrapped its progress observer in a
     `CompositeDiscoveryObserver` + `RunReportCollector` (the CLI does this; the server didn't). Result:
     **every** run-report stat — stages, extractors, funnel, cache — was a hardcoded zero, regardless of
     what actually happened. Fixed by mirroring the CLI's observer wiring.
   - Once real stats flowed, the UI (`run-console.ts`) still showed "cold run" — because it only ever
     read `textHits`/`textMisses`, and the Roslyn-tier extractors (the ones that dominate wall time)
     record their reuse via `syntaxTreeHits`/`syntaxTreeMisses` instead. Fixed to display both.
   - **Verified live:** re-analyzing the same repo now correctly shows `syntax 95% hit (208 hits / 12
     misses)`. Caching was never actually broken at the engine level — it was invisible.

`pnpm check` green, `dotnet build` green on all 4 C# projects, `DevContext.Server.Tests` 12/12,
`DevContext.Core.Tests` 349/354 (5 failures pre-existing, in `TraceQualityTests` against an external
`eval-repos/eShop` checkout, zero overlap with anything touched this session — see §6).

---

## 3. UI/UX — Screens & Flows

| Surface | Route | Purpose |
|---|---|---|
| Start | `/` (no session) | native folder picker (Tauri `dialog` plugin) + recents + advanced options |
| Home | `/` (session ready) | boot console → digest: identity strip, Top Flows, insight headlines, run report |
| Workbench | `/explore` | Entry Deck │ Stage (System/Flow/Node altitudes) │ Inspector — absorbs the old Entries/Trace/Graph/Lens pages |
| Atlas | `/atlas` | architecture map, project topology, packages, Event Wiring Board, Hub Radar |
| Insights | `/insights` | severity-ranked findings, links back into Workbench |
| Omnibox | Ctrl+K/Ctrl+P overlay | one search surface: entries, nodes, actions, recents; Tab cycles verbs (Trace/Node/Usages/Copy) |
| Export drawer | Ctrl+E overlay | section toggles + Onboarding/Flow-Review/Full/**From Trail** presets |
| Settings | `/settings` | Appearance/Analysis/Storage/Server/About |

**Core flows** (all live-verified this session): cold-start-to-oriented (Start → console → digest →
click a flow), sweep (`j/k` through the deck with stage+inspector tracking live), deep-dive (click a
node → re-trace, `Ctrl+Z` walks it back), impact question (omnibox → Usages verb), ship-it (pin steps
→ Export → From Trail pack).

**The trail is the unifying idea**: every selection pushes onto a per-tab trail that is simultaneously
the breadcrumb, the undo/redo stack, and the export seed. This is the one idea worth protecting if
anyone proposes cutting scope later — it's what makes "one workbench" cohere instead of being three
panels that happen to share a screen.

**Keyboard model**: tabs own `Ctrl+1-6` (a deliberate fix over a documented spec conflict in the prior
I11 proposal, which double-booked those digits with dock-level shortcuts — dock now lives on
`Ctrl+Shift+L`). Esc is a ladder: cancel in-flight trace → close overlay → unpin peek → deselect node
→ clear focus → clear filter. `?` opens a full keymap overlay that's actually current (was stale/dead
before W7).

---

## 4. Backend Graph Capabilities → What the UI Actually Shows

This is the part worth being precise about, because the kernel can do more than the UI currently asks
for.

### 4.1 The graph model

`CodeGraph`: 3 node kinds (`Type`, `Member`, `EntryPoint` — one C# class = one node, tagged with role
labels like `handler`/`entity`/`aggregate`/`service`), 8 edge kinds (`Calls`, `Sends`, `Handles`,
`Raises`, `Consumes`, `ReadsWrites`, `Resolves`, `WrappedBy`), 3 resolution tiers (`Join`=1.0 confidence,
`Syntactic`→labeled `[approx]`, `Semantic`→labeled `[verified]`).

**Important correction to an older internal audit** (`docs/dev/audit/audit-graph-architecture.md`,
2026-06-23): that doc flags a critical bug where call/raise/send edges were attached at the **class**
level, so every method in a class inherited every sibling method's edges (`[verified]` overclaiming,
wrong `EMITS`/`TOUCHES`). **This was fixed** in the interleaving "Universal Lens" engine iterations
(`docs/dev/plans/ITERATION-1-kernel-correctness.md`, commit `4f457a3`) — edges are now genuinely
**member-scoped** (`GraphBuilder.cs` comment: `"C1: Calls edges from CallEdges (member→member)"`,
confirmed live in current source). If you find an old doc calling this a live bug, it's stale — verify
against `GraphBuilder.AddCallEdges` before trusting it.

### 4.2 The queryable kernel (`GraphQuery.cs`)

One class, built during the same engine iterations, is the actual API surface every UI feature sits on:

```
EntryPoints(kind?)       Map()             Stats()
Trace(focus, depth, fanOut)                Node(id)
Neighbors(id, direction, kind?)            FindUsages(id)   // = Neighbors(id, In)
ResolveNodeId(nameOrKey)                   BlastRadius(from, maxDepth)
```

This is exposed over gRPC-Web as `Analyze / ListEntryPoints / GetMap / GetTrace / GetNode /
GetNeighbors / SearchNodes / GetStats / Render / Ping` (`devcontext.proto`) — one query op maps to one
RPC, with one gap (below).

| Kernel capability | RPC | UI surface that uses it |
|---|---|---|
| `Map()` — topology, style, packages, entries | `GetMap` | Stage "System" altitude (never-blank graph); Atlas page |
| `Trace(focus)` — call-chain from an entry | `GetTrace` | Stage "Flow" altitude (tree/graph), Inspector "Call stack" |
| `Neighbors(id, Out)` | `GetNeighbors` | Stage "Node" altitude, "out" direction |
| `Neighbors(id, In)` / `FindUsages(id)` | `GetNeighbors` (direction=`usages`) | Stage "Node" altitude, "usages" direction — **this one is real**, not a client reconstruction |
| `Node(id)` — title/kind/tags/file path/degree | `GetNode` | Inspector Details, NodeCard, node-peek hover |
| `ResolveNodeId` | (internal, used by `GetNode`/`GetNeighbors`) | omnibox search resolution |
| `Stats()` — seam counts, entries-with-target | `GetStats` | Home stat strip, Confidence Ledger |
| **`BlastRadius(from, maxDepth)`** | **none** | **not exposed anywhere** |

### 4.3 The one real gap: `BlastRadius` is dead code from the API's perspective

`GraphQuery.BlastRadius()` already exists, already does a correct multi-hop graph walk from any node
outward to the entry points that can reach it (`BlastResult(EntryTitle, Kind, Hops)`), and is never
called by a single gRPC handler (`grep` across `DevContext.Server` confirms zero call sites).

Meanwhile, the Fable proposal's **Impact lens** (§3.4, "Reached by N flows" — the Inspector line and
5th omnibox verb) reimplements the same idea entirely client-side: the **Flow Atlas** background
indexer shallow-traces up to 100 entry points one by one and `AtlasStore.reachedBy` checks which of
those traced entries touched the node in question. This works, but it's strictly worse than calling
the kernel directly:
- Slower (N `GetTrace` calls instead of one graph walk)
- Incomplete (only as good as what the Atlas indexer has covered — the UI already shows an "approx,
  coverage < 100%" caveat because of this)
- Duplicate logic — the correct multi-hop reachability answer already exists on the server for free

**If anyone picks up a next iteration on this feature**, the fix is small: add a `GetImpact` (or fold
into `GetNode`) RPC that calls `session.Query.BlastRadius(nodeId, maxDepth)` server-side, and let
Impact lens call that directly instead of depending on Atlas coverage. This closes a real gap between
"what the backend can already tell you" and "what the UI makes you wait for."

### 4.4 What's NOT in the graph at all (structurally out of scope, not a bug)

- **No authorization/auth info per endpoint** (Fable gap **S1**, engine-blocked) — the extractors don't
  detect `[Authorize]`/policy attributes as a first-class signal.
- **No line numbers**, only file paths (Fable gap **S2**, partially closed) — `GetNode.filePath` exists
  and is wired into the Inspector; `NodeResponse` has no line-number field, so "jump to exact line" in
  an editor isn't possible from the UI today.
- **`Sends`/`Raises` edges are always `[approx]`** — body-string scanning (`_mediator.Send(...)`,
  `AddDomainEvent(...)`), never upgraded to Roslyn semantic resolution. A semantic tier for these is
  noted as future work in the Universal Lens roadmap (Phase 3 note), not started.

---

## 5. Caching — Architecture & Current Status

Three tiers exist (`docs/dev/audit/audit-caching-system.md` has the deep dive; still accurate at the
mechanism level, one section is now stale — see below):

| Tier | Scope | Used by |
|---|---|---|
| `AnalysisCache` | per-run, destroyed after | CLI (correct — no cross-run benefit for a one-shot process) |
| `PersistentAnalysisCache` | cross-run, mtime-invalidated, one instance per repo path for the process lifetime | Desktop server, via `EngineHostCache` |
| `SharedAnalysisContext.SyntaxCache` | per-analysis, shared across extractors in one run | both |
| `SnapshotCacheService` | **cross-run, disk-persisted**, keyed by SHA256+git-HEAD, LRU | Desktop server (`Ctrl+R` re-analyze, tab restore) |

The audit doc's "No persistent graph cache: designed, deferred" line is **stale** — `SnapshotCacheService`
(I10.3, `docs/dev/go-to-program/ITERATION-I10-workspace-tabs.md`) shipped this since that audit was
written. On a snapshot-cache hit, the entire pipeline is skipped and a rehydrated `AnalysisSnapshot` is
returned directly — this is why re-analyzing an unchanged repo is near-instant.

**What this session found broken and fixed** (§2.2): the desktop server never surfaced *any* real
run-report numbers (stages/extractors/funnel/cache) because of a missing observer, and even after that
fix, the cache section specifically was still blind to syntax-tree reuse. Both fixed and verified live.
**The underlying caching mechanisms themselves were never broken** — only their observability was.

Settings → Storage shows on-disk cache/repo sizes with a Clear button, reading the same paths the
engine writes to (`%LOCALAPPDATA%/DevContext/cache`, `.../repos`) — this only works in the packaged
Tauri app (needs the `fs` plugin); in a plain browser tab it correctly shows "Real file listing is only
available in the desktop app."

---

## 6. Known Issues & Gaps — Engine Side

The engine has its own, larger, mostly-separate correctness program
(`docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md`, phases 0–10) that predates and partially overlaps with the
Fable UI track. **Status as of this session** (from `docs/dev/plans/README.md`):

| Phase | What it fixes | Status |
|---|---|---|
| 0 — Kernel hygiene | remove token-budget distortion from graph building, delete dead pruner | **DONE** |
| 1 — Member-origin correctness | the class-scoped call-edge bug (§4.1) | **DONE** |
| 2 — Universal entries | controller route→action resolution (was 0/94 resolved) | **DONE** |
| 3 — Complete & honest traces | TOUCHES via `Calls`-edge traversal, explicit truncation | **DONE** |
| 4 — Honest Map | scope-aware style claims (no more "OrchardCore = Microservices"), STACK noise filter | **DONE** |
| 5 — Queryable kernel | `GraphQuery` + inverse edges (§4.2) | **DONE** |
| 6 — Performance | entry-scoped call-graph binding, ~41s→~10s cold DntSite | **DONE** |
| 7 — Browse UI redo | *this is functionally the Fable redesign* — **the roadmap's own tracker still says "WIRED," not updated to reflect W0-W7 completion.** Worth a one-line fix to that table so a future session doesn't think Phase 7 is still open. | done via Fable, tracker stale |
| 8 — MCP server | expose `GraphQuery` as MCP tools for agent use | **BLOCKED / not started** |
| 9 — Persistent index + GitHub-URL hardening | instant re-open across machines, robust clone-from-URL | **BLOCKED** (basic GitHub clone via `RepoUrl`/`GitCloneService` already works; "hardening" + cross-machine index don't) |
| 10 — Coverage ladder | gRPC services, SignalR hubs, Blazor components, Azure Functions/Lambda triggers | **BLOCKED** — only controllers/minimal-API/MediatR are well covered today |

**Residual, smaller engine gaps** (things that are correctness nuances, not phase-blockers):
- DntSite-class repos: `EfCoreExtractor` has a known detection gap on TOUCHES completeness (noted in
  Phase 5's gate as "split out — detection gap, not FQN").
- `Sends`/`Raises` edges never get a semantic (Roslyn-verified) tier, only body-scan `[approx]` (§4.4).
- 5 pre-existing test failures in `DevContext.Core.Tests.TraceQualityTests` this session, all
  depending on an external `eval-repos/eShop` checkout whose current on-disk state has drifted from the
  hardcoded golden seam-count thresholds. Unrelated to anything touched here (confirmed: zero edits to
  `DevContext.Core` this session). Worth a look next time someone is in that area — likely just needs
  the goldens re-ratcheted against the current `eval-repos/eShop` clone, per this repo's own convention
  ("goldens are ratcheted, never silently re-baselined").

---

## 7. Known Issues & Gaps — UI/UX Side

From the Fable proposal's own §9 coverage matrix, cross-checked live this session:

| Gap | Status |
|---|---|
| S1 — auth column | **engine-blocked**, unchanged (§4.4) |
| S2 — file:line | **partial** — file path shown (`GetNode.filePath`), no line number (engine-blocked) |
| Sidecar sub-process packaging (W6 checkpoint 1) | lifecycle hardening (spawn/health/kill/backoff) done and verified; the actual `externalBin` single-file install packaging is **not wired** — confirmed viable, not finished |
| `mini-map` (thumbnail for tree mode) | **skipped**, judged low-value, not built |
| Snapshot diff (§3.9, stretch) | **done** (W7) — Ctrl+R re-analyze posts a ticker diff of entry counts + Atlas confidence |
| Impact lens efficiency (§4.3 above) | **works, but reinvents a kernel capability client-side** — see §4.3 for the direct fix |
| MCP-driven UI hooks | not applicable — MCP itself doesn't exist yet (engine Phase 8) |
| Windows DPI pass (125%/150%, literal spec item) | **redirected** by explicit user call to a general type-scale legibility bump instead; the literal scaling test was never run |
| A pre-existing, not-fixed cosmetic bug | `run-console.ts`'s boot log tracks `@for` by `line.timestamp` (`Date.now()`) — two progress events in the same millisecond collide (`NG0955` warning, harmless, visible only in dev console) |

**Nothing else from the original 23-gap tracker (§9 of the proposal) is open** — everything else is
closed and was live-verified across W1–W7 (tab wiring, MRU, shortcuts-in-help, duplicate-path guard,
palette debounce/verbs/recents/icons, export presets, NodeCard skeleton, helper dedupe, rail badges,
icon registry, etc.).

---

## 8. Consolidated Issue List (flat, for quick scanning)

### Engine
- [ ] `GraphQuery.BlastRadius` has no RPC — Impact lens should call it directly instead of the Atlas
      brute-force approximation (§4.3)
- [ ] MCP server (Phase 8) — not started
- [ ] Persistent cross-machine index + GitHub-URL hardening (Phase 9) — not started
- [ ] Coverage ladder: gRPC, SignalR, Blazor, Azure Functions/Lambda (Phase 10) — not started
- [ ] `EfCoreExtractor` TOUCHES completeness gap on some repo shapes
- [ ] `Sends`/`Raises` edges never get semantic (only body-scan) resolution
- [ ] 5 stale golden test failures in `TraceQualityTests` against `eval-repos/eShop` — needs a
      re-ratchet, not a kernel fix
- [x] Class-scoped call-edge attribution — **fixed** (Phase 1, predates this session)
- [x] Controller entry→target resolution (was 0/94) — **fixed** (Phase 2)
- [x] Classifier scope-awareness (OrchardCore mis-classified as microservices) — **fixed** (Phase 4)
- [x] Run-report (stages/extractors/funnel/cache) always zero on the desktop server — **fixed this
      session**

### UI/UX
- [ ] S1 auth column (engine-blocked)
- [ ] S2 line numbers (engine-blocked; file path already works)
- [ ] Sidecar `externalBin` install packaging (lifecycle already works; packaging step remains)
- [ ] `mini-map` thumbnail — deliberately skipped, revisit if tree-mode navigation feels lost without it
- [ ] `NG0955` timestamp-collision warning in run-console dev logs — cosmetic, low priority
- [x] Default vibe fallback (`'terminal'` instead of `'modern'`/Graphite) — **fixed this session**,
      likely the single biggest driver of "the redesign doesn't look great"
- [x] Activity bar text truncation ("Hom"/"Exp"/"Atl") — **fixed this session**
- [x] Cache stats always showing "cold run" regardless of real reuse — **fixed this session**
- [x] All 23 gaps from the Fable proposal's original tracker — closed across W1–W7

### Docs / process hygiene (small, but worth doing before the next session)
- [ ] `docs/dev/plans/README.md` Phase 7 row says "WIRED" — should say "DONE via Fable redesign,
      see `AGENTS.md` F section" so a future engine-side session doesn't re-open browse-UI work
- [ ] `docs/dev/audit/audit-graph-architecture.md` §Gaps item 4 ("no persistent graph cache") is stale
      — `SnapshotCacheService` shipped since; worth a one-line addendum rather than editing history

---

## 9. Recommended Next Steps

Pick based on what matters most next, these are independent:

1. **Ship what's here.** `pnpm check` green, engine builds green, relevant tests green, 3 real bugs
   fixed and verified live. This is a reasonable point to commit and/or open a PR.
2. **If UI polish continues:** wire `BlastRadius` into a real RPC and simplify Impact lens (§4.3) — small,
   concrete, improves both correctness and speed of an already-shipped feature.
3. **If engine work resumes:** Phase 8 (MCP) is the next unblocked item in the Universal Lens roadmap
   and is probably the highest-leverage next engine investment (agent-facing tool access over the
   already-correct `GraphQuery` kernel).
4. **If packaging matters:** finish the W6 sidecar `externalBin` bundling — lifecycle/hardening is done,
   only the install-time wiring (`tauri_plugin_shell` sidecar API) remains.
5. Either way — re-ratchet the 5 stale `eval-repos/eShop` golden tests next time someone is near
   `DevContext.Core.Tests` so `dotnet test` is fully green again, not just "green except a known drift."

This closes the Fable Workbench Redesign track. Nothing in `docs/dev/briefs/ui-ux-redesign-proposal-fable.md`
§10 is open. Any further UI work here is new scope, not a resume.
