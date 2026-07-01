import { Component, inject, signal } from '@angular/core';

import type { StatsResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { ActivityService } from '../../core/activity/activity.service';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { ViewFrame } from '../../shell/view-frame';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { Card } from '../../ui/card/card';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-stats-view',
  imports: [Icon, Badge, Card, Button, ViewFrame],
  template: `
    <app-view-frame [showSidebar]="false">
      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Stats</h2>
        <app-button variant="ghost" size="sm" (click)="loadStats()" [disabled]="loading()"><app-icon name="refresh" [size]="12" /> Refresh</app-button>
      </div>

      @if (session.ready()) {
        @if (loading()) {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Loading stats…</p></div>
        } @else if (stats()) {
          <div class="overflow-auto p-5 space-y-5">
            <div class="grid grid-cols-1 gap-5 lg:grid-cols-2">
              <div class="space-y-4">
                <h3 class="text-sm font-semibold text-ink">Run</h3>
                <app-card>
                  <h4 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Stages</h4>
                  <div class="mt-2 space-y-1">
                    @for (s of stats()!.stages; track s.stage) {
                      <div class="flex items-center justify-between text-xs"><span class="text-ink">{{ s.stage }}</span><span class="font-mono tabular-nums text-ink-muted">{{ s.elapsedMs }}ms</span></div>
                    }
                  </div>
                  <div class="mt-2 border-t border-line pt-2 text-xs"><span class="font-medium text-ink">Total wall</span><span class="ml-2 font-mono tabular-nums text-ink-muted">{{ stats()!.totalWallMs }}ms</span></div>
                </app-card>
                <app-card>
                  <h4 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Extractors</h4>
                  <div class="mt-2 space-y-1">
                    @for (e of stats()!.extractors; track e.name) {
                      <div class="flex items-center justify-between gap-2 text-xs"><span class="truncate text-ink">{{ e.name }}</span><div class="flex shrink-0 items-center gap-2">@if (e.skipped) { <app-badge variant="warn">skipped</app-badge> }<span class="font-mono tabular-nums text-ink-muted">{{ e.elapsedMs }}ms</span><span class="font-mono tabular-nums text-ink-subtle">+{{ e.typesAdded }}</span></div></div>
                    }
                  </div>
                </app-card>
                <app-card>
                  <h4 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Cache</h4>
                  <div class="mt-2 grid grid-cols-4 gap-2 text-xs">
                    <div><span class="text-ink-subtle">Text hits</span><p class="font-mono tabular-nums text-success">{{ stats()!.cache?.textHits ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Text misses</span><p class="font-mono tabular-nums text-ink-muted">{{ stats()!.cache?.textMisses ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Syntax hits</span><p class="font-mono tabular-nums text-success">{{ stats()!.cache?.syntaxTreeHits ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Syntax misses</span><p class="font-mono tabular-nums text-ink-muted">{{ stats()!.cache?.syntaxTreeMisses ?? 0 }}</p></div>
                  </div>
                </app-card>
              </div>
              <div class="space-y-4">
                <h3 class="text-sm font-semibold text-ink">Code</h3>
                <app-card>
                  <h4 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Graph</h4>
                  <div class="mt-2 grid grid-cols-4 gap-2 text-xs">
                    <div><span class="text-ink-subtle">Nodes</span><p class="font-mono tabular-nums text-ink">{{ stats()!.graph?.nodes ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Edges</span><p class="font-mono tabular-nums text-ink">{{ stats()!.graph?.edges ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Entries</span><p class="font-mono tabular-nums text-ink">{{ stats()!.graph?.entries ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Wired</span><p class="font-mono tabular-nums text-ink">{{ stats()!.graph?.entriesWithTarget ?? 0 }}</p></div>
                  </div>
                </app-card>
                <app-card>
                  <h4 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Corpus</h4>
                  <div class="mt-2 grid grid-cols-3 gap-2 text-xs">
                    <div><span class="text-ink-subtle">Files</span><p class="font-mono tabular-nums text-ink">{{ stats()!.corpus?.totalFiles ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">C# files</span><p class="font-mono tabular-nums text-ink">{{ stats()!.corpus?.csharpFiles ?? 0 }}</p></div>
                    <div><span class="text-ink-subtle">Projects</span><p class="font-mono tabular-nums text-ink">{{ stats()!.corpus?.projects ?? 0 }}</p></div>
                  </div>
                </app-card>
                <app-card>
                  <h4 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Seams</h4>
                  <div class="mt-2 space-y-1">
                    @for (s of stats()!.seams; track s.seam) {
                      <div class="flex items-center justify-between text-xs"><span class="text-ink">{{ s.seam }}</span><div class="flex items-center gap-1.5"><span class="font-mono tabular-nums text-ink">{{ s.count }}</span>@if (s.approx) { <app-badge variant="warn">approx</app-badge> }</div></div>
                    }
                  </div>
                </app-card>
              </div>
            </div>
          </div>
        } @else {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Click Refresh to load stats.</p></div>
        }
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to see stats.</p></div>
      }
    </app-view-frame>
  `,
})
export class StatsView {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly activity = inject(ActivityService);
  protected readonly stats = signal<StatsResponse | null>(null);
  protected readonly loading = signal(false);

  constructor() {
    this.loadStats();
  }

  protected async loadStats(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.loading.set(true);
    await this.activity.runSecondary('Loading stats…', async () => {
      const res = await this.api.getStats(handle);
      this.stats.set(res);
    });
    this.loading.set(false);
  }
}
