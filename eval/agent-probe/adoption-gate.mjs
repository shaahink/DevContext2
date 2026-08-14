// The A1.2 adoption gate - DESIGN section 3.1's manipulation check, re-run on its own.
// Pure ASCII on purpose (PowerShell 5.1 reads unmarked UTF-8 as cp1252).
//
//   node eval/agent-probe/adoption-gate.mjs [--dir a1.2-adoption-gate] [--out <path.md>]
//
// This file reports a number and names the branch that number fires. It does NOT decide anything:
// both branches were written into DESIGN.md on 2026-08-14 (A1.1) BEFORE any run, which is the
// whole point of pre-registering a manipulation check.
//
// THE ESTIMATOR IS FIXED, AND IT MATTERS. One dataset gives three different "shares":
// the pilot's 18 arm-B rows are median 0.015, pooled 0.090, mean 0.061. The published headline
// (RESULTS.md section 2) is the MEDIAN of per-run (mcp calls / executed calls) - analyse.mjs's
// per-run share column, median() at analyse.mjs L30. This gate reports the same statistic against
// the same floor, and prints the other two beside it so nobody can pick one after the fact.

import { readFileSync, existsSync, mkdirSync, writeFileSync } from "fs";
import { join, dirname, resolve } from "path";
import { fileURLToPath } from "url";

const HERE = dirname(fileURLToPath(import.meta.url));
const RESULTS = join(HERE, "results");

const argOf = (name, fallback = null) => {
  const i = process.argv.indexOf(`--${name}`);
  if (i === -1) return fallback;
  const v = process.argv[i + 1];
  return v === undefined || v.startsWith("--") ? fallback : v;
};

const FLOOR = 0.2;               // DESIGN section 3.1. Not configurable - the floor is the gate.
const GATE_DIR = argOf("dir", "a1.2-adoption-gate");
const OUT = argOf("out", null);

// analyse.mjs L30, copied rather than imported so this file cannot be broken by an edit there.
function median(xs) {
  const s = xs.filter((x) => x != null && Number.isFinite(x)).sort((a, b) => a - b);
  if (!s.length) return null;
  const m = s.length >> 1;
  return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2;
}
const sum = (xs) => xs.reduce((a, b) => a + b, 0);
const f = (x, d = 3) => (x == null ? "n/a" : Number(x).toFixed(d));

function load(path) {
  if (!existsSync(path)) return [];
  return readFileSync(path, "utf8").split("\n").filter((l) => l.trim()).map((l) => JSON.parse(l));
}

// One row -> the two counts the share is built from. Identical derivation for both datasets, so
// the pilot column and the gate column are the same measurement of two different tool surfaces.
function shareOf(r) {
  const executed = (r.toolCallsExecuted || []).map(String);
  const mcp = executed.filter((t) => t.startsWith("mcp__")).length;
  return { executed: executed.length, mcp, share: executed.length ? mcp / executed.length : null };
}

function describe(rows, label) {
  const per = rows.map(shareOf);
  const shares = per.map((p) => p.share).filter((x) => x != null);
  return {
    label,
    n: rows.length,
    withCalls: shares.length,
    medianShare: median(shares),
    pooledShare: sum(per.map((p) => p.executed)) ? sum(per.map((p) => p.mcp)) / sum(per.map((p) => p.executed)) : null,
    meanShare: shares.length ? sum(shares) / shares.length : null,
    belowFloor: shares.filter((x) => x < FLOOR).length,
    anyMcp: shares.filter((x) => x > 0).length,
    mcpCalls: sum(per.map((p) => p.mcp)),
    executedCalls: sum(per.map((p) => p.executed)),
    costUsd: sum(rows.map((r) => r.costUsd || 0)),
    censored: rows.filter((r) => r.censored).length,
    isolationBreaches: rows.filter((r) => r.isolationOk === false).length,
    zeroCost: rows.filter((r) => !(r.costUsd > 0)).length,
    repoShas: [...new Set(rows.map((r) => r.repoSha))],
    devcontextShas: [...new Set(rows.map((r) => r.devcontextSha))],
    isolation: [...new Set(rows.map((r) => r.isolation))],
    mcpToolsOffered: [...new Set(rows.map((r) => r.mcpToolsOffered))],
    per,
  };
}

const gateRows = load(join(RESULTS, GATE_DIR, "runs.jsonl")).filter((r) => r.arm === "B");
const pilotRows = load(join(RESULTS, "runs.jsonl")).filter((r) => r.arm === "B");
if (!gateRows.length) {
  console.error(`REFUSED: no arm-B rows at results/${GATE_DIR}/runs.jsonl - nothing to report`);
  process.exit(2);
}

const gate = describe(gateRows, `gate (results/${GATE_DIR})`);
const pilot = describe(pilotRows, "pilot (results/runs.jsonl)");
const clears = gate.medianShare != null && gate.medianShare >= FLOOR;

const L = [];
const P = (s = "") => L.push(s);

P(`# A1.2 - the adoption gate: does arm B reach for the tools?`);
P();
P(`DESIGN.md section 3.1, amended 2026-08-14 (A1.1) **before this run**: arm B alone, one repo`);
P(`(eShop), 18 runs, prompt and system text unchanged, against the tool surface as revised by the`);
P(`trust pack. The floor is \`mcp_call_share\` **>= ${FLOOR}**, and it is the same floor in both branches.`);
P();
P(`## The number`);
P();
P(`| statistic | gate (n=${gate.n}) | pilot (n=${pilot.n}) | floor | clears? |`);
P(`|---|---|---|---|---|`);
P(`| **median per-run share** (the decision statistic) | **${f(gate.medianShare)}** | ${f(pilot.medianShare)} | ${FLOOR} | **${clears ? "YES" : "NO"}** |`);
P(`| pooled MCP calls / executed calls | ${f(gate.pooledShare)} | ${f(pilot.pooledShare)} | - | reported, not decisive |`);
P(`| mean per-run share | ${f(gate.meanShare)} | ${f(pilot.meanShare)} | - | reported, not decisive |`);
P(`| runs below the floor | ${gate.belowFloor}/${gate.withCalls} | ${pilot.belowFloor}/${pilot.withCalls} | - | - |`);
P(`| runs that called the MCP at all | ${gate.anyMcp}/${gate.withCalls} | ${pilot.anyMcp}/${pilot.withCalls} | - | - |`);
P(`| MCP calls / all executed tool calls | ${gate.mcpCalls}/${gate.executedCalls} | ${pilot.mcpCalls}/${pilot.executedCalls} | - | - |`);
P();
P(`## Per run`);
P();
P(`| # | question | rep | executed calls | mcp calls | share | cost | censored |`);
P(`|---|---|---|---|---|---|---|---|`);
gateRows.forEach((r, i) => {
  const p = gate.per[i];
  P(`| ${i + 1} | ${r.questionId} | ${r.rep} | ${p.executed} | ${p.mcp} | ${p.share == null ? "n/a" : f(p.share, 2)} | $${f(r.costUsd, 2)} | ${r.censored ? "yes" : "no"} |`);
});
P();
P(`## Pre-flight (DESIGN section 8; assertion 3 is arm-M only and does not apply)`);
P();
P(`- repo pin: ${gate.repoShas.map((s) => String(s).slice(0, 8)).join(", ")} (assertion 5)`);
P(`- DevContext build under test: ${gate.devcontextShas.map((s) => String(s).slice(0, 8)).join(", ")} (assertion 5)`);
P(`- \`total_cost_usd\` non-zero: ${gate.n - gate.zeroCost}/${gate.n} rows (assertion 4)`);
P(`- arm isolation breaches: ${gate.isolationBreaches} (the harness stops the batch on the first one)`);
P(`- isolation mode: ${gate.isolation.join(", ")} (pilot: ${pilot.isolation.join(", ")})`);
P(`- MCP tools offered to the agent: ${gate.mcpToolsOffered.join(", ")} (pilot: ${pilot.mcpToolsOffered.join(", ")})`);
P(`- censored runs: ${gate.censored}/${gate.n}`);
P(`- batch cost: $${f(gate.costUsd, 2)} (pilot arm B: $${f(pilot.costUsd, 2)})`);
P();
P(`## The branch this fires`);
P();
if (clears) {
  P(`**>= ${FLOOR} - PROCEED.** The manipulation took: with a curated, described surface an agent`);
  P(`does reach for the tools unprompted. The full study runs as specified and arm B stays the`);
  P(`primary treatment arm. The B-vs-G contrast is a test of the MCP.`);
} else {
  P(`**< ${FLOOR} - the honest fallback.** This is a **product finding**, not a failed measurement:`);
  P(`with a curated, described surface an agent still does not reach for the tools unprompted.`);
  P(`DESIGN section 3.1 fixed all three consequences in advance, and all three apply:`);
  P();
  P(`- (a) the primary contrast becomes **M-vs-G**, and the study reports *sufficiency* ("can the`);
  P(`  graph alone answer these?") rather than *augmentation*;`);
  P(`- (b) **BI** is promoted from secondary to the arm that carries the product claim, because`);
  P(`  instructed use is then the only configuration in which the tools are used at all;`);
  P(`- (c) arm B is retained and reported as the measurement of unprompted adoption, which is`);
  P(`  the finding.`);
  P();
  P(`The floor is not moved and nothing below it is re-described as a pass.`);
}
P();
P(`## Provenance`);
P();
P(`- rows: \`eval/agent-probe/results/${GATE_DIR}/runs.jsonl\` (${gate.n}), transcripts under \`raw/\`.`);
P(`- pilot comparison: \`eval/agent-probe/results/runs.jsonl\` arm B (${pilot.n}), the 22-tool undescribed surface.`);
P(`- regenerate: \`node eval/agent-probe/adoption-gate.mjs --dir ${GATE_DIR}\`.`);
P(`- estimator: median of per-run (mcp calls / executed tool calls), \`analyse.mjs\` L30/L401 -`);
P(`  the same statistic that produced the pilot's published ${f(pilot.medianShare)}.`);

const text = L.join("\n") + "\n";
if (OUT) {
  mkdirSync(dirname(resolve(OUT)), { recursive: true });
  writeFileSync(resolve(OUT), text, "utf8");
  console.log(`wrote ${resolve(OUT)}`);
}
console.log(text);
process.exit(0);
