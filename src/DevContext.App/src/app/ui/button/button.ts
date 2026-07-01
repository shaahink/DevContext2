import { Component, input } from '@angular/core';

@Component({
  selector: 'app-button',
  template: '<ng-content />',
  host: {
    class:
      'inline-flex cursor-pointer select-none items-center justify-center gap-1.5 rounded-md text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent',
    // `app-button` is a custom element, so the Tailwind `disabled:` variant
    // (which keys off the `:disabled` pseudo-class) never matches. Bind the
    // disabled appearance + interaction-block directly off the signal instead.
    '[class.pointer-events-none]': 'disabled()',
    '[class.opacity-40]': 'disabled()',
    '[class.bg-accent]': 'variant() === "primary"',
    '[class.text-accent-ink]': 'variant() === "primary"',
    '[class.hover:brightness-110]': 'variant() === "primary"',
    '[class.bg-surface-2]': 'variant() === "secondary"',
    '[class.text-ink]': 'variant() === "secondary"',
    '[class.hover:bg-elevated]': 'variant() === "secondary"',
    '[class.text-ink-muted]': 'variant() === "ghost" || variant() === "danger"',
    '[class.hover:bg-surface-2]': 'variant() === "ghost" || variant() === "danger"',
    '[class.hover:text-ink]': 'variant() !== "primary"',
    '[class.px-2]': 'size() === "sm"',
    '[class.py-1]': 'size() === "sm"',
    '[class.text-xs]': 'size() === "sm"',
    '[class.px-3]': 'size() === "md"',
    '[class.py-1.5]': 'size() === "md"',
    '[class.px-4]': 'size() === "lg"',
    '[class.py-2]': 'size() === "lg"',
    role: 'button',
    '[attr.tabindex]': 'disabled() ? -1 : 0',
    '[attr.aria-disabled]': 'disabled() ? true : null',
  },
})
export class Button {
  readonly variant = input<'primary' | 'secondary' | 'ghost' | 'danger'>('secondary');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly disabled = input(false);
}
