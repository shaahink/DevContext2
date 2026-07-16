/**
 * T6.0 — shamshir pole of the 7-page UI audit (the eShop pole ran 2026-07-15).
 * Drives the LIVE dev stack (web :4200, server :5179), analyzes C:\code\shamshir
 * (monolith + workers + SignalR — the anti-microservices pole), sweeps every route,
 * exercises key interactions, and dumps per-screen artifacts (fullpage PNG + innerText
 * + element inventory) plus keyboard/theme probes (T6.5/T6.6 evidence) and a per-page
 * RPC counter (T7.4 evidence).
 *
 * Run: node --experimental-strip-types scripts/audit-drive-shamshir.mts
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = 'C:\\code\\shamshir';
const OUT =
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/dd47d312-d532-43e6-8208-cb4564473068/scratchpad/audit/shamshir';
const ANALYZE_TIMEOUT = 420_000;

mkdirSync(OUT, { recursive: true });

const consoleLog: Array<{ where: string; type: string; text: string }> = [];
const rpcLog: Array<{ where: string; rpc: string }> = [];
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
  const page = await context.newPage();

  page.on('console', (m) => {
    if (m.type() === 'error' || m.type() === 'warning')
      consoleLog.push({ where, type: m.type(), text: m.text().slice(0, 400) });
  });
  page.on('pageerror', (e) => consoleLog.push({ where, type: 'pageerror', text: String(e).slice(0, 400) }));
  page.on('request', (r) => {
    const u = r.url();
    const m = u.match(/devcontext\.v1\.DevContextService\/(\w+)/);
    if (m) rpcLog.push({ where, rpc: m[1] });
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
        text: (e as HTMLElement).innerText?.trim().replace(/\s+/g, ' ').slice(0, 80) || null,
        placeholder: e.getAttribute('placeholder'),
        aria: e.getAttribute('aria-label'),
        title: e.getAttribute('title'),
        disabled: e.hasAttribute('disabled'),
      }));
    });
    writeFileSync(join(OUT, `${name}.elements.json`), JSON.stringify(els, null, 1), 'utf-8');
  }

  async function nav(path: string, settle = 1800) {
    await page.goto(`${APP}${path}`, { waitUntil: 'domcontentloaded', timeout: 20_000 }).catch(() => {});
    await sleep(settle);
  }

  // ── 1. Home empty + analyze shamshir ─────────────────────────
  await step('01-home-empty', async () => {
    await nav('/');
    await snap('01-home-empty');
    await inventory('01-home-empty');
  });

  await step('02-analyze-shamshir', async () => {
    const input = page.locator('app-start-hero input').first();
    if ((await input.count()) === 0) throw new Error('start-hero input not found (session already active?)');
    await input.fill(REPO);
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await sleep(2500);
    await snap('02-analyzing');
    await page.waitForSelector('app-identity-strip', { timeout: ANALYZE_TIMEOUT });
    await sleep(3000);
    await snap('03-home-analyzed');
    await inventory('03-home-analyzed');
  });

  // ── 2. Route sweep with per-page RPC counting ─────────────────
  for (const [name, path] of [
    ['04-explore', '/explore'],
    ['05-atlas', '/atlas'],
    ['06-insights', '/insights'],
    ['07-mcp', '/mcp'],
    ['08-context', '/context'],
    ['09-settings', '/settings'],
  ] as const) {
    await step(name, async () => {
      const before = rpcLog.length;
      await nav(path, name === '07-mcp' ? 3500 : 2500);
      await snap(name);
      await inventory(name);
      const calls = rpcLog.slice(before).map((r) => r.rpc);
      const byRpc: Record<string, number> = {};
      for (const c of calls) byRpc[c] = (byRpc[c] ?? 0) + 1;
      writeFileSync(join(OUT, `${name}.rpc.json`), JSON.stringify({ total: calls.length, byRpc }, null, 1), 'utf-8');
    });
  }

  // ── 3. Home revisit: do START-HERE tiles survive? (B3 re-check) ──
  await step('10-home-revisit', async () => {
    await nav('/');
    await snap('10-home-revisit');
  });

  // ── 4. Explore deep dive ──────────────────────────────────────
  await step('11-explore-first-entry', async () => {
    await nav('/explore');
    await page.waitForSelector('app-entry-deck', { timeout: 15_000 });
    await sleep(1000);
    const row = page.locator('app-entry-deck .list-row').first();
    await row.click();
    await sleep(3000);
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
      await sleep(2500);
      await snap(`12-lens-${i}-${labels[i]?.replace(/[^a-z0-9]/gi, '') || i}`);
    }
  });

  // ── 5. Atlas one-pager export (T6.11 baseline) ────────────────
  await step('13-atlas-onepager', async () => {
    await nav('/atlas', 3000);
    const btn = page.locator('button', { hasText: 'Export one-pager' }).first();
    await btn.click();
    await sleep(800);
    const clip = await page.evaluate(() => navigator.clipboard.readText());
    writeFileSync(join(OUT, 'atlas-onepager.md'), clip, 'utf-8');
  });

  // ── 6. Context studio: preset seed on a worker repo ───────────
  await step('14-context-preset', async () => {
    await nav('/context', 2500);
    await snap('14-context-initial');
    const preset = page.locator('button', { hasText: "I'm changing this entry" }).first();
    if ((await preset.count()) > 0) {
      await preset.click();
      await sleep(3500);
      await snap('14-context-preset');
    }
  });

  // ── 7. Keyboard battery (T6.5 evidence): real key presses ─────
  await step('15-keyboard', async () => {
    const obs: Record<string, string> = {};
    await nav('/', 1500);
    // single-key h/e/a (the affordance the audit says is declared): does it navigate?
    await page.keyboard.press('e');
    await sleep(900);
    obs['single-e'] = page.url();
    await nav('/', 1200);
    // g-prefix nav (the wired one)
    await page.keyboard.press('g');
    await sleep(250);
    await page.keyboard.press('a');
    await sleep(900);
    obs['g-then-a'] = page.url();
    // ? help
    await page.keyboard.press('?');
    await sleep(500);
    obs['help-open'] = String(await page.locator('text=Keyboard Shortcuts').count());
    await page.keyboard.press('Escape');
    // Ctrl+K omnibox
    await page.keyboard.press('Control+k');
    await sleep(500);
    obs['omnibox-open'] = String(await page.evaluate(() => !!document.querySelector('app-omnibox input')));
    await page.keyboard.press('Escape');
    writeFileSync(join(OUT, 'keyboard-observations.json'), JSON.stringify(obs, null, 1), 'utf-8');
  });

  // ── 8. Theme parity probe (T6.6 evidence): light mode shell ───
  await step('16-theme-light', async () => {
    await nav('/settings', 1500);
    // find the mode toggle in settings (Appearance group)
    const lightBtn = page.locator('button', { hasText: /^light$/i }).first();
    if ((await lightBtn.count()) > 0) {
      await lightBtn.click();
      await sleep(800);
    } else {
      // fall back: set via localStorage-backed theme service if exposed
      await page.evaluate(() => localStorage.setItem('devcontext.theme.mode', 'light'));
      await page.reload();
      await sleep(2000);
    }
    await nav('/', 1500);
    await snap('16-home-light');
    // sample shell surface colors
    const shellColors = await page.evaluate(() => {
      const pick = (sel: string) => {
        const el = document.querySelector(sel);
        return el ? getComputedStyle(el).backgroundColor : null;
      };
      return {
        titlebar: pick('app-titlebar > *') ?? pick('app-titlebar'),
        tabstrip: pick('app-tab-strip > *') ?? pick('app-tab-strip'),
        activityBar: pick('app-activity-bar nav'),
        main: pick('main'),
        bodyClassOrAttr: document.documentElement.getAttribute('data-mode') ?? document.documentElement.className,
      };
    });
    writeFileSync(join(OUT, 'theme-light-colors.json'), JSON.stringify(shellColors, null, 1), 'utf-8');
    // restore dark
    await nav('/settings', 1200);
    const darkBtn = page.locator('button', { hasText: /^dark$/i }).first();
    if ((await darkBtn.count()) > 0) { await darkBtn.click(); await sleep(500); }
  });

  // ── 9. Wrap: logs + verdicts ──────────────────────────────────
  writeFileSync(join(OUT, 'console-log.json'), JSON.stringify(consoleLog, null, 1), 'utf-8');
  const rpcByPage: Record<string, Record<string, number>> = {};
  for (const r of rpcLog) {
    rpcByPage[r.where] ??= {};
    rpcByPage[r.where][r.rpc] = (rpcByPage[r.where][r.rpc] ?? 0) + 1;
  }
  writeFileSync(join(OUT, 'rpc-by-page.json'), JSON.stringify(rpcByPage, null, 1), 'utf-8');
  writeFileSync(join(OUT, 'results.json'), JSON.stringify(results, null, 1), 'utf-8');
  console.log('\nRESULTS:');
  for (const r of results) console.log(`  ${r.ok ? 'PASS' : 'FAIL'} ${r.step}${r.err ? ' — ' + r.err : ''}`);

  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
