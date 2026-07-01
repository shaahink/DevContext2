import { Component, input } from '@angular/core';

@Component({
  selector: 'app-section-card',
  template: `
    <section
      [id]="id()"
      class="scroll-mt-14 py-8"
      [class.border-b]="!last()"
      [class.border-line/50]="!last()"
    >
      @if (title()) {
        <div class="mb-4 flex items-center gap-2">
          <h2 class="text-sm font-semibold uppercase tracking-wide text-ink-muted">{{ title() }}</h2>
          @if (subtitle()) {
            <span class="text-xs tabular-nums text-ink-subtle">{{ subtitle() }}</span>
          }
        </div>
      }
      <ng-content />
    </section>
  `,
  host: { class: 'block' },
})
export class SectionCard {
  readonly id = input.required<string>();
  readonly title = input<string>();
  readonly subtitle = input<string>();
  readonly last = input(false);
}
