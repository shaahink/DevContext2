import { Component, input } from '@angular/core';

@Component({
  selector: 'app-panel',
  template: `
    @if (title()) {
      <div class="flex items-center gap-2 border-b border-line px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-ink-subtle">
        {{ title() }}
      </div>
    }
    <ng-content />
  `,
  host: { class: 'flex flex-col border border-line bg-surface rounded-lg overflow-hidden' },
})
export class Panel {
  readonly title = input<string>();
}
