import { describe, expect, it } from 'vitest';

import { classifyServiceRoles, classifyTransport, declaredStores, hasDataStore, isTraffic, orchestratorMembership, serviceKindGlyph, serviceLabel, storeLabel } from './semantics';

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

  it('declares each resource once, whoever named it (S8 lane tail)', () => {
    // eShop: Basket.API and Ordering.API both declare the same eventbus. Two names, one RabbitMQ.
    const stores = declaredStores([
      { displayName: 'Basket.API', stores: [{ name: 'basketdb', resourceType: 'redis' }, { name: 'eventbus', resourceType: 'rabbitmq' }] },
      { displayName: 'Ordering.API', stores: [{ name: 'eventbus', resourceType: 'rabbitmq' }, { name: 'orderingdb', resourceType: 'database' }] },
    ]);
    expect(stores.map((s) => s.name)).toEqual(['basketdb', 'eventbus', 'orderingdb']);
    expect(stores.find((s) => s.name === 'eventbus')?.owners).toEqual(['Basket.API', 'Ordering.API']);
    expect(stores.find((s) => s.name === 'basketdb')?.owners).toEqual(['Basket.API']);
  });

  it('lets a named resource type win over silence, and never lets a repeat overwrite it', () => {
    const stores = declaredStores([
      { displayName: 'A', stores: [{ name: 'cache', resourceType: '' }] },
      { displayName: 'B', stores: [{ name: 'cache', resourceType: 'redis' }] },
      { displayName: 'C', stores: [{ name: 'cache', resourceType: 'valkey' }] },
    ]);
    expect(stores).toHaveLength(1);
    expect(stores[0].resourceType).toBe('redis');
    expect(stores[0].owners).toEqual(['A', 'B', 'C']);
  });

  it('is empty for the repos that declare nothing — most of the 47-pole matrix', () => {
    expect(declaredStores([{ displayName: 'FluentValidation', stores: [] }])).toEqual([]);
    expect(declaredStores([])).toEqual([]);
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

/**
 * R3 D-4 (G6.1) — one role classification, read by the canvas AND by the Atlas per-service
 * breakdown. The eShop shape is the case the strand doc found: twelve services, of which the canvas
 * drew nine boxes, framed the AppHost and trayed two, while the breakdown listed twelve identical
 * peers with nothing joining the two surfaces.
 */
describe('service roles (R3 D-4)', () => {
  const eShop = [
    { displayName: 'eShop.AppHost', orchestrates: ['Basket.API', 'Catalog.API', 'WebApp', 'ClientApp', 'HybridApp'] },
    { displayName: 'Basket.API', orchestrates: [] },
    { displayName: 'Catalog.API', orchestrates: [] },
    { displayName: 'WebApp', orchestrates: [] },
    { displayName: 'ClientApp', orchestrates: [] },
    { displayName: 'HybridApp', orchestrates: [] },
  ];
  const transports = [
    { fromService: 'WebApp', toService: 'Basket.API' },
    { fromService: 'WebApp', toService: 'Catalog.API' },
  ];

  it('accounts for every service exactly once — drawn, orchestrator, or in no relationship', () => {
    const roles = classifyServiceRoles(eShop, transports);
    expect(roles.size).toBe(eShop.length);
    expect(roles.get('eShop.AppHost')).toBe('orchestrator');
    expect(roles.get('WebApp')).toBe('linked');
    expect(roles.get('Basket.API')).toBe('linked');
    expect(roles.get('ClientApp')).toBe('isolated');
    expect(roles.get('HybridApp')).toBe('isolated');
  });

  it('draws every box when NOTHING is linked — a tray holding everything separates nothing', () => {
    const roles = classifyServiceRoles(
      [{ displayName: 'A', orchestrates: [] }, { displayName: 'B', orchestrates: [] }], []);
    expect([...roles.values()]).toEqual(['linked', 'linked']);
  });

  it('does not promote an orchestrator whose members all fell out of scope', () => {
    const roles = classifyServiceRoles(
      [{ displayName: 'Host', orchestrates: ['Gone'] }, { displayName: 'A', orchestrates: [] }],
      [{ fromService: 'A', toService: 'A' }]);
    expect(roles.get('Host')).toBe('linked');   // no members => an ordinary service, not a frame
  });

  it('never trays a frame — containment IS a relationship', () => {
    const roles = classifyServiceRoles(eShop, transports);
    expect(roles.get('eShop.AppHost')).not.toBe('isolated');
  });

  it('gives membership the same answer the roles do', () => {
    const frameOf = orchestratorMembership(eShop);
    expect(frameOf.get('Basket.API')).toBe('eShop.AppHost');
    expect(frameOf.get('ClientApp')).toBe('eShop.AppHost');
    expect(frameOf.has('eShop.AppHost')).toBe(false);
  });
});
