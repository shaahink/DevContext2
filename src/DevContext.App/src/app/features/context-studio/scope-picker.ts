import { Component, computed, inject, input, model, output, signal } from '@angular/core';

import { middleEllipsis } from '../../core/format';
import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { KIND_COLORS, KIND_ICONS, KIND_LABELS } from '../../models/view-models';
import { Icon } from '../../ui/icon/icon';
import { Withheld, type WithheldReason } from '../../ui/withheld/withheld';
import { ToastService } from '../../ui/toast/toast';

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

/** T5.4 — "I'm changing this entry" seeds cards matched to the entry KIND: a hub method
 * wants its orchestrator spine + consumer wiring, a worker wants its loop + the config it
 * reads — not an endpoint-shaped validator card. Anchors exist since 202c593 (hub/worker
 * member anchors). Exported for the spec. */
export function presetSeedsFor(entry: EntryVm): ContextCardSeed[] {
  const entryIds = [entry.nodeId];
  const label = entry.route || entry.title;
  const kindLabel = KIND_LABELS[entry.kind] ?? entry.kind;

  switch (entry.kind) {
    case 'SignalRHub':
      return [
        { type: 'flow', title: `Hub method flow: ${label}`, entryIds, estimatedLines: 15 },
        { type: 'bodies', title: `Hub method + orchestrator bodies: ${entry.target || entry.title}`, entryIds, estimatedLines: 30 },
        { type: 'di_wiring', title: `Consumers and wiring: ${label}`, entryIds, estimatedLines: 12 },
        { type: 'contracts', title: `Messages (${kindLabel})`, entryIds, estimatedLines: 10 },
        { type: 'tests', title: `Tests for ${label}`, entryIds, estimatedLines: 15 },
      ];
    case 'HostedService':
    case 'ScheduledJob':
    case 'MessageConsumer':
      return [
        { type: 'flow', title: `Worker flow: ${label}`, entryIds, estimatedLines: 15 },
        { type: 'bodies', title: `Worker bodies: ${entry.target || entry.title}`, entryIds, estimatedLines: 30 },
        { type: 'config', title: `Config read by ${label}`, entryIds, estimatedLines: 10 },
        { type: 'contracts', title: `Messages (${kindLabel})`, entryIds, estimatedLines: 10 },
        { type: 'tests', title: `Tests for ${label}`, entryIds, estimatedLines: 15 },
      ];
    default:
      return [
        { type: 'flow', title: `Flow: ${label}`, entryIds, estimatedLines: 15 },
        { type: 'bodies', title: `Member bodies: ${entry.target || entry.title}`, entryIds, estimatedLines: 30 },
        { type: 'contracts', title: `Contracts (${kindLabel})`, entryIds, estimatedLines: 10 },
        { type: 'tests', title: `Validators for ${label}`, entryIds, estimatedLines: 10 },
        { type: 'tests', title: `Tests for ${label}`, entryIds, estimatedLines: 15 },
      ];
  }
}

/**
 * R3 C-3 — why the scope picker has nothing to offer.
 *
 * It used to have only one answer: "Analyze a repo to see its services and entries", shown whenever
 * the entry count was zero. On a library that repo HAS been analyzed — the sentence is false and the
 * instruction is one the reader already carried out. Zero entries is not no analysis.
 *
 * Exported for the spec, same as presetSeedsFor.
 */
export function scopePickerWithheld(analyzed: boolean, isLibrary: boolean): { reason: WithheldReason; text: string } {
  if (!analyzed) {
    return { reason: 'not-computed', text: 'Analyze a repo to see its services and entries.' };
  }
  return isLibrary
    ? { reason: 'archetype', text: 'No entry points — a library is scoped by its public surface, not by services. Pick types from the omnibox above.' }
    : { reason: 'archetype', text: 'No entry points were found in this repo, so there is nothing to group by service. Pick a type from the omnibox above.' };
}

@Component({
  selector: 'app-scope-picker',
  imports: [Icon, Withheld],
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

    <!-- D4.5 (L4): preset gets an explicit name + a one-line effect (the audit read
         "I'm changing this entry" as an edit action with an invisible outcome). -->
    <div class="border-b border-line px-2 py-1">
      <button
        type="button"
        class="flex w-full items-center justify-center gap-1 rounded px-2 py-1 text-xs text-accent hover:bg-accent/10 disabled:opacity-40 disabled:hover:bg-transparent transition-colors"
        [disabled]="totalEntryCount() === 0"
        [title]="totalEntryCount() === 0 ? pickerWithheld().text : 'Pick an entry; its kind decides the cards (a hub seeds consumer wiring, a worker its config)'"
        (click)="showPresetPicker.set(!showPresetPicker())"
      >
        <app-icon name="edit" [size]="14" />
        Change-impact pack
      </button>
      <p class="px-1 pb-0.5 text-center text-2xs text-ink-subtle">
        seeds flow &middot; bodies &middot; contracts &middot; tests for one entry, by kind
      </p>
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
            [title]="'Adds ' + presetEffect(entry)"
            (click)="applyPreset(entry); showPresetPicker.set(false)"
          >
            <app-icon [name]="kindIcon(entry.kind)" [size]="14" class="shrink-0" [style.color]="kindColor(entry.kind)" [title]="kindTitle(entry.kind)" />
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
              <!-- S10: selected state was bg-hover — the SAME class the hover rule sets, so a
                   picked row was indistinguishable from the one under the cursor. It now carries
                   the accent bar + tint the rest of the app uses for selection. -->
              <button
                type="button"
                class="flex w-full items-center gap-1.5 border-l-2 px-2 py-1 text-left text-xs hover:bg-hover transition-colors"
                [class.border-transparent]="!selectedEntries().has(entry.focus)"
                [class.border-accent]="selectedEntries().has(entry.focus)"
                [class.bg-accent/10]="selectedEntries().has(entry.focus)"
                [attr.aria-pressed]="selectedEntries().has(entry.focus)"
                (click)="toggleEntry(entry)"
              >
                @if (entry.httpMethod) {
                  <span class="w-8 shrink-0 text-2xs font-semibold" [class]="methodClass(entry.httpMethod)">{{ entry.httpMethod }}</span>
                }
                <!-- S10: middle-ellipsis, same rule as the entry deck (A-4). Plain CSS truncate
                     rendered eleven identical /api/catalog/i... rows in this 230px column. -->
                <span class="min-w-0 flex-1 truncate font-mono" [title]="entry.route ? entry.route + ' — ' + entry.title : entry.title">{{ shortLabel(entry) }}</span>
                <!-- T5.5 (finding 50) — the kind glyph says WHAT it is on hover; color alone
                     read as an error badge. -->
                <app-icon [name]="kindIcon(entry.kind)" [size]="14" class="shrink-0" [style.color]="kindColor(entry.kind)" [title]="kindTitle(entry.kind)" />
              </button>
            }
          </div>
        </details>
      } @empty {
        <!-- R3 C-3: this read "Analyze a repo to see its services and entries" on a repo that HAD
             been analyzed — zero entries was being reported as no analysis. On a library it is an
             instruction the reader has already carried out, and the sentence is false. -->
        @if (totalEntryCount() === 0) {
          <app-withheld [reason]="pickerWithheld().reason" [text]="pickerWithheld().text" />
        } @else {
          <div class="px-3 py-6 text-center text-xs text-ink-subtle">
            No matches for &ldquo;{{ filterText() }}&rdquo;.
          </div>
        }
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
  /** R3 C-3: whether a repo has been analyzed at all. Without it this component could only see
   * "zero entries" and reported it as "no analysis" — an instruction the reader had already
   * carried out, and false on every library. */
  readonly analyzed = input(false);
  readonly isLibrary = input(false);

  readonly cardsChange = output<readonly ContextCardSeed[]>();
  readonly trailSeedRequest = output<void>();
  readonly omniboxCard = output<ContextCardSeed>();

  readonly filterText = model('');

  protected readonly showPresetPicker = signal(false);

  protected readonly selectedEntries = signal<ReadonlySet<string>>(new Set());

  protected readonly totalEntryCount = computed(() =>
    this.entryGroups().reduce((n, g) => n + g.entries.length, 0),
  );

  protected readonly pickerWithheld = computed(() =>
    scopePickerWithheld(this.analyzed(), this.isLibrary()));

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

  private readonly toast = inject(ToastService);

  /** D4.5 (L4) — the preset's card-type list, deduped in seed order ("flow + bodies +
   * config + contracts + tests" for a worker). Shown pre-click (row tooltip) and
   * post-apply (the scope-delta toast). */
  protected presetEffect(entry: EntryVm): string {
    return [...new Set(presetSeedsFor(entry).map((s) => s.type))].join(' + ');
  }

  protected applyPreset(entry: EntryVm): void {
    const seeds = presetSeedsFor(entry);
    this.cardsChange.emit(seeds);
    this.showPresetPicker.set(false);
    // D4.5 (L4) — the visible scope delta: name what the preset just added.
    this.toast.show(`Preset added ${seeds.length} cards: ${this.presetEffect(entry)}`, 'success');
  }

  /** The picker column is narrower than the entry deck's, so the budget is smaller (A-4's rule:
   * the budget must sit under what the column can show or CSS truncate cuts first). A routed
   * entry keeps its tail, a named one (bus consumer, handler) keeps its head — see
   * `middleEllipsis`. */
  protected shortLabel(entry: EntryVm): string {
    return entry.route
      ? middleEllipsis(entry.route, 26, 'tail')
      : middleEllipsis(entry.title, 26, 'head');
  }

  protected kindIcon(kind: string): string {
    return KIND_ICONS[kind] ?? 'dot';
  }

  protected kindColor(kind: string): string {
    return KIND_COLORS[kind] ?? 'var(--vibe-ink-subtle)';
  }

  /** T5.5 (finding 50) — tooltip names the entry kind so a colored glyph can't read as an error. */
  protected kindTitle(kind: string): string {
    return `${KIND_LABELS[kind] ?? kind} entry`;
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
