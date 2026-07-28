/**
 * S10 / R3 D-E (E3) — the front page asks the archetype's question.
 *
 * Three poles because the decision is ABOUT the difference between them, and each is the others'
 * negative control: a section that renders on all three is a section that is not reading its
 * archetype.
 *
 *   eShop            service   canvas + "how services connect" + ranked flows
 *   FluentValidation library   NO canvas at all -- front doors and namespaces instead
 *   GitVersion       clitool   NO canvas, no services toggle -- the command surface
 *
 * Also asserts E1's rule (each fact stated once) and E-2's (one ranking rule). Before this, eShop's
 * Home printed entries/projects/types twice forty pixels apart, the wiring fact three times in two
 * notations, and two sections named "Top flows" -- here and on Atlas -- disagreed about the repo.
 *
 * Usage: node --experimental-strip-types s10-verify-home.mts [eshop|fluentvalidation|gitversion]
 * Exits non-zero on any failure.
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/s10-home';

const POLES = {
  eshop: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\eShop',
    body: 'service' as const,
    /** MEASURED: 12 services, 23 transport links -- comfortably drawable. */
    canvas: true,
    // MEASURED, not assumed: heroHeading() keys on the ARCHETYPE, which is "App" — "Microservices"
    // is eShop's architecture STYLE, a different field. So the heading is the monolith-safe one.
    heading: 'What runs',
    /** E-2: the first Top flow must be request-shaped, not an internal handler. */
    firstFlowIsRequest: true,
  },
  fluentvalidation: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\FluentValidation',
    body: 'library' as const,
    canvas: false,
    heading: null,
    firstFlowIsRequest: null, // 0 entries -- no flows to rank
  },
  gitversion: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\GitVersion',
    body: 'clitool' as const,
    canvas: false,
    heading: 'What it does',
    firstFlowIsRequest: null, // one CLI entry; the command surface is the subject
  },
} as const;

const which = (process.argv[2] ?? 'eshop').toLowerCase() as keyof typeof POLES;
const pole = POLES[which];
if (!pole) { console.error(`unknown pole: ${which}`); process.exit(2); }
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

/**
 * How many times a COUNT of `token` is stated — a number immediately followed by the noun. Counting
 * the bare word instead trips over things that are not restatements: "All projects" is a canvas
 * toggle, and "45 of 109 entries have no resolved target" is a finding that legitimately quotes the
 * number it is about. E1 governs the stat-bearing furniture, not prose.
 */
function statedCounts(text: string, token: string): number {
  const noun = token.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  return (text.match(new RegExp(`\\d[\\d.,KM]*\\s+${noun}\\b`, 'g')) ?? []).length;
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1100 }, deviceScaleFactor: 1.5 });
  const page = await context.newPage();
  const fail: string[] = [];

  await page.goto(`${APP}/`, { waitUntil: 'domcontentloaded', timeout: 20_000 });
  await sleep(1500);
  const input = page.locator('app-start-hero input').first();
  if (await input.count()) {
    await input.fill(pole.repo);
    await sleep(300);
    await page.locator('app-start-hero app-button[variant="primary"]').first().click();
    await page.waitForSelector('app-identity-strip', { timeout: 900_000 });
  }
  await page.waitForFunction(
    () => !/\b(analyzing|cloning)\b/i.test(document.body.innerText),
    undefined,
    { timeout: 900_000 },
  );
  await sleep(6000);

  const body = await page.locator('body').innerText();
  writeFileSync(join(OUT, `${which}-home.txt`), body, 'utf-8');
  await page.screenshot({ path: join(OUT, `${which}-home.png`), fullPage: true });

  // ---- E3: the right body for the archetype ------------------------------------------------
  const hasCanvas = await page.locator('app-service-map-hero app-graph-canvas').count() > 0;
  const hasCommands = await page.locator('app-command-surface').count() > 0;
  console.log(`  body: canvas=${hasCanvas} commands=${hasCommands} (expected ${pole.body})`);

  if (hasCanvas !== pole.canvas) {
    fail.push(pole.canvas
      ? 'a service repo draws no topology canvas'
      : `a ${pole.body} draws a topology canvas — E3 says the question does not apply`);
  }
  if ((pole.body === 'clitool') !== hasCommands) {
    fail.push(pole.body === 'clitool'
      ? 'a CLI tool does not show its command surface'
      : 'a non-CLI repo shows a command surface');
  }
  // "What runs" is a runtime promise. Only a repo with a runtime may make it.
  if (pole.body !== 'service' && /What runs/.test(body)) {
    fail.push(`"What runs" is still asked of a ${pole.body}`);
  }
  if (pole.heading && !body.includes(pole.heading)) {
    fail.push(`expected the heading "${pole.heading}"`);
  }
  // The services/all-projects toggle is meaningless without a canvas.
  if (!pole.canvas && /All projects/.test(body)) {
    fail.push(`a ${pole.body} still offers the Services / All projects toggle`);
  }

  // ---- E1: each fact stated once ------------------------------------------------------------
  const strip = await page.locator('app-identity-strip').innerText();
  // The stat-bearing furniture: the identity strip and the tiles. This is exactly where the
  // duplication lived — the headline sentence, the strip beneath it and the tiles below each
  // printed the same counts. The status bar is global chrome on every page and is excluded, as is
  // findings prose, which quotes numbers to make a claim rather than to state a fact twice.
  const tiles = (await page.locator('app-home-tiles').count())
    ? await page.locator('app-home-tiles').innerText() : '';
  const furniture = `${strip}\n${tiles}`;
  writeFileSync(join(OUT, `${which}-furniture.txt`), furniture, 'utf-8');
  for (const token of ['entries', 'projects', 'types', 'public types', 'namespaces']) {
    const n = statedCounts(furniture, token);
    if (n > 1) {
      fail.push(`"${token}" is counted ${n} times across the strip and tiles — E1 says once`);
    }
  }
  // The strip must not carry a second coverage-shaped percentage beside the wiring one (E-1).
  if (/\d+% verified/.test(strip)) {
    fail.push('the identity strip still prints "% verified" — E-1 moved it into the ledger');
  }
  if (!/confidence/i.test(strip)) {
    fail.push('no confidence chip on the strip — the Confidence Ledger would be unreachable');
  }
  console.log(`  facts: no count stated twice · strip carries the ledger opener, not a second %`);

  // ---- E-2: one ranking rule ----------------------------------------------------------------
  if (pole.firstFlowIsRequest !== null) {
    const flows = /Top flows([\s\S]*?)(Needs attention|START HERE|See all|Run report)/i.exec(body)?.[1] ?? '';
    const firstLine = flows.split('\n').map((l) => l.trim()).filter(Boolean)[0] ?? '';
    const isRequest = /^(GET|POST|PUT|DELETE|PATCH)$/i.test(firstLine) || /^\//.test(firstLine);
    console.log(`  top flows: first row "${firstLine}"`);
    if (pole.firstFlowIsRequest && !isRequest) {
      fail.push(`Top flows still leads with an internal reaction: "${firstLine}"`);
    }
    // START HERE must agree with the list right above it.
    const hero = /START HERE([\s\S]*?)(See all|Run report|$)/i.exec(body)?.[1] ?? '';
    if (/RelayCommand|ViewModel/i.test(hero)) {
      fail.push(`START HERE offers a UI view-model command: ${hero.replace(/\s+/g, ' ').trim().slice(0, 120)}`);
    }
    console.log(`  start here: ${hero.replace(/\s+/g, ' ').trim().slice(0, 90)}`);
  }

  await browser.close();

  if (fail.length) {
    console.error(`\nFAIL (${which}) — ${fail.length} problem(s):`);
    for (const f of fail) console.error(`  - ${f}`);
    process.exit(1);
  }
  console.log(`\nPASS (${which}) — ${pole.body} body, facts stated once, one ranking rule · ${OUT}`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
