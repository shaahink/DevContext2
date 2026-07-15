/** Round 2 — deep interactions: real trace, inspector sections, exports, MCP try-a-tool, settings groups. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4300';
const SERVER = 'http://127.0.0.1:5279';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/02b5676e-ee2f-410d-a117-3bd886869592/scratchpad/audit/eshop2';
mkdirSync(OUT, { recursive: true });

const consoleLog: Array<{ where: string; type: string; text: string }> = [];
let where = 'boot';
const results: Array<{ step: string; ok: boolean; err?: string }> = [];

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function step(name: string, fn: () => Promise<void>) {
  where = name;
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
  const context = await browser.newContext({
    viewport: { width: 1600, height: 1000 },
    deviceScaleFactor: 1.5,
  });
  await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: APP });
  await context.addInitScript(`globalThis.__DEVCONTEXT_SERVER__ = '${SERVER}';`);
  const page = await context.newPage();
  page.on('console', (m) => {
    if (m.type() === 'error') consoleLog.push({ where, type: m.type(), text: m.text().slice(0, 300) });
  });
  page.on('pageerror', (e) => consoleLog.push({ where, type: 'pageerror', text: String(e).slice(0, 300) }));

  async function snap(name: string) {
    await page.screenshot({ path: join(OUT, `${name}.png`), fullPage: true });
    writeFileSync(join(OUT, `${name}.txt`), await page.evaluate(() => document.body.innerText), 'utf-8');
  }
  async function nav(path: string, settle = 1800) {
    await page.goto(`${APP}${path}`, { waitUntil: 'domcontentloaded', timeout: 20_000 }).catch(() => {});
    await sleep(settle);
  }
  async function clickRowContaining(scope: string, text: string): Promise<boolean> {
    const rows = page.locator(`${scope} .list-row`);
    const n = await rows.count();
    for (let i = 0; i < n; i++) {
      const t = (await rows.nth(i).innerText().catch(() => '')) || '';
      if (t.includes(text)) {
        await rows.nth(i).click();
        return true;
      }
    }
    return false;
  }

  // Session should already exist (analyzed in run 1). Home first to confirm.
  await nav('/');
  const hasSession = (await page.locator('app-identity-strip').count()) > 0;
  writeFileSync(join(OUT, 'session-state.txt'), hasSession ? 'session restored' : 'NO SESSION — re-analyze needed', 'utf-8');
  if (!hasSession) {
    const input = page.locator('app-start-hero input').first();
    if (await input.count()) {
      await input.fill('C:\\code\\DevContext2\\eval-repos\\eShop');
      await page.locator('app-start-hero app-button[variant="primary"]').first().click();
      await page.waitForSelector('app-identity-strip', { timeout: 300_000 });
    }
  }

  // ── 1. Meaningful trace: orders draft ─────────────────────────
  await step('21-orders-trace', async () => {
    await nav('/explore');
    await page.waitForSelector('app-entry-deck', { timeout: 15_000 });
    const found = await clickRowContaining('app-entry-deck', '/api/orders/draft');
    if (!found) throw new Error('orders/draft row not found');
    await sleep(2500);
    // ensure Flow lens
    const flowBtn = page.locator('app-lens-switcher button:has-text("Flow")');
    if (await flowBtn.count()) await flowBtn.first().click();
    await sleep(2000);
    await snap('21-orders-trace-flow');
  });

  // ── 2. Tree/Graph toggle + approx only ────────────────────────
  await step('22-graph-toggle', async () => {
    const graphBtn = page.locator('button:has-text("Graph")').first();
    await graphBtn.click();
    await sleep(2500);
    await snap('22-orders-graph');
    const approx = page.locator('text=approx only').first();
    if (await approx.count()) {
      await approx.click().catch(() => {});
      await sleep(1200);
      await snap('22-approx-toggled');
    }
    const treeBtn = page.locator('button:has-text("Tree")').first();
    if (await treeBtn.count()) await treeBtn.click().catch(() => {});
    await sleep(1000);
  });

  // ── 3. Inspector sections expand ──────────────────────────────
  await step('23-inspector-sections', async () => {
    const secs = page.locator('app-inspector .section-h');
    const n = await secs.count();
    writeFileSync(join(OUT, 'inspector-section-count.txt'), String(n), 'utf-8');
    for (let i = 0; i < n; i++) {
      await secs.nth(i).click().catch(() => {});
      await sleep(900);
    }
    await sleep(1000);
    await snap('23-inspector-expanded');
  });

  // ── 4. Click a trace step node → peek/card ────────────────────
  await step('24-node-peek', async () => {
    const stepNodes = page.locator('app-stage app-trace-node');
    const n = await stepNodes.count();
    writeFileSync(join(OUT, 'trace-node-count.txt'), String(n), 'utf-8');
    if (n > 1) {
      await stepNodes.nth(1).click();
      await sleep(1500);
      await snap('24-node-clicked');
    }
    const peek = page.locator('app-node-peek, app-node-card');
    if (await peek.count()) {
      writeFileSync(join(OUT, 'peek-present.txt'), 'yes', 'utf-8');
    }
  });

  // ── 5. Atlas export one-pager ─────────────────────────────────
  await step('25-atlas-export', async () => {
    await nav('/atlas', 2500);
    const btn = page.locator('button:has-text("Export one-pager"), app-button:has-text("Export one-pager")').first();
    if ((await btn.count()) === 0) throw new Error('no export one-pager button');
    const dl = page.waitForEvent('download', { timeout: 8_000 }).catch(() => null);
    await btn.click();
    await sleep(1500);
    const d = await dl;
    if (d) {
      await d.saveAs(join(OUT, `atlas-onepager-${d.suggestedFilename()}`));
      writeFileSync(join(OUT, 'atlas-export-kind.txt'), `download: ${d.suggestedFilename()}`, 'utf-8');
    } else {
      const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
      writeFileSync(join(OUT, 'atlas-onepager-clipboard.md'), clip || '(clipboard empty, no download)', 'utf-8');
      writeFileSync(join(OUT, 'atlas-export-kind.txt'), `clipboard len=${(clip || '').length}`, 'utf-8');
    }
    await snap('25-atlas-after-export');
  });

  // ── 6. Home: Run report + Trace checkout ──────────────────────
  await step('26-home-run-report', async () => {
    await nav('/');
    const btn = page.locator('text=Run report').first();
    if ((await btn.count()) === 0) throw new Error('no Run report tile');
    const dl = page.waitForEvent('download', { timeout: 8_000 }).catch(() => null);
    await btn.click();
    await sleep(2500);
    const d = await dl;
    if (d) {
      await d.saveAs(join(OUT, `home-report-${d.suggestedFilename()}`));
    }
    await snap('26-after-run-report');
  });

  await step('27-trace-checkout', async () => {
    await nav('/');
    const btn = page.locator('text=Trace checkout').first();
    if ((await btn.count()) === 0) throw new Error('no Trace checkout tile');
    await btn.click();
    await sleep(3000);
    await snap('27-trace-checkout');
  });

  // ── 7. Context studio: real composition + exports ─────────────
  await step('28-context-real-compose', async () => {
    await nav('/context', 2500);
    await page.waitForSelector('app-scope-picker', { timeout: 15_000 });
    let added = 0;
    for (const t of ['/api/orders/draft', '/api/catalog/items', 'OrderStartedIntegrationEventHandler']) {
      if (await clickRowContaining('app-scope-picker', t)) added++;
      await sleep(1200);
    }
    writeFileSync(join(OUT, 'scope-added.txt'), String(added), 'utf-8');
    await sleep(4000);
    await snap('28-context-real-compose');
  });

  await step('29-budget-min-omitted', async () => {
    const slider = page.locator('input[type="range"]').first();
    if (await slider.count()) {
      await slider.evaluate((el: HTMLInputElement) => {
        el.value = el.min || '1000';
        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));
      });
      await sleep(3500);
      await snap('29-budget-min');
    }
  });

  await step('30-copy-markdown', async () => {
    const copyBtn = page.locator('button:not([disabled]):has-text("Copy")').first();
    if ((await copyBtn.count()) === 0) throw new Error('Copy still disabled');
    await copyBtn.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
    writeFileSync(join(OUT, 'export-markdown.md'), clip || '(empty)', 'utf-8');
  });

  await step('31-copy-plain', async () => {
    const plainBtn = page.locator('button:has-text("plain")').first();
    if (await plainBtn.count()) {
      await plainBtn.click();
      await sleep(800);
      const copyBtn = page.locator('button:not([disabled]):has-text("Copy")').first();
      await copyBtn.click();
      await sleep(1500);
      const clip = await page.evaluate(() => navigator.clipboard.readText()).catch(() => '');
      writeFileSync(join(OUT, 'export-plain.txt'), clip || '(empty)', 'utf-8');
    }
  });

  await step('32-save-file', async () => {
    const saveBtn = page.locator('button:not([disabled]):has-text("Save")').first();
    if ((await saveBtn.count()) === 0) throw new Error('Save disabled');
    const dl = page.waitForEvent('download', { timeout: 10_000 }).catch(() => null);
    await saveBtn.click();
    const d = await dl;
    writeFileSync(join(OUT, 'save-filename.txt'), d ? d.suggestedFilename() : 'NO DOWNLOAD', 'utf-8');
    if (d) await d.saveAs(join(OUT, `saved-${d.suggestedFilename()}`));
  });

  // ── 8. MCP try-a-tool ─────────────────────────────────────────
  await step('33-mcp-try-tool', async () => {
    await nav('/mcp', 3000);
    const sel = page.locator('app-mcp-page select').first();
    if ((await sel.count()) === 0) throw new Error('no tool select');
    await sel.selectOption({ label: 'entrypoints' }).catch(async () => {
      await sel.selectOption('entrypoints');
    });
    await sleep(500);
    const run = page.locator('app-mcp-page button:has-text("Run")').first();
    await run.click();
    await sleep(3000);
    await snap('33-mcp-tool-result');
  });

  // ── 9. Settings groups ────────────────────────────────────────
  await step('34-settings-groups', async () => {
    await nav('/settings', 1800);
    for (const g of ['Analysis', 'Storage', 'Server', 'About']) {
      const btn = page.locator(`app-settings-view :text("${g}")`).first();
      if (await btn.count()) {
        await btn.click().catch(() => {});
        await sleep(1000);
        await snap(`34-settings-${g.toLowerCase()}`);
      }
    }
  });

  writeFileSync(join(OUT, 'console-errors.json'), JSON.stringify(consoleLog, null, 1), 'utf-8');
  writeFileSync(join(OUT, 'steps.json'), JSON.stringify(results, null, 1), 'utf-8');
  console.log(`\n${results.filter((r) => r.ok).length}/${results.length} steps ok · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
