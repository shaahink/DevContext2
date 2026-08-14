/**
 * README screenshot capture — drives Angular + Playwright in light theme,
 * analyzes eShop microservices, captures 12 screenshots for the README.
 *
 * AI-agent friendly patterns:
 *  - Each shot has a hard 90s sub-timeout via Promise.race
 *  - Progress heartbeat every 10s during long waits
 *  - Never blocks the sequence — errors caught per-shot
 *  - Running tally after every shot
 *  - --no-spawn: relies on external process management
 */

import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { seedAgentCalls } from './seed-agent-calls.mjs';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const ROOT = join(__dirname, '..', '..', '..');

const APP_URL = 'http://localhost:4200';
const TARGET_REPO =
  'C:\\Users\\shahi\\source\\repos\\run-aspnetcore-microservices\\src\\eshop-microservices.sln';
const SHOT_DIR = join(ROOT, 'docs', 'screenshots');
const SHOT_TIMEOUT = 90_000; // per-shot hard limit
// Z1.2 — the MCP shot drives a real sidecar first (see seed-agent-calls.mjs), so it needs more
// room than a navigate-and-wait shot. Still bounded: one slow shot must not eat the sequence.
const MCP_SHOT_TIMEOUT = 360_000;
const SERVER_URL = 'http://127.0.0.1:5179'; // start-dev-bg.ps1's $ServerUrl
const SETTLE_MS = 1200;
const ANALYZE_TIMEOUT = 300_000; // first analysis can be slow

// ── Progress helpers ────────────────────────────────────

let shotsOk = 0;
let shotsTotal = 0;

function progress(name: string, ok: boolean, msg?: string): void {
  const icon = ok ? '✓' : '✗';
  console.log(`  [${shotsOk}/${shotsTotal}] ${icon} ${name}.png${msg ? ' — ' + msg : ''}`);
}

// ── Wait with heartbeat ─────────────────────────────────

function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}

async function waitWithHeartbeat<T>(
  promise: Promise<T>,
  label: string,
  intervalMs = 10_000,
): Promise<T> {
  const heartbeat = setInterval(() => {
    console.log(`    ⌛ still waiting: ${label} (${(intervalMs / 1000).toFixed(0)}s tick)`);
  }, intervalMs);
  try {
    return await promise;
  } finally {
    clearInterval(heartbeat);
  }
}

// ── Timeout wrapper per-shot ────────────────────────────

async function withTimeout<T>(promise: Promise<T>, ms: number, label: string): Promise<T> {
  let timer: ReturnType<typeof setTimeout>;
  const timeout = new Promise<never>((_, reject) => {
    timer = setTimeout(() => {
      console.log(`    ⏰ timed out after ${(ms / 1000).toFixed(0)}s: ${label}`);
      reject(new Error(`Timeout: ${label}`));
    }, ms);
  });
  try {
    return await Promise.race([promise, timeout]);
  } finally {
    clearTimeout(timer!);
  }
}

// ── Navigation helpers ──────────────────────────────────

async function navigate(page: any, url: string): Promise<void> {
  console.log(`    → navigating to ${url.replace(APP_URL, '')}`);
  await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 15_000 }).catch(() => {
    // retry once
    return page.goto(url, { waitUntil: 'domcontentloaded', timeout: 15_000 });
  });
  await sleep(SETTLE_MS);
}

async function waitVisible(page: any, selector: string, timeoutMs = 12_000): Promise<boolean> {
  try {
    await withTimeout(
      waitWithHeartbeat(
        page.waitForSelector(selector, { timeout: timeoutMs }),
        `selector '${selector}'`,
      ),
      timeoutMs + 5_000,
      `waitVisible(${selector})`,
    );
    return true;
  } catch {
    console.log(`    ⚠ selector '${selector}' not found within ${(timeoutMs / 1000).toFixed(0)}s`);
    return false;
  }
}

async function clickIfExists(page: any, locator: any, label: string, afterMs = 1200): Promise<boolean> {
  const count = await locator.count().catch(() => 0);
  if (count > 0) {
    console.log(`    → clicking ${label}`);
    try {
      await withTimeout(locator.first().click({ timeout: 5_000 }), 10_000, `click ${label}`);
      await sleep(afterMs);
      return true;
    } catch (e: any) {
      console.log(`    ⚠ click '${label}' failed: ${e.message?.slice(0, 60)}`);
      return false;
    }
  }
  return false;
}

// ── Capture (each shot runs inside a hard timeout) ──────

const results: Array<{ name: string; ok: boolean; error?: string }> = [];

async function capture(
  page: any,
  name: string,
  fn: () => Promise<void>,
  timeoutMs = SHOT_TIMEOUT,
): Promise<void> {
  shotsTotal++;
  console.log(`\n  ── ${name} ──`);
  try {
    await withTimeout(fn(), timeoutMs, `capture ${name}`);
    await page.screenshot({ path: join(SHOT_DIR, `${name}.png`), fullPage: true });
    results.push({ name, ok: true });
    shotsOk++;
    progress(name, true);
  } catch (err: any) {
    try {
      await page.screenshot({ path: join(SHOT_DIR, `${name}.png`), fullPage: true });
    } catch {}
    const msg = err?.message ?? String(err);
    results.push({ name, ok: false, error: msg });
    progress(name, false, msg.slice(0, 80));
  }
}

// ── Main ────────────────────────────────────────────────

async function main() {
  mkdirSync(SHOT_DIR, { recursive: true });
  console.log(`Screenshots → ${SHOT_DIR}\n`);
  console.log(`Shot timeout: ${(SHOT_TIMEOUT / 1000).toFixed(0)}s each\n`);

  // Verify services
  console.log('[check] Verifying backend services…');
  try {
    const r = await fetch(`${APP_URL}`);
    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    console.log('[ready] Angular dev server is up');
  } catch {
    console.error('[fatal] Angular dev server not reachable. Run:');
    console.error('  powershell -File src/DevContext.App/scripts/start-dev-bg.ps1');
    process.exit(1);
  }

  // Launch browser (light theme)
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
    colorScheme: 'light',
  });
  await context.addInitScript(() => {
    localStorage.setItem('devcontext-vibe', 'modern');
    localStorage.setItem('devcontext-theme', 'light');
  });
  const page = await context.newPage();

  try {
    // ── 1. Home — analyze repo (empty → analyzing → done) ─
    await capture(page, '01-home', async () => {
      console.log('    Loading home page…');
      await navigate(page, APP_URL);

      // Check if we need to analyze or already have a session
      const startInput = page.locator('app-start-hero input[placeholder*="Path"]');
      const inputCount = await startInput.count().catch(() => 0);

      if (inputCount > 0) {
        console.log('    Entering repo path…');
        await startInput.first().fill(TARGET_REPO);
        await sleep(400);

        await clickIfExists(page, page.locator('app-start-hero app-button[variant="primary"]'), 'Analyze button');
        console.log('    Waiting for analysis to complete (may take 2-3 min)…');

        const found = await waitVisible(page, 'app-identity-strip', ANALYZE_TIMEOUT);
        if (!found) {
          // Take screenshot of whatever state we're in
          console.log('    ⚠ identity strip not found — capturing current state');
        }
        await sleep(SETTLE_MS);
      } else {
        console.log('    ⚠ start-hero input not found — session already active, capturing current state');
        await sleep(1000);
      }
    });

    // ── 2. Atlas — service map + flows + cards ─────────
    await capture(page, '02-atlas', async () => {
      await navigate(page, `${APP_URL}/atlas`);
      await waitVisible(page, 'app-atlas-page', 10_000);
    });

    // ── 3. Explore — entry deck + trace ────────────────
    await capture(page, '03-explore', async () => {
      await navigate(page, `${APP_URL}/explore`);
      await waitVisible(page, 'app-entry-deck', 10_000);
      await clickIfExists(page, page.locator('app-entry-deck .list-row').first(), 'first entry row');
    });

    // ── 4. Graph view ──────────────────────────────────
    await capture(page, '04-graph', async () => {
      await navigate(page, `${APP_URL}/explore`);
      await waitVisible(page, 'app-entry-deck', 10_000);
      await clickIfExists(page, page.locator('app-entry-deck .list-row').first(), 'first entry');

      // Try Graph chip — try various texts
      const graphBtn = page.locator('app-stage button.chip:has-text("Graph"), app-lens-switcher button:has-text("Graph")').first();
      await clickIfExists(page, graphBtn, 'Graph lens');
    });

    // ── 5. Code Inspector ───────────────────────────────
    await capture(page, '05-code-inspector', async () => {
      await navigate(page, `${APP_URL}/explore`);
      await waitVisible(page, 'app-entry-deck', 10_000);
      await clickIfExists(page, page.locator('app-entry-deck .list-row').first(), 'first entry');

      // Open inspector dock (Ctrl+L or click)
      const inspector = page.locator('app-inspector');
      if (!(await inspector.count()) || !(await inspector.isVisible().catch(() => false))) {
        await page.keyboard.press('Control+L');
        await sleep(1200);
      }

      // Click Code tab
      await clickIfExists(page, page.locator('app-inspector button:has-text("Code")'), 'Code tab');
    });

    // ── 6. Table Lens ──────────────────────────────────
    await capture(page, '06-table-lens', async () => {
      await navigate(page, `${APP_URL}/explore`);
      await waitVisible(page, 'app-entry-deck', 10_000);

      const tableBtn = page.locator('app-lens-switcher button:has-text("Table")');
      const clicked = await clickIfExists(page, tableBtn, 'Table lens');
      if (clicked) {
        await waitVisible(page, 'app-table-lens', 10_000);
      }
    });

    // ── 7. Insights ────────────────────────────────────
    await capture(page, '07-insights', async () => {
      await navigate(page, `${APP_URL}/insights`);
      await waitVisible(page, 'app-insights-page', 10_000);
    });

    // ── 8. Context Studio — scope + composition ────────
    await capture(page, '08-context-studio', async () => {
      await navigate(page, `${APP_URL}/context`);
      await waitVisible(page, 'app-context-studio', 10_000);

      // Click scope tree items to seed composition
      const items = page.locator('app-scope-picker [role="treeitem"], app-scope-picker .list-row');
      const count = await items.count().catch(() => 0);
      if (count > 0) {
        await clickIfExists(page, items.first(), 'first scope item');
      }
      if (count > 2) {
        await clickIfExists(page, items.nth(1), 'second scope item', 800);
      }

      // Try preset button
      await clickIfExists(page, page.locator('app-scope-picker button:has-text("changing this endpoint")'), 'preset button');
    });

    // ── 9. Export — Context Studio with copy ───────────
    await capture(page, '09-export', async () => {
      await navigate(page, `${APP_URL}/context`);
      await waitVisible(page, 'app-context-studio', 10_000);

      // Seed composition
      const items = page.locator('app-scope-picker [role="treeitem"], app-scope-picker .list-row');
      if ((await items.count().catch(() => 0)) > 0) {
        await clickIfExists(page, items.first(), 'first scope item');
        await sleep(1200);
      }

      // Wait for cards to populate
      await sleep(1500);

      // Click Copy button (non-disabled)
      const copyBtn = page.locator('app-budget-panel button:not([disabled]):has-text("Copy")').first();
      await clickIfExists(page, copyBtn, 'Copy button');
    });

    // ── 10. MCP ─────────────────────────────────────────
    // Z1.2 — the page is subscribed BEFORE the traffic, because the feed is live-only: the server
    // streams to current observers and replays no history. So the order matters — open the page,
    // then drive a real sidecar against the same server, then shoot. What that buys the README is
    // the thing N4.3 built: rows in the agent's vocabulary, each with the affordance that follows
    // it (open ↗ for a trace, replay ↗ for a get_context).
    await capture(
      page,
      '10-mcp',
      async () => {
        // Use load event instead of networkidle — MCP page has live connections
        await page.goto(`${APP_URL}/mcp`, { waitUntil: 'load', timeout: 15_000 }).catch(() => {});
        await sleep(3000);
        await waitVisible(page, 'app-mcp-page', 10_000);

        const seeded = await seedAgentCalls({
          repoRoot: ROOT,
          endpoint: SERVER_URL,
          repoPath: TARGET_REPO,
          log: console.log,
        });
        console.log(`    ${seeded.ok ? '✓' : '⚠'} feed seed: ${seeded.detail}`);
        if (seeded.ok) {
          // The rows arrive over the open stream; give the page a beat to render them.
          await page.waitForSelector('[data-testid="feed-open"]', { timeout: 20_000 }).catch(() => {
            console.log('    ⚠ no row affordance rendered — capturing whatever the feed shows');
          });
          await sleep(1200);
        }
      },
      MCP_SHOT_TIMEOUT,
    );

    // ── 11. Settings ────────────────────────────────────
    await capture(page, '11-settings', async () => {
      await navigate(page, `${APP_URL}/settings`);
      await waitVisible(page, 'app-settings-view', 10_000);
    });

    // ── 12. Home full page ──────────────────────────────
    await capture(page, '12-home-full', async () => {
      await navigate(page, APP_URL);
      // Check current state — after analysis home shows identity strip
      const hasStrip = await page.locator('app-identity-strip').count().catch(() => 0);
      if (hasStrip === 0) {
        console.log('    ⚠ identity strip not present — capturing current state');
      }
      await sleep(SETTLE_MS);
    });

    // ── 13. MCP live feed ───────────────────────────────
    // Z1.2 — the page is taller than the viewport, and 10-mcp shows its top half (status, host
    // config, served catalog). The feed is the other half, and it is the half N4.3 changed, so it
    // gets its own shot: the same seeded agent session, scrolled to the rows. The seed runs again
    // because leaving the route tore the stream down and there is no backlog to re-read.
    await capture(
      page,
      '13-mcp-feed',
      async () => {
        await page.goto(`${APP_URL}/mcp`, { waitUntil: 'load', timeout: 15_000 }).catch(() => {});
        await sleep(2000);
        await waitVisible(page, 'app-mcp-page', 10_000);

        const seeded = await seedAgentCalls({
          repoRoot: ROOT,
          endpoint: SERVER_URL,
          repoPath: TARGET_REPO,
          log: console.log,
        });
        console.log(`    ${seeded.ok ? '✓' : '⚠'} feed seed: ${seeded.detail}`);

        const row = page.locator('[data-testid="feed-open"]').first();
        if ((await row.count().catch(() => 0)) > 0) {
          await row.scrollIntoViewIfNeeded({ timeout: 10_000 }).catch(() => {});
        } else {
          console.log('    ⚠ no row affordance to scroll to — capturing the feed as it stands');
          await page
            .locator('text=Live Feed')
            .first()
            .scrollIntoViewIfNeeded({ timeout: 10_000 })
            .catch(() => {});
        }
        await sleep(1200);
      },
      MCP_SHOT_TIMEOUT,
    );

    // ── Report ──────────────────────────────────────────
    const allOk = results.filter((r) => r.ok).length;
    const report = `# README Screenshot Capture

**Date:** ${new Date().toISOString().split('T')[0]}
**Target repo:** \`${TARGET_REPO}\`

| # | Screenshot | Status |
|---|-----------|--------|
${results.map((r, i) => `| ${String(i + 1).padStart(2, '0')} | \`${r.name}.png\` | ${r.ok ? '✅' : '❌ ' + (r.error ?? '')} |`).join('\n')}

**${allOk}/${results.length} successful**
`;
    writeFileSync(join(SHOT_DIR, 'README.md'), report, 'utf-8');
    console.log(`\nReport → docs/screenshots/README.md`);
    console.log(`\n${'═'.repeat(40)}`);
    console.log(`  ${allOk}/${results.length} screenshots captured`);
    console.log(`${'═'.repeat(40)}\n`);

  } catch (err) {
    console.error('[fatal]', err);
  } finally {
    await browser.close();
  }
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
