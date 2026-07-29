import { create } from '@bufbuild/protobuf';
import { describe, expect, it } from 'vitest';

import { AnalysisSummarySchema, SessionInfoSchema } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { freshnessOf, parseAnalyzedAt } from './session.store';

/**
 * G3.3 (R4 §1 item 10) — the Freshness tile's age.
 *
 * The card used to be built from SessionInfo.age_seconds, which is how long ago the SESSION
 * opened. A snapshot-cache hit opens a BRAND-NEW session in ~200ms however old the analysis is,
 * so age_seconds came back ~0 and home-tiles printed "just now" (its threshold is 90s) over
 * numbers that could be days old. These pin the replacement: the server's analyzed_at.
 */
describe('session freshness', () => {
  const THREE_DAYS_AGO = new Date(Date.now() - 3 * 86_400_000);
  const iso = (d: Date) => d.toISOString().replace(/\.\d+Z$/, 'Z');

  it('dates a cache-served analysis by when it RAN, not when the session opened', () => {
    const summary = create(AnalysisSummarySchema, {
      analyzedAt: iso(THREE_DAYS_AGO),
      gitHead: 'a'.repeat(40),
      fromCache: true,
    });

    const f = freshnessOf(summary);

    // The old formula was Date.now() minus a ~0 session age. Anything inside home-tiles'
    // 90s "just now" window means the card is lying about a three-day-old snapshot.
    expect(Date.now() - f.analyzedAtMs).toBeGreaterThan(90_000);
    // The server's stamp is second-resolution, so allow the sub-second truncation and no more.
    expect(Math.abs(f.analyzedAtMs - THREE_DAYS_AGO.getTime())).toBeLessThan(1000);
    expect(f.fromCache).toBe(true);
    expect(f.commitSha).toBe('a'.repeat(40));
  });

  it('an adopted session carries the same instant on the wire', () => {
    const info = create(SessionInfoSchema, {
      analyzedAt: iso(THREE_DAYS_AGO),
      ageSeconds: 2n, // the session really is 2 seconds old — that is the trap
      commitSha: 'b'.repeat(40),
      fromCache: true,
    });

    const analyzedAtMs = parseAnalyzedAt(info.analyzedAt, () => Date.now() - Number(info.ageSeconds) * 1000);

    expect(Date.now() - analyzedAtMs).toBeGreaterThan(90_000);
  });

  it('falls back to the caller when the server cannot date the analysis', () => {
    // Empty outside git and on a pre-item-10 server. A degraded card beats a NaN one.
    expect(parseAnalyzedAt('', () => 1234)).toBe(1234);
    expect(parseAnalyzedAt('not-a-date', () => 1234)).toBe(1234);
    expect(freshnessOf(create(AnalysisSummarySchema, {})).analyzedAtMs).toBeGreaterThan(0);
  });
});
