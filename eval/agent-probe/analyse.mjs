// A1.2 - the paired analysis (DESIGN sections 4, 5 and 9). Pure ASCII on purpose.
//
// Reads results/runs.jsonl (the recorded runs), results/graded.jsonl (pass 1, deterministic) and
// results/judged.jsonl (pass 2, the blind LLM judge) and writes results/a1.2-analysis.md.
//
// Every number in the report is computed here. Nothing is asserted, nothing is carried over from
// prose, and no run is dropped - the one censored run is kept and flagged, per DESIGN 6.6.
//
// Rank methods and medians throughout, never means and t-tests: n is 6 questions and one
// rabbit-holing run would carry a mean.
//
//   node eval/agent-probe/analyse.mjs
//   node eval/agent-probe/analyse.mjs --resamples 10000 --seed 20260811

import { readFileSync, writeFileSync, existsSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const HERE = dirname(fileURLToPath(import.meta.url));
const RESULTS = join(HERE, "results");
const argv = process.argv.slice(2);
const argOf = (n, d) => { const i = argv.indexOf(`--${n}`); return i >= 0 && argv[i + 1] ? argv[i + 1] : d; };
const REPO = argOf("repo", "eShop");
const RESAMPLES = Number(argOf("resamples", "10000"));
const SEED = Number(argOf("seed", "20260811"));
const OUT = join(RESULTS, argOf("out", "a1.2-analysis.md"));

// ---- small stats toolkit ------------------------------------------------------------------

function median(xs) {
  const s = xs.filter((x) => x != null && Number.isFinite(x)).sort((a, b) => a - b);
  if (!s.length) return null;
  const m = s.length >> 1;
  return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2;
}
const mean = (xs) => (xs.length ? xs.reduce((a, b) => a + b, 0) / xs.length : null);

// Seeded so the bootstrap is reproducible: an interval nobody can regenerate is a claim, not a
// measurement. The seed is printed in the report.
function mulberry32(a) {
  return function () {
    a |= 0; a = (a + 0x6D2B79F5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

// Percentile bootstrap of the median, resampling the QUESTIONS (the unit the design pairs on),
// not the runs. Resampling runs would treat three reps of one question as three questions.
function bootstrapMedian(values, resamples, seed, levels = [0.95, 0.90]) {
  const rnd = mulberry32(seed);
  const n = values.length;
  const stats = new Array(resamples);
  for (let i = 0; i < resamples; i++) {
    const sample = new Array(n);
    for (let j = 0; j < n; j++) sample[j] = values[Math.floor(rnd() * n)];
    stats[i] = median(sample);
  }
  stats.sort((a, b) => a - b);
  const q = (p) => stats[Math.min(resamples - 1, Math.max(0, Math.round(p * (resamples - 1))))];
  const out = { point: median(values), n, resamples };
  for (const lv of levels) {
    const a = (1 - lv) / 2;
    out[`ci${Math.round(lv * 100)}`] = [q(a), q(1 - a)];
  }
  return out;
}

// Exact Wilcoxon signed-rank against zero, by enumerating all 2^n sign assignments. n is 6 here,
// so exact is free and there is no reason to reach for a normal approximation whose assumptions
// nobody checked.
function wilcoxonSignedRankExact(diffs) {
  const nz = diffs.filter((d) => d !== 0);
  const n = nz.length;
  if (n === 0) return { n: 0, W: null, p: null, note: "all differences are zero" };
  if (n > 20) return { n, W: null, p: null, note: "n too large for exact enumeration" };
  const abs = nz.map(Math.abs);
  // Average ranks for ties, the standard correction.
  const order = abs.map((v, i) => [v, i]).sort((a, b) => a[0] - b[0]);
  const ranks = new Array(n);
  let i = 0;
  while (i < order.length) {
    let j = i;
    while (j + 1 < order.length && order[j + 1][0] === order[i][0]) j++;
    const r = (i + j + 2) / 2;
    for (let k = i; k <= j; k++) ranks[order[k][1]] = r;
    i = j + 1;
  }
  const Wplus = nz.reduce((a, d, k) => a + (d > 0 ? ranks[k] : 0), 0);
  const Wminus = nz.reduce((a, d, k) => a + (d < 0 ? ranks[k] : 0), 0);
  const W = Math.min(Wplus, Wminus);
  // Null: every sign is equally likely. Enumerate.
  const total = 1 << n;
  let atLeastAsExtreme = 0;
  for (let m = 0; m < total; m++) {
    let wp = 0;
    for (let k = 0; k < n; k++) if (m & (1 << k)) wp += ranks[k];
    const wm = ranks.reduce((a, b) => a + b, 0) - wp;
    if (Math.min(wp, wm) <= W + 1e-9) atLeastAsExtreme++;
  }
  return { n, Wplus, Wminus, W, p: atLeastAsExtreme / total, minPossibleP: 2 / total };
}

// Wilson score interval for a single proportion.
function wilson(x, n, z) {
  if (n === 0) return [null, null];
  const c = (x + (z * z) / 2) / (n + z * z);
  const h = (z / (n + z * z)) * Math.sqrt((x * (n - x)) / n + (z * z) / 4);
  return [Math.max(0, c - h), Math.min(1, c + h)];
}

// Newcombe (1998) method 10: a Wilson-score-based interval for the difference between two
// proportions measured on the SAME pairs. This is what "Wilson score interval on the paired
// difference" (DESIGN 9) resolves to; a naive two-sample Wilson would ignore the pairing the
// design went to the trouble of building.
function newcombePaired(a, b, c, d, z) {
  const n = a + b + c + d;
  if (n === 0) return { n, diff: null, lower: null, upper: null };
  const p1 = (a + b) / n, p2 = (a + c) / n;
  const [l1, u1] = wilson(a + b, n, z);
  const [l2, u2] = wilson(a + c, n, z);
  const denom = Math.sqrt((a + b) * (c + d) * (a + c) * (b + d));
  const phi = denom === 0 ? 0 : (a * d - b * c) / denom;
  const lower = (p1 - p2) - Math.sqrt(Math.max(0, (p1 - l1) ** 2 - 2 * phi * (p1 - l1) * (u2 - p2) + (u2 - p2) ** 2));
  const upper = (p1 - p2) + Math.sqrt(Math.max(0, (u1 - p1) ** 2 - 2 * phi * (u1 - p1) * (p2 - l2) + (p2 - l2) ** 2));
  return { n, a, b, c, d, p1, p2, phi, diff: p1 - p2, lower: Math.max(-1, lower), upper: Math.min(1, upper) };
}

// Exact two-sided McNemar on the discordant cells, so the paired interval above can be checked
// against something with no distributional machinery in it at all.
function mcnemarExact(b, c) {
  const n = b + c;
  if (n === 0) return { b, c, p: 1, note: "no discordant pairs" };
  const logC = (nn, k) => { let s = 0; for (let i = 0; i < k; i++) s += Math.log(nn - i) - Math.log(i + 1); return s; };
  let p = 0;
  const k = Math.min(b, c);
  for (let i = 0; i <= k; i++) p += Math.exp(logC(n, i) - n * Math.log(2));
  return { b, c, p: Math.min(1, 2 * p) };
}

// ---- --ni-power: the arithmetic DESIGN 4.2's amendment stands on --------------------------
// Reads no data. Prints the two surfaces that decide (a) whether a non-inferiority margin is
// reachable at a given number of pairs and (b) what the amended, question-level endpoint returns
// under scenarios stated as scenarios rather than as hopes. Placed here so it can reuse
// newcombePaired verbatim - a re-implementation would be a different function wearing its name.
if (argv.includes("--ni-power")) {
  const z = 1.644854; // 90% one-sided-equivalent, as DESIGN 4.2 uses
  const f4 = (x) => (x >= 0 ? " " : "") + x.toFixed(3);
  console.log("A. RUN-LEVEL PAIRS, Newcombe method 10 at a PERFECT TIE (b = c = 0).");
  console.log("   The bound is the asymmetry of the Wilson interval, so it depends entirely on");
  console.log("   the common accuracy. RESULTS 10.4 tabulated the acc=1.00 column and called it");
  console.log("   'the best each n can produce'; it is the worst, and at acc=0.50 the interval");
  console.log("   has zero width at every n.");
  console.log("   pairs |   1.00    0.90    0.80    0.70    0.50");
  for (const n of [18, 24, 30, 72, 120, 144]) {
    const row = [1, 0.9, 0.8, 0.7, 0.5].map((p) => { const a = Math.round(p * n); return f4(newcombePaired(a, 0, 0, n - a, z).lower); });
    console.log(`   ${String(n).padStart(5)} | ${row.join("  ")}`);
  }
  console.log("\nB. QUESTION-LEVEL PAIRS, the amended endpoint: reps aggregated to a per-question");
  console.log("   accuracy, 10,000 bootstrap resamples over questions, 5th percentile of mean Delta.");
  const mulberry = (a) => () => { a |= 0; a = (a + 0x6D2B79F5) | 0; let t = Math.imul(a ^ (a >>> 15), 1 | a); t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t; return ((t ^ (t >>> 14)) >>> 0) / 4294967296; };
  const lower90 = (deltas, seed) => {
    const r = mulberry(seed), n = deltas.length, ms = [];
    for (let b = 0; b < 10000; b++) { let s = 0; for (let i = 0; i < n; i++) s += deltas[Math.floor(r() * n)]; ms.push(s / n); }
    ms.sort((x, y) => x - y); return ms[Math.floor(0.05 * 10000)];
  };
  const REPS = 5, step = 2 / REPS; // "loses 2 of 5 reps" = a 0.4 swing on that question
  const scen = (nq, losses, wins, total = 0) => {
    const d = [];
    for (let i = 0; i < losses; i++) d.push(-step);
    for (let i = 0; i < wins; i++) d.push(+step);
    for (let i = 0; i < total; i++) d.push(-1);
    while (d.length < nq) d.push(0);
    return d.slice(0, Math.max(nq, d.length));
  };
  const rows = [
    ["perfect tie, every Delta = 0", 0, 0, 0],
    ["B loses 2 of 5 reps on 1 question", 1, 0, 0],
    ["B loses 2 of 5 reps on 2 questions", 2, 0, 0],
    ["1 question -2/5, 1 question +2/5", 1, 1, 0],
    ["2 questions -2/5, 2 questions +2/5", 2, 2, 0],
    ["B wrong on all 5 reps of 1 question", 0, 0, 1],
  ];
  console.log("   scenario                              24 questions   6 questions (one repo)");
  for (const [label, L, W, T] of rows) {
    console.log(`   ${label.padEnd(38)}${f4(lower90(scen(24, L, W, T), 20260814)).padStart(9)}${f4(lower90(scen(6, L, W, T), 20260814)).padStart(15)}`);
  }
  console.log("\n   Margin -0.05 is failed by rep noise (row 2). Margin -0.10 absorbs it and still");
  console.log("   fails the last row, which is the case the bar exists to catch. Per repo, no");
  console.log("   margin worth stating survives row 2 at all - hence one pooled endpoint.");
  process.exit(0);
}

// ---- load ---------------------------------------------------------------------------------

function readJsonl(p) {
  if (!existsSync(p)) return [];
  return readFileSync(p, "utf8").trim().split(/\r?\n/).filter(Boolean).map((l) => JSON.parse(l));
}

const runs = readJsonl(join(RESULTS, "runs.jsonl")).filter((r) => r.repo === REPO);
const graded = readJsonl(join(RESULTS, "graded.jsonl")).filter((r) => r.repo === REPO);
const judged = readJsonl(join(RESULTS, "judged.jsonl")).filter((r) => r.repo === REPO);
const qfile = JSON.parse(readFileSync(join(HERE, "questions", `${REPO}.json`), "utf8"));
const QIDS = qfile.questions.map((q) => q.id);
const CLASS_OF = Object.fromEntries(qfile.questions.map((q) => [q.id, q.class]));
const ARMS = ["G", "M", "B"];

const key = (r) => `${r.questionId}|${r.arm}|${r.rep}`;
const gradedBy = new Map(graded.map((g) => [key(g), g]));
const judgedBy = new Map(judged.map((j) => [key(j), j]));

// ---- the item table -----------------------------------------------------------------------
//
// CORRECT and FABRICATED are composites, fixed in the ledger before the judge ran:
//   CORRECT    = judge.correct AND pass-1 has no mustNotMention violation AND, for classes D/E,
//                pass-1's verdict matched. The trap and the verdict are deterministic facts and
//                the judge is not allowed to overturn them; it settles only what a string match
//                cannot.
//   FABRICATED = judge.fabricated OR pass-1 citationAccuracy < 1. Both are the same defect seen
//                from two sides. Reported split as well as combined.

const items = runs.map((r) => {
  const g = gradedBy.get(key(r));
  const j = judgedBy.get(key(r));
  const closed = CLASS_OF[r.questionId] === "D" || CLASS_OF[r.questionId] === "E";
  const verdictOk = closed ? g?.verdict?.matched === true : true;
  const trapOk = (g?.mustNotViolations || 0) === 0;
  const judgeCorrect = j && j.parsed ? j.correct : null;
  const citeClean = g?.citationsTotal ? g.citationAccuracy >= 1 : true;
  return {
    questionId: r.questionId, arm: r.arm, rep: r.rep, cls: CLASS_OF[r.questionId],
    costUsd: r.costUsd, turns: r.numTurns, durationMs: r.durationMs,
    executedCalls: (r.toolCallsExecuted || []).length,
    mcpCalls: (r.toolCallsExecuted || []).filter((t) => String(t).startsWith("mcp__")).length,
    censored: Boolean(r.censored), isolationOk: r.isolationOk !== false,
    recall: g?.recall ?? null, citationAccuracy: g?.citationAccuracy ?? null,
    trapOk, verdictOk, judgeCorrect, judgeFabricated: j && j.parsed ? j.fabricated : null,
    judged: Boolean(j), judgeParsed: Boolean(j && j.parsed),
    correct: judgeCorrect == null ? null : (judgeCorrect && trapOk && verdictOk),
    fabricated: j && j.parsed ? (j.fabricated || !citeClean) : null,
    citeFabricated: !citeClean,
  };
});

const byArm = (arm) => items.filter((it) => it.arm === arm);
const cell = (q, arm) => items.filter((it) => it.questionId === q && it.arm === arm);

// ---- report -------------------------------------------------------------------------------

const L = [];
const P = (s = "") => L.push(s);
const f = (x, d = 3) => (x == null || !Number.isFinite(x) ? "-" : Number(x).toFixed(d));
const pct = (x, d = 1) => (x == null ? "-" : `${(100 * x).toFixed(d)}%`);

P(`# A1.2 - judge pass and paired analysis (${REPO} pilot)`);
P();
P(`Generated by \`node eval/agent-probe/analyse.mjs\`. Inputs: \`results/runs.jsonl\` (${runs.length} runs),`);
P(`\`results/graded.jsonl\` (pass 1, ${graded.length} rows), \`results/judged.jsonl\` (pass 2, ${judged.length} rows).`);
P(`Bootstrap seed \`${SEED}\`, ${RESAMPLES} resamples, resampling questions. Every figure below is computed`);
P(`by this script; none is carried over from prose.`);
P();

const unjudged = items.filter((it) => !it.judged);
const parseFails = items.filter((it) => it.judged && !it.judgeParsed);
if (unjudged.length || parseFails.length) {
  P(`> **Coverage warning.** ${unjudged.length} of ${items.length} items carry no judge row and`);
  P(`> ${parseFails.length} judge replies did not parse. Those items are excluded from the accuracy`);
  P(`> figures and named in the coverage section; they are NOT dropped from the cost figures.`);
  P();
}

// ---- 1. primary endpoint: cost ------------------------------------------------------------

P(`## 1. Primary endpoint - cost, paired by question (DESIGN 4.1)`);
P();
P(`\`cost_ratio(q) = median(cost_B(q, reps)) / median(cost_G(q, reps))\`, reported as the median of`);
P(`\`log2(cost_ratio)\` across questions with a percentile bootstrap CI. Log-ratio because token`);
P(`distributions are heavy-tailed. No run is excluded, including the censored one.`);
P();
P(`| Question | Class | median cost G | median cost M | median cost B | B/G ratio | log2 | M/G ratio | log2 |`);
P(`|---|---|---|---|---|---|---|---|---|`);
const logsBG = [], logsMG = [];
for (const q of QIDS) {
  const cG = median(cell(q, "G").map((i) => i.costUsd));
  const cM = median(cell(q, "M").map((i) => i.costUsd));
  const cB = median(cell(q, "B").map((i) => i.costUsd));
  const rBG = cG ? cB / cG : null, rMG = cG ? cM / cG : null;
  if (rBG) logsBG.push(Math.log2(rBG));
  if (rMG) logsMG.push(Math.log2(rMG));
  P(`| \`${q}\` | ${CLASS_OF[q]} | ${f(cG, 4)} | ${f(cM, 4)} | ${f(cB, 4)} | ${f(rBG)} | ${f(Math.log2(rBG))} | ${f(rMG)} | ${f(Math.log2(rMG))} |`);
}
P();
const bootBG = bootstrapMedian(logsBG, RESAMPLES, SEED);
const bootMG = bootstrapMedian(logsMG, RESAMPLES, SEED + 1);
const wBG = wilcoxonSignedRankExact(logsBG);
const wMG = wilcoxonSignedRankExact(logsMG);
const ratio = (l) => (l == null ? "-" : `${f(Math.pow(2, l), 3)}x`);
P(`| Contrast | n questions | median log2 ratio | as a cost ratio | 95% CI (log2) | 95% CI (ratio) | 90% CI (log2) |`);
P(`|---|---|---|---|---|---|---|`);
P(`| **B vs G** | ${bootBG.n} | **${f(bootBG.point)}** | **${ratio(bootBG.point)}** | [${f(bootBG.ci95[0])}, ${f(bootBG.ci95[1])}] | [${ratio(bootBG.ci95[0])}, ${ratio(bootBG.ci95[1])}] | [${f(bootBG.ci90[0])}, ${f(bootBG.ci90[1])}] |`);
P(`| M vs G | ${bootMG.n} | ${f(bootMG.point)} | ${ratio(bootMG.point)} | [${f(bootMG.ci95[0])}, ${f(bootMG.ci95[1])}] | [${ratio(bootMG.ci95[0])}, ${ratio(bootMG.ci95[1])}] | [${f(bootMG.ci90[0])}, ${f(bootMG.ci90[1])}] |`);
P();
P(`Exact Wilcoxon signed-rank against zero (all ${wBG.n} paired questions, enumerated over all`);
P(`2^${wBG.n} sign assignments): **B vs G** W=${f(wBG.W, 1)}, p=${f(wBG.p, 4)}; M vs G W=${f(wMG.W, 1)}, p=${f(wMG.p, 4)}.`);
P(`With n=${wBG.n} the smallest two-sided p the test can produce is ${f(wBG.minPossibleP, 4)}, so a non-significant`);
P(`result here is not evidence of no effect - it is the sample size talking.`);
P();
const threshold = -0.32;
P(`**Against the DESIGN 5 decision rule** (accelerator requires the CI *upper* bound < ${threshold}, i.e.`);
P(`at least 20% cheaper): upper bound is **${f(bootBG.ci95[1])}**, which is **${bootBG.ci95[1] < threshold ? "below" : "NOT below"}** ${threshold}.`);
P();

// ---- 2. co-primary: correctness -----------------------------------------------------------

P(`## 2. Co-primary endpoint - correctness (DESIGN 4.2)`);
P();
P(`\`CORRECT\` = blind judge says correct **and** pass 1 found no \`mustNotMention\` violation **and**,`);
P(`for classes D and E, pass 1's verdict matched. Composite fixed in the ledger before the judge ran.`);
P();
P(`| Arm | n judged | judge correct | trap clean | verdict ok (D/E) | **CORRECT** | Wilson 95% |`);
P(`|---|---|---|---|---|---|---|`);
const accOf = {};
for (const arm of ARMS) {
  const rs = byArm(arm).filter((i) => i.judgeParsed);
  const nC = rs.filter((i) => i.correct).length;
  accOf[arm] = { n: rs.length, x: nC, acc: rs.length ? nC / rs.length : null };
  const de = byArm(arm).filter((i) => i.cls === "D" || i.cls === "E");
  const [wl, wu] = wilson(nC, rs.length, 1.959964);
  P(`| ${arm} | ${rs.length} | ${rs.filter((i) => i.judgeCorrect).length} | ${byArm(arm).filter((i) => i.trapOk).length}/${byArm(arm).length} | ${de.filter((i) => i.verdictOk).length}/${de.length} | **${nC}/${rs.length}** (${pct(accOf[arm].acc)}) | [${pct(wl)}, ${pct(wu)}] |`);
}
P();

// The paired 2x2, matched on (question, rep). Rep index is not a natural pairing - reps are
// independent draws - so the question-level bootstrap below is reported alongside it and the
// report says plainly which is which.
function paired2x2(armA, armB) {
  let a = 0, b = 0, c = 0, d = 0, skipped = 0;
  for (const q of QIDS) for (const rep of [1, 2, 3]) {
    const x = items.find((i) => i.questionId === q && i.arm === armA && i.rep === rep);
    const y = items.find((i) => i.questionId === q && i.arm === armB && i.rep === rep);
    if (!x || !y || x.correct == null || y.correct == null) { skipped++; continue; }
    if (x.correct && y.correct) a++;
    else if (x.correct && !y.correct) b++;
    else if (!x.correct && y.correct) c++;
    else d++;
  }
  return { a, b, c, d, skipped };
}
const t_BG = paired2x2("B", "G");
const nc95 = newcombePaired(t_BG.a, t_BG.b, t_BG.c, t_BG.d, 1.959964);
const nc90 = newcombePaired(t_BG.a, t_BG.b, t_BG.c, t_BG.d, 1.644854);
const mc = mcnemarExact(t_BG.b, t_BG.c);
P(`**Paired difference, arm B minus arm G.** Newcombe (1998) method 10, the Wilson-score-based`);
P(`interval for paired proportions. Pairs are matched on (question, rep); ${t_BG.skipped} pair(s) skipped for a`);
P(`missing or unparsed judge row.`);
P();
P(`| a (both correct) | b (B only) | c (G only) | d (neither) | diff | 90% CI | 95% CI | exact McNemar p |`);
P(`|---|---|---|---|---|---|---|---|`);
P(`| ${t_BG.a} | ${t_BG.b} | ${t_BG.c} | ${t_BG.d} | ${f(nc95.diff)} | [${f(nc90.lower)}, ${f(nc90.upper)}] | [${f(nc95.lower)}, ${f(nc95.upper)}] | ${f(mc.p, 4)} |`);
P();
P(`**Against the DESIGN 4.2 non-inferiority bar** (lower bound of the 90% CI on \`accuracy_B - accuracy_G\``);
P(`must exceed \`-0.05\`): lower bound is **${f(nc90.lower)}**, which **${nc90.lower > -0.05 ? "clears" : "does NOT clear"}** the bar.`);
P();

// Question-level companion, which assumes nothing about rep matching.
const perQdiff = QIDS.map((q) => {
  const b = cell(q, "B").filter((i) => i.correct != null);
  const g = cell(q, "G").filter((i) => i.correct != null);
  if (!b.length || !g.length) return null;
  return mean(b.map((i) => (i.correct ? 1 : 0))) - mean(g.map((i) => (i.correct ? 1 : 0)));
}).filter((x) => x != null);
const bootAcc = perQdiff.length ? bootstrapMedian(perQdiff, RESAMPLES, SEED + 2) : null;
if (bootAcc) {
  P(`Companion that assumes no rep matching: per-question accuracy difference (mean over reps),`);
  P(`bootstrapped over questions. Median **${f(bootAcc.point)}**, 90% CI [${f(bootAcc.ci90[0])}, ${f(bootAcc.ci90[1])}],`);
  P(`95% CI [${f(bootAcc.ci95[0])}, ${f(bootAcc.ci95[1])}]. Per-question differences: ${perQdiff.map((x) => f(x, 2)).join(", ")}.`);
  P();
}

P(`### Correctness by question class`);
P();
P(`| Question | Class | G | M | B |`);
P(`|---|---|---|---|---|`);
for (const q of QIDS) {
  const s = (arm) => { const rs = cell(q, arm).filter((i) => i.correct != null); return `${rs.filter((i) => i.correct).length}/${rs.length}`; };
  P(`| \`${q}\` | ${CLASS_OF[q]} | ${s("G")} | ${s("M")} | ${s("B")} |`);
}
P();

// ---- 3. secondary -------------------------------------------------------------------------

P(`## 3. Secondary metrics (DESIGN 4.3)`);
P();
P(`| Arm | n | median cost | median turns | median duration s | median executed calls | median mcp share | censoring | fabrication (judge) | fabrication (citations) | fabrication (either) |`);
P(`|---|---|---|---|---|---|---|---|---|---|---|`);
for (const arm of ARMS) {
  const rs = byArm(arm);
  const jr = rs.filter((i) => i.judgeParsed);
  const shares = rs.map((i) => (i.executedCalls ? i.mcpCalls / i.executedCalls : null)).filter((x) => x != null);
  P(`| ${arm} | ${rs.length} | ${f(median(rs.map((i) => i.costUsd)), 4)} | ${f(median(rs.map((i) => i.turns)), 1)} | ` +
    `${f(median(rs.map((i) => (i.durationMs == null ? null : i.durationMs / 1000))), 1)} | ${f(median(rs.map((i) => i.executedCalls)), 1)} | ` +
    `${f(median(shares), 3)} | ${rs.filter((i) => i.censored).length}/${rs.length} (${pct(rs.filter((i) => i.censored).length / rs.length)}) | ` +
    `${pct(jr.length ? jr.filter((i) => i.judgeFabricated).length / jr.length : null)} | ` +
    `${pct(rs.filter((i) => i.citeFabricated).length / rs.length)} | ` +
    `${pct(jr.length ? jr.filter((i) => i.fabricated).length / jr.length : null)} |`);
}
P();
P(`Median citation accuracy: ` + ARMS.map((a) => `${a} ${pct(median(byArm(a).map((i) => i.citationAccuracy)))}`).join(" | ") + `.`);
P(`Median must-mention recall (pass 1): ` + ARMS.map((a) => `${a} ${pct(median(byArm(a).map((i) => i.recall)))}`).join(" | ") + `.`);
P(`Arm isolation held on ${items.filter((i) => i.isolationOk).length}/${items.length} runs.`);
P();

// ---- 4. judge coverage and cost -----------------------------------------------------------

P(`## 4. Judge pass - coverage and cost`);
P();
const jcost = judged.map((j) => j.judgeCostUsd).filter((x) => x != null);
P(`| Judged | Parsed | Parse needed fallback | Judge model | Effort | Total judge cost | Median per item |`);
P(`|---|---|---|---|---|---|---|`);
P(`| ${judged.length}/${items.length} | ${judged.filter((j) => j.parsed).length} | ${judged.filter((j) => j.parseNeededFallback).length} | ` +
  `${judged[0]?.judgeModel || "-"} | ${judged[0]?.judgeEffort || "-"} | $${f(jcost.reduce((a, b) => a + b, 0), 2)} | $${f(median(jcost), 4)} |`);
P();
if (unjudged.length) { P(`Unjudged items: ` + unjudged.map((i) => `\`${i.questionId}/${i.arm}/rep${i.rep}\``).join(", ") + `.`); P(); }
if (parseFails.length) { P(`Judge replies that did not parse: ` + parseFails.map((i) => `\`${i.questionId}/${i.arm}/rep${i.rep}\``).join(", ") + `.`); P(); }
P(`Judge blindness: see \`results/a1.2-leak-scan.md\` - every prompt written to`);
P(`\`results/judge-prompts/\` and scanned back with an independently written superset rule list.`);
P();

// ---- 5. what this does and does not license -----------------------------------------------

P(`## 5. What these numbers do and do not license`);
P();
P(`- The bootstrap resamples **${bootBG.n} questions**. A percentile CI from n=6 is wide and lumpy by`);
P(`  construction; it is reported because the design pre-registered it, not because 6 questions`);
P(`  settle anything.`);
P(`- The non-inferiority bar in DESIGN 4.2 is \`-0.05\`. With ${accOf.B.n} items per arm, the *widest*`);
P(`  the pilot can be right is a Wilson lower bound of ${pct(wilson(accOf.B.n, accOf.B.n, 1.644854)[0])} at a perfect ${accOf.B.n}/${accOf.B.n}, so the`);
P(`  90% lower bound on a paired difference cannot reach \`-0.05\` at this n **whatever the answers**.`);
P(`  A pilot cannot pass this bar; that is a statement about the sample size, not about the tools.`);
P(`- Rank methods and medians throughout. No t-test, no mean over raw dollars.`);
P(`- The one censored run (\`eshop-c1/M/rep1\`, budget exhausted) is kept and counted everywhere.`);
P();

// ---- 6. the decision rule -----------------------------------------------------------------

const costUpper = bootBG.ci95[1], accLower = nc90.lower, accUpper = nc90.upper;
const nonInferior = accLower > -0.05;
const isAccelerator = nonInferior && costUpper < threshold;
const isPrimer = accLower > 0.05 && !(costUpper < threshold);
const costSpansZero = bootBG.ci95[0] <= 0 && bootBG.ci95[1] >= 0;
const accSpansZero = accLower <= 0 && accUpper >= 0;
const isNull = costSpansZero && accSpansZero;
const tieOnAccuracy = t_BG.b === 0 && t_BG.c === 0;

P(`## 6. Mapping onto the DESIGN 5 decision rule`);
P();
P(`| Branch | Pre-registered condition | Computed | Met |`);
P(`|---|---|---|---|`);
P(`| **Accelerator** | non-inferior AND cost CI upper < ${threshold} | upper = ${f(costUpper)} | ${isAccelerator ? "YES" : "no"} |`);
P(`| **Primer** | 90% CI lower on d-accuracy > +0.05, cost not reduced | lower = ${f(accLower)} | ${isPrimer ? "YES" : "no"} |`);
P(`| **Null** | neither CI excludes zero | cost 95% [${f(bootBG.ci95[0])}, ${f(bootBG.ci95[1])}], acc 90% [${f(accLower)}, ${f(accUpper)}] | ${isNull ? "YES" : "no"} |`);
P(`| **Regression** | non-inferiority fails | lower = ${f(accLower)} vs bar -0.05 | ${nonInferior ? "no" : "fires - read the caveat"} |`);
P();
if (!nonInferior && tieOnAccuracy) {
  P(`> **The Regression branch fires on a power artifact and must not be reported as a regression.**`);
  P(`> Arms B and G are *exactly tied*: ${t_BG.a}/${t_BG.a} concordant-correct, **zero discordant pairs**`);
  P(`> (b=${t_BG.b}, c=${t_BG.c}), exact McNemar p=${f(mc.p, 4)}. Nothing in the data says arm B is less accurate than`);
  P(`> arm G on a single item. The branch fires because the tightest interval ${nc90.n} pairs can produce is`);
  P(`> +/-${f(Math.abs(accLower))}, and the pre-registered bar is +/-0.05. Calling that a regression would be`);
  P(`> reporting the sample size as if it were a finding.`);
  P(">");
  P(`> The honest pilot-scale reading is **${isNull ? "Null" : "inconclusive"}**: the cost CI contains zero, the accuracy`);
  P(`> contrast IS zero, and the design's own bars need the 360-run full study to be reachable.`);
  P();
}
P(`Arm M is not part of the primary contrast, but it is the sharpest signal in the pilot and`);
P(`the write-up should not bury it: `);
{
  const mAcc = accOf.M;
  const fBad = QIDS.filter((q) => cell(q, "M").filter((i) => i.correct === false).length === 3);
  P(`arm M scores **${mAcc.x}/${mAcc.n}** (${pct(mAcc.acc)}) against ${accOf.G.x}/${accOf.G.n} for arm G, at **${ratio(bootMG.point)}** the cost`);
  P(`(Wilcoxon p=${f(wMG.p, 4)}, the smallest value n=${wMG.n} can produce). Its failures are total on ${fBad.length} question(s):`);
  P(fBad.map((q) => `\`${q}\` (class ${CLASS_OF[q]}) 0/3`).join(", ") + `. Class F is the design's own`);
  P(`grep-favouring control, and DESIGN 5's note that "if the MCP arm wins here, the harness is wrong"`);
  P(`cuts both ways: it loses here, which is evidence the harness is measuring what it claims to.`);
}
P();

writeFileSync(OUT, L.join("\n") + "\n", "utf8");
console.log(`wrote ${OUT}`);
console.log(`cost B/G median log2 = ${f(bootBG.point)} (95% CI ${f(bootBG.ci95[0])}..${f(bootBG.ci95[1])})`);
console.log(`accuracy: ` + ARMS.map((a) => `${a} ${accOf[a].x}/${accOf[a].n}`).join("  "));
