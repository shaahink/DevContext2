/**
 * Feature-design audit driver — drives the app on ISOLATED ports (web :4300, server :5279),
 * analyzes eShop, sweeps every route, exercises key interactions, and dumps per-screen
 * artifacts (fullpage PNG + innerText + interactive-element inventory) plus the real
 * Copy/Save export artifacts and a console/RPC error log.
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4300';
const SERVER = 'http://127.0.0.1:5279';
const REPO = 'C:\\code\\DevContext2\\eval-repos\\eShop';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/02b5676e-ee2f-410d-a117-3bd886869592/scratchpad/audit/eshop';
const ANALYZE_TIMEOUT = 300_000;

mkdirSync(OUT, { recursive: true });

const consoleLog: Array<{ where: string; type: string; text: string }> = [];
const netLog: Array<{ where: string; url: string; status: string }> = [];
let where = 'boot';
const results: Array<{ step: string; ok: boolean; err?: string }> = [];

function sleep(ms: number) {
  return new Promise((r) => setTimeout(r, ms));
}

async function step(name: string, fn: () => Promise<void>) {
  where = name;
  console.log(`── ${name}`);
  try {
    await fn();
    results.push({ step: name, ok: true });
    console.log(`   ok`);
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
    if (m.type() === 'error' || m.type() === 'warning')
      consoleLog.push({ where, type: m.type(), text: m.text().slice(0, 400) });
  });
  page.on('pageerror', (e) => consoleLog.push({ where, type: 'pageerror', text: String(e).slice(0, 400) }));
  page.on('requestfailed', (r) =>
    netLog.push({ where, url: r.url().slice(0, 160), status: r.failure()?.errorText ?? 'failed' }),
  );
  page.on('response', (r) => {
    if (r.status() >= 400) netLog.push({ where, url: r.url().slice(0, 160), status: String(r.status()) });
  });

  async function snap(name: string) {
    await page.screenshot({ path: join(OUT, `${name}.png`), fullPage: true });
    const txt = await page.evaluate(() => document.body.innerText);
    writeFileSync(join(OUT, `${name}.txt`), txt, 'utf-8');
  }

  async function inventory(name: string) {
    const els = await page.evaluate(() => {
      const sel = 'button, [role="tab"], [role="treeitem"], a[href], input, select, textarea';
      return Array.from(document.querySelectorAll(sel)).map((e) => ({
        tag: e.tagName.toLowerCase(),
        type: e.getAttribute('type'),
        text: (e as HTMLElement).innerText?.trim().replace(/\s+/g, ' ').slice(0, 80) || null,
        placeholder: e.getAttribute('placeholder'),
        aria: e.getAttribute('aria-label'),
        title: e.getAttribute('title'),
        disabled: e.hasAttribute('disabled'),
      }));
    });
    writeFileSync(join(OUT, `${name}.elements.json`), JSON.stringify(els, null, 1), 'utf-8');
  }

  async function nav(path: string, settle = 1500) {
    await page.goto(`${APP}${path}`, { waitUntil: 'domcontentloaded', timeout: 20_000 }).catch(() => {});
    await sleep(settle);
  }

  // ── 1. Home empty state + analyze eShop ─────────────────────────
  await step('01-home-empty', async () => {
    await nav('/');
    await snap('01-home-empty');
    await inventory('01-home-empty');
  });

  await step('02-analyze-eshop', async () => {
    const input = page.locator('app-start-hero input').first();
    if ((await input.count()) === 0) throw new Error('start-hero input not found (session already active?)');
    await input.fill(REPO);
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await sleep(1500);
    await snap('02-analyzing');
    await page.waitForSelector('app-identity-strip', { timeout: ANALYZE_TIMEOUT });
    await sleep(2000);
    await snap('03-home-analyzed');
    await inventory('03-home-analyzed');
  });

  // ── 2. Route sweep ──────────────────────────────────────────────
  for (const [name, path] of [
    ['04-explore', '/explore'],
    ['05-atlas', '/atlas'],
    ['06-insights', '/insights'],
    ['07-mcp', '/mcp'],
    ['08-context', '/context'],
    ['09-settings', '/settings'],
    ['10-styleguide', '/styleguide'],
  ] as const) {
    await step(name, async () => {
      await nav(path, name === '07-mcp' ? 3500 : 2000);
      await snap(name);
      await inventory(name);
    });
  }

  // ── 3. Explore deep dive ────────────────────────────────────────
  await step('11-explore-first-entry', async () => {
    await nav('/explore');
    await page.waitForSelector('app-entry-deck', { timeout: 15_000 });
    const row = page.locator('app-entry-deck .list-row').first();
    await row.click();
    await sleep(2500);
    await snap('11-explore-first-entry');
    await inventory('11-explore-first-entry');
  });

  await step('12-explore-lenses', async () => {
    const lensButtons = page.locator('app-lens-switcher button');
    const n = await lensButtons.count();
    const labels: string[] = [];
    for (let i = 0; i < n; i++) labels.push(((await lensButtons.nth(i).innerText()) || '').trim());
    writeFileSync(join(OUT, 'lens-labels.json'), JSON.stringify(labels), 'utf-8');
    for (let i = 0; i < n; i++) {
      await lensButtons.nth(i).click().catch(() => {});
      await sleep(2200);
      await snap(`12-lens-${i}-${labels[i]?.replace(/[^a-z0-9]/gi, '') || i}`);
    }
  });

  await step('13-inspector', async () => {
    await page.keyboard.press('Control+L');
    await sleep(1200);
    const tabs = page.locator('app-inspector [role="tab"], app-inspector button.tab, app-inspector app-tabs button');
    const n = await tabs.count();
    for (let i = 0; i < Math.min(n, 8); i++) {
      const label = ((await tabs.nth(i).innerText().catch(() => '')) || String(i)).trim();
      await tabs.nth(i).click().catch(() => {});
      await sleep(1200);
      await snap(`13-inspector-${i}-${label.replace(/[^a-z0-9]/gi, '')}`);
    }
    if (n === 0) {
      await snap('13-inspector-none');
      await inventory('13-inspector-none');
    }
  });

  await step('14-omnibox', async () => {
    await page.keyboard.press('Escape');
    await sleep(400);
    await page.keyboard.press('Control+K');
    await sleep(900);
    await snap('14-omnibox-open');
    await page.keyboard.type('order', { delay: 40 });
    await sleep(1500);
    await snap('14-omnibox-results');
    await page.keyboard.press('Escape');
  });

  // ── 4. Atlas interactions ───────────────────────────────────────
  await step('15-atlas-click', async () => {
    await nav('/atlas', 2500);
    const clickable = page.locator('app-atlas-page .card, app-atlas-page .list-row, app-atlas-page [class*="flow-row"]');
    const n = await clickable.count();
    writeFileSync(join(OUT, 'atlas-clickable-count.txt'), String(n), 'utf-8');
    if (n > 0) {
      await clickable.first().click().catch(() => {});
      await sleep(2000);
      await snap('15-atlas-clicked');
    }
  });

  // ── 5. Context Studio deep dive (token export focus) ────────────
  await step('16-context-compose', async () => {
    await nav('/context', 2500);
    await page.waitForSelector('app-context-studio', { timeout: 15_000 });
    // preset first, else scope items
    const preset = page.locator('app-scope-picker button:has-text("changing this endpoint")');
    if (await preset.count()) {
      await preset.first().click();
    } else {
      const items = page.locator('app-scope-picker [role="treeitem"], app-scope-picker .list-row');
      if (await items.count()) await items.first().click();
      if ((await items.count()) > 1) await items.nth(1).click();
    }
    await sleep(4000);
    await snap('16-context-composed');
    await inventory('16-context-composed');
  });

  await step('17-context-intents', async () => {
    for (const intent of ['explain', 'review', 'trace']) {
      const btn = page.locator(`app-context-studio button:has-text("${intent}")`).first();
      if (await btn.count()) {
        await btn.click().catch(() => {});
        await sleep(2500);
        await snap(`17-intent-${intent}`);
      }
    }
  });

  await step('18-budget-slider', async () => {
    const slider = page.locator('input[type="range"]').first();
    if (await slider.count()) {
      await slider.evaluate((el: HTMLInputElement) => {
        el.value = el.min || '1000';
        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));
      });
      await sleep(3000);
      await snap('18-budget-min');
      await slider.evaluate((el: HTMLInputElement) => {
        el.value = el.max || '16000';
        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));
      });
      await sleep(3000);
      await snap('18-budget-max');
    } else {
      writeFileSync(join(OUT, '18-no-slider.txt'), 'no input[type=range] found', 'utf-8');
    }
  });

  await step('19-copy-export', async () => {
    const copyBtn = page.locator('button:not([disabled]):has-text("Copy")').first();
    if ((await copyBtn.count()) === 0) throw new Error('no enabled Copy button');
    await copyBtn.click();
    await sleep(1500);
    const clip = await page.evaluate(() => navigator.clipboard.readText()).catch((e) => `CLIPBOARD READ FAILED: ${e}`);
    writeFileSync(join(OUT, 'export-copy.md'), clip ?? '(empty)', 'utf-8');
    await snap('19-after-copy');
  });

  await step('20-save-export', async () => {
    const saveBtn = page.locator('button:not([disabled]):has-text("Save")').first();
    if ((await saveBtn.count()) === 0) throw new Error('no enabled Save button');
    const dl = page.waitForEvent('download', { timeout: 10_000 }).catch(() => null);
    await saveBtn.click();
    const d = await dl;
    if (d) {
      await d.saveAs(join(OUT, `export-save-${d.suggestedFilename()}`));
      writeFileSync(join(OUT, 'save-filename.txt'), d.suggestedFilename(), 'utf-8');
    } else {
      writeFileSync(join(OUT, 'save-filename.txt'), 'NO DOWNLOAD EVENT', 'utf-8');
    }
  });

  // ── 6. Final: error logs ────────────────────────────────────────
  writeFileSync(join(OUT, 'console-errors.json'), JSON.stringify(consoleLog, null, 1), 'utf-8');
  writeFileSync(join(OUT, 'net-errors.json'), JSON.stringify(netLog, null, 1), 'utf-8');
  writeFileSync(join(OUT, 'steps.json'), JSON.stringify(results, null, 1), 'utf-8');
  console.log(`\n${results.filter((r) => r.ok).length}/${results.length} steps ok · artifacts → ${OUT}`);

  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
