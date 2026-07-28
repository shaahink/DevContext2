/** S6 verification: the cross-service disclosure row opens and restores the original subtree. */
import { chromium } from 'playwright';
import { join } from 'node:path';

const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/r3-current-state/eshop-after';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1.5 } as never);

// Bootstrap the session first — /explore alone has no analyzed repo to render.
await page.goto('http://localhost:4200/', { waitUntil: 'domcontentloaded' });
await sleep(1500);
const input = page.locator('app-start-hero input').first();
if (await input.count()) {
  await input.fill('C:\\code\\DevContext2\\eval-repos\\eShop');
  await sleep(300);
  await page.locator('app-start-hero app-button[variant="primary"]').first().click();
  await page.waitForSelector('app-identity-strip', { timeout: 900_000 });
  await sleep(2500);
}

await page.goto(`http://localhost:4200/explore?focus=${encodeURIComponent('POST /api/orders/')}`, {
  waitUntil: 'domcontentloaded',
});
await sleep(6000);

const row = page.locator('button[aria-expanded]', { hasText: 'crosses' }).first();
const count = await row.count();
console.log(`disclosure rows found: ${count}`);
if (count === 0) {
  console.log('FAIL: no collapsed cross-service row rendered');
  await browser.close();
  process.exit(1);
}

console.log('collapsed label :', (await row.innerText()).replace(/\s+/g, ' ').trim().slice(0, 160));
console.log('aria-expanded   :', await row.getAttribute('aria-expanded'));

const before = await page.locator('app-trace-node').count();
await row.click();
await sleep(1200);
const after = await page.locator('app-trace-node').count();

console.log('aria-expanded   :', await row.getAttribute('aria-expanded'));
console.log(`app-trace-node count: ${before} -> ${after}`);
await page.screenshot({ path: join(OUT, '12-hopgroup-expanded.png'), fullPage: true });

// collapse again
await row.click();
await sleep(900);
const collapsed = await page.locator('app-trace-node').count();
console.log(`re-collapsed to: ${collapsed}`);

const ok = after > before && collapsed === before;
console.log(ok ? 'PASS: expands and re-collapses' : 'FAIL: toggle did not round-trip');
await browser.close();
process.exit(ok ? 0 : 1);
