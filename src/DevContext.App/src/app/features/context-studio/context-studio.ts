import { Component, inject } from '@angular/core';

import { SessionStore } from '../../state/session.store';

@Component({
  selector: 'app-context-studio',
  template: `
    <div class="flex h-full min-h-0">
      <!-- Scope Picker (left) -->
      <div class="w-56 shrink-0 border-r border-line bg-surface p-3">
        <h2 class="mb-2 text-xs font-semibold uppercase tracking-wider text-ink-muted">Scope</h2>
        @if (session.ready()) {
          <p class="text-2xs text-ink-subtle">Select entries, types, or flows to include.</p>
        } @else {
          <p class="text-2xs text-ink-subtle">Analyze a repo to start building context.</p>
        }
      </div>

      <!-- Composition (center) -->
      <div class="flex min-w-0 flex-1 flex-col bg-base p-3">
        <h2 class="mb-2 text-xs font-semibold uppercase tracking-wider text-ink-muted">Composition</h2>
        <div class="flex flex-1 flex-col items-center justify-center gap-2 text-xs text-ink-subtle">
          <p>No cards yet. Pick items from the scope picker to build context.</p>
        </div>
      </div>

      <!-- Budget Panel (right) -->
      <div class="w-48 shrink-0 border-l border-line bg-surface p-3">
        <h2 class="mb-2 text-xs font-semibold uppercase tracking-wider text-ink-muted">Budget</h2>
        <p class="text-2xs text-ink-subtle">Token meter and export controls will appear here.</p>
      </div>
    </div>
  `,
  host: { class: 'h-full min-h-0' },
})
export class ContextStudio {
  protected readonly session = inject(SessionStore);
}
