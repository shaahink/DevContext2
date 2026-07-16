import { Component, inject, input, output, signal } from '@angular/core';

import { Icon } from '../../ui/icon/icon';
import { ToastService } from '../../ui/toast/toast';
import type { ContextCardType } from './scope-picker';

/** T5.3 — a card's server section with its real content and provenance (T4.4 fields). */
export interface CardSection {
  readonly key: string;
  readonly tokens: number;
  readonly content: string;
  readonly sourceLocations: readonly string[];
  readonly verified: number;
  readonly approx: number;
}

export interface ContextCard {
  readonly id: string;
  readonly type: ContextCardType;
  readonly title: string;
  readonly entryIds: readonly string[];
  estimatedLines: number;
  content: string | null;
  loading: boolean;
  bodyEnabled: boolean;
  serverTokens: number | null;
  sections: readonly CardSection[];
  provenance: readonly string[];
  /** T5.1 (audit R4) — RPC failure message; a failed card must say so, not just stop spinning. */
  error: string | null;
}

const CARD_TYPE_LABELS: Record<ContextCardType, string> = {
  flow: 'Flow',
  signatures: 'Signatures',
  bodies: 'Bodies',
  di_wiring: 'DI Wiring',
  config: 'Config',
  entities: 'Entities',
  contracts: 'Contracts',
  tests: 'Tests',
  identity: 'Identity',
};

// T5.5 — danger is reserved for ERROR states (a red type badge reads as a failure), and
// --vibe-accent-pink never existed in styles.css (di_wiring's badge silently rendered
// with no color at all).
const CARD_TYPE_COLORS: Record<ContextCardType, string> = {
  flow: 'var(--vibe-info)',
  signatures: 'var(--vibe-accent)',
  bodies: 'var(--vibe-success)',
  di_wiring: 'var(--vibe-accent-dim)',
  config: 'var(--vibe-warn)',
  entities: 'var(--vibe-accent)',
  contracts: 'var(--vibe-info)',
  tests: 'var(--vibe-warn)',
  identity: 'var(--vibe-ink-muted)',
};

@Component({
  selector: 'app-composition-view',
  imports: [Icon],
  host: { class: 'flex h-full min-h-0 flex-col' },
  template: `
    <div class="min-h-0 flex-1 overflow-y-auto px-2 py-1">
      @for (card of cards(); track card.id; let i = $index) {
        <div
          class="mb-1 rounded border border-line bg-surface transition-colors"
          [class.border-accent/30]="card.type === 'flow' && !card.error"
          [class.border-danger/50]="!!card.error"
          [class.opacity-60]="card.loading"
          draggable="true"
          (dragstart)="onDragStart($event, i)"
          (dragover)="onDragOver($event, i)"
          (dragend)="onDragEnd()"
        >
          <div class="flex items-center gap-1.5 px-2 py-1">
            <app-icon name="grip-vertical" [size]="14" class="shrink-0 cursor-grab text-ink-subtle hover:text-ink" />
            <span
              class="shrink-0 rounded px-1 py-0.5 text-2xs font-medium"
              [style.background-color]="typeColor(card.type) + '20'"
              [style.color]="typeColor(card.type)"
            >{{ typeLabel(card.type) }}</span>
            <span class="min-w-0 flex-1 truncate text-xs text-ink" [title]="card.title">{{ card.title }}</span>
            @if (card.loading) {
              <app-icon name="loader" [size]="12" class="shrink-0 animate-spin text-ink-subtle" />
            } @else if (card.serverTokens !== null) {
              <span class="shrink-0 text-2xs tabular-nums" [class.text-success]="true" [title]="'Server-computed: ' + card.serverTokens + ' tokens'">{{ formatTokens(card.serverTokens) }}</span>
            } @else {
              <!-- T5.5 — one unit everywhere: estimates are shown in tokens, not line counts. -->
              <span class="shrink-0 text-2xs tabular-nums text-ink-subtle" title="Estimated — server tokens pending">~{{ formatTokens(estTokens(card)) }}</span>
            }
            <button
              type="button"
              class="ml-1 shrink-0 rounded p-0.5 text-ink-subtle hover:bg-hover hover:text-ink transition-colors disabled:opacity-30"
              title="Copy this card only"
              data-testid="card-copy"
              [disabled]="card.sections.length === 0"
              (click)="onCopyCard(card)"
            >
              <app-icon name="copy" [size]="14" />
            </button>
            <button
              type="button"
              class="shrink-0 rounded p-0.5 text-ink-subtle hover:bg-hover hover:text-ink transition-colors"
              [class.opacity-30]="!card.bodyEnabled"
              [title]="card.bodyEnabled ? 'Hide code bodies' : 'Show code bodies'"
              (click)="onToggleBody(card.id)"
            >
              <app-icon [name]="card.bodyEnabled ? 'eye' : 'eye-off'" [size]="14" />
            </button>
            <button
              type="button"
              class="shrink-0 rounded p-0.5 text-ink-subtle hover:text-danger transition-colors"
              title="Remove card"
              (click)="onRemove(card.id)"
            >
              <app-icon name="x" [size]="14" />
            </button>
          </div>
          @if (card.error && !card.loading) {
            <div class="flex items-center gap-1.5 border-t border-danger/30 bg-danger/10 px-2 py-1" data-testid="card-error">
              <app-icon name="alert-triangle" [size]="12" class="shrink-0 text-danger" />
              <span class="min-w-0 flex-1 truncate text-2xs text-danger" [title]="card.error">{{ card.error }}</span>
              <button
                type="button"
                class="shrink-0 rounded border border-danger/40 px-1.5 py-px text-2xs text-danger hover:bg-danger/20 transition-colors"
                data-testid="card-retry"
                (click)="onRetry()"
              >
                Retry
              </button>
            </div>
          }
          @if (cardLocations(card).length > 0 || card.provenance.length > 0 || card.sections.length > 0) {
            <div class="flex items-center gap-1 px-2 py-0.5 border-t border-line/50">
              <!-- T5.3 (audit §4.2) — chips are the card's OWN file:line source set (T4.4
                   provenance), not the entry's provenance echoed on every card; click copies
                   the repo-relative file:line for the editor/agent. Entry provenance only
                   fills in while the pack is still loading. -->
              @if (cardLocations(card).length > 0) {
                @for (loc of cardLocations(card).slice(0, 4); track loc) {
                  <button
                    type="button"
                    class="shrink-0 rounded bg-hover px-1 py-px text-2xs font-mono text-ink-muted hover:text-ink transition-colors"
                    data-testid="provenance-chip"
                    [title]="loc + ' — click to copy'"
                    (click)="onCopyLocation(loc)"
                  >{{ shortLocation(loc) }}</button>
                }
                @if (cardLocations(card).length > 4) {
                  <span class="shrink-0 text-2xs text-ink-subtle">+{{ cardLocations(card).length - 4 }}</span>
                }
              } @else {
                @for (pv of card.provenance; track pv) {
                  <span class="shrink-0 rounded bg-hover px-1 py-px text-2xs font-mono text-ink-muted" [title]="pv">{{ shortLocation(pv) }}</span>
                }
              }
              @if (card.sections.length > 0) {
                <span class="ml-auto shrink-0 text-2xs tabular-nums text-ink-subtle">
                  @for (st of card.sections; track st.key; let last = $last) {
                    {{ st.key }}: {{ formatTokens(st.tokens) }}{{ last ? '' : ' · ' }}
                  }
                </span>
              }
            </div>
          }
          @if (card.content !== null && !card.loading) {
            <div class="border-t border-line px-2 py-1">
              <pre class="max-h-24 overflow-y-auto text-2xs text-ink-muted leading-relaxed whitespace-pre-wrap">{{ card.content }}</pre>
            </div>
          }
        </div>
        @if (dragOverIndex() === i + 1 && dragIndex() !== i) {
          <div class="mb-1 h-0.5 rounded bg-accent"></div>
        }
      } @empty {
        <div class="flex h-full flex-col items-center justify-center gap-2 px-3 py-6 text-center text-xs text-ink-subtle">
          <app-icon name="layers" [size]="24" class="text-ink-subtle/50" />
          <span>No cards yet. Pick items from the scope picker to build context.</span>
        </div>
      }
    </div>

    <div class="flex items-center justify-between border-t border-line px-2 py-0.5 text-2xs text-ink-subtle">
      <span>{{ cards().length }} card{{ cards().length !== 1 ? 's' : '' }}</span>
      <span>{{ totalTokens() }}</span>
    </div>
  `,
})
export class CompositionView {
  private readonly toast = inject(ToastService);

  readonly cards = input<readonly ContextCard[]>([]);

  readonly cardToggleBody = output<string>();
  readonly cardRemove = output<string>();
  readonly cardReorder = output<{ fromIndex: number; toIndex: number }>();
  /** T5.1 (audit R4) — retry re-requests every failed card in one batch. */
  readonly cardRetry = output<void>();

  protected readonly dragIndex = signal<number | null>(null);
  protected readonly dragOverIndex = signal<number | null>(null);

  protected readonly totalTokens = (): string => {
    const tok = this.cards().reduce((n, c) => n + (c.serverTokens ?? Math.round(c.estimatedLines * 2.5)), 0);
    if (tok < 1000) return `${tok} tok`;
    return `${(tok / 1000).toFixed(1)}k tok`;
  };

  protected typeLabel(type: ContextCardType): string {
    return CARD_TYPE_LABELS[type];
  }

  protected typeColor(type: ContextCardType): string {
    return CARD_TYPE_COLORS[type];
  }

  protected formatTokens(tok: number): string {
    if (tok < 1000) return `${tok} tok`;
    return `${(tok / 1000).toFixed(1)}k tok`;
  }

  protected estTokens(card: ContextCard): number {
    return Math.round(card.estimatedLines * 2.5);
  }

  /** T5.3 (audit §4.2) — the card's own source set: union of its sections' file:line
   * locations, in section order, deduped. */
  protected cardLocations(card: ContextCard): readonly string[] {
    const seen = new Set<string>();
    for (const s of card.sections) {
      for (const loc of s.sourceLocations) seen.add(loc);
    }
    return [...seen];
  }

  /** Filename:line tail for the chip; the full repo-relative path lives on [title]. */
  protected shortLocation(location: string): string {
    const lastSep = Math.max(location.lastIndexOf('/'), location.lastIndexOf('\\'));
    return lastSep >= 0 ? location.slice(lastSep + 1) : location;
  }

  /** T5.3 (R7) — copy ONE card: its heading + the real section content, pack-shaped. */
  protected onCopyCard(card: ContextCard): void {
    if (card.sections.length === 0) return;
    const text = `## ${card.title}\n_type: ${card.type}, ${card.serverTokens ?? 0} tok_\n\n`
      + card.sections.map((s) => s.content).join('\n');
    void navigator.clipboard.writeText(text);
    this.toast.show('Card copied to clipboard', 'success');
  }

  /** T5.3 — chips click-through: copy the repo-relative file:line for the editor/agent. */
  protected onCopyLocation(location: string): void {
    void navigator.clipboard.writeText(location);
    this.toast.show(`${location} copied`, 'success');
  }

  protected onToggleBody(id: string): void {
    this.cardToggleBody.emit(id);
  }

  protected onRemove(id: string): void {
    this.cardRemove.emit(id);
  }

  protected onRetry(): void {
    this.cardRetry.emit();
  }

  protected onDragStart(event: DragEvent, index: number): void {
    this.dragIndex.set(index);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', String(index));
    }
  }

  protected onDragOver(event: DragEvent, index: number): void {
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
    this.dragOverIndex.set(index);
  }

  protected onDragEnd(): void {
    const from = this.dragIndex();
    let to = this.dragOverIndex();
    if (from !== null && to !== null && from !== to) {
      if (to > from) to = to - 1;
      if (from !== to) {
        this.cardReorder.emit({ fromIndex: from, toIndex: to });
      }
    }
    this.dragIndex.set(null);
    this.dragOverIndex.set(null);
  }
}
