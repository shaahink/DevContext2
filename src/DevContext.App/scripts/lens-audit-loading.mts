/** Capture the analyze loading experience: screenshots at 8s and 25s during wolverine analysis. */
import { chromium } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = 'C:\\Users\\shahi\\AppData\\Local\\Temp\\claude\\C--code-DevContext2\\21fab51e-9c82-4278-8271-a302683a111a\\scratchpad\\repos\\wolverine';
const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-17\\lens-audit\\ui';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
await page.goto(APP, { waitUntil: 'domcontentloaded' });
await sleep(3000);
const newBtn = page.getByText('New', { exact: true }).first();
if (await newBtn.count()) { await newBtn.click(); await sleep(2000); }
const input = page.locator('app-start-hero input').first();
if ((await input.count()) === 0) { console.log('FATAL no hero'); process.exit(1); }
await input.fill(REPO);
await sleep(300);
await page.locator("app-start-hero app-button[variant='primary']").first().click();
await sleep(8000);
await page.screenshot({ path: join(OUT, 'loading-08s.png'), fullPage: true });
console.log('snap 8s');
await sleep(17000);
await page.screenshot({ path: join(OUT, 'loading-25s.png'), fullPage: true });
console.log('snap 25s');
await page.waitForSelector('app-identity-strip', { timeout: 300_000 }).catch(() => console.log('analyze did not finish in 5m'));
await page.screenshot({ path: join(OUT, 'loading-done.png'), fullPage: true });
console.log('snap done');
await browser.close();
console.log('LOADING-DRIVE-DONE');
