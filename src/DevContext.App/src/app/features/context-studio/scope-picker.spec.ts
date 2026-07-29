import { describe, expect, it } from 'vitest';

import type { EntryVm } from '../../models/view-models';
import { presetSeedsFor, scopePickerWithheld } from './scope-picker';

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
});
