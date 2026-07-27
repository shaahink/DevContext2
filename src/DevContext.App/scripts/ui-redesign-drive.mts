/** UI redesign audit driver — round 1 recon: every page, screenshots + text + console. */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync, appendFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\eShop';
const NAME = process.argv[3] ?? 'eshop';
const FOCUS = process.argv[4] ?? 'POST /api/orders/';
const OUT = join(
  'C:/Users/shahi/AppData/Local/Temp/claude/C--code-DevContext2/c8cb5a0b-4d41-45f5-8c84-f03d5e178176/scratchpad/ui-audit',
  NAME,
);
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1.5 });
  const page = await context.newPage();
  const consoleLog = join(OUT, 'console.log');
  writeFileSync(consoleLog, '', 'utf-8');
  page.on('console', (m) => {
    if (m.type() === 'error' || m.type() === 'warning')
      appendFileSync(consoleLog, `[${m.type()}] ${page.url()} :: ${m.text()}\n`, 'utf-8');
  });
  page.on('pageerror', (e) => appendFileSync(consoleLog, `[pageerror] ${page.url()} :: ${e.message}\n`, 'utf-8'));

  async function snap(name: string, fullPage = true) {
    await page.screenshot({ path: join(OUT, `${name}.png`), fullPage });
    writeFileSync(join(OUT, `${name}.txt`), await page.evaluate(() => document.body.innerText), 'utf-8');
  }
  async function buttons(name: string) {
    const list = await page.evaluate(() =>
      Array.from(document.querySelectorAll('button, a[href]'))
        .map((el) => {
          const t = (el as HTMLElement).innerText?.trim().replace(/\s+/g, ' ') ?? '';
          const title = el.getAttribute('title') ?? el.getAttribute('aria-label') ?? '';
          const href = el.getAttribute('href') ?? '';
          return `${el.tagName.toLowerCase()}${href ? `[${href}]` : ''} "${t}"${title ? ` (${title})` : ''}`;
        })
        .filter((s) => s.length > 6),
    );
    writeFileSync(join(OUT, `${name}.controls.txt`), list.join('\n'), 'utf-8');
  }

  // ---- bootstrap / analyze (idempotent server-side; re-attach is fast)
  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill(REPO);
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    // capture the loading show mid-flight
    await sleep(2500);
    await snap('00-loading', false);
    await page.waitForSelector('app-identity-strip', { timeout: 300_000 });
    await sleep(2000);
  }
  await snap('01-home');
  await buttons('01-home');

  // run report disclosure
  const runReport = page.locator('summary, [role="button"]', { hasText: 'Run report' }).first();
  if (await runReport.count()) {
    await runReport.click();
    await sleep(1200);
    await snap('02-home-runreport');
  }

  // ---- explore (workspace) default
  await page.goto(`${APP}/explore`, { waitUntil: 'domcontentloaded' });
  await sleep(3500);
  await snap('10-explore-default');
  await buttons('10-explore');

  // focus a flow (trace mode)
  await page.goto(`${APP}/explore?focus=${encodeURIComponent(FOCUS)}`, { waitUntil: 'domcontentloaded' });
  await sleep(4500);
  await snap('11-explore-focus');
  await buttons('11-explore-focus');

  // try canvas mode switches by visible text
  for (const mode of ['Topology', 'Neighbors', 'Trace']) {
    const b = page.locator('button', { hasText: mode }).first();
    if (await b.count()) {
      await b.click();
      await sleep(2500);
      await snap(`12-explore-mode-${mode.toLowerCase()}`);
    }
  }

  // omnibox
  await page.keyboard.press('Control+k');
  await sleep(800);
  await page.keyboard.type('Order', { delay: 30 });
  await sleep(1200);
  await snap('13-omnibox', false);
  await page.keyboard.press('Escape');
  await sleep(400);

  // ---- atlas
  await page.goto(`${APP}/atlas`, { waitUntil: 'domcontentloaded' });
  await sleep(4000);
  await snap('20-atlas');
  await buttons('20-atlas');
  // click through any tab-like controls on atlas
  for (const t of ['Flows', 'Events', 'Hubs', 'Services', 'Architecture']) {
    const b = page.locator('app-atlas-page button, [class*=atlas] button', { hasText: t }).first();
    if (await b.count()) {
      await b.click();
      await sleep(1800);
      await snap(`21-atlas-${t.toLowerCase()}`);
    }
  }

  // ---- insights
  await page.goto(`${APP}/insights`, { waitUntil: 'domcontentloaded' });
  await sleep(2500);
  await snap('30-insights');
  await buttons('30-insights');
  // expand first insight row
  const insightRow = page.locator('app-insights-page [class*=row], app-insights-page li, app-insights-page button').first();
  if (await insightRow.count()) {
    await insightRow.click();
    await sleep(1200);
    await snap('31-insights-expanded');
  }

  // ---- context studio
  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(3000);
  await snap('40-context');
  await buttons('40-context');

  // ---- mcp
  await page.goto(`${APP}/mcp`, { waitUntil: 'domcontentloaded' });
  await sleep(2500);
  await snap('50-mcp');
  await buttons('50-mcp');

  // ---- settings
  await page.goto(`${APP}/settings`, { waitUntil: 'domcontentloaded' });
  await sleep(2000);
  await snap('60-settings');

  console.log(`done · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
