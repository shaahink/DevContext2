import { Injectable, signal, inject } from '@angular/core';
import { SessionStore } from './session.store';
import { DevContextApi } from '../data-access/devcontext-api';
import { NodeResponse, NeighborsResponse } from '../core/grpc/gen/devcontext/v1/devcontext_pb';

@Injectable({ providedIn: 'root' })
export class NodeStore {
  private api = inject(DevContextApi);
  private session = inject(SessionStore);

  readonly open = signal(false);
  readonly nodeId = signal<string | null>(null);
  readonly node = signal<NodeResponse | null>(null);
  readonly neighbors = signal<NeighborsResponse | null>(null);
  readonly loading = signal(false);

  async show(nodeId: string): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.nodeId.set(nodeId);
    this.open.set(true);
    this.loading.set(true);
    try {
      const [n, neigh] = await Promise.all([
        this.api.getNode(handle, nodeId),
        this.api.getNeighbors(handle, nodeId, 'both'),
      ]);
      this.node.set(n);
      this.neighbors.set(neigh);
    } catch {
      this.node.set(null);
      this.neighbors.set(null);
    } finally {
      this.loading.set(false);
    }
  }

  hide(): void {
    this.open.set(false);
    this.nodeId.set(null);
    this.node.set(null);
    this.neighbors.set(null);
  }

  /** Exposed for NodeCard's Trace button. */
  sessionHandle(): string | null { return this.session.handle(); }
}
