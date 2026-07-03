import { Component, computed, input } from '@angular/core';

export type MeterVariant = 'accent' | 'success' | 'warn' | 'danger';

/** Thin horizontal gauge for a 0-100 value (confidence ledger %, coverage, funnel stages).
 * `.hairline`-adjacent but a resting indicator, not a loading state — no animation. */
@Component({
  selector: 'app-meter',
  template: `
    <div class="h-1 min-w-0 flex-1 overflow-hidden rounded-full bg-surface-2">
      <div class="h-full rounded-full" [class]="fillClass()" [style.width.%]="clamped()"></div>
    </div>
  `,
  host: { class: 'flex items-center', role: 'meter', '[attr.aria-valuenow]': 'clamped()' },
})
export class Meter {
  readonly value = input.required<number>();
  readonly variant = input<MeterVariant>('accent');

  protected readonly clamped = computed(() => Math.min(100, Math.max(0, this.value())));
  protected readonly fillClass = computed(() => {
    switch (this.variant()) {
      case 'success':
        return 'bg-success';
      case 'warn':
        return 'bg-warn';
      case 'danger':
        return 'bg-danger';
      default:
        return 'bg-accent';
    }
  });
}
