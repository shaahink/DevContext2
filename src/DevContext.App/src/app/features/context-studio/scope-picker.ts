import { Component, computed, input, model, output, signal } from '@angular/core';

import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { KIND_COLORS, KIND_ICONS, KIND_LABELS } from '../../models/view-models';
import { Icon } from '../../ui/icon/icon';

export interface ServiceGroup {
  readonly project: string;
  readonly entries: readonly EntryVm[];
}

export type ContextCardType = 'flow' | 'signatures' | 'bodies' | 'di_wiring' | 'config' | 'entities' | 'contracts' | 'tests' | 'identity';
export type ContextIntent = 'trace' | 'explain' | 'review';
/** T5.3 (audit R8) — json is the structured export: cards/sections/provenance/verification. */
export type OutputFormat = 'markdown' | 'plain' | 'json';

export interface ContextCardSeed {
  readonly type: ContextCardType;
  readonly title: string;
  readonly entryIds: string[];
  readonly estimatedLines: number;
}

@Component({
  selector: 'app-scope-picker',
  imports: [Icon],
  host: { class: 'flex h-full min-h-0 flex-col' },
  template: `
    <div class="relative">
      <div class="flex items-center gap-1 border-b border-line px-2 py-1.5">
        <app-icon name="search" [size]="14" class="shrink-0 text-ink-subtle" />
        <input
          #searchBox
          type="text"
          class="w-full min-w-0 bg-transparent text-xs text-ink placeholder:text-ink-subtle focus:outline-none"
          placeholder="Filter services and entries…"
          [value]="filterText()"
          (input)="filterText.set(searchBox.value)"
          (focus)="omniboxOpen.set(true)"
          (blur)="delayedCloseOmnibox()"
          (keydown.escape)="closeOmnibox()"
        />
        @if (filterText()) {
          <button type="button" class="shrink-0 text-ink-subtle hover:text-ink" (click)="filterText.set(''); searchBox.focus()" title="Clear">
            <app-icon name="x" [size]="12" />
          </button>
        }
      </div>

      @if (omniboxOpen() && filterText()) {
        <div class="absolute left-0 right-0 top-full z-10 max-h-60 overflow-y-auto border-x border-b border-line bg-surface shadow-lg">
          @for (item of omniboxResults(); track item.title) {
            <button
              type="button"
              class="flex w-full items-center gap-2 px-2 py-1 text-left text-xs hover:bg-hover transition-colors"
              (mousedown)="addOmniboxItem(item); closeOmnibox()"
            >
              <app-icon [name]="kindIcon(item.kind)" [size]="14" class="shrink-0" [style.color]="kindColor(item.kind)" />
              <span class="min-w-0 flex-1 truncate font-mono">{{ item.title }}</span>
              <span class="shrink-0 rounded px-1 py-0.5 text-2xs text-ink-subtle bg-hover">{{ item.kindLabel }}</span>
            </button>
          } @empty {
            <div class="px-2 py-3 text-center text-xs text-ink-subtle">No matches.</div>
          }
        </div>
      }
    </div>

    <div class="flex items-center gap-1 border-b border-line px-2 py-1">
      <button
        type="button"
        class="flex flex-1 items-center justify-center gap-1 rounded px-2 py-1 text-xs text-accent hover:bg-accent/10 disabled:opacity-40 disabled:hover:bg-transparent transition-colors"
        [disabled]="totalEntryCount() === 0"
        [title]="totalEntryCount() === 0 ? 'Analyze a repo first — no entries to seed from' : 'Seeds 5 context cards for the endpoint you pick'"
        (click)="showPresetPicker.set(!showPresetPicker())"
      >
        <app-icon name="edit" [size]="14" />
        I&rsquo;m changing this endpoint
      </button>
    </div>

    <div class="flex items-center gap-1 border-b border-line px-2 py-1">
      <button
        type="button"
        class="flex flex-1 items-center justify-center gap-1 rounded px-2 py-1 text-xs text-ink-subtle hover:bg-hover hover:text-ink transition-colors"
        title="Seed cards from current explore trail"
        (click)="trailSeedRequest.emit()"
      >
        <app-icon name="history" [size]="14" />
        From current trail
      </button>
    </div>

    @if (showPresetPicker()) {
      <div class="max-h-60 overflow-y-auto border-b border-line">
        @for (entry of allEntries(); track entry.focus) {
          <button
            type="button"
            class="flex w-full items-center gap-2 px-2 py-1 text-left text-xs hover:bg-hover transition-colors"
            (click)="applyPreset(entry); showPresetPicker.set(false)"
          >
            <app-icon [name]="kindIcon(entry.kind)" [size]="14" class="shrink-0" [style.color]="kindColor(entry.kind)" />
            <span class="min-w-0 flex-1 truncate font-mono">{{ entry.route || entry.title }}</span>
            <span class="shrink-0 text-2xs text-ink-subtle">{{ entry.project }}</span>
          </button>
        } @empty {
          <div class="px-2 py-3 text-center text-xs text-ink-subtle">No entries available.</div>
        }
      </div>
    }

    <div class="min-h-0 flex-1 overflow-y-auto">
      @for (svc of filteredServices(); track svc.project; let last = $last) {
        <details class="group" open>
          <summary
            class="flex cursor-pointer items-center gap-1.5 px-2 py-1 text-xs font-medium text-ink-muted hover:text-ink hover:bg-hover/50 transition-colors"
          >
            <app-icon name="chevron-right" [size]="10" class="shrink-0 transition-transform group-open:rotate-90" />
            <app-icon name="box" [size]="14" class="shrink-0 text-ink-subtle" />
            <span class="min-w-0 flex-1 truncate">{{ svc.project }}</span>
            <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ svc.entries.length }}</span>
          </summary>
          <div class="pl-6">
            @for (entry of svc.entries; track entry.focus) {
              <button
                type="button"
                class="flex w-full items-center gap-1.5 px-2 py-1 text-left text-xs hover:bg-hover transition-colors"
                [class.bg-hover]="selectedEntries().has(entry.focus)"
                (click)="toggleEntry(entry)"
              >
                @if (entry.httpMethod) {
                  <span class="w-8 shrink-0 text-2xs font-semibold" [class]="methodClass(entry.httpMethod)">{{ entry.httpMethod }}</span>
                }
                <span class="min-w-0 flex-1 truncate font-mono">{{ entry.route || entry.title }}</span>
                <app-icon [name]="kindIcon(entry.kind)" [size]="14" class="shrink-0" [style.color]="kindColor(entry.kind)" />
              </button>
            }
          </div>
        </details>
      } @empty {
        <div class="px-3 py-6 text-center text-xs text-ink-subtle">
          @if (totalEntryCount() === 0) {
            Analyze a repo to see its services and entries.
          } @else {
            No matches for &ldquo;{{ filterText() }}&rdquo;.
          }
        </div>
      }
    </div>

    <div class="flex items-center gap-2 border-t border-line px-2 py-1">
      <span class="text-2xs text-ink-subtle">
        {{ selectedEntries().size }} of {{ totalEntryCount() }} selected
      </span>
      <button
        type="button"
        class="ml-auto rounded px-2 py-0.5 text-xs font-medium transition-colors disabled:opacity-30"
        [class.bg-accent]="selectedEntries().size > 0"
        [class.text-accent-ink]="selectedEntries().size > 0"
        [class.hover:bg-accent/90]="selectedEntries().size > 0"
        [class.text-accent]="selectedEntries().size === 0"
        [disabled]="selectedEntries().size === 0"
        data-testid="add-to-context"
        (click)="addSelected()"
      >
        Add{{ selectedEntries().size > 0 ? ' ' + selectedEntries().size : '' }} to context
      </button>
    </div>
  `,
})
export class ScopePicker {
  readonly entryGroups = input<readonly EntryGroupVm[]>([]);

  readonly cardsChange = output<readonly ContextCardSeed[]>();
  readonly trailSeedRequest = output<void>();
  readonly omniboxCard = output<ContextCardSeed>();

  readonly filterText = model('');

  protected readonly showPresetPicker = signal(false);

  protected readonly selectedEntries = signal<ReadonlySet<string>>(new Set());

  protected readonly totalEntryCount = computed(() =>
    this.entryGroups().reduce((n, g) => n + g.entries.length, 0),
  );

  protected readonly allEntries = computed<readonly EntryVm[]>(() =>
    this.entryGroups().flatMap((g) => g.entries),
  );

  protected readonly filteredServices = computed<readonly ServiceGroup[]>(() => {
    const query = this.filterText().trim().toLowerCase();
    const all = this.entryGroups().flatMap((g) => g.entries);
    const byProject = new Map<string, EntryVm[]>();
    for (const e of all) {
      const key = e.project || 'Default';
      let list = byProject.get(key);
      if (!list) {
        list = [];
        byProject.set(key, list);
      }
      list.push(e);
    }
    if (query === '') return [...byProject.entries()].map(([project, entries]) => ({ project, entries }));
    return [...byProject.entries()]
      .filter(([project, entries]) =>
        project.toLowerCase().includes(query) ||
        entries.some((e) =>
          e.title.toLowerCase().includes(query) ||
          (e.route ?? '').toLowerCase().includes(query),
        ),
      )
      .map(([project, entries]) => ({
        project,
        entries: entries.filter((e) =>
          e.title.toLowerCase().includes(query) ||
          (e.route ?? '').toLowerCase().includes(query) ||
          project.toLowerCase().includes(query),
        ),
      }));
  });

  protected toggleEntry(entry: EntryVm): void {
    this.selectedEntries.update((set) => {
      const next = new Set(set);
      if (next.has(entry.focus)) {
        next.delete(entry.focus);
      } else {
        next.add(entry.focus);
      }
      return next;
    });
  }

  protected addSelected(): void {
    const entries = this.allEntries().filter((e) => this.selectedEntries().has(e.focus));
    if (entries.length === 0) return;
    const entryIds = entries.map((e) => e.nodeId);
    const cards: ContextCardSeed[] = [
      { type: 'flow', title: `Flow for ${entries.length} endpoint${entries.length > 1 ? 's' : ''}`, entryIds, estimatedLines: entries.length * 15 },
      { type: 'signatures', title: `Member signatures for ${entries.length} endpoint${entries.length > 1 ? 's' : ''}`, entryIds, estimatedLines: entries.length * 25 },
      { type: 'bodies', title: `Member bodies for ${entries.length} endpoint${entries.length > 1 ? 's' : ''}`, entryIds, estimatedLines: entries.length * 60 },
      { type: 'di_wiring', title: 'DI wiring', entryIds, estimatedLines: 15 },
      { type: 'config', title: 'Configuration keys', entryIds, estimatedLines: 10 },
      { type: 'entities', title: 'Entities', entryIds, estimatedLines: 20 },
      { type: 'contracts', title: 'Contracts and interfaces', entryIds, estimatedLines: 15 },
      { type: 'tests', title: 'Tests', entryIds, estimatedLines: 15 },
      { type: 'identity', title: 'Repo identity', entryIds, estimatedLines: 8 },
    ];
    this.cardsChange.emit(cards);
    this.selectedEntries.set(new Set());
  }

  protected applyPreset(entry: EntryVm): void {
    const entryIds = [entry.nodeId];
    const kindLabel = KIND_LABELS[entry.kind] ?? entry.kind;
    const cards: ContextCardSeed[] = [
      { type: 'flow', title: `Flow: ${entry.route || entry.title}`, entryIds, estimatedLines: 15 },
      { type: 'bodies', title: `Member bodies: ${entry.target || entry.title}`, entryIds, estimatedLines: 30 },
      { type: 'contracts', title: `Contracts (${kindLabel})`, entryIds, estimatedLines: 10 },
      { type: 'tests', title: `Validators for ${entry.route || entry.title}`, entryIds, estimatedLines: 10 },
      { type: 'tests', title: `Tests for ${entry.route || entry.title}`, entryIds, estimatedLines: 15 },
    ];
    this.cardsChange.emit(cards);
    this.showPresetPicker.set(false);
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

  protected readonly omniboxOpen = signal(false);
  private closeTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly omniboxResults = computed<readonly { title: string; kind: string; kindLabel: string; entry: EntryVm }[]>(() => {
    const q = this.filterText().trim().toLowerCase();
    if (!q) return [];
    const results: { title: string; kind: string; kindLabel: string; entry: EntryVm }[] = [];
    for (const group of this.entryGroups()) {
      for (const entry of group.entries) {
        if (entry.title.toLowerCase().includes(q) || (entry.route ?? '').toLowerCase().includes(q) || (entry.target ?? '').toLowerCase().includes(q)) {
          results.push({
            title: entry.route || entry.title,
            kind: entry.kind,
            kindLabel: KIND_LABELS[entry.kind] ?? entry.kind,
            entry,
          });
        }
      }
    }
    return results.slice(0, 15);
  });

  protected delayedCloseOmnibox(): void {
    this.closeTimer = setTimeout(() => this.omniboxOpen.set(false), 150);
  }

  protected closeOmnibox(): void {
    if (this.closeTimer) clearTimeout(this.closeTimer);
    this.omniboxOpen.set(false);
  }

  protected addOmniboxItem(item: { entry: EntryVm }): void {
    const entry = item.entry;
    const seed: ContextCardSeed = {
      type: 'flow',
      title: `Flow: ${entry.route || entry.title}`,
      entryIds: [entry.nodeId],
      estimatedLines: 15,
    };
    this.omniboxCard.emit(seed);
  }
}
