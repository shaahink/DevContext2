# Execution Roadmap — Sessions, Gates, and Your Intervention Points

> How to get from here to release with ~8 DeepSeek v4 Pro sessions and minimal human time.
> Strategy: **reform in place** (never rewrite — the detection logic + 232 tests are the
> asset), **linear** execution (Plans 1–3 share hot files: `DiscoveryPipeline`,
> `AnalysisService`, `MainViewModel`, both Razor panels), **one optional worktree** (S7).

## Session schedule

| # | Scope | Prompt to give the agent | Gate after (you, ~5 min) |
|---|-------|--------------------------|---------------------------|
| S0 | Plan 0, all (self-validation harness) | "Execute `docs/dev/plans/PLAN-0-SELF-VALIDATION.md` in full. Read `docs/product/DETECTION-GUIDE.md` §1 first — the eval expectations you seed encode that value model." | run `eval/gates.ps1` yourself once; skim the aspirational-fail list |
| S1 | Plan 1, Phases 0–3 | "Execute Phases 0–3 of `docs/dev/plans/PLAN-1-ANALYZE-ONCE-RENDER-MANY.md` exactly. Stop after Phase 3 verification. Read DESIGN-PHILOSOPHY.md P1/P2 first for intent." | G-core |
| S1b | Plan 1, Phase 3b (Scoring Model v2 — behavior-changing) | "Execute Phase 3b of PLAN-1 exactly. Read DETECTION-GUIDE.md §1 and §6 first. Golden/eval diffs are expected — list and justify every one." | G-core + read the justified diff list; AutoMapper eval must improve |
| S2 | Plan 1, Phases 4–5 | "Execute Phases 4–5 of PLAN-1. Phases 0–3b are done — verify by building first." | G-core + G-desktop |
| S3 | Plan 1, Phases 6–7 | "Execute Phases 6–7 of PLAN-1. If Stage-3 parallelization causes flaky tests, follow the plan's revert instruction." | G-core + record before/after timing |
| S4 | Plan 2, all | "Execute `docs/dev/plans/PLAN-2-UNIFIED-FOCUS-UX.md` in full." (If it stalls mid-desktop, finish desktop in S4b.) | G-core + CLI matrix from Plan 2 Phase 6 |
| S5 | Plan 3, Phases 0–2 | "Execute Phases 0–2 of `docs/dev/plans/PLAN-3-NERD-STATS.md`." | G-core + eyeball `--stats` output |
| S6 | Plan 3, Phases 3–4 | "Execute Phases 3–4 of PLAN-3." | G-desktop (Stats tab) |
| S7 | Plan 4, Phase 1 (extractor + service tests only; goldens need S5 done first) | "Execute Phase 1 of `docs/dev/plans/PLAN-4-WORLD-CLASS-REPO.md`." | G-core |
| S8 | Plan 4, Phases 0, 2–4 | "Execute Phase 0, then Phases 2–4 of PLAN-4. Phase 0 = work the aspirational eval checks following `docs/product/DETECTION-GUIDE.md`; flip fixed ones to `expected`. Unfixable ones go into the README limits section." | Read the README; skim remaining aspirational list |

Then **you alone**: Plan 4 Phase 5 (history reset checklist) → tag → release → record demo GIF.

## How to run a session (opencode + DeepSeek v4 Pro)

One session at a time, in table order, each in a **fresh** opencode session (clean context;
the plan file is the spec, not the previous chat). One git branch per plan
(`plan-0-validation`, `plan-1-snapshot`, …) off `develop`; merge after the plan's final gate.

Wrap every prompt from the table in this template:

```
You are executing a pre-written implementation plan in this repo. Rules:
1. Before any edit, read in full: docs/product/DESIGN-PHILOSOPHY.md and docs/dev/plans/<PLAN-FILE>.
   If the work touches extractors, scoring, or pruning, also read docs/product/DETECTION-GUIDE.md
   — it is binding.
2. Task: 01
3. Execute the plan EXACTLY as written, phase by phase, in order. Do not expand scope, do
   not refactor neighboring code, do not start the next plan. The plan's "Ground rules"
   override your defaults.
4. Build after every phase: dotnet build DevContext.sln. Run the tests the plan names.
5. If a plan instruction conflicts with the code you find, do NOT improvise: write
   "DEVIATION:" with the conflict and your minimal resolution, or stop with "BLOCKED:".
6. When done: run eval/gates.ps1 (if it exists yet) and paste the tail. List files changed,
   tests added, DEVIATION/BLOCKED lines, goldens regenerated (with justification).
7. Commit at each phase boundary: "PLAN-<N> Phase <X>: <summary>".
```

Fix-up session (gate failed — use once per plan, then escalate to a stronger model):

```
The previous session executed Phases <X–Y> of docs/dev/plans/<PLAN-FILE> but the gate fails.
Gate output: <PASTE FAILING TAIL>
Read the plan and DESIGN-PHILOSOPHY.md first. Fix with the smallest change consistent with
the plan's intent. Do not continue to later phases. End by running eval/gates.ps1.
```

## Gates (self-validating from S0 onward)

**G-core** — `pwsh eval/gates.ps1` (exists after S0). It runs build, fast tests, the eval
expectation suite, and a `--strict` CLI matrix (output self-checks become exit codes), ending
in `GATE: PASS/FAIL`. **The agent runs it too** — every session prompt implicitly ends with
"run `eval/gates.ps1` and paste the tail; a session ending on GATE: FAIL is not done." Your
job shrinks to reading the tail and the aspirational-check delta (it must never grow).

Until S0 lands, G-core is the manual version:
```powershell
dotnet build DevContext.sln
dotnet test
dotnet run --project src/DevContext.Cli -- analyze . --max-tokens 4000
```

**G-desktop** — once per plan that touches Desktop:
```powershell
dotnet run --project src/DevContext.Desktop
```
Analyze this repo → toggle 2 sections → move token slider → switch all tabs → Copy LLM.
Pass = no freeze, toggles don't re-run analysis (progress text must NOT reappear), output updates.

**Failed gate protocol:** don't debug yourself — start a fix-up session: paste the gate output
+ "fix this; the intent is in docs/dev/plans/PLAN-N section X". Budget one fix-up session per plan;
if a second is needed, that's the signal to involve a stronger model on that plan instead.

## Worktree policy

- **S7 only** may run in a `git worktree` parallel to S5/S6 (purely additive test files, zero
  overlap). Merge S7 after S6.
- Everything else linear. Two agents editing `MainViewModel`/`AnalysisService` concurrently
  produces merge conflicts whose resolution is riskier than the time saved.

## Realistic budget

8 sessions + up to 4 fix-up sessions + your gates ≈ **1–2 focused days** wall time if run
back-to-back. The long pole is S1/S2 (the snapshot/lens refactor); if S1 goes badly twice,
stop and reassess scope rather than iterating — the plan's phase boundaries exist so a partial
result (e.g. scoring split done, lens not) still builds and tests green.
