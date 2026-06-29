import { Component, input } from '@angular/core';

@Component({
  selector: 'app-tabs',
  template: '<ng-content />',
  host: { class: 'flex border-b border-line' },
})
export class Tabs {}

@Component({
  selector: 'app-tab',
  template: '<ng-content />',
  host: {
    class:
      'px-3 py-1.5 text-xs cursor-pointer transition-colors border-b-2',
    '[class.border-accent]': 'active()',
    '[class.text-ink]': 'active()',
    '[class.font-medium]': 'active()',
    '[class.border-transparent]': '!active()',
    '[class.text-ink-muted]': '!active()',
    '[class.hover:text-ink]': '!active()',
  },
})
export class Tab {
  readonly active = input(false);
}
