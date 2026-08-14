// Deep-link truth probe (N4.3) — does a feed row carry what the desktop needs to FOLLOW it?
//
// The MCP page's rows are navigable now: a trace row opens its subject in Explore, a get_context
// row replays its pack in Studio. Both decisions are made client-side off two things the server
// puts on ToolCallEvent — `tool` (the gRPC method that ran) and `primary_arg` (the one argument
// that says what the call was about, stamped by the MCP sidecar's tool layer). If either is empty
// or spelled differently than the app expects, the row silently loses its link: nothing errors,
// nothing logs, the button is just not there. That is exactly the failure mode this program keeps
// paying for, so it is measured off the REAL wire rather than argued from the diff.
//
// What it does: spawns devcontext-mcp over stdio, analyzes a repo, subscribes to ObserveToolCalls
// over gRPC-web (the same transport the desktop uses), then calls `trace` and `get_context` as an
// agent would and reads back the events the server streamed.
//
// Usage:
//   node eval/mcp-qa/deep-link-truth.js [outDir] [repoPath]
// Defaults: outDir = eval-results/<today>/n4.3-deep-link-truth, repoPath = <repo>/eval-repos/TodoApi
//
// Prints PASS/FAIL and exits non-zero on any FAIL.

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync } = require("fs");

const { ENDPOINT, probeEnv, verifyServerIdentity } = require("./server-identity");

const REPO_ROOT = join(__dirname, "..", "..");
const OUT_DIR = resolve(process.argv[2]
  ?? join(REPO_ROOT, "eval-results", new Date().toISOString().slice(0, 10), "n4.3-deep-link-truth"));
const REPO_PATH = resolve(process.argv[3] ?? join(REPO_ROOT, "eval-repos", "TodoApi"));
const MCP_EXE = join(REPO_ROOT, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

// The two RPC names the desktop routes on (mcp-page.ts ROW_ROUTES, read off the generated service
// descriptor). Spelled here as literals ON PURPOSE: this probe's job is to catch the day the wire
// stops agreeing with the app, and a probe that derived the name from the same source could not.
const EXPLORE_RPC = "GetTrace";
const STUDIO_RPC = "GetContext";

// ---------------------------------------------------------------------------
// Minimal protobuf + gRPC-web reading. Enough to read ToolCallEvent off the wire
// without pulling a codegen toolchain into a probe.
// ---------------------------------------------------------------------------

function varint(buf, pos) {
  let result = 0, shift = 0, p = pos;
  for (;;) {
    const b = buf[p++];
    result += (b & 0x7f) * Math.pow(2, shift);
    if ((b & 0x80) === 0) break;
    shift += 7;
  }
  return [result, p];
}

/** Field number -> raw bytes (length-delimited fields only; the rest are skipped). */
function fields(buf) {
  const out = {};
  let p = 0;
  while (p < buf.length) {
    const [key, afterKey] = varint(buf, p);
    const field = key >>> 3, wire = key & 7;
    p = afterKey;
    if (wire === 0) { [, p] = varint(buf, p); }
    else if (wire === 1) { p += 8; }
    else if (wire === 5) { p += 4; }
    else if (wire === 2) {
      const [len, afterLen] = varint(buf, p);
      (out[field] ??= []).push(buf.subarray(afterLen, afterLen + len));
      p = afterLen + len;
    } else throw new Error(`unsupported wire type ${wire}`);
  }
  return out;
}

const str = (bytes) => (bytes ? Buffer.from(bytes).toString("utf8") : "");
const first = (f, n) => (f[n] ? str(f[n][0]) : "");

/** ToolCallEvent, in the fields this probe judges. */
function toolCallEvent(buf) {
  const f = fields(buf);
  return {
    sessionHandle: first(f, 1),
    rpc: first(f, 2),
    argsDigest: first(f, 3),
    sessionRepo: first(f, 8),
    origin: first(f, 9),
    mcpTool: first(f, 10),
    primaryArg: first(f, 11),
  };
}

/** One gRPC-web request frame: flags byte + big-endian length + body. */
function frame(body = Buffer.alloc(0)) {
  const head = Buffer.alloc(5);
  head.writeUInt8(0, 0);
  head.writeUInt32BE(body.length, 1);
  return Buffer.concat([head, body]);
}

/** Splits a gRPC-web byte stream into DATA frames (flag 0), ignoring the trailer frame (0x80). */
function frameSplitter(onMessage) {
  let buf = Buffer.alloc(0);
  return (chunk) => {
    buf = Buffer.concat([buf, Buffer.from(chunk)]);
    for (;;) {
      if (buf.length < 5) return;
      const flags = buf.readUInt8(0);
      const len = buf.readUInt32BE(1);
      if (buf.length < 5 + len) return;
      const body = buf.subarray(5, 5 + len);
      buf = buf.subarray(5 + len);
      if ((flags & 0x80) === 0) onMessage(body);
    }
  };
}

async function unary(method, body = Buffer.alloc(0), timeoutMs = 120000) {
  const res = await fetch(`${ENDPOINT}/devcontext.v1.DevContextService/${method}`, {
    method: "POST",
    headers: { "content-type": "application/grpc-web+proto", "x-user-agent": "deep-link-truth" },
    body: frame(body),
    signal: AbortSignal.timeout(timeoutMs),
  });
  const bytes = Buffer.from(await res.arrayBuffer());
  let message = null;
  frameSplitter((m) => { message ??= m; })(bytes);
  const status = res.headers.get("grpc-status");
  if (status && status !== "0") throw new Error(`${method}: grpc-status ${status} ${res.headers.get("grpc-message") ?? ""}`);
  if (!message) throw new Error(`${method}: no message frame in reply`);
  return message;
}

/** Subscribes to ObserveToolCalls and appends every event to `sink` until aborted. */
function observe(sink) {
  const abort = new AbortController();
  const done = (async () => {
    const res = await fetch(`${ENDPOINT}/devcontext.v1.DevContextService/ObserveToolCalls`, {
      method: "POST",
      headers: { "content-type": "application/grpc-web+proto", "x-user-agent": "deep-link-truth" },
      body: frame(),
      signal: abort.signal,
    });
    const feed = frameSplitter((m) => sink.push(toolCallEvent(m)));
    try {
      for await (const chunk of res.body) feed(chunk);
    } catch (e) {
      if (!abort.signal.aborted) throw e;
    }
  })();
  return { abort: () => abort.abort(), done: done.catch(() => {}) };
}

// ---------------------------------------------------------------------------
// MCP stdio client (same shape as partial-truth.js — see its note on close()).
// ---------------------------------------------------------------------------

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
  function call(method, params = {}, timeoutMs = 60000) {
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
    // Closing stdin lets the sidecar shut its server child down; kill is only the backstop.
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

async function tool(client, name, args, timeoutMs = 180000) {
  const raw = await client.call("tools/call", { name, arguments: args }, timeoutMs);
  const text = (raw.result?.content ?? []).map((c) => c.text ?? "").join("\n");
  let body = null;
  try { body = JSON.parse(text); } catch { /* plain text reply */ }
  return { raw, text, body };
}

const results = [];
let failed = 0;
function check(label, ok, detail = "") {
  console.log(`  ${ok ? "PASS" : "FAIL"}  ${label}${detail ? " - " + detail : ""}`);
  results.push({ label, ok, detail });
  if (!ok) { failed++; process.exitCode = 1; }
}

function dump(name, obj) {
  if (!existsSync(OUT_DIR)) mkdirSync(OUT_DIR, { recursive: true });
  const p = join(OUT_DIR, name);
  writeFileSync(p, JSON.stringify(obj, null, 2), "utf8");
  console.log(`  wrote ${p}`);
  return p;
}

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

(async () => {
  if (!existsSync(MCP_EXE)) {
    console.error(`MCP exe not found: ${MCP_EXE}\nBuild it: dotnet build src/DevContext.Mcp`);
    process.exit(2);
  }
  if (!existsSync(REPO_PATH)) {
    console.error(`Repo not found: ${REPO_PATH}\nRun: git submodule update --init`);
    process.exit(2);
  }
  console.log(`deep-link-truth: ${MCP_EXE}\n  repo: ${REPO_PATH}\n  out:  ${OUT_DIR}\n`);

  const client = mcpClient(MCP_EXE);
  const events = [];
  let stream = null;
  try {
    const init = await client.call("initialize", {
      protocolVersion: "2024-11-05", capabilities: {},
      clientInfo: { name: "deep-link-truth", version: "0.0.1" },
    }, 180000);
    if (init.error) throw new Error(`init failed: ${JSON.stringify(init.error)}`);
    client.notify("notifications/initialized", {});

    // analyze first: it is what starts the server this probe then talks to.
    const analyzed = await tool(client, "analyze", { path: REPO_PATH }, 300000);
    const handle = analyzed.body?.handle ?? analyzed.body?.session?.handle;
    if (!handle) throw new Error(`analyze returned no handle: ${analyzed.text.slice(0, 300)}`);
    console.log(`  handle: ${handle}`);

    // WHICH engine answered (the stale/foreign-build trap this repo has already paid for twice).
    const identity = await verifyServerIdentity();
    check("the server answering is THIS repo's build", identity.ok, identity.detail);
    if (!identity.ok) throw new Error("refusing to measure a foreign build");

    // Subscribe BEFORE the calls under test — the stream is live-only, there is no backlog.
    stream = observe(events);
    await sleep(400);

    // A focus an agent would actually send. entrypoints names them; take the first with a target.
    const entries = await tool(client, "entrypoints", { handle, full: true });
    const focus = (entries.body?.entries ?? []).find((e) => e.title)?.title ?? "TodoApi";
    console.log(`  focus: ${focus}`);

    await tool(client, "trace", { handle, focus });
    await tool(client, "get_context", { handle, focus });
    await sleep(1200); // let the last events land before the stream is torn down

    dump("events.json", events);

    const agentRows = events.filter((e) => e.origin === "agent");
    check("agent calls arrive tagged as agent traffic", agentRows.length > 0,
      `${agentRows.length} of ${events.length} rows`);

    const traceRow = events.find((e) => e.mcpTool === "trace" && e.rpc === EXPLORE_RPC);
    check(`a trace call records ${EXPLORE_RPC} under the agent's verb`, !!traceRow,
      traceRow ? "" : `saw ${JSON.stringify(events.map((e) => `${e.mcpTool}/${e.rpc}`))}`);
    if (traceRow) {
      // The whole point of field 11: the row can be OPENED, not just read.
      check("the trace row carries the focus the agent sent", traceRow.primaryArg === focus,
        `primary_arg=${JSON.stringify(traceRow.primaryArg)}`);
      check("the trace row still carries its args digest", traceRow.argsDigest.length > 0,
        traceRow.argsDigest);
      check("the trace row names the repo it was called against", traceRow.sessionRepo.length > 0,
        traceRow.sessionRepo);
    }

    const packRow = events.find((e) => e.mcpTool === "get_context" && e.rpc === STUDIO_RPC);
    check(`a get_context call records ${STUDIO_RPC} under the agent's verb`, !!packRow,
      packRow ? "" : `saw ${JSON.stringify(events.map((e) => `${e.mcpTool}/${e.rpc}`))}`);
    if (packRow) {
      check("the get_context row carries the focus the agent sent", packRow.primaryArg === focus,
        `primary_arg=${JSON.stringify(packRow.primaryArg)}`);
    }

    // The previous session shipped ListMcpTools without ever driving it against a live sidecar.
    const listed = fields(await unary("ListMcpTools"));
    const listedOk = (listed[4]?.length ?? 0) > 0;
    const names = (listed[4] ?? []).map((t) => first(fields(t), 1));
    const specialists = (listed[5] ?? []).map((t) => first(fields(t), 1));
    check("ListMcpTools answers off a LIVE tools/list", listedOk,
      `${names.length} advertised, ${specialists.length} specialists`);
    check("the served menu is the curated one (trace + get_context advertised)",
      names.includes("trace") && names.includes("get_context"), names.join(", "));
    dump("catalog.json", { tools: names, specialists });

    dump("verdicts.json", { focus, handle, endpoint: ENDPOINT, results });
  } finally {
    stream?.abort();
    await stream?.done;
    await client.close();
  }

  console.log(`\n${failed === 0 ? "PASS" : "FAIL"} - ${results.length - failed}/${results.length} checks`);
})().catch((err) => {
  console.error(`deep-link-truth: ${err.stack ?? err.message}`);
  process.exit(1);
});
