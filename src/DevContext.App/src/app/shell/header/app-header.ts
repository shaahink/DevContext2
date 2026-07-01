import { Component, inject, input } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';

@Component({
  selector: 'app-header',
  template: `
    <header
      class="fixed top-0 z-40 flex h-12 w-full items-center gap-3 border-b px-4 transition-all duration-300 select-none"
      data-tauri-drag-region
      [class.bg-base]="!transparent()"
      [class.bg-base/70]="transparent()"
      [class.backdrop-blur-md]="transparent()"
      [class.border-line]="!transparent()"
      [class.border-transparent]="transparent()"
    >
      <button
        class="flex cursor-pointer items-center gap-1.5 rounded font-mono text-sm font-semibold tracking-tight text-ink hover:text-accent transition-colors"
        (click)="scrollTop()"
        title="Back to top"
      >
        <span class="text-accent">&diams;</span>
        <span>DevContext</span>
      </button>

      <div class="mx-auto flex max-w-lg flex-1 items-center pointer-events-auto">
        <ng-content select="[analyze]" />
      </div>

      <div class="flex items-center gap-3 pointer-events-auto">
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

        <div class="flex items-center gap-0.5">
          <button class="rounded px-1.5 py-1 text-xs text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="minimize()" title="Minimize">━</button>
          <button class="rounded px-1.5 py-1 text-xs text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="toggleMaximize()" title="Maximize">☐</button>
          <button class="rounded px-1.5 py-1 text-xs text-ink-muted hover:bg-danger hover:text-ink" (click)="close()" title="Close">✕</button>
        </div>
      </div>
    </header>
  `,
  host: { class: 'contents' },
})
export class AppHeader {
  readonly transparent = input(false);
  protected readonly connection = inject(ConnectionStore);

  scrollTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  protected async minimize(): Promise<void> {
    try {
      const { getCurrentWindow } = await import('@tauri-apps/api/window');
      await getCurrentWindow().minimize();
    } catch { /* not in Tauri */ }
  }

  protected async toggleMaximize(): Promise<void> {
    try {
      const { getCurrentWindow } = await import('@tauri-apps/api/window');
      const win = getCurrentWindow();
      const maximized = await win.isMaximized();
      if (maximized) await win.unmaximize();
      else await win.maximize();
    } catch { /* not in Tauri */ }
  }

  protected async close(): Promise<void> {
    try {
      const { getCurrentWindow } = await import('@tauri-apps/api/window');
      await getCurrentWindow().close();
    } catch { /* not in Tauri */ }
  }
}
