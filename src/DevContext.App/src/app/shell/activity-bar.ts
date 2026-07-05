import { Component, DestroyRef, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs/operators';

import { SessionStore } from '../state/session.store';
import { Icon } from '../ui/icon/icon';

export interface RailItem {
  id: string;
  label: string;
  icon: string;
  route: string;
  shortKey: string;
  requiresSession: boolean;
  badge?: () => number;
}

@Component({
  selector: 'app-activity-bar',
  imports: [RouterLink, Icon],
  template: `
    <nav class="flex h-full w-12 shrink-0 flex-col border-r border-line bg-surface py-2 select-none">
      @for (item of railItems; track item.id) {
        <a
          class="group relative flex flex-col items-center justify-center py-2.5 text-2xs transition-colors"
          [class.text-accent]="isActive(item.route)"
          [class.text-ink-subtle]="!isActive(item.route) && enabled(item)"
          [class.opacity-40]="!enabled(item)"
          [class.pointer-events-none]="!enabled(item)"
          [class.hover:text-ink]="enabled(item)"
          [routerLink]="enabled(item) ? item.route : null"
          [attr.title]="enabled(item) ? item.label + ' (g ' + item.shortKey + ')' : 'Analyze a repo first'"
        >
          @if (isActive(item.route)) {
            <span class="absolute left-0 top-1 bottom-1 w-[3px] rounded-r bg-accent"></span>
          }
          <span class="relative">
            <app-icon [name]="item.icon" [size]="18" />
            @if (item.badge && item.badge(); as count) {
              <span class="absolute -right-2 -top-1.5 flex h-3.5 min-w-3.5 items-center justify-center rounded-full bg-accent px-0.5 text-[9px] font-medium leading-none text-accent-ink tabular-nums">
                {{ count > 99 ? '99+' : count }}
              </span>
            }
          </span>
        </a>
      }
    </nav>
  `,
})
export class ActivityBar {
  private readonly session = inject(SessionStore);
  private readonly router = inject(Router);

  private readonly _currentUrl = signal('');

  /** Proposal §8.1's 5-icon rail: map(Home) layers(Explore) boxes(Atlas) info(Insights) settings. */
  protected readonly railItems: readonly RailItem[] = [
    { id: 'home', label: 'Home', icon: 'map', route: '/', shortKey: 'h', requiresSession: false },
    { id: 'explore', label: 'Explore', icon: 'layers', route: '/explore', shortKey: 'e', requiresSession: true, badge: () => this.session.entryCount() },
    { id: 'atlas', label: 'Atlas', icon: 'boxes', route: '/atlas', shortKey: 'a', requiresSession: true },
    { id: 'insights', label: 'Insights', icon: 'zap', route: '/insights', shortKey: 'i', requiresSession: true, badge: () => this.session.insightCount() },
    { id: 'settings', label: 'Settings', icon: 'settings', route: '/settings', shortKey: 's', requiresSession: false },
  ];

  constructor() {
    this._currentUrl.set(this.router.url);
    const sub = this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe((e) => {
      this._currentUrl.set(e.urlAfterRedirects);
    });
    inject(DestroyRef).onDestroy(() => sub.unsubscribe());
  }

  protected enabled(item: RailItem): boolean {
    return !item.requiresSession || this.session.ready();
  }

  protected isActive(route: string): boolean {
    const url = this._currentUrl();
    return url === route || url.startsWith(route + '?');
  }
}
