/** UI redesign audit — round 3: node card, graph depth, studio compose, MCP sandbox. */
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

  // ---- node card in tree
  await page.goto(`${APP}/explore?focus=${encodeURIComponent('POST /api/orders/')}`, {
    waitUntil: 'domcontentloaded',
  });
  await sleep(4000);
  const nodeBtn = page.locator('button[title*="IdentifiedCommand"]').first();
  if (await nodeBtn.count()) {
    await nodeBtn.click();
    await sleep(2500);
    await snap('90-nodecard');
    for (const acc of ['Code', 'Call Stack', 'Insights']) {
      const a = page.locator('button', { hasText: acc }).first();
      if (await a.count()) await a.click().catch(() => {});
      await sleep(1200);
    }
    await snap('91-nodecard-expanded', true);
  } else {
    writeFileSync(join(OUT, 'nodecard-MISS.txt'), 'no node button', 'utf-8');
  }

  // ---- graph depth raise
  const graphBtn = page.locator('button', { hasText: 'Graph' }).first();
  if (await graphBtn.count()) {
    await graphBtn.click();
    await sleep(3000);
    const depthBtn = page.locator('button, select', { hasText: 'depth' }).first();
    if (await depthBtn.count()) {
      const tag = await depthBtn.evaluate((el) => el.tagName.toLowerCase());
      if (tag === 'select') {
        await depthBtn.selectOption({ index: 5 }).catch(() => depthBtn.selectOption({ index: 3 }));
      } else {
        await depthBtn.click();
        await sleep(800);
        await snap('92-depth-menu');
        // click a menu item labeled 4 or 6
        for (const d of ['6', '4', '3']) {
          const opt = page.locator('button, [role=option], [role=menuitem], li', { hasText: new RegExp(`^\\s*${d}\\s*$`) }).first();
          if (await opt.count()) {
            await opt.click();
            break;
          }
        }
      }
      await sleep(4500);
      await snap('93-graph-deep');
    }
  }

  // ---- studio compose: click row labels inside scope picker (not headers)
  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(3000);
  let added = 0;
  for (const label of ['POST\n/api/orders/', '/api/orders/', 'OrderStartedIntegrationEventHandler', 'PUT\n/api/catalog/items']) {
    const row = page.locator('app-scope-picker [class*=cursor-pointer], app-scope-picker li, app-scope-picker button, app-scope-picker [role=option]', { hasText: label.replace('\n', ' ').trim() }).first();
    if (await row.count()) {
      await row.click().catch(() => {});
      await sleep(1200);
      const sel = await page.locator('text=/\\d+ of 109 selected/').first().innerText().catch(() => '');
      if (sel && !sel.startsWith('0')) {
        added++;
        if (added >= 2) break;
      }
    }
  }
  writeFileSync(join(OUT, 'studio-selected.txt'), String(added), 'utf-8');
  const addBtn = page.locator('button[data-testid="add-to-context"]').first();
  if ((await addBtn.count()) && (await addBtn.isEnabled())) {
    await addBtn.click();
    await sleep(7000);
    await snap('94-studio-composed', true);
    // budget to 1k
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
  const toolSelect = page.locator('select').first();
  if (await toolSelect.count()) {
    await toolSelect.selectOption('trace').catch(() => {});
    await sleep(600);
    const argBox = page.locator('input[placeholder*=Arg], input[name=arg], app-mcp-page input[type=text]').last();
    if (await argBox.count()) {
      await argBox.fill('POST /api/orders/');
      await sleep(400);
    }
    const runBtn = page.locator('button', { hasText: 'Run' }).first();
    if (await runBtn.count()) {
      await runBtn.click();
      await sleep(5000);
      await snap('96-mcp-trace-result', true);
    }
  }

  console.log(`done · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
