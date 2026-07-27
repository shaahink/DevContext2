/** D4.5 probe (L4/L5/F4/F5): drives the Studio live preview on podcasts (the D4.7 DoD
 * instrument: build a pack entirely from the preview), the entry browser, unified
 * session naming on refit, and the MCP feed origin fix.
 * Prints STUDIO-NAV-PROBE: PASS|FAIL. */
import { chromium, type Page } from 'playwright';
import { mkdirSync } from 'node:fs';
import { join } from 'node:path';

const OUT = 'C:\\code\\DevContext2\\eval-results\\2026-07-18\\prism-d4\\d45';
mkdirSync(OUT, { recursive: true });
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

const failures: string[] = [];
async function assertThat(cond: boolean | Promise<boolean>, label: string): Promise<void> {
  if (await cond) console.log('ok  ', label);
  else { failures.push(label); console.log('FAIL', label); }
}
async function becomesVisible(locator: import('playwright').Locator, timeout = 8000): Promise<boolean> {
  try { await locator.waitFor({ state: 'visible', timeout }); return true; } catch { return false; }
}

const browser = await chromium.launch({ channel: 'chrome', headless: true });
const context = await browser.newContext({
  viewport: { width: 1600, height: 1000 },
  permissions: ['clipboard-read', 'clipboard-write'],
});
const page: Page = await context.newPage();
page.on('pageerror', (e) => { failures.push(`pageerror: ${e.message.slice(0, 120)}`); console.log('PAGEERROR:', e.message.slice(0, 200)); });

async function analyze(repo: string): Promise<void> {
  await page.goto('http://localhost:4200', { waitUntil: 'domcontentloaded' });
  const input = page.locator('app-start-hero input').first();
  await input.waitFor({ timeout: 15_000 });
  await input.fill(repo);
  await sleep(300);
  await page.locator("app-start-hero app-button[variant='primary']").first().click();
  await page.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: 180_000 });
  await sleep(1000);
}

// ================= podcasts: Studio live preview (L4) ============================
await analyze('C:\\code\\DevContext2\\eval-repos\\dotnet-podcasts');

await page.goto('http://localhost:4200/context', { waitUntil: 'domcontentloaded' });
await sleep(800);
// Empty state first: preview pane invites, no pack yet.
await assertThat(page.getByText('the assembled pack renders here').first().isVisible(), 'preview empty-state invites before any cards');

// Build the pack from the preset — the L4 loop: preset → cards → live preview.
await page.getByText('Change-impact pack').first().click();
await assertThat(becomesVisible(page.getByText(/seeds flow · bodies · contracts · tests/).first(), 3000), 'preset shows its one-line effect');
await sleep(300);
const presetRow = page.locator('app-scope-picker button', { hasText: '/' }).first();
await presetRow.click();
await assertThat(becomesVisible(page.getByText(/Preset added \d+ cards:/).first(), 5000), 'scope-delta toast names the added cards');

// The live preview renders the assembled pack + server token accounting.
const preview = page.locator('.pack-preview');
await assertThat(becomesVisible(preview.first(), 20_000), 'live preview pane renders');
await assertThat(becomesVisible(page.getByText(/\d+ tok · allocated \d+ · budget \d+/).first(), 20_000), 'server token accounting in the preview header');
await sleep(1500);
await page.screenshot({ path: join(OUT, 'podcasts-1-studio-preview.png'), fullPage: false });

// Copy copies EXACTLY what's shown.
const shown = (await preview.locator('code').textContent() ?? '').replace(/\r\n/g, '\n').trim();
await page.getByTestId('copy-context').click();
await sleep(500);
const copied = ((await page.evaluate(() => navigator.clipboard.readText())) ?? '').replace(/\r\n/g, '\n').trim();
await assertThat(shown.length > 100, `preview has real content (${shown.length} chars)`);
const copyMatches = copied === shown;
if (!copyMatches) {
  const { writeFileSync } = await import('node:fs');
  writeFileSync(join(OUT, 'copy-shown.txt'), shown);
  writeFileSync(join(OUT, 'copy-clipboard.txt'), copied);
  writeFileSync(join(OUT, 'copy-innerhtml.txt'), await preview.locator('code').innerHTML());
  console.log(`  (copy diff dumped: shown=${shown.length} chars, clipboard=${copied.length} chars)`);
}
await assertThat(copyMatches, 'Copy copies byte-what the preview shows');

// Budget change recomputes the preview (live loop) — watch the header total change or at least a re-pack.
const before = await page.getByText(/\d+ tok · allocated/).first().textContent();
const slider = page.locator('app-budget-panel input[type="range"]').first();
await slider.fill('12000');
await sleep(2000);
const after = await page.getByText(/\d+ tok · allocated/).first().textContent();
await assertThat(before !== after, `budget change recomputes the preview header (${before?.trim()} -> ${after?.trim()})`);
await page.screenshot({ path: join(OUT, 'podcasts-2-studio-rebudget.png'), fullPage: false });

// ================= entry browser (L5) ============================================
await page.goto('http://localhost:4200/explore', { waitUntil: 'domcontentloaded' });
await sleep(2500);
await page.getByText('Browse all entries').first().click();
const browserPanel = page.locator('app-entry-browser');
await assertThat(becomesVisible(browserPanel.first(), 8000), 'entry browser opens from the deck affordance');
await assertThat(becomesVisible(browserPanel.locator('h3').first(), 5000), 'browser groups by service');
await assertThat(browserPanel.getByText(/\d+ of \d+ · \d+ services/).first().isVisible(), 'browser header counts entries and services');
await page.screenshot({ path: join(OUT, 'podcasts-3-entry-browser.png'), fullPage: false });

// Filter-as-you-type narrows.
const totalLine = await browserPanel.getByText(/\d+ of \d+/).first().textContent() ?? '';
await browserPanel.locator('input').fill('feed');
await sleep(400);
const narrowed = await browserPanel.getByText(/\d+ of \d+/).first().textContent() ?? '';
await assertThat(totalLine !== narrowed, `filter narrows the count (${totalLine.trim()} -> ${narrowed.trim()})`);
await page.screenshot({ path: join(OUT, 'podcasts-4-entry-browser-filtered.png'), fullPage: false });

// Raw table survives as the power view.
await browserPanel.getByText('Raw table').click();
await assertThat(becomesVisible(page.locator('app-table-lens').first(), 5000), 'raw table reachable from the browser (power view)');
await page.keyboard.press('Escape');

// ================= F5: MCP feed origin ===========================================
await page.goto('http://localhost:4200/mcp', { waitUntil: 'domcontentloaded' });
await sleep(1500);
// Generate ui traffic from a SECOND page while /mcp observes.
const page2 = await context.newPage();
await page2.goto('http://localhost:4200/atlas', { waitUntil: 'domcontentloaded' });
await sleep(4000);
await page2.close();
// Default filter = agents only → the app's own RPCs must NOT appear (they did pre-fix).
const feedRows = page.locator('.font-mono', { hasText: /GetMap|GetFlowIndex|GetStats|GetGraphFacets/ });
await assertThat(feedRows.count().then((n) => n === 0), 'agents-only feed hides the app\'s own RPCs (F5)');
// Toggle to all origins → the same RPCs appear tagged ui.
await page.getByText(/agents only/i).first().click();
await sleep(800);
await assertThat(page.getByText('ui').first().isVisible(), 'all-origins feed shows the app RPCs tagged ui');
await page.screenshot({ path: join(OUT, 'podcasts-5-mcp-feed-origins.png'), fullPage: false });

// ================= F4: unified naming on refit ===================================
// Fresh context (clean localStorage) — the first context now restores the podcasts
// session on '/', so there is no start hero there.
const ctx2 = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
const page3 = await ctx2.newPage();
page3.on('pageerror', (e) => { failures.push(`pageerror(f4): ${e.message.slice(0, 120)}`); });
await page3.goto('http://localhost:4200', { waitUntil: 'domcontentloaded' });
const input3 = page3.locator('app-start-hero input').first();
await input3.waitFor({ timeout: 15_000 });
await input3.fill('C:\\code\\DevContext2\\eval-repos\\refit');
await sleep(300);
await page3.locator("app-start-hero app-button[variant='primary']").first().click();
await page3.locator('app-identity-strip').first().waitFor({ state: 'visible', timeout: 180_000 });
await sleep(1000);
const identity = await page3.locator('app-identity-strip p').first().textContent() ?? '';
await assertThat(identity.includes('Refit'), `home identity says Refit (got: ${identity.slice(0, 60)})`);
await assertThat(!identity.includes('DevContext.slnx'), 'home identity no longer leaks DevContext.slnx (F4)');
const tabLabel = await page3.locator('app-tab-strip, [class*="tab"]').first().textContent() ?? '';
await assertThat(tabLabel.includes('Refit') || tabLabel.includes('refit'), `tab agrees with the session identity (got: ${tabLabel.slice(0, 40)})`);
await page3.screenshot({ path: join(OUT, 'refit-6-unified-naming.png'), fullPage: false });
await ctx2.close();

await browser.close();
if (failures.length) {
  console.log('STUDIO-NAV-PROBE: FAIL');
  for (const f of failures) console.log('  -', f);
  process.exit(1);
}
console.log('STUDIO-NAV-PROBE: PASS');
