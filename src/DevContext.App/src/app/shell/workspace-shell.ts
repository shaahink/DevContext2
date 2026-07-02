import { Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

import { ConnectionStore } from '../state/connection.store';
import { SessionStore } from '../state/session.store';
import { ThemeService } from '../core/theme/theme.service';
import { AppHeader } from './header/app-header';
import { AppFooter } from './footer/app-footer';
import { NavigationRail } from './navigation-rail';
import { Icon } from '../ui/icon/icon';
import { Palette } from '../features/palette/palette';

const VIEW_SHORTCUTS: Record<string, string> = {
  o: '/overview',
  e: '/entries',
  t: '/trace',
  g: '/graph',
  i: '/insights',
  x: '/export',
  s: '/settings',
};

const SHORTCUT_HELP = [
  { keys: 'g o', desc: 'Go to Overview' },
  { keys: 'g e', desc: 'Go to Entries' },
  { keys: 'g t', desc: 'Go to Trace' },
  { keys: 'g g', desc: 'Go to Graph' },
  { keys: 'g i', desc: 'Go to Insights' },
  { keys: 'g x', desc: 'Go to Export' },
  { keys: 'g s', desc: 'Go to Settings' },
  { keys: 'Ctrl+K', desc: 'Command palette' },
  { keys: 'Escape', desc: 'Close modal / palette' },
  { keys: '?', desc: 'Show this help' },
];

@Component({
  selector: 'app-workspace-shell',
  imports: [RouterOutlet, AppHeader, AppFooter, NavigationRail, Icon, Palette],
  template: `
    <app-header />
    <div class="flex flex-1 overflow-hidden" style="height: calc(100vh - 2.75rem - 1.75rem); margin-top: 2.75rem;">
      <app-navigation-rail />
      <main class="flex-1 overflow-y-auto" #main>
        <router-outlet />
      </main>
    </div>
    <app-footer />
    <app-palette />

    @if (helpOpen()) {
      <div class="fixed inset-0 z-[60] flex items-center justify-center" (click)="helpOpen.set(false)" (keydown.escape)="helpOpen.set(false)" role="dialog" tabindex="0">
        <div class="absolute inset-0 bg-base/80 backdrop-blur-sm"></div>
        <div class="relative w-[420px] max-h-[70vh] overflow-y-auto rounded-lg border border-line bg-elevated shadow-2xl" (click)="$event.stopPropagation()" (keydown)="$event.stopPropagation()" tabindex="-1">
          <div class="flex items-center justify-between border-b border-line px-4 py-3">
            <h2 class="text-sm font-semibold text-ink">Keyboard Shortcuts</h2>
            <button class="text-ink-muted hover:text-ink text-xs px-1" (click)="helpOpen.set(false)" (keydown.enter)="helpOpen.set(false)" (keydown.space)="helpOpen.set(false); $event.preventDefault()">✕</button>
          </div>
          <div class="p-4 space-y-2">
            @for (s of helpItems; track s.keys) {
              <div class="flex items-center justify-between">
                <span class="text-xs text-ink">{{ s.desc }}</span>
                <kbd class="rounded border border-line bg-surface-2 px-2 py-0.5 font-mono text-2xs text-ink-muted">{{ s.keys }}</kbd>
              </div>
            }
          </div>
        </div>
      </div>
    }

    @if (gPending()) {
      <div class="fixed bottom-12 left-1/2 z-50 -translate-x-1/2 rounded border border-line bg-surface px-3 py-1.5 font-mono text-xs text-ink-muted shadow-lg">
        Press a key to navigate (<kbd class="text-accent">?</kbd> for help)
      </div>
    }
  `,
  host: { class: 'flex flex-col h-screen' },
})
export class WorkspaceShell {
  private readonly router = inject(Router);
  protected readonly helpItems = SHORTCUT_HELP;
  protected readonly helpOpen = signal(false);

  private gTimer: ReturnType<typeof setTimeout> | null = null;
  protected readonly gPending = signal(false);

  constructor() {
    inject(ConnectionStore).start();
    inject(ThemeService);
    inject(SessionStore);
  }

  @HostListener('window:keydown', ['$event'])
  onKeydown(e: KeyboardEvent): void {
    if (e.ctrlKey || e.metaKey || e.altKey) return;
    const active = document.activeElement;
    if (active?.tagName === 'INPUT' || active?.tagName === 'TEXTAREA') return;

    if (e.key === '?') {
      e.preventDefault();
      this.helpOpen.update((v) => !v);
      return;
    }

    if (e.key === 'Escape') {
      this.helpOpen.set(false);
      return;
    }

    if (e.key === 'g') {
      e.preventDefault();
      this.gPending.set(true);
      if (this.gTimer) clearTimeout(this.gTimer);
      this.gTimer = setTimeout(() => this.gPending.set(false), 1500);
      return;
    }

    if (this.gPending()) {
      const route = VIEW_SHORTCUTS[e.key];
      if (route) {
        e.preventDefault();
        void this.router.navigateByUrl(route);
      }
      this.gPending.set(false);
      if (this.gTimer) clearTimeout(this.gTimer);
    }
  }
}
