/**
 * R3 D-D (D1) — the CLI landing rule and its counts, as pure functions.
 *
 * Same pattern as library-surface.vm.ts: the decision about WHEN a CliTool's workspace shows its
 * commands instead of a topology is a rule worth pinning, not a condition buried in a template.
 * Structural input types (subsets of the generated proto messages) keep this free of message
 * constructors.
 */

export interface CommandRowLike {
  readonly title: string;
  readonly target?: string;
}

export interface CommandGroupLike {
  readonly project: string;
  readonly entries: readonly CommandRowLike[];
}

export interface CommandSurfaceInput {
  readonly archetype: string;
  /** The active lens id — only the Flow lens lands here (see below). */
  readonly lens: string;
  readonly hasFocus: boolean;
  readonly groups: readonly CommandGroupLike[];
}

/**
 * True when the centre pane should be the command surface.
 *
 * Conditioned on the Flow lens with nothing focused, which is exactly the state R3 D-A sends to the
 * topology canvas — the service/layer/feature lenses exist to draw that topology and keep drawing
 * it. A CliTool whose commands the engine could not project falls through to the canvas rather than
 * to an empty promise, which is the same rule the library workbench follows.
 */
export function shouldShowCommandSurface(input: CommandSurfaceInput): boolean {
  if (input.lens !== 'flow' || input.hasFocus) return false;
  if (!/clitool/i.test(input.archetype)) return false;
  return input.groups.some((g) => g.entries.length > 0);
}

/** How many commands there are, and how many the engine could not join to a handler. The second
 * number is a finding, not a blank: GitVersion's five verbs all reach `ICommand<T>` classes whose
 * execute member never resolved, and hiding that would make the surface look complete. */
export function commandCounts(groups: readonly CommandGroupLike[]): { total: number; unwired: number } {
  let total = 0;
  let unwired = 0;
  for (const g of groups) {
    for (const e of g.entries) {
      total++;
      if (!e.target) unwired++;
    }
  }
  return { total, unwired };
}

/** First-wins index by title — the join that lets a command row focus through the same path a deck
 * selection uses. A duplicate title keeps the first entry rather than the last, so the row a reader
 * clicks is the one the deck would have scrubbed to. */
export function indexByTitle<T extends { readonly title: string }>(entries: readonly T[]): ReadonlyMap<string, T> {
  const map = new Map<string, T>();
  for (const e of entries) if (!map.has(e.title)) map.set(e.title, e);
  return map;
}
