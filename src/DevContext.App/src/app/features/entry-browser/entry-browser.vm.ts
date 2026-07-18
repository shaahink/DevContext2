import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { KIND_LABELS } from '../../models/view-models';

/**
 * D4.5 (L5) — pure grouping for the entry browser: service → kind → ranked entries.
 * Ranking mirrors the deck's contract (wired-first, then flow score, stable ties);
 * services order by entry count (the busiest service is the story's protagonist).
 */

export interface BrowserKindGroup {
  readonly kind: string;
  readonly label: string;
  readonly entries: readonly EntryVm[];
}

export interface BrowserServiceGroup {
  readonly service: string;
  readonly total: number;
  readonly kinds: readonly BrowserKindGroup[];
}

export function groupForBrowser(
  groups: readonly EntryGroupVm[],
  filter: string,
  kind: string | null,
): readonly BrowserServiceGroup[] {
  const query = filter.trim().toLowerCase();
  const kindOrder = new Map(groups.map((g, i) => [g.kind, i]));

  const byService = new Map<string, Map<string, EntryVm[]>>();
  for (const g of groups) {
    if (kind !== null && g.kind !== kind) continue;
    for (const e of g.entries) {
      if (query !== '' && !matches(e, query)) continue;
      const service = e.project || 'Default';
      let kinds = byService.get(service);
      if (!kinds) byService.set(service, (kinds = new Map()));
      let list = kinds.get(g.kind);
      if (!list) kinds.set(g.kind, (list = []));
      list.push(e);
    }
  }

  return [...byService.entries()]
    .map(([service, kinds]) => {
      const kindGroups = [...kinds.entries()]
        .sort((a, b) => (kindOrder.get(a[0]) ?? 99) - (kindOrder.get(b[0]) ?? 99))
        .map(([k, entries]) => ({ kind: k, label: KIND_LABELS[k] ?? k, entries: rank(entries) }));
      return {
        service,
        total: kindGroups.reduce((n, kg) => n + kg.entries.length, 0),
        kinds: kindGroups,
      };
    })
    .sort((a, b) => b.total - a.total || a.service.localeCompare(b.service));
}

function matches(e: EntryVm, query: string): boolean {
  return (
    e.title.toLowerCase().includes(query) ||
    (e.route ?? '').toLowerCase().includes(query) ||
    (e.target ?? '').toLowerCase().includes(query) ||
    (e.project ?? '').toLowerCase().includes(query)
  );
}

function rank(entries: readonly EntryVm[]): readonly EntryVm[] {
  return entries
    .map((e, i) => ({ e, i }))
    .sort(
      (a, b) =>
        Number(!!b.e.target) - Number(!!a.e.target) ||
        (b.e.score ?? 0) - (a.e.score ?? 0) ||
        a.i - b.i,
    )
    .map((x) => x.e);
}
