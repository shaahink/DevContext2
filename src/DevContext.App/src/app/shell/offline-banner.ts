import { Component, inject } from '@angular/core';

import { ConnectionStore } from '../state/connection.store';

/**
 * Offline banner (proposal §5.4) — a 24px inline banner under the tab strip when the
 * engine is unreachable. `ConnectionStore` already 5s-polls `Ping`; this just renders
 * its state. Auto-clears on reconnect since it's driven by the same signal, not a
 * one-shot dismiss.
 */
@Component({
  selector: 'app-offline-banner',
  imports: [],
  template: `
    @if (connection.checked() && !connection.online()) {
      <div class="flex h-6 w-full shrink-0 items-center justify-center gap-1.5 border-b border-line bg-danger/10 px-2 text-2xs text-danger select-none">
        <span class="inline-block h-1.5 w-1.5 rounded-full bg-danger"></span>
        Engine offline — retrying…
      </div>
    }
  `,
  host: { class: 'contents' },
})
export class OfflineBanner {
  protected readonly connection = inject(ConnectionStore);
}
