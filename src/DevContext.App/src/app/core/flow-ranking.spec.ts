import { describe, expect, it } from 'vitest';

import { compareFlows, flowBand, isRequestShaped, pickHeroFlow, rankFlows } from './flow-ranking';

/**
 * G10.1 — the rule D-E decided (band first, magnitude second) had no spec, and the START HERE tile
 * still ran a depth filter in front of it. These cases pin the ORDER of the two, which is the only
 * thing a starved-graph threshold can invert.
 */
describe('flow band', () => {
  it('reads a caller outside the process as band 0', () => {
    for (const k of ['HttpEndpoint', 'GrpcService', 'GraphQlField', 'SignalRHub', 'FunctionEntry', 'CliCommand']) {
      expect(isRequestShaped(k)).toBe(true);
      expect(flowBand(k)).toBe(0);
    }
  });

  it('reads a person-facing surface as band 1 and everything else as band 2', () => {
    expect(flowBand('UiEntry')).toBe(1);
    expect(flowBand('PublicApi')).toBe(1);
    expect(flowBand('DomainEventHandler')).toBe(2);
    expect(flowBand('MessageConsumer')).toBe(2);
    expect(flowBand(undefined)).toBe(2);
  });

  it('ranks band before magnitude, and magnitude within a band', () => {
    const flows = [
      { kind: 'DomainEventHandler', nodeCount: 48 },
      { kind: 'HttpEndpoint', nodeCount: 3 },
      { kind: 'HttpEndpoint', nodeCount: 9 },
      { kind: 'UiEntry', nodeCount: 20 },
    ];
    expect(rankFlows(flows).map((f) => `${f.kind}:${f.nodeCount}`)).toEqual([
      'HttpEndpoint:9',
      'HttpEndpoint:3',
      'UiEntry:20',
      'DomainEventHandler:48',
    ]);
  });

  it('keeps input order for flows that tie on every key', () => {
    const a = { kind: 'HttpEndpoint', nodeCount: 4 };
    const b = { kind: 'HttpEndpoint', nodeCount: 4 };
    expect(rankFlows([a, b])[0]).toBe(a);
    expect(compareFlows(a, b)).toBe(0);
  });
});

describe('pickHeroFlow', () => {
  /**
   * RED on the pre-G10.1 code. The old body filtered to `nodeCount >= 4` BEFORE ranking, so this
   * shape — every request-shaped flow shallower than the gate, one deep internal reaction — handed
   * the tile to the internal reaction. That is the same inversion E-2 found, arriving through the
   * depth number instead of the checkout title.
   */
  it('opens on a shallow request-shaped flow rather than a deep internal reaction', () => {
    const flows = [
      { kind: 'DomainEventHandler', nodeCount: 48, focus: 'OrderPaidHandler' },
      { kind: 'HttpEndpoint', nodeCount: 3, focus: 'POST /api/orders/draft' },
    ];
    expect(pickHeroFlow(flows)?.focus).toBe('POST /api/orders/draft');
  });

  it('still prefers the deeper flow when both are request-shaped', () => {
    const flows = [
      { kind: 'HttpEndpoint', nodeCount: 3, focus: 'GET /health' },
      { kind: 'HttpEndpoint', nodeCount: 12, focus: 'POST /api/orders/draft' },
    ];
    expect(pickHeroFlow(flows)?.focus).toBe('POST /api/orders/draft');
  });

  it('prefers a UI surface over an internal reaction, and a request over both', () => {
    const ui = { kind: 'UiEntry', nodeCount: 4, focus: 'CheckoutViewModel.CheckoutAsync' };
    const internal = { kind: 'DomainEventHandler', nodeCount: 48, focus: 'OrderPaidHandler' };
    expect(pickHeroFlow([internal, ui])?.focus).toBe('CheckoutViewModel.CheckoutAsync');
    const http = { kind: 'HttpEndpoint', nodeCount: 2, focus: 'POST /api/orders/draft' };
    expect(pickHeroFlow([internal, ui, http])?.focus).toBe('POST /api/orders/draft');
  });

  it('returns null rather than a tile when there is nothing to open on', () => {
    expect(pickHeroFlow([])).toBeNull();
  });
});
