# Loom — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/loom-graph-design.md`
(the design authority — MANDATORY) → your stage in `docs/dev/briefs/proposal-loom.md`
→ `eval-results/2026-07-07/SESSION-AUDIT.md` (the findings your stage fixes).
Branch scheme: `feat/loom-l<stage>`. Dogfood repo:
`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff  (overwrite this block, ≤10 lines, no history)
last: L0 session #3 — QA'd s2 (L0.1 genuine; ratcheted 2 green-washing vectors in truth tests, tightened
      only). Delivered L0.2 (cold-agent harness, 0/12 baseline) + L0.3 (UI drive gate, 1/4 pass, 3 red w/owners).
stage: **L0 COMPLETE** (L0.1 ✅ L0.2 ✅ L0.3 ✅). Truth harness live; all red items enumerated with owner stage.
gate: dotnet build 0w/0e · tests (Core 355P/3S, Server 12P, Desktop 64P) · pnpm 27/27 · MCP QA 8/8 · truth 4P/4S
QA verdict s2: L0.1 DONE & real; checkout truth ratcheted ≥2→≥5+cross-service, service-libs negative encoded.
next: **start L1 (identity spine)** — new branch feat/loom-l1 off here. L1.1 SymbolId/SymbolRef/tiers/SymbolTable.
trap: kill DevContext.Server before build (DLL lock — bit me again, a stray server was on :5179); a healthy
      server may linger from prior sessions/conductor; do NOT write truth files from DevContext output.
evidence: eval-results/2026-07-07/{gate-battery-l0-s3.txt, mcp-cold-qa.md, ui/ui-gate.md}. UI D-preset already green (M8).

## Baseline numbers (2026-07-07, fresh runs — drift >5% without explanation blocks)

| Metric | Value |
|---|---|
| Dogfood | 493 nodes · 316 edges · 34 entries · 6 ServiceLinks · verified 59% · 3.9s |
| Checkout trace depth (CLI, `--focus "POST /basket/checkout"`) | **1 (broken — L2.4 target ≥5)** |
| Cold-agent MCP naive-call actionability | **0/12 (run-cold.js; L5.5 target ≥90%)** |
| Tab strip height | **18px (L6.1 target ≥30px; ui-gate.md)** |
| DntSite | 4,965 n · 2,160 e · 17.9s |
| MassTransit | 24,819 n · 2,929 e · 46.4s |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence = artifact path produced by a
run this phase (a code path is not evidence). Scope changes get a `> scope change:`
line under the row — never silent renumbering.

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| L0.1 | Truth expectations (6 repos, named flows/services/negatives) | DONE | bdcc840 | eval-results/2026-07-07/truth-gate-l0.1.txt |
| L0.2 | Cold-agent MCP QA harness + baseline | DONE | 930fbf8 | eval-results/2026-07-07/mcp-cold-qa.md |
| L0.3 | UI drive gate from ui-audit-drive.mjs (red items enumerated) | DONE | 9506977 | eval-results/2026-07-07/ui/ui-gate.md |
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
