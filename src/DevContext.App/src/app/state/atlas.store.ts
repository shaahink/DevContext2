import { computed, effect, inject, Injectable, signal } from '@angular/core';

import { DevContextApi } from '../data-access/devcontext-api';
import { type EntryVm, type TraceNodeVm, toTraceVm } from '../models/view-models';
import { WorkspaceStore } from './workspace.store';

/** Per-entry flow statistics — everything derivable from one shallow trace (§3.1). */
export interface FlowStat {
  readonly focus: string;
  readonly title: string;
  readonly kind: string;
  readonly found: boolean;
  readonly nodeCount: number;
  readonly maxDepth: number;
  /** Count of nodes reached across a process/messaging boundary (send/consumes/raises/handler seams). */
  readonly boundaryCrossings: number;
  /** Count of `data` seam nodes on the flow. */
  readonly dataTouches: number;
  /** % of nodes with resolution === 'Semantic' (Roslyn-verified). */
  readonly verifiedPct: number;
  readonly touchedEntities: readonly string[];
  readonly emittedEvents: readonly string[];
  /** Every nodeId on the flow — feeds reachedBy(). */
  readonly nodeIds: readonly string[];
  /** nodeIds minus data-seam nodes — feeds hubs(). A DbContext and its EF members sit at
   * the end of most flows on data-heavy repos (shamshir: 7 of 10 hub rows were
   * TradingDbContext.*, T6.0 S1.4) — they're plumbing every flow shares, not the
   * orchestration hubs the radar is for. */
  readonly hubIds: readonly string[];
  /** Importance ranking: breadth × (1 + boundary crossings). */
  readonly score: number;
}

export interface EventWire {
  readonly event: string;
  readonly publisherFocus: string;
  readonly publisherTitle: string;
  /** null = orphan event: emitted but no consumer entry matched (still worth showing). */
  readonly consumerFocus: string | null;
  readonly consumerTitle: string | null;
  /** True only for the legacy client-side name-match join (facet-less sessions) — the
   * server's T2.6 projection rows are the real graph join, not an approximation. */
  readonly approx: boolean;
  readonly crossService: boolean;
}

export interface HubStat {
  readonly nodeId: string;
  readonly title: string;
  /** In how many distinct indexed flows this node appears. */
  readonly flowCount: number;
}

export interface NodeDegree {
  readonly inDegree: number;
  readonly outDegree: number;
}

export type AtlasStatus = 'idle' | 'indexing' | 'paused' | 'done' | 'cancelled';

interface AtlasSlice {
  readonly flows: Readonly<Record<string, FlowStat>>;
  readonly status: AtlasStatus;
  readonly indexed: number;
  readonly total: number;
}

interface IndexerControl {
  cancelled: boolean;
  paused: boolean;
  waiters: (() => void)[];
}

const EMPTY_SLICE: AtlasSlice = { flows: {}, status: 'idle', indexed: 0, total: 0 };
const CONCURRENCY = 4;
const MAX_FLOWS = 100;
const INDEX_DEPTH = 3;
const BOUNDARY_SEAMS = new Set(['send', 'consumes', 'raises', 'handler']);
const CONSUMER_KINDS = new Set(['MessageConsumer', 'DomainEventHandler']);

/**
 * Flow Atlas (proposal §3.1) — background shallow-traces every entry point of a tab's
 * snapshot into FlowStats, enabling Top Flows (§3.2), the Event Wiring Board (§3.3),
 * the impact lens (§3.4), confidence (§3.5) and Hub Radar (§3.7) with ZERO engine
 * changes: it is just N stateless getTrace calls against the immutable snapshot.
 *
 * Cooperative and cancellable by design: pause() parks the workers between RPCs
 * (call it while a user-initiated trace is in flight — user latency wins), resume()
 * releases them, and closing the tab cancels outright (slices self-GC).
 *
 * Facade signals reflect the ACTIVE tab, same pattern as SessionStore/TraceStore.
 */
@Injectable({ providedIn: 'root' })
export class AtlasStore {
  private readonly api = inject(DevContextApi);
  private readonly workspace = inject(WorkspaceStore);

  private readonly _slices = signal<ReadonlyMap<string, AtlasSlice>>(new Map());
  /** Imperative control blocks — deliberately NOT signals (workers mutate them). */
  private readonly controls = new Map<string, IndexerControl>();
  /** §3.7 degree enrichment — best-effort `getNode` cache, keyed by node id (node ids
   * are effectively unique per analyzed repo, so no per-tab scoping needed here). */
  private readonly degreeCache = signal<ReadonlyMap<string, NodeDegree>>(new Map());

  private readonly active = computed(
    () => this._slices().get(this.workspace.activeId() ?? '') ?? EMPTY_SLICE,
  );

  readonly status = computed(() => this.active().status);
  readonly indexed = computed(() => this.active().indexed);
  readonly total = computed(() => this.active().total);
  readonly running = computed(() => this.status() === 'indexing' || this.status() === 'paused');
  /** Statusbar segment text. Empty when idle/done. T6.8 (audit B6): the old "atlas 42/94"
   * read as a mysterious SCORE that "changed between pages" — it's indexing progress; say so. */
  readonly progressLabel = computed(() =>
    this.running() ? `indexing flows ${this.indexed()}/${this.total()}` : '',
  );

  readonly flows = computed(() => Object.values(this.active().flows));

  /** §3.2 — importance-ranked flows for the Home digest. */
  readonly topFlows = computed(() =>
    this.flows()
      .filter((f) => f.found)
      .sort((a, b) => b.score - a.score)
      .slice(0, 10),
  );

  /** §3.5 — repo-wide confidence over indexed flows (node-weighted). */
  readonly overallVerifiedPct = computed(() => {
    const flows = this.flows().filter((f) => f.found && f.nodeCount > 0);
    const nodes = flows.reduce((n, f) => n + f.nodeCount, 0);
    if (nodes === 0) return null;
    const verified = flows.reduce((n, f) => n + (f.verifiedPct / 100) * f.nodeCount, 0);
    return Math.round((verified / nodes) * 100);
  });

  /** §3.7 — nodes appearing in the most distinct flows ("everything passes through X").
   * Appearance frequency only; degree enrichment via getNode is a W5 wiring TODO. */
  readonly hubs = computed<readonly HubStat[]>(() => {
    const counts = new Map<string, number>();
    for (const flow of this.flows()) {
      for (const id of new Set(flow.hubIds)) counts.set(id, (counts.get(id) ?? 0) + 1);
    }
    return [...counts.entries()]
      .filter(([, n]) => n > 1)
      .sort((a, b) => b[1] - a[1])
      .slice(0, 10)
      .map(([nodeId, flowCount]) => ({ nodeId, title: shortTitle(nodeId), flowCount }));
  });

  /** §3.7 — `hubs()` enriched with real in/out-degree, once `getNode` resolves (see the
   * enrichment effect below). `degree` is null until then — render without it, don't wait. */
  readonly hubsWithDegree = computed<readonly (HubStat & { readonly degree: NodeDegree | null })[]>(() =>
    this.hubs().map((h) => ({ ...h, degree: this.degreeCache().get(h.nodeId) ?? null })),
  );

  /** §3.3 — the event board's rows. T6.11: prefer the server's ONE T2.6 join (the
   * EventWiring facet — publisher→event→consumer from real Raises/Consumes edges); the
   * heuristic name-match join survives only for facet-less sessions and stays marked
   * [approx]. */
  readonly eventWiring = computed<readonly EventWire[]>(() => {
    const facet = this.workspace.activeTab()?.session.graphFacets?.eventWiring;
    if (facet && facet.wires.length > 0) {
      const wires: EventWire[] = [];
      for (const w of facet.wires) {
        const pub = w.publishers[0];
        if (!pub) continue; // consumer-only rows have no board anchor
        if (w.consumers.length === 0) {
          wires.push({ event: w.eventName, publisherFocus: pub.nodeId, publisherTitle: pub.title,
            consumerFocus: null, consumerTitle: null, approx: false, crossService: w.isCrossService });
        } else {
          for (const c of w.consumers) {
            wires.push({ event: w.eventName, publisherFocus: pub.nodeId, publisherTitle: pub.title,
              consumerFocus: c.nodeId, consumerTitle: c.title, approx: false, crossService: w.isCrossService });
          }
        }
      }
      return wires;
    }

    const groups = this.workspace.activeTab()?.session.entryGroups ?? [];
    const consumers = groups
      .filter((g) => CONSUMER_KINDS.has(g.kind))
      .flatMap((g) => g.entries);

    const wires: EventWire[] = [];
    for (const flow of this.flows()) {
      for (const event of flow.emittedEvents) {
        const needle = normalizeEventName(event);
        const match =
          needle.length >= 4
            ? consumers.find((c) => normalizeEventName(c.title).includes(needle))
            : undefined;
        wires.push({
          event,
          publisherFocus: flow.focus,
          publisherTitle: flow.title,
          consumerFocus: match?.focus ?? null,
          consumerTitle: match?.title ?? null,
          approx: true,
          crossService: false,
        });
      }
    }
    return wires;
  });

  constructor() {
    // GC + cancel indexers for closed tabs.
    effect(() => {
      const live = new Set(this.workspace.tabs().map((t) => t.id));
      for (const tabId of [...this.controls.keys()]) {
        if (!live.has(tabId)) this.cancelTab(tabId);
      }
      const slices = this._slices();
      if ([...slices.keys()].some((id) => !live.has(id))) {
        this._slices.set(new Map([...slices].filter(([id]) => live.has(id))));
      }
    });

    // §3.7 degree enrichment — batch (best-effort, unbounded concurrency: hubs() is
    // already capped to 10) getNode over hub node ids not yet cached. Re-runs
    // automatically as hubs() changes (more flows indexed → different top-10).
    effect(() => {
      const handle = this.workspace.activeTab()?.session.handle;
      const hubList = this.hubs();
      if (!handle) return;
      const cache = this.degreeCache();
      for (const h of hubList) {
        if (cache.has(h.nodeId)) continue;
        void this.api
          .getNode(handle, h.nodeId)
          .then((res) => {
            if (!res.found) return;
            this.degreeCache.update((m) => new Map(m).set(h.nodeId, { inDegree: res.inDegree, outDegree: res.outDegree }));
          })
          .catch(() => { /* best-effort enrichment, silent failure OK */ });
      }
    });
  }

  /** §3.4 — impact lens: which indexed entry flows reach this node. */
  reachedBy(nodeId: string): readonly FlowStat[] {
    return this.flows().filter((f) => f.nodeIds.includes(nodeId));
  }

  /** Starts (or restarts) indexing a tab's entries. Captures tabId/handle once —
   * results always land in the tab that asked, never the currently active one. */
  start(tabId: string, handle: string, entries: readonly EntryVm[]): void {
    this.cancelTab(tabId);

    const seen = new Set<string>();
    const queue: EntryVm[] = [];
    for (const e of entries) {
      if (e.focus && !seen.has(e.focus)) {
        seen.add(e.focus);
        queue.push(e);
      }
      if (queue.length >= MAX_FLOWS) break;
    }
    if (queue.length === 0) return;

    const control: IndexerControl = { cancelled: false, paused: false, waiters: [] };
    this.controls.set(tabId, control);
    this.update(tabId, () => ({ flows: {}, status: 'indexing', indexed: 0, total: queue.length }));

    const workers = Array.from({ length: Math.min(CONCURRENCY, queue.length) }, () =>
      this.worker(tabId, handle, queue, control),
    );
    void Promise.all(workers).then(() => {
      if (this.controls.get(tabId) !== control) return; // superseded by a restart
      this.controls.delete(tabId);
      if (!control.cancelled) this.update(tabId, (s) => ({ ...s, status: 'done' }));
    });
  }

  /** Parks workers between RPCs. Call while a user trace is in flight. */
  pause(tabId = this.workspace.activeId() ?? ''): void {
    const control = this.controls.get(tabId);
    if (!control || control.cancelled || control.paused) return;
    control.paused = true;
    this.update(tabId, (s) => (s.status === 'indexing' ? { ...s, status: 'paused' } : s));
  }

  resume(tabId = this.workspace.activeId() ?? ''): void {
    const control = this.controls.get(tabId);
    if (!control || control.cancelled || !control.paused) return;
    control.paused = false;
    this.update(tabId, (s) => (s.status === 'paused' ? { ...s, status: 'indexing' } : s));
    control.waiters.splice(0).forEach((release) => release());
  }

  cancelTab(tabId: string): void {
    const control = this.controls.get(tabId);
    if (!control) return;
    control.cancelled = true;
    control.waiters.splice(0).forEach((release) => release());
    this.controls.delete(tabId);
    this.update(tabId, (s) => (s.status === 'done' ? s : { ...s, status: 'cancelled' }));
  }

  private async worker(
    tabId: string,
    handle: string,
    queue: EntryVm[],
    control: IndexerControl,
  ): Promise<void> {
    while (!control.cancelled) {
      if (control.paused) {
        await new Promise<void>((release) => control.waiters.push(release));
        continue;
      }
      const entry = queue.shift();
      if (!entry) return;

      let stat: FlowStat;
      try {
        const res = await this.api.getTrace(handle, entry.focus, INDEX_DEPTH, 'salient');
        if (control.cancelled) return;
        stat =
          res.found && res.root
            ? computeFlowStat(entry, toTraceVm(res.root), res.touchedEntities, res.emittedEvents)
            : emptyFlowStat(entry, false);
      } catch {
        if (control.cancelled) return;
        stat = emptyFlowStat(entry, false);
      }
      this.update(tabId, (s) => ({
        ...s,
        flows: { ...s.flows, [entry.focus]: stat },
        indexed: s.indexed + 1,
      }));
    }
  }

  private update(tabId: string, fn: (s: AtlasSlice) => AtlasSlice): void {
    this._slices.update((map) => {
      const prev = map.get(tabId) ?? EMPTY_SLICE;
      const next = fn(prev);
      if (next === prev) return map;
      return new Map(map).set(tabId, next);
    });
  }
}

function computeFlowStat(
  entry: EntryVm,
  root: TraceNodeVm,
  touched: readonly string[],
  emitted: readonly string[],
): FlowStat {
  let nodeCount = 0;
  let maxDepth = 0;
  let boundaryCrossings = 0;
  let dataTouches = 0;
  let verified = 0;
  const nodeIds: string[] = [];
  const hubIds: string[] = [];

  const stack: TraceNodeVm[] = [root];
  while (stack.length > 0) {
    const node = stack.pop()!;
    nodeCount++;
    nodeIds.push(node.id);
    if (node.depth > maxDepth) maxDepth = node.depth;
    const seam = (node.seam ?? '').toLowerCase();
    if (BOUNDARY_SEAMS.has(seam)) boundaryCrossings++;
    if (seam === 'data') dataTouches++;
    else hubIds.push(node.id);
    if (node.resolution === 'Semantic') verified++;
    for (const child of node.children) stack.push(child);
  }

  return {
    focus: entry.focus,
    title: entry.title,
    kind: entry.kind,
    found: true,
    nodeCount,
    maxDepth,
    boundaryCrossings,
    dataTouches,
    verifiedPct: nodeCount > 0 ? Math.round((verified / nodeCount) * 100) : 0,
    touchedEntities: touched,
    emittedEvents: emitted,
    nodeIds,
    hubIds,
    score: nodeCount * (1 + boundaryCrossings),
  };
}

function emptyFlowStat(entry: EntryVm, found: boolean): FlowStat {
  return {
    focus: entry.focus,
    title: entry.title,
    kind: entry.kind,
    found,
    nodeCount: 0,
    maxDepth: 0,
    boundaryCrossings: 0,
    dataTouches: 0,
    verifiedPct: 0,
    touchedEntities: [],
    emittedEvents: [],
    nodeIds: [],
    hubIds: [],
    score: 0,
  };
}

/** "MyApp.Orders.OrderCreatedEvent" → "ordercreated" (suffix-stripped, lowercased). */
function normalizeEventName(name: string): string {
  const last = name.split(/[./:]/).pop() ?? name;
  return last.toLowerCase().replace(/[^a-z0-9]/g, '').replace(/(event|handler|consumer)$/g, '');
}

function shortTitle(nodeId: string): string {
  const parts = nodeId.split(/[./:]/).filter(Boolean);
  return parts.length > 1 ? parts.slice(-2).join('.') : nodeId;
}
