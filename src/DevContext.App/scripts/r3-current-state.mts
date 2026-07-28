/**
 * S6 / R3 — current-state capture on the POST-Batch-A..E engine.
 *
 * The 2026-07-27 ui-feature-audit screenshots were taken against the pre-Batch-A engine, when the
 * graph was starved (E1 no sync transports, E2 broken handler join, E3 dup-name noise, E6 silent
 * single-sln scope). R3 §3 says canvas mock-ups must be made AFTER those land. This driver re-takes
 * the same frames so "current state" in the decision session is today's truth.
 *
 * Usage: node --experimental-strip-types r3-current-state.mts <repoPath> <name> [focus]
 * Writes: eval-results/2026-07-28/r3-current-state/<name>/*.png + *.txt
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync, appendFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = process.argv[2] ?? 'C:\\code\\DevContext2\\eval-repos\\eShop';
const NAME = process.argv[3] ?? 'eshop';
const FOCUS = process.argv[4] ?? 'POST /api/orders/';
const OUT = join('C:/code/DevContext2/eval-results/2026-07-28/r3-current-state', NAME);
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1.5 });
  const page = await context.newPage();
  const consoleLog = join(OUT, 'console.log');
  writeFileSync(consoleLog, '', 'utf-8');
  page.on('console', (m) => {
    if (m.type() === 'error' || m.type() === 'warning')
      appendFileSync(consoleLog, `[${m.type()}] ${page.url()} :: ${m.text()}\n`, 'utf-8');
  });
  page.on('pageerror', (e) => appendFileSync(consoleLog, `[pageerror] ${page.url()} :: ${e.message}\n`, 'utf-8'));

  async function snap(name: string, fullPage = true) {
    await page.screenshot({ path: join(OUT, `${name}.png`), fullPage });
    writeFileSync(join(OUT, `${name}.txt`), await page.evaluate(() => document.body.innerText), 'utf-8');
    console.log(`  snap ${name}`);
  }

  /** Dump the rendered graph/canvas structure as text so the grid can be read without the PNG. */
  async function canvasDump(name: string) {
    const dump = await page.evaluate(() => {
      const svgs = Array.from(document.querySelectorAll('svg'));
      const out: string[] = [];
      for (const svg of svgs) {
        const nodes = Array.from(svg.querySelectorAll('text')).map((t) => t.textContent?.trim() ?? '');
        const edges = Array.from(svg.querySelectorAll('path, line')).map((p) => {
          const cls = p.getAttribute('class') ?? '';
          const stroke = p.getAttribute('stroke') ?? '';
          const dash = p.getAttribute('stroke-dasharray') ?? '';
          const marker = p.getAttribute('marker-end') ?? '';
          return `path cls="${cls}" stroke="${stroke}" dash="${dash}" marker="${marker}"`;
        });
        if (nodes.length === 0 && edges.length === 0) continue;
        out.push(`--- svg (${svg.getAttribute('class') ?? ''}) ${nodes.length} texts / ${edges.length} paths`);
        out.push('LABELS: ' + nodes.filter(Boolean).join(' | '));
        const styles = new Map<string, number>();
        for (const e of edges) styles.set(e, (styles.get(e) ?? 0) + 1);
        out.push('EDGE STYLES:');
        for (const [k, v] of [...styles.entries()].sort((a, b) => b[1] - a[1]).slice(0, 25))
          out.push(`  ${v}x  ${k}`);
      }
      return out.join('\n');
    });
    writeFileSync(join(OUT, `${name}.canvas.txt`), dump, 'utf-8');
  }

  // ---- bootstrap / analyze (idempotent server-side; re-attach is fast)
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
  await snap('01-home');
  await canvasDump('01-home');

  // home canvas: the "all projects" lanes toggle (C2 in FINDINGS)
  const allProjects = page.locator('button', { hasText: 'All projects' }).first();
  if (await allProjects.count()) {
    await allProjects.click();
    await sleep(2500);
    await snap('02-home-allprojects');
    await canvasDump('02-home-allprojects');
  }

  // ---- explore (workspace) default — W1 empty center
  await page.goto(`${APP}/explore`, { waitUntil: 'domcontentloaded' });
  await sleep(4000);
  await snap('10-explore-default');

  // focus a flow (trace mode) — E2/E5
  await page.goto(`${APP}/explore?focus=${encodeURIComponent(FOCUS)}`, { waitUntil: 'domcontentloaded' });
  await sleep(5000);
  await snap('11-explore-focus');
  await canvasDump('11-explore-focus');

  // graph mode + depth (W2)
  const graphBtn = page.locator('button', { hasText: /^Graph$/ }).first();
  if (await graphBtn.count()) {
    await graphBtn.click();
    await sleep(3500);
    await snap('12-explore-graph');
    await canvasDump('12-explore-graph');
  }

  // ---- atlas (the topology hero — C1/C4/E8)
  await page.goto(`${APP}/atlas`, { waitUntil: 'domcontentloaded' });
  await sleep(5000);
  await snap('20-atlas');
  await canvasDump('20-atlas');

  // ---- insights (I2/I3)
  await page.goto(`${APP}/insights`, { waitUntil: 'domcontentloaded' });
  await sleep(3000);
  await snap('30-insights');

  // ---- context studio (S3/S4)
  await page.goto(`${APP}/context`, { waitUntil: 'domcontentloaded' });
  await sleep(3500);
  await snap('40-context');

  console.log(`done · ${OUT}`);
  await browser.close();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
