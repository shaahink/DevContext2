/** D4.3 probe: scrolled viewport shots of the atlas boards (event/queue, data stores,
 * per-service cards) for eShop + the refit library fallback. */
import { chromium, type Page } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-18\\prism-d4\\gate-d43';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: 'chrome', headless: true });

async function drive(repo: string, tag: string, sections: string[]): Promise<void> {
  const page: Page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
  page.on('pageerror', (e) => console.log('PAGEERROR:', e.message.slice(0, 200)));
  await page.goto('http://localhost:4200', { waitUntil: 'domcontentloaded' });
  const input = page.locator('app-start-hero input').first();
  await input.waitFor({ timeout: 15_000 });
  await input.fill(repo);
  await sleep(300);
  await page.locator("app-start-hero app-button[variant='primary']").first().click();
  await page.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: 120_000 });
  await page.goto('http://localhost:4200/atlas', { waitUntil: 'domcontentloaded' });
  await sleep(4000);
  for (const section of sections) {
    const h = page.locator('h2', { hasText: section }).first();
    if ((await h.count()) === 0) { console.log(`MISSING section: ${section}`); continue; }
    await h.scrollIntoViewIfNeeded();
    await sleep(600);
    const slug = section.toLowerCase().replace(/[^a-z]+/g, '-');
    await page.screenshot({ path: join(OUT, `${tag}-probe-${slug}.png`), fullPage: false });
    console.log('snap', `${tag}-probe-${slug}`);
  }
  await page.context().close();
}

await drive('C:\\code\\DevContext2\\eval-repos\\eShop', 'eshop', ['Event & queue board', 'Data stores', 'Per-service breakdown']);
await drive('C:\\code\\DevContext2\\eval-repos\\refit', 'refit', ['Architecture']);
await browser.close();
console.log('BOARDS-PROBE-DONE');
