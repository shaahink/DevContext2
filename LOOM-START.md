# Loom — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/loom-graph-design.md`
(the design authority — MANDATORY) → your stage in `docs/dev/briefs/proposal-loom.md`
→ `eval-results/2026-07-07/SESSION-AUDIT.md` (the findings your stage fixes).
Branch scheme: `feat/loom-l<stage>`. Dogfood repo:
`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff  (overwrite this block, ≤10 lines, no history)
last: L0.1 truth expectations landed — TruthPendingAttribute, TruthExpectationTests (8 tests: 4 green
      baseline + 4 red-but-skipped ratchets for checkout/L1/RazorPages/L7/Blazor), bench.ps1 -Truth mode.
stage: L0 in progress. L0.1 DONE. L0.2–L0.3 next.
gate: dotnet build 0w/0e · tests green (Core 355/3skip, Server 12, Desktop 64) · pnpm check green · truth gate 4/4 pass
dirty: none
next: L0.2 cold-agent MCP QA harness (eval/mcp-qa/run-cold.js) OR L0.3 UI drive gate (ui-audit-drive.mjs promotion).
trap: do NOT write truth files from DevContext output — write from TARGET REPO SOURCE.

## Baseline numbers (2026-07-07, fresh runs — drift >5% without explanation blocks)

| Metric | Value |
|---|---|
| Dogfood | 493 nodes · 316 edges · 34 entries · 6 ServiceLinks · verified 59% · 3.9s |
| Checkout trace depth (CLI, `--focus "POST /basket/checkout"`) | **1 (broken — L2.4 target ≥5)** |
| Cold-agent MCP naive-call success | **0/15 (L5 target: actionable guidance ≥90%)** |
| Tab strip height | **17px (L6 target ≥30px)** |
| DntSite | 4,965 n · 2,160 e · 17.9s |
| MassTransit | 24,819 n · 2,929 e · 46.4s |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence = artifact path produced by a
run this phase (a code path is not evidence). Scope changes get a `> scope change:`
line under the row — never silent renumbering.

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| L0.1 | Truth expectations (6 repos, named flows/services/negatives) | DONE | 5084826 | eval-results/2026-07-07/truth-gate-l0.1.txt |
| L0.2 | Cold-agent MCP QA harness + baseline | TODO | | |
| L0.3 | UI drive gate from ui-audit-drive.mjs (red items enumerated) | TODO | | |
| L1.1 | SymbolId/SymbolRef/ResolutionTier/SymbolTable + ambiguity fixtures | TODO | | |
| L1.2 | Service/Message/Store node kinds + boundary inference + proto | TODO | | |
| L1.3 | ServiceLinks on Service nodes; `_eventPublishers` static deleted | TODO | | |
| L1.4 | loom-guards.ps1 ban-list green | TODO | | |
| L1.5 | impact/scope grouping via Service identity (no "(unknown)"/"Default") | TODO | | |
| L2.1 | BodyFacts in the existing parse, cached (facts-v1) | TODO | | |
| L2.2 | Seam detectors ×5 with dogfood-verbatim fixtures | TODO | | |
| L2.3 | Assembler consumes SeamMatches; regex paths deleted; ≤400-line assembler | TODO | | |
| L2.4 | **Checkout truth test GREEN (depth ≥5, cross-service)** | TODO | | |
| L3.1 | SemanticLitePopulator (assets.json → compilations, degrade path) | TODO | | |
| L3.2 | Targeted semantic upgrades (Law R2) | TODO | | |
| L3.3 | Verified-edge ratchet ≥80% dogfood; truth bench re-run | TODO | | |
| L4.1 | Flow store; spine-only TOUCHES/EMITS | TODO | | |
| L4.2 | Projections + GetGraphFacets RPC (per-node lens data) | TODO | | |
| L4.3 | Home/Atlas/MCP consume projections (ad-hoc walks deleted) | TODO | | |
| L4.4 | Server ContextPack round-trip (Trap A closed) | TODO | | |
| L5.1 | Default-session ergonomics | TODO | | |
| L5.2 | Error envelopes (error+hint+example ≤80 tok) | TODO | | |
| L5.3 | Unified ranked resolution (`resolve "Order"` → aggregate #1) | TODO | | |
| L5.4 | Real `flow` tool + fuzzy focus | TODO | | |
| L5.5 | Cold-agent QA becomes the gate | TODO | | |
| L6.1 | Tabs: 32px+, New=createTab, clone-close confirm | TODO | | |
| L6.2 | Code pane: entry selection shows source; states visible | TODO | | |
| L6.3 | Inspector insights: adjacency filter + honest chip | TODO | | |
| L6.4 | Context Studio v2: service tree, preset scaffolds real cards | TODO | | |
| L6.5 | Table lens button + focus-proof shortcut | TODO | | |
| L6.6 | Chrome polish batch (MCP status, confidence stat, DPI sweep) | TODO | | |
| L7.1 | Call-spine completion (≥70% entries ≥2-deep on MVC-class repos) | TODO | | |
| L7.2 | Archetype projections (desktop/worker/library/blazor) | TODO | | |
| L7.3 | Style-detection guardrails + E9 scope fix | TODO | | |
| L7.4 | Truth files per archetype; 22-repo truth bench | TODO | | |
| L8.1 | Close-out: clean-clone battery, HANDOVER-LOOM.md, AGENTS.md rituals | TODO | | |

## Quick commands

```powershell
dotnet build DevContext.slnx                                   # 0w 0e is the bar
dotnet test DevContext.slnx --filter "Category!=Eval"
powershell -File scripts/bench.ps1                             # presence bench (L0 adds -Truth)
node eval/mcp-qa/run.js                                        # scripted QA (kept as regression)
node eval/mcp-qa/run-cold.js                                   # L0.2+ cold-agent QA
cd src/DevContext.App; pnpm check                              # UI gate
node src/DevContext.App/scripts/ui-audit-drive.mjs             # UI drive gate (server+ng required)
dotnet run --project src/DevContext.Cli --no-build -- report <abs-repo-path> -o out.md
```
