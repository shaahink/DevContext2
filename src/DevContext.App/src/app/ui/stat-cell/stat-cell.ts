import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-cell',
  template: `
    <div class="flex flex-col items-center rounded-sm border border-line bg-surface px-3 py-2">
      <span class="text-lg font-semibold tabular-nums text-ink">{{ value() }}</span>
      <span class="text-2xs text-ink-subtle">{{ label() }}</span>
    </div>
  `,
  host: { class: 'contents' },
})
export class StatCell {
  readonly value = input.required<number | string>();
  readonly label = input.required<string>();
}
