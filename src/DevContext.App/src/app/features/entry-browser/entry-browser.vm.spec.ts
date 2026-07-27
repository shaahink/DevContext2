import { describe, expect, it } from 'vitest';

import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { groupForBrowser } from './entry-browser.vm';

function entry(overrides: Partial<EntryVm>): EntryVm {
  return {
    kind: 'HttpEndpoint',
    title: 'GET /x',
    nodeId: `n-${Math.random().toString(36).slice(2, 8)}`,
    focus: overrides.route ?? overrides.title ?? 'GET /x',
    ...overrides,
  } as EntryVm;
}

const GROUPS: readonly EntryGroupVm[] = [
  {
    kind: 'HttpEndpoint',
    label: 'HTTP',
    entries: [
      entry({ title: 'GET /orders', route: 'GET /orders', project: 'Ordering.API', target: 'OrderService.List', score: 5, httpMethod: 'GET' }),
      entry({ title: 'POST /orders', route: 'POST /orders', project: 'Ordering.API', target: 'OrderService.Create', score: 20, httpMethod: 'POST', authAttributes: ['Authorize'] }),
      entry({ title: 'GET /items', route: 'GET /items', project: 'Catalog.API', score: 9, httpMethod: 'GET' }),
    ],
  },
  {
    kind: 'MessageConsumer',
    label: 'Bus consumers',
    entries: [entry({ title: 'OrderPlacedConsumer', kind: 'MessageConsumer', project: 'Ordering.API', target: 'Handle', score: 3 })],
  },
];

describe('entry browser grouping (D4.5 L5)', () => {
  it('groups service → kind → entries; busiest service first; kinds keep deck order', () => {
    const out = groupForBrowser(GROUPS, '', null);
    expect(out.map((s) => s.service)).toEqual(['Ordering.API', 'Catalog.API']);
    expect(out[0].total).toBe(3);
    expect(out[0].kinds.map((k) => k.kind)).toEqual(['HttpEndpoint', 'MessageConsumer']);
  });

  it('ranks wired-first then score inside a kind (the deck contract)', () => {
    const catalogFirst = groupForBrowser(GROUPS, '', null)[1];
    expect(catalogFirst.kinds[0].entries[0].route).toBe('GET /items'); // only row
    const ordering = groupForBrowser(GROUPS, '', null)[0];
    // POST (score 20) outranks GET (score 5); both wired.
    expect(ordering.kinds[0].entries.map((e) => e.route)).toEqual(['POST /orders', 'GET /orders']);
  });

  it('filter matches route, target, and service; empty groups drop', () => {
    const byTarget = groupForBrowser(GROUPS, 'orderservice.create', null);
    expect(byTarget).toHaveLength(1);
    expect(byTarget[0].total).toBe(1);

    const byService = groupForBrowser(GROUPS, 'catalog', null);
    expect(byService.map((s) => s.service)).toEqual(['Catalog.API']);

    expect(groupForBrowser(GROUPS, 'zzz', null)).toHaveLength(0);
  });

  it('kind chip narrows to one kind without losing the service grouping', () => {
    const consumersOnly = groupForBrowser(GROUPS, '', 'MessageConsumer');
    expect(consumersOnly).toHaveLength(1);
    expect(consumersOnly[0].service).toBe('Ordering.API');
    expect(consumersOnly[0].kinds).toHaveLength(1);
    expect(consumersOnly[0].kinds[0].label).toBe('Bus consumers');
  });
});
