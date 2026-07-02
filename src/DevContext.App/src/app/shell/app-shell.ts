import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { ConnectionStore } from '../state/connection.store';
import { SessionStore } from '../state/session.store';
import { ThemeService } from '../core/theme/theme.service';
import { Icon } from '../ui/icon/icon';
import { Toast, ToastService } from '../ui/toast/toast';
import { StatusBar } from './status-bar';
import { TitleBar } from './title-bar';
import { NodeCard } from '../features/node-card/node-card';
import { Palette } from '../features/palette/palette';

interface NavItem {
  route: string;
  icon: string;
  label: string;
  gateSession: boolean;
}

const TOP_ITEMS: readonly NavItem[] = [
  { route: '/', icon: 'folder-open', label: 'Source', gateSession: false },
  { route: '/cache', icon: 'boxes', label: 'Cache', gateSession: false },
];

const LENS_ITEMS: readonly NavItem[] = [
  { route: '/overview', icon: 'map', label: 'Overview', gateSession: true },
  { route: '/entries', icon: 'webhook', label: 'Entries', gateSession: true },
  { route: '/graph', icon: 'share-2', label: 'Graph', gateSession: true },
  { route: '/insights', icon: 'lightbulb', label: 'Insights', gateSession: true },
  { route: '/browse', icon: 'search', label: 'Browse', gateSession: true },
  { route: '/trace', icon: 'network', label: 'Trace', gateSession: true },
  { route: '/document', icon: 'code', label: 'Document', gateSession: true },
  { route: '/stats', icon: 'database', label: 'Stats', gateSession: true },
];

@Component({
  selector: 'app-app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon, TitleBar, StatusBar, Toast, NodeCard, Palette],
  template: `
    <div class="grid h-screen overflow-hidden" style="grid-template-rows: 36px 1fr 24px; grid-template-columns: 48px 1fr;">
      <app-title-bar />

      <nav class="row-start-2 flex flex-col border-r border-line bg-surface py-2">
        <div class="flex flex-col items-center gap-1">
          @for (item of topItems; track item.route) {
            <a
              [routerLink]="item.route"
              routerLinkActive="bg-accent/15 text-accent"
              [routerLinkActiveOptions]="{ exact: item.route === '/' }"
              class="flex flex-col items-center gap-0.5 rounded-md px-1 py-1.5 text-2xs text-ink-subtle transition-colors hover:text-ink"
              [title]="item.label"
            >
              <app-icon [name]="item.icon" [size]="18" />
              <span class="leading-none">{{ item.label }}</span>
            </a>
          }
        </div>

        <div class="mx-3 my-2 border-t border-line"></div>

        <div class="flex flex-col items-center gap-1">
          @for (item of lensItems; track item.route) {
            <a
              [routerLink]="session.ready() ? item.route : null"
              routerLinkActive="bg-accent/15 text-accent"
              [routerLinkActiveOptions]="{ exact: false }"
              class="flex flex-col items-center gap-0.5 rounded-md px-1 py-1.5 text-2xs transition-colors"
              [class.text-ink-subtle]="!session.ready()"
              [class.opacity-40]="!session.ready()"
              [class.cursor-not-allowed]="!session.ready()"
              [class.hover:text-ink]="session.ready()"
              [attr.aria-disabled]="!session.ready() ? true : null"
              [title]="session.ready() ? item.label : item.label + ' — Analyze a repo first'"
            >
              <app-icon [name]="item.icon" [size]="18" />
              <span class="leading-none flex items-center gap-1">
                {{ item.label }}
                @if (item.route === '/insights' && session.insightCount()) {
                  <span class="inline-flex items-center justify-center rounded-full bg-accent px-1 text-3xs leading-none text-ink">{{ session.insightCount() }}</span>
                }
              </span>
            </a>
          }
        </div>
      </nav>

      <main class="row-start-2 col-start-2 min-w-0 overflow-auto">
        <!-- I4.5 Honesty ribbon -->
        @if (session.ready() && session.summary(); as s) {
          <div class="flex items-center gap-3 border-b border-line bg-surface px-3 py-1.5 text-xs text-ink-muted">
            <span class="font-semibold text-ink">{{ s.label?.split(' ')[0] ?? '—' }}</span>
            @if (s.archetype) { <span>·</span><span>{{ s.archetype }}</span> }
            <span>·</span>
            <span>{{ s.projects }} projects</span>
            @if (s.entries) { <span>·</span><span>{{ s.entries }} entries</span> }
            @if (s.entriesWithTarget !== null && s.entries) {
              <span>·</span>
              <span [class.text-success]="s.entriesWithTarget / s.entries >= 0.5"
                    [class.text-warn]="s.entriesWithTarget / s.entries < 0.5">
                {{ s.entriesWithTarget }}/{{ s.entries }} targets
              </span>
            }
          </div>
        }
        <router-outlet />
      </main>

      <app-status-bar />
      <app-toast [messages]="toast.messages()" />
      <app-node-card />
      <app-palette />
    </div>
  `,
})
export class AppShell {
  protected readonly session = inject(SessionStore);
  protected readonly toast = inject(ToastService);
  protected readonly topItems = TOP_ITEMS;
  protected readonly lensItems = LENS_ITEMS;

  constructor() {
    inject(ConnectionStore).start();
    inject(ThemeService); // instantiate to set data-vibe/data-theme on <html>
  }
}
