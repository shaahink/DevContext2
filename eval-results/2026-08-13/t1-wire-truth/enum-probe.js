// T1.3 / BUG-BACKLOG #10 spot probe — an out-of-range enum must be REJECTED, on the real wire.
// No analyze needed: every check below is rejected before ResolveHandle runs.
//   node eval-results/2026-08-13/t1-wire-truth/enum-probe.js
const { spawn } = require("child_process");
const { createInterface } = require("readline");
const { join } = require("path");
const { writeFileSync } = require("fs");

const EXE = join(__dirname, "..", "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");
const proc = spawn(EXE, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
let id = 1; const pending = new Map();
rl.on("line", (l) => { try { const m = JSON.parse(l); if (pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); } } catch (_) {} });
proc.stderr.resume();
const call = (method, params, ms = 60000) => new Promise((res, rej) => {
  const i = id++; const t = setTimeout(() => rej(new Error("timeout " + method)), ms);
  pending.set(i, (m) => { clearTimeout(t); res(m); });
  proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id: i, method, params }) + "\n");
});

const CASES = [
  ["read_source", { query: "X", mode: "full" }, "mode"],
  ["neighbors", { query: "X", direction: "sideways" }, "direction"],
  ["impact", { query: "X", direction: "sideways" }, "direction"],
  ["trace", { focus: "X", format: "verbose" }, "format"],
  ["get_context", { focus: "X", intent: "summarise" }, "intent"],
  // The valid values must still pass through to the real code path (they get past the enum guard
  // and fail on "no session", which is a DIFFERENT error - the guard is not swallowing them).
  ["read_source", { query: "X", mode: "member" }, null],
  ["impact", { query: "X", direction: "both" }, null],
];

(async () => {
  const init = await call("initialize", { protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "enum-probe", version: "0" } }, 180000);
  if (init.error) throw new Error(JSON.stringify(init.error));
  proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method: "notifications/initialized", params: {} }) + "\n");

  const out = []; let bad = 0;
  for (const [tool, args, param] of CASES) {
    const r = await call("tools/call", { name: tool, arguments: args });
    const text = (r.result?.content ?? []).map((c) => c.text ?? "").join("");
    let body = null; try { body = JSON.parse(text); } catch (_) {}
    const rejected = typeof body?.error === "string" && body.error.startsWith("Invalid ");
    const ok = param ? rejected : !rejected;
    if (!ok) bad++;
    out.push({ tool, args, expect: param ? `reject ${param}` : "pass the guard", ok, reply: body ?? text });
    console.log(`  ${ok ? "PASS" : "FAIL"}  ${tool}(${JSON.stringify(args)}) -> ${text.slice(0, 120)}`);
  }
  writeFileSync(join(__dirname, "enum-probe.json"), JSON.stringify(out, null, 2), "utf8");
  console.log(bad === 0 ? "\nenum-probe: GREEN" : `\nenum-probe: RED (${bad})`);
  rl.close(); proc.kill();
  process.exit(bad === 0 ? 0 : 1);
})().catch((e) => { console.error(e); proc.kill(); process.exit(1); });
