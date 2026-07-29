import { describe, expect, it } from 'vitest';

import { humanizeArity, nodeIdLabel } from './format';

/**
 * R3 D-4 (G6.2) — "raw metadata arity never reaches the UI".
 *
 * The engine keeps arity in canonical IDS and out of TITLES; that half is measured on real repos in
 * eval-results/2026-07-29/G6/arity-sweep-before.txt (0 titles, 248 ids across five poles). What
 * leaked was the CLIENT turning an id into a label, in eight places with eight rules. These pin the
 * one rule that replaced them.
 */
describe('nodeIdLabel', () => {
  it('spells generic arity the way C# spells an unbound generic — never the metadata marker', () => {
    expect(nodeIdLabel('Type:Microsoft.Extensions.Logging.ILogger`1')).toBe(
      'Microsoft.Extensions.Logging.ILogger<>',
    );
    expect(nodeIdLabel('Type:System.Collections.Generic.IDictionary`2')).toBe(
      'System.Collections.Generic.IDictionary<,>',
    );
  });

  it('drops the node KIND prefix — a kind is not part of a name', () => {
    // The exact defect D-4 named: the old surgery (split on dot/colon, keep the last two segments)
    // rendered this as "Service.WebApp", a node kind posing as a namespace, sitting in one list
    // beside real types.
    expect(nodeIdLabel('Service:WebApp')).toBe('WebApp');
    expect(nodeIdLabel('Service:Webhooks.API')).toBe('Webhooks.API');
    expect(nodeIdLabel('Store:identitydb')).toBe('identitydb');
  });

  it('keeps everything else — shortening is the column\'s job, not this rule\'s', () => {
    // The old rule kept the LAST TWO segments, which is what made "Logging.ILogger" + marker out of
    // a fully-qualified type: two arbitrary segments read as a namespace-qualified name.
    expect(nodeIdLabel('Type:eShop.Ordering.Domain.AggregatesModel.Order')).toBe(
      'eShop.Ordering.Domain.AggregatesModel.Order',
    );
  });

  it('handles member keys, including a generic owner', () => {
    expect(nodeIdLabel('Member:eShop.IntegrationEventLogEF.Services.IntegrationEventLogService`1::MarkEventAsFailedAsync'))
      .toBe('eShop.IntegrationEventLogEF.Services.IntegrationEventLogService<>::MarkEventAsFailedAsync');
  });

  it('does not eat a scheme inside an EntryPoint key (the colon after the kind is not the only colon)', () => {
    expect(nodeIdLabel('EntryPoint:domain:ContributorDeletedHandler')).toBe('domain:ContributorDeletedHandler');
    expect(nodeIdLabel('EntryPoint:GET /api/catalog/items')).toBe('GET /api/catalog/items');
  });

  it('leaves a string that is not a node id alone', () => {
    // Only the real NodeKind set is treated as a prefix, so a route or a bare name passes through.
    expect(nodeIdLabel('GET /api/v1.0/items')).toBe('GET /api/v1.0/items');
    expect(nodeIdLabel('CatalogApi.GetAllItemsV1')).toBe('CatalogApi.GetAllItemsV1');
    expect(nodeIdLabel('')).toBe('');
  });
});

describe('humanizeArity', () => {
  it('rewrites every marker in a nested generic chain', () => {
    expect(humanizeArity('Ns.Outer`2.Inner`1')).toBe('Ns.Outer<,>.Inner<>');
  });

  it('leaves text with no marker untouched, including a zero or malformed one', () => {
    expect(humanizeArity('Ns.Plain')).toBe('Ns.Plain');
    expect(humanizeArity('Ns.Weird`0')).toBe('Ns.Weird`0');
  });
});
