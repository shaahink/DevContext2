#!/usr/bin/env node
// R1.2 - Cohen's kappa between the human grader and the blind judge, on the 20% stratified
// sample drawn by sample-human-check.mjs.
//
// DESIGN section 7: "a human grades a 20% stratified sample. Compute Cohen's kappa against the
// judge. If kappa < 0.8, the judge's scores are discarded and the whole set is graded by hand."
//
// Three things this script will not do, because each of them would manufacture a pass:
//   - it will not run on an unfilled sheet;
//   - it will not report a degenerate kappa (1 - pe == 0) as 1.0, it reports it as undefined;
//   - it will not fold the disagreement census into the sample.
//
//   node eval/agent-probe/kappa.mjs              compute from the filled sheet
//   node eval/agent-probe/kappa.mjs --self-test  check the arithmetic against known values

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const DIR = path.join(HERE, "results", "r1.2-human-sample");
const GATE = 0.8;

// Cohen's kappa for two raters and two categories. Returns kappa: null when it is undefined,
// which happens whenever expected agreement is 1 - i.e. both raters used a single category.
function cohensKappa(pairs) {
  const n = pairs.length;
  const t = { yy: 0, yn: 0, ny: 0, nn: 0 };
  for (const [h, j] of pairs) t[(h ? "y" : "n") + (j ? "y" : "n")]++;
  const po = (t.yy + t.nn) / n;
  const hYes = (t.yy + t.yn) / n, jYes = (t.yy + t.ny) / n;
  const pe = hYes * jYes + (1 - hYes) * (1 - jYes);
  const denom = 1 - pe;
  return {
    n, table: t, po, pe,
    kappa: denom === 0 ? null : (po - pe) / denom,
    degenerate: denom === 0,
    humanYes: t.yy + t.yn, judgeYes: t.yy + t.ny,
  };
}

function report(label, k) {
  console.log(`\n## ${label}  (n=${k.n})`);
  console.log(`                judge y   judge n`);
  console.log(`  human y   ${String(k.table.yy).padStart(7)}   ${String(k.table.yn).padStart(7)}`);
  console.log(`  human n   ${String(k.table.ny).padStart(7)}   ${String(k.table.nn).padStart(7)}`);
  console.log(`  raw agreement po = ${k.po.toFixed(4)}  (${k.table.yy + k.table.nn}/${k.n})`);
  console.log(`  expected     pe = ${k.pe.toFixed(4)}`);
  if (k.degenerate) {
    console.log(`  Cohen's kappa    = UNDEFINED (1 - pe = 0)`);
    console.log(`  Both raters used a single category, so there is no chance-corrected agreement`);
    console.log(`  to measure. This is NOT a kappa of 1.0 and must not be recorded as one. The`);
    console.log(`  DESIGN section 7 gate cannot be evaluated on this sample; report raw agreement`);
    console.log(`  and say the gate is unevaluable at this sample size.`);
  } else {
    console.log(`  Cohen's kappa    = ${k.kappa.toFixed(4)}`);
  }
  return k;
}

if (process.argv.includes("--self-test")) {
  console.log("kappa.mjs self-test - arithmetic only, no real grades involved");
  const mk = (yy, yn, ny, nn) => [
    ...Array(yy).fill([true, true]), ...Array(yn).fill([true, false]),
    ...Array(ny).fill([false, true]), ...Array(nn).fill([false, false]),
  ];
  const cases = [
    { name: "textbook 20/5/10/15", pairs: mk(20, 5, 10, 15), expect: 0.4 },
    { name: "perfect, both categories used", pairs: mk(6, 0, 0, 5), expect: 1 },
    { name: "chance-level", pairs: mk(25, 25, 25, 25), expect: 0 },
    { name: "all agree, single category", pairs: mk(11, 0, 0, 0), expect: null },
  ];
  let bad = 0;
  for (const c of cases) {
    const k = cohensKappa(c.pairs);
    const ok = c.expect === null ? k.kappa === null : Math.abs(k.kappa - c.expect) < 1e-9;
    if (!ok) bad++;
    console.log(`  ${ok ? "ok  " : "FAIL"} ${c.name}: kappa=${k.kappa === null ? "undefined" : k.kappa.toFixed(4)}, expected ${c.expect === null ? "undefined" : c.expect}`);
  }
  console.log(bad === 0 ? "\nself-test PASS" : `\nself-test FAIL (${bad})`);
  process.exit(bad === 0 ? 0 : 1);
}

// --sheet relocates the input only; every other rule still applies. It exists so the parse and
// reporting paths can be exercised against a throwaway sheet without ever writing fake verdicts
// into the owner's real one.
const sheetArg = process.argv.indexOf("--sheet");
const sheetPath = sheetArg > -1 ? process.argv[sheetArg + 1] : path.join(DIR, "GRADING-SHEET.md");
if (!fs.existsSync(sheetPath)) {
  console.error(`no grading sheet at ${sheetPath} - run sample-human-check.mjs first`);
  process.exit(2);
}

const rows = [];
for (const line of fs.readFileSync(sheetPath, "utf8").split("\n")) {
  const m = line.match(/^\|\s*(item-\d+)\s*\|\s*([ynYN?])\s*\|\s*([ynYN?])\s*\|/);
  if (m) rows.push({ item: m[1], correct: m[2].toLowerCase(), fabricated: m[3].toLowerCase() });
}

const unfilled = rows.filter((r) => r.correct === "?" || r.fabricated === "?");
if (rows.length === 0 || unfilled.length > 0) {
  console.error(`grading sheet is not filled in: ${unfilled.length} of ${rows.length} rows still carry "?".`);
  console.error(`Grade the item-NN.txt files by hand first. This script will not guess, and no`);
  console.error(`agent in this program grades them - DESIGN section 7 asks for a human.`);
  process.exit(2);
}

const key = JSON.parse(fs.readFileSync(path.join(DIR, "SEALED-key.json"), "utf8"));
const byItem = new Map(key.items.map((i) => [i.item, i]));

const pairsCorrect = [], pairsFab = [];
for (const r of rows) {
  const k = byItem.get(r.item);
  if (!k) throw new Error(`sheet row ${r.item} is not in SEALED-key.json`);
  pairsCorrect.push([r.correct === "y", k.judgeCorrect === true]);
  pairsFab.push([r.fabricated === "y", k.judgeFabricated === true]);
}

console.log(`# R1.2 - human vs blind judge, ${rows.length} items (DESIGN section 7)`);
console.log(`Sample: ${path.relative(process.cwd(), DIR)}, seed ${key.seed}.`);
console.log(`Rule: ${key.rule}`);

const kc = report("correct - THIS is the DESIGN section 7 gate", cohensKappa(pairsCorrect));
report("fabricated - reported alongside, not gated on", cohensKappa(pairsFab));

console.log(`\n## Consequence, pre-registered in DESIGN section 7`);
if (kc.degenerate) {
  console.log(`  Kappa on "correct" is undefined, so the >= ${GATE} gate cannot be evaluated.`);
  console.log(`  Report that plainly. Do not record a pass, and do not record a fail.`);
  process.exit(3);
}
if (kc.kappa < GATE) {
  console.log(`  kappa ${kc.kappa.toFixed(4)} < ${GATE}: the judge's scores are DISCARDED and all 54 runs`);
  console.log(`  must be graded by hand. Every judge-derived figure in RESULTS.md is void until then.`);
  process.exit(1);
}
console.log(`  kappa ${kc.kappa.toFixed(4)} >= ${GATE}: the judge's scores stand. RESULTS.md sections 3, 4`);
console.log(`  and 5 can drop the "provisional" flag, and section 9 should record this number.`);
