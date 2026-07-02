import { Component, inject } from '@angular/core';
import { Sheet } from '../../ui/sheet/sheet';
import { NodeStore } from '../../state/node.store';
import { TraceStore } from '../../state/trace.store';
import { NodeLink } from '../../ui/node-link/node-link';

@Component({
  selector: 'app-node-card',
  standalone: true,
  imports: [Sheet, NodeLink],
  template: `
    <app-sheet [open]="store.open()" (closed)="store.hide()">
      <div class="flex flex-col h-full">
        <div class="flex items-center justify-between border-b border-line p-3">
          <h3 class="text-sm font-semibold text-ink truncate" [title]="store.node()?.title ?? ''">
            {{ store.node()?.title ?? 'Node' }}
          </h3>
          <button class="text-ink-muted hover:text-ink text-xs px-1" (click)="store.hide()">✕</button>
        </div>
        @if (store.loading()) {
          <div class="flex-1 flex items-center justify-center text-ink-muted text-sm">Loading...</div>
        } @else if (store.node(); as n) {
          <div class="flex-1 overflow-y-auto p-3 space-y-3">
            <div><span class="text-2xs text-ink-muted uppercase">Kind</span>
              <p class="text-sm text-ink">{{ n.kind }}</p></div>
            @if (n.filePath) {
              <div><span class="text-2xs text-ink-muted uppercase">Location</span>
                <p class="text-xs font-mono text-ink">{{ n.filePath }}{{ n.line ? ':' + n.line : '' }}</p></div>
            }
            @if (n.tags?.length) {
              <div><span class="text-2xs text-ink-muted uppercase">Tags</span>
                <div class="flex flex-wrap gap-1 mt-1">
                  @for (t of n.tags; track t) {
                    <span class="rounded bg-surface-2 px-1.5 py-0.5 text-2xs text-ink-muted">{{ t }}</span>
                  }
                </div></div>
            }
            <div class="flex gap-3"><span class="text-xs text-ink-muted">In: {{ n.inDegree ?? 0 }}</span>
              <span class="text-xs text-ink-muted">Out: {{ n.outDegree ?? 0 }}</span></div>

            @if (store.neighbors(); as neigh) {
              @if (neigh.incoming?.length) {
                <div><span class="text-2xs text-ink-muted uppercase">Called by</span>
                  @for (e of neigh.incoming; track e.from) {
                    <app-node-link class="block" [nodeId]="e.from" [label]="e.otherTitle || e.from" />
                  }
                </div>
              }
              @if (neigh.outgoing?.length) {
                <div><span class="text-2xs text-ink-muted uppercase">Calls</span>
                  @for (e of neigh.outgoing; track e.to) {
                    <app-node-link class="block" [nodeId]="e.to" [label]="e.otherTitle || e.to" />
                  }
                </div>
              }
            }
            @if (store.nodeId(); as nid) {
              <div class="flex gap-2 pt-2 border-t border-line">
                <button class="flex-1 rounded bg-accent text-accent-ink text-xs py-1.5"
                        (click)="traceFromNode(nid); store.hide()">Trace</button>
                <button class="flex-1 rounded bg-surface-2 text-ink text-xs py-1.5"
                        (click)="copyId(nid)">Copy ID</button>
              </div>
            }
          </div>
        }
      </div>
    </app-sheet>
  `,
})
export class NodeCard {
  readonly store = inject(NodeStore);
  readonly traceStore = inject(TraceStore);

  traceFromNode(nodeId: string): void {
    const h = this.store.sessionHandle();
    if (h) this.traceStore.trace(h, nodeId);
  }

  copyId(id: string): void {
    navigator.clipboard?.writeText(id);
  }
}
