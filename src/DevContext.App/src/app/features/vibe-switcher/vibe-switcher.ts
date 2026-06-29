import { Component, inject } from '@angular/core';

import { ThemeService } from '../../core/theme/theme.service';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-vibe-switcher',
  imports: [Icon, Button],
  template: `
    <div class="flex items-center gap-1">
      <app-button
        variant="ghost"
        size="sm"
        (click)="cycleVibe()"
      >
        <app-icon name="refresh" [size]="14" class="text-ink-subtle" />
        <span class="text-ink-muted text-[11px]">{{ theme.vibeDef().name }}</span>
      </app-button>
      @for (v of theme.vibes(); track v.id) {
        <button
          class="rounded px-1.5 py-0.5 text-[11px] transition-colors"
          [class.bg-accent]="theme.vibe() === v.id"
          [class.text-accent-ink]="theme.vibe() === v.id"
          [class.text-ink-muted]="theme.vibe() !== v.id"
          [class.hover:text-ink]="theme.vibe() !== v.id"
          (click)="theme.setVibe(v.id)"
        >
          {{ v.name }}
        </button>
      }
      @if (theme.vibeDef().themes.length > 1) {
        <app-button
          variant="ghost"
          size="sm"
          (click)="toggleTheme()"
          [title]="'Toggle ' + (theme.theme() === 'dark' ? 'light' : 'dark') + ' theme'"
        >
          <app-icon [name]="theme.theme() === 'dark' ? 'dot' : 'square'" [size]="13" class="text-ink-subtle" />
        </app-button>
      }
    </div>
  `,
})
export class VibeSwitcher {
  protected readonly theme = inject(ThemeService);

  cycleVibe(): void {
    const vibes = this.theme.vibes();
    const i = vibes.findIndex((v) => v.id === this.theme.vibe());
    const next = vibes[(i + 1) % vibes.length];
    this.theme.setVibe(next.id);
  }

  toggleTheme(): void {
    const current = this.theme.theme();
    const alt = current === 'dark' ? 'light' : 'dark';
    this.theme.setTheme(alt);
  }
}
