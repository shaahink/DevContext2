import type { LogLine } from '../../state/workspace.store';

/**
 * D4.6 (L7) — the live loading waterfall, derived purely from the streamed progress
 * log the store already records (each line carries a client receipt timestamp). Stages
 * appear in first-seen order under the SERVER'S OWN names — no brittle client-side
 * phase catalog to drift (the old checklist substring-mapped stages onto a hand-kept
 * 9-phase list). A stage ends when the next one starts (the pipeline is serial); the
 * last stage ticks against `nowMs` while the run is live.
 */
export interface WaterfallRow {
  readonly stage: string;
  readonly elapsedMs: number;
  readonly active: boolean;
  readonly lastMessage: string;
  readonly lastPercent: number;
}

export function buildWaterfall(
  log: readonly LogLine[],
  nowMs: number,
  live: boolean,
): readonly WaterfallRow[] {
  const order: string[] = [];
  const byStage = new Map<string, { startMs: number; lastMessage: string; lastPercent: number }>();
  for (const line of log) {
    const existing = byStage.get(line.stage);
    if (!existing) {
      order.push(line.stage);
      byStage.set(line.stage, { startMs: line.timestamp, lastMessage: line.message, lastPercent: line.percent });
    } else {
      existing.lastMessage = line.message;
      existing.lastPercent = line.percent;
    }
  }
  return order.map((stage, i) => {
    const s = byStage.get(stage)!;
    const isLast = i === order.length - 1;
    const endMs = isLast ? nowMs : byStage.get(order[i + 1])!.startMs;
    return {
      stage,
      elapsedMs: Math.max(0, endMs - s.startMs),
      active: isLast && live,
      // Early progress events echo the stage name as their message — showing
      // "ProjectStructure ProjectStructure" reads as a bug, so blank the echo.
      lastMessage: s.lastMessage === stage ? '' : s.lastMessage,
      lastPercent: s.lastPercent,
    };
  });
}

export function formatElapsedShort(ms: number): string {
  const s = ms / 1000;
  return s < 10 ? `${s.toFixed(1)}s` : s < 60 ? `${Math.round(s)}s` : `${Math.floor(s / 60)}m ${Math.round(s % 60)}s`;
}
