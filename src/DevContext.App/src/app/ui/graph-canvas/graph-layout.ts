import type { ELK, ElkNode } from 'elkjs/lib/elk-api';

/**
 * D4.1 (F2/L6): ONE deterministic layered layout for every canvas altitude. Pure module —
 * elements in, center-point geometry out — so determinism is unit-testable without a DOM.
 *
 * Why ELK layered (not fcose/dagre): architecture reads in dependency ranks, so a layered
 * left-to-right layout IS the domain shape; force-directed placement gave the baseline its
 * hairballs (33-project bitwarden) and rank-less blobs. ELK is deterministic for a given
 * input order — and input order is pinned by sorting nodes/edges lexicographically here,
 * so even shuffled callers get byte-identical geometry (spec-pinned).
 *
 * D4.2 (F3/M): one level of hierarchy — a node may carry children (an expanded service's
 * projects, a DDD-layer lane). ELK lays nested graphs natively ('INCLUDE_CHILDREN');
 * results flatten to absolute coordinates so cytoscape's preset layout stays flat, with
 * compound membership expressed via `parent` on the element definitions.
 */

export interface LayoutNodeIn {
  readonly id: string;
  readonly label: string;
  /** One level of nesting: an expanded service's members or a layer lane's projects. */
  readonly children?: readonly LayoutNodeIn[];
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
  /**
   * Flow axis. R3 D-B (B-3): a layered graph is as wide as its longest chain and as tall as its
   * widest layer, so a RIGHT layout in a portrait pane fits to WIDTH and leaves the height empty —
   * which is what actually made eShop's topology use a quarter of the pane, not the zoom clamp.
   * The Stage's pane is portrait, so the topology lays out DOWN there and fills it.
   */
  readonly direction?: 'RIGHT' | 'DOWN';
}

export const NODE_HEIGHT = 26;
/** Compound (parent) boxes reserve headroom for their top-left label. */
export const COMPOUND_LABEL_PAD = 22;
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

function sortNodes(nodes: readonly LayoutNodeIn[]): LayoutNodeIn[] {
  return [...nodes].sort((a, b) => a.id.localeCompare(b.id, 'en'));
}

function collectIds(nodes: readonly LayoutNodeIn[], into: Set<string>): void {
  for (const n of nodes) {
    into.add(n.id);
    if (n.children?.length) collectIds(n.children, into);
  }
}

function toElkNode(n: LayoutNodeIn, compact: boolean): ElkNode {
  if (!n.children?.length) {
    return { id: n.id, width: nodeWidthForLabel(n.label), height: NODE_HEIGHT };
  }
  return {
    id: n.id,
    layoutOptions: {
      'elk.padding': `[top=${COMPOUND_LABEL_PAD + 6},left=10,bottom=10,right=10]`,
      'elk.spacing.nodeNode': compact ? '10' : '16',
      'elk.layered.spacing.nodeNodeBetweenLayers': compact ? '30' : '44',
    },
    children: sortNodes(n.children).map((c) => toElkNode(c, compact)),
  };
}

export async function layoutGraph(
  nodes: readonly LayoutNodeIn[],
  edges: readonly LayoutEdgeIn[],
  opts: LayoutOptions = {},
): Promise<Map<string, NodeGeometry>> {
  const result = new Map<string, NodeGeometry>();
  if (nodes.length === 0) return result;

  const sortedNodes = sortNodes(nodes);
  const nodeIds = new Set<string>();
  collectIds(sortedNodes, nodeIds);
  const sortedEdges = [...edges]
    .filter((e) => nodeIds.has(e.source) && nodeIds.has(e.target))
    .sort((a, b) => a.source.localeCompare(b.source, 'en') || a.target.localeCompare(b.target, 'en') || a.id.localeCompare(b.id, 'en'));

  const graph: ElkNode = {
    id: 'root',
    layoutOptions: {
      'elk.algorithm': 'layered',
      'elk.direction': opts.direction ?? 'RIGHT',
      'elk.hierarchyHandling': 'INCLUDE_CHILDREN',
      'elk.layered.spacing.nodeNodeBetweenLayers': opts.compact ? '40' : '60',
      'elk.spacing.nodeNode': opts.compact ? '12' : '20',
      'elk.spacing.edgeNode': '12',
      'elk.padding': '[top=8,left=8,bottom=8,right=8]',
    },
    children: sortedNodes.map((n) => toElkNode(n, opts.compact ?? false)),
    edges: sortedEdges.map((e) => ({ id: e.id, sources: [e.source], targets: [e.target] })),
  };

  const elk = await getElk();
  const laid = await elk.layout(graph);

  // ELK child coordinates are relative to the parent — flatten to absolute centers.
  const flatten = (children: readonly ElkNode[] | undefined, offsetX: number, offsetY: number): void => {
    for (const child of children ?? []) {
      const w = child.width ?? MIN_NODE_WIDTH;
      const h = child.height ?? NODE_HEIGHT;
      const absX = offsetX + (child.x ?? 0);
      const absY = offsetY + (child.y ?? 0);
      result.set(child.id, { x: absX + w / 2, y: absY + h / 2, width: w, height: h });
      flatten(child.children, absX, absY);
    }
  };
  flatten(laid.children, 0, 0);
  return result;
}
