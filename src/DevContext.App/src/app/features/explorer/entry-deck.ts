import { Component, computed, ElementRef, input, output, signal, viewChild } from '@angular/core';

import { type EntryGroupVm, type EntryVm, KIND_ICONS, KIND_LABELS } from '../../models/view-models';
import { Icon } from '../../ui/icon/icon';

interface KindStat {
  readonly kind: string;
  readonly label: string;
  readonly count: number;
}

/**
 * Entry Deck (F proposal §2) — the left column of the Workbench. A flat keyboard-first
 * listbox, NOT a table: j/k (or arrows) scrub the selection, `/` focuses the filter,
 * Enter re-emits the current row, Shift+E asks the parent for the full audit table.
 * The parent owns what selection MEANS (debounced trace + trail push) — the deck only
 * moves a cursor.
 */
@Component({
  selector: 'app-entry-deck',
  imports: [Icon],
  host: {
    class: 'panel flex h-full min-h-0 flex-col outline-none',
    tabindex: '0',
    '(keydown)': 'onKey($event)',
  },
  template: `
    @if (projectFilter()) {
      <div class="flex items-center gap-1.5 border-b border-line bg-hover px-2 py-1 text-2xs text-ink-muted">
        <span>Project</span>
        <span class="chip active font-mono">{{ projectFilter() }}</span>
        <button type="button" class="ml-auto text-ink-subtle hover:text-ink" (click)="projectFilterCleared.emit()" title="Clear project filter">
          ✕
        </button>
      </div>
    }
    <div class="flex items-center gap-1 border-b border-line px-2 py-1">
      <app-icon name="search" [size]="12" class="shrink-0 text-ink-subtle" />
      <input
        #filterBox
        type="text"
        class="w-full min-w-0 bg-transparent text-xs text-ink placeholder:text-ink-subtle focus:outline-none"
        placeholder="Filter entries…  /"
        [value]="filterText()"
        (input)="filterText.set(filterBox.value)"
        (keydown.escape)="clearFilter(); filterBox.blur()"
      />
      <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ flat().length }}</span>
    </div>

    @if (kindStats().length > 1) {
      <div class="flex flex-wrap gap-1 border-b border-line px-2 py-1">
        @for (stat of kindStats(); track stat.kind) {
          <button
            type="button"
            class="chip"
            [class.active]="activeKind() === stat.kind"
            (click)="toggleKind(stat.kind)"
          >
            {{ stat.label }} <span class="tabular-nums">{{ stat.count }}</span>
          </button>
        }
      </div>
    }

    <div class="min-h-0 flex-1 overflow-y-auto" role="listbox">
      @for (entry of flat(); track entry.focus; let i = $index) {
        <div
          class="list-row"
          role="option"
          tabindex="0"
          [class.selected]="entry.focus === selectedFocus()"
          [attr.aria-selected]="entry.focus === selectedFocus()"
          (click)="select(i)"
          (keydown.enter)="select(i)"
          (keydown.space)="select(i); $event.preventDefault()"
        >
          @if (entry.httpMethod) {
            <span class="w-9 shrink-0 text-2xs font-semibold" [class]="methodClass(entry.httpMethod)">
              {{ entry.httpMethod }}
            </span>
          }
          <span class="min-w-0 flex-1 truncate font-mono text-xs" [title]="entry.title">
            {{ entry.route || entry.title }}
          </span>
          @if (!entry.target) {
            <span class="shrink-0 text-2xs text-warn" title="Unwired: no resolved target">○</span>
          }
          <app-icon [name]="kindIcon(entry.kind)" [size]="12" class="shrink-0 text-ink-subtle" />
        </div>
      } @empty {
        <div class="px-3 py-6 text-center text-xs text-ink-subtle">
          @if (totalCount() === 0) {
            Analyze a repo to list its entry points.
          } @else if (filterText()) {
            No entries match “{{ filterText() }}” —
            <button type="button" class="text-accent hover:underline" (click)="clearFilter()">
              clear filters
            </button>
          } @else {
            No entries in this project —
            <button type="button" class="text-accent hover:underline" (click)="clearFilter(); projectFilterCleared.emit()">
              clear filters
            </button>
          }
        </div>
      }
    </div>

    <div class="flex items-center justify-between border-t border-line px-2 py-0.5 text-2xs text-ink-subtle">
      <span><span class="kbd">j</span> <span class="kbd">k</span> scrub</span>
      <button type="button" class="hover:text-ink" (click)="openAudit.emit()">
        <span class="kbd">Shift+E</span> table
      </button>
    </div>
  `,
})
export class EntryDeck {
  readonly groups = input<readonly EntryGroupVm[]>([]);
  readonly selectedFocus = input<string | null>(null);
  /** Set by the Stage's System altitude (click a project -> filter, proposal §2). */
  readonly projectFilter = input<string | null>(null);

  readonly selectionChange = output<EntryVm>();
  readonly openAudit = output<void>();
  readonly projectFilterCleared = output<void>();

  protected readonly filterText = signal('');
  /** Single-select kind chip; null = all kinds. */
  protected readonly activeKind = signal<string | null>(null);

  private readonly filterBox = viewChild.required<ElementRef<HTMLInputElement>>('filterBox');

  protected readonly totalCount = computed(() =>
    this.groups().reduce((n, g) => n + g.entries.length, 0),
  );

  protected readonly kindStats = computed<readonly KindStat[]>(() =>
    this.groups()
      .filter((g) => g.entries.length > 0)
      .map((g) => ({ kind: g.kind, label: KIND_LABELS[g.kind] ?? g.kind, count: g.entries.length })),
  );

  protected readonly flat = computed<readonly EntryVm[]>(() => {
    const kind = this.activeKind();
    const project = this.projectFilter();
    const query = this.filterText().trim().toLowerCase();
    return this.groups()
      .filter((g) => kind === null || g.kind === kind)
      .flatMap((g) => g.entries)
      .filter((e) => project === null || e.project === project)
      .filter(
        (e) =>
          query === '' ||
          e.title.toLowerCase().includes(query) ||
          (e.route ?? '').toLowerCase().includes(query),
      );
  });

  private readonly selectedIndex = computed(() =>
    this.flat().findIndex((e) => e.focus === this.selectedFocus()),
  );

  protected onKey(event: KeyboardEvent): void {
    // Keys that must work while the filter input has focus:
    if (event.key === 'Escape') return; // handled on the input itself
    const inFilter = event.target === this.filterBox().nativeElement;

    switch (event.key) {
      case 'ArrowDown':
        this.move(1, event);
        return;
      case 'ArrowUp':
        this.move(-1, event);
        return;
    }
    if (inFilter) return; // j/k etc. type into the filter, not navigate

    switch (event.key) {
      case 'j':
        this.move(1, event);
        break;
      case 'k':
        this.move(-1, event);
        break;
      case 'Home':
        this.moveTo(0, event);
        break;
      case 'End':
        this.moveTo(this.flat().length - 1, event);
        break;
      case '/':
        event.preventDefault();
        this.filterBox().nativeElement.focus();
        break;
      case 'Enter': {
        const current = this.flat()[this.selectedIndex()];
        if (current) this.selectionChange.emit(current);
        break;
      }
      case 'E':
        if (event.shiftKey) {
          event.preventDefault();
          this.openAudit.emit();
        }
        break;
    }
  }

  protected select(index: number): void {
    const entry = this.flat()[index];
    if (entry) this.selectionChange.emit(entry);
  }

  protected toggleKind(kind: string): void {
    this.activeKind.update((k) => (k === kind ? null : kind));
  }

  protected clearFilter(): void {
    this.filterText.set('');
    this.activeKind.set(null);
  }

  protected kindIcon(kind: string): string {
    return KIND_ICONS[kind] ?? 'dot';
  }

  protected methodClass(method: string): string {
    switch (method.toUpperCase()) {
      case 'GET':
        return 'text-info';
      case 'POST':
        return 'text-success';
      case 'PUT':
      case 'PATCH':
        return 'text-warn';
      case 'DELETE':
        return 'text-danger';
      default:
        return 'text-ink-muted';
    }
  }

  private move(delta: number, event: KeyboardEvent): void {
    event.preventDefault();
    const list = this.flat();
    if (list.length === 0) return;
    const current = this.selectedIndex();
    const next = current === -1 ? 0 : Math.min(list.length - 1, Math.max(0, current + delta));
    if (next !== current) this.selectionChange.emit(list[next]);
  }

  private moveTo(index: number, event: KeyboardEvent): void {
    event.preventDefault();
    const entry = this.flat()[index];
    if (entry && index !== this.selectedIndex()) this.selectionChange.emit(entry);
  }
}
