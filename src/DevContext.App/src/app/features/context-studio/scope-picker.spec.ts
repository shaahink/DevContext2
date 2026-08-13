import { describe, expect, it } from 'vitest';

import type { EntryVm } from '../../models/view-models';
import { entryRowIdentity, presetSeedsFor, scopePickerWithheld, typeCardSeeds, typeFocus } from './scope-picker';

function entry(kind: string, over: Partial<EntryVm> = {}): EntryVm {
  return { kind, title: 'T', nodeId: 'n1', focus: 'T', ...over } as EntryVm;
}

describe('presetSeedsFor (T5.4)', () => {
  it('hub method seeds hub flow + orchestrator bodies + consumer wiring', () => {
    const seeds = presetSeedsFor(entry('SignalRHub', { title: 'NotificationsHub.Send', target: 'NotificationsHub.Send' }));
    expect(seeds.map((s) => s.type)).toEqual(['flow', 'bodies', 'di_wiring', 'contracts', 'tests']);
    expect(seeds[0].title).toContain('Hub method flow');
    expect(seeds[1].title).toContain('orchestrator bodies');
    expect(seeds[2].title).toContain('Consumers and wiring');
  });

  it('worker kinds seed worker flow + the config they read', () => {
    for (const kind of ['HostedService', 'ScheduledJob', 'MessageConsumer']) {
      const seeds = presetSeedsFor(entry(kind, { title: 'OrderSyncWorker' }));
      expect(seeds.map((s) => s.type)).toEqual(['flow', 'bodies', 'config', 'contracts', 'tests']);
      expect(seeds[0].title).toContain('Worker flow');
      expect(seeds[2].title).toContain('Config read by');
    }
  });

  it('endpoints keep the endpoint shape (validators card, no worker/hub cards)', () => {
    const seeds = presetSeedsFor(entry('HttpEndpoint', { route: 'POST /orders' }));
    expect(seeds.map((s) => s.type)).toEqual(['flow', 'bodies', 'contracts', 'tests', 'tests']);
    expect(seeds[3].title).toBe('Validators for POST /orders');
    expect(seeds.every((s) => s.entryIds[0] === 'n1')).toBe(true);
  });
});

/**
 * R3 C-3 — zero entries is not "no analysis".
 *
 * The picker had one sentence for both states: "Analyze a repo to see its services and entries",
 * shown on an ANALYZED library (measured — eval-results/2026-07-29/G7/g72-withhold-sweep-*.txt).
 * An instruction the reader has already carried out is worse than no message.
 */
describe('scopePickerWithheld (R3 C-3)', () => {
  it('only tells you to analyze when nothing has been analyzed', () => {
    expect(scopePickerWithheld(false, false).text).toMatch(/analyze a repo/i);
    expect(scopePickerWithheld(false, false).reason).toBe('not-computed');
  });

  it('an analyzed repo is never told to analyze a repo', () => {
    for (const isLibrary of [true, false]) {
      const note = scopePickerWithheld(true, isLibrary);
      expect(note.text).not.toMatch(/analyze a repo/i);
      expect(note.reason).toBe('archetype');
      expect(note.text.length).toBeGreaterThanOrEqual(20);
    }
  });

  it('an analyzed library is pointed at the surface it does have', () => {
    expect(scopePickerWithheld(true, true).text).toMatch(/public surface/i);
  });

  /** N2.1 (audit §3.F.8) — the C-3 replacement was a half-fix: it sent the reader to an omnibox
   * that searched entries only, on the one repo that has none. The Types tab is a real place. */
  it('points at a place that exists, not the entries-only omnibox', () => {
    for (const isLibrary of [true, false]) {
      const note = scopePickerWithheld(true, isLibrary);
      expect(note.text).toMatch(/Types tab/);
      expect(note.text).not.toMatch(/omnibox/i);
    }
  });
});

/** N2.1 (owner decision 2) — a type-scoped pack is a different pack, and its defining card is
 * `usage`: the inbound direction that makes "who calls this" answerable on a library. */
describe('typeCardSeeds (N2.1)', () => {
  it('seeds the symbol card set, usage included, on the resolver-notation focus', () => {
    const seeds = typeCardSeeds(['FluentValidation.AbstractValidator'], 'AbstractValidator');
    expect(seeds.map((s) => s.type)).toEqual(['signatures', 'usage', 'bodies', 'flow', 'contracts', 'identity']);
    expect(seeds.every((s) => s.entryIds[0] === 'FluentValidation.AbstractValidator')).toBe(true);
    expect(seeds[1].title).toBe('Who uses AbstractValidator');
  });

  it('carries no card a type cannot answer (no route wiring, no config it reads)', () => {
    const types = typeCardSeeds(['A.B'], 'B').map((s) => s.type);
    expect(types).not.toContain('di_wiring');
    expect(types).not.toContain('config');
  });

  it('namespace-qualifies the focus, because library short names collide', () => {
    expect(typeFocus('FluentValidation.Internal', 'RuleBuilder')).toBe('FluentValidation.Internal.RuleBuilder');
    expect(typeFocus('', 'RuleBuilder')).toBe('RuleBuilder');
  });
});

/**
 * D-G row identity (audit §3.C) — five rows that render as one row are one row.
 *
 * MEASURED: eShop's bus consumers are `OrderStatusChangedTo{AwaitingValidation,Paid,Shipped,
 * StockConfirmed,Cancelled}IntegrationEventHandler`. At the picker's 26-character budget the
 * head-biased ellipsis keeps 20 leading + 5 trailing characters — and all five share exactly
 * `OrderStatusChangedTo` and `ndler`. S10 fixed the tail collision; this one survived it.
 */
describe('entryRowIdentity (N2.1, D-G)', () => {
  const handlers = ['AwaitingValidation', 'Paid', 'Shipped', 'StockConfirmed', 'Cancelled'];

  it('the label alone still collides — this is the defect being fixed', () => {
    const primaries = handlers.map((h) =>
      entryRowIdentity(entry('MessageConsumer', { title: `OrderStatusChangedTo${h}IntegrationEventHandler` })).primary);
    expect(new Set(primaries).size).toBe(1);
  });

  it('the row is distinguishable once it carries what the entry dispatches to', () => {
    const rows = handlers.map((h) =>
      entryRowIdentity(entry('MessageConsumer', {
        title: `OrderStatusChangedTo${h}IntegrationEventHandler`,
        target: `Set${h}StatusCommand`,
        project: 'Ordering.API',
      })));
    expect(new Set(rows.map((r) => r.primary + '|' + r.secondary)).size).toBe(5);
    expect(new Set(rows.map((r) => r.tooltip)).size).toBe(5);
  });

  it('falls back to the project when an entry dispatches to nothing named', () => {
    const row = entryRowIdentity(entry('HostedService', { title: 'PriceWorker', project: 'Web' }));
    expect(row.secondary).toBe('Web');
  });

  it('a route keeps its tail and never repeats itself in the sub-line', () => {
    const row = entryRowIdentity(entry('HttpEndpoint', {
      title: 'CatalogController.GetItemById',
      route: '/api/v1/catalog/items/{id:int}',
      httpMethod: 'GET',
      target: 'CatalogController.GetItemById',
      project: 'Catalog.API',
    }));
    expect(row.primary.endsWith('{id:int}')).toBe(true);
    expect(row.secondary).toBe('Catalog.API');
    expect(row.tooltip).toContain('GET /api/v1/catalog/items/{id:int}');
    expect(row.tooltip).toContain('Catalog.API');
  });
});
