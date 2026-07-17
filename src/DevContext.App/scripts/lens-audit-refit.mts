/** Refit-only leg: open a NEW session tab, analyze refit, tour pages. */
import { chromium } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = 'C:\\Users\\shahi\\AppData\\Local\\Temp\\claude\\C--code-DevContext2\\21fab51e-9c82-4278-8271-a302683a111a\\scratchpad\\repos\\refit';
const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-17\\lens-audit\\ui';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();

await page.goto(APP, { waitUntil: 'domcontentloaded' });
await sleep(3000);

// open a fresh session: the "New" control in the top bar
const newBtn = page.getByText('New', { exact: true }).first();
if (await newBtn.count()) { await newBtn.click(); await sleep(2000); }

const input = page.locator('app-start-hero input').first();
if ((await input.count()) === 0) { console.log('FATAL: no start hero after New'); process.exit(1); }
await input.fill(REPO);
await sleep(300);
await page.locator("app-start-hero app-button[variant='primary']").first().click();
await page.waitForSelector('app-identity-strip', { timeout: 300_000 });
await sleep(3000);
await page.screenshot({ path: join(OUT, 'refit2-01-home.png'), fullPage: true });
console.log('snap home');

for (const [route, name] of [['/explore', 'explore'], ['/atlas', 'atlas'], ['/insights', 'insights'], ['/context', 'context']] as const) {
  try {
    await page.goto(APP + route, { waitUntil: 'domcontentloaded' });
    await sleep(5000);
    await page.screenshot({ path: join(OUT, `refit2-${name}.png`), fullPage: true });
    console.log('snap', name);
  } catch (e: any) { console.log('SKIP', name, e.message.slice(0, 100)); }
}
await browser.close();
console.log('REFIT-DRIVE-DONE');
