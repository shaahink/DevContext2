/**
 * G6.2 (R3 D-4) — does raw metadata arity reach the UI?
 *
 * The engine-side half is measured with the CLI (eval-results/2026-07-29/G6/arity-sweep-*.txt:
 * 0 node titles carry arity, 248 node IDS do, which is correct — arity is identity). What was left
 * was the CLIENT turning an id into a label. This walks the real app and greps what a user can
 * actually read for a metadata arity marker:
 *
 *   - every TEXT node on the page
 *   - every title= attribute (the hover tooltip is UI too, and two of the leaks lived there)
 *
 * across every route, and it deliberately exercises the surfaces whose ONLY label source is an id:
 * the trail crumb (a stage node click), the neighbours-list header, and the node peek.
 *
 * Usage (services already running — start-dev-bg.ps1 first, and RESTART it after any build):
 *   node --experimental-strip-types src/DevContext.App/scripts/g62-arity-dom.mts <repo> <outFile>
 */
import { chromium, type Page } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\eShop';
const OUT = process.argv[3] ?? 'C:/code/DevContext2/eval-results/2026-07-29/G6/g62-arity-dom.txt';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const lines: string[] = [];
const say = (s: string) => { lines.push(s); console.log(s); };

/** Text nodes AND title attributes carrying a metadata arity marker. Returns what a user can read. */
async function arityHits(page: Page): Promise<{ text: string[]; titles: string[] }> {
  return page.evaluate(() => {
    const text: string[] = [];
    const walk = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    let n: Node | null;
    while ((n = walk.nextNode())) {
      const t = n.textContent ?? '';
      if (/`\d/.test(t)) text.push(t.replace(/\s+/g, ' ').trim());
    }
    const titles = [...document.querySelectorAll('[title]')]
      .map((e) => e.getAttribute('title') ?? '')
      .filter((t) => /`\d/.test(t));
    return { text: [...new Set(text)], titles: [...new Set(titles)] };
  });
}

async function report(page: Page, where: string, totals: { text: number; titles: number }) {
  const hits = await arityHits(page);
  totals.text += hits.text.length;
  totals.titles += hits.titles.length;
  say(`[${where}] text-node hits: ${hits.text.length}   title-attribute hits: ${hits.titles.length}`);
  for (const h of hits.text) say(`      TEXT  ${h.slice(0, 200)}`);
  for (const h of hits.titles) say(`      TITLE ${h.slice(0, 200)}`);
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1200 } });
  const page = await context.newPage();
  const totals = { text: 0, titles: 0 };

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
  say(`REPO ${REPO}`);
  say('');

  for (const route of ['/', '/explore', '/atlas', '/insights', '/context']) {
    await page.goto(`${APP}${route}`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
    await sleep(3500);
    await report(page, `route ${route}`, totals);
  }
  say('');

  // ── the three surfaces whose only label source is an id ──────────────────────
  await page.goto(`${APP}/explore`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(3000);

  // 1. trace an entry, then click stage nodes — each click pushes a TRAIL CRUMB, which before
  //    G6.2 was named by carving the node id up client-side.
  const entry = page.locator('app-entry-deck .list-row, app-entry-browser .list-row').first();
  if (await entry.count()) {
    await entry.click();
    await sleep(6000);
  }
  const stageNodes = page.locator('app-trace-node .list-row, app-trace-node [role="button"]');
  const n = Math.min(await stageNodes.count(), 8);
  say(`[trail] clicked ${n} stage node(s) to push crumbs`);
  for (let i = 0; i < n; i++) {
    await stageNodes.nth(i).click().catch(() => {});
    await sleep(700);
  }
  await sleep(1500);
  await report(page, 'explore + trail crumbs', totals);

  const crumbs = await page.evaluate(() =>
    [...document.querySelectorAll('app-trail-bar button, app-trail-bar .list-row')]
      .map((e) => (e.textContent ?? '').replace(/\s+/g, ' ').trim())
      .filter(Boolean));
  say(`[trail] crumbs on screen (${crumbs.length}): ${crumbs.join(' | ').slice(0, 400)}`);
  say('');

  // 2. the NEIGHBOURS-LIST header — the id printed verbatim before G6.2. It lives behind the NODE
  //    altitude, so switch altitude first: without that the stage is still showing Flow and the
  //    branch under test never renders (measured — the first run of this probe missed it).
  const nodeAlt = page.locator('app-stage button', { hasText: /^node$/i }).first();
  if (await nodeAlt.count()) { await nodeAlt.click().catch(() => {}); await sleep(1500); }
  const listToggle = page.locator('app-stage button', { hasText: /^list$/i }).first();
  if (await listToggle.count()) { await listToggle.click().catch(() => {}); await sleep(1500); }
  await report(page, 'neighbours list (node altitude)', totals);
  const header = await page.evaluate(() => {
    const p = document.querySelector('app-stage p.font-mono');
    return (p?.textContent ?? '').trim();
  });
  say(`[neighbours] header text: "${header}"`);
  const rows = await page.evaluate(() =>
    [...document.querySelectorAll('app-stage .list-row')].slice(0, 8)
      .map((r) => (r.textContent ?? '').replace(/\s+/g, ' ').trim()));
  say(`[neighbours] first rows: ${rows.join(' | ').slice(0, 400)}`);
  say('');

  // 3. the NODE PEEK — 200ms hover card whose header row is the id.
  const link = page.locator('app-node-link').first();
  if (await link.count()) {
    await link.hover().catch(() => {});
    await sleep(1200);
    await report(page, 'node peek (hover)', totals);
    const peek = await page.evaluate(() => (document.querySelector('app-node-peek')?.textContent ?? '').replace(/\s+/g, ' ').trim());
    say(`[peek] card text: "${peek.slice(0, 240)}"`);
  } else {
    say('[peek] no app-node-link on screen to hover — not measured');
  }

  say('');
  say(`TOTAL arity hits — text nodes: ${totals.text}   title attributes: ${totals.titles}`);
  say(`VERDICT: ${totals.text === 0 && totals.titles === 0 ? 'CLEAN — no metadata arity anywhere a user can read it' : 'ARITY REACHES THE UI'}`);

  mkdirSync(dirname(OUT), { recursive: true });
  writeFileSync(OUT, lines.join('\n') + '\n', 'utf-8');
  await page.screenshot({ path: OUT.replace(/\.txt$/, '.png'), fullPage: true });
  await browser.close();
  console.log(`\nwrote ${OUT}`);
}

await main();
