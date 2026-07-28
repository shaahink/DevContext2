import type { ContextCard } from './composition-view';

/**
 * Batch E (R2 §2.E item 2) — THE token count for a context card, and for a set of them.
 *
 * The Studio header and the budget panel each carried their own copy of this reduce. Identical today,
 * and identical is the problem: two copies of a number the user compares side by side is a divergence
 * waiting for the next edit to either one. One function, one number, one test.
 *
 * A card's REAL cost is what the server charged for it (`serverTokens`). The line estimate is a
 * placeholder shown only until the card's content arrives — which is why any surface printing a total
 * that mixes the two must also say so (see `allCardsPriced`).
 */
export function cardTokens(card: ContextCard): number {
  return card.serverTokens ?? Math.round(card.estimatedLines * 2.5);
}

/** Total tokens across cards, using {@link cardTokens} per card. */
export function totalCardTokens(cards: readonly ContextCard[]): number {
  return cards.reduce((n, c) => n + cardTokens(c), 0);
}

/** True when every card has a SERVER token count — i.e. the total is measured, not estimated.
 * Surfaces prefix an estimated total with "~"; a measured one gets no hedge. */
export function allCardsPriced(cards: readonly ContextCard[]): boolean {
  return cards.length > 0 && cards.every((c) => c.serverTokens !== null);
}
