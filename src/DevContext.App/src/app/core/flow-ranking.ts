/**
 * R3 D-E (E-2) — ONE ranking rule for "which flows matter in this repo".
 *
 * Before this, three surfaces answered that question with three different rules and gave three
 * different answers on the same repo, in the same session:
 *
 *   Home  › Top flows   the engine's composite entry score  -> PUT /api/catalog/items, POST /api/orders/
 *   Atlas › Top flows   indexed flow depth                  -> five internal *DomainEventHandlers
 *   START HERE          any /checkout/i title reaching 4+ nodes
 *                                                           -> [RelayCommand] CheckoutViewModel.CheckoutAsync
 *
 * None is wrong on its own terms. Together they tell a reader the product has no opinion about what
 * matters, and two sections sharing the name "Top flows" disagree.
 *
 * The rule: a request-shaped entry — something outside the process asked for — outranks an internal
 * reaction, and within each band the deeper/higher-scoring flow wins. A domain-event handler with 48
 * steps is genuinely the biggest thing in eShop's graph; it is still not the answer to "show me this
 * repo", because nobody calls it directly. That preference already existed inside
 * `onboarding-row.ts` and was documented there ("Trace POST /api/orders/draft is the story a first
 * visit should open on") — it was just fenced behind a checkout special case that Batches A–E
 * inverted, and never shared with the two lists that needed it.
 */

/** Entry kinds a caller outside the process reaches directly. */
const REQUEST_KINDS = new Set([
  'HttpEndpoint',
  'GrpcService',
  'GraphQlField',
  'SignalRHub',
  'FunctionEntry',
  'CliCommand',
]);

/**
 * UI entries are reachable by a person, but they are the near side of a flow rather than a way into
 * the system's behaviour — eShop's MAUI `CheckoutViewModel.CheckoutAsync` stops at the view-model.
 * They rank below request kinds and above internal reactions.
 */
const SURFACE_KINDS = new Set(['UiEntry', 'PublicApi']);

export function isRequestShaped(kind: string | undefined): boolean {
  return !!kind && REQUEST_KINDS.has(kind);
}

/** 0 = asked for from outside · 1 = a person's surface · 2 = an internal reaction. */
export function flowBand(kind: string | undefined): number {
  if (isRequestShaped(kind)) return 0;
  if (kind && SURFACE_KINDS.has(kind)) return 1;
  return 2;
}

export interface RankableFlow {
  readonly kind?: string;
  /** The engine's composite entry score, where the caller has one. */
  readonly score?: number;
  /** Indexed flow depth, where the caller has one instead. */
  readonly nodeCount?: number;
}

/**
 * Sort comparator: band first, then whichever magnitude the caller carries. Stable for equal keys,
 * so a caller's own input order survives — deterministic lists, same as everywhere else.
 */
export function compareFlows(a: RankableFlow, b: RankableFlow): number {
  const band = flowBand(a.kind) - flowBand(b.kind);
  if (band !== 0) return band;
  const score = (b.score ?? 0) - (a.score ?? 0);
  if (score !== 0) return score;
  return (b.nodeCount ?? 0) - (a.nodeCount ?? 0);
}

/** Convenience: rank a list without mutating it. */
export function rankFlows<T extends RankableFlow>(flows: readonly T[]): T[] {
  return [...flows].sort(compareFlows);
}

/**
 * The one flow a first visit opens on (START HERE). Ranked by the rule above and nothing else.
 *
 * G10.1 — this used to read `rankFlows(flows.filter((f) => f.nodeCount >= 4))[0] ?? rankFlows(flows)[0]`:
 * a DEPTH FILTER applied BEFORE the band rule, so it could delete the entire request-shaped band and
 * then rank what was left. That 4 is the last surviving number from the same starved-graph
 * calibration E-2 uncovered (the >=3-hop gate the retired checkout special case leaned on), and it
 * inverts the same way: the deeper a repo's internal reactions get, the more likely a
 * DomainEventHandler outranks the endpoint that a caller outside the process actually asks for.
 *
 * It is also redundant. compareFlows already prefers the deeper flow WITHIN a band, and the caller
 * has already dropped 1-node flows, so depth still decides every comparison it should decide - it
 * just stops deciding the ones it should not. Measured 2026-08-02 on the entry-kind mix the rule
 * exists to arbitrate (eval-results/2026-08-02/G10/threshold-grid.txt): eShop 46 request-shaped
 * entries / 42 UiEntry / 21 internal, DntSite 70 / 0 / 24, wolverine 22 / 0 / 29 - on all three the
 * top band and the deepest flows are different populations, which is precisely when a depth
 * pre-filter changes the answer.
 */
export function pickHeroFlow<T extends RankableFlow>(flows: readonly T[]): T | null {
  return rankFlows(flows)[0] ?? null;
}
