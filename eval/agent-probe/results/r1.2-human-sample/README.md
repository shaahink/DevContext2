# R1.2 - the 20% human check the judge's scores depend on

**This is owner work. No agent in this program grades these items or reports a kappa.**

DESIGN section 7 pre-registered the validation and its consequence:

> **Validation:** a human grades a 20% stratified sample. Compute Cohen's kappa against
> the judge. **If kappa < 0.8, the judge's scores are discarded and the whole set is
> graded by hand.** A judge nobody checked is a random number generator with good manners.

Until that runs, every judge-derived number in
`eval-results/agent-probe/RESULTS.md` is provisional - including arm M's 12/18, which is
the most consequential figure in the pilot. Pass 1's deterministic results (recall,
`mustNotMention`, citation resolution, the D/E/F verdicts) do not depend on the judge and
stand either way.

## What to do

1. Read `item-01.txt` .. `item-11.txt`. Each is byte-identical to the prompt the blind
   judge received for that run: question, verified answer key, redacted candidate answer,
   and the same rubric. Nothing identifies the arm.
2. Fill `GRADING-SHEET.md` - two columns, `y` or `n`.
3. `node eval/agent-probe/kappa.mjs`. It refuses to run on an unfilled sheet, prints the
   confusion table and raw agreement alongside kappa, and reports a degenerate kappa as
   degenerate rather than as 1.0.
4. Only then open `SEALED-key.json`.

## How these 11 were chosen

Fixed in the ledger before the draw ran, so it could not be chosen to flatter a result.

- n = `ceil(0.20 * 54)` = **11**.
- Strata: the **18 (arm x question-class) cells**, 3 reps each. Draw 11 cells uniformly
  without replacement, then one rep uniformly from each. Every run's inclusion
  probability is `11/18 * 1/3 = 11/54` = **20.4%**, identical for all 54, so the sample is
  an unbiased 20% sample and is spread across arms and classes by construction.
- Seed **20260811**, `mulberry32`, the same generator and seed as the bootstrap.
  Reproduce the whole package with `node eval/agent-probe/sample-human-check.mjs`.

## The weakness in this test, stated rather than engineered around

The judge marked **6 of 54** runs incorrect, and they sit in only 2 cells
(M/eshop-c1, M/eshop-f1) - every rep of each. A proportional 20% sample therefore expects
about **1.2** of them (2 cells x 11/18) and has a **13.7%** chance of containing none.

Two consequences the owner should know before grading:

- Cohen's kappa on 11 items with roughly one minority item is unstable, so the
  `kappa >= 0.8` gate is a weak test here. That is a property of the pilot's size, not
  something to fix by redrawing.
- If both raters mark all 11 items the same single category, kappa is `0/0` and
  **undefined**. `kappa.mjs` will say so and print raw agreement instead. A tool that
  printed 1.0 there would be manufacturing a pass on the DESIGN section 7 gate.

The sample was **not** oversampled to compensate. Kappa is not invariant to marginal
distributions, so enriching the minority class would bias the estimate the design asked
for. Instead, see the census below.

### How much disagreement this sample can absorb before it fails the gate

Computed from the judge's verdicts on these 11 items, which are already published, so
nothing here depends on how the human grades. On this draw the judge marked
**10 correct and 1 incorrect**, and against that marginal split Cohen's kappa behaves like this:

| human disagreements with the judge | Cohen's kappa | clears the 0.8 gate |
|---|---|---|
| none - the human agrees on every item | 1.0000 | yes |
| 1, on an item the judge called correct | 0.6207 | **no** |
| 1, on an item the judge called incorrect | 0.0000 | **no** |
| 2, both on items the judge called correct | 0.4211 | **no** |

**Only perfect agreement passes.** One disagreement on a single item out of 11 fails the
DESIGN section 7 gate, and if the disagreement lands on the one item the judge marked
incorrect, kappa collapses to 0. That is the arithmetic of a chance-corrected statistic on
11 items with a 10:1 marginal, not a judgement about the judge.

**The gate is not being moved.** DESIGN section 7 is pre-registered and its consequence
stands as written: kappa below 0.8 discards the judge's scores and all 54 runs go to hand
grading. What this table changes is how a failure should be *read* - at this sample size a
failure means "the pilot is too small to validate its judge", not "the judge is wrong".
Note also that the judge marked `fabricated: false` on all 11 of these items, so unless
the human flags one, the fabrication kappa is undefined by construction. Both facts are
carried into the full run's requirements in RESULTS.md section 10, item 5.

## Disagreement census - optional, separate, and NOT part of kappa

All 6 runs the judge marked incorrect, offered as a qualitative check the proportional
sample cannot guarantee to cover. **Grade these only after the sheet above is filled and
kappa is computed**, and do not merge them into the kappa sample.

They are all in one arm, so this list is **not blind** - which is exactly why it cannot
enter the kappa estimate.

| judge prompt | question | class | arm | rep | judge fabricated |
|---|---|---|---|---|---|
| `results/judge-prompts/eshop-c1_M_rep1.txt` | eshop-c1 | C | M | 1 | no |
| `results/judge-prompts/eshop-c1_M_rep2.txt` | eshop-c1 | C | M | 2 | yes |
| `results/judge-prompts/eshop-c1_M_rep3.txt` | eshop-c1 | C | M | 3 | yes |
| `results/judge-prompts/eshop-f1_M_rep1.txt` | eshop-f1 | F | M | 1 | no |
| `results/judge-prompts/eshop-f1_M_rep2.txt` | eshop-f1 | F | M | 2 | no |
| `results/judge-prompts/eshop-f1_M_rep3.txt` | eshop-f1 | F | M | 3 | no |

## What the full run needs

Not repeated here. `eval-results/agent-probe/RESULTS.md` section 10 lists it in the order
that changes the answer most, and item 5 is this validation - run it on the pilot's 54
before committing to 360, because a judge that fails kappa turns the whole correctness
endpoint into hand grading, and that is a schedule decision rather than a footnote.
