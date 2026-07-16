import { Component, computed, effect, inject, signal } from '@angular/core';

import { DevContextApi } from '../../data-access/devcontext-api';
import { type EntryVm } from '../../models/view-models';
import { SessionStore } from '../../state/session.store';
import { TrailStore } from '../../state/trail.store';
import { BudgetPanel } from './budget-panel';
import { type ContextCard, CompositionView } from './composition-view';
import { type ContextCardSeed, ScopePicker, type ContextIntent, type OutputFormat } from './scope-picker';

const INTENT_CARD_ORDER: Record<ContextIntent, readonly string[]> = {
  trace: ['flow', 'signatures', 'bodies', 'di_wiring', 'config', 'entities', 'contracts', 'tests', 'identity'],
  explain: ['identity', 'di_wiring', 'entities', 'contracts', 'signatures', 'bodies', 'flow', 'tests', 'config'],
  review: ['flow', 'bodies', 'signatures', 'di_wiring', 'entities', 'contracts', 'tests', 'config', 'identity'],
};

@Component({
  selector: 'app-context-studio',
  imports: [ScopePicker, CompositionView, BudgetPanel],
  template: `
    <div class="flex h-full min-h-0">
      <app-scope-picker
        class="w-56 shrink-0 border-r border-line bg-surface"
        [entryGroups]="session.entryGroups()"
        (cardsChange)="onCardsChange($event)"
        (trailSeedRequest)="onTrailSeed()"
        (omniboxCard)="onCardsChange([$event])"
      />

      <app-composition-view
        class="min-w-0 flex-1 bg-base"
        [cards]="cards()"
        (cardToggleBody)="onToggleBody($event)"
        (cardRemove)="onRemove($event)"
        (cardReorder)="onReorder($event)"
        (cardRetry)="onRetry()"
      />

      <app-budget-panel
        class="w-48 shrink-0 border-l border-line bg-surface"
        [cards]="cards()"
        [omitted]="packOmitted()"
        [(budget)]="budgetTokens"
        [(selectedIntent)]="selectedIntent"
        [(selectedFormat)]="selectedFormat"
        [(showAllBodies)]="showAllBodies"
        (copyRequest)="onCopy()"
        (saveRequest)="onSave()"
        (globalBodiesChange)="onGlobalBodiesChange()"
      />
    </div>
  `,
  host: { class: 'h-full min-h-0' },
})
export class ContextStudio {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly trailStore = inject(TrailStore);

  protected readonly cards = signal<readonly ContextCard[]>([]);
  protected readonly selectedIntent = signal<ContextIntent>('trace');
  protected readonly selectedFormat = signal<OutputFormat>('markdown');
  protected readonly showAllBodies = signal(true);
  protected readonly budgetTokens = signal(4000);

  private lastIntent: ContextIntent = 'trace';

  constructor() {
    effect(() => {
      const intent = this.selectedIntent();
      if (this.lastIntent !== intent && this.cards().length > 0) {
        this.sortByIntent(intent);
      }
      this.lastIntent = intent;
    });
  }

  private sortByIntent(intent: ContextIntent): void {
    const order = INTENT_CARD_ORDER[intent];
    const idx = new Map(order.map((k, i) => [k, i]));
    this.cards.update((prev) =>
      [...prev].sort((a, b) => (idx.get(a.type) ?? 99) - (idx.get(b.type) ?? 99)),
    );
  }

  protected readonly totalTokens = computed(() =>
    this.cards().reduce((n, c) => n + (c.serverTokens ?? Math.round(c.estimatedLines * 2.5)), 0),
  );

  protected serverPackMarkdown: string | null = null;

  /** T5.1 (audit R1) — what the server cut, rendered in the budget panel. */
  protected readonly packOmitted = signal<readonly string[]>([]);

  protected onCardsChange(seeds: readonly ContextCardSeed[]): void {
    const handle = this.session.handle();
    if (!handle) return;

    const entryMap = new Map<string, EntryVm>();
    for (const group of this.session.entryGroups()) {
      for (const e of group.entries) {
        entryMap.set(e.nodeId, e);
      }
    }

    const newCards: ContextCard[] = seeds.map((s) => ({
      id: crypto.randomUUID(),
      type: s.type,
      title: s.title,
      entryIds: s.entryIds,
      estimatedLines: s.estimatedLines,
      content: null,
      loading: true,
      bodyEnabled: this.showAllBodies(),
      serverTokens: null,
      sectionTokens: [],
      provenance: s.entryIds
        .map((id) => entryMap.get(id)?.provenance)
        .filter((p): p is string => !!p),
      error: null,
    }));

    this.cards.update((prev) => [...prev, ...newCards]);

    void this.loadAllCards(newCards, handle);
  }

  private async loadAllCards(newCards: readonly ContextCard[], handle: string): Promise<void> {
    // L4.4 — Single GetContextPack call replaces N individual GetContext calls (closes Trap A).
    const requestedIds = new Set(newCards.map((c) => c.id));
    const cardSpecs = newCards
      .filter((c) => c.type !== 'config' && c.type !== 'tests')
      .map((c) => ({ type: c.type, title: c.title, entryIds: [...c.entryIds] }));

    if (cardSpecs.length === 0) {
      this.cards.update((prev) =>
        prev.map((c) => (requestedIds.has(c.id) ? { ...c, loading: false } : c)),
      );
      return;
    }

    try {
      const pack = await this.api.getContextPack(handle, cardSpecs, {
        budgetTokens: this.budgetTokens(),
        intent: this.selectedIntent(),
      });

      this.serverPackMarkdown = pack.assembledMarkdown || null;
      this.packOmitted.set(pack.omitted);

      const cardByType = new Map<string, typeof pack.cards[0]>();
      for (const ci of pack.cards) {
        cardByType.set(ci.type, ci);
      }

      this.cards.update((prev) =>
        prev.map((c) => {
          if (!requestedIds.has(c.id)) return c;
          const ci = cardByType.get(c.type);
          if (!ci) return { ...c, loading: false };
          return {
            ...c,
            content: ci.title,
            loading: false,
            error: null,
            serverTokens: ci.tokens > 0 ? ci.tokens : null,
            estimatedLines: ci.tokens > 0 ? Math.max(1, Math.round(ci.tokens / 2.5)) : c.estimatedLines,
            sectionTokens: ci.sections.map((s) => ({ key: s.key, tokens: s.tokens })),
          };
        }),
      );
    } catch (e) {
      // T5.1 (audit R4) — a failed RPC must SAY so on the cards, not just stop the spinners.
      // The old path also mutated card objects in place, which never triggered a signal update.
      const message = e instanceof Error ? e.message : 'Context pack request failed';
      this.cards.update((prev) =>
        prev.map((c) => (requestedIds.has(c.id) ? { ...c, loading: false, error: message } : c)),
      );
    }
  }

  /** T5.1 (audit R4) — retry every failed card in one batch. */
  protected onRetry(): void {
    const handle = this.session.handle();
    if (!handle) return;
    const failed = this.cards().filter((c) => c.error !== null);
    if (failed.length === 0) return;
    this.cards.update((prev) =>
      prev.map((c) => (c.error !== null ? { ...c, loading: true, error: null } : c)),
    );
    void this.loadAllCards(failed, handle);
  }

  protected onToggleBody(id: string): void {
    this.cards.update((prev) =>
      prev.map((c) => (c.id === id ? { ...c, bodyEnabled: !c.bodyEnabled } : c)),
    );
  }

  protected onGlobalBodiesChange(): void {
    const showAll = this.showAllBodies();
    this.cards.update((prev) =>
      prev.map((c) => ({ ...c, bodyEnabled: showAll })),
    );
  }

  protected onRemove(id: string): void {
    this.cards.update((prev) => prev.filter((c) => c.id !== id));
  }

  protected onTrailSeed(): void {
    const steps = this.trailStore.steps();
    if (steps.length === 0) return;
    const seeds: ContextCardSeed[] = [];
    const seen = new Set<string>();
    for (const step of steps) {
      if (step.kind === 'entry') {
        const found = this.findEntryByFocus(step.focus);
        if (found && !seen.has(found.nodeId)) {
          seen.add(found.nodeId);
          seeds.push({
            type: 'flow',
            title: `Flow: ${step.title}`,
            entryIds: [found.nodeId],
            estimatedLines: 15,
          });
        }
      }
    }
    if (seeds.length > 0) {
      this.onCardsChange(seeds);
    }
  }

  private findEntryByFocus(focus: string) {
    for (const group of this.session.entryGroups()) {
      for (const e of group.entries) {
        if (e.focus === focus) return e;
      }
    }
    return null;
  }

  protected onReorder(event: { fromIndex: number; toIndex: number }): void {
    this.cards.update((prev) => {
      const arr = [...prev];
      const [item] = arr.splice(event.fromIndex, 1);
      arr.splice(event.toIndex, 0, item);
      return arr;
    });
  }

  protected onCopy(): void {
    const text = this.buildContext(this.selectedFormat());
    void navigator.clipboard.writeText(text);
  }

  protected onSave(): void {
    const format = this.selectedFormat();
    const text = this.buildContext(format);
    const blob = new Blob([text], { type: format === 'plain' ? 'text/plain' : 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = this.saveFileName(format);
    a.click();
    URL.revokeObjectURL(url);
  }

  /** T5.1 (audit R5) — plain format saves .txt, not a hardcoded .md. */
  protected saveFileName(format: OutputFormat): string {
    return format === 'plain' ? 'devcontext-context.txt' : 'devcontext-context.md';
  }

  private buildContext(format: OutputFormat): string {
    // L4.4 — Copy/Save = exactly the server-assembled pack when available (closes Trap A).
    if (this.serverPackMarkdown && this.serverPackMarkdown.length > 0) {
      if (format === 'plain') {
        return this.serverPackMarkdown
          .replace(/^#.*$/gm, '')
          .replace(/^_.*_$/gm, '')
          .replace(/^<!--.*-->$/gm, '')
          .replace(/^\n{3,}/gm, '\n\n')
          .trim();
      }
      return this.serverPackMarkdown;
    }

    // Fallback: client-side assembly for pre-L4.4 sessions or error cases.
    const cards = this.cards();
    if (cards.length === 0) return '';

    if (format === 'plain') {
      const lines: string[] = [];
      for (const card of cards) {
        const tok = card.serverTokens ?? Math.round(card.estimatedLines * 2.5);
        lines.push(`${card.title} [${card.type}]`);
        if (!card.bodyEnabled) lines.push('(code bodies omitted)');
        if (card.content !== null) {
          lines.push(card.content);
        } else {
          lines.push(`(No content — ~${tok} tok)`);
        }
        lines.push('');
      }
      lines.push('---');
      lines.push(`Generated by DevContext Context Studio — ${new Date().toISOString()}`);
      return lines.join('\n');
    }

    const lines: string[] = ['# DevContext — Context Pack', ''];
    for (const card of cards) {
      const tok = card.serverTokens ?? Math.round(card.estimatedLines * 2.5);
      const prefix = card.serverTokens !== null ? '' : '~';
      lines.push(`## ${card.title}`);
      lines.push(`_type: ${card.type}, ${card.entryIds.length} source(s), ${prefix}${tok} tok_`);
      if (!card.bodyEnabled) {
        lines.push('_(code bodies omitted)_');
      }
      lines.push('');
      if (card.content !== null) {
        lines.push(card.content);
      } else {
        lines.push(`_Loading content…_`);
      }
      lines.push('');
      lines.push(`<!-- context card: ${card.type} -->`);
      lines.push('');
    }
    lines.push('---');
    lines.push(`_Generated by DevContext Context Studio — ${new Date().toISOString()}_`);
    return lines.join('\n');
  }
}
