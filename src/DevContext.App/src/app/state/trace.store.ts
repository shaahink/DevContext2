import { computed, inject, Injectable, signal } from '@angular/core';

import { DevContextApi } from '../data-access/devcontext-api';
import {
  type EdgeVm,
  type NodeDetailVm,
  toEdgeVm,
  toNodeDetailVm,
  type TraceNodeVm,
  toTraceVm,
} from '../models/view-models';

export type TraceDetail = 'signature' | 'salient' | 'full';

/**
 * The trace + node-browse store. Holds the current focus/depth/detail and the resulting trace tree,
 * plus the selected node's detail and neighbours. Every dial change is a render-time re-query against
 * the same analyzed snapshot — never a re-analysis.
 */
@Injectable({ providedIn: 'root' })
export class TraceStore {
  private readonly api = inject(DevContextApi);
  private handle: string | null = null;

  private readonly _focus = signal<string | null>(null);
  private readonly _depth = signal(6);
  private readonly _detail = signal<TraceDetail>('salient');
  private readonly _error = signal<string | null>(null);
  private readonly _loading = signal(false);
  private readonly _found = signal(true);
  private readonly _tree = signal<TraceNodeVm | null>(null);
  private readonly _markdown = signal('');
  private readonly _touched = signal<readonly string[]>([]);
  private readonly _emitted = signal<readonly string[]>([]);
  private readonly _selectedNodeId = signal<string | null>(null);
  private readonly _nodeDetail = signal<NodeDetailVm | null>(null);
  private readonly _neighbors = signal<readonly EdgeVm[]>([]);

  readonly focus = this._focus.asReadonly();
  readonly depth = this._depth.asReadonly();
  readonly detail = this._detail.asReadonly();
  readonly error = this._error.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly found = this._found.asReadonly();
  readonly tree = this._tree.asReadonly();
  readonly markdown = this._markdown.asReadonly();
  readonly touched = this._touched.asReadonly();
  readonly emitted = this._emitted.asReadonly();
  readonly selectedNodeId = this._selectedNodeId.asReadonly();
  readonly nodeDetail = this._nodeDetail.asReadonly();
  readonly neighbors = this._neighbors.asReadonly();
  readonly active = computed(() => this._focus() !== null);

  async trace(handle: string, focus: string): Promise<void> {
    this.handle = handle;
    this._focus.set(focus);
    this._selectedNodeId.set(null);
    this._nodeDetail.set(null);
    this._neighbors.set([]);
    await this.run();
  }

  async setDepth(depth: number): Promise<void> {
    this._depth.set(depth);
    if (this._focus()) await this.run();
  }

  async setDetail(detail: TraceDetail): Promise<void> {
    this._detail.set(detail);
    if (this._focus()) await this.run();
  }

  clear(): void {
    this._focus.set(null);
    this._tree.set(null);
    this._markdown.set('');
    this._touched.set([]);
    this._emitted.set([]);
    this._selectedNodeId.set(null);
    this._nodeDetail.set(null);
    this._neighbors.set([]);
    this._error.set(null);
  }

  async selectNode(nodeId: string): Promise<void> {
    if (!this.handle) return;
    this._selectedNodeId.set(nodeId);
    const [node, neighbors] = await Promise.all([
      this.api.getNode(this.handle, nodeId),
      this.api.getNeighbors(this.handle, nodeId, 'out'),
    ]);
    this._nodeDetail.set(node.found ? toNodeDetailVm(node) : null);
    this._neighbors.set(neighbors.edges.map(toEdgeVm));
  }

  private async run(): Promise<void> {
    const handle = this.handle;
    const focus = this._focus();
    if (!handle || !focus) return;

    this._loading.set(true);
    this._error.set(null);
    try {
      const res = await this.api.getTrace(handle, focus, this._depth(), this._detail());
      this._found.set(res.found);
      this._tree.set(res.found && res.root ? toTraceVm(res.root) : null);
      this._markdown.set(res.markdown);
      this._touched.set(res.touchedEntities);
      this._emitted.set(res.emittedEvents);
    } catch (err) {
      this._error.set(err instanceof Error ? err.message : 'Trace request failed');
      this._found.set(false);
      this._tree.set(null);
    } finally {
      this._loading.set(false);
    }
  }
}
