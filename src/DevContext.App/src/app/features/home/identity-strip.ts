import { Component, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { SessionStore } from '../../state/session.store';
import { Badge } from '../../ui/badge/badge';
import { formatCompact } from '../../core/format';

@Component({
  selector: 'app-identity-strip',
  imports: [Badge, DecimalPipe],
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

      <!-- Identity sentence -->
      <p class="text-base leading-relaxed text-ink">{{ identitySentence() }}</p>

      <!-- Stat strip — human labels -->
      <div class="flex flex-wrap items-center gap-3 text-2xs text-ink-subtle">
        @for (label of statLabels(); track label[0]) {
          <span class="tabular-nums" [title]="label[2]">
            <span class="text-ink font-semibold">{{ label[1] }}</span> {{ label[0] }}
          </span>
        }
      </div>

      <!-- Archetype + style badges -->
      <div class="flex flex-wrap items-center gap-2">
        @if (archetype(); as a) {
          <app-badge variant="accent" class="text-xs">{{ a }}</app-badge>
        }
        @if (style(); as s) {
          <span class="text-xs text-ink-muted">{{ s }}
            @if (styleConfidence() > 0) {
              <span class="font-mono tabular-nums text-ink-subtle"> &middot; {{ (styleConfidence() * 100) | number:'1.0-0' }}%</span>
            }
          </span>
        }
        @if (stack().length) {
          @for (item of stack(); track item) {
            <span class="rounded bg-surface-2 px-1.5 py-0.5 font-mono text-2xs text-ink-muted">{{ item }}</span>
          }
        }
      </div>

      <!-- Confidence Ledger (collapsed by default) -->
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

  /** Human-readable identity sentence: "ASP.NET Core web API · 85 endpoints across 3 services · EF Core + RabbitMQ" */
  protected readonly identitySentence = computed(() => {
    const s = this.summary();
    const m = this.map();
    if (!s) return 'Analyze a repo to get started.';
    const parts: string[] = [];
    if (s.label) parts.push(s.label);
    if (m?.archetype) parts.push(m.archetype.toLowerCase());
    if (s.entries > 0) parts.push(`${s.entries} endpoints`);
    if (s.projects > 1) parts.push(`${s.projects} services`);
    if (s.nodes > 0) parts.push(`${formatCompact(s.nodes)} types`);
    if (s.elapsedMs > 0) {
      const sec = (Number(s.elapsedMs) / 1000).toFixed(1);
      parts.push(`analyzed in ${sec}s`);
    }
    return parts.join(' · ') + '.';
  });

  /** Stat labels: [label, value, tooltip] */
  protected readonly statLabels = computed((): readonly (readonly [string, string, string])[] => {
    const s = this.summary();
    const l = this.ledger();
    if (!s) return [];
    const wired = s.entriesWithTarget ?? 0;
    const total = s.entries ?? 0;
    const labels: [string, string, string][] = [
      ['endpoints', String(s.entries), 'Total entry points (HTTP, consumers, handlers, workers)'],
    ];
    if (s.projects > 0) {
      labels.push(['services', String(s.projects), 'Projects in the solution']);
    }
    if (s.nodes > 0) {
      labels.push(['types', formatCompact(s.nodes), 'Types discovered in the graph']);
    }
    if (total > 0) {
      labels.push(['wired', `${wired}/${total}`, `${wired} of ${total} entries have resolved targets`]);
    }
    if (l) {
      labels.push(['confidence', `${Math.round(l.overall * 100)}%`, `${Math.round(l.verifiedEdgePct * 100)}% edges verified, ${Math.round(l.approxEdgePct * 100)}% approximate`]);
    }
    return labels;
  });

  protected readonly stale = computed(() => {
    const s = this.summary();
    return s?.stale ? (s.staleMessage || 'Repo has changed — Re-analyze?') : null;
  });

  protected readonly archetype = computed(() => this.map()?.archetype);
  protected readonly style = computed(() => this.map()?.style);
  protected readonly styleConfidence = computed(() => this.map()?.styleConfidence ?? 0);
  protected readonly stack = computed(() => this.map()?.stack ?? []);

  protected reanalyze() {
    void this.session.reAnalyze();
  }
}
