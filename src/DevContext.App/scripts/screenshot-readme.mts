/**
 * README screenshot capture — drives the Angular app via Playwright in light theme,
 * analyzes the eShop microservices repo, and captures 12 screenshots for the README.
 *
 * Usage:
 *   pnpm server                     # in one terminal
 *   pnpm dev:web                    # in another
 *   node --experimental-strip-types scripts/screenshot-readme.mts
 *
 * If server/web are already running, pass --no-spawn to skip process management.
 * Output: docs/screenshots/ (overwrites existing files).
 */

import { chromium } from 'playwright';
import { spawn, type ChildProcess } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const ROOT = join(__dirname, '..', '..', '..');

const APP_URL = 'http://localhost:4200';
const SERVER_URL = 'http://127.0.0.1:5179';
const TARGET_REPO =
  'C:\\Users\\shahi\\source\\repos\\run-aspnetcore-microservices\\src\\eshop-microservices.sln';
const SCREENSHOT_DIR = join(ROOT, 'docs', 'screenshots');
const NO_SPAWN = process.argv.includes('--no-spawn');
const SETTLE_MS = 1200;
const TIMEOUT = 240_000; // 4 min for analysis

function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}

async function retry(
  fn: () => Promise<boolean>,
  label: string,
  timeoutMs: number,
): Promise<void> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (await fn()) return;
    await sleep(800);
  }
  throw new Error(`Timed out waiting for: ${label}`);
}

// ── Process management ────────────────────────────────────

let serverProc: ChildProcess | null = null;
let webProc: ChildProcess | null = null;

function spawnServer(): void {
  console.log('[spawn] Starting .NET server…');
  const dll = join(
    ROOT,
    'src',
    'DevContext.Server',
    'bin',
    'Debug',
    'net10.0',
    'DevContext.Server.dll',
  );
  serverProc = spawn('dotnet', [dll, '--urls', SERVER_URL], {
    stdio: 'ignore',
    detached: true,
  });
  serverProc.unref();
}

function spawnWeb(): void {
  console.log('[spawn] Starting Angular dev server…');
  webProc = spawn('pnpm', ['ng', 'serve', '--port', '4200'], {
    cwd: join(ROOT, 'src', 'DevContext.App'),
    stdio: 'ignore',
    detached: true,
  });
  webProc.unref();
}

async function waitForServer(): Promise<void> {
  await retry(
    async () => {
      try {
        const r = await fetch(`${SERVER_URL}/health`);
        return r.ok;
      } catch {
        return false;
      }
    },
    'server /health',
    90_000,
  );
  console.log('[ready] .NET server is up');
}

async function waitForWeb(): Promise<void> {
  await retry(
    async () => {
      try {
        const r = await fetch(APP_URL);
        return r.ok;
      } catch {
        return false;
      }
    },
    'Angular dev server',
    120_000,
  );
  console.log('[ready] Angular dev server is up');
}

function killAll(): void {
  for (const proc of [serverProc, webProc]) {
    if (proc?.pid) {
      try {
        process.kill(-proc.pid, 'SIGTERM');
      } catch {
        /* already dead */
      }
    }
  }
}

// ── Screenshot helpers ────────────────────────────────────

const results: Array<{ name: string; ok: boolean; error?: string }> = [];

async function capture(
  page: any,
  name: string,
  fn: () => Promise<void>,
): Promise<void> {
  console.log(`  [capture] ${name}…`);
  try {
    await fn();
    await page.screenshot({
      path: join(SCREENSHOT_DIR, `${name}.png`),
      fullPage: true,
    });
    results.push({ name, ok: true });
    console.log(`    ✓ ${name}.png`);
  } catch (err: any) {
    // Try a last-resort screenshot even on error
    try {
      await page.screenshot({
        path: join(SCREENSHOT_DIR, `${name}.png`),
        fullPage: true,
      });
    } catch {}
    const msg = err?.message ?? String(err);
    results.push({ name, ok: false, error: msg });
    console.log(`    ✗ ${name}.png — ${msg}`);
  }
}

function writeReport(): void {
  const md = `# README Screenshot Capture

**Date:** ${new Date().toISOString().split('T')[0]}
**Target repo:** \`${TARGET_REPO}\`

| Screenshot | Status |
|---|---|
${results.map((r) => `| \`${r.name}.png\` | ${r.ok ? '✅' : '❌ ' + (r.error ?? '')} |`).join('\n')}
`;
  writeFileSync(join(SCREENSHOT_DIR, 'README.md'), md, 'utf-8');
  console.log(`\nReport → docs/screenshots/README.md`);
}

// ── Main ──────────────────────────────────────────────────

async function main() {
  mkdirSync(SCREENSHOT_DIR, { recursive: true });
  console.log(`Screenshots → ${SCREENSHOT_DIR}\n`);

  // 1. Start infrastructure
  if (!NO_SPAWN) {
    spawnServer();
    await waitForServer();
    spawnWeb();
    await waitForWeb();
  } else {
    console.log('[skip] Using existing server/web (--no-spawn)');
    await waitForServer();
    await waitForWeb();
  }

  // 2. Launch browser (light theme)
  const browser = await chromium.launch({ channel: 'chrome', headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
    colorScheme: 'light',
  });

  // Pre-set localStorage so app boots with modern vibe + light theme
  await context.addInitScript(() => {
    localStorage.setItem('devcontext-vibe', 'modern');
    localStorage.setItem('devcontext-theme', 'light');
  });

  const page = await context.newPage();

  try {
    // ── Home: analyze repo ──────────────────────────────
    await capture(page, 'home', async () => {
      await page.goto(APP_URL, { waitUntil: 'networkidle' });
      await sleep(SETTLE_MS);

      // Wait for server connection (connection dot)
      console.log('  Waiting for server connection…');
      await sleep(3000);

      // Enter repo path
      const input = page.locator('app-start-hero input[type="text"]');
      await input.waitFor({ timeout: 30_000 });
      await input.fill(TARGET_REPO);
      await sleep(500);

      // Click Analyze
      const analyzeBtn = page.locator('app-start-hero app-button:has-text("Analyze")');
      await analyzeBtn.click();
      console.log('  Analyzing eShop repo (may take 2-3 min)…');

      // Wait for analysis — look for identity strip on home page
      await page.waitForSelector('app-identity-strip', { timeout: TIMEOUT });
      await sleep(SETTLE_MS);
    });

    // ── Explore: entry deck + system topology ──────────
    await capture(page, 'explore', async () => {
      await page.goto(`${APP_URL}/explore`, { waitUntil: 'networkidle' });
      await sleep(2000);

      // Wait for entry deck to populate
      await page.waitForSelector('app-entry-deck', { timeout: 15_000 });
      await sleep(SETTLE_MS);
    });

    // ── Explore detail: entry click + trace + inspector ─
    await capture(page, 'explore-detail', async () => {
      // Make sure we're on explore page
      if (!page.url().includes('/explore')) {
        await page.goto(`${APP_URL}/explore`, { waitUntil: 'networkidle' });
        await sleep(2000);
      }

      // Click the first entry row in the deck
      const entryRow = page.locator('app-entry-deck .list-row').first();
      if (await entryRow.count()) {
        await entryRow.click();
        console.log('  Clicked first entry row');
      } else {
        // Fallback: click any button-like element in the deck
        const anyEntry = page.locator('app-entry-deck button, app-entry-deck [role="button"]').first();
        if (await anyEntry.count()) {
          await anyEntry.click();
          console.log('  Clicked first deck button');
        }
      }
      await sleep(2000);

      // Open inspector: press Ctrl+Shift+L to toggle dock
      // First check if inspector is already visible
      const inspector = page.locator('app-inspector');
      if ((await inspector.count()) === 0 || !(await inspector.isVisible())) {
        await page.keyboard.press('Control+L'); // simpler shortcut if available
        await sleep(800);
      }

      // If inspector still not visible, try Ctrl+Shift+L
      if ((await inspector.count()) === 0 || !(await inspector.isVisible())) {
        await page.keyboard.press('Control+Shift+L');
        await sleep(800);
      }

      await sleep(SETTLE_MS);
    });

    // ── Code pane: inspector code tab ──────────────────
    await capture(page, 'code-pane', async () => {
      // Make sure we're on explore with inspector open
      if (!page.url().includes('/explore')) {
        await page.goto(`${APP_URL}/explore`, { waitUntil: 'networkidle' });
        await sleep(2000);
      }

      // Try to click a "Code" tab if present in inspector
      const codeTab = page.locator('app-inspector button:has-text("Code")');
      if (await codeTab.count()) {
        await codeTab.click();
        await sleep(1500);
      } else {
        // Try looking for any tab-like elements in inspector
        const tabs = page.locator('app-inspector [role="tab"], app-inspector .tab, app-inspector button.chip');
        const count = await tabs.count();
        for (let i = 0; i < count; i++) {
          const text = await tabs.nth(i).textContent();
          if (text?.toLowerCase().includes('code') || text?.toLowerCase().includes('source')) {
            await tabs.nth(i).click();
            await sleep(1500);
            break;
          }
        }
      }

      await sleep(SETTLE_MS);
    });

    // ── Graph: switch to graph view ───────────────────
    await capture(page, 'graph', async () => {
      await page.goto(`${APP_URL}/explore`, { waitUntil: 'networkidle' });
      await sleep(2000);

      // First click an entry to get a trace
      const entryRow = page.locator('app-entry-deck .list-row').first();
      if (await entryRow.count()) {
        await entryRow.click();
        await sleep(2000);
      }

      // Click "Graph" button in the stage toolbar
      const graphBtn = page.locator('app-stage button.chip:has-text("Graph")');
      if (await graphBtn.count()) {
        await graphBtn.click();
        await sleep(2000);
      } else {
        // Try "Flow" lens first then graph
        const flowBtn = page.locator('app-lens-switcher button.chip:has-text("Flow")');
        if (await flowBtn.count()) {
          await flowBtn.click();
          await sleep(500);
          const graphBtn2 = page.locator('app-stage button.chip:has-text("Graph")');
          if (await graphBtn2.count()) {
            await graphBtn2.click();
            await sleep(2000);
          }
        }
      }

      await sleep(SETTLE_MS);
    });

    // ── Atlas ──────────────────────────────────────────
    await capture(page, 'atlas', async () => {
      await page.goto(`${APP_URL}/atlas`, { waitUntil: 'networkidle' });
      await sleep(2000);
      await page.waitForSelector('app-atlas-page', { timeout: 15_000 });
      await sleep(SETTLE_MS);
    });

    // ── Insights ───────────────────────────────────────
    await capture(page, 'insights', async () => {
      await page.goto(`${APP_URL}/insights`, { waitUntil: 'networkidle' });
      await sleep(2000);
      await page.waitForSelector('app-insights-page', { timeout: 15_000 });
      await sleep(SETTLE_MS);
    });

    // ── Table lens ─────────────────────────────────────
    await capture(page, 'table-lens', async () => {
      await page.goto(`${APP_URL}/explore`, { waitUntil: 'networkidle' });
      await sleep(2000);

      // Click "Table" button in the lens switcher
      const tableBtn = page.locator('app-lens-switcher button:has-text("Table")');
      if (await tableBtn.count()) {
        await tableBtn.click();
        await sleep(2000);
      }

      // The table opens as full-screen overlay
      await page.waitForSelector('app-table-lens', { timeout: 10_000 }).catch(() => {});
      await sleep(SETTLE_MS);
    });

    // ── Context Studio ─────────────────────────────────
    await capture(page, 'context-studio', async () => {
      await page.goto(`${APP_URL}/context`, { waitUntil: 'networkidle' });
      await sleep(2000);

      // Wait for the context studio to render
      await page.waitForSelector('app-context-studio', { timeout: 15_000 }).catch(() => {});
      // Click scope picker items if available to populate composition
      const scopeItem = page.locator('app-context-studio .list-row, app-context-studio [role="treeitem"]').first();
      if (await scopeItem.count()) {
        await scopeItem.click();
        await sleep(1500);
      }
      await sleep(SETTLE_MS);
    });

    // ── MCP ────────────────────────────────────────────
    await capture(page, 'mcp', async () => {
      await page.goto(`${APP_URL}/mcp`, { waitUntil: 'networkidle' });
      await sleep(2000);
      await page.waitForSelector('app-mcp-page', { timeout: 15_000 }).catch(() => {});
      await sleep(SETTLE_MS);
    });

    // ── Export (Context Studio with copy action) ──────
    await capture(page, 'export', async () => {
      await page.goto(`${APP_URL}/context`, { waitUntil: 'networkidle' });
      await sleep(2000);

      // Try to click "Copy" or "Export" button
      const copyBtn = page.locator('app-context-studio button:has-text("Copy"), app-context-studio button:has-text("Export")').first();
      if (await copyBtn.count()) {
        await copyBtn.click();
        await sleep(1500);
      }

      await sleep(SETTLE_MS);
    });

    // ── Settings ───────────────────────────────────────
    await capture(page, 'settings', async () => {
      await page.goto(`${APP_URL}/settings`, { waitUntil: 'networkidle' });
      await sleep(2000);
      await page.waitForSelector('app-settings-view, app-settings', { timeout: 15_000 }).catch(() => {});
      await sleep(SETTLE_MS);
    });

    // ── Write report ──────────────────────────────────
    writeReport();
    console.log('\nAll captures complete.');
  } catch (err) {
    console.error('Fatal error:', err);
  } finally {
    await browser.close();
    if (!NO_SPAWN) {
      killAll();
    }
  }
}

main().catch((err) => {
  console.error(err);
  if (!NO_SPAWN) killAll();
  process.exit(1);
});
