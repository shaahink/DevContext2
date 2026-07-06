# Meridian — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/meridian-agent-playbook.md`
(mandatory) → your stage in `docs/dev/briefs/proposal-meridian.md` →
`docs/dev/briefs/lighthouse-delivery-audit.md` (if your stage cites W-findings).
Branch: `feat/meridian-m0` off `feat/lighthouse-l2`. Dogfood repo:
`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff  (overwrite this block, ≤10 lines, no history)
last: 51f8239 feat(m1.8-m1.9): HTTP ServiceLinks + path-pattern normalization + per-service style rollup
stage: M1 — DONE (M1.8 HTTP route matching complete, M1.9 per-service style rollup complete)
gate: run — out.md (6 ServiceLinks: 1 bus, 1 gRPC, 4 http; 6 per-service styles; Style Microservices)
dirty: none
next: M2.1 — Retire/repair discredited insight sources (see proposal-meridian.md §M2)
trap: none

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence = artifact path produced by a
run this phase (a code path is not evidence). Scope changes get a `> scope change:`
line under the row — never silent renumbering.

| # | Checkpoint | Status | Commit | Evidence |
| |-----------|--------|--------|----------|
| M0.1 | App-repo bench gate (content-asserted reports) | DONE | e1e971b | eval-results/2026-07-05/ (19/22 reports, 0 stubs) |
| M0.2 | MCP agent-QA harness (`eval/mcp-qa/`) + transport regressions | DONE | fdfc45f | eval-results/2026-07-05/mcp-qa.md (5/5 QA baseline, transport checks green) |
| M0.3 | Playwright visual gate, 4 surfaces, interaction steps | DONE | 09f5215 | eval-results/2026-07-05/ui/ (8 screenshots, 1 interaction) |
| M1.1 | Handler joins via interface closure (W1) — dogfood Handles ≥14 | DONE | 9002d50 | out.md (Handles 2→18, golden ≥14) |
| M1.2 | Semantic Sends (Adapt/factory/local) (W2) | DONE | be1001e | out.md (Sends 19→26) |
| M1.3 | Trace traverses Sends→Handles→Raises (W3) | DONE | 2de8605 | DELETE /orders trace depth 5, TOUCHES populated |
| M1.4 | Project-scoped NameResolver (W5) | DONE | b80e6a7 | out.md (all resolve call sites updated, build+tests green) |
| M1.5 | Razor routes real; bus entries de-noised; footer fix | DONE | dca278c | out.md (GET /ProductDetail, Bus 3→1, 11 projects)
| M1.6 | ServiceLink: bus publish→consume (W4) | DONE | baf9c3d | out.md (1 bus ServiceLink: Basket.API→Ordering.Application) |
| M1.7 | ServiceLink: gRPC client→server (W4) | DONE | baf9c3d | out.md (1 gRPC ServiceLink: Basket.API→Discount.Grpc) |
| M1.8 | ServiceLink: Refit/HttpClient + YARP route join (W4) | DONE | 51f8239 | out.md (4 HTTP ServiceLinks: Shopping.Web→YarpApiGateway + 3 gateway→backend) |
| M1.9 | Microservices archetype + per-service style (D5) | DONE | 51f8239 | out.md (Style: Microservices, 6 per-service styles, MAP archetype, 6 runnable services) |
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

Baseline numbers (2026-07-06, post-M1 completion, dogfood repo): 493 nodes · 316 edges ·
36 entries · **18 Handles edges** · 26 Sends edges · **6 ServiceLinks** (bus=1, gRPC=1, http=4) ·
checkout trace depth 5 · 1 Bus entry (noise cleaned) · 11 projects in footer · Razor routes correct.
**Style: Microservices** (confidence high) — 6 runnable web services detected, 6 per-service styles.
