/** Round 4 — bootstrap session, then: home tiles, context compose + exports, MCP try-a-tool with handle. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4300';
const SERVER = 'http://127.0.0.1:5279';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/02b5676e-ee2f-410d-a117-3bd886869592/scratchpad/audit/eshop4';
mkdirSync(OUT, { recursive: true });

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));
const results: Array<{ step: string; ok: boolean; err?: string }> = [];
async function step(name: string, fn: () => Promise<void>) {
  console.log(`── ${name}`);
  try {
    await fn();
    results.push({ step: name, ok: true });
    console.log('   ok');
  } catch (e: any) {
    results.push({ step: name, ok: false, err: (e?.message ?? String(e)).slice(0, 200) });
    console.log(`   FAIL: ${(e?.message ?? String(e)).slice(0, 200)}`);
  }
}

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

  // Bootstrap session
  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill('C:\\code\\DevContext2\\eval-repos\\eShop');
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await page.waitForSelector('app-identity-strip', { timeout: 300_000 });
    await sleep(2000);
  }

  await step('50-home-tiles', async () => {
    await snap('50-home-after-analyze');
    const runReport = await page.locator('text=Run report').count();
    const traceCheckout = await page.locator('text=Trace checkout').count();
    writeFileSync(join(OUT, 'home-tiles.txt'), `Run report: ${runReport} · Trace checkout: ${traceCheckout}`, 'utf-8');
  });

  await step('51-trace-checkout', async () => {
    const btn = page.locator('text=Trace checkout').first();
    if ((await btn.count()) === 0) throw new Error('no tile');
    await btn.click();
    await sleep(3000);
    await snap('51-trace-checkout');
  });

  await step('52-run-report', async () => {
    await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded' });
    await sleep(1500);
    const btn = page.locator('text=Run report').first();
    if ((await btn.count()) === 0) throw new Error('no tile');
    const dl = page.waitForEvent('download', { timeout: 8_000 }).catch(() => null);
    await btn.click();
    await sleep(2500);
    const d = await dl;
    if (d) await d.saveAs(join(OUT, `home-report-${d.suggestedFilename()}`));
    await snap('52-after-run-report');
  });

  await step('53-compose', async () => {
    await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
    await sleep(2500);
    for (const label of ['POST /api/orders/draft', 'PUT /api/catalog/items/{id:int}', 'OrderStartedIntegrationEventHandler']) {
      const btn = page.locator(`app-scope-picker button:text-is("${label}")`).first();
      if (await btn.count()) {
        await btn.click();
        await sleep(1500);
      } else console.log(`   (missing: ${label})`);
    }
    await sleep(5000);
    await snap('53-composed');
  });

  await step('54-copy-markdown', async () => {
    const copyBtn = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    if ((await copyBtn.count()) === 0) throw new Error('Copy disabled with composed cards');
    await copyBtn.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
    writeFileSync(join(OUT, 'export-markdown.md'), clip || '(empty)', 'utf-8');
  });

  await step('55-budget-min', async () => {
    const slider = page.locator('input[type="range"]').first();
    await slider.evaluate((el: HTMLInputElement) => {
      el.value = el.min || '1000';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await sleep(4000);
    await snap('55-budget-min');
    const hasOmitted = await page.evaluate(() => document.body.innerText.toLowerCase().includes('omit'));
    writeFileSync(join(OUT, 'omitted-visible.txt'), String(hasOmitted), 'utf-8');
    const copyBtn = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    if (await copyBtn.count()) {
      await copyBtn.click();
      await sleep(1500);
      const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
      writeFileSync(join(OUT, 'export-markdown-1k.md'), clip || '(empty)', 'utf-8');
    }
  });

  await step('56-copy-plain', async () => {
    await page.locator('button:text-is("plain")').first().click();
    await sleep(800);
    const copyBtn = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    await copyBtn.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
    writeFileSync(join(OUT, 'export-plain.txt'), clip || '(empty)', 'utf-8');
  });

  await step('57-save', async () => {
    const saveBtn = page.locator('button:not([disabled])', { hasText: 'Save' }).first();
    if ((await saveBtn.count()) === 0) throw new Error('Save disabled');
    const dl = page.waitForEvent('download', { timeout: 10_000 }).catch(() => null);
    await saveBtn.click();
    const d = await dl;
    writeFileSync(join(OUT, 'save-filename.txt'), d ? d.suggestedFilename() : 'NO DOWNLOAD', 'utf-8');
    if (d) await d.saveAs(join(OUT, `saved-${d.suggestedFilename()}`));
  });

  await step('58-mcp-tool', async () => {
    await page.goto(`${APP}/mcp`, { waitUntil: 'load', timeout: 20_000 }).catch(() => {});
    await sleep(3000);
    // handle from sessions table (2nd cell of first data row)
    const handle = await page
      .locator('app-mcp-page table tbody tr td:nth-child(2)')
      .first()
      .innerText()
      .catch(() => '');
    writeFileSync(join(OUT, 'mcp-handle.txt'), handle || '(none)', 'utf-8');
    const sel = page.locator('app-mcp-page select').first();
    await sel.selectOption({ label: 'entrypoints' }).catch(async () => sel.selectOption('entrypoints'));
    const handleInput = page.locator('app-mcp-page input').first();
    if (handle) await handleInput.fill(handle.trim());
    await sleep(500);
    const run = page.locator('app-mcp-page button:has-text("Run")').first();
    await run.click({ timeout: 8000 });
    await sleep(3500);
    await snap('58-mcp-entrypoints');
  });

  writeFileSync(join(OUT, 'steps.json'), JSON.stringify(results, null, 1), 'utf-8');
  console.log(`\n${results.filter((r) => r.ok).length}/${results.length} ok · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
