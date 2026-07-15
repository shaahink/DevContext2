// Deep user-style QA drive — dogfood eShop, every clickable behavior, keyboard
// shortcuts, studio controls, export paths, both themes. Assumes server :5179 +
// ng :4200 running. Output: eval-results/2026-07-15/wrapup-drive/ui-deep-qa/
import { chromium } from "playwright";
import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, "..", "..", "..");
const OUT = path.join(ROOT, "eval-results", "2026-07-15", "wrapup-drive", "ui-deep-qa");
fs.mkdirSync(OUT, { recursive: true });
const APP = "http://localhost:4200";
const DOGFOOD = "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";

const notes = [];
const note = (s) => { notes.push(s); console.log("NOTE " + s); };
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: "chrome", headless: true });
const ctx = await browser.newContext({ viewport: { width: 1600, height: 950 }, colorScheme: "dark" });
const page = await ctx.newPage();
page.on("console", (m) => { if (m.type() === "error") note("console.error: " + m.text().slice(0, 180)); });
page.on("pageerror", (e) => note("pageerror: " + String(e).slice(0, 180)));
let shotN = 0;
const shot = async (name) => { try { await page.screenshot({ path: path.join(OUT, `${String(++shotN).padStart(2, "0")}-${name}.png`) }); console.log("shot", name); } catch (e) { note(`shot ${name} failed`); } };
const step = async (name, fn) => { try { await fn(); } catch (e) { note(`STEP FAIL [${name}]: ${e.message?.slice(0, 160)}`); } };
const bodyText = () => page.evaluate(() => document.body.innerText);

// ── boot + analyze ──
await page.goto(APP, { waitUntil: "domcontentloaded" });
await sleep(2500);
await step("analyze", async () => {
  const input = page.locator("input:visible").first();
  await input.fill(DOGFOOD);
  await input.press("Enter");
  for (let i = 0; i < 150; i++) { const t = await bodyText(); if (/entries/i.test(t) && !/Analyzing/i.test(t)) break; await sleep(1000); }
});
await sleep(2000);
await shot("home-dark");

// ── home interactions ──
await step("home-trace-checkout-link", async () => {
  const link = page.locator("a,button", { hasText: /trace checkout|checkout/i }).first();
  if (!(await link.count())) return note("home: no [Trace checkout] onboarding link");
  await link.click(); await sleep(2500); await shot("home-click-trace-checkout");
  note("trace-checkout landed on: " + page.url());
});
await step("home-needs-attention", async () => {
  await page.goto(APP + "/", { waitUntil: "domcontentloaded" }); await sleep(2000);
  const ins = page.locator("a,button", { hasText: /see all|insight/i }).first();
  if (!(await ins.count())) return note("home: no insights link");
  await ins.click(); await sleep(2000); await shot("home-insights-link");
  note("insights link landed on: " + page.url());
});

// ── keyboard map ──
await step("kbd-overlay", async () => {
  await page.goto(APP + "/", { waitUntil: "domcontentloaded" }); await sleep(1500);
  await page.keyboard.press("?"); await sleep(900); await shot("kbd-overlay");
  const t = await bodyText();
  note("kbd overlay visible: " + /shortcut|keyboard/i.test(t));
  await page.keyboard.press("Escape"); await sleep(400);
});
for (const [key, expect] of [["e", "/explore"], ["a", "/atlas"], ["i", "/insights"], ["m", "/mcp"], ["c", "/context"], ["s", "/settings"], ["h", "/"]]) {
  await step(`kbd-${key}`, async () => {
    await page.keyboard.press(key); await sleep(1200);
    const ok = new URL(page.url()).pathname === expect;
    note(`kbd '${key}' -> ${new URL(page.url()).pathname} (want ${expect}) ${ok ? "OK" : "FAIL"}`);
  });
}

// ── explore: lenses, view modes, inspector tabs ──
await page.goto(APP + "/explore", { waitUntil: "domcontentloaded" }); await sleep(2500);
for (const lens of ["Service", "Layer", "Feature", "Flow"]) {
  await step(`lens-${lens}`, async () => {
    const b = page.locator("button", { hasText: new RegExp(`^${lens}$`, "i") }).first();
    if (!(await b.count())) return note(`lens ${lens}: button not found`);
    const disabled = await b.isDisabled().catch(() => false);
    if (disabled) return note(`lens ${lens}: DISABLED`);
    await b.click(); await sleep(1800); await shot(`lens-${lens.toLowerCase()}`);
  });
}
for (const mode of ["Table", "Tree", "Graph"]) {
  await step(`view-${mode}`, async () => {
    const b = page.locator("button", { hasText: new RegExp(`^${mode}$`, "i") }).first();
    if (!(await b.count())) return note(`view ${mode}: button not found`);
    await b.click(); await sleep(2200); await shot(`view-${mode.toLowerCase()}`);
    if (mode === "Graph") {
      const nodes = await page.evaluate(() => document.querySelectorAll("app-graph-canvas canvas").length);
      note(`graph canvases: ${nodes}`);
    }
  });
}
await step("approx-only-toggle", async () => {
  const b = page.locator("button,label", { hasText: /approx only/i }).first();
  if (!(await b.count())) return note("approx-only: not found");
  await b.click(); await sleep(1200); await shot("approx-only-on"); await b.click(); await sleep(400);
});
await step("entry-select-inspector", async () => {
  const row = page.locator(".list-row").first();
  if (!(await row.count())) return note("explore: no .list-row");
  await row.click(); await sleep(2000); await shot("entry-selected");
  for (const sec of ["Details", "Code", "Insights", "Call Stack", "Trail"]) {
    const h = page.locator("app-inspector .section-h", { hasText: sec }).first();
    if (!(await h.count())) { note(`inspector section missing: ${sec}`); continue; }
    await h.click(); await sleep(1400); await shot(`inspector-${sec.toLowerCase().replace(" ", "-")}`);
    if (sec === "Code") {
      const len = await page.evaluate(() => (document.querySelector("app-inspector pre, app-inspector code")?.textContent ?? "").trim().length);
      note(`code pane len: ${len}`);
    }
  }
});
await step("table-lens-shortcut", async () => {
  await page.keyboard.press("Shift+E"); await sleep(1500); await shot("table-lens-shift-e");
  const csv = page.locator("button", { hasText: /csv/i }).first();
  if (await csv.count()) { note("csv button present"); } else note("csv button NOT found in table lens");
});

// ── omnibox ──
await step("omnibox", async () => {
  await page.keyboard.press("Control+k"); await sleep(900); await shot("omnibox-open");
  await page.keyboard.type("checkout"); await sleep(1200); await shot("omnibox-results");
  const t = await bodyText();
  note("omnibox has checkout results: " + /checkout/i.test(t));
  await page.keyboard.press("Enter"); await sleep(2200); await shot("omnibox-enter");
  note("omnibox enter landed: " + page.url());
});

// ── flow/trace view for checkout ──
await step("flow-trace-checkout", async () => {
  await page.goto(APP + "/explore?lens=flow", { waitUntil: "domcontentloaded" }); await sleep(2200);
  const search = page.locator("input[placeholder*='Filter' i]").first();
  if (await search.count()) { await search.fill("checkout"); await sleep(1200); }
  const row = page.locator(".list-row").first();
  if (await row.count()) { await row.click(); await sleep(2500); await shot("flow-checkout"); }
  else note("flow: no checkout row after filter");
});

// ── atlas ──
await step("atlas", async () => {
  await page.goto(APP + "/atlas", { waitUntil: "domcontentloaded" }); await sleep(2500);
  await shot("atlas-top");
  await page.mouse.wheel(0, 1200); await sleep(800); await shot("atlas-mid");
  await page.mouse.wheel(0, 1600); await sleep(800); await shot("atlas-bottom");
  const exp = page.locator("button", { hasText: /export one-pager/i }).first();
  if (await exp.count()) {
    await exp.click(); await sleep(1200); await shot("atlas-export-clicked");
    const t = await bodyText(); note("atlas export feedback (Copied?): " + /copied/i.test(t));
  } else note("atlas: export button not found");
});

// ── insights page + deep link ──
await step("insights-deeplink", async () => {
  await page.goto(APP + "/insights", { waitUntil: "domcontentloaded" }); await sleep(2200);
  await shot("insights-page");
  const row = page.locator("a,button,[class*='insight']").filter({ hasText: /WARN|NOTE|Risk|Wiring|Topology/i }).first();
  if (!(await row.count())) return note("insights: no clickable insight row");
  await row.click(); await sleep(2000); await shot("insight-clicked");
  note("insight click landed: " + page.url());
});

// ── context studio deep ──
await step("studio", async () => {
  await page.goto(APP + "/context", { waitUntil: "domcontentloaded" }); await sleep(2200);
  await shot("studio-initial");
  const preset = page.locator("button", { hasText: /changing this endpoint/i }).first();
  if (await preset.count()) {
    await preset.click(); await sleep(800);
    const entry = page.locator("app-scope-picker button", { hasText: /GET|POST|PUT|DELETE|\// }).first();
    if (await entry.count()) { await entry.click(); await sleep(3000); }
    await shot("studio-preset-cards");
    const t = await bodyText();
    note("studio totals text sample: " + (t.match(/[~\d,.]+\s*tok[a-z]*[^\n]{0,40}/gi) || []).slice(0, 4).join(" | "));
  } else note("studio: preset button missing");
  // budget slider to minimum → omitted visibility (audit R1)
  const slider = page.locator("input[type='range']").first();
  if (await slider.count()) {
    await slider.evaluate((el) => { el.value = el.min; el.dispatchEvent(new Event("input", { bubbles: true })); el.dispatchEvent(new Event("change", { bubbles: true })); });
    await sleep(2500); await shot("studio-budget-min");
    const t = await bodyText();
    note("omitted visible after min budget: " + /omitt/i.test(t));
  } else note("studio: budget slider not found");
  for (const intent of ["explain", "review", "trace"]) {
    const b = page.locator("button", { hasText: new RegExp(`^${intent}$`, "i") }).first();
    if (await b.count()) { await b.click(); await sleep(1200); } else note(`studio: intent ${intent} button missing`);
  }
  await shot("studio-intent-review");
  const plain = page.locator("button", { hasText: /^plain$/i }).first();
  if (await plain.count()) { await plain.click(); await sleep(800); } else note("studio: plain format button missing");
  const eye = page.locator("app-composition-view button[title*='bod' i], app-composition-view button:has(svg)").first();
  if (await eye.count()) { await eye.click(); await sleep(800); await shot("studio-body-toggle"); }
  const copy = page.locator("button", { hasText: /^copy$/i }).first();
  if (await copy.count()) {
    await copy.click(); await sleep(1200); await shot("studio-copy");
    const t = await bodyText(); note("studio copy feedback: " + /copied/i.test(t));
  } else note("studio: copy button missing");
  const save = page.locator("button", { hasText: /^save$/i }).first();
  if (await save.count()) {
    const dl = page.waitForEvent("download", { timeout: 5000 }).catch(() => null);
    await save.click();
    const d = await dl;
    note("studio save download: " + (d ? d.suggestedFilename() : "NO DOWNLOAD EVENT"));
  } else note("studio: save button missing");
});

// ── mcp page ──
await step("mcp", async () => {
  await page.goto(APP + "/mcp", { waitUntil: "domcontentloaded" }); await sleep(2500);
  await shot("mcp-page");
  const t = await bodyText();
  note("mcp status text: " + (t.match(/running|stopped|session[^\n]{0,60}/i) || ["?"])[0]);
  const copyCfg = page.locator("button", { hasText: /copy/i }).first();
  if (await copyCfg.count()) { await copyCfg.click(); await sleep(800); note("mcp copy config clicked"); }
});

// ── settings + light theme sweep ──
await step("theme-light-sweep", async () => {
  await page.goto(APP + "/settings", { waitUntil: "domcontentloaded" }); await sleep(2000);
  await shot("settings-dark");
  const light = page.locator("button,label", { hasText: /light|paper/i }).first();
  if (!(await light.count())) return note("settings: no light theme control found");
  await light.click(); await sleep(1500); await shot("settings-light");
  for (const [r, n] of [["/", "home-light"], ["/explore", "explore-light"], ["/atlas", "atlas-light"], ["/context", "studio-light"]]) {
    await page.goto(APP + r, { waitUntil: "domcontentloaded" }); await sleep(2200); await shot(n);
  }
  await page.goto(APP + "/settings", { waitUntil: "domcontentloaded" }); await sleep(1500);
  const dark = page.locator("button,label", { hasText: /dark|graphite/i }).first();
  if (await dark.count()) { await dark.click(); await sleep(800); }
});

fs.writeFileSync(path.join(OUT, "notes.md"), "# Deep QA notes — eShop dogfood (2026-07-15)\n\n" + notes.map((n) => "- " + n).join("\n"), "utf8");
await browser.close();
console.log("DONE — " + OUT + " (" + shotN + " shots)");
