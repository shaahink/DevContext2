import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { type EntryVm } from '../../models/view-models';
import { KIND_LABELS, KIND_ICONS } from '../../models/view-models';
import { ViewFrame } from '../../shell/view-frame';
import { Badge } from '../../ui/badge/badge';
import { Icon } from '../../ui/icon/icon';
import { SearchField } from '../../ui/search-field/search-field';

@Component({
  selector: 'app-entries-view',
  imports: [Icon, Badge, SearchField, ViewFrame],
  template: `
    <app-view-frame>
      <div sidebar class="flex flex-col h-full">
        <div class="border-b border-line px-3 py-2">
          <app-search-field [(query)]="search" />
        </div>
        <div class="flex-1 overflow-auto py-1">
          @for (group of filteredGroups(); track group.kind) {
            <button
              class="flex w-full items-center gap-2 px-3 py-1.5 text-xs text-ink-muted transition-colors hover:bg-surface-2 hover:text-ink"
              [class.bg-accent/10]="kindFilter() === group.kind"
              [class.text-accent]="kindFilter() === group.kind"
              (click)="kindFilter.set(kindFilter() === group.kind ? null : group.kind)"
            >
              <app-icon [name]="KIND_ICONS[group.kind] ?? 'dot'" [size]="12" />
              {{ group.kind }}
              <span class="ml-auto rounded bg-surface-2 px-1.5 py-0.5 text-2xs text-ink-subtle">{{ group.entries.length }}</span>
            </button>
          }
        </div>
      </div>

      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Entry Points</h2>
        <span class="text-xs text-ink-subtle">{{ totalEntries() }} entries</span>
        @if (kindFilter()) {
          <app-badge variant="accent">{{ kindFilter() }}</app-badge>
          <button class="text-ink-subtle hover:text-ink" (click)="kindFilter.set(null)"><app-icon name="x" [size]="12" /></button>
        }
      </div>

      @if (session.ready()) {
        <div class="divide-y divide-line">
          @for (group of filteredGroups(); track group.kind) {
            @for (entry of group.entries; track entry.nodeId) {
              <div
                class="flex items-center gap-3 px-4 py-2 text-xs hover:bg-surface-2 cursor-pointer"
                (click)="traceEntry(entry)"
                (keydown.enter)="traceEntry(entry)"
                tabindex="0"
              >
                <div class="min-w-0 flex-1">
                  <p class="truncate font-mono text-ink">{{ entry.route || entry.title }}</p>
                  @if (entry.target) { <p class="truncate text-ink-muted">&rarr; {{ entry.target }}</p> }
                </div>
                <div class="flex shrink-0 items-center gap-1.5">
                  @if (entry.provenance === 'Syntactic') { <app-badge variant="warn">approx</app-badge> }
                  @if (entry.httpMethod) { <app-badge variant="accent">{{ entry.httpMethod }}</app-badge> }
                </div>
              </div>
            }
          }
        </div>
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to see entry points.</p></div>
      }
    </app-view-frame>
  `,
})
export class EntriesView {
  protected readonly session = inject(SessionStore);
  private readonly trace = inject(TraceStore);
  private readonly router = inject(Router);

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly KIND_LABELS = KIND_LABELS;
  protected readonly KIND_ICONS = KIND_ICONS;

  protected readonly filteredGroups = computed(() => {
    const groups = this.session.entryGroups();
    const q = this.search().toLowerCase();
    const kf = this.kindFilter();
    let filtered = groups;
    if (kf) filtered = filtered.filter((g) => g.kind === kf);
    if (q) {
      filtered = filtered.map((g) => ({
        ...g,
        entries: g.entries.filter((e) => (e.route ?? e.title).toLowerCase().includes(q) || (e.target ?? '').toLowerCase().includes(q)),
      })).filter((g) => g.entries.length > 0);
    }
    return filtered;
  });

  protected totalEntries(): number {
    return this.filteredGroups().reduce((n, g) => n + g.entries.length, 0);
  }

  protected traceEntry(entry: EntryVm): void {
    const handle = this.session.handle();
    if (!handle) return;
    void this.trace.trace(handle, entry.focus);
    this.router.navigate(['/trace']);
  }
}
