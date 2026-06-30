import { Component, input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  template: `<svg class="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none" [attr.stroke]="color()" stroke-width="2.5">
    <circle cx="12" cy="12" r="10" stroke-opacity="0.25" />
    <path d="M12 2a10 10 0 0 1 10 10" />
  </svg>`,
  host: { class: 'inline-flex items-center justify-center' },
})
export class Spinner {
  readonly color = input('var(--vibe-accent)');
}
