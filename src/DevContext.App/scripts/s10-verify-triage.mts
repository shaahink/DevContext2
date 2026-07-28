/**
 * S10 — proof that the four defects this session found are fixed, in the running app.
 *
 * All four are the shape this program keeps finding: the engine computes something honest and a
 * client fails to read it. S9's contract sweep catches a field NO client reads; these are the
 * variant it cannot catch — a field every client reads with the WRONG KEY, which is silent twice
 * over because the data arrives, the code runs, and the branch simply never matches.
 *
 *   1. severity casing   The wire shipped "Warning" (Severity.ToString()). The app keyed every
 *                        lookup on "warning" and the MCP filtered on "WARNING". Consequences, all
 *                        live on eShop before this fix: the Insights page filed three security
 *                        warnings under "Know this" and NEVER rendered its "Act on this" group;
 *                        those warnings drew the info-blue border instead of danger; Home's "Needs
 *                        attention" showed only the one row the app synthesizes itself (which uses
 *                        a lowercase literal), hiding all three engine warnings behind a "See all
 *                        10 insights" link; and mcp stats.warnings was [] on every repo.
 *   2. style sentinel    Batch C answers "NotApplicable" for a CliTool rather than guessing. The
 *                        CLI prints no STYLE line; the app printed the enum name, so GitVersion's
 *                        headline read "CliTool  NotApplicable".
 *   3. picker truncation A-4's middle-ellipsis reached the entry deck in S7 and not the Context
 *                        Studio, whose narrower column rendered ELEVEN rows reading
 *                        "/api/catalog/i...". Same defect, second component.
 *   4. selection state   The picker marked a selected row with `bg-hover` -- the same class the
 *                        hover rule sets, so the picked row and the row under the cursor were
 *                        indistinguishable.
 *
 * Two poles, each the other's negative control: eShop HAS warnings and a style, GitVersion has
 * neither and is the repo whose sentinel leaked.
 *
 * Usage: node --experimental-strip-types s10-verify-triage.mts [eshop|gitversion]
 * Exits non-zero on any failure.
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/s10-triage';

const POLES = {
  eshop: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\eShop',
    /** MEASURED (eshop-03-insights.txt), not assumed: three Severity.Warning insights. */
    warnings: 3,
    /** "Act on this" groups warning AND notable — see IMPACT_GROUPS in insights-view.ts. */
    actOn: true,
    /** A substring of one engine warning title — must reach Home's triage list. */
    warningTitle: 'endpoints anonymous',
    style: 'Microservices',
  },
  gitversion: {
    repo: 'C:\\code\\DevContext2\\eval-repos\\GitVersion',
    warnings: 0,
    // MEASURED: no warnings, but two NOTABLE insights (most-depended-upon, multi-impl
    // interfaces), which belong under "Act on this" too. The first draft of this driver
    // asserted "no warnings -> no Act-on-this group" and failed against a correct app — the
    // same mistake S9 made reading a verdict off a doc comment instead of measuring it.
    actOn: true,
    warningTitle: null,
    style: null, // NotApplicable -> the chip must collapse, not print the sentinel
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
  await page.waitForFunction(
    () => !/\b(analyzing|cloning)\b/i.test(document.body.innerText),
    undefined,
    { timeout: 900_000 },
  );
  await sleep(6000);

  // ---- 2. The style sentinel (identity strip) ----------------------------------------------
  const strip = await page.locator('app-identity-strip').innerText();
  writeFileSync(join(OUT, `${which}-01-strip.txt`), strip, 'utf-8');
  if (/NotApplicable|Unknown/.test(strip)) {
    fail.push(`the identity strip prints a sentinel as a style: ${strip.replace(/\s+/g, ' ').slice(0, 160)}`);
  }
  if (pole.style && !strip.includes(pole.style)) {
    fail.push(`a real style (${pole.style}) stopped rendering — the suppression is too broad`);
  }
  console.log(`  style chip: ${pole.style ?? '(suppressed, correctly)'}`);

  // ---- 1a. Home triage ----------------------------------------------------------------------
  const home = await page.locator('body').innerText();
  writeFileSync(join(OUT, `${which}-02-home.txt`), home, 'utf-8');
  await page.screenshot({ path: join(OUT, `${which}-02-home.png`), fullPage: true });

  if (pole.warningTitle) {
    const attention = /Needs attention([\s\S]*?)(START HERE|See all|Run report)/i.exec(home)?.[1] ?? '';
    if (!attention.includes(pole.warningTitle)) {
      fail.push(
        `Home's "Needs attention" does not carry the engine warning "${pole.warningTitle}" — ` +
        `it read: ${attention.replace(/\s+/g, ' ').trim().slice(0, 200)}`);
    } else {
      console.log(`  home triage: engine warning present ("${pole.warningTitle}")`);
    }
  }

  // ---- 1b. Insights grouping ----------------------------------------------------------------
  await page.goto(`${APP}/insights`, { waitUntil: 'domcontentloaded' });
  await sleep(3500);
  const insights = await page.locator('body').innerText();
  writeFileSync(join(OUT, `${which}-03-insights.txt`), insights, 'utf-8');
  await page.screenshot({ path: join(OUT, `${which}-03-insights.png`), fullPage: true });

  const actOn = /ACT ON THIS/i.test(insights);
  if (pole.actOn !== actOn) {
    fail.push(pole.actOn
      ? 'the Insights page has actionable findings but renders no "Act on this" group'
      : 'a repo with nothing actionable still renders an "Act on this" group');
  }
  // Every warning must sit ABOVE the "Know this" heading, i.e. inside the acted-on group.
  const actBlock = /ACT ON THIS([\s\S]*?)(KNOW THIS|COVERAGE)/i.exec(insights)?.[1] ?? '';
  const seen = (actBlock.match(/\bwarning\b/g) ?? []).length;
  console.log(`  insights: "Act on this" ${actOn ? 'present' : 'absent'}, ${seen} warning chip(s) inside it`);
  if (seen !== pole.warnings) {
    fail.push(`expected ${pole.warnings} warning(s) under "Act on this", saw ${seen}`);
  }
  // The casing regression itself: a PascalCase severity means the wire spelling drifted back.
  if (/\b(Warning|Notable|Info)\b/.test(insights)) {
    fail.push('a PascalCase severity is reaching the page — the wire spelling drifted');
  }

  // ---- 3 + 4. Context Studio picker ----------------------------------------------------------
  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(3500);

  const rows = page.locator('app-scope-picker button[aria-pressed]');
  const count = await rows.count();
  if (count === 0) {
    fail.push('the scope picker rendered no entry rows');
  } else {
    // Shown label AND the full text on [title]. The bar is not "no two rows read alike" — eShop
    // genuinely ships three Identity.API actions on GET /Account, and saying so is honest. The bar
    // is that TRUNCATION must not ADD ambiguity the data does not have: two rows may share a label
    // only if they share a full label too.
    const seen: { short: string; full: string }[] = [];
    for (let i = 0; i < Math.min(count, 60); i++) {
      const row = rows.nth(i);
      const label = row.locator('span.font-mono').first();
      seen.push({
        // The WHOLE row, because the HTTP method is a sibling span: three rows reading
        // "/api/catalog/items/{id:int}" are GET/PUT/DELETE on screen and not ambiguous at all.
        // Comparing the label span alone reported them as a collision — a defect in the
        // measurement, not the product.
        short: (await row.innerText()).replace(/\s+/g, ' ').trim(),
        full: ((await label.getAttribute('title')) ?? '').replace(/\s+/g, ' ').trim(),
      });
    }
    writeFileSync(join(OUT, `${which}-04-picker.txt`),
      seen.map((s) => `${s.short}\t${s.full}`).join('\n'), 'utf-8');

    const byShort = new Map<string, Set<string>>();
    for (const { short, full } of seen) {
      if (!byShort.has(short)) byShort.set(short, new Set());
      byShort.get(short)!.add(full);
    }
    // Two different failure shapes, and only the first is a truncation defect:
    //   truncated  the label was ellipsised and the cut destroyed the difference. THIS is what
    //              A-4 is about and what this session fixed — it must be zero.
    //   unshown    the label was NOT cut and still is not unique, because the row renders the
    //              route and the data disambiguates elsewhere (eShop's Identity.API ships GET
    //              /Account, GET /Account [Logout] and GET /Account [AccessDenied]). Reported,
    //              not failed: what a picker row should show is an OPEN D-G decision, and
    //              guessing one here would be inventing a design the owner has not chosen.
    const collisions = [...byShort].filter(([, fulls]) => fulls.size > 1);
    const truncated = collisions.filter(([short]) => short.includes('…'));
    const unshown = collisions.filter(([short]) => !short.includes('…'));
    // A-4's scope is a ROUTE cut into ambiguity — the audit's six identical `/api/catalog/i…`
    // rows. That is the regression bar and it is zero. A long NAMED entry is a different
    // problem with no truncation answer: eShop ships five OrderStatusChangedTo*EventHandlers
    // that agree on their first 24 and last 18 characters, so no 26-character rendering can
    // separate them. Reported as D-G evidence, not asserted, because the fix is a design call
    // (show the target member, widen the column, or two-line rows) that the owner has not made.
    const routeCollisions = truncated.filter(([, fulls]) => [...fulls].some((f) => f.includes('/')));
    console.log(`  picker: ${count} rows · ${routeCollisions.length} ROUTE truncation collision(s) · ` +
      `${truncated.length - routeCollisions.length} long-name collision(s) · ` +
      `${unshown.length} label(s) not unique before truncation`);
    for (const [s, f] of [...truncated, ...unshown]) {
      if (routeCollisions.some(([r]) => r === s)) continue;
      console.log(`      D-G evidence — "${s}" <- ${[...f].map((x) => x.split('—').pop()?.trim()).join(' | ')}`);
    }
    if (routeCollisions.length) {
      fail.push('truncation makes distinct ROUTES read alike: ' +
        routeCollisions.map(([s, f]) => `"${s}" <- ${[...f].join(' | ')}`).join('; '));
    }

    // Selection must be visually distinct from hover, which means a class hover does not set.
    await rows.first().click();
    await sleep(400);
    const cls = (await rows.first().getAttribute('class')) ?? '';
    const pressed = await rows.first().getAttribute('aria-pressed');
    if (pressed !== 'true') fail.push('clicking a picker row did not select it');
    if (!/border-accent|bg-accent/.test(cls)) {
      fail.push(`a selected row carries no distinct selected styling (class="${cls}")`);
    } else {
      console.log('  picker: selection is visually distinct from hover');
    }
    await page.screenshot({ path: join(OUT, `${which}-05-picker.png`), fullPage: false });
  }

  await browser.close();

  if (fail.length) {
    console.error(`\nFAIL (${which}) — ${fail.length} problem(s):`);
    for (const f of fail) console.error(`  - ${f}`);
    process.exit(1);
  }
  console.log(`\nPASS (${which}) — triage, style chip and picker all honest · ${OUT}`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
