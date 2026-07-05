# Meridian — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/meridian-agent-playbook.md`
(mandatory) → your stage in `docs/dev/briefs/proposal-meridian.md` →
`docs/dev/briefs/lighthouse-delivery-audit.md` (if your stage cites W-findings).
Branch: `feat/meridian-m0` off `feat/lighthouse-l2`. Dogfood repo:
`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff  (overwrite this block, ≤10 lines, no history)
last: 5305b12 + this commit — audit, plan v2, playbook, tracker created
stage: not started — M0 is next
gate: n/a — phase not begun
dirty: none
next: M0.1 — build the app-repo bench gate (content-asserted reports), then M0.2 MCP QA harness (seed from the audit session's mcp-dogfood.js pattern: poll list_sessions in parallel with analyze until M3 fixes the flush bug)
trap: MCP transport won't flush the analyze reply until the next inbound request (fixed only in M3); MCP DI startup crash already fixed in 5305b12

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence = artifact path produced by a
run this phase (a code path is not evidence). Scope changes get a `> scope change:`
line under the row — never silent renumbering.

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| M0.1 | App-repo bench gate (stub reports ⇒ fail; regen eShop/TodoApi) | TODO | | |
| M0.2 | MCP agent-QA harness (`eval/mcp-qa/`) + transport regressions | TODO | | |
| M0.3 | Playwright visual gate, 4 surfaces, interaction steps | TODO | | |
| M1.1 | Handler joins via interface closure (W1) — dogfood Handles ≥14 | TODO | | |
| M1.2 | Semantic Sends (Adapt/factory/local) (W2) | TODO | | |
| M1.3 | Trace traverses Sends→Handles→Raises (W3) | TODO | | |
| M1.4 | Project-scoped NameResolver (W5) | TODO | | |
| M1.5 | Razor routes real; bus entries de-noised; footer fix | TODO | | |
| M1.6 | ServiceLink: bus publish→consume (W4) | TODO | | |
| M1.7 | ServiceLink: gRPC client→server (W4) | TODO | | |
| M1.8 | ServiceLink: Refit/HttpClient + YARP route join (W4) | TODO | | |
| M1.9 | Microservices archetype + per-service style (D5) | TODO | | |
| M2.1 | Retire/repair discredited insight sources | TODO | | |
| M2.2 | Wiring-grounded insight classes | TODO | | |
| M2.3 | Typed insight actions end-to-end (D6) | TODO | | |
| M2.4 | Layer/feature facets + LayerViolation (D9, engine only) | TODO | | |
| M3.1 | Server-of-record; MCP = stdio shim; flush bug fixed at root | TODO | | |
| M3.2 | Tool descriptions + envelope trim (D4) | TODO | | |
| M3.3 | Tool-call event stream + dedicated MCP page (D8) | TODO | | |
| M4.1 | `overview` ≤600 tok | TODO | | |
| M4.2 | `resolve` with mandatory disambiguation | TODO | | |
| M4.3 | `flow` compact cross-service text (flagship) | TODO | | |
| M4.4 | `impact` transitive + diff-aware mode | TODO | | |
| M4.5 | `read_source` full-member mode | TODO | | |
| M4.6 | `find` paginated; lambda-title leak fixed at source | TODO | | |
| M4.7 | `config` keys → binding/consumption sites | TODO | | |
| M4.8 | `get_context` v2 (real content, cross-service) | TODO | | |
| M4.9 | `tests_for` best-effort | TODO | | |
| M5.1 | QA set → 5 repos + token ratchets | TODO | | |
| M5.2 | Real agent transcript (checkout question) committed | TODO | | |
| M5.3 | CI wiring (McpQa category + bench smoke) | TODO | | |
| M6.1 | Home repo card (spec §UI-Home) | TODO | | |
| M6.2 | Atlas one-pager + export (spec §UI-Atlas) | TODO | | |
| M7.0 | Design-token pass (12px/14–16px/contrast) | TODO | | |
| M7.1 | Graph↔code binding (spec §UI-Explore) | TODO | | |
| M7.2 | Lenses: Service/Layer/Feature/Flow; per-page defaults (D9) | TODO | | |
| M7.3 | Trail dedupe/group/cap; deck legibility | TODO | | |
| M7.4 | Chrome pass + feedback affordances (spec §UI-Chrome) | TODO | | |
| M7.5 | Table lens v2 (spec §UI-Table) | TODO | | |
| M8.1 | Context Studio surface + old panes retired (spec §UI-Context) | TODO | | |
| M8.2 | Composition model (cards/seeds/presets) | TODO | | |
| M8.3 | Budget/meter/intent/copy controls | TODO | | |
| M8.4 | Provenance + staleness + builder round-trip | TODO | | |
| M9.1 | Full bench incl. PowerToys + MassTransit | TODO | | |
| M9.2 | AUDIT.md with fresh-artifact verdicts only | TODO | | |
| M9.3 | HANDOVER-MERIDIAN.md + memory + tracker close | TODO | | |

## Quick commands

```powershell
dotnet build DevContext.slnx                                   # 0w 0e is the bar
dotnet test DevContext.slnx --filter "Category!=Eval"
powershell -File scripts/bench.ps1                             # M0.1 hardens this
node eval/mcp-qa/run.js                                        # exists after M0.2
cd src/DevContext.App; pnpm check                              # UI gate
dotnet run --project src/DevContext.Cli --no-build -- report <abs-repo-path> -o out.md
```

Baseline numbers (2026-07-05, pre-M0, dogfood repo): 474 nodes · 213 edges ·
36 entries · **2 Handles edges** · 0 ServiceLinks · checkout trace = 2 steps ·
`impact(CheckoutBasketCommandHandler)` = 0 results · MCP full session ≈ 9.7k tok.
Meridian exists to make every number in this line embarrassing.
