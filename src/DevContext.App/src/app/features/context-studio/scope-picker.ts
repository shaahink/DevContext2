import { Component, computed, inject, input, model, output, signal } from '@angular/core';

import { middleEllipsis } from '../../core/format';
import { filterGroups, type LibrarySurfaceVm, type SurfaceGroupVm } from '../library/library-surface.vm';
import type { EntryGroupVm, EntryVm } from '../../models/view-models';
import { KIND_COLORS, KIND_ICONS, KIND_LABELS } from '../../models/view-models';
import { Icon } from '../../ui/icon/icon';
import { Withheld, type WithheldReason } from '../../ui/withheld/withheld';
import { ToastService } from '../../ui/toast/toast';

export interface ServiceGroup {
  readonly project: string;
  readonly entries: readonly EntryVm[];
}

/** N2.1 (audit §3.C / owner decision 2) — `usage` is the inbound direction of a symbol-rooted
 * pack ("who calls this"). The engine has built the section for every symbol root since G1.2;
 * no card type could pick it until now (audit §3.F.15). */
export type ContextCardType = 'flow' | 'signatures' | 'bodies' | 'di_wiring' | 'config' | 'entities' | 'contracts' | 'tests' | 'identity' | 'usage';
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
 * N2.1 — and the replacement was a half-fix (audit §3.F.8): it pointed at an omnibox that searched
 * ENTRIES ONLY, so on the repo where it appears — the one with no entries — it could not comply.
 * It now names the Types tab, which is a real place with the public surface in it.
 *
 * Exported for the spec, same as presetSeedsFor.
 */
export function scopePickerWithheld(analyzed: boolean, isLibrary: boolean): { reason: WithheldReason; text: string } {
  if (!analyzed) {
    return { reason: 'not-computed', text: 'Analyze a repo to see its services and entries.' };
  }
  return isLibrary
    ? { reason: 'archetype', text: 'No entry points — a library is scoped by its public surface, not by services. Its types are in the Types tab above.' }
    : { reason: 'archetype', text: 'No entry points were found in this repo, so there is nothing to group by service. Scope this pack from the Types tab above.' };
}

/** N2.1 — the ONE label for a surface type, used by the row, the omnibox and the card titles so
 * three places can't spell the same type three ways. Namespace-qualified, because a library's
 * short names collide across namespaces far more often than a repo's entries do. */
export function typeFocus(namespace: string, name: string): string {
  return namespace ? `${namespace}.${name}` : name;
}

/**
 * N2.1 (owner decision 2) — the card set for a TYPE-scoped pack. It is deliberately not the entry
 * set: a type has no route, no DI registration of its own and no config it reads, but it does have
 * an inbound direction the entry set never needed. `usage` is the card that makes a library
 * answerable ("who calls AbstractValidator.RuleFor") — the question D-G was opened for.
 *
 * Exported for the spec.
 */
export function typeCardSeeds(focuses: readonly string[], label: string): ContextCardSeed[] {
  const entryIds = [...focuses];
  const n = focuses.length;
  return [
    { type: 'signatures', title: `Members of ${label}`, entryIds, estimatedLines: n * 25 },
    { type: 'usage', title: `Who uses ${label}`, entryIds, estimatedLines: n * 15 },
    { type: 'bodies', title: `Member bodies: ${label}`, entryIds, estimatedLines: n * 60 },
    { type: 'flow', title: `Flow from ${label}`, entryIds, estimatedLines: n * 15 },
    { type: 'contracts', title: `Contracts around ${label}`, entryIds, estimatedLines: 15 },
    { type: 'identity', title: 'Repo identity', entryIds, estimatedLines: 8 },
  ];
}

/**
 * D-G row identity (audit §3.C) — what a picker row must say to be a different row.
 *
 * MEASURED on eShop before this existed: the five `OrderStatusChangedTo*IntegrationEventHandler`
 * consumers rendered as five identical strings. `middleEllipsis(title, 26, 'head')` keeps the first
 * 20 characters and the last 5, and those handlers share `OrderStatusChangedTo` (20) and `ndler`
 * (5) exactly — S10's head bias fixed the tail-collision and left this one standing. The label
 * alone cannot carry the identity, so the row carries the other two things the entry knows: what it
 * DISPATCHES TO (the target member) and which project it lives in.
 */
export function entryRowIdentity(entry: EntryVm, budget = 26): {
  primary: string;
  secondary: string | null;
  tooltip: string;
} {
  const primary = entry.route
    ? middleEllipsis(entry.route, budget, 'tail')
    : middleEllipsis(entry.title, budget, 'head');
  // The target is the distinguishing fact for a consumer/handler (the five eShop rows differ only
  // there); for a routed entry it is the action the route lands on. Never repeat the primary.
  const target = entry.target && entry.target !== entry.title ? entry.target : null;
  const secondary = target
    ? middleEllipsis(target, budget, 'head')
    : entry.project ?? null;
  const tooltip = [
    entry.route ? `${entry.httpMethod ? entry.httpMethod + ' ' : ''}${entry.route}` : null,
    entry.route && entry.title !== entry.route ? entry.title : entry.route ? null : entry.title,
    entry.target ? `→ ${entry.target}` : null,
    entry.project ? `· ${entry.project}` : null,
  ].filter((p): p is string => !!p).join(' ');
  return { primary, secondary, tooltip };
}

@Component({
  selector: 'app-scope-picker',
  imports: [Icon, Withheld],
  host: { class: 'flex h-full min-h-0 flex-col' },
  template: `
    <!-- N2.1 (audit §3.C, owner decision 2) — the second tab. Studio scope was entries-only in a
         symbol-rooted product: the kernel resolves types and members, and every library rendered a
         picker with nothing in it. Counts are on the tabs so an empty one says so before it is
         opened. -->
    <div class="flex items-center border-b border-line text-2xs" role="tablist">
      <button
        type="button"
        role="tab"
        class="flex-1 border-b-2 px-2 py-1 font-medium transition-colors"
        [class.border-accent]="tab() === 'entries'"
        [class.text-accent]="tab() === 'entries'"
        [class.border-transparent]="tab() !== 'entries'"
        [class.text-ink-subtle]="tab() !== 'entries'"
        [attr.aria-selected]="tab() === 'entries'"
        data-testid="picker-tab-entries"
        (click)="tabOverride.set('entries')"
      >
        Entries {{ totalEntryCount() }}
      </button>
      <button
        type="button"
        role="tab"
        class="flex-1 border-b-2 px-2 py-1 font-medium transition-colors"
        [class.border-accent]="tab() === 'types'"
        [class.text-accent]="tab() === 'types'"
        [class.border-transparent]="tab() !== 'types'"
        [class.text-ink-subtle]="tab() !== 'types'"
        [attr.aria-selected]="tab() === 'types'"
        [title]="typeCount() === 0 ? 'No public surface in this analysis' : typeCount() + ' public types across ' + typeGroups().length + ' namespaces'"
        data-testid="picker-tab-types"
        (click)="tabOverride.set('types')"
      >
        Types {{ typeCount() }}
      </button>
    </div>

    <div class="relative">
      <div class="flex items-center gap-1 border-b border-line px-2 py-1.5">
        <app-icon name="search" [size]="14" class="shrink-0 text-ink-subtle" />
        <input
          #searchBox
          type="text"
          class="w-full min-w-0 bg-transparent text-xs text-ink placeholder:text-ink-subtle focus:outline-none"
          [placeholder]="tab() === 'types' ? 'Filter namespaces, types and members…' : 'Filter services and entries…'"
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
              [title]="item.tooltip"
              (mousedown)="addOmniboxItem(item); closeOmnibox()"
            >
              <app-icon [name]="item.entry ? kindIcon(item.kind) : 'code'" [size]="14" class="shrink-0" [style.color]="item.entry ? kindColor(item.kind) : 'var(--vibe-accent-dim)'" />
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

    <!-- N1.2 (audit §3.A) — this said "From current trail" whatever the trail held, and
         no-opped in silence when it held nothing pinnable. It now names its SOURCE (pins beat
         the raw trail) and its COUNT, and is disabled with a stated reason at zero. -->
    <div class="flex items-center gap-1 border-b border-line px-2 py-1">
      <button
        type="button"
        class="flex flex-1 items-center justify-center gap-1 rounded px-2 py-1 text-xs text-ink-subtle hover:bg-hover hover:text-ink disabled:opacity-40 disabled:hover:bg-transparent transition-colors"
        [class.text-accent]="pinCount() > 0"
        [disabled]="pinCount() === 0 && trailCount() === 0"
        [title]="seedTitle()"
        data-testid="trail-seed"
        (click)="trailSeedRequest.emit()"
      >
        <app-icon [name]="pinCount() > 0 ? 'bookmark' : 'history'" [size]="14" />
        {{ seedLabel() }}
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

    @if (tab() === 'types') {
      <!-- N2.1 — the LibrarySurface list, the same MapResponse.surface the library workbench
           renders. Rows carry namespace + kind + member count; a click scopes the pack to that
           type, which the server now resolves through the resolver get_context uses. -->
      <div class="min-h-0 flex-1 overflow-y-auto">
        @for (group of typeGroups(); track group.namespace) {
          <details class="group" open>
            <summary class="flex cursor-pointer items-center gap-1.5 px-2 py-1 text-xs font-medium text-ink-muted hover:text-ink hover:bg-hover/50 transition-colors">
              <app-icon name="chevron-right" [size]="10" class="shrink-0 transition-transform group-open:rotate-90" />
              <app-icon name="layers" [size]="14" class="shrink-0 text-ink-subtle" />
              <span class="min-w-0 flex-1 truncate" [title]="group.namespace">{{ group.namespace }}</span>
              <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ group.types.length }}</span>
            </summary>
            <div class="pl-6">
              @for (t of group.types; track t.name) {
                <button
                  type="button"
                  class="flex w-full items-center gap-1.5 border-l-2 px-2 py-1 text-left text-xs hover:bg-hover transition-colors"
                  [class.border-transparent]="!selectedTypes().has(typeKey(group, t))"
                  [class.border-accent]="selectedTypes().has(typeKey(group, t))"
                  [attr.aria-pressed]="selectedTypes().has(typeKey(group, t))"
                  [title]="typeKey(group, t) + ' — ' + t.kind + ', ' + t.members.length + ' public members' + (t.doc ? ' — ' + t.doc : '')"
                  data-testid="picker-type-row"
                  (click)="toggleType(group, t)"
                >
                  <span class="min-w-0 flex-1 truncate font-mono">{{ t.name }}</span>
                  <span class="shrink-0 text-2xs text-ink-subtle">{{ t.kind }}</span>
                  <span class="w-6 shrink-0 text-right text-2xs tabular-nums text-ink-subtle">{{ t.members.length }}</span>
                </button>
              }
            </div>
          </details>
        } @empty {
          @if (typeCount() === 0) {
            <app-withheld [reason]="typesWithheld().reason" [text]="typesWithheld().text" />
          } @else {
            <div class="px-3 py-6 text-center text-xs text-ink-subtle">
              No types match &ldquo;{{ filterText() }}&rdquo;.
            </div>
          }
        }
      </div>
    } @else {
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
                     rendered eleven identical /api/catalog/i... rows in this 230px column.
                     N2.1 (D-G row identity): the label alone still collided — five eShop
                     OrderStatusChangedTo*IntegrationEventHandler rows share the 20 leading and 5
                     trailing characters the head-biased ellipsis keeps. The row now also says what
                     the entry dispatches to, which is where those five differ. -->
                <span class="flex min-w-0 flex-1 flex-col" [title]="rowIdentity(entry).tooltip">
                  <span class="truncate font-mono">{{ rowIdentity(entry).primary }}</span>
                  @if (rowIdentity(entry).secondary; as sub) {
                    <span class="truncate font-mono text-2xs text-ink-subtle" data-testid="entry-row-identity">{{ sub }}</span>
                  }
                </span>
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
    }

    <div class="flex items-center gap-2 border-t border-line px-2 py-1">
      <span class="text-2xs text-ink-subtle">
        {{ selectedCount() }} of {{ tab() === 'types' ? typeCount() : totalEntryCount() }} selected
      </span>
      <button
        type="button"
        class="ml-auto rounded px-2 py-0.5 text-xs font-medium transition-colors disabled:opacity-30"
        [class.bg-accent]="selectedCount() > 0"
        [class.text-accent-ink]="selectedCount() > 0"
        [class.hover:bg-accent/90]="selectedCount() > 0"
        [class.text-accent]="selectedCount() === 0"
        [disabled]="selectedCount() === 0"
        [title]="tab() === 'types'
          ? 'Adds signatures + usage + bodies + flow + contracts + identity for the selected types'
          : 'Adds the full entry card set for the selected entries'"
        data-testid="add-to-context"
        (click)="addSelected()"
      >
        Add{{ selectedCount() > 0 ? ' ' + selectedCount() : '' }} to context
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
  /** N2.1 — MapResponse.surface, the same structured surface the library workbench renders.
   * The Types tab is a second view of it, not a second source. */
  readonly surface = input<LibrarySurfaceVm | undefined>(undefined);
  /** N1.2 — how many steps the seed button would actually draw on, by source. Pins win. */
  readonly pinCount = input(0);
  readonly trailCount = input(0);

  readonly cardsChange = output<readonly ContextCardSeed[]>();
  readonly trailSeedRequest = output<void>();
  readonly omniboxCard = output<ContextCardSeed>();

  readonly filterText = model('');

  protected readonly showPresetPicker = signal(false);

  /** N1.2 — label and title state the source, the count, and (at zero) why nothing happens. */
  protected readonly seedLabel = computed(() => {
    const pins = this.pinCount();
    if (pins > 0) return `From ${pins} pinned step${pins === 1 ? '' : 's'}`;
    const trail = this.trailCount();
    return trail > 0 ? `From current trail (${trail})` : 'From current trail';
  });

  protected readonly seedTitle = computed(() => {
    const pins = this.pinCount();
    if (pins > 0) return `Seeds one flow card per pinned step (${pins}) — pins win over the raw trail`;
    if (this.trailCount() > 0) return 'Seeds one flow card per trail step. Press p in Explore to pin the ones that matter — pins take priority here';
    return 'Nothing to seed from yet — explore an entry, then press p to pin it';
  });

  protected readonly selectedEntries = signal<ReadonlySet<string>>(new Set());
  /** N2.1 — type selection is keyed by the namespace-qualified name, which is also the focus
   * string sent to the server; one identity, no translation step. */
  protected readonly selectedTypes = signal<ReadonlySet<string>>(new Set());

  protected readonly totalEntryCount = computed(() =>
    this.entryGroups().reduce((n, g) => n + g.entries.length, 0),
  );

  protected readonly typeCount = computed(() =>
    (this.surface()?.groups ?? []).reduce((n, g) => n + g.types.length, 0),
  );

  /** N2.1 — the tab the user picked, or (until they pick one) the tab that has something in it.
   * A library opens on Types instead of on an empty entry list explaining itself. */
  protected readonly tabOverride = signal<'entries' | 'types' | null>(null);
  protected readonly tab = computed<'entries' | 'types'>(() =>
    this.tabOverride() ?? (this.totalEntryCount() === 0 && this.typeCount() > 0 ? 'types' : 'entries'));

  protected readonly typeGroups = computed<readonly SurfaceGroupVm[]>(() =>
    filterGroups(this.surface()?.groups ?? [], this.filterText()));

  protected readonly selectedCount = computed(() =>
    this.tab() === 'types' ? this.selectedTypes().size : this.selectedEntries().size);

  protected readonly pickerWithheld = computed(() =>
    scopePickerWithheld(this.analyzed(), this.isLibrary()));

  /** N2.1 — the Types tab's own empty state. "Analyze a repo" is only true when nothing was
   * analyzed; an app with no library surface is a different, honest sentence. */
  protected readonly typesWithheld = computed<{ reason: WithheldReason; text: string }>(() =>
    this.analyzed()
      ? { reason: 'archetype', text: 'No public surface in this analysis — this repo is scoped by its entry points. Use the Entries tab.' }
      : { reason: 'not-computed', text: 'Analyze a repo to see its public types.' });

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

  protected typeKey(group: SurfaceGroupVm, t: { name: string }): string {
    return typeFocus(group.namespace, t.name);
  }

  protected toggleType(group: SurfaceGroupVm, t: { name: string }): void {
    const key = this.typeKey(group, t);
    this.selectedTypes.update((set) => {
      const next = new Set(set);
      if (!next.delete(key)) next.add(key);
      return next;
    });
  }

  /** N2.1 — the D-G row identity, per row. Called from the template; the function is pure and
   * exported so the collision it fixes is pinned by a spec, not by a screenshot. */
  protected rowIdentity(entry: EntryVm): { primary: string; secondary: string | null; tooltip: string } {
    return entryRowIdentity(entry);
  }

  protected addSelected(): void {
    if (this.tab() === 'types') {
      const focuses = [...this.selectedTypes()];
      if (focuses.length === 0) return;
      const label = focuses.length === 1
        ? (focuses[0].split('.').pop() ?? focuses[0])
        : `${focuses.length} types`;
      this.cardsChange.emit(typeCardSeeds(focuses, label));
      this.selectedTypes.set(new Set());
      return;
    }
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

  /** N2.1 — the omnibox searches BOTH tabs. Two empty states pointed the reader at "the omnibox
   * above" to pick a type (audit §3.F.8) and it only ever searched entries — on the one repo
   * where the sentence appears, a library, it had nothing to return. */
  protected readonly omniboxResults = computed<readonly OmniboxItem[]>(() => {
    const q = this.filterText().trim().toLowerCase();
    if (!q) return [];
    const results: OmniboxItem[] = [];
    for (const group of this.entryGroups()) {
      for (const entry of group.entries) {
        if (entry.title.toLowerCase().includes(q) || (entry.route ?? '').toLowerCase().includes(q) || (entry.target ?? '').toLowerCase().includes(q)) {
          results.push({
            title: entry.route || entry.title,
            kind: entry.kind,
            kindLabel: KIND_LABELS[entry.kind] ?? entry.kind,
            tooltip: entryRowIdentity(entry).tooltip,
            entry,
          });
        }
      }
    }
    for (const group of this.surface()?.groups ?? []) {
      for (const t of group.types) {
        if (!t.name.toLowerCase().includes(q) && !group.namespace.toLowerCase().includes(q)) continue;
        const focus = typeFocus(group.namespace, t.name);
        results.push({
          title: t.name,
          kind: t.kind,
          kindLabel: t.kind,
          tooltip: `${focus} — ${t.kind}, ${t.members.length} public members`,
          typeFocus: focus,
        });
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

  protected addOmniboxItem(item: OmniboxItem): void {
    if (item.typeFocus) {
      // N2.1 — a type from the omnibox seeds the card that answers the question a library reader
      // is actually asking: who uses this. One card, not the whole type set — the omnibox is the
      // quick path; the Types tab's Add is the deliberate one.
      this.omniboxCard.emit({
        type: 'usage',
        title: `Who uses ${item.title}`,
        entryIds: [item.typeFocus],
        estimatedLines: 15,
      });
      return;
    }
    const entry = item.entry!;
    const seed: ContextCardSeed = {
      type: 'flow',
      title: `Flow: ${entry.route || entry.title}`,
      entryIds: [entry.nodeId],
      estimatedLines: 15,
    };
    this.omniboxCard.emit(seed);
  }
}

/** N2.1 — one omnibox row, from either tab: an entry (carries its EntryVm) or a surface type
 * (carries the focus string the server resolves). Exactly one of the two is set. */
interface OmniboxItem {
  readonly title: string;
  readonly kind: string;
  readonly kindLabel: string;
  readonly tooltip: string;
  readonly entry?: EntryVm;
  readonly typeFocus?: string;
}
