/**
 * S8 / R3 D-C (C2) — the library's front doors open onto a real consumer path.
 *
 * FluentValidation's Library surface was a read-only list: the rows were list items, nothing on the
 * page could be selected, and the product's whole loop (inspect, trail, pin, export pack) was
 * unreachable from a library's main surface. C2 makes the front doors the spine — clicking one
 * traces the type or member it names and shows what the framework calls back.
 *
 * Usage: node --experimental-strip-types r3-verify-consumer-path.mts [repoPath]
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\FluentValidation';
const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/r3-current-state/fluentvalidation-c2';
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
    await sleep(3000);
  }

  await page.goto(`${APP}/explore`, { waitUntil: 'domcontentloaded' });
  await sleep(4000);

  const bench = page.locator('app-library-workbench');
  if (!(await bench.count())) {
    fail.push('the library workbench did not render');
  } else {
    await page.screenshot({ path: join(OUT, '01-doors-closed.png'), fullPage: true });

    // The derive door is the one whose path is the interesting answer: what a consumer overrides.
    const door = bench.locator('button', { hasText: 'AbstractValidator' }).first();
    if (!(await door.count())) {
      fail.push('no AbstractValidator front door to open');
    } else {
      await door.click();
      await sleep(6000);
      await page.screenshot({ path: join(OUT, '02-path-open.png'), fullPage: true });

      const aside = bench.locator('aside');
      if (!(await aside.count())) {
        fail.push('opening a front door produced no path panel');
      } else {
        const text = await aside.innerText();
        writeFileSync(join(OUT, '02-path.txt'), text, 'utf-8');
        console.log('  path:\n' + text.split('\n').slice(0, 24).map((l) => '    ' + l).join('\n'));
        if (!/consumer path/i.test(text)) fail.push('the panel is not labelled');
        // The path must be a real call path, not a restatement of the door.
        if (!/Validate|PreValidate|RuleFor|ValidationContext/i.test(text)) {
          fail.push('the panel shows no call into the validator pipeline');
        }
      }
    }
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
