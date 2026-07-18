import { describe, expect, it } from 'vitest';

import { layoutGraph, nodeWidthForLabel, NODE_HEIGHT, type LayoutEdgeIn, type LayoutNodeIn } from './graph-layout';

/** bitwarden-shaped fixture: 33 nodes, 76 edges, generated with a seeded LCG so the
 * fixture itself is deterministic (no Math.random in specs). */
function bigFixture(): { nodes: LayoutNodeIn[]; edges: LayoutEdgeIn[] } {
  const nodes: LayoutNodeIn[] = [];
  for (let i = 0; i < 33; i++) {
    nodes.push({ id: `Svc.Project${String(i).padStart(2, '0')}`, label: `Svc.Project${String(i).padStart(2, '0')}.WithSuffix` });
  }
  let seed = 42;
  const next = () => {
    seed = (seed * 1103515245 + 12345) & 0x7fffffff;
    return seed;
  };
  const edges: LayoutEdgeIn[] = [];
  const seen = new Set<string>();
  while (edges.length < 76) {
    const a = next() % 33;
    const b = next() % 33;
    if (a === b) continue;
    const key = `${a}->${b}`;
    if (seen.has(key)) continue;
    seen.add(key);
    edges.push({ id: key, source: nodes[a].id, target: nodes[b].id });
  }
  return { nodes, edges };
}

function shuffled<T>(arr: readonly T[]): T[] {
  // Fixed permutation (reverse + interleave) — deterministic, order-destroying.
  const rev = [...arr].reverse();
  const out: T[] = [];
  for (let i = 0; i < rev.length; i += 2) out.push(rev[i]);
  for (let i = 1; i < rev.length; i += 2) out.push(rev[i]);
  return out;
}

describe('graph-layout (D4.1 determinism + no-clip contract)', () => {
  it('same input twice → identical geometry', async () => {
    const { nodes, edges } = bigFixture();
    const a = await layoutGraph(nodes, edges);
    const b = await layoutGraph(nodes, edges);
    expect([...a.entries()]).toEqual([...b.entries()]);
  });

  it('shuffled input order → identical geometry (input order is pinned by sorting)', async () => {
    const { nodes, edges } = bigFixture();
    const a = await layoutGraph(nodes, edges);
    const b = await layoutGraph(shuffled(nodes), shuffled(edges));
    for (const [id, g] of a) {
      expect(b.get(id)).toEqual(g);
    }
  });

  it('no two node boxes intersect (labels live inside boxes — overlap is impossible)', async () => {
    const { nodes, edges } = bigFixture();
    const geo = [...(await layoutGraph(nodes, edges)).values()];
    for (let i = 0; i < geo.length; i++) {
      for (let j = i + 1; j < geo.length; j++) {
        const a = geo[i];
        const b = geo[j];
        const overlapX = Math.abs(a.x - b.x) < (a.width + b.width) / 2;
        const overlapY = Math.abs(a.y - b.y) < (a.height + b.height) / 2;
        expect(overlapX && overlapY).toBe(false);
      }
    }
  });

  it('all geometry is finite and inside a sane bounding box (nothing flung to NaN/infinity)', async () => {
    const { nodes, edges } = bigFixture();
    const geo = await layoutGraph(nodes, edges);
    expect(geo.size).toBe(33);
    for (const g of geo.values()) {
      expect(Number.isFinite(g.x)).toBe(true);
      expect(Number.isFinite(g.y)).toBe(true);
      expect(g.x - g.width / 2).toBeGreaterThanOrEqual(0);
      expect(g.y - g.height / 2).toBeGreaterThanOrEqual(0);
      expect(g.width).toBeGreaterThan(0);
      expect(g.height).toBe(NODE_HEIGHT);
    }
  });

  it('edges referencing unknown nodes are dropped, not crashed on', async () => {
    const nodes: LayoutNodeIn[] = [{ id: 'A', label: 'A' }, { id: 'B', label: 'B' }];
    const edges: LayoutEdgeIn[] = [
      { id: 'ok', source: 'A', target: 'B' },
      { id: 'dangling', source: 'A', target: 'External.Package' },
    ];
    const geo = await layoutGraph(nodes, edges);
    expect(geo.size).toBe(2);
  });

  it('empty graph → empty result', async () => {
    expect((await layoutGraph([], [])).size).toBe(0);
  });

  it('node width tracks label length within clamps', () => {
    expect(nodeWidthForLabel('ab')).toBeGreaterThanOrEqual(56);
    expect(nodeWidthForLabel('a'.repeat(200))).toBeLessThanOrEqual(250);
    expect(nodeWidthForLabel('MediumLengthLabel')).toBeGreaterThan(nodeWidthForLabel('Tiny'));
  });
});
