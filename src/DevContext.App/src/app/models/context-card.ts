/**
 * The vocabulary of a context pack — card types, the seed a surface emits to ask for one, and the
 * two shaping choices (intent, output format).
 *
 * N3.1 moved these out of `features/context-studio/scope-picker.ts`. They had lived there because
 * the picker was the only thing that produced a seed; N3.1 gives Explore, Insights and the NodeCard
 * the same power, and their hand-off rides through `state/studio-handoff.store.ts`. A store in
 * `state/` importing a type from `features/` would be the app's first state->features import (there
 * were zero before this checkpoint) — so the shared vocabulary moves down to `models/` where the
 * layering says it belongs, and the picker imports it like everyone else.
 */

/** N2.1 (audit §3.C / owner decision 2) — `usage` is the inbound direction of a symbol-rooted
 * pack ("who calls this"). The engine has built the section for every symbol root since G1.2;
 * no card type could pick it until then (audit §3.F.15). */
export type ContextCardType =
  | 'flow' | 'signatures' | 'bodies' | 'di_wiring' | 'config'
  | 'entities' | 'contracts' | 'tests' | 'identity' | 'usage';

export type ContextIntent = 'trace' | 'explain' | 'review';

/** T5.3 (audit R8) — json is the structured export: cards/sections/provenance/verification. */
export type OutputFormat = 'markdown' | 'plain' | 'json';

export interface ContextCardSeed {
  readonly type: ContextCardType;
  readonly title: string;
  readonly entryIds: string[];
  readonly estimatedLines: number;
}

/**
 * N3.1 — a set of seeds together with the sentence that says where they came from. It is ONE type
 * because it travels: `features/context-studio/pack-proposal.ts` builds it, `StudioHandoffStore`
 * carries it between rooms, Studio renders its `source` in the proposal banner and the senders toast
 * it. A second name for this shape in the store would be the same field spelled twice.
 */
export interface PackProposal {
  readonly seeds: readonly ContextCardSeed[];
  /** Human sentence naming WHERE the cards came from — rendered verbatim, so a proposed pack is
   * never mistaken for a hand-picked one. */
  readonly source: string;
}
