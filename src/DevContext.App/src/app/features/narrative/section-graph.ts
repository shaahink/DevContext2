import { Component, inject, signal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { GraphCanvas } from '../../ui/graph-canvas/graph-canvas';
import { Icon } from '../../ui/icon/icon';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-section-graph',
  imports: [SectionCard, GraphCanvas, Icon, FormsModule],
  template: `
    <app-section-card id="graph" title="Call Graph">
      @if (traceStore.tree(); as root) {
        <div class="mb-3 flex items-center gap-3 text-2xs">
          <label class="flex items-center gap-1.5">
            <span class="text-ink-subtle">Depth</span>
            <select class="rounded border border-line bg-base px-1.5 py-1 text-xs text-ink outline-none focus:border-accent" [ngModel]="graphDepth()" (ngModelChange)="graphDepth.set(+$event)">
              <option [value]="1">1</option>
              <option [value]="2">2</option>
              <option [value]="3">3</option>
              <option [value]="4">4</option>
            </select>
          </label>
          <span class="text-ink-subtle">|</span>
          <span class="text-ink-muted">Click a node to trace it</span>
          <button
            class="ml-auto flex items-center gap-1 rounded px-1.5 py-0.5 text-ink-muted hover:bg-surface-2 hover:text-ink"
            (click)="expanded.set(!expanded())"
          >
            <app-icon [name]="expanded() ? 'x' : 'play'" [size]="12" />
            {{ expanded() ? 'Shrink' : 'Expand' }}
          </button>
        </div>
        <div [class.max-h-[500px]]="!expanded()" class="transition-all">
          <app-graph-canvas
            [data]="{ mode: 'trace', root, maxDepth: graphDepth() }"
            (nodeSelected)="onNodeSelected($event)"
          />
        </div>
      } @else {
        <p class="py-8 text-center text-xs text-ink-subtle">
          <app-icon name="arrow-right" [size]="12" class="inline text-ink-subtle" />
          Select an entry and trace it to visualize the call graph.
        </p>
      }
    </app-section-card>
  `,
})
export class SectionGraph {
  protected readonly session = inject(SessionStore);
  protected readonly traceStore = inject(TraceStore);

  protected readonly graphDepth = signal(2);
  protected readonly expanded = signal(false);

  protected onNodeSelected(nodeId: string): void {
    const handle = this.session.handle();
    if (!handle || !nodeId) return;
    void this.traceStore.selectNode(nodeId);
    void this.traceStore.trace(handle, nodeId);
  }
}
