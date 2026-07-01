import { Component, computed, inject } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { Badge } from '../../ui/badge/badge';
import { SEAM_COLORS } from '../../models/seam-colors';

@Component({
  selector: 'app-section-architecture',
  imports: [SectionCard, Badge],
  template: `
    <app-section-card id="architecture" title="Architecture">
      <div class="space-y-6">
        @if (topology().length) {
          <div>
            <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Projects</h3>
            <div class="space-y-1 rounded-md border border-line bg-surface p-3 font-mono text-xs">
              @for (p of topology(); track p.name) {
                <div class="flex items-center gap-2">
                  <span class="text-ink">{{ p.name }}</span>
                  @if (p.dependsOn.length) {
                    <span class="text-ink-subtle">&rarr;</span>
                    <span class="text-ink-muted">{{ p.dependsOn.join(', ') }}</span>
                  }
                </div>
              }
            </div>
          </div>
        }

        @if (pipelineBehaviors().length) {
          <div>
            <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Pipeline Behaviors</h3>
            <div class="flex flex-wrap gap-1.5">
              @for (b of pipelineBehaviors(); track b) {
                <span class="rounded bg-surface-2 px-2 py-0.5 font-mono text-2xs text-ink-muted">{{ b }}</span>
              }
            </div>
          </div>
        }

        @if (aggregates().length) {
          <div>
            <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Aggregates</h3>
            <div class="flex flex-wrap gap-1.5">
              @for (a of aggregates(); track a) {
                <app-badge variant="default">{{ a }}</app-badge>
              }
            </div>
          </div>
        }

        @if (packages().length) {
          <div>
            <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Packages</h3>
            <div class="space-y-2">
              @for (pg of packages(); track pg.label) {
                <div class="text-xs">
                  <span class="font-medium text-ink">{{ pg.label }}:</span>
                  <span class="text-ink-muted">{{ pg.packages.join(', ') }}</span>
                </div>
              }
            </div>
          </div>
        }

        @if (seamPct().length) {
          <div>
            <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Seams</h3>
            <div class="space-y-1.5">
              @for (s of seamPct(); track s.seam) {
                <div class="flex items-center gap-2">
                  <span class="w-16 text-right text-2xs font-mono tabular-nums text-ink-muted">{{ s.seam }}</span>
                  <div class="h-2.5 flex-1 overflow-hidden rounded-full bg-surface-2">
                    <div
                      class="h-full rounded-full transition-all duration-500"
                      [style.width.%]="s.pct"
                      [style.backgroundColor]="seamColor(s.seam)"
                    ></div>
                  </div>
                  <span class="w-8 text-right text-2xs tabular-nums text-ink-subtle">{{ s.count }}</span>
                </div>
              }
            </div>
          </div>
        }
      </div>
    </app-section-card>
  `,
})
export class SectionArchitecture {
  protected readonly session = inject(SessionStore);
  protected readonly map = this.session.mapResponse;

  protected readonly topology = computed(() => this.map()?.topology ?? []);
  protected readonly pipelineBehaviors = computed(() => this.map()?.pipelineBehaviors ?? []);
  protected readonly aggregates = computed(() => this.map()?.aggregates ?? []);
  protected readonly packages = computed(() => this.map()?.packages ?? []);
  protected readonly seamPct = computed(() => {
    const stats = this.session.stats();
    if (!stats?.seams.length) return [];
    const max = Math.max(...stats.seams.map((s) => s.count), 1);
    return stats.seams
      .map((s) => ({ seam: s.seam, count: s.count, pct: Math.round((s.count / max) * 100) }))
      .sort((a, b) => b.count - a.count);
  });

  protected seamColor(seam: string): string {
    return SEAM_COLORS[seam] ?? '#6b7480';
  }
}
