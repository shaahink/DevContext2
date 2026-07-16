import type {
  Edge,
  EntryPoint,
  NodeResponse,
  TraceNode,
} from '../core/grpc/gen/devcontext/v1/devcontext_pb';

export type AnalysisStatus = 'idle' | 'cloning' | 'analyzing' | 'ready' | 'error';

export interface EntryVm {
  readonly kind: string;
  readonly title: string;
  readonly nodeId: string;
  readonly httpMethod?: string;
  readonly route?: string;
  readonly target?: string;
  readonly provenance?: string;
  readonly project?: string;
  readonly groupPath?: string;
  /** L3.2 — Graph-aware composite score (0..1) for ranking entries by importance. */
  readonly score?: number;
  /** L3.5 — Authorization attributes (e.g. "[Authorize]", "[AllowAnonymous]"). */
  readonly authAttributes?: readonly string[];
  /** The string passed to GetTrace to trace this entry. */
  readonly focus: string;
  /** D9 — Architectural layer (Api, Application, Domain, Infrastructure, etc.) */
  readonly layer?: string;
  /** D9 — Feature area derived from namespace/folder conventions. */
  readonly feature?: string;
}

export interface EntryGroupVm {
  readonly kind: string;
  readonly label: string;
  readonly entries: readonly EntryVm[];
}

export interface TraceNodeVm {
  readonly id: string;
  readonly title: string;
  readonly kind: string;
  readonly seam: string;
  readonly depth: number;
  readonly provenance?: string;
  readonly resolution: string;
  readonly truncated: boolean;
  readonly omitted: number;
  readonly salient?: string;
  readonly tags: readonly string[];
  readonly children: readonly TraceNodeVm[];
  readonly layer?: string;
  readonly feature?: string;
}

export interface NodeDetailVm {
  readonly id: string;
  readonly title: string;
  readonly kind: string;
  readonly tags: readonly string[];
  readonly filePath?: string;
  readonly outDegree: number;
  readonly inDegree: number;
  readonly lineNumber?: number;
  readonly layer?: string;
  readonly feature?: string;
}

export interface EdgeVm {
  readonly from: string;
  readonly to: string;
  readonly kind: string;
  readonly resolution: string;
  readonly provenance?: string;
  readonly otherTitle: string;
}

export const KIND_LABELS: Record<string, string> = {
  HttpEndpoint: 'HTTP',
  MessageConsumer: 'Bus consumers',
  HostedService: 'Hosted services',
  ScheduledJob: 'Scheduled jobs',
  DomainEventHandler: 'Domain events',
  PublicApi: 'Public API',
  GrpcService: 'gRPC',
  SignalRHub: 'SignalR hubs',
  FunctionEntry: 'Functions',
  GrainMethod: 'Grains',
  GraphQlField: 'GraphQL',
  CliCommand: 'CLI',
  UiEntry: 'UI',
};

export const KIND_ICONS: Record<string, string> = {
  HttpEndpoint: 'webhook',
  MessageConsumer: 'arrow-right',
  HostedService: 'play',
  ScheduledJob: 'refresh',
  DomainEventHandler: 'dot',
  PublicApi: 'network',
};

/** Per-kind CSS color variable references — the single registry for kind coloring
 * across the whole UI (M7.0). Every surface maps kinds → hue from here, never inline.
 * Values are `var(--vibe-*)` so they automatically follow the active vibe/theme. */
export const KIND_COLORS: Record<string, string> = {
  HttpEndpoint: 'var(--vibe-info)',
  MessageConsumer: 'var(--vibe-warn)',
  HostedService: 'var(--vibe-success)',
  ScheduledJob: 'var(--vibe-accent)',
  DomainEventHandler: 'var(--vibe-accent-dim)',
  PublicApi: 'var(--vibe-info)',
  // T5.5 (audit finding 50) — danger is reserved for ERROR states: the red gRPC kind glyph
  // on DiscountProtoService read as an error badge to the auditor. Nothing was wrong.
  GrpcService: 'var(--vibe-accent-dim)',
  SignalRHub: 'var(--vibe-warn)',
  FunctionEntry: 'var(--vibe-success)',
  GrainMethod: 'var(--vibe-accent)',
  GraphQlField: 'var(--vibe-info)',
  CliCommand: 'var(--vibe-ink-muted)',
  UiEntry: 'var(--vibe-accent)',
};

const ENTRY_KIND_LABELS: Record<string, string> = KIND_LABELS;

const ENTRY_KIND_ORDER = [
  'HttpEndpoint',
  'MessageConsumer',
  'DomainEventHandler',
  'HostedService',
  'ScheduledJob',
  'PublicApi',
];

export function toEntryVm(e: EntryPoint): EntryVm {
  const focus = e.httpMethod && e.route ? `${e.httpMethod} ${e.route}` : e.title;
  return {
    kind: e.kind,
    title: e.title,
    nodeId: e.nodeId,
    httpMethod: e.httpMethod,
    route: e.route,
    target: e.target,
    provenance: e.provenance,
    project: e.project,
    groupPath: e.groupPath,
    score: e.score,
    authAttributes: e.authAttributes,
    focus,
    layer: e.layer,
    feature: e.feature,
  };
}

export function groupEntries(entries: readonly EntryPoint[]): EntryGroupVm[] {
  const byKind = new Map<string, EntryVm[]>();
  for (const e of entries) {
    let list = byKind.get(e.kind);
    if (!list) {
      list = [];
      byKind.set(e.kind, list);
    }
    list.push(toEntryVm(e));
  }
  return [...byKind.keys()]
    .sort((a, b) => orderIndex(a) - orderIndex(b))
    .map((kind) => ({
      kind,
      label: ENTRY_KIND_LABELS[kind] ?? kind,
      entries: byKind.get(kind)!,
    }));
}

function orderIndex(kind: string): number {
  const i = ENTRY_KIND_ORDER.indexOf(kind);
  return i === -1 ? ENTRY_KIND_ORDER.length : i;
}

/** Confidence Ledger tree filter (proposal §3.5) — keeps a node if it's itself
 * non-`Semantic` OR any descendant is, so matches stay reachable from the root
 * instead of orphaning them by cutting a fully-verified ancestor. Returns null
 * when nothing in the (sub)tree matches. */
export function filterApproxTree(node: TraceNodeVm): TraceNodeVm | null {
  const children = node.children
    .map(filterApproxTree)
    .filter((c): c is TraceNodeVm => c !== null);
  const selfMatches = node.resolution !== 'Semantic';
  if (!selfMatches && children.length === 0) return null;
  return { ...node, children };
}

export function toTraceVm(node: TraceNode): TraceNodeVm {
  return {
    id: node.nodeId,
    title: node.title,
    kind: node.kind,
    seam: node.seam,
    depth: node.depth,
    provenance: node.provenance,
    resolution: node.resolution,
    truncated: node.truncated,
    omitted: node.omitted,
    salient: node.salient,
    tags: node.tags,
    children: node.children.map(toTraceVm),
    layer: node.layer,
    feature: node.feature,
  };
}

export function toNodeDetailVm(n: NodeResponse): NodeDetailVm {
  return {
    id: n.nodeId,
    title: n.title,
    kind: n.kind,
    tags: n.tags,
    filePath: n.filePath,
    outDegree: n.outDegree,
    inDegree: n.inDegree,
    lineNumber: n.lineNumber,
    layer: n.layer,
    feature: n.feature,
  };
}

export function toEdgeVm(e: Edge): EdgeVm {
  return {
    from: e.from,
    to: e.to,
    kind: e.kind,
    resolution: e.resolution,
    provenance: e.provenance,
    otherTitle: e.otherTitle,
  };
}
