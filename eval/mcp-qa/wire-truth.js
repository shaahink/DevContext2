// Wire truth — what an agent ACTUALLY receives from `tools/list`, measured off a real MCP
// handshake over stdio. Nothing here reads the C# source: the whole point is that the source
// carried 26 XML doc summaries for a year while the wire carried 22 empty strings (BUG-BACKLOG #5).
//
// Usage:
//   node eval/mcp-qa/wire-truth.js [outDir]
//
// Writes:
//   <outDir>/tools-list.json      the RAW tools/list result, verbatim
//   <outDir>/wire-truth.json      the derived measurement (per-tool description + schema sizes)
// Prints PASS/FAIL lines and exits non-zero on any FAIL, so it can be lifted into the battery.

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync } = require("fs");

const REPO_ROOT = join(__dirname, "..", "..");
const OUT_DIR = resolve(process.argv[2]
  ?? join(REPO_ROOT, "eval-results", new Date().toISOString().slice(0, 10), "t1-wire-truth"));
const MCP_EXE = join(REPO_ROOT, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

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
        clearTimeout(w.timer);
        w.resolve(msg);
        pending.delete(msg.id);
      }
    } catch (_) { /* non-JSON line */ }
  });
  proc.stderr.resume();
  function call(method, params = {}, timeoutMs = 45000) {
    return new Promise((res, rej) => {
      const id = nextId++;
      const timer = setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); rej(new Error(`Timeout: ${method}`)); }
      }, timeoutMs);
      pending.set(id, { resolve: res, timer });
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    });
  }
  function notify(method, params = {}) {
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  }
  return { call, notify, close: () => { rl.close(); proc.kill(); } };
}

function dump(name, obj) {
  if (!existsSync(OUT_DIR)) mkdirSync(OUT_DIR, { recursive: true });
  const p = join(OUT_DIR, name);
  writeFileSync(p, JSON.stringify(obj, null, 2), "utf8");
  console.log(`  wrote ${p}`);
  return p;
}

let failed = 0;
function check(label, ok, detail = "") {
  console.log(`  ${ok ? "PASS" : "FAIL"}  ${label}${detail ? " - " + detail : ""}`);
  if (!ok) { failed++; process.exitCode = 1; }
}

// The menu costs what the JSON costs. chars/4 is the usual English-token approximation and is only
// used for an order-of-magnitude read; the billed number is the probe's measure-tax.mjs delta.
function sizeOf(obj) {
  const chars = JSON.stringify(obj).length;
  return { chars, approxTokens: Math.round(chars / 4) };
}

(async () => {
  if (!existsSync(MCP_EXE)) {
    console.error(`MCP exe not found: ${MCP_EXE}\nBuild it: dotnet build src/DevContext.Mcp`);
    process.exit(2);
  }
  console.log(`wire-truth: ${MCP_EXE}`);
  const client = mcpClient(MCP_EXE);
  try {
    const init = await client.call("initialize", {
      protocolVersion: "2024-11-05", capabilities: {},
      clientInfo: { name: "wire-truth", version: "0.0.1" },
    }, 180000);
    if (init.error) throw new Error(`init failed: ${JSON.stringify(init.error)}`);
    client.notify("notifications/initialized", {});

    const listed = await client.call("tools/list", {}, 60000);
    if (listed.error) throw new Error(`tools/list failed: ${JSON.stringify(listed.error)}`);
    const tools = listed.result?.tools ?? [];
    dump("tools-list.json", listed.result);

    const rows = tools.map((t) => {
      const props = t.inputSchema?.properties ?? {};
      const paramNames = Object.keys(props);
      const described = paramNames.filter((p) => (props[p]?.description ?? "").trim().length > 0);
      return {
        name: t.name,
        descriptionChars: (t.description ?? "").length,
        description: t.description ?? "",
        params: paramNames.length,
        paramsDescribed: described.length,
        paramsUndescribed: paramNames.filter((p) => !described.includes(p)),
        schemaChars: JSON.stringify(t).length,
      };
    });

    const undescribedTools = rows.filter((r) => r.description.trim().length === 0).map((r) => r.name);
    const toolsWithUndescribedParams = rows.filter((r) => r.paramsUndescribed.length > 0)
      .map((r) => `${r.name}(${r.paramsUndescribed.join(",")})`);
    const total = sizeOf(listed.result);
    const totalParams = rows.reduce((a, r) => a + r.params, 0);
    const describedParams = rows.reduce((a, r) => a + r.paramsDescribed, 0);

    const measurement = {
      measuredAt: new Date().toISOString(),
      mcpExe: MCP_EXE,
      toolCount: tools.length,
      toolsWithEmptyDescription: undescribedTools.length,
      undescribedTools,
      paramCount: totalParams,
      paramsWithDescription: describedParams,
      toolsWithUndescribedParams,
      payload: total,
      perToolChars: Math.round(total.chars / Math.max(1, tools.length)),
      tools: rows,
    };
    dump("wire-truth.json", measurement);

    console.log("");
    console.log(`  tools: ${tools.length} | described: ${tools.length - undescribedTools.length}`
      + ` | params: ${describedParams}/${totalParams} described`);
    console.log(`  tools/list payload: ${total.chars} chars (~${total.approxTokens} tokens),`
      + ` ${measurement.perToolChars} chars/tool`);
    console.log("");

    check("every tool on the wire has a non-empty description",
      undescribedTools.length === 0, undescribedTools.join(" ") || "all described");
    check("every tool parameter on the wire has a description",
      toolsWithUndescribedParams.length === 0, toolsWithUndescribedParams.join(" ") || "all described");
    check("tools/list is non-empty", tools.length > 0, `${tools.length} tools`);
  } finally {
    client.close();
  }
  console.log(failed === 0 ? "\nwire-truth: GREEN" : `\nwire-truth: RED (${failed} failed)`);
})().catch((e) => { console.error(e); process.exit(1); });
