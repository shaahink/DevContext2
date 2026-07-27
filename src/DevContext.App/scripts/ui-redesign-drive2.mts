/** UI redesign audit — round 2: eShop targeted interactions. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/c8cb5a0b-4d41-45f5-8c84-f03d5e178176/scratchpad/ui-audit/eshop2';
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

  // bootstrap — session already exists server-side; re-analyze re-attaches
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

  // home: All projects toggle
  const allProj = page.locator('button', { hasText: 'All projects' }).first();
  if (await allProj.count()) {
    await allProj.click();
    await sleep(2500);
    await snap('70-home-allprojects');
  }

  // explore focus, Graph mode
  await page.goto(`${APP}/explore?focus=${encodeURIComponent('POST /api/orders/')}`, {
    waitUntil: 'domcontentloaded',
  });
  await sleep(4000);
  const graphBtn = page.locator('button', { hasText: 'Graph' }).first();
  if (await graphBtn.count()) {
    await graphBtn.click();
    await sleep(4000);
    await snap('71-explore-graph');
  }
  // layer coloring on graph
  const layerBtn = page.locator('button', { hasText: 'Layer' }).first();
  if (await layerBtn.count()) {
    await layerBtn.click();
    await sleep(2500);
    await snap('72-explore-graph-layer');
  }
  // back to tree, open node card for IdentifiedCommand
  const treeBtn = page.locator('button', { hasText: 'Tree' }).first();
  if (await treeBtn.count()) {
    await treeBtn.click();
    await sleep(2000);
  }
  const nodeBtn = page.locator('button', { hasText: 'IdentifiedCommand' }).first();
  if (await nodeBtn.count()) {
    await nodeBtn.click();
    await sleep(2000);
    await snap('73-explore-nodecard');
    // expand Code + Call Stack accordions
    for (const acc of ['Code', 'Call Stack']) {
      const a = page.locator('button', { hasText: acc }).first();
      if (await a.count()) {
        await a.click();
        await sleep(1500);
      }
    }
    await snap('74-explore-nodecard-code', true);
  }
  // approx only
  const approxBtn = page.locator('button', { hasText: 'approx only' }).first();
  if (await approxBtn.count()) {
    await approxBtn.click();
    await sleep(2000);
    await snap('75-explore-approxonly');
    await approxBtn.click();
    await sleep(800);
  }
  // entries table
  const entriesBtn = page.locator('button', { hasText: 'Entries' }).first();
  if (await entriesBtn.count()) {
    await entriesBtn.click();
    await sleep(2500);
    await snap('76-explore-entries', true);
  }
  // full audit table
  await page.keyboard.press('Shift+E');
  await sleep(2000);
  await snap('77-entry-audit', true);
  await page.keyboard.press('Escape');
  await sleep(600);

  // atlas canvas viewport only (top region)
  await page.goto(`${APP}/atlas`, { waitUntil: 'domcontentloaded' });
  await sleep(4500);
  await snap('80-atlas-canvas');
  const legend = page.locator('button, summary', { hasText: 'Legend' }).first();
  if (await legend.count()) {
    await legend.click();
    await sleep(1200);
    await snap('81-atlas-legend');
  }

  // insights expand first card
  await page.goto(`${APP}/insights`, { waitUntil: 'domcontentloaded' });
  await sleep(2500);
  const firstInsight = page.locator('main button').first();
  if (await firstInsight.count()) {
    await firstInsight.click();
    await sleep(1500);
    await snap('82-insights-expanded', true);
  }

  // context studio: add two scopes, preview
  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(3000);
  let added = 0;
  const scopeButtons = page.locator('app-scope-picker button');
  const n = await scopeButtons.count();
  for (let i = 0; i < Math.min(n, 6) && added < 2; i++) {
    const t = (await scopeButtons.nth(i).innerText().catch(() => '')).trim();
    if (t.includes('/api/') || t.includes('Handler')) {
      await scopeButtons.nth(i).click();
      added++;
      await sleep(1500);
    }
  }
  const addBtn = page.locator('button', { hasText: 'Add to context' }).first();
  if (await addBtn.count()) {
    await addBtn.click();
    await sleep(6000);
  }
  await snap('83-context-composed', true);

  // mcp page
  await page.goto(`${APP}/mcp`, { waitUntil: 'domcontentloaded' });
  await sleep(2500);
  await snap('84-mcp', true);
  const startMcp = page.locator('button', { hasText: 'Start' }).first();
  if (await startMcp.count()) {
    await startMcp.click();
    await sleep(3000);
    await snap('85-mcp-started', true);
  }

  console.log(`done · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
