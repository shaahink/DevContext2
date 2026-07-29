/**
 * G6.3 (R3 D-4) — MEASURE, on the live Atlas page, that the scope boundary names what it drops
 * and that nothing calls those projects services.
 *
 * The claim under test has two halves and a DOM grep only proves one of them, so both are read:
 *   1. the "outside this solution" block exists and names the apps, with their style;
 *   2. NONE of those names appear among the per-service cards (app-service-cards).
 * Half 2 is the load-bearing one — a fix that merely re-listed them under "services" would satisfy
 * a naive text grep for "MAUI" and re-open the defect D-4 closed.
 *
 * Usage (services already running — start-dev-bg.ps1 first):
 *   node --experimental-strip-types src/DevContext.App/scripts/g63-outside-scope-dom.mts <repo> <outFile>
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\dotnet-podcasts';
const OUT = process.argv[3] ?? 'C:/code/DevContext2/eval-results/2026-07-29/G6/g63-outside-scope-dom.txt';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const lines: string[] = [];
const say = (s: string) => { lines.push(s); console.log(s); };

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1400 }, deviceScaleFactor: 1 });
  const page = await context.newPage();

  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill(REPO);
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await page.waitForSelector('app-identity-strip', { timeout: 900_000 });
    await sleep(4000);
  }

  await page.goto(`${APP}/atlas`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await page.waitForSelector('app-graph-canvas', { timeout: 60_000 });
  await sleep(3000);

  say(`REPO  ${REPO}`);
  say('');

  // [1] the boundary block, read out of the DOM as the user sees it
  const block = await page.evaluate(() => {
    const heads = Array.from(document.querySelectorAll('h3'));
    const h = heads.find((e) => /outside this solution/i.test(e.textContent ?? ''));
    if (!h) return null;
    const box = h.closest('div');
    return box ? (box as HTMLElement).innerText.replace(/\u00a0/g, ' ') : null;
  });
  say('[1] OUTSIDE-SCOPE BLOCK');
  if (block) { for (const l of block.split('\n')) say(`    ${l}`); }
  else say('    (absent)');
  say('');

  // [2] the per-service cards — the set that IS allowed to be called services
  const cards: string[] = await page.evaluate(() => {
    const host = document.querySelector('app-service-cards');
    if (!host) return [];
    return Array.from(host.querySelectorAll('*'))
      .filter((e) => e.children.length === 0)
      .map((e) => (e.textContent ?? '').trim())
      .filter((t) => t.length > 0);
  });
  say(`[2] SERVICE CARD TEXT (${cards.length} leaf strings)`);
  say(`    ${cards.join(' | ')}`);
  say('');

  // [3] the verdict
  const outsideNames = block
    ? block.split('\n').map((l) => l.trim().split(/\s+/)[0]).filter((n) => /\./.test(n) || /Maui/i.test(n))
    : [];
  const leaked = outsideNames.filter((n) => cards.some((c) => c.includes(n)));
  say('[3] VERDICT');
  say(`    outside-scope apps named : ${outsideNames.length}  (${outsideNames.join(', ') || 'none'})`);
  say(`    of those, ALSO on a service card: ${leaked.length}  (${leaked.join(', ') || 'none'})`);
  say(`    ${block && outsideNames.length > 0 && leaked.length === 0 ? 'PASS' : 'FAIL'}`);

  await page.screenshot({ path: OUT.replace(/\.txt$/, '.png'), fullPage: true });
  await browser.close();

  mkdirSync(dirname(OUT), { recursive: true });
  writeFileSync(OUT, lines.join('\n'), 'utf8');
}

main().catch((e) => { console.error(e); process.exit(1); });
