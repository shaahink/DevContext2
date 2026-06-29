import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-sheet',
  template: `
    @if (open()) {
      <div class="fixed inset-0 z-40" role="dialog" aria-modal="true" (click)="closed.emit()" (keydown.escape)="closed.emit()" tabindex="-1">
        <div class="absolute inset-0 bg-base/60"></div>
        <div class="absolute right-0 top-0 h-full w-80 border-l border-line bg-surface shadow-xl" (click)="$event.stopPropagation()" (keydown)="$event.stopPropagation()" tabindex="-1">
          <ng-content />
        </div>
      </div>
    }
  `,
  host: { class: 'contents' },
})
export class Sheet {
  readonly open = input(false);
  readonly closed = output<void>();
}
