# Pre-registration deviations

`DESIGN.md` was written before any run, and §3 of it says the point of writing it first is that
the decision rule cannot be chosen after seeing the numbers. Where execution departed from that
document, it is recorded here — what changed, why, when it was decided, and which way it could
bend the result. Opened at the P2 boundary on an orchestrator flag; it covers everything found in
a sweep of DESIGN against the harness and the 54 recorded pilot rows, not only the flagged item.

**Nothing in this file changes a recorded number, and no cell was re-run because of it.**
Every deviation below was in force for **all three arms equally** unless the row says otherwise.
That is the load-bearing property: a control applied identically to G, M and B cannot bias the
paired within-question contrast that §4.1 reports. It can still limit how far the *absolute*
numbers generalise, which is what the "effect on the result" column is for.

The decision rule in §5, the arm definitions in §3.1, the question set and every answer key are
**untouched**. No threshold moved.

---

## D1 — `--bare` replaced by `--setting-sources ""` (DESIGN §6.3)

| | |
|---|---|
| Pre-registered | "every run uses `--bare` (skips hooks, LSP, plugin sync, auto-memory, and `CLAUDE.md` auto-discovery)" |
| Actual | `--setting-sources ""` on all 54 runs; recorded per row as `isolation: "no-settings-fallback"` |
| Decided | H1, before any timed run |
| Scope | All 54 runs, all three arms |

**Why.** `--bare` cannot authenticate on an OAuth-only machine: it reads `ANTHROPIC_API_KEY` or an
`apiKeyHelper` and nothing else, so every run returns "Not logged in". The harness selects the
fallback only when no API key is present (`ISOLATION = HAVE_API_KEY ? "bare" : ...`), records the
mode on every row, and refuses to start in fallback mode unless `--allow-no-bare` is passed
explicitly. It never engages by accident.

**Effect on the result.** The fallback reproduces the parts §6.3 names as the reason for the
control — no user/project/local settings, therefore no hooks and no ambient permissions; only the
MCP config passed on the command line (`--strict-mcp-config`); an explicit system prompt. It is
*verified*, not asserted: the audit's ambient-context table shows `memory paths: none`,
`apiKeySource: none`, `permission mode: default` and CLI `2.1.227` on all 54 runs, and the fixture
has no `CLAUDE.md` or `AGENTS.md` for auto-discovery to find. Residual risk is confined to LSP and
plugin sync, which are identical across arms. **Not a threat to the paired contrast.**

## D2 — run cap $1.50, not $2.00 (DESIGN §6.6)

| | |
|---|---|
| Pre-registered | "Cap every run at `--max-budget-usd 2.00`" |
| Actual | `--max-budget-usd 1.50` on every invocation; recorded per row as `maxBudgetUsd: 1.5` |
| Decided | Before H1, by the orchestrator's standing probe rules, not by a session |
| Scope | All 54 runs, all three arms |

**Why.** Each probe run is a separate headless `claude` process, so its spend never reaches the
conductor's cost accumulator and `maxRunCostUsd` cannot stop it. The per-invocation cap and the
60-runs-per-invocation ceiling are the only two brakes that exist. The cap was tightened to buy
margin against a runaway batch nobody could see.

**Effect on the result.** A tighter cap can only *increase* censoring, so the reported censoring
rate is an upper bound relative to the pre-registered design. One run censored across the pilot —
`eshop-c1/M/rep1`, `error_max_budget_usd`, $1.5134 — kept and flagged, never dropped, per §6.6. It
might have completed under $2.00. **The headline B-vs-G comparison is unaffected: no arm-B and no
arm-G run was censored.** The between-arm censoring spread (G 0%, M 5.6%, B 0%) stays inside
§6.6's 10-point comparability threshold either way.

## D3 — the tool-schema tax is reported on a second statistic (DESIGN §4.4)

| | |
|---|---|
| Pre-registered | "record turn-1 `input_tokens + cache_creation_input_tokens`. The delta is the tax" |
| Actual | That statistic **is** reported — it came back **9 tokens** — alongside a cache-state-invariant reading of the same two runs, ~**2535 tokens**, which is the number carried into the write-up |
| Decided | P1.2, after measuring, and it is a reporting change not a threshold change |
| Scope | The tax measurement only; no pilot run is affected |

**Why.** The literal statistic only measures the tax on a *cold* prefix. The server-side prompt
cache was already warm from P1.1 thirty minutes earlier, so the 22 schemas arrived as
`cache_read` (6396) rather than `cache_creation` (236), and the pre-registered subtraction
cancelled almost the whole tax. Adding `cache_read_input_tokens` to both sides makes the reading
invariant to cache state: B 6634 vs G 4094, delta 2540; rep2 gives 2535; and three
already-recorded `eshop-a1` rep-1 runs with `cache_read = 0` on all three give G 4163, B 6694,
delta 2531. Three independent measurements agree at ~2535 tokens, ~115 per schema.

**Effect on the result.** Both numbers are published with an explanation of which is which
(`results/p1.2-tool-schema-tax.md`). The 9-token figure is an artefact of cache state, not a
finding, and reporting it alone would have understated the treatment's fixed cost by ~99.6%.

## D4 — arm isolation is enforced by an exhaustive deny list (DESIGN §8 argv)

| | |
|---|---|
| Pre-registered | §8's argv blocks, e.g. arm G `--allowedTools "Read,Grep,Glob"` |
| Actual | Same intent, enforced through an exhaustive `--disallowedTools` universe; arm G and B also keep `Bash` |
| Decided | H1, after the first three real runs demonstrated it |
| Scope | All 54 runs |

**Why, and it is not a judgement call.** `--allowedTools` is an **auto-approve** list, not a
restriction: anything not named in `--disallowedTools` is still offered and still runs. Under
§8's literal argv, arm G executed three `Bash` calls and arm M — the arm whose entire purpose is
to have no filesystem — executed the subagent tool, whose subagent then read files with `cat` and
`ls`, plus five `Monitor` calls. Taking §8 literally would have **voided the experiment** under
probe rule 4. Each arm is therefore defined by denying the whole non-MCP tool universe minus what
that arm should have.

The `Bash` question is a genuine internal inconsistency in DESIGN, not a departure from it: §3.1
defines arm G as `Read`/`Grep`/`Glob`/`Bash(git *)` while §8's auto-approve line omits Bash. The
harness followed §3.1, on the grounds that denying the *control* arm a shell would bias the
experiment toward the treatment — the one direction this design must not lean.

**Effect on the result.** This is what makes assertions 2 and 3 pass 54/54. Arm G made 0 MCP
calls and arm M made 0 Read/Grep/Glob calls on every run, verified from transcripts by
`audit-preflight.mjs`, which shares no code with the harness.

## D5 — the per-run warmth check is a proxy, and it was narrowed (DESIGN §8.1)

| | |
|---|---|
| Pre-registered | "**Pre-flight assertions, run before every batch**" — 1. "`analyze` returns `cached: true` for every repo in every arm" |
| Actual | Batch-level bar met and evidenced (`results/p2.1-warm-gate.txt`). P1's *additional* per-run re-derivation now treats zero-analyze runs as `n/a` rather than FAIL |
| Decided | P2 repair session, recorded in the ledger **before** the edit |
| Scope | Auditor only; `run-probe.mjs` untouched, no number changed, no cell re-run |

**Why.** The pre-registered object is a per-batch pre-flight, and it passed: the warm gate
analysed the repo per arm and refused to start otherwise, with pass 2 reporting `cached=true` in a
**fresh** mcp process, which proves the cache is on disk for every later subprocess. The per-run
transcript check was P1's stricter proxy on top of that, and it had a false-positive mode — it
failed 9 arm-B runs that made zero MCP calls, on the grounds that warmth was "unproven from this
transcript". A run that calls no `analyze` performed no analysis and cannot have paid a cold cost,
so the hazard is structurally absent rather than unproven. Arm G was already `n/a` for exactly
that reason.

**Effect on the result.** None on detection power: a run that *does* call `analyze` must still
report `cached=true`. The 9 runs are reported more prominently than before, under their own
heading, because they are the pilot's headline finding — see below.

## D6 — the build SHA moved once mid-experiment (DESIGN §8.5)

| | |
|---|---|
| Pre-registered | "The DevContext build under test is a recorded git SHA, **pinned for the whole experiment**" |
| Actual | Two SHAs across the 54 rows: `e2f7372` on 3 runs, `8807f48` on the other 51 |
| Found | P2 boundary, by sweeping `runs.jsonl` rather than trusting the first row |
| Scope | 3 runs — `eshop-a1` rep 1, in **all three arms** |

**Why.** Three cells were recorded at H1.2 as the end-to-end smoke, then reused as pilot cells
rather than re-run. Between them and the batch, commit `8807f48` landed the pre-run harness
correction that separates a censored run from an infrastructure failure.

**Effect on the result.** Nil, and it is checkable rather than argued:

```
git diff --name-only e2f73724 8807f48e -- src/ proto/     # empty
```

The two SHAs differ **only** under `eval/agent-probe/**` — the harness, plus recorded results.
No engine, MCP-server or proto file changed, so the DevContext build actually under test is
identical across all 54 runs and the letter of §8.5 is met in substance. The three affected cells
are also one complete question × rep triple covering G, M **and** B, so even a real build
difference would have hit all three arms equally and could not bend the paired contrast.

The `repoSha` pin — the thing the answer keys describe — held perfectly: `9b4f9434` on all 54.
The harness re-checks it before every batch and refuses to run against a different tree.

---

## Not deviations, recorded so the question isn't reopened

- **3 reps, not 5.** §6.5 specifies `n = 5`; §9 specifies the **pilot** as 6 questions × 3 arms ×
  3 reps = 54 runs. The pilot ran to §9. `n = 5` governs the full run.
- **`--output-format stream-json --verbose`** instead of §8's `json`. §8 itself requires the
  stream-json transcript for tool-call counting and `mcp_call_share`; the result object is
  recovered from the stream's final event. Same data, one process instead of two.
- **The censored run was kept.** §6.6 requires it; it is in `runs.jsonl` scored incorrect at
  cost = cap. Infrastructure failures are a different event and are quarantined to
  `results/infra-failures.jsonl` with their spend reported separately — that classification was
  decided and recorded before any P2 result was seen.

## The thing that is a result, not a deviation

DESIGN §3.1's manipulation check for arm B **failed**: median `mcp_call_share` 0.01, 17 of 18 runs
below the 0.20 floor, with classes D, E and F using the MCP zero times across 3/3 reps each while
being offered 22 connected DevContext tools. §3.1 pre-committed to what happens in that case —
"the B-vs-G comparison is not a test of the MCP and must be reported as such, not as a null
result" — so R1.1 is bound by the pre-registration to report it that way. Detail in
`results/p2.1-pilot.md` and `results/p1.1-preflight-audit.md`.
