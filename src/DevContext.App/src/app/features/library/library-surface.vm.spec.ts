import { describe, expect, it } from 'vitest';

import {
  defaultSection,
  entryKindCounts,
  filterGroups,
  internalTypeCount,
  namespaceCount,
  namespacesBySize,
  publicTypeCount,
  railItems,
  type LibrarySurfaceVm,
} from './library-surface.vm';

const REFIT_LIKE: LibrarySurfaceVm = {
  groups: [
    {
      namespace: 'Refit',
      types: [
        { name: 'ApiException', kind: 'Class', members: ['Create', 'GetContentAs'], doc: 'An error.' },
        { name: 'IRequestBuilder', kind: 'Interface', members: [] },
      ],
    },
    {
      namespace: 'Refit.Testing',
      types: [{ name: 'StubHttp', kind: 'Class', members: ['Add', 'CreateClient'] }],
    },
  ],
  internals: [
    { namespace: 'Refit.Internal', types: [{ name: 'Helper', kind: 'Class', members: [] }] },
  ],
  entryApi: [
    { title: '[Get]', kind: 'annotate', doc: "HTTP 'GET'.", location: 'GetAttribute.cs' },
    { title: '[Post]', kind: 'annotate' },
    { title: 'RestService', kind: 'build' },
  ],
  abstractions: [{ name: 'IHttpContentSerializer', kind: 'interface', implementorCount: 11 }],
  generators: [{ name: 'InterfaceStubGeneratorV2', kind: 'generator', doc: 'Stubs.' }],
  consumerPaths: ['annotate  →  [Get] on a partial class/member'],
};

describe('library surface vm (D4.4 F1)', () => {
  it('counts the surface: public types across groups, namespaces, internals', () => {
    expect(publicTypeCount(REFIT_LIKE)).toBe(3);
    expect(namespaceCount(REFIT_LIKE)).toBe(2);
    expect(internalTypeCount(REFIT_LIKE)).toBe(1);
    expect(publicTypeCount(undefined)).toBe(0);
  });

  it('rail carries the five CLI sections in CLI order, zero counts stay visible', () => {
    const rail = railItems(REFIT_LIKE);
    expect(rail.map((r) => r.id)).toEqual([
      'entry-api',
      'abstractions',
      'generators',
      'surface',
      'consumer-paths',
    ]);
    expect(rail.map((r) => r.count)).toEqual([3, 1, 1, 3, 1]);
    // No surface at all (defensive) — rail still enumerates all five, honestly zeroed.
    expect(railItems(undefined).map((r) => r.count)).toEqual([0, 0, 0, 0, 0]);
  });

  it('lands on ENTRY API when present, else the public surface', () => {
    expect(defaultSection(REFIT_LIKE)).toBe('entry-api');
    expect(defaultSection({ ...REFIT_LIKE, entryApi: [] })).toBe('surface');
    expect(defaultSection(undefined)).toBe('surface');
  });

  it('filters across type names, members, and namespaces; empty groups drop', () => {
    const byType = filterGroups(REFIT_LIKE.groups, 'apiexc');
    expect(byType).toHaveLength(1);
    expect(byType[0].types.map((t) => t.name)).toEqual(['ApiException']);

    const byMember = filterGroups(REFIT_LIKE.groups, 'createclient');
    expect(byMember[0].namespace).toBe('Refit.Testing');

    // Namespace match keeps the whole group.
    const byNs = filterGroups(REFIT_LIKE.groups, 'refit.testing');
    expect(byNs).toHaveLength(1);
    expect(byNs[0].types).toHaveLength(1);

    expect(filterGroups(REFIT_LIKE.groups, 'zzz-nothing')).toHaveLength(0);
    // Empty query is identity.
    expect(filterGroups(REFIT_LIKE.groups, '  ')).toBe(REFIT_LIKE.groups);
  });

  it('aggregates entry kinds in first-appearance order and ranks namespaces by size', () => {
    expect(entryKindCounts(REFIT_LIKE)).toEqual([
      { kind: 'annotate', count: 2 },
      { kind: 'build', count: 1 },
    ]);
    expect(namespacesBySize(REFIT_LIKE, 10)).toEqual([
      { namespace: 'Refit', count: 2 },
      { namespace: 'Refit.Testing', count: 1 },
    ]);
    expect(namespacesBySize(REFIT_LIKE, 1)).toHaveLength(1);
  });
});
