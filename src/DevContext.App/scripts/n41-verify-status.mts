/**
 * N4.1 (STUDIO-MCP audit section 3.D / section 4 Room 2) — proves the MCP page's status card
 * MEASURES, against the running app and a real devcontext-mcp process.
 *
 * Three claims, three live checks:
 *   1. binary probe   the card names an executable that exists, with the path a host must use
 *   2. handshake      clicking it spawns that executable and shows the tool menu it answered
 *   3. agent traffic  after a REAL agent call (this script drives the MCP binary over stdio,
 *                     i.e. native gRPC, i.e. origin=agent), Re-check reports it
 *
 * Check 3 is the one that cannot be faked: the page's own gRPC-web calls are origin=ui and must
 * NOT move the number, which is exactly what the old "MCP endpoint active" dot got wrong.
 *
 * Usage (services already running via scripts/start-dev-bg.ps1):
 *   node --experimental-strip-types src/DevContext.App/scripts/n41-verify-status.mts
 * Exits non-zero on any failed check.
 */
import { spawn } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';
import { createInterface } from 'node:readline';
import { chromium } from 'playwright';

const APP = 'http://localhost:4200/mcp';
const REPO = 'C:/Code/DevContext2-desktop';
const MCP_EXE = `${REPO}/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe`;
const FIXTURE = `${REPO}/tests/fixtures/ControllerApp`;
const OUT = `${REPO}/eval-results/2026-08-14`;

const failures: string[] = [];
function check(name: string, ok: boolean, detail: string) {
  console.log(`${ok ? 'PASS' : 'FAIL'}  ${name} — ${detail}`);
  if (!ok) failures.push(`${name}: ${detail}`);
}

/** One JSON-RPC conversation with the MCP binary over stdio — the agent's own transport. */
async function driveAgent(): Promise<{ tools: number; handle: string; statsCall: string }> {
  const child = spawn(MCP_EXE, [], { stdio: ['pipe', 'pipe', 'pipe'] });
  child.stderr.resume();
  const lines = createInterface({ input: child.stdout });
  const pending = new Map<number, (v: any) => void>();
  lines.on('line', (line) => {
    let msg: any;
    try { msg = JSON.parse(line); } catch { return; }
    const resolve = pending.get(msg?.id);
    if (resolve) { pending.delete(msg.id); resolve(msg); }
  });
  const send = (id: number, method: string, params: unknown) =>
    new Promise<any>((resolve, reject) => {
      pending.set(id, resolve);
      child.stdin.write(`${JSON.stringify({ jsonrpc: '2.0', id, method, params })}\n`);
      setTimeout(() => reject(new Error(`${method} timed out`)), 180_000).unref();
    });

  try {
    await send(1, 'initialize', {
      protocolVersion: '2024-11-05', capabilities: {},
      clientInfo: { name: 'n41-verify', version: '1.0' },
    });
    child.stdin.write(`${JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' })}\n`);

    const list = await send(2, 'tools/list', {});
    const tools = list.result?.tools?.length ?? 0;

    console.log('  … agent: analyze(ControllerApp)');
    const analyzed = await send(3, 'tools/call', { name: 'analyze', arguments: { path: FIXTURE } });
    const analyzedText: string = analyzed.result?.content?.[0]?.text ?? '';
    const handle = /\b[0-9a-f]{8,}\b/.exec(analyzedText)?.[0] ?? '';

    console.log(`  … agent: stats(${handle.slice(0, 8)}…)`);
    const stats = await send(4, 'tools/call', { name: 'stats', arguments: { handle } });
    return { tools, handle, statsCall: (stats.result?.content?.[0]?.text ?? '').slice(0, 80) };
  } finally {
    child.kill();
  }
}

const browser = await chromium.launch();
const page = await browser.newPage();
const textOf = (id: string) =>
  page.locator(`[data-testid="${id}"]`).first().textContent().then((t) => (t ?? '').trim()).catch(() => '');

try {
  mkdirSync(OUT, { recursive: true });
  await page.goto(APP, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="mcp-status-card"]', { timeout: 30_000 });
  await page.waitForTimeout(1_500);

  const label = await textOf('mcp-status-label');
  const path = await textOf('mcp-binary-path');
  const chip = await textOf('mcp-binary-check');
  const watchers = await textOf('mcp-status-text');
  const agentBefore = await textOf('mcp-last-agent-call');

  check('binary probe found an executable', label.includes('found') && !label.includes('not found'), label);
  check('the card names the absolute path', path.toLowerCase().endsWith('devcontext-mcp.exe'), path);
  check('the source is named', /bundle|path|dev-build/.test(chip), chip.replace(/\s+/g, ' '));
  check('this page is counted as a watcher', /[1-9]\d* watcher/.test(watchers), watchers);
  check('the global mute is gone', (await page.locator('[data-testid="mcp-toggle"]').count()) === 0,
    'no Start/Stop button on the page');

  console.log('  … clicking Test handshake');
  await page.click('[data-testid="mcp-handshake-run"]');
  await page.waitForSelector('[data-testid="mcp-handshake-result"]', { timeout: 90_000 });
  await page.waitForTimeout(500);
  const handshake = (await textOf('mcp-handshake-result')).replace(/\s+/g, ' ');
  const handshakeTools = (await textOf('mcp-handshake-tools')).replace(/\s+/g, ' ');
  check('handshake ran a real tools/list', handshake.includes('tools/list answered'), handshake);
  check('handshake shows the menu it got', handshakeTools.split('·').length > 5,
    `${handshakeTools.split('·').length} tools`);

  await page.screenshot({ path: `${OUT}/N4.1-status-card.png`, clip: { x: 0, y: 0, width: 1280, height: 420 } });

  const agent = await driveAgent();
  check('the MCP binary served an agent call', agent.handle.length > 0,
    `${agent.tools} tools, handle ${agent.handle.slice(0, 8)}…, stats: ${agent.statsCall.replace(/\s+/g, ' ')}`);

  await page.click('[data-testid="mcp-status-refresh"]');
  await page.waitForTimeout(1_500);
  const agentAfter = await textOf('mcp-last-agent-call');
  check('agent traffic moved the measurement',
    agentBefore !== agentAfter && /Last agent call/.test(agentAfter), `before: "${agentBefore}" | after: "${agentAfter}"`);

  await page.screenshot({ path: `${OUT}/N4.1-after-agent-call.png`, clip: { x: 0, y: 0, width: 1280, height: 420 } });

  writeFileSync(`${OUT}/N4.1-live-probe.json`, JSON.stringify({
    label, path, chip: chip.replace(/\s+/g, ' '), watchers, agentBefore, agentAfter,
    handshake, handshakeTools, agentToolCount: agent.tools, failures,
  }, null, 2));
} finally {
  await browser.close();
}

console.log(failures.length === 0 ? '\nALL CHECKS PASSED' : `\n${failures.length} CHECK(S) FAILED`);
process.exit(failures.length === 0 ? 0 : 1);
