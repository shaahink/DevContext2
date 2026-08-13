import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';

import type { ContextPackResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { PrefsStore } from '../../state/prefs.store';
import { SessionStore } from '../../state/session.store';
import { TrailStore, type TrailStep } from '../../state/trail.store';
import type { EntryGroupVm } from '../../models/view-models';
import { ToastService } from '../../ui/toast/toast';
import type { ContextCard } from './composition-view';
import { ContextStudio } from './context-studio';
import type { ContextCardSeed, ContextIntent, OutputFormat } from './scope-picker';
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
  onIntentChange(intent: ContextIntent): void;
  onToggleBody(id: string): void;
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
  // N1.1 (wire item 4) — the ledger rides the pack it describes.
  verification: { key: string; stale: boolean; filesChecked: number; changed: { file: string; status: string; lineDelta: number }[] }[];
  anyStale: boolean;
  analyzedGitHead: string;
  currentGitHead: string;
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
    verification: overrides.verification ?? [
      { key: 'trace', stale: false, filesChecked: 3, changed: [] },
    ],
    anyStale: overrides.anyStale ?? false,
    analyzedGitHead: overrides.analyzedGitHead ?? 'abc1234',
    currentGitHead: overrides.currentGitHead ?? 'abc1234',
  } as unknown as ContextPackResponse;
}

function flowSeed(title = 'Flow: POST /checkout'): ContextCardSeed {
  return { type: 'flow', title, entryIds: ['node-1'], estimatedLines: 15 };
}

/** One macrotask hop — enough for the 0ms-debounce timer plus the RPC microtasks. */
async function flush(): Promise<void> {
  await new Promise((r) => setTimeout(r, 5));
  await Promise.resolve();
}

describe('ContextStudio', () => {
  let getContextPack: Mock;
  /** N1.1 — kept as a mock precisely so the specs can prove it is NEVER called: the ledger
   * rides the pack response now, so a VerifyContext RPC from the Studio is a regression. */
  let verifyContext: Mock;
  let reAnalyze: Mock;
  let handle: ReturnType<typeof signal<string | null>>;
  // N1.2 — the trail/pins the Studio seeds from, and the graph it resolves them against.
  let trailSteps: ReturnType<typeof signal<TrailStep[]>>;
  let pins: ReturnType<typeof signal<TrailStep[]>>;
  let entryGroups: ReturnType<typeof signal<EntryGroupVm[]>>;
  let prefs: {
    studioBudget: Mock; studioIntent: Mock; studioFormat: Mock;
    setStudioBudget: Mock; setStudioIntent: Mock; setStudioFormat: Mock;
  };

  beforeEach(() => {
    getContextPack = vi.fn();
    verifyContext = vi.fn();
    reAnalyze = vi.fn();
    handle = signal<string | null>('h1');
    trailSteps = signal<TrailStep[]>([]);
    pins = signal<TrailStep[]>([]);
    entryGroups = signal<EntryGroupVm[]>([]);
    prefs = {
      studioBudget: vi.fn().mockReturnValue(4000),
      studioIntent: vi.fn().mockReturnValue('trace'),
      studioFormat: vi.fn().mockReturnValue('markdown'),
      setStudioBudget: vi.fn(),
      setStudioIntent: vi.fn(),
      setStudioFormat: vi.fn(),
    };
    TestBed.configureTestingModule({
      providers: [
        { provide: DevContextApi, useValue: { getContextPack, verifyContext } },
        { provide: PrefsStore, useValue: prefs },
        {
          provide: SessionStore,
          useValue: {
            handle,
            entryGroups,
            summary: signal({ label: 'eshop-microservices' }),
            // R3 C-3: the Studio now tells the scope picker whether a repo was analyzed at all,
            // so the picker can stop reporting "zero entries" as "no analysis".
            ready: signal(true),
            mapResponse: signal(null),
            reAnalyze,
          },
        },
        { provide: TrailStore, useValue: { steps: trailSteps, pins } },
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

  it('the ledger IS the pack response — no VerifyContext RPC at all (N1.1 wire item 4)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    // The whole point of moving verification onto GetContextPack: the Studio used to fan out
    // one VerifyContext per focus, each handed the WHOLE budget and each verifying every
    // section of that focus — a ledger for a pack that was never built (backlog #28).
    expect(verifyContext).not.toHaveBeenCalled();
    expect(getContextPack).toHaveBeenCalledTimes(1);

    const v = studio.packVerification();
    expect(v).not.toBeNull();
    expect(v!.anyStale).toBe(false);
    expect(v!.sections).toEqual([{ key: 'trace', stale: false, filesChecked: 3, changed: [] }]);
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="verification-panel"]')).not.toBeNull();
    expect(el.querySelector('[data-testid="verification-fresh"]')).not.toBeNull();
    expect(el.querySelector('[data-testid="verification-stale"]')).toBeNull();
    // backlog #28's dead field: checkedAt was set and declared and never rendered.
    expect(el.querySelector('[data-testid="verification-checked-at"]')?.textContent)
      .toContain('Checked');
  });

  it('a stale pack renders the warning, the drifted file and Re-analyze (T5.2 R6 / N1.1)', async () => {
    getContextPack.mockResolvedValue(packResponse({
      anyStale: true,
      currentGitHead: 'def5678',
      verification: [
        { key: 'trace', stale: true, filesChecked: 5, changed: [{ file: 'src/App/Handler.cs', status: 'modified', lineDelta: 4 }] },
      ],
    }));
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
    expect(el.textContent).toContain('abc1234');   // HEAD moved line

    (el.querySelector('[data-testid="verification-reanalyze"]') as HTMLButtonElement).click();
    expect(reAnalyze).toHaveBeenCalledTimes(1);
  });

  it('a pack the server could not verify shows no ledger, and exports are unaffected (T5.2)', async () => {
    getContextPack.mockResolvedValue(packResponse({ verification: [] }));
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();

    expect(studio.packVerification()).toBeNull();
    expect(studio.exportReady()).toBe(true); // exports unaffected
    expect(studio.cards()[0].error).toBeNull();
  });

  it('refresh rebuilds the pack — the ledger is a property of a build (N1.1)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { studio } = createStudio();
    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(getContextPack).toHaveBeenCalledTimes(1);

    studio.onVerifyRefresh();
    await flush();

    expect(getContextPack).toHaveBeenCalledTimes(2);
    expect(verifyContext).not.toHaveBeenCalled();
  });

  // ---- N1.1: body toggles reach the wire (audit §3.F.2 / backlog #27) --------------

  it('bodyEnabled rides the request and a toggle re-packs (N1.1 §3.F.2)', async () => {
    getContextPack.mockResolvedValue(packResponse({
      cards: [{ type: 'bodies', title: 'Bodies: POST /checkout', tokens: 300 }],
    }));
    const { studio } = createStudio();

    studio.onCardsChange([{ type: 'bodies', title: 'Bodies: POST /checkout', entryIds: ['node-1'], estimatedLines: 40 }]);
    await flush();

    expect(getContextPack).toHaveBeenLastCalledWith(
      'h1',
      [{ type: 'bodies', title: 'Bodies: POST /checkout', entryIds: ['node-1'], excludeBodies: false }],
      { budgetTokens: 4000, intent: 'trace' },
    );

    // The toggle used to repaint an icon and leave the bytes alone.
    studio.onToggleBody(studio.cards()[0].id);
    await flush();

    expect(getContextPack).toHaveBeenCalledTimes(2);
    expect(getContextPack).toHaveBeenLastCalledWith(
      'h1',
      [{ type: 'bodies', title: 'Bodies: POST /checkout', entryIds: ['node-1'], excludeBodies: true }],
      { budgetTokens: 4000, intent: 'trace' },
    );
  });

  it('renders verified/approx PER CARD, and offers the body toggle only where it acts (N1.1)', async () => {
    getContextPack.mockResolvedValue(packResponse({
      cards: [
        { type: 'flow', title: 'Flow: POST /checkout', tokens: 120 },
        { type: 'bodies', title: 'Bodies: POST /checkout', tokens: 300 },
      ],
    }));
    const { fixture, studio } = createStudio();

    studio.onCardsChange([
      flowSeed(),
      { type: 'bodies', title: 'Bodies: POST /checkout', entryIds: ['node-1'], estimatedLines: 40 },
    ]);
    await flush();
    fixture.detectChanges();

    // verified/approx have ridden the wire since T4.4 and no surface rendered them.
    const el: HTMLElement = fixture.nativeElement;
    const mixes = [...el.querySelectorAll('[data-testid="card-provenance-mix"]')];
    expect(mixes).toHaveLength(2);
    expect(mixes[0].textContent).toContain('2 verified');
    expect(mixes[0].textContent).toContain('1 approx');
    expect(mixes[0].textContent).toContain('67% verified');

    // …and no inert control survives: only the bodies card can act on the toggle.
    expect(el.querySelectorAll('[data-testid="card-body-toggle"]')).toHaveLength(1);
  });

  // ---- N1.1: state lifecycle (audit §3.F.6 / backlog #29) -------------------------

  it('a new session handle clears the cards that addressed the old graph (N1.1 §3.F.6)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();
    expect(studio.cards()).toHaveLength(1);
    expect(studio.serverPack()).not.toBeNull();

    handle.set('h2');            // re-analyze / repo switch
    fixture.detectChanges();
    await flush();

    expect(studio.cards()).toHaveLength(0);
    expect(studio.serverPack()).toBeNull();
    expect(studio.packVerification()).toBeNull();
    expect(studio.packPending()).toBe(false);
    // and no pack was requested for the new handle off the back of the old cards
    expect(getContextPack).toHaveBeenCalledTimes(1);
  });

  it('budget / intent / format are persisted as preferences (N1.1 §3.F.6)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();

    studio.onBudgetChange(12000);
    studio.onIntentChange('review');
    studio.selectedFormat.set('json');
    fixture.detectChanges();
    await flush();

    expect(prefs.setStudioBudget).toHaveBeenCalledWith(12000);
    expect(prefs.setStudioIntent).toHaveBeenCalledWith('review');
    expect(prefs.setStudioFormat).toHaveBeenCalledWith('json');
  });

  it('restores the persisted shaping on construction (N1.1 §3.F.6)', async () => {
    prefs.studioBudget.mockReturnValue(16000);
    prefs.studioIntent.mockReturnValue('explain');
    getContextPack.mockResolvedValue(packResponse());
    const { studio } = createStudio();

    studio.onCardsChange([flowSeed()]);
    await flush();

    expect(getContextPack).toHaveBeenLastCalledWith(
      'h1', expect.anything(), { budgetTokens: 16000, intent: 'explain' },
    );
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

  // N0.1 (audit §3.F.7) — Copy used to fire "Context copied to clipboard" and flip the button
  // to "Copied!" on click, while the clipboard promise was still in flight and its rejection
  // went nowhere. The toast and the label are now reports, not predictions.
  it('Copy reports the clipboard OUTCOME — success toast + Copied! label only on resolve', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();
    const toast = TestBed.inject(ToastService);
    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const copyBtn = (fixture.nativeElement as HTMLElement)
      .querySelector('[data-testid="copy-context"]') as HTMLButtonElement;

    copyBtn.click();
    expect(toast.messages()).toHaveLength(0); // nothing claimed before the write resolves
    await flush();
    fixture.detectChanges();
    expect(writeText).toHaveBeenCalledWith(studio.previewText());
    expect(toast.messages().map((m) => [m.text, m.kind]))
      .toEqual([['Context copied to clipboard', 'success']]);
    expect(copyBtn.textContent?.trim()).toBe('Copied!');
  });

  it('Copy failure toasts the error and never says "copied" (N0.1 §3.F.7)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();
    const toast = TestBed.inject(ToastService);
    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    const writeText = vi.fn().mockRejectedValue(new Error('clipboard blocked'));
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const copyBtn = (fixture.nativeElement as HTMLElement)
      .querySelector('[data-testid="copy-context"]') as HTMLButtonElement;

    copyBtn.click();
    await flush();
    fixture.detectChanges();
    expect(toast.messages().map((m) => [m.text, m.kind]))
      .toEqual([['Copy failed: clipboard blocked', 'error']]);
    expect(copyBtn.textContent?.trim()).toBe('Copy');
  });

  it('a per-card copy failure is reported, not swallowed (N0.1 §3.F.7)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    const { fixture, studio } = createStudio();
    const toast = TestBed.inject(ToastService);
    studio.onCardsChange([flowSeed()]);
    await flush();
    fixture.detectChanges();

    const writeText = vi.fn().mockRejectedValue(new Error('no clipboard'));
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const el: HTMLElement = fixture.nativeElement;
    (el.querySelector('[data-testid="card-copy"]') as HTMLButtonElement).click();
    await flush();
    expect(toast.messages().map((m) => [m.text, m.kind]))
      .toEqual([['Copy failed: no clipboard', 'error']]);
  });

  // ---------------------------------------------------------------------------------------
  // N1.2 (audit §3.A / backlog #26) — pins seed the pack. Before this, `TrailStore.pins()` had
  // no reader outside its own store and spec while three surfaces advertised the mechanism.
  // ---------------------------------------------------------------------------------------

  function step(kind: TrailStep['kind'], id: string, title: string, focus: string): TrailStep {
    return { kind, id, title, focus, ts: 1 };
  }

  function entry(nodeId: string, title: string, focus: string) {
    return { kind: 'http', title, nodeId, focus, project: 'Web' };
  }

  function seedGraph(...entries: ReturnType<typeof entry>[]): void {
    entryGroups.set([{ kind: 'http', label: 'HTTP', entries }]);
  }

  /** Clicks the picker's seed button — the product path, not the handler directly. */
  function clickSeed(fixture: { nativeElement: HTMLElement }): void {
    (fixture.nativeElement.querySelector('[data-testid="trail-seed"]') as HTMLButtonElement).click();
  }

  it('seeds the pack from PINS, not the raw trail, when any step is pinned (N1.2)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    seedGraph(
      entry('node-checkout', 'POST /checkout', 'CheckoutController.Post'),
      entry('node-orders', 'GET /orders', 'OrdersController.Get'),
    );
    trailSteps.set([
      step('entry', 'node-checkout', 'POST /checkout', 'CheckoutController.Post'),
      step('entry', 'node-orders', 'GET /orders', 'OrdersController.Get'),
    ]);
    pins.set([step('entry', 'node-orders', 'GET /orders', 'OrdersController.Get')]);

    const { fixture, studio } = createStudio();
    fixture.detectChanges();
    clickSeed(fixture);
    await flush();

    // ONE card, for the pinned entry — the trail's other step is not in the pack.
    expect(studio.cards().map((c) => c.entryIds)).toEqual([['node-orders']]);
    expect(studio.cards()[0].title).toBe('Flow: GET /orders');
    expect(getContextPack).toHaveBeenCalledTimes(1);
    expect(TestBed.inject(ToastService).messages().map((m) => m.text))
      .toEqual(['Seeded 1 card from 1 pinned step']);
  });

  it('falls back to the trail when nothing is pinned, and seeds node steps too (N1.2)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    seedGraph(entry('node-checkout', 'POST /checkout', 'CheckoutController.Post'));
    // A pinned graph NODE carries the focus of the trace it was explored under, so it resolves
    // to that entry. The pre-N1.2 body kept `kind === 'entry'` only and seeded nothing for it.
    trailSteps.set([
      step('node', 'node-handler', 'CheckoutHandler.Handle', 'CheckoutController.Post'),
      step('entry', 'node-checkout', 'POST /checkout', 'CheckoutController.Post'),
    ]);

    const { fixture, studio } = createStudio();
    fixture.detectChanges();
    clickSeed(fixture);
    await flush();

    // Both steps resolve to the same entry — deduped to one card.
    expect(studio.cards().map((c) => c.entryIds)).toEqual([['node-checkout']]);
    expect(TestBed.inject(ToastService).messages().map((m) => m.text))
      .toEqual(['Seeded 1 card from 2 trail steps']);
  });

  it('resolves pins against the LIVE graph and reports the ones that no longer exist (N1.2)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    // The graph moved on: only one of the two pinned focuses still exists, and the surviving
    // entry has a NEW node id. Seeding by focus is what keeps a dead id out of the pack.
    seedGraph(entry('node-checkout-v2', 'POST /checkout', 'CheckoutController.Post'));
    pins.set([
      step('entry', 'node-checkout', 'POST /checkout', 'CheckoutController.Post'),
      step('entry', 'node-gone', 'DELETE /legacy', 'LegacyController.Delete'),
    ]);

    const { fixture, studio } = createStudio();
    fixture.detectChanges();
    clickSeed(fixture);
    await flush();

    expect(studio.cards().map((c) => c.entryIds)).toEqual([['node-checkout-v2']]);
    expect(TestBed.inject(ToastService).messages().map((m) => [m.text, m.kind])).toEqual([
      ['Seeded 1 card from 2 pinned steps — 1 did not resolve in this graph', 'info'],
    ]);
  });

  it('says why nothing happened instead of no-opping in silence (N1.2)', async () => {
    getContextPack.mockResolvedValue(packResponse());
    seedGraph(entry('node-checkout', 'POST /checkout', 'CheckoutController.Post'));
    // A reroot step carries no focus, so it can never resolve to an entry.
    pins.set([step('reroot', 'node-x', 'CheckoutHandler', '')]);

    const { fixture, studio } = createStudio();
    fixture.detectChanges();
    clickSeed(fixture);
    await flush();

    expect(studio.cards()).toEqual([]);
    expect(getContextPack).not.toHaveBeenCalled();
    expect(TestBed.inject(ToastService).messages().map((m) => [m.text, m.kind])).toEqual([
      ['Nothing in pins resolves to an entry in this graph (1 of 1 unresolved)', 'error'],
    ]);
  });

  it('the seed button names its source and count, and is disabled at zero (N1.2)', () => {
    seedGraph(entry('node-checkout', 'POST /checkout', 'CheckoutController.Post'));
    const { fixture } = createStudio();
    fixture.detectChanges();
    const button = () =>
      (fixture.nativeElement as HTMLElement).querySelector('[data-testid="trail-seed"]') as HTMLButtonElement;

    expect(button().disabled).toBe(true);
    expect(button().title).toContain('Nothing to seed from yet');

    trailSteps.set([step('entry', 'node-checkout', 'POST /checkout', 'CheckoutController.Post')]);
    fixture.detectChanges();
    expect(button().disabled).toBe(false);
    expect(button().textContent).toContain('From current trail (1)');

    pins.set([step('entry', 'node-checkout', 'POST /checkout', 'CheckoutController.Post')]);
    fixture.detectChanges();
    expect(button().textContent).toContain('From 1 pinned step');
    expect(button().title).toContain('pins win over the raw trail');
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
