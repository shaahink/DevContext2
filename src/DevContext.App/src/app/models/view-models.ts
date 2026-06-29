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
  /** The string passed to GetTrace to trace this entry. */
  readonly focus: string;
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
}

export interface NodeDetailVm {
  readonly id: string;
  readonly title: string;
  readonly kind: string;
  readonly tags: readonly string[];
  readonly filePath?: string;
  readonly outDegree: number;
  readonly inDegree: number;
}

export interface EdgeVm {
  readonly from: string;
  readonly to: string;
  readonly kind: string;
  readonly resolution: string;
  readonly provenance?: string;
  readonly otherTitle: string;
}

const ENTRY_KIND_LABELS: Record<string, string> = {
  HttpEndpoint: 'HTTP',
  MessageConsumer: 'Bus consumers',
  HostedService: 'Hosted services',
  ScheduledJob: 'Scheduled jobs',
  DomainEventHandler: 'Domain events',
  PublicApi: 'Public API',
};

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
    focus,
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
