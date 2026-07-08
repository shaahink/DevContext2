// L4.3 evidence capture — dumps overview + top_flows raw output to prove they read
// the graph projections (full service names, projection-ranked flows). Not a gate; an artifact.
const { spawn } = require("child_process");
const { join } = require("path");
const { createInterface } = require("readline");

const REPO = "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";
const MCP_EXE = join(__dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

function client(exe) {
  const proc = spawn(exe, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  let id = 1; const pending = new Map();
  rl.on("line", (l) => { try { const m = JSON.parse(l); if (m.id !== undefined && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); } } catch {} });
  proc.stderr.resume();
  const call = (method, params = {}) => new Promise((res, rej) => { const i = id++; pending.set(i, res); proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id: i, method, params }) + "\n"); setTimeout(() => { if (pending.has(i)) { pending.delete(i); rej(new Error("Timeout " + method)); } }, 60000); });
  const notify = (method, params = {}) => proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  return { call, notify, close: () => { rl.close(); proc.kill(); } };
}
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
function content(r) { const t = r?.content?.filter((c) => c.type === "text").map((c) => c.text).join("\n") ?? ""; try { return JSON.parse(t); } catch { return { text: t }; } }

async function main() {
  const c = client(MCP_EXE);
  await c.call("initialize", { protocolVersion: "2024-11-05", capabilities: {}, clientInfo: { name: "l43-evidence", version: "1" } });
  c.notify("notifications/initialized", {});
  await sleep(3000); // let ServerShim spawn + connect
  const analyzePromise = c.call("tools/call", { name: "analyze", arguments: { path: REPO } });
  let handle = null;
  for (let i = 0; i < 300 && !handle; i++) { await sleep(500); try { const r = content((await c.call("tools/call", { name: "list_sessions", arguments: {} })).result); const s = (r.sessions ?? []).find((x) => x.status === "ready" || x.status === "done"); if (s) handle = s.handle; } catch {} }
  try { const ar = content((await analyzePromise).result); if (!handle && ar?.handle) handle = ar.handle; } catch {}
  if (!handle) { console.error("no handle"); c.close(); process.exit(1); }

  const overview = content((await c.call("tools/call", { name: "overview", arguments: { handle } })).result);
  const topFlows = content((await c.call("tools/call", { name: "top_flows", arguments: { handle } })).result);

  console.log("# L4.3 Consumer Evidence — MCP overview + top_flows read projections\n");
  console.log("Repo: " + REPO + "\n");
  console.log("## overview() — services from ServiceMapProjection (full DisplayNames), flows from FlowListProjection\n");
  console.log("```\n" + (overview.text ?? JSON.stringify(overview, null, 2)) + "\n```\n");
  console.log("Tokens: " + (overview.tokens ?? "?") + "\n");
  console.log("## top_flows() — ranked + shaped by FlowListProjection (server-side), count=" + (topFlows.count ?? "?") + "\n");
  console.log("```json\n" + JSON.stringify(topFlows.topFlows?.slice(0, 10) ?? topFlows, null, 2) + "\n```");

  await c.call("tools/call", { name: "close_session", arguments: { handle } }).catch(() => {});
  c.close();
}
main().catch((e) => { console.error(e); process.exit(1); });
