import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { ViewFrame } from '../../shell/view-frame';
import { TraceNodeComponent } from './trace-node';

@Component({
  selector: 'app-trace-view',
  imports: [FormsModule, ViewFrame, TraceNodeComponent],
  template: `
    <app-view-frame>
      <div sidebar class="flex flex-col h-full p-3 space-y-3">
        <div class="space-y-1.5">
          <p class="text-2xs font-semibold uppercase text-ink-subtle">Focus</p>
          <input
            class="w-full rounded border border-line bg-surface-2 px-2 py-1 font-mono text-xs text-ink outline-none focus:border-accent"
            placeholder="Search entry or symbol…"
            [value]="focusQuery()"
            (input)="onFocusInput($event)"
          />
          @if (filteredEntries().length) {
            <div class="max-h-40 overflow-auto space-y-0.5">
              @for (e of filteredEntries(); track e.focus) {
                <button
                  class="w-full rounded px-2 py-1 text-left text-xs text-ink-muted hover:bg-surface-2 hover:text-ink"
                  (click)="trace(e.focus)"
                >{{ e.title }} <span class="text-ink-subtle">{{ e.kind }}</span></button>
              }
            </div>
          }
        </div>
        <div class="space-y-1.5">
          <p class="text-2xs font-semibold uppercase text-ink-subtle">Depth</p>
          <select class="w-full rounded border border-line bg-surface-2 px-2 py-1 text-xs text-ink outline-none focus:border-accent" [ngModel]="traceStore.depth()" (ngModelChange)="traceStore.setDepth($event)">
            @for (d of [1,2,3,4,5,6,7,8,9,10]; track d) { <option [value]="d">{{ d }}</option> }
          </select>
        </div>
        <div class="space-y-1.5">
          <p class="text-2xs font-semibold uppercase text-ink-subtle">Detail</p>
          <select class="w-full rounded border border-line bg-surface-2 px-2 py-1 text-xs text-ink outline-none focus:border-accent" [ngModel]="traceStore.detail()" (ngModelChange)="traceStore.setDetail($event)">
            <option value="salient">Salient</option>
            <option value="signature">Signature</option>
            <option value="full">Full</option>
          </select>
        </div>
      </div>

      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Trace</h2>
        @if (traceStore.focus()) { <span class="font-mono text-xs text-ink-muted">{{ traceStore.focus() }}</span> }
      </div>

      @if (session.ready()) {
        @if (traceStore.loading()) {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Tracing…</p></div>
        } @else if (traceStore.tree(); as root) {
          <div class="overflow-auto p-4">
            <app-trace-node [node]="root" [depth]="0" />
          </div>
        } @else if (!traceStore.found()) {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Trace not found for this focus.</p></div>
        } @else {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Select or search an entry to trace its call chain.</p></div>
        }
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to trace call chains.</p></div>
      }
    </app-view-frame>
  `,
})
export class TraceView {
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
