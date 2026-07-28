import { Component, computed, inject, signal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore } from '../../state/trail.store';
import { TraceNodeComponent } from '../trace/trace-node';
import {
  defaultSection,
  filterGroups,
  focusForSurfaceEntry,
  internalTypeCount,
  namespaceCount,
  publicTypeCount,
  railItems,
  type LibSectionId,
} from './library-surface.vm';

/**
 * D4.4 (F1) — the library workbench: archetype Library routes Explore here instead of
 * the entry-deck/stage/inspector triad (a library has surface, not flows). Left rail =
 * the CLI's five sections (ENTRY API / ABSTRACTIONS / GENERATORS / PUBLIC SURFACE /
 * CONSUMER PATHS) with counts; the main panel renders the active section from
 * MapResponse.surface — the same engine truth the CLI prints, now structured (the
 * proto carried only groups+extension_points before this checkpoint).
 */
@Component({
  selector: 'app-library-workbench',
  imports: [TraceNodeComponent],
  host: { class: 'flex h-full min-h-0' },
  template: `
    <!-- Section rail -->
    <nav class="flex w-56 shrink-0 flex-col gap-0.5 border-r border-line p-2" aria-label="Library surface sections">
      <p class="px-2 pb-1 pt-1 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">
        Library surface
      </p>
      @for (item of rail(); track item.id) {
        <button
          type="button"
          class="list-row flex items-center justify-between px-2 py-1.5 text-left text-xs"
          [class.selected]="section() === item.id"
          [class.text-ink-subtle]="item.count === 0"
          (click)="chosenSection.set(item.id)"
        >
          <span>{{ item.label }}</span>
          <span class="tabular-nums text-2xs text-ink-subtle">{{ item.count }}</span>
        </button>
      }
    </nav>

    <!-- Section content -->
    <div class="min-w-0 flex-1 overflow-y-auto p-4">
      <header class="mb-3 flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <h2 class="text-sm font-semibold text-ink">{{ libraryName() }}</h2>
        <span class="text-2xs text-ink-subtle">
          {{ typeCount() }} public {{ typeCount() === 1 ? 'type' : 'types' }}
          · {{ nsCount() }} {{ nsCount() === 1 ? 'namespace' : 'namespaces' }}
          @if (internalCount() > 0) {
            · {{ internalCount() }} internal
          }
        </span>
      </header>

      @switch (section()) {
        @case ('entry-api') {
          @if (surface()?.entryApi?.length) {
            <ul class="space-y-2">
              @for (e of surface()!.entryApi; track e.title) {
                <li>
                  <!-- R3 D-C (C2): a front door is the spine, so it opens. Selecting one traces the
                       type or member it names, and the path panel shows what the framework actually
                       calls back — the answer to "how do I use this". -->
                  <!-- The list-row class is a flex ROW, so the doc has to be told to stack or it
                       becomes a squeezed column the moment the path panel takes half the width. -->
                  <button
                    type="button"
                    class="list-row w-full flex-col items-start gap-0 px-2 py-1 text-left text-xs"
                    [class.selected]="selectedDoor() === e.title"
                    (click)="openDoor(e.title)"
                  >
                    <span class="flex w-full flex-wrap items-baseline gap-x-2">
                      <span class="chip shrink-0 text-2xs">{{ e.kind }}</span>
                      <span class="font-mono font-semibold text-ink">{{ e.title }}</span>
                      @if (e.location) {
                        <span class="text-2xs text-ink-subtle">{{ e.location }}</span>
                      }
                    </span>
                    @if (e.doc) {
                      <span class="pl-1 text-2xs text-ink-muted">{{ e.doc }}</span>
                    }
                  </button>
                </li>
              }
            </ul>
          } @else {
            <p class="text-xs text-ink-muted">No ranked entry API detected — start from the public surface.</p>
          }
        }
        @case ('abstractions') {
          @if (surface()?.abstractions?.length) {
            <ul class="space-y-1.5">
              @for (a of surface()!.abstractions; track a.name) {
                <li>
                  <!-- An abstraction is a seat you implement, which makes it a front door too. -->
                  <button
                    type="button"
                    class="list-row flex w-full items-baseline gap-2 px-2 py-1 text-left text-xs"
                    [class.selected]="selectedDoor() === a.name"
                    (click)="openDoor(a.name)"
                  >
                    <span class="font-mono text-ink">{{ a.name }}</span>
                    <span class="text-2xs text-ink-subtle">({{ a.kind }})</span>
                    <span class="ml-auto tabular-nums text-2xs text-ink-muted">
                      {{ a.implementorCount }} {{ a.implementorCount === 1 ? 'implementor' : 'implementors' }}
                    </span>
                  </button>
                </li>
              }
            </ul>
          } @else {
            <p class="text-xs text-ink-muted">No abstractions with in-repo implementors.</p>
          }
        }
        @case ('generators') {
          @if (surface()?.generators?.length) {
            <ul class="space-y-2">
              @for (g of surface()!.generators; track g.name) {
                <li class="text-xs">
                  <div class="flex items-baseline gap-2">
                    <span class="chip shrink-0 text-2xs">{{ g.kind }}</span>
                    <span class="font-mono font-semibold text-ink">{{ g.name }}</span>
                  </div>
                  @if (g.doc) {
                    <p class="mt-0.5 pl-1 text-2xs text-ink-muted">{{ g.doc }}</p>
                  }
                </li>
              }
            </ul>
          } @else {
            <p class="text-xs text-ink-muted">This library ships no source generators, analyzers, or code fixers.</p>
          }
        }
        @case ('surface') {
          <input
            type="text"
            class="mb-3 w-full max-w-sm rounded-md border border-line bg-surface px-2 py-1 text-xs text-ink placeholder:text-ink-subtle focus:border-accent focus:outline-none"
            placeholder="Filter types, members, namespaces…"
            [value]="filterText()"
            (input)="filterText.set($any($event.target).value)"
          />
          @if (filteredGroups().length) {
            <div class="space-y-4">
              @for (group of filteredGroups(); track group.namespace) {
                <section>
                  <h3 class="mb-1.5 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">
                    {{ group.namespace }}
                  </h3>
                  <ul class="space-y-1.5">
                    @for (t of group.types; track t.name) {
                      <li class="text-xs">
                        <div class="flex items-baseline gap-2">
                          <span class="font-mono font-semibold text-ink">{{ t.name }}</span>
                          <span class="text-2xs text-ink-subtle">({{ t.kind.toLowerCase() }})</span>
                        </div>
                        @if (t.members.length) {
                          <p class="mt-0.5 break-words pl-1 font-mono text-2xs text-ink-muted">{{ t.members.join(', ') }}</p>
                        }
                        @if (t.doc) {
                          <p class="mt-0.5 pl-1 text-2xs text-ink-subtle">{{ t.doc }}</p>
                        }
                      </li>
                    }
                  </ul>
                </section>
              }
            </div>
            @if (internalCount() > 0 && !filterText().trim()) {
              <p class="mt-4 text-2xs text-ink-subtle">
                + {{ internalCount() }} {{ internalCount() === 1 ? 'type' : 'types' }} in *.Internal namespaces (demoted out of the main surface)
              </p>
            }
          } @else {
            <p class="text-xs text-ink-muted">
              @if (filterText().trim()) {
                Nothing matches "{{ filterText().trim() }}".
              } @else {
                No public surface detected.
              }
            </p>
          }
        }
        @case ('consumer-paths') {
          @if (surface()?.consumerPaths?.length) {
            <ul class="space-y-1.5">
              @for (p of surface()!.consumerPaths; track p) {
                <li class="font-mono text-xs text-ink">{{ p }}</li>
              }
            </ul>
          } @else {
            <p class="text-xs text-ink-muted">No usage recipes derived — see the entry API.</p>
          }
        }
      }
    </div>

    <!-- R3 D-C (C2) — the consumer path. It appears only once a door is open, so the browser keeps
         the full width until there is something to put beside it. -->
    @if (selectedDoor(); as door) {
      <aside class="flex w-[45%] min-w-0 shrink-0 flex-col border-l border-line">
        <header class="flex items-baseline gap-2 border-b border-line px-3 py-2">
          <span class="font-mono text-xs font-semibold text-ink">{{ door }}</span>
          <span class="text-2xs text-ink-subtle">consumer path</span>
          <button
            type="button"
            class="ml-auto text-2xs text-ink-subtle hover:text-ink"
            (click)="closeDoor()"
          >close</button>
        </header>

        <div class="min-h-0 flex-1 overflow-auto p-2" [class.opacity-60]="trace.loading()">
          @if (trace.tree(); as tree) {
            <app-trace-node [node]="tree" (nodeSelected)="onNode($event)" />
          } @else if (trace.loading()) {
            <p class="p-2 text-xs text-ink-subtle">Following the calls…</p>
          } @else {
            <!-- The resolver was asked and answered. Saying so beats an empty panel. -->
            <p class="p-2 text-xs text-ink-muted">
              No call path resolved for <span class="font-mono">{{ focusOf(door) }}</span> — it is
              public surface the repo itself never calls.
            </p>
          }
        </div>
      </aside>
    }
  `,
})
export class LibraryWorkbench {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  private readonly trail = inject(TrailStore);

  protected readonly surface = computed(() => this.session.mapResponse()?.surface);
  protected readonly rail = computed(() => railItems(this.surface()));
  protected readonly typeCount = computed(() => publicTypeCount(this.surface()));
  protected readonly nsCount = computed(() => namespaceCount(this.surface()));
  protected readonly internalCount = computed(() => internalTypeCount(this.surface()));
  protected readonly libraryName = computed(
    () => this.session.mapResponse()?.solutionName || this.session.summary()?.label || 'Library',
  );

  protected readonly filterText = signal('');
  /** User selection wins; until then, land on the CLI's leading section. */
  protected readonly chosenSection = signal<LibSectionId | null>(null);
  protected readonly section = computed(() => this.chosenSection() ?? defaultSection(this.surface()));

  protected readonly filteredGroups = computed(() =>
    filterGroups(this.surface()?.groups ?? [], this.filterText()),
  );

  /** R3 D-C (C2) — the open front door, by its surface title. */
  protected readonly selectedDoor = signal<string | null>(null);

  protected focusOf(title: string): string {
    return focusForSurfaceEntry(title);
  }

  /** Trace the door. The Trail collects it exactly as it collects an entry elsewhere, so a
   * library finally feeds the pin/export loop it has been locked out of. */
  protected openDoor(title: string): void {
    const handle = this.session.handle();
    if (!handle) return;
    if (this.selectedDoor() === title) {
      this.closeDoor();
      return;
    }
    this.selectedDoor.set(title);
    const focus = focusForSurfaceEntry(title);
    this.trail.push({ kind: 'entry', id: focus, title, focus });
    void this.trace.trace(handle, focus);
  }

  protected closeDoor(): void {
    this.selectedDoor.set(null);
  }

  protected onNode(nodeId: string): void {
    void this.trace.selectNode(nodeId);
  }
}
