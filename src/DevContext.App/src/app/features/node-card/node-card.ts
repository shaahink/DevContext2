import { Component, computed, inject } from '@angular/core';
import { Sheet } from '../../ui/sheet/sheet';
import { NodeStore } from '../../state/node.store';
import { TraceStore } from '../../state/trace.store';
import { ToastService } from '../../ui/toast/toast';
import { NodeLink } from '../../ui/node-link/node-link';
import { Skeleton } from '../../ui/skeleton/skeleton';
import type { Edge } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { copyToClipboard } from '../../core/clipboard';
import { nodeIdLabel } from '../../core/format';
import { StudioHandoffStore } from '../../state/studio-handoff.store';
import { symbolCardSeeds } from '../context-studio/pack-proposal';

@Component({
  selector: 'app-node-card',
  standalone: true,
  imports: [Sheet, NodeLink, Skeleton],
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
          <!-- Content-preserving loading (proposal §5.2, GAP-B8): shapes of the sections
               below, not a spinner — first-load only, so the sheet never blanks. -->
          <div class="flex-1 overflow-y-auto p-3 space-y-3">
            <div class="space-y-1"><app-skeleton width="2.5rem" height="0.625rem" /><app-skeleton width="60%" /></div>
            <div class="space-y-1"><app-skeleton width="3.5rem" height="0.625rem" /><app-skeleton width="85%" /></div>
            <app-skeleton width="35%" />
            <div class="space-y-1 pt-1"><app-skeleton width="4rem" height="0.625rem" /><app-skeleton width="70%" /></div>
          </div>
        } @else if (store.error()) {
          <div class="flex-1 flex flex-col items-center justify-center gap-3 p-6">
            <span class="text-danger text-xs">{{ store.error() }}</span>
            <button class="rounded bg-surface-2 px-3 py-1.5 text-xs text-ink hover:bg-surface-1" (click)="retry()">Retry</button>
            <button class="text-2xs text-ink-subtle hover:text-ink" (click)="copyError()">Copy details</button>
          </div>
        } @else if (store.node(); as n) {
          @if (!n.found) {
            <!-- Trust principle (§1.4): never show fabricated field values for a node
                 the server couldn't actually resolve. -->
            <div class="flex-1 flex items-center justify-center p-6">
              <p class="text-xs text-ink-subtle">Node not found.</p>
            </div>
          } @else {
          <div class="flex-1 overflow-y-auto p-3 space-y-3">
            <div><span class="text-2xs text-ink-muted uppercase">Kind</span>
              <p class="text-sm text-ink">{{ n.kind }}</p></div>
            @if (n.filePath) {
              <div><span class="text-2xs text-ink-muted uppercase">Location</span>
                <p class="text-xs font-mono text-ink">{{ n.filePath }}</p></div>
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
              @if (incomingEdges().length) {
                <div><span class="text-2xs text-ink-muted uppercase">Called by</span>
                  @for (e of incomingEdges(); track e.from) {
                    <app-node-link class="block" [nodeId]="e.from" [label]="e.otherTitle || nodeLabel(e.from)" />
                  }
                </div>
              }
              @if (outgoingEdges().length) {
                <div><span class="text-2xs text-ink-muted uppercase">Calls</span>
                  @for (e of outgoingEdges(); track e.to) {
                    <app-node-link class="block" [nodeId]="e.to" [label]="e.otherTitle || nodeLabel(e.to)" />
                  }
                </div>
              }
              @if (!incomingEdges().length && !outgoingEdges().length) {
                <p class="text-xs text-ink-subtle py-2">No callers or callees.</p>
              }
            }
            @if (store.nodeId(); as nid) {
              <div class="flex gap-2 pt-2 border-t border-line">
                <button class="flex-1 rounded bg-accent text-accent-ink text-xs py-1.5"
                        (click)="traceFromNode(nid); store.hide()">Trace</button>
                <!-- N3.1 (audit §3.A) — the second thing a reader wants from a node they have just
                     understood: hand it to the agent. The card knew the symbol and had no way to
                     say so. -->
                <button class="flex-1 rounded bg-surface-2 text-ink text-xs py-1.5"
                        data-testid="node-card-send-to-studio"
                        title="Compose a context pack rooted at this symbol (flow, bodies, callers)"
                        (click)="sendToStudio(nid)">→ Studio</button>
                <button class="flex-1 rounded bg-surface-2 text-ink text-xs py-1.5"
                        (click)="copyId(nid)">Copy ID</button>
              </div>
            }
          </div>
          }
        }
      </div>
    </app-sheet>
  `,
})
export class NodeCard {
  readonly store = inject(NodeStore);
  readonly traceStore = inject(TraceStore);
  private readonly toast = inject(ToastService);
  private readonly studio = inject(StudioHandoffStore);

  /** R3 D-4 (G6.2) — the one id-to-text rule, for an edge whose otherTitle came back empty.
   * "Copy ID" still copies the RAW canonical id: that one is identity, not a name. */
  protected readonly nodeLabel = nodeIdLabel;

  readonly nid = computed(() => this.store.nodeId());
  readonly incomingEdges = computed(() => {
    const id = this.nid();
    if (!id) return [] as Edge[];
    return (this.store.neighbors()?.edges ?? []).filter(e => e.to === id);
  });
  readonly outgoingEdges = computed(() => {
    const id = this.nid();
    if (!id) return [] as Edge[];
    return (this.store.neighbors()?.edges ?? []).filter(e => e.from === id);
  });

  traceFromNode(nodeId: string): void {
    const h = this.store.sessionHandle();
    if (h) this.traceStore.trace(h, nodeId);
  }

  /** N3.1 — the node id goes over as the card's entry id verbatim. MEASURED 2026-08-14 in
   * `ContextPackBuilder.ResolveCardFocuses` / `NormalizeSymbolFocus`: the builder strips the
   * NodeKind prefix and resolves the rest through the same `ResolveEntry` path `get_context` uses,
   * so `Type:Acme.OrderService` and `Member:Acme.OrderService::Handle` are both legal here. There is
   * nothing for this component to look up. */
  sendToStudio(nodeId: string): void {
    const label = this.store.node()?.title || nodeIdLabel(nodeId);
    this.store.hide();
    void this.studio.open({ seeds: symbolCardSeeds(nodeId, label), source: `the node “${label}”` })
      .then((ok) => this.toast.show(
        ok ? `Sent ${label} to Context Studio` : 'Could not open Context Studio',
        ok ? 'success' : 'error',
      ));
  }

  copyId(id: string): void {
    void copyToClipboard(id)
      .then(() => this.toast.show('Node ID copied', 'info'))
      .catch(() => this.toast.show('Copy failed', 'error'));
  }

  retry(): void {
    const nid = this.store.nodeId();
    if (nid) void this.store.show(nid);
  }

  copyError(): void {
    const err = this.store.error();
    if (err) {
      void copyToClipboard(err)
        .then(() => this.toast.show('Error copied', 'info'))
        .catch(() => this.toast.show('Copy failed', 'error'));
    }
  }
}
