// E1.4 - the probe's class-C impact question, asked of the ENGINE instead of an agent.
//
// eval/agent-probe/questions/eShop.json#eshop-c1 is the question the pilot scored 0/3 on:
//   "I need to change the behaviour of CreateOrderCommandHandler in Ordering.API. What depends on
//    it, directly or indirectly, and what could break? List everything, including anything outside
//    Ordering.API."
// Its key names five things that MUST appear and one that MUST NOT. W2's whole acceptance is that
// the post-#11/#12/#8/#7 graph can now resolve that set. This driver asks the question over a REAL
// MCP session - the same surface the probe's agent had - and scores the answer against the sealed
// key, recording WHICH tool call produced each item so the answer is attributable, not asserted.
//
// It does not read eShop source to find the answer: every hit must come out of an MCP response.
//
// Usage:
//   node eval/mcp-qa/classc-impact.js [outDir] [repoPath]
// Defaults: outDir = eval-results/<today>/e1-batch/classc, repoPath = <repo>/eval-repos/eShop
//
// Exits non-zero if any key item is unreachable, so the result cannot be misread as a pass.

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync, readFileSync } = require("fs");

const { ENDPOINT, probeEnv, verifyServerIdentity } = require("./server-identity");

const REPO_ROOT = join(__dirname, "..", "..");
const OUT_DIR = resolve(process.argv[2]
  ?? join(REPO_ROOT, "eval-results", new Date().toISOString().slice(0, 10), "e1-batch", "classc"));
const REPO_PATH = resolve(process.argv[3] ?? join(REPO_ROOT, "eval-repos", "eShop"));
const MCP_EXE = join(REPO_ROOT, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");
const KEY_FILE = join(REPO_ROOT, "eval", "agent-probe", "questions", "eShop.json");

const SUBJECT = "CreateOrderCommandHandler";

function mcpClient(exePath) {
  const proc = spawn(exePath, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true, env: probeEnv() });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  let nextId = 1;
  const pending = new Map();
  rl.on("line", (line) => {
    try {
      const msg = JSON.parse(line);
      if (msg.id !== undefined && pending.has(msg.id)) {
        const w = pending.get(msg.id);
        clearTimeout(w.timer);
        w.resolve(msg);
        pending.delete(msg.id);
      }
    } catch (_) { /* non-JSON line */ }
  });
  proc.stderr.resume();
  function call(method, params = {}, timeoutMs = 120000) {
    return new Promise((res, rej) => {
      const id = nextId++;
      const timer = setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); rej(new Error(`Timeout: ${method}`)); }
      }, timeoutMs);
      pending.set(id, { resolve: res, timer });
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    });
  }
  return {
    call,
    notify: (m, p = {}) => proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method: m, params: p }) + "\n"),
    // Close stdin, do not kill: proc.kill() orphans the DevContext.Server the MCP spawned and it
    // then holds DevContext.Core.dll against the next build (measured in T1.3).
    close: async () => {
      rl.close();
      proc.stdin.end();
      await new Promise((res) => {
        const t = setTimeout(res, 5000);
        proc.once("exit", () => { clearTimeout(t); res(); });
      });
      if (proc.exitCode === null) proc.kill();
    },
  };
}

const transcript = [];
async function tool(client, name, args, timeoutMs = 600000) {
  const raw = await client.call("tools/call", { name, arguments: args }, timeoutMs);
  const text = (raw.result?.content ?? []).map((c) => c.text ?? "").join("\n");
  let body = null;
  try { body = JSON.parse(text); } catch { /* plain text */ }
  transcript.push({ step: transcript.length + 1, tool: name, arguments: args, text, body });
  return { text, body, step: transcript.length };
}

function out(name, obj) {
  if (!existsSync(OUT_DIR)) mkdirSync(OUT_DIR, { recursive: true });
  const p = join(OUT_DIR, name);
  writeFileSync(p, typeof obj === "string" ? obj : JSON.stringify(obj, null, 2), "utf8");
  console.log(`  wrote ${p}`);
  return p;
}

// A key item counts as RESOLVED only when it appears in a response that is ANSWERING the impact
// question - impact / usages / tests_for / trace / neighbors / seam - not merely in a name search.
const ANSWERING = new Set(["impact", "usages", "tests_for", "trace", "neighbors", "seam", "get_context"]);

function hitsFor(needle) {
  const hits = [];
  for (const t of transcript) {
    if (!ANSWERING.has(t.tool)) continue;
    if ((t.text ?? "").includes(needle)) hits.push({ step: t.step, tool: t.tool, arguments: t.arguments });
  }
  return hits;
}

(async () => {
  if (!existsSync(MCP_EXE)) {
    console.error(`MCP exe not found: ${MCP_EXE}\nBuild it: dotnet build src/DevContext.Mcp`);
    process.exit(2);
  }
  const key = JSON.parse(readFileSync(KEY_FILE, "utf8"));
  const q = (Array.isArray(key) ? key : key.questions ?? []).find((x) => x.id === "eshop-c1");
  if (!q) { console.error("eshop-c1 not found in the question key"); process.exit(2); }

  console.log(`classc-impact\n  mcp:  ${MCP_EXE}\n  repo: ${REPO_PATH}\n  out:  ${OUT_DIR}\n`);
  const client = mcpClient(MCP_EXE);
  let identity = null;
  try {
    await client.call("initialize", {
      protocolVersion: "2024-11-05", capabilities: {},
      clientInfo: { name: "classc-impact", version: "1" },
    });
    client.notify("notifications/initialized");

    const analyzed = await tool(client, "analyze", { path: REPO_PATH });
    const handle = analyzed.body?.handle ?? analyzed.body?.sessionId;
    if (!handle) throw new Error(`analyze gave no handle: ${analyzed.text.slice(0, 300)}`);
    console.log(`  handle: ${handle}`);

    // WHICH ENGINE ANSWERED - an unattributed measurement is worse than none (T1.4).
    identity = await verifyServerIdentity(ENDPOINT, resolve(REPO_ROOT));
    console.log(`  identity: ${identity.ok ? "OK" : "FAIL"} - ${identity.detail ?? ""}`);

    // 1. Address the subject the way an agent would.
    const found = await tool(client, "find", { handle, query: SUBJECT, limit: 10 });
    const nodeId = found.body?.results?.[0]?.nodeId ?? found.body?.matches?.[0]?.nodeId ?? null;
    console.log(`  subject nodeId: ${nodeId ?? "(none - falling back to fuzzy query)"}`);
    const addr = nodeId ? { nodeId } : { query: SUBJECT };

    // 2. The impact question itself, all three directions.
    await tool(client, "impact", { handle, ...addr, direction: "up", maxDepth: 4 });
    await tool(client, "impact", { handle, ...addr, direction: "down", maxDepth: 4 });
    await tool(client, "impact", { handle, ...addr, direction: "both", maxDepth: 4 });

    // 3. The reverse-navigation suite an agent reaches for next.
    await tool(client, "usages", { handle, ...addr });
    await tool(client, "tests_for", { handle, ...addr, maxDepth: 6 });
    await tool(client, "neighbors", { handle, ...addr, direction: "in" });
    await tool(client, "neighbors", { handle, ...addr, direction: "out" });
    await tool(client, "trace", { handle, focus: SUBJECT, format: "compact" });

    // 4. The cross-service hop the key says is the one that is easy to miss: the handler emits
    //    OrderStartedIntegrationEvent and Basket.API subscribes. Ask the graph to carry it.
    await tool(client, "impact", { handle, query: "OrderStartedIntegrationEvent", direction: "down", maxDepth: 4 });
    await tool(client, "usages", { handle, query: "OrderStartedIntegrationEvent" });

    out("transcript.json", { measuredAt: new Date().toISOString(), mcpExe: MCP_EXE, repo: REPO_PATH, handle, identity, transcript });

    // 5. Score against the sealed key. Presence only counts inside an ANSWERING response.
    const scored = { question: q.id, prompt: q.prompt, mustMention: [], mustNotMention: [] };
    let failed = 0;
    for (const m of q.mustMention) {
      const hits = hitsFor(m);
      const ok = hits.length > 0;
      if (!ok) failed++;
      scored.mustMention.push({ name: m, resolved: ok, hops: hits.slice(0, 6) });
      console.log(`  ${ok ? "RESOLVED " : "MISSING  "} ${m}${ok ? `  <- ${hits.map((h) => `${h.tool}#${h.step}`).slice(0, 4).join(", ")}` : ""}`);
    }
    for (const m of q.mustNotMention) {
      const hits = hitsFor(m);
      const ok = hits.length === 0;
      if (!ok) failed++;
      scored.mustNotMention.push({ name: m, absent: ok, hops: hits.slice(0, 6) });
      console.log(`  ${ok ? "ABSENT   " : "FALSE-POS"} ${m}${ok ? "" : `  <- ${hits.map((h) => `${h.tool}#${h.step}`).slice(0, 4).join(", ")}`}`);
    }
    scored.identityOk = identity?.ok ?? false;
    scored.verdict = failed === 0 && identity?.ok ? "PASS" : "FAIL";
    out("score.json", scored);
    console.log(`\n  VERDICT: ${scored.verdict} (${failed} key item(s) wrong)`);
    if (scored.verdict !== "PASS") process.exitCode = 1;
  } catch (e) {
    console.error(`classc-impact ERROR: ${e.message}`);
    if (transcript.length) out("transcript.json", { error: e.message, transcript });
    process.exitCode = 2;
  } finally {
    await client.close();
  }
})();
