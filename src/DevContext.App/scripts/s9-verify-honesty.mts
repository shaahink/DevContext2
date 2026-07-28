/**
 * S9 / contract sweep — proof that two facts the engine has always computed now reach a reader.
 *
 * Both were dead by construction in the app, the third and fourth instances of the shape this
 * program keeps finding (S7's kind glyph, S8's scope_note and ArchetypeView):
 *
 *   swallowed failures  J1/J3 counted every exception extraction ate, the CLI prints a table of
 *                       them, and the app rendered every OTHER section of the same stats payload.
 *   sparse graph        L3.4 broadens call-edge binding when a repo has few entries, and NOTHING
 *                       said so -- not the CLI, not the app. The confidence panel quoted an edge
 *                       percentage without saying those edges were found under a looser rule.
 *
 * Two poles, because no single repo proves both AND proves they are conditional:
 *   eShop  2 ConfigDefaultsSource failures, NOT sparse -> failures table, NO sparse line
 *   CLI    a clean run, sparse over 9 hubs             -> sparse line, NO failures table
 * Each pole is therefore the other's negative control: a section that renders on both is a section
 * that is not reading its data.
 *
 * The truth below is MEASURED (`DevContext.Cli.exe query stats --path <repo>`), not inferred from
 * L3.4's doc comment. That comment says sparseness is "entries < 5 or edge/node ratio < 0.1", and
 * on that reading FluentValidation (0 entries) should be sparse -- it is not, because the rule also
 * requires enough central types to broaden over, and most entry-poor libraries fail that second
 * test. The first draft of this driver asserted the comment and failed against a correct app.
 *
 * Usage: node --experimental-strip-types s9-verify-honesty.mts [eshop|cli]
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/s9-contract-sweep';

/** Ground truth per pole — the same shape as an eval expectation file, inline because it is two rows. */
const POLES = {
  eshop: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\eShop',
    failures: 'ConfigDefaultsSource', // measured: config-json x2
    sparse: false,                    // measured: 109 entries, not broadened -> no caveat
  },
  cli: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\CLI',
    failures: null,                   // measured: a clean run -> the section must NOT appear
    sparse: true,                     // measured: sparse, binding broadened over 9 central types
  },
} as const;

const which = (process.argv[2] ?? 'eshop').toLowerCase() as keyof typeof POLES;
const pole = POLES[which];
if (!pole) { console.error(`unknown pole: ${which}`); process.exit(2); }
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

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
  // Stats arrive after the identity strip; the console only flips to report mode once they land.
  await page.waitForFunction(
    () => !/\b(analyzing|cloning)\b/i.test(document.body.innerText),
    undefined,
    { timeout: 900_000 },
  );
  await sleep(6000);

  // On a ready Home the console lives inside a COLLAPSED <details> ("Run report") — the inline
  // <app-run-console /> above it only renders while the analysis is running. innerText skips
  // collapsed content, so the report has to be opened before any of it can be asserted.
  const report = page.locator('summary', { hasText: 'Run report' }).first();
  if (await report.count()) {
    await report.click();
    await sleep(2500);
  } else {
    fail.push('no "Run report" disclosure on a ready Home');
  }

  // The run console is mounted twice on Home (boot layout + ready layout, one hidden), so assert
  // against the page rather than a component that may be the hidden copy.
  const body = await page.locator('body').innerText();
  writeFileSync(join(OUT, `${which}-01-home.txt`), body, 'utf-8');
  await page.screenshot({ path: join(OUT, `${which}-01-home.png`), fullPage: true });

  // ---- 1. Swallowed failures ---------------------------------------------------------------
  const hasSection = /Swallowed failures/i.test(body);
  if (pole.failures) {
    if (!hasSection) fail.push('run console does not show the swallowed-failure section');
    if (!body.includes(pole.failures)) fail.push(`the failing source (${pole.failures}) is not named`);
    if (!/config-json/.test(body)) fail.push('the failure category is not shown');
    const n = new RegExp(`${pole.failures}\\s+config-json\\s+(\\d+)`).exec(body)?.[1];
    console.log(`  swallowed failures: ${n ?? '(count not parsed)'}`);
    if (!n || Number(n) < 1) fail.push('failure count is missing or zero');
  } else if (hasSection) {
    fail.push('a clean run still renders the swallowed-failure section');
  } else {
    console.log('  swallowed failures: none, and no section drawn (correct)');
  }

  // ---- 2. Sparse graph ---------------------------------------------------------------------
  const strip = page.locator('app-identity-strip');
  if (/Sparse graph/i.test(await strip.innerText())) {
    fail.push('the sparse caveat renders while the ledger is collapsed (it belongs inside it)');
  }

  // The ledger opens from the confidence chip ONLY. Every other button[title] in this strip is a
  // switch-solution button, and clicking one re-analyzes the repo under a different scope.
  // S10 (D-E E-1): that chip used to read "8% verified" and now reads "edge confidence" — the
  // percentage moved inside the panel it opens, because three coverage-shaped numbers shared the
  // strip and only two of them meant the same thing.
  const chip = strip.locator('button', { hasText: /confidence/ }).first();
  if (!(await chip.count())) {
    fail.push(`no verified chip to open the ledger with; strip read: ${(await strip.innerText()).replace(/\s+/g, ' ').slice(0, 200)}`);
  } else {
    await chip.click();
    await sleep(1200);
    const expanded = await strip.innerText();
    writeFileSync(join(OUT, `${which}-02-ledger.txt`), expanded, 'utf-8');
    await strip.screenshot({ path: join(OUT, `${which}-02-ledger.png`) }).catch(() => {});

    if (!/Confidence Ledger/i.test(expanded)) fail.push('the ledger did not open');
    const sparseShown = /Sparse graph/i.test(expanded);
    if (pole.sparse && !sparseShown) {
      fail.push('an entry-poor repo does not report its graph as sparse');
    } else if (!pole.sparse && sparseShown) {
      fail.push('a dense repo claims a sparse graph');
    } else if (pole.sparse) {
      const hubs = /broadened over (\d+) central/.exec(expanded)?.[1];
      console.log(`  sparse caveat: broadened over ${hubs ?? '(unstated)'} central types`);
      if (!hubs) fail.push('the caveat does not say how many central types binding was broadened over');
    } else {
      console.log('  sparse caveat: absent on a dense repo (correct)');
    }
  }

  await browser.close();
  if (fail.length) {
    console.error(`FAIL (${which}):\n  - ` + fail.join('\n  - '));
    process.exit(1);
  }
  console.log(`PASS · ${which} · ${OUT}`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
