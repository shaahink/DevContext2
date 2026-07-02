# Go-To Program (2026-07-02) — docs index & live tracker

The engine/tool audit against the "go-to for every .NET repo" goal, the phased strategy, the faces
design, and the **execution iterations** written for agent sessions (DeepSeek v4 Pro). Branch
`docs/go-to-engine-audit` (base `develop` @ `7228d1e`).

## Read in this order (fresh session)

1. [`ENGINE-VALUE-AUDIT.md`](ENGINE-VALUE-AUDIT.md) — where the engine stands per repo shape; the
   hardening findings (incl. one confirmed bug); F1–F12 facet inventory; kernel-hygiene verdicts.
2. [`PROGRAM-PLAN.md`](PROGRAM-PLAN.md) — strategy phases V1–V5 with every fork voted.
3. [`FACES-DESIGN.md`](FACES-DESIGN.md) — CLI v2 signature, desktop UX direction, the Insights spec.
4. [`DEV-PAINS.md`](DEV-PAINS.md) — demand-side: pains → features, CORE/MENU/LATER tiers.
5. [`ITERATION-R2-verify-and-finish.md`](ITERATION-R2-verify-and-finish.md) — **current** round-2 plan:
   the verification result (what the tracker over-claims), the finding list, and the remaining UI/UX +
   engine work. MCP stays deferred.
6. The iteration guide whose Status below ≠ DONE. Do its Step 0 first; guides cite `file:line` as of
   authoring — verify before editing.

## Iteration tracker

| # | Guide | Phase | Depends on | Status |
|---|---|---|---|---|
| I1 | [Trust at breadth](ITERATION-I1-trust.md) — span bug, dispatch catalog, event type-sets, pattern zoo, hygiene | V1 | — | **DONE** (I1.5 deferred) |
| I2 | [CLI v2 + kernel wire](ITERATION-I2-cli-kernel.md) — W9 retirement, kernel JSON, `query`, flag sweep | V4→pulled fwd | I1 | **DONE** (eval migration + full W9 deletion deferred) |
| I3 | [Insights engine](ITERATION-I3-insights.md) — 10 sources, `--stats` reshape, desktop cards | V2/V3 | I2 (soft) | **PARTIAL** — 4/10 sources built but **not on the wire** (see [R2](ITERATION-R2-verify-and-finish.md) F1) |
| I4 | [Desktop UX](ITERATION-I4-desktop-ux.md) — node card, palette, smart sections, honesty ribbon, packs | V3/V7 | I2 (+I3) | **PARTIAL** — shell/node-card/palette/ribbon shipped; Insights view broken, no NodeLink/Graph/Settings (see [R2](ITERATION-R2-verify-and-finish.md)) |
| I5 | [Facet menu](ITERATION-I5-facet-menu.md) — F1–F13 pick-any (★ blast radius, message matrix) | V2/V3 | I1+I2 | **PARTIAL** — catalog + F13 only (F1–F12 open, see [R2](ITERATION-R2-verify-and-finish.md) E4) |
| **R2** | [**Verify & Finish**](ITERATION-R2-verify-and-finish.md) — insights-on-wire fix, NodeLink, Graph/Settings views, entries table, trace/palette/connection polish + engine carry-over | — | I2 | **CURRENT** — start here |
| I6 | [MCP server](ITERATION-I6-mcp.md) — stdio tools = GraphQuery ops | V4 | I2 | DEFERRED |
| I7 | [Benchmark + insights audit](ITERATION-I7-benchmark-audit.md) — new-shape repos, run, judge, ratchet | V5.4 | I3 | NOT STARTED |
| — | Scale backlog: persistent index · GitHub-URL hardening · huge-repo scoping · snapshot diff (P9) · tests lens (P13) | V5 | I2 | LATER |

**Order rule:** CORE spine is I1 → I2 → I3 → I4/I6; I5 picks interleave anywhere after I2; I7 closes
each batch. Bugs/trust always outrank features (the votes in PROGRAM-PLAN §0 explain why).

## Maintenance protocol (applies to every iteration)

1. **Docs move with code, same commit:** any flag/op change edits `docs/product/cli-reference.md`;
   any UI change edits `docs/product/desktop-ui.md`; any insight/facet edits its reference doc.
   The gate for each iteration includes "reference docs match `--help`/screenshot".
2. **Goldens are ratcheted, never silently re-baselined:** eval expectations and seam-count goldens
   change only with the before/after numbers in the commit message. New features land with a positive
   AND a negative expectation (absent gate ⇒ absent output).
3. **One wire contract:** anything a face shows must exist as a `GraphQuery` op / kernel JSON field
   first. If a face needs data the kernel can't answer, the kernel work comes first.
4. **Screenshots for UI work** — green checks don't catch dead UX (the WPF D1–D11 lesson).
5. Update this tracker's Status column (+ record I5 picks) at every iteration boundary.
