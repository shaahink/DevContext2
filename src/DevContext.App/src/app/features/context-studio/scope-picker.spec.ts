import { describe, expect, it } from 'vitest';

import type { EntryVm } from '../../models/view-models';
import { presetSeedsFor } from './scope-picker';

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
