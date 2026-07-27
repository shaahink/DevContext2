/** D4.4 probe (F1): drive refit and assert the library workbench end-to-end —
 * Explore = the five-section surface browser (full surface, not the dead entry
 * empty-state), Home = surface metrics + NO style chip, Atlas chip strip = style-free.
 * Prints LIBRARY-PROBE: PASS|FAIL on its own asserts. */
import { chromium, type Page } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-18\\prism-d4\\d44';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const failures: string[] = [];
async function assertThat(cond: boolean | Promise<boolean>, label: string): Promise<void> {
  if (await cond) console.log('ok  ', label);
  else { failures.push(label); console.log('FAIL', label); }
}
/** isVisible() doesn't auto-wait — poll via waitFor so zoneless renders can flush. */
async function becomesVisible(locator: import('playwright').Locator, timeout = 5000): Promise<boolean> {
  try { await locator.waitFor({ state: 'visible', timeout }); return true; } catch { return false; }
}

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const page: Page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
page.on('pageerror', (e) => { failures.push(`pageerror: ${e.message.slice(0, 120)}`); console.log('PAGEERROR:', e.message.slice(0, 200)); });

await page.goto('http://localhost:4200', { waitUntil: 'domcontentloaded' });
const input = page.locator('app-start-hero input').first();
await input.waitFor({ timeout: 15_000 });
await input.fill('C:\\code\\DevContext2\\eval-repos\\refit');
await sleep(300);
await page.locator("app-start-hero app-button[variant='primary']").first().click();
await page.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: 120_000 });
await sleep(1500);

// --- Home: surface metrics, style chip suppressed -------------------------------
const identity = page.locator('app-identity-strip');
await assertThat(identity.getByText('public types', { exact: false }).first().isVisible(), 'home identity strip shows public-types metric');
await assertThat(identity.getByText('ControllerBased').count().then((n) => n === 0), 'home style chip suppressed (no ControllerBased)');
await assertThat(page.getByText('Surface by namespace').first().isVisible(), 'home tile: Surface by namespace');
await assertThat(page.getByText('Consumer front doors').first().isVisible(), 'home tile: Consumer front doors');
await assertThat(page.getByText('No entry data available').count().then((n) => n === 0), 'home has no dead entry tile');
await page.screenshot({ path: join(OUT, 'refit-1-home.png'), fullPage: false });

// --- Explore: the library workbench ---------------------------------------------
await page.goto('http://localhost:4200/explore', { waitUntil: 'domcontentloaded' });
const workbench = page.locator('app-library-workbench');
await workbench.waitFor({ state: 'visible', timeout: 15_000 });
await sleep(500);
for (const section of ['Entry API', 'Abstractions', 'Generators', 'Public surface', 'Consumer paths']) {
  await assertThat(workbench.getByRole('button', { name: section }).first().isVisible(), `rail section: ${section}`);
}
await assertThat(workbench.getByText(/\d+ public types/).first().isVisible(), 'header shows the public-type count');
await assertThat(page.getByText('Analyze a repo to list its entry points').count().then((n) => n === 0), 'dead explore empty-state gone (F1)');
// Default section = ENTRY API with refit's annotate rows.
await assertThat(workbench.getByText('[Get]', { exact: true }).first().isVisible(), 'ENTRY API shows [Get] annotate row');
await page.screenshot({ path: join(OUT, 'refit-2-explore-entry-api.png'), fullPage: false });

// Walk the other sections and screenshot each.
await workbench.getByRole('button', { name: 'Abstractions' }).click();
await assertThat(becomesVisible(workbench.getByText(/\d+ implementors?/).first()), 'ABSTRACTIONS rows carry implementor counts');
await page.screenshot({ path: join(OUT, 'refit-3-explore-abstractions.png'), fullPage: false });

await workbench.getByRole('button', { name: 'Generators' }).click();
await assertThat(becomesVisible(workbench.getByText('InterfaceStubGeneratorV2').first()), 'GENERATORS shows the source generator');
await page.screenshot({ path: join(OUT, 'refit-4-explore-generators.png'), fullPage: false });

await workbench.getByRole('button', { name: 'Public surface' }).click();
await assertThat(becomesVisible(workbench.getByText('ApiException').first()), 'PUBLIC SURFACE lists ApiException');
await page.screenshot({ path: join(OUT, 'refit-5-explore-surface.png'), fullPage: false });
// Filter-as-you-type narrows the surface.
await workbench.locator('input').fill('StubHttp');
await sleep(300);
await assertThat(workbench.getByText('StubHttp').first().isVisible(), 'filter finds StubHttp');
await assertThat(workbench.getByText('ApiException').count().then((n) => n === 0), 'filter hides non-matches');
await page.screenshot({ path: join(OUT, 'refit-6-explore-surface-filtered.png'), fullPage: false });
await workbench.locator('input').fill('');

await workbench.getByRole('button', { name: 'Consumer paths' }).click();
await assertThat(becomesVisible(workbench.getByText('annotate').first()), 'CONSUMER PATHS renders recipes');
await page.screenshot({ path: join(OUT, 'refit-7-explore-consumer-paths.png'), fullPage: false });

// --- Atlas: chip strip carries archetype but no style ----------------------------
await page.goto('http://localhost:4200/atlas', { waitUntil: 'domcontentloaded' });
await sleep(3000);
await assertThat(page.getByText('ControllerBased').count().then((n) => n === 0), 'atlas style chip suppressed');
await page.screenshot({ path: join(OUT, 'refit-8-atlas.png'), fullPage: false });

await browser.close();
if (failures.length) {
  console.log('LIBRARY-PROBE: FAIL');
  for (const f of failures) console.log('  -', f);
  process.exit(1);
}
console.log('LIBRARY-PROBE: PASS');
