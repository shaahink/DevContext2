/**
 * S8 / R3 D-D (D2) — proof that the solution scope is now visible AND reachable.
 *
 * GitVersion declares three solutions. The scorer picks `src/GitVersion.slnx`, whose CLI is the
 * legacy hand-rolled parser: one Main. The five verbs the tool ships live in `new-cli/`, and until
 * this change the desktop had no way to say which slice it read, let alone read another one.
 *
 * The run: analyze the repo the way a user first meets it → assert the scope row names the pick and
 * counts the rest → click the alternative → assert the entry count actually changes.
 *
 * Usage: node --experimental-strip-types r3-verify-scope.mts [repoPath]
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\GitVersion';
const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/r3-current-state/gitversion-scope';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1.5 });
  const page = await context.newPage();
  const fail: string[] = [];

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

  const strip = page.locator('app-identity-strip');
  const before = await strip.innerText();
  writeFileSync(join(OUT, '01-default-scope.txt'), before, 'utf-8');
  await page.screenshot({ path: join(OUT, '01-default-scope.png'), fullPage: true });

  // The default read must NAME its slice and COUNT the others.
  if (!/Analyzed\s+src[/\\]GitVersion\.slnx/i.test(before)) fail.push('scope row does not name src/GitVersion.slnx');
  if (!/1 of 3 solutions/i.test(before)) fail.push('scope row does not count the three solutions');
  if (!before.includes('new-cli/GitVersion.slnx')) fail.push('the new-cli solution is not offered');

  const entriesBefore = /(\d+)\s+entries/.exec(before)?.[1] ?? '?';
  console.log(`  default scope: ${entriesBefore} entries`);

  // Switch to the solution that holds the modern CLI.
  const alt = strip.locator('button', { hasText: 'new-cli/GitVersion.slnx' }).first();
  if (!(await alt.count())) {
    fail.push('no picker button for new-cli/GitVersion.slnx');
  } else {
    await alt.click();
    await page.waitForFunction(
      () => !/\b(analyzing|cloning)\b/i.test(document.body.innerText),
      undefined,
      { timeout: 900_000 },
    );
    await sleep(5000);
    const after = await strip.innerText();
    writeFileSync(join(OUT, '02-new-cli-scope.txt'), after, 'utf-8');
    await page.screenshot({ path: join(OUT, '02-new-cli-scope.png'), fullPage: true });

    const entriesAfter = /(\d+)\s+entries/.exec(after)?.[1] ?? '?';
    console.log(`  new-cli scope: ${entriesAfter} entries`);

    if (!/Analyzed\s+new-cli[/\\]GitVersion\.slnx/i.test(after)) fail.push('after switching, the scope row still names the old solution');
    if (entriesAfter === entriesBefore) fail.push(`entry count did not move (${entriesBefore} → ${entriesAfter})`);
  }

  await browser.close();
  if (fail.length) {
    console.error('FAIL:\n  - ' + fail.join('\n  - '));
    process.exit(1);
  }
  console.log(`PASS · ${OUT}`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
