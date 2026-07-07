// Cold-agent MCP QA harness (Loom L0.2).
// Usage: node eval/mcp-qa/run-cold.js [--repo <path>] [--quiet] [--gate]
//
// Unlike run.js (which knows every signature, threads handles, and hard-codes the
// exact checkout route), this harness plays a NAIVE agent with zero prior knowledge:
//   - it calls tools before analyzing / without a handle,
//   - it guesses natural-language focuses ("how does checkout work"),
//   - it invents symbol names, renames parameters, omits required ones,
//   - it calls a tool that does not exist.
//
// For every call that cannot succeed, we score the response on two axes the audit
// (§4) says are missing today:
//   (a) does it say WHAT WAS WRONG (a real error/So-not-found signal), and
//   (b) does it give a COPYABLE NEXT STEP (hint / example / candidates / schema)?
// A response that returns a zero-shaped "success" (impact totalAffected:0 on a
// made-up type, usages count:0, config totalKeys:0, an empty success for an unknown
// tool) is the trap the audit calls out — it is counted NOT actionable.
//
// Baseline is expected ~0% today (audit drive #1: 15/15 opaque failures). L0 records
// it; L5.5 flips this into the enforced gate (>=90% actionable). Pass --gate to make
// the process exit non-zero below threshold (used from L5 on).

const { spawn } = require("child_process");
const { join } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync } = require("fs");

const REPO = process.argv.includes("--repo")
  ? process.argv[process.argv.indexOf("--repo") + 1]
  : "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";
const QUIET = process.argv.includes("--quiet");
const GATE = process.argv.includes("--gate");
const GATE_THRESHOLD = 0.9; // L5.5 target: >=90% of naive failures are actionable

const MCP_EXE = join(
  __dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0",
  "devcontext-mcp.exe"
);

// ---- JSON-RPC transport over stdio (mirrors run.js) ----

function mcpClient(exePath) {
  const proc = spawn(exePath, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  let nextId = 1;
  const pending = new Map();

  rl.on("line", (line) => {
    try {
      const msg = JSON.parse(line);
      if (msg.id !== undefined && pending.has(msg.id)) {
        pending.get(msg.id)(msg);
        pending.delete(msg.id);
      }
    } catch (_) { /* skip logs */ }
  });
  proc.stderr.resume();

  function call(method, params = {}) {
    return new Promise((resolve, reject) => {
      const id = nextId++;
      pending.set(id, resolve);
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
      setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); }
      }, 45000);
    });
  }
  function notify(method, params = {}) {
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  }
  function close() { rl.close(); proc.kill(); }
  return { call, notify, close };
}

function sleep(ms) { return new Promise((r) => setTimeout(r, ms)); }

async function bootstrap(client) {
  const initResp = await client.call("initialize", {
    protocolVersion: "2024-11-05",
    capabilities: {},
    clientInfo: { name: "mcp-cold-qa", version: "0.0.1" },
  });
  if (initResp.error) throw new Error(`Init failed: ${JSON.stringify(initResp.error)}`);
  client.notify("notifications/initialized", {});
  const toolsResp = await client.call("tools/list", {});
  const tools = toolsResp.result?.tools ?? [];
  return { toolNames: tools.map((t) => t.name).sort(), toolsListResp: toolsResp };
}

function parseToolResult(text) {
  if (!text || typeof text !== "string") return text ?? {};
  try { return JSON.parse(text); } catch { return { text }; }
}
function extractContent(result) {
  if (result?.content && Array.isArray(result.content)) {
    const texts = result.content.filter((c) => c.type === "text").map((c) => c.text).join("\n");
    return parseToolResult(texts);
  }
  return parseToolResult(result);
}
function estimateTokens(s) { return typeof s === "string" ? Math.ceil(s.length / 4) : 0; }

// Raw call: never throws, returns everything a naive agent would actually see.
async function rawToolCall(client, tool, args) {
  let resp;
  try {
    resp = await client.call("tools/call", { name: tool, arguments: args });
  } catch (err) {
    return { mcpError: { message: err.message }, isError: true, data: {}, rawText: err.message };
  }
  const mcpError = resp.error ?? null;
  const result = resp.result ?? {};
  const data = extractContent(result);
  const rawText = typeof data === "object" ? JSON.stringify(data) : String(data);
  return { mcpError, isError: result.isError === true, data, rawText };
}

async function analyzeRepo(client, repoPath) {
  // Capture the rejection immediately: on large repos `analyze` can exceed the
  // per-call timeout while the poll loop below still finds the ready handle.
  // Without this guard the timed-out promise would surface as an unhandledRejection.
  const analyzePromise = client
    .call("tools/call", { name: "analyze", arguments: { path: repoPath } })
    .then((resp) => resp, (err) => ({ error: err }));
  let handle = null;
  for (let i = 0; i < 240; i++) {
    await sleep(500);
    try {
      const listResp = await client.call("tools/call", { name: "list_sessions", arguments: {} });
      if (!handle && listResp.result) {
        const sessions = extractContent(listResp.result).sessions ?? [];
        const ready = sessions.find((s) => s.status === "ready" || s.status === "done");
        if (ready) handle = ready.handle;
      }
    } catch (_) {}
    if (handle) break;
  }
  try {
    const r = extractContent((await analyzePromise).result);
    handle = handle ?? r?.handle ?? null;
  } catch (_) {}
  return handle;
}

// ---- Actionability classifier ----
//
// signaled:      the agent can TELL the call did not succeed.
// nextStep:      the response hands the agent something copyable to try next.
// actionable = signaled && nextStep && !falseSuccess.
// A zero-shaped "success" (numeric-zero / empty-list / bare found:false with no
// message) does NOT signal wrong — that is the silent-wrong-answer trap.

const NEXT_STEP_KEYS = ["hint", "example", "candidates", "didYouMean", "did_you_mean",
  "suggestions", "suggestion", "expected", "schema", "availableTools", "available_tools",
  "tools", "usage", "nextStep", "next_step"];

function hasNextStep(data, rawText) {
  if (data && typeof data === "object") {
    for (const k of NEXT_STEP_KEYS) {
      const v = data[k];
      if (typeof v === "string" && v.trim().length > 0) return true;
      if (Array.isArray(v) && v.length > 0) return true;
      if (v && typeof v === "object" && Object.keys(v).length > 0) return true;
    }
  }
  // Prose hint embedded in an error string ("did you mean", "try", "expected", "run analyze")
  const t = (rawText || "").toLowerCase();
  return /did you mean|try (calling|`|the)|expected (a|an|one of|schema|parameter)|available tools|call analyze|run `?analyze|provide a|must (be|provide)/.test(t);
}

function classify(probe, res) {
  const { mcpError, isError, data, rawText } = res;
  const zeroShaped =
    (typeof data?.totalAffected === "number" && data.totalAffected === 0) ||
    (typeof data?.count === "number" && data.count === 0 &&
      !(Array.isArray(data?.tests) && probe.softZeroOk)) ||
    (typeof data?.totalKeys === "number" && data.totalKeys === 0) ||
    (data?.found === false) ||
    (Array.isArray(data?.candidates) && data.candidates.length === 0) ||
    (Array.isArray(data?.results) && data.results.length === 0);

  // Opaque transport error like "An error occurred invoking '<tool>'"
  const opaque = /an error occurred invoking/i.test(rawText || "") ||
    (mcpError && /an error occurred invoking/i.test(mcpError.message || ""));

  const hardError = !!mcpError || isError === true || typeof data?.error === "string";
  const explicitProblem = hardError ||
    /not found|unknown|no such|ambiguous|invalid|missing|unrecognized|does not exist/i.test(rawText || "");

  // Did the naive call "succeed" in a way that hides the failure?
  const falseSuccess = !explicitProblem && zeroShaped;

  // A signal that is ONLY the opaque string still "signals wrong" but gives no next step.
  const signaled = explicitProblem; // hard error or explicit problem word
  const nextStep = hasNextStep(data, rawText);
  const actionable = signaled && nextStep && !falseSuccess;

  let verdict;
  if (actionable) verdict = "actionable";
  else if (falseSuccess) verdict = "false-success (silent wrong answer)";
  else if (signaled && !nextStep) verdict = opaque ? "opaque error (no next step)" : "signaled, no next step";
  else verdict = "unactionable";

  return { signaled, nextStep, falseSuccess, opaque, actionable, verdict };
}

// ---- Naive probes ----
// phase A: before analyzing (no handle) — the agent hasn't learned the workflow.
// phase B: after analyzing (has handle) — wrong args, NL focus, invented names.

function buildProbes() {
  return [
    // ---- phase A: no session yet ----
    { id: "A1-overview-no-handle", phase: "A", intent: "overview before analyze",
      tool: "overview", args: {} },
    { id: "A2-trace-no-handle", phase: "A", intent: "trace before analyze",
      tool: "trace", args: { focus: "checkout" } },
    { id: "A3-resolve-no-handle", phase: "A", intent: "resolve before analyze",
      tool: "resolve", args: { query: "Order" } },

    // ---- phase B: has handle, but naive arguments ----
    { id: "B1-nonexistent-tool", phase: "B", intent: "call a tool that does not exist",
      tool: "flow", args: (h) => ({ handle: h, focus: "checkout" }) },
    { id: "B2-trace-nl-focus", phase: "B", intent: "natural-language focus",
      tool: "trace", args: (h) => ({ handle: h, focus: "how does checkout work" }) },
    { id: "B3-impact-madeup", phase: "B", intent: "impact of a symbol that does not exist",
      tool: "impact", args: (h) => ({ handle: h, nodeId: "TotallyMadeUpType", maxDepth: 3 }) },
    { id: "B4-usages-shortname", phase: "B", intent: "usages by short name (not a nodeId)",
      tool: "usages", args: (h) => ({ handle: h, nodeId: "IBasketRepository" }) },
    { id: "B5-config-exact-key", phase: "B", intent: "config filtered by a plausible key",
      tool: "config", args: (h) => ({ handle: h, key: "ConnectionStrings" }) },
    { id: "B6-getcontext-nl-focus", phase: "B", intent: "get_context with NL focus",
      tool: "get_context", args: (h) => ({ handle: h, focus: "basket checkout" }) },
    { id: "B7-resolve-missing-required", phase: "B", intent: "omit the required query param",
      tool: "resolve", args: (h) => ({ handle: h }) },
    { id: "B8-trace-wrong-param-name", phase: "B", intent: "guess param name 'route' not 'focus'",
      tool: "trace", args: (h) => ({ handle: h, route: "POST /basket/checkout" }) },
    { id: "B9-find-noise-query", phase: "B", intent: "find a common word, expect ranked not noise",
      tool: "find", args: (h) => ({ handle: h, query: "Order" }), rankQuality: true },
  ];
}

// ---- Main ----

async function main() {
  if (!existsSync(MCP_EXE)) {
    console.error(`MCP binary not found: ${MCP_EXE}\nBuild: dotnet build src/DevContext.Mcp`);
    process.exit(1);
  }
  if (!existsSync(REPO)) { console.error(`Repo not found: ${REPO}`); process.exit(1); }

  console.log("DevContext MCP COLD-agent QA (Loom L0.2)");
  console.log(`Repo: ${REPO}`);
  console.log("A naive agent with no prior knowledge drives the tools.\n");

  const client = mcpClient(MCP_EXE);
  const rows = [];
  let toolNames = [];
  let toolsListTokens = 0;
  let b9note = "";

  try {
    const bs = await bootstrap(client);
    toolNames = bs.toolNames;
    toolsListTokens = estimateTokens(JSON.stringify(bs.toolsListResp.result ?? {}));
    log(`Server ready. ${toolNames.length} tools. tools/list ~${toolsListTokens} tok.`);

    const probes = buildProbes();

    // phase A — run with NO handle first (agent hasn't learned to analyze)
    for (const p of probes.filter((x) => x.phase === "A")) {
      const args = typeof p.args === "function" ? p.args(null) : p.args;
      const res = await rawToolCall(client, p.tool, args);
      const c = classify(p, res);
      rows.push({ ...p, res, c });
      log(`  [${p.id}] ${p.intent} -> ${c.verdict}`);
    }

    // learn the workflow: analyze
    log("\nNaive agent discovers `analyze` from tools/list, analyzes...");
    const start = Date.now();
    const handle = await analyzeRepo(client, REPO);
    log(`Analyzed in ${((Date.now() - start) / 1000).toFixed(1)}s, handle=${handle ?? "(none)"}\n`);

    // phase B — has handle, naive args
    for (const p of probes.filter((x) => x.phase === "B")) {
      const args = typeof p.args === "function" ? p.args(handle) : p.args;
      const res = await rawToolCall(client, p.tool, args);
      const c = classify(p, res);
      rows.push({ ...p, res, c });
      log(`  [${p.id}] ${p.intent} -> ${c.verdict}`);
    }

    // Rank-quality note for B9: is the aggregate ranked #1? (audit §4 resolve "Order")
    const b9 = rows.find((r) => r.id === "B9-find-noise-query");
    if (b9) {
      const results = b9.res.data?.results ?? b9.res.data?.candidates ?? [];
      const top = results[0];
      const topTitle = top?.title ?? top?.name ?? top?.nodeId ?? "(none)";
      const orderAggregateTop = /(^|\.)Order($|\b)/.test(String(topTitle)) &&
        !/Ordering|Command|Query|Handler|Dto|Event/i.test(String(topTitle));
      b9note = `rank-quality (B9 find "Order"): top="${topTitle}" aggregate#1=${orderAggregateTop} results=${results.length}`;
      log(`  ${b9note}`);
    }

  } finally {
    client.close();
  }

  // ---- Score ----
  const total = rows.length;
  const actionable = rows.filter((r) => r.c.actionable).length;
  const falseSuccess = rows.filter((r) => r.c.falseSuccess).length;
  const opaque = rows.filter((r) => r.c.opaque).length;
  const pct = total ? (actionable / total) : 0;

  console.log("\n========================================");
  console.log("Cold-agent actionability (L0.2 baseline)");
  console.log("========================================");
  console.log("| Probe | Intent | Verdict |");
  console.log("|-------|--------|---------|");
  for (const r of rows) {
    console.log(`| ${r.id.padEnd(26)} | ${r.intent.slice(0, 34).padEnd(34)} | ${r.c.verdict} |`);
  }
  console.log("");
  console.log(`Actionable failures: ${actionable}/${total} (${(pct * 100).toFixed(0)}%)`);
  console.log(`False-successes (silent wrong answers): ${falseSuccess}`);
  console.log(`Opaque errors (no next step): ${opaque}`);
  if (b9note) console.log(b9note);
  console.log(`Gate threshold (L5.5): ${(GATE_THRESHOLD * 100).toFixed(0)}%  ->  ${pct >= GATE_THRESHOLD ? "PASS" : "BELOW (expected at L0 baseline)"}`);

  // ---- Artifact ----
  const dateStr = new Date().toISOString().slice(0, 10);
  const resultsDir = join(__dirname, "..", "..", "eval-results", dateStr);
  if (!existsSync(resultsDir)) mkdirSync(resultsDir, { recursive: true });

  const md = [];
  md.push("# MCP Cold-Agent QA — Baseline (Loom L0.2)");
  md.push("");
  md.push(`**Repo:** \`${REPO}\`  `);
  md.push(`**Date:** ${dateStr}  `);
  md.push(`**Tools:** ${toolNames.length} (\`${toolNames.join(", ")}\`)  `);
  md.push(`**tools/list envelope:** ~${toolsListTokens} tok (L5 target ≤1.5k)  `);
  md.push("");
  md.push("A naive agent with zero prior knowledge drives the tools: it calls before");
  md.push("analyzing, guesses natural-language focuses, invents symbol names, renames and");
  md.push("omits parameters, and calls a tool that does not exist. Each failing response is");
  md.push("scored on (a) does it say what was wrong, and (b) does it give a copyable next step.");
  md.push("");
  md.push("## Actionability");
  md.push("");
  md.push(`**Actionable failures: ${actionable}/${total} (${(pct * 100).toFixed(0)}%)**  `);
  md.push(`False-successes (silent wrong answers): ${falseSuccess}  `);
  md.push(`Opaque errors (no next step): ${opaque}  `);
  md.push(`Gate (L5.5 ≥${(GATE_THRESHOLD * 100).toFixed(0)}%): ${pct >= GATE_THRESHOLD ? "PASS" : "BELOW — baseline, gate arms in L5.5"}  `);
  if (b9note) md.push(`${b9note}  `);
  md.push("");
  md.push("## Per-probe");
  md.push("");
  md.push("| Probe | Phase | Intent | Verdict | Signaled | NextStep | Response (truncated) |");
  md.push("|-------|-------|--------|---------|----------|----------|----------------------|");
  for (const r of rows) {
    const resp = (r.res.mcpError ? `mcpError: ${r.res.mcpError.message}` : r.res.rawText).slice(0, 90).replace(/\|/g, "/").replace(/\n/g, " ");
    md.push(`| ${r.id} | ${r.phase} | ${r.intent} | ${r.c.verdict} | ${r.c.signaled} | ${r.c.nextStep} | ${resp} |`);
  }
  md.push("");
  md.push("## Owning stages for the red items");
  md.push("- Missing-session default / handle ergonomics → **L5.1**");
  md.push("- Error envelope `{error, hint, example}`, unknown-tool → tool list, schema on bad params → **L5.2**");
  md.push("- Unified ranked resolution (`resolve/find/usages/impact` short-name + did-you-mean) → **L5.3**");
  md.push("- Real `flow` tool + fuzzy focus suggestions → **L5.4**");
  md.push("- This harness becomes the enforced gate → **L5.5**");
  md.push("");

  const artPath = join(resultsDir, "mcp-cold-qa.md");
  writeFileSync(artPath, md.join("\n"), "utf8");
  console.log(`\nArtifact written to ${artPath}`);

  if (GATE && pct < GATE_THRESHOLD) {
    console.error(`\nGATE FAILED: ${(pct * 100).toFixed(0)}% < ${(GATE_THRESHOLD * 100).toFixed(0)}%`);
    process.exit(1);
  }
}

function log(msg) { if (!QUIET) console.log(msg); }

main().catch((err) => { console.error("FATAL:", err.message); process.exit(1); });
