# Agent probe — pilot results

**Does the DevContext MCP server make a fresh code-browsing agent more effective than
`Read`/`Grep`/`Glob`?**

Pilot, 2026-08-11. One repo (`eval-repos/eShop` @ `9b4f9434`), 6 questions × 3 arms × 3 reps =
**54 runs**, `claude-opus-5`. Pre-registration: [`eval/agent-probe/DESIGN.md`](../../eval/agent-probe/DESIGN.md),
written before any run. Departures: [`DEVIATIONS.md`](../../eval/agent-probe/DEVIATIONS.md), D1–D7.
Every number below is computed by `eval/agent-probe/analyse.mjs` from `results/runs.jsonl`,
`results/graded.jsonl` and `results/judged.jsonl`, and is re-derivable; where this file does
arithmetic over published numbers it says so on the line.

---

## Verdict

**The B-vs-G comparison did not test the MCP. None of the four pre-registered outcomes is
earned by it, and reporting it as a Null would break the pre-registration.**

DESIGN §3.1 fixed the manipulation check and what to do if it fails, before any run:

> Record `mcp_call_share` (MCP calls ÷ all tool calls) in arm B as a manipulation check:
> **if `mcp_call_share` < 0.2 across arm B, the B-vs-G comparison is not a test of the MCP and
> must be reported as such, not as a null result.**

Arm B's median `mcp_call_share` is **0.015**, and **17 of 18 runs** are below the 0.20 floor.
The check failed, so §3.1 binds this report: the treatment arm was offered the tools and largely
did not use them, therefore the B-vs-G numbers measure an agent that mostly grepped against an
agent that only grepped. That is a fact about tool adoption, not a measurement of the graph.

**What the experiment did test, and the answer is negative.** Arm M — the MCP as the *sole*
source of truth, `mcp_call_share` 1.00 on 18/18 — is a genuine test and it came out against the
product: **12/18 correct (66.7%) against arm G's 18/18, at 1.508× the cost** (Wilcoxon
p = 0.0313, the smallest value n = 6 can produce). The MCP alone is not sufficient to answer the
question set. Detail in §5 below.

**The number the decision rule mechanically produces, reported because the rule produces it:**
**Null**, with the **Regression** branch also firing on a power artifact that must not be read as
a regression (§6). Both are stated in full below and neither is the headline, because §3.1
disqualifies the contrast they are computed from.

---

## 1. The manipulation check, in full

| Arm | median `mcp_call_share` | runs below 0.20 | reading |
|---|---|---|---|
| G | 0.000 | 18/18 | 0 by construction — arm isolation held |
| M | 1.000 | 0/18 | clears |
| B | **0.015** | **17/18** | **below the floor — the manipulation did not take** |

The median hides the shape, and the shape is the finding. Per question, arm B:

| Question | Class | reps with 0 MCP calls | median MCP calls | median file calls | median share | offered at init |
|---|---|---|---|---|---|---|
| `eshop-a1` | A orientation | 0/3 | 4.0 | 3.0 | 0.17 | 22 tools, connected |
| `eshop-b1` | B indirection | 0/3 | 2.0 | 16.0 | 0.12 | 22 tools, connected |
| `eshop-c1` | C impact | 0/3 | 2.0 | 29.0 | 0.06 | 22 tools, connected |
| `eshop-d1` | D attribution trap | **3/3** | 0.0 | 4.0 | 0.00 | 22 tools, connected |
| `eshop-e1` | E negative control | **3/3** | 0.0 | 5.0 | 0.00 | 22 tools, connected |
| `eshop-f1` | F grep control | **3/3** | 0.0 | 1.0 | 0.00 | 22 tools, connected |

`offered at init` is recorded from each run's own MCP handshake: 22 DevContext tools present,
server `connected`. A run with zero MCP calls **chose** not to call them; it was not unable to.
On half the question set — including the class-D attribution trap that DevContext's Phase 1
member-origin fix was built for, where the graph is *known* to be right — the agent reached for
the MCP zero times in 3/3 reps.

Even where arm B did use the tools, it never cleared the floor: the highest per-question share is
0.17 on orientation. This is the pilot's primary finding and it is a product finding, not a
statistical one. DESIGN §5's Null row names the two candidate causes — "either the tool
descriptions need work or the surface is wrong" — and this pilot does not distinguish between
them.

---

## 2. Primary endpoint — cost, paired by question (DESIGN §4.1)

`cost_ratio(q) = median(cost_B(q, reps)) / median(cost_G(q, reps))`, reported as the median of
`log2(cost_ratio)` across questions with a percentile bootstrap CI (seed `20260811`, 10,000
resamples over **questions**). No run excluded, including the censored one.

| Question | Class | median $ G | median $ M | median $ B | B/G | log2 | M/G | log2 |
|---|---|---|---|---|---|---|---|---|
| `eshop-a1` | A | 0.6105 | 0.6761 | 0.4879 | 0.799 | −0.323 | 1.108 | +0.147 |
| `eshop-b1` | B | 0.4137 | 0.9792 | 0.4951 | 1.197 | +0.259 | 2.367 | +1.243 |
| `eshop-c1` | C | 0.9532 | 1.3463 | 0.9613 | 1.008 | +0.012 | 1.412 | +0.498 |
| `eshop-d1` | D | 0.0663 | 0.1012 | 0.0777 | 1.172 | +0.229 | 1.527 | +0.610 |
| `eshop-e1` | E | 0.1207 | 0.1798 | 0.0840 | 0.696 | −0.523 | 1.490 | +0.575 |
| `eshop-f1` | F | 0.0352 | 0.3686 | 0.0476 | 1.350 | +0.433 | 10.463 | +3.387 |

| Contrast | n questions | median log2 | as a ratio | 95% CI (log2) | 95% CI (ratio) | 90% CI (log2) |
|---|---|---|---|---|---|---|
| **B vs G** | 6 | **+0.120** | **1.087×** | **[−0.423, +0.346]** | [0.746×, 1.271×] | [−0.323, +0.331] |
| M vs G | 6 | +0.593 | 1.508× | [+0.323, +2.315] | [1.251×, 4.976×] | [+0.361, +1.999] |

Exact Wilcoxon signed-rank against zero, enumerated over all 2⁶ sign assignments:
**B vs G** W = 10.0, **p = 1.0000**; M vs G W = 0.0, p = 0.0313.

**Read the interval, not the point.** The point estimate says arm B cost **8.7% more** than arm G,
but the 95% CI runs from **25% cheaper to 27% more expensive**. That interval is consistent with a
meaningful saving and with a meaningful penalty at the same time. It is not "no difference"; it is
**not knowing**, and six questions is why. The accelerator branch needs the CI *upper* bound below
−0.32; the upper bound is **+0.346**. Nor is the point estimate carried by an outlier that a
larger sample would wash out: **four of the six questions favour arm G** (`b1`, `c1`, `d1`, `f1`
all positive) and only two favour arm B (`a1` −0.323, `e1` −0.523). There is no cost saving
visible here to be confirmed by more data.

**The point estimate is also not robust to a composition mistake (D7).** DESIGN §3.3
pre-registers 2 class-B questions and half-weight E and F per repo; the shipped set is one per
class, halving the core-claim class and doubling the controls. Arithmetic over the per-question
`log2` column above: drop F and the median is **+0.012**; drop E and it is **+0.229**; the
§3.3-shaped mixes land at +0.120 and +0.244. A 0.23 log2 (~17%) swing on which control question
is present. The **verdict does not move** — nothing in that range comes within 0.33 of the
threshold, and the CI is wider than the whole span — but the headline *number* carries that
caveat. See `DEVIATIONS.md` D7.

---

## 3. Co-primary endpoint — correctness (DESIGN §4.2)

`CORRECT` = blind judge says correct **and** pass 1 found no `mustNotMention` violation **and**,
for classes D and E, pass 1's deterministic verdict matched. The composite was fixed in the
ledger before the judge ran.

| Arm | n | judge correct | trap clean | verdict ok (D/E) | **CORRECT** | Wilson 95% |
|---|---|---|---|---|---|---|
| G | 18 | 18 | 18/18 | 6/6 | **18/18 (100.0%)** | [82.4%, 100.0%] |
| M | 18 | 12 | 18/18 | 6/6 | **12/18 (66.7%)** | [43.7%, 83.7%] |
| B | 18 | 18 | 18/18 | 6/6 | **18/18 (100.0%)** | [82.4%, 100.0%] |

Paired difference **B − G**, Newcombe (1998) method 10, pairs matched on (question, rep):

| both correct | B only | G only | neither | diff | 90% CI | 95% CI | exact McNemar p |
|---|---|---|---|---|---|---|---|
| 18 | **0** | **0** | 0 | **0.000** | [−0.131, +0.131] | [−0.176, +0.176] | **1.0000** |

Arms B and G are **exactly tied**: zero discordant pairs, on every one of 18 matched items.

Zero `mustNotMention` violations in any arm, so the class-D attribution trap
(`ProductPriceChangedIntegrationEvent` attributed to `CatalogApi.CreateItem`) caught nobody —
including arm G, which had to read the file the trap lives in. Median must-mention recall is
100.0% in all three arms; two key terms (`IOrderRepository`, `NewOrderRequestHandlerTest`) were
missed by 9/9 runs in **every** arm, a constant offset that cannot move a paired contrast.

---

## 4. Correctness by question class

A win driven by one class is a different product claim from a win across the set, so the
breakdown is reported before any aggregate.

| Question | Class | G | M | B |
|---|---|---|---|---|
| `eshop-a1` | A orientation | 3/3 | 3/3 | 3/3 |
| `eshop-b1` | B indirection | 3/3 | 3/3 | 3/3 |
| `eshop-c1` | C impact | 3/3 | **0/3** | 3/3 |
| `eshop-d1` | D attribution trap | 3/3 | 3/3 | 3/3 |
| `eshop-e1` | E negative control | 3/3 | 3/3 | 3/3 |
| `eshop-f1` | F grep control | 3/3 | **0/3** | 3/3 |

There is no class in which arm B beats arm G, and none in which it loses. The only structure in
the accuracy data is arm M's two total failures.

---

## 5. Arm M — the result that is not in the primary contrast and should not be buried

Arm M is the only arm that actually exercised the MCP, and it is the sharpest signal in the pilot.

- **12/18 (66.7%)** correct against arm G's 18/18, Wilson 95% [43.7%, 83.7%].
- **1.508×** arm G's cost, 95% CI [1.251×, 4.976×], Wilcoxon p = 0.0313 — the smallest two-sided
  p that n = 6 can produce, i.e. every question ranked the same way.
- Median 23 turns and 22 executed tool calls against arm G's 13 and 11.
- Failures are **total, not scattered**: `eshop-c1` (class C, impact) **0/3** and `eshop-f1`
  (class F, grep control) **0/3**. The other four classes are 3/3.
- The pilot's only censored run is arm M's: `eshop-c1/M/rep1` hit the $1.50 cap
  (`error_max_budget_usd`, $1.5134), scored incorrect at cost = cap per §6.6, kept and counted.
- Judge-flagged fabrication: **11.1%** in arm M, 0.0% in G and B. Arm M nonetheless had the
  *fewest* unresolvable citations (5, against G's 14 and B's 15) — when it cites, it cites
  something real; the fabrication is at the level of the claim, not the reference.

**Class F losing is evidence the harness works.** DESIGN §3.3 put the literal-string question in
precisely as a design check — "if the MCP arm wins here, something is wrong with the harness". It
did not win; it scored zero, at 10.5× arm G's cost. Read in the direction the design intended,
that is a passing sanity check on the instrument, and it is the reason the arm-M result can be
believed rather than explained away.

**Class C losing is the substantive one.** Impact analysis ("what breaks if I change
`CheckoutBasketCommandHandler`") is the set-valued, cross-project question the graph exists to
answer. Arm M is the configuration in which the graph has to carry it alone, and it went 0/3 —
one run to the budget cap. This is the single most product-relevant number in the pilot.

---

## 6. Mapping onto the pre-registered decision rule (DESIGN §5)

Reported in full, including the branch that is unflattering and the branch that fires wrongly.
This table is **subordinate to the verdict above**: §3.1 disqualifies the B-vs-G contrast before
§5 is reached, so what follows is what the arithmetic says, not what the experiment concluded.

| Branch | Pre-registered condition | Computed | Fires |
|---|---|---|---|
| **Accelerator** | non-inferior AND cost CI upper < −0.32 | upper = **+0.346** | no |
| **Primer** | 90% CI lower on Δaccuracy > +0.05, cost not reduced | lower = **−0.131** | no |
| **Null** | neither CI excludes zero | cost 95% [−0.423, +0.346]; accuracy 90% [−0.131, +0.131] | **yes** |
| **Regression** | non-inferiority fails | lower = **−0.131** vs the −0.05 bar | **yes — power artifact** |

**The Regression branch fires and must not be reported as a regression.** Arms B and G are
exactly tied: 18/18 concordant-correct, **zero** discordant pairs in either direction, exact
McNemar p = 1.0000. Nothing in the data says arm B is less accurate than arm G on a single item.
The branch fires because the *tightest interval 18 pairs can produce* is ±0.131 and the
pre-registered bar is −0.05. At n = 18 the bar is unreachable **whatever the answers had been** —
a perfect 18/18 tie still fails it. Publishing that as "the MCP made the agent wrong" would be
reporting the sample size as if it were a finding.

**The Null branch fires and must not be reported as a Null either**, for the different reason
given in the verdict: §3.1's manipulation check failed first. A Null means "the tools are a tax";
this pilot cannot say that, because the treatment arm did not meaningfully take the treatment.
What it can say is that a capable agent, handed 22 connected DevContext tools and no instruction
to use them, mostly did not — and that where the MCP was *forced* (arm M) it was worse and more
expensive.

---

## 7. The tool-schema tax (DESIGN §4.4)

The fixed cost the treatment has to earn back. Measured by `measure-tax.mjs` on the byte-identical
trivial prompt `reply with the word ok`, arm B minus arm G, the two command lines differing in
exactly one flag (`--mcp-config`).

| Statistic | Value | Note |
|---|---|---|
| DESIGN §4.4 literal (turn-1 `input + cache_creation`) | **9 tokens** | An artifact of a warm server-side prefix cache, not the tax — see D3 |
| Cache-state-invariant (turn-1 `input + cache_creation + cache_read`) | **~2,535 tokens** | Three independent measurements agree (2540, 2535, 2531) |
| Per tool schema | ~115 tokens | 22 DevContext schemas plus MCP preamble |
| Priced, Opus 5, 1-hour cache write (2×) | **$0.0254** once per session, then **$0.00127** per later turn | |

As a share of what a run actually costs (arithmetic over the arm-B medians in §2): **10.1%** of
the median arm-B run, but **53.4%** of `eshop-f1`, **32.7%** of `eshop-d1` and **30.2%** of
`eshop-e1` — and **2.6%** of `eshop-c1`. DESIGN §4.4's warning holds exactly: the tax hurts short
runs most, which is where classes D, E and F live, and averaging it away would hide that. Note
that those three short classes are also the three where arm B called the MCP **zero** times, so
on half the question set the treatment paid the tax in full and bought nothing.

---

## 8. Censoring, infrastructure, and cost of the pilot

| Arm | n | censored | rate |
|---|---|---|---|
| G | 18 | 0 | 0.0% |
| M | 18 | 1 | 5.6% |
| B | 18 | 0 | 0.0% |

Spread **5.6 points**, inside DESIGN §6.6's 10-point comparability threshold, so the medians are
comparable. The one censored run is kept, scored incorrect at cost = cap, never dropped. Neither
arm of the headline contrast was censored at all.

One further attempt (`eshop-b1/M/rep1`, attempt 1) died on an API error mid-response and is
**not** in `runs.jsonl`. It is an infrastructure interruption, not a §6.6 censoring event — the
distinction was fixed in code before any P2 result was seen — and it is quarantined in
`results/infra-failures.jsonl` with the cell re-run. It burnt **$0.99**, reported here rather than
hidden.

**Actual spend** (DESIGN §10 asks for actual, not estimated): 54 recorded runs **$23.67**
+ quarantined attempt **$0.99** + judge pass **$3.48** = **$28.14**. DESIGN §10 estimated $25–40
for the pilot and ~$15 for judging.

---

## 9. What is provisional, and what is proven

**Proven, from artifacts rather than assertion:**

- **Arm isolation held on 54/54 runs.** Arm G made zero `mcp__` calls; arm M made zero
  `Read`/`Grep`/`Glob` calls. Re-derived from the recorded transcripts by `audit-preflight.mjs`,
  which shares no code with the harness. `total_cost_usd` non-zero on all 54 rows.
- **Judge blindness was shown, not claimed.** All 54 prompts are written to
  `results/judge-prompts/` and scanned back with an independently written superset rule list:
  **zero** residual arm-identifying hits (`results/a1.2-leak-scan.md`).
- **The repo pin held**: `9b4f9434` on all 54 rows; the harness re-checks it before every batch.

**Provisional — the correctness numbers in §3, §4 and §5 all depend on it:**

DESIGN §7 requires a human to grade a 20% stratified sample and compute Cohen's κ against the
judge, and states that **if κ < 0.8 the judge's scores are discarded and the whole set is graded
by hand**. That validation has **not** been performed. The sample is drawn and laid out for the
owner at `eval/agent-probe/results/r1.2-human-sample/` — 11 items (`ceil(0.20 × 54)`), stratified
over the 18 arm × class cells at an equal 11/54 inclusion probability per run, seed `20260811`,
copied to arm-free filenames with the mapping sealed. **No κ is reported here because none has
been computed, and this report does not estimate one.** Until it exists, every judge-derived
figure — including arm M's 12/18, the most consequential number in this document — is
unvalidated. Pass 1's deterministic results (recall, `mustNotMention`, citation resolution, the
D/E/F verdicts) do not depend on the judge and stand either way.

And the validation itself is underpowered, which the owner should know before grading rather than
after. The judge marked 10 of the 11 drawn items correct, and against a 10:1 marginal a
chance-corrected statistic is brutal: **one** human disagreement on **one** item drops κ to 0.62,
and if it lands on the single judge-incorrect item κ collapses to 0.00. Only *perfect* 11/11
agreement clears the 0.8 gate. The gate is **not** being moved — DESIGN §7 is pre-registered and
its consequence stands — but a failure at this n means "the pilot is too small to validate its
judge", not "the judge is wrong". Derivation and the full table are in that directory's
`README.md`; the arithmetic is self-tested by `kappa.mjs --self-test`.

**Departures from the pre-registration**: seven, recorded in
[`DEVIATIONS.md`](../../eval/agent-probe/DEVIATIONS.md) with direction of effect. D1–D6 applied
identically to all three arms and cannot bend a paired contrast; **D7 can and does** — it is the
question-set composition, and §2 above carries its effect. The decision rule, the arm definitions,
every question and every answer key are untouched. No threshold moved after a number was seen.

---

## 10. What the full run needs to turn this into a defensible number

In the order that changes the answer most.

1. **Resolve the manipulation check before spending anything else.** The 360-run study is not
   worth running while arm B ignores the tools: it would buy a tighter interval around a contrast
   that still is not a test of the MCP. This is a pre-run gate, not an analysis step. Either
   demonstrate that arm B clears `mcp_call_share` ≥ 0.2 under an unchanged prompt after the tool
   surface is revised, or state the design's honest fallback — that the real comparison is **M vs
   G**, in which case arm B becomes secondary and the study reports sufficiency rather than
   augmentation. Whichever is chosen must be written into DESIGN **before** the next run, because
   choosing it afterwards is exactly what pre-registration exists to prevent.
2. **The unseen repo (DESIGN §6.1), which this pilot has none of.** eShop is a famous public
   repo the model may have memorised; §6.1 calls contamination "the one that could invalidate
   everything" and its mitigation "not optional", and §12 requires the unseen repo's number to be
   reported **first**. Every number in this document comes from a contaminated repo, and
   memorisation is a live alternative explanation for why arm B did not need the tools. The full
   run's four-repo set must include a private repo or a mechanically identifier-renamed public
   one.
3. **Ship DESIGN §3.3's question mix (D7).** 2 × class B per repo, E and F at half weight
   alternating across repos. 24 questions, not 18. Fixing composition is worth more than adding
   reps: the bootstrap resamples questions, so questions are the sample size that matters. Do not
   retrofit the missing class-B question onto the pilot's numbers — probe rule 3, and the pilot's
   verdict is unaffected either way.
4. **Decide, in advance, at what level the −0.05 non-inferiority bar is evaluated.** §4.2's
   margin is not merely unmet at pilot scale — at some sample sizes it is arithmetically
   unreachable. Running `newcombePaired(n, 0, 0, 0, z=1.644854)` from `analyse.mjs` at a *perfect*
   tie gives the best 90% lower bound each n can produce:

   | pairs | what that is | 90% lower at a perfect tie | clears −0.05? |
   |---|---|---|---|
   | 18 | this pilot | **−0.131** | no, and no data could |
   | 30 | one repo in the full run (6 q × 5 reps) | **−0.083** | **no, and no data could** |
   | 120 | all four repos pooled | −0.022 | yes, with room for ~2 net losses |

   So the full run does **not** fix this per repo: at 30 pairs the bar fails at a perfect tie,
   exactly as it does here. It is reachable only **pooled across all four repos**, and only if arm
   B loses at most about two items net (b = 0, c = 2 → −0.049; b = 0, c = 4 → −0.072, fail). That
   collides with §6.1, which requires the headline to be reported **per repo** with the unseen
   repo first. Pick one and write it into DESIGN before the next run: non-inferiority as a single
   pooled whole-study endpoint with per-repo cost figures alongside, or a per-repo bar widened to
   a margin 30 pairs can actually test, with the widening justified on its own terms. Deciding
   this after seeing the pilot's ±0.131 is pre-registration drift; deciding it now, in writing,
   is not.
5. **Run the κ validation, honour its consequence, and power it.** The pilot's sample is drawn
   and waiting (`eval/agent-probe/results/r1.2-human-sample/`); grade it before committing to 360,
   because a judge that fails κ turns the entire correctness endpoint into hand grading, and that
   is a schedule decision rather than a footnote. But 20% of 54 is 11 items with a 10:1 verdict
   split, where a single disagreement fails the 0.8 gate (§9) — so at pilot scale the validation
   can only ever return "perfect agreement" or "unevaluable". The full run must fix that in the
   pre-registration, not afterwards: 20% of 360 is 72 items, which is enough for κ to mean
   something, and the design should also say what happens when a marginal is degenerate — the
   judge flagged `fabricated: false` on all 11 drawn items here, so that κ is undefined by
   construction and no threshold can be applied to it.
6. **n = 5 reps, as §6.5 specifies.** The pilot's between-rep coefficient of variation is a median
   of 0.116 across cells (max 0.258), so the standard error of a cell mean at n = 5 is ~0.052
   against the 0.20 effect the rule must resolve. Reps are adequate; they were never the binding
   constraint. 4 repos × 6 questions × 3 arms × 5 reps = 360 runs, ~$150–250 by §10.
7. **Report the tax against the run lengths it actually falls on.** ~2,535 tokens is 2.6% of a
   long impact question and 53.4% of a short literal-string one. A single averaged percentage
   would be true and useless.

---

### Provenance

- Inputs: `eval/agent-probe/results/runs.jsonl` (54), `graded.jsonl` (54), `judged.jsonl` (54),
  `infra-failures.jsonl` (1), `p1.2-tool-schema-tax.json`, `p1.1-preflight-audit.json`.
- Regenerate the statistics: `node eval/agent-probe/analyse.mjs` ·
  re-check pass 1: `node eval/agent-probe/grade.mjs --check` (exits 1 on drift) ·
  gate: `node eval/agent-probe/verify.mjs --tier fast`.
- Bootstrap seed `20260811`, 10,000 resamples over questions. Judge: `claude-opus-5`, effort
  `high`, fresh session per item, zero tools, 54/54 parsed, $3.48.
- DESIGN §12 names the output path `eval-results/<date>/agent-probe/RESULTS.md`; this pilot writes
  the undated `eval-results/agent-probe/RESULTS.md` the R1 checkpoint specifies, so the full run's
  results do not overwrite the pilot's.
- DESIGN §12 also asks for a one-line verdict in `PRODUCT-DIRECTION.md` §9 replacing "primer, not
  accelerator". That line is **not** written yet: it should not be written off a contrast the
  manipulation check disqualified, and the standing "primer" verdict is not refuted by this pilot
  either. It is a full-run output.
