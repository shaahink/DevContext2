/** D4.6 probe (L2/L7/K2): live loading waterfall during a cold analysis (OrchardCore;
 * first run used DntSite — evidence keeps both), stage timeline in the run report +
 * insights Engine details, freshness card with age + HEAD + state chip.
 * Prints UILITE-PROBE: PASS|FAIL. */
import { chromium, type Page } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-18\\prism-d4\\d46';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const failures: string[] = [];
async function assertThat(cond: boolean | Promise<boolean>, label: string): Promise<void> {
  if (await cond) console.log('ok  ', label);
  else { failures.push(label); console.log('FAIL', label); }
}
async function becomesVisible(locator: import('playwright').Locator, timeout = 8000): Promise<boolean> {
  try { await locator.waitFor({ state: 'visible', timeout }); return true; } catch { return false; }
}

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const page: Page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
page.on('pageerror', (e) => { failures.push(`pageerror: ${e.message.slice(0, 120)}`); console.log('PAGEERROR:', e.message.slice(0, 200)); });

await page.goto('http://localhost:4200', { waitUntil: 'domcontentloaded' });
const input = page.locator('app-start-hero input').first();
await input.waitFor({ timeout: 15_000 });
await input.fill('C:\\code\\DevContext2\\eval-repos\\OrchardCore');
await sleep(300);
await page.locator("app-start-hero app-button[variant='primary']").first().click();

// ---- L7: the live waterfall while the run streams --------------------------------
const console_ = page.locator('app-run-console');
await console_.waitFor({ state: 'visible', timeout: 20_000 });
await assertThat(becomesVisible(console_.getByText('First analysis can take minutes').first(), 10_000), 'honest expectation line during loading');
// Let stages accumulate, then require ≥2 waterfall rows with elapsed values.
await sleep(6000);
const rowCount1 = await console_.locator('.animate-spin').count();
const elapsedCells = console_.getByText(/^\d+(\.\d+)?s$|^\d+m \d+s$/);
const elapsedCount1 = await elapsedCells.count();
await assertThat(elapsedCount1 >= 2, `waterfall shows stage rows with elapsed (${elapsedCount1} elapsed cells, ${rowCount1} spinners)`);
await page.screenshot({ path: join(OUT, 'orchard-1-loading-waterfall.png'), fullPage: false });
await sleep(6000);
await page.screenshot({ path: join(OUT, 'orchard-2-loading-waterfall-later.png'), fullPage: false });

// ---- wait for ready --------------------------------------------------------------
await page.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: 300_000 });
await sleep(2500);

// ---- L2: freshness card ----------------------------------------------------------
await assertThat(page.getByText(/Analyzed (just now|\d+m ago)/).first().isVisible(), 'freshness card shows analyzed-at age');
await assertThat(becomesVisible(page.getByText(/HEAD [0-9a-f]{7}/).first(), 10_000), 'freshness card shows the HEAD sha');
await assertThat(page.getByText('Current', { exact: true }).first().isVisible(), 'freshness card shows the Current chip');
await page.screenshot({ path: join(OUT, 'orchard-3-freshness-card.png'), fullPage: false });

// ---- K2: stage timeline in the run report ----------------------------------------
await page.getByText('Run report').first().click();
await sleep(800);
await assertThat(becomesVisible(page.getByText('Stage timeline').first(), 8000), 'run report has the Stage timeline section');
const bars = page.locator('app-stage-timeline .bg-accent');
await assertThat(bars.count().then((n) => n >= 3), 'timeline renders proportional stage bars');
await page.screenshot({ path: join(OUT, 'orchard-4-report-timeline.png'), fullPage: false });

// ---- K2: insights Engine details timeline ----------------------------------------
await page.goto('http://localhost:4200/insights', { waitUntil: 'domcontentloaded' });
await sleep(1500);
await page.getByText('Engine details').first().click();
await sleep(500);
await assertThat(becomesVisible(page.locator('app-stage-timeline').first(), 8000), 'insights Engine details renders the timeline');
await page.screenshot({ path: join(OUT, 'orchard-5-insights-timeline.png'), fullPage: false });

await browser.close();
if (failures.length) {
  console.log('UILITE-PROBE: FAIL');
  for (const f of failures) console.log('  -', f);
  process.exit(1);
}
console.log('UILITE-PROBE: PASS');
