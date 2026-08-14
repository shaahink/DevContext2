// A1.1 - deterministic grading pass (DESIGN.md section 7, pass 1).
//
//   node eval/agent-probe/grade.mjs                 # grade every run in results/runs.jsonl
//   node eval/agent-probe/grade.mjs --check         # re-grade, compare to results/graded.jsonl, exit 1 on drift
//   node eval/agent-probe/grade.mjs --repo eShop
//
// No model is involved here and none may be: pass 1 is the half of the protocol that has to be
// reproducible byte-for-byte, so every rule below is a string operation over the answer text.
// Pass 2 (the LLM judge) is A1.2 and lives elsewhere.
//
// The rules were fixed in the ledger BEFORE this file was written, and the two that carry real
// weight are pre-registered outside it: DESIGN 7 ("Class E items score correct only on an explicit
// 'none'") and SCHEMA.md ("How mustNotMention is scored" - a violation requires the answer to put
// the term FORWARD; naming it in order to exclude it is the discrimination the trap tests for).
//
// What this file deliberately does NOT do is score classes A, B and C correct/incorrect. DESIGN
// 3.3 scores C on recall and precision and B on trace ordering; both are judge work. Those rows
// carry recall plus deterministicCorrect: null. Inventing a 0/1 there would manufacture a number
// the pre-registration does not license.

import { readFileSync, writeFileSync, existsSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { execFileSync } from "node:child_process";

const HERE = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = join(HERE, "..", "..");
const EVAL_REPOS = join(REPO_ROOT, "eval-repos");
const RUNS = join(HERE, "results", "runs.jsonl");
const OUT = join(HERE, "results", "graded.jsonl");
const OUT_MD = join(HERE, "results", "a1.1-grading.md");

const CHECK_ONLY = process.argv.includes("--check");
const repoArg = process.argv.indexOf("--repo");
const ONLY_REPO = repoArg > -1 ? process.argv[repoArg + 1] : null;

function die(msg) {
  console.error(`\nGRADER REFUSED: ${msg}\n`);
  process.exit(2);
}

// ---- text normalisation -----------------------------------------------------

// Windows answers mix \ and / in paths. Normalise for path work only; symbol matching is done
// against the raw text so word boundaries mean what they say.
const slash = (s) => String(s).replace(/\\/g, "/");
const isPathLike = (term) => term.includes("/") || /\.(cs|json|razor|csproj|md|sln|ya?ml)$/i.test(term);

const escapeRx = (s) => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

// Sentence split that survives code. A terminator only splits when followed by whitespace AND an
// uppercase letter, a quote or a bullet - so "appsettings.json" and "Extensions.cs is" stay whole
// while "SaveChangesAsync(). Then" splits. Newlines and list markers also split, because answers
// are usually markdown.
function sentences(text) {
  return String(text)
    .split(/\r?\n/)
    .flatMap((line) => line.split(/(?<=[.!?;:])\s+(?=[A-Z"'`(\[*-])/))
    .map((s) => s.trim())
    .filter(Boolean);
}

// ---- mustMention / mustNotMention -------------------------------------------

// Returns every occurrence of `term` in `answer` as {sentence}. Symbols match case-sensitively on
// word boundaries; paths match case-insensitively, and require enough of the path to disambiguate
// (see the eshop-f1 note below).
function occurrences(answer, term) {
  const hits = [];
  if (isPathLike(term)) {
    const norm = slash(answer).toLowerCase();
    const full = slash(term).toLowerCase();
    // eshop-f1 has src/OrderProcessor/appsettings.json in mustMention and
    // src/Ordering.API/appsettings.json in mustNotMention. Matching on the basename would score a
    // single "appsettings.json" as BOTH a hit and a violation, so a candidate must keep at least
    // one directory component. `src/` may be dropped (agents often cite from the repo root).
    const candidates = [full];
    const noSrc = full.replace(/^src\//, "");
    if (noSrc !== full && noSrc.includes("/")) candidates.push(noSrc);
    for (const c of candidates) {
      let i = norm.indexOf(c);
      while (i !== -1) {
        hits.push({ term, matched: c, sentence: windowAround(answer, i, c.length) });
        i = norm.indexOf(c, i + c.length);
        if (hits.length > 50) break;
      }
      if (hits.length) break; // the fuller candidate wins; don't double-count the same text
    }
    return hits;
  }
  const rx = new RegExp(`(?<![A-Za-z0-9_])${escapeRx(term)}(?![A-Za-z0-9_])`, "g");
  let m;
  while ((m = rx.exec(answer)) !== null) {
    hits.push({ term, matched: term, sentence: windowAround(answer, m.index, term.length) });
    if (hits.length > 50) break;
  }
  return hits;
}

// The sentence containing offset..offset+len, for the exclusion test and for human re-check.
function windowAround(answer, offset, len) {
  const before = answer.slice(0, offset);
  const startNl = Math.max(before.lastIndexOf("\n"), before.lastIndexOf("\r"));
  let start = startNl + 1;
  const sentBreak = /[.!?;:]\s+(?=[A-Z"'`(\[*-])/g;
  sentBreak.lastIndex = start;
  let m;
  while ((m = sentBreak.exec(before)) !== null) start = m.index + m[0].length;
  const after = answer.slice(offset + len);
  const endNl = after.search(/[\r\n]/);
  const endSent = after.search(/[.!?;:]\s+(?=[A-Z"'`(\[*-])/);
  const cands = [endNl, endSent].filter((x) => x > -1);
  const end = offset + len + (cands.length ? Math.min(...cands) + 1 : Math.min(after.length, 240));
  return answer.slice(start, end).replace(/\s+/g, " ").trim();
}

// SCHEMA.md, pre-registered: an answer that names a trap term IN ORDER TO EXCLUDE IT is exhibiting
// exactly the discrimination the trap tests for and scores clean. A naive substring test would
// penalise the most careful answers in every arm and add noise to the primary endpoint.
const EXCLUSION = [
  /\bnot\s+(affected|impacted|involved|included|related|part|a\s+consumer|among)\b/i,
  /\bunaffected\b/i, /\bunrelated\b/i, /\bexcluded?\b/i, /\bdoes\s+not\b/i, /\bdo\s+not\b/i,
  /\bdoesn'?t\b/i, /\bdon'?t\b/i, /\bis\s+not\b/i, /\bare\s+not\b/i, /\bwas\s+not\b/i,
  /\bwere\s+not\b/i, /\bwill\s+not\b/i, /\bwon'?t\b/i, /\bcannot\b/i, /\bcan'?t\b/i,
  /\bnever\b/i, /\bno\s+(other|such|longer)\b/i, /\bnothing\b/i, /\bnone\s+of\b/i,
  /\brather\s+than\b/i, /\binstead\s+of\b/i, /\bas\s+opposed\s+to\b/i, /\bseparate\b/i,
  /\bdistinct\b/i, /\bdifferent\s+(endpoint|handler|path|flow|file)\b/i,
  /\bNOT\b/, /\bonly\s+(place|one|subscriber)\b/i,
];
const isExclusionContext = (sentence) => EXCLUSION.some((rx) => rx.test(sentence));

// ---- verdicts (classes D and E) ---------------------------------------------

const AFFIRM = [/^\s*(\*\*)?yes\b/i, /\byes,\s/i, /\bit\s+does\s+publish\b/i, /\bdoes\s+publish\b/i];
const NEGATE = [
  /^\s*(\*\*)?no\b/i, /\bno,\s/i, /\bthe\s+answer\s+is\s+no\b/i,
  /\bdoes\s+not\s+publish\b/i, /\bdoesn'?t\s+publish\b/i, /\bnever\s+publishes\b/i,
  /\bnever\s+touches\s+the\s+event\s+bus\b/i, /\bdoes\s+not\s+(raise|emit|send|fire)\b/i,
];
// DESIGN 7: "Class E items score correct only on an explicit 'none'."
const NONE = [
  /\bthere\s+are\s+none\b/i, /\bnone\b/i, /\bno\s+(such\s+)?handlers?\b/i,
  /\bno\s+integration\s+event\s+handlers?\b/i, /\bnothing\s+(matches|consumes|subscribes)\b/i,
  /\bthere\s+are\s+no\b/i, /\bdoes\s+not\s+(have|contain|register|subscribe)\b/i,
  /\bno\s+code\s+that\b/i,
];

function verdictOf(answer, expected) {
  const head = answer.slice(0, 600); // an explicit verdict belongs up front; the prompt asks for it
  const hit = (pats, text) => pats.some((rx) => rx.test(text));
  if (expected === "no") {
    const neg = hit(NEGATE, head) || hit(NEGATE, answer);
    const aff = hit(AFFIRM, head);
    // Both, or neither, is not an explicit verdict. Do not resolve it by first-wins.
    return { expected, matched: neg && !aff, sawAffirmative: aff, sawNegative: neg };
  }
  if (expected === "none") {
    const none = hit(NONE, answer);
    return { expected, matched: none, sawNone: none };
  }
  if (expected === "yes") {
    const aff = hit(AFFIRM, head);
    const neg = hit(NEGATE, head);
    return { expected, matched: aff && !neg, sawAffirmative: aff, sawNegative: neg };
  }
  return { expected: null, matched: null };
}

// ---- citation resolution -----------------------------------------------------

// Pull file references out of the answer and resolve them against the pinned tree. Read-only:
// eval-repos is a fixture and nothing here may write to it.
function repoFileSet(repoDir) {
  const out = execFileSync("git", ["-C", repoDir, "ls-files"], { encoding: "utf8", maxBuffer: 64 * 1024 * 1024 });
  const set = new Set();
  const byLower = new Map();
  for (const line of out.split(/\r?\n/)) {
    if (!line) continue;
    set.add(line);
    byLower.set(line.toLowerCase(), line);
  }
  return { set, byLower };
}

const CITE_RX = /(?:^|[\s(`"'\[|>])((?:[A-Za-z0-9_.\-]+\/)+[A-Za-z0-9_.\-]+\.(?:cs|json|razor|csproj|sln|md|ya?ml|props|targets|http|sql))(?::(\d+))?/g;

function citations(answer, files) {
  const seen = new Map();
  let m;
  while ((m = CITE_RX.exec(slash(answer))) !== null) {
    const raw = m[1];
    const line = m[2] ? Number(m[2]) : null;
    const key = `${raw}${line ? `:${line}` : ""}`;
    if (seen.has(key)) continue;
    // Accept a path given from the repo root, or with the eval-repos prefix an agent may echo.
    const trimmed = raw.replace(/^.*?eval-repos\/[A-Za-z0-9_.\-]+\//i, "");
    let resolved = files.byLower.get(trimmed.toLowerCase()) || null;
    if (!resolved) {
      // A suffix match is still a resolution: "Apis/CatalogApi.cs" for "src/Catalog.API/Apis/CatalogApi.cs".
      const suffix = `/${trimmed.toLowerCase()}`;
      for (const [lower, actual] of files.byLower) {
        if (lower.endsWith(suffix)) { resolved = actual; break; }
      }
    }
    seen.set(key, { ref: key, path: trimmed, line, resolved });
  }
  return [...seen.values()];
}

// ---- grade one run ------------------------------------------------------------

function gradeRun(run, question, files) {
  const answer = String(run.answer ?? "");

  const mustMention = question.mustMention.map((term) => {
    const occ = occurrences(answer, term);
    return { term, hit: occ.length > 0, count: occ.length, firstContext: occ[0]?.sentence ?? null };
  });
  const hits = mustMention.filter((m) => m.hit).length;
  const recall = question.mustMention.length ? hits / question.mustMention.length : null;

  const mustNotMention = question.mustNotMention.map((term) => {
    const occ = occurrences(answer, term);
    const asserted = occ.filter((o) => !isExclusionContext(o.sentence));
    return {
      term,
      mentioned: occ.length > 0,
      violation: occ.length > 0 && asserted.length > 0,
      occurrences: occ.length,
      excludedOccurrences: occ.length - asserted.length,
      // Every occurrence is kept verbatim: the R1.2 human sample has to be able to re-judge this
      // call, not take it on trust.
      contexts: occ.map((o) => ({ sentence: o.sentence, scoredAs: isExclusionContext(o.sentence) ? "clean-exclusion" : "violation" })),
    };
  });
  const violations = mustNotMention.filter((m) => m.violation).length;

  const verdict = verdictOf(answer, question.expectedVerdict);
  const cites = citations(answer, files);
  const unresolved = cites.filter((c) => !c.resolved);

  // Classes D, E and F have a closed answer, so pass 1 settles them. A, B and C are recall- and
  // ordering-scored (DESIGN 3.3) and belong to the judge.
  let deterministicCorrect = null;
  const cls = question.class;
  if (cls === "D") deterministicCorrect = verdict.matched === true && violations === 0 && hits === question.mustMention.length;
  else if (cls === "E") deterministicCorrect = verdict.matched === true && violations === 0;
  else if (cls === "F") deterministicCorrect = hits === question.mustMention.length && violations === 0;

  // DESIGN 6.6 overrides the text: a run that hit its cap is scored incorrect at cost = cap,
  // never dropped and never credited for the fragment it produced.
  const censoredOverride = run.censored === true;
  if (censoredOverride) deterministicCorrect = false;

  return {
    repo: run.repo,
    questionId: run.questionId,
    questionClass: cls,
    arm: run.arm,
    rep: run.rep,
    model: run.model,
    censored: run.censored === true,
    costUsd: run.costUsd,
    answerChars: answer.length,
    mustMentionHits: hits,
    mustMentionTotal: question.mustMention.length,
    recall,
    mustMention,
    mustNotViolations: violations,
    mustNotMention,
    verdict,
    citationsTotal: cites.length,
    citationsResolved: cites.length - unresolved.length,
    citationAccuracy: cites.length ? (cites.length - unresolved.length) / cites.length : null,
    citationsUnresolved: unresolved.map((c) => c.ref),
    citations: cites,
    deterministicCorrect,
    censoredOverride,
    gradedBy: "grade.mjs pass 1 (deterministic)",
  };
}

// ---- main ---------------------------------------------------------------------

if (!existsSync(RUNS)) die(`no runs at ${RUNS}`);
const runs = readFileSync(RUNS, "utf8").trim().split(/\r?\n/).filter(Boolean).map((l) => JSON.parse(l));

const qCache = new Map();
function questionsFor(repo) {
  if (qCache.has(repo)) return qCache.get(repo);
  const path = join(HERE, "questions", `${repo}.json`);
  if (!existsSync(path)) die(`no question set for repo ${repo}`);
  const doc = JSON.parse(readFileSync(path, "utf8"));
  const repoDir = join(EVAL_REPOS, doc.repo);
  if (!existsSync(repoDir)) die(`eval-repos/${doc.repo} is not cloned`);
  const head = execFileSync("git", ["-C", repoDir, "rev-parse", "HEAD"], { encoding: "utf8" }).trim();
  if (!head.startsWith(doc.sha)) {
    die(`eval-repos/${doc.repo} is at ${head.slice(0, 8)} but the keys were written against ${doc.sha}. ` +
        "The keys describe one exact tree; grading against another is not a measurement.");
  }
  const entry = { doc, byId: new Map(doc.questions.map((q) => [q.id, q])), files: repoFileSet(repoDir) };
  qCache.set(repo, entry);
  return entry;
}

const graded = [];
for (const run of runs) {
  if (ONLY_REPO && run.repo !== ONLY_REPO) continue;
  const { byId, files } = questionsFor(run.repo);
  const q = byId.get(run.questionId);
  if (!q) die(`run references unknown question ${run.repo}/${run.questionId}`);
  graded.push(gradeRun(run, q, files));
}

if (!graded.length) die("nothing to grade");

const jsonl = graded.map((g) => JSON.stringify(g)).join("\n") + "\n";

// --check separates the two ways this file can disagree with runs.jsonl, because they mean
// opposite things. A SHARED cell whose grade moved is drift - pass 1 is supposed to be
// reproducible byte-for-byte, so that is a real failure. A cell that exists in runs.jsonl and not
// in graded.jsonl is merely stale, which is the normal state of affairs while a batch is still
// appending, and failing a gate for it would turn every mid-batch gate red for hours.
//   exit 0 = reproducible   exit 1 = DRIFT   exit 3 = STALE   exit 2 = refused
if (CHECK_ONLY) {
  if (!existsSync(OUT)) {
    console.error(`STALE: ${OUT} does not exist; run the grader first`);
    process.exit(3);
  }
  const keyOf = (g) => `${g.repo}|${g.questionId}|${g.arm}|${g.rep}|${g.model}`;
  const prev = new Map(
    readFileSync(OUT, "utf8").trim().split(/\r?\n/).filter(Boolean)
      .map((l) => JSON.parse(l)).map((g) => [keyOf(g), g]));

  const drifted = [];
  const missing = [];
  for (const g of graded) {
    const p = prev.get(keyOf(g));
    if (!p) { missing.push(keyOf(g)); continue; }
    if (JSON.stringify(p) !== JSON.stringify(g)) drifted.push(keyOf(g));
  }
  if (drifted.length) {
    console.error(`GRADING DRIFT on ${drifted.length} cell(s): ${drifted.slice(0, 5).join(", ")}`);
    console.error("Pass 1 must be reproducible byte-for-byte. The grader changed, or an answer did.");
    process.exit(1);
  }
  if (missing.length) {
    console.error(`STALE: ${missing.length} recorded run(s) are not in graded.jsonl - re-run the grader.`);
    process.exit(3);
  }
  console.log(`grading reproducible - ${graded.length} run(s) re-graded identically`);
  process.exit(0);
}

writeFileSync(OUT, jsonl, "utf8");

// ---- report -------------------------------------------------------------------

const byArm = (arm) => graded.filter((g) => g.arm === arm);
// The closed-answer classes. Scoped by CLASS, not by "deterministicCorrect is not null" - a
// censored class-C run is also forced to a verdict (DESIGN 6.6) and would otherwise be counted
// into a column labelled D/E/F, quietly reporting 8/10 where the denominator should be 9.
const CLOSED_CLASSES = new Set(["D", "E", "F"]);
const closedOf = (rows) => rows.filter((r) => CLOSED_CLASSES.has(r.questionClass));
const arms = [...new Set(graded.map((g) => g.arm))].sort();
const med = (xs) => {
  const s = xs.filter((x) => x !== null && x !== undefined).slice().sort((a, b) => a - b);
  if (!s.length) return null;
  const m = Math.floor(s.length / 2);
  return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2;
};
const pct = (x) => (x === null ? "-" : `${(100 * x).toFixed(1)}%`);

const L = [];
L.push("# A1.1 - deterministic grading pass (DESIGN 7, pass 1)");
L.push("");
L.push("Generated by `eval/agent-probe/grade.mjs`. No model is involved; every number here is a");
L.push("string operation over the answer text and is reproducible with `--check`. The rules were");
L.push("fixed in the ledger before the grader was written, and the two that matter most are");
L.push("pre-registered outside it: DESIGN 7 (class E scores correct only on an explicit \"none\") and");
L.push("SCHEMA.md (a `mustNotMention` violation requires the answer to put the term forward; naming");
L.push("it in order to exclude it is clean).");
L.push("");
L.push(`Runs graded: **${graded.length}**.`);
L.push("");
L.push("## What pass 1 can and cannot settle");
L.push("");
L.push("Classes **D, E and F** have a closed answer - a verdict or a fixed file set - so they carry a");
L.push("deterministic correct/incorrect. Classes **A, B and C** do not: DESIGN 3.3 scores C on recall");
L.push("and precision and B on trace ordering, which is pass-2 judge work (A1.2). Those rows carry");
L.push("`deterministicCorrect: null` and their recall is reported below without being collapsed to a");
L.push("0/1 the pre-registration does not license.");
L.push("");
L.push("## Per arm");
L.push("");
L.push("| arm | n | median recall | mustNot violations | median citation accuracy | unresolved citations | D/E/F correct |");
L.push("|---|---|---|---|---|---|---|");
for (const arm of arms) {
  const g = byArm(arm);
  const closed = closedOf(g);
  const nCorrect = closed.filter((x) => x.deterministicCorrect).length;
  L.push(`| ${arm} | ${g.length} | ${pct(med(g.map((x) => x.recall)))} | ${g.reduce((a, x) => a + x.mustNotViolations, 0)} `
    + `| ${pct(med(g.map((x) => x.citationAccuracy)))} | ${g.reduce((a, x) => a + x.citationsUnresolved.length, 0)} `
    + `| ${nCorrect}/${closed.length} |`);
}
L.push("");
L.push("## Per question x arm");
L.push("");
L.push("| question | class | arm | median recall | violations | D/E/F correct | median citation accuracy |");
L.push("|---|---|---|---|---|---|---|");
const qids = [...new Set(graded.map((g) => g.questionId))].sort();
for (const qid of qids) {
  for (const arm of arms) {
    const g = graded.filter((x) => x.questionId === qid && x.arm === arm);
    if (!g.length) continue;
    const closed = closedOf(g);
    L.push(`| \`${qid}\` | ${g[0].questionClass} | ${arm} | ${pct(med(g.map((x) => x.recall)))} `
      + `| ${g.reduce((a, x) => a + x.mustNotViolations, 0)} `
      + `| ${closed.length ? `${closed.filter((x) => x.deterministicCorrect).length}/${closed.length}` : "judge"} `
      + `| ${pct(med(g.map((x) => x.citationAccuracy)))} |`);
  }
}
L.push("");
L.push("## Censored runs (DESIGN 6.6)");
L.push("");
const cens = graded.filter((g) => g.censored);
if (!cens.length) L.push("None.");
else {
  L.push("Scored **incorrect at cost = cap** by the pre-registration, whatever the partial answer says.");
  L.push("");
  L.push("| run | arm | cost | recall as written | forced to |");
  L.push("|---|---|---|---|---|");
  for (const c of cens) {
    L.push(`| \`${c.questionId}/${c.arm}/rep${c.rep}\` | ${c.arm} | ${c.costUsd} | ${pct(c.recall)} | incorrect |`);
  }
}
L.push("");
L.push("## Trap terms - every occurrence, with how it was scored");
L.push("");
L.push("`mustNotMention` is the one rule where a naive substring test would punish the most careful");
L.push("answers, so each occurrence is listed with its sentence and the call made on it. This table");
L.push("is what the R1.2 human sample re-checks.");
L.push("");
const trapRows = graded.flatMap((g) => g.mustNotMention.flatMap((m) => m.contexts.map((c) => ({ g, term: m.term, ...c }))));
if (!trapRows.length) L.push("No trap term was mentioned in any run.");
else {
  L.push("| run | arm | term | scored | sentence |");
  L.push("|---|---|---|---|---|");
  for (const r of trapRows) {
    const s = r.sentence.replace(/\|/g, "\\|").slice(0, 200);
    L.push(`| \`${r.g.questionId}/rep${r.g.rep}\` | ${r.g.arm} | \`${r.term}\` | ${r.scoredAs} | ${s} |`);
  }
}
L.push("");
L.push("## Key terms no run produced, in any arm");
L.push("");
L.push("A `mustMention` term that **every** run misses is a constant offset on recall, not a signal:");
L.push("it lowers all three arms identically and cannot move the paired contrast. It is listed here");
L.push("because it is calibration information for the full run, and because a reader who sees recall");
L.push("capped below 100% deserves to know why. **No key is changed on the strength of this** -");
L.push("probe rule 3 forbids adjusting a key after seeing results, and none of these is wrong: the");
L.push("gate independently proves every one of them resolves in the repo at the pinned SHA.");
L.push("");
const missedByAll = [];
for (const qid of qids) {
  const rows = graded.filter((g) => g.questionId === qid);
  if (!rows.length) continue;
  for (const term of rows[0].mustMention.map((m) => m.term)) {
    const missed = rows.filter((r) => !r.mustMention.find((m) => m.term === term)?.hit).length;
    if (missed === rows.length) missedByAll.push({ qid, cls: rows[0].questionClass, term, n: rows.length });
  }
}
if (!missedByAll.length) L.push("None - every key term was produced by at least one run.");
else {
  L.push("| question | class | term | missed by |");
  L.push("|---|---|---|---|");
  for (const m of missedByAll) L.push(`| \`${m.qid}\` | ${m.cls} | \`${m.term}\` | ${m.n}/${m.n} runs |`);
}
L.push("");
L.push("Re-derive: `node eval/agent-probe/grade.mjs --check` (exits 1 on any drift).");
L.push("");

writeFileSync(OUT_MD, L.join("\n"), "utf8");

console.log(`graded ${graded.length} run(s) -> results/graded.jsonl + results/a1.1-grading.md`);
for (const arm of arms) {
  const g = byArm(arm);
  const closed = closedOf(g);
  console.log(`  arm ${arm}: n=${g.length} medianRecall=${pct(med(g.map((x) => x.recall)))} `
    + `violations=${g.reduce((a, x) => a + x.mustNotViolations, 0)} `
    + `D/E/F correct=${closed.filter((x) => x.deterministicCorrect).length}/${closed.length}`);
}
