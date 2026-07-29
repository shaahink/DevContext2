// R4 §2 Task 1 — the dogfood driver. NOT a pass/fail case runner (that is drive-r4.js): this is a
// transport for a human/agent driving the MCP adaptively, one batch of calls at a time, with every
// call logged mechanically so the grades in the report can be re-checked against what the tool
// actually returned.
//
// Usage: node eval/mcp-qa/dogfood.js <batch.json>
//   batch.json = { "outDir": "<abs>", "calls": [ {tool, args, preview?, previewPath?, timeoutMs?, why?} ] }
//   tool "tools/list" is special-cased to the JSON-RPC method of the same name.
//   Any string arg equal to "$HANDLE" is replaced by the handle recorded by the last analyze
//   (persisted in <outDir>/.handle, so it survives across driver invocations).
//
// Per call it: appends one line to <outDir>/call-log.jsonl (seq, tool, args, ms, chars, tokens),
// writes the full response to <outDir>/raw/NNN-<tool>.json, and prints a bounded preview. The
// LOGGED token count is always the FULL response — the preview bound is what enters the driving
// agent's context, never what the cost is reported as.

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync, appendFileSync, readFileSync } = require("fs");

const BATCH_FILE = process.argv[2];
if (!BATCH_FILE) { console.error("usage: node dogfood.js <batch.json>"); process.exit(2); }
const batch = JSON.parse(readFileSync(BATCH_FILE, "utf8"));
const OUT_DIR = resolve(batch.outDir);
const RAW_DIR = join(OUT_DIR, "raw");
const LOG = join(OUT_DIR, "call-log.jsonl");
const HANDLE_FILE = join(OUT_DIR, ".handle");
const MCP_EXE = join(__dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

for (const d of [OUT_DIR, RAW_DIR]) if (!existsSync(d)) mkdirSync(d, { recursive: true });

let handle = existsSync(HANDLE_FILE) ? readFileSync(HANDLE_FILE, "utf8").trim() : null;
let seq = existsSync(LOG) ? readFileSync(LOG, "utf8").split("\n").filter(Boolean).length : 0;

function mcpClient(exePath) {
  const proc = spawn(exePath, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  let nextId = 1;
  const pending = new Map();
  rl.on("line", (line) => {
    try {
      const msg = JSON.parse(line);
      if (msg.id !== undefined && pending.has(msg.id)) {
        const w = pending.get(msg.id);
        clearTimeout(w.timer);           // an uncleared 10-minute timer holds node's loop open
        w.resolve(msg);
        pending.delete(msg.id);
      }
    } catch (_) { /* non-JSON line */ }
  });
  proc.stderr.resume();
  function call(method, params = {}, timeoutMs = 60000) {
    return new Promise((res, rej) => {
      const id = nextId++;
      const timer = setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); rej(new Error(`Timeout: ${method} after ${timeoutMs}ms`)); }
      }, timeoutMs);
      pending.set(id, { resolve: res, timer });
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    });
  }
  return {
    call,
    notify: (m, p = {}) => proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method: m, params: p }) + "\n"),
    close: () => { rl.close(); proc.kill(); },
  };
}

function subst(v) {
  if (typeof v === "string") return v === "$HANDLE" ? handle : v;
  if (Array.isArray(v)) return v.map(subst);
  if (v && typeof v === "object") return Object.fromEntries(Object.entries(v).map(([k, x]) => [k, subst(x)]));
  return v;
}

function pick(obj, path) {
  return path.split(".").reduce((o, k) => (o == null ? o : o[k]), obj);
}

(async () => {
  if (!existsSync(MCP_EXE)) { console.error(`MCP binary missing: ${MCP_EXE}`); process.exit(2); }
  const client = mcpClient(MCP_EXE);
  try {
    const init = await client.call("initialize", {
      protocolVersion: "2024-11-05", capabilities: {},
      clientInfo: { name: "dogfood-g4", version: "1" },
    }, 180000);
    if (init.error) throw new Error(`init failed: ${JSON.stringify(init.error)}`);
    client.notify("notifications/initialized", {});

    for (const c of batch.calls) {
      const args = subst(c.args ?? {});
      const t0 = Date.now();
      let text, parsed, err = null;
      try {
        const resp = c.tool === "tools/list"
          ? await client.call("tools/list", {}, c.timeoutMs ?? 60000)
          : await client.call("tools/call", { name: c.tool, arguments: args }, c.timeoutMs ?? 60000);
        if (resp.error) { err = JSON.stringify(resp.error); text = err; }
        else if (c.tool === "tools/list") { text = JSON.stringify(resp.result, null, 2); }
        else {
          text = (resp.result?.content ?? []).filter((x) => x.type === "text").map((x) => x.text).join("\n");
          if (resp.result?.isError) err = "isError";
        }
      } catch (e) { err = e.message; text = `ERROR: ${e.message}`; }
      const ms = Date.now() - t0;
      try { parsed = JSON.parse(text); } catch { parsed = null; }

      seq += 1;
      const id = String(seq).padStart(3, "0");
      const rawPath = join(RAW_DIR, `${id}-${c.tool.replace(/\W/g, "_")}.json`);
      writeFileSync(rawPath, text, "utf8");
      const chars = text.length;
      const tokens = Math.ceil(chars / 4);
      appendFileSync(LOG, JSON.stringify({
        seq, tool: c.tool, args, why: c.why ?? null, ms, chars, tokens,
        error: err, raw: `raw/${id}-${c.tool.replace(/\W/g, "_")}.json`,
      }) + "\n", "utf8");

      if (parsed?.handle && c.tool === "analyze") {
        handle = parsed.handle;
        writeFileSync(HANDLE_FILE, handle, "utf8");
      }

      const cap = c.preview ?? 1500;
      let shown = text;
      if (c.previewPath && parsed) {
        const v = pick(parsed, c.previewPath);
        shown = typeof v === "string" ? v : JSON.stringify(v, null, 2);
        shown = shown ?? `(no such path: ${c.previewPath})`;
      }
      console.log(`\n===== #${seq} ${c.tool} ${JSON.stringify(args).slice(0, 200)} `
        + `| ${(ms / 1000).toFixed(1)}s | ${tokens} tok | ${rawPath}${err ? " | ERR " + err : ""} =====`);
      console.log(shown.slice(0, cap) + (shown.length > cap ? `\n... [+${shown.length - cap} chars, see raw]` : ""));
    }
  } finally {
    client.close();
  }
  process.exit(0);
})().catch((e) => { console.error("FATAL:", e.message); process.exit(1); });
