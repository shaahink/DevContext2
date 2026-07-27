# R3 — Feature/UI redesign: the decision session

> Owner direction: a dedicated session to walk UI/feature possibilities as mock-ups (image or text),
> DECIDE the key features, then implement to the decided spec. This doc is that session's agenda +
> raw material. Read `eval-results/2026-07-27/ui-feature-audit/FINDINGS.md` first (evidence PNGs in
> `evidence/`).

## 1. How the session runs

1. Per decision area below: present 2–3 alternatives as mock-ups (ASCII/text wireframes inline;
   HTML mock-ups as artifacts when layout matters; annotated screenshots of the current state for
   contrast). Cheap before pretty.
2. Owner picks / merges; decision recorded in a DECISIONS.md with a one-line rationale.
3. Implementation only starts once the page's decision set is complete (avoid re-churning a page).
4. Respect PRODUCT-DIRECTION.md §3 (five artifacts: entry points · topology · trace · stats · browse)
   — a redesign reshapes these; adding a sixth needs an explicit owner override.

## 2. Decision areas (ranked by product impact)

### D-A. Workspace default + information architecture (the big one)
Current (VERIFIED): three panes; center EMPTY until selection ("Select an entry — j/k to scrub");
left = truncated flat entry list; right = permanently-collapsed accordions; toolbar = 3 unlabeled
control axes; graph defaults depth 1.
Alternatives to mock:
- A1 "Map-first": land on the topology canvas (services+transports); entry list as a filter overlay;
  selecting an entry re-roots the canvas into trace mode. One canvas, states, no Entries/Tree/Graph pills.
- A2 "Master-detail tree-first": entry list → rich expandable tree center (per-branch omission
  counts, verified/approx styling) → docked inspector right; Graph as a toggle of the SAME center.
- A3 "Dual mode" (closest to today, cleaned): keep Tree|Graph but labeled, depth budget-elastic,
  center defaults to topology when no focus.
Sub-decisions: node inspection = docked panel vs modal (kill the modal? — evidence says yes);
Trail/pin loop promoted to a visible right-panel section; controls labeling; j/k + Shift+E survive.

### D-B. Canvas semantic language (all canvases)
Decide ONE visual grammar: transport-labeled edge styles (http solid / grpc double / queue dashed-
arrow?), kind glyphs, store cylinders, dashed externals, orchestrator (AppHost) as grouping frame,
lanes on/off per view, verified-vs-approx edge rendering. Mock on eShop data AFTER R2 Batch B lands
(canvas mocks on today's starved data mislead). The EXPERIENCE-ADDENDUM §M "what an LLM sketches"
paragraph is the north star.

### D-C. Library experience
Current: workbench = good text tabs, ZERO graph anywhere, Atlas = empty-state page, Studio dead.
Decide: (1) library canvas — namespace map vs abstraction-inheritance wheel vs none; (2) type-rooted
Studio packs (pairs with the engine's type-rooted get_context — R2 §3); (3) Atlas for libraries —
suppress vs re-purpose (per-namespace breakdown, consumer-path board); (4) reconcile the two
public-type counts (92 vs 273 — which is the product number?).

### D-D. CliTool + sample-collection experiences
CliTool: command tree as the entry surface (table? tree? both?); what does Trace mean for a command
(Main → command handler → services)? Style chip suppressed.
Sample collection: per-sample cards (SamplesAreTheProduct) + the solution picker UX (also multi-sln
apps generally — banner + switcher).

### D-E. Home
Keep: identity strip, freshness, wiring health. Decide: recents/sessions on the landing hero;
START-HERE ranking rule (user-facing endpoints first); verified% chip semantics (one number, one
tooltip, consistent); does the mini-canvas stay on Home at all vs a static services thumbnail
linking to workspace?

### D-F. Insights
Mechanism keeps. Decide: dedup policy (one auth insight, not two); validity bar (command-level
validator counting; identity-server quickstart exclusion); should top-3 insights render into Map
markdown/CLI too (currently app+MCP only)?

### D-G. Studio
Keep the machinery wholesale (meters/preview/verification/omitted). Decide: type-rooted packs UI;
selection affordance (row click vs checkbox — today's row click doesn't select); fix state bugs
(selected-count, meter-vs-preview totals); is "Change-impact pack" the right frame or one of several
pack intents (trace/review/explain already exist as INTENT toggles)?

### D-H. Navigation + chrome
Icons+labels vs icon-only rail (badges currently REPLACE icons — "99+" is the Explore label);
status-bar hint rotation vs a real shortcuts/help surface; tab model (multi-repo tabs exist — keep).

## 3. Constraints from the other strands

- Canvas decisions (D-B) and workspace decisions (D-A) assume R2 Batches A+B (true edges, true
  handler chains, transports). Mock AFTER those land or mock explicitly against "future data".
- The render kernel (deferred in R2) should be built to serve whatever D-A/D-B decide — one
  MapDocument/TraceDocument contract, app/CLI/MCP as projections. Decide features here FIRST,
  then R2 builds the kernel to match.
- Screenshot-gate harness (`screenshot-gate.mts`, 16 checks) must be re-pointed at the redesigned
  pages as each decision is implemented — it's the regression net for this strand.

## 4. Assets

- Current-state screenshots: `eval-results/2026-07-27/ui-feature-audit/evidence/*.png`
- Reusable drivers for before/after captures: `src/DevContext.App/scripts/ui-redesign-drive*.mts`
- Prior design language work: D4 ELK canvas design pass (PRISM-START.md D4.1-D4.3), styleguide page.
