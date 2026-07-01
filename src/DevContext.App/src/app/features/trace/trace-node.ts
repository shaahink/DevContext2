import { Component, input } from '@angular/core';

import type { TraceNodeVm } from '../../models/view-models';
import { Badge } from '../../ui/badge/badge';

@Component({
  selector: 'app-trace-node',
  imports: [Badge],
  template: `
    <div class="ml-[calc({{depth()}}*20px)] border-l-2 border-line pl-3 py-1">
      <div class="flex items-start gap-2">
        <span class="shrink-0 rounded bg-surface-2 px-1 py-0.5 font-mono text-2xs text-accent">{{ node().seam }}</span>
        <div class="min-w-0">
          <p class="font-mono text-xs text-ink">{{ node().title }}</p>
          @if (node().salient) { <p class="mt-0.5 text-3xs text-ink-muted line-clamp-2">{{ node().salient }}</p> }
          <div class="mt-0.5 flex items-center gap-1.5 text-2xs">
            @if (node().resolution === 'Syntactic') { <app-badge variant="warn">approx</app-badge> }
            @if (node().truncated) { <app-badge variant="default">truncated</app-badge> }
            @if (node().omitted > 0) { <span class="text-ink-subtle">{{ node().omitted }} omitted</span> }
          </div>
        </div>
      </div>
      @for (child of node().children; track child.id) {
        <app-trace-node [node]="child" [depth]="depth() + 1" />
      }
    </div>
  `,
})
export class TraceNodeComponent {
  readonly node = input.required<TraceNodeVm>();
  readonly depth = input(0);
}
