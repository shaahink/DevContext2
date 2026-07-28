import { Component, computed, input, output, signal } from '@angular/core';

import { groupServiceHops, isServiceHopGroup, type TraceNodeVm } from '../../models/view-models';
import { Badge } from '../../ui/badge/badge';
import { NodeLink } from '../../ui/node-link/node-link';
import { SeamChip } from '../../ui/seam-chip/seam-chip';

@Component({
  selector: 'app-trace-node',
  imports: [Badge, NodeLink, SeamChip],
  template: `
    <div class="border-l-2 border-line pl-3 py-1">
      <div class="flex items-start gap-2 group cursor-pointer hover:bg-surface-2 rounded -ml-0.5 px-0.5 transition-colors" (click)="nodeSelected.emit(node().id)" (keydown.enter)="nodeSelected.emit(node().id)" (keydown.space)="nodeSelected.emit(node().id); $event.preventDefault()" tabindex="0" role="button">
        <app-seam-chip [seam]="node().seam" class="shrink-0" />
        <div class="min-w-0">
          <app-node-link [nodeId]="node().id" [label]="node().title" />
          @if (node().salient) { <p class="mt-0.5 text-3xs text-ink-muted line-clamp-2">{{ node().salient }}</p> }
          <div class="mt-0.5 flex items-center gap-1.5 text-2xs">
            @if (node().resolution === 'Syntactic') { <app-badge variant="warn">approx</app-badge> }
            @if (node().resolution === 'Semantic') { <app-badge variant="success">verified</app-badge> }
            @if (node().truncated) { <app-badge variant="default">truncated</app-badge> }
            @if (node().omitted > 0) { <span class="text-ink-subtle">{{ node().omitted }} omitted</span> }
          </div>
        </div>
      </div>

      @for (child of displayedChildren(); track child.id) {
        @if (asGroup(child); as group) {
          <!-- R3 D-A A-2: a run of cross-service hops collapses to one disclosure row.
               Closed, it still names the services — that IS the answer to "where does this
               go" — and states the hop/omitted counts it is hiding. Open, it renders the
               original subtree verbatim. -->
          <div class="border-l-2 border-line pl-3 py-1">
            <button
              type="button"
              class="flex w-full items-start gap-2 rounded -ml-0.5 px-0.5 text-left transition-colors hover:bg-surface-2"
              [attr.aria-expanded]="isOpen(group.id)"
              (click)="toggle(group.id)"
            >
              <app-seam-chip seam="CrossService" class="shrink-0" />
              <div class="min-w-0">
                <span class="text-xs text-ink-muted">
                  <span class="mr-1 inline-block w-2 text-accent">{{ isOpen(group.id) ? '▾' : '▸' }}</span>
                  crosses {{ group.services.length }} {{ group.services.length === 1 ? 'service' : 'services' }}
                  <span class="text-ink-subtle">· {{ group.hops }} hops</span>
                  @if (group.omitted > 0) {
                    <!-- &nbsp; because Angular strips the whitespace across an @if boundary -->
                    <span class="text-ink-subtle">&nbsp;· {{ group.omitted }} omitted</span>
                  }
                </span>
                <p class="mt-0.5 font-mono text-3xs text-ink-subtle line-clamp-2">
                  {{ group.services.join(' · ') }}
                </p>
              </div>
            </button>

            @if (isOpen(group.id)) {
              @for (member of group.members; track member.id) {
                <app-trace-node [node]="member" [depth]="depth() + 1" (nodeSelected)="onChildSelected($event)" />
              }
            }
          </div>
        } @else {
          <app-trace-node [node]="asNode(child)" [depth]="depth() + 1" (nodeSelected)="onChildSelected($event)" />
        }
      }
    </div>
  `,
})
export class TraceNodeComponent {
  readonly node = input.required<TraceNodeVm>();
  readonly depth = input(0);
  readonly nodeSelected = output<string>();

  /** Groups the caller has expanded. Local, deliberately: the collapse is a reading aid,
   * not navigation state worth surviving a re-trace. */
  private readonly expanded = signal<ReadonlySet<string>>(new Set());

  protected readonly displayedChildren = computed(() => groupServiceHops(this.node().children));

  protected isOpen(id: string): boolean {
    return this.expanded().has(id);
  }

  protected toggle(id: string): void {
    this.expanded.update((open) => {
      const next = new Set(open);
      if (!next.delete(id)) next.add(id);
      return next;
    });
  }

  protected asGroup(child: ReturnType<typeof groupServiceHops>[number]) {
    return isServiceHopGroup(child) ? child : null;
  }

  protected asNode(child: ReturnType<typeof groupServiceHops>[number]): TraceNodeVm {
    return child as TraceNodeVm;
  }

  protected onChildSelected(nodeId: string): void {
    this.nodeSelected.emit(nodeId);
  }
}
