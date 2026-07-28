/**
 * R3 D-B — a close-up of the topology canvas alone.
 *
 * The full-page frames from r3-current-state.mts answer "what is on the page"; they are too small to
 * judge a node SHAPE or an edge weight, which is most of what a canvas-language decision is about.
 * This screenshots the canvas element by itself at 3x so the vocabulary can actually be read.
 *
 * Usage: node --experimental-strip-types r3-canvas-zoom.mts <repoPath> <outName>
 */
import { chromium } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\eShop';
const NAME = process.argv[3] ?? 'zoom';
const OUT = join('C:/code/DevContext2/eval-results/2026-07-28/r3-current-state', NAME);
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 3 });
  const page = await context.newPage();

  // Bootstrap via Home: navigating straight to /explore lands on an unanalysed app (S6 trap 4).
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
  await sleep(5000);

  const canvas = page.locator('app-graph-canvas').first();
  await canvas.screenshot({ path: join(OUT, 'canvas.png') });
  console.log(`  canvas -> ${join(OUT, 'canvas.png')}`);

  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
