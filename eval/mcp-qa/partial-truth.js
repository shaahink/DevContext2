// Partial-truth probe (T1.3 / BUG-BACKLOG #6, #9, #2, #10) — the confident-wrong-answer family,
// measured off a REAL MCP session over stdio. Nothing here reads the C# source: every verdict is
// what an agent would actually receive.
//
// The three shapes it hunts, all of which look like success:
//   #6  trace(nodeId) answers found:true with a vacuous/phantom tree while trace(bareName) works.
//   #9  get_context elides a body ("... (+N lines)") and its fillNote/omitted names no elision.
//   #2  entrypoints renders a title that get_context/trace cannot resolve back.
//   #10 an out-of-range enum is rejected rather than silently re-read (regression guard).
//
// Usage:
//   node eval/mcp-qa/partial-truth.js [outDir] [repoPath]
// Defaults: outDir = eval-results/<today>/t1-partial-truth, repoPath = <repo>/eval-repos/TodoApi
//
// Prints PASS/FAIL and exits non-zero on any FAIL, so T1.4 can lift it into the battery verbatim.

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync } = require("fs");

const REPO_ROOT = join(__dirname, "..", "..");
const OUT_DIR = resolve(process.argv[2]
  ?? join(REPO_ROOT, "eval-results", new Date().toISOString().slice(0, 10), "t1-partial-truth"));
const REPO_PATH = resolve(process.argv[3] ?? join(REPO_ROOT, "eval-repos", "TodoApi"));
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
  function call(method, params = {}, timeoutMs = 60000) {
    return new Promise((res, rej) => {
      const id = nextId++;
      const timer = setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); rej(new Error(`Timeout: ${method} ${JSON.stringify(params).slice(0, 120)}`)); }
      }, timeoutMs);
      pending.set(id, { resolve: res, timer });
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    });
  }
  return {
    call,
    notify: (method, params = {}) =>
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n"),
    // MEASURED twice this session: proc.kill() leaves the DevContext.Server the MCP spawned RUNNING
    // and holding DevContext.Core.dll, so the next `dotnet build` fails with MSB3027 "locked by
    // DevContext.Server". Closing stdin lets the MCP shut its child down the way it means to; the
    // kill is only the backstop for a host that ignores EOF.
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
async function tool(client, name, args, timeoutMs = 120000) {
  const raw = await client.call("tools/call", { name, arguments: args }, timeoutMs);
  const text = (raw.result?.content ?? []).map((c) => c.text ?? "").join("\n");
  let body = null;
  try { body = JSON.parse(text); } catch { /* plain text reply */ }
  transcript.push({ tool: name, arguments: args, text, body });
  return { raw, text, body };
}

function dump(name, obj) {
  if (!existsSync(OUT_DIR)) mkdirSync(OUT_DIR, { recursive: true });
  const p = join(OUT_DIR, name);
  writeFileSync(p, JSON.stringify(obj, null, 2), "utf8");
  console.log(`  wrote ${p}`);
  return p;
}

const results = [];
let failed = 0;
function check(bug, label, ok, detail = "") {
  console.log(`  ${ok ? "PASS" : "FAIL"}  [#${bug}] ${label}${detail ? " - " + detail : ""}`);
  results.push({ bug, label, ok, detail });
  if (!ok) { failed++; process.exitCode = 1; }
}

// An elision marker an agent would SEE in the pack text. Both forms the builder can emit.
const ELISION_RE = /… \(\+(\d+) (lines|more members)/g;
function elisionsIn(text) {
  const out = [];
  for (const m of (text ?? "").matchAll(ELISION_RE)) out.push({ count: Number(m[1]), what: m[2] });
  return out;
}

(async () => {
  if (!existsSync(MCP_EXE)) {
    console.error(`MCP exe not found: ${MCP_EXE}\nBuild it: dotnet build src/DevContext.Mcp`);
    process.exit(2);
  }
  if (!existsSync(REPO_PATH)) {
    console.error(`Repo not found: ${REPO_PATH}\nRun: git submodule update --init`);
    process.exit(2);
  }
  console.log(`partial-truth: ${MCP_EXE}\n  repo: ${REPO_PATH}\n  out:  ${OUT_DIR}\n`);
  const client = mcpClient(MCP_EXE);
  try {
    const init = await client.call("initialize", {
      protocolVersion: "2024-11-05", capabilities: {},
      clientInfo: { name: "partial-truth", version: "0.0.1" },
    }, 180000);
    if (init.error) throw new Error(`init failed: ${JSON.stringify(init.error)}`);
    client.notify("notifications/initialized", {});

    const analyzed = await tool(client, "analyze", { path: REPO_PATH }, 600000);
    const handle = analyzed.body?.handle ?? analyzed.body?.sessionId;
    if (!handle) throw new Error(`analyze gave no handle: ${analyzed.text.slice(0, 400)}`);
    console.log(`  handle: ${handle}\n`);

    // ---------------------------------------------------------------- #2 entry-name round-trip
    // The obvious agent move: read a name off `entrypoints`, hand it straight to get_context/trace.
    const eps = await tool(client, "entrypoints", { handle, full: true });
    const entries = eps.body?.entries ?? [];
    const sample = entries.slice(0, 8);
    // #2 is a NAME-agreement bug, so it is checked over EVERY entry the tool lists, not a sample —
    // one unaddressable name in the inventory is the whole defect.
    const roundTrip = [];
    for (const e of entries.slice(0, 40)) {
      const title = e.title;
      if (!title) continue;
      const gc = await tool(client, "get_context", { handle, focus: title, budgetTokens: 2000 });
      const tr = await tool(client, "trace", { handle, focus: title, format: "compact" });
      roundTrip.push({
        kind: e.kind, title, nodeId: e.nodeId ?? null,
        getContextResolved: gc.body?.totalTokens > 0 && !gc.body?.error,
        getContextError: gc.body?.error ?? null,
        traceResolved: tr.body?.found === true,
        traceSteps: tr.body?.steps ?? null,
        traceError: tr.body?.error ?? null,
      });
    }
    dump("entry-roundtrip.json", roundTrip);
    const rtBad = roundTrip.filter((r) => !r.getContextResolved || !r.traceResolved);
    check(2, "every entrypoints title resolves in get_context AND trace",
      roundTrip.length > 0 && rtBad.length === 0,
      rtBad.length ? rtBad.map((r) => `${r.title} (ctx:${r.getContextResolved} trace:${r.traceResolved})`).join(" | ")
        : `${roundTrip.length} titles round-tripped`);

    // ---------------------------------------------------------------- #6 trace by nodeId
    // Every other tool takes a nodeId and trace's own did-you-mean envelope hands nodeIds back,
    // so "read the id off resolve, hand it to trace" is the first thing an agent tries.
    const nodeIds = [];
    for (const e of sample) if (e.nodeId) nodeIds.push({ nodeId: e.nodeId, title: e.title });
    // plus a Type node found the way an agent finds one
    const found = await tool(client, "find", { handle, query: "e", limit: 40 });
    const hits = found.body?.results ?? [];
    for (const h of hits) {
      if (nodeIds.length >= 12) break;
      if (h.nodeId && String(h.nodeId).startsWith("Type:")) nodeIds.push({ nodeId: h.nodeId, title: h.title });
    }
    const traceById = [];
    for (const n of nodeIds) {
      const byId = await tool(client, "trace", { handle, focus: n.nodeId, format: "compact" });
      const bare = n.title
        ? await tool(client, "trace", { handle, focus: n.title, format: "compact" })
        : { body: null };
      const entryLine = (byId.body?.text ?? "").split("\n")[0] ?? "";
      traceById.push({
        nodeId: n.nodeId, title: n.title,
        found: byId.body?.found ?? false,
        steps: byId.body?.steps ?? null,
        note: byId.body?.note ?? null,
        entryLine,
        bareFound: bare.body?.found ?? null,
        bareSteps: bare.body?.steps ?? null,
      });
    }
    dump("trace-by-nodeid.json", traceById);

    // A phantom is: the rendered entry line repeats the node KIND as the title ("Type: Type"),
    // or the id resolved to a different subject than the same node's bare name did.
    const phantom = traceById.filter((t) => /^Entry: (\w+): \1\s*$/.test(t.entryLine.trim()));
    check(6, "trace(nodeId) never renders a phantom whose title IS its kind",
      phantom.length === 0,
      phantom.map((p) => `${p.nodeId} -> "${p.entryLine.trim()}"`).join(" | ") || `${traceById.length} nodeIds`);

    const vacuous = traceById.filter((t) => t.found === true && (t.steps ?? 0) === 0 && !t.note);
    check(6, "a found:true trace with 0 steps says WHY (note) instead of an empty tree",
      vacuous.length === 0,
      vacuous.map((v) => v.nodeId).join(" | ") || "no silent empty trace");

    const idLoses = traceById.filter((t) => t.bareFound === true && (t.bareSteps ?? 0) > 0
      && ((t.steps ?? 0) === 0 || t.found !== true));
    check(6, "trace(nodeId) is not weaker than trace(bare name) for the same node",
      idLoses.length === 0,
      idLoses.map((t) => `${t.nodeId}: id=${t.steps} bare=${t.bareSteps}`).join(" | ") || "id and name agree");

    // ---------------------------------------------------------------- #9 elision honesty
    // The discriminating case is a focus WITH a body that elides at a low budget. Walk candidate
    // focuses at a small budget until one actually elides; a focus with nothing to cut proves nothing.
    const focusPool = [
      ...sample.map((e) => e.title).filter(Boolean),
      ...hits.slice(0, 20).map((h) => h.title).filter(Boolean),
    ];
    const elisionRows = [];
    let discriminator = null;
    for (const f of focusPool) {
      const gc = await tool(client, "get_context", { handle, focus: f, budgetTokens: 1500 });
      if (!gc.body || gc.body.error) continue;
      const el = elisionsIn(gc.body.content);
      const omittedText = (gc.body.omitted ?? []).join(" | ");
      const noteText = gc.body.fillNote ?? "";
      const namesElision = /raise budgetTokens|budgetTokens for the rest|elided|truncat/i.test(noteText + " " + omittedText);
      const claimsComplete = /already contains everything reachable/i.test(noteText);
      const row = {
        focus: f, budgetTokens: 1500, totalTokens: gc.body.totalTokens,
        elisions: el, fillNote: gc.body.fillNote ?? null, omitted: gc.body.omitted ?? [],
        namesElision, claimsComplete,
      };
      elisionRows.push(row);
      if (el.length > 0 && !discriminator) discriminator = row;
    }
    // The same focus at a big budget: proves the lever the note must name actually works.
    if (discriminator) {
      const wide = await tool(client, "get_context",
        { handle, focus: discriminator.focus, budgetTokens: 20000 });
      discriminator.atWideBudget = {
        budgetTokens: 20000, totalTokens: wide.body?.totalTokens ?? null,
        elisions: elisionsIn(wide.body?.content), fillNote: wide.body?.fillNote ?? null,
      };
    }
    dump("elision-honesty.json", { discriminator, rows: elisionRows });

    check(9, "the probe found a real elided pack to judge (else the check is vacuous)",
      discriminator !== null,
      discriminator ? `${discriminator.focus} elides ${discriminator.elisions.map((e) => "+" + e.count + " " + e.what).join(", ")}`
        : "no focus elided at budget 1500 - widen the pool before trusting a green here");

    const liars = elisionRows.filter((r) => r.elisions.length > 0 && r.claimsComplete);
    check(9, "no pack claims 'everything reachable' while its own text shows an elision",
      liars.length === 0,
      liars.map((r) => r.focus).join(" | ") || "no false completeness claim");

    const silent = elisionRows.filter((r) => r.elisions.length > 0 && !r.namesElision);
    check(9, "every elided pack names the elision AND the budgetTokens lever",
      silent.length === 0,
      silent.map((r) => `${r.focus} (note: ${String(r.fillNote).slice(0, 60)})`).join(" | ") || "all elisions named");

    // ---------------------------------------------------------------- #10 regression guard
    const badMode = await tool(client, "read_source", { handle, nodeId: nodeIds[0]?.nodeId ?? "x", mode: "full" });
    check(10, "an out-of-range enum is rejected, not silently re-read",
      typeof badMode.body?.error === "string" && /mode/i.test(badMode.body.error),
      String(badMode.body?.error ?? badMode.text).slice(0, 90));

    dump("transcript.json", transcript);
    dump("partial-truth.json", {
      measuredAt: new Date().toISOString(), mcpExe: MCP_EXE, repo: REPO_PATH, handle,
      checks: results, failed,
    });
  } finally {
    await client.close();
  }
  console.log(failed === 0 ? "\npartial-truth: GREEN" : `\npartial-truth: RED (${failed} failed)`);
})().catch((e) => { console.error(e); process.exit(1); });
