// Generic MCP lens drive: repo-agnostic smoke used by eval/lens-audit.ps1.
// Usage: node lens-drive.js <repoPath> <transcriptOut>
// Exit 0 = every call returned non-error, the map is non-trivial, and the navigation probes
// pass (D5.1/G1: get_context fill >=85% OR carries fillNote; neighbors on a central node
// non-empty). Exit 1 otherwise.
// Repo-specific deep drives stay in drive-generic.js / run.js; this one must work on ANY repo.
const { spawn } = require("child_process");
const { createInterface } = require("readline");
const { writeFileSync } = require("fs");
const { join } = require("path");

const REPO = process.argv[2];
const OUT = process.argv[3];
if (!REPO || !OUT) {
  console.error("usage: node lens-drive.js <repoPath> <transcriptOut>");
  process.exit(1);
}

const MCP_EXE = join(__dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

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

function call(method, params = {}, timeoutMs = 120000) {
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
  if (Array.isArray(c)) return c.filter((x) => x.type === "text").map((x) => x.text).join("\n");
  return JSON.stringify(resp?.result ?? resp?.error ?? resp);
}
function isError(resp) {
  return !!resp?.error || resp?.result?.isError === true;
}

const transcript = [];
let failures = 0;
function log(step, text, err) {
  const tok = Math.ceil((text || "").length / 4);
  transcript.push(`\n===== ${step}  (~${tok} tokens)${err ? "  [ERROR]" : ""} =====\n${text}`);
  console.log(`${step}: ~${tok} tokens${err ? "  [ERROR]" : ""}`);
  if (err) failures++;
}

(async () => {
  await call("initialize", { protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "lens-audit", version: "0" } });
  notify("notifications/initialized", {});

  const tools = await call("tools/list", {});
  log("tools/list", ((tools.result?.tools ?? []).map((t) => t.name).sort()).join(", "), isError(tools));

  const t0 = Date.now();
  const a = await call("tools/call", { name: "analyze", arguments: { path: REPO } }, 600000);
  log(`analyze  [${((Date.now() - t0) / 1000).toFixed(1)}s]`, content(a), isError(a));

  const steps = [
    ["overview", {}],
    ["map", {}],
    ["entrypoints", { full: true }],
  ];
  let mapTokens = 0;
  let entrypointsText = "";
  for (const [name, args] of steps) {
    const r = await call("tools/call", { name, arguments: args }, 180000);
    const text = content(r);
    log(name, text, isError(r));
    if (name === "map") mapTokens = Math.ceil(text.length / 4);
    if (name === "entrypoints") entrypointsText = text;
  }

  if (mapTokens < 100) {
    transcript.push(`\n===== VERDICT =====\nFAIL: map is trivial (~${mapTokens} tokens)`);
    failures++;
  }

  // ---- D5.1 (G1) navigation probes: zero empty navigations / zero silent fill breaches ----
  // get_context on the repo's best entry: must build a pack, and a <85% fill must carry the
  // fillNote explanation (the T4.2 promise is allowed to fail only OUT LOUD).
  let entries = [];
  try { entries = JSON.parse(entrypointsText).entries ?? []; } catch (_) {}
  if (entries.length > 0) {
    const best = entries.reduce((a, b) => ((b.score ?? 0) > (a.score ?? 0) ? b : a), entries[0]);
    const focus = best.httpMethod && best.route ? `${best.httpMethod} ${best.route}` : best.title;
    const budget = 6000;
    const r = await call("tools/call", { name: "get_context", arguments: { focus, budgetTokens: budget } }, 180000);
    const text = content(r);
    log(`get_context {"focus":"${focus}","budgetTokens":${budget}}`, text, isError(r));
    if (isError(r) || text.includes("No context could be built")) {
      transcript.push(`\n===== PROBE =====\nFAIL: empty navigation — get_context built nothing for the top-scored entry '${focus}'`);
      failures++;
    } else {
      try {
        const pack = JSON.parse(text);
        const fill = (pack.totalTokens ?? 0) / budget;
        if (fill < 0.85 && !pack.fillNote) {
          transcript.push(`\n===== PROBE =====\nFAIL: silent fill breach — get_context filled ${Math.round(fill * 100)}% of budget with no fillNote`);
          failures++;
        }
      } catch (_) {
        transcript.push(`\n===== PROBE =====\nFAIL: get_context returned unparseable JSON`);
        failures++;
      }
    }
  } else {
    transcript.push(`\n===== PROBE =====\nSKIP: get_context probe (no entry points — library surface rides the map probe)`);
  }

  // neighbors on a central node (overview's startHere, which absorbed interesting_points in
  // G2.1): a node the lens itself calls central
  // must navigate somewhere (C3 type rollups) — out, else in; both empty = dead end.
  const ip = await call("tools/call", { name: "overview", arguments: {} }, 180000);
  const ipText = content(ip);
  log("overview(startHere)", ipText, isError(ip));
  let points = [];
  try { points = JSON.parse(ipText).startHere ?? []; } catch (_) {}
  if (points.length > 0) {
    const nodeId = points[0].nodeId;
    let navigated = false;
    for (const direction of ["out", "in"]) {
      const n = await call("tools/call", { name: "neighbors", arguments: { nodeId, direction } }, 180000);
      const nText = content(n);
      log(`neighbors {"nodeId":"${nodeId}","direction":"${direction}"}`, nText, isError(n));
      try { if ((JSON.parse(nText).count ?? 0) > 0) { navigated = true; break; } } catch (_) {}
    }
    if (!navigated) {
      transcript.push(`\n===== PROBE =====\nFAIL: empty navigation — neighbors out+in both empty on central node '${nodeId}'`);
      failures++;
    }
  } else {
    transcript.push(`\n===== PROBE =====\nSKIP: neighbors probe (no interesting points reported)`);
  }
  transcript.push(`\n===== VERDICT =====\n${failures === 0 ? "PASS" : `FAIL (${failures} failing calls)`}`);
  writeFileSync(OUT, transcript.join("\n"), "utf8");
  proc.kill();
  process.exit(failures === 0 ? 0 : 1);
})().catch((e) => {
  transcript.push(`\n===== FATAL =====\n${e.message}`);
  try { writeFileSync(OUT, transcript.join("\n"), "utf8"); } catch (_) {}
  try { proc.kill(); } catch (_) {}
  console.error("FATAL:", e.message);
  process.exit(1);
});
