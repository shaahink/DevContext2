#!/usr/bin/env node
// P1.1 - independent pre-flight audit of DESIGN section 8 assertions 1-4.
//
// This file deliberately shares NO code with run-probe.mjs and never reads runs.jsonl. The
// harness's own bookkeeping (offeredOutsideArm / calledOutsideArm / isolationOk) is the thing
// under test at P1, so the audit re-derives every fact straight from the recorded artifacts:
// results/raw/<repo>/<cell>.stream.jsonl and results/raw/<repo>/<cell>.result.json. The arm
// predicate below is written from DESIGN sections 3.1 and 8, not imported from the harness.
//
//   node eval/agent-probe/audit-preflight.mjs             # audit + rewrite the markdown artifact
//   node eval/agent-probe/audit-preflight.mjs --check     # audit only, no write; exit 1 on failure
//   node eval/agent-probe/audit-preflight.mjs --check --raw results/void/raw
//                                                        # negative control: must come back RED
//
// Exit code is 1 if any assertion fails, so this is safe to wire into a gate.

import { readFileSync, writeFileSync, readdirSync, existsSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const HERE = dirname(fileURLToPath(import.meta.url));
const rawArg = process.argv.indexOf("--raw");
const RAW = rawArg > -1 ? join(HERE, process.argv[rawArg + 1]) : join(HERE, "results", "raw");
const OUT_MD = join(HERE, "results", "p1.1-preflight-audit.md");
const OUT_JSON = join(HERE, "results", "p1.1-preflight-audit.json");
const CHECK_ONLY = process.argv.includes("--check");

// ---- the arm predicate, restated from DESIGN 3.1 ---------------------------
// G: file tools + a shell, no MCP server configured at all.
// M: the devcontext MCP and nothing else - no filesystem, no shell.
// B: both.
const FILE_TOOLS = ["Read", "Grep", "Glob"];
const ARM_ALLOWS = {
  G: { file: true, shell: true, mcp: false },
  M: { file: false, shell: false, mcp: true },
  B: { file: true, shell: true, mcp: true },
};

function classify(name) {
  const n = String(name);
  if (n.startsWith("mcp__devcontext__") || n === "mcp__devcontext") return "mcp-devcontext";
  if (n.startsWith("mcp__")) return "mcp-foreign";
  if (FILE_TOOLS.includes(n)) return "file";
  if (n === "Bash" || n === "BashOutput" || n === "KillShell" || n === "PowerShell") return "shell";
  return "other";
}

// A tool is inside the arm only if its class is one the arm allows. "other" is never in-arm:
// Task/Agent/ToolSearch/Skill and friends can each re-acquire a capability the arm denies.
function permitted(arm, name) {
  const a = ARM_ALLOWS[arm];
  switch (classify(name)) {
    case "mcp-devcontext": return a.mcp;
    case "mcp-foreign": return false;
    case "file": return a.file;
    case "shell": return a.shell;
    default: return false;
  }
}

// ---- published rates, for the cost cross-check (DESIGN 4.1) ----------------
// Per MTok. Cache READ is 0.1x input. Cache WRITE depends on TTL: 1.25x for the 5-minute
// cache, 2x for the 1-hour cache - see the P1 ledger note; these runs use the 1-hour cache.
const RATES = {
  "claude-opus-5": { in: 5, out: 25 },
  "claude-sonnet-5": { in: 3, out: 15 },
  "claude-haiku-4-5": { in: 1, out: 5 },
};

function reconstructCost(modelUsage, usage) {
  const ttl1h = (usage?.cache_creation?.ephemeral_1h_input_tokens || 0) > 0;
  const writeMult = ttl1h ? 2 : 1.25;
  let total = 0;
  let known = true;
  for (const [model, u] of Object.entries(modelUsage || {})) {
    const r = RATES[u.canonicalModel] || RATES[model];
    if (!r) { known = false; continue; }
    total += (u.inputTokens * r.in
      + u.cacheCreationInputTokens * r.in * writeMult
      + u.cacheReadInputTokens * r.in * 0.1
      + u.outputTokens * r.out) / 1e6;
  }
  return { total, writeMult, ttl: ttl1h ? "1h" : "5m", allModelsPriced: known };
}

// ---- parse one recorded run ------------------------------------------------
function parseStream(path) {
  const events = readFileSync(path, "utf8").trim().split(/\r?\n/)
    .filter((l) => l.trim().length > 0)
    .map((l, i) => {
      try { return JSON.parse(l); } catch { throw new Error(`${path}: line ${i + 1} is not JSON`); }
    });

  const init = events.find((e) => e.type === "system" && e.subtype === "init") || null;
  const uses = [];       // every tool_use block the model emitted, executed or not
  const results = new Map(); // tool_use_id -> { isError, text }

  for (const e of events) {
    if (e.type === "assistant") {
      for (const c of e.message?.content || []) {
        if (c.type === "tool_use") uses.push({ id: c.id, name: c.name, input: c.input });
      }
    }
    if (e.type === "user") {
      for (const c of e.message?.content || []) {
        if (c.type !== "tool_result") continue;
        const text = typeof c.content === "string"
          ? c.content
          : (c.content || []).map((b) => (typeof b === "string" ? b : b.text || "")).join("\n");
        results.set(c.tool_use_id, { isError: c.is_error === true, text });
      }
    }
  }
  return { events, init, uses, results };
}

// analyze's tool_result is a JSON document rendered as text; pull the cached flag out of it.
function analyzeCachedFlags(uses, results) {
  const flags = [];
  for (const u of uses) {
    if (!/^mcp__devcontext__analyze$/.test(u.name)) continue;
    const r = results.get(u.id);
    if (!r) { flags.push({ path: u.input?.path, cached: null, why: "no tool_result recorded" }); continue; }
    let cached = null;
    try { cached = JSON.parse(r.text).cached === true; } catch {
      const m = /"cached"\s*:\s*(true|false)/.exec(r.text);
      if (m) cached = m[1] === "true";
    }
    flags.push({ path: u.input?.path, cached, isError: r.isError });
  }
  return flags;
}

function auditRun(repo, cell, streamPath, resultPath) {
  const arm = cell.split("__")[1];
  if (!ARM_ALLOWS[arm]) throw new Error(`${cell}: cannot read an arm out of the file name`);
  const { init, uses, results } = parseStream(streamPath);
  const result = JSON.parse(readFileSync(resultPath, "utf8"));

  const offered = (init?.tools || []).slice().sort();
  const mcpServers = (init?.mcp_servers || []).map((s) => `${s.name}:${s.status}`);
  const offeredOutside = offered.filter((t) => !permitted(arm, t));
  const attempted = uses.map((u) => u.name);
  const attemptedOutside = [...new Set(attempted.filter((t) => !permitted(arm, t)))].sort();
  const executed = uses.filter((u) => results.get(u.id) && !results.get(u.id).isError).map((u) => u.name);

  const mcpAttempts = attempted.filter((t) => classify(t).startsWith("mcp"));
  const fileAttempts = attempted.filter((t) => classify(t) === "file");
  const cachedFlags = analyzeCachedFlags(uses, results);
  const recon = reconstructCost(result.modelUsage, result.usage);

  // DESIGN section 8 pre-flight assertions 1-4, one entry each.
  const assertions = [];
  if (ARM_ALLOWS[arm].mcp) {
    const ok = cachedFlags.length > 0 && cachedFlags.every((f) => f.cached === true);
    assertions.push({
      id: "A1-analyze-cached", ok,
      detail: cachedFlags.length === 0
        ? "arm has the MCP but the run never called analyze - warmth unproven from this transcript"
        : cachedFlags.map((f) => `analyze(${f.path}) cached=${f.cached}`).join("; "),
    });
  } else {
    assertions.push({
      id: "A1-analyze-cached", ok: true, na: true,
      detail: `no MCP server configured for arm ${arm} (init.mcp_servers=[]), so no analysis can run `
        + "inside this arm and it cannot pay a cold cost; the pre-batch warm gate in run-probe.mjs "
        + "covers the batch (DESIGN 4.5)",
    });
  }
  assertions.push({
    id: "A2-armG-zero-mcp",
    ok: arm !== "G" || (mcpAttempts.length === 0 && mcpServers.length === 0 && !offered.some((t) => classify(t).startsWith("mcp"))),
    na: arm !== "G",
    detail: arm !== "G" ? "applies to arm G only"
      : `mcp tool_use blocks=${mcpAttempts.length}, mcp servers at init=[${mcpServers.join(",")}], mcp tools offered=${offered.filter((t) => classify(t).startsWith("mcp")).length}`,
  });
  assertions.push({
    id: "A3-armM-zero-file",
    ok: arm !== "M" || (fileAttempts.length === 0 && !offered.some((t) => classify(t) === "file")),
    na: arm !== "M",
    detail: arm !== "M" ? "applies to arm M only"
      : `Read/Grep/Glob tool_use blocks=${fileAttempts.length}, offered at init=[${offered.filter((t) => classify(t) === "file").join(",")}]`,
  });
  assertions.push({
    id: "A4-cost-nonzero",
    ok: typeof result.total_cost_usd === "number" && result.total_cost_usd > 0,
    detail: `total_cost_usd=${result.total_cost_usd}`,
  });
  // Not one of the four, but the experiment is void without it, and it is free to check here.
  assertions.push({
    id: "X-nothing-outside-arm",
    ok: offeredOutside.length === 0 && attemptedOutside.length === 0,
    detail: `offered outside arm=[${offeredOutside.join(",")}], attempted outside arm=[${attemptedOutside.join(",")}]`,
  });

  return {
    repo, cell, arm,
    model: init?.model, cliVersion: init?.claude_code_version, permissionMode: init?.permissionMode,
    apiKeySource: init?.apiKeySource,
    memoryPaths: init?.memory_paths || [],
    toolsOfferedCount: offered.length,
    mcpToolsOfferedCount: offered.filter((t) => classify(t).startsWith("mcp")).length,
    mcpServers,
    offeredOutsideArm: offeredOutside,
    toolUseBlocks: uses.length,
    attemptedDistinct: [...new Set(attempted)].sort(),
    attemptedOutsideArm: attemptedOutside,
    executedCount: executed.length,
    mcpAttempts: mcpAttempts.length,
    fileAttempts: fileAttempts.length,
    analyzeCached: cachedFlags,
    costUsd: result.total_cost_usd,
    costReconstructed: Number(recon.total.toFixed(6)),
    costReconDeltaPct: result.total_cost_usd > 0
      ? Number((100 * (recon.total - result.total_cost_usd) / result.total_cost_usd).toFixed(3)) : null,
    cacheWriteTtl: recon.ttl,
    cacheWriteMultiplier: recon.writeMult,
    numTurns: result.num_turns,
    durationMs: result.duration_ms,
    assertions,
    ok: assertions.every((a) => a.ok),
    artifacts: [streamPath, resultPath].map((p) => p.replace(/\\/g, "/").split("/eval/agent-probe/")[1]),
  };
}

// ---- walk the recorded runs ------------------------------------------------
if (!existsSync(RAW)) {
  console.error(`no recorded runs under ${RAW}`);
  process.exit(1);
}
const audits = [];
// results/raw is one directory per repo. The void archive is flat, so accept either shape.
const repoDirs = readdirSync(RAW, { withFileTypes: true }).filter((d) => d.isDirectory()).map((d) => d.name);
const scanTargets = repoDirs.length ? repoDirs.map((r) => [r, join(RAW, r)]) : [["(flat)", RAW]];
for (const [repo, dir] of scanTargets) {
  for (const f of readdirSync(dir)) {
    if (!f.endsWith(".stream.jsonl")) continue;
    const cell = f.replace(/\.stream\.jsonl$/, "");
    const resultPath = join(dir, `${cell}.result.json`);
    if (!existsSync(resultPath)) {
      audits.push({ repo, cell, arm: cell.split("__")[1], ok: false, assertions: [
        { id: "A0-artifacts", ok: false, detail: "stream transcript has no matching result.json" }] });
      continue;
    }
    audits.push(auditRun(repo, cell, join(dir, f), resultPath));
  }
}
audits.sort((a, b) => (a.repo + a.cell).localeCompare(b.repo + b.cell));

const failures = audits.filter((a) => !a.ok);
const armsSeen = [...new Set(audits.map((a) => a.arm))].sort();

// ---- render ----------------------------------------------------------------
const nz = (a) => (a && a.length ? a.join(", ") : "none");
const lines = [];
lines.push("# P1.1 - pre-flight audit, re-derived from the recorded transcripts");
lines.push("");
lines.push("Generated by `eval/agent-probe/audit-preflight.mjs`. It reads only");
lines.push("`results/raw/<repo>/*.stream.jsonl` and `*.result.json`. It does not read `runs.jsonl` and");
lines.push("shares no code with `run-probe.mjs`, because the harness's own isolation bookkeeping is");
lines.push("exactly what this checkpoint is supposed to test. The arm predicate is restated in the audit");
lines.push("script from DESIGN sections 3.1 and 8.");
lines.push("");
lines.push(`Runs audited: **${audits.length}** across arms ${armsSeen.join(", ")}. `
  + `Failures: **${failures.length}**.`);
lines.push("");
lines.push("## DESIGN section 8 pre-flight assertions, per run");
lines.push("");
lines.push("| run | arm | 1. analyze cached | 2. G: 0 mcp calls | 3. M: 0 Read/Grep/Glob | 4. cost > 0 | nothing outside arm |");
lines.push("|---|---|---|---|---|---|---|");
for (const a of audits) {
  const cellFor = (id) => {
    const x = a.assertions?.find((s) => s.id === id);
    if (!x) return "-";
    return x.na ? "n/a" : (x.ok ? "PASS" : "**FAIL**");
  };
  lines.push(`| \`${a.cell}\` | ${a.arm} | ${cellFor("A1-analyze-cached")} | ${cellFor("A2-armG-zero-mcp")} `
    + `| ${cellFor("A3-armM-zero-file")} | ${cellFor("A4-cost-nonzero")} | ${cellFor("X-nothing-outside-arm")} |`);
}
lines.push("");
lines.push("## What each run actually offered, attempted and spent");
lines.push("");
lines.push("| run | arm | tools offered (mcp) | mcp servers | tool_use blocks | mcp attempts | Read/Grep/Glob attempts | offered outside arm | attempted outside arm | costUsd |");
lines.push("|---|---|---|---|---|---|---|---|---|---|");
for (const a of audits) {
  if (!a.assertions) continue;
  lines.push(`| \`${a.cell}\` | ${a.arm} | ${a.toolsOfferedCount} (${a.mcpToolsOfferedCount}) | ${nz(a.mcpServers)} `
    + `| ${a.toolUseBlocks} | ${a.mcpAttempts} | ${a.fileAttempts} | ${nz(a.offeredOutsideArm)} `
    + `| ${nz(a.attemptedOutsideArm)} | ${a.costUsd} |`);
}
lines.push("");
lines.push("Attempts are counted as `tool_use` blocks, so a call that was denied or errored still counts");
lines.push("against its arm. That is stricter than counting executed calls, and it is the count the");
lines.push("isolation claim needs.");
lines.push("");
lines.push("## Assertion 1 in detail - warmth (DESIGN 4.5)");
lines.push("");
for (const a of audits) {
  const x = a.assertions?.find((s) => s.id === "A1-analyze-cached");
  if (!x) continue;
  lines.push(`- **${a.cell}** (arm ${a.arm}): ${x.detail}`);
}
lines.push("");
lines.push("## Assertion 4 in detail - is the cost figure real");
lines.push("");
lines.push("Method used: **reported `total_cost_usd`**, not a reconstruction. The reconstruction column is");
lines.push("a cross-check only, priced from `modelUsage` at published rates with cache reads at 0.1x input");
lines.push("and cache writes at the multiplier for the TTL the run actually used.");
lines.push("");
lines.push("| run | arm | costUsd | reconstructed | delta | cache write TTL | multiplier |");
lines.push("|---|---|---|---|---|---|---|");
for (const a of audits) {
  if (!a.assertions) continue;
  lines.push(`| \`${a.cell}\` | ${a.arm} | ${a.costUsd} | ${a.costReconstructed} | ${a.costReconDeltaPct}% `
    + `| ${a.cacheWriteTtl} | ${a.cacheWriteMultiplier}x |`);
}
lines.push("");
lines.push("DESIGN section 4.1 prescribes 1.25x for cache creation. These runs write to the **1-hour**");
lines.push("cache (`usage.cache_creation.ephemeral_1h_input_tokens` carries the whole write), which bills at");
lines.push("2x base input. At 1.25x the reconstruction lands about 19% low. It does not affect the headline");
lines.push("number, because cost is non-zero here and the reported field is what the experiment uses - but");
lines.push("if a later batch ever returns zero, the fallback must use 2x for 1-hour writes.");
lines.push("");
lines.push("## Ambient-context check (DESIGN 6.3)");
lines.push("");
lines.push("| run | arm | cli | model | permission mode | apiKeySource | memory paths |");
lines.push("|---|---|---|---|---|---|---|");
for (const a of audits) {
  if (!a.assertions) continue;
  lines.push(`| \`${a.cell}\` | ${a.arm} | ${a.cliVersion} | ${a.model} | ${a.permissionMode} | ${a.apiKeySource} | ${nz(a.memoryPaths)} |`);
}
lines.push("");
lines.push("## Verdict");
lines.push("");
lines.push(failures.length === 0
  ? "All four DESIGN section 8 pre-flight assertions hold on every recorded run, re-derived independently"
  + " of the harness. Arm isolation held: no run was offered a tool outside its arm, and no run attempted"
  + " one. The pilot may proceed."
  : `**${failures.length} run(s) fail an assertion.** Every number collected so far is void until the`
  + " harness is fixed and the affected cells are re-run in all three arms.");
for (const a of failures) {
  for (const s of (a.assertions || []).filter((x) => !x.ok)) {
    lines.push(`- \`${a.cell}\` (arm ${a.arm}) **${s.id}**: ${s.detail}`);
  }
}
lines.push("");
lines.push("## Scope");
lines.push("");
lines.push("This audits what has been recorded so far, which at P1 is one class-A question on eShop, one");
lines.push("repetition per arm. It says the arms are isolated and the meter works. It says nothing about");
lines.push("whether the MCP helps.");
lines.push("");

const md = lines.join("\n");
if (!CHECK_ONLY) {
  writeFileSync(OUT_MD, md, "utf8");
  writeFileSync(OUT_JSON, JSON.stringify({ generatedFrom: "results/raw", runs: audits }, null, 2), "utf8");
  console.log(`wrote ${OUT_MD}`);
  console.log(`wrote ${OUT_JSON}`);
}
for (const a of audits) {
  console.log(`${a.ok ? "PASS" : "FAIL"} ${a.repo}/${a.cell} arm=${a.arm}`
    + ` mcpAttempts=${a.mcpAttempts} fileAttempts=${a.fileAttempts} cost=${a.costUsd}`);
  for (const s of (a.assertions || []).filter((x) => !x.ok)) console.log(`     ${s.id}: ${s.detail}`);
}
console.log(failures.length === 0
  ? `pre-flight audit GREEN over ${audits.length} run(s)`
  : `pre-flight audit RED: ${failures.length} of ${audits.length} run(s) failed`);
process.exit(failures.length === 0 ? 0 : 1);
