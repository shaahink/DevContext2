// r1-aggregate.mjs -- turns the R1.1 sweep dumps into the post-E1 threshold-input grid.
//
// Successor to eval-results/2026-08-02/G10/g10-aggregate.mjs. Reads raw/<pole>.stats.json only:
// `query stats` now reports every quantity the three metrics under recalibration read, INCLUDING
// the insight list (so "did graph.orphans actually emit" is measured, not inferred from the
// clauses). raw/<pole>.map.md supplies the archetype line, which is graph.orphans' first clause and
// the one stats does not carry.
//
// Run:  node C:/Code/DevContext2-engine/eval-results/2026-08-14/r1-metrics/r1-aggregate.mjs <dir>
import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { join } from 'node:path';

const dir = process.argv[2] ?? 'C:/Code/DevContext2-engine/eval-results/2026-08-14/r1-metrics';
const raw = join(dir, 'raw');
const poles = readdirSync(raw).filter((f) => f.endsWith('.stats.json')).map((f) => f.replace('.stats.json', ''));

// The 2026-08-02 G10 grid, for the before/after column. Poles absent from this machine are listed
// under MISSING below rather than dropped in silence.
const G10 = {
  CleanArchitecture: { calls: 27, sem: 0.259, spine: 1.0, sparse: false },
  MediatR: { calls: 26, sem: 0.731, spine: 1.0, sparse: false },
  Dapper: { calls: 61, sem: 0.41, spine: 0, sparse: false },
  Serilog: { calls: 91, sem: 0.099, spine: 0, sparse: false },
  'dotnet-podcasts': { calls: 208, sem: 0.01, spine: 1.0, sparse: false },
  'MahApps.Metro': { calls: 105, sem: 0.495, spine: 0, sparse: false },
  GitVersion: { calls: 495, sem: 0.091, spine: 0, sparse: false },
  eShop: { calls: 699, sem: 0.057, spine: 0.982, sparse: false },
  self: { calls: 1377, sem: 0.173, spine: 1.0, sparse: false },
  DntSite: { calls: 4023, sem: 0.088, spine: 1.0, sparse: false },
  wolverine: { calls: 3688, sem: 0.161, spine: 0.961, sparse: false },
};

const rows = [];
for (const p of poles) {
  const s = JSON.parse(readFileSync(join(raw, `${p}.stats.json`), 'utf8'));
  const seam = (k) => s.seams?.find((x) => x.kind === k) ?? { total: 0, verified: 0, joined: 0, approx: 0 };
  const calls = seam('Calls');
  const entries = s.entryCount ?? 0;
  const wired = s.entriesWithTarget ?? 0;

  // graph.orphans' archetype clause (line 35 of GraphOrphansSource): a Library never emits.
  // The map renders the archetype as the first token of line 1 ("LIBRARY  Hangfire (279 public types)").
  let archetype = '?';
  const mapPath = join(raw, `${p}.map.md`);
  if (existsSync(mapPath)) {
    const first = readFileSync(mapPath, 'utf8').split('\n')[0] ?? '';
    const m = first.match(/^([A-Z][A-Za-z]*)\s/);
    if (m) archetype = m[1];
  }

  const orphansInsight = (s.insights ?? []).find((i) => i.id === 'graph.orphans');

  rows.push({
    pole: p,
    arch: archetype,
    nodes: s.nodeCount ?? 0,
    edges: s.edgeCount ?? 0,
    ratio: s.nodeCount ? +(s.edgeCount / s.nodeCount).toFixed(3) : 0,
    entries,
    wired,
    wiredRatio: entries ? +(wired / entries).toFixed(3) : 0,
    calls: calls.total,
    semShare: calls.total ? +(calls.verified / calls.total).toFixed(3) : 0,
    joinShare: calls.total ? +(calls.joined / calls.total).toFixed(3) : 0,
    handles: seam('Handles').total,
    sends: seam('Sends').total,
    deepSpine: s.entriesWithDeepSpine ?? 0,
    spineRatio: +(s.deepSpineRatio ?? 0).toFixed(3),
    sparseGraph: !!s.sparseGraph,
    hubScope: s.hubScopeNodes ?? 0,
    orphansFired: orphansInsight ? orphansInsight.evidence.length : 0,
  });
}

rows.sort((a, b) => a.nodes - b.nodes);

const cols = Object.keys(rows[0]);
const w = cols.map((c) => Math.max(c.length, ...rows.map((r) => String(r[c]).length)));
const line = (cells) => '| ' + cells.map((v, i) => String(v).padEnd(w[i])).join(' | ') + ' |';
console.log('POST-E1 GRID (this build). semShare = Calls edges with Resolution.Semantic / Calls  -- the');
console.log('number GraphOrphansSource reads through EdgeConfidence.IsVerified. orphansFired = how many');
console.log('types the graph.orphans insight actually named on this pole (0 = the source emitted nothing).');
console.log('');
console.log(line(cols));
console.log('|' + w.map((n) => '-'.repeat(n + 2)).join('|') + '|');
for (const r of rows) console.log(line(cols.map((c) => r[c])));

console.log('\nBEFORE/AFTER vs the 2026-08-02 G10 grid (same pole, same quantity)');
console.log('| pole | calls 08-02 -> now | semShare 08-02 -> now | spineRatio 08-02 -> now | sparse 08-02 -> now |');
console.log('|---|---|---|---|---|');
for (const r of rows) {
  const b = G10[r.pole];
  if (!b) continue;
  console.log(`| ${r.pole} | ${b.calls} -> ${r.calls} | ${b.sem} -> ${r.semShare} | ${b.spine} -> ${r.spineRatio} | ${b.sparse} -> ${r.sparseGraph} |`);
}
const missing = Object.keys(G10).filter((k) => !rows.some((r) => r.pole === k));
console.log(`\nG10 POLES NOT ON THIS MACHINE (not measured, not dropped silently): ${missing.join(', ')}`);

// ---- threshold verdicts -----------------------------------------------------------------------
const side = (pred) => rows.filter(pred).map((r) => r.pole).join(', ') || '(none)';
console.log('\nTHRESHOLD OUTCOMES ON POST-E1 DATA');
console.log(`#22 GraphOrphansSource full gate (non-Library, handles>=5||sends>=10, calls>=30, sem>=.5, wired>=.5):`);
console.log(`      PASSES : ${side((r) => r.arch.toUpperCase() !== 'LIBRARY' && (r.handles >= 5 || r.sends >= 10) && r.calls >= 30 && r.semShare >= 0.5 && r.wiredRatio >= 0.5)}`);
console.log(`      sem>=.5 alone (the clause that shut it in 08-02): ${side((r) => r.semShare >= 0.5)}`);
console.log(`      sem in [.4,.5) - within one E1-sized step of the floor: ${side((r) => r.semShare >= 0.4 && r.semShare < 0.5)}`);
console.log(`      insight ACTUALLY emitted: ${side((r) => r.orphansFired > 0)}`);
console.log(`#23 L3.4 gate says sparse (entries<5 || ratio<0.1): ${side((r) => r.entries < 5 || r.ratio < 0.1)}`);
console.log(`      engine-reported sparseGraph=true (broadening RAN): ${side((r) => r.sparseGraph)}`);
console.log(`#24 deepSpineRatio == 1.000 (saturated): ${side((r) => r.spineRatio === 1)}`);
console.log(`      deepSpineRatio in (0,1) - the metric separating anything: ${side((r) => r.spineRatio > 0 && r.spineRatio < 1)}`);
console.log(`      deepSpineRatio == 0 with entries > 0 (a real zero): ${side((r) => r.spineRatio === 0 && r.entries > 0)}`);
