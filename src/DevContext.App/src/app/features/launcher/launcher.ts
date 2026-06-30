import { Component, inject, output, signal } from '@angular/core';

import { RecentStore } from '../../state/recent.store';
import { SessionStore } from '../../state/session.store';
import { Icon } from '../../ui/icon/icon';
import { RepoBrowser } from '../repo-browser/repo-browser';

@Component({
  selector: 'app-launcher',
  imports: [Icon, RepoBrowser],
  template: `
    <div class="flex h-full flex-col items-center overflow-auto px-6 py-10">
      <div class="mb-6 flex flex-col items-center text-center">
        <app-icon name="network" [size]="48" class="text-accent" />
        <h1 class="mt-3 text-xl font-semibold text-ink">Read any .NET repo, fast.</h1>
        <p class="mt-2 max-w-md text-sm leading-relaxed text-ink-muted">
          Point DevContext at a folder, <span class="font-mono">.sln</span>, or a GitHub URL.
          It maps the architecture, lists every entry point, and traces any flow down the wiring.
        </p>
      </div>

      <div class="mb-4 flex items-center overflow-hidden rounded-md border border-line">
        @for (t of tabs; track t.id) {
          <button
            class="px-3 py-1.5 text-xs transition-colors"
            [class.bg-accent]="activeTab() === t.id"
            [class.text-accent-ink]="activeTab() === t.id"
            [class.text-ink-muted]="activeTab() !== t.id"
            [class.hover:text-ink]="activeTab() !== t.id"
            (click)="activeTab.set(t.id)"
          >
            {{ t.label }}
          </button>
        }
      </div>

      <div class="w-full max-w-4xl rounded-lg border border-line bg-surface">
        @switch (activeTab()) {
          @case ('search') {
            <app-repo-browser (pick)="pickRepo.emit($event)" />
          }
          @case ('local') {
            <div class="flex flex-col items-center gap-4 p-8">
              <app-icon name="folder-open" [size]="32" class="text-ink-subtle" />
              <p class="text-sm text-ink-muted">Enter a local path, <span class="font-mono">.sln</span>, or GitHub URL in the source bar above.</p>
              <p class="text-xs text-ink-subtle">Press <kbd class="rounded border border-line bg-surface-2 px-1 py-0.5 font-mono">Enter</kbd> to analyze.</p>
            </div>
          }
          @case ('recents') {
            @if (recentStore.recents().length) {
              <div class="divide-y divide-line">
                @for (r of recentStore.recents(); track r.path) {
                  <div class="flex items-center justify-between px-4 py-2.5">
                    <div class="min-w-0 flex-1">
                      <button class="truncate font-mono text-sm text-ink hover:text-accent" (click)="pickRepo.emit(r.path)">
                        {{ r.label }}
                      </button>
                      <div class="truncate text-[10px] text-ink-subtle">{{ r.path }}</div>
                    </div>
                    <button class="ml-2 shrink-0 text-ink-subtle hover:text-ink" (click)="recentStore.remove(r.path)">
                      <app-icon name="x" [size]="12" />
                    </button>
                  </div>
                }
              </div>
            } @else {
              <div class="flex flex-col items-center gap-3 p-8 text-center">
                <app-icon name="refresh" [size]="24" class="text-ink-subtle" />
                <p class="text-xs text-ink-muted">No recent repos yet. Search GitHub or enter a local path to get started.</p>
              </div>
            }
          }
        }
      </div>
    </div>
  `,
})
export class Launcher {
  protected readonly session = inject(SessionStore);
  protected readonly recentStore = inject(RecentStore);
  readonly pickRepo = output<string>();

  readonly activeTab = signal('search');
  readonly tabs = [
    { id: 'search', label: 'Search GitHub' },
    { id: 'local', label: 'Local path' },
    { id: 'recents', label: 'Recents' },
  ] as const;
}
