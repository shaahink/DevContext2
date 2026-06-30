import { Component, input, output } from '@angular/core';

import type { EdgeVm, NodeDetailVm } from '../../models/view-models';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-node-detail',
  imports: [Icon, Badge, Button],
  template: `
    @if (detail(); as d) {
      <div class="flex h-full flex-col overflow-hidden">
        <div class="border-b border-line p-3">
          <div class="flex items-center gap-2">
            <app-icon name="code" [size]="14" class="text-accent" />
            <span class="truncate font-mono text-sm text-ink" [title]="d.title">{{ d.title }}</span>
          </div>
          <div class="mt-1 text-xs text-ink-subtle">{{ d.kind }} &middot; {{ d.outDegree }} out / {{ d.inDegree }} in</div>
          @if (d.filePath) {
            <div class="mt-1 truncate font-mono text-[11px] text-ink-subtle" [title]="d.filePath">{{ d.filePath }}</div>
          }
          @if (d.tags.length) {
            <div class="mt-2 flex flex-wrap gap-1">
              @for (tag of d.tags; track tag) {
                <app-badge>{{ tag }}</app-badge>
              }
            </div>
          }
        </div>
        <div class="min-h-0 flex-1 overflow-y-auto p-2">
          <div class="px-1 pb-1 text-[10px] font-semibold uppercase tracking-wide text-ink-subtle">Calls / uses</div>
          @for (n of neighbors(); track n.to + n.kind + ':' + n.from; let idx = $index) {
            <app-button
              variant="ghost"
              size="sm"
              class="w-full justify-start"
              (click)="navigate.emit(n.to)"
            >
              <app-badge variant="default">{{ n.kind }}</app-badge>
              <app-icon name="arrow-right" [size]="11" class="text-ink-subtle" />
              <span class="truncate font-mono text-xs text-ink">{{ n.otherTitle }}</span>
            </app-button>
          } @empty {
            <p class="px-2 py-3 text-xs text-ink-subtle">No outgoing edges.</p>
          }
        </div>
      </div>
    } @else {
      <div class="flex h-full items-center justify-center px-4 text-center text-xs text-ink-subtle">
        Click a node to inspect its declaration and neighbours.
      </div>
    }
  `,
})
export class NodeDetail {
  readonly detail = input<NodeDetailVm | null>(null);
  readonly neighbors = input<readonly EdgeVm[]>([]);
  readonly navigate = output<string>();
}
