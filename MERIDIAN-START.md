# Meridian — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/meridian-agent-playbook.md`
(mandatory) → your stage in `docs/dev/briefs/proposal-meridian.md` →
`docs/dev/briefs/lighthouse-delivery-audit.md` (if your stage cites W-findings).
Branch: `feat/meridian-m0` off `feat/lighthouse-l2`. Dogfood repo:
`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff  (overwrite this block, ≤10 lines, no history)
last: M9.3 — HANDOVER-MERIDIAN.md + PROGRESS-LOG + tracker close. Full bench 22/22 green (PowerToys 5141n, MassTransit 24819n). AUDIT.md written. Phase closed.
stage: M9 — DONE (full bench, AUDIT.md, HANDOVER-MERIDIAN.md, tracker closed)
gate: dotnet build 0w 0e, dotnet test green, pnpm check green (lint 0/0, test 27/27, build 0w/0e), bench 22/22 repos pass
dirty: none
next: Engine Gaps 1-2 (read_source RPC → layer/feature uplumb). Plan in src/DevContext.App/AGENTS.md:133-189.
trap: buildContext() still client-side v0; stale banner needs freshness probe RPC; proto regeneration can be finicky (need correct protoc/plugin versions)

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
| M2.1 | Retire/repair discredited insight sources | DONE | 4b109ea | out.md (CLI leaks retired, ServiceLink-only evidence, DI-excluded dead-code, Razor filter) |
| M2.2 | Wiring-grounded insight classes | DONE | 4b109ea | out.md (4 new sources: EventFlow, Spof, UnvalidatedEndpoints, ConfigDefaults) |
| M2.3 | Typed insight actions end-to-end (D6) | DONE | 4b109ea | out.md (TypedAction Focus/Node/Filter engine→proto→UI, pnpm check green) |
| M2.4 | Layer/feature facets + LayerViolation (D9, engine only) | DONE | 4b109ea | out.md (InferLayer + DeriveFeature + DetectLayerViolations, dotnet build 0w) |
| M3.1 | Server-of-record; MCP = stdio shim; flush bug fixed at root | DONE | c90fb0b | out.md (repo+HEAD keyed, shim proxies to gRPC, Analyze flush restructured) |
| M3.2 | Tool descriptions + envelope trim (D4) | DONE | c90fb0b | out.md (18 tool XML docs with examples, compact meta envelopes) |
| M3.3 | Tool-call event stream + dedicated MCP page (D8) | DONE | c90fb0b | out.md (/mcp page with status dot, config snippets, session list, live feed, try-a-tool) |
| M4.1 | `overview` ≤600 tok | DONE | cc37ea6 | out.md |
| M4.2 | `resolve` with mandatory disambiguation | DONE | cc37ea6 | out.md |
| M4.3 | `flow` compact cross-service text (flagship) | DONE | cc37ea6 | out.md |
| M4.4 | `impact` transitive + diff-aware mode | DONE | 45981ee | out.md |
| M4.5 | `read_source` full-member mode | DONE | cc37ea6 | out.md |
| M4.6 | `find` paginated; lambda-title leak fixed at source | DONE | cc37ea6 | out.md |
| M4.7 | `config` keys → binding/consumption sites | DONE | 45981ee | out.md |
| M4.8 | `get_context` v2 (real content, cross-service) | DONE | cc37ea6 | out.md |
| M4.9 | `tests_for` best-effort | DONE | 45981ee | out.md |
| M4.G | QA gate: 8/8 passing, checkout ≤3c/2k tok, MCP-VS-GREP | DONE | 8531882 | eval-results/2026-07-06/ |
| M5.1 | QA set → 5 repos + token ratchets | DONE | 8e63f38 | eval/mcp-qa/run-multi.js + eval-results/2026-07-06/m5-ratchet.json (5 repos, 38 calls, 6889 tok) |
| M5.2 | Real agent transcript (checkout question) committed | DONE | 8e63f38 | eval/mcp-qa/record-transcript.js + eval-results/2026-07-06/agent-transcript.md (2 calls, 313 tok, gate PASS) |
| M5.3 | CI wiring (McpQa category + bench smoke) | DONE | 8e63f38 | tests/DevContext.Core.Tests/McpQaGateTests.cs (2 tests: harness gate + bench smoke check) |
| M6.1 | Home repo card (spec §UI-Home) | DONE | | src/DevContext.App/src/app/features/pages/home-page.ts + features/shared/ (service-map-hero, home-tiles, onboarding-row) |
| M6.2 | Atlas one-pager + export (spec §UI-Atlas) | DONE | | src/DevContext.App/src/app/features/pages/atlas-page.ts + features/shared/ (flow-stepper, service-cards, export via clipboard) |
| M7.0 | Design-token pass (12px/14–16px/contrast) | DONE | efa02ff | 14 files, pnpm check green |
| M7.1 | Graph↔code binding (spec §UI-Explore) | DONE | c7affae | Code tab, highlightedNodeId, prismjs |
| M7.2 | Lenses: Service/Layer/Feature/Flow; per-page defaults | DONE | 691d319 | lens-switcher.ts, lens→altitude effect |
| M7.3 | Trail dedupe/group/cap; deck legibility | DONE | c3e7078 | TrailFlowGroup, groupedBreadcrumb, kind-colored dots |
| M7.4 | Chrome pass + feedback affordances (spec §UI-Chrome) | DONE | 1d2f26d | titlebar 40px, rail hover labels, statusbar clickable |
| M7.5 | Table lens v2 (spec §UI-Table) | DONE | e0ca191 | CDK-virtualized, archetype columns, CSV export |
| M8.1 | Context Studio surface + old panes retired (spec §UI-Context) | DONE | 2781d56 | context-studio/ (scope-picker.ts, composition-view.ts, budget-panel.ts, context-studio.ts) |
| M8.2 | Composition model (cards/seeds/presets) | DONE | 2781d56 | 9 card types, getContext wired, preset, omnibox, drag-drop, trail seeds, format/intent |
| M8.3 | Budget/meter/server-token wiring | DONE | 08f4340 | budget-panel.ts (model-based budget, serverTokens), context-studio.ts (budget→RPC, sectionTokens) |
| M8.4 | Provenance chips + per-section tokens | DONE | 08f4340 | composition-view.ts (provenance chips, server token badge, section breakdown), context-studio.ts (provenance from entries) |
| M9.1 | Full bench incl. PowerToys + MassTransit | DONE | (m9.1) | eval-results/2026-07-07/ (22/22 reports, 0 stubs) |
| M9.2 | AUDIT.md with fresh-artifact verdicts only | DONE | (m9.2) | eval-results/2026-07-07/AUDIT.md |
| M9.3 | HANDOVER-MERIDIAN.md + memory + tracker close | DONE | (m9.3) | docs/dev/HANDOVER-MERIDIAN.md |

## Quick commands

```powershell
dotnet build DevContext.slnx                                   # 0w 0e is the bar
dotnet test DevContext.slnx --filter "Category!=Eval"
dotnet test DevContext.slnx --filter "Category=McpQa"          # M5.3 — MCP QA gate (slow, ~130s)
powershell -File scripts/bench.ps1                             # M0.1 hardens this
node eval/mcp-qa/run.js                                        # single-repo QA (dogfood)
node eval/mcp-qa/run-multi.js                                  # M5.1 — multi-repo QA (5 repos)
cd src/DevContext.App; pnpm check                              # UI gate
dotnet run --project src/DevContext.Cli --no-build -- report <abs-repo-path> -o out.md
```

Baseline numbers (2026-07-06, post-M3 completion, dogfood repo): 493 nodes · 316 edges ·
36 entries · 6 ServiceLinks · 10 insights · Analyzed in 3.6s.
**M3 adds:** MCP exe is thin gRPC proxy (stripped in-process engine), server-of-record with
repo+HEAD session keying, 18 MCP tools with XML doc descriptions, /mcp Angular page
with live feed + config snippets + session list + try-a-tool console.
