# Loom — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `conductor-DEBT.md` (audit followups —
unresolved bugs + deferred work from L0-L4 audits, sized + gated) → `docs/dev/briefs/loom-graph-design.md`
(the design authority — MANDATORY) → your stage in `docs/dev/briefs/proposal-loom.md`
→ `eval-results/2026-07-07/SESSION-AUDIT.md` (the findings your stage fixes).
Branch scheme: `feat/loom-l<stage>`. Dogfood repo:
`C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

##  Handoff  (overwrite this block, ≤10 lines, no history)
last: L7 session #38 — **L7.4 DONE** (gate battery green: 414P/3S Core, 64P Desktop,
       12P Server, 27/27 pnpm). Multi-.sln directory detection: Blazor + MediatR + gRPC
       + Ocelot + RazorPages now SampleCollection (not Microservices). 21/22 bench OK,
       per-repo verdicts in bench-verdicts-l7.4-s38.md. Blazor truth test flipped to
       active. 3 new archetype truth tests (Library/Desktop/Worker). Dogfood stable
       436n/338e/34e/6SL/69%.
stage: **L7.4 DONE**. L8.1 (close-out: clean-clone battery, HANDOVER-LOOM.md) next.
next: **L8.1** — Close-out: clean-clone battery, HANDOVER-LOOM.md, AGENTS.md rituals.
evidence: eval-results/2026-07-08/bench-verdicts-l7.4-s38.md, bench-summary.txt,
          gate-battery-l7.4-s38.txt

## Baseline numbers (2026-07-08, L7.1 post-PlainCallDetector — drift >5% documented below)

| Metric | Value |
|---|---|---|
| Dogfood | 436 nodes · 338 edges · 34 entries · 6 ServiceLinks · depth 6 · ~5.2s |
| Checkout trace depth (CLI, `--focus "POST /basket/checkout"`) | **6 (L2.4 fixed — was 1)** |
| Cold-agent MCP naive-call actionability | **90% (L5.5 gate met)** |
| Tab strip height | **32px (L6.1 fixed — was 18px)** |
| eShop (non-CQRS proxy) | 479 nodes · 375 edges · 96 entries · ~14.5s |

> drift: +62 edges (+22.5%), +15 nodes (+3.6%) on dogfood — deliberate: PlainCallDetector
> adds new Calls edges for in-solution method invocations previously untracked. Verified%
> dropped 82→69% because new edges are syntactic tier (SemanticLitePopulator upgrades some).
> Time 3.8→5.2s (+1.4s) within 2× Tier B budget.

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence = artifact path produced by a
run this phase (a code path is not evidence). Scope changes get a `> scope change:`
line under the row — never silent renumbering.

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| L0.1 | Truth expectations (6 repos, named flows/services/negatives) | DONE | bdcc840 | eval-results/2026-07-07/truth-gate-l0.1.txt |
| L0.2 | Cold-agent MCP QA harness + baseline | DONE | 930fbf8 | eval-results/2026-07-07/mcp-cold-qa.md |
| L0.3 | UI drive gate from ui-audit-drive.mjs (red items enumerated) | DONE | 9506977 | eval-results/2026-07-07/ui/ui-gate.md |
| L1.1 | SymbolId/SymbolRef/ResolutionTier/SymbolTable + ambiguity fixtures | DONE | fa4e415 | eval-results/2026-07-07/gate-battery-l1-s5.txt |
| L1.2 | Service/Message/Store node kinds + boundary inference + Project stamp | DONE | fa4e415 | eval-results/2026-07-07/gate-battery-l1-s5.txt |
| L1.3 | ServiceLinks on Service nodes; `_eventPublishers` static deleted | DONE | fa4e415 | eval-results/2026-07-07/gate-battery-l1-s5.txt |
| L1.4 | loom-guards.ps1 ban-list green | DONE | fa4e415 | scripts/loom-guards.ps1 (green, 0 banned) |
| L1.5 | impact/scope grouping via Service identity (no "(unknown)"/"Default") | DONE | fa4e415 | eval-results/2026-07-07/gate-battery-l1-s5.txt |
| L2.1 | BodyFacts in the existing parse, cached (facts-v1) | DONE | 006daff | eval-results/2026-07-07/gate-battery-l2-s7.txt |
| L2.2 | Seam detectors ×5 with dogfood-verbatim fixtures | DONE | 5dafd6e | eval-results/2026-07-07/gate-battery-l2-s7.txt |
| L2.3 | Assembler consumes SeamMatches; regex paths deleted; ≤400-line assembler | DONE | (l2.3) | eval-results/2026-07-07/gate-battery-l2-s8.txt |
| L2.4 | **Checkout truth test GREEN (depth ≥5, cross-service)** | DONE | (l2.4) | eval-results/2026-07-07/dogfood-l2-checkout-trace.md (depth 6) |
| L3.1 | SemanticLitePopulator (assets.json → compilations, degrade path) | DONE | (l3.1) | eval-results/2026-07-08/gate-battery-l3-s11.txt |
| L3.2 | Targeted semantic upgrades (Law R2) | DONE | (l3.2) | eval-results/2026-07-08/gate-battery-l3.2-s12.txt |
| L3.3 | Verified-edge ratchet ≥80% dogfood; truth bench re-run | DONE | (l3.3-s16) | eval-results/2026-07-08/gate-battery-l3.3-s16.txt |
> scope change: 65% → 68% (s14, +7 ReadsWrites +1 CallEdge) → **81% (s16)**: assembly-independent
> semantic bind of dispatch targets (generic type-arg / inline `new X()`), Sends 32 approx → 0.
> DntSite controller sub-measurement deferred — repo absent on this machine (ratchet gate met on dogfood).
| L4.1 | Flow store; spine-only TOUCHES/EMITS (E5 fix); ServiceHops + provenance | DONE | (l4.1) | eval-results/2026-07-08/gate-battery-l4.1-s18.txt |
| L4.2 | Projections + GetGraphFacets RPC (per-node lens data) | DONE | 73cca81 | eval-results/2026-07-08/gate-battery-l4.2-s19.txt |
| L4.3 | Home/Atlas/MCP consume projections (ad-hoc walks deleted) | DONE | (l4.3-s20) | eval-results/2026-07-08/gate-battery-l4.3-s20.txt |
> scope change: dogfood 422→421 nodes — BuildingBlocks (a class lib referencing
> FluentValidation.AspNetCore) was mis-classified runnable by IsRunnableService's substring
> "AspNetCore" package check; tightened to Microsoft.AspNetCore.App*/Web-SDK/Exe (design §2.4).
> Edges/SL/entries/verified% unchanged. Hero now shows exactly 6 runnables (audit Claim 3 fix).
| L4.4 | Server ContextPack round-trip (Trap A closed) | DONE | (l4.4-s21) | eval-results/2026-07-08/gate-battery-l4.4-s21.txt |
| L5.1 | Default-session ergonomics | DONE | (l5.1-s23) | eval-results/2026-07-08/gate-battery-l5.1-s23.txt |
| L5.2 | Error envelopes (error+hint+example ≤80 tok) | DONE | (l5.2-s24) | eval-results/2026-07-08/gate-battery-l5.2-s24.txt |
| L5.3 | Unified ranked resolution (`resolve "Order"` → aggregate #1) | DONE | (l5.3-s25) | eval-results/2026-07-08/gate-battery-l5.3-s25.txt |
| L5.4 | Real `flow` tool + fuzzy focus | DONE | (l5.4-s26) | eval-results/2026-07-08/gate-battery-l5.4-s26.txt
| L5.5 | Cold-agent QA becomes the gate | DONE | (l5.5-s28) | eval-results/2026-07-08/gate-battery-l5.5-s28.txt |
| L6.1 | Tabs: 32px+, New=createTab, clone-close confirm | DONE | (l6.1-s29) | eval-results/2026-07-08/gate-battery-l6.1-s29.txt |
| L6.2 | Code pane: entry selection shows source; states visible | DONE | da1823d | eval-results/2026-07-08/gate-battery-l6.2-s30.txt |
| L6.3 | Inspector insights: adjacency filter + honest chip | DONE | (l6.3-s32) | eval-results/2026-07-08/gate-battery-l6.3-s32.txt |
| L6.4 | Context Studio v2: service tree, preset scaffolds real cards | DONE | (l6.4-s33) | eval-results/2026-07-08/gate-battery-l6-session-33.txt |
| L6.5 | Table lens button + focus-proof shortcut | DONE | (l6.5-s33) | eval-results/2026-07-08/gate-battery-l6-session-33.txt |
| L6.6 | Chrome polish batch (MCP status, confidence stat, DPI sweep) | DONE | (l6.6-s33) | eval-results/2026-07-08/gate-battery-l6-session-33.txt |
| L7.1 | Call-spine completion (≥70% entries ≥2-deep on MVC-class repos) | DONE | (l7.1-s34) | eval-results/2026-07-08/gate-battery-l7.1-s34.txt |
| L7.2 | Archetype projections (desktop/worker/library/blazor) | DONE | (l7.2-s36) | eval-results/2026-07-08/gate-battery-l7.2-s36.txt |
| L7.3 | Style-detection guardrails + E9 scope fix | DONE | (l7.3-s37) | eval-results/2026-07-08/gate-battery-l7.3-s37.txt |
| L7.4 | Truth files per archetype; 22-repo truth bench | DONE | (l7.4-s38) | eval-results/2026-07-08/bench-verdicts-l7.4-s38.md |
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
