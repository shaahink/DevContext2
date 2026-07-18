import type { ELK, ElkNode } from 'elkjs/lib/elk-api';

/**
 * D4.1 (F2/L6): ONE deterministic layered layout for every canvas altitude. Pure module —
 * elements in, center-point geometry out — so determinism is unit-testable without a DOM.
 *
 * Why ELK layered (not fcose/dagre): architecture reads in dependency ranks, so a layered
 * left-to-right layout IS the domain shape; force-directed placement gave the baseline its
 * hairballs (33-project bitwarden) and rank-less blobs. ELK is deterministic for a given
 * input order — and input order is pinned by sorting nodes/edges lexicographically here,
 * so even shuffled callers get byte-identical geometry (spec-pinned). Its compound-node
 * support is also what D4.2's per-service expansion and lane grouping need.
 */

export interface LayoutNodeIn {
  readonly id: string;
  readonly label: string;
}

export interface LayoutEdgeIn {
  readonly id: string;
  readonly source: string;
  readonly target: string;
}

/** x/y are CENTER coordinates (cytoscape preset positions), not ELK's top-left. */
export interface NodeGeometry {
  readonly x: number;
  readonly y: number;
  readonly width: number;
  readonly height: number;
}

export interface LayoutOptions {
  /** Hero embedding: tighter spacing so small canvases stay dense. */
  readonly compact?: boolean;
}

export const NODE_HEIGHT = 26;
/** Cascadia Code advance width at font-size 10 — monospace makes label→box width pure
 * arithmetic, so the layout engine always knows the full visual footprint of a node and
 * label overlap/clipping is impossible by construction (the baseline's floating labels
 * were invisible to the layout). */
const CHAR_WIDTH = 6.1;
const LABEL_PAD_X = 18;
const MIN_NODE_WIDTH = 56;
const MAX_NODE_WIDTH = 250;

export function nodeWidthForLabel(label: string): number {
  return Math.round(Math.min(MAX_NODE_WIDTH, Math.max(MIN_NODE_WIDTH, label.length * CHAR_WIDTH + LABEL_PAD_X)));
}

let elkInstance: ELK | null = null;

async function getElk(): Promise<ELK> {
  if (!elkInstance) {
    // Bundled build (no web worker): graphs here are ≤ a few hundred nodes, layout runs in ms.
    const mod = await import('elkjs/lib/elk.bundled.js');
    const Ctor = (mod.default ?? mod) as unknown as new () => ELK;
    elkInstance = new Ctor();
  }
  return elkInstance;
}

export async function layoutGraph(
  nodes: readonly LayoutNodeIn[],
  edges: readonly LayoutEdgeIn[],
  opts: LayoutOptions = {},
): Promise<Map<string, NodeGeometry>> {
  const result = new Map<string, NodeGeometry>();
  if (nodes.length === 0) return result;

  const sortedNodes = [...nodes].sort((a, b) => a.id.localeCompare(b.id, 'en'));
  const nodeIds = new Set(sortedNodes.map((n) => n.id));
  const sortedEdges = [...edges]
    .filter((e) => nodeIds.has(e.source) && nodeIds.has(e.target))
    .sort((a, b) => a.source.localeCompare(b.source, 'en') || a.target.localeCompare(b.target, 'en') || a.id.localeCompare(b.id, 'en'));

  const graph: ElkNode = {
    id: 'root',
    layoutOptions: {
      'elk.algorithm': 'layered',
      'elk.direction': 'RIGHT',
      'elk.layered.spacing.nodeNodeBetweenLayers': opts.compact ? '40' : '60',
      'elk.spacing.nodeNode': opts.compact ? '12' : '20',
      'elk.spacing.edgeNode': '12',
      'elk.padding': '[top=8,left=8,bottom=8,right=8]',
    },
    children: sortedNodes.map((n) => ({
      id: n.id,
      width: nodeWidthForLabel(n.label),
      height: NODE_HEIGHT,
    })),
    edges: sortedEdges.map((e) => ({ id: e.id, sources: [e.source], targets: [e.target] })),
  };

  const elk = await getElk();
  const laid = await elk.layout(graph);
  for (const child of laid.children ?? []) {
    const w = child.width ?? MIN_NODE_WIDTH;
    const h = child.height ?? NODE_HEIGHT;
    result.set(child.id, {
      x: (child.x ?? 0) + w / 2,
      y: (child.y ?? 0) + h / 2,
      width: w,
      height: h,
    });
  }
  return result;
}
