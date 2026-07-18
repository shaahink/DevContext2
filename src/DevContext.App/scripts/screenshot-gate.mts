/**
 * D4.0 screenshot gate — repeatable loading/home/Explore/Atlas captures for the DoD four
 * (podcasts, refit, eShop, bitwarden). Each repo runs in a FRESH browser context (clean
 * localStorage: no tabs/recents variance), so runs are comparable shot-for-shot against
 * the committed baseline (eval-results/<date>/prism-d4/gate-baseline/).
 *
 * Usage:
 *   ./scripts/start-dev-bg.ps1     # server + web must already be up (idempotent)
 *   node --experimental-strip-types scripts/screenshot-gate.mts [--out <dir>] [--repos podcasts,refit]
 *
 * Output: <out>/<tag>-1-loading.png … <tag>-4-atlas.png + manifest.json.
 * Prints SCREENSHOT-GATE: PASS|FAIL — FAIL on analyze timeout, missing capture, or an
 * uncaught page error not in ALLOWED_ERRORS. Warm cache HITs are legit: the loading shot
 * then shows the brief warm loading state; manifest.analyzeMs records which world you got.
 */
import { chromium, type BrowserContext, type Page } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const SERVER = 'http://127.0.0.1:5179';
const REPO_ROOT = 'C:\\code\\DevContext2\\eval-repos';
const ANALYZE_TIMEOUT = 360_000; // cold bitwarden ~100s CLI; server adds margin
const CANVAS_SETTLE = 5_000; // explore/atlas canvases render after networkidle

const ALL_REPOS: Record<string, string> = {
  podcasts: join(REPO_ROOT, 'dotnet-podcasts'),
  refit: join(REPO_ROOT, 'refit'),
  eshop: join(REPO_ROOT, 'eShop'),
  bitwarden: join(REPO_ROOT, 'bitwarden-server'),
};

/** Known-benign page errors (empty = every uncaught error fails the gate). */
const ALLOWED_ERRORS: RegExp[] = [];

// ── args ──
function argValue(flag: string): string | undefined {
  const i = process.argv.indexOf(flag);
  return i >= 0 ? process.argv[i + 1] : undefined;
}
const today = new Date().toISOString().slice(0, 10);
const OUT = argValue('--out') ?? `C:\\code\\DevContext2\\eval-results\\${today}\\prism-d4\\gate-run`;
const tags = (argValue('--repos') ?? Object.keys(ALL_REPOS).join(',')).split(',').map((s) => s.trim());
for (const t of tags) if (!ALL_REPOS[t]) { console.error(`unknown repo tag: ${t} (know: ${Object.keys(ALL_REPOS).join(', ')})`); process.exit(1); }

mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

interface RepoResult {
  tag: string;
  repo: string;
  analyzeMs: number | null;
  loading: 'captured' | 'missed';
  shots: string[];
  pageErrors: string[];
  consoleErrors: string[];
  failure: string | null;
}

async function preflight(): Promise<void> {
  for (const [name, url] of [['server', `${SERVER}/health`], ['web', APP]] as const) {
    try {
      const r = await fetch(url);
      if (!r.ok) throw new Error(`${r.status}`);
    } catch {
      console.error(`FATAL: ${name} not answering at ${url} — run ./scripts/start-dev-bg.ps1 first`);
      process.exit(1);
    }
  }
}

async function snap(page: Page, file: string, result: RepoResult): Promise<void> {
  await page.screenshot({ path: join(OUT, file), fullPage: true });
  result.shots.push(file);
  console.log('  snap', file);
}

async function driveRepo(context: BrowserContext, tag: string, repo: string): Promise<RepoResult> {
  const result: RepoResult = { tag, repo, analyzeMs: null, loading: 'missed', shots: [], pageErrors: [], consoleErrors: [], failure: null };
  const page = await context.newPage();
  page.on('pageerror', (e) => result.pageErrors.push(e.message.slice(0, 300)));
  page.on('console', (m) => { if (m.type() === 'error') result.consoleErrors.push(m.text().slice(0, 300)); });

  console.log(`\n── ${tag} (${repo}) ──`);
  await page.goto(APP, { waitUntil: 'domcontentloaded' });
  const input = page.locator('app-start-hero input').first();
  await input.waitFor({ timeout: 15_000 });
  await input.fill(repo);
  await sleep(300);

  const t0 = Date.now();
  await page.locator("app-start-hero app-button[variant='primary']").first().click();

  // Loading state: run-console appears while session.busy(). On a warm cache HIT it may
  // only exist for ~2s — poll fast; if analyze finishes before we ever see it, record
  // 'missed' honestly (that is a finding about loading UX, not a harness failure).
  try {
    await page.locator('app-run-console').first().waitFor({ state: 'visible', timeout: 20_000 });
    await sleep(700);
    await snap(page, `${tag}-1-loading.png`, result);
    result.loading = 'captured';
  } catch {
    console.log('  loading state never became visible (fast HIT or instant fail)');
  }

  try {
    await page.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: ANALYZE_TIMEOUT });
  } catch {
    result.failure = `analyze did not complete within ${ANALYZE_TIMEOUT / 1000}s`;
    await snap(page, `${tag}-X-analyze-timeout.png`, result);
    return result;
  }
  result.analyzeMs = Date.now() - t0;
  console.log(`  analyze done in ${(result.analyzeMs / 1000).toFixed(1)}s`);
  await sleep(2_500);
  await snap(page, `${tag}-2-home.png`, result);

  for (const [route, name, ord] of [['/explore', 'explore', 3], ['/atlas', 'atlas', 4]] as const) {
    await page.goto(APP + route, { waitUntil: 'domcontentloaded' });
    await sleep(CANVAS_SETTLE);
    await snap(page, `${tag}-${ord}-${name}.png`, result);
  }
  return result;
}

await preflight();
const browser = await chromium.launch({ channel: 'chrome', headless: true });
const results: RepoResult[] = [];

for (const tag of tags) {
  const context = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
  try {
    results.push(await driveRepo(context, tag, ALL_REPOS[tag]));
  } catch (e) {
    results.push({ tag, repo: ALL_REPOS[tag], analyzeMs: null, loading: 'missed', shots: [], pageErrors: [], consoleErrors: [], failure: (e as Error).message.slice(0, 300) });
  }
  await context.close();
}
await browser.close();

// ── verdict ──
const EXPECTED_SHOTS = 4;
let pass = true;
for (const r of results) {
  const realErrors = r.pageErrors.filter((e) => !ALLOWED_ERRORS.some((rx) => rx.test(e)));
  const missingShots = r.failure ? EXPECTED_SHOTS : EXPECTED_SHOTS - r.shots.length;
  if (r.failure || missingShots > 0 || realErrors.length > 0) pass = false;
  console.log(`\n${r.tag}: shots=${r.shots.length}/${EXPECTED_SHOTS} loading=${r.loading} analyze=${r.analyzeMs !== null ? (r.analyzeMs / 1000).toFixed(1) + 's' : '-'} pageErrors=${realErrors.length} consoleErrors=${r.consoleErrors.length}${r.failure ? ` FAILURE: ${r.failure}` : ''}`);
  for (const e of realErrors) console.log(`  pageerror: ${e}`);
}

writeFileSync(join(OUT, 'manifest.json'), JSON.stringify({ capturedAt: new Date().toISOString(), out: OUT, results }, null, 2));
console.log(`\nmanifest → ${join(OUT, 'manifest.json')}`);
console.log(`SCREENSHOT-GATE: ${pass ? 'PASS' : 'FAIL'}`);
process.exit(pass ? 0 : 1);
