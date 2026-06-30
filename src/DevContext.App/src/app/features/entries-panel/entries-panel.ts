import { Component, computed, effect, inject, signal } from '@angular/core';

import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';
import { SearchField } from '../../ui/search-field/search-field';

@Component({
  selector: 'app-entries-panel',
  imports: [Icon, SearchField, Badge, Button],
  template: `
    <div class="flex h-full flex-col border-r border-line bg-surface">
      <div class="space-y-2 border-b border-line p-2">
        <app-button
          variant="ghost"
          size="sm"
          class="w-full justify-start"
          (click)="showMap()"
        >
          <app-icon name="map" [size]="14" />
          <span class="font-medium">Overview map</span>
        </app-button>
        <app-search-field [(query)]="query" />
      </div>

      <div class="min-h-0 flex-1 overflow-y-auto py-1">
        @for (group of filteredGroups(); track group.kind) {
          <div class="mb-1">
            <div class="flex items-center justify-between px-3 py-1 text-[10px] font-semibold uppercase tracking-wide text-ink-subtle">
              <span>{{ group.label }}</span>
              <app-badge variant="default">{{ group.entries.length }}</app-badge>
            </div>
            @for (entry of group.entries; track entry.nodeId) {
              <button
                (click)="select(entry)"
                class="block w-full px-3 py-1.5 text-left hover:bg-surface-2"
                [class.bg-elevated]="entry.focus === activeFocus()"
              >
                <div class="flex items-center gap-2">
                  @if (entry.httpMethod) {
                    <app-badge variant="accent">{{ entry.httpMethod }}</app-badge>
                  }
                  <span class="truncate font-mono text-xs text-ink">{{ entry.route || entry.title }}</span>
                </div>
                @if (entry.target) {
                  <div class="mt-0.5 flex items-center gap-1 truncate pl-1 text-[11px] text-ink-subtle">
                    <app-icon name="arrow-right" [size]="11" />
                    <span class="truncate">{{ entry.target }}</span>
                  </div>
                }
              </button>
            }
          </div>
        } @empty {
          <p class="px-3 py-6 text-center text-xs text-ink-subtle">
            {{ session.ready() ? 'No entries match.' : 'Analyze a repo to list its entry points.' }}
          </p>
        }
      </div>
    </div>
  `,
})
export class EntriesPanel {
  protected readonly session = inject(SessionStore);
  private readonly trace = inject(TraceStore);

  readonly query = signal('');
  readonly activeFocus = this.trace.focus;
  private prevHandle: string | null = null;

  readonly filteredGroups = computed<EntryGroupVm[]>(() => {
    const q = this.query().trim().toLowerCase();
    const groups = this.session.entryGroups();
    if (!q) return [...groups];
    return groups
      .map((g) => ({ ...g, entries: g.entries.filter((e) => matches(e, q)) }))
      .filter((g) => g.entries.length > 0);
  });

  constructor() {
    effect(() => {
      const h = this.session.handle();
      if (h !== this.prevHandle) {
        this.prevHandle = h;
        this.query.set('');
      }
    });
  }

  select(entry: EntryVm): void {
    const handle = this.session.handle();
    if (handle) void this.trace.trace(handle, entry.focus);
  }

  showMap(): void {
    this.trace.clear();
  }
}

function matches(e: EntryVm, q: string): boolean {
  return (
    e.title.toLowerCase().includes(q) ||
    (e.route?.toLowerCase().includes(q) ?? false) ||
    (e.target?.toLowerCase().includes(q) ?? false)
  );
}
