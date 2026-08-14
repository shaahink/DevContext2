import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';

import { type EntryGroupVm, type EntryVm, KIND_LABELS } from '../../models/view-models';
import { copyToClipboard } from '../../core/clipboard';
import { edgeTier } from '../../core/format';
import { ToastService } from '../../ui/toast/toast';
import type { TableColumn } from './table-lens-columns';
import { WEB_COLUMNS, refreshSharedTargets } from './table-lens-columns';

type SortDir = 'asc' | 'desc';

/**
 * Table Lens v2 (M7.5) — data-dense, CDK-virtualized entry spreadsheet with
 * archetype-default columns, column picker, and CSV export. Replaces the old
 * audit-table overlay as the primary table view on Shift+E.
 *
 * Rows: entries from the analyzer. Columns: per-archetype defaults (WEB_COLUMNS
 * for the dogfood repo's microservices archetype). Future: relationship chips,
 * mini flow stepper row expand, touch-risk columns.
 */
@Component({
  selector: 'app-table-lens',
  imports: [ScrollingModule],
  template: `
    <div class="flex h-full min-h-0 flex-col bg-surface">
      <!-- Toolbar -->
      <div class="flex flex-wrap items-center gap-2 border-b border-line px-3 py-2">
        <input
          type="text"
          class="w-48 rounded border border-line bg-base px-2 py-1 text-xs text-ink outline-none placeholder:text-ink-subtle focus:border-accent"
          placeholder="Filter…"
          [value]="search()"
          (input)="search.set($any($event.target).value)"
        />
        <span class="h-4 w-px bg-line"></span>
        @for (kind of allKinds(); track kind) {
          <button type="button" class="chip" [class.active]="kindFilter() === kind" (click)="kindFilter.set(kindFilter() === kind ? null : kind)">
            {{ KIND_LABELS[kind] ?? kind }}
            <span class="tabular-nums text-ink-subtle">{{ kindCounts()[kind] }}</span>
          </button>
        }
        <span class="flex-1"></span>
        <span class="text-2xs tabular-nums text-ink-subtle">{{ subtitle() }}</span>
        <button type="button" class="chip" [class.active]="pickerOpen()" (click)="pickerOpen.set(!pickerOpen())" title="Column picker">
          Columns
        </button>
        <button type="button" class="chip" (click)="exportCSV()" title="Export as CSV">CSV</button>
        <button type="button" class="chip" (click)="dismissed.emit()" title="Close (Esc)">✕</button>
      </div>

      <!-- Column picker dropdown -->
      @if (pickerOpen()) {
        <div class="flex flex-wrap gap-1 border-b border-line bg-surface-2 px-3 py-2">
          @for (col of availableColumns; track col.key) {
            <button
              type="button"
              class="chip"
              [class.active]="isVisible(col.key)"
              (click)="toggleColumn(col.key)"
              (keydown.enter)="toggleColumn(col.key)"
              (keydown.space)="toggleColumn(col.key); $event.preventDefault()"
              [title]="col.tooltip"
            >
              {{ col.label }}
            </button>
          }
        </div>
      }

      <!-- Header row -->
      <div class="flex shrink-0 items-center border-b border-line bg-surface-2 px-3 py-1.5">
        @for (col of visibleColumns(); track col.key) {
          <button
            type="button"
            class="cursor-pointer select-none truncate bg-transparent text-left text-2xs font-semibold uppercase tracking-wider text-ink-muted hover:text-ink"
            [style.width.px]="col.width"
            [style.minWidth.px]="col.width"
            (click)="col.sortable ? toggleSort(col.key) : null"
            (keydown.enter)="col.sortable ? toggleSort(col.key) : null"
            (keydown.space)="onSortKey(col, $event)"
          >
            {{ col.label }} {{ sortArrow(col.key) }}
          </button>
        }
      </div>

      <!-- Virtualized body -->
      <cdk-virtual-scroll-viewport [itemSize]="28" class="flex-1 min-h-0" style="height: 100%">
        <div *cdkVirtualFor="let entry of sortedEntries(); trackBy: trackByNodeId" class="flex items-center border-b border-line/50 px-3 py-1 transition-colors hover:bg-hover">
          @for (col of visibleColumns(); track col.key) {
            <div
              class="truncate font-mono text-xs text-ink"
              [style.width.px]="col.width"
              [style.minWidth.px]="col.width"
              [title]="col.value(entry)"
            >
              @if (col.key === 'method' && entry.httpMethod) {
                <span class="chip shrink-0">{{ entry.httpMethod }}</span>
              } @else if (col.key === 'provenance' && edgeTier(entry.provenance) === 'approx') {
                <span class="chip text-warn">approx</span>
              } @else if (col.key === 'auth' && entry.authAttributes?.length) {
                <span class="text-accent">{{ col.value(entry) }}</span>
              } @else {
                {{ col.value(entry) }}
              }
            </div>
          }
        </div>
      </cdk-virtual-scroll-viewport>
    </div>
  `,
  host: { class: 'h-full min-h-0' },
})
export class TableLens {
  readonly groups = input<readonly EntryGroupVm[]>([]);
  readonly selectionChange = output<EntryVm>();
  readonly dismissed = output<void>();

  protected readonly KIND_LABELS = KIND_LABELS;
  /** V1.1 (#25) — the app's one reading of a wire resolution string (core/format). */
  protected readonly edgeTier = edgeTier;
  protected readonly availableColumns = WEB_COLUMNS;

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly sortColumn = signal<string | null>('route');
  protected readonly sortDir = signal<SortDir>('asc');
  protected readonly pickerOpen = signal(false);
  /** M7.5: Hidden column keys — persisted per archetype via localStorage. */
  protected readonly hiddenColumns = signal<readonly string[]>(this.loadHidden());

  private readonly toast = inject(ToastService);

  constructor() {
    effect(() => {
      const entries = this.groups().flatMap((g) => g.entries);
      refreshSharedTargets(entries);
    });
  }

  protected readonly visibleColumns = computed<readonly TableColumn[]>(() => {
    const hidden = new Set(this.hiddenColumns());
    return WEB_COLUMNS.filter((c) => !hidden.has(c.key));
  });

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

  protected readonly subtitle = computed(() => {
    const total = this.groups().reduce((n, g) => n + g.entries.length, 0);
    const filtered = this.sortedEntries().length;
    return filtered === total ? `${total} entries` : `${filtered} / ${total} entries`;
  });

  private readonly filteredEntries = computed<EntryVm[]>(() => {
    const kf = this.kindFilter();
    const q = this.search().trim().toLowerCase();
    let entries = (kf ? this.groups().filter((g) => g.kind === kf) : this.groups()).flatMap((g) => g.entries);
    if (q) {
      entries = entries.filter(
        (e) => e.title.toLowerCase().includes(q) || (e.route ?? '').toLowerCase().includes(q) || (e.target ?? '').toLowerCase().includes(q),
      );
    }
    return entries;
  });

  protected readonly sortedEntries = computed<EntryVm[]>(() => {
    const entries = [...this.filteredEntries()];
    const colKey = this.sortColumn();
    const dir = this.sortDir();
    if (!colKey) return entries;
    const col = WEB_COLUMNS.find((c) => c.key === colKey);
    if (!col) return entries;
    entries.sort((a, b) => {
      const av = col.value(a);
      const bv = col.value(b);
      if (av < bv) return dir === 'asc' ? -1 : 1;
      if (av > bv) return dir === 'asc' ? 1 : -1;
      return 0;
    });
    return entries;
  });

  protected toggleSort(key: string): void {
    if (this.sortColumn() === key) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(key);
      this.sortDir.set('asc');
    }
  }

  protected onSortKey(col: TableColumn, event: Event): void {
    event.preventDefault();
    if (col.sortable) this.toggleSort(col.key);
  }

  protected sortArrow(key: string): string {
    if (this.sortColumn() !== key) return '';
    return this.sortDir() === 'asc' ? '▲' : '▼';
  }

  protected isVisible(key: string): boolean {
    return !this.hiddenColumns().includes(key);
  }

  protected toggleColumn(key: string): void {
    this.hiddenColumns.update((h) => {
      const next = h.includes(key) ? h.filter((k) => k !== key) : [...h, key];
      this.saveHidden(next);
      return next;
    });
  }

  protected trackByNodeId(_: number, entry: EntryVm): string {
    return entry.nodeId;
  }

  /** M7.5: CSV export — downloads entries as comma-separated values. */
  protected exportCSV(): void {
    const cols = this.visibleColumns();
    const entries = this.sortedEntries();
    const header = cols.map((c) => quoteCSV(c.label)).join(',');
    const rows = entries.map((e) => cols.map((c) => quoteCSV(c.value(e))).join(','));
    const csv = [header, ...rows].join('\n');

    void copyToClipboard(csv)
      .then(() => this.toast.show(`CSV copied — ${entries.length} rows`, 'info'))
      .catch(() => {
        // Fallback: download as file
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'devcontext-entries.csv';
        a.click();
        URL.revokeObjectURL(url);
        this.toast.show(`Downloaded ${entries.length} rows`, 'info');
      });
  }

  private loadHidden(): readonly string[] {
    try {
      const raw = localStorage.getItem('devcontext-table-hidden-columns');
      return raw ? JSON.parse(raw) as string[] : [];
    } catch {
      return [];
    }
  }

  private saveHidden(columns: readonly string[]): void {
    try { localStorage.setItem('devcontext-table-hidden-columns', JSON.stringify(columns)); } catch { /* ignore */ }
  }
}

function quoteCSV(value: string): string {
  if (value.includes(',') || value.includes('"') || value.includes('\n')) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}
