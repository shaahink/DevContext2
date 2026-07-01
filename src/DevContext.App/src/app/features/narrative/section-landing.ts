import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { RecentStore } from '../../state/recent.store';
import { ActivityService } from '../../core/activity/activity.service';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';
import { Spinner } from '../../ui/spinner/spinner';
import { SectionCard } from '../../ui/section-card/section-card';

type InputType = 'local' | 'github' | null;

@Component({
  selector: 'app-section-landing',
  imports: [FormsModule, Icon, Button, Spinner, SectionCard],
  template: `
    <app-section-card id="landing">
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
                <span class="text-ink">{{ activity.label() || 'Working…' }}</span>
                @if (activity.stage()) {
                  <span class="font-mono text-ink-subtle">{{ activity.stage() }}</span>
                }
                @if (activity.percent() > 0) {
                  <span class="ml-auto font-mono tabular-nums text-accent">{{ activity.percent() }}%</span>
                }
              </div>
              @if (activity.percent() > 0) {
                <div class="h-1 w-full overflow-hidden rounded-full bg-surface-2">
                  <div class="h-full bg-accent transition-all duration-300" [style.width.%]="activity.percent()"></div>
                </div>
              }
            </div>
          }
        </div>

        @if (recents().length) {
          <div class="w-full max-w-xl pt-6">
            <p class="mb-2 text-xs font-semibold uppercase tracking-wider text-ink-subtle">Recent</p>
            <div class="grid gap-1.5">
              @for (r of recents(); track r.path) {
                <button
                  class="flex w-full items-center gap-2 rounded-md border border-line bg-surface px-3 py-2 text-left transition-colors hover:border-line-strong hover:bg-surface-2"
                  (click)="selectRecent(r.path)"
                  [disabled]="session.busy()"
                >
                  <app-icon name="folder-open" [size]="14" class="shrink-0 text-ink-muted" />
                  <div class="min-w-0 flex-1">
                    <p class="truncate font-mono text-sm text-ink">{{ r.label }}</p>
                    <p class="truncate text-3xs text-ink-subtle">{{ r.path }}</p>
                  </div>
                </button>
              }
            </div>
          </div>
        }

        <details class="group mt-6 w-full max-w-xl text-left">
          <summary class="cursor-pointer text-xs font-medium text-ink-muted hover:text-ink">Advanced options</summary>
          <div class="mt-3 space-y-3 rounded-md border border-line bg-surface p-3">
            <div class="grid grid-cols-2 gap-3">
              <label class="flex flex-col gap-1">
                <span class="text-3xs text-ink-subtle">Depth (1–10)</span>
                <input type="number" min="1" max="10" class="rounded border border-line bg-base px-2 py-1 font-mono text-xs text-ink outline-none focus:border-accent" [(ngModel)]="depth" [disabled]="session.busy()" />
              </label>
              <label class="flex flex-col gap-1">
                <span class="text-3xs text-ink-subtle">Detail</span>
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
              <span class="text-3xs text-ink-subtle">Clone cleanup</span>
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
    </app-section-card>
  `,
})
export class SectionLanding {
  protected readonly session = inject(SessionStore);
  protected readonly activity = inject(ActivityService);
  private readonly recentStore = inject(RecentStore);

  protected readonly path = signal('');
  protected readonly depth = signal(6);
  protected readonly detail = signal<'salient' | 'signature' | 'full'>('salient');
  protected readonly noRoslyn = signal(false);
  protected readonly cleanup = signal<'auto' | 'keep'>('auto');
  protected readonly inputType = signal<InputType>(null);
  protected readonly recents = this.recentStore.recents;

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

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
