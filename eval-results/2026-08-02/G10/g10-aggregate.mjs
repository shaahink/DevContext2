// g10-aggregate.mjs -- turns the G10.1 sweep dumps into the threshold-input grid.
//
// Reads raw/<pole>.stats.json (the gate OUTCOMES the engine actually reported: sparseGraph,
// entriesWithTarget, deepSpineRatio, per-kind seam totals) and raw/<pole>.graphdump.json (the one
// quantity stats does not carry: how many nodes have >= 2 outgoing Resolves edges, which is what
// GraphQuery's interesting-point "seat" rule counts).
//
// Run:  node C:/code/DevContext2/eval-results/2026-08-02/G10/g10-aggregate.mjs <dir>
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const dir = process.argv[2] ?? 'C:/code/DevContext2/eval-results/2026-08-02/G10';
const raw = join(dir, 'raw');
const poles = readdirSync(raw).filter((f) => f.endsWith('.stats.json')).map((f) => f.replace('.stats.json', ''));

const rows = [];
for (const p of poles) {
  const s = JSON.parse(readFileSync(join(raw, `${p}.stats.json`), 'utf8'));
  const seam = (k) => s.seams?.find((x) => x.kind === k) ?? { total: 0, verified: 0 };
  const calls = seam('Calls');
  const entries = s.entryCount ?? 0;
  const wired = s.entriesWithTarget ?? 0;

  let seats = 0, resolvesEdges = 0, maxSeat = 0;
  try {
    const g = JSON.parse(readFileSync(join(raw, `${p}.graphdump.json`), 'utf8'));
    const out = new Map();
    for (const e of g.edges) {
      if (e.kind !== 'Resolves') continue;
      resolvesEdges++;
      out.set(e.from, (out.get(e.from) ?? 0) + 1);
    }
    for (const n of out.values()) { if (n >= 2) seats++; maxSeat = Math.max(maxSeat, n); }
  } catch { /* dump missing -> seats stay 0, flagged by resolvesEdges 0 */ }

  rows.push({
    pole: p,
    nodes: s.nodeCount ?? 0,
    edges: s.edgeCount ?? 0,
    ratio: s.nodeCount ? +(s.edgeCount / s.nodeCount).toFixed(3) : 0,
    entries,
    wired,
    wiredRatio: entries ? +(wired / entries).toFixed(3) : 0,
    calls: calls.total,
    callsVerifiedRatio: calls.total ? +(calls.verified / calls.total).toFixed(3) : 0,
    handles: seam('Handles').total,
    sends: seam('Sends').total,
    resolvesEdges,
    seats,
    maxSeat,
    deepSpine: s.entriesWithDeepSpine ?? 0,
    deepSpineRatio: s.deepSpineRatio ?? 0,
    sparseGraph: !!s.sparseGraph,
    hubScopeNodes: s.hubScopeNodes ?? 0,
  });
}

rows.sort((a, b) => a.nodes - b.nodes);

const cols = Object.keys(rows[0]);
const w = cols.map((c) => Math.max(c.length, ...rows.map((r) => String(r[c]).length)));
const line = (cells) => '| ' + cells.map((v, i) => String(v).padEnd(w[i])).join(' | ') + ' |';
console.log(line(cols));
console.log('|' + w.map((n) => '-'.repeat(n + 2)).join('|') + '|');
for (const r of rows) console.log(line(cols.map((c) => r[c])));

// ---- threshold verdicts, one line per rule, stating which poles each side of the bar --------------
const side = (pred) => rows.filter(pred).map((r) => r.pole).join(', ') || '(none)';
console.log('\nTHRESHOLD OUTCOMES ON CURRENT DATA');
console.log(`T-A GraphBuilder.Seams entries>=5 && ratio>=0.1  DENSE: ${side((r) => r.entries >= 5 && r.ratio >= 0.1)}`);
console.log(`T-A engine-reported sparseGraph=true            : ${side((r) => r.sparseGraph)}`);
console.log(`T-B GraphOrphans calls>=30 && verified>=.5 && wired>=.5 PASSES: ${side((r) => r.calls >= 30 && r.callsVerifiedRatio >= 0.5 && r.wiredRatio >= 0.5)}`);
console.log(`T-B  blocked by wiredRatio<0.5 alone            : ${side((r) => r.calls >= 30 && r.callsVerifiedRatio >= 0.5 && r.wiredRatio < 0.5)}`);
console.log(`T-B2 handles>=5 || sends>=10                    : ${side((r) => r.handles >= 5 || r.sends >= 10)}`);
console.log(`T-C home-page unwired/entries>0.2 => 'warning'  : ${side((r) => r.entries > 0 && r.wired < r.entries && (r.entries - r.wired) / r.entries > 0.2)}`);
console.log(`T-D GraphStats deepSpineRatio == 1 (saturated)  : ${side((r) => r.deepSpineRatio === 1)}`);
console.log(`T-E GraphQuery seats(>=2 Resolves) > 20 (Take20 truncates): ${side((r) => r.seats > 20)}`);
