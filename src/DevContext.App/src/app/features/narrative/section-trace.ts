import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { SectionCard } from '../../ui/section-card/section-card';
import { TraceNodeComponent } from '../trace/trace-node';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-section-trace',
  imports: [FormsModule, SectionCard, TraceNodeComponent, Icon],
  template: `
    <app-section-card id="trace" title="Trace" [subtitle]="traceStore.focus() ?? ''">
      @if (traceStore.active()) {
        <div class="mb-4 flex flex-wrap items-center gap-3 rounded-md border border-line bg-surface px-3 py-2">
          <div class="flex items-center gap-2">
            <span class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Focus</span>
            <input
              class="w-56 rounded border border-line bg-base px-2 py-1 font-mono text-xs text-ink outline-none focus:border-accent"
              placeholder="Search entry or symbol…"
              [value]="focusQuery()"
              (input)="onFocusInput($event)"
            />
            @if (filteredEntries().length) {
              <div class="absolute top-full left-0 z-10 mt-1 max-h-40 w-72 overflow-auto rounded border border-line bg-elevated shadow-lg">
                @for (e of filteredEntries(); track e.focus) {
                  <button
                    class="w-full px-2 py-1 text-left text-xs text-ink-muted hover:bg-surface-2 hover:text-ink"
                    (click)="trace(e.focus)"
                  >{{ e.title }} <span class="text-ink-subtle">{{ e.kind }}</span></button>
                }
              </div>
            }
          </div>
          <span class="text-ink-subtle">|</span>
          <label class="flex items-center gap-1.5 text-2xs">
            <span class="text-ink-subtle">Depth</span>
            <select
              class="rounded border border-line bg-base px-1.5 py-1 text-xs text-ink outline-none focus:border-accent"
              [ngModel]="traceStore.depth()"
              (ngModelChange)="traceStore.setDepth($event)"
            >
              @for (d of [1,2,3,4,5,6,7,8,9,10]; track d) { <option [value]="d">{{ d }}</option> }
            </select>
          </label>
          <label class="flex items-center gap-1.5 text-2xs">
            <span class="text-ink-subtle">Detail</span>
            <select
              class="rounded border border-line bg-base px-1.5 py-1 text-xs text-ink outline-none focus:border-accent"
              [ngModel]="traceStore.detail()"
              (ngModelChange)="traceStore.setDetail($event)"
            >
              <option value="salient">Salient</option>
              <option value="signature">Signature</option>
              <option value="full">Full</option>
            </select>
          </label>
          <button
            class="ml-auto flex items-center gap-1 rounded px-1.5 py-1 text-2xs text-ink-muted hover:bg-surface-2 hover:text-ink"
            (click)="traceStore.clear()"
          >
            <app-icon name="x" [size]="11" /> Clear
          </button>
        </div>
      }

      @if (session.ready()) {
        @if (traceStore.loading()) {
          <div class="flex items-center justify-center py-12 text-xs text-ink-muted">Tracing&hellip;</div>
        } @else if (traceStore.tree(); as root) {
          <div class="max-h-[70vh] overflow-auto py-1">
            <app-trace-node [node]="root" [depth]="0" />
          </div>
        } @else if (!traceStore.found()) {
          <p class="py-8 text-center text-xs text-ink-subtle">Trace not found for this focus.</p>
        } @else {
          <p class="py-8 text-center text-xs text-ink-subtle">
            <app-icon name="arrow-right" [size]="12" class="inline text-ink-subtle" />
            Select an entry above to trace its call chain.
          </p>
        }
      }
    </app-section-card>
  `,
})
export class SectionTrace {
  protected readonly session = inject(SessionStore);
  protected readonly traceStore = inject(TraceStore);

  protected readonly focusQuery = signal('');
  protected readonly filteredEntries = signal<{ focus: string; title: string; kind: string }[]>([]);

  constructor() {
    effect(() => {
      const q = this.focusQuery();
      const entries = this.session.entryGroups().flatMap((g) => g.entries);
      if (!q.trim()) { this.filteredEntries.set(entries.slice(0, 10).map((e) => ({ focus: e.focus, title: e.title, kind: e.kind }))); return; }
      const lower = q.toLowerCase();
      this.filteredEntries.set(
        entries.filter((e) => e.focus.toLowerCase().includes(lower) || e.title.toLowerCase().includes(lower)).slice(0, 10).map((e) => ({ focus: e.focus, title: e.title, kind: e.kind })),
      );
    });
  }

  protected onFocusInput(e: Event): void {
    this.focusQuery.set((e.target as HTMLInputElement).value);
  }

  protected trace(focus: string): void {
    const handle = this.session.handle();
    if (!handle) return;
    void this.traceStore.trace(handle, focus);
  }
}
