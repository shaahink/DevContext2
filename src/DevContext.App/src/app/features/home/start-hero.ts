import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { RecentStore } from '../../state/recent.store';
import { PrefsStore } from '../../state/prefs.store';
import { ActivityService } from '../../core/activity/activity.service';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';
import { Spinner } from '../../ui/spinner/spinner';

type InputType = 'local' | 'github' | null;

/**
 * Start (proposal §2) — the no-session state of `/`: native-feel folder picker,
 * recents, advanced options. Card-free (ported from the old section-landing.ts,
 * which wrapped this in the now-deleted SectionCard).
 */
@Component({
  selector: 'app-start-hero',
  imports: [FormsModule, Icon, Button, Spinner],
  template: `
    <div class="flex min-h-[60vh] flex-col items-center justify-center text-center">
      <span class="mb-3 text-4xl text-accent">&diams;</span>
      <h1 class="mb-2 text-2xl font-bold text-ink">DevContext</h1>
      <p class="mb-8 max-w-md text-sm text-ink-muted">
        The devtool lens for any .NET repository. Instant architecture understanding.
      </p>

      <div class="w-full max-w-xl space-y-4">
        <div class="flex gap-2">
          <div class="relative flex-1">
            <input
              class="w-full rounded-md border border-line bg-surface px-3 py-2.5 pr-8 font-mono text-sm text-ink outline-none placeholder:text-ink-subtle focus:border-accent"
              placeholder="Path, .sln, .csproj, or github.com/user/repo"
              [value]="path()"
              (input)="onPathInput($event)"
              (keydown.enter)="analyze()"
              [disabled]="session.busy()"
            />
            @if (path()) {
              <button class="absolute right-2 top-1/2 -translate-y-1/2 text-ink-subtle hover:text-ink" (click)="clearPath()">
                <app-icon name="x" [size]="14" />
              </button>
            }
          </div>
          <app-button variant="primary" (click)="analyze()" [disabled]="!path() || session.busy()">
            <app-icon name="play" [size]="14" />
            Analyze
          </app-button>
        </div>

        @if (inputType() === 'local') {
          <p class="text-xs text-success">Local path — will analyze directly.</p>
        } @else if (inputType() === 'github') {
          <p class="text-xs text-ink-muted">GitHub repo — will clone and analyze.</p>
        }

        @if (session.busy()) {
          <div class="space-y-2 rounded-md border border-line bg-surface px-3 py-2.5 text-left">
            <div class="flex items-center gap-2 text-xs">
              <app-spinner />
              <span class="flex-1 text-ink">{{ activity.label() || 'Working…' }}</span>
              @if (activity.percent() > 0) {
                <span class="font-mono tabular-nums text-accent">{{ activity.percent() }}%</span>
              }
              <button
                class="flex cursor-pointer items-center gap-1 rounded px-2 py-1 text-2xs text-ink-muted hover:bg-surface-2 hover:text-danger transition-colors"
                (click)="session.cancel()"
              >
                <app-icon name="x" [size]="14" /> Cancel
              </button>
            </div>
            @if (activity.percent() > 0) {
              <div class="h-1 w-full overflow-hidden rounded-full bg-surface-2">
                <div class="h-full bg-accent transition-all duration-300" [style.width.%]="activity.percent()"></div>
              </div>
            }
          </div>
        }

        @if (recents().length) {
          <div class="w-full max-w-xl pt-6">
            <p class="mb-2 text-xs font-semibold uppercase tracking-wider text-ink-subtle">Recent</p>
            <div class="grid gap-1.5">
              @for (r of recents(); track r.path) {
                <div class="group flex items-center gap-2 rounded-md border border-line bg-surface px-3 py-2 transition-colors hover:border-line-strong hover:bg-surface-2">
                  <button class="flex flex-1 items-center gap-2 text-left min-w-0" (click)="selectRecent(r.path)" [disabled]="session.busy()">
                    <app-icon name="folder-open" [size]="14" class="shrink-0 text-ink-muted" />
                    <div class="min-w-0 flex-1">
                      <p class="truncate font-mono text-sm text-ink">{{ r.label }}</p>
                      <p class="truncate text-2xs text-ink-subtle">{{ r.path }}</p>
                    </div>
                  </button>
                  <button
                    class="shrink-0 cursor-pointer rounded p-0.5 text-ink-muted opacity-0 group-hover:opacity-100 hover:bg-surface-2 hover:text-ink transition-all"
                    (click)="recentStore.remove(r.path); $event.stopPropagation()"
                    title="Remove from recents"
                  >
                    <app-icon name="x" [size]="14" />
                  </button>
                </div>
              }
            </div>
          </div>
        }

        <details class="group mt-6 w-full max-w-xl text-left">
          <summary class="cursor-pointer text-xs font-medium text-ink-muted hover:text-ink">Advanced options</summary>
          <div class="mt-3 space-y-3 rounded-md border border-line bg-surface p-3">
            <div class="grid grid-cols-2 gap-3">
              <label class="flex flex-col gap-1">
                <span class="text-2xs text-ink-subtle">Depth (1–10)</span>
                <input type="number" min="1" max="10" class="rounded border border-line bg-base px-2 py-1 font-mono text-xs text-ink outline-none focus:border-accent" [(ngModel)]="depth" [disabled]="session.busy()" />
              </label>
              <label class="flex flex-col gap-1">
                <span class="text-2xs text-ink-subtle">Detail</span>
                <select class="rounded border border-line bg-base px-2 py-1 text-xs text-ink outline-none focus:border-accent" [(ngModel)]="detail" [disabled]="session.busy()">
                  <option value="salient">Salient</option>
                  <option value="signature">Signature</option>
                  <option value="full">Full</option>
                </select>
              </label>
            </div>
            <label class="flex items-center gap-1.5 text-xs text-ink-muted">
              <input type="checkbox" [(ngModel)]="noRoslyn" class="rounded border-line" [disabled]="session.busy()" />
              No Roslyn
            </label>
            <div class="flex items-center gap-3">
              <span class="text-2xs text-ink-subtle">Clone cleanup</span>
              <label class="flex items-center gap-1.5 text-xs text-ink-muted">
                <input type="radio" name="landing-cleanup" value="auto" [(ngModel)]="cleanup" [disabled]="session.busy()" />
                Auto
              </label>
              <label class="flex items-center gap-1.5 text-xs text-ink-muted">
                <input type="radio" name="landing-cleanup" value="keep" [(ngModel)]="cleanup" [disabled]="session.busy()" />
                Keep
              </label>
            </div>
          </div>
        </details>
      </div>
    </div>
  `,
})
export class StartHero {
  protected readonly session = inject(SessionStore);
  protected readonly activity = inject(ActivityService);
  protected readonly recentStore = inject(RecentStore);
  private readonly prefs = inject(PrefsStore);

  protected readonly path = signal('');
  protected readonly depth = signal(this.prefs.defaultDepth());
  protected readonly detail = signal(this.prefs.defaultDetail());
  protected readonly noRoslyn = signal(!this.prefs.useRoslyn());
  protected readonly cleanup = signal(this.prefs.autoCleanup() ? 'auto' : 'keep' as 'auto' | 'keep');
  protected readonly inputType = signal<InputType>(null);
  protected readonly recents = this.recentStore.recents;

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      if (this.debounceTimer) clearTimeout(this.debounceTimer);
    });
  }

  protected onPathInput(e: Event): void {
    const val = (e.target as HTMLInputElement).value;
    this.path.set(val);
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => this.classify(val), 300);
  }

  private classify(val: string): void {
    if (!val.trim()) { this.inputType.set(null); return; }
    if (/^https?:\/\//i.test(val) || /github\.com\/[\w.-]+\/[\w.-]+/i.test(val)) {
      this.inputType.set('github');
    } else {
      this.inputType.set('local');
    }
  }

  protected clearPath(): void {
    this.path.set('');
    this.inputType.set(null);
  }

  protected selectRecent(path: string): void {
    this.path.set(path);
    this.analyze();
  }

  protected analyze(): void {
    const p = this.path().trim();
    if (!p || this.session.busy()) return;
    void this.session.analyze({
      path: p,
      depth: this.depth(),
      detail: this.detail(),
      noRoslyn: this.noRoslyn(),
      cleanup: this.cleanup(),
    });
  }
}
