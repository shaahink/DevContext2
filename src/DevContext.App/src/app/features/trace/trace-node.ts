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
            @if (node().omitted > 0) { <span class="text-ink-subtle">{{ omittedLabel() }}</span> }
            <!-- M1.1 - the three DI honesty annotations the CLI has rendered since I1.6/C5/T2.1.
                 They were computed, they rode Core, and the wire dropped them; the app showed a
                 resolve step as if it were the only binding. -->
            @if (node().multiImplCount > 1) {
              <app-badge variant="warn" title="Dependency injection has this many implementations for this service type">{{ node().multiImplCount }} impls</app-badge>
            }
            @if (node().diHostCount > 1) {
              <app-badge variant="warn" title="Registered by this many hosts, none of them the traced host - the cited site is the deterministic first">{{ node().diHostCount }} hosts</app-badge>
            }
            @if (node().testOnly) {
              <app-badge variant="warn" title="This binding comes only from a test project - it is not the production wiring">test-only</app-badge>
            }
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

  /** M1.1 — CLI parity for the truncation marker. The count alone says something was cut; the
   * names say whether it mattered and where to point the next trace. Ellipsis when the engine
   * capped the name list below the omitted count (TraceBuilder.MaxOmittedNames). */
  protected readonly omittedLabel = computed(() => {
    const n = this.node();
    const names = n.omittedNames;
    if (names.length === 0) return `${n.omitted} omitted`;
    const tail = names.length < n.omitted ? ', …' : '';
    return `${n.omitted} omitted: ${names.join(', ')}${tail}`;
  });

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
