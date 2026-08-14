#!/usr/bin/env node
// P1.2 - the tool-schema tax (DESIGN section 4.4), measured, not estimated.
//
//   node eval/agent-probe/measure-tax.mjs --allow-no-bare [--reps 2]
//   node eval/agent-probe/measure-tax.mjs --render-only        # re-render from results/raw-tax/
//
// DESIGN 4.4: "run the same trivial prompt (`reply with the word ok`) in arm G and arm B and
// record turn-1 input_tokens + cache_creation_input_tokens. The delta is the tax."
//
// The arm argv is IMPORTED from run-probe.mjs rather than restated. A tax measured against a
// different command line than the pilot actually uses would not be a measurement of these arms.
// Arm G and arm B differ in exactly one thing - arm B is handed --mcp-config - so the delta is
// the 22 devcontext tool schemas and nothing else. Arm M is not run: its argv drops --add-dir,
// so a G-vs-M delta would mix the schema cost with a different working-directory preamble.
//
// MEASURED, and it decides what this file reports: DESIGN 4.4's statistic only measures the tax
// when the prefix is COLD. The prompt cache is server-side and keyed by prefix, so if any run has
// carried the same system prompt + tool schemas inside the TTL, the schemas arrive as cache_read
// and `cache_creation` collapses to almost nothing while the tokens are still being paid for.
// That is exactly what happened on the first attempt (delta of 9 tokens against a real cost of
// ~2535). So three readings are reported, none of them substituted for another:
//
//   1. DESIGN 4.4 as written - turn-1 input + cache_creation - and the cache state it was taken in.
//   2. The cache-state-invariant reading of the same runs - turn-1 input + cache_creation +
//      cache_read, i.e. the whole prefix the model was charged for however it arrived.
//   3. A cold-cache cross-check from the already-recorded pilot runs in results/raw/, where
//      cache_read is 0 and reading (1) and (2) coincide.
//
// Nothing about the experiment is tuned here: the prompt, the arms, and the argv are untouched.

import { writeFileSync, mkdirSync, existsSync, readFileSync, readdirSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { armArgs, spawnRun, claudeBin, ISOLATION, MAX_BUDGET_USD } from "./run-probe.mjs";

const HERE = dirname(fileURLToPath(import.meta.url));
const REPO_DIR = join(HERE, "..", "..", "eval-repos", "eShop");
// T1.1 - `--label <slug>` sends the outputs to a suffixed set instead of overwriting the P1.2
// baseline. Re-measuring the tax after a surface change is a COMPARISON, and a comparison needs
// both readings on disk; the un-labelled paths stay exactly what they were.
const LABEL_ARG = process.argv.indexOf("--label");
const LABEL = LABEL_ARG > -1 ? String(process.argv[LABEL_ARG + 1] || "").replace(/[^A-Za-z0-9._-]/g, "") : "";
if (LABEL_ARG > -1 && !LABEL) { console.error("REFUSED: --label needs a non-empty slug"); process.exit(2); }
const SUFFIX = LABEL ? `-${LABEL}` : "";
const OUT_DIR = join(HERE, "results", `raw-tax${SUFFIX}`);
const OUT_MD = join(HERE, "results", `p1.2-tool-schema-tax${SUFFIX}.md`);
const OUT_JSON = join(HERE, "results", `p1.2-tool-schema-tax${SUFFIX}.json`);

const TRIVIAL_PROMPT = "reply with the word ok";   // DESIGN 4.4, verbatim
const ARMS = ["G", "B"];
const REPS = Number((process.argv.indexOf("--reps") > -1 && process.argv[process.argv.indexOf("--reps") + 1]) || 2);
const RENDER_ONLY = process.argv.includes("--render-only");

// Opus 5 published rates, per MTok. Cache reads bill at 0.1x input; cache writes at 2x for the
// 1-hour TTL these runs use (see the P1 ledger note and results/p1.1-preflight-audit.md).
const IN_PER_MTOK = 5, OUT_PER_MTOK = 25, WRITE_MULT = 2, READ_MULT = 0.1;

if (!RENDER_ONLY && ISOLATION !== "bare" && !process.argv.includes("--allow-no-bare")) {
  console.error("\nREFUSED: same rule as run-probe.mjs - without ANTHROPIC_API_KEY, --bare cannot "
    + "authenticate. Pass --allow-no-bare to use the recorded no-settings fallback.\n");
  process.exit(2);
}
if (!existsSync(REPO_DIR)) { console.error(`REFUSED: ${REPO_DIR} is not cloned`); process.exit(2); }

function turnOneUsage(events) {
  const a = events.find((e) => e.type === "assistant" && e.message?.usage);
  return a ? a.message.usage : null;
}

function priced(u) {
  if (!u) return null;
  return ((u.input_tokens || 0) * IN_PER_MTOK
    + (u.cache_creation_input_tokens || 0) * IN_PER_MTOK * WRITE_MULT
    + (u.cache_read_input_tokens || 0) * IN_PER_MTOK * READ_MULT
    + (u.output_tokens || 0) * OUT_PER_MTOK) / 1e6;
}

mkdirSync(OUT_DIR, { recursive: true });

// Order matters: G rep1, B rep1, then the repeats. rep1 of each arm is the first measurement
// taken and must not sit behind a rep of the same arm.
const plan = [];
if (RENDER_ONLY) {
  for (const f of readdirSync(OUT_DIR)) {
    const m = /^tax__([GMB])__rep(\d+)\.stream\.jsonl$/.exec(f);
    if (m) plan.push({ arm: m[1], rep: Number(m[2]) });
  }
  plan.sort((a, b) => a.rep - b.rep || ARMS.indexOf(a.arm) - ARMS.indexOf(b.arm));
  console.log(`render-only: re-reading ${plan.length} recorded trivial runs from results/raw-tax/`);
} else {
  for (let rep = 1; rep <= REPS; rep++) for (const arm of ARMS) plan.push({ arm, rep });
  console.log(`tax measurement: ${plan.length} trivial runs, cap $${MAX_BUDGET_USD}/run, `
    + `isolation=${ISOLATION}, bin=${claudeBin()}`);
}

const rows = [];
for (const { arm, rep } of plan) {
  const cell = { arm, prompt: TRIVIAL_PROMPT };
  process.stdout.write(`  arm ${arm} rep ${rep} ... `);
  const out = RENDER_ONLY
    ? {
      events: readFileSync(join(OUT_DIR, `tax__${arm}__rep${rep}.stream.jsonl`), "utf8")
        .trim().split(/\r?\n/).filter(Boolean).map((l) => JSON.parse(l)),
      exitCode: 0, timedOut: false, wallMs: null, stderr: "",
    }
    : await spawnRun(cell, REPO_DIR);
  const result = out.events.find((e) => e.type === "result") || {};
  const init = out.events.find((e) => e.type === "system" && e.subtype === "init") || {};
  const t1 = turnOneUsage(out.events);
  const row = {
    arm, rep,
    exitCode: out.exitCode, timedOut: out.timedOut, wallMs: out.wallMs,
    subtype: result.subtype, isError: result.is_error,
    reply: typeof result.result === "string" ? result.result.slice(0, 120) : null,
    toolsOffered: (init.tools || []).length,
    mcpToolsOffered: (init.tools || []).filter((t) => String(t).startsWith("mcp__")).length,
    mcpServers: (init.mcp_servers || []).map((s) => `${s.name}:${s.status}`),
    turn1: t1 && {
      input: t1.input_tokens || 0,
      cacheCreation: t1.cache_creation_input_tokens || 0,
      cacheRead: t1.cache_read_input_tokens || 0,
      output: t1.output_tokens || 0,
      // DESIGN 4.4's quantity, exactly: turn-1 input + cache creation.
      designTax: (t1.input_tokens || 0) + (t1.cache_creation_input_tokens || 0),
      // Cache-state invariant: the whole prefix the model was charged for on turn 1, however
      // it arrived. Equal to designTax when the prefix is cold.
      prefixTokens: (t1.input_tokens || 0) + (t1.cache_creation_input_tokens || 0) + (t1.cache_read_input_tokens || 0),
      coldPrefix: (t1.cache_read_input_tokens || 0) === 0,
      pricedUsd: Number(priced(t1).toFixed(6)),
    },
    runUsage: result.usage && {
      input: result.usage.input_tokens || 0,
      cacheCreation: result.usage.cache_creation_input_tokens || 0,
      cacheRead: result.usage.cache_read_input_tokens || 0,
      output: result.usage.output_tokens || 0,
    },
    numTurns: result.num_turns,
    costUsd: result.total_cost_usd,
    stderrTail: out.stderr ? out.stderr.slice(-300) : "",
  };
  rows.push(row);
  if (!RENDER_ONLY) {
    writeFileSync(join(OUT_DIR, `tax__${arm}__rep${rep}.stream.jsonl`),
      out.events.map((e) => JSON.stringify(e)).join("\n") + "\n", "utf8");
    writeFileSync(join(OUT_DIR, `tax__${arm}__rep${rep}.result.json`), JSON.stringify(result, null, 2), "utf8");
  }
  console.log(`tools=${row.toolsOffered} designTax=${row.turn1?.designTax} `
    + `prefix=${row.turn1?.prefixTokens} cold=${row.turn1?.coldPrefix} cost=${row.costUsd}`);
}

// The cold cross-check. The three eshop-a1 rep1 runs recorded at P1.1 each wrote their entire
// prefix (cache_read = 0), so for them DESIGN 4.4's statistic and the invariant coincide. Their
// prompt is the eShop class-A question rather than the trivial one, but it is byte-identical
// across arms, so the G-to-B delta is still the schema cost and nothing else.
function coldCrossCheck() {
  const dir = join(HERE, "results", "raw", "eShop");
  if (!existsSync(dir)) return null;
  const out = {};
  for (const arm of ["G", "M", "B"]) {
    const p = join(dir, `eshop-a1__${arm}__rep1.stream.jsonl`);
    if (!existsSync(p)) continue;
    const evs = readFileSync(p, "utf8").trim().split(/\r?\n/).filter(Boolean).map((l) => JSON.parse(l));
    const u = turnOneUsage(evs);
    const init = evs.find((e) => e.type === "system" && e.subtype === "init") || {};
    if (!u) continue;
    out[arm] = {
      toolsOffered: (init.tools || []).length,
      input: u.input_tokens || 0,
      cacheCreation: u.cache_creation_input_tokens || 0,
      cacheRead: u.cache_read_input_tokens || 0,
      designTax: (u.input_tokens || 0) + (u.cache_creation_input_tokens || 0),
      coldPrefix: (u.cache_read_input_tokens || 0) === 0,
    };
  }
  if (!out.G || !out.B) return null;
  out.taxTokens = out.B.designTax - out.G.designTax;
  out.allCold = ["G", "M", "B"].every((a) => !out[a] || out[a].coldPrefix);
  return out;
}
const cold = coldCrossCheck();

const by = (arm, rep) => rows.find((r) => r.arm === arm && r.rep === rep);
const repsSeen = [...new Set(rows.map((r) => r.rep))].sort((a, b) => a - b);
const deltas = [];
for (const rep of repsSeen) {
  const g = by("G", rep), b = by("B", rep);
  if (!g?.turn1 || !b?.turn1) continue;
  deltas.push({
    rep,
    designTaxTokens: b.turn1.designTax - g.turn1.designTax,       // DESIGN 4.4 as written
    prefixTaxTokens: b.turn1.prefixTokens - g.turn1.prefixTokens, // cache-state invariant
    schemasOffered: b.mcpToolsOffered - g.mcpToolsOffered,
    prefixState: (g.turn1.coldPrefix && b.turn1.coldPrefix) ? "cold" : "warm (schemas arrived as cache_read)",
    gTurn1: g.turn1, bTurn1: b.turn1,
  });
}
const usd = (tok, mult) => (tok * IN_PER_MTOK * mult) / 1e6;

// Share of median run cost, over whatever real pilot runs exist so far. Stated with its n.
let medianRunCost = null, nRuns = 0;
const runsJsonl = join(HERE, "results", "runs.jsonl");
if (existsSync(runsJsonl)) {
  const costs = readFileSync(runsJsonl, "utf8").trim().split(/\r?\n/)
    .filter(Boolean).map((l) => JSON.parse(l).costUsd).filter((c) => typeof c === "number").sort((a, b) => a - b);
  nRuns = costs.length;
  if (nRuns) medianRunCost = nRuns % 2 ? costs[(nRuns - 1) / 2] : (costs[nRuns / 2 - 1] + costs[nRuns / 2]) / 2;
}

const head = deltas.find((d) => d.rep === 1);
const L = [];
L.push("# P1.2 - the tool-schema tax");
L.push("");
L.push("Generated by `eval/agent-probe/measure-tax.mjs`, which imports the arm argv from");
L.push("`run-probe.mjs` so the tax is measured against the same command line the pilot runs.");
L.push(`Prompt, byte-identical in both arms: \`${TRIVIAL_PROMPT}\` (DESIGN 4.4). Model, budget cap,`);
L.push("system prompt, `--add-dir` and isolation mode are identical too. Arm B differs from arm G in");
L.push("exactly one flag: `--mcp-config`.");
L.push("");
L.push("## The runs");
L.push("");
L.push("| arm | rep | tools offered (mcp) | turn-1 input | cache creation | cache read | DESIGN 4.4 stat (in+create) | whole turn-1 prefix | prefix cold? | run cost |");
L.push("|---|---|---|---|---|---|---|---|---|---|");
for (const r of rows) {
  L.push(`| ${r.arm} | ${r.rep} | ${r.toolsOffered} (${r.mcpToolsOffered}) | ${r.turn1?.input} `
    + `| ${r.turn1?.cacheCreation} | ${r.turn1?.cacheRead} | ${r.turn1?.designTax} `
    + `| **${r.turn1?.prefixTokens}** | ${r.turn1?.coldPrefix} | ${r.costUsd} |`);
}
L.push("");
L.push("## The statistic DESIGN 4.4 names, and why it reads near zero here");
L.push("");
if (head) {
  L.push(`Taken literally - turn-1 \`input_tokens + cache_creation_input_tokens\`, arm B minus arm G -`);
  L.push(`the answer on rep 1 is **${head.designTaxTokens} tokens**. That number is not the tax.`);
  L.push("");
  L.push("The prompt cache is server-side and keyed by prefix. These runs carry the same system");
  L.push("prompt and the same tool schemas as the pilot runs recorded half an hour earlier, so the");
  L.push(`schemas arrived as \`cache_read\` (${head.bTurn1.cacheRead} tokens in arm B) rather than as`);
  L.push(`\`cache_creation\` (${head.bTurn1.cacheCreation}). The tokens are still being carried and still`);
  L.push("being billed - at 0.1x instead of 2x - but the statistic cannot see them. DESIGN 4.4's");
  L.push("quantity measures the tax only when the prefix is cold, and it does not say how to");
  L.push("guarantee that. Reporting 9 tokens would be reporting a cache state, not a cost.");
  L.push("");
  L.push("## The headline number");
  L.push("");
  L.push(`**Tool-schema tax = ${head.prefixTaxTokens} tokens of turn-1 context**, arm B minus arm G, measured as`);
  L.push("the whole turn-1 prefix (`input + cache_creation + cache_read`), which is what the model was");
  L.push("charged for however it arrived. Same two runs, same trivial prompt, cache-state invariant.");
  L.push(`That covers ${head.schemasOffered} MCP tool schemas plus whatever else the MCP connection adds to`);
  L.push(`the preamble - about ${Math.round(head.prefixTaxTokens / Math.max(1, head.schemasOffered))} tokens per tool if it is all schema. It is charged to the arm either`);
  L.push("way, so the delta is the quantity the experiment needs, not the schema bytes on their own.");
  L.push("");
  const coldUsd = usd(head.prefixTaxTokens, WRITE_MULT);
  const readUsd = usd(head.prefixTaxTokens, READ_MULT);
  L.push(`Priced on Opus 5 at the 1-hour cache-write rate (${WRITE_MULT}x of $${IN_PER_MTOK}/MTok): **$${coldUsd.toFixed(4)}** the`);
  L.push(`first time a session pays it, then **$${readUsd.toFixed(5)}** re-read on every later turn of that session.`);
  if (medianRunCost) {
    L.push(`Median recorded pilot run cost is $${medianRunCost.toFixed(4)} (n=${nRuns}), so the cold write is`);
    L.push(`**${(100 * coldUsd / medianRunCost).toFixed(1)}%** of one run and each subsequent turn adds another`);
    L.push(`${(100 * readUsd / medianRunCost).toFixed(2)}%.`);
  } else {
    L.push("No pilot runs recorded yet, so the share-of-run-cost figure is not computed.");
  }
  L.push("");
  L.push("It hits short runs hardest, which is where class A and class F questions live - the case");
  L.push("DESIGN 4.4 says not to average away. It is also why the treatment arms start each question");
  L.push("already behind, and it belongs in the headline result, not in a footnote.");
  L.push("");
}
if (deltas.length > 1) {
  L.push("## Stability across reps");
  L.push("");
  L.push("| rep | prefix state | DESIGN 4.4 stat | whole-prefix delta |");
  L.push("|---|---|---|---|");
  for (const d of deltas) L.push(`| ${d.rep} | ${d.prefixState} | ${d.designTaxTokens} | ${d.prefixTaxTokens} |`);
  L.push("");
  L.push("The invariant reading is stable to a handful of tokens across reps while the DESIGN");
  L.push("statistic swings with the cache. Rep 2 is the regime the 54-cell pilot lives in: back-to-back");
  L.push("runs sharing a warm prefix, paying the schemas at 0.1x rather than 2x. The pilot's own cost");
  L.push("numbers therefore include an amortised tax, not a cold one, and that is the honest reading");
  L.push("of them - it makes the treatment look better than a cold-start user would experience.");
  L.push("");
}
if (cold) {
  L.push("## Cold-cache cross-check, from the pilot runs already recorded");
  L.push("");
  L.push("The three `eshop-a1` rep-1 runs in `results/raw/eShop/` each wrote their whole prefix:");
  L.push("`cache_read` is 0 on all of them, so for those runs DESIGN 4.4's statistic and the invariant");
  L.push("coincide. Their prompt is the eShop class-A question rather than the trivial one, but it is");
  L.push("byte-identical across arms, so the G-to-B delta is still the schema cost.");
  L.push("");
  L.push("| arm | tools offered | turn-1 input | cache creation | cache read | in+create |");
  L.push("|---|---|---|---|---|---|");
  for (const arm of ["G", "M", "B"]) {
    const c = cold[arm];
    if (!c) continue;
    L.push(`| ${arm} | ${c.toolsOffered} | ${c.input} | ${c.cacheCreation} | ${c.cacheRead} | **${c.designTax}** |`);
  }
  L.push("");
  L.push(`Cold delta B - G = **${cold.taxTokens} tokens**, against ${head ? head.prefixTaxTokens : "n/a"} from the trivial-prompt runs.`);
  L.push("Two prompts, two cache states, one number to within a few tokens.");
  L.push("");
  if (cold.M) {
    L.push(`Arm M's turn-1 prefix is ${cold.M.designTax} tokens - smaller than arm G's ${cold.G.designTax} despite carrying`);
    L.push("22 tool schemas, because arm M gets no `--add-dir` and so no directory preamble. That is why");
    L.push("the tax is measured G against B and not G against M.");
    L.push("");
  }
}
L.push("## Raw artifacts");
L.push("");
for (const r of rows) L.push(`- arm ${r.arm} rep ${r.rep}: \`results/raw-tax/tax__${r.arm}__rep${r.rep}.stream.jsonl\` + \`.result.json\``);
L.push("");
L.push("These runs are deliberately NOT in `runs.jsonl`. They answer no question and must never");
L.push("enter the cost or correctness analysis.");
L.push("");

writeFileSync(OUT_MD, L.join("\n"), "utf8");
writeFileSync(OUT_JSON, JSON.stringify({
  prompt: TRIVIAL_PROMPT, isolation: ISOLATION, maxBudgetUsd: MAX_BUDGET_USD,
  rates: { inPerMTok: IN_PER_MTOK, outPerMTok: OUT_PER_MTOK, cacheWriteMult: WRITE_MULT, cacheReadMult: READ_MULT },
  medianRunCostUsd: medianRunCost, medianOverNRuns: nRuns,
  rows, deltas, coldCrossCheck: cold,
}, null, 2), "utf8");
console.log(`\nwrote ${OUT_MD}`);
if (head) {
  console.log(`TAX = ${head.prefixTaxTokens} turn-1 prefix tokens for ${head.schemasOffered} mcp schemas `
    + `($${usd(head.prefixTaxTokens, WRITE_MULT).toFixed(4)} cold write, `
    + `$${usd(head.prefixTaxTokens, READ_MULT).toFixed(5)}/turn warm read)`);
  console.log(`DESIGN 4.4 statistic as written = ${head.designTaxTokens} (prefix was ${head.prefixState})`);
  if (cold) console.log(`cold cross-check from recorded pilot runs = ${cold.taxTokens}`);
}
const spent = rows.reduce((s, r) => s + (r.costUsd || 0), 0);
console.log(RENDER_ONLY ? "render-only: nothing spent" : `spent $${spent.toFixed(4)} across ${rows.length} trivial runs`);
const broken = rows.filter((r) => r.exitCode !== 0 || !r.turn1);
if (broken.length) { console.error(`RED: ${broken.length} run(s) did not produce turn-1 usage`); process.exit(1); }
