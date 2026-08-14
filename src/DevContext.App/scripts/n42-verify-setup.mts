/**
 * N4.2 (STUDIO-MCP audit section 4, Room 2 "setup that works") — proves the MCP page's setup
 * half is TRUE against the running app, a real devcontext-mcp process, and a real repo on disk.
 *
 * Four claims, four live checks:
 *   1. shipped        the published bundle directory holds devcontext-mcp beside the server, and
 *                     the page's probe resolves an executable that exists
 *   2. snippets       every host snippet parses as JSON and its command equals the ABSOLUTE path
 *                     the status card just named — no placeholder, no invented path
 *   3. write-for-me   clicking Write .mcp.json writes it into the ANALYZED repo, merging with an
 *                     MCP server that was already registered there rather than replacing it
 *   4. honest repeat  clicking again reports "already points here" instead of claiming a write
 *
 * Check 3 is the one that cannot be faked: the assertion reads the file back off disk and parses
 * it. A response that names a path while nothing is there is the exact defect class this
 * checkpoint exists to remove.
 *
 * Usage (services already running via scripts/start-dev-bg.ps1):
 *   node --experimental-strip-types src/DevContext.App/scripts/n42-verify-setup.mts
 * Exits non-zero on any failed check.
 */
import { spawn } from 'node:child_process';
import { cpSync, existsSync, mkdirSync, mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createInterface } from 'node:readline';
import { chromium } from 'playwright';

const APP = 'http://localhost:4200/mcp';
const REPO = 'C:/Code/DevContext2-desktop';
const MCP_EXE = `${REPO}/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe`;
const BUNDLE_DIR = `${REPO}/src/DevContext.App/src-tauri/resources/server`;
const FIXTURE = `${REPO}/tests/fixtures/ControllerApp`;
const OUT = `${REPO}/eval-results/2026-08-14`;

const failures: string[] = [];
function check(name: string, ok: boolean, detail: string) {
  console.log(`${ok ? 'PASS' : 'FAIL'}  ${name} — ${detail}`);
  if (!ok) failures.push(`${name}: ${detail}`);
}

/** Analyze a repo the way an AGENT does — over stdio, through the MCP binary. */
async function agentAnalyze(path: string): Promise<string> {
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
      clientInfo: { name: 'n42-verify', version: '1.0' },
    });
    child.stdin.write(`${JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' })}\n`);
    const analyzed = await send(2, 'tools/call', { name: 'analyze', arguments: { path } });
    return analyzed.result?.content?.[0]?.text ?? '';
  } finally {
    child.kill();
  }
}

// A throwaway copy of the fixture, so a passing run never leaves a .mcp.json in the repo.
const workRepo = mkdtempSync(join(tmpdir(), 'devcontext-n42-'));
cpSync(FIXTURE, workRepo, { recursive: true });
// Pre-registered server: the merge has to keep this, or the button eats a user's setup.
const existingConfig = { mcpServers: { github: { command: 'gh-mcp', args: ['--stdio'] } } };
writeFileSync(join(workRepo, '.mcp.json'), JSON.stringify(existingConfig, null, 2));

const bundled = ['devcontext-mcp.exe', 'DevContext.Server.dll'].filter((f) => existsSync(join(BUNDLE_DIR, f)));
check('the publish puts the MCP binary beside the server in the bundle directory',
  bundled.length === 2, `${BUNDLE_DIR}: ${bundled.join(', ') || 'nothing'}`);

console.log('  … agent: analyze(temp copy of ControllerApp)');
const analyzeText = await agentAnalyze(workRepo);
check('an agent analyze produced a session for the page to write into',
  analyzeText.length > 0, analyzeText.replace(/\s+/g, ' ').slice(0, 90));

const browser = await chromium.launch();
const page = await browser.newPage();
const textOf = (id: string) =>
  page.locator(`[data-testid="${id}"]`).first().textContent().then((t) => (t ?? '').trim()).catch(() => '');

try {
  mkdirSync(OUT, { recursive: true });
  await page.goto(APP, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="mcp-status-card"]', { timeout: 30_000 });
  await page.waitForTimeout(1_500);

  const binaryPath = await textOf('mcp-binary-path');
  const setupNote = await textOf('mcp-setup-note');
  const repoShown = await textOf('write-config-repo');

  const commands: Record<string, string> = {};
  for (const host of ['claude', 'cursor', 'vscode']) {
    const snippet = await textOf(`snippet-${host}`);
    const parsed = JSON.parse(snippet);
    const servers = parsed.mcpServers ?? parsed.servers;
    commands[host] = servers?.devcontext?.command ?? '';
  }
  const allNameTheProbedPath = Object.values(commands).every((c) => c === binaryPath) && binaryPath.length > 0;
  check('every host snippet names the ABSOLUTE path the probe resolved',
    allNameTheProbedPath, `probe: ${binaryPath} | snippets: ${JSON.stringify(commands)}`);
  check('no page-side placeholder survives',
    !JSON.stringify(commands).includes('C:/path/to/'), JSON.stringify(commands).slice(0, 120));
  check('the page targets the repo the agent just analyzed',
    repoShown.replace(/\\/g, '/').toLowerCase() === workRepo.replace(/\\/g, '/').toLowerCase(),
    `shown: ${repoShown} | analyzed: ${workRepo}`);

  await page.screenshot({ path: `${OUT}/N4.2-host-config.png`, fullPage: true });

  console.log('  … clicking Write .mcp.json');
  await page.click('[data-testid="write-config-claude"]');
  await page.waitForSelector('[data-testid="write-result-claude"]', { timeout: 30_000 });
  const firstResult = await textOf('write-result-claude');

  const onDisk = JSON.parse(readFileSync(join(workRepo, '.mcp.json'), 'utf8'));
  check('the config was written into the analyzed repo, naming the probed binary',
    onDisk.mcpServers?.devcontext?.command === binaryPath,
    `${join(workRepo, '.mcp.json')} -> ${onDisk.mcpServers?.devcontext?.command}`);
  check('the MCP server that was already registered there survived the merge',
    onDisk.mcpServers?.github?.command === 'gh-mcp', JSON.stringify(onDisk.mcpServers ?? {}).slice(0, 140));
  check('the page reported an UPDATE, because a config was already there',
    firstResult.startsWith('Updated'), firstResult);
  check('the written command is a file that exists', existsSync(onDisk.mcpServers?.devcontext?.command ?? ''),
    onDisk.mcpServers?.devcontext?.command ?? '');

  console.log('  … clicking Write .vscode/mcp.json');
  await page.click('[data-testid="write-config-vscode"]');
  await page.waitForSelector('[data-testid="write-result-vscode"]', { timeout: 30_000 });
  const vscodeResult = await textOf('write-result-vscode');
  const vscodeOnDisk = existsSync(join(workRepo, '.vscode', 'mcp.json'))
    ? JSON.parse(readFileSync(join(workRepo, '.vscode', 'mcp.json'), 'utf8'))
    : {};
  check('a host with its own file layout and key gets that layout',
    vscodeOnDisk.servers?.devcontext?.type === 'stdio' && vscodeOnDisk.servers?.devcontext?.command === binaryPath,
    `${vscodeResult} -> ${JSON.stringify(vscodeOnDisk).slice(0, 140)}`);

  console.log('  … clicking Write .mcp.json again');
  await page.click('[data-testid="write-config-claude"]');
  await page.waitForTimeout(1_000);
  const secondResult = await textOf('write-result-claude');
  check('a second click reports UNCHANGED instead of claiming another write',
    secondResult.includes('already points here'), secondResult);

  await page.screenshot({ path: `${OUT}/N4.2-config-written.png`, fullPage: true });

  writeFileSync(`${OUT}/N4.2-live-probe.json`, JSON.stringify({
    bundleDirectory: BUNDLE_DIR, bundled, binaryPath, setupNote, repoShown, commands,
    firstResult, vscodeResult, secondResult,
    mcpJson: onDisk, vscodeMcpJson: vscodeOnDisk, workRepo, failures,
  }, null, 2));
} finally {
  await browser.close();
}

console.log(failures.length === 0 ? '\nALL CHECKS PASSED' : `\n${failures.length} CHECK(S) FAILED`);
process.exit(failures.length === 0 ? 0 : 1);
