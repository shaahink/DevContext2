import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';

import { ThemeService } from '../../core/theme/theme.service';
import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { ConnectionStore } from '../../state/connection.store';
import { ActivityService } from '../../core/activity/activity.service';
import { Icon } from '../../ui/icon/icon';
import { Ticker } from '../../ui/ticker/ticker';

/**
 * StatusBar (proposal §6, 22px) — renamed/restyled from the old fixed-position 28px
 * app-footer, and moved into normal document flow as the shell's last grid row instead
 * of `fixed bottom-0` (the whole point of the W1 regrid: no more manual
 * `calc(100vh - ...)` height math in the middle region).
 *
 * Segments left→right: Context (repo summary, click → Home) · Task (busy state) ·
 * Ticker (statusbar insight ticker, sources wired in `workspace-shell.ts`'s constructor —
 * analysis facts, engine insights, Flow Atlas discoveries, keyboard tips) · Connection ·
 * Vibe cycler.
 */
@Component({
  selector: 'app-statusbar',
  imports: [Icon, Ticker],
  template: `
    <footer class="flex h-[22px] w-full shrink-0 items-center justify-between border-t border-line bg-base px-2 select-none">
      <div class="flex min-w-0 items-center gap-3 text-2xs tabular-nums text-ink-muted">
        <button type="button" class="flex items-center gap-3 transition-colors hover:text-ink" (click)="goHome()">
          @if (session.busy()) {
            <span class="text-accent">{{ activityLabel() }}</span>
            @if (activity.percent() > 0) {
              <span class="tabular-nums">{{ activity.percent() }}%</span>
            }
          } @else if (session.ready()) {
            <span class="font-medium text-ink">{{ summaryLabel() }}</span>
            @if (atlas.running()) {
              <span class="text-ink-subtle">&middot;</span>
              <span>{{ atlas.progressLabel() }}</span>
            }
          } @else if (session.status() === 'error') {
            <span class="text-danger">Analysis failed</span>
          } @else {
            <span>Ready</span>
          }
        </button>

        <app-ticker />
      </div>

      <div class="flex shrink-0 items-center gap-2">
        <span
          class="flex items-center gap-1 text-2xs"
          [class.text-success]="connection.online()"
          [class.text-danger]="connection.checked() && !connection.online()"
          [class.text-ink-subtle]="!connection.checked()"
          [title]="connection.online() ? 'Server v' + (connection.version() || '') : 'Server offline'"
        >
          <span
            class="inline-block h-1.5 w-1.5 rounded-full"
            [class.bg-success]="connection.online()"
            [class.bg-danger]="connection.checked() && !connection.online()"
            [class.bg-ink-subtle]="!connection.checked()"
          ></span>
          <span>{{ connection.version() || '' }}</span>
        </span>
        <span class="text-2xs text-ink-subtle">{{ theme.vibe() }}</span>
        <button
          type="button"
          class="flex cursor-pointer items-center rounded-sm p-0.5 text-ink-muted transition-colors hover:bg-hover hover:text-ink"
          (click)="cycleVibe()"
          title="Cycle theme vibe"
        >
          <app-icon [name]="vibeIcon()" [size]="14" />
        </button>
      </div>
    </footer>
  `,
  host: { class: 'contents' },
})
export class Statusbar {
  protected readonly session = inject(SessionStore);
  protected readonly theme = inject(ThemeService);
  protected readonly atlas = inject(AtlasStore);
  protected readonly connection = inject(ConnectionStore);
  protected readonly activity = inject(ActivityService);
  private readonly router = inject(Router);

  protected readonly summaryLabel = computed(() => this.session.summary()?.label ?? '');
  protected readonly activityLabel = computed(() => this.activity.label() || 'Working…');
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

  protected goHome(): void {
    void this.router.navigateByUrl('/');
  }

  protected cycleVibe(): void {
    const vibes = this.theme.vibes();
    const idx = vibes.findIndex((v) => v.id === this.theme.vibe());
    const next = vibes[(idx + 1) % vibes.length];
    if (next) this.theme.setVibe(next.id);
  }
}
