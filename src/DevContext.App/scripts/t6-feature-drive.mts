/**
 * T6 feature drive — verifies the T6 checkpoints end-to-end on eShop (the microservices
 * pole; the shamshir/monolith pole ran in T6.0). Asserts, per checkpoint:
 *   T6.7  hero renders the cytoscape canvas (edges drawn — screenshot evidence) + no ;-joined TFMs
 *   T6.11 one-pager: full service names (no bare "API" rows), >=8 event-wiring rows on eShop,
 *         cross-service labels, download button present
 *   T6.9  reload reattaches WITHOUT an Analyze RPC; "Trace checkout" lands on a deep flow
 *   T6.10 sessions table "use" button prefills try-a-tool; feed default = agents only
 *   T6.3  insights render tier words, never "% conf"; no desktop/library copy on a web repo
 *   T6.4  settings Server group shows the live URL
 *   T6.8  deck rows carry no absolute C:\ paths; entry provenance repo-relative
 *
 * Run: node --experimental-strip-types scripts/t6-feature-drive.mts
 */
import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const APP = 'http://localhost:4200';
const REPO = 'C:\\code\\DevContext2\\eval-repos\\eShop';
const OUT = join(process.cwd(), '..', '..', 'eval-results', new Date().toISOString().slice(0, 10), 'tapestry-t6', 'feature-drive');
const ANALYZE_TIMEOUT = 300_000;

mkdirSync(OUT, { recursive: true });

const results: Array<{ step: string; pass: boolean; detail: string }> = [];
function assert(step: string, pass: boolean, detail: string) {
  results.push({ step, pass, detail });
  console.log(`${pass ? 'PASS' : 'FAIL'} ${step} — ${detail}`);
}
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const context = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: APP });
const page = await context.newPage();

const rpcCounts: Record<string, number> = {};
page.on('request', (r) => {
  const m = r.url().match(/devcontext\.v1\.DevContextService\/(\w+)/);
  if (m) rpcCounts[m[1]] = (rpcCounts[m[1]] ?? 0) + 1;
});

async function snap(name: string) {
  await page.screenshot({ path: join(OUT, `${name}.png`), fullPage: true });
}

// ── 1. Analyze eShop ─────────────────────────────────────────────
await page.goto(APP, { waitUntil: 'networkidle' });
await sleep(2000);
const input = page.locator('app-start-hero input').first();
if (await input.count()) {
  await input.fill(REPO);
  await sleep(300);
  await page.locator("app-start-hero app-button[variant='primary']").first().click();
  await page.waitForSelector('app-identity-strip', { timeout: ANALYZE_TIMEOUT });
  await sleep(3000);
}

// ── 2. T6.7 hero canvas + TFM hygiene ───────────────────────────
{
  const heroCanvas = await page.evaluate(() =>
    !!document.querySelector('app-service-map-hero app-graph-canvas canvas'));
  assert('T6.7 hero-canvas', heroCanvas, `cytoscape canvas inside service-map-hero: ${heroCanvas}`);
  const rawTfm = await page.evaluate(() => /net\d+\.\d+-\w+;net\d+/.test(document.body.innerText));
  assert('T6.7 no-joined-tfms', !rawTfm, `;-joined TFM string in home DOM: ${rawTfm}`);
  await snap('01-home-hero-eshop');
}

// ── 3. T6.9 trace hero tile lands deep ───────────────────────────
{
  // The tile resolves via the atlas flow index — wait for indexing to finish (statusbar
  // shows "indexing flows i/N" while running) so the deep-flow pick is deterministic.
  for (let i = 0; i < 60; i++) {
    const label = await page.evaluate(() => /indexing flows \d+\/\d+/.test(document.body.innerText));
    if (!label && i > 3) break; // absent after a few polls = done (or finished before we looked)
    await sleep(3000);
  }
  const tile = page.locator("[data-testid='trace-hero']").first();
  if (await tile.count()) {
    const tileTitle = await tile.getAttribute('title');
    await tile.click();
    await sleep(6000);
    const focus = await page.evaluate(() => new URLSearchParams(location.search).get('focus'));
    // ≥3 hops (plan gate): the flow tree renders recursive app-trace-node elements —
    // 4 nesting levels = root + 3 hops.
    const depthOk = await page.evaluate(() =>
      document.querySelectorAll('app-trace-node app-trace-node app-trace-node app-trace-node').length > 0);
    assert('T6.9 trace-hero-deep', !!focus && depthOk, `tile="${tileTitle}" focus=${focus} depth>=3: ${depthOk}`);
    await snap('02-trace-hero');
  } else {
    assert('T6.9 trace-hero-deep', false, 'trace hero tile not found on home');
  }
}

// ── 4. T6.8 no absolute paths in the workbench dump ──────────────
{
  const absPaths = await page.evaluate(() => (document.body.innerText.match(/C:\\[^\s]{10,}/g) ?? []).length);
  assert('T6.8 no-abs-paths-explore', absPaths === 0, `C:\\ path fragments in explore DOM: ${absPaths}`);
}

// ── 5. T6.11 one-pager golden ─────────────────────────────────────
{
  await page.goto(APP + '/atlas', { waitUntil: 'domcontentloaded' });
  await sleep(4000);
  await snap('03-atlas-eshop');
  const dl = await page.locator("[data-testid='onepager-download']").count();
  assert('T6.11 download-button', dl > 0, `download button present: ${dl > 0}`);
  const btn = page.locator('button', { hasText: 'Export one-pager' }).first();
  await btn.click();
  await sleep(800);
  const md = await page.evaluate(() => navigator.clipboard.readText());
  writeFileSync(join(OUT, 'eshop-onepager.md'), md, 'utf-8');
  const serviceLines = md.split('\n').filter((l) => l.startsWith('- **'));
  const bareApi = serviceLines.filter((l) => /^- \*\*API\*\*/.test(l)).length;
  const eventRows = (md.match(/^- .+ → \*\*.+\*\* → /gm) ?? []).length;
  assert('T6.11 full-names', serviceLines.length > 0 && bareApi === 0,
    `services=${serviceLines.length} bare-API rows=${bareApi}`);
  assert('T6.11 event-rows', eventRows >= 8, `event wiring rows=${eventRows} (want >=8)`);
  assert('T6.11 cross-service-labels', /cross-service/.test(md), `cross-service label present: ${/cross-service/.test(md)}`);
}

// ── 6. T6.3 insights tier words ──────────────────────────────────
{
  await page.goto(APP + '/insights', { waitUntil: 'domcontentloaded' });
  await sleep(2500);
  await snap('04-insights-eshop');
  const txt = await page.evaluate(() => document.body.innerText);
  const pctConf = /%\s*conf/.test(txt);
  const tierWords = /(high|moderate|low) confidence/.test(txt);
  const desktopCopy = /desktop app|library's 'heart'|--focus/.test(txt);
  assert('T6.3 tier-words', !pctConf && tierWords, `pctConf=${pctConf} tierWords=${tierWords}`);
  assert('T6.3 archetype-copy', !desktopCopy, `desktop/library/CLI copy on a web repo: ${desktopCopy}`);
}

// ── 7. T6.4 settings live server URL ─────────────────────────────
{
  await page.goto(APP + '/settings', { waitUntil: 'domcontentloaded' });
  await sleep(1200);
  await page.locator('button', { hasText: 'Server' }).first().click();
  await sleep(600);
  const txt = await page.evaluate(() => document.body.innerText);
  // Case-insensitive: the label renders as "SERVER URL" (CSS uppercase affects innerText).
  const live = /Server URL[\s\S]*http:\/\/127\.0\.0\.1:5179/i.test(txt) && /Health target/i.test(txt);
  assert('T6.4 live-server-url', live, `live URL + health target rendered: ${live}`);
  await snap('05-settings-server');
}

// ── 8. T6.10 MCP ergonomics ──────────────────────────────────────
{
  await page.goto(APP + '/mcp', { waitUntil: 'domcontentloaded' });
  await sleep(3500);
  const filterDefault = await page.locator("[data-testid='feed-origin-filter']").innerText().catch(() => '');
  assert('T6.10 feed-default-agents', /agents only/i.test(filterDefault), `filter chip: "${filterDefault}"`);
  const useBtn = page.locator("[data-testid='session-use']").first();
  if (await useBtn.count()) {
    await useBtn.click();
    await sleep(300);
    const handleVal = await page.locator('#try-handle-input').inputValue();
    const runEnabled = await page.locator('button', { hasText: /^Run$/ }).first().isEnabled();
    assert('T6.10 use-prefills', handleVal.length >= 16 && runEnabled, `handle len=${handleVal.length} runEnabled=${runEnabled}`);
    // actually run the tool — the audit's broken loop was "shown handle rejected"
    await page.locator('button', { hasText: /^Run$/ }).first().click();
    await sleep(2500);
    const result = await page.evaluate(() => document.querySelector('pre.max-h-40, .rounded.border pre')?.textContent ?? document.body.innerText);
    const ok = !/not_found|Unknown session handle/i.test(result ?? '');
    assert('T6.10 try-a-tool-succeeds', ok, `no not_found error: ${ok}`);
  } else {
    assert('T6.10 use-prefills', false, 'no session rows rendered');
  }
  await snap('06-mcp-page');
}

// ── 9. T6.9 reattach on reload without Analyze ───────────────────
{
  const analyzesBefore = rpcCounts['Analyze'] ?? 0;
  await page.goto(APP + '/', { waitUntil: 'domcontentloaded' });
  await sleep(8000);
  const ready = await page.evaluate(() => !!document.querySelector('app-identity-strip'));
  const analyzesAfter = rpcCounts['Analyze'] ?? 0;
  assert('T6.9 reattach-no-reanalyze', ready && analyzesAfter === analyzesBefore,
    `ready=${ready} analyzeRpcs before=${analyzesBefore} after=${analyzesAfter}`);
  const tiles = await page.evaluate(() => document.body.innerText.includes('START HERE'));
  assert('T6.9 tiles-on-revisit', tiles, `START HERE present after reload: ${tiles}`);
  await snap('07-home-reattached');
}

writeFileSync(join(OUT, 'results.json'), JSON.stringify({ results, rpcCounts }, null, 1), 'utf-8');
const fails = results.filter((r) => !r.pass);
console.log(`\n${results.length - fails.length}/${results.length} pass`);
if (fails.length) { console.log('FAILS:', fails.map((f) => f.step).join(', ')); process.exitCode = 1; }
await browser.close();
