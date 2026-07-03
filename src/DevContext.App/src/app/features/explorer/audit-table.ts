import { Component, computed, inject, input, output, signal } from '@angular/core';

import { KIND_LABELS, type EntryGroupVm, type EntryVm } from '../../models/view-models';
import { Icon } from '../../ui/icon/icon';
import { KindIcon } from '../../ui/kind-icon/kind-icon';
import { ToastService } from '../../ui/toast/toast';
import { copyToClipboard } from '../../core/clipboard';

type SortColumn = 'method' | 'route' | 'target' | 'kind';
type SortDir = 'asc' | 'desc';

/**
 * Audit table (proposal §2/§8.1) — Shift+E overlay from the Entry Deck: today's
 * sortable, filterable section-entries table, for reviewing/auditing the full entry
 * list rather than the deck's keyboard-first sweep. Self-contained filter/sort state
 * (no URL sync — it's a transient overlay, like the omnibox or export drawer, not a
 * page). Row "Trace" re-emits `selectionChange`; the Workbench decides what that means
 * (trace + trail push, same as a deck selection) and closes the overlay.
 */
@Component({
  selector: 'app-audit-table',
  imports: [Icon, KindIcon],
  template: `
    <div class="fixed inset-0 z-50" [class.hidden]="!open()">
      <div
        class="absolute inset-0 bg-base/70"
        role="button"
        tabindex="0"
        aria-label="Close audit table"
        (click)="dismissed.emit()"
        (keydown.enter)="dismissed.emit()"
      ></div>
      <div class="overlay-float absolute inset-6 flex flex-col overflow-hidden md:inset-12">
        <div class="flex items-center gap-2 border-b border-line px-3 py-2">
          <h2 class="text-sm font-semibold text-ink">Entry Audit</h2>
          <span class="text-2xs text-ink-subtle">{{ subtitle() }}</span>
          <span class="flex-1"></span>
          <button type="button" class="chip" (click)="dismissed.emit()" title="Close (Esc)">✕</button>
        </div>

        <div class="flex flex-wrap items-center gap-2 border-b border-line px-3 py-2">
          <input
            type="text"
            class="w-56 rounded border border-line bg-base px-2 py-1 text-xs text-ink outline-none placeholder:text-ink-subtle focus:border-accent"
            placeholder="Filter…"
            [value]="search()"
            (input)="search.set($any($event.target).value)"
          />
          <span class="h-4 w-px bg-line"></span>
          <button type="button" class="chip" [class.active]="!kindFilter()" (click)="kindFilter.set(null)">All</button>
          @for (kind of allKinds(); track kind) {
            <button type="button" class="chip" [class.active]="kindFilter() === kind" (click)="kindFilter.set(kindFilter() === kind ? null : kind)">
              <app-kind-icon [kind]="kind" [size]="11" />
              {{ KIND_LABELS[kind] ?? kind }}
              <span class="tabular-nums text-ink-subtle">{{ kindCounts()[kind] }}</span>
            </button>
          }
          <span class="h-4 w-px bg-line"></span>
          <button type="button" class="chip" [class.active]="filterApprox()" (click)="filterApprox.set(!filterApprox())">
            approx <span class="tabular-nums text-ink-subtle">{{ quickCounts().approx }}</span>
          </button>
          <button type="button" class="chip" [class.active]="filterHasTarget()" (click)="filterHasTarget.set(!filterHasTarget())">
            has target <span class="tabular-nums text-ink-subtle">{{ quickCounts().hasTarget }}</span>
          </button>
        </div>

        <div class="min-h-0 flex-1 overflow-y-auto">
          <table class="w-full text-left text-xs">
            <thead class="sticky top-0 z-10 bg-surface">
              <tr class="border-b border-line text-2xs font-semibold uppercase tracking-wider text-ink-muted">
                <th class="w-16 cursor-pointer select-none px-3 py-2 hover:text-ink" (click)="toggleSort('method')">
                  Method {{ sortArrow('method') }}
                </th>
                <th class="cursor-pointer select-none px-3 py-2 hover:text-ink" (click)="toggleSort('route')">
                  Route {{ sortArrow('route') }}
                </th>
                <th class="cursor-pointer select-none px-3 py-2 hover:text-ink" (click)="toggleSort('target')">
                  Target {{ sortArrow('target') }}
                </th>
                <th class="w-28 cursor-pointer select-none px-3 py-2 hover:text-ink" (click)="toggleSort('kind')">
                  Kind {{ sortArrow('kind') }}
                </th>
                <th class="w-20 px-3 py-2"></th>
              </tr>
            </thead>
            <tbody class="divide-y divide-line">
              @for (entry of sortedEntries(); track entry.nodeId) {
                <tr
                  class="group cursor-pointer hover:bg-hover"
                  tabindex="0"
                  (click)="selectEntry(entry)"
                  (keydown)="onRowKey($event, entry)"
                >
                  <td class="w-16 px-3 py-1.5">
                    @if (entry.httpMethod) {
                      <span class="chip">{{ entry.httpMethod }}</span>
                    }
                  </td>
                  <td class="px-3 py-1.5">
                    <div class="flex items-center gap-1.5">
                      <span class="font-mono text-ink">{{ entry.route || entry.title }}</span>
                      @if (entry.provenance === 'Syntactic') {
                        <span class="chip text-warn">approx</span>
                      }
                    </div>
                  </td>
                  <td class="px-3 py-1.5 font-mono text-ink-muted">
                    @if (entry.target) {
                      <app-icon name="arrow-right" [size]="11" class="inline text-ink-subtle" />
                      {{ entry.target }}
                    }
                  </td>
                  <td class="w-28 px-3 py-1.5">
                    <span class="text-2xs text-ink-subtle">{{ KIND_LABELS[entry.kind] ?? entry.kind }}</span>
                  </td>
                  <td class="w-20 px-3 py-1.5">
                    <div class="flex items-center gap-1 opacity-0 transition-opacity group-hover:opacity-100 group-focus-within:opacity-100">
                      <button type="button" class="rounded p-1 text-ink-muted hover:bg-hover hover:text-ink" (click)="copyRoute(entry, $event)" title="Copy route (Ctrl+C)">
                        <app-icon name="copy" [size]="12" />
                      </button>
                    </div>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="5" class="px-3 py-6 text-center text-xs text-ink-subtle">
                    No entries match — clear filters.
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
})
export class AuditTable {
  readonly open = input(false);
  readonly groups = input<readonly EntryGroupVm[]>([]);

  readonly selectionChange = output<EntryVm>();
  readonly dismissed = output<void>();

  private readonly toast = inject(ToastService);

  protected readonly KIND_LABELS = KIND_LABELS;

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly filterApprox = signal(false);
  protected readonly filterHasTarget = signal(false);
  protected readonly sortColumn = signal<SortColumn>('route');
  protected readonly sortDir = signal<SortDir>('asc');

  protected readonly allKinds = computed(() => {
    const kinds = new Set<string>();
    for (const g of this.groups()) kinds.add(g.kind);
    return [...kinds];
  });

  protected readonly kindCounts = computed<Record<string, number>>(() => {
    const counts: Record<string, number> = {};
    for (const g of this.groups()) counts[g.kind] = g.entries.length;
    return counts;
  });

  protected readonly quickCounts = computed(() => {
    let approx = 0;
    let hasTarget = 0;
    for (const g of this.groups()) {
      for (const e of g.entries) {
        if (e.provenance === 'Syntactic') approx++;
        if (e.target) hasTarget++;
      }
    }
    return { approx, hasTarget };
  });

  private readonly filteredEntries = computed<EntryVm[]>(() => {
    const kf = this.kindFilter();
    const q = this.search().trim().toLowerCase();
    const approx = this.filterApprox();
    const hasTarget = this.filterHasTarget();

    let entries = (kf ? this.groups().filter((g) => g.kind === kf) : this.groups()).flatMap((g) => g.entries);
    if (q) {
      entries = entries.filter(
        (e) => (e.route ?? e.title).toLowerCase().includes(q) || (e.target ?? '').toLowerCase().includes(q),
      );
    }
    if (approx) entries = entries.filter((e) => e.provenance === 'Syntactic');
    if (hasTarget) entries = entries.filter((e) => e.target != null);
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

  protected readonly subtitle = computed(() => {
    const total = this.groups().reduce((n, g) => n + g.entries.length, 0);
    const filtered = this.sortedEntries().length;
    return filtered === total ? `${total} entries` : `${filtered} / ${total} entries`;
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

  protected selectEntry(entry: EntryVm): void {
    this.selectionChange.emit(entry);
  }

  protected onRowKey(e: KeyboardEvent, entry: EntryVm): void {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      this.selectionChange.emit(entry);
    } else if ((e.ctrlKey || e.metaKey) && e.key === 'c') {
      e.preventDefault();
      this.copyRoute(entry, e);
    }
  }

  protected copyRoute(entry: EntryVm, event: Event): void {
    event.stopPropagation();
    const text = entry.route || entry.title;
    void copyToClipboard(text)
      .then(() => this.toast.show('Copied: ' + text, 'info'))
      .catch(() => this.toast.show('Copy failed', 'error'));
  }
}

function sortValue(e: EntryVm, col: SortColumn): string {
  switch (col) {
    case 'method':
      return e.httpMethod ?? '';
    case 'route':
      return e.route ?? e.title;
    case 'target':
      return e.target ?? '';
    case 'kind':
      return KIND_LABELS[e.kind] ?? e.kind;
  }
}
