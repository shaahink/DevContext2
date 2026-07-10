// M0.3 Playwright visual gate — screenshots 4 surfaces + interaction steps.
// Usage: node --experimental-strip-types scripts/visual-gate.mts
// Requires: pnpm server + ng serve running.
// Output: ../../eval-results/<date>/ui/

import { chromium } from "playwright";
import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, "..", "..", "..");

const DOGFOOD = "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";
const APP_URL = "http://localhost:4200";
const DATE = new Date().toISOString().slice(0, 10);
const OUT = path.join(ROOT, "eval-results", DATE, "ui");

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  fs.mkdirSync(OUT, { recursive: true });
  const browser = await chromium.launch({ channel: "chrome", headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await ctx.newPage();

  try {
    // 1. Load app
    await page.goto(APP_URL, { waitUntil: "networkidle" });
    await sleep(3000);

    // 2. Wait until server connection dot appears green
    await waitForServer(page);

    // 3. Try to analyze the dogfood repo via the home page
    const analyzed = await tryAnalyze(page);
    if (!analyzed) console.log("  Skipped analysis — doing static screenshots");

    // 4. Screenshot surfaces
    const darkColors = page.emulateMedia.bind(page, { colorScheme: "dark" });
    const lightColors = page.emulateMedia.bind(page, { colorScheme: "light" });

    for (const [label, route, selector] of SURFACES) {
      // Navigate client-side
      await navigateTo(page, route);
      await sleep(800);

      await page.screenshot({ path: path.join(OUT, `${label}-dark.png`), fullPage: true });
      console.log(`  ${label} dark`);

      await lightColors();
      await sleep(200);
      await page.screenshot({ path: path.join(OUT, `${label}-light.png`), fullPage: true });
      console.log(`  ${label} light`);

      await darkColors();
    }

    // 5. Interaction: try clicking graph/deck entries on Explore
    await navigateTo(page, "/explore");
    await sleep(1500);

    // Click a deck row if visible
    const row = await page.$(".list-row, .deck-row, [data-testid='entry-row']");
    if (row) {
      await row.click();
      await sleep(1500);
    }

    await page.screenshot({ path: path.join(OUT, "explore-interaction.png"), fullPage: true });
    console.log("  explore-interaction");

    console.log(`\nScreenshots saved to ${OUT}`);
  } finally {
    await browser.close();
  }
}

const SURFACES: Array<[string, string, string]> = [
  ["home", "/", "app-home-page"],
  ["explore", "/explore", "app-workbench-page"],
  ["atlas", "/atlas", "app-atlas-page"],
  ["insights", "/insights", "app-insights-page"],
];

async function waitForServer(page: any) {
  for (let i = 0; i < 30; i++) {
    const online = await page.evaluate(() => {
      const el = document.querySelector("[class*='online']");
      return !!el;
    });
    if (online) { console.log("  Server: online"); return; }
    await sleep(1000);
  }
  console.log("  Server: not detected in 30s");
}

async function tryAnalyze(page: any): Promise<boolean> {
  // Find the repo input on the home page
  const input = await page.$(
    'input:not([type="hidden"]), textarea'
  );
  if (!input) { console.log("  No input found"); return false; }

  await input.fill(DOGFOOD);
  await sleep(300);

  // Press Enter or click a button
  const btn = await page.$("button");
  if (btn) await btn.click();
  else await input.press("Enter");

  console.log("  Analyzing...");

  // Wait for analysis to complete
  for (let i = 0; i < 90; i++) {
    const hasStats = await page.evaluate(() =>
      document.body.innerText.includes("Nodes")
    );
    if (hasStats) { console.log("  Analysis complete"); return true; }
    await sleep(1000);
  }
  console.log("  Analysis timed out");
  return false;
}

async function navigateTo(page: any, route: string) {
  await page.evaluate((r: string) => {
    const a = document.querySelector(`a[href="${r}"]`) as HTMLElement;
    if (a) a.click();
  }, route);
}

main().catch((e) => {
  console.error("FATAL:", e);
  process.exitCode = 1;
});
