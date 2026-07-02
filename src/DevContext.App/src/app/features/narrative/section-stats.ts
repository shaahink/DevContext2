import { Component, computed, inject } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { SEAM_COLORS } from '../../models/seam-colors';

@Component({
  selector: 'app-section-stats',
  imports: [SectionCard],
  template: `
    <app-section-card id="stats" title="Pipeline">
      @if (session.stats(); as s) {
        <div class="space-y-5">
          @if (s.stages.length) {
            <div>
              <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Stages</h3>
              <div class="flex items-end gap-1 h-8">
                @for (stage of s.stages; track stage.stage) {
                  <div
                    class="flex-1 rounded-t-sm bg-accent transition-all"
                    [style.height]="pct(num(stage.elapsedMs), maxStageMs()) + '%'"
                    [title]="stage.stage + ': ' + ms(stage.elapsedMs) + 's'"
                  ></div>
                }
              </div>
              <div class="mt-1 flex gap-1 text-2xs text-ink-subtle">
                @for (stage of s.stages; track stage.stage) {
                  <span class="flex-1 truncate text-center">{{ stage.stage }} {{ ms(stage.elapsedMs) }}s</span>
                }
              </div>
            </div>
          }

          @if (s.seams.length) {
            <div>
              <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Seams</h3>
              <div class="grid grid-cols-3 gap-1.5">
                @for (seam of s.seams; track seam.seam) {
                  <div class="flex items-center gap-1.5 rounded bg-surface px-2 py-1">
                    <span class="h-2 w-2 shrink-0 rounded-full" [style.backgroundColor]="seamColor(seam.seam)"></span>
                    <span class="font-mono text-2xs tabular-nums text-ink">{{ seam.count }}</span>
                    <span class="text-2xs text-ink-muted">{{ seam.seam }}</span>
                    @if (seam.approx) { <span class="text-ink-subtle">~{{ seam.approx }}</span> }
                  </div>
                }
              </div>
            </div>
          }

          @if (s.extractors.length) {
            <div>
              <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Extractors</h3>
              <div class="overflow-x-auto rounded border border-line">
                <table class="w-full text-left text-2xs">
                  <thead>
                    <tr class="border-b border-line bg-surface-2 text-ink-muted">
                      <th class="px-2 py-1 font-medium">Extractor</th>
                      <th class="px-2 py-1 w-16 text-right tabular-nums">Time</th>
                      <th class="px-2 py-1 w-14 text-right tabular-nums">Types</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-line">
                    @for (e of s.extractors; track e.name) {
                      <tr class="hover:bg-surface-2">
                        <td class="px-2 py-1 text-ink">{{ e.name }}</td>
                        <td class="px-2 py-1 text-right tabular-nums text-ink-muted">{{ ms2(e.elapsedMs) }}s</td>
                        <td class="px-2 py-1 text-right tabular-nums text-ink-muted">{{ e.typesAdded || '—' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          }

          <div class="grid grid-cols-3 gap-2">
            @if (s.cache) {
              <div class="rounded border border-line bg-surface p-2">
                <div class="text-2xs font-semibold text-ink-muted">Cache</div>
                @if (s.cache.textHits + s.cache.textMisses > 0) {
                  <div class="mt-1 text-xs tabular-nums text-ink">
                    {{ cacheHitRate() }}% hit
                  </div>
                  <div class="text-2xs text-ink-subtle">{{ s.cache.textHits }} hits / {{ s.cache.textMisses }} misses</div>
                } @else {
                  <div class="mt-1 text-xs text-ink-muted">cold run — no cache reuse</div>
                }
              </div>
            }
            @if (s.corpus) {
              <div class="rounded border border-line bg-surface p-2">
                <div class="text-2xs font-semibold text-ink-muted">Corpus</div>
                <div class="mt-1 text-xs tabular-nums text-ink">{{ s.corpus.totalFiles }} files · {{ s.corpus.projects }} proj</div>
              </div>
            }
            @if (s.funnel) {
              <div class="rounded border border-line bg-surface p-2">
                <div class="text-2xs font-semibold text-ink-muted">Funnel</div>
                <div class="mt-1 space-y-1">
                  <div class="flex items-center gap-1 text-2xs">
                    <span class="w-9 tabular-nums text-ink">{{ s.funnel.typesDiscovered }}</span>
                    <div class="flex-1 h-1.5 rounded-sm overflow-hidden bg-surface-2">
                      <div class="h-full bg-accent" [style.width]="funnelTypesPct() + '%'"></div>
                    </div>
                    <span class="w-9 text-right tabular-nums text-ink">{{ s.funnel.typesIncluded }}</span>
                  </div>
                  <div class="flex items-center gap-1 text-2xs">
                    <span class="w-9 tabular-nums text-ink-muted">{{ fmtK(s.funnel.rawTokens) }}</span>
                    <div class="flex-1 h-1.5 rounded-sm overflow-hidden bg-surface-2">
                      <div class="h-full bg-warn" [style.width]="funnelTokensPct() + '%'"></div>
                    </div>
                    <span class="w-9 text-right tabular-nums text-ink-muted">{{ s.funnel.budget }}</span>
                  </div>
                  <div class="text-2xs text-ink-subtle">rendered {{ fmtK(s.funnel.renderedTokens) }} tokens</div>
                </div>
              </div>
            }
          </div>
        </div>
      } @else {
        <p class="py-8 text-center text-xs text-ink-subtle">Stats loaded automatically when analysis completes.</p>
      }
    </app-section-card>
  `,
})
export class SectionStats {
  protected readonly session = inject(SessionStore);

  protected readonly maxStageMs = computed(() => {
    const s = this.session.stats();
    if (!s?.stages.length) return 1;
    return Math.max(...s.stages.map((st) => Number(st.elapsedMs)));
  });

  protected readonly cacheHitRate = computed(() => {
    const c = this.session.stats()?.cache;
    if (!c || c.textHits + c.textMisses === 0) return 0;
    return Math.round(c.textHits / (c.textHits + c.textMisses) * 100);
  });

  protected readonly funnelTypesPct = computed(() => {
    const f = this.session.stats()?.funnel;
    if (!f || f.typesDiscovered === 0) return 0;
    return Math.round(f.typesIncluded / f.typesDiscovered * 100);
  });

  protected readonly funnelTokensPct = computed(() => {
    const f = this.session.stats()?.funnel;
    if (!f || f.budget === 0) return 0;
    return Math.round(f.renderedTokens / f.budget * 100);
  });

  protected pct(ms: number, max: number): number {
    return max > 0 ? Math.max((ms / max) * 100, 3) : 0;
  }

  protected num(v: bigint): number {
    return Number(v);
  }

  protected ms(v: bigint): string {
    return (Number(v) / 1000).toFixed(1);
  }

  protected ms2(v: bigint): string {
    return (Number(v) / 1000).toFixed(2);
  }

  protected seamColor(seam: string): string {
    return SEAM_COLORS[seam] ?? '#6b7480';
  }

  protected fmt(n: number | bigint): string {
    const v = Number(n);
    if (v >= 1000) return (v / 1000).toFixed(1) + 'K';
    return String(v);
  }

  protected fmtK(n: number): string {
    if (n >= 1000) return (n / 1000).toFixed(1) + 'K';
    return String(n);
  }
}
