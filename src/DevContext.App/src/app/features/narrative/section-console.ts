import { afterRender, Component, computed, ElementRef, inject, viewChild } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-section-console',
  imports: [SectionCard, Icon],
  template: `
    <app-section-card id="console" title="Console" [subtitle]="subtitle()">
      <div class="console-surface" #surface>
        @if (mode() === 'boot') {
          <div class="console-log">
            @for (line of session.consoleLog(); track line.timestamp) {
              <div class="log-line">
                <span class="line-prompt">&gt;</span>
                <span class="line-stage">[{{ line.stage }}]</span>
                <span class="line-msg">{{ line.message }}</span>
                @if (line.percent > 0) {
                  <span class="line-pct tabular-nums">{{ line.percent }}%</span>
                }
              </div>
            }
            <div class="log-cursor">&#9608;</div>
          </div>
        } @else if (mode() === 'report') {
          @if (session.stats(); as s) {
            <div class="space-y-5">
              @if (s.totalWallMs) {
                <div class="report-line">
                  <span class="report-label">Total wall time</span>
                  <span class="report-value tabular-nums">{{ ms(s.totalWallMs) }}s</span>
                </div>
              }

              @if (session.consoleLog().length) {
                <div class="report-line">
                  <span class="report-label">Log entries</span>
                  <span class="report-value tabular-nums">{{ session.consoleLog().length }}</span>
                </div>
              }

              @if (s.stages.length) {
                <div>
                  <h3 class="console-section-title">Stages</h3>
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

              @if (s.funnel) {
                <div>
                  <h3 class="console-section-title">Funnel</h3>
                  <div class="space-y-2">
                    <div class="flex items-center gap-2 text-2xs">
                      <span class="w-20 shrink-0 text-ink-muted">Types</span>
                      <div class="flex-1 h-3 rounded-sm overflow-hidden bg-surface-2">
                        <div class="h-full bg-accent transition-all" [style.width]="funnelTypesPct() + '%'"></div>
                      </div>
                      <span class="tabular-nums text-ink">{{ s.funnel.typesDiscovered }} &rarr; {{ s.funnel.typesIncluded }}</span>
                    </div>
                    <div class="flex items-center gap-2 text-2xs">
                      <span class="w-20 shrink-0 text-ink-muted">Tokens</span>
                      <div class="flex-1 h-3 rounded-sm overflow-hidden bg-surface-2">
                        <div class="h-full bg-warn transition-all" [style.width]="funnelTokensPct() + '%'"></div>
                      </div>
                      <span class="tabular-nums text-ink">{{ fmtK(s.funnel.renderedTokens) }} / {{ fmtK(s.funnel.budget) }} budget</span>
                    </div>
                    <div class="flex items-center gap-2 text-2xs">
                      <span class="w-20 shrink-0 text-ink-muted">Raw in</span>
                      <span class="tabular-nums text-ink-muted">{{ fmtK(s.funnel.rawTokens) }} raw tokens</span>
                    </div>
                  </div>
                </div>
              }

              @if (s.cache) {
                <div class="flex items-center gap-3">
                  <span class="console-section-title">Cache</span>
                  @if (s.cache.textHits + s.cache.textMisses > 0) {
                    <div class="flex items-center gap-1 text-2xs tabular-nums">
                      <span class="text-ink-muted">text</span>
                      <span class="text-ink">{{ cacheHitRate() }}% hit</span>
                      <span class="text-ink-subtle">({{ s.cache.textHits }} hits / {{ s.cache.textMisses }} misses)</span>
                    </div>
                  } @else {
                    <span class="text-2xs text-ink-muted">cold run — no cache reuse</span>
                  }
                </div>
              }

              @if (s.extractors.length) {
                <div>
                  <h3 class="console-section-title">Extractors</h3>
                  <div class="overflow-x-auto rounded border border-line">
                    <table class="w-full text-left text-2xs">
                      <thead>
                        <tr class="border-b border-line bg-surface-2 text-ink-muted">
                          <th class="px-2 py-1 font-medium">Extractor</th>
                          <th class="px-2 py-1 w-14 text-right tabular-nums">Time</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-line">
                        @for (e of topExtractors(); track e.name) {
                          <tr class="hover:bg-surface-2">
                            <td class="px-2 py-1 text-ink">{{ e.name }}</td>
                            <td class="px-2 py-1 text-right tabular-nums text-ink-muted">{{ ms2(e.elapsedMs) }}s</td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  </div>
                </div>
              }
            </div>
          } @else {
            <div class="flex items-center gap-2 py-8 text-xs text-ink-subtle">
              <app-icon name="loader" [size]="14" class="animate-spin" />
              Loading report…
            </div>
          }
        } @else {
          <p class="py-8 text-center text-xs text-ink-subtle">Analyze a repo to see the live console.</p>
        }
      </div>
    </app-section-card>
  `,
  host: { class: 'contents' },
})
export class SectionConsole {
  protected readonly session = inject(SessionStore);
  private readonly scrollAnchor = viewChild<ElementRef>('surface');

  protected readonly subtitle = computed(() => {
    switch (this.mode()) {
      case 'boot': return 'Live';
      case 'report': return 'Complete';
      default: return '';
    }
  });

  protected readonly mode = computed<'boot' | 'report' | 'idle'>(() => {
    const status = this.session.status();
    if (status === 'analyzing' || status === 'cloning') return 'boot';
    if (status === 'ready') return this.session.stats() ? 'report' : 'boot';
    return 'idle';
  });

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

  protected readonly topExtractors = computed(() => {
    const ex = this.session.stats()?.extractors ?? [];
    return [...ex].sort((a, b) => Number(b.elapsedMs) - Number(a.elapsedMs)).slice(0, 10);
  });

  constructor() {
    afterRender(() => {
      if (this.mode() !== 'boot') return;
      const el = this.scrollAnchor()?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    });
  }

  protected num(v: bigint): number { return Number(v); }
  protected ms(v: bigint): string { return (Number(v) / 1000).toFixed(1); }
  protected ms2(v: bigint): string { return (Number(v) / 1000).toFixed(2); }
  protected pct(ms: number, max: number): number { return max > 0 ? Math.max((ms / max) * 100, 3) : 0; }
  protected fmtK(n: number): string { if (n >= 1000) return (n / 1000).toFixed(1) + 'K'; return String(n); }
}
