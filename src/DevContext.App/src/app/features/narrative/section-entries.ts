import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { KIND_LABELS, KIND_ICONS, type EntryVm } from '../../models/view-models';
import { SectionCard } from '../../ui/section-card/section-card';
import { Icon } from '../../ui/icon/icon';
import { Badge } from '../../ui/badge/badge';
import { SearchField } from '../../ui/search-field/search-field';

@Component({
  selector: 'app-section-entries',
  imports: [FormsModule, SectionCard, Icon, Badge, SearchField],
  template: `
    <app-section-card id="entries" title="Entry Points" [subtitle]="totalEntries() + ' entries'">
      <div class="space-y-3">
        <div class="flex flex-wrap items-center gap-2">
          <app-search-field [(query)]="search" class="w-56" />
          <span class="text-2xs text-ink-subtle">Filter:</span>
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
              class="rounded px-2 py-1 text-2xs font-medium transition-colors"
              [class.bg-accent]="kindFilter() === kind"
              [class.text-accent-ink]="kindFilter() === kind"
              [class.bg-surface-2]="kindFilter() !== kind"
              [class.text-ink-muted]="kindFilter() !== kind"
              [class.hover:bg-surface-2]="kindFilter() !== kind"
              (click)="kindFilter.set(kindFilter() === kind ? null : kind)"
            >
              <app-icon [name]="KIND_ICONS[kind] ?? 'dot'" [size]="11" />
              {{ KIND_LABELS[kind] ?? kind }}
            </button>
          }
        </div>

        <div class="overflow-x-auto rounded-md border border-line">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="border-b border-line bg-surface-2 text-2xs font-semibold uppercase tracking-wider text-ink-muted">
                <th class="px-3 py-2 w-16">Method</th>
                <th class="px-3 py-2">Route / Title</th>
                <th class="px-3 py-2">Target</th>
                <th class="px-3 py-2 w-28">Kind</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-line">
              @for (entry of filteredEntries(); track entry.nodeId) {
                <tr
                  class="cursor-pointer transition-colors hover:bg-surface-2"
                  (click)="traceEntry(entry)"
                  (keydown.enter)="traceEntry(entry)"
                  tabindex="0"
                >
                  <td class="px-3 py-1.5">
                    @if (entry.httpMethod) {
                      <app-badge variant="accent">{{ entry.httpMethod }}</app-badge>
                    }
                  </td>
                  <td class="px-3 py-1.5">
                    <span class="font-mono text-ink">{{ entry.route || entry.title }}</span>
                    @if (entry.provenance === 'Syntactic') {
                      <app-badge variant="warn" class="ml-2">approx</app-badge>
                    }
                  </td>
                  <td class="px-3 py-1.5 font-mono text-ink-muted">
                    @if (entry.target) {
                      <app-icon name="arrow-right" [size]="11" class="inline text-ink-subtle" />
                      {{ entry.target }}
                    }
                  </td>
                  <td class="px-3 py-1.5">
                    <span class="text-2xs text-ink-subtle">{{ KIND_LABELS[entry.kind] ?? entry.kind }}</span>
                  </td>
                </tr>
              }
            </tbody>
          </table>
          @if (!filteredEntries().length) {
            <p class="px-3 py-6 text-center text-xs text-ink-subtle">
              {{ session.ready() ? 'No entries match.' : 'Analyze a repo to list its entry points.' }}
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

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly KIND_LABELS = KIND_LABELS;
  protected readonly KIND_ICONS = KIND_ICONS;

  protected readonly allKinds = computed(() => {
    const kinds = new Set<string>();
    for (const g of this.session.entryGroups()) kinds.add(g.kind);
    return [...kinds];
  });

  protected readonly filteredEntries = computed<EntryVm[]>(() => {
    const groups = this.session.entryGroups();
    const kf = this.kindFilter();
    const q = this.search().toLowerCase();
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
    return entries;
  });

  protected totalEntries = computed(() => this.filteredEntries().length);

  protected traceEntry(entry: EntryVm): void {
    const handle = this.session.handle();
    if (!handle) return;
    void this.trace.trace(handle, entry.focus);
  }
}
