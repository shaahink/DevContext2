/**
 * Lens-audit UI drive — screenshots every page against two unseen repos:
 * dotnet-podcasts (multi-surface app) and refit (library). Judged offline.
 * Run: node --experimental-strip-types scripts/lens-audit-drive.mts
 */
import { chromium } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const SCRATCH = 'C:\\Users\\shahi\\AppData\\Local\\Temp\\claude\\C--code-DevContext2\\21fab51e-9c82-4278-8271-a302683a111a\\scratchpad\\repos';
const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-17\\lens-audit\\ui';
const ANALYZE_TIMEOUT = 300_000;

mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const context = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
const page = await context.newPage();
page.on('pageerror', (e) => console.log('PAGEERROR:', e.message.slice(0, 200)));

async function snap(name: string) {
  await page.screenshot({ path: join(OUT, `${name}.png`), fullPage: true });
  console.log('snap', name);
}

async function analyzeRepo(repo: string, tag: string) {
  await page.goto(APP, { waitUntil: 'networkidle' });
  await sleep(2000);
  const input = page.locator('app-start-hero input').first();
  if ((await input.count()) === 0) {
    console.log('no start-hero input — maybe a session is already loaded; using nav');
    return;
  }
  await input.fill(repo);
  await sleep(300);
  await page.locator("app-start-hero app-button[variant='primary']").first().click();
  await page.waitForSelector('app-identity-strip', { timeout: ANALYZE_TIMEOUT });
  await sleep(3000);
  await snap(`${tag}-01-home-after-analyze`);
}

async function tour(tag: string) {
  const pages: Array<[string, string]> = [
    ['/explore', 'explore'],
    ['/atlas', 'atlas'],
    ['/insights', 'insights'],
    ['/mcp', 'mcp'],
    ['/context', 'context'],
    ['/settings', 'settings'],
  ];
  let i = 2;
  for (const [route, name] of pages) {
    try {
      await page.goto(APP + route, { waitUntil: 'domcontentloaded' });
      await sleep(5000); // let canvases render (mcp page never reaches networkidle — live feed)
      await snap(`${tag}-${String(i).padStart(2, '0')}-${name}`);
    } catch (e: any) {
      console.log(`SKIP ${name}: ${e.message.slice(0, 120)}`);
    }
    i++;
  }
}

// ── repo 1: dotnet-podcasts ──
await analyzeRepo(join(SCRATCH, 'dotnet-podcasts'), 'podcasts');
await tour('podcasts');

// ── repo 2: refit (library) ──
await analyzeRepo(join(SCRATCH, 'refit'), 'refit');
await tour('refit');

await browser.close();
console.log('UI-DRIVE-DONE');
