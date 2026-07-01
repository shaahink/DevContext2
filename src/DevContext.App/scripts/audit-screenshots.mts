/**
 * Headless screenshot audit — drives the Angular app via Playwright,
 * captures before/during/after states on every page, and writes an
 * audit report to __screenshots__/audit.md.
 *
 * Usage:
 *   pnpm server           # in one terminal (or let this script spawn it)
 *   pnpm dev:web          # in another
 *   node --experimental-strip-types scripts/audit-screenshots.mts
 *
 * If the server/web are already running on 127.0.0.1:5179 / :4200,
 * pass --no-spawn to skip process management.
 */

import { chromium, type Browser, type Page } from 'playwright';
import { spawn, type ChildProcess } from 'node:child_process';
import { mkdirSync, writeFileSync, existsSync } from 'node:fs';
import { join } from 'node:path';

// ── Config ────────────────────────────────────────────────

const BASE = 'http://localhost:4200';
const SERVER_URL = 'http://127.0.0.1:5179';
const TARGET = process.env.TARGET_REPO ?? 'C:\\Code\\Shamshir\\TradingEngine.slnx';
const SCREENSHOT_DIR = join(process.cwd(), '__screenshots__');
const NO_SPAWN = process.argv.includes('--no-spawn');
const TIMEOUT = 120_000; // 2 min for analysis
const SETTLE_MS = 800;

// ── Helpers ───────────────────────────────────────────────

async function retry(fn: () => Promise<boolean>, label: string, timeoutMs: number): Promise<void> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (await fn()) return;
    await sleep(500);
  }
  throw new Error(`Timed out waiting for: ${label}`);
}

function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}

function shot(page: Page, name: string): Promise<void> {
  const fp = join(SCREENSHOT_DIR, `${name}.png`);
  return page.screenshot({ path: fp, fullPage: true });
}

// ── Process management ────────────────────────────────────

let serverProc: ChildProcess | null = null;
let webProc: ChildProcess | null = null;

function spawnServer(): void {
  console.log('[spawn] Starting .NET server…');
  const dll = '..\\DevContext.Server\\bin\\Debug\\net10.0\\DevContext.Server.dll';
  // Use stdio:'ignore' so the process isn't tied to the parent's stdio
  serverProc = spawn('dotnet', [dll, '--urls', SERVER_URL], {
    stdio: 'ignore',
    detached: true,
  });
  serverProc.unref();
  serverProc.on('exit', (code) => console.log(`[spawn] Server exited with code ${code}`));
}

function spawnWeb(): void {
  console.log('[spawn] Starting Angular dev server…');
  webProc = spawn('pnpm', ['ng', 'serve', '--port', '4200'], {
    stdio: 'ignore',
    detached: true,
  });
  webProc.unref();
  webProc.on('exit', (code) => console.log(`[spawn] Web server exited with code ${code}`));
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
        const r = await fetch(BASE);
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
  if (serverProc?.pid) {
    try { process.kill(-serverProc.pid, 'SIGTERM'); } catch { /* already dead */ }
  }
  if (webProc?.pid) {
    try { process.kill(-webProc.pid, 'SIGTERM'); } catch { /* already dead */ }
  }
}

// ── Page helpers ──────────────────────────────────────────

async function navigate(page: Page, route: string): Promise<void> {
  await page.goto(`${BASE}${route}`, { waitUntil: 'networkidle' });
  await sleep(SETTLE_MS);
}

/** Click on the vibe switcher button to cycle to the next vibe. */
async function cycleVibe(page: Page): Promise<void> {
  await page.click('app-title-bar button');
  await sleep(400);
}

/** Wait until the route includes the given fragment. */
async function waitForRoute(page: Page, fragment: string, timeoutMs: number = TIMEOUT): Promise<void> {
  await page.waitForURL((url) => url.pathname.includes(fragment), { timeout: timeoutMs });
  await sleep(SETTLE_MS);
}

// ── Audit sections ────────────────────────────────────────

const notes: string[] = [];
function note(msg: string): void {
  notes.push(msg);
  console.log(`  [note] ${msg}`);
}

async function auditSource(page: Page, browser: Browser): Promise<void> {
  console.log('\n── Source / ──');
  await navigate(page, '/');

  // 1 — Empty state
  await shot(page, '01-source-empty');
  note('Source: empty state loads with terminal vibe (amber accent, dark base, zero radius).');

  // 2 — Verify terminal vibe is default
  const vibeAttr = await page.evaluate(() => document.documentElement.getAttribute('data-vibe'));
  note(`Default vibe: "${vibeAttr}" — ${vibeAttr === 'terminal' ? '✅ terminal is default' : '❌ expected terminal'}`);

  // 3 — Typing state: enter path
  await page.fill('input[placeholder*="Path"]', TARGET);
  await sleep(600);
  await shot(page, '02-source-typing');
  note('Source: path entered, "Local path" hint visible, Analyze button enabled.');

  // 4 — Click Analyze, capture during
  await page.click('app-button[variant="primary"]');
  await sleep(2500); // let the server start streaming progress
  await shot(page, '03-source-analyzing');
  note('Source: analyzing — spinner, stage label, and progress bar visible. Analyze button should be disabled.');

  // Verify Analyze button is disabled
  const btnDisabled = await page.evaluate(() => {
    const el = document.querySelector('app-button[variant="primary"]');
    return el?.getAttribute('aria-disabled') === 'true';
  });
  note(`Analyze button disabled: ${btnDisabled ? '✅' : '❌'}`);

  // 5 — Wait for analysis to finish (auto-redirect to /overview)
  console.log('  Waiting for analysis to complete (may take 30-60s)…');
  try {
    await waitForRoute(page, '/overview', TIMEOUT);
    await shot(page, '04-overview-post-analysis');
    note('Overview: auto-navigated after analysis. Architecture cards populated with data.');
  } catch {
    await shot(page, '04-analysis-timeout');
    note('❌ Analysis timed out — server may have failed. Check server logs.');
  }
}

async function auditOverview(page: Page): Promise<void> {
  console.log('\n── Overview /overview ──');
  if (!page.url().includes('/overview')) {
    await navigate(page, '/overview');
  }
  await shot(page, '05-overview-detail');
  note('Overview: archetype, topology, packages, aggregates, pipeline, library surface visible.');
}

async function auditEntries(page: Page): Promise<void> {
  console.log('\n── Entries /entries ──');
  await navigate(page, '/entries');
  await sleep(1000); // let entries load
  await shot(page, '06-entries');
  note('Entries: entry point groups with counts visible.');
}

async function auditBrowse(page: Page): Promise<void> {
  console.log('\n── Browse /browse ──');
  await navigate(page, '/browse');

  // Type a search
  const searchInput = page.locator('app-search-field input');
  if (await searchInput.count()) {
    await searchInput.fill('Service');
    await sleep(2000); // debounce + RPC
    await shot(page, '07-browse-search');
    note('Browse: search results visible with kind/tag badges.');

    // Click first result to show node detail
    const firstRow = page.locator('app-browse-view > div > div:first-child [tabindex="0"]');
    if (await firstRow.count()) {
      await firstRow.first().click();
      await sleep(1000);
      await shot(page, '08-browse-node-detail');
      note('Browse: node detail panel with neighbors buttons visible.');
    }
  } else {
    await shot(page, '07-browse-empty');
    note('Browse: no search input found — may need analysis first.');
  }
}

async function auditTrace(page: Page): Promise<void> {
  console.log('\n── Trace /trace ──');
  await navigate(page, '/trace');
  await sleep(1500);
  await shot(page, '09-trace');
  note('Trace: call graph view with focus/depth controls.');
}

async function auditDocument(page: Page): Promise<void> {
  console.log('\n── Document /document ──');
  await navigate(page, '/document');

  // Before render
  await shot(page, '10-document-before-render');
  note('Document: before render — placeholder text visible, Render button enabled.');

  // Click Render
  const renderBtn = page.locator('app-button', { hasText: 'Render' });
  if (await renderBtn.count()) {
    await renderBtn.click();
    console.log('  Waiting for document render…');
    await page.waitForSelector('pre', { timeout: 30_000 }).catch(() => {});
    await sleep(1000);
    await shot(page, '11-document-after-render');
    note('Document: markdown content rendered, sections sidebar populated with token counts.');

    // Click Copy for toast
    const copyBtn = page.locator('app-button', { hasText: 'Copy' });
    if (await copyBtn.count()) {
      await copyBtn.click();
      await sleep(600);
      await shot(page, '12-document-copy-toast');
      note('Document: copy toast visible — "Copied to clipboard" success message.');
    }
  }
}

async function auditStats(page: Page): Promise<void> {
  console.log('\n── Stats /stats ──');
  await navigate(page, '/stats');

  // Stats auto-loads in constructor; wait for data or click Refresh
  await sleep(2000);
  const refreshBtn = page.locator('app-button', { hasText: 'Refresh' });
  if (await refreshBtn.count()) {
    await refreshBtn.click();
    await sleep(2000);
  }
  await shot(page, '13-stats');
  note('Stats: stages, extractors, cache, graph, corpus, seams tables visible with tabular-nums.');

  // Verify tabular-nums class is present on stat cells
  const tabularCount = await page.evaluate(() =>
    document.querySelectorAll('.tabular-nums').length,
  );
  note(`Stats tabular-nums elements: ${tabularCount} — ${tabularCount > 0 ? '✅' : '❌'}`);
}

async function auditCache(page: Page): Promise<void> {
  console.log('\n── Cache /cache ──');
  await navigate(page, '/cache');
  await sleep(1000);
  await shot(page, '14-cache');
  note('Cache: recent runs list visible.');
}

async function auditVibeSwitcher(page: Page): Promise<void> {
  console.log('\n── Vibe switcher ──');
  await navigate(page, '/');

  // Initial
  const initialVibe = await page.evaluate(() =>
    document.documentElement.getAttribute('data-vibe'),
  );
  note(`Vibe before cycling: "${initialVibe}"`);

  // Cycle to modern
  await cycleVibe(page);
  const vibe1 = await page.evaluate(() =>
    document.documentElement.getAttribute('data-vibe'),
  );
  note(`After first click: "${vibe1}"`);

  // Cycle to hacker
  await cycleVibe(page);
  const vibe2 = await page.evaluate(() =>
    document.documentElement.getAttribute('data-vibe'),
  );
  note(`After second click: "${vibe2}"`);
  await shot(page, '15-vibe-hacker');
  note('Vibe: hacker skin (green-on-black) applied via switcher.');

  // Cycle back to terminal
  await cycleVibe(page);
  await shot(page, '16-vibe-terminal');
  note('Vibe: cycled back to terminal — amber-on-black with scanline.');

  note(
    `Vibe cycling: terminal→${vibe1}→${vibe2}→terminal — ${vibe1 !== initialVibe && vibe2 !== vibe1 ? '✅' : '❌'}`,
  );
}

async function auditErrorState(page: Page, browser: Browser): Promise<void> {
  console.log('\n── Error state ──');
  await navigate(page, '/browse');

  // Trigger a search
  const searchInput = page.locator('app-search-field input');
  if (await searchInput.count()) {
    await searchInput.fill('Service');
    await sleep(300); // debounce starts

    // Kill the server mid-request
    console.log('  Killing server to trigger error…');
    serverProc?.kill('SIGTERM');
    await sleep(1000);

    // Force a new request that will fail
    await searchInput.fill('ErrorTest');
    await sleep(4000);

    await shot(page, '17-error-toast');
    note('Error: server killed mid-Browse — toast with error message should be visible.');

    // Check for toast element
    const toastVisible = await page.evaluate(() =>
      document.querySelector('app-toast div') !== null,
    );
    note(`Error toast visible: ${toastVisible ? '✅' : '❌ — toast may have auto-dismissed or not triggered'}`);
  }
}

async function auditCursor(page: Page): Promise<void> {
  console.log('\n── Cursor check ──');
  await navigate(page, '/');
  await sleep(500);

  const cursorStyle = await page.evaluate(() => {
    const btn = document.querySelector('app-button');
    if (!btn) return 'no button found';
    const style = window.getComputedStyle(btn);
    const hostCursor = style.cursor;
    // Also check inner element
    const inner = btn.shadowRoot?.querySelector('button') ?? btn.firstElementChild;
    const innerCursor = inner ? window.getComputedStyle(inner).cursor : 'no-inner';
    return `host:${hostCursor} inner:${innerCursor}`;
  });
  note(`Button cursor: ${cursorStyle}`);
}

// ── Audit report ──────────────────────────────────────────

function writeReport(): void {
  const md = `# DevContext Desktop — UI/UX Screenshot Audit

**Date:** ${new Date().toISOString().split('T')[0]}
**Target repo:** \`${TARGET}\`
**Branch:** \`feat/desktop-v2\`

---

## Gate checklist

| # | Check | Phase | Expected | Observed |
|---|---|---|---|---|
| 1 | Disabled buttons dim + unclickable during busy | P0 | \`opacity-40\` + \`aria-disabled=true\` | See #03 |
| 2 | Progress bar tracks real engine % | P0 | Bar width matches streamed percent | See #03 |
| 3 | Cursor is pointer over buttons | P0 | \`cursor: pointer\` on host | See cursor check |
| 4 | Toast on RPC error | P0 | Error toast visible after server kill | See #17 |
| 5 | Terminal is default vibe | P1 | \`data-vibe="terminal"\` on first load | See #01 |
| 6 | Scanline texture visible (terminal) | P1 | Subtle horizontal lines overlay | See #01/#16 (zoom) |
| 7 | Vibe switcher cycles live + persists | P1 | Click cycles terminal→modern→hacker | See #15/#16 |
| 8 | Consistent type scale (no ad-hoc sizes) | P1 | All text uses \`text-2xs\`/\`text-3xs\`/\`text-xs\` | Audit all |
| 9 | Tabular nums in stats columns | P1 | \`tabular-nums\` class on stat cells | See #13 |
| 10 | Phosphor glow on focus (terminal) | P1 | Amber box-shadow, not blue outline | Visual |
| 11 | No dead controls or phantom zeros | general | All buttons functional, stats populated | All pages |
| 12 | Source boot-log framing | P1 | "DevContext" branding, terminal character | See #01 |

---

## Screenshots

| # | File | Page | State |
|---|---|---|---|
| 01 | \`01-source-empty.png\` | Source / | Empty — first load, terminal vibe |
| 02 | \`02-source-typing.png\` | Source / | Typing — path entered, hint visible |
| 03 | \`03-source-analyzing.png\` | Source / | During analysis — progress bar visible |
| 04 | \`04-overview-post-analysis.png\` | Overview /overview | After analysis — architecture cards |
| 05 | \`05-overview-detail.png\` | Overview /overview | Scrolled detail |
| 06 | \`06-entries.png\` | Entries /entries | Entry point groups |
| 07 | \`07-browse-search.png\` | Browse /browse | Search results |
| 08 | \`08-browse-node-detail.png\` | Browse /browse | Node detail + neighbors |
| 09 | \`09-trace.png\` | Trace /trace | Call graph |
| 10 | \`10-document-before-render.png\` | Document /document | Before render |
| 11 | \`11-document-after-render.png\` | Document /document | After render — markdown |
| 12 | \`12-document-copy-toast.png\` | Document /document | Copy toast visible |
| 13 | \`13-stats.png\` | Stats /stats | All stat tables |
| 14 | \`14-cache.png\` | Cache /cache | Recent runs |
| 15 | \`15-vibe-hacker.png\` | Source / | Hacker vibe via switcher |
| 16 | \`16-vibe-terminal.png\` | Source / | Terminal vibe restored |
| 17 | \`17-error-toast.png\` | Browse /browse | Error toast after server kill |

---

## Audit notes

${notes.map((n) => `- ${n}`).join('\n')}
`;

  writeFileSync(join(SCREENSHOT_DIR, 'audit.md'), md, 'utf-8');
  console.log(`\n✓ Audit report written to __screenshots__/audit.md`);
}

// ── Main ──────────────────────────────────────────────────

async function main(): Promise<void> {
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

  // 2. Launch browser
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
  });
  const page = await context.newPage();

  try {
    // P0/P1 audit sequence
    await auditSource(page, browser);
    await auditOverview(page);
    await auditEntries(page);
    await auditBrowse(page);
    await auditTrace(page);
    await auditDocument(page);
    await auditStats(page);
    await auditCache(page);
    await auditVibeSwitcher(page);
    await auditCursor(page);

    // Error state LAST (kills the server)
    if (!NO_SPAWN) {
      // Re-spawn server for error test if it's still up from earlier
      if (serverProc?.exitCode != null) {
        spawnServer();
        await waitForServer();
      }
      await auditErrorState(page, browser);
    } else {
      note('⚠️ Skipping error state test (--no-spawn) — server must be killed manually.');
    }

  } catch (err) {
    console.error('Audit error:', err);
    note(`❌ Fatal: ${err instanceof Error ? err.message : String(err)}`);
  } finally {
    // 3. Write report
    writeReport();

    // 4. Cleanup
    await browser.close();
    if (!NO_SPAWN) {
      killAll();
    }
    console.log('\nDone.');
  }
}

main().catch((err) => {
  console.error(err);
  killAll();
  process.exit(1);
});
