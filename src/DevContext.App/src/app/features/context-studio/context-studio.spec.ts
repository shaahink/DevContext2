import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';

import type { ContextPackResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TrailStore } from '../../state/trail.store';
import type { ContextCard } from './composition-view';
import { ContextStudio } from './context-studio';
import type { ContextCardSeed, OutputFormat } from './scope-picker';

/** The protected surface the specs drive — kept in sync with ContextStudio by the cast site. */
interface StudioTestSurface {
  cards(): readonly ContextCard[];
  packOmitted(): readonly string[];
  onCardsChange(seeds: readonly ContextCardSeed[]): void;
  onRetry(): void;
  saveFileName(format: OutputFormat): string;
  budgetTokens: { set(v: number): void };
}

function packResponse(overrides: Partial<{
  omitted: string[];
  assembledMarkdown: string;
  cards: { type: string; title: string; tokens: number; sections: { key: string; tokens: number }[] }[];
}> = {}): unknown {
  return {
    cards: overrides.cards ?? [
      { type: 'flow', title: 'entry → handler → data', tokens: 120, sections: [{ key: 'trace', tokens: 120 }] },
    ],
    assembledMarkdown: overrides.assembledMarkdown ?? '# repo — Context Pack\n\n_Intent: trace · Budget: 4000 tokens_\n\ncontent',
    totalTokens: 120,
    allocatedTokens: 4000,
    omitted: overrides.omitted ?? [],
  };
}

function flowSeed(): ContextCardSeed {
  return { type: 'flow', title: 'Flow: POST /checkout', entryIds: ['node-1'], estimatedLines: 15 };
}

async function flush(): Promise<void> {
  await Promise.resolve();
  await Promise.resolve();
}

describe('ContextStudio', () => {
  let getContextPack: Mock;

  beforeEach(() => {
    getContextPack = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        { provide: DevContextApi, useValue: { getContextPack } },
        {
          provide: SessionStore,
          useValue: {
            handle: signal('h1'),
            entryGroups: signal([]),
            summary: signal({ label: 'eshop-microservices' }),
          },
        },
        { provide: TrailStore, useValue: { steps: signal([]) } },
      ],
    });
  });

  function createStudio() {
    const fixture = TestBed.createComponent(ContextStudio);
    fixture.detectChanges();
    const studio = fixture.componentInstance as unknown as StudioTestSurface;
    return { fixture, studio };
  }

  it('renders the server omitted[] list in the budget panel (T5.1 R1)', async () => {
    getContextPack.mockResolvedValue(
      packResponse({ omitted: ['signatures: omitted (1450 tokens, budget exhausted)'] }) as ContextPackResponse,
    );
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    expect(studio.packOmitted()).toEqual(['signatures: omitted (1450 tokens, budget exhausted)']);
    const el: HTMLElement = fixture.nativeElement;
    const list = el.querySelector('[data-testid="omitted-list"]');
    expect(list).not.toBeNull();
    expect(list!.textContent).toContain('signatures: omitted (1450 tokens, budget exhausted)');
  });

  it('marks failed cards with the error and shows a retry affordance (T5.1 R4)', async () => {
    getContextPack.mockRejectedValue(new Error('server unavailable'));
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    const card = studio.cards()[0];
    expect(card.loading).toBe(false);
    expect(card.error).toBe('server unavailable');

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="card-error"]')?.textContent).toContain('server unavailable');
    expect(el.querySelector('[data-testid="card-retry"]')).not.toBeNull();
  });

  it('retry clears the error and reloads content from the server (T5.1 R4)', async () => {
    getContextPack.mockRejectedValueOnce(new Error('boom'));
    getContextPack.mockResolvedValue(packResponse() as ContextPackResponse);
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.cards()[0].error).toBe('boom');

    studio.onRetry();
    await flush();
    fixture.detectChanges();

    const card = studio.cards()[0];
    expect(card.error).toBeNull();
    expect(card.loading).toBe(false);
    expect(card.content).toBe('entry → handler → data');
    expect(card.serverTokens).toBe(120);
    expect(getContextPack).toHaveBeenCalledTimes(2);
  });

  it('saves plain format as .txt, markdown as .md (T5.1 R5)', () => {
    const { studio } = createStudio();
    expect(studio.saveFileName('markdown')).toBe('devcontext-context.md');
    expect(studio.saveFileName('plain')).toBe('devcontext-context.txt');
  });
});
