#!/usr/bin/env node
// R1.2 - draw the 20% stratified human-check sample that DESIGN section 7 pre-registers,
// and lay it out so the owner can grade it BLIND.
//
// The sampling rule was fixed in the conductor ledger before this script was written and
// before the draw was run. Repeating it here so the artifact carries its own provenance:
//
//   n      = ceil(0.20 * 54) = 11
//   strata = the 18 (arm x questionClass) cells, 3 reps each
//   draw   = 11 of the 18 cells uniformly WITHOUT replacement, then 1 rep uniformly per cell,
//            so every run has inclusion probability 11/18 * 1/3 = 11/54 = 20.4%, equal for all
//            54. That is an unbiased 20% sample that is also spread across arms and classes.
//   rng    = mulberry32, seed 20260811 (the program's seed), so the draw is reproducible
//
// Blindness: the judge-prompt filenames encode the arm (eshop-a1_B_rep1.txt), so they are
// copied VERBATIM to item-NN.txt and the mapping goes in SEALED-key.json, which the owner does
// not open until the sheet is filled. The item files are byte-identical to what the judge saw,
// so the human is graded against the same instrument on the same material.
//
// This script does not grade anything and does not compute kappa. See kappa.mjs.

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const RESULTS = path.join(HERE, "results");
const OUT = path.join(RESULTS, "r1.2-human-sample");
const SEED = 20260811;
const SAMPLE_FRACTION = 0.20;

// Same generator as analyse.mjs and run-probe.mjs, so "seed 20260811" means one thing in this
// program and not three.
function mulberry32(a) {
  return function () {
    a |= 0; a = (a + 0x6D2B79F5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

function readJsonl(p) {
  return fs.readFileSync(p, "utf8").trim().split("\n").filter(Boolean).map((l) => JSON.parse(l));
}

const judged = readJsonl(path.join(RESULTS, "judged.jsonl"));
if (judged.length !== 54) throw new Error(`expected 54 judged rows, got ${judged.length}`);

// Deterministic cell order, independent of the order judged.jsonl happens to be in.
const cells = new Map();
for (const r of judged) {
  const key = `${r.arm}|${r.questionClass}`;
  if (!cells.has(key)) cells.set(key, []);
  cells.get(key).push(r);
}
const cellKeys = [...cells.keys()].sort();
if (cellKeys.length !== 18) throw new Error(`expected 18 (arm x class) cells, got ${cellKeys.length}`);
for (const k of cellKeys) {
  const reps = cells.get(k);
  if (reps.length !== 3) throw new Error(`cell ${k} has ${reps.length} reps, expected 3`);
  reps.sort((a, b) => a.rep - b.rep);
}

const n = Math.ceil(SAMPLE_FRACTION * judged.length);

// Fisher-Yates over the ordered cell list, then take the first n. Without replacement by
// construction, so no cell contributes two items.
const rnd = mulberry32(SEED);
const shuffled = [...cellKeys];
for (let i = shuffled.length - 1; i > 0; i--) {
  const j = Math.floor(rnd() * (i + 1));
  [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
}
const drawnCells = shuffled.slice(0, n);
const picked = drawnCells.map((k) => cells.get(k)[Math.floor(rnd() * 3)]);

// Present the items in draw order, which is already independent of arm and class. Numbering is
// the only thing the owner sees.
fs.rmSync(OUT, { recursive: true, force: true });
fs.mkdirSync(OUT, { recursive: true });

const items = picked.map((r, i) => {
  const id = String(i + 1).padStart(2, "0");
  const src = path.join(RESULTS, "judge-prompts", r.promptFile);
  const bytes = fs.readFileSync(src);
  fs.writeFileSync(path.join(OUT, `item-${id}.txt`), bytes);
  return { item: `item-${id}`, bytes: bytes.length, row: r };
});

// The census: every run the judge marked incorrect. Reported so the owner knows what the
// proportional sample could not guarantee to contain. NOT part of the kappa sample.
const judgeIncorrect = judged.filter((r) => r.correct === false);
const censusCells = [...new Set(judgeIncorrect.map((r) => `${r.arm}/${r.questionId}`))].sort();

fs.writeFileSync(
  path.join(OUT, "SEALED-key.json"),
  JSON.stringify(
    {
      note: "Do not open until GRADING-SHEET.md is filled in. It maps each item to its arm and to the judge's verdict, and reading it first destroys the blind.",
      seed: SEED,
      rule: "ceil(0.20*54)=11 of 18 (arm x class) cells without replacement, 1 rep each; inclusion probability 11/54 for every run",
      generatedBy: "eval/agent-probe/sample-human-check.mjs",
      items: items.map((it) => ({
        item: it.item,
        questionId: it.row.questionId,
        questionClass: it.row.questionClass,
        arm: it.row.arm,
        rep: it.row.rep,
        promptFile: it.row.promptFile,
        judgeCorrect: it.row.correct,
        judgeFabricated: it.row.fabricated,
      })),
    },
    null,
    2
  ) + "\n"
);

const sheet = [];
sheet.push("# R1.2 - human grading sheet (DESIGN section 7 validation)");
sheet.push("");
sheet.push("Fill the two verdict columns and nothing else, then run:");
sheet.push("");
sheet.push("    node eval/agent-probe/kappa.mjs");
sheet.push("");
sheet.push("**Grade from the `item-NN.txt` files only.** Each one is byte-identical to what the");
sheet.push("blind judge was given: the question, the verified answer key, the redacted candidate");
sheet.push("answer, and the same rubric. Do not open `SEALED-key.json` and do not look at");
sheet.push("`judged.jsonl` until every row below is filled - both of those name the arm and the");
sheet.push("judge's verdict, and reading either first is the end of the blind.");
sheet.push("");
sheet.push("Write `y` or `n`. Leave `?` and `kappa.mjs` will refuse to run.");
sheet.push("");
sheet.push("| item | correct | fabricated |");
sheet.push("|---|---|---|");
for (const it of items) sheet.push(`| ${it.item} | ? | ? |`);
sheet.push("");
sheet.push("Definitions, copied from the rubric the judge was held to, so both raters are");
sheet.push("answering the same question:");
sheet.push("");
sheet.push("- **correct** - the answer substantively answers the question and agrees with the key.");
sheet.push("  Judge the substance, not the wording, length or formatting. An answer that reaches");
sheet.push("  the key's conclusion by a different route is correct. An answer that hedges into both");
sheet.push("  a right and a wrong conclusion is not correct.");
sheet.push("- **fabricated** - the answer asserts a file, symbol, method or code path that the key");
sheet.push("  contradicts, or states as fact something the key shows to be false. Being incomplete");
sheet.push("  is NOT fabrication.");
sheet.push("- `[REDACTED]` is an uninformative placeholder inserted to keep the grading blind.");
sheet.push("  Never let its presence or absence count for or against an answer.");
sheet.push("");
fs.writeFileSync(path.join(OUT, "GRADING-SHEET.md"), sheet.join("\n"));

// How brittle the DESIGN section 7 gate is on THIS draw. Derived from the judge's already
// published verdicts on the drawn items, so it does not depend on, and cannot prejudge, how the
// human grades. kappa.mjs is the authority on this formula and self-tests it against known
// values; this is the same arithmetic, inlined so the README is generated rather than typed.
const drawnJudgeYes = items.filter((it) => it.row.correct === true).length;
const drawnJudgeNo = items.length - drawnJudgeYes;
function kappaOf(yy, yn, ny, nn) {
  const m = yy + yn + ny + nn;
  const po = (yy + nn) / m;
  const hY = (yy + yn) / m, jY = (yy + ny) / m;
  const pe = hY * jY + (1 - hY) * (1 - jY);
  return pe === 1 ? null : (po - pe) / (1 - pe);
}
const toleranceRows = [
  { label: "none - the human agrees on every item", kappa: kappaOf(drawnJudgeYes, 0, 0, drawnJudgeNo) },
  { label: "1, on an item the judge called correct", kappa: kappaOf(drawnJudgeYes - 1, 0, 1, drawnJudgeNo) },
  { label: "1, on an item the judge called incorrect", kappa: kappaOf(drawnJudgeYes, 1, 0, drawnJudgeNo - 1) },
  { label: "2, both on items the judge called correct", kappa: kappaOf(drawnJudgeYes - 2, 0, 2, drawnJudgeNo) },
];

const readme = [];
readme.push("# R1.2 - the 20% human check the judge's scores depend on");
readme.push("");
readme.push("**This is owner work. No agent in this program grades these items or reports a kappa.**");
readme.push("");
readme.push("DESIGN section 7 pre-registered the validation and its consequence:");
readme.push("");
readme.push("> **Validation:** a human grades a 20% stratified sample. Compute Cohen's kappa against");
readme.push("> the judge. **If kappa < 0.8, the judge's scores are discarded and the whole set is");
readme.push("> graded by hand.** A judge nobody checked is a random number generator with good manners.");
readme.push("");
readme.push("Until that runs, every judge-derived number in");
readme.push("`eval-results/agent-probe/RESULTS.md` is provisional - including arm M's 12/18, which is");
readme.push("the most consequential figure in the pilot. Pass 1's deterministic results (recall,");
readme.push("`mustNotMention`, citation resolution, the D/E/F verdicts) do not depend on the judge and");
readme.push("stand either way.");
readme.push("");
readme.push("## What to do");
readme.push("");
readme.push("1. Read `item-01.txt` .. `item-" + String(items.length).padStart(2, "0") + ".txt`. Each is byte-identical to the prompt the blind");
readme.push("   judge received for that run: question, verified answer key, redacted candidate answer,");
readme.push("   and the same rubric. Nothing identifies the arm.");
readme.push("2. Fill `GRADING-SHEET.md` - two columns, `y` or `n`.");
readme.push("3. `node eval/agent-probe/kappa.mjs`. It refuses to run on an unfilled sheet, prints the");
readme.push("   confusion table and raw agreement alongside kappa, and reports a degenerate kappa as");
readme.push("   degenerate rather than as 1.0.");
readme.push("4. Only then open `SEALED-key.json`.");
readme.push("");
readme.push("## How these 11 were chosen");
readme.push("");
readme.push("Fixed in the ledger before the draw ran, so it could not be chosen to flatter a result.");
readme.push("");
readme.push("- n = `ceil(0.20 * 54)` = **" + items.length + "**.");
readme.push("- Strata: the **18 (arm x question-class) cells**, 3 reps each. Draw 11 cells uniformly");
readme.push("  without replacement, then one rep uniformly from each. Every run's inclusion");
readme.push("  probability is `11/18 * 1/3 = 11/54` = **20.4%**, identical for all 54, so the sample is");
readme.push("  an unbiased 20% sample and is spread across arms and classes by construction.");
readme.push("- Seed **" + SEED + "**, `mulberry32`, the same generator and seed as the bootstrap.");
readme.push("  Reproduce the whole package with `node eval/agent-probe/sample-human-check.mjs`.");
readme.push("");
readme.push("## The weakness in this test, stated rather than engineered around");
readme.push("");
readme.push("The judge marked **" + judgeIncorrect.length + " of 54** runs incorrect, and they sit in only " + censusCells.length + " cells");
readme.push("(" + censusCells.join(", ") + ") - every rep of each. A proportional 20% sample therefore expects");
readme.push("about **1.2** of them (2 cells x 11/18) and has a **13.7%** chance of containing none.");
readme.push("");
readme.push("Two consequences the owner should know before grading:");
readme.push("");
readme.push("- Cohen's kappa on 11 items with roughly one minority item is unstable, so the");
readme.push("  `kappa >= 0.8` gate is a weak test here. That is a property of the pilot's size, not");
readme.push("  something to fix by redrawing.");
readme.push("- If both raters mark all 11 items the same single category, kappa is `0/0` and");
readme.push("  **undefined**. `kappa.mjs` will say so and print raw agreement instead. A tool that");
readme.push("  printed 1.0 there would be manufacturing a pass on the DESIGN section 7 gate.");
readme.push("");
readme.push("The sample was **not** oversampled to compensate. Kappa is not invariant to marginal");
readme.push("distributions, so enriching the minority class would bias the estimate the design asked");
readme.push("for. Instead, see the census below.");
readme.push("");
readme.push("### How much disagreement this sample can absorb before it fails the gate");
readme.push("");
readme.push("Computed from the judge's verdicts on these " + items.length + " items, which are already published, so");
readme.push("nothing here depends on how the human grades. On this draw the judge marked");
readme.push("**" + drawnJudgeYes + " correct and " + (items.length - drawnJudgeYes) + " incorrect**, and against that marginal split Cohen's kappa behaves like this:");
readme.push("");
readme.push("| human disagreements with the judge | Cohen's kappa | clears the 0.8 gate |");
readme.push("|---|---|---|");
for (const row of toleranceRows) {
  readme.push(`| ${row.label} | ${row.kappa === null ? "undefined" : row.kappa.toFixed(4)} | ${row.kappa !== null && row.kappa >= 0.8 ? "yes" : "**no**"} |`);
}
readme.push("");
readme.push("**Only perfect agreement passes.** One disagreement on a single item out of " + items.length + " fails the");
readme.push("DESIGN section 7 gate, and if the disagreement lands on the one item the judge marked");
readme.push("incorrect, kappa collapses to 0. That is the arithmetic of a chance-corrected statistic on");
readme.push("11 items with a " + drawnJudgeYes + ":" + (items.length - drawnJudgeYes) + " marginal, not a judgement about the judge.");
readme.push("");
readme.push("**The gate is not being moved.** DESIGN section 7 is pre-registered and its consequence");
readme.push("stands as written: kappa below 0.8 discards the judge's scores and all 54 runs go to hand");
readme.push("grading. What this table changes is how a failure should be *read* - at this sample size a");
readme.push("failure means \"the pilot is too small to validate its judge\", not \"the judge is wrong\".");
readme.push("Note also that the judge marked `fabricated: false` on all " + items.length + " of these items, so unless");
readme.push("the human flags one, the fabrication kappa is undefined by construction. Both facts are");
readme.push("carried into the full run's requirements in RESULTS.md section 10, item 5.");
readme.push("");
readme.push("## Disagreement census - optional, separate, and NOT part of kappa");
readme.push("");
readme.push("All " + judgeIncorrect.length + " runs the judge marked incorrect, offered as a qualitative check the proportional");
readme.push("sample cannot guarantee to cover. **Grade these only after the sheet above is filled and");
readme.push("kappa is computed**, and do not merge them into the kappa sample.");
readme.push("");
readme.push("They are all in one arm, so this list is **not blind** - which is exactly why it cannot");
readme.push("enter the kappa estimate.");
readme.push("");
readme.push("| judge prompt | question | class | arm | rep | judge fabricated |");
readme.push("|---|---|---|---|---|---|");
for (const r of judgeIncorrect.sort((a, b) => a.promptFile.localeCompare(b.promptFile))) {
  readme.push(`| \`results/judge-prompts/${r.promptFile}\` | ${r.questionId} | ${r.questionClass} | ${r.arm} | ${r.rep} | ${r.fabricated ? "yes" : "no"} |`);
}
readme.push("");
readme.push("## What the full run needs");
readme.push("");
readme.push("Not repeated here. `eval-results/agent-probe/RESULTS.md` section 10 lists it in the order");
readme.push("that changes the answer most, and item 5 is this validation - run it on the pilot's 54");
readme.push("before committing to 360, because a judge that fails kappa turns the whole correctness");
readme.push("endpoint into hand grading, and that is a schedule decision rather than a footnote.");
readme.push("");
fs.writeFileSync(path.join(OUT, "README.md"), readme.join("\n"));

console.log(`wrote ${items.length} items + GRADING-SHEET.md + SEALED-key.json + README.md to`);
console.log(`  ${path.relative(process.cwd(), OUT)}`);
console.log(`seed ${SEED}, drawn cells (arm|class): ${drawnCells.join(", ")}`);
console.log(`judge-incorrect in population: ${judgeIncorrect.length}/54 across cells ${censusCells.join(", ")}`);
