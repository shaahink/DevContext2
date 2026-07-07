// UI audit drive — screenshots every surface + probes specific reported issues.
// Usage: node scripts/ui-audit-drive.mjs
// Requires: server on :5179 + ng serve on :4200 already running.
// Output: ../../eval-results/2026-07-07/ui-audit/

import { chromium } from "playwright";
import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, "..", "..", "..");
const DOGFOOD = "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";
const SECOND_REPO = path.join(ROOT, "eval-repos", "TodoApi").replaceAll("\\", "/");
const APP_URL = "http://localhost:4200";
const OUT = path.join(ROOT, "eval-results", "2026-07-07", "ui-audit");

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const notes = [];
const note = (s) => { notes.push(s); console.log("NOTE: " + s); };

async function shot(page, name) {
  await page.screenshot({ path: path.join(OUT, name + ".png") });
  console.log("  shot: " + name);
}

async function main() {
  fs.mkdirSync(OUT, { recursive: true });
  const browser = await chromium.launch({ channel: "chrome", headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1600, height: 950 }, colorScheme: "dark" });
  const page = await ctx.newPage();
  page.on("console", (m) => { if (m.type() === "error") note("console.error: " + m.text().slice(0, 200)); });

  // ── 1. boot + empty state ──
  await page.goto(APP_URL, { waitUntil: "networkidle" });
  await sleep(2500);
  await shot(page, "01-boot-initial");

  // ── 2. analyze dogfood ──
  const input = page.locator("input:visible").first();
  if (await input.count()) {
    await input.fill(DOGFOOD);
    await input.press("Enter");
    note("analyze: submitted dogfood path via first visible input");
  } else {
    note("analyze: NO visible input found on boot screen");
  }
  for (let i = 0; i < 120; i++) {
    const txt = await page.evaluate(() => document.body.innerText);
    if (/Nodes|entries|Entries/.test(txt) && !/Analyzing/.test(txt)) break;
    await sleep(1000);
  }
  await sleep(1500);
  await shot(page, "02-home-after-analyze");

  // ── 3. tab strip measurements ──
  const tabInfo = await page.evaluate(() => {
    const strip = document.querySelector("app-tab-strip");
    if (!strip) return { found: false };
    const r = strip.getBoundingClientRect();
    const tabs = [...strip.querySelectorAll("[role='tab']")].map((t) => {
      const b = t.getBoundingClientRect();
      const cs = getComputedStyle(t);
      return { w: Math.round(b.width), h: Math.round(b.height), fontSize: cs.fontSize, label: t.textContent?.trim().slice(0, 30) };
    });
    return { found: true, stripH: Math.round(r.height), tabs };
  });
  note("tab-strip: " + JSON.stringify(tabInfo));
  const stripEl = page.locator("app-tab-strip");
  if (await stripEl.count()) await stripEl.screenshot({ path: path.join(OUT, "03-tab-strip-closeup.png") });

  // ── 4. icon size survey ──
  const iconInfo = await page.evaluate(() => {
    const sizes = {};
    for (const svg of document.querySelectorAll("app-icon svg, .lucide, svg[width]")) {
      const b = svg.getBoundingClientRect();
      const k = Math.round(b.width) + "x" + Math.round(b.height);
      sizes[k] = (sizes[k] ?? 0) + 1;
    }
    return sizes;
  });
  note("icon sizes on home: " + JSON.stringify(iconInfo));

  // ── 5. second tab via + button, analyze second repo ──
  const plusBtn = page.locator("app-tab-strip button", { hasText: "+" });
  if (await plusBtn.count()) {
    await plusBtn.click();
    await sleep(1200);
    await shot(page, "04-new-tab-created");
    const input2 = page.locator("input:visible").first();
    if (await input2.count()) {
      await input2.fill(SECOND_REPO);
      await input2.press("Enter");
      for (let i = 0; i < 60; i++) {
        const txt = await page.evaluate(() => document.body.innerText);
        if (/Nodes|entries|Entries/.test(txt) && !/Analyzing/.test(txt)) break;
        await sleep(1000);
      }
      await sleep(1000);
      await shot(page, "05-second-tab-analyzed");
    }
    const tabsNow = await page.evaluate(() =>
      [...document.querySelectorAll("app-tab-strip [role='tab']")].map((t) => t.textContent?.trim()),
    );
    note("tabs after second analyze: " + JSON.stringify(tabsNow));
  } else {
    note("plus button NOT found in tab strip");
  }

  // ── 6. titlebar New button — the reported breaker ──
  const newBtn = page.locator("header button", { hasText: "New" });
  if (await newBtn.count()) {
    await newBtn.first().click();
    await sleep(1500);
    await shot(page, "06-after-titlebar-new");
    const tabsAfter = await page.evaluate(() =>
      [...document.querySelectorAll("app-tab-strip [role='tab']")].map((t) => t.textContent?.trim()),
    );
    note("tabs after titlebar New: " + JSON.stringify(tabsAfter));
  } else {
    note("titlebar New button not visible (session not ready?)");
  }

  // ── back to a working tab: click tab 1 if present ──
  const firstTab = page.locator("app-tab-strip [role='tab']").first();
  if (await firstTab.count()) { await firstTab.click(); await sleep(1500); }

  // If no session ready anymore, re-analyze dogfood
  {
    const txt = await page.evaluate(() => document.body.innerText);
    if (!/Nodes \d|nodes/.test(txt)) {
      const in3 = page.locator("input:visible").first();
      if (await in3.count()) {
        await in3.fill(DOGFOOD);
        await in3.press("Enter");
        for (let i = 0; i < 90; i++) {
          const t2 = await page.evaluate(() => document.body.innerText);
          if (/Nodes|entries|Entries/.test(t2) && !/Analyzing/.test(t2)) break;
          await sleep(1000);
        }
      }
    }
  }

  // ── 7. Explore: deck → node select → inspector sections ──
  await page.evaluate(() => (document.querySelector("a[href='/explore']"))?.click());
  await sleep(2500);
  await shot(page, "07-explore-initial");
  const row = page.locator(".list-row").first();
  if (await row.count()) {
    await row.click();
    await sleep(2500);
    await shot(page, "08-explore-entry-selected");
    // inspector section headers
    const sections = await page.evaluate(() =>
      [...document.querySelectorAll("app-inspector .section-h")].map((b) => b.textContent?.trim()),
    );
    note("inspector sections: " + JSON.stringify(sections));
    // open Code section
    const codeBtn = page.locator("app-inspector .section-h", { hasText: "Code" });
    if (await codeBtn.count()) {
      await codeBtn.click();
      await sleep(1500);
      await shot(page, "09-inspector-code-open");
      const codeContent = await page.evaluate(() => {
        const pre = document.querySelector("app-inspector pre, app-inspector code");
        return pre ? (pre.textContent ?? "").slice(0, 120) : null;
      });
      note("inspector code content sample: " + JSON.stringify(codeContent));
    } else {
      note("inspector Code section NOT found");
    }
    // open Insights + Call Stack (new M9-ext sections)
    for (const sec of ["Insights", "Call Stack"]) {
      const b = page.locator("app-inspector .section-h", { hasText: sec });
      if (await b.count()) { await b.click(); await sleep(800); }
      else note(`inspector section '${sec}' NOT found`);
    }
    await shot(page, "10-inspector-all-sections");
  } else {
    note("explore: no .list-row found in deck");
  }

  // ── 8. lens switcher: layer lens ──
  const lensBtns = await page.evaluate(() =>
    [...document.querySelectorAll("button")].filter((b) => /^(Service|Flow|Layer|Feature)$/.test(b.textContent?.trim() ?? "")).map((b) => b.textContent?.trim()),
  );
  note("lens buttons found: " + JSON.stringify(lensBtns));
  const layerBtn = page.locator("button", { hasText: /^Layer$/ }).first();
  if (await layerBtn.count()) {
    await layerBtn.click();
    await sleep(2000);
    await shot(page, "11-explore-layer-lens");
  }

  // ── 9. table lens (Shift+E) ──
  await page.keyboard.press("Shift+E");
  await sleep(1500);
  await shot(page, "12-table-lens");

  // ── 10. Atlas ──
  await page.evaluate(() => (document.querySelector("a[href='/atlas']"))?.click());
  await sleep(2500);
  await shot(page, "13-atlas");

  // ── 11. Insights page ──
  await page.evaluate(() => (document.querySelector("a[href='/insights']"))?.click());
  await sleep(1800);
  await shot(page, "14-insights");

  // ── 12. Context Studio ──
  await page.evaluate(() => (document.querySelector("a[href='/context']"))?.click());
  await sleep(2200);
  await shot(page, "15-context-studio-initial");
  // try clicking a scope tree entry then a preset
  const scopeRow = page.locator("app-scope-picker .list-row, app-scope-picker button").first();
  if (await scopeRow.count()) {
    await scopeRow.click();
    await sleep(1200);
    await shot(page, "16-context-scope-clicked");
  }
  const anyPreset = page.locator("button", { hasText: /preset|Preset/ }).first();
  if (await anyPreset.count()) { await anyPreset.click(); await sleep(800); }
  const copyBtn = page.locator("button", { hasText: /^Copy/ }).first();
  if (await copyBtn.count()) {
    await copyBtn.click();
    await sleep(800);
    await shot(page, "17-context-after-copy");
  }

  // ── 13. MCP page ──
  await page.evaluate(() => (document.querySelector("a[href='/mcp']"))?.click());
  await sleep(1800);
  await shot(page, "18-mcp-page");

  // ── 14. Settings ──
  await page.evaluate(() => (document.querySelector("a[href='/settings']"))?.click());
  await sleep(1500);
  await shot(page, "19-settings");

  // ── 15. light theme home ──
  await page.emulateMedia({ colorScheme: "light" });
  await page.evaluate(() => (document.querySelector("a[href='/']"))?.click());
  await sleep(1200);
  await shot(page, "20-home-light");
  await page.emulateMedia({ colorScheme: "dark" });

  fs.writeFileSync(path.join(OUT, "notes.md"), notes.join("\n"), "utf8");
  console.log("\nDONE. Notes:\n" + notes.join("\n"));
  await browser.close();
}

main().catch((e) => { console.error("FATAL:", e); process.exitCode = 1; });
