import { create } from '@bufbuild/protobuf';
import { describe, expect, it } from 'vitest';

import { EntryPointSchema } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { groupEntries, toEntryVm } from './view-models';

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
