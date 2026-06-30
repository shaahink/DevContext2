import { Component, model } from '@angular/core';
import { Icon } from '../icon/icon';

@Component({
  selector: 'app-search-field',
  imports: [Icon],
  template: `
    <div class="flex items-center gap-2 rounded-md border border-line bg-base px-2 py-1 focus-within:border-accent">
      <app-icon name="search" [size]="13" class="text-ink-subtle" />
      <input
        class="w-full bg-transparent text-xs text-ink outline-none placeholder:text-ink-subtle"
        placeholder="Search…"
        [value]="query()"
        (input)="onInput($event)"
      />
      @if (query()) {
        <button (click)="onClear()" class="flex items-center text-ink-subtle hover:text-ink">
          <app-icon name="x" [size]="12" />
        </button>
      }
    </div>
  `,
})
export class SearchField {
  readonly query = model('');

  protected onInput(e: Event): void {
    this.query.set((e.target as HTMLInputElement).value);
  }

  protected onClear(): void {
    this.query.set('');
  }
}
