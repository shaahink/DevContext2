// Shamshir UI page tour — assumes server :5179 + ng serve :4200 already running.
// Analyzes shamshir via the UI, then screenshots all 7 pages.
// Usage: node eval-results/2026-07-15/wrapup-drive/shamshir-ui-tour.mjs
import { chromium } from "playwright";
import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.join(__dirname, "ui-shamshir");
fs.mkdirSync(OUT, { recursive: true });
const APP = "http://localhost:4200";
const REPO = "C:/code/shamshir";

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1600, height: 950 } });
const shot = async (name) => { await page.screenshot({ path: path.join(OUT, `${name}.png`) }); console.log("shot:", name); };

await page.goto(APP, { waitUntil: "networkidle" });
await shot("00-landing");

// Analyze shamshir: source bar input (same selectors ui-audit-drive uses)
const input = page.locator("input[placeholder*='path' i], input[placeholder*='repo' i], input[type='text']").first();
await input.fill(REPO);
await input.press("Enter");
console.log("analyzing…");
// wait for entries badge / analysis completion (up to 180s)
try {
  await page.waitForFunction(
    () => !document.body.innerText.includes("Analyzing") && (document.body.innerText.match(/\d+\s*entries/i) || document.body.innerText.includes("entry")),
    null, { timeout: 180000 });
} catch { console.log("analysis wait timed out — continuing"); }
await page.waitForTimeout(3000);
await shot("01-home");

for (const [route, name] of [["/explore", "02-explore"], ["/atlas", "03-atlas"], ["/insights", "04-insights"], ["/context", "05-context"], ["/mcp", "06-mcp"], ["/settings", "07-settings"]]) {
  await page.goto(APP + route, { waitUntil: "networkidle" });
  await page.waitForTimeout(2500);
  await shot(name);
}

// Explore: select first entry to populate inspector, then table lens
await page.goto(APP + "/explore", { waitUntil: "networkidle" });
await page.waitForTimeout(2000);
const entry = page.locator("[class*='entry'], [data-testid*='entry']").first();
try { await entry.click({ timeout: 5000 }); await page.waitForTimeout(2000); await shot("08-explore-entry-selected"); } catch { console.log("no entry clickable"); }

await browser.close();
console.log("done →", OUT);
