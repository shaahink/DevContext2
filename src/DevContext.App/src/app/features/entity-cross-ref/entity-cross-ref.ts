import { Component, effect, inject, signal } from '@angular/core';

import type { EntryPointsResponse, TraceResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';
import { Panel } from '../../ui/panel/panel';

interface EntityRef {
  readonly name: string;
  readonly touchedBy: string[];
}

@Component({
  selector: 'app-entity-cross-ref',
  imports: [Badge, Button, Icon, Panel],
  template: `
    <div class="flex-1 overflow-y-auto p-2 space-y-3">
      <div class="mb-2 flex items-center justify-between">
        <div class="flex items-center gap-2 text-xs">
          <app-icon name="database" [size]="13" class="text-accent" />
          <span class="font-medium text-ink">Entity cross-reference</span>
        </div>
        <app-button variant="ghost" size="sm" (click)="compute()" [disabled]="loading()">
          <app-icon name="refresh" [size]="12" /> {{ loading() ? 'Scanning…' : 'Scan' }}
        </app-button>
      </div>

      @if (entityRefs().length) {
        @for (ref of entityRefs(); track ref.name) {
          <app-panel>
            <div class="p-2">
              <div class="flex items-center gap-2">
                <app-badge variant="accent">{{ ref.name }}</app-badge>
                <span class="text-[10px] text-ink-muted">{{ ref.touchedBy.length }} entries touch this entity</span>
              </div>
              <div class="mt-1.5 flex flex-wrap gap-1">
                @for (entry of ref.touchedBy.slice(0, 5); track entry) {
                  <button
                    class="rounded bg-base px-1.5 py-0.5 font-mono text-[10px] text-ink hover:text-accent"
                    (click)="traceEntry(entry)"
                  >
                    {{ entry }}
                  </button>
                }
                @if (ref.touchedBy.length > 5) {
                  <span class="text-[10px] text-ink-subtle">+{{ ref.touchedBy.length - 5 }} more</span>
                }
              </div>
            </div>
          </app-panel>
        }
      } @else if (!loading()) {
        <p class="p-4 text-center text-xs text-ink-subtle">
          Click Scan to map which entries touch which entities.
        </p>
      }
    </div>
  `,
})
export class EntityCrossRef {
  private readonly api = inject(DevContextApi);
  protected readonly session = inject(SessionStore);
  private readonly trace = inject(TraceStore);

  readonly loading = signal(false);
  readonly entityRefs = signal<EntityRef[]>([]);

  async compute(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.loading.set(true);
    try {
      const entries = await this.api.listEntryPoints(handle);
      const entityMap = new Map<string, string[]>();

      for (const ep of entries.entryPoints.slice(0, 30)) {
        const focus = ep.httpMethod && ep.route ? `${ep.httpMethod} ${ep.route}` : ep.title;
        try {
          const trace = await this.api.getTrace(handle, focus, 4, 'signature');
          for (const entity of trace.touchedEntities) {
            if (!entityMap.has(entity)) entityMap.set(entity, []);
            entityMap.get(entity)!.push(ep.route || ep.title);
          }
        } catch { /* skip entries that fail to trace */ }
      }

      this.entityRefs.set(
        [...entityMap.entries()]
          .sort((a, b) => b[1].length - a[1].length)
          .map(([name, touchedBy]) => ({ name, touchedBy })),
      );
    } finally {
      this.loading.set(false);
    }
  }

  traceEntry(focus: string): void {
    const handle = this.session.handle();
    if (handle) void this.trace.trace(handle, focus);
  }
}
