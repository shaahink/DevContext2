import { allCardsPriced, cardTokens, totalCardTokens } from './card-tokens';
import type { ContextCard } from './composition-view';

function card(partial: Partial<ContextCard>): ContextCard {
  return {
    id: 'c1',
    type: 'flow',
    title: 'Card',
    entryIds: [],
    estimatedLines: 0,
    content: null,
    loading: false,
    bodyEnabled: true,
    serverTokens: null,
    sections: [],
    provenance: [],
    error: null,
    ...partial,
  } as ContextCard;
}

describe('card token counting (Batch E — one counting function per stat)', () => {
  it('prefers the server price over the line estimate', () => {
    expect(cardTokens(card({ serverTokens: 1234, estimatedLines: 10 }))).toBe(1234);
  });

  it('falls back to the line estimate only when the server has not priced the card', () => {
    expect(cardTokens(card({ serverTokens: null, estimatedLines: 10 }))).toBe(25);
  });

  it('counts a server price of zero as zero, not as unpriced', () => {
    // ?? not ||: a card the server measured at 0 tokens is measured, and must not silently
    // fall back to an estimate that would print a non-zero number for an empty card.
    expect(cardTokens(card({ serverTokens: 0, estimatedLines: 100 }))).toBe(0);
  });

  it('totals cards with the same per-card rule', () => {
    const cards = [
      card({ id: 'a', serverTokens: 100 }),
      card({ id: 'b', serverTokens: null, estimatedLines: 40 }),
    ];
    expect(totalCardTokens(cards)).toBe(200);
  });

  it('reports a total as measured only when every card is priced', () => {
    expect(allCardsPriced([card({ serverTokens: 5 }), card({ serverTokens: 7 })])).toBe(true);
    expect(allCardsPriced([card({ serverTokens: 5 }), card({ serverTokens: null })])).toBe(false);
    expect(allCardsPriced([])).toBe(false);
  });
});
