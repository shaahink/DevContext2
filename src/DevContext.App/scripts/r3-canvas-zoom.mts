/**
 * R3 D-B — a close-up of the topology canvas alone.
 *
 * The full-page frames from r3-current-state.mts answer "what is on the page"; they are too small to
 * judge a node SHAPE or an edge weight, which is most of what a canvas-language decision is about.
 * This screenshots the canvas element by itself at 3x so the vocabulary can actually be read.
 *
 * S8: an optional third arg picks the canvas LEVEL ("projects" for the all-projects lane view,
 * "services" for C4 level 1). The lane views are where R3 D-B's tail landed, and they are one click
 * off the default — a capture that never clicks it cannot judge them.
 *
 * Usage: node --experimental-strip-types r3-canvas-zoom.mts <repoPath> <outName> [services|projects]
 */
import { chromium } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\eShop';
const NAME = process.argv[3] ?? 'zoom';
const LEVEL = process.argv[4] as 'services' | 'projects' | undefined;
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

  if (LEVEL) {
    const chip = canvas.locator('button', { hasText: LEVEL === 'projects' ? 'All projects' : 'Services' }).first();
    if (await chip.count()) {
      await chip.click();
      await sleep(4000);
    } else {
      // The chips appear only when the ServiceMap facet gave ≥2 services; on a library or a CLI
      // there is no toggle because the projects level IS the only level. Say so rather than
      // silently shooting a different canvas than the one that was asked for.
      console.log(`  note: no level toggle on this repo — the canvas is already the projects level`);
    }
  }

  await canvas.screenshot({ path: join(OUT, 'canvas.png') });
  console.log(`  canvas -> ${join(OUT, 'canvas.png')}`);

  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
