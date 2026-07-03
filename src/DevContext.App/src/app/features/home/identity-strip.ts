import { Component, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { AtlasStore } from '../../state/atlas.store';
import { SessionStore } from '../../state/session.store';
import { StatCell } from '../../ui/stat-cell/stat-cell';
import { Badge } from '../../ui/badge/badge';

/**
 * Home's identity strip (proposal §2 "identity strip"): archetype/style/scope,
 * node/edge/entry/wired/coverage stats, detected stack. Card-free (ported from the
 * old section-identity.ts, which wrapped this in the now-deleted SectionCard).
 */
@Component({
  selector: 'app-identity-strip',
  imports: [StatCell, Badge, DecimalPipe],
  template: `
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

      <div class="grid grid-cols-3 gap-2 sm:grid-cols-7">
        <app-stat-cell [value]="summary()?.nodes ?? 0" label="nodes" />
        <app-stat-cell [value]="summary()?.edges ?? 0" label="edges" />
        <app-stat-cell [value]="summary()?.entries ?? 0" label="entries" />
        <app-stat-cell [value]="wired()" label="wired" />
        <app-stat-cell [value]="unwired()" label="unwired" />
        <app-stat-cell [value]="coverage() + '%'" label="coverage" />
        <app-stat-cell [value]="confidence() === null ? '—' : confidence() + '%'" label="confidence" />
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
  `,
  host: { class: 'contents' },
})
export class IdentityStrip {
  protected readonly session = inject(SessionStore);
  private readonly atlas = inject(AtlasStore);

  protected readonly summary = this.session.summary;
  protected readonly map = this.session.mapResponse;

  protected readonly archetype = computed(() => this.map()?.archetype);
  protected readonly style = computed(() => this.map()?.style);
  protected readonly styleConfidence = computed(() => this.map()?.styleConfidence ?? 0);
  protected readonly scope = computed(() => this.map()?.scopeNote);
  protected readonly stack = computed(() => this.map()?.stack ?? []);

  protected readonly wired = computed(() => this.summary()?.entriesWithTarget ?? 0);
  /** §3.6 — `summary.entries - summary.entriesWithTarget`, the same subtraction the
   * server-computed `entriesWithTarget` implies; no new field needed. */
  protected readonly unwired = computed(() => {
    const s = this.summary();
    return s ? s.entries - s.entriesWithTarget : 0;
  });
  protected readonly coverage = computed(() => {
    const s = this.summary();
    if (!s || s.entries === 0) return 0;
    return Math.round((s.entriesWithTarget / s.entries) * 100);
  });

  /** §3.5 repo-wide confidence, from the Flow Atlas's indexed flows — null (rendered
   * "—") until at least one flow has been indexed, not 0, since 0% is a real value. */
  protected readonly confidence = this.atlas.overallVerifiedPct;
}
