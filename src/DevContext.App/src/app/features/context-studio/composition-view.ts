import { Component, input, output, signal } from '@angular/core';

import { Icon } from '../../ui/icon/icon';
import type { ContextCardType } from './scope-picker';

export interface ContextCard {
  readonly id: string;
  readonly type: ContextCardType;
  readonly title: string;
  readonly entryIds: readonly string[];
  estimatedLines: number;
  content: string | null;
  loading: boolean;
  bodyEnabled: boolean;
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

const CARD_TYPE_COLORS: Record<ContextCardType, string> = {
  flow: 'var(--vibe-info)',
  signatures: 'var(--vibe-accent)',
  bodies: 'var(--vibe-success)',
  di_wiring: 'var(--vibe-accent-pink)',
  config: 'var(--vibe-warn)',
  entities: 'var(--vibe-accent)',
  contracts: 'var(--vibe-info)',
  tests: 'var(--vibe-danger)',
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
          [class.border-accent/30]="card.type === 'flow'"
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
            } @else if (card.content !== null) {
              <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ card.estimatedLines }}L</span>
            } @else {
              <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ card.estimatedLines }}L</span>
            }
            <button
              type="button"
              class="ml-1 shrink-0 rounded p-0.5 text-ink-subtle hover:bg-hover hover:text-ink transition-colors"
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
      <span>{{ totalLines() }} lines</span>
    </div>
  `,
})
export class CompositionView {
  readonly cards = input<readonly ContextCard[]>([]);

  readonly cardToggleBody = output<string>();
  readonly cardRemove = output<string>();
  readonly cardReorder = output<{ fromIndex: number; toIndex: number }>();

  protected readonly dragIndex = signal<number | null>(null);
  protected readonly dragOverIndex = signal<number | null>(null);

  protected readonly totalLines = (): number =>
    this.cards().reduce((n, c) => n + c.estimatedLines, 0);

  protected typeLabel(type: ContextCardType): string {
    return CARD_TYPE_LABELS[type];
  }

  protected typeColor(type: ContextCardType): string {
    return CARD_TYPE_COLORS[type];
  }

  protected onToggleBody(id: string): void {
    this.cardToggleBody.emit(id);
  }

  protected onRemove(id: string): void {
    this.cardRemove.emit(id);
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
