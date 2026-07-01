import { Component, inject } from '@angular/core';

import { ThemeService } from '../../core/theme/theme.service';
import { RecentStore } from '../../state/recent.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-section-settings',
  imports: [SectionCard, Icon],
  template: `
    <app-section-card id="settings" title="Settings">
      <div class="grid gap-6 md:grid-cols-2">
        <div class="space-y-2">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Theme</h3>
          <div class="grid grid-cols-2 gap-1.5">
            @for (vibe of theme.vibes(); track vibe.id) {
              <button
                class="flex cursor-pointer items-center gap-2 rounded border px-3 py-2 text-left text-xs transition-colors"
                [class.border-accent]="theme.vibe() === vibe.id"
                [class.bg-accent/10]="theme.vibe() === vibe.id"
                [class.border-line]="theme.vibe() !== vibe.id"
                [class.hover:border-line-strong]="theme.vibe() !== vibe.id"
                (click)="theme.setVibe(vibe.id)"
              >
                <span class="font-medium text-ink">{{ vibe.name }}</span>
                <span class="ml-auto text-2xs text-ink-subtle">{{ theme.vibe() === vibe.id ? 'Active' : '' }}</span>
              </button>
            }
          </div>
          @if (currentVibe(); as v) {
            <div class="flex flex-wrap gap-1.5">
              @for (t of v.themes; track t) {
                <button
                  class="flex cursor-pointer items-center gap-1 rounded border px-2 py-1 text-2xs transition-colors"
                  [class.border-accent]="theme.theme() === t"
                  [class.bg-accent/10]="theme.theme() === t"
                  [class.border-line]="theme.theme() !== t"
                  [class.hover:border-line-strong]="theme.theme() !== t"
                  (click)="theme.setTheme(t)"
                >
                  <app-icon [name]="t === 'dark' ? 'moon' : t === 'light' ? 'sun' : 'laptop'" [size]="11" />
                  {{ t }}
                </button>
              }
            </div>
          }
        </div>

        <div class="space-y-2">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Recents</h3>
          @if (recents().length) {
            <div class="space-y-1">
              @for (r of recents(); track r.path) {
                <div class="flex items-center gap-2 rounded border border-line bg-surface px-2 py-1.5">
                  <div class="min-w-0 flex-1">
                    <p class="truncate font-mono text-xs text-ink">{{ r.label }}</p>
                    <p class="truncate text-3xs text-ink-subtle">{{ r.path }}</p>
                  </div>
                  <button
                    class="shrink-0 rounded p-0.5 text-ink-muted hover:bg-surface-2 hover:text-ink"
                    (click)="recentStore.remove(r.path)"
                    title="Remove"
                  >
                    <app-icon name="x" [size]="12" />
                  </button>
                </div>
              }
            </div>
          } @else {
            <p class="text-xs text-ink-muted">No recent repos.</p>
          }
        </div>

        <div class="space-y-2 md:col-span-2">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">About</h3>
          <div class="rounded border border-line bg-surface p-3">
            <div class="flex items-start gap-3">
              <span class="text-lg text-accent">&diams;</span>
              <div>
                <p class="text-sm font-semibold text-ink">DevContext</p>
                <p class="text-xs text-ink-muted">The devtool lens for any .NET repository. Instant architecture understanding.</p>
                <p class="mt-1 text-2xs text-ink-subtle">
                  Built with Angular + Tauri. Engine via gRPC.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </app-section-card>
  `,
})
export class SectionSettings {
  protected readonly theme = inject(ThemeService);
  protected readonly recentStore = inject(RecentStore);
  protected readonly recents = this.recentStore.recents;

  protected readonly currentVibe = this.theme.vibeDef;
}
