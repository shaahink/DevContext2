/** Round 5 — compose via robust selectors, capture Copy/Save exports at 4k and 1k budgets. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4300';
const SERVER = 'http://127.0.0.1:5279';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/02b5676e-ee2f-410d-a117-3bd886869592/scratchpad/audit/eshop6';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1.5 });
  await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: APP });
  await context.addInitScript(`globalThis.__DEVCONTEXT_SERVER__ = '${SERVER}';`);
  const page = await context.newPage();

  async function snap(name: string) {
    await page.screenshot({ path: join(OUT, `${name}.png`), fullPage: true });
    writeFileSync(join(OUT, `${name}.txt`), await page.evaluate(() => document.body.innerText), 'utf-8');
  }

  // bootstrap
  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill('C:\\code\\DevContext2\\eval-repos\\eShop');
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await page.waitForSelector('app-identity-strip', { timeout: 300_000 });
  }

  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(2500);

  // compose: substring matching with index fallback
  let added = 0;
  for (const sub of ['/api/orders/draft', '/api/catalog/items/{id:int}', 'OrderStartedIntegrationEventHandler']) {
    const btn = page.locator('app-scope-picker button', { hasText: sub }).first();
    if (await btn.count()) {
      await btn.click();
      added++;
      await sleep(1500);
    }
  }
  if (added === 0) {
    const all = page.locator('app-scope-picker button');
    const n = await all.count();
    for (let i = 2; i < Math.min(n, 5); i++) {
      await all.nth(i).click();
      added++;
      await sleep(1500);
    }
  }
  writeFileSync(join(OUT, 'scope-added.txt'), String(added), 'utf-8');
  const addBtn = page.locator('button', { hasText: 'Add to context' }).first();
  if (await addBtn.count()) { await addBtn.click(); await sleep(6000); }
  await sleep(5000);
  await snap('60-composed');

  // copy markdown @4k
  const copy1 = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
  if (await copy1.count()) {
    await copy1.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
    writeFileSync(join(OUT, 'export-markdown-4k.md'), clip || '(empty)', 'utf-8');
  } else {
    writeFileSync(join(OUT, 'export-markdown-4k.md'), 'COPY DISABLED', 'utf-8');
  }

  // budget → 1k, re-copy
  const slider = page.locator('input[type="range"]').first();
  if (await slider.count()) {
    await slider.evaluate((el: HTMLInputElement) => {
      el.value = el.min || '1000';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await sleep(4000);
    await snap('61-budget-1k');
    writeFileSync(
      join(OUT, 'omitted-visible.txt'),
      String(await page.evaluate(() => document.body.innerText.toLowerCase().includes('omit'))),
      'utf-8',
    );
    const copy2 = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    if (await copy2.count()) {
      await copy2.click();
      await sleep(1500);
      const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
      writeFileSync(join(OUT, 'export-markdown-1k.md'), clip || '(empty)', 'utf-8');
    }
  }

  // plain format
  const plainBtn = page.locator('button', { hasText: 'plain' }).first();
  if (await plainBtn.count()) {
    await plainBtn.click();
    await sleep(800);
    const copy3 = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    if (await copy3.count()) {
      await copy3.click();
      await sleep(1500);
      const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
      writeFileSync(join(OUT, 'export-plain.txt'), clip || '(empty)', 'utf-8');
    }
  }

  // save
  const saveBtn = page.locator('button:not([disabled])', { hasText: 'Save' }).first();
  if (await saveBtn.count()) {
    const dl = page.waitForEvent('download', { timeout: 10_000 }).catch(() => null);
    await saveBtn.click();
    const d = await dl;
    writeFileSync(join(OUT, 'save-filename.txt'), d ? d.suggestedFilename() : 'NO DOWNLOAD', 'utf-8');
    if (d) await d.saveAs(join(OUT, `saved-${d.suggestedFilename()}`));
  } else {
    writeFileSync(join(OUT, 'save-filename.txt'), 'SAVE DISABLED', 'utf-8');
  }

  console.log(`done · added=${added} · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
