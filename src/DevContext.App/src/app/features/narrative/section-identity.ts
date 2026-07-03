import { Component, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { SessionStore } from '../../state/session.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { StatCell } from '../../ui/stat-cell/stat-cell';
import { Badge } from '../../ui/badge/badge';

@Component({
  selector: 'app-section-identity',
  imports: [SectionCard, StatCell, Badge, DecimalPipe],
  template: `
    <app-section-card id="identity" title="Identity" [subtitle]="repoLabel()">
      @if (!session.ready()) {
        <p class="py-8 text-center text-xs text-ink-subtle">Analyze a repo to see identity stats.</p>
      } @else {
        <div class="space-y-5">
          <div class="flex flex-wrap items-center gap-3">
            @if (archetype(); as a) {
              <app-badge variant="accent" class="text-xs">{{ a }}</app-badge>
            }
            @if (style(); as s) {
              <span class="text-xs text-ink-muted">
                {{ s }}
                @if (styleConfidence() > 0) {
                  <span class="font-mono tabular-nums text-ink-subtle"> &middot; {{ (styleConfidence() * 100) | number:'1.0-0' }}%</span>
                }
              </span>
            }
            @if (scope(); as sc) {
              <span class="text-2xs text-ink-subtle">&#8212; {{ sc }}</span>
            }
          </div>

          <div class="grid grid-cols-5 gap-2">
            <app-stat-cell [value]="summary()?.nodes ?? 0" label="nodes" />
            <app-stat-cell [value]="summary()?.edges ?? 0" label="edges" />
            <app-stat-cell [value]="summary()?.entries ?? 0" label="entries" />
            <app-stat-cell [value]="wired()" label="wired" />
            <app-stat-cell [value]="coverage() + '%'" label="coverage" />
          </div>

          @if (stack().length) {
            <div class="flex flex-wrap items-center gap-1.5">
              <span class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Stack</span>
              @for (item of stack(); track item) {
                <span class="rounded bg-surface-2 px-1.5 py-0.5 font-mono text-2xs text-ink-muted">{{ item }}</span>
              }
            </div>
          }
        </div>
      }
    </app-section-card>
  `,
})
export class SectionIdentity {
  protected readonly session = inject(SessionStore);

  protected readonly summary = this.session.summary;
  protected readonly map = this.session.mapResponse;

  protected readonly repoLabel = computed(() => this.summary()?.label ?? '');
  protected readonly archetype = computed(() => this.map()?.archetype);
  protected readonly style = computed(() => this.map()?.style);
  protected readonly styleConfidence = computed(() => this.map()?.styleConfidence ?? 0);
  protected readonly scope = computed(() => this.map()?.scopeNote);
  protected readonly stack = computed(() => this.map()?.stack ?? []);

  protected readonly wired = computed(() => this.summary()?.entriesWithTarget ?? 0);
  protected readonly coverage = computed(() => {
    const s = this.summary();
    if (!s || s.entries === 0) return 0;
    return Math.round((s.entriesWithTarget / s.entries) * 100);
  });
}
