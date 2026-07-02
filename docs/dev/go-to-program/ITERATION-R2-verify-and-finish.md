# Iteration R2 — Verify & Finish: close the wire, build the missing faces

> Round-2 plan (2026-07-02) after verifying the DeepSeek session's `go-to/implement-iterations`.
> Branch: `go-to/implement-iterations` · Base: `develop` @ `7228d1e` · **Worktree: `C:/Code/DevContext2-goto-audit`**
> Scope: fix what the tracker over-claims, finish the desktop faces to the UI-UX contract, continue the
> engine backlog. **I6 (MCP) stays deferred — out of scope for R2.**
> Reads before starting: `HANDOVER.md`, `UI-UX-GUIDELINES.md`, `UNIFIED-TRACKER.md`, this doc.

---

## 0. Why this doc exists — the verification result

The round-1 session shipped real, good work (I1 seam hardening, CLI `query`, the workspace shell, node
card, palette, honesty ribbon). But two tracker rows are **not true as written**, and three specced
faces were never built. This iteration corrects the record and finishes the job.

**The one broken contract (root cause of everything below).** README maintenance rule #3 —
*"anything a face shows must exist as a `GraphQuery` op / kernel JSON field first"* — was skipped for the
flagship deliverable. `InsightsBuilder` computes insights into `AnalysisSnapshot.Insights`, but **nothing
serializes them to any face**: no proto message, no RPC, no `StatsResponse` field, and
`KernelJsonRenderer` emits architecture `Signals` (from `model.Architecture.All`), *not* the `Insight[]`.
So I3 (engine) and I4.6 (desktop insights card) are both marked DONE while the data dies in the snapshot.

### 0.1 Findings (verified — fix these first, they are defects not features)

| # | Severity | Finding | Locus | Fix in |
|---|----------|---------|-------|--------|
| F1 | **High** | Insights never cross the wire — `AnalysisSnapshot.Insights` is serialized by nothing. Face shows re-derived summary, not the ranked `Insight[]`. | `KernelJsonRenderer.cs` (emits `Signals`, not insights); `proto/devcontext/v1/devcontext.proto` (no insight field) | R2.1 |
| F2 | **High** | `insights-view.ts` calls `store.lastStats()` which **does not exist** on `SessionStore`. The Engine-details drawer is dead. | `features/insights/insights-view.ts:50`; `state/session.store.ts` (no `lastStats`, never calls `GetStats`) | R2.1 |
| F3 | **High** | `insights-view.ts` reads `s.projectCount` / `s.entryTargetRatio` / `s.seamCounts` off `AnalysisSummary`, whose real fields are `projects` / `entries` / `entriesWithTarget`. Renders `undefined projects` / `undefined%`. | `features/insights/insights-view.ts:20-38` vs `devcontext_pb.ts` `AnalysisSummary` | R2.1 |
| F4 | **Med** | `/insights` route exists but is **not in the shell nav rail** (`LENS_ITEMS`) and not in the palette nav list — reachable only by typing the URL. Orphaned. | `shell/app-shell.ts:26-33`; `features/palette/palette.ts:99-103` | R2.1 / R2.7 |
| F5 | **Med** | No `NodeLink` primitive. The core I4 premise "every name is a node link" was never built; entry `target`, trace node titles, and overview topology names are plain text. You can only open a Node Card from palette search or from inside an already-open card. | absent (`ui/` has no `node-link`) | R2.2 |
| F6 | **Med** | `trace-node.ts:22` uses `ml-[calc({{depth()}}*20px)]` — a Tailwind arbitrary value built from a runtime binding. Tailwind can't generate that class at build; the interpolation is dead (indentation only works because of the nested `border-l`/`pl-3`). | `features/trace/trace-node.ts:22` | R2.4 |
| F7 | **Low** | Connection is binary Connected/Offline with no tooltip and no restart/retry action; spec §2.1 wants 3-state + server version/port tooltip + red-state "restart server". | `shell/title-bar.ts:18-28` | R2.8 |
| F8 | **Low** | Stats-fetch failure is swallowed to a blank drawer (no `_statsError`); spec §2.2 wants a retriable inline error. | `state/session.store.ts` (no stats path yet) | R2.1 |

### 0.2 Specced faces that don't exist yet

- **Graph view** (UI-UX §5) — no `graph-view.ts`; rail has no Graph item. The `GetNeighbors`/`GetNode`
  RPCs are wired only into the Node Card.
- **Settings view** (UI-UX §7) — no `settings-view.ts`; only vibe-cycle in the title bar. §7's
  Appearance / Analysis / Storage(I8) / Server / About(I9) groups are all absent.
- **Entries table** (UI-UX §4) — current view is a good grouped *list* but not the specced table
  (columns, sortable headers, row actions, keyboard nav, auth badges, virtual scroll).

### 0.3 Information-architecture reconciliation (do this in R2.1 before adding views)

The shell rail today is the desktop-v2 skeleton: `Source · Cache | Overview · Entries · Browse · Trace ·
Document · Stats`. The UI-UX §1 route table is `Overview · Entries · Trace · Graph · Insights · Export ·
Settings`. Reconcile once, up front:

| Skeleton view | Decision (default) |
|---|---|
| `Document` | Rename/relabel to **Export** (it IS the LLM doc — UI-UX §1 last line). Keep. |
| `Stats` | Fold into **Insights → Engine drawer** (telemetry belongs there, UI-UX §1/I4.6). Retire the rail item after Insights lands. |
| `Cache` | Fold into **Settings → Storage** (UI-UX §7 I8). Retire the rail item after Settings lands. |
| `Browse` | OPEN — keep as a source browser (useful, not in spec) **or** fold into palette. Default: keep, low priority. |
| `Insights`, `Graph`, `Settings` | **Add to rail** (Insights now; Graph R2.5; Settings R2.6). |

---

## R2.1 — Insights on the wire (the flagship fix) · Phase V2/V3 · **do first**

**Goal:** the `Insight[]` the engine already computes reaches the CLI JSON and the desktop, and the
Insights view renders the real ranked cards instead of a re-derived summary. This unblocks I3's remaining
sources (they become visible the moment they exist).

| Step | What | Locus |
|------|------|-------|
| R2.1a | **Kernel JSON:** add `insights: [{ id, category, severity, title, detail, evidence[] }]` to `KernelJsonRenderer` output, sourced from `snapshot.Insights` (the ranked/capped list). Keep `Signals` — they're architecture facts, a different thing. | `Rendering/KernelJsonRenderer.cs`, `Pipeline/AnalysisSnapshot.cs` |
| R2.1b | **Proto:** add `message Insight { string id; string category; string severity; string title; string detail; repeated string evidence; }` and `repeated Insight insights = N;` on `StatsResponse` (insights are a stats-time product; `GetStats` already exists). Regenerate: server C# + `pnpm gen` for `devcontext_pb.ts`. | `proto/devcontext/v1/devcontext.proto` |
| R2.1c | **Server:** map `snapshot.Insights` → `StatsResponse.Insights` in the `GetStats` handler. | `DevContext.Server` stats handler |
| R2.1d | **API + store:** add `getStats(handle)` to `DevContextApi`; `SessionStore` fetches stats after analyze (alongside map/entries) into a new `stats` signal, and exposes `insights = computed(() => stats()?.insights ?? [])`. Add `_statsError` (fixes **F8**). This creates the `lastStats()`/`stats()` the view already assumes (fixes **F2**). | `data-access/devcontext-api.ts`, `state/session.store.ts` |
| R2.1e | **Insights view rewrite:** render one card per insight grouped by category, severity color (warn/danger/info), evidence as chips; keep the coverage bar and the Engine drawer (node/edge counts, schema) reading the real `stats()`. Delete the `projectCount`/`entryTargetRatio`/`seamCounts` reads (fixes **F3**). Empty/error/loading triad (UI-UX §3). | `features/insights/insights-view.ts` |
| R2.1f | **Rail + palette:** add Insights to `LENS_ITEMS` with a count badge = insight count; add "Go to Insights" to the palette (fixes **F4**). | `shell/app-shell.ts`, `features/palette/palette.ts` |
| R2.1g | **CLI face:** `--stats` prints insights first (per I3.3 / tracker I2.5) reading the same list. | `AnalyzeCommand.cs` / stats renderer |

**Verify:** analyze eShop → `devcontext ... --format json` shows ≥4 insights → desktop Insights card
shows the same 4, rail badge = 4, Engine drawer shows real node/edge counts. TodoApi stays quiet (0–1).
Screenshot in the commit (README rule #4).

**Gate:** `dotnet build` 0w · `dotnet test --filter Category!=Eval` green · `pnpm check` green.

---

## R2.2 — NodeLink primitive (the missing foundation) · Phase V3

**Goal:** deliver I4's "Move 1 — every name is a node link" that R1 skipped. Without it the Node Card is a
dead end (only reachable from palette).

| Step | What | Locus |
|------|------|-------|
| R2.2a | `ui/node-link/node-link.ts` — `[nodeId]`/`[label]` inputs, subtle underline, mono ink, click → `NodeStore.show(nodeId)`. Real `<button>`. | new |
| R2.2b | Use it for entry `target` (Entries), trace node titles (Trace), Node-Card neighbor rows (already links — migrate for consistency). | `entries-view.ts`, `trace-node.ts`, `node-card.ts` |
| R2.2c | **Conservative markdown linkify** (UI-UX §3 / I4.1 "tricky bit"): only in the Export/Document preview and only whole-word matches against the entry+node title set (cache one Map per session in a store). Never regex arbitrary identifiers. | `document-view.ts` + session store title-set |

**Verify:** click a target in Entries → Node Card opens; click a neighbor → navigates; trace title click →
card. **Gate:** `pnpm check` + screenshot of entry → target-link → card → neighbor.

---

## R2.3 — Entries table to spec · Phase V3

Upgrade the grouped list (keep chips, search, click-to-trace, approx/method badges) to UI-UX §4:

| Aspect | Task |
|---|---|
| Columns | Method badge · Route/Title (mono, param tokens dimmed) · Target (NodeLink from R2.2) · Auth (`[anon]` warn / policy — hide column when F1 data absent) · Kind (icon+label) · file:line (subtle, reveal-on-click) |
| Sorting | header click: Route · **Target (resolved-first, default)** · Kind; persist in `?sort=` |
| Filter | keep chips+search; add "has target" / "approx" toggle chips; count = `filtered / total` |
| Row actions | hover reveals Trace · Node card · Copy route · Reveal file. Keyboard `↑↓`/`Enter`/`n`/`c` |
| Scale | >150 rows → `@defer` + CDK virtual scroll, sticky header |
| Empty | not-analyzed → CTA to landing; filtered-to-zero → "clear filters" |

**Gate:** `pnpm check` + screenshot; keyboard nav demoed.

---

## R2.4 — Interactive trace to spec · Phase V3

| Step | What | Locus |
|------|------|-------|
| R2.4a | **Fix F6:** replace `ml-[calc({{depth()}}*20px)]` with `[style.marginLeft.px]="depth()*16"` (or lean on nested border only). | `trace-node.ts:22` |
| R2.4b | Expand-on-node: click a node → `GetTrace` deeper anchored there, splice into the tree. | `trace.store.ts`, `trace-node.ts` |
| R2.4c | Focus breadcrumb (history stack; back = pop); node titles as NodeLink. | `trace-view.ts` |
| R2.4d | Honest empty: resolved-but-no-edges shows the engine's W3b hint verbatim (not "Trace not found"). | `trace-view.ts` + trace response hint |
| R2.4e | "Add subtree to Export pack" on any node (feeds R2.10). | `trace-node.ts` |

**Gate:** `pnpm check` + screenshot of expand + breadcrumb + honest-empty.

---

## R2.5 — Graph view (new face) · Phase V3/V7 · UI-UX §5

`features/graph/graph-view.ts` + rail item. Seed from: current trace focus > selected node > top
interesting points (F7) — **never the whole graph**. Node click = Node Card (not navigation). Seam-kind
filter chips reuse `SEAM_COLORS`. Depth slider. Uses `GetNeighbors` iteratively from the seed set.
Keep it simple (SVG/canvas force layout or a lightweight lib already vendored — check before adding a dep).

**Gate:** `pnpm check` + screenshot; graph never renders >N nodes unseeded.

---

## R2.6 — Settings view (new face) · Phase V7 · UI-UX §7 (folds I8 + I9)

`features/settings/settings-view.ts` with left sub-tabs; add `prefs.store` (persisted, one key, schema
version). Groups:

| Group | Content |
|---|---|
| Appearance | vibes/themes (move the title-bar cycle's authority here; keep the quick-cycle button) |
| Analysis | default depth/detail · excluded-dirs editor · Roslyn-tier toggle (`--no-roslyn` inverse) · lite mode |
| Storage (**I8**) | cache location + per-repo cache list w/ sizes + clear · clone folder path + open-in-explorer + "keep clones" · disk-usage bar. **Fold the `Cache` view here.** |
| Server | port · status · restart (Tauri sidecar) · open logs folder |
| About (**I9**) | app + engine version + commit · license · third-party notices · check-updates · links |

Recents move to Landing (`source-view`), keeping the remove-x.

**Gate:** `pnpm check` + screenshot each group; Settings works **while analyzing** (UI-UX §2.5).

---

## R2.7 — Palette completeness · Phase V3 · UI-UX §6

Current palette is Ctrl+K-wired but thin. Add: all nav targets incl. **Insights / Browse / Document** ·
verbs per hit (`Tab` cycles Trace · Node · Usages · Copy; `Enter` = default) · **Recents** section ·
node hits show kind icon + title (not raw `nodeId`) · no-results row "search the graph for '<q>'".
Palette verbs and CLI `query` ops share names (FACES §1.1).

**Gate:** `pnpm check` + screenshot.

---

## R2.8 — Connection 3-state + restart · Phase V7 · UI-UX §2.1 (fixes F7)

`title-bar.ts` dot → green(online)/amber(degraded/reconnecting)/red(offline) with tooltip (server version
+ port from `Ping`), re-ping on window focus, red-state offers "restart server" (Tauri sidecar) / "retry".

**Gate:** `pnpm check` + screenshot of each state (kill server to force red).

---

## R2.9 — Overview reshape + honesty ribbon polish · Phase V3 · UI-UX §1

Overview is currently a Map dump. Spec wants it short: **identity + top-3 insights (from R2.1) + topology
summary + "start here" (F7 interesting points)**. Move the full Map facets under a "details" disclosure.
Honesty ribbon: add scope note + approx% (it has archetype/projects/entries/targets today).

**Gate:** `pnpm check` + screenshot.

---

## R2.10 — Export packs presets · Phase V7 · I4.7

In the Document/Export view add preset compositions — **Onboarding · Trace pack · Review pack** — of
existing sections + the trace subtrees queued from R2.4e. Token estimate = client-side `chars/4` (render
-time only, no kernel budget).

**Gate:** `pnpm check` + screenshot of each preset.

---

## Engine backlog (carry-over, unchanged in intent)

| ID | Item | Guide | Notes |
|----|------|-------|-------|
| **E1** | I3 remaining **6 insight sources** (`wiring.hubs`, `graph.orphans`, `wiring.external-events`, `data.busiest-aggregate`, `topology.chokepoint`, `wiring.multi-impl`) | `ITERATION-I3-insights.md` | Each ships with a positive + negative eval expectation. They become visible via R2.1 automatically. |
| **E2** | I1.5 **pattern-zoo corpus** — `tests/fixtures/PatternZoo/` exercising modern C# through seam scanners | `ITERATION-I1-trust.md` §I1.5 | Spec exists, no code. Raw-string trap must be covered. |
| **E3** | I2 **full W9 deletion** — migrate eval `json-*` checks to kernel-JSON shape, then delete `PatternRelevancePruner.cs`, `TokenBudgetEnforcer.cs`, `RenderPlanBuilder`, `FinalScore`, `TokenBudget`, `OutputSelfCheck` token checks | `ITERATION-I2-cli-kernel.md` §I2.2 | Blocked only by the eval migration; DI already retired. |
| **E4** | I5 **remaining facets** — ★ **F3 message matrix** (producers→consumers per message) first, then F1 auth surface, F2 middleware, F4 data map, F5 talks-to, F6 config, F7 interesting points, F8 DI health, F9–F12 | `ITERATION-I5-facet-menu.md` | Each facet = a `GraphQuery` op first (rule #3), then CLI `query` + a face. F1 unblocks the Entries Auth column (R2.3) and `auth.anonymous` insight. |
| **E5** | I7 **benchmark expansion** — clone/register the 8 missing-archetype repos, run suite, judge insight quality, ratchet expectations | `ITERATION-I7-benchmark-audit.md` | Needs `eval-repos/` populated in this worktree (junction to `C:\code\DevContext2\eval-repos`). Closes each batch. |

**I6 (MCP): still deferred. Not in R2.**

---

## Delivery order

```
R2.1 (insights wire) ─┬─► R2.2 (NodeLink) ─┬─► R2.3 Entries ─► R2.4 Trace ─► R2.5 Graph
   (unblocks E1,E4)   │                    └─► R2.9 Overview/ribbon
                      ├─► R2.6 Settings (I8+I9)   ├─► R2.7 Palette   ├─► R2.8 Connection
                      └─► R2.10 Export packs
Engine (parallel): E1 (needs R2.1) · E2 · E3 · E4 (F3,F1 first) · E5 (closes batches)
```

Rule: **R2.1 before any other face work** — it's both the biggest bug and the unblock for insights/engine
visibility. `NodeLink` (R2.2) before Entries/Trace/Graph since they depend on it.

## Gate (every step)

1. `dotnet build DevContext.slnx` — 0 warnings
2. `dotnet test DevContext.slnx --filter "Category!=Eval"` — green
3. Desktop step → `pnpm check` (lint+type) + a **screenshot in the commit** (green checks don't catch
   dead UX — the D1–D11 lesson; this is exactly how F1–F3 shipped as "DONE").
4. Wire step → the face reads a real kernel-JSON field / RPC (README rule #3), never an imagined shape.
5. Docs move with code: `docs/product/cli-reference.md` / `docs/product/desktop-ui.md` same commit.

**Do-not-regress anchors:** `BudgetIndependenceTests` · `TraceQualityTests` sibling-divergence Facts ·
`GraphBuilderSpanTests` (3) · `NoiseFilterTests` · `ArchetypeDetectorTests`.

## Resume protocol (next agent)

1. Work in the **`C:/Code/DevContext2-goto-audit`** worktree — `git checkout go-to/implement-iterations`
   there. Do **not** touch the `feat/narrative-canvas` checkout in `C:/Code/DevContext2` (separate desktop
   lineage).
2. Read `UNIFIED-TRACKER.md` + this doc; start at **R2.1**.
3. Populate `eval-repos/` (junction to `C:\code\DevContext2\eval-repos` or clone per `eval-repos.json`)
   before any `eval/gates.ps1` run.
4. Deliver per-step; flip `UNIFIED-TRACKER.md` / `README.md` status honestly (a face counts as DONE only
   when it renders a real wire field with a screenshot in the commit).
5. Append to `PROGRESS-LOG.md` (date · changed · verified · next).
