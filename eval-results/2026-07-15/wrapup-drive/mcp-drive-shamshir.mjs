// Qualitative MCP drive against shamshir (wrap-up session 2026-07-15).
// Unlike run-cold.js (actionability scoring), this captures FULL tool outputs for
// qualitative review of context generation: what an agent actually receives.
// Usage: node eval-results/2026-07-15/wrapup-drive/mcp-drive-shamshir.mjs
import { spawn } from "child_process";
import { createInterface } from "readline";
import { writeFileSync } from "fs";
import { join, dirname } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const REPO = "C:/code/shamshir";
const MCP_EXE = join(__dirname, "..", "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");
const OUT = join(__dirname, "mcp-shamshir-transcript.md");

function mcpClient(exePath) {
  const proc = spawn(exePath, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
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
    } catch { /* logs */ }
  });
  proc.stderr.resume();
  const call = (method, params = {}) => new Promise((resolve, reject) => {
    const id = nextId++;
    pending.set(id, resolve);
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    setTimeout(() => { if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); } }, 180000);
  });
  const notify = (method, params = {}) => proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  return { call, notify, close: () => { rl.close(); proc.kill(); } };
}
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

function contentText(resp) {
  const c = resp?.result?.content;
  if (Array.isArray(c)) return c.filter((x) => x.type === "text").map((x) => x.text).join("\n");
  return JSON.stringify(resp?.error ?? resp?.result ?? resp);
}

const sections = [];
async function drive(client, title, tool, args) {
  const t0 = Date.now();
  let resp;
  try { resp = await client.call("tools/call", { name: tool, arguments: args }); }
  catch (e) { resp = { error: { message: e.message } }; }
  const text = contentText(resp);
  const tok = Math.ceil(text.length / 4);
  sections.push(`## ${title}\n\n\`${tool}(${JSON.stringify(args)})\` — ${Date.now() - t0}ms · ~${tok} tok\n\n\`\`\`\n${text}\n\`\`\`\n`);
  console.log(`${title}: ${Date.now() - t0}ms ~${tok}tok`);
  return text;
}

const client = mcpClient(MCP_EXE);
try {
  await client.call("initialize", { protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "wrapup-drive", version: "0" } });
  client.notify("notifications/initialized", {});

  // analyze (poll for readiness like run-cold.js)
  client.call("tools/call", { name: "analyze", arguments: { path: REPO } }).catch(() => {});
  let handle = null;
  for (let i = 0; i < 360 && !handle; i++) {
    await sleep(500);
    try {
      const ls = await client.call("tools/call", { name: "list_sessions", arguments: {} });
      const sessions = JSON.parse(contentText(ls)).sessions ?? [];
      const ready = sessions.find((s) => s.status === "ready" || s.status === "done");
      if (ready) handle = ready.handle;
    } catch { }
  }
  console.log("handle:", handle);

  await drive(client, "Q1 — What is this repo?", "overview", {});
  const entries = await drive(client, "Q2 — What are the entry points?", "entrypoints", {});
  await drive(client, "Q3 — Trace a SignalR hub", "trace", { focus: "RunHub" });
  await drive(client, "Q4 — Trace a background worker", "trace", { focus: "EngineWorker" });
  await drive(client, "Q5 — Trace an endpoint by bare route", "trace", { focus: "/api/bars" });
  await drive(client, "Q6 — Top flows", "top_flows", {});
  await drive(client, "Q7 — Context pack for the cancel-run endpoint", "get_context", { focus: "DELETE /api/runs/{runId}", budgetTokens: 4000 });
  await drive(client, "Q8 — Impact of changing BacktestOrchestrator", "impact", { query: "BacktestOrchestrator" });
  await drive(client, "Q9 — Insights", "insights", {});
  await drive(client, "Q10 — Config keys", "config", {});
  await drive(client, "Q11 — Tests for the orchestrator", "tests_for", { query: "BacktestOrchestrator" });
  await drive(client, "Q12 — Read source of a hub", "read_source", { query: "WalkForwardHub" });

  writeFileSync(OUT, `# MCP qualitative drive — shamshir (2026-07-15)\n\nRepo: ${REPO} · handle: ${handle}\n\n${sections.join("\n")}`);
  console.log("Wrote", OUT);
} finally {
  client.close();
}
