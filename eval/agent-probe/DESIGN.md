# Agent Probe — does the MCP make a code-browsing agent more effective?

> Pre-registration. Written before any run. The point of writing it first is that the
> decision rule (§5) cannot be chosen after seeing the numbers.
>
> Lineage: this is the fresh-agent A/B that `IDEAL-OUTPUT-TARGET.md` §7 specified and
> `docs/dev/archive/reports/probe-kit.md` scoped, and that
> `docs/dev/reports/phase1-member-origin-reprobe.md` §4 explicitly deferred as
> "a human-in-the-loop step". The standing verdict on record is **"primer, not
> accelerator."** This design exists to replace that verdict with a measurement.

---

## 1. What already exists, and the gap this fills

| Existing | What it measures | What it can't tell you |
|---|---|---|
| `eval/expectations/*.json` (~30 repos) | Does the engine produce the right map/trace/entries? | Nothing about agents |
| `eval/graph-truth.ps1` + matrix | Is the graph correct per defect class? | Nothing about agents |
| `eval/mcp-qa/run.js` | Does a **scripted** call sequence answer 11 QA questions, and does the checkout question fit in ≤3 calls / ≤2k tokens? | The calls are hand-written. It tests the *tool contract*, not whether an agent left to its own devices is better off |
| `eval/mcp-qa/dogfood.js` | Per-call `ms`, `chars`, `tokens` into `call-log.jsonl` + raw responses | It's a transport for a human driving the tools, not an experiment |

Every one of those measures **DevContext**. None measures **an agent using DevContext**.
The product claim — "let AI agents query your codebase instead of grepping it" (README) —
has never been tested against the thing it claims to beat.

That is the gap: **there is no grep baseline anywhere in the repo.** The one place the
comparison is named at all is the informal "ripgrep test" in `PRODUCT-DIRECTION.md` §9,
which is a bar for *usefulness*, not a controlled comparison.

---

## 2. The question, stated so it can come out "no"

> For a fresh agent answering questions about an unfamiliar .NET repository, does
> having the DevContext MCP server available reduce the cost of reaching a correct
> answer, without reducing how often it is correct?

Three ways this can fail, all of which the design must be able to detect:

- **Null** — the MCP arm costs the same and is right as often. The tools are neutral; the token cost of 22 tool schemas is a pure tax.
- **Primer** — the MCP arm is *more correct* but costs the same or more. That is the verdict already on record. Valuable, but it is not the claim on the README.
- **Harmful** — the MCP arm is cheaper but *less correct*, because a confident wrong graph answer stops the agent looking. This is the outcome that matters most and the one a naive "did it get cheaper?" experiment would miss.

---

## 3. Design

### 3.1 Arms

Three arms, same model, same prompt, same repo, fresh session each time.

| Arm | Tools available | Tests |
|---|---|---|
| **G** (grep baseline / control) | `Read`, `Grep`, `Glob`, `Bash(git *)` — **no MCP** | What an agent does today |
| **M** (MCP only) | `mcp__devcontext__*` — **no file tools** | Is the MCP *sufficient*? Isolates the graph from the file system |
| **B** (both / treatment) | Everything in G plus everything in M | The real-world configuration |

Arm **M** is not decoration. Without it, arm B's result is unattributable: if B ≈ G, you
cannot distinguish "the tools don't help" from "the agent never called them". M forces
the tools to carry the whole task, and the M-vs-B gap tells you how much of B's behaviour
is actually MCP-driven. Record `mcp_call_share` (MCP calls ÷ all tool calls) in arm B as a
manipulation check: **if `mcp_call_share` < 0.2 across arm B, the B-vs-G comparison is not
a test of the MCP and must be reported as such, not as a null result.**

### 3.2 Repositories

Four, chosen for *shape* and for one specific confound (§6.1).

| Repo | Shape | Why |
|---|---|---|
| `eval-repos/eShop` @ `9b4f9434` | CQRS / MediatR / integration events, 7 services | The hard case: cross-project indirection grep genuinely can't follow |
| `eval-repos/TodoApi` @ `307a1ead` | Minimal API, small | Floor check — on a repo this small, grep should win. If the MCP wins here too, suspect the design |
| `eval-repos/FluentValidation` @ `94397908` | Library, no entry points | Library surface, a different question shape entirely |
| **One private/unseen repo** | Anything | **Load-bearing.** See §6.1 |

### 3.3 Task set

Per repo, 6 questions across the classes below (24 items total). Each is a **question with a
written answer key**, produced by reading the repo by hand *before any run*, and stored as
`eval/agent-probe/questions/<repo>.json`.

| Class | n/repo | Example (eShop) | Why it's in the set |
|---|---|---|---|
| **A. Orientation** | 1 | "What kind of system is this and what are its entry points?" | Cheap, high-frequency, where `overview` should shine |
| **B. Indirection** | 2 | "`POST /basket/checkout` — trace it to where the order is persisted." | The core claim. Send → handler → domain event → EF is exactly what grep can't follow |
| **C. Impact** | 1 | "What breaks if I change `CheckoutBasketCommandHandler`?" | Set-valued answer; scoring is recall/precision against the key, not 0/1 |
| **D. Attribution trap** | 1 | "Does `CatalogApi.CreateItem` publish `ProductPriceChangedIntegrationEvent`?" | **True answer: no** — `UpdateItem` does. An agent reading the file sees the event in the class and can mis-attribute. This is the exact defect Phase 1 fixed (`phase1-member-origin-reprobe.md`), so the graph is *known* to be right here |
| **E. Negative control** | ½ | "Which handlers consume `<event that nothing consumes>`?" | True answer is "none". Detects fabrication in **both** arms |
| **F. grep-favouring control** | ½ | "Which files contain the literal string `X`?" | If the MCP arm wins here, something is wrong with the harness — this is a sanity check on the design, not on the product |

Classes E and F are what keep this honest. A design with only classes B and C is built to
produce a win.

**Selection discipline:** the questions are written from the repo, not from the tool
catalogue. Do not write a question because `seam` answers it well. Write the questions a
new engineer actually asks in week one, then see which tool covers them.

---

## 4. Metrics

Everything below comes out of a single `claude -p --output-format json` result object.

### 4.1 Primary endpoint

**Cost to a correct answer**, paired by question:

```
cost_ratio(q) = median(cost_B(q, reps)) / median(cost_G(q, reps))
```

reported as the median of `log2(cost_ratio)` across questions, with a bootstrap 95% CI.
Log-ratio because token distributions are heavy-tailed and a mean over raw dollars is
dominated by one rabbit-holing run.

`cost` is `total_cost_usd` from the JSON result. **Trap:** on a subscription/OAuth account
`total_cost_usd` comes back `0`. Verify it is non-zero on the pilot's first run; if it is
zero, compute cost from `usage` + `modelUsage` at published rates (Opus 5 `$5 / $25` per
MTok; Sonnet 5 `$3 / $15`, intro `$2 / $10` through 2026-08-31; Haiku 4.5 `$1 / $5`) and
price `cache_creation_input_tokens` at 1.25× input and `cache_read_input_tokens` at 0.1×.

### 4.2 Co-primary: correctness

Per-item score against the answer key (§7). Arm B must be **non-inferior** to arm G:
the lower bound of the 90% CI on `(accuracy_B − accuracy_G)` must exceed `−0.05`.

A cost win with a correctness loss is not a win. This is the single most important line
in the document.

### 4.3 Secondary

| Metric | Source | Why |
|---|---|---|
| `num_turns`, tool-call count | result JSON / transcript | Latency proxy, and the closest thing to the existing "≤3 calls" gate |
| `duration_ms` | result JSON | Wall clock, the thing a user actually feels |
| `fabrication_rate` | grader flag | Asserted a file/symbol/edge that does not exist. Counted separately from wrong-but-honest |
| `mcp_call_share` | transcript, arm B | Manipulation check (§3.1) |
| `citation_accuracy` | grader | Do the cited `file:line` refs resolve? Cheap, mechanical, and it catches plausible-sounding wrong answers |
| **Tool-schema tax** | §4.4 | The MCP's fixed cost, which the treatment must earn back |

### 4.4 The tool-schema tax — measure it, don't hide it

22 tool descriptions enter the system prompt in arms M and B. The descriptions in
`docs/product/mcp-reference.md` are long (the `neighbors` and `trace` entries run to
paragraphs). That is a real, unavoidable cost of the treatment and it must appear in the
headline number.

Measure it directly: run the same trivial prompt (`"reply with the word ok"`) in arm G and
arm B and record turn-1 `input_tokens + cache_creation_input_tokens`. The delta is the tax.
Report it as an absolute token count and as a percentage of median run cost.

It is paid once per session as a cache write (1.25×) and then read at ~0.1× per turn, so
it hurts short runs most — which is exactly where class A and F questions live. Do not
average it away.

### 4.5 Not measured, and why

- **Cold `analyze` time.** A first analysis of a large repo takes minutes. Amortising it into per-question cost would be dishonest in either direction. Instead: **warm every arm before the clock starts** (analyse once, confirm `cached: true`), report cold analysis cost separately as a one-off setup number, and state plainly that the headline figure is the warm case.

---

## 5. Decision rule (fixed before running)

Mapped onto the repo's own vocabulary from `ACCEPTANCE.md`:

| Result | Verdict |
|---|---|
| Correctness non-inferior **and** median `log2(cost_ratio)` CI upper bound < `−0.32` (≥20% cheaper) | **Accelerator.** The README claim is earned. Ship the number |
| Correctness improves (CI lower bound on Δaccuracy > `+0.05`) but cost not reduced | **Primer.** Unchanged from the 2026-06 verdict. Reposition the product on accuracy, not token savings |
| Neither CI excludes zero | **Null.** The tools are a tax. Either the tool descriptions need work or the surface is wrong |
| Correctness non-inferiority fails | **Regression.** Stop and find out which questions; a confident wrong graph answer is worse than no tool |

The 20% threshold is not arbitrary: below it the result is swamped by the tool-schema tax
on short questions and is not a claim anyone should put on a CV or a README.

---

## 6. Confounds, and what is done about each

### 6.1 Pretraining contamination — the one that could invalidate everything

eShop, TodoApi, and FluentValidation are famous public repos. The model may have memorised
them. That inflates arm G (grep + recall beats grep alone) and *understates* the treatment.
It could equally cut the other way if recall is stale and wrong.

**Mitigation, and it is not optional:** at least one repo in the set must be one the model
cannot have seen — a private repo, or a public one with a mechanical identifier rename
(types, methods, namespaces, routes) applied before the run. **The headline number is
reported per repo, and the unseen repo's number is reported first.** If the effect exists
only on the famous repos, the honest conclusion is that the experiment measured recall.

### 6.2 Prompt leakage in the tool descriptions

The MCP tool descriptions name domain concepts — "MediatR handlers", "seams",
"integration events". Some of the treatment effect is the *descriptions* teaching the agent
what to look for, independent of the graph. This is legitimately part of the product, but
it must be stated in the write-up rather than discovered by a reader. Arm M vs arm B partly
separates it.

### 6.3 Ambient context bleeding into the runs

`CLAUDE.md`, hooks, auto-memory, project settings, and installed plugins all differ from
machine to machine and would silently change behaviour between arms.

**Control:** every run uses `--bare` (skips hooks, LSP, plugin sync, auto-memory, and
`CLAUDE.md` auto-discovery), `--strict-mcp-config` (only the MCP config passed on the
command line), and `--setting-sources` empty. The system prompt is supplied explicitly and
is byte-identical across arms.

### 6.4 Grader bias

**Blind grading.** The graded artifact is the agent's **final answer text only** — never the
transcript, because the tool calls reveal the arm. Arm labels are stripped and items are
shuffled before grading.

### 6.5 Stochasticity

`n = 5` repetitions per (question × arm), fresh session each. Report medians, not means.
Question order randomised; arms run interleaved, not blocked, so a mid-experiment change in
API latency or model routing hits all arms equally.

### 6.6 Runaway runs

A rabbit-holing baseline run would distort the mean and, worse, could be quietly excluded
later. **Cap every run at `--max-budget-usd 2.00`.** A run that hits the cap is scored
**incorrect with cost = cap** (right-censored), never dropped. Censoring rate per arm is
reported; if it differs by more than 10 points between arms, the medians are not comparable
and the write-up says so.

---

## 7. Grading protocol

Two passes, both mechanical where possible.

**Pass 1 — deterministic.** Answer keys carry `must_mention` (symbols/files that must
appear) and `must_not_mention` (the trap answers, e.g. `ProductPriceChangedIntegrationEvent`
for class D). Class E items score correct only on an explicit "none". Citation refs are
resolved against the repo at the pinned SHA by script. No model involved.

**Pass 2 — LLM judge**, for the parts a string match can't settle (class B trace ordering,
class C set overlap, explanation quality).

- Model: `claude-opus-5`, fresh session per item, `--bare`, effort `high`.
- Input: question + answer key + the anonymised final answer. **Nothing else.**
- Output: structured — `{correct: bool, fabricated: bool, missing: [], extra: []}`.
- **Validation:** a human grades a 20% stratified sample. Compute Cohen's κ against the judge. **If κ < 0.8, the judge's scores are discarded and the whole set is graded by hand.** A judge nobody checked is a random number generator with good manners.

---

## 8. Harness

Build on what exists. `eval/mcp-qa/dogfood.js` already has the per-call token logging; the
new part is driving a *real agent* rather than a scripted call list. The runner is
`claude -p` in headless mode — it reports usage and cost directly and enforces the arm
boundary through its own permission system, so the isolation is not something the harness
has to fake.

```powershell
# Arm G — grep baseline. No MCP server is even configured.
claude -p --bare --output-format json `
  --model claude-opus-5 `
  --add-dir C:\Code\DevContext2\eval-repos\eShop `
  --allowedTools "Read,Grep,Glob" `
  --disallowedTools "Edit,Write,WebFetch,WebSearch" `
  --max-budget-usd 2.00 `
  --system-prompt-file eval\agent-probe\system.txt `
  "$question"

# Arm M — MCP only. --strict-mcp-config keeps any user-level MCP config out.
claude -p --bare --output-format json `
  --model claude-opus-5 `
  --mcp-config eval\agent-probe\mcp.json --strict-mcp-config `
  --allowedTools "mcp__devcontext" `
  --disallowedTools "Read,Grep,Glob,Bash,Edit,Write" `
  --max-budget-usd 2.00 `
  --system-prompt-file eval\agent-probe\system.txt `
  "$question"

# Arm B — both.
claude -p --bare --output-format json `
  --model claude-opus-5 `
  --add-dir C:\Code\DevContext2\eval-repos\eShop `
  --mcp-config eval\agent-probe\mcp.json --strict-mcp-config `
  --allowedTools "Read,Grep,Glob,mcp__devcontext" `
  --disallowedTools "Edit,Write,WebFetch,WebSearch" `
  --max-budget-usd 2.00 `
  --system-prompt-file eval\agent-probe\system.txt `
  "$question"
```

Per run, persist: the full result JSON, the `--output-format stream-json` transcript (for
tool-call counting and `mcp_call_share`), and a row in `runs.jsonl` keyed
`(repo, question_id, arm, rep, model)`.

**Pre-flight assertions, run before every batch — a batch that fails any of these is void:**

1. `analyze` returns `cached: true` for every repo in every arm (warm, per §4.5).
2. Arm G's transcript contains **zero** `mcp__` tool calls.
3. Arm M's transcript contains **zero** `Read`/`Grep`/`Glob` calls.
4. `total_cost_usd` is non-zero on the first run (§4.1).
5. The DevContext build under test is a recorded git SHA, pinned for the whole experiment.

---

## 9. Sample size and analysis

- Pilot: 1 repo (eShop) × 6 questions × 3 arms × 3 reps = **54 runs**.
- Full: 4 repos × 6 questions × 3 arms × 5 reps = **360 runs**.

Analysis is paired by question. Bootstrap (10,000 resamples over questions) the median
`log2(cost_ratio)`; Wilcoxon signed-rank as a distribution-free companion. **No t-tests on
raw token counts** — the distribution is not normal and one censored run would carry the
result.

Accuracy differences: Wilson score interval on the paired difference.

---

## 10. Cost and time

Rough, from the run shape (60–150k input tokens with caching, 2–5k output):

| Phase | Runs | Estimated |
|---|---|---|
| Pilot | 54 | ~$25–40 |
| Full | 360 | ~$150–250 |
| Judging | ~450 items | ~$15 |

Comparable to the Conductor run's `$224.20`, and reported the same way — actual spend
recorded, not estimated, in the write-up.

**Run the pilot first and stop.** Its job is not to produce a result; it is to find out
whether the harness is measuring anything: does `mcp_call_share` clear 0.2, does
`total_cost_usd` populate, do the arm-isolation assertions hold, is the between-rep variance
small enough that n=5 can resolve a 20% effect. Read those four numbers, fix what is broken,
then commit to the full run.

---

## 11. Extensions (only after the main result lands)

- **Model × arm.** Repeat arms G and B on `claude-sonnet-5`. The commercially interesting claim is not "Opus gets cheaper" but *"the MCP lets a cheaper model match a more expensive one's grep-only accuracy"* — a 3× cost story rather than a 20% one. This is the strongest possible finding and the reason to keep the harness parameterised by model from day one.
- **Implementation task.** The original probe-kit task ("add a per-line discount to orders") as a secondary endpoint, scored by does-it-build plus a rubric. Expensive to grade; worth it only once the Q&A result is in.
- **Per-tool attribution.** Which of the 22 tools appear in winning traces. Feeds the folding decisions that already removed `flow`, `insights`, and `interesting_points`.

---

## 12. Outputs

- `eval-results/<date>/agent-probe/RESULTS.md` — the numbers, per repo, unseen repo first.
- `runs.jsonl` + raw result JSON — so any number in the write-up can be re-derived.
- A one-line verdict in `PRODUCT-DIRECTION.md` §9 replacing "primer, not accelerator" with whatever this actually finds, including if that is "still a primer".
