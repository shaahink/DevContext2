import { Injectable, signal, inject } from '@angular/core';
import { SessionStore } from './session.store';
import { ToastService } from '../ui/toast/toast';
import { DevContextApi } from '../data-access/devcontext-api';
import { NodeResponse, NeighborsResponse, NeighborsResponseSchema } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { create } from '@bufbuild/protobuf';

@Injectable({ providedIn: 'root' })
export class NodeStore {
  private api = inject(DevContextApi);
  private session = inject(SessionStore);
  private toast = inject(ToastService);

  readonly open = signal(false);
  readonly nodeId = signal<string | null>(null);
  readonly node = signal<NodeResponse | null>(null);
  readonly neighbors = signal<NeighborsResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  async show(nodeId: string): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.nodeId.set(nodeId);
    this.open.set(true);
    this.loading.set(true);
    this.error.set(null);
    try {
      const [n, outNeigh, inNeigh] = await Promise.all([
        this.api.getNode(handle, nodeId),
        this.api.getNeighbors(handle, nodeId, 'out'),
        this.api.getNeighbors(handle, nodeId, 'in'),
      ]);
      this.node.set(n);
      this.neighbors.set(create(NeighborsResponseSchema, { edges: [...outNeigh.edges, ...inNeigh.edges] }));
    } catch {
      this.node.set(null);
      this.neighbors.set(null);
      this.error.set('Failed to load node details');
      this.toast.show('Failed to load node details', 'error');
    } finally {
      this.loading.set(false);
    }
  }

  hide(): void {
    this.open.set(false);
    this.nodeId.set(null);
    this.node.set(null);
    this.neighbors.set(null);
    this.error.set(null);
  }

  /** Exposed for NodeCard's Trace button. */
  sessionHandle(): string | null { return this.session.handle(); }
}
