import { Component, inject, input } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';

@Component({
  selector: 'app-header',
  template: `
    <header
      class="fixed top-0 z-40 flex h-12 w-full items-center gap-3 border-b px-4 transition-all duration-300"
      [class.bg-base]="!transparent()"
      [class.bg-base/70]="transparent()"
      [class.backdrop-blur-md]="transparent()"
      [class.border-line]="!transparent()"
      [class.border-transparent]="transparent()"
    >
      <a
        class="flex cursor-pointer select-none items-center gap-1.5 font-mono text-sm font-semibold tracking-tight text-ink no-underline"
        (click)="scrollTop()"
        (keydown.enter)="scrollTop()"
        tabindex="0"
        title="Back to top"
      >
        <span class="text-accent">&diams;</span>
        <span>DevContext</span>
      </a>

      <div class="mx-auto flex max-w-lg flex-1 items-center">
        <ng-content select="[analyze]" />
      </div>

      <div class="flex items-center gap-2">
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
}
