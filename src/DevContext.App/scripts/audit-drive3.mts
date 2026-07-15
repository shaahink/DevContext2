/** Round 3 — Context Studio for real: scope buttons, preset-after-selection, budget, omitted, Copy/Save. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4300';
const SERVER = 'http://127.0.0.1:5279';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/02b5676e-ee2f-410d-a117-3bd886869592/scratchpad/audit/eshop3';
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

  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(2500);

  await step('40-compose', async () => {
    for (const label of ['POST /api/orders/draft', 'PUT /api/catalog/items/{id:int}', 'OrderStartedIntegrationEventHandler']) {
      const btn = page.locator(`app-scope-picker button:text-is("${label}")`).first();
      if (await btn.count()) {
        await btn.click();
        await sleep(1500);
      } else {
        console.log(`   (missing: ${label})`);
      }
    }
    await sleep(5000);
    await snap('40-composed');
  });

  await step('41-preset-after-selection', async () => {
    const preset = page.locator('button:has-text("changing this endpoint")').first();
    if (await preset.count()) {
      await preset.click();
      await sleep(4000);
      await snap('41-preset');
    }
  });

  await step('42-copy-markdown', async () => {
    const copyBtn = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    if ((await copyBtn.count()) === 0) throw new Error('Copy disabled with composed cards');
    await copyBtn.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
    writeFileSync(join(OUT, 'export-markdown.md'), clip || '(empty)', 'utf-8');
  });

  await step('43-budget-min', async () => {
    const slider = page.locator('input[type="range"]').first();
    await slider.evaluate((el: HTMLInputElement) => {
      el.value = el.min || '1000';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await sleep(4000);
    await snap('43-budget-min');
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

  await step('44-copy-plain', async () => {
    const plainBtn = page.locator('button:text-is("plain")').first();
    await plainBtn.click();
    await sleep(800);
    const copyBtn = page.locator('button:not([disabled])', { hasText: 'Copy' }).first();
    await copyBtn.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
    writeFileSync(join(OUT, 'export-plain.txt'), clip || '(empty)', 'utf-8');
  });

  await step('45-save', async () => {
    const saveBtn = page.locator('button:not([disabled])', { hasText: 'Save' }).first();
    if ((await saveBtn.count()) === 0) throw new Error('Save disabled');
    const dl = page.waitForEvent('download', { timeout: 10_000 }).catch(() => null);
    await saveBtn.click();
    const d = await dl;
    writeFileSync(join(OUT, 'save-filename.txt'), d ? d.suggestedFilename() : 'NO DOWNLOAD', 'utf-8');
    if (d) await d.saveAs(join(OUT, `saved-${d.suggestedFilename()}`));
  });

  writeFileSync(join(OUT, 'steps.json'), JSON.stringify(results, null, 1), 'utf-8');
  console.log(`\n${results.filter((r) => r.ok).length}/${results.length} ok · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
