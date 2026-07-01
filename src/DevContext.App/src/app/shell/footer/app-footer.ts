import { Component, computed, inject, input } from '@angular/core';

import { ThemeService } from '../../core/theme/theme.service';
import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-footer',
  imports: [Icon],
  template: `
    <footer
      class="fixed bottom-0 z-40 flex h-7 w-full items-center justify-between border-t px-3 transition-all duration-300"
      [class.bg-surface]="!transparent()"
      [class.bg-surface/70]="transparent()"
      [class.backdrop-blur-md]="transparent()"
      [class.border-line]="!transparent()"
      [class.border-transparent]="transparent()"
    >
      <div class="flex items-center gap-3 text-2xs tabular-nums text-ink-muted">
        @if (session.ready()) {
          <span class="font-medium text-ink">{{ summaryLabel() }}</span>
          <span class="text-ink-subtle">&middot;</span>
          <span>{{ session.entryCount() }} entries</span>
          <span class="text-ink-subtle">&middot;</span>
          <span>{{ session.summary()?.nodes ?? 0 }} nodes</span>
          <span class="text-ink-subtle">&middot;</span>
          <span>{{ session.summary()?.edges ?? 0 }} edges</span>
          @if (coverage(); as cov) {
            <span class="text-ink-subtle">&middot;</span>
            <span>{{ cov }}% wired</span>
          }
        } @else {
          <span>Ready</span>
        }
      </div>

      <div class="flex items-center gap-1.5">
        <span class="text-2xs text-ink-subtle">{{ theme.vibe() }}</span>
        <button
          class="flex cursor-pointer items-center rounded p-0.5 text-ink-muted transition-colors hover:bg-surface-2 hover:text-ink"
          (click)="cycleVibe()"
          title="Cycle theme vibe"
        >
          <app-icon [name]="vibeIcon()" [size]="12" />
        </button>
      </div>
    </footer>
  `,
  host: { class: 'contents' },
})
export class AppFooter {
  readonly transparent = input(false);
  protected readonly session = inject(SessionStore);
  protected readonly theme = inject(ThemeService);

  protected readonly summaryLabel = computed(() => this.session.summary()?.label ?? '');
  protected readonly coverage = computed(() => {
    const s = this.session.summary();
    if (!s || !s.entries) return null;
    return Math.round((s.entriesWithTarget / s.entries) * 100);
  });

  protected vibeIcon = computed(() => {
    const v = this.theme.vibe();
    if (v === 'hacker') return 'zap';
    if (v === 'terminal') return 'laptop';
    return 'palette';
  });

  cycleVibe(): void {
    const vibes = this.theme.vibes();
    const idx = vibes.findIndex((v) => v.id === this.theme.vibe());
    const next = vibes[(idx + 1) % vibes.length];
    if (next) this.theme.setVibe(next.id);
  }
}
