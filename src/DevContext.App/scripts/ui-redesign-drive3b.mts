/** UI redesign audit — round 3b: graph depth, studio compose, MCP sandbox. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/c8cb5a0b-4d41-45f5-8c84-f03d5e178176/scratchpad/ui-audit/eshop3';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1.5 });
  const page = await context.newPage();

  async function snap(name: string, fullPage = false) {
    await page.screenshot({ path: join(OUT, `${name}.png`), fullPage });
    writeFileSync(join(OUT, `${name}.txt`), await page.evaluate(() => document.body.innerText), 'utf-8');
  }

  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill('C:\\code\\DevContext2\\eval-repos\\eShop');
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await page.waitForSelector('app-identity-strip', { timeout: 300_000 });
    await sleep(1500);
  }

  // ---- graph depth raise
  await page.goto(`${APP}/explore?focus=${encodeURIComponent('POST /api/orders/')}`, {
    waitUntil: 'domcontentloaded',
  });
  await sleep(3500);
  const graphBtn = page.locator('button.chip', { hasText: 'Graph' }).first();
  await graphBtn.click();
  await sleep(3000);
  const depthBtn = page.locator('button', { hasText: 'depth' }).first();
  if (await depthBtn.count()) {
    await depthBtn.click();
    await sleep(800);
    await snap('92-depth-menu');
    for (const d of ['6', '5', '4', '3', '2']) {
      const opt = page
        .locator('button, [role=option], [role=menuitem], li', { hasText: new RegExp(`^\\s*depth ${d}\\s*$|^\\s*${d}\\s*$`) })
        .first();
      if (await opt.count()) {
        await opt.click().catch(() => {});
        break;
      }
    }
    await sleep(4500);
    await snap('93-graph-deep');
  } else {
    writeFileSync(join(OUT, 'depth-MISS.txt'), 'no depth control', 'utf-8');
  }

  // ---- studio compose
  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(3000);
  const rows = page.locator('app-scope-picker button');
  const n = await rows.count();
  let added = 0;
  const clicked: string[] = [];
  for (let i = 0; i < n && added < 2; i++) {
    const t = (await rows.nth(i).innerText().catch(() => '')).replace(/\s+/g, ' ').trim();
    if (/^(POST|PUT) \/api\/orders/.test(t) || t.includes('OrderStartedIntegrationEventHandler')) {
      await rows.nth(i).click().catch(() => {});
      clicked.push(t);
      await sleep(1000);
      added++;
    }
  }
  writeFileSync(join(OUT, 'studio-clicked.txt'), clicked.join('\n'), 'utf-8');
  await sleep(1000);
  const addBtn = page.locator('button[data-testid="add-to-context"]').first();
  if ((await addBtn.count()) && (await addBtn.isEnabled().catch(() => false))) {
    await addBtn.click();
    await sleep(7000);
    await snap('94-studio-composed', true);
    const slider = page.locator('input[type="range"]').first();
    if (await slider.count()) {
      await slider.evaluate((el: HTMLInputElement) => {
        el.value = el.min || '1000';
        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));
      });
      await sleep(4000);
      await snap('95-studio-1k', true);
    }
  } else {
    await snap('94-studio-FAILED', true);
  }

  // ---- MCP sandbox: run trace
  await page.goto(`${APP}/mcp`, { waitUntil: 'domcontentloaded' });
  await sleep(2500);
  const selects = page.locator('select');
  if (await selects.count()) {
    await selects.first().selectOption('trace').catch(() => {});
    await sleep(600);
  }
  const argBox = page.locator('input').last();
  if (await argBox.count()) {
    await argBox.fill('POST /api/orders/').catch(() => {});
    await sleep(400);
  }
  const runBtn = page.locator('button', { hasText: 'Run' }).first();
  if (await runBtn.count()) {
    await runBtn.click();
    await sleep(6000);
    await snap('96-mcp-trace-result', true);
  }

  console.log(`done · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
