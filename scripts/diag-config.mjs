// Quick config tool diagnostic
const { spawn } = require("child_process");
const { join } = require("path");
const { createInterface } = require("readline");
const { existsSync } = require("fs");

const REPO = "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";
const MCP_EXE = join(__dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

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
    } catch (_) {}
  });

  proc.stderr.resume();

  function call(method, params = {}) {
    return new Promise((resolve, reject) => {
      const id = nextId++;
      pending.set(id, resolve);
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
      setTimeout(() => { if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); } }, 45000);
    });
  }

  function notify(method, params = {}) {
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  }

  function close() { rl.close(); proc.kill(); }
  return { call, notify, close };
}

function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

function extractContent(result) {
  if (result?.content && Array.isArray(result.content)) {
    const texts = result.content.filter(c => c.type === "text").map(c => c.text).join("\n");
    try { return JSON.parse(texts); } catch { return { text: texts }; }
  }
  return result;
}

async function main() {
  const client = mcpClient(MCP_EXE);
  
  console.log("Initializing...");
  await client.call("initialize", {
    protocolVersion: "2024-11-05",
    capabilities: {},
    clientInfo: { name: "diag", version: "1.0" },
  });
  client.notify("notifications/initialized", {});

  // Analyze
  console.log("Analyzing repo...");
  const analyzePromise = client.call("tools/call", { name: "analyze", arguments: { path: REPO } });

  let handle = null;
  for (let i = 0; i < 240; i++) {
    await sleep(500);
    try {
      const listResp = await client.call("tools/call", { name: "list_sessions", arguments: {} });
      if (!handle && listResp.result) {
        const data = extractContent(listResp.result);
        const sessions = data.sessions ?? [];
        const ready = sessions.find(s => s.status === "ready" || s.status === "done");
        if (ready) handle = ready.handle;
      }
    } catch (_) {}
    if (handle) break;
  }

  if (!handle) {
    console.log("Failed to analyze!");
    client.close();
    return;
  }

  console.log(`Analyzed, handle: ${handle}`);

  // Call config tool
  console.log("\nCalling config tool...");
  const configResp = await client.call("tools/call", { name: "config", arguments: { handle } });
  console.log("\n=== RAW config response ===");
  console.log(JSON.stringify(configResp, null, 2));
  
  const data = extractContent(configResp.result);
  console.log("\n=== EXTRACTED content ===");
  console.log(JSON.stringify(data, null, 2));

  client.close();
}

main().catch(err => { console.error(err); process.exit(1); });
