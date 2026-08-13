import { Component, computed, input, model, output } from '@angular/core';

import { Icon } from '../../ui/icon/icon';
import { allCardsPriced, cardTokens as cardTokensOf, totalCardTokens } from './card-tokens';
import { BODY_CAPABLE_CARD_TYPES, type ContextCard } from './composition-view';
import type { ContextIntent, OutputFormat } from './scope-picker';

const BUDGET_STOPS = [1000, 2000, 4000, 8000, 12000, 16000];

@Component({
  selector: 'app-budget-panel',
  imports: [Icon],
  host: { class: 'flex h-full min-h-0 flex-col' },
  template: `
    <div class="min-h-0 flex-1 overflow-y-auto p-2">
      <h3 class="mb-1.5 text-2xs font-semibold uppercase tracking-wider text-ink-muted">Budget</h3>
      <div class="mb-3">
        <div class="flex items-center justify-between mb-1">
          <label class="text-xs text-ink-subtle" for="budget-slider">Token budget</label>
          <span class="text-xs tabular-nums font-medium text-ink">{{ budget() }} tok</span>
        </div>
        <input
          id="budget-slider"
          type="range"
          class="w-full slider-track"
          [min]="budgetStops[0]"
          [max]="budgetStops[budgetStops.length - 1]"
          [step]="1000"
          [value]="budget()"
          (input)="onBudgetInput($event)"
        />
        <div class="flex justify-between text-2xs text-ink-subtle mt-0.5">
          @for (stop of budgetStops; track stop) {
            <span>{{ stop / 1000 }}k</span>
          }
        </div>
      </div>

      <h3 class="mb-1.5 text-2xs font-semibold uppercase tracking-wider text-ink-muted">Per-card meter</h3>
      @for (card of cards(); track card.id) {
        <div class="mb-1.5">
          <div class="flex items-center justify-between text-2xs mb-0.5">
            <span class="truncate text-ink-subtle">{{ card.title }}</span>
            <span class="shrink-0 tabular-nums" [class.text-warn]="cardTokens(card) > budgetPerCard()" [class.text-ink-subtle]="cardTokens(card) <= budgetPerCard()">
              {{ card.serverTokens !== null ? '' : '~' }}{{ cardTokens(card) }}
            </span>
          </div>
          <div class="h-1 rounded-full bg-hover">
            <div
              class="h-full rounded-full transition-[width] duration-300"
              [style.width.%]="barPct(card)"
              [style.background-color]="barColor(card)"
            ></div>
          </div>
        </div>
      } @empty {
        <p class="text-2xs text-ink-subtle">No cards to meter.</p>
      }

      <div class="mt-3 border-t border-line pt-2">
        <div class="flex items-center justify-between mb-1">
          <span class="text-xs font-medium text-ink">Total</span>
          <span class="text-xs tabular-nums" [class.text-warn]="totalTokens() > budget()" [class.text-success]="totalTokens() <= budget()">
            {{ allServer() ? '' : '~' }}{{ totalTokens() }} / {{ budget() }} tok
          </span>
        </div>
        <div class="h-2 rounded-full bg-hover">
          <div
            class="h-full rounded-full transition-[width] duration-300"
            [style.width.%]="totalBarPct()"
            [style.background-color]="totalTokens() > budget() ? 'var(--vibe-danger)' : 'var(--vibe-success)'"
          ></div>
        </div>
        @if (totalTokens() > budget()) {
          <p class="mt-1 text-2xs text-warn">Over budget &mdash; remove cards or increase limit.</p>
        }
      </div>

      <!-- T5.2 — the verification ledger is projected here by the studio. -->
      <ng-content />

      @if (omitted().length > 0) {
        <div class="mt-3 border-t border-line pt-2" data-testid="omitted-list">
          <h3 class="mb-1 flex items-center gap-1 text-2xs font-semibold uppercase tracking-wider text-warn">
            <app-icon name="alert-triangle" [size]="12" />
            Omitted ({{ omitted().length }})
          </h3>
          <ul class="space-y-0.5">
            @for (line of omitted(); track line) {
              <li class="text-2xs leading-snug text-ink-subtle" [title]="line">{{ line }}</li>
            }
          </ul>
        </div>
      }

      <div class="mt-3 border-t border-line pt-2">
        <h3 class="mb-1 text-2xs font-semibold uppercase tracking-wider text-ink-muted">Intent</h3>
        <div class="flex gap-1 mb-2">
          @for (intent of intents; track intent) {
            <button
              type="button"
              class="rounded px-2 py-0.5 text-xs transition-colors"
              [class.bg-accent]="selectedIntent() === intent"
              [class.text-accent-ink]="selectedIntent() === intent"
              [class.text-ink-subtle]="selectedIntent() !== intent"
              [class.hover:bg-hover]="selectedIntent() !== intent"
              (click)="selectedIntent.set(intent)"
            >{{ intentLabel(intent) }}</button>
          }
        </div>
      </div>

      <!-- N1.1 (audit §3.F.2) — the pill only appears when the pack HAS a card that carries
           code bodies. It used to claim "All bodies hidden" over a pack that still contained
           every body, and it said it even when no card could carry one. -->
      @if (bodyCardCount() > 0) {
        <div class="mt-2 border-t border-line pt-2">
          <h3 class="mb-1 text-2xs font-semibold uppercase tracking-wider text-ink-muted">Bodies</h3>
          <button
            type="button"
            class="flex w-full items-center gap-1.5 rounded px-2 py-1 text-xs transition-colors"
            data-testid="all-bodies-toggle"
            [class.text-success]="showAllBodies()"
            [class.text-ink-subtle]="!showAllBodies()"
            [class.hover:bg-hover]="true"
            (click)="toggleAllBodies()"
          >
            <app-icon [name]="showAllBodies() ? 'eye' : 'eye-off'" [size]="14" />
            {{ showAllBodies() ? 'Bodies included' : 'Bodies excluded from the pack' }}
          </button>
        </div>
      }

      <div class="mt-2 border-t border-line pt-2">
        <h3 class="mb-1 text-2xs font-semibold uppercase tracking-wider text-ink-muted">Format</h3>
        <div class="flex gap-1">
          @for (fmt of formats; track fmt) {
            <button
              type="button"
              class="rounded px-2 py-0.5 text-xs transition-colors"
              [class.bg-accent]="selectedFormat() === fmt"
              [class.text-accent-ink]="selectedFormat() === fmt"
              [class.text-ink-subtle]="selectedFormat() !== fmt"
              [class.hover:bg-hover]="selectedFormat() !== fmt"
              (click)="selectedFormat.set(fmt)"
            >{{ fmt }}</button>
          }
        </div>
      </div>
    </div>

    <div class="flex gap-1.5 border-t border-line px-2 py-1.5">
      <button
        type="button"
        class="flex flex-1 items-center justify-center gap-1 rounded bg-accent px-2 py-1 text-xs font-medium text-accent-ink hover:bg-accent/90 disabled:opacity-30 transition-colors"
        data-testid="copy-context"
        [disabled]="cards().length === 0 || !exportReady()"
        (click)="onCopy()"
      >
        {{ packPending() ? 'Packing…' : copyLabel() }}
      </button>
      <button
        type="button"
        class="flex items-center justify-center gap-1 rounded border border-line px-2 py-1 text-xs text-ink hover:bg-hover disabled:opacity-30 transition-colors"
        data-testid="save-context"
        [disabled]="cards().length === 0 || !exportReady()"
        (click)="onSave()"
      >
        Save
      </button>
    </div>
  `,
})
export class BudgetPanel {
  readonly cards = input<readonly ContextCard[]>([]);
  /** T5.1 (audit R1) — the server's omitted[] lines; silent truncation is a trust bug. */
  readonly omitted = input<readonly string[]>([]);
  /** T5.6 (audit C1) — re-pack in flight: Copy shows "Packing…" so the wait is visible. */
  readonly packPending = input(false);
  /** T5.6 (audit C1) — false while the pack is stale/absent; Copy/Save disabled, never stale bytes. */
  readonly exportReady = input(true);

  readonly copyRequest = output<void>();
  readonly saveRequest = output<void>();
  readonly intentChange = output<ContextIntent>();
  readonly formatChange = output<OutputFormat>();
  readonly globalBodiesChange = output<void>();

  readonly budget = model(4000);
  readonly selectedIntent = model<ContextIntent>('trace');
  readonly selectedFormat = model<OutputFormat>('markdown');
  readonly showAllBodies = model(true);
  /** N0.1 (audit §3.F.7) — owned by the parent, which performs the copy: "Copied!" now appears
   * only after the clipboard write actually resolved, never optimistically on click. */
  readonly copied = input(false);

  readonly budgetStops = BUDGET_STOPS;
  readonly intents: readonly ContextIntent[] = ['trace', 'explain', 'review'];
  readonly formats: readonly OutputFormat[] = ['markdown', 'plain', 'json'];

  // Batch E (R2 §2.E item 2): both totals on this screen come from ONE function (card-tokens.ts).
  readonly totalTokens = (): number => totalCardTokens(this.cards());

  readonly allServer = (): boolean => allCardsPriced(this.cards());

  readonly budgetPerCard = (): number => {
    const n = this.cards().length || 1;
    return Math.floor(this.budget() / n);
  };

  cardTokens(card: ContextCard): number {
    return cardTokensOf(card);
  }

  barPct(card: ContextCard): number {
    const perCard = this.budgetPerCard();
    if (perCard === 0) return 0;
    return Math.min(100, (this.cardTokens(card) / perCard) * 100);
  }

  totalBarPct(): number {
    const b = this.budget();
    if (b === 0) return 100;
    return Math.min(100, (this.totalTokens() / b) * 100);
  }

  barColor(card: ContextCard): string {
    return this.cardTokens(card) > this.budgetPerCard()
      ? 'var(--vibe-danger)'
      : 'var(--vibe-success)';
  }

  intentLabel(intent: ContextIntent): string {
    switch (intent) {
      case 'trace': return 'Trace';
      case 'explain': return 'Explain';
      case 'review': return 'Review';
      default: return intent;
    }
  }

  copyLabel(): string {
    return this.copied() ? 'Copied!' : 'Copy';
  }

  onBudgetInput(event: Event): void {
    const value = parseInt((event.target as HTMLInputElement).value, 10);
    this.budget.set(value);
  }

  /** N1.1 — how many cards the bodies switch can actually act on. Zero means no switch. */
  readonly bodyCardCount = computed(() =>
    this.cards().filter((c) => BODY_CAPABLE_CARD_TYPES.includes(c.type)).length);

  toggleAllBodies(): void {
    this.showAllBodies.update((v) => !v);
    this.globalBodiesChange.emit();
  }

  onCopy(): void {
    this.copyRequest.emit();
  }

  onSave(): void {
    this.saveRequest.emit();
  }
}
