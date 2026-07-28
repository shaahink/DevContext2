/**
 * S8 / R3 D-D (D1) — the CLI workspace opens on its commands.
 *
 * GitVersion's new-cli solution ships five verbs. Before this, Explore landed a CliTool on the
 * topology canvas, which for a command-line tool is two boxes and no edges: a CLI has no transports
 * by construction. The engine has projected a COMMAND SURFACE since L7.2 and no desktop surface read
 * it.
 *
 * The run: analyze GitVersion → switch to the solution that holds the verbs → assert Explore's
 * centre is the command surface and not a canvas → click a command → assert it traces.
 *
 * Usage: node --experimental-strip-types r3-verify-cli-surface.mts
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = 'C:\\code\\DevContext2\\eval-repos\\GitVersion';
const OUT = 'C:/code/DevContext2/eval-results/2026-07-28/r3-current-state/gitversion-commands';
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

  // The verbs live in new-cli/, which the scorer does not pick.
  const alt = page.locator('app-identity-strip button', { hasText: 'new-cli/GitVersion.slnx' }).first();
  if (await alt.count()) {
    await alt.click();
    await page.waitForFunction(() => !/\b(analyzing|cloning)\b/i.test(document.body.innerText), undefined, { timeout: 900_000 });
    await sleep(4000);
  }

  await page.goto(`${APP}/explore`, { waitUntil: 'domcontentloaded' });
  await sleep(5000);

  const surface = page.locator('app-command-surface');
  if (!(await surface.count())) {
    fail.push('Explore did not land on the command surface');
  } else {
    const text = await surface.innerText();
    writeFileSync(join(OUT, '01-command-surface.txt'), text, 'utf-8');
    if (!/COMMAND SURFACE/i.test(text)) fail.push('the section is not labelled COMMAND SURFACE');
    if (!/\d+ commands?/i.test(text)) fail.push('the command count is missing');
    console.log('  surface:\n' + text.split('\n').map((l) => '    ' + l).join('\n'));
  }
  await page.screenshot({ path: join(OUT, '01-command-surface.png'), fullPage: true });

  // Clicking a command must focus it the way the deck does.
  const firstCommand = surface.locator('button:not([disabled])').first();
  if (!(await firstCommand.count())) {
    fail.push('no command row is clickable (none matched a loaded entry)');
  } else {
    const label = (await firstCommand.innerText()).split('\n')[0];
    await firstCommand.click();
    await sleep(4000);
    await page.screenshot({ path: join(OUT, '02-command-focused.png'), fullPage: true });
    const after = await page.locator('app-stage').innerText();
    writeFileSync(join(OUT, '02-command-focused.txt'), after, 'utf-8');
    if (await page.locator('app-command-surface').count()) {
      fail.push(`clicking "${label}" left the surface up instead of tracing it`);
    }
    console.log(`  focused: ${label}`);
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
