import { Component, inject, input } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';
import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-header',
  imports: [Icon],
  template: `
    <header
      class="fixed top-0 z-40 flex h-11 w-full items-center border-b border-line/50 px-3 transition-all duration-300 select-none shadow-sm"
      [class.bg-base]="!transparent()"
      [class.bg-base/80]="transparent()"
      [class.backdrop-blur-lg]="transparent()"
      [class.shadow-sm]="!transparent()"
      [class.shadow-none]="transparent()"
    >
      <div class="flex items-center gap-3" data-tauri-drag-region style="flex:1; height:100%; display:flex; align-items:center;">
        <span
          class="flex cursor-pointer items-center gap-1.5 rounded font-mono text-sm font-semibold tracking-tight text-ink no-drag"
          (click)="scrollTop()"
          (keydown.enter)="scrollTop()"
          tabindex="0"
          role="button"
        >
          <span class="text-accent">&diams;</span>
          <span>DevContext</span>
        </span>
      </div>

      <div class="flex flex-1 items-center justify-center pointer-events-auto">
        <ng-content select="[analyze]" />
      </div>

      <div class="flex items-center gap-2 pointer-events-auto">
        @if (session.ready()) {
          <button
            class="flex cursor-pointer items-center gap-1 rounded px-2 py-1 text-2xs text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors"
            (click)="resetWorkspace()"
            title="New analysis"
          >
            <app-icon name="play" [size]="11" /> New
          </button>
        }
        <span
          class="flex items-center gap-1.5 text-2xs text-ink-muted"
          [class.text-success]="connection.online()"
          [class.text-danger]="connection.checked() && !connection.online()"
          [class.text-ink-muted]="!connection.checked()"
        >
          <span
            class="inline-block h-1.5 w-1.5 rounded-full"
            [class.bg-success]="connection.online()"
            [class.bg-danger]="connection.checked() && !connection.online()"
            [class.bg-ink-muted]="!connection.checked()"
          ></span>
          {{ connection.checked() ? (connection.online() ? 'Connected' : 'Offline') : '...' }}
        </span>

        <div class="flex items-center -mr-1">
          <button class="flex h-7 w-9 cursor-pointer items-center justify-center rounded text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors" (click)="minimize()" title="Minimize">
            <svg width="10" height="1" viewBox="0 0 10 1"><rect width="10" height="1" fill="currentColor"/></svg>
          </button>
          <button class="flex h-7 w-9 cursor-pointer items-center justify-center rounded text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors" (click)="toggleMaximize()" title="Maximize">
            <svg width="10" height="10" viewBox="0 0 10 10"><rect x="1" y="1" width="8" height="8" fill="none" stroke="currentColor" stroke-width="1.2"/></svg>
          </button>
          <button class="flex h-7 w-9 cursor-pointer items-center justify-center rounded text-ink-muted hover:bg-danger hover:text-ink transition-colors" (click)="close()" title="Close">
            <svg width="10" height="10" viewBox="0 0 10 10"><line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.2"/><line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.2"/></svg>
          </button>
        </div>
      </div>
    </header>
  `,
  host: { class: 'contents' },
})
export class AppHeader {
  readonly transparent = input(false);
  protected readonly connection = inject(ConnectionStore);
  protected readonly session = inject(SessionStore);

  scrollTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetWorkspace(): void {
    this.session.cancel();
    window.location.reload();
  }

  protected async minimize(): Promise<void> {
    try {
      const m = await import('@tauri-apps/api/window');
      await m.getCurrentWindow().minimize();
    } catch { /* not in Tauri */ }
  }

  protected async toggleMaximize(): Promise<void> {
    try {
      const m = await import('@tauri-apps/api/window');
      const win = m.getCurrentWindow();
      if (await win.isMaximized()) await win.unmaximize();
      else await win.maximize();
    } catch { /* not in Tauri */ }
  }

  protected async close(): Promise<void> {
    try {
      const m = await import('@tauri-apps/api/window');
      await m.getCurrentWindow().close();
    } catch { /* not in Tauri */ }
  }
}
