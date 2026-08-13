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
//   <outDir>/enum-dials.json      every out-of-range enum call and the envelope it got back
// Prints PASS/FAIL lines and exits non-zero on any FAIL, so it can be lifted into the battery.

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync } = require("fs");

const { ENDPOINT, probeEnv, verifyServerIdentity } = require("./server-identity");

const REPO_ROOT = join(__dirname, "..", "..");
const OUT_DIR = resolve(process.argv[2]
  ?? join(REPO_ROOT, "eval-results", new Date().toISOString().slice(0, 10), "t1-wire-truth"));
const MCP_EXE = join(REPO_ROOT, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

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
  // Close stdin FIRST and give the host a moment: proc.kill() alone orphans the DevContext.Server
  // this MCP spawned, and that orphan holds DevContext.Core.dll — the next `dotnet build` then dies
  // MSB3027. Measured twice in T1.3; the kill is only the backstop for a host that ignores EOF.
  async function close() {
    rl.close();
    proc.stdin.end();
    await new Promise((res) => {
      const t = setTimeout(res, 5000);
      proc.once("exit", () => { clearTimeout(t); res(); });
    });
    if (proc.exitCode === null) proc.kill();
  }
  return { call, notify, close };
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

    // Before any verdict: WHICH engine answered? (see server-identity.js — a probe served by
    // another checkout's build reported this repo's fixes as still broken.)
    const identity = await verifyServerIdentity(ENDPOINT, REPO_ROOT);
    dump("server-identity.json", { endpoint: ENDPOINT, ...identity });
    check("the server answering is THIS repo's fresh build", identity.ok, identity.detail);

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

    // T1.2 — curation must not be a capability cut. An UNLISTED specialist has to answer for real,
    // and an actually-unknown name has to teach. list_sessions needs no analysis, so both checks
    // are cheap enough to live in the gate. Read the reply, not the exit code: the whole defect
    // family here is a confident-looking envelope where an answer should be.
    const spec = await client.call("tools/call",
      { name: "list_sessions", arguments: {} }, 60000);
    const specText = (spec.result?.content ?? []).map((c) => c.text ?? "").join("\n");
    let specBody = null;
    try { specBody = JSON.parse(specText); } catch { /* not JSON */ }
    dump("specialist-list_sessions.json", { protocolError: spec.error ?? null, result: spec.result ?? null });
    const listedNames = tools.map((t) => t.name);
    check("an unlisted specialist is still callable and answers for real",
      !spec.error && spec.result?.isError !== true && specBody !== null && !("availableTools" in specBody),
      `list_sessions ${listedNames.includes("list_sessions") ? "(listed)" : "(unlisted)"} -> ${specText.slice(0, 90)}`);

    const unknown = await client.call("tools/call",
      { name: "no_such_tool_xyz", arguments: {} }, 30000);
    const unkText = (unknown.result?.content ?? []).map((c) => c.text ?? "").join("\n");
    let unkBody = null;
    try { unkBody = JSON.parse(unkText); } catch { /* not JSON */ }
    dump("unknown-tool-envelope.json", unknown.result ?? { protocolError: unknown.error });
    const advertised = unkBody?.availableTools ?? [];
    check("an unknown name gets the did-you-mean envelope",
      unkBody !== null && typeof unkBody.error === "string" && Array.isArray(unkBody.availableTools),
      unkText.slice(0, 90));
    check("the envelope's availableTools equals the advertised menu",
      advertised.length === listedNames.length
      && [...advertised].sort().join(",") === [...listedNames].sort().join(","),
      `${advertised.length} vs ${listedNames.length}`);
    check("the envelope names the unlisted specialists, each with what it answers",
      unkBody?.specialistTools && Object.keys(unkBody.specialistTools).length > 0
      && Object.values(unkBody.specialistTools).every((v) => typeof v === "string" && v.length > 0),
      Object.keys(unkBody?.specialistTools ?? {}).join(" ") || "none named");
    check("no specialist is also on the advertised menu",
      !Object.keys(unkBody?.specialistTools ?? {}).some((n) => listedNames.includes(n)),
      "menu and specialists are disjoint");

    // T1.3 #10 — the enum dials. Folded in here from the T1.3 spot probe (which lived under
    // eval-results/, where no gate could ever reach it) because the T1.4 bar is "invalid
    // mode/direction/format rejected" and one of those three was never gated. Costs nothing:
    // every case below is answered BEFORE ResolveHandle, so no analyze is needed.
    // The two null-param cases are the control: a valid value must still reach the real code path
    // (it fails on "no session", a DIFFERENT error) — otherwise a guard that rejects everything
    // would score as a pass.
    const ENUM_CASES = [
      ["read_source", { query: "X", mode: "full" }, "mode"],
      ["neighbors", { query: "X", direction: "sideways" }, "direction"],
      ["impact", { query: "X", direction: "sideways" }, "direction"],
      ["trace", { focus: "X", format: "verbose" }, "format"],
      ["get_context", { focus: "X", intent: "summarise" }, "intent"],
      ["read_source", { query: "X", mode: "member" }, null],
      ["impact", { query: "X", direction: "both" }, null],
    ];
    const enumRows = [];
    for (const [name, args, param] of ENUM_CASES) {
      const r = await client.call("tools/call", { name, arguments: args }, 60000);
      const text = (r.result?.content ?? []).map((c) => c.text ?? "").join("");
      let body = null;
      try { body = JSON.parse(text); } catch { /* plain text reply */ }
      const rejected = typeof body?.error === "string" && body.error.startsWith("Invalid ");
      enumRows.push({
        tool: name, arguments: args, dial: param,
        expect: param ? `reject ${param}` : "pass the guard",
        ok: param ? rejected : !rejected, reply: body ?? text,
      });
    }
    dump("enum-dials.json", enumRows);
    const enumBad = enumRows.filter((r) => !r.ok);
    check("every out-of-range enum dial is rejected, not silently re-read",
      enumBad.filter((r) => r.dial).length === 0,
      enumBad.filter((r) => r.dial).map((r) => `${r.tool}.${r.dial}=${r.arguments[r.dial]}`).join(" | ")
        || `${enumRows.filter((r) => r.dial).length} dials reject`);
    check("a VALID enum value still reaches the real code path (the guard is not a blanket reject)",
      enumBad.filter((r) => !r.dial).length === 0,
      enumBad.filter((r) => !r.dial).map((r) => `${r.tool} ${JSON.stringify(r.arguments)}`).join(" | ")
        || "valid values pass the guard");

    measurement.advertised = listedNames;
    measurement.specialists = unkBody?.specialistTools ?? null;
    measurement.enumDials = enumRows;
    dump("wire-truth.json", measurement);
  } finally {
    await client.close();
  }
  console.log(failed === 0 ? "\nwire-truth: GREEN" : `\nwire-truth: RED (${failed} failed)`);
})().catch((e) => { console.error(e); process.exit(1); });
