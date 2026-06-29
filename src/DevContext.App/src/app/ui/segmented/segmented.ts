import { Component, input, model } from '@angular/core';

@Component({
  selector: 'app-segmented',
  template: `
    <div class="flex items-center overflow-hidden rounded-md border border-line">
      @for (opt of options(); track opt.value) {
        <button
          (click)="selected.set(opt.value)"
          class="px-2 py-1 text-xs capitalize transition-colors"
          [class.bg-accent]="selected() === opt.value"
          [class.text-accent-ink]="selected() === opt.value"
          [class.text-ink-muted]="selected() !== opt.value"
          [class.hover:text-ink]="selected() !== opt.value"
        >
          {{ opt.label }}
        </button>
      }
    </div>
  `,
  host: { class: 'inline-flex' },
})
export class Segmented {
  readonly options = input.required<readonly { label: string; value: string }[]>();
  readonly selected = model<string>('');
}
