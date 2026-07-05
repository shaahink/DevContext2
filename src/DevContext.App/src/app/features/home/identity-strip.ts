import { Component, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';

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
      @if (stale(); as msg) {
        <div class="flex items-center gap-2 rounded-lg bg-amber-500/10 px-3 py-2 text-sm text-amber-400">
          <span class="i-lucide-alert-triangle h-4 w-4 shrink-0"></span>
          <span>{{ msg }}</span>
          <button type="button"
            class="ml-auto rounded-md bg-amber-500/20 px-2 py-0.5 text-xs font-medium text-amber-300 hover:bg-amber-500/30 transition-colors"
            (click)="reanalyze()">
            Re-analyze
          </button>
        </div>
      }

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
        <button type="button" class="contents cursor-pointer" (click)="showLedger = !showLedger">
          <app-stat-cell [value]="confPct()" label="confidence" />
        </button>
      </div>

      @if (showLedger && ledger(); as l) {
        <div class="rounded-lg border border-line bg-surface-2 p-3 space-y-2 text-xs">
          <p class="font-semibold text-ink">Confidence Ledger</p>
          <div class="grid grid-cols-2 gap-x-4 gap-y-1 text-ink-muted">
            <span>Overall</span><span class="tabular-nums font-mono text-ink">{{ (l.overall * 100).toFixed(0) }}%</span>
            <span>Verified edges</span><span class="tabular-nums font-mono text-ink">{{ (l.verifiedEdgePct * 100).toFixed(0) }}%</span>
            <span>Approximate edges</span><span class="tabular-nums font-mono text-ink">{{ (l.approxEdgePct * 100).toFixed(0) }}%</span>
            <span>Auth coverage</span><span class="tabular-nums font-mono text-ink">{{ l.endpointsWithAuth }}/{{ l.totalEndpoints }}</span>
            <span>Entry targets</span><span class="tabular-nums font-mono text-ink">{{ l.entriesWithTarget }}/{{ l.totalEntries }}</span>
          </div>
          @if (l.perSeam.length) {
            <div class="pt-1">
              <span class="text-2xs text-ink-subtle">Per seam</span>
              <div class="mt-1 grid grid-cols-4 gap-x-2 gap-y-0.5 text-2xs">
                @for (s of l.perSeam; track s.seam) {
                  <span class="font-mono">{{ s.seam }}</span>
                  <span class="tabular-nums text-ink-muted">{{ s.total }}</span>
                  <span class="tabular-nums text-accent">{{ s.verified }}&#x2713;</span>
                  <span class="tabular-nums text-warn">{{ s.approx }}~</span>
                }
              </div>
            </div>
          }
        </div>
      }

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

  protected readonly summary = this.session.summary;
  protected readonly map = this.session.mapResponse;
  protected readonly ledger = this.session.confidenceLedger;
  protected showLedger = false;

  protected readonly stale = computed(() => {
    const s = this.summary();
    return s?.stale ? (s.staleMessage || 'Repo has changed — Re-analyze?') : null;
  });

  protected readonly archetype = computed(() => this.map()?.archetype);
  protected readonly style = computed(() => this.map()?.style);
  protected readonly styleConfidence = computed(() => this.map()?.styleConfidence ?? 0);
  protected readonly scope = computed(() => this.map()?.scopeNote);
  protected readonly stack = computed(() => this.map()?.stack ?? []);

  protected readonly wired = computed(() => this.summary()?.entriesWithTarget ?? 0);
  protected readonly unwired = computed(() => {
    const s = this.summary();
    return s ? s.entries - s.entriesWithTarget : 0;
  });
  protected readonly coverage = computed(() => {
    const s = this.summary();
    if (!s || s.entries === 0) return 0;
    return Math.round((s.entriesWithTarget / s.entries) * 100);
  });

  protected readonly confPct = computed(() => {
    const l = this.ledger();
    return l ? `${Math.round(l.overall * 100)}%` : '—';
  });

  protected reanalyze() {
    void this.session.reAnalyze();
  }
}

