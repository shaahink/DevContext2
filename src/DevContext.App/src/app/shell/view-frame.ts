import { Component, input } from '@angular/core';

@Component({
  selector: 'app-view-frame',
  template: `
    <div class="flex h-full">
      @if (showSidebar()) {
        <aside class="w-56 shrink-0 overflow-auto border-r border-line bg-surface">
          <ng-content select="[sidebar]" />
        </aside>
      }
      <div class="flex min-w-0 flex-1 flex-col">
        @if (showHeader()) {
          <header class="shrink-0">
            <ng-content select="[header]" />
          </header>
        }
        <div class="min-h-0 flex-1 overflow-auto">
          <ng-content />
        </div>
      </div>
    </div>
  `,
})
export class ViewFrame {
  readonly showSidebar = input(true);
  readonly showHeader = input(true);
}
