import { Component, effect, inject, signal } from '@angular/core';

import type { EdgeVm } from '../../models/view-models';
import { toEdgeVm } from '../../models/view-models';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';
import { Panel } from '../../ui/panel/panel';

@Component({
  selector: 'app-impact-panel',
  imports: [Badge, Button, Icon, Panel],
  template: `
    @if (trace.nodeDetail(); as d) {
      <div class="p-3 border-b border-line">
        <div class="flex items-center gap-2">
          <app-icon name="code" [size]="14" class="text-accent" />
          <span class="truncate font-mono text-sm text-ink" [title]="d.title">{{ d.title }}</span>
        </div>
        <div class="mt-1 text-xs text-ink-subtle">{{ d.kind }} &middot; {{ d.outDegree }} out / {{ d.inDegree }} in</div>
        @if (d.filePath) {
          <div class="mt-1 truncate font-mono text-[11px] text-ink-subtle">{{ d.filePath }}</div>
        }
        @if (d.tags.length) {
          <div class="mt-2 flex flex-wrap gap-1">
            @for (tag of d.tags; track tag) {
              <app-badge>{{ tag }}</app-badge>
            }
          </div>
        }
      </div>

      <div class="p-2 space-y-2 text-xs">
        <div class="flex gap-2">
          <app-button
            [variant]="direction() === 'out' ? 'primary' : 'secondary'"
            size="sm"
            class="flex-1"
            (click)="setDirection('out')"
          >
            <app-icon name="arrow-right" [size]="11" /> Outgoing {{ d.outDegree }}
          </app-button>
          <app-button
            [variant]="direction() === 'in' ? 'primary' : 'secondary'"
            size="sm"
            class="flex-1"
            (click)="setDirection('in')"
          >
            <app-icon name="arrow-right" [size]="11" class="rotate-180" /> Incoming {{ d.inDegree }}
          </app-button>
        </div>

        <app-panel [title]="direction() === 'out' ? 'Calls out to' : 'Called by'">
          <div class="divide-y divide-line">
            @for (n of visibleNeighbors(); track n.to + n.kind + ':' + n.from) {
              <button
                class="flex w-full items-center gap-2 px-2 py-1.5 text-left hover:bg-surface-2"
                (click)="trace.selectNode(n.to)"
              >
                <app-badge variant="default">{{ n.kind }}</app-badge>
                <span class="truncate font-mono text-[11px] text-ink">{{ n.otherTitle }}</span>
                @if (n.resolution === 'Syntactic') {
                  <app-badge variant="warn">approx</app-badge>
                }
              </button>
            } @empty {
              <p class="px-2 py-3 text-xs text-ink-subtle">
                {{ direction() === 'out' ? 'No outgoing edges.' : 'No incoming edges.' }}
              </p>
            }
          </div>
        </app-panel>
      </div>
    } @else {
      <div class="flex h-full items-center justify-center px-4 text-center text-xs text-ink-subtle">
        Click a node to inspect its connections.
      </div>
    }
  `,
})
export class ImpactPanel {
  protected readonly trace = inject(TraceStore);
  private readonly api = inject(DevContextApi);
  private readonly session = inject(SessionStore);

  readonly direction = signal<'out' | 'in'>('out');
  readonly inboundNeighbors = signal<readonly EdgeVm[]>([]);
  private lastNodeId: string | null = null;

  readonly visibleNeighbors = signal<readonly EdgeVm[]>([]);

  constructor() {
    effect(() => {
      const nid = this.trace.selectedNodeId();
      if (nid && nid !== this.lastNodeId) {
        this.lastNodeId = nid;
        this.inboundNeighbors.set([]);
      }
    });

    effect(() => {
      const dir = this.direction();
      if (dir === 'out') {
        this.visibleNeighbors.set(this.trace.neighbors());
      } else {
        this.visibleNeighbors.set(this.inboundNeighbors());
      }
    });
  }

  setDirection(dir: 'out' | 'in'): void {
    this.direction.set(dir);
    if (dir === 'in') {
      void this.loadInbound();
    }
  }

  private async loadInbound(): Promise<void> {
    const handle = this.session.handle();
    const nid = this.trace.selectedNodeId();
    if (!handle || !nid) return;
    try {
      const res = await this.api.getNeighbors(handle, nid, 'in');
      this.inboundNeighbors.set(res.edges.map(toEdgeVm));
    } catch { /* ignore */ }
  }
}
