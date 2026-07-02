import { computed, inject, Injectable } from '@angular/core';

import { DevContextApi } from '../data-access/devcontext-api';
import { toEdgeVm, toNodeDetailVm, toTraceVm } from '../models/view-models';
import { DEFAULT_TRACE_SLICE, type TraceDetail, WorkspaceStore } from './workspace.store';

export type { TraceDetail } from './workspace.store';

/**
 * Facade over the ACTIVE tab's trace slice in WorkspaceStore (I10). Public signal API is
 * unchanged from the pre-tabs version — components keep working without modification.
 *
 * Every public method captures its tabId once at the top and threads it through the whole async
 * chain, so a response that lands after the user has switched tabs still updates the tab that
 * asked for it, not whichever tab is active by the time it resolves.
 */
@Injectable({ providedIn: 'root' })
export class TraceStore {
  private readonly api = inject(DevContextApi);
  private readonly workspace = inject(WorkspaceStore);

  private readonly activeTrace = computed(() => this.workspace.activeTab()?.trace ?? DEFAULT_TRACE_SLICE);

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

  async selectNode(nodeId: string): Promise<void> {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    const handle = this.workspace.tabById(tabId)?.session.handle;
    if (!handle) return;

    this.workspace.updateTrace(tabId, (s) => ({ ...s, selectedNodeId: nodeId }));
    const [node, neighbors] = await Promise.all([
      this.api.getNode(handle, nodeId),
      this.api.getNeighbors(handle, nodeId, 'out'),
    ]);
    this.workspace.updateTrace(tabId, (s) => ({
      ...s,
      nodeDetail: node.found ? toNodeDetailVm(node) : null,
      neighbors: neighbors.edges.map(toEdgeVm),
    }));
  }

  private async run(tabId: string, handle: string): Promise<void> {
    const focus = this.workspace.tabById(tabId)?.trace.focus;
    if (!focus) return;

    this.workspace.updateTrace(tabId, (s) => ({ ...s, loading: true, error: null }));
    try {
      const t = this.workspace.tabById(tabId)?.trace;
      const res = await this.api.getTrace(handle, focus, t?.depth ?? DEFAULT_TRACE_SLICE.depth, t?.detail ?? DEFAULT_TRACE_SLICE.detail);
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
