import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

import { TickerService } from '../../core/ticker.service';
import { Icon } from '../icon/icon';

/** StatusBar insight ticker (proposal §6) — presentational shell around `TickerService`.
 * Sources (`workspace-shell.ts`'s constructor: analysis facts, insights, AtlasStore
 * discoveries, static tips) call `post()`; this renders `current()` and nothing when
 * the pool is empty (e.g. no session yet). */
@Component({
  selector: 'app-ticker',
  imports: [Icon],
  template: `
    @if (ticker.current(); as item) {
      <button
        type="button"
        class="flex min-w-0 items-center gap-1 text-2xs text-ink-muted transition-colors hover:text-ink"
        (mouseenter)="ticker.pause()"
        (mouseleave)="ticker.resume()"
        (click)="onClick(item.link)"
        [title]="item.text"
      >
        @if (item.icon) {
          <app-icon [name]="item.icon" [size]="10" class="shrink-0" />
        }
        <span class="truncate">{{ item.text }}</span>
      </button>
    }
  `,
  host: { class: 'contents' },
})
export class Ticker {
  protected readonly ticker = inject(TickerService);
  private readonly router = inject(Router);

  protected onClick(link: string | undefined): void {
    if (link) void this.router.navigateByUrl(link);
  }
}
