import { describe, expect, it } from 'vitest';

import type { EntryGroupVm } from '../../models/view-models';
import type { TrailStep } from '../../state/trail.store';
import type { LibrarySurfaceVm } from '../library/library-surface.vm';
import {
  ARCHETYPE_PRESET_SIZE,
  archetypeProposal,
  findEntryByFocus,
  proposeFromTrail,
  seedsFromSteps,
  symbolCardSeeds,
} from './pack-proposal';

function step(kind: TrailStep['kind'], id: string, title: string, focus: string): TrailStep {
  return { kind, id, title, focus, ts: 1 };
}

function groups(...entries: { nodeId: string; title: string; focus: string; kind?: string; score?: number; route?: string }[]): EntryGroupVm[] {
  return [{
    kind: 'http',
    label: 'HTTP',
    entries: entries.map((e) => ({ kind: e.kind ?? 'HttpEndpoint', title: e.title, nodeId: e.nodeId, focus: e.focus, score: e.score, route: e.route })),
  }];
}

function surface(...types: { namespace: string; name: string; members: string[] }[]): LibrarySurfaceVm {
  const byNs = new Map<string, { name: string; kind: string; members: readonly string[] }[]>();
  for (const t of types) {
    const list = byNs.get(t.namespace) ?? [];
    list.push({ name: t.name, kind: 'class', members: t.members });
    byNs.set(t.namespace, list);
  }
  return {
    groups: [...byNs].map(([namespace, ts]) => ({ namespace, types: ts })),
    internals: [], entryApi: [], abstractions: [], generators: [], consumerPaths: [],
  };
}

describe('pack-proposal — seedsFromSteps (N3.1)', () => {
  it('resolves by FOCUS and carries the LIVE title, not the one captured at push time', () => {
    const g = groups({ nodeId: 'node-v2', title: 'POST /checkout (renamed)', focus: 'Checkout.Post' });

    const { seeds, unresolved } = seedsFromSteps([step('entry', 'node-v1', 'POST /checkout', 'Checkout.Post')], g);

    expect(unresolved).toBe(0);
    expect(seeds).toEqual([
      { type: 'flow', title: 'Flow: POST /checkout (renamed)', entryIds: ['node-v2'], estimatedLines: 15 },
    ]);
  });

  it('dedupes steps that land on the same entry and counts the ones that do not resolve', () => {
    const g = groups({ nodeId: 'node-checkout', title: 'POST /checkout', focus: 'Checkout.Post' });

    const { seeds, unresolved } = seedsFromSteps([
      step('node', 'node-handler', 'CheckoutHandler.Handle', 'Checkout.Post'),
      step('entry', 'node-checkout', 'POST /checkout', 'Checkout.Post'),
      step('entry', 'node-gone', 'DELETE /legacy', 'Legacy.Delete'),
      step('reroot', 'node-x', 'CheckoutHandler', ''),   // a reroot carries no focus at all
    ], g);

    expect(seeds).toHaveLength(1);
    expect(unresolved).toBe(2);
  });
});

describe('pack-proposal — the default state (N3.1)', () => {
  const g = groups(
    { nodeId: 'node-checkout', title: 'POST /checkout', focus: 'Checkout.Post' },
    { nodeId: 'node-orders', title: 'GET /orders', focus: 'Orders.Get' },
  );

  it('pins WIN over the raw trail — a pin is an explicit "this one matters"', () => {
    const proposal = proposeFromTrail(
      [step('entry', 'node-orders', 'GET /orders', 'Orders.Get')],
      [step('entry', 'node-checkout', 'POST /checkout', 'Checkout.Post'),
       step('entry', 'node-orders', 'GET /orders', 'Orders.Get')],
      g,
    );

    expect(proposal?.seeds.map((s) => s.entryIds)).toEqual([['node-orders']]);
    expect(proposal?.source).toBe('1 pinned step');
  });

  it('falls back to the trail, and names how many steps it used', () => {
    const proposal = proposeFromTrail([], [
      step('entry', 'node-checkout', 'POST /checkout', 'Checkout.Post'),
      step('entry', 'node-orders', 'GET /orders', 'Orders.Get'),
    ], g);

    expect(proposal?.seeds).toHaveLength(2);
    expect(proposal?.source).toBe('your trail (2 steps)');
  });

  it('is null when there is nothing, and null when nothing resolves — never an empty proposal', () => {
    expect(proposeFromTrail([], [], g)).toBeNull();
    expect(proposeFromTrail([step('reroot', 'x', 'X', '')], [], g)).toBeNull();
  });
});

describe('pack-proposal — archetypeProposal (N3.1)', () => {
  it('an app gets its top flows, ranked by the ONE ranking rule and capped', () => {
    const many = groups(
      ...Array.from({ length: 5 }, (_, i) => ({
        nodeId: `node-${i}`, title: `GET /r${i}`, focus: `R${i}.Get`, score: i / 10,
      })),
    );

    const proposal = archetypeProposal(many, undefined, false);

    expect(proposal?.seeds).toHaveLength(ARCHETYPE_PRESET_SIZE);
    // rankFlows sorts by score DESC inside the band, so the highest-scored entries lead.
    expect(proposal?.seeds.map((s) => s.entryIds[0])).toEqual(['node-4', 'node-3', 'node-2']);
    expect(proposal?.source).toBe("this repo's top 3 flows");
  });

  it('a library gets its WIDEST public types instead, as one multi-focus card set', () => {
    const proposal = archetypeProposal([], surface(
      { namespace: 'FluentValidation', name: 'IValidator', members: ['Validate'] },
      { namespace: 'FluentValidation', name: 'AbstractValidator', members: ['RuleFor', 'Validate', 'When'] },
    ), true);

    // Widest first; both focuses namespace-qualified, the notation the server's resolver takes.
    expect(proposal?.seeds[0].entryIds).toEqual([
      'FluentValidation.AbstractValidator', 'FluentValidation.IValidator',
    ]);
    expect(proposal?.seeds.map((s) => s.type)).toContain('usage');
    expect(proposal?.source).toBe("this library's 2 widest public types");
  });

  it('proposes nothing rather than an empty pack when the repo has neither', () => {
    expect(archetypeProposal([], undefined, false)).toBeNull();
    expect(archetypeProposal([], undefined, true)).toBeNull();
  });
});

describe('pack-proposal — symbolCardSeeds + findEntryByFocus (N3.1)', () => {
  it('sends the entry id VERBATIM — the server strips the NodeKind prefix, not the client', () => {
    const seeds = symbolCardSeeds('Member:Acme.OrderService::Handle', 'OrderService.Handle');

    expect(seeds.map((s) => s.type)).toEqual(['flow', 'bodies', 'usage']);
    expect(seeds.every((s) => s.entryIds[0] === 'Member:Acme.OrderService::Handle')).toBe(true);
  });

  it('findEntryByFocus matches on focus and returns null for a focus this graph has lost', () => {
    const g = groups({ nodeId: 'node-checkout', title: 'POST /checkout', focus: 'Checkout.Post' });

    expect(findEntryByFocus(g, 'Checkout.Post')?.nodeId).toBe('node-checkout');
    expect(findEntryByFocus(g, 'node-checkout')).toBeNull();
  });
});
