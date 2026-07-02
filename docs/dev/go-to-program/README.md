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
| I3 | [Insights engine](ITERATION-I3-insights.md) — 10 sources, `--stats` reshape, desktop cards | V2/V3 | I2 (soft) | **PARTIAL** — 4/10 sources **on wire** (R2.1). Remaining 6 → E1. |
| I4 | [Desktop UX](ITERATION-I4-desktop-ux.md) — node card, palette, smart sections, honesty ribbon, packs | V3/V7 | I2 (+I3) | **PARTIAL** — shell/node-card/palette/ribbon shipped; Insights view, NodeLink, Graph, Settings, Entries table, trace/palette/connection/overview/export all delivered (R2). MCP deferred. |
| I5 | [Facet menu](ITERATION-I5-facet-menu.md) — F1–F13 pick-any (★ blast radius, message matrix) | V2/V3 | I1+I2 | **PARTIAL** — catalog + F13 only (F1–F12 open, see E4) |
| **R2** | [**Verify & Finish**](ITERATION-R2-verify-and-finish.md) — insights-on-wire fix, NodeLink, Graph/Settings views, entries table, trace/palette/connection polish + engine carry-over | — | I2 | **DONE** — R2.1–R2.10 delivered. E1–E5 engine carry-over still pending. |
| I6 | [MCP server](ITERATION-I6-mcp.md) — stdio tools = GraphQuery ops | V4 | I2 | DEFERRED |
| I7 | [Benchmark + insights audit](ITERATION-I7-benchmark-audit.md) — new-shape repos, run, judge, ratchet | V5.4 | I3 | NOT STARTED |
| I8 | [Caching & storage](ITERATION-I8-caching-storage.md) — repo-hash snapshot cache, clone consolidation, Settings→Storage | V5 | I2 | NOT STARTED |
| I9 | [Release readiness](ITERATION-I9-release-readiness.md) — about/updates/logs/errors, CLI polish floor | V7 | I4, I8 | NOT STARTED |
| I10 | [Workspace tabs](ITERATION-I10-workspace-tabs.md) — up to 6 repos, VS Code-grade tab strip, memory-honest | V7 | I4 (+I8) | NOT STARTED |
| A | [Harder repos](ADDENDUM-A-harder-repos.md) — F14 EF depth ★, F15 build intelligence ★, extended insights | V3/V5 | I1, I2 | NOT STARTED |
| — | Scale backlog: persistent index · GitHub-URL hardening · huge-repo scoping · snapshot diff (P9) · tests lens (P13) | V5 | I2 | LATER |

**Order rule:** CORE spine is I1 → I2 → I3 → I4 → **I8 → I10 → I9** (+ I6 MCP anywhere after I2;
I5/A picks interleave; I7 closes each batch and now includes A5's repos). Bugs/trust always outrank
features. Addendum rationale: I8 before I10 (tabs need snapshot rehydration), I9 last (release gate
audits everything shipped before it).

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
