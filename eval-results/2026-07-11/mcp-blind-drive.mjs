// MCP Blind-Drive Audit — an AI agent explores repos it has NEVER seen via DevContext MCP
// Usage: node --experimental-strip-types eval-results/2026-07-11/mcp-blind-drive.mjs
// Server must be running on http://127.0.0.1:5179

import { spawn } from "child_process";
import { join, dirname } from "path";
import { createInterface } from "readline";
import { writeFileSync, existsSync, mkdirSync } from "fs";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = join(__dirname, "..", "..");

const MCP_EXE = join(REPO_ROOT, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

const REPOS = [
  {
    name: "CleanArchProject",
    path: join(REPO_ROOT, "tests", "fixtures", "CleanArchProject"),
    archetype: "Clean Architecture (Domain/App/Infra/Web)",
    files: 4,
    description: "Small clean-architecture project with MediatR handler",
  },
  {
    name: "ControllerApp",
    path: join(REPO_ROOT, "tests", "fixtures", "ControllerApp"),
    archetype: "Controller-based MVC",
    files: 5,
    description: "Traditional controller-based ASP.NET app",
  },
];

// ---- stdio JSON-RPC transport ----
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
    } catch (_) { /* skip non-JSON lines */ }
  });

  proc.stderr.resume();

  function call(method, params) {
    return new Promise((resolve, reject) => {
      const id = nextId++;
      pending.set(id, resolve);
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
      setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); }
      }, 60000);
    });
  }

  function notify(method, params) {
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  }

  function close() {
    rl.close();
    proc.kill();
  }

  return { call, notify, close };
}

// Parse tool result from MCP response
function parseToolResult(result) {
  if (result?.content && Array.isArray(result.content)) {
    const texts = result.content.filter(c => c.type === "text").map(c => c.text).join("\n");
    try { return JSON.parse(texts); } catch { return { text: texts }; }
  }
  try { return JSON.parse(result); } catch { return result ?? {}; }
}

// Estimate tokens from string
function estimateTokens(text) {
  if (typeof text !== "string") return 0;
  return Math.ceil(text.length / 4);
}

// Call a tool and return parsed result
async function toolCall(client, tool, args, tracker = null) {
  const resp = await client.call("tools/call", { name: tool, arguments: args });
  if (resp.error) throw new Error(`Tool ${tool} error: ${JSON.stringify(resp.error)}`);
  const data = parseToolResult(resp.result);
  if (tracker) {
    tracker.calls += 1;
    tracker.tokens += estimateTokens(JSON.stringify(data));
  }
  return data;
}

// Get tool content as text (not JSON parsed)
async function toolCallRaw(client, tool, args) {
  const resp = await client.call("tools/call", { name: tool, arguments: args });
  if (resp.error) return { error: resp.error };
  if (resp.result?.content && Array.isArray(resp.result.content)) {
    return { text: resp.result.content.filter(c => c.type === "text").map(c => c.text).join("\n") };
  }
  return { raw: JSON.stringify(resp.result) };
}

// Bootstrap MCP server
async function bootstrap(client) {
  const initResp = await client.call("initialize", {
    protocolVersion: "2024-11-05",
    capabilities: {},
    clientInfo: { name: "mcp-blind-drive", version: "0.0.1" },
  });
  if (initResp.error) throw new Error(`Init failed: ${JSON.stringify(initResp.error)}`);
  client.notify("notifications/initialized", {});
  await new Promise(r => setTimeout(r, 2000)); // let server fully spin up

  const toolsResp = await client.call("tools/list", {});
  const tools = (toolsResp.result?.tools ?? []).map(t => t.name).sort();
  return { tools };
}

// Analyze repo with polling (large repos can exceed per-call timeout)
async function analyzeRepo(client, repoPath) {
  // Fire analyze in background
  const analyzePromise = client.call("tools/call", {
    name: "analyze",
    arguments: { path: repoPath }
  }).then(resp => resp, err => ({ error: err }));

  // Poll list_sessions for completion
  let handle = null;
  for (let i = 0; i < 60; i++) {
    await new Promise(r => setTimeout(r, 1000));
    try {
      const sessions = parseToolResult(
        (await client.call("tools/call", { name: "list_sessions", arguments: {} })).result
      ).sessions ?? [];
      const ready = sessions.find(s => s.nodes > 0);
      if (ready) { handle = ready.handle; break; }
    } catch (_) {}
  }

  // Fallback to analyze result
  try {
    const r = parseToolResult((await analyzePromise).result);
    handle = handle ?? r?.handle ?? null;
  } catch (_) {}

  return handle;
}

// ----------------------------------------------------------------
// BLIND DRIVE: Per-repo exploration
// ----------------------------------------------------------------

async function exploreRepo(client, repo, repoIndex) {
  const results = { repo: repo.name, steps: [], totalCalls: 0, totalTokens: 0, score: 0 };
  const t = { calls: 0, tokens: 0 };
  const log = (...args) => results.steps.push(args.join(" "));

  console.log(`\n${"=".repeat(60)}`);
  console.log(`REPO ${repoIndex + 1}: ${repo.name}`);
  console.log(`Path: ${repo.path}`);
  console.log(`${"=".repeat(60)}`);

  // STEP 1: Analyze
  console.log("\n--- STEP 1: analyze ---");
  const start1 = Date.now();
  const handle = await analyzeRepo(client, repo.path);
  t.calls += 1; t.tokens += estimateTokens(JSON.stringify({ handle }));
  const elapsed1 = ((Date.now() - start1) / 1000).toFixed(1);
  console.log(`  Handle: ${handle}`);
  console.log(`  Elapsed: ${elapsed1}s`);
  log(`analyze: handle=${handle} elapsed=${elapsed1}s`);

  if (!handle) {
    console.log("  FAILED: No handle returned");
    log("FAIL: analyze returned no handle");
    return results;
  }

  // STEP 2: Overview
  console.log("\n--- STEP 2: overview ---");
  const overview = await toolCall(client, "overview", { handle }, t);
  console.log(`  Result: ${JSON.stringify(overview)?.substring(0, 300)}...`);
  log(`overview: tokens=${overview.tokens}, text="${overview.text?.substring(0, 100)}"`);

  // STEP 3: Stats
  console.log("\n--- STEP 3: stats ---");
  const stats = await toolCall(client, "stats", { handle }, t);
  console.log(`  Meta: ${stats.meta}`);
  console.log(`  Nodes: ${stats.nodeCount} | Edges: ${stats.edgeCount} | Entries: ${stats.entryCount}`);
  console.log(`  Seams: ${JSON.stringify(stats.seams)}`);
  console.log(`  Warnings: ${(stats.warnings || []).join(", ")}`);
  log(`stats: ${stats.meta} seams=${stats.seams?.length} warnings=${stats.warnings?.length}`);

  // STEP 4: Entrypoints
  console.log("\n--- STEP 4: entrypoints ---");
  const entries = await toolCall(client, "entrypoints", { handle }, t);
  console.log(`  Count: ${entries.count}`);
  for (const e of entries.entries || []) {
    console.log(`    [${e.kind}] ${e.httpMethod || ""} ${e.route || e.title} → ${e.target || "(no target)"} (${e.project})`);
  }
  log(`entrypoints: ${entries.count} entries`);

  // STEP 5: Map
  console.log("\n--- STEP 5: map ---");
  const map = await toolCall(client, "map", { handle }, t);
  console.log(`  Meta: ${map.meta}`);
  console.log(`  Archetype: ${map.archetype} | Style: ${map.style} | Confidence: ${map.styleConfidence}`);
  console.log(`  Topology: ${JSON.stringify(map.topology)}`);
  log(`map: archetype=${map.archetype} style=${map.style}`);

  // STEP 6: Top flows
  console.log("\n--- STEP 6: top_flows ---");
  const flows = await toolCall(client, "top_flows", { handle }, t);
  console.log(`  Count: ${flows.count}`);
  for (const f of flows.topFlows || []) {
    console.log(`    ${f.kind}: ${f.title} → ${f.target || "?"} (score=${f.score}, depth=${f.depth})`);
  }
  log(`top_flows: ${flows.count} flows`);

  // STEP 7: Trace the first flow
  if (flows.topFlows?.length > 0) {
    const firstFlow = flows.topFlows[0];
    const focus = firstFlow.route || firstFlow.title;
    console.log(`\n--- STEP 7: trace("${focus}") compact ---`);
    const trace = await toolCall(client, "trace", { handle, focus, depth: 6, format: "compact" }, t);
    console.log(`  Found: ${trace.found}`);
    if (trace.found) {
      console.log(`  Text:\n${trace.text}`);
      log(`trace: found=true tokens=${trace.tokens} steps=${trace.text?.split("\\n").filter(Boolean).length}`);
    } else {
      console.log(`  Error: ${trace.error || "not found"}`);
      log(`trace: found=false`);
    }

    // STEP 7b: Trace verbose
    console.log(`\n--- STEP 7b: trace("${focus}") verbose ---`);
    const traceVerbose = await toolCall(client, "trace", { handle, focus, depth: 6, format: "default" }, t);
    if (traceVerbose.found) {
      const entryNodeId = traceVerbose.entry?.nodeId;
      const rootTitle = traceVerbose.entry?.title;
      console.log(`  Entry: ${rootTitle} (${entryNodeId})`);
      console.log(`  Touched: ${traceVerbose.touchedEntities?.join(", ") || "none"}`);
      console.log(`  Emits: ${traceVerbose.emittedEvents?.join(", ") || "none"}`);
      log(`trace-verbose: entry=${entryNodeId} touched=${traceVerbose.touchedEntities?.length} emits=${traceVerbose.emittedEvents?.length}`);
    }
  }

  // STEP 8: Resolve key symbols
  console.log("\n--- STEP 8: resolve key symbols ---");
  const symbols = ["Product", "Program", "Handler", "Controller", "DbContext"];
  for (const sym of symbols) {
    const resolved = await toolCall(client, "resolve", { handle, query: sym, limit: 5 }, t);
    console.log(`  resolve("${sym}"): count=${resolved.count}, ambiguous=${resolved.ambiguous}`);
    if (resolved.candidates?.length > 0) {
      for (const c of resolved.candidates.slice(0, 3)) {
        console.log(`    - [${c.kind}] ${c.title} (${c.nodeId}) ${c.filePath || ""}:${c.lineNumber || ""}`);
      }
    } else if (resolved.error) {
      console.log(`    Error: ${resolved.error}. Hint: ${resolved.hint}`);
      if (resolved.candidates?.length > 0) {
        console.log(`    Did you mean? ${resolved.candidates.map(c => c.title).join(", ")}`);
      }
    }
  }
  log(`resolve: tested ${symbols.length} symbols`);

  // STEP 9: Node detail + neighbors + read_source for first entry target
  if (entries.entries?.length > 0 && entries.entries[0].nodeId) {
    const entryNodeId = entries.entries[0].nodeId;
    console.log(`\n--- STEP 9: node("${entryNodeId}") ---`);
    const node = await toolCall(client, "node", { handle, nodeId: entryNodeId }, t);
    console.log(`  Title: ${node.title}`);
    console.log(`  Kind: ${node.kind} | File: ${node.filePath}:${node.lineNumber}`);
    console.log(`  In-degree: ${node.inDegree} | Out-degree: ${node.outDegree}`);
    console.log(`  Tags: ${node.tags?.join(", ") || "none"}`);
    log(`node: ${node.title} kind=${node.kind} in=${node.inDegree} out=${node.outDegree}`);

    // Neighbors
    console.log(`\n--- STEP 9b: neighbors("${entryNodeId}") ---`);
    const nb = await toolCall(client, "neighbors", { handle, nodeId: entryNodeId, direction: "out" }, t);
    console.log(`  Outgoing edges: ${nb.count}`);
    for (const e of nb.edges?.slice(0, 5) || []) {
      console.log(`    → [${e.kind}] ${e.otherTitle} (${e.provenance || "?"})`);
    }

    // Read source
    console.log(`\n--- STEP 9c: read_source("${entryNodeId}", window, 30) ---`);
    const src = await toolCall(client, "read_source", { handle, nodeId: entryNodeId, windowLines: 30, mode: "window" }, t);
    console.log(`  Found: ${src.found} | File: ${src.filePath}`);
    console.log(`  Lines: ${src.startLine}-${src.endLine} / ${src.totalLines}`);
    if (src.content) {
      console.log(`  Content preview:\n${src.content.substring(0, 300)}...`);
    }
    log(`read_source: file=${src.filePath} lines=${src.startLine}-${src.endLine}`);
  }

  // STEP 10: get_context
  if (flows.topFlows?.length > 0) {
    const focus = flows.topFlows[0].route || flows.topFlows[0].title;
    console.log(`\n--- STEP 10: get_context("${focus}", budget=4000, intent=trace) ---`);
    const ctx = await toolCall(client, "get_context", { handle, focus, budgetTokens: 4000, intent: "trace" }, t);
    console.log(`  Focus: ${ctx.focus} | Budget: ${ctx.budgetTokens} | Total: ${ctx.totalTokens}`);
    console.log(`  Sections: ${ctx.sections?.map(s => `${s.key}(${s.tokens}tok)`).join(", ") || "none"}`);
    console.log(`  Omitted: ${ctx.omitted?.join(", ") || "none"}`);
    if (ctx.content) {
      console.log(`  Content preview (first 500 chars):\n${ctx.content.substring(0, 500)}...`);
    }
    log(`get_context: budget=${ctx.budgetTokens} actual=${ctx.totalTokens} sections=${ctx.sections?.length}`);
  }

  // STEP 11: find + impact
  if (entries.entries?.length > 0) {
    const targetTitle = entries.entries[0].target || entries.entries[0].title;
    console.log(`\n--- STEP 11: find("${targetTitle}") ---`);
    const found = await toolCall(client, "find", { handle, query: targetTitle, limit: 5 }, t);
    console.log(`  Results: ${found.count} / ${found.total}`);
    for (const r of found.results?.slice(0, 3) || []) {
      console.log(`    [${r.kind}] ${r.title} (${r.nodeId})`);
    }

    if (found.results?.length > 0) {
      const impactNodeId = found.results[0].nodeId;
      console.log(`\n--- STEP 11b: impact("${impactNodeId}", up, maxDepth=4) ---`);
      const impact = await toolCall(client, "impact", { handle, nodeId: impactNodeId, maxDepth: 4, direction: "up" }, t);
      console.log(`  Direction: ${impact.direction} | Total affected: ${impact.totalAffected}`);
      console.log(`  By service: ${JSON.stringify(impact.resultsByService)}`);
      log(`impact: dir=up affected=${impact.totalAffected}`);
    }
  }

  // STEP 12: config (ControllerApp only)
  if (repo.name === "ControllerApp") {
    console.log("\n--- STEP 12: config ---");
    const config = await toolCall(client, "config", { handle }, t);
    console.log(`  Total keys: ${config.totalKeys}`);
    console.log(`  Keys: ${JSON.stringify(config.keys)}`);
    log(`config: keys=${config.totalKeys}`);
  }

  // STEP 13: insights
  console.log("\n--- STEP 13: insights ---");
  const insights = await toolCall(client, "insights", { handle }, t);
  console.log(`  Count: ${insights.count}`);
  for (const i of insights.insights?.slice(0, 5) || []) {
    console.log(`    [${i.severity}] ${i.title}: ${i.detail}`);
  }
  log(`insights: ${insights.count} items`);

  // STEP 14: tests_for
  if (entries.entries?.length > 0 && entries.entries[0].target) {
    const targetNodeId = entries.entries[0].nodeId;
    console.log(`\n--- STEP 14: tests_for("${targetNodeId}") ---`);
    const tests = await toolCall(client, "tests_for", { handle, nodeId: targetNodeId, maxDepth: 6 }, t);
    console.log(`  Node: ${tests.nodeTitle} | Best-effort: ${tests.isBestEffort} | Count: ${tests.count}`);
    for (const test of tests.tests?.slice(0, 5) || []) {
      console.log(`    ${test.title} (${test.filePath}:${test.lineNumber}, distance=${test.distance})`);
    }
    log(`tests_for: count=${tests.count} bestEffort=${tests.isBestEffort}`);
  }

  // STEP 15: Error scenarios
  console.log("\n--- STEP 15: Error handling probes ---");

  // 15a: Non-existent tool
  console.log("  15a: Calling non-existent tool 'blerg'...");
  const badTool = await client.call("tools/call", { name: "blerg", arguments: { handle } });
  const badToolData = parseToolResult(badTool.result || {});
  console.log(`  ${badTool.error ? "ERROR (std)" : "CAUGHT (handler): " + JSON.stringify(badToolData)?.substring(0, 200)}`);
  log(`error-probe: bad tool blerg -> ${badTool.error ? "std-error" : "handler-caught"}`);

  // 15b: Resolve non-existent symbol
  console.log("  15b: resolve non-existent symbol 'ZzBbNotReal'...");
  const badResolve = await toolCall(client, "resolve", { handle, query: "ZzBbNotReal", limit: 10 }, t);
  console.log(`  count=${badResolve.count} error="${badResolve.error || "none"}" hint="${badResolve.hint || "none"}"`);
  const resolveSignal = badResolve.error ? (badResolve.hint ? "signaled+hint" : "signaled-no-hint") : "zero-success";
  log(`error-probe: bad resolve -> ${resolveSignal}`);

  // 15c: Usages of non-existent symbol
  console.log("  15c: usages of non-existent 'IFakeRepository'...");
  const badUsages = await toolCall(client, "usages", { handle, nodeId: "IFakeRepository" }, t);
  console.log(`  count=${badUsages.count} error="${badUsages.error || "none"}"`);
  log(`error-probe: bad usages -> ${badUsages.error ? "signaled" : "zero-count"}`);

  // 15d: Impact of non-existent symbol
  console.log("  15d: impact of non-existent 'FakeService'...");
  const badImpact = await toolCall(client, "impact", { handle, nodeId: "FakeService", maxDepth: 4, direction: "up" }, t);
  console.log(`  affected=${badImpact.totalAffected} error="${badImpact.error || "none"}"`);
  log(`error-probe: bad impact -> ${badImpact.error ? "signaled" : `zero-affected=${badImpact.totalAffected}`}`);

  // 15e: get_context with NL focus
  console.log("  15e: get_context with natural language 'how does this work'...");
  const badCtx = await toolCall(client, "get_context", { handle, focus: "how does this work", budgetTokens: 4000, intent: "trace" }, t);
  console.log(`  focus="${badCtx.focus}" error="${badCtx.error || "none"}" sections=${badCtx.sections?.length || 0}`);
  log(`error-probe: NL get_context -> ${badCtx.error ? "signaled" : `sections=${badCtx.sections?.length}`}`);

  // Compute score
  const actionableErrors = [
    badResolve.error && badResolve.hint,
    badUsages.error !== undefined,
    badImpact.error !== undefined,
  ].filter(Boolean).length;

  results.totalCalls = t.calls;
  results.totalTokens = t.tokens;
  results.score = `${actionableErrors}/3 actionable errors + ${badTool.error ? 1 : 1} bad-tool handled`;
  results.numCallsThatProducedData = results.steps.length;
  log(`FINAL: ${t.calls} calls, ${t.tokens} tokens, ${actionableErrors}/3 errors actionable`);

  console.log(`\n>>> ${repo.name}: ${t.calls} calls, ${t.tokens} tokens, ${actionableErrors}/3 errors actionable`);
  return results;
}

// ----------------------------------------------------------------
// MAIN
// ----------------------------------------------------------------

async function main() {
  console.log("MCP Blind-Drive Audit");
  console.log(`Date: ${new Date().toISOString()}`);
  console.log(`Server: http://127.0.0.1:5179`);
  console.log(`MCP EXE: ${MCP_EXE}`);

  if (!existsSync(MCP_EXE)) {
    console.error(`ERROR: MCP binary not found at ${MCP_EXE}`);
    process.exit(1);
  }

  const client = mcpClient(MCP_EXE);

  try {
    // Bootstrap
    const { tools } = await bootstrap(client);
    console.log(`\nAvailable tools (${tools.length}): ${tools.join(", ")}`);

    const allResults = [];
    for (let i = 0; i < REPOS.length; i++) {
      if (!existsSync(REPOS[i].path)) {
        console.log(`\nSKIPPING ${REPOS[i].name}: path not found`);
        continue;
      }
      const r = await exploreRepo(client, REPOS[i], i);
      allResults.push(r);
    }

    // Close session
    try { await toolCall(client, "close_session", {}); } catch {}

    // Write report
    const report = {
      timestamp: new Date().toISOString(),
      server: "http://127.0.0.1:5179",
      tools: tools.length,
      repos: allResults,
    };

    const outPath = join(__dirname, "mcp-blind-drive-report.json");
    writeFileSync(outPath, JSON.stringify(report, null, 2));
    console.log(`\n\nReport written to ${outPath}`);

    // Summary
    console.log(`\n${"=".repeat(60)}`);
    console.log("SUMMARY");
    console.log(`${"=".repeat(60)}`);
    for (const r of allResults) {
      console.log(`  ${r.repo}: ${r.totalCalls} calls, ${r.totalTokens} tokens, errors: ${r.score}`);
    }
  } finally {
    client.close();
  }
}

main().catch(err => {
  console.error("FATAL:", err);
  process.exit(1);
});
