/**
 * D4.4 (F1): pure view-model over MapResponse.surface — the library workbench's
 * vocabulary. Structural input types (subsets of the generated proto messages) keep
 * this DOM-free and unit-testable with plain literals, same pattern as the canvas's
 * semantics.ts. The section list mirrors the CLI's LibrarySurfaceRenderer order:
 * ENTRY API → ABSTRACTIONS → GENERATORS → PUBLIC SURFACE → CONSUMER PATHS.
 */

export interface SurfaceTypeVm {
  readonly name: string;
  readonly kind: string;
  readonly members: readonly string[];
  readonly doc?: string;
}

export interface SurfaceGroupVm {
  readonly namespace: string;
  readonly types: readonly SurfaceTypeVm[];
}

export interface SurfaceEntryVm {
  readonly title: string;
  readonly kind: string;
  readonly doc?: string;
  readonly location?: string;
}

export interface SurfaceAbstractionVm {
  readonly name: string;
  readonly kind: string;
  readonly implementorCount: number;
}

export interface SurfaceGeneratorVm {
  readonly name: string;
  readonly kind: string;
  readonly doc?: string;
}

export interface LibrarySurfaceVm {
  readonly groups: readonly SurfaceGroupVm[];
  readonly internals: readonly SurfaceGroupVm[];
  readonly entryApi: readonly SurfaceEntryVm[];
  readonly abstractions: readonly SurfaceAbstractionVm[];
  readonly generators: readonly SurfaceGeneratorVm[];
  readonly consumerPaths: readonly string[];
}

export type LibSectionId = 'entry-api' | 'abstractions' | 'generators' | 'surface' | 'consumer-paths';

export interface LibRailItem {
  readonly id: LibSectionId;
  readonly label: string;
  readonly count: number;
}

export function publicTypeCount(surface: LibrarySurfaceVm | undefined): number {
  return surface?.groups.reduce((n, g) => n + g.types.length, 0) ?? 0;
}

export function internalTypeCount(surface: LibrarySurfaceVm | undefined): number {
  return surface?.internals.reduce((n, g) => n + g.types.length, 0) ?? 0;
}

export function namespaceCount(surface: LibrarySurfaceVm | undefined): number {
  return surface?.groups.length ?? 0;
}

/** The five rail sections, CLI order, with honest counts (0 stays visible — the
 * section renders its empty line rather than silently vanishing). */
export function railItems(surface: LibrarySurfaceVm | undefined): readonly LibRailItem[] {
  return [
    { id: 'entry-api', label: 'Entry API', count: surface?.entryApi.length ?? 0 },
    { id: 'abstractions', label: 'Abstractions', count: surface?.abstractions.length ?? 0 },
    { id: 'generators', label: 'Generators', count: surface?.generators.length ?? 0 },
    { id: 'surface', label: 'Public surface', count: publicTypeCount(surface) },
    { id: 'consumer-paths', label: 'Consumer paths', count: surface?.consumerPaths.length ?? 0 },
  ];
}

/** Land where the CLI leads: ENTRY API when it exists, else the raw surface. */
export function defaultSection(surface: LibrarySurfaceVm | undefined): LibSectionId {
  return surface?.entryApi.length ? 'entry-api' : 'surface';
}

/**
 * R3 D-C (C2) — the focus token for a front door, so a consumer path is the REAL call path.
 *
 * Surface titles come off `LibrarySurfaceBuilder` in three shapes and the trace resolver takes two
 * of them verbatim: a bare type (`AbstractValidator`, from derive/implement/build/extend) already
 * resolves, and `Type.Member` (from register) is the same identity written with the wrong
 * separator — the resolver's notation is `Type:Member`. An `annotate` row names an attribute in
 * brackets (`[Get]`), whose type is the bracketless name; the resolver is asked for that and says
 * so honestly when it cannot find it, which beats refusing to try.
 */
export function focusForSurfaceEntry(title: string): string {
  const bare = title.startsWith('[') && title.endsWith(']') ? title.slice(1, -1) : title;
  const lastDot = bare.lastIndexOf('.');
  return lastDot > 0 ? `${bare.slice(0, lastDot)}:${bare.slice(lastDot + 1)}` : bare;
}

/** Case-insensitive filter over namespace, type name, and member names. Groups whose
 * namespace matches keep ALL their types; otherwise types are kept when the type name
 * or a member matches. Empty groups drop out. Empty query returns the input as-is. */
export function filterGroups(
  groups: readonly SurfaceGroupVm[],
  query: string,
): readonly SurfaceGroupVm[] {
  const q = query.trim().toLowerCase();
  if (!q) return groups;
  const result: SurfaceGroupVm[] = [];
  for (const g of groups) {
    if (g.namespace.toLowerCase().includes(q)) {
      result.push(g);
      continue;
    }
    const types = g.types.filter(
      (t) => t.name.toLowerCase().includes(q) || t.members.some((m) => m.toLowerCase().includes(q)),
    );
    if (types.length) result.push({ namespace: g.namespace, types });
  }
  return result;
}

/** Entry-API kinds with counts, first-appearance order (the builder already ranks
 * tiers) — feeds the Home tile's surface metrics. */
export function entryKindCounts(
  surface: LibrarySurfaceVm | undefined,
): readonly { kind: string; count: number }[] {
  const counts = new Map<string, number>();
  for (const e of surface?.entryApi ?? []) counts.set(e.kind, (counts.get(e.kind) ?? 0) + 1);
  return [...counts.entries()].map(([kind, count]) => ({ kind, count }));
}

/** Namespaces ranked by public-type count (desc, then name) — Home tile bars. */
export function namespacesBySize(
  surface: LibrarySurfaceVm | undefined,
  max: number,
): readonly { namespace: string; count: number }[] {
  const rows = (surface?.groups ?? [])
    .map((g) => ({ namespace: g.namespace, count: g.types.length }))
    .sort((a, b) => b.count - a.count || a.namespace.localeCompare(b.namespace));
  return rows.slice(0, max);
}
