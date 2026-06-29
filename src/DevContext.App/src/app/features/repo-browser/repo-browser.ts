import { Component, effect, inject, output, signal } from '@angular/core';

import { GitHubStore } from '../../state/github.store';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';
import { RepoCard } from '../../ui/repo-card/repo-card';

@Component({
  selector: 'app-repo-browser',
  imports: [Icon, RepoCard, Button],
  template: `
    <div class="flex flex-col">
      <div class="border-b border-line px-3 py-2">
        <div class="flex items-center gap-2 rounded-md border border-line bg-base px-2 py-1.5 focus-within:border-accent">
          <app-icon name="search" [size]="14" class="text-ink-subtle" />
          <input
            class="flex-1 bg-transparent text-sm text-ink outline-none placeholder:text-ink-subtle"
            placeholder="Search .NET repos on GitHub… (e.g. dotnet, aspnet, clean architecture)"
            [value]="query()"
            (input)="onQuery($event)"
            (keydown.escape)="cancel()"
          />
          @if (store.loading()) {
            <app-icon name="loader" [size]="14" class="animate-spin text-accent" />
          } @else if (query()) {
            <button (click)="cancel()" class="text-ink-subtle hover:text-ink">
              <app-icon name="x" [size]="13" />
            </button>
          }
        </div>
      </div>

      @if (store.error()) {
        <div class="border-b border-danger/40 bg-danger/10 px-3 py-2 text-xs text-danger">{{ store.error() }}</div>
      }

      <div class="min-h-0 flex-1 overflow-y-auto p-3">
        <div class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-3">
          @for (repo of store.results(); track repo.id) {
            <app-repo-card [repo]="repo" (analyze)="pick.emit($event)" />
          } @empty {
            @if (!store.loading()) {
              <div class="col-span-full flex flex-col items-center justify-center py-12 text-center">
                <app-icon name="search" [size]="32" class="text-ink-subtle" />
                <p class="mt-3 text-sm text-ink-muted">Search for .NET repos on GitHub</p>
                <p class="mt-1 text-xs text-ink-subtle">Try "dotnet", "aspnet", "blazor", or "clean architecture"</p>
              </div>
            }
          }
        </div>

        @if (store.hasMore() && !store.loading()) {
          <div class="mt-4 flex justify-center">
            <app-button
              variant="secondary"
              size="sm"
              (click)="store.loadMore()"
              [disabled]="store.loading()"
            >
              Load more ({{ store.results().length }} / {{ store.totalCount() }})
            </app-button>
          </div>
        }

        @if (store.loading() && store.results().length > 0) {
          <div class="mt-4 flex justify-center">
            <app-icon name="loader" [size]="16" class="animate-spin text-accent" />
          </div>
        }
      </div>
    </div>
  `,
})
export class RepoBrowser {
  protected readonly store = inject(GitHubStore);
  readonly pick = output<string>();

  readonly query = signal('');

  constructor() {
    effect(() => {
      const q = this.query();
      const timer = setTimeout(() => this.store.search(q), 300);
      return () => clearTimeout(timer);
    });
    void this.store.loadCurated();
  }

  protected onQuery(e: Event): void {
    this.query.set((e.target as HTMLInputElement).value);
  }

  protected cancel(): void {
    this.query.set('');
    this.store.cancel();
    void this.store.loadCurated();
  }
}
