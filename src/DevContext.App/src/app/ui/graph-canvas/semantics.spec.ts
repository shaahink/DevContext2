import { describe, expect, it } from 'vitest';

import { classifyTransport, hasDataStore, isTraffic, serviceKindGlyph, serviceLabel, storeLabel } from './semantics';

describe('canvas semantics (D4.2 vocabulary)', () => {
  it('classifies the engine transport tags seen in the wild', () => {
    expect(classifyTransport('http')).toEqual({ label: 'HTTP', cls: 'HTTP' });
    expect(classifyTransport('typed-client')).toEqual({ label: 'HTTP', cls: 'HTTP' });
    expect(classifyTransport('bus')).toEqual({ label: 'queue', cls: 'queue' });
    expect(classifyTransport('AzureStorageQueue')).toEqual({ label: 'queue', cls: 'queue' });
    expect(classifyTransport('RabbitMQ')).toEqual({ label: 'queue', cls: 'queue' });
    expect(classifyTransport('grpc')).toEqual({ label: 'gRPC', cls: 'gRPC' });
    expect(classifyTransport('integration-event')).toEqual({ label: 'event', cls: 'event' });
  });

  it('classifies the Batch B transport tags (sync clients + AppHost references)', () => {
    // A typed client pointed straight at a service address is HTTP; refit is HTTP too.
    expect(classifyTransport('http-direct')).toEqual({ label: 'HTTP', cls: 'HTTP' });
    expect(classifyTransport('http-via-gateway')).toEqual({ label: 'HTTP', cls: 'HTTP' });
    expect(classifyTransport('refit-direct')).toEqual({ label: 'HTTP', cls: 'HTTP' });
    // An AppHost reference is a deployment fact, not a protocol — labelled, not guessed. R3 D-B
    // gives it its own class so the canvas can draw it recessively instead of letting it compete
    // with calls: `apphost` repeated nine times was the loudest thing on eShop's topology.
    expect(classifyTransport('aspire-reference')).toEqual({ label: 'deployment ref', cls: 'deploy' });
  });

  it('separates traffic from deployment facts (R3 D-B: only traffic draws a label)', () => {
    expect(isTraffic('HTTP')).toBe(true);
    expect(isTraffic('queue')).toBe(true);
    expect(isTraffic('gRPC')).toBe(true);
    expect(isTraffic('event')).toBe(true);
    expect(isTraffic('other')).toBe(true);
    expect(isTraffic('deploy')).toBe(false);
  });

  it('unknown tags stay verbatim (honesty over taxonomy), truncated when long', () => {
    expect(classifyTransport('carrier-pigeon')).toEqual({ label: 'carrier-pig…', cls: 'other' });
    expect(classifyTransport('ipc')).toEqual({ label: 'ipc', cls: 'other' });
  });

  it('maps ClassifyService kinds to glyphs; plain Service gets none', () => {
    expect(serviceKindGlyph('Web API')).toBe('API');
    expect(serviceKindGlyph('Gateway')).toBe('GW');
    expect(serviceKindGlyph('gRPC')).toBe('RPC');
    expect(serviceKindGlyph('Service')).toBe('');
  });

  it('covers the kinds R3 D-B revived the classifier to produce', () => {
    // Before D-B every service classified as "Service" (the classifier read a Layer that service
    // nodes never carry), so every one of these glyphs was unreachable in the product.
    expect(serviceKindGlyph('Worker')).toBe('JOB');
    expect(serviceKindGlyph('UI')).toBe('UI');
    expect(serviceKindGlyph('Functions')).toBe('FN');
    expect(serviceKindGlyph('CLI')).toBe('CLI');
    expect(serviceKindGlyph('GraphQL')).toBe('GQL');
    expect(serviceKindGlyph('Realtime')).toBe('HUB');
    expect(serviceKindGlyph('Grains')).toBe('GRAIN');
    expect(serviceKindGlyph('Library')).toBe('LIB');
  });

  it('detects the RoleTags.DataStore stack tag and renders the [db] mark', () => {
    expect(hasDataStore(['aggregate', 'datastore'])).toBe(true);
    expect(hasDataStore(['handler'])).toBe(false);
    const id = (s: string) => s;
    expect(serviceLabel('Basket.API', 'Web API', ['datastore'], id)).toBe('[API] Basket.API [db]');
    expect(serviceLabel('Worker', 'Service', [], id)).toBe('Worker');
  });

  it('yields the [db] mark to a drawn store, so the same fact is not stated twice', () => {
    const id = (s: string) => s;
    // Code-level evidence only: the mark is the sole store signal, so it stays.
    expect(serviceLabel('Basket.API', 'Web API', ['datastore'], id, 0)).toBe('[API] Basket.API [db]');
    // The deployment declared the resource and the canvas draws it — the mark stands down.
    expect(serviceLabel('Basket.API', 'Web API', ['datastore'], id, 1)).toBe('[API] Basket.API');
  });

  it('labels a store with the orchestrator words, and never invents a type it was not given', () => {
    const id = (s: string) => s;
    expect(storeLabel('basketdb', 'redis', id)).toBe('[db] basketdb · redis');
    expect(storeLabel('eventbus', 'rabbitmq', id)).toBe('[db] eventbus · rabbitmq');
    expect(storeLabel('basketdb', '', id)).toBe('[db] basketdb');
  });

  it('drops a store type the name already says, in full or abbreviated', () => {
    const id = (s: string) => s;
    // eShop declares AddPostgres(...).AddDatabase("catalogdb"), so every store came back
    // "catalogdb · database" — a third of the box spent repeating the name.
    expect(storeLabel('catalogdb', 'database', id)).toBe('[db] catalogdb');
    expect(storeLabel('orderingdb', 'Database', id)).toBe('[db] orderingdb');
    expect(storeLabel('redis', 'redis', id)).toBe('[db] redis');
    // A name that merely ends in a letter the type starts with keeps its type.
    expect(storeLabel('cache', 'redis', id)).toBe('[db] cache · redis');
  });
});
