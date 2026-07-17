import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { DevContextApi } from '../data-access/devcontext-api';
import { AtlasStore } from './atlas.store';
import { WorkspaceStore } from './workspace.store';

const ENTRY = { focus: 'POST /orders', title: 'POST /orders', kind: 'HttpEndpoint' };

function flowRow(overrides: Record<string, unknown> = {}) {
  return {
    focus: 'POST /orders',
    title: 'POST /orders',
    kind: 'HttpEndpoint',
    found: true,
    nodeCount: 7,
    maxDepth: 3,
    boundaryCrossings: 2,
    dataTouches: 1,
    verifiedPct: 71,
    touchedEntities: ['Order'],
    emittedEvents: ['OrderCreatedEvent'],
    nodeIds: ['EntryPoint:POST /orders', 'Type:OrderHandler', 'Store:OrdersDb'],
    hubIds: ['EntryPoint:POST /orders', 'Type:OrderHandler'],
    score: 21,
    ...overrides,
  };
}

describe('AtlasStore server-side flow index (T7.4)', () => {
  function setup(getFlowIndex: ReturnType<typeof vi.fn>) {
    TestBed.configureTestingModule({
      providers: [{ provide: DevContextApi, useValue: { getFlowIndex } }],
    });
    const workspace = TestBed.inject(WorkspaceStore);
    const atlas = TestBed.inject(AtlasStore);
    const tabId = workspace.createTab('C:\\repo', 'repo');
    return { workspace, atlas, tabId };
  }

  it('start() fetches the index in ONE call and maps rows to FlowStats (no per-entry getTrace)', async () => {
    const getFlowIndex = vi.fn().mockResolvedValue({
      flows: [flowRow(), flowRow({ focus: 'GET /orders', title: 'GET /orders', found: false,
        nodeCount: 0, score: 0, nodeIds: [], hubIds: [], touchedEntities: [], emittedEvents: [] })],
      hubDegrees: [{ nodeId: 'Type:OrderHandler', inDegree: 4, outDegree: 6 }],
    });
    const { atlas, tabId } = setup(getFlowIndex);

    atlas.start(tabId, 'h1', [ENTRY as never]);
    expect(atlas.status()).toBe('indexing');
    await vi.waitFor(() => expect(atlas.status()).toBe('done'));

    expect(getFlowIndex).toHaveBeenCalledTimes(1);
    expect(getFlowIndex).toHaveBeenCalledWith('h1', expect.anything());

    const flows = atlas.flows();
    expect(flows).toHaveLength(2);
    const found = flows.find((f) => f.focus === 'POST /orders')!;
    expect(found.boundaryCrossings).toBe(2);
    expect(found.score).toBe(21);
    expect(found.nodeIds).toContain('Store:OrdersDb');
    expect(atlas.reachedBy('Store:OrdersDb')).toHaveLength(1);
  });

  it('seeds hub degrees from the response — the getNode enrichment fan-out is gone', async () => {
    const getFlowIndex = vi.fn().mockResolvedValue({
      flows: [flowRow(), flowRow({ focus: 'PUT /orders', title: 'PUT /orders' })],
      hubDegrees: [{ nodeId: 'Type:OrderHandler', inDegree: 4, outDegree: 6 }],
    });
    const { atlas, tabId } = setup(getFlowIndex);

    atlas.start(tabId, 'h1', [ENTRY as never]);
    await vi.waitFor(() => expect(atlas.status()).toBe('done'));

    const hub = atlas.hubsWithDegree().find((h) => h.nodeId === 'Type:OrderHandler');
    expect(hub?.degree).toEqual({ inDegree: 4, outDegree: 6 });
  });

  it('a failed index fetch lands in cancelled, not a forever-indexing state', async () => {
    const getFlowIndex = vi.fn().mockRejectedValue(new Error('boom'));
    const { atlas, tabId } = setup(getFlowIndex);

    atlas.start(tabId, 'h1', [ENTRY as never]);
    await vi.waitFor(() => expect(atlas.status()).toBe('cancelled'));
    expect(atlas.flows()).toHaveLength(0);
  });

  it('a restart supersedes the in-flight fetch — the stale response never lands', async () => {
    let resolveFirst!: (v: unknown) => void;
    const getFlowIndex = vi
      .fn()
      .mockImplementationOnce(() => new Promise((r) => { resolveFirst = r; }))
      .mockResolvedValueOnce({ flows: [flowRow({ focus: 'GET /fresh', title: 'GET /fresh' })], hubDegrees: [] });
    const { atlas, tabId } = setup(getFlowIndex);

    atlas.start(tabId, 'h1', [ENTRY as never]);
    atlas.start(tabId, 'h1', [ENTRY as never]); // supersede
    resolveFirst({ flows: [flowRow({ focus: 'GET /stale', title: 'GET /stale' })], hubDegrees: [] });
    await vi.waitFor(() => expect(atlas.status()).toBe('done'));

    expect(atlas.flows().map((f) => f.focus)).toEqual(['GET /fresh']);
  });
});
