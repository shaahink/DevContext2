import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import type { WritableSignal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { NodeStore } from '../../state/node.store';
import { type EntryVm } from '../../models/view-models';
import { KIND_LABELS, KIND_ICONS } from '../../models/view-models';
import { ViewFrame } from '../../shell/view-frame';
import { Badge } from '../../ui/badge/badge';
import { Icon } from '../../ui/icon/icon';
import { SearchField } from '../../ui/search-field/search-field';
import { NodeLink } from '../../ui/node-link/node-link';

type SortColumn = 'route' | 'target' | 'kind';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-entries-view',
  imports: [Icon, Badge, SearchField, ViewFrame, NodeLink],
  template: `
    <app-view-frame>
      <div sidebar class="flex flex-col h-full">
        <div class="border-b border-line px-3 py-2">
          <app-search-field [(query)]="search" />
        </div>
        <div class="flex-1 overflow-auto py-1">
          @for (group of session.entryGroups(); track group.kind) {
            <button
              class="flex w-full items-center gap-2 px-3 py-1.5 text-xs text-ink-muted transition-colors hover:bg-surface-2 hover:text-ink"
              [class.bg-accent/10]="kindFilter() === group.kind"
              [class.text-accent]="kindFilter() === group.kind"
              (click)="kindFilter.set(kindFilter() === group.kind ? null : group.kind)"
            >
              <app-icon [name]="KIND_ICONS[group.kind] ?? 'dot'" [size]="12" />
              {{ group.kind }}
              <span class="ml-auto rounded bg-surface-2 px-1.5 py-0.5 text-2xs text-ink-subtle">{{ group.entries.length }}</span>
            </button>
          }
        </div>
      </div>

      <div header class="flex items-center gap-2 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Entry Points</h2>
        <span class="text-xs text-ink-subtle">{{ totalEntries() }} / {{ session.entryCount() }} entries</span>
        @if (kindFilter()) {
          <app-badge variant="accent">{{ kindFilter() }}</app-badge>
          <button class="text-ink-subtle hover:text-ink" (click)="kindFilter.set(null)"><app-icon name="x" [size]="12" /></button>
        }
        <span class="flex-1"></span>
        <div class="flex items-center gap-1">
          <button class="rounded px-1.5 py-0.5 text-2xs transition-colors"
                  [class.bg-accent/10]="hasTargetFilter()" [class.text-accent]="hasTargetFilter()"
                  [class.text-ink-muted]="!hasTargetFilter()" (click)="toggleHasTarget()">has target</button>
          <button class="rounded px-1.5 py-0.5 text-2xs transition-colors"
                  [class.bg-warn/10]="approxFilter()" [class.text-warn]="approxFilter()"
                  [class.text-ink-muted]="!approxFilter()" (click)="approxFilter.set(!approxFilter())">approx</button>
        </div>
      </div>

      @if (session.ready()) {
        @if (sortedEntries().length) {
          <table class="w-full text-xs">
            <thead class="sticky top-0 bg-surface z-10">
              <tr class="border-b border-line text-2xs text-ink-muted uppercase">
                <th class="py-1.5 px-3 text-left font-medium cursor-pointer select-none hover:text-ink w-12"
                    (click)="sortBy('route')">Route {{ sortIcon('route') }}</th>
                <th class="py-1.5 px-3 text-left font-medium cursor-pointer select-none hover:text-ink"
                    (click)="sortBy('target')">Target {{ sortIcon('target') }}</th>
                <th class="py-1.5 px-3 text-left font-medium cursor-pointer select-none hover:text-ink w-20"
                    (click)="sortBy('kind')">Kind {{ sortIcon('kind') }}</th>
                <th class="py-1.5 px-3 text-right w-16 opacity-0">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-line">
              @for (entry of sortedEntries(); track entry.nodeId; let idx = $index) {
                <tr class="hover:bg-surface-2 cursor-pointer group"
                    (click)="traceEntry(entry)"
                    (keydown.enter)="traceEntry(entry)"
                    [attr.tabindex]="0">
                  <td class="py-1.5 px-3 align-top">
                    <div class="flex items-center gap-1.5">
                      @if (entry.httpMethod) { <app-badge variant="accent" class="text-3xs">{{ entry.httpMethod }}</app-badge> }
                      @if (entry.provenance === 'Syntactic') { <app-badge variant="warn" class="text-3xs">~</app-badge> }
                    </div>
                  </td>
                  <td class="py-1.5 px-3 align-top">
                    <p class="font-mono text-ink text-xs truncate max-w-96">{{ entry.route || entry.title }}</p>
                    @if (entry.target) { <p class="text-ink-muted text-2xs">&rarr; <app-node-link [nodeId]="entry.target" [label]="entry.target" /></p> }
                  </td>
                  <td class="py-1.5 px-3 align-top">
                    <div class="flex items-center gap-1">
                      <app-icon [name]="KIND_ICONS[entry.kind] ?? 'dot'" [size]="10" />
                      <span class="text-ink-muted text-2xs">{{ entry.kind }}</span>
                    </div>
                  </td>
                  <td class="py-1.5 px-3 align-top text-right">
                    <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button class="rounded p-0.5 text-ink-muted hover:bg-surface-2 hover:text-accent" title="Trace"
                              (click)="$event.stopPropagation(); traceEntry(entry)"><app-icon name="network" [size]="12" /></button>
                      <button class="rounded p-0.5 text-ink-muted hover:bg-surface-2 hover:text-accent" title="Node card"
                              (click)="$event.stopPropagation(); onNodeCard(entry)"><app-icon name="info" [size]="12" /></button>
                      <button class="rounded p-0.5 text-ink-muted hover:bg-surface-2 hover:text-accent" title="Copy route"
                              (click)="$event.stopPropagation(); copyRoute(entry)"><app-icon name="code" [size]="12" /></button>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        } @else {
          <div class="flex h-full items-center justify-center text-ink-muted">
            <p class="text-sm">No entries match — <button class="text-accent hover:underline" (click)="search.set(''); kindFilter.set(null); hasTargetFilter.set(false); approxFilter.set(false)">clear filters</button></p>
          </div>
        }
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to see entry points.</p></div>
      }
    </app-view-frame>
  `,
})
export class EntriesView {
  protected readonly session = inject(SessionStore);
  private readonly trace = inject(TraceStore);
  private readonly router = inject(Router);
  private readonly nodeStore = inject(NodeStore);

  protected readonly search = signal('');
  protected readonly kindFilter = signal<string | null>(null);
  protected readonly hasTargetFilter = signal(false);
  protected readonly approxFilter = signal(false);
  protected readonly sortColumn: WritableSignal<SortColumn> = signal('target');
  protected readonly sortDir: WritableSignal<SortDir> = signal('desc');
  protected readonly KIND_LABELS = KIND_LABELS;
  protected readonly KIND_ICONS = KIND_ICONS;

  protected readonly sortedEntries = computed(() => {
    const groups = this.session.entryGroups();
    if (!groups.length) return [];

    let entries = groups.flatMap(g => g.entries);
    const q = this.search().toLowerCase();
    const kf = this.kindFilter();

    if (kf) entries = entries.filter(e => e.kind === kf);
    if (q) entries = entries.filter(e => (e.route ?? e.title).toLowerCase().includes(q) || (e.target ?? '').toLowerCase().includes(q));
    if (this.hasTargetFilter()) entries = entries.filter(e => !!e.target);
    if (this.approxFilter()) entries = entries.filter(e => e.provenance === 'Syntactic');

    const col = this.sortColumn();
    const dir = this.sortDir();
    entries = [...entries].sort((a, b) => {
      let cmp = 0;
      if (col === 'target') {
        const aT = a.target ? 1 : 0;
        const bT = b.target ? 1 : 0;
        cmp = aT - bT;
        if (cmp === 0) cmp = (a.route || a.title).localeCompare(b.route || b.title);
      } else if (col === 'route') {
        cmp = (a.route || a.title).localeCompare(b.route || b.title);
      } else if (col === 'kind') {
        cmp = a.kind.localeCompare(b.kind);
      }
      return dir === 'asc' ? cmp : -cmp;
    });

    return entries;
  });

  protected totalEntries(): number { return this.sortedEntries().length; }

  protected toggleHasTarget(): void { this.hasTargetFilter.set(!this.hasTargetFilter()); }

  protected sortBy(col: SortColumn): void {
    if (this.sortColumn() === col) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(col);
      this.sortDir.set(col === 'target' ? 'desc' : 'asc');
    }
  }

  protected sortIcon(col: SortColumn): string {
    if (this.sortColumn() !== col) return '';
    return this.sortDir() === 'asc' ? '\u2191' : '\u2193';
  }

  protected traceEntry(entry: EntryVm): void {
    const handle = this.session.handle();
    if (!handle) return;
    void this.trace.trace(handle, entry.focus);
    this.router.navigate(['/trace']);
  }

  protected onNodeCard(entry: EntryVm): void {
    this.nodeStore.show(entry.target || entry.nodeId);
  }

  protected copyRoute(entry: EntryVm): void {
    void navigator.clipboard.writeText(entry.route || entry.title);
  }
}
