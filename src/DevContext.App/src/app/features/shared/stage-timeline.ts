import { Component, computed, input } from '@angular/core';

/**
 * D4.6 (K2) — the analysis-stage TIMELINE: one gantt-style row per pipeline stage,
 * bar offset = cumulative start, width ∝ elapsed. Replaces run-console's equal-width
 * vertical bars (which showed rank but not proportion or sequence) and gives the
 * insights page its first stages view. Data = StatsResponse.stages, which persists
 * with the snapshot since D3.3 — fresh AND rehydrated sessions both render it.
 */
export interface StageTimelineRow {
  readonly stage: string;
  readonly ms: number;
  readonly leftPct: number;
  readonly widthPct: number;
}

export function timelineRows(
  stages: readonly { stage: string; elapsedMs: number | bigint }[],
): readonly StageTimelineRow[] {
  const ms = stages.map((s) => Number(s.elapsedMs));
  const total = ms.reduce((n, v) => n + v, 0);
  if (total <= 0) return [];
  let cursor = 0;
  return stages.map((s, i) => {
    const leftPct = (cursor / total) * 100;
    cursor += ms[i];
    return {
      stage: s.stage,
      ms: ms[i],
      leftPct,
      // Floor at 0.5% so a 2ms stage stays a visible tick instead of vanishing.
      widthPct: Math.max((ms[i] / total) * 100, 0.5),
    };
  });
}

@Component({
  selector: 'app-stage-timeline',
  template: `
    <div class="space-y-1">
      @for (row of rows(); track row.stage) {
        <div class="flex items-center gap-2 text-2xs">
          <span class="w-28 shrink-0 truncate text-ink-muted" [title]="row.stage">{{ row.stage }}</span>
          <div class="relative h-3 flex-1 overflow-hidden rounded-sm bg-surface-2">
            <div
              class="absolute top-0 h-full rounded-sm bg-accent"
              [style.left.%]="row.leftPct"
              [style.width.%]="row.widthPct"
              [title]="row.stage + ': ' + fmt(row.ms)"
            ></div>
          </div>
          <span class="w-14 shrink-0 text-right tabular-nums text-ink-subtle">{{ fmt(row.ms) }}</span>
        </div>
      }
    </div>
  `,
  host: { class: 'block' },
})
export class StageTimeline {
  readonly stages = input<readonly { stage: string; elapsedMs: number | bigint }[]>([]);

  protected readonly rows = computed(() => timelineRows(this.stages()));

  protected fmt(ms: number): string {
    return ms >= 1000 ? `${(ms / 1000).toFixed(1)}s` : `${ms}ms`;
  }
}
