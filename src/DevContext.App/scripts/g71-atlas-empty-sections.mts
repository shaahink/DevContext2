/**
 * G7.1 (R3 C-2) — MEASURE, on the live Atlas page, that every section either FILLS or
 * WITHHOLDS ITSELF WITH A STATED REASON.
 *
 * The governing precedent is S9's: the Confidence Ledger was suppressed entirely on every
 * library, and the fix was to show the panel and have the entry-dependent ROWS withhold
 * themselves. C-2 applies that shape to Atlas — so this probe checks the shape, not a
 * string. A grep for "No entry points" would pass on a page that had simply deleted the
 * sections, which is the defect S9 named.
 *
 * The invariant, per section (an h2.section-h and the div that owns it):
 *   FILLED   — the section carries at least one [data-section-content] element, or
 *   WITHHELD — it carries exactly one [data-withheld] element whose data-reason names the
 *              reason class and whose text is a real sentence.
 *   Never both. Never neither.
 *
 * Plus three assertions taken from the BEFORE measurement rather than from a doc:
 *   A2  no section renders a heading over a blank body;
 *   A3  no withheld section issues an instruction the reader cannot act on — on a repo with
 *       no entry points, "index flows" is not something the reader can do;
 *   A4  no section describes an empty set as if it were a set ("The 0 services …").
 *
 * Usage (services already running — start-dev-bg.ps1 first):
 *   node --experimental-strip-types src/DevContext.App/scripts/g71-atlas-empty-sections.mts <repo> <outFile>
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\FluentValidation';
const OUT = process.argv[3] ?? 'C:/code/DevContext2/eval-results/2026-07-29/G7/g71-atlas-sections.txt';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const lines: string[] = [];
const say = (s: string) => { lines.push(s); console.log(s); };

interface SectionRead {
  readonly heading: string;
  readonly text: string;
  readonly contentCount: number;
  readonly withheld: readonly { readonly reason: string; readonly text: string }[];
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
    await sleep(4000);
  }

  await page.goto(`${APP}/atlas`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await page.waitForSelector('h2.section-h', { timeout: 60_000 });
  await sleep(3500);

  // The archetype the page itself declares (the header chip), not a value handed to the probe.
  const archetype: string = await page.evaluate(() => {
    const known = ['App', 'Library', 'Gateway', 'Desktop', 'Worker', 'Blazor', 'CliTool'];
    const chip = Array.from(document.querySelectorAll('span.chip'))
      .map((e) => (e.textContent ?? '').trim())
      .find((t) => known.includes(t));
    return chip ?? '(none)';
  });

  const sections: SectionRead[] = await page.evaluate(() => {
    return Array.from(document.querySelectorAll('h2.section-h')).map((h) => {
      const box = h.parentElement as HTMLElement;
      return {
        heading: (h.textContent ?? '').trim(),
        text: (box.innerText ?? '').replace(/\u00a0/g, ' ').trim(),
        contentCount: box.querySelectorAll('[data-section-content]').length,
        withheld: Array.from(box.querySelectorAll('[data-withheld]')).map((w) => ({
          reason: (w as HTMLElement).getAttribute('data-reason') ?? '',
          text: ((w as HTMLElement).innerText ?? '').replace(/\u00a0/g, ' ').trim(),
        })),
      };
    });
  });

  // "This repo has no entry points" as the PAGE states it — read back, not assumed.
  const entryless = archetype === 'Library' || sections.some((s) => /no entry points/i.test(s.text));

  say(`REPO      ${REPO}`);
  say(`ARCHETYPE ${archetype}${entryless ? '  (page states: no entry points)' : ''}`);
  say(`SECTIONS  ${sections.length}`);
  say('');

  say('[1] EVERY SECTION, AS THE READER SEES IT');
  for (const s of sections) {
    const body = s.text.startsWith(s.heading) ? s.text.slice(s.heading.length).trim() : s.text;
    say(`  --- ${s.heading}  [content=${s.contentCount} withheld=${s.withheld.length}]`);
    for (const l of (body || '(BLANK BODY)').split('\n')) say(`      ${l}`);
  }
  say('');

  const fails: string[] = [];

  say('[2] A1 — FILLS OR WITHHOLDS WITH A REASON');
  for (const s of sections) {
    const filled = s.contentCount > 0;
    const withheld = s.withheld.length;
    let verdict: string;
    if (filled && withheld === 0) verdict = 'FILLED';
    else if (!filled && withheld === 1 && s.withheld[0].reason && s.withheld[0].text.length >= 20) {
      verdict = `WITHHELD (${s.withheld[0].reason})`;
    } else if (filled && withheld > 0) {
      verdict = 'FAIL — both filled and withheld';
      fails.push(`A1 ${s.heading}: both content and a withheld notice`);
    } else if (!filled && withheld === 0) {
      verdict = 'FAIL — neither content nor a stated reason';
      fails.push(`A1 ${s.heading}: empty with no stated reason`);
    } else {
      verdict = 'FAIL — malformed withheld notice';
      fails.push(`A1 ${s.heading}: ${withheld} withheld notices / reason="${s.withheld[0]?.reason ?? ''}" len=${s.withheld[0]?.text.length ?? 0}`);
    }
    say(`    ${verdict.startsWith('FAIL') ? 'FAIL' : 'ok  '}  ${s.heading.padEnd(24)} ${verdict}`);
  }
  say('');

  say('[3] A2 — NO HEADING OVER A BLANK BODY');
  for (const s of sections) {
    const body = s.text.startsWith(s.heading) ? s.text.slice(s.heading.length).trim() : s.text;
    if (!body) { fails.push(`A2 ${s.heading}: blank body`); say(`    FAIL  ${s.heading}`); }
  }
  if (!fails.some((f) => f.startsWith('A2'))) say('    ok    no blank bodies');
  say('');

  say('[4] A3 — NO INSTRUCTION THE READER CANNOT ACT ON');
  if (entryless) {
    for (const s of sections) {
      if (/index flows/i.test(s.text)) {
        fails.push(`A3 ${s.heading}: tells a reader with 0 entry points to index flows`);
        say(`    FAIL  ${s.heading}: "index flows" on a repo with no entry points`);
      }
    }
    if (!fails.some((f) => f.startsWith('A3'))) say('    ok    no unactionable instruction');
  } else {
    say(`    n/a   this repo has entry points`);
  }
  say('');

  // A4 also catches "The 1 services …" — the AutoMapper pole printed it. A count sentence that
  // does not agree with its own number is the same defect as "The 0 services", one step milder.
  say('[5] A4 — NO EMPTY SET DESCRIBED AS A SET');
  for (const s of sections) {
    const m = s.text.match(/\b(?:The\s+)?0\s+(?:services|projects|flows|hubs|packages)\b|\b1\s+(?:services|projects)\b/i);
    if (m) {
      fails.push(`A4 ${s.heading}: "${m[0]}"`);
      say(`    FAIL  ${s.heading}: "${m[0]}"`);
    }
  }
  if (!fails.some((f) => f.startsWith('A4'))) say('    ok    no zero-count set sentences');
  say('');

  say('[6] VERDICT');
  if (fails.length === 0) say('    PASS — every section fills or withholds with a stated reason');
  else { say(`    FAIL — ${fails.length} problem(s)`); for (const f of fails) say(`      · ${f}`); }

  await page.screenshot({ path: OUT.replace(/\.txt$/, '.png'), fullPage: true });
  await browser.close();

  mkdirSync(dirname(OUT), { recursive: true });
  writeFileSync(OUT, lines.join('\n'), 'utf8');
  if (fails.length > 0) process.exit(1);
}

main().catch((e) => { console.error(e); process.exit(1); });
