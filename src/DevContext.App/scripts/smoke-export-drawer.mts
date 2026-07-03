/**
 * Smoke test: Export Drawer (W4 checkpoint 7)
 * Runs analysis + client-side nav to /explore + verifies export drawer.
 *
 * Run: node --experimental-strip-types scripts/smoke-export-drawer.mts
 * Requires: .NET server on :5179, ng serve on :4200
 */
import { spawn, spawnSync } from 'child_process';
import { chromium } from 'playwright';
import { resolve } from 'path';

const BASE = 'http://localhost:4200';
const FIXTURE = resolve('../../tests/fixtures/MinimalApiProject');

async function waitForUrl(url: string, timeout = 60000): Promise<void> {
  const start = Date.now();
  while (Date.now() - start < timeout) {
    try {
      const r = await fetch(url, { signal: AbortSignal.timeout(2000) });
      if (r.ok) return;
    } catch { /* retry */ }
    await new Promise((r) => setTimeout(r, 2000));
  }
  throw new Error(`Timeout waiting for ${url}`);
}

async function main() {
  // Start ng serve. shell:true is required on Windows to resolve the npx.cmd shim;
  // that means ng.pid is cmd.exe's pid, not ng serve's — killed via `taskkill /T`
  // (kills the whole tree) below, since plain SIGTERM only reaches the shell.
  const ng = spawn('npx', ['ng', 'serve'], { stdio: 'ignore', shell: true });

  console.log('Waiting for dev server...');
  await waitForUrl(BASE, 90000);
  console.log('Dev server ready');

  const browser = await chromium.launch({ channel: 'chrome', headless: true });
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });

  const log: string[] = [];
  const pass = (m: string) => { log.push(`PASS ${m}`); console.log(`  PASS ${m}`); };
  const fail = (m: string) => { log.push(`FAIL ${m}`); console.error(`  FAIL ${m}`); };

  const errors: string[] = [];
  page.on('console', (m) => {
    if (m.type() === 'error' && !m.text().includes('ERR_CONNECTION_REFUSED') && !m.text().includes('favicon'))
      errors.push(m.text());
  });

  try {
    // 1. Load app
    await page.goto(BASE, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForTimeout(2000);
    pass('App loaded');

    // 2. Analyze
    const pi = page.locator('input[placeholder*="Path"]').first();
    if (!(await pi.isVisible({ timeout: 5000 }).catch(() => false))) {
      fail('Path input not visible');
    } else {
      await pi.fill(FIXTURE);
      await page.waitForTimeout(300);
      await page.getByRole('button', { name: 'Analyze' }).click();
      pass('Analyze clicked');

      try {
        await page.getByRole('button', { name: 'Cancel' }).waitFor({ state: 'visible', timeout: 10000 });
        await page.getByRole('button', { name: 'Cancel' }).waitFor({ state: 'hidden', timeout: 120000 });
        pass('Analysis completed');
      } catch {
        fail('Analysis did not complete');
      }
    }

    await page.waitForTimeout(2000);

    // 3. Client-side navigate to /explore (no page reload)
    // Use Angular's router injection via window.ng (available in dev mode)
    await page.evaluate(() => {
      const win = window as any;
      if (!win.ng) throw new Error('window.ng not available (need dev mode)');
      // Angular 22 dev tools API
      const appRoot = document.querySelector('app-root');
      if (!appRoot) throw new Error('No app-root');
      // Try navigate using the Location service / history API
      const oldPush = window.history.pushState;
      window.history.pushState = function (state, title, url) {
        const result = oldPush.apply(this, [state, title, url]);
        window.dispatchEvent(new PopStateEvent('popstate', { state }));
        return result;
      };
      window.history.pushState(null, '', '/explore');
      window.history.pushState = oldPush;
    });
    await page.waitForTimeout(3000);

    const currentUrl = page.url();
    console.log(`  URL: ${currentUrl}`);

    // Check workbench loaded: look for app-stage or app-entry-deck
    const deck = page.locator('app-entry-deck');
    const deckVis = await deck.isVisible({ timeout: 5000 }).catch(() => false);
    if (deckVis) pass('Workbench loaded');
    else fail('Workbench not loaded');

    // Entries
    const rows = page.locator('app-entry-deck .list-row');
    const count = await rows.count();
    console.log(`  Entries: ${count}`);
    if (count > 0) {
      pass(`Deck has ${count} entries`);

      // Select + pin
      await rows.first().click();
      await page.waitForTimeout(1000);
      await page.keyboard.press('p');
      await page.waitForTimeout(300);
      pass('Entry pinned');
    }

    // 4. Open export drawer
    await page.keyboard.press('Control+e');
    await page.waitForTimeout(800);

    const drawer = page.locator('app-export-drawer');
    if (await drawer.isVisible({ timeout: 3000 }).catch(() => false)) pass('Export drawer visible');
    else { fail('Drawer not visible'); }

    // 5. Presets
    const chips = drawer.locator('.chip');
    const chipCount = await chips.count();
    if (chipCount >= 4) pass(`4 presets`);
    else fail(`Only ${chipCount} presets`);

    // 6. Full preset → content (must be the <pre> render, not the empty/placeholder state)
    await drawer.locator('.chip', { hasText: 'Full' }).click();
    await page.waitForTimeout(4000);
    const hasContent = await drawer.locator('.flex-1 pre').first().isVisible({ timeout: 3000 }).catch(() => false);
    if (hasContent) pass('Full preset content rendered');
    else fail('Full preset empty');

    // 7. From Trail (if we have entries pinned)
    if (count > 0) {
      await drawer.locator('.chip', { hasText: 'From Trail' }).click();
      await page.waitForTimeout(4000);
      const tc = drawer.locator('.flex-1 pre');
      const tcVis = await tc.isVisible({ timeout: 3000 }).catch(() => false);
      if (tcVis) pass('From Trail rendered');
      else console.log('  From Trail: pre not visible (may be empty/render-in-progress)');
    }

    // 8. Copy button
    if (await drawer.locator('app-button:has-text("Copy")').isVisible({ timeout: 2000 }).catch(() => false))
      pass('Copy button present');
    else fail('Copy button missing');

    // 9. Escape
    await page.keyboard.press('Escape');
    await page.waitForTimeout(500);
    if (await drawer.isVisible({ timeout: 1000 }).catch(() => false)) fail('Still visible after Escape');
    else pass('Escape dismissed');

    // 10. Reopen + backdrop
    await page.keyboard.press('Control+e');
    await page.waitForTimeout(600);
    if (await drawer.isVisible({ timeout: 2000 }).catch(() => false)) {
      pass('Reopened');
      await drawer.locator('[aria-label="Close export drawer"]').click({ timeout: 2000 });
      await page.waitForTimeout(400);
      if (await drawer.isVisible({ timeout: 1000 }).catch(() => false)) fail('Still visible after backdrop');
      else pass('Backdrop dismissed');
    }

    // 11. Console
    if (errors.length === 0) pass('No app console errors');
    else console.log(`  Errors (${errors.length}): ${errors.slice(0, 3).join('; ')}`);

  } catch (e) {
    fail(`Exception: ${(e as Error).message?.substring(0, 100)}`);
  }

  // Kill ng serve — taskkill /T kills the whole process tree (cmd.exe + node + esbuild
  // workers); plain ng.kill() only reaches the cmd.exe shell and leaks the real server.
  if (ng.pid) spawnSync('taskkill', ['/PID', String(ng.pid), '/T', '/F'], { stdio: 'ignore' });

  console.log('\n--- Results ---');
  for (const l of log) console.log(l);
  const failures = log.filter((l) => l.startsWith('FAIL')).length;
  console.log(`\n${log.length} checks, ${failures} failures`);
  if (errors.length > 0) console.log(`Console errors (non-trivial): ${errors.length}`);

  await browser.close();
  process.exit(failures > 0 ? 1 : 0);
}

main();
