/**
 * G7.2 (R3 C-3) — the withhold-don't-suppress rule, swept across every page.
 *
 * C-2 (G7.1) proved the rule on one page. C-3 asks whether the product keeps it wherever a surface
 * has no entries. This walks the app's routes on a given pole and reads, per page:
 *   · every section heading and the body a reader actually sees;
 *   · which bodies are marked [data-withheld] (a stated reason) and with what reason class;
 *   · every OTHER empty-looking line — a paragraph whose text starts "No " and that is NOT marked.
 *
 * That last bucket is the finding. An unmarked "No X" line is not necessarily wrong, but it is
 * unaudited: nothing distinguishes "does not apply here" from "we looked and found none" from
 * "not built yet", which is the distinction C-2 established.
 *
 * It also counts, per page, how much of the page rendered at all — a page that is BLANK on a
 * library is the S9 defect itself (suppression), and a sweep that only reads existing sections
 * cannot see it.
 *
 * Usage (services already running — start-dev-bg.ps1 first):
 *   node --experimental-strip-types src/DevContext.App/scripts/g72-withhold-sweep.mts <repo> <outFile>
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\FluentValidation';
const OUT = process.argv[3] ?? 'C:/code/DevContext2/eval-results/2026-07-29/G7/g72-withhold-sweep.txt';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const ROUTES = ['/', '/explore', '/atlas', '/insights', '/context', '/mcp'];

const lines: string[] = [];
const say = (s: string) => { lines.push(s); console.log(s); };

interface PageRead {
  readonly route: string;
  /** What the reader actually sees. The heuristics below can only flag what they know to look for;
   * this is the part a human reads to find what they did not. */
  readonly fullText: string;
  readonly textLength: number;
  readonly headings: readonly string[];
  readonly withheld: readonly { readonly reason: string; readonly text: string }[];
  readonly unmarked: readonly { readonly tag: string; readonly text: string }[];
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1600 }, deviceScaleFactor: 1 });
  const page = await context.newPage();

  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill(REPO);
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await page.waitForSelector('app-identity-strip', { timeout: 900_000 });
    await sleep(5000);
  }

  const reads: PageRead[] = [];
  for (const route of ROUTES) {
    await page.goto(`${APP}${route}`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
    await sleep(3000);
    const read = await page.evaluate(() => {
      const main = document.querySelector('main') ?? document.body;
      const withheld = Array.from(main.querySelectorAll('[data-withheld]')).map((w) => ({
        reason: (w as HTMLElement).getAttribute('data-reason') ?? '',
        text: ((w as HTMLElement).innerText ?? '').replace(/\u00a0/g, ' ').trim(),
      }));
      // Leaf text blocks that read like an empty state but carry no reason class.
      const unmarked: { tag: string; text: string }[] = [];
      for (const el of Array.from(main.querySelectorAll('p, div, span, li'))) {
        if (el.querySelector('*')) continue;                       // leaves only
        if (el.closest('[data-withheld]')) continue;               // already accounted for
        const text = ((el as HTMLElement).innerText ?? '').replace(/\u00a0/g, ' ').trim();
        // Word boundary, or "Nodes appearing in the most distinct flows…" reads as an empty state.
        if (!/^(no|none|nothing|not|0)\b/i.test(text)) continue;
        if (text.length > 200) continue;
        unmarked.push({ tag: el.tagName.toLowerCase(), text });
      }
      return {
        fullText: ((main as HTMLElement).innerText ?? '').replace(/ /g, ' ').trim(),
        textLength: ((main as HTMLElement).innerText ?? '').trim().length,
        headings: Array.from(main.querySelectorAll('h1, h2, h3')).map((h) => (h.textContent ?? '').trim()).filter(Boolean),
        withheld,
        unmarked,
      };
    });
    reads.push({ route, ...read });
    await page.screenshot({ path: OUT.replace(/\.txt$/, `${route.replace(/\//g, '-') || '-home'}.png`), fullPage: true });
  }

  await browser.close();

  say(`REPO   ${REPO}`);
  say(`ROUTES ${ROUTES.join(' · ')}`);
  say('');

  for (const r of reads) {
    say(`=== ${r.route}   [${r.textLength} chars rendered · ${r.headings.length} headings]`);
    say(`    headings: ${r.headings.join(' | ') || '(none)'}`);
    say(`    withheld WITH a reason: ${r.withheld.length}`);
    for (const w of r.withheld) say(`      [${w.reason}] ${w.text}`);
    say(`    empty-looking, NO reason class: ${r.unmarked.length}`);
    for (const u of r.unmarked) say(`      <${u.tag}> ${u.text}`);
    say('    --- what the reader sees ---');
    for (const l of r.fullText.split('\n')) say(`      ${l}`);
    say('');
  }

  const totalUnmarked = reads.reduce((n, r) => n + r.unmarked.length, 0);
  const totalWithheld = reads.reduce((n, r) => n + r.withheld.length, 0);
  say('SUMMARY');
  say(`    marked withheld (a stated reason) : ${totalWithheld}`);
  say(`    empty-looking, unaudited          : ${totalUnmarked}`);
  for (const r of reads) {
    if (r.textLength < 400) say(`    NEARLY-BLANK PAGE: ${r.route} rendered only ${r.textLength} chars`);
  }

  mkdirSync(dirname(OUT), { recursive: true });
  writeFileSync(OUT, lines.join('\n'), 'utf8');
}

main().catch((e) => { console.error(e); process.exit(1); });
