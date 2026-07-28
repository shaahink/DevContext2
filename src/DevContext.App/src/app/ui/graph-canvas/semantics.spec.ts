import { describe, expect, it } from 'vitest';

import { classifyTransport, hasDataStore, serviceKindGlyph, serviceLabel } from './semantics';

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
    // An AppHost reference is a deployment fact, not a protocol — labelled, not guessed.
    expect(classifyTransport('aspire-reference')).toEqual({ label: 'apphost', cls: 'other' });
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

  it('detects the RoleTags.DataStore stack tag and renders the [db] mark', () => {
    expect(hasDataStore(['aggregate', 'datastore'])).toBe(true);
    expect(hasDataStore(['handler'])).toBe(false);
    const id = (s: string) => s;
    expect(serviceLabel('Basket.API', 'Web API', ['datastore'], id)).toBe('[API] Basket.API [db]');
    expect(serviceLabel('Worker', 'Service', [], id)).toBe('Worker');
  });
});
