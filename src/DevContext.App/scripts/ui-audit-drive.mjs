// UI drive gate (Loom L0.3) — promoted from the audit screenshot survey.
// Usage: node scripts/ui-audit-drive.mjs [--gate]
// Requires: DevContext.Server on :5179 + `ng serve` on :4200 already running
//   (pnpm server + pnpm dev:web, or reuse a live server).
//
// Drives the real UI headless and asserts four user-visible truths the audit (§5)
// found broken. Each is EXPECTED RED until L6 fixes it — the gate records the red
// with its owning stage and does NOT fail the battery by default (no green-washing,
// no false green). Pass --gate (armed in L6) to make the process exit non-zero when a
// non-expected-red assertion regresses.
//
// Assertions:
//   A. tab strip height >= 30px               (audit U1, owner L6.1)
//   B. titlebar "New" preserves other tabs     (audit U2, owner L6.1)
//   C. code pane non-empty on entry selection  (audit U3, owner L6.2)
//   D. context studio preset seeds >= 1 card    (audit U6, owner L6.4)
//   E. tiny-budget pack renders omitted[] list  (audit R1, owner T5.1)
//   F. failed pack RPC shows error + retry works (audit R4, owner T5.1)
//
// Output: eval-results/<date>/ui/  (screenshots + ui-gate.json + ui-gate.md)

import { chromium } from "playwright";
import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, "..", "..", "..");
const DOGFOOD = "C:/Users/shahi/source/repos/run-aspnetcore-microservices/src";
const SECOND_REPO = path.join(ROOT, "eval-repos", "TodoApi").replaceAll("\\", "/");
const APP_URL = "http://localhost:4200";
const DATE = new Date().toISOString().slice(0, 10);
const OUT = path.join(ROOT, "eval-results", DATE, "ui");
const GATE = process.argv.includes("--gate");

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const notes = [];
const note = (s) => { notes.push(s); console.log("NOTE: " + s); };

// Boot-liveness precondition (L0.5): fail distinctly if the app never renders.
// Without this, `page.goto` to a dead server silently fails all assertions as
// `expectedRedUntil` and `--gate` passes green on a dead environment.
async function checkBootLiveness(page) {
  for (let i = 0; i < 20; i++) {
    try {
      const hasApp = await page.evaluate(() => {
        const root = document.querySelector("app-root");
        const titlebar = document.querySelector("app-titlebar");
        const tabStrip = document.querySelector("app-tab-strip");
        const input = document.querySelector("input");
        return !!(root || titlebar || tabStrip || input) ? "found" : null;
      });
      if (hasApp) { console.log("LIVENESS: app shell rendered ok"); return true; }
    } catch (_) { /* page may not be ready */ }
    await sleep(500);
  }
  return false;
}

// Each assertion: { id, desc, owner, expectedRedUntil, run() -> {pass, detail} }.
const assertions = [];
const results = [];

async function shot(page, name) {
  try { await page.screenshot({ path: path.join(OUT, name + ".png") }); console.log("  shot: " + name); }
  catch (e) { note(`shot ${name} failed: ${e.message}`); }
}

async function waitAnalyzed(page, maxSec) {
  for (let i = 0; i < maxSec; i++) {
    const txt = await page.evaluate(() => document.body.innerText);
    if (/Nodes|entries|Entries/.test(txt) && !/Analyzing/.test(txt)) return true;
    await sleep(1000);
  }
  return false;
}

async function main() {
  fs.mkdirSync(OUT, { recursive: true });
  const browser = await chromium.launch({ channel: "chrome", headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1600, height: 950 }, colorScheme: "dark" });
  const page = await ctx.newPage();
  page.on("console", (m) => { if (m.type() === "error") note("console.error: " + m.text().slice(0, 200)); });

  // Shared drive state captured for the assertions.
  const state = { stripH: null, tabsBeforeNew: null, tabsAfterNew: null, codeContent: null, presetCards: null,
    omittedVisible: null, omittedText: null, errorShown: null, errorClearedAfterRetry: null };

  try {
    // ── boot + analyze dogfood ──
    let gotoOk = false;
    try { await page.goto(APP_URL, { waitUntil: "networkidle" }); gotoOk = true; }
    catch (e) { note("page.goto failed: " + e.message.slice(0, 120)); }

    const alive = await checkBootLiveness(page);
    if (!alive) {
      note("BOOT-LIVENESS FAILED: app shell did not render within 10s. Is server+ng running?");
      await browser.close();
      console.log("\n========================================");
      console.log("UI drive gate — BOOT-LIVENESS FAILED");
      console.log("========================================");
      console.log("The app shell did not render. The server or Angular dev server may be down.");
      console.log("Start: pnpm server (terminal 1) + pnpm dev:web (terminal 2), then retry.");
      console.log("");
      if (GATE) {
        console.error("GATE FAILED (boot-liveness): app shell did not render");
        process.exit(1);
      }
      process.exit(0);
    }
    await sleep(2500);
    await shot(page, "01-boot");

    const input = page.locator("input:visible").first();
    if (await input.count()) { await input.fill(DOGFOOD); await input.press("Enter"); note("analyze: dogfood submitted"); }
    else note("analyze: NO visible input on boot");
    const analyzed = await waitAnalyzed(page, 150);
    note("dogfood analyzed: " + analyzed);
    await sleep(1500);
    await shot(page, "02-home");

    // ── A. tab strip height ──
    const tabInfo = await page.evaluate(() => {
      const strip = document.querySelector("app-tab-strip");
      if (!strip) return { found: false };
      const r = strip.getBoundingClientRect();
      const tabs = [...strip.querySelectorAll("[role='tab']")].map((t) => {
        const b = t.getBoundingClientRect(); const cs = getComputedStyle(t);
        return { h: Math.round(b.height), fontSize: cs.fontSize, label: t.textContent?.trim().slice(0, 30) };
      });
      return { found: true, stripH: Math.round(r.height), tabs };
    });
    note("tab-strip: " + JSON.stringify(tabInfo));
    state.stripH = tabInfo.found ? tabInfo.stripH : null;
    const stripEl = page.locator("app-tab-strip");
    if (await stripEl.count()) await stripEl.screenshot({ path: path.join(OUT, "03-tab-strip.png") }).catch(() => {});

    // ── B. New-button preservation: open a 2nd tab, then click titlebar New ──
    const plusBtn = page.locator("app-tab-strip button", { hasText: "+" });
    if (await plusBtn.count()) {
      await plusBtn.first().click(); await sleep(1200);
      const input2 = page.locator("input:visible").first();
      if (await input2.count()) { await input2.fill(SECOND_REPO); await input2.press("Enter"); await waitAnalyzed(page, 90); }
      await sleep(800);
    } else note("plus button NOT found");

    state.tabsBeforeNew = await page.evaluate(() =>
      [...document.querySelectorAll("app-tab-strip [role='tab']")].map((t) => t.textContent?.trim()));
    note("tabs before New: " + JSON.stringify(state.tabsBeforeNew));
    await shot(page, "04-two-tabs");

    const newBtn = page.locator("header button", { hasText: "New" });
    if (await newBtn.count()) {
      await newBtn.first().click(); await sleep(1500);
      state.tabsAfterNew = await page.evaluate(() =>
        [...document.querySelectorAll("app-tab-strip [role='tab']")].map((t) => t.textContent?.trim()));
      note("tabs after New: " + JSON.stringify(state.tabsAfterNew));
      await shot(page, "05-after-new");
    } else note("titlebar New button not visible");

    // Recover a live session for the Explore/Code assertion.
    const firstTab = page.locator("app-tab-strip [role='tab']").first();
    if (await firstTab.count()) { await firstTab.click(); await sleep(1200); }
    {
      const txt = await page.evaluate(() => document.body.innerText);
      if (!/Nodes|entries|Entries/.test(txt)) {
        const in3 = page.locator("input:visible").first();
        if (await in3.count()) { await in3.fill(DOGFOOD); await in3.press("Enter"); await waitAnalyzed(page, 120); }
      }
    }

    // ── C. code pane on entry selection ──
    await page.evaluate(() => document.querySelector("a[href='/explore']")?.click());
    await sleep(2500);
    await shot(page, "06-explore");
    const row = page.locator(".list-row").first();
    if (await row.count()) {
      await row.click(); await sleep(2000);
      await shot(page, "07-entry-selected");
      const codeBtn = page.locator("app-inspector .section-h", { hasText: "Code" });
      if (await codeBtn.count()) {
        await codeBtn.first().click(); await sleep(1800);
        state.codeContent = await page.evaluate(() => {
          const pre = document.querySelector("app-inspector pre, app-inspector code");
          return pre ? (pre.textContent ?? "").trim() : null;
        });
        note("code pane content len: " + (state.codeContent?.length ?? "null"));
        await shot(page, "08-code-pane");
      } else note("inspector Code section NOT found");
    } else note("explore: no .list-row found");

    // ── D. context studio preset seeds cards ──
    await page.evaluate(() => document.querySelector("a[href='/context']")?.click());
    await sleep(2200);
    await shot(page, "09-context-initial");
    const presetBtn = page.locator("button", { hasText: /changing this endpoint/i }).first();
    if (await presetBtn.count()) {
      await presetBtn.click(); await sleep(800);
      // preset opens an entry picker; select the first entry to seed cards
      const presetEntry = page.locator("app-scope-picker button", { hasText: /\/|GET|POST|PUT|DELETE/ }).first();
      if (await presetEntry.count()) { await presetEntry.click(); await sleep(2500); }
      state.presetCards = await page.evaluate(() =>
        document.querySelectorAll("app-composition-view [draggable='true']").length);
      note("context preset cards: " + state.presetCards);
      await shot(page, "10-context-preset");
    } else note("context preset button NOT found");

    // ── E. tiny-budget pack renders omitted[] (T5.1, audit R1) ──
    // Slide the budget to the 1k floor, then seed a fresh preset batch — the pack
    // request runs at 1k, the server cuts sections, and the panel must SAY so.
    const setSlider = async (value) => page.evaluate((v) => {
      const el = document.querySelector("#budget-slider");
      if (!el) return false;
      const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value").set;
      setter.call(el, String(v));
      el.dispatchEvent(new Event("input", { bubbles: true }));
      return true;
    }, value);
    const seedPreset = async () => {
      const btn = page.locator("button", { hasText: /changing this endpoint/i }).first();
      if (!(await btn.count())) return false;
      await btn.click(); await sleep(600);
      const entry = page.locator("app-scope-picker button", { hasText: /\/|GET|POST|PUT|DELETE/ }).first();
      if (!(await entry.count())) return false;
      await entry.click(); await sleep(2500);
      return true;
    };
    if (await setSlider(1000)) {
      note("budget slider set to 1000");
      if (await seedPreset()) {
        state.omittedVisible = await page.evaluate(() =>
          !!document.querySelector("[data-testid='omitted-list']"));
        state.omittedText = await page.evaluate(() =>
          document.querySelector("[data-testid='omitted-list']")?.textContent?.trim().slice(0, 160) ?? null);
        note("omitted list visible: " + state.omittedVisible + " text: " + state.omittedText);
        await shot(page, "11-context-omitted");
      } else note("E: preset re-seed failed");
    } else note("E: budget slider NOT found");

    // ── F. failed pack RPC shows per-card error + retry recovers (T5.1, audit R4) ──
    await page.route("**/GetContextPack", (route) => route.abort("connectionrefused"));
    if (await seedPreset()) {
      state.errorShown = await page.evaluate(() =>
        document.querySelectorAll("[data-testid='card-error']").length > 0);
      note("card error shown while server unreachable: " + state.errorShown);
      await shot(page, "12-context-error");
      await page.unroute("**/GetContextPack");
      const retry = page.locator("[data-testid='card-retry']").first();
      if (await retry.count()) {
        await retry.click(); await sleep(2500);
        state.errorClearedAfterRetry = await page.evaluate(() =>
          document.querySelectorAll("[data-testid='card-error']").length === 0);
        note("errors cleared after retry: " + state.errorClearedAfterRetry);
        await shot(page, "13-context-retry");
      } else note("F: retry button NOT found");
    } else { note("F: preset re-seed failed"); await page.unroute("**/GetContextPack"); }

  } catch (e) {
    note("DRIVE ERROR: " + e.message);
  } finally {
    // ── Evaluate assertions from captured state ──
    assertions.push(
      { id: "A-tabstrip-height", desc: "tab strip height >= 30px", audit: "U1", owner: "L6.1", expectedRedUntil: "L6",
        run: () => ({ pass: (state.stripH ?? 0) >= 30, detail: `stripH=${state.stripH}px (want >=30)` }) },
      { id: "B-new-preserves-tabs", desc: "titlebar New preserves other tabs", audit: "U2", owner: "L6.1", expectedRedUntil: "L6",
        run: () => {
          const before = state.tabsBeforeNew ?? [], after = state.tabsAfterNew ?? [];
          if (!state.tabsAfterNew) return { pass: false, detail: "New button not exercised (no session/button)" };
          const lost = before.filter((t) => !after.includes(t));
          const added = after.length > before.length;
          return { pass: lost.length === 0 && added, detail: `before=${JSON.stringify(before)} after=${JSON.stringify(after)} lost=${JSON.stringify(lost)} added=${added}` };
        } },
      { id: "C-code-pane-nonempty", desc: "code pane non-empty on entry selection", audit: "U3", owner: "L6.2", expectedRedUntil: "L6",
        run: () => ({ pass: (state.codeContent?.length ?? 0) > 0, detail: `code length=${state.codeContent?.length ?? "null"}` }) },
      { id: "D-context-preset-cards", desc: "context studio preset seeds >= 1 card", audit: "U6", owner: "L6.4", expectedRedUntil: "L6",
        run: () => ({ pass: (state.presetCards ?? 0) >= 1, detail: `cards=${state.presetCards ?? "null"}` }) },
      { id: "E-omitted-list-rendered", desc: "tiny-budget pack renders the omitted[] list", audit: "R1", owner: "T5.1",
        run: () => ({ pass: state.omittedVisible === true, detail: `visible=${state.omittedVisible} text=${String(state.omittedText).slice(0, 80)}` }) },
      { id: "F-pack-error-retry", desc: "failed pack RPC shows card error; retry recovers", audit: "R4", owner: "T5.1",
        run: () => ({ pass: state.errorShown === true && state.errorClearedAfterRetry === true, detail: `errorShown=${state.errorShown} clearedAfterRetry=${state.errorClearedAfterRetry}` }) },
    );
    for (const a of assertions) {
      let r; try { r = a.run(); } catch (e) { r = { pass: false, detail: "assert error: " + e.message }; }
      results.push({ ...a, pass: r.pass, detail: r.detail, run: undefined });
    }
    fs.writeFileSync(path.join(OUT, "notes.md"), notes.join("\n"), "utf8");
    await browser.close();
  }

  // ── Report + artifact ──
  const reds = results.filter((r) => !r.pass);
  const unexpectedFails = reds.filter((r) => !r.expectedRedUntil);

  console.log("\n========================================");
  console.log("UI drive gate (Loom L0.3)");
  console.log("========================================");
  console.log("| Assertion | Pass | Owner | Detail |");
  console.log("|-----------|------|-------|--------|");
  for (const r of results) {
    const tag = r.pass ? "PASS" : (r.expectedRedUntil ? `RED(${r.owner})` : "REGRESS");
    console.log(`| ${r.id.padEnd(24)} | ${tag.padEnd(10)} | ${r.owner} | ${String(r.detail).slice(0, 70)} |`);
  }
  console.log(`\n${results.filter((r) => r.pass).length}/${results.length} pass · ${reds.length} red (expected until their owner stage).`);

  const md = [];
  md.push("# UI Drive Gate — Baseline (Loom L0.3)");
  md.push("");
  md.push(`**Date:** ${DATE}  `);
  md.push(`**App:** ${APP_URL} (server :5179 + ng :4200)  `);
  md.push(`**Screenshots:** \`eval-results/${DATE}/ui/*.png\` (10) + \`notes.md\`  `);
  md.push("");
  md.push("Headless drive of the real UI. Each assertion targets a confirmed audit §5 defect and is");
  md.push("EXPECTED RED until its owner stage. The gate records reds with owners; it does not");
  md.push("green-wash. Run with `--gate` (armed in L6) to enforce.");
  md.push("");
  md.push("## Assertions");
  md.push("");
  md.push("| # | Assertion | Result | Audit | Owner | Detail |");
  md.push("|---|-----------|--------|-------|-------|--------|");
  for (const r of results) {
    const res = r.pass ? "PASS" : (r.expectedRedUntil ? `RED (until ${r.expectedRedUntil})` : "REGRESSION");
    md.push(`| ${r.id} | ${r.desc} | ${res} | ${r.audit} | ${r.owner} | ${String(r.detail).replace(/\|/g, "/").slice(0, 120)} |`);
  }
  md.push("");
  md.push("## Red items enumerated (owner stage)");
  for (const r of reds) md.push(`- **${r.id}** — ${r.desc} → **${r.owner}** (audit ${r.audit})`);
  md.push("");
  md.push(`**Gate status:** ${unexpectedFails.length === 0 ? "OK (all reds are expected-red with owners; no regression)" : "REGRESSION — " + unexpectedFails.map((r) => r.id).join(", ")}`);
  md.push("");
  fs.writeFileSync(path.join(OUT, "ui-gate.md"), md.join("\n"), "utf8");
  fs.writeFileSync(path.join(OUT, "ui-gate.json"), JSON.stringify({ date: DATE, results, reds: reds.map((r) => r.id), unexpectedFails: unexpectedFails.map((r) => r.id) }, null, 2), "utf8");
  console.log(`\nArtifact: eval-results/${DATE}/ui/ui-gate.md (+ .json, 10 screenshots)`);

  if (GATE && unexpectedFails.length > 0) {
    console.error("GATE FAILED (regression): " + unexpectedFails.map((r) => r.id).join(", "));
    process.exitCode = 1;
  }
}

main().catch((e) => { console.error("FATAL:", e); process.exitCode = 1; });
