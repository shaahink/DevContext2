import { AfterViewInit, Component, computed, input, model, output, viewChild, ElementRef } from '@angular/core';

import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { KIND_COLORS, KIND_ICONS, KIND_LABELS } from '../../models/view-models';
import { Icon } from '../../ui/icon/icon';
import { groupForBrowser } from './entry-browser.vm';

/**
 * D4.5 (L5) — the entry BROWSER: the primary "all entries" surface. Grouped
 * service → kind → route, ranked (wired-first, then flow score), filter-as-you-type,
 * kind chips, auth badges. Replaces the raw audit table as the default affordance;
 * the table stays reachable as the Shift+E power view (CSV export, column picker).
 */
@Component({
  selector: 'app-entry-browser',
  imports: [Icon],
  host: { class: 'flex h-full min-h-0 flex-col bg-base' },
  template: `
    <header class="flex shrink-0 items-center gap-3 border-b border-line px-4 py-2.5">
      <h2 class="text-sm font-semibold text-ink">Entries</h2>
      <span class="text-2xs text-ink-subtle tabular-nums">
        {{ shownCount() }} of {{ totalCount() }} · {{ serviceGroups().length }} services
      </span>
      <div class="ml-auto flex items-center gap-2">
        <button type="button" class="text-2xs text-ink-subtle hover:text-ink" (click)="tableRequest.emit()">
          Raw table <span class="kbd">Shift+E</span>
        </button>
        <button type="button" class="text-ink-subtle hover:text-ink" (click)="dismissed.emit()" title="Close (Esc)">
          <app-icon name="x" [size]="16" />
        </button>
      </div>
    </header>

    <div class="flex shrink-0 flex-wrap items-center gap-1.5 border-b border-line px-4 py-2">
      <input
        #filterBox
        type="text"
        class="w-64 rounded-md border border-line bg-surface px-2 py-1 text-xs text-ink placeholder:text-ink-subtle focus:border-accent focus:outline-none"
        placeholder="Filter routes, targets, services…"
        [value]="filterText()"
        (input)="filterText.set(filterBox.value)"
        (keydown.escape)="filterText() ? filterText.set('') : dismissed.emit()"
      />
      <button
        type="button"
        class="chip text-2xs"
        [class.active]="activeKind() === null"
        (click)="activeKind.set(null)"
      >All</button>
      @for (stat of kindStats(); track stat.kind) {
        <button
          type="button"
          class="chip text-2xs"
          [class.active]="activeKind() === stat.kind"
          (click)="activeKind.set(activeKind() === stat.kind ? null : stat.kind)"
        >{{ stat.label }} <span class="tabular-nums text-ink-subtle">{{ stat.count }}</span></button>
      }
    </div>

    <div class="min-h-0 flex-1 overflow-y-auto px-4 py-3">
      @for (svc of serviceGroups(); track svc.service) {
        <section class="mb-4">
          <h3 class="mb-1 flex items-baseline gap-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">
            {{ svc.service }}
            <span class="font-normal normal-case tabular-nums">{{ svc.total }}</span>
          </h3>
          @for (kg of svc.kinds; track kg.kind) {
            <div class="mb-1.5">
              @if (svc.kinds.length > 1 || activeKind() === null) {
                <p class="mb-0.5 flex items-center gap-1 pl-1 text-2xs text-ink-subtle">
                  <app-icon [name]="kindIcon(kg.kind)" [size]="11" [style.color]="kindColor(kg.kind)" />
                  {{ kg.label }}
                </p>
              }
              <ul>
                @for (e of kg.entries; track e.nodeId) {
                  <li>
                    <button
                      type="button"
                      class="list-row flex w-full items-center gap-2 px-2 py-1 text-left text-xs"
                      (click)="pick(e)"
                    >
                      @if (e.httpMethod) {
                        <span class="w-11 shrink-0 font-mono text-2xs font-semibold" [class]="methodClass(e.httpMethod)">{{ e.httpMethod }}</span>
                      }
                      <span class="min-w-0 flex-1 truncate font-mono text-ink">{{ e.route || e.title }}</span>
                      @if (e.authAttributes?.length) {
                        <app-icon name="lock" [size]="11" class="shrink-0 text-warn" [title]="e.authAttributes!.join(', ')" />
                      }
                      @if (e.target) {
                        <span class="max-w-64 shrink-0 truncate text-2xs text-ink-subtle">→ {{ e.target }}</span>
                      } @else {
                        <span class="shrink-0 text-2xs text-ink-subtle" title="No resolved target">unwired</span>
                      }
                    </button>
                  </li>
                }
              </ul>
            </div>
          }
        </section>
      } @empty {
        <p class="py-8 text-center text-xs text-ink-subtle">
          @if (filterText().trim() || activeKind() !== null) {
            Nothing matches — clear the filter or kind chip.
          } @else {
            No entries in this session.
          }
        </p>
      }
    </div>
  `,
})
export class EntryBrowser implements AfterViewInit {
  readonly groups = input<readonly EntryGroupVm[]>([]);

  readonly selectionChange = output<EntryVm>();
  readonly dismissed = output<void>();
  /** Power-view escape hatch — the raw audit table (CSV export, column picker). */
  readonly tableRequest = output<void>();

  readonly filterText = model('');
  readonly activeKind = model<string | null>(null);

  private readonly filterBox = viewChild.required<ElementRef<HTMLInputElement>>('filterBox');

  ngAfterViewInit(): void {
    this.filterBox().nativeElement.focus();
  }

  protected readonly serviceGroups = computed(() =>
    groupForBrowser(this.groups(), this.filterText(), this.activeKind()),
  );

  protected readonly totalCount = computed(() =>
    this.groups().reduce((n, g) => n + g.entries.length, 0),
  );

  protected readonly shownCount = computed(() =>
    this.serviceGroups().reduce((n, s) => n + s.total, 0),
  );

  protected readonly kindStats = computed(() =>
    this.groups()
      .filter((g) => g.entries.length > 0)
      .map((g) => ({ kind: g.kind, label: KIND_LABELS[g.kind] ?? g.kind, count: g.entries.length })),
  );

  protected pick(entry: EntryVm): void {
    this.selectionChange.emit(entry);
    this.dismissed.emit();
  }

  protected kindIcon(kind: string): string {
    return KIND_ICONS[kind] ?? 'dot';
  }

  protected kindColor(kind: string): string {
    return KIND_COLORS[kind] ?? 'var(--vibe-ink-subtle)';
  }

  protected methodClass(method: string): string {
    switch (method.toUpperCase()) {
      case 'GET': return 'text-info';
      case 'POST': return 'text-success';
      case 'PUT': case 'PATCH': return 'text-warn';
      case 'DELETE': return 'text-danger';
      default: return 'text-ink-muted';
    }
  }
}
