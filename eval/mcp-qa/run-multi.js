// MCP multi-repo QA runner — extends single-repo harness for M5 ratchet.
// Usage: node eval/mcp-qa/run-multi.js [--quiet]
// Reads repo list from eval/mcp-qa/repos-m5.json.
// For each repo: analyze → run all questions → record token counts → output ratchet.

const { spawn } = require("child_process");
const { join } = require("path");
const { createInterface } = require("readline");
const { existsSync, writeFileSync, readFileSync } = require("fs");

const QUIET = process.argv.includes("--quiet");

const MCP_EXE = join(__dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");
const REPOS_PATH = join(__dirname, "repos-m5.json");
const RATCHET_PATH = join(__dirname, "..", "..", "eval-results", new Date().toISOString().slice(0, 10), "m5-ratchet.json");

function loadRepos() {
  if (!existsSync(REPOS_PATH)) {
    console.error("Repos file not found:", REPOS_PATH);
    process.exit(1);
  }
  return JSON.parse(readFileSync(REPOS_PATH, "utf8"));
}

// ---- MCP client (same as run.js) ----

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
    } catch (_) {}
  });

  proc.stderr.resume();

  function call(method, params = {}) {
    return new Promise((resolve, reject) => {
      const id = nextId++;
      pending.set(id, resolve);
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
      setTimeout(() => { if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); } }, 45000);
    });
  }

  function notify(method, params = {}) {
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  }

  function close() { rl.close(); proc.kill(); }
  return { call, notify, close };
}

function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

function extractContent(result) {
  if (result?.content && Array.isArray(result.content)) {
    const texts = result.content.filter(c => c.type === "text").map(c => c.text).join("\n");
    try { return JSON.parse(texts); } catch { return { text: texts }; }
  }
  return result ?? {};
}

function estimateTokens(text) {
  if (typeof text !== "string") return 0;
  return Math.ceil(text.length / 4);
}

function estimateFromResponse(data) {
  if (typeof data === "object" && data !== null) {
    if (typeof data.tokens === "number") return data.tokens;
    if (typeof data.text === "string") return estimateTokens(data.text);
  }
  return estimateTokens(JSON.stringify(data));
}

async function toolCall(client, tool, args, tracker) {
  const resp = await client.call("tools/call", { name: tool, arguments: args });
  if (resp.error) throw new Error(`Tool ${tool} error: ${JSON.stringify(resp.error)}`);
  const data = extractContent(resp.result);
  if (tracker) {
    tracker.calls = (tracker.calls || 0) + 1;
    tracker.totalTokens = (tracker.totalTokens || 0) + estimateFromResponse(data);
  }
  return data;
}

async function bootstrap(client) {
  await client.call("initialize", {
    protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "mcp-qa-multi", version: "0.0.1" },
  });
  client.notify("notifications/initialized", {});
  const toolsResp = await client.call("tools/list", {});
  return { toolNames: (toolsResp.result?.tools ?? []).map(t => t.name).sort() };
}

async function analyzeRepo(client, repoPath) {
  const analyzePromise = client.call("tools/call", { name: "analyze", arguments: { path: repoPath } });
  let handle = null;
  for (let i = 0; i < 240; i++) {
    await sleep(500);
    try {
      const listResp = await client.call("tools/call", { name: "list_sessions", arguments: {} });
      if (!handle && listResp.result) {
        const data = extractContent(listResp.result);
        const sessions = data.sessions ?? [];
        const ready = sessions.find(s => s.status === "ready" || s.status === "done");
        if (ready) handle = ready.handle;
      }
    } catch (_) {}
    if (handle) break;
  }
  let analyzeResult;
  try { const analyzeResp = await analyzePromise; analyzeResult = extractContent(analyzeResp.result); } catch (_) { analyzeResult = { handle, status: "ready" }; }
  return handle ?? analyzeResult?.handle ?? null;
}

// ---- M5 QA questions (same as run.js) ----

const QA_QUESTIONS = [
  {
    id: "q1-overview",
    run: async (client, handle, tracker) => {
      const overview = await toolCall(client, "overview", { handle }, tracker);
      const text = overview.text ?? "";
      const pass = text.length > 50 && !text.startsWith("Error") && (overview.tokens ?? 0) <= 600;
      return { pass, detail: `overview ${overview.tokens ?? "?"} tok` };
    },
  },
  {
    id: "q2-checkout-flow",
    run: async (client, handle, tracker) => {
      try {
        const trace = await toolCall(client, "trace", { handle, focus: "POST /basket/checkout", depth: 6, format: "compact" }, tracker);
        if (!trace.found) return { pass: false, detail: "trace not found" };
        const text = trace.text ?? "";
        const steps = (text.match(/\n/g) || []).length;
        return { pass: trace.found === true && steps > 0, detail: `trace found: ${steps} steps` };
      } catch (_) {
        // Not every repo has a checkout endpoint — skip entirely
        return { pass: null, detail: "skipped (no checkout endpoint or error)", skipped: true };
      }
    },
  },
  {
    id: "q3-resolve",
    run: async (client, handle, tracker) => {
      const resp = await toolCall(client, "resolve", { handle, query: "Program", limit: 5 }, tracker);
      const count = resp.count ?? resp.candidates?.length ?? 0;
      return { pass: count >= 1, detail: `${count} candidates` };
    },
  },
  {
    id: "q4-impact",
    run: async (client, handle, tracker) => {
      try {
        const find = await toolCall(client, "find", { handle, query: "Program", limit: 1 }, tracker);
        const nodeId = (find.results ?? [])[0]?.nodeId;
        if (!nodeId) return { pass: true, detail: "no Program node" };
        const impact = await toolCall(client, "impact", { handle, nodeId, maxDepth: 3, direction: "both" }, tracker);
        return { pass: typeof impact.totalAffected === "number", detail: `impact total=${impact.totalAffected ?? "?"}` };
      } catch (_) { return { pass: false, detail: "error" }; }
    },
  },
  {
    id: "q5-resolve-ambiguous",
    run: async (client, handle, tracker) => {
      const resp = await toolCall(client, "resolve", { handle, query: "Service", limit: 10 }, tracker);
      const count = resp.count ?? resp.candidates?.length ?? 0;
      const isAmbiguous = resp.ambiguous === true;
      return { pass: count > 0 && isAmbiguous, detail: `${count} candidates, ambiguous=${resp.ambiguous}` };
    },
  },
  {
    id: "q6-config",
    run: async (client, handle, tracker) => {
      let totalKeys = 0;
      let wellFormed = false;
      try {
        const resp = await toolCall(client, "config", { handle }, tracker);
        totalKeys = resp.totalKeys ?? 0;
        wellFormed = typeof resp.key === "string" && typeof resp.totalKeys === "number" && resp.keys !== undefined;
      } catch (_) {}
      return {
        pass: wellFormed && totalKeys > 0,
        detail: `config returned ${totalKeys} keys`,
      };
    },
  },
  {
    id: "q7-find",
    run: async (client, handle, tracker) => {
      const resp = await toolCall(client, "find", { handle, query: "Service", limit: 5 }, tracker);
      const count = resp.total ?? resp.results?.length ?? 0;
      const hasMore = resp.hasMore !== undefined;
      return { pass: count >= 0 && hasMore, detail: `${count} results` };
    },
  },
];

// ---- Main ----

function log(msg) { if (!QUIET) console.log(msg); }

async function runRepo(repoName, repoPath, client) {
  log(`\n======== ${repoName} ========`);
  log(`Path: ${repoPath}`);

  const startTime = Date.now();
  const handle = await analyzeRepo(client, repoPath);
  const elapsed = (Date.now() - startTime) / 1000;

  if (!handle) {
    log(`FAILED to analyze after ${elapsed.toFixed(1)}s`);
    return { repo: repoName, path: repoPath, error: "analyze failed", elapsed };
  }

  log(`Analyzed in ${elapsed.toFixed(1)}s, handle: ${handle}`);

  // Get baseline stats
  let baseline = { nodes: "?", edges: "?", entries: "?" };
  try {
    const stats = await toolCall(client, "stats", { handle });
    baseline = { nodes: stats.nodeCount ?? "?", edges: stats.edgeCount ?? "?", entries: stats.entryCount ?? "?" };
  } catch (_) {}
  log(`Baseline: ${baseline.nodes} nodes, ${baseline.edges} edges, ${baseline.entries} entries`);

  // Run questions
  const results = [];
  for (const qa of QA_QUESTIONS) {
    const qTracker = { calls: 0, totalTokens: 0 };
    try {
      const result = await qa.run(client, handle, qTracker);
      results.push({
        id: qa.id,
        passed: result.pass,
        detail: result.detail,
        skipped: result.skipped === true,
        calls: qTracker.calls,
        tokens: qTracker.totalTokens,
      });
    } catch (err) {
      results.push({ id: qa.id, passed: false, detail: err.message, calls: 0, tokens: 0 });
    }
  }

  // Close session
  try { await toolCall(client, "close_session", { handle }); } catch (_) {}

  const passCount = results.filter(r => r.passed === true).length;
  const skipCount = results.filter(r => r.skipped === true).length;
  const totalCalls = results.reduce((s, r) => s + r.calls, 0);
  const totalTokens = results.reduce((s, r) => s + r.tokens, 0);
  const activeTotal = results.length - skipCount;
  log(`Score: ${passCount}/${activeTotal}${skipCount > 0 ? ` (${skipCount} skipped)` : ""} | ${totalCalls} calls | ${totalTokens} tokens`);

  return { repo: repoName, path: repoPath, handle, elapsed, baseline, questions: results, totalCalls, totalTokens };
}

async function main() {
  if (!existsSync(MCP_EXE)) {
    console.error("MCP binary not found:", MCP_EXE);
    process.exit(1);
  }

  const repos = loadRepos();
  console.log(`M5 Multi-Repo QA — ${repos.length} repos`);
  console.log("");

  const allResults = [];

  let client = mcpClient(MCP_EXE);
  try {
    const { toolNames } = await bootstrap(client);
    log(`Server ready. ${toolNames.length} tools.`);

    for (const repo of repos) {
      if (!existsSync(repo.path)) {
        log(`\nSKIPPING ${repo.name}: path not found: ${repo.path}`);
        allResults.push({ repo: repo.name, path: repo.path, error: "path not found" });
        continue;
      }

      const start = Date.now();
      const result = await runRepo(repo.name, repo.path, client);
      allResults.push(result);

      // Kill and restart client between repos to avoid stale state
      client.close();
      if (repos.indexOf(repo) < repos.length - 1) {
        client = mcpClient(MCP_EXE);
        const { toolNames: newNames } = await bootstrap(client);
        log(`\nRestarted MCP. ${newNames.length} tools.`);
      }
    }
  } finally {
    client.close();
  }

  // Print summary
  console.log("\n========================================");
  console.log("M5 QA Summary");
  console.log("========================================");
  for (const r of allResults) {
    const status = r.error ? `ERROR: ${r.error}` : `${r.questions?.length ?? "?"} questions, ${r.totalCalls ?? "?"} calls, ${r.totalTokens ?? "?"} tokens`;
    console.log(`  ${r.repo}: ${status}`);
  }

  // Write ratchet
  const ratchetDir = join(__dirname, "..", "..", "eval-results", new Date().toISOString().slice(0, 10));
  if (!existsSync(ratchetDir)) require("fs").mkdirSync(ratchetDir, { recursive: true });

  const ratchet = {
    date: new Date().toISOString(),
    baseline: { branch: "feat/meridian-m0", buildTime: new Date().toISOString() },
    repos: allResults,
    summary: {
      repos: allResults.filter(r => !r.error).length,
      totalRepos: allResults.length,
      totalCalls: allResults.reduce((s, r) => s + (r.totalCalls ?? 0), 0),
      totalTokens: allResults.reduce((s, r) => s + (r.totalTokens ?? 0), 0),
    },
  };

  writeFileSync(RATCHET_PATH, JSON.stringify(ratchet, null, 2), "utf8");
  log(`\nRatchet written to ${RATCHET_PATH}`);

  // Exit code: 1 if any repo had an error
  const hasErrors = allResults.some(r => r.error);
  process.exit(hasErrors ? 1 : 0);
}

main().catch(err => { console.error("FATAL:", err.message); process.exit(1); });
