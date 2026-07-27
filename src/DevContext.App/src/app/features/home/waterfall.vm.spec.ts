import { describe, expect, it } from 'vitest';

import { buildWaterfall, formatElapsedShort } from './waterfall.vm';
import { timelineRows } from '../shared/stage-timeline';

const T0 = 1_000_000;
const LOG = [
  { stage: 'Discovery', message: 'scanning projects', percent: 5, timestamp: T0 },
  { stage: 'Discovery', message: '125 projects', percent: 10, timestamp: T0 + 400 },
  { stage: 'Extract (generic)', message: '230 types', percent: 40, timestamp: T0 + 1000 },
  { stage: 'Semantic upgrade', message: 'binding', percent: 70, timestamp: T0 + 3000 },
];

describe('loading waterfall vm (D4.6 L7)', () => {
  it('one row per stage, first-seen order, server names verbatim', () => {
    const rows = buildWaterfall(LOG, T0 + 5000, true);
    expect(rows.map((r) => r.stage)).toEqual(['Discovery', 'Extract (generic)', 'Semantic upgrade']);
  });

  it('a stage ends when the next begins; the last row ticks against now while live', () => {
    const rows = buildWaterfall(LOG, T0 + 5000, true);
    expect(rows[0].elapsedMs).toBe(1000); // T0 → next stage start
    expect(rows[1].elapsedMs).toBe(2000);
    expect(rows[2].elapsedMs).toBe(2000); // now − start, still ticking
    expect(rows[2].active).toBe(true);
    expect(rows[0].active).toBe(false);
  });

  it('rows carry the stage\'s LAST message/percent; live=false marks nothing active', () => {
    const rows = buildWaterfall(LOG, T0 + 5000, false);
    expect(rows[0].lastMessage).toBe('125 projects');
    expect(rows[0].lastPercent).toBe(10);
    expect(rows.every((r) => !r.active)).toBe(true);
    expect(buildWaterfall([], T0, true)).toEqual([]);
  });

  it('a message that merely echoes the stage name is blanked, not doubled', () => {
    const rows = buildWaterfall(
      [{ stage: 'ProjectStructure', message: 'ProjectStructure', percent: 15, timestamp: T0 }],
      T0 + 100,
      true,
    );
    expect(rows[0].lastMessage).toBe('');
  });

  it('formats elapsed for humans at each magnitude', () => {
    expect(formatElapsedShort(1400)).toBe('1.4s');
    expect(formatElapsedShort(42_000)).toBe('42s');
    expect(formatElapsedShort(95_000)).toBe('1m 35s');
  });
});

describe('stage timeline rows (D4.6 K2)', () => {
  it('bars are proportional and cumulative — a gantt, not a bar chart', () => {
    const rows = timelineRows([
      { stage: 'Discovery', elapsedMs: 1000 },
      { stage: 'SemanticLite', elapsedMs: 2000 },
      { stage: 'Render', elapsedMs: 1000 },
    ]);
    expect(rows.map((r) => r.leftPct)).toEqual([0, 25, 75]);
    expect(rows.map((r) => r.widthPct)).toEqual([25, 50, 25]);
  });

  it('tiny stages stay visible (0.5% floor); bigint elapsed accepted; zero total = no rows', () => {
    const rows = timelineRows([
      { stage: 'fw', elapsedMs: 2n },
      { stage: 'bind', elapsedMs: 79_717n },
    ]);
    expect(rows[0].widthPct).toBe(0.5);
    expect(rows[1].widthPct).toBeGreaterThan(99);
    expect(timelineRows([])).toEqual([]);
    expect(timelineRows([{ stage: 'x', elapsedMs: 0 }])).toEqual([]);
  });
});
