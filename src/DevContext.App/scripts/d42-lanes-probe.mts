/** D4.2 probe: click the canvas's "All projects" chip on podcasts home → lanes view. */
import { chromium } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-18\\prism-d4\\gate-d42';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
page.on('pageerror', (e) => console.log('PAGEERROR:', e.message.slice(0, 200)));

await page.goto('http://localhost:4200', { waitUntil: 'domcontentloaded' });
const input = page.locator('app-start-hero input').first();
await input.waitFor({ timeout: 15_000 });
await input.fill('C:\\code\\DevContext2\\eval-repos\\dotnet-podcasts');
await sleep(300);
await page.locator("app-start-hero app-button[variant='primary']").first().click();
await page.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: 120_000 });
await sleep(2500);
await page.getByText('All projects', { exact: true }).first().click();
await sleep(2500);
await page.screenshot({ path: join(OUT, 'podcasts-5-lanes-probe.png'), fullPage: false });
console.log('snap podcasts-5-lanes-probe');
await browser.close();
console.log('LANES-PROBE-DONE');
