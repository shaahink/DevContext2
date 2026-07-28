import { computed, effect, inject, Injectable, signal } from '@angular/core';

import { DevContextApi } from '../data-access/devcontext-api';
import { rankFlows } from '../core/flow-ranking';
import { type EntryVm } from '../models/view-models';
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

const EMPTY_SLICE: AtlasSlice = { flows: {}, status: 'idle', indexed: 0, total: 0 };
const CONSUMER_KINDS = new Set(['MessageConsumer', 'DomainEventHandler']);

/**
 * Flow Atlas (proposal §3.1) — per-entry FlowStats enabling Top Flows (§3.2), the Event
 * Wiring Board (§3.3), the impact lens (§3.4), confidence (§3.5) and Hub Radar (§3.7).
 *
 * T7.4 (audit B11): the stats come from ONE `getFlowIndex` RPC, computed and memoized
 * server-side per session. The old client indexer background-fired up to 100 `getTrace`
 * calls plus ~10 `getNode` degree lookups — re-run on EVERY app boot/reattach — which is
 * exactly the "~150 RPCs in ~2s" storm the audit flagged. Hub degrees ride the same
 * response. (The server also fixes boundary-seam counting: the old client set compared
 * 'consumes'/'raises'/'handler' against the wire's 'consume'/'raise'/'handle', so only
 * send-hops ever counted toward flow scores.)
 *
 * Facade signals reflect the ACTIVE tab, same pattern as SessionStore/TraceStore.
 */
@Injectable({ providedIn: 'root' })
export class AtlasStore {
  private readonly api = inject(DevContextApi);
  private readonly workspace = inject(WorkspaceStore);

  private readonly _slices = signal<ReadonlyMap<string, AtlasSlice>>(new Map());
  /** In-flight index fetch per tab — aborted on tab close / restart. */
  private readonly inflight = new Map<string, AbortController>();
  /** §3.7 hub degrees, keyed by node id — seeded from the flow-index response (node ids
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
  /** R3 D-E (E-2): the same ranking Home's Top flows and the START HERE tile use. Sorting on the
   * flow score alone put five internal `*DomainEventHandler`s at the top of eShop's Atlas while
   * Home's list of the same name showed HTTP endpoints — two sections, one name, two answers. */
  readonly topFlows = computed(() =>
    rankFlows(this.flows().filter((f) => f.found)).slice(0, 10),
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
    // GC + cancel in-flight index fetches for closed tabs. (Hub degrees ride the
    // flow-index response — the old per-hub getNode enrichment effect is gone, T7.4.)
    effect(() => {
      const live = new Set(this.workspace.tabs().map((t) => t.id));
      for (const tabId of [...this.inflight.keys()]) {
        if (!live.has(tabId)) this.cancelTab(tabId);
      }
      const slices = this._slices();
      if ([...slices.keys()].some((id) => !live.has(id))) {
        this._slices.set(new Map([...slices].filter(([id]) => live.has(id))));
      }
    });
  }

  /** §3.4 — impact lens: which indexed entry flows reach this node. */
  reachedBy(nodeId: string): readonly FlowStat[] {
    return this.flows().filter((f) => f.nodeIds.includes(nodeId));
  }

  /** Fetches (or refetches) the tab's flow index — ONE memoized server call. Captures
   * tabId/handle once — results always land in the tab that asked, never the currently
   * active one. The entries param sizes the optimistic progress total only; the server
   * derives the flows from its own entry inventory. */
  start(tabId: string, handle: string, entries: readonly EntryVm[]): void {
    this.cancelTab(tabId);
    if (entries.length === 0) return;

    const controller = new AbortController();
    this.inflight.set(tabId, controller);
    this.update(tabId, () => ({
      flows: {},
      status: 'indexing',
      indexed: 0,
      total: Math.min(entries.length, 100),
    }));

    void this.api
      .getFlowIndex(handle, controller.signal)
      .then((res) => {
        if (this.inflight.get(tabId) !== controller) return; // superseded / cancelled
        this.inflight.delete(tabId);

        const flows: Record<string, FlowStat> = {};
        for (const f of res.flows) {
          flows[f.focus] = {
            focus: f.focus,
            title: f.title,
            kind: f.kind,
            found: f.found,
            nodeCount: f.nodeCount,
            maxDepth: f.maxDepth,
            boundaryCrossings: f.boundaryCrossings,
            dataTouches: f.dataTouches,
            verifiedPct: f.verifiedPct,
            touchedEntities: f.touchedEntities,
            emittedEvents: f.emittedEvents,
            nodeIds: f.nodeIds,
            hubIds: f.hubIds,
            score: f.score,
          };
        }
        this.update(tabId, () => ({
          flows,
          status: 'done',
          indexed: res.flows.length,
          total: res.flows.length,
        }));

        if (res.hubDegrees.length > 0) {
          this.degreeCache.update((m) => {
            const next = new Map(m);
            for (const h of res.hubDegrees)
              next.set(h.nodeId, { inDegree: h.inDegree, outDegree: h.outDegree });
            return next;
          });
        }
      })
      .catch(() => {
        if (this.inflight.get(tabId) !== controller) return;
        this.inflight.delete(tabId);
        this.update(tabId, (s) => (s.status === 'done' ? s : { ...s, status: 'cancelled' }));
      });
  }

  cancelTab(tabId: string): void {
    const controller = this.inflight.get(tabId);
    if (!controller) return;
    this.inflight.delete(tabId);
    controller.abort();
    this.update(tabId, (s) => (s.status === 'done' ? s : { ...s, status: 'cancelled' }));
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

/** "MyApp.Orders.OrderCreatedEvent" → "ordercreated" (suffix-stripped, lowercased). */
function normalizeEventName(name: string): string {
  const last = name.split(/[./:]/).pop() ?? name;
  return last.toLowerCase().replace(/[^a-z0-9]/g, '').replace(/(event|handler|consumer)$/g, '');
}

function shortTitle(nodeId: string): string {
  const parts = nodeId.split(/[./:]/).filter(Boolean);
  return parts.length > 1 ? parts.slice(-2).join('.') : nodeId;
}
