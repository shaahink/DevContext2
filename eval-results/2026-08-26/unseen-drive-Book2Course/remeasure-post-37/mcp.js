// Minimal MCP stdio driver. Usage: node mcp.js <calls.json> <out.txt>
// calls.json = [{ "name": "analyze", "arguments": {...} }, ...]
const { spawn } = require("child_process");
const { createInterface } = require("readline");
const { writeFileSync, readFileSync } = require("fs");

const CALLS = JSON.parse(readFileSync(process.argv[2], "utf8"));
const OUT = process.argv[3];
const MCP_EXE = "C:/Code/DevContext2/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe";
// #39 rule: the probe gets its own endpoint - it neither joins nor disturbs the shared default.

const proc = spawn(MCP_EXE, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true, env: { ...process.env, DEVCONTEXT_ENDPOINT: "http://127.0.0.1:5391" } });
const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
let nextId = 1;
const pending = new Map();
rl.on("line", (line) => {
  try {
    const msg = JSON.parse(line);
    if (msg.id !== undefined && pending.has(msg.id)) { pending.get(msg.id)(msg); pending.delete(msg.id); }
  } catch (_) {}
});
proc.stderr.resume();

function call(method, params = {}, timeoutMs = 900000) {
  return new Promise((resolve, reject) => {
    const id = nextId++;
    pending.set(id, resolve);
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    setTimeout(() => { if (pending.has(id)) { pending.delete(id); reject(new Error("Timeout: " + method)); } }, timeoutMs);
  });
}
const notify = (m, p = {}) => proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method: m, params: p }) + "\n");
function content(r) {
  const c = r?.result?.content;
  if (Array.isArray(c)) return c.filter(x => x.type === "text").map(x => x.text).join("\n");
  return JSON.stringify(r?.result ?? r?.error ?? r, null, 2);
}

const out = [];
const emit = (h, t) => {
  const tok = Math.ceil((t || "").length / 4);
  out.push(`\n===== ${h}  (~${tok} tok) =====\n${t}`);
  console.log(`${h}  ~${tok} tok`);
};

(async () => {
  await call("initialize", { protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "b2c-drive", version: "0" } });
  notify("notifications/initialized", {});
  const tools = await call("tools/list", {});
  const list = tools.result?.tools ?? [];
  emit("tools/list", list.map(t => `${t.name}: ${(t.description || "").slice(0, 110)}`).join("\n"));

  let handle = null;
  for (const c of CALLS) {
    const args = { ...c.arguments };
    if (handle && !args.handle && c.name !== "analyze") args.handle = handle;
    const t0 = Date.now();
    let text;
    try { text = content(await call("tools/call", { name: c.name, arguments: args })); }
    catch (e) { text = "ERROR: " + e.message; }
    const secs = ((Date.now() - t0) / 1000).toFixed(1);
    emit(`${c.label || c.name}  [${secs}s]  args=${JSON.stringify(args)}`, text);
    if (c.name === "analyze" && !handle) {
      try { handle = JSON.parse(text).handle ?? null; } catch (_) {
        const m = text.match(/"handle"\s*:\s*"([^"]+)"/); if (m) handle = m[1];
      }
      console.log("handle:", handle);
    }
  }
  writeFileSync(OUT, out.join("\n"), "utf8");
  proc.kill();
  process.exit(0);
})().catch(e => { console.error(e); writeFileSync(OUT, out.join("\n") + "\nFATAL: " + e.stack, "utf8"); proc.kill(); process.exit(1); });
