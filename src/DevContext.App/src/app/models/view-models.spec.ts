import { create } from '@bufbuild/protobuf';
import { describe, expect, it } from 'vitest';

import { EntryPointSchema } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { groupEntries, groupServiceHops, isServiceHopGroup, toEntryVm, type TraceNodeVm } from './view-models';

function ep(init: {
  kind: string;
  title: string;
  httpMethod?: string;
  route?: string;
  target?: string;
}) {
  return create(EntryPointSchema, { nodeId: `Type:${init.title}`, ...init });
}

describe('view-models', () => {
  it('builds an HTTP focus from method + route', () => {
    const vm = toEntryVm(ep({ kind: 'HttpEndpoint', title: 'GetProducts', httpMethod: 'GET', route: '/api/products' }));
    expect(vm.focus).toBe('GET /api/products');
  });

  it('falls back to the title when not an HTTP endpoint', () => {
    const vm = toEntryVm(ep({ kind: 'PublicApi', title: 'OrderService' }));
    expect(vm.focus).toBe('OrderService');
  });

  it('groups entries by kind, HTTP first', () => {
    const groups = groupEntries([
      ep({ kind: 'PublicApi', title: 'A' }),
      ep({ kind: 'HttpEndpoint', title: 'B', httpMethod: 'GET', route: '/b' }),
      ep({ kind: 'HttpEndpoint', title: 'C', httpMethod: 'POST', route: '/c' }),
    ]);
    expect(groups[0]?.kind).toBe('HttpEndpoint');
    expect(groups[0]?.entries.length).toBe(2);
    expect(groups.at(-1)?.kind).toBe('PublicApi');
  });
});

function tn(init: Partial<TraceNodeVm> & { id: string; title: string; seam: string }): TraceNodeVm {
  return {
    kind: 'Type',
    depth: 0,
    resolution: 'Semantic',
    truncated: false,
    omitted: 0,
    tags: [],
    children: [],
    ...init,
  };
}

const hop = (title: string, extra: Partial<TraceNodeVm> = {}) =>
  tn({ id: `svc:${title}`, title, seam: 'CrossService', ...extra });

describe('groupServiceHops (R3 D-A A-2)', () => {
  it('collapses a run of sibling cross-service hops into one group', () => {
    const out = groupServiceHops([hop('Basket.API'), hop('Catalog.API'), hop('WebApp')]);
    expect(out.length).toBe(1);
    const group = out[0];
    if (!isServiceHopGroup(group)) throw new Error('expected a group');
    expect(group.services).toEqual(['Basket.API', 'Catalog.API', 'WebApp']);
    expect(group.hops).toBe(3);
    expect(group.members.length).toBe(3);
  });

  it('leaves a lone cross-service hop expanded — one hop is signal, not noise', () => {
    const out = groupServiceHops([hop('Basket.API')]);
    expect(out.length).toBe(1);
    expect(isServiceHopGroup(out[0])).toBe(false);
  });

  it('only groups CONSECUTIVE siblings, so a hop between calls keeps its place in the flow', () => {
    const call = tn({ id: 'c1', title: 'DoWork', seam: 'Calls' });
    const out = groupServiceHops([hop('A'), hop('B'), call, hop('C'), hop('D')]);
    expect(out.length).toBe(3);
    expect(isServiceHopGroup(out[0])).toBe(true);
    expect(isServiceHopGroup(out[1])).toBe(false);
    expect(isServiceHopGroup(out[2])).toBe(true);
  });

  it('counts NESTED hops and omissions, so the row states everything it hides', () => {
    const nested = hop('Basket.API', {
      omitted: 1,
      children: [hop('Ordering.API', { omitted: 6 }), hop('Webhooks.API', { omitted: 1 })],
    });
    const out = groupServiceHops([nested, hop('Catalog.API', { omitted: 2 })]);
    const group = out[0];
    if (!isServiceHopGroup(group)) throw new Error('expected a group');
    expect(group.hops).toBe(4); // Basket + 2 nested + Catalog
    expect(group.omitted).toBe(10); // 1 + 6 + 1 + 2
    expect(group.services).toContain('Ordering.API');
  });

  it('names services in trace order and de-duplicates repeats', () => {
    const out = groupServiceHops([
      hop('Ordering.API'),
      hop('Webhooks.API'),
      hop('Ordering.API', { id: 'svc:Ordering.API#2' }),
    ]);
    const group = out[0];
    if (!isServiceHopGroup(group)) throw new Error('expected a group');
    expect(group.services).toEqual(['Ordering.API', 'Webhooks.API']);
    expect(group.hops).toBe(3); // three hops, two distinct services
  });

  it('keeps the original members reachable — collapsing hides nothing', () => {
    const members = [hop('A'), hop('B')];
    const group = groupServiceHops(members)[0];
    if (!isServiceHopGroup(group)) throw new Error('expected a group');
    expect(group.members).toEqual(members);
  });

  it('passes a non-cross-service child list through untouched', () => {
    const children = [tn({ id: 'a', title: 'A', seam: 'Calls' }), tn({ id: 'b', title: 'B', seam: 'Sends' })];
    expect(groupServiceHops(children)).toEqual(children);
  });
});
