import { rankFlows } from '../../core/flow-ranking';
import type { ContextCardSeed, PackProposal } from '../../models/context-card';
import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import type { TrailStep } from '../../state/trail.store';
import type { LibrarySurfaceVm } from '../library/library-surface.vm';
import { typeCardSeeds, typeFocus } from './scope-picker';

/**
 * N3.1 (audit §4 Room 1 / owner decision 3) — what Studio PROPOSES, as pure functions.
 *
 * The audit's requirement is that Studio "never opens empty after exploration": its default state is
 * the current trail (and pins) rendered as a proposed pack, and on a fresh session the archetype
 * preset. Explore, Insights and the NodeCard all send INTO the same shaping, so the seed-building
 * rules live here rather than inside the Studio component — one join, four callers, and each rule is
 * pinned by a spec instead of by a screenshot.
 *
 * Every seed carries entry ids the SERVER resolves: `ContextPackBuilder.ResolveCardFocuses` tries the
 * declared-entry inventory first and then `NormalizeSymbolFocus` + `ResolveEntry` — the same path
 * `get_context` uses (N2.1). MEASURED 2026-08-14 in `ContextPackBuilder.cs`: the normalizer strips a
 * NodeKind prefix, so a raw `Type:Acme.OrderService` / `Member:Acme.OrderService::Handle` node id and
 * a bare `RuleFor` are both legal entry ids. Nothing here needs a client-side symbol lookup.
 */
/** Resolution by FOCUS against the live entry inventory — never by a stored nodeId, which a
 * re-analyze can move (the trap N1.1's card invalidation was about). */
export function findEntryByFocus(
  groups: readonly EntryGroupVm[],
  focus: string,
): EntryVm | null {
  for (const group of groups) {
    for (const e of group.entries) {
      if (e.focus === focus) return e;
    }
  }
  return null;
}

export interface StepSeeds {
  readonly seeds: readonly ContextCardSeed[];
  /** Steps whose focus no longer names an entry in this graph. Reported, never swallowed. */
  readonly unresolved: number;
}

/**
 * One flow card per distinct step that still resolves. Every step KIND resolves through its `focus`
 * (a node step carries the focus of the trace it was explored under — workbench-page.ts `onNode`),
 * so a pinned graph node is worth exactly as much as a pinned entry.
 *
 * The LIVE entry's title is used, not the step's: the step's was captured at push time and a
 * re-analyze can have renamed it.
 */
export function seedsFromSteps(
  steps: readonly TrailStep[],
  groups: readonly EntryGroupVm[],
): StepSeeds {
  const seeds: ContextCardSeed[] = [];
  const seen = new Set<string>();
  let unresolved = 0;
  for (const step of steps) {
    const found = step.focus ? findEntryByFocus(groups, step.focus) : null;
    if (!found) {
      unresolved++;
      continue;
    }
    if (seen.has(found.nodeId)) continue;
    seen.add(found.nodeId);
    seeds.push({
      type: 'flow',
      title: `Flow: ${found.title}`,
      entryIds: [found.nodeId],
      estimatedLines: 15,
    });
  }
  return { seeds, unresolved };
}

/**
 * The default state after exploration: PINS win over the raw trail when there are any — a pin is an
 * explicit "this one matters", the trail is just where the user has been (N1.2's rule, unchanged).
 * Null when neither has anything that resolves; the caller falls back to the archetype preset.
 */
export function proposeFromTrail(
  pins: readonly TrailStep[],
  steps: readonly TrailStep[],
  groups: readonly EntryGroupVm[],
): PackProposal | null {
  const fromPins = pins.length > 0;
  const source = fromPins ? pins : steps;
  if (source.length === 0) return null;
  const { seeds } = seedsFromSteps(source, groups);
  if (seeds.length === 0) return null;
  const noun = fromPins
    ? `${seeds.length} pinned step${seeds.length === 1 ? '' : 's'}`
    : `your trail (${seeds.length} step${seeds.length === 1 ? '' : 's'})`;
  return { seeds, source: noun };
}

/**
 * How many roots a fresh-session proposal opens with. Three, because a proposal is something the
 * reader EDITS DOWN: at the 8000-token default a three-flow pack still leaves room for the bodies
 * the reader adds, and a wrong guess costs three removals rather than ten.
 */
export const ARCHETYPE_PRESET_SIZE = 3;

/**
 * The fresh-session proposal (audit §4: "on a fresh session: the archetype preset — app → top flows;
 * library → top public types once scope converges"). Scope converged in N2.1, so both branches are
 * real here.
 *
 * The app branch ranks with `rankFlows` — the ONE ranking rule Home's Top flows, the START HERE tile
 * and Atlas already share. Deriving a fourth "top flows" answer here is exactly the divergence
 * R3 D-E closed.
 *
 * The library branch takes the widest public types (member count, then name for determinism) and
 * hands them to `typeCardSeeds` as ONE multi-focus card set — the same thing the picker's Types tab
 * emits when three rows are selected.
 */
export function archetypeProposal(
  groups: readonly EntryGroupVm[],
  surface: LibrarySurfaceVm | undefined,
  isLibrary: boolean,
): PackProposal | null {
  if (isLibrary) {
    const types = (surface?.groups ?? []).flatMap((g) =>
      g.types.map((t) => ({ focus: typeFocus(g.namespace, t.name), name: t.name, members: t.members.length })));
    if (types.length === 0) return null;
    const top = [...types]
      .sort((a, b) => (b.members - a.members) || a.focus.localeCompare(b.focus))
      .slice(0, ARCHETYPE_PRESET_SIZE);
    const label = top.length === 1 ? top[0].name : `${top.length} public types`;
    return {
      seeds: typeCardSeeds(top.map((t) => t.focus), label),
      source: `this library's ${top.length === 1 ? 'widest public type' : `${top.length} widest public types`}`,
    };
  }

  const entries = groups.flatMap((g) => g.entries);
  const top = rankFlows(entries).slice(0, ARCHETYPE_PRESET_SIZE);
  if (top.length === 0) return null;
  return {
    seeds: top.map((e) => ({
      type: 'flow' as const,
      title: `Flow: ${e.route || e.title}`,
      entryIds: [e.nodeId],
      estimatedLines: 15,
    })),
    source: `this repo's top ${top.length === 1 ? 'flow' : `${top.length} flows`}`,
  };
}

/**
 * The card set for ONE symbol sent from outside Studio (a NodeCard, an insight's target). It is
 * deliberately the symbol-rooted trio and nothing else: what it does (`flow`), what its code says
 * (`bodies`), and who depends on it (`usage`) — the three questions that made someone open the node
 * in the first place. The reader adds contracts/tests from the picker if the question turns out to
 * be bigger; a nine-card preset would blow the budget on a hand-off meant to be one click.
 */
export function symbolCardSeeds(entryId: string, label: string): ContextCardSeed[] {
  const entryIds = [entryId];
  return [
    { type: 'flow', title: `Flow: ${label}`, entryIds, estimatedLines: 15 },
    { type: 'bodies', title: `Bodies: ${label}`, entryIds, estimatedLines: 30 },
    { type: 'usage', title: `Who uses ${label}`, entryIds, estimatedLines: 15 },
  ];
}
