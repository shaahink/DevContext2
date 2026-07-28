import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';

import type { ContextPackResponse, VerifyContextResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TrailStore } from '../../state/trail.store';
import type { ContextCard } from './composition-view';
import { ContextStudio } from './context-studio';
import type { ContextCardSeed, OutputFormat } from './scope-picker';
import type { PackVerification } from './verification-panel';

/** The protected surface the specs drive — kept in sync with ContextStudio by the cast site. */
interface StudioTestSurface {
  cards(): readonly ContextCard[];
  packOmitted(): readonly string[];
  serverPack(): string | null;
  packPending(): boolean;
  exportReady(): boolean;
  packDebounceMs: number;
  packVerification(): PackVerification | null;
  onCardsChange(seeds: readonly ContextCardSeed[]): void;
  onBudgetChange(value: number): void;
  onRemove(id: string): void;
  onRetry(): void;
  onVerifyRefresh(): void;
  onReanalyze(): void;
  saveFileName(format: OutputFormat): string;
  buildContext(format: OutputFormat): string | null;
  // D4.5 (L4) — the live preview surface
  previewText(): string | null;
  packTotals(): { total: number; allocated: number } | null;
  previewOpen(): boolean;
  selectedFormat: { set(value: OutputFormat): void };
}

interface PackCardOverride {
  type: string;
  title: string;
  tokens: number;
  sections?: { key: string; tokens: number }[];
}

function packResponse(overrides: Partial<{
  omitted: string[];
  assembledMarkdown: string;
  cards: PackCardOverride[];
}> = {}): ContextPackResponse {
  // The server echoes the REQUEST card titles back on pack items (correlation key).
  const cards = (overrides.cards ?? [
    { type: 'flow', title: 'Flow: POST /checkout', tokens: 120 },
  ]).map((c) => ({
    sections: [{
      key: 'trace',
      tokens: c.tokens,
      content: `entry -> handler -> data (${c.title})`,
      sourceLocations: ['src/App/Handler.cs:12', 'src/App/Endpoint.cs:8'],
      verified: 2,
      approx: 1,
    }],
    ...c,
  }));
  return {
    cards,
    assembledMarkdown: overrides.assembledMarkdown ??
      '# repo — Context Pack\n\n_Intent: trace · Budget: 4000 tokens_\n\n```csharp\nvar x = 1;\n```\n\ncontent',
    totalTokens: cards.reduce((n, c) => n + c.tokens, 0),
    allocatedTokens: 4000,
    omitted: overrides.omitted ?? [],
  } as unknown as ContextPackResponse;
}

function flowSeed(title = 'Flow: POST /checkout'): ContextCardSeed {
  return { type: 'flow', title, entryIds: ['node-1'], estimatedLines: 15 };
}

function verifyResponse(overrides: Partial<{
  found: boolean;
  anyStale: boolean;
  analyzedGitHead: string;
  currentGitHead: string;
  sections: { key: string; stale: boolean; filesChecked: number; changed: { file: string; status: string; lineDelta: number }[] }[];
}> = {}): VerifyContextResponse {
  return {
    found: overrides.found ?? true,
    focus: 'POST /checkout',
    anyStale: overrides.anyStale ?? false,
    analyzedGitHead: overrides.analyzedGitHead ?? 'abc1234',
    currentGitHead: overrides.currentGitHead ?? 'abc1234',
    sections: overrides.sections ?? [
      { key: 'trace', stale: false, filesChecked: 3, changed: [] },
    ],
  } as unknown as VerifyContextResponse;
}

/** One macrotask hop — enough for the 0ms-debounce timer plus the RPC microtasks. */
async function flush(): Promise<void> {
  await new Promise((r) => setTimeout(r, 5));
  await Promise.resolve();
}

describe('ContextStudio', () => {
  let getContextPack: Mock;
  let verifyContext: Mock;
  let reAnalyze: Mock;

  beforeEach(() => {
    getContextPack = vi.fn();
    verifyContext = vi.fn().mockResolvedValue(verifyResponse());
    reAnalyze = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        { provide: DevContextApi, useValue: { getContextPack, verifyContext } },
        {
          provide: SessionStore,
          useValue: {
            handle: signal('h1'),
            entryGroups: signal([]),
            summary: signal({ label: 'eshop-microservices' }),
            reAnalyze,
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
    studio.packDebounceMs = 0;
    return { fixture, studio };
  }

  it('renders the server omitted[] list in the budget panel (T5.1 R1)', async () => {
    getContextPack.mockResolvedValue(
      packResponse({ omitted: ['signatures: omitted (1450 tokens, budget exhausted)'] }),
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
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.cards()[0].error).toBe('boom');
    expect(studio.exportReady()).toBe(false);

    studio.onRetry();
    await flush();
    fixture.detectChanges();

    const card = studio.cards()[0];
    expect(card.error).toBeNull();
    expect(card.loading).toBe(false);
    expect(card.serverTokens).toBe(120);
    expect(studio.exportReady()).toBe(true);
    expect(getContextPack).toHaveBeenCalledTimes(2);
  });

  it('re-packs the WHOLE card set when the budget changes, at the new budget (T5.6 C1)', async () => {
    getContextPack.mockResolvedValueOnce(packResponse({ assembledMarkdown: '# pack @4k\n\ncontent' }));
    getContextPack.mockResolvedValueOnce(packResponse({ assembledMarkdown: '# pack @1k\n\nless content' }));
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed(), { type: 'tests', title: 'Tests', entryIds: ['node-1'], estimatedLines: 15 }]);
    await flush();
    expect(studio.serverPack()).toBe('# pack @4k\n\ncontent');

    studio.onBudgetChange(1000);
    expect(studio.packPending()).toBe(true);
    expect(studio.exportReady()).toBe(false);
    await flush();

    expect(getContextPack).toHaveBeenCalledTimes(2);
    const [, specs, options] = getContextPack.mock.calls[1] as [string, { type: string }[], { budgetTokens: number }];
    expect(specs.map((s) => s.type)).toEqual(['flow', 'tests']); // whole set incl. tests (T4.3 real)
    expect(options.budgetTokens).toBe(1000);
    expect(studio.serverPack()).toBe('# pack @1k\n\nless content');
    expect(studio.exportReady()).toBe(true);
  });

  it('a later add re-packs everything — the export is never a stale batch (T5.6 C1)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed('Flow: A')]);
    await flush();
    studio.onCardsChange([flowSeed('Flow: B')]);
    await flush();

    expect(getContextPack).toHaveBeenCalledTimes(2);
    const [, specs] = getContextPack.mock.calls[1] as [string, { title: string }[]];
    expect(specs.map((s) => s.title)).toEqual(['Flow: A', 'Flow: B']);
  });

  it('cards sharing a type correlate by title, not clobbered (T5.6)', async () => {
    getContextPack.mockResolvedValue(packResponse({
      cards: [
        { type: 'tests', title: 'Validators for /checkout', tokens: 50 },
        { type: 'tests', title: 'Tests for /checkout', tokens: 90 },
      ],
    }));
    const { studio } = createStudio();

    studio.onCardsChange([
      { type: 'tests', title: 'Validators for /checkout', entryIds: ['node-1'], estimatedLines: 10 },
      { type: 'tests', title: 'Tests for /checkout', entryIds: ['node-1'], estimatedLines: 15 },
    ]);
    await flush();

    expect(studio.cards()[0].serverTokens).toBe(50);
    expect(studio.cards()[1].serverTokens).toBe(90);
  });

  it('exports serve exactly the server pack; plain strips syntax, loses nothing (T5.6)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { studio } = createStudio();

    expect(studio.buildContext('markdown')).toBeNull(); // no pack yet → Copy/Save disabled

    studio.onCardsChange([flowSeed()]);
    await flush();

    const markdown = studio.buildContext('markdown');
    const plain = studio.buildContext('plain');
    expect(markdown).toBe('# repo — Context Pack\n\n_Intent: trace · Budget: 4000 tokens_\n\n```csharp\nvar x = 1;\n```\n\ncontent');
    expect(plain).not.toBe(markdown);
    expect(plain).toContain('Intent: trace · Budget: 4000 tokens'); // meta kept, underscores gone
    expect(plain).toContain('var x = 1;'); // fence markers gone, code kept
    expect(plain).not.toContain('# repo');
    expect(plain).not.toContain('```');
  });

  it('a failed re-pack clears the pack — no stale export survives (T5.6 C1)', async () => {
    getContextPack.mockResolvedValueOnce(packResponse());
    getContextPack.mockRejectedValueOnce(new Error('gone'));
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.serverPack()).not.toBeNull();

    studio.onBudgetChange(1000);
    await flush();

    expect(studio.serverPack()).toBeNull();
    expect(studio.buildContext('markdown')).toBeNull();
    expect(studio.exportReady()).toBe(false);
    expect(studio.cards()[0].error).toBe('gone');
  });

  it('removing the last card clears the pack and omitted list (T5.6)', async () => {
    getContextPack.mockResolvedValue(packResponse({ omitted: ['x: omitted'] }));
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.serverPack()).not.toBeNull();

    studio.onRemove(studio.cards()[0].id);
    await flush();

    expect(studio.cards()).toHaveLength(0);
    expect(studio.serverPack()).toBeNull();
    expect(studio.packOmitted()).toEqual([]);
    expect(getContextPack).toHaveBeenCalledTimes(1); // no RPC for an empty set
  });

  it('cards carry their sections\' real content + provenance; json export is structured (T5.3 R7/R8)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    const card = studio.cards()[0];
    expect(card.sections).toHaveLength(1);
    expect(card.sections[0].content).toBe('entry -> handler -> data (Flow: POST /checkout)');
    expect(card.sections[0].sourceLocations).toEqual(['src/App/Handler.cs:12', 'src/App/Endpoint.cs:8']);

    // file:line chips = the card's OWN source set, full path on title, tail as text
    const el: HTMLElement = fixture.nativeElement;
    const chips = [...el.querySelectorAll('[data-testid="provenance-chip"]')];
    expect(chips.map((c) => c.textContent?.trim())).toEqual(['Handler.cs:12', 'Endpoint.cs:8']);
    expect(chips[0].getAttribute('title')).toContain('src/App/Handler.cs:12');

    // per-card copy is enabled and copies the card's real content
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const copyBtn = el.querySelector('[data-testid="card-copy"]') as HTMLButtonElement;
    expect(copyBtn.disabled).toBe(false);
    copyBtn.click();
    expect(writeText).toHaveBeenCalledTimes(1);
    const copied = writeText.mock.calls[0][0] as string;
    expect(copied).toContain('## Flow: POST /checkout');
    expect(copied).toContain('entry -> handler -> data');

    // json export: structured pack with sections, provenance, omissions, verification, markdown
    const json = JSON.parse(studio.buildContext('json')!) as {
      repo: string; budgetTokens: number; cards: { type: string; sections: { content: string; sourceLocations: string[] }[] }[]; markdown: string;
    };
    expect(json.repo).toBe('eshop-microservices');
    expect(json.budgetTokens).toBe(4000);
    expect(json.cards[0].sections[0].sourceLocations).toContain('src/App/Handler.cs:12');
    expect(json.markdown.length).toBeGreaterThan(0);
    const date = new Date().toISOString().slice(0, 10);
    expect(studio.saveFileName('json')).toBe(`eshop-microservices-context-${date}.json`);
  });

  it('previews render the sections\' real content, never a title echo (T5.5)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    expect(studio.cards()[0].content).toBe('entry -> handler -> data (Flow: POST /checkout)');
    const pre = (fixture.nativeElement as HTMLElement).querySelector('app-composition-view pre');
    expect(pre?.textContent).toContain('entry -> handler -> data');
    expect(pre?.textContent?.trim()).not.toBe('Flow: POST /checkout'); // the audit's echo
  });

  it('verifies the pack after every successful re-pack, unprompted (T5.2 R6)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    expect(verifyContext).toHaveBeenCalledWith('h1', 'node-1', 4000);
    const v = studio.packVerification();
    expect(v).not.toBeNull();
    expect(v!.anyStale).toBe(false);
    expect(v!.sections).toEqual([{ key: 'trace', stale: false, filesChecked: 3, changed: [] }]);
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="verification-panel"]')).not.toBeNull();
    expect(el.querySelector('[data-testid="verification-fresh"]')).not.toBeNull();
    expect(el.querySelector('[data-testid="verification-stale"]')).toBeNull();
  });

  it('merges verification across focuses; stale renders the warning + Re-analyze (T5.2 R6)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    verifyContext.mockImplementation((_h: string, focus: string) =>
      Promise.resolve(focus === 'node-2'
        ? verifyResponse({
            anyStale: true,
            currentGitHead: 'def5678',
            sections: [{ key: 'trace', stale: true, filesChecked: 2, changed: [{ file: 'src/App/Handler.cs', status: 'modified', lineDelta: 4 }] }],
          })
        : verifyResponse()));
    const { fixture, studio } = createStudio();

    studio.onCardsChange([
      flowSeed(),
      { type: 'flow', title: 'Flow: GET /orders', entryIds: ['node-2'], estimatedLines: 15 },
    ]);
    await flush();
    fixture.detectChanges();

    const v = studio.packVerification();
    expect(v!.anyStale).toBe(true);
    expect(v!.sections).toEqual([
      { key: 'trace', stale: true, filesChecked: 5, changed: [{ file: 'src/App/Handler.cs', status: 'modified', lineDelta: 4 }] },
    ]);
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="verification-stale"]')).not.toBeNull();
    expect(el.textContent).toContain('Handler.cs');

    (el.querySelector('[data-testid="verification-reanalyze"]') as HTMLButtonElement).click();
    expect(reAnalyze).toHaveBeenCalledTimes(1);
  });

  it('a failed verification is advisory — panel disappears, Studio unaffected (T5.2)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    verifyContext.mockRejectedValue(new Error('no fingerprints'));
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();

    expect(studio.packVerification()).toBeNull();
    expect(studio.exportReady()).toBe(true); // exports unaffected
    expect(studio.cards()[0].error).toBeNull();
  });

  it('saves as ${repo}-context-${date} with the format extension (T5.1 R5 + T5.6)', () => {
    const { studio } = createStudio();
    const date = new Date().toISOString().slice(0, 10);
    expect(studio.saveFileName('markdown')).toBe(`eshop-microservices-context-${date}.md`);
    expect(studio.saveFileName('plain')).toBe(`eshop-microservices-context-${date}.txt`);
  });

  // ---- D4.5 (L4): the live pack preview ------------------------------------------

  it('preview text IS the export string; format switches it client-side, no re-pack (D4.5 L4)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { studio } = createStudio();
    studio.onCardsChange([flowSeed()]);
    await flush();

    expect(studio.previewText()).toBe(studio.buildContext('markdown'));
    expect(getContextPack).toHaveBeenCalledTimes(1);

    studio.selectedFormat.set('json');
    const json = studio.previewText();
    expect(json).toContain('"markdown"');
    // S5: this asserted byte equality against a SECOND buildContext('json') call, which re-stamps
    // `generatedAt: new Date()`. It therefore passed only when both calls landed in the same
    // millisecond — a flake the S5 close battery caught on a 1ms boundary. The property under test is
    // that the preview IS the export string, so compare everything except the stamp, and check the
    // stamp separately. (Copy and Save both read previewText(), so the product is byte-exact.)
    const withoutStamp = (s: string | null) => s?.replace(/"generatedAt": "[^"]+"/, '"generatedAt": "*"');
    expect(withoutStamp(json)).toBe(withoutStamp(studio.buildContext('json')));
    expect(json).toMatch(/"generatedAt": "\d{4}-\d{2}-\d{2}T/);
    // Format is presentation, not scope — switching must not re-pack.
    expect(getContextPack).toHaveBeenCalledTimes(1);
  });

  it('surfaces the server token accounting; cleared with the pack (D4.5 L4)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { studio } = createStudio();
    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.packTotals()).toEqual({ total: 120, allocated: 4000 });

    getContextPack.mockRejectedValue(new Error('boom'));
    studio.onBudgetChange(8000);
    await flush();
    expect(studio.packTotals()).toBeNull();
  });

  it('normalizes server CRLF to LF — preview/Copy/Save serve one byte form (D4.5 L4)', async () => {
    getContextPack.mockResolvedValue(
      packResponse({ assembledMarkdown: '# Pack\r\n\r\n_meta_\r\ncontent' }),
    );
    const { studio } = createStudio();
    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.buildContext('markdown')).toBe('# Pack\n\n_meta_\ncontent');
    expect(studio.previewText()).not.toContain('\r');
  });

  it('renders the preview pane with highlighted fences, open by default (D4.5 L4)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();
    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    expect(studio.previewOpen()).toBe(true);
    const pane = (fixture.nativeElement as HTMLElement).querySelector('.pack-preview');
    expect(pane).not.toBeNull();
    // The mock pack carries a ```csharp fence — Prism token spans prove real rendering.
    expect(pane!.innerHTML).toContain('token');
    expect(pane!.textContent).toContain('# repo — Context Pack');
  });
});
