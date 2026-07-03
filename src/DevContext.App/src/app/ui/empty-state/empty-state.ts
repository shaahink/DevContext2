import { Component, input } from '@angular/core';

import { Icon } from '../icon/icon';

/** Standard empty/zero-data block (proposal §3.3: "Empty/loading/error triad is
 * MANDATORY per view"). `title` is the one required line; project a description or
 * action (e.g. "clear filters", "Analyze a repo first →") below it. */
@Component({
  selector: 'app-empty-state',
  imports: [Icon],
  template: `
    @if (icon()) {
      <app-icon [name]="icon()!" [size]="20" class="mb-2 text-ink-subtle" />
    }
    <p class="text-xs text-ink-subtle">{{ title() }}</p>
    <ng-content />
  `,
  host: { class: 'flex flex-1 flex-col items-center justify-center gap-1 p-6 text-center' },
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly icon = input<string | null>(null);
}
