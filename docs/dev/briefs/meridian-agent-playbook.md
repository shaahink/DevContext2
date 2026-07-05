# Meridian Agent Playbook — how to deliver a stage without gaps

> Mandatory reading for every executing agent (you are likely DeepSeek V4 Pro or
> Claude Sonnet 5 — §10 has notes for each). This document exists because the
> previous phase was marked DONE while its MCP server crashed on startup and its
> benchmark "audit" scored fixes against repos it never re-ran. Every rule here maps
> to a real failure from this repo's history. Read once fully at phase start; re-read
> §2, §4, and your stage's UI spec (§9) at each session start.

---

## 1. Session start ritual (10 minutes, non-negotiable)

1. `git checkout` the phase branch; read `MERIDIAN-START.md` (root) top to bottom —
   it is short by design. The **Handoff** block tells you exactly where the last
   session stopped and what it left dirty.
2. Read your stage's section in `proposal-meridian.md` + its **Gate** line. The gate
   is the definition of done; the checkpoint list is the route, not the destination.
3. Read the relevant sections of `lighthouse-delivery-audit.md` if your stage cites
   W-findings.
4. **Run the baseline before touching code**: the harness relevant to your stage
   (§5). Paste the baseline numbers into your tracker row. If you can't run the
   baseline, you are blocked — record BLOCKED in the tracker and stop; do not "code
   anyway".

## 2. Session end ritual — handoff without bulky progress files

There is **no progress.md, no per-session log file, no appended history**. Git is the
history. The handoff lives in exactly one place: the `## Handoff` block at the top of
`MERIDIAN-START.md`, which you **overwrite** (never append) with at most 10 lines:

```
## Handoff  (overwrite this block, ≤10 lines, no history)
last: <commit sha> <one-line what>
stage: M<x> checkpoint <x.y> — <IN PROGRESS|DONE|BLOCKED>
gate: <run|not run> — <artifact path if run>
dirty: <uncommitted files / known-broken thing / none>
next: <the single next action, concrete: "run bench, then implement 1.7 gRPC join">
trap: <one thing that will bite the next agent, or "none">
```

Rules:
- Update the checkpoint table row(s) you touched: Status, Commit, Evidence path.
- Evidence = a path to an artifact produced by *running* something this session
  (bench report, QA table, screenshot dir, golden diff). A code path is not evidence.
- Commit at every green checkpoint (small commits; message prefix `feat(mX.Y):` /
  `fix(mX.Y):`). Never leave a session with uncommitted work unmentioned in `dirty:`.
- If you renumber, rescope, or drop ANY checkpoint, say so in the commit message AND
  in a `> scope change:` line under the stage tracker row. Silent renumbering is how
  the last phase hid its gaps.

## 3. The quality bar (definition of done)

A checkpoint is DONE when **all** of these hold:

1. The behavior is demonstrated by a fresh artifact from a real run (harness output,
   report diff, screenshot, transcript) — produced *after* your last code change.
2. The artifact covers the **dogfood repo** (`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`),
   not only a hand-built fixture. Fixtures prove the mechanism; the dogfood repo
   proves the product.
3. `dotnet build DevContext.slnx` → 0 warnings 0 errors (warnings are errors here);
   `dotnet test DevContext.slnx --filter "Category!=Eval"` green;
   if UI touched: `pnpm check` (from `src/DevContext.App`) green.
4. No regression on the library bench repos (M0.1 diff clean) when engine code moved.
5. The tracker row has the evidence path.

If any leg is missing, the row says IN PROGRESS. A stage with one IN PROGRESS row is
not DONE, no matter how good the story is.

## 4. Anti-pattern catalog — all previously observed in THIS repo

Each of these was actually shipped here and later caught by audit. If you catch
yourself doing one, stop and fix the approach.

| # | Anti-pattern | Real instance | Rule |
|---|--------------|---------------|------|
| A1 | **Dead-parameter fix** — add/plumb a parameter, never consume it | MCP `intent` plumbed to `ContextPackBuilder.Build()` which ignored it; commit said "fixed" | After adding a param, grep for where its VALUE changes behavior; paste that line in the commit body |
| A2 | **Silent checkpoint renumbering** — drop a hard checkpoint, renumber the rest, tracker still says DONE | L6 dropped "adopt voted layout lib" + "LLM pane restyle" this way | Diff your tracker rows against the proposal's original numbering before writing DONE |
| A3 | **Stub artifact** — the artifact exists but is empty; scorecard cites it anyway | L7's eShop/TodoApi reports: `_No analysis data available._`, still counted in "18/21 FIXED" | Any generated report must be content-asserted (M0.1 does this); never cite an artifact you haven't opened |
| A4 | **Gate skipped, claimed run** — "Playwright-verified" with no screenshot in the tree | L6's mandated visual gate never ran | Gates produce files; no file ⇒ not run |
| A5 | **Verify where it's cheap** — prove an app-shape fix on library repos | L7 benched 6 libraries, skipped the app repos the fixes were for | Verify on the repo shape the claim is about |
| A6 | **Ship-without-launch** — code compiles, tool never executed once | The MCP server crashed on `GetRequiredService` at startup; "DONE" for two commits | First test of any executable: start it and make one real call |
| A7 | **Literal-string type matching** — match framework types by exact name only | `IRequestHandler<` literal missed all derived `ICommandHandler<,>` handlers | Resolve through the type model (base types, interface closure), never `StartsWith` on a type name, and handle generic arity |
| A8 | **Cross-scope name grabbing** — resolve a short name across project boundaries | `Product` (Shopping.Web) matched Ordering's context; produced the only — and false — impact result | Name resolution is scoped: same project, then referenced projects, never unrelated |
| A9 | **Framework noise as signal** — DI/infra artifacts presented as domain facts | `AddMassTransit → AddMassTransit` as a Bus *entry point*; Mapster `.Adapt` as a "downstream service" | Every entry/insight needs an evidence gate: would a dev call this an entry? |
| A10 | **Dead-end navigation** — UI navigates on strings hoping they resolve | Insight chips → `/explore?focus="27 no auth annotation"` | Only typed targets (D6). If you can't type it, don't link it |
| A11 | **Silent success UI** — button acts with zero visible feedback | Copy buttons across the app | Feedback affordance rule (M7.4): every action confirms visibly within 200ms |
| A12 | **The identical-canvas shortcut** — reuse one graph view everywhere because it exists | Same System canvas on every page | Each surface renders its lens (D9); reuse the engine, not the projection |
| A13 | **Fixture-shaped fixture** — test fixture written to match your code, not reality | several L0 fixtures pass while dogfood repo fails the same behavior | Derive fixture code from the dogfood repo's actual patterns (Carter modules, `Adapt<T>()`, BuildingBlocks interfaces) |
| A14 | **TODO-as-delivery** — leave `// TODO(agent)` where the feature was supposed to be | `GraphBuilder.cs:559` — the Sends edge TODO sat inside the "worked example" | A TODO in the diff of your own checkpoint = the checkpoint is not done |
| A15 | **Catch-and-continue** — swallow exceptions to keep a demo green | multiple `catch {}` in session paths | Catch specific exceptions; log with context; failures surface in status |

## 5. How to build, run, drive, test

Windows PowerShell 5.1. .NET SDK 10 (pinned via `global.json`). Node 24 + pnpm.

```powershell
# build everything (warnings are errors)
dotnet build DevContext.slnx
# engine changed? rebuild CLI before running it (its bin has its own Core copy)
dotnet build src/DevContext.Cli -clp:ErrorsOnly

# CLI against the dogfood repo (absolute paths always)
dotnet run --project src/DevContext.Cli --no-build -- report C:\Users\shahi\source\repos\run-aspnetcore-microservices\src -o out.md
dotnet run --project src/DevContext.Cli --no-build -- analyze <repo> --focus "POST /basket/checkout"

# tests (Eval category needs cloned eval repos; excluded by default)
dotnet test DevContext.slnx --filter "Category!=Eval"

# bench (M0.1 extends this; content-asserts reports)
powershell -File scripts/bench.ps1

# UI dev loop (src/DevContext.App)
pnpm check          # lint + unit + build — the UI gate
pnpm start          # dev server; desktop shell wraps the same app

# MCP server (stdio) — drive it with the QA harness, never by hand-typing JSON
# harness: eval/mcp-qa/ (from M0.2; seed script pattern: poll list_sessions in
# parallel with analyze until M3 fixes the flush bug)
dotnet build src/DevContext.Mcp -clp:ErrorsOnly
node eval/mcp-qa/run.js     # produces the scored table artifact

# Playwright visual gate — playwright-core + channel:'chrome' (no bundled chromium
# here). Scripts must include INTERACTION steps (zoom, select, lens switch), not just
# one screenshot per page: function-valued cytoscape styles freeze without
# cy.style().update() and only live interaction catches it.
```

Verification pattern that works (from the audit sessions): capture "before" artifact →
make the change → rebuild → capture "after" → diff textually → paste the diff summary
into the commit body. Use `> log; echo $?` style, never `cmd | tail` (it eats exit
codes).

## 6. Engine engineering directions (.NET / Roslyn / graph)

- **Where things live:** extraction pipeline in `DevContext.Core` (`Extractors/`,
  `Graph/GraphBuilder.cs`, `Graph/GraphQuery.cs`, `Pipeline/`). The
  detection→join pattern is documented on `AddHandlerJoins` (GraphBuilder) — every
  new seam follows it: extractor emits typed `Detection`s; GraphBuilder joins them
  into nodes/edges with FQN resolution via `NameResolver`; provenance = `file:line`
  on every edge.
- **Roslyn:** use the compilations already loaded via `SharedAnalysisContext` /
  syntax cache. Do NOT introduce `MSBuildWorkspace` (voted LATER), do not spin new
  ad-hoc compilations per extractor (perf budget below). Semantic lookups: prefer
  `SemanticModel.GetTypeInfo`/`GetSymbolInfo` at the call site you already have;
  compare symbols by fully-qualified metadata name, not display string. Generic
  types: match on name + arity (`ICommandHandler` + 2), and walk `AllInterfaces` /
  `BaseType` chains — never string-match a declaration list (A7).
- **Name resolution scope (A8):** same project → directly referenced projects →
  stop. Any cross-project edge must ride an explicit seam (Handles, ServiceLink,
  gRPC/proto join), never a bare short-name match.
- **Resolution honesty:** every edge carries `Resolution` (Join/Semantic/Syntactic).
  Body-scan guesses are `Syntactic` and render `[approx]`. Never upgrade a tier to
  make output look better; the ledger exists to be honest.
- **Immutability:** `AnalysisSnapshot`/graph are immutable records; builders mutate,
  snapshots don't. Follow `with`-expression update style seen in session managers.
- **Config parsing (M1.8):** YARP/Refit/gRPC endpoints live in `appsettings*.json` —
  parse with `System.Text.Json` (already a dependency), tolerate missing files, and
  record the config file:line as provenance. No new NuGet packages without a decision
  log entry in the proposal (that's a scope change — announce it).
- **Perf budgets (assert in bench):** dogfood repo full analysis ≤ 10s cold, ≤ 2s
  from snapshot; no extractor > 2.5s on dogfood (the run report table shows
  per-extractor timings — read it after your change).
- **Snapshot compatibility:** graph shape changes ⇒ bump `SnapshotSchema.Version`
  (stale caches must reject, not half-load). Forgetting this reintroduces the L7
  0-node-graph bug.

## 7. UI engineering directions (Angular 19 / Tauri / cytoscape)

- Signals-first: `signal`/`computed`/`effect`; no manual `subscribe` in components;
  derived state is `computed`, not recomputed in templates (a real finding: dedupe
  allocated per template evaluation). One `effect` per concern — a duplicated
  `effect()` caused double graph rebuilds.
- **cytoscape gotcha (recurring):** function-valued style properties are evaluated
  once; after zoom/pan-dependent changes call `cy.style().update()` or the style
  silently freezes at first paint. Any zoom-dependent behavior must have a Playwright
  interaction step proving it.
- **CSP:** desktop webview blocks ALL external hosts — no CDN scripts/fonts/styles.
  Bundle everything.
- Design tokens (M7.0 owns them): text ≥ 12px, icons 14–16px, per-kind colors from
  one registry (`KIND_ICONS`/kind color map) — never inline a one-off size/color.
- Navigation only via typed targets (D6). `routerLink` + `queryParams` built from
  typed objects; no string-splitting URLs in components.
- Feedback affordances (A11): use the shared toast/confirm primitives (M7.4 adds
  them); an action without visible confirmation fails review.
- State: stores in `src/app/state/*.store.ts` own RPC access; components never call
  gRPC clients directly. Session state is in-memory by design — full page reloads
  drop it; use client-side navigation.
- Keyboard model is a contract: Ctrl+1..6 tabs, Esc ladder, `?` overlay, j/k deck,
  Shift+E table. New surfaces (MCP page, Context Studio) must register in the same
  system, not invent parallel shortcuts.

## 8. Assertion recommendations (what "tested" means per artifact kind)

- **Graph/engine change** → golden test in `tests/DevContext.Core.Tests` derived from
  dogfood-repo patterns (A13) **plus** a bench assertion (M0.1) on the real repo:
  e.g. M1.1 asserts `Handles ≥ 14` on dogfood; M1.6 asserts a
  `ServiceLink(bus)` edge Basket.API→Ordering.Application exists with provenance.
  Content-assert rendered reports (markers like `TRACE`, step count), never just
  exit code.
- **Trace quality** → assert the *shape*: depth ≥ N, seams sequence contains
  `Entry→Call→Sends→Handles`, every step has file:line, `[approx]` only on
  Syntactic steps.
- **MCP tool** → QA harness case with: question, scripted calls, expected substrings,
  token ceiling. Ratchet ceilings down, never up, without a decision-log note.
  Include the two transport regressions (cold start; unprompted flush) in every run.
- **UI change** → `pnpm check` + Playwright script with interactions + screenshot
  diff eyeballed and committed under `eval-results/<date>/ui/`. A screenshot that
  wasn't looked at is A4.
- **Insight source** → for each emitted insight on dogfood: does the evidence list
  contain only real members of the claimed class? Write the "senior dev nod" table
  (insight → verdict → why) in the PR body.

## 9. Nominated UI/UX per surface (build THIS, don't improvise)

Weaker sessions must not invent layout. Each spec: purpose → structure → don'ts.

### UI-Home (M6.1) — "the repo card"
- Purpose: 10 seconds → what is this, how does it talk, where do I start.
- Structure, top to bottom: ① identity paragraph (prose, generated, claims are
  links); ② **service map hero** — deterministic layout (gateway left, services
  center by dependency rank, bus/broker bottom rail), ServiceLink arrows labeled by
  seam glyph (HTTP/gRPC/bus), edge click → Explore flow; ③ three tiles: *Entries by
  kind per service* (stacked bar), *Wiring health* (% entries with complete flows +
  link to unwired list), *Freshness* (analyzed when, HEAD drift chip); ④ Top Flows
  (real routes only, service-colored dots); ⑤ onboarding row: [Trace checkout] [Open
  atlas] [Point your agent here → MCP page].
- Don'ts: no raw counts without a verb ("474 types" banned), no "confidence %" chip
  (ledger lives in a diagnostics drawer), no physics layout for the hero.

### UI-Atlas (M6.2) — "the printable one-pager"
- Order: ① service diagram (hero, larger, edge counts); ② top 3–5 flows as
  horizontal **stepper strips** (service-colored chips, seam glyph between steps,
  click-through); ③ event wiring board (real ServiceLinks; columns publisher/event/
  consumer(s), unconsumed events highlighted); ④ per-service cards: style, stack,
  layer bar (Api/App/Domain/Infra proportions), entry count; ⑤ cross-cutting
  (behaviors pipeline, health checks); ⑥ hub radar. "Export one-pager" → single
  self-contained HTML.
- Don'ts: no raw CLI markdown anywhere; no section that is only a progress bar; empty
  states must say why + what would fill them (never reference internal stage names).

### UI-Explore (M7.1–7.3) — "graph and code, one motion"
- Three-pane: deck (left) · canvas (center) · inspector (right, tabs: Code | Node |
  Trail). Selecting a node = canvas focus + Code tab shows the full member,
  highlighted, file:line header with copy (feedback rule). Selecting a trace step =
  edge highlight + code line scroll. Esc: selection → focus → altitude, one level per
  press.
- Lens switcher top-left of canvas: Service | Layer | Feature | Flow — page remembers
  choice. Layer lens = horizontal bands with violation edges in warn color.
- Don'ts: never render the raw System canvas as the default answer to everything
  (A12); trail must not grow unbounded (group + cap, 7.3).

### UI-Table lens (M7.5) — "the repo spreadsheet"
- Data-dense table, virtualized; archetype-default columns (§M7.5 list); relationship
  chips ("shares OrderRepository with 3") open a popover listing the siblings, each a
  typed link. Row expand = mini flow stepper inline. Column picker persists per
  archetype. Export CSV.
- Don'ts: no free-text cells that are secretly links (chips only), no column without
  a tooltip defining it (metadata must be explainable or it's noise).

### UI-MCP page (M3.3) — "the agent cockpit"
- Rail entry with live status dot. Layout: top status card (running/stopped toggle,
  uptime, endpoint, per-host config snippet with copy buttons); left: sessions list
  (repo, age, calls, token total, evict button); right/main: **live feed** — one row
  per tool call: time, tool, args digest, ms, ~tokens; click expands raw
  request/response; header shows cumulative tokens + budget cap editor; bottom: "try
  a tool" console (tool dropdown → schema-driven form → run → response + cost).
- Don'ts: don't hide the feed behind a modal; don't show raw JSON by default
  (digest first, expand for raw); stopped state must clearly say what agents will
  experience ("connections refused").

### UI-Context Studio (M8) — "precise context, zero file dumping"
- Three-pane: scope picker left (tree: services → entries/types/flows + omnibox +
  presets incl. "I'm changing this endpoint"); composition center (cards in order,
  each: title, provenance chips, per-card body toggle, per-card token count, drag to
  reorder, × to remove); controls right (budget slider + total meter with per-card
  bars, intent selector, format, [Copy] [Save] with confirm states).
- The meter is honest: server-computed, same estimator as MCP; over-budget cards show
  trim preview.
- Don'ts: no client-side string assembly (must round-trip `ContextPackBuilder`); no
  "select all" default (start from the seed's minimal pack); Ctrl+E and the old LLM
  pane redirect here — no duplicate surfaces left alive.

### UI-Chrome (M7.4) — "the VS Code bar test"
- Top bar: workspace picker (repo name + branch/HEAD chip) | center omnibox (⌘K
  affordance visible) | right: MCP status dot + New Analysis. Height ≥ 40px, hit
  targets ≥ 28px.
- Left rail: icons + 2px active indicator bar + label on hover (or persistent labels
  ≥ 1280px width); badges ≥ 10px text, never overlap the icon.
- Tabs: height ≥ 32px, title + dirty/status dot, hover close, active tab = raised
  contrast + accent underline; overflow scrolls, never shrinks below readable.
- Status bar: segments [repo · nodes/edges/entries] [warnings] [engine ○ version]
  [theme]; every segment clickable to its detail; text ≥ 11px.

## 10. Model-specific notes

**Both:** never invent an API, file path, or RPC — grep first, cite the line in your
plan; if a gate can't run, BLOCKED beats improvised; prefer 5 small verified commits
to 1 heroic one; when the proposal and reality conflict, write the conflict into the
tracker and choose the smaller interpretation.

**Claude Sonnet 5:** your historic failure mode here is *declaring victory early*
(A1/A4 were Sonnet-session artifacts): you summarize confidently while the gate never
ran. Countermeasure: the LAST action of every checkpoint is running its gate and
pasting real output; if your summary contains a number (edges, tokens, %), it must
appear in a pasted artifact, not from memory.

**DeepSeek V4 Pro:** guard against *scope creep and drive-by refactors* — large diffs
that "clean up while here" bury the actual change and break the warnings-as-errors
build on style analyzers (MA/CA rules are strict). Countermeasure: diff budget — if
`git diff --stat` exceeds ~15 files or touches files your checkpoint didn't name,
split the commit or revert the extras. Also: don't translate existing idioms into
your own (records stay records, signals stay signals); match the file you're in.

**Session hygiene for both:** one checkpoint in flight at a time; re-read your
stage's Gate before the final commit; end with the §2 ritual — the next agent starts
from `MERIDIAN-START.md`, not from your chat transcript.
