// MCP blind-drive: realistic agent session against an unseen repo, full transcript out.
// Usage: node mcp-drive.js <repoPath> <transcriptOut>
const { spawn } = require("child_process");
const { createInterface } = require("readline");
const { writeFileSync } = require("fs");

const REPO = process.argv[2];
const OUT = process.argv[3];
const MCP_EXE = "C:/code/DevContext2/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe";

const proc = spawn(MCP_EXE, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
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

function call(method, params = {}, timeoutMs = 300000) {
  return new Promise((resolve, reject) => {
    const id = nextId++;
    pending.set(id, resolve);
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    setTimeout(() => {
      if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); }
    }, timeoutMs);
  });
}
function notify(method, params = {}) {
  proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
}
function content(resp) {
  const c = resp?.result?.content;
  if (Array.isArray(c)) return c.filter(x => x.type === "text").map(x => x.text).join("\n");
  return JSON.stringify(resp?.result ?? resp?.error ?? resp);
}

const transcript = [];
function log(step, text) {
  const tok = Math.ceil((text || "").length / 4);
  transcript.push(`\n===== ${step}  (~${tok} tokens) =====\n${text}`);
  console.log(`${step}: ~${tok} tokens`);
}

(async () => {
  await call("initialize", { protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "lens-audit", version: "0" } });
  notify("notifications/initialized", {});

  const tools = await call("tools/list", {});
  const names = (tools.result?.tools ?? []).map(t => t.name).sort();
  log("tools/list", names.join(", "));

  const t0 = Date.now();
  const a = await call("tools/call", { name: "analyze", arguments: { path: REPO } });
  const aText = content(a);
  log(`analyze  [${((Date.now() - t0) / 1000).toFixed(1)}s]`, aText);
  let handle = null;
  try { handle = JSON.parse(aText).handle ?? null; } catch (_) {
    const m = aText.match(/"handle"\s*:\s*"([^"]+)"/) || aText.match(/handle[:=\s]+(\S+)/);
    if (m) handle = m[1];
  }
  console.log("handle:", handle);

  const steps = [
    ["overview", {}],
    ["map", {}],
    ["entrypoints", { kind: "HttpEndpoint" }],
    ["entrypoints", { full: true }],
    ["resolve", { query: "ListenTogetherHub" }],
    ["find", { query: "podcast", limit: 5 }],
    ["node", { query: "PodcastService" }],
    ["neighbors", { query: "PodcastService", direction: "out" }],
    ["usages", { query: "ShowClient" }],
    ["read_source", { query: "ListenTogetherHub", mode: "window", windowLines: 20 }],
    ["flow", { query: "GET /listen-together" }],
    ["trace", { query: "POST /", budgetTokens: 2000 }],
    ["impact", { query: "PodcastService", direction: "up" }],
    ["tests_for", { query: "FeedsApi" }],
    ["config", {}],
    ["get_context", { query: "GET /listen-together", budgetTokens: 6000 }],
    ["verify_context", { query: "GET /listen-together" }],
    ["top_flows", {}],
    ["interesting_points", {}],
    ["insights", {}],
    ["stats", {}],
  ];

  for (const [name, args] of steps) {
    const full = handle ? { handle, ...args } : args;
    const t = Date.now();
    try {
      const r = await call("tools/call", { name, arguments: full });
      log(`${name} ${JSON.stringify(args)}  [${((Date.now() - t) / 1000).toFixed(1)}s]`, content(r));
    } catch (e) {
      log(`${name} ${JSON.stringify(args)}`, `ERROR: ${e.message}`);
    }
  }

  await call("tools/call", { name: "close_session", arguments: { handle } }).catch(() => {});
  writeFileSync(OUT, transcript.join("\n"), "utf8");
  console.log("TRANSCRIPT-WRITTEN");
  proc.kill();
  process.exit(0);
})().catch(e => { writeFileSync(OUT, transcript.join("\n") + "\nFATAL: " + e.message, "utf8"); proc.kill(); process.exit(1); });
