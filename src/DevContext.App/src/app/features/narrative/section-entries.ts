import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { NodeStore } from '../../state/node.store';
import { KIND_LABELS, KIND_ICONS, type EntryVm } from '../../models/view-models';
import { ToastService } from '../../ui/toast/toast';
import { SectionCard } from '../../ui/section-card/section-card';
import { Icon } from '../../ui/icon/icon';
import { Badge } from '../../ui/badge/badge';
import { SearchField } from '../../ui/search-field/search-field';

type SortColumn = 'method' | 'route' | 'target' | 'kind';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-section-entries',
  imports: [FormsModule, SectionCard, Icon, Badge, SearchField],
  template: `
    <app-section-card id="entries" title="Entry Points" [subtitle]="subtitle()">
      <div class="space-y-3">
        <div class="flex flex-wrap items-center gap-2">
          <app-search-field [(query)]="search" class="w-56" />
          <span class="text-2xs text-ink-subtle">Filter:</span>
          <button
            class="rounded px-2 py-1 text-2xs font-medium transition-colors"
            [class.bg-accent]="!kindFilter()"
            [class.text-accent-ink]="!kindFilter()"
            [class.bg-surface-2]="kindFilter()"
            [class.text-ink-muted]="kindFilter()"
            [class.hover:bg-surface-2]="kindFilter()"
            (click)="kindFilter.set(null)"
          >All</button>
          @for (kind of allKinds(); track kind) {
            <button
              class="rounded px-2 py-1 text-2xs font-medium transition-colors"
              [class.bg-accent]="kindFilter() === kind"
              [class.text-accent-ink]="kindFilter() === kind"
              [class.bg-surface-2]="kindFilter() !== kind"
              [class.text-ink-muted]="kindFilter() !== kind"
              [class.hover:bg-surface-2]="kindFilter() !== kind"
              (click)="kindFilter.set(kindFilter() === kind ? null : kind)"
            >
              <app-icon [name]="KIND_ICONS[kind] ?? 'dot'" [size]="11" />
              {{ KIND_LABELS[kind] ?? kind }}
            </button>
          }
        </div>

        <div class="overflow-x-auto rounded-md border border-line">
          <table class="w-full text-left text-xs" (keydown)="onTableKey($event)" tabindex="0">
            <thead>
              <tr class="border-b border-line bg-surface-2 text-2xs font-semibold uppercase tracking-wider text-ink-muted">
                <th class="px-3 py-2 w-16 cursor-pointer hover:text-ink select-none" (click)="toggleSort('method')">
                  Method {{ sortArrow('method') }}
                </th>
                <th class="px-3 py-2 cursor-pointer hover:text-ink select-none" (click)="toggleSort('route')">
                  Route / Title {{ sortArrow('route') }}
                </th>
                <th class="px-3 py-2 cursor-pointer hover:text-ink select-none" (click)="toggleSort('target')">
                  Target {{ sortArrow('target') }}
                </th>
                <th class="px-3 py-2 w-28 cursor-pointer hover:text-ink select-none" (click)="toggleSort('kind')">
                  Kind {{ sortArrow('kind') }}
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-line">
              @for (entry of sortedEntries(); track entry.nodeId; let idx = $index) {
                <tr
                  class="cursor-pointer transition-colors hover:bg-surface-2"
                  [class.bg-accent/10]="selectedIndex() === idx"
                  (click)="selectEntry(entry, idx)"
                  (keydown)="onRowKey($event, entry)"
                  tabindex="0"
                  (focus)="selectedIndex.set(idx)"
                >
                  <td class="px-3 py-1.5">
                    @if (entry.httpMethod) {
                      <app-badge variant="accent">{{ entry.httpMethod }}</app-badge>
                    }
                  </td>
                  <td class="px-3 py-1.5">
                    <div class="flex items-center gap-1.5">
                      <span class="font-mono text-ink">{{ entry.route || entry.title }}</span>
                      @if (entry.provenance === 'Syntactic') {
                        <app-badge variant="warn">approx</app-badge>
                      }
                    </div>
                  </td>
                  <td class="px-3 py-1.5 font-mono text-ink-muted">
                    @if (entry.target) {
                      <app-icon name="arrow-right" [size]="11" class="inline text-ink-subtle" />
                      {{ entry.target }}
                    }
                  </td>
                  <td class="px-3 py-1.5">
                    <span class="text-2xs text-ink-subtle">{{ KIND_LABELS[entry.kind] ?? entry.kind }}</span>
                  </td>
                </tr>
              }
            </tbody>
          </table>
          @if (!sortedEntries().length) {
            <p class="px-3 py-6 text-center text-xs text-ink-subtle">
              {{ session.ready() ? 'No entries match.' : 'Analyze a repo to list its entry points.' }}
            </p>
          }
        </div>
      </div>
    </app-section-card>
  `,
})
export class SectionEntries {
  protected readonly session = inject(SessionStore);
  private readonly trace = inject(TraceStore);
  private readonly nodeStore = inject(NodeStore);
  private readonly toast = inject(ToastService);

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly KIND_LABELS = KIND_LABELS;
  protected readonly KIND_ICONS = KIND_ICONS;

  protected readonly sortColumn = signal<SortColumn>('route');
  protected readonly sortDir = signal<SortDir>('asc');
  protected readonly selectedIndex = signal(0);

  protected readonly allKinds = computed(() => {
    const kinds = new Set<string>();
    for (const g of this.session.entryGroups()) kinds.add(g.kind);
    return [...kinds];
  });

  protected readonly filteredEntries = computed<EntryVm[]>(() => {
    const groups = this.session.entryGroups();
    const kf = this.kindFilter();
    const q = this.search().toLowerCase();
    let entries = (kf
      ? groups.filter((g) => g.kind === kf)
      : groups
    ).flatMap((g) => g.entries);
    if (q) {
      entries = entries.filter(
        (e) =>
          (e.route ?? e.title).toLowerCase().includes(q) ||
          (e.target ?? '').toLowerCase().includes(q),
      );
    }
    return entries;
  });

  protected readonly sortedEntries = computed<EntryVm[]>(() => {
    const entries = [...this.filteredEntries()];
    const col = this.sortColumn();
    const dir = this.sortDir();
    entries.sort((a, b) => {
      const av = sortValue(a, col);
      const bv = sortValue(b, col);
      if (av < bv) return dir === 'asc' ? -1 : 1;
      if (av > bv) return dir === 'asc' ? 1 : -1;
      return 0;
    });
    return entries;
  });

  protected readonly totalCount = computed(() => this.session.entryCount());
  protected readonly filteredCount = computed(() => this.sortedEntries().length);

  protected readonly subtitle = computed(() => {
    const fc = this.filteredCount();
    const tc = this.totalCount();
    return fc === tc ? `${fc} entries` : `${fc} / ${tc} entries`;
  });

  protected toggleSort(col: SortColumn): void {
    if (this.sortColumn() === col) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(col);
      this.sortDir.set('asc');
    }
  }

  protected sortArrow(col: SortColumn): string {
    if (this.sortColumn() !== col) return '';
    return this.sortDir() === 'asc' ? '▲' : '▼';
  }

  protected selectEntry(entry: EntryVm, idx: number): void {
    this.selectedIndex.set(idx);
    this.traceEntry(entry);
  }

  protected onTableKey(e: KeyboardEvent): void {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      this.selectedIndex.update((i) => Math.min(i + 1, this.sortedEntries().length - 1));
      this.scrollToSelected();
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      this.selectedIndex.update((i) => Math.max(i - 1, 0));
      this.scrollToSelected();
    }
  }

  protected onRowKey(e: KeyboardEvent, entry: EntryVm): void {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      this.traceEntry(entry);
    } else if (e.key === 'n' && !e.ctrlKey && !e.metaKey) {
      e.preventDefault();
      this.nodeStore.show(entry.nodeId);
    } else if ((e.ctrlKey || e.metaKey) && e.key === 'c') {
      e.preventDefault();
      const text = entry.route || entry.title;
      navigator.clipboard?.writeText(text)
        .then(() => this.toast.show('Copied: ' + text, 'info'))
        .catch(() => this.toast.show('Copy failed', 'error'));
    }
  }

  protected traceEntry(entry: EntryVm): void {
    const handle = this.session.handle();
    if (!handle) return;
    void this.trace.trace(handle, entry.focus);
  }

  private scrollToSelected(): void {
    queueMicrotask(() => {
      const rows = document.querySelectorAll('app-section-entries tbody tr');
      const row = rows[this.selectedIndex()];
      row?.scrollIntoView({ block: 'nearest' });
    });
  }
}

function sortValue(e: EntryVm, col: SortColumn): string {
  switch (col) {
    case 'method': return e.httpMethod ?? '';
    case 'route': return e.route ?? e.title;
    case 'target': return e.target ?? '';
    case 'kind': return KIND_LABELS[e.kind] ?? e.kind;
  }
}
