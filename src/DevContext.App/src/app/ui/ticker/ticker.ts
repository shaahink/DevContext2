import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

import { TickerService } from '../../core/ticker.service';
import { Icon } from '../icon/icon';

/** StatusBar insight ticker (proposal §6) — presentational shell around `TickerService`.
 * The service is inert (empty `current()`) until something calls `post()`; sources get
 * wired in W5 (SessionStore analysis events, insights, AtlasStore). Until then this
 * renders nothing, which is the correct "static placeholder" state for W1. */
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
