import { computed, effect, inject, Injectable } from '@angular/core';

import { isStale, LatestGate } from '../core/rpc-call';
import { DevContextApi, type NeighborDirection } from '../data-access/devcontext-api';
import { toEdgeVm, toNodeDetailVm, toTraceVm, type TraceNodeVm } from '../models/view-models';
import { DEFAULT_TRACE_SLICE, type TraceDetail, WorkspaceStore } from './workspace.store';

/** DFS lookup used by `TraceStore.reroot` — a tree is rarely more than a few hundred
 * nodes (capped by trace depth), so a plain walk is plenty fast. */
function findNode(root: TraceNodeVm, nodeId: string): TraceNodeVm | null {
  if (root.id === nodeId) return root;
  for (const child of root.children) {
    const found = findNode(child, nodeId);
    if (found) return found;
  }
  return null;
}

export type { TraceDetail } from './workspace.store';

/**
 * Facade over the ACTIVE tab's trace slice in WorkspaceStore (I10). Public signal API is
 * unchanged from the pre-tabs version — components keep working without modification.
 *
 * Every public method captures its tabId once at the top and threads it through the whole async
 * chain, so a response that lands after the user has switched tabs still updates the tab that
 * asked for it, not whichever tab is active by the time it resolves.
 *
 * `trace()`/`selectNode()` go through a LatestGate (proposal §5.1, keyed `${tabId}:trace` /
 * `${tabId}:node`) so rapid re-triggers (j/k scrub) can never let a stale response paint over a
 * newer one, even if it resolves last.
 */
@Injectable({ providedIn: 'root' })
export class TraceStore {
  private readonly api = inject(DevContextApi);
  private readonly workspace = inject(WorkspaceStore);
  private readonly gate = new LatestGate();
  private liveTabIds = new Set<string>();

  private readonly activeTrace = computed(() => this.workspace.activeTab()?.trace ?? DEFAULT_TRACE_SLICE);

  constructor() {
    // Abort any in-flight trace/node RPCs for a tab the moment it closes — a response landing
    // after close would just be discarded state-wise, but there's no reason to let it finish
    // over the wire.
    effect(() => {
      const live = new Set(this.workspace.tabs().map((t) => t.id));
      for (const id of this.liveTabIds) {
        if (!live.has(id)) this.gate.cancelAll(`${id}:`);
      }
      this.liveTabIds = live;
    });
  }

  readonly focus = computed(() => this.activeTrace().focus);
  readonly depth = computed(() => this.activeTrace().depth);
  readonly detail = computed(() => this.activeTrace().detail);
  readonly error = computed(() => this.activeTrace().error);
  readonly loading = computed(() => this.activeTrace().loading);
  readonly found = computed(() => this.activeTrace().found);
  readonly tree = computed(() => this.activeTrace().tree);
  readonly markdown = computed(() => this.activeTrace().markdown);
  readonly touched = computed(() => this.activeTrace().touched);
  readonly emitted = computed(() => this.activeTrace().emitted);
  readonly selectedNodeId = computed(() => this.activeTrace().selectedNodeId);
  readonly nodeDetail = computed(() => this.activeTrace().nodeDetail);
  readonly neighbors = computed(() => this.activeTrace().neighbors);
  readonly neighborDirection = computed(() => this.activeTrace().neighborDirection);
  readonly active = computed(() => this.focus() !== null);

  async trace(handle: string, focus: string): Promise<void> {
    const tabId = this.workspace.activeId();
    if (!tabId) return;

    this.workspace.updateTrace(tabId, (s) => ({ ...DEFAULT_TRACE_SLICE, depth: s.depth, detail: s.detail, focus }));
    await this.run(tabId, handle);
  }

  async setDepth(depth: number): Promise<void> {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    const tab = this.workspace.tabById(tabId);
    this.workspace.updateTrace(tabId, (s) => ({ ...s, depth }));
    if (tab?.trace.focus && tab.session.handle) await this.run(tabId, tab.session.handle);
  }

  async setDetail(detail: TraceDetail): Promise<void> {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    const tab = this.workspace.tabById(tabId);
    this.workspace.updateTrace(tabId, (s) => ({ ...s, detail }));
    if (tab?.trace.focus && tab.session.handle) await this.run(tabId, tab.session.handle);
  }

  clear(): void {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    this.workspace.updateTrace(tabId, (s) => ({ ...DEFAULT_TRACE_SLICE, depth: s.depth, detail: s.detail }));
  }

  /** Double-click "re-trace from it" (proposal §2), done client-side with ZERO new RPC:
   * `GetTrace`'s `focus` only resolves registered entry-point keys (confirmed by hand —
   * an internal node id like `Member:Foo.<lambda>` comes back `found: false`), so a real
   * re-fetch from an arbitrary node isn't possible without an engine change. Instead this
   * finds `nodeId` inside the tree ALREADY loaded and re-roots the display at it — instant,
   * but depth is capped at whatever was already fetched relative to the original root.
   * Returns false (no-op) if the id isn't in the current tree. */
  reroot(nodeId: string): boolean {
    const tabId = this.workspace.activeId();
    if (!tabId) return false;
    const current = this.workspace.tabById(tabId)?.trace.tree;
    if (!current) return false;
    const sub = findNode(current, nodeId);
    if (!sub) return false;
    this.workspace.updateTrace(tabId, (s) => ({ ...s, tree: sub, selectedNodeId: null, nodeDetail: null, neighbors: [] }));
    return true;
  }

  async selectNode(nodeId: string, direction?: NeighborDirection): Promise<void> {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    const handle = this.workspace.tabById(tabId)?.session.handle;
    if (!handle) return;

    const dir = direction ?? this.workspace.tabById(tabId)?.trace.neighborDirection ?? 'out';
    this.workspace.updateTrace(tabId, (s) => ({ ...s, selectedNodeId: nodeId, neighborDirection: dir }));
    const res = await this.gate.run(`${tabId}:node`, async (signal) => {
      const [node, neighbors] = await Promise.all([
        this.api.getNode(handle, nodeId, signal),
        this.api.getNeighbors(handle, nodeId, dir, signal),
      ]);
      return { node, neighbors };
    });
    if (isStale(res)) return;
    this.workspace.updateTrace(tabId, (s) => ({
      ...s,
      nodeDetail: res.node.found ? toNodeDetailVm(res.node) : null,
      neighbors: res.neighbors.edges.map(toEdgeVm),
    }));
  }

  private async run(tabId: string, handle: string): Promise<void> {
    const focus = this.workspace.tabById(tabId)?.trace.focus;
    if (!focus) return;

    this.workspace.updateTrace(tabId, (s) => ({ ...s, loading: true, error: null }));
    const t = this.workspace.tabById(tabId)?.trace;
    const depth = t?.depth ?? DEFAULT_TRACE_SLICE.depth;
    const detail = t?.detail ?? DEFAULT_TRACE_SLICE.detail;
    try {
      const res = await this.gate.run(`${tabId}:trace`, (signal) => this.api.getTrace(handle, focus, depth, detail, signal));
      if (isStale(res)) return;
      this.workspace.updateTrace(tabId, (s) => ({
        ...s,
        found: res.found,
        tree: res.found && res.root ? toTraceVm(res.root) : null,
        markdown: res.markdown,
        touched: res.touchedEntities,
        emitted: res.emittedEvents,
        loading: false,
      }));
    } catch (err) {
      this.workspace.updateTrace(tabId, (s) => ({
        ...s,
        error: err instanceof Error ? err.message : 'Trace request failed',
        found: false,
        tree: null,
        loading: false,
      }));
    }
  }
}
