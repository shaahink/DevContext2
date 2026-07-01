import { Component, input } from '@angular/core';

@Component({
  selector: 'app-badge',
  template: '<ng-content />',
  host: {
    class: 'inline-flex items-center rounded px-1.5 py-0.5 text-2xs font-medium',
    '[class.bg-surface-2]': 'variant() === "default"',
    '[class.text-ink-muted]': 'variant() === "default"',
    '[class.bg-accent]': 'variant() === "accent"',
    '[class.text-accent-ink]': 'variant() === "accent" || variant() === "success" || variant() === "danger"',
    '[class.bg-success]': 'variant() === "success"',
    '[class.bg-warn]': 'variant() === "warn"',
    '[class.text-base]': 'variant() === "warn"',
    '[class.bg-danger]': 'variant() === "danger"',
  },
})
export class Badge {
  readonly variant = input<'default' | 'accent' | 'success' | 'warn' | 'danger'>('default');
}
