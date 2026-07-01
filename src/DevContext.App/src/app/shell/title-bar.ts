import { Component, inject } from '@angular/core';
import { ConnectionStore } from '../state/connection.store';
import { ThemeService } from '../core/theme/theme.service';

@Component({
  selector: 'app-title-bar',
  template: `
    <div class="flex h-full items-center justify-between border-b border-line bg-surface px-3" data-tauri-drag-region>
      <span class="text-xs font-semibold text-ink-muted">DevContext</span>
      <div class="flex items-center gap-2">
        <button
          class="cursor-pointer select-none rounded px-2 py-0.5 font-mono text-2xs uppercase tracking-wider text-ink-subtle transition-colors hover:bg-surface-2 hover:text-ink"
          (click)="cycleVibe()"
          [title]="'Vibe: ' + theme.vibeDef().name + ' — click to cycle'"
        >
          {{ theme.vibeDef().name.toUpperCase() }}
        </button>
        @if (connection.online()) {
          <span class="flex items-center gap-1 text-2xs text-success">
            <span class="h-1.5 w-1.5 rounded-full bg-success"></span>
            Connected
          </span>
        } @else {
          <span class="flex items-center gap-1 text-2xs text-danger">
            <span class="h-1.5 w-1.5 rounded-full bg-danger"></span>
            Offline
          </span>
        }
      </div>
    </div>
  `,
  host: { class: 'col-span-3' },
})
export class TitleBar {
  protected readonly connection = inject(ConnectionStore);
  protected readonly theme = inject(ThemeService);

  protected cycleVibe(): void {
    const vibes = this.theme.vibes();
    const idx = vibes.findIndex((v) => v.id === this.theme.vibe());
    const next = vibes[(idx + 1) % vibes.length];
    this.theme.setVibe(next.id);
  }
}
