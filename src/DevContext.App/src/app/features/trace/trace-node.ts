import { Component, input, output } from '@angular/core';

import type { TraceNodeVm } from '../../models/view-models';
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
      @for (child of node().children; track child.id) {
        <app-trace-node [node]="child" [depth]="depth() + 1" (nodeSelected)="onChildSelected($event)" />
      }
    </div>
  `,
})
export class TraceNodeComponent {
  readonly node = input.required<TraceNodeVm>();
  readonly depth = input(0);
  readonly nodeSelected = output<string>();

  protected onChildSelected(nodeId: string): void {
    this.nodeSelected.emit(nodeId);
  }
}
