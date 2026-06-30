import { Component, inject, signal } from '@angular/core';

import type { StatsResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';
import { Panel } from '../../ui/panel/panel';
import { Sheet } from '../../ui/sheet/sheet';
import { Button } from '../../ui/button/button';

@Component({
  selector: 'app-stats-drawer',
  imports: [Sheet, Panel, Icon, Button],
  template: `
    <app-button variant="ghost" size="sm" (click)="open.set(true)" title="Analysis stats">
      <app-icon name="database" [size]="14" /> Stats
    </app-button>

    <app-sheet [open]="open()" (closed)="open.set(false)">
      <div class="flex h-full flex-col">
        <div class="flex items-center justify-between border-b border-line px-4 py-3">
          <div class="flex items-center gap-2">
            <app-icon name="database" [size]="15" class="text-accent" />
            <span class="font-medium text-ink">Analysis stats</span>
          </div>
          <app-button variant="ghost" size="sm" (click)="open.set(false)">
            <app-icon name="x" [size]="14" />
          </app-button>
        </div>

        <div class="flex-1 overflow-y-auto p-4 space-y-4">
          @if (stats(); as s) {
            <app-panel title="Graph">
              <div class="p-3 grid grid-cols-2 gap-2 text-xs">
                <div><span class="text-ink-muted">Nodes</span> <span class="font-mono text-ink">{{ s.graph?.nodes }}</span></div>
                <div><span class="text-ink-muted">Edges</span> <span class="font-mono text-ink">{{ s.graph?.edges }}</span></div>
                <div><span class="text-ink-muted">Entries</span> <span class="font-mono text-ink">{{ s.graph?.entries }}</span></div>
                <div><span class="text-ink-muted">With target</span> <span class="font-mono text-ink">{{ s.graph?.entriesWithTarget }}</span></div>
              </div>
            </app-panel>

            @if (s.seams.length) {
              <app-panel title="Seams">
                <ul class="p-3 space-y-1 text-xs">
                  @for (sm of s.seams; track sm.seam) {
                    <li class="flex justify-between">
                      <span class="text-ink">{{ sm.seam }}</span>
                      <span class="font-mono text-ink-muted">{{ sm.count }} <span class="text-ink-subtle">({{ sm.approx }} approx)</span></span>
                    </li>
                  }
                </ul>
              </app-panel>
            }

            @if (s.stages.length) {
              <app-panel title="Stages">
                <ul class="p-3 space-y-1 text-xs">
                  @for (st of s.stages; track st.stage) {
                    <li class="flex justify-between">
                      <span class="text-ink">{{ st.stage }}</span>
                      <span class="font-mono text-ink-muted">{{ st.elapsedMs }} ms</span>
                    </li>
                  }
                </ul>
              </app-panel>
            }

            <app-panel title="Corpus & cache">
              <div class="p-3 grid grid-cols-2 gap-2 text-xs">
                <div><span class="text-ink-muted">Files</span> <span class="font-mono text-ink">{{ s.corpus?.totalFiles }}</span></div>
                <div><span class="text-ink-muted">C# files</span> <span class="font-mono text-ink">{{ s.corpus?.csharpFiles }}</span></div>
                <div><span class="text-ink-muted">Projects</span> <span class="font-mono text-ink">{{ s.corpus?.projects }}</span></div>
                <div><span class="text-ink-muted">Total wall</span> <span class="font-mono text-ink">{{ s.totalWallMs }} ms</span></div>
              </div>
              <div class="px-3 pb-3 grid grid-cols-2 gap-2 text-xs">
                <div><span class="text-ink-muted">Text hits</span> <span class="font-mono text-ink">{{ s.cache?.textHits }}</span></div>
                <div><span class="text-ink-muted">Text misses</span> <span class="font-mono text-ink">{{ s.cache?.textMisses }}</span></div>
                <div><span class="text-ink-muted">Syntax hits</span> <span class="font-mono text-ink">{{ s.cache?.syntaxTreeHits }}</span></div>
                <div><span class="text-ink-muted">Syntax misses</span> <span class="font-mono text-ink">{{ s.cache?.syntaxTreeMisses }}</span></div>
              </div>
            </app-panel>
          } @else {
            <div class="flex items-center justify-center py-12 text-xs text-ink-subtle">
              Click Refresh to load stats.
            </div>
          }
        </div>

        <div class="border-t border-line p-4">
          <app-button variant="primary" (click)="load()" class="w-full">
            <app-icon name="refresh" [size]="14" /> Refresh
          </app-button>
        </div>
      </div>
    </app-sheet>
  `,
})
export class StatsDrawer {
  private readonly api = inject(DevContextApi);
  private readonly session = inject(SessionStore);

  readonly open = signal(false);
  readonly stats = signal<StatsResponse | null>(null);
  private loadId = 0;

  async load(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    const id = ++this.loadId;
    try {
      const s = await this.api.getStats(handle);
      if (id === this.loadId) this.stats.set(s);
    } catch { /* ignore */ }
  }
}
