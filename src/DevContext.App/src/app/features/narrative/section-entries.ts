import { Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
        <!-- Filter bar -->
        <div class="flex flex-wrap items-center gap-2">
          <app-search-field [(query)]="search" class="w-56" />
          <span class="h-4 w-px bg-line"></span>
          <span class="text-2xs text-ink-subtle shrink-0">Kind:</span>
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
              class="rounded px-2 py-1 text-2xs font-medium transition-colors flex items-center gap-1"
              [class.bg-accent]="kindFilter() === kind"
              [class.text-accent-ink]="kindFilter() === kind"
              [class.bg-surface-2]="kindFilter() !== kind"
              [class.text-ink-muted]="kindFilter() !== kind"
              [class.hover:bg-surface-2]="kindFilter() !== kind"
              (click)="kindFilter.set(kindFilter() === kind ? null : kind)"
            >
              <app-icon [name]="KIND_ICONS[kind] ?? 'dot'" [size]="11" />
              {{ KIND_LABELS[kind] ?? kind }}
              <span class="tabular-nums text-ink-subtle">{{ kindCounts()[kind] }}</span>
            </button>
          }
          <span class="h-4 w-px bg-line"></span>
          <button
            class="rounded px-2 py-1 text-2xs font-medium transition-colors flex items-center gap-1"
            [class.bg-accent]="filterApprox()"
            [class.text-accent-ink]="filterApprox()"
            [class.bg-surface-2]="!filterApprox()"
            [class.text-ink-muted]="!filterApprox()"
            [class.hover:bg-surface-2]="!filterApprox()"
            (click)="filterApprox.set(!filterApprox())"
          >approx <span class="tabular-nums text-ink-subtle">{{ quickCounts().approx }}</span></button>
          <button
            class="rounded px-2 py-1 text-2xs font-medium transition-colors flex items-center gap-1"
            [class.bg-accent]="filterHasTarget()"
            [class.text-accent-ink]="filterHasTarget()"
            [class.bg-surface-2]="!filterHasTarget()"
            [class.text-ink-muted]="!filterHasTarget()"
            [class.hover:bg-surface-2]="!filterHasTarget()"
            (click)="filterHasTarget.set(!filterHasTarget())"
          >has target <span class="tabular-nums text-ink-subtle">{{ quickCounts().hasTarget }}</span></button>
        </div>

        <!-- Table -->
        <div class="overflow-hidden rounded-md border border-line">
          <div class="max-h-[500px] overflow-y-auto">
            <table class="w-full text-left text-xs">
              <thead class="sticky top-0 z-10">
                <tr class="border-b border-line bg-surface-2 text-2xs font-semibold uppercase tracking-wider text-ink-muted">
                  <th class="px-3 py-2 w-16 cursor-pointer hover:text-ink select-none" (click)="toggleSort('method')">
                    Method {{ sortArrow('method') }}
                  </th>
                  <th class="px-3 py-2 cursor-pointer hover:text-ink select-none" (click)="toggleSort('route')">
                    Route {{ sortArrow('route') }}
                  </th>
                  <th class="px-3 py-2 cursor-pointer hover:text-ink select-none" (click)="toggleSort('target')">
                    Target {{ sortArrow('target') }}
                  </th>
                  <th class="px-3 py-2 w-28 cursor-pointer hover:text-ink select-none" (click)="toggleSort('kind')">
                    Kind {{ sortArrow('kind') }}
                  </th>
                  <th class="px-3 py-2 w-24"></th>
                </tr>
              </thead>
              <tbody class="divide-y divide-line">
                @for (entry of sortedEntries(); track entry.nodeId; let idx = $index) {
                  <tr
                    class="group cursor-pointer transition-colors hover:bg-surface-2"
                    [class.bg-accent/10]="selectedIndex() === idx"
                    (click)="selectEntry(entry, idx)"
                    (keydown)="onRowKey($event, entry)"
                    tabindex="0"
                    (focus)="selectedIndex.set(idx)"
                  >
                    <td class="px-3 py-1.5 w-16">
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
                    <td class="px-3 py-1.5 w-28">
                      <span class="text-2xs text-ink-subtle">{{ KIND_LABELS[entry.kind] ?? entry.kind }}</span>
                    </td>
                    <td class="px-3 py-1.5 w-24">
                      <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity">
                        <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="traceEntry(entry)" title="Trace">
                          <app-icon name="arrow-right" [size]="12" />
                        </button>
                        <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="openNodeCard(entry)" title="Node card">
                          <app-icon name="info" [size]="12" />
                        </button>
                        <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="copyRoute(entry)" title="Copy route">
                          <app-icon name="copy" [size]="12" />
                        </button>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          @if (!sortedEntries().length) {
            <p class="px-3 py-6 text-center text-xs text-ink-subtle">
              {{ session.ready() ? 'No entries match — clear filters' : 'Analyze a repo to list its entry points.' }}
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
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly filterApprox = signal(false);
  protected readonly filterHasTarget = signal(false);
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

  protected readonly kindCounts = computed<Record<string, number>>(() => {
    const counts: Record<string, number> = {};
    for (const g of this.session.entryGroups()) counts[g.kind] = g.entries.length;
    return counts;
  });

  protected readonly quickCounts = computed(() => {
    let approx = 0;
    let hasTarget = 0;
    for (const g of this.session.entryGroups()) {
      for (const e of g.entries) {
        if (e.provenance === 'Syntactic') approx++;
        if (e.target) hasTarget++;
      }
    }
    return { approx, hasTarget };
  });

  protected readonly filteredEntries = computed<EntryVm[]>(() => {
    const groups = this.session.entryGroups();
    const kf = this.kindFilter();
    const q = this.search().toLowerCase();
    const aprox = this.filterApprox();
    const hasTgt = this.filterHasTarget();

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
    if (aprox) entries = entries.filter((e) => e.provenance === 'Syntactic');
    if (hasTgt) entries = entries.filter((e) => e.target != null);

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
      const af = a.route ?? a.title;
      const bf = b.route ?? b.title;
      return af < bf ? -1 : 1;
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

  constructor() {
    this.route.queryParams.subscribe((params) => {
      const col = params['sort'];
      if (col && ['method', 'route', 'target', 'kind'].includes(col)) {
        this.sortColumn.set(col as SortColumn);
      }
      const dir = params['dir'];
      if (dir === 'asc' || dir === 'desc') {
        this.sortDir.set(dir);
      }
    });

    effect(() => {
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { sort: this.sortColumn(), dir: this.sortDir(), kind: this.kindFilter() ?? undefined, q: this.search() || undefined },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    });
  }

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

  protected onRowKey(e: KeyboardEvent, entry: EntryVm): void {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      this.traceEntry(entry);
    } else if (e.key === 'n' && !e.ctrlKey && !e.metaKey) {
      e.preventDefault();
      this.openNodeCard(entry);
    } else if ((e.ctrlKey || e.metaKey) && e.key === 'c') {
      e.preventDefault();
      this.copyRoute(entry);
    }
  }

  protected traceEntry(entry: EntryVm): void {
    const handle = this.session.handle();
    if (!handle) return;
    void this.trace.trace(handle, entry.focus);
  }

  protected openNodeCard(entry: EntryVm): void {
    void this.nodeStore.show(entry.nodeId);
  }

  protected copyRoute(entry: EntryVm): void {
    const text = entry.route || entry.title;
    navigator.clipboard?.writeText(text)
      .then(() => this.toast.show('Copied: ' + text, 'info'))
      .catch(() => this.toast.show('Copy failed', 'error'));
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
