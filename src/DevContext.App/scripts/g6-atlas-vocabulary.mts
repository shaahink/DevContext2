/**
 * G6 (R3 D-H / D-4) — MEASURE, on the live page, the three vocabularies for "service" that meet
 * on Atlas. Reads what the user sees, not what a projection intends:
 *
 *   1. Architecture canvas  — the cytoscape nodes actually DRAWN (via the container's _cyreg), plus
 *                             the "in no relationship" tray.
 *   2. Per-service breakdown — the service-card names in the DOM.
 *   3. Hub radar             — the row titles in the DOM.
 *
 * Also greps every rendered string on the page for metadata arity (a backtick followed by a digit).
 *
 * Usage (services already running):
 *   node --experimental-strip-types src/DevContext.App/scripts/g6-atlas-vocabulary.mts <repo> <outFile>
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\eShop';
const OUT = process.argv[3] ?? 'C:/code/DevContext2/eval-results/2026-07-29/G6/atlas-vocab.txt';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const lines: string[] = [];
const say = (s: string) => { lines.push(s); console.log(s); };

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1200 }, deviceScaleFactor: 1 });
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
  // The hub radar needs the flow index; give it room, then proceed either way.
  for (let i = 0; i < 30; i++) {
    const done = await page.evaluate(() => !/indexing flows/i.test(document.body.innerText));
    if (done) break;
    await sleep(2000);
  }
  await sleep(2500);

  say(`REPO ${REPO}`);
  say(`URL  ${page.url()}`);
  say('');

  // ── 1. canvas: what cytoscape actually drew ────────────────────────────────
  const canvas = await page.evaluate(() => {
    const host = document.querySelector('app-graph-canvas');
    const div = host?.querySelector('div > div');
    // cytoscape registers itself on its container element
    const cy = (div as unknown as { _cyreg?: { cy?: unknown } } | null)?._cyreg?.cy as
      | { nodes: () => { map: (f: (n: never) => unknown) => unknown[] } }
      | undefined;
    if (!cy) return { ok: false as const, nodes: [] as { label: string; cls: string }[] };
    const nodes = cy.nodes().map((n: never) => {
      const nn = n as unknown as { data: (k?: string) => Record<string, unknown>; classes: () => string[] };
      const d = nn.data();
      return { label: String(d['fullLabel'] ?? d['label'] ?? ''), cls: nn.classes().join(' ') };
    }) as { label: string; cls: string }[];
    return { ok: true as const, nodes };
  });
  const drawn = canvas.nodes.filter((n) => !n.cls.includes('lane'));
  say(`[1] CANVAS — cytoscape nodes drawn (cyreg ok=${canvas.ok})  n=${drawn.length}`);
  for (const n of [...drawn].sort((a, b) => a.label.localeCompare(b.label, 'en')))
    say(`    ${n.label}    [${n.cls}]`);
  const tray = await page.evaluate(() => {
    const el = [...document.querySelectorAll('app-graph-canvas div')]
      .find((d) => /in no relationship/i.test(d.textContent ?? ''));
    return el?.textContent?.replace(/\s+/g, ' ').trim() ?? '';
  });
  say(`    TRAY: ${tray || '(none)'}`);
  say('');

  // ── the two captions, verbatim: do the sections state the same set? ───────
  const captions = await page.evaluate(() => {
    const of = (heading: RegExp) => {
      const h2 = [...document.querySelectorAll('h2')].find((h) => heading.test(h.textContent ?? ''));
      return h2?.parentElement?.querySelector('p')?.textContent?.replace(/\s+/g, ' ').trim() ?? '';
    };
    return { architecture: of(/architecture/i), breakdown: of(/per-service breakdown/i) };
  });
  say(`[CAPTION] Architecture:  ${captions.architecture}`);
  say(`[CAPTION] Breakdown:     ${captions.breakdown}`);
  say('');

  // ── 2. per-service breakdown ───────────────────────────────────────────────
  const cards = await page.evaluate(() =>
    [...document.querySelectorAll('app-service-cards .service-card')].map((c) => {
      const name = c.querySelector('span.font-mono');
      const role = [...c.querySelectorAll('div')]
        .map((d) => d.textContent?.trim() ?? '')
        .find((t) => /canvas/i.test(t)) ?? '';
      return {
        shown: name?.textContent?.trim() ?? '',
        full: name?.getAttribute('title') ?? '',
        chip: c.querySelector('span.chip')?.textContent?.trim() ?? '',
        role,
      };
    }));
  say(`[2] PER-SERVICE BREAKDOWN — service cards in the DOM  n=${cards.length}`);
  for (const c of [...cards].sort((a, b) => a.full.localeCompare(b.full, 'en')))
    say(`    ${c.full}   shown="${c.shown}"  style="${c.chip}"  canvas-state="${c.role}"`);
  say('');

  // ── 1 vs 2 ─────────────────────────────────────────────────────────────────
  const canvasNames = new Set(drawn.map((n) => n.label));
  const cardNames = new Set(cards.map((c) => c.full));
  const onlyCanvas = [...canvasNames].filter((n) => !cardNames.has(n)).sort();
  const onlyCards = [...cardNames].filter((n) => !canvasNames.has(n)).sort();
  say(`[1 vs 2] drawn-on-canvas but NOT a service card (${onlyCanvas.length}): ${onlyCanvas.join(' · ') || '(none)'}`);
  say(`[1 vs 2] a service card but NOT drawn on canvas (${onlyCards.length}): ${onlyCards.join(' · ') || '(none)'}`);
  say('');

  // ── 3. hub radar ───────────────────────────────────────────────────────────
  const hubs = await page.evaluate(() => {
    const h2 = [...document.querySelectorAll('h2')].find((h) => /hub radar/i.test(h.textContent ?? ''));
    const block = h2?.parentElement;
    return [...(block?.querySelectorAll('.list-row') ?? [])].map((r) => ({
      row: (r.textContent ?? '').replace(/\s+/g, ' ').trim(),
    }));
  });
  say(`[3] HUB RADAR — rows in the DOM  n=${hubs.length}`);
  for (const h of hubs) say(`    ${h.row}`);
  say('');

  // ── arity leak, page-wide ──────────────────────────────────────────────────
  const arity = await page.evaluate(() => {
    const hits: string[] = [];
    const walk = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    let n: Node | null;
    while ((n = walk.nextNode())) {
      const t = n.textContent ?? '';
      if (/`\d/.test(t)) hits.push(t.replace(/\s+/g, ' ').trim());
    }
    return [...new Set(hits)];
  });
  say(`[ARITY] page text nodes carrying a metadata backtick-arity: ${arity.length}`);
  for (const a of arity) say(`    ${a}`);

  mkdirSync(dirname(OUT), { recursive: true });
  writeFileSync(OUT, lines.join('\n') + '\n', 'utf-8');
  await page.screenshot({ path: OUT.replace(/\.txt$/, '.png'), fullPage: true });
  await browser.close();
  console.log(`\nwrote ${OUT}`);
}

await main();
