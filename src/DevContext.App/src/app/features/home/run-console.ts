import { Component, computed, effect, inject, viewChild, ElementRef } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';
import { formatCompact } from '../../core/format';

interface PhaseStatus {
  key: string;
  label: string;
  seen: boolean;
  completed: boolean;
  lastMessage: string;
  lastPercent: number;
}

/**
 * Home's console ⇄ report (proposal §2 "console during analysis → digest"). Two
 * modes off the same live log: `boot` streams while analyzing, `report` renders once
 * `session.stats()` lands. L1.3 adds a phase checklist (no jumping bar — each phase
 * shows its live count), while keeping the detailed log below as a collapsible detail.
 */
@Component({
  selector: 'app-run-console',
  imports: [Icon],
  template: `
    <div class="console-surface" #surface>
      @if (mode() === 'boot') {
        <!-- L1.3 — phase checklist with live counts -->
        <div class="space-y-1.5 mb-4">
          @for (phase of phases(); track phase.key) {
            <div class="flex items-center gap-2 text-2xs"
              [class.text-ink-muted]="!phase.seen"
              [class.text-ink]="phase.seen && !phase.completed"
              [class.text-ink-subtle]="phase.completed">
              <!-- Status icon -->
              @if (phase.completed) {
                <span class="i-lucide-check h-3 w-3 shrink-0 text-green-400"></span>
              } @else if (phase.seen) {
                <span class="i-lucide-loader h-3 w-3 shrink-0 animate-spin text-accent"></span>
              } @else {
                <span class="i-lucide-minus h-3 w-3 shrink-0 opacity-30"></span>
              }
              <!-- Phase label -->
              <span class="w-32 shrink-0 font-medium">{{ phase.label }}</span>
              <!-- Live detail -->
              @if (phase.seen) {
                <span class="truncate">{{ phase.lastMessage }}</span>
                @if (phase.lastPercent > 0 && phase.lastPercent < 100) {
                  <span class="tabular-nums text-ink-subtle ml-auto">{{ phase.lastPercent }}%</span>
                }
              }
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
export class RunConsole {
  protected readonly session = inject(SessionStore);
  private readonly scrollAnchor = viewChild<ElementRef>('surface');

  protected readonly mode = computed<'boot' | 'report' | 'idle'>(() => {
    const status = this.session.status();
    if (status === 'analyzing' || status === 'cloning') return 'boot';
    if (status === 'ready') return this.session.stats() ? 'report' : 'boot';
    return 'idle';
  });

  /** L1.3 — derive a phase checklist from the raw log lines. Each phase is
   *  tracked by its canonical stage key; the last-seen message + percent powers
   *  the live count display. */
  protected readonly phases = computed<PhaseStatus[]>(() => {
    const log = this.session.consoleLog();
    const seen = new Map<string, { msg: string; pct: number }>();

    for (const line of log) {
      const key = normalizePhaseKey(line.stage);
      seen.set(key, { msg: line.message, pct: line.percent });
    }

    return PHASE_ORDER.map(({ key, label }) => {
      const last = seen.get(key);
      return {
        key,
        label,
        seen: last !== undefined,
        completed: last !== undefined && last.pct >= 99,
        lastMessage: last?.msg ?? '—',
        lastPercent: last?.pct ?? 0,
      };
    });
  });

  protected readonly maxStageMs = computed(() => {
    const s = this.session.stats();
    if (!s?.stages.length) return 1;
    return Math.max(...s.stages.map((st) => Number(st.elapsedMs)));
  });

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
  }

  protected num(v: bigint): number { return Number(v); }
  protected ms(v: bigint): string { return (Number(v) / 1000).toFixed(1); }
  protected ms2(v: bigint): string { return (Number(v) / 1000).toFixed(2); }
  protected pct(ms: number, max: number): number { return max > 0 ? Math.max((ms / max) * 100, 3) : 0; }
  protected fmtK(n: number): string { return formatCompact(n); }
}

function normalizePhaseKey(stage: string): string {
  const s = stage.toLowerCase();
  if (s.includes('clon') || s.includes('enumerat') || s.includes('count') || s.includes('compress') || s.includes('receiv') || s.includes('resolv') || s.includes('checkout')) return 'clone';
  if (s.includes('discov') || s.includes('cach') || s.includes('warmup')) return 'discover';
  if (s.includes('extract') || s.includes('generic') || s.includes('struct')) return 'extract';
  if (s.includes('seal') || s.includes('signal')) return 'seal';
  if (s.includes('deep') || s.includes('specific') || s.includes('roslyn')) return 'deep';
  if (s.includes('scor')) return 'score';
  if (s.includes('compress')) return 'compress';
  if (s.includes('render')) return 'render';
  if (s.includes('done') || s.includes('complete')) return 'done';
  return 'other';
}

const PHASE_ORDER = [
  { key: 'clone', label: 'Clone' },
  { key: 'discover', label: 'Discover' },
  { key: 'extract', label: 'Extract' },
  { key: 'seal', label: 'Seal' },
  { key: 'deep', label: 'Deep analysis' },
  { key: 'score', label: 'Score' },
  { key: 'compress', label: 'Compress' },
  { key: 'render', label: 'Render' },
  { key: 'done', label: 'Done' },
] as const;
