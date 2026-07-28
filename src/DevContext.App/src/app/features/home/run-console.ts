import { Component, computed, effect, inject, signal, viewChild, ElementRef, OnDestroy } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';
import { formatCompact } from '../../core/format';
import { StageTimeline } from '../shared/stage-timeline';
import { buildWaterfall, formatElapsedShort } from './waterfall.vm';

/**
 * Home's console ⇄ report (proposal §2 "console during analysis → digest"). Two
 * modes off the same live log: `boot` streams while analyzing, `report` renders once
 * `session.stats()` lands. D4.6 (L7): boot renders a LIVE WATERFALL — one ticking row
 * per pipeline stage under the server's own stage names, elapsed derived from the log's
 * receipt timestamps (the old checklist substring-mapped stages onto a hand-kept phase
 * list and showed no timing at all). K2: the report's stages render as a proportional
 * timeline (shared with the insights page) instead of equal-width bars.
 */
@Component({
  selector: 'app-run-console',
  imports: [Icon, StageTimeline],
  template: `
    <div class="console-surface" #surface>
      @if (mode() === 'boot') {
        <!-- D4.6 (L7) — live waterfall: server stage names, ticking elapsed, honest expectations -->
        <p class="mb-3 text-2xs text-ink-subtle">
          First analysis can take minutes on a large repo — the result is snapshotted, so re-runs are instant.
        </p>
        <div class="space-y-1.5 mb-4">
          @for (row of waterfall(); track row.stage) {
            <div class="flex items-center gap-2 text-2xs"
              [class.text-ink]="row.active"
              [class.text-ink-subtle]="!row.active">
              @if (row.active) {
                <span class="i-lucide-loader h-3 w-3 shrink-0 animate-spin text-accent"></span>
              } @else {
                <span class="i-lucide-check h-3 w-3 shrink-0 text-green-400"></span>
              }
              <span class="w-32 shrink-0 truncate font-medium" [title]="row.stage">{{ row.stage }}</span>
              <span class="min-w-0 truncate">{{ row.lastMessage }}</span>
              @if (row.active && row.lastPercent > 0 && row.lastPercent < 100) {
                <span class="shrink-0 tabular-nums text-ink-subtle">{{ row.lastPercent }}%</span>
              }
              <span class="ml-auto shrink-0 tabular-nums" [class.text-accent]="row.active">{{ elapsed(row.elapsedMs) }}</span>
            </div>
          } @empty {
            <div class="flex items-center gap-2 text-2xs text-ink-muted">
              <span class="i-lucide-loader h-3 w-3 shrink-0 animate-spin text-accent"></span>
              Starting analysis…
            </div>
          }
        </div>

        <!-- Detailed log (collapsible) -->
        <details class="mt-2">
          <summary class="cursor-pointer text-2xs text-ink-subtle hover:text-ink-muted transition-colors">Show raw log</summary>
          <div class="console-log mt-2">
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
        </details>
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
                <h3 class="console-section-title">Stage timeline</h3>
                <!-- D4.6 (K2) — proportional gantt rows; persists with the snapshot (D3.3),
                     so rehydrated sessions show the ORIGINAL run's timings verbatim. -->
                <app-stage-timeline [stages]="s.stages" />
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
                    <span class="text-ink">{{ textCacheHitRate() }}% hit</span>
                    <span class="text-ink-subtle">({{ s.cache.textHits }} hits / {{ s.cache.textMisses }} misses)</span>
                  </div>
                }
                @if (s.cache.syntaxTreeHits + s.cache.syntaxTreeMisses > 0) {
                  <div class="flex items-center gap-1 text-2xs tabular-nums">
                    <span class="text-ink-muted">syntax</span>
                    <span class="text-ink">{{ syntaxCacheHitRate() }}% hit</span>
                    <span class="text-ink-subtle">({{ s.cache.syntaxTreeHits }} hits / {{ s.cache.syntaxTreeMisses }} misses)</span>
                  </div>
                }
                @if (s.cache.textHits + s.cache.textMisses + s.cache.syntaxTreeHits + s.cache.syntaxTreeMisses === 0) {
                  <span class="text-2xs text-ink-muted">cold run — no cache reuse</span>
                }
              </div>
            }

            <!-- S9 contract sweep — J1/J3's swallowed-failure counters have ridden this exact
                 stats payload since the silent-failure amnesty, and the CLI prints a table of them
                 (AnalyzeCommand's "Failures are surfaced after the display closes, never swallowed").
                 The app rendered every other section of the same object and dropped this one, so a
                 desktop reader was the only reader who could not tell a clean run from a lossy one.
                 Absent when the run was clean: a zero row on every repo is noise. -->
            @if (extractionFailures().length) {
              <div>
                <h3 class="console-section-title">Swallowed failures</h3>
                <p class="mb-1.5 text-2xs text-ink-subtle">
                  Extraction continued past these — what they touched is missing from the graph, not wrong in it.
                </p>
                <div class="overflow-x-auto rounded border border-warn/40">
                  <table class="w-full text-left text-2xs">
                    <thead>
                      <tr class="border-b border-line bg-surface-2 text-ink-muted">
                        <th class="px-2 py-1 font-medium">Source</th>
                        <th class="px-2 py-1 font-medium">Category</th>
                        <th class="px-2 py-1 w-12 text-right tabular-nums">Count</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-line">
                      @for (f of extractionFailures(); track f.source + f.category) {
                        <tr class="hover:bg-surface-2" [title]="f.sample">
                          <td class="px-2 py-1 text-ink">{{ f.source }}</td>
                          <td class="px-2 py-1 text-ink-muted">{{ f.category }}</td>
                          <td class="px-2 py-1 text-right tabular-nums text-warn">{{ f.count }}</td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
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
  `,
  host: { class: 'contents' },
})
export class RunConsole implements OnDestroy {
  protected readonly session = inject(SessionStore);
  private readonly scrollAnchor = viewChild<ElementRef>('surface');

  protected readonly mode = computed<'boot' | 'report' | 'idle'>(() => {
    const status = this.session.status();
    if (status === 'analyzing' || status === 'cloning') return 'boot';
    if (status === 'ready') return this.session.stats() ? 'report' : 'boot';
    return 'idle';
  });

  /** D4.6 (L7) — a 500ms clock drives the active row's ticking elapsed; runs only in
   * boot mode (the effect below manages it) so an idle Home costs nothing. */
  private readonly now = signal(Date.now());
  private clock: ReturnType<typeof setInterval> | null = null;

  protected readonly waterfall = computed(() =>
    buildWaterfall(this.session.consoleLog(), this.now(), this.mode() === 'boot'),
  );

  protected elapsed(ms: number): string {
    return formatElapsedShort(ms);
  }

  protected readonly textCacheHitRate = computed(() => {
    const c = this.session.stats()?.cache;
    if (!c || c.textHits + c.textMisses === 0) return 0;
    return Math.round(c.textHits / (c.textHits + c.textMisses) * 100);
  });

  protected readonly syntaxCacheHitRate = computed(() => {
    const c = this.session.stats()?.cache;
    if (!c || c.syntaxTreeHits + c.syntaxTreeMisses === 0) return 0;
    return Math.round(c.syntaxTreeHits / (c.syntaxTreeHits + c.syntaxTreeMisses) * 100);
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

  /** S9 — loudest first, so a 400-count parse failure is not below a 1-count one. */
  protected readonly extractionFailures = computed(() => {
    const f = this.session.stats()?.extractionFailures ?? [];
    return [...f].sort((a, b) => b.count - a.count);
  });

  protected readonly topExtractors = computed(() => {
    const ex = this.session.stats()?.extractors ?? [];
    return [...ex].sort((a, b) => Number(b.elapsedMs) - Number(a.elapsedMs)).slice(0, 10);
  });

  constructor() {
    effect(() => {
      if (this.mode() !== 'boot') return;
      this.session.consoleLog();
      const el = this.scrollAnchor()?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    });
    // Start/stop the waterfall clock with boot mode.
    effect(() => {
      const boot = this.mode() === 'boot';
      if (boot && this.clock === null) {
        this.clock = setInterval(() => this.now.set(Date.now()), 500);
      } else if (!boot && this.clock !== null) {
        clearInterval(this.clock);
        this.clock = null;
      }
    });
  }

  ngOnDestroy(): void {
    if (this.clock !== null) clearInterval(this.clock);
  }

  protected ms(v: bigint): string { return (Number(v) / 1000).toFixed(1); }
  protected ms2(v: bigint): string { return (Number(v) / 1000).toFixed(2); }
  protected fmtK(n: number): string { return formatCompact(n); }
}
