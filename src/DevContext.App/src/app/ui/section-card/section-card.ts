import { Component, input } from '@angular/core';

@Component({
  selector: 'app-section-card',
  template: `
    <section
      [id]="id()"
      class="scroll-mt-14 py-10"
      [class.border-b]="!last()"
      [class.border-line]="!last()"
    >
      @if (title()) {
        <div class="mb-5 flex items-center gap-2">
          <h2 class="text-xs font-semibold uppercase tracking-widest text-ink-muted">{{ title() }}</h2>
          @if (subtitle()) {
            <span class="text-2xs tabular-nums text-ink-subtle">{{ subtitle() }}</span>
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
