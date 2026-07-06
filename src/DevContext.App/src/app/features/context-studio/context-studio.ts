import { Component, computed, effect, inject, signal } from '@angular/core';

import { DevContextApi } from '../../data-access/devcontext-api';
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
      />

      <app-budget-panel
        class="w-48 shrink-0 border-l border-line bg-surface"
        [cards]="cards()"
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

  protected readonly totalLines = computed(() =>
    this.cards().reduce((n, c) => n + c.estimatedLines, 0),
  );

  protected onCardsChange(seeds: readonly ContextCardSeed[]): void {
    const handle = this.session.handle();
    if (!handle) return;

    const newCards: ContextCard[] = seeds.map((s) => ({
      id: crypto.randomUUID(),
      type: s.type,
      title: s.title,
      entryIds: s.entryIds,
      estimatedLines: s.estimatedLines,
      content: null,
      loading: false,
      bodyEnabled: this.showAllBodies(),
    }));

    this.cards.update((prev) => [...prev, ...newCards]);

    for (const card of newCards) {
      void this.loadCardContent(card, handle);
    }
  }

  private async loadCardContent(card: ContextCard, handle: string): Promise<void> {
    const focusEntry = this.findEntryForCard(card);
    if (!focusEntry) return;

    card.loading = true;
    try {
      const res = await this.api.getContext(handle, focusEntry.focus, {
        intent: this.selectedIntent(),
        budgetTokens: 4000,
      });
      const parts: string[] = [];
      for (const section of res.sections) {
        parts.push(`## ${section.key}\n${section.content}`);
      }
      card.content = parts.join('\n\n') || null;
      if (res.totalTokens > 0) {
        card.estimatedLines = Math.max(1, Math.round(res.totalTokens / 2.5));
      }
    } catch {
      card.content = null;
    } finally {
      card.loading = false;
    }
  }

  private findEntryForCard(card: ContextCard) {
    for (const group of this.session.entryGroups()) {
      for (const e of group.entries) {
        if (card.entryIds.includes(e.nodeId)) return e;
      }
    }
    return null;
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
    const text = this.buildContext(this.selectedFormat());
    const blob = new Blob([text], { type: this.selectedFormat() === 'plain' ? 'text/plain' : 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'devcontext-context.md';
    a.click();
    URL.revokeObjectURL(url);
  }

  private buildContext(format: OutputFormat): string {
    const cards = this.cards();
    if (cards.length === 0) return '';

    if (format === 'plain') {
      const lines: string[] = [];
      for (const card of cards) {
        lines.push(`${card.title} [${card.type}]`);
        if (!card.bodyEnabled) lines.push('(code bodies omitted)');
        if (card.content !== null) {
          lines.push(card.content);
        } else {
          lines.push(`(No content — ~${Math.round(card.estimatedLines * 2.5)} tok)`);
        }
        lines.push('');
      }
      lines.push('---');
      lines.push(`Generated by DevContext Context Studio — ${new Date().toISOString()}`);
      return lines.join('\n');
    }

    const lines: string[] = ['# DevContext — Context Pack', ''];
    for (const card of cards) {
      lines.push(`## ${card.title}`);
      lines.push(`_type: ${card.type}, ${card.entryIds.length} source(s), ~${Math.round(card.estimatedLines * 2.5)} tok_`);
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
