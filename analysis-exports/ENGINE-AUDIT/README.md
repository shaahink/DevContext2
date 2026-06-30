# Engine Audit — resumable workspace

**North star:** DevContext should be the *go-to lens for **any** .NET repo*. Drop the tool on a repo and a
developer (or their LLM) should, **at a glance**, learn:

1. **What is this?** — archetype (web app / library / desktop / gateway / microservices / worker / CLI…),
   stack, topology, architecture style.
2. **How do I use it / where do I start?** — the real **entry points** (and nothing fake: no test assets,
   samples, or scaffolding).
3. **How does any part work?** — **focus/trace** into an entry or type and follow it *down the wiring*
   (calls, DI, dispatch, data, events), indirection bridged.

We assess this by running the **engine + CLI output** (the same artifacts the desktop UI shows) against a
**matrix of deliberately different .NET repos** and closing the gaps in **graph, entry points, and
trace dive-ins**. Output quality is the product; detection counts are only a substrate.

> **Token budgeting is explicitly *not* the core concern** (and is a candidate for removal — see
> Theme E). The prior DeepSeek analysis mis-attributed most gaps to the token budget; the budget governs a
> dead legacy catalog path and never touches the Map/Trace. Don't spend effort there.

---

## How this workspace is organised (read in this order)

| File | Purpose |
|------|---------|
| `README.md` (this) | North star, status snapshot, resume protocol. **Start here.** |
| `OUTPUT-CONTRACT.md` | **What info each output MUST contain** to answer Q1/Q2/Q3 per archetype — the gradeable spec + scorecard. Grade every repo against this. |
| `PHASED-PLAN.md` | **Execution order** (phases 10.A–10.F) with exit gates as acceptance ratchets. The "what to do next, in what order." |
| `BENCHMARK-MATRIX.md` | The assessment grid: repo × archetype × Q1/Q2/Q3 × gaps. The scoreboard, plus archetype-coverage gaps to add. |
| `WORKITEMS.md` | The work items (W1–W9) grouped by theme, each with **locus + fix + self-VERIFY**. The executable detail the phases reference. |
| `PROGRESS-LOG.md` | Append-only session log: what each agent did, what it verified, what's next. |
| `../VERIFIED-PLAN.md` | The audit **verdicts**: which DeepSeek claim is confirmed/misdiagnosed and *why* (evidence + real loci). Reference, not a task list. |
| `../GAPS-AND-ISSUES.md`, `../ACTION-PLAN-FOR-AGENT.md` | The original DeepSeek reports (raw input, not ground truth — several root causes are wrong; see VERIFIED-PLAN). |

**Canonical contract in the main repo** (this workspace extends, never contradicts):
`docs/product/ACCEPTANCE.md` (acceptable output per artifact + phase ratchet) ·
`docs/product/IDEAL-OUTPUT-TARGET.md` (the ideal Map/Trace/library-surface shape) ·
`docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md` (phases 0–10; this audit executes **Phase 10**).

Analysis artifacts: `../{serilog,ocelot,files,aspnetcore}/{map,trace-*}.md` (the captured outputs).
Cloned repos: `../../analysis-repos/{serilog,ocelot,files,aspnetcore}` (for re-running).

---

## Status snapshot

Branch: `feat/engine-cross-repo-analysis`. Legend: ✅ done · 🔄 in progress · ⬜ todo · 🔬 research.

| Theme | ID | Item | Status |
|-------|----|------|--------|
| **B · Entry-point fidelity** | W1 | Exclude test-asset / stress / template entries from inventory | ✅ committed `b4321d8` — aspnetcore 518→10, full gate green (eval 27/27) |
| **C · Trace dive-in** | W3 | Library / type-rooted traces follow member call edges | ⬜ |
| **C · Trace dive-in** | W3b | Honest message when a focus resolves but has no out-edges | ⬜ |
| **A · Archetype** | W5 | Desktop-app archetype + entry points (WinUI/WPF/Avalonia); fix auxiliary-exe heuristic | ⬜ |
| **D · At-a-glance Map** | W4 | Structural section caps + ranking for huge repos (NOT token-driven) | ⬜ |
| **B · Entry-point fidelity** | W6 | Prefer the product solution when several sit at repo root | ⬜ |
| **A · Archetype** | W7 | API-gateway / reverse-proxy archetype (Ocelot, YARP) | ⬜ |
| **B · Entry-point fidelity** | W8 | Entry→target fallback for view/no-call controller actions | ⬜ |
| **E · Engine hygiene** | W9 | Quarantine/retire the legacy catalog + token machinery | 🔬 |
| **Coverage** | C-* | Add missing archetypes to the benchmark matrix (CLI, worker, gRPC, Blazor, MAUI, MVC, serverless) | 🔬 |

Phased order (see `PHASED-PLAN.md`): **10.A** entries/honesty (W1✅, W6, W8, L2) → **10.B** dive-in (W3, W3b) →
**10.C** archetypes (W5, W7) → **10.D** readability (W4); **10.E** coverage + **10.F** hygiene (W9) in parallel.

---

## Resume protocol (for the next agent)

**Cold start:** read this README → `PHASED-PLAN.md` (what's next + order) → `OUTPUT-CONTRACT.md` (the bar you're
coding to). Then run the per-item loop:

1. **Pick** the current phase's next item from `PHASED-PLAN.md` (or the one the user named). `WORKITEMS.md` has
   its locus + fix + VERIFY.
2. **Reproduce** the gap from the captured artifact in `../<repo>/`, then re-run live to confirm it still repros:
   ```powershell
   # absolute path required — a relative "owner/repo"-shaped path is treated as a GitHub clone target
   dotnet run --no-build -c Release --project src/DevContext.Cli -- `
     analyze C:/code/DevContext2-analysis/analysis-repos/<repo> [-f "<focus>"] -o <out.md> --stats
   ```
3. **Implement** the fix at the named locus. Reform in place; do not rewrite extractors.
4. **Verify** with the item's VERIFY block, then grade the re-captured output against `OUTPUT-CONTRACT.md`
   (the relevant Q1/Q2/Q3 cell must flip to PASS). Gate must stay green:
   ```powershell
   dotnet build DevContext.slnx                                 # 0 warnings (analyzer warnings = errors)
   dotnet test  DevContext.slnx --filter "Category!=Eval"       # fast tests
   powershell -File eval/gates.ps1                              # full gate incl. eval expectations
   ```
   Do-not-regress anchors: `BudgetIndependenceTests` (Map/Trace stays budget-independent),
   `TraceQualityTests` sibling-divergence Facts (the narrow handler bridge stays narrow).
5. **Ratchet:** flip the item's eval check `aspirational` → `expected` in the **same commit** that fixes it.
6. **Record:** update the scorecard in `OUTPUT-CONTRACT.md` + `BENCHMARK-MATRIX.md`, the status tables (here +
   `WORKITEMS.md`), and append a dated `PROGRESS-LOG.md` entry (changed · verified · next). Commit.

A phase is **done** only when its `PHASED-PLAN.md` exit gate is green. "Go-to for any .NET repo" is reached when
every `BENCHMARK-MATRIX.md` row has all of Q1/Q2/Q3 + noise PASS.

**State of truth lives in the repo, not in chat:** code + tests + eval fixtures + these four docs. Anyone can
resume from a cold start by reading them in the order above.
