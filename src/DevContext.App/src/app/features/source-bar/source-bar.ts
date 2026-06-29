import { Component, inject, signal } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';
import { SessionStore } from '../../state/session.store';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';
import { KbdHint } from '../../ui/kbd-hint/kbd-hint';

@Component({
  selector: 'app-source-bar',
  imports: [Icon, Button, KbdHint],
  template: `
    <div class="flex items-center gap-2 border-b border-line bg-surface px-3 py-2">
      <app-icon name="folder-open" class="text-ink-subtle" />
      <input
        class="min-w-0 flex-1 rounded-md border border-line bg-base px-3 py-1.5 font-mono text-ink outline-none placeholder:text-ink-subtle focus:border-accent"
        placeholder="Path, .sln, .csproj, or github.com/user/repo"
        [value]="path()"
        (input)="onInput($event)"
        (keydown)="onKey($event)"
      />
      @if (busy()) {
        <app-button
          variant="secondary"
          (click)="cancel()"
        >
          <app-icon name="square" [size]="14" /> Cancel
        </app-button>
      } @else {
        <app-button
          variant="primary"
          (click)="analyze()"
          [disabled]="!path().trim()"
        >
          <app-icon name="play" [size]="14" /> Analyze
        </app-button>
      }
      <span
        class="ml-1 h-2 w-2 rounded-full"
        [class.bg-success]="online()"
        [class.bg-danger]="!online()"
        [title]="online() ? 'Server connected' : 'Server offline'"
      ></span>
      <app-kbd-hint>Ctrl+K</app-kbd-hint>
    </div>

    @if (busy()) {
      <div class="h-0.5 w-full bg-surface-2">
        <div class="h-full bg-accent transition-all duration-300" [style.width.%]="progress().percent"></div>
      </div>
      <div class="flex items-center gap-2 bg-surface px-3 py-1 text-xs text-ink-muted">
        <app-icon name="loader" [size]="12" class="animate-spin" />
        {{ progress().message }}
      </div>
    }

    @if (error(); as e) {
      <div class="border-b border-danger/40 bg-danger/10 px-3 py-1.5 text-xs text-danger">{{ e }}</div>
    }

    @if (summary(); as s) {
      <div class="flex flex-wrap items-center gap-x-4 gap-y-1 border-b border-line bg-surface px-3 py-1 text-xs text-ink-muted">
        <span class="font-medium text-ink">{{ s.label }}</span>
        <span>{{ s.projects }} projects</span>
        <span>{{ s.nodes }} nodes &middot; {{ s.edges }} edges</span>
        <span>{{ s.entries }} entries &middot; {{ s.entriesWithTarget }} &rarr; target</span>
        <span>{{ s.elapsedMs }} ms</span>
      </div>
    }
  `,
})
export class SourceBar {
  private readonly session = inject(SessionStore);
  private readonly connection = inject(ConnectionStore);

  readonly path = signal('');
  readonly busy = this.session.busy;
  readonly progress = this.session.progress;
  readonly summary = this.session.summary;
  readonly error = this.session.error;
  readonly online = this.connection.online;

  analyze(): void {
    const p = this.path().trim();
    if (p) void this.session.analyze({ path: p });
  }

  cancel(): void {
    this.session.cancel();
  }

  onInput(e: Event): void {
    this.path.set((e.target as HTMLInputElement).value);
  }

  onKey(e: KeyboardEvent): void {
    if (e.key === 'Enter') this.analyze();
  }
}
