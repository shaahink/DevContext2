import { Component, computed, inject, input, output } from '@angular/core';

import type { EntryVm } from '../../models/view-models';
import { SessionStore } from '../../state/session.store';
import { commandCounts, indexByTitle } from './command-surface.vm';

/**
 * R3 D-D (D1): a CLI's product surface is its command set, so that is what its workspace opens on.
 *
 * The engine has published this projection since L7.2 — `ArchetypeView` groups a CliTool's commands
 * by project, with the handler each one reaches — and the CLI's Map has rendered it as
 * "COMMAND SURFACE" ever since. Nothing in the desktop read it, so a CLI repo landed on a topology
 * canvas instead: GitVersion drew two unconnected boxes, because a command-line tool has no
 * transports by construction and never will.
 *
 * Rows are the engine's rows. A row is clickable when it matches a loaded entry, because focusing a
 * command must do exactly what focusing it in the deck does — one focus path, not two.
 */
@Component({
  selector: 'app-command-surface',
  template: `
    <div class="flex h-full flex-col overflow-y-auto p-4">
      <header class="mb-3 flex flex-wrap items-baseline gap-x-3">
        <h2 class="font-mono text-2xs uppercase tracking-wider text-ink-subtle">{{ sectionLabel() }}</h2>
        <span class="text-2xs text-ink-subtle">
          {{ commandCount() }} {{ commandCount() === 1 ? 'command' : 'commands' }}
          @if (unwired() > 0) {
            · <span class="text-warn">{{ unwired() }} with no resolved handler</span>
          }
        </span>
      </header>

      @for (group of groups(); track group.project) {
        <section class="mb-4">
          <p class="mb-1.5 flex items-baseline gap-2 text-xs">
            <span class="font-mono text-ink">{{ group.project }}</span>
            @if (group.layer) {
              <span class="text-2xs text-ink-subtle">[{{ group.layer }}]</span>
            }
            <span class="tabular-nums text-2xs text-ink-subtle">{{ group.entries.length }}</span>
          </p>
          <ul class="space-y-0.5">
            @for (row of group.entries; track row.title) {
              <li>
                <button
                  type="button"
                  class="list-row flex w-full items-baseline gap-2 px-2 py-1 text-left text-xs disabled:cursor-default disabled:opacity-70"
                  [disabled]="!focusFor(row.title)"
                  [title]="focusFor(row.title) ? 'Trace this command' : 'Not in the loaded entry list, so it cannot be traced from here'"
                  (click)="select(row.title)"
                >
                  <span class="shrink-0 font-mono font-semibold text-ink">{{ row.title }}</span>
                  @if (row.target) {
                    <span class="truncate font-mono text-2xs text-ink-muted">&rarr; {{ row.target }}</span>
                  } @else {
                    <!-- Honest about a join the engine could not make: a verb whose handler is
                         unknown is a finding, and an empty column would hide it. -->
                    <span class="shrink-0 text-2xs text-warn">no resolved handler</span>
                  }
                  @if (row.hops > 0) {
                    <span class="ml-auto shrink-0 tabular-nums text-2xs text-ink-subtle">{{ row.hops }} hops</span>
                  }
                </button>
              </li>
            }
          </ul>
        </section>
      } @empty {
        <p class="text-xs text-ink-muted">No commands detected for this tool.</p>
      }
    </div>
  `,
  host: { class: 'contents' },
})
export class CommandSurface {
  private readonly session = inject(SessionStore);

  /** The loaded entries — used only to resolve a row to the focus the deck would send. */
  readonly entries = input<readonly EntryVm[]>([]);
  readonly commandSelected = output<EntryVm>();

  private readonly view = computed(() => this.session.mapResponse()?.archetypeView ?? null);
  protected readonly sectionLabel = computed(() => this.view()?.sectionLabel || 'COMMAND SURFACE');
  protected readonly groups = computed(() => this.view()?.groups ?? []);
  private readonly counts = computed(() => commandCounts(this.groups()));
  protected readonly commandCount = computed(() => this.counts().total);
  protected readonly unwired = computed(() => this.counts().unwired);

  private readonly byTitle = computed(() => indexByTitle(this.entries()));

  protected focusFor(title: string): EntryVm | undefined {
    return this.byTitle().get(title);
  }

  protected select(title: string): void {
    const entry = this.focusFor(title);
    if (entry) this.commandSelected.emit(entry);
  }
}
