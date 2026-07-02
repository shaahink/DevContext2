import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs/operators';

import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';

export interface RailItem {
  id: string;
  label: string;
  icon: string;
  route: string;
  shortKey: string;
  requiresSession: boolean;
}
const RAIL_ITEMS: RailItem[] = [
  { id: 'overview', label: 'Overview', icon: 'home', route: '/overview', shortKey: 'o', requiresSession: true },
  { id: 'entries', label: 'Entries', icon: 'list', route: '/entries', shortKey: 'e', requiresSession: true },
  { id: 'trace', label: 'Trace', icon: 'arrow-right', route: '/trace', shortKey: 't', requiresSession: true },
  { id: 'graph', label: 'Graph', icon: 'network', route: '/graph', shortKey: 'g', requiresSession: true },
  { id: 'insights', label: 'Insights', icon: 'lightbulb', route: '/insights', shortKey: 'i', requiresSession: true },
  { id: 'export', label: 'Export', icon: 'file-text', route: '/export', shortKey: 'x', requiresSession: true },
  { id: 'settings', label: 'Settings', icon: 'settings', route: '/settings', shortKey: 's', requiresSession: false },
];

@Component({
  selector: 'app-navigation-rail',
  imports: [RouterLink, RouterLinkActive, Icon],
  template: `
    <nav class="flex h-full w-14 shrink-0 flex-col border-r border-line bg-surface py-2 select-none">
      @for (item of visibleItems(); track item.id) {
        <a
          class="group flex flex-col items-center gap-0.5 pb-2.5 pt-2 text-2xs text-ink-subtle transition-colors hover:text-ink"
          [class.text-accent]="isActive(item.route)"
          [routerLink]="item.route"
          [attr.title]="item.label + ' (g ' + item.shortKey + ')'"
        >
          <app-icon [name]="item.icon" [size]="16" />
          <span class="leading-tight">{{ item.label.slice(0, 3) }}</span>
        </a>
      }
    </nav>
  `,
})
export class NavigationRail {
  private readonly session = inject(SessionStore);
  private readonly router = inject(Router);

  private readonly _currentUrl = signal('');

  constructor() {
    this._currentUrl.set(this.router.url);
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe((e) => {
      this._currentUrl.set(e.urlAfterRedirects);
    });
  }

  protected readonly visibleItems = computed(() =>
    RAIL_ITEMS.filter((item) => !item.requiresSession || this.session.ready()),
  );

  protected isActive(route: string): boolean {
    const url = this._currentUrl();
    return url === route || url.startsWith(route + '?');
  }

  protected navigate(route: string): void {
    void this.router.navigateByUrl(route);
  }

  static readonly shortcuts: Record<string, string> = Object.fromEntries(
    RAIL_ITEMS.map((i) => [i.shortKey, i.route]),
  );
}
