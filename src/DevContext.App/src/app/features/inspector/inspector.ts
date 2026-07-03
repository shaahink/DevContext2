import { Component, computed, inject, output, signal } from '@angular/core';

import { TrailStore, type TrailStep } from '../../state/trail.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';

type SectionId = 'details' | 'callstack' | 'insights' | 'llm' | 'trail';

/**
 * Inspector (F proposal §2) — the right panel. Content is driven ENTIRELY by the
 * current selection; sections collapse independently. Details fill instantly from
 * local data (selection echo, §5.2) while RPC-backed sections follow.
 *
 * TODO(W4): LLM section renders via the Render RPC (migrate section-lens debounce);
 *   until then it shows the trace markdown already in the store — real, just coarser.
 * TODO(W4): Call stack hosts a compact ProgressiveTraceTree at depth 2.
 * TODO(W5): Insights section filters stats().insights by the current selection;
 *   "Reached by N flows" line reads AtlasStore.reachedBy(nodeId).
 * TODO(W4): file path row gets "Reveal in Explorer" via the Tauri opener plugin (W6).
 */
@Component({
  selector: 'app-inspector',
  host: { class: 'panel flex h-full min-h-0 flex-col overflow-y-auto' },
  template: `
    <!-- Details -->
    <button type="button" class="section-h border-b border-line" (click)="toggle('details')">
      <span class="text-2xs">{{ open('details') ? '▾' : '▸' }}</span> Details
    </button>
    @if (open('details')) {
      @if (trace.nodeDetail(); as node) {
        <div class="space-y-1 border-b border-line px-2 py-2">
          <p class="break-all font-mono text-xs text-ink">{{ node.title }}</p>
          <p class="text-2xs text-ink-muted">{{ node.kind }}</p>
          @if (node.filePath) {
            <p class="break-all font-mono text-2xs text-ink-subtle" [title]="node.filePath">{{ node.filePath }}</p>
          }
          <p class="text-2xs tabular-nums text-ink-muted">in {{ node.inDegree }} · out {{ node.outDegree }}</p>
          @if (node.tags.length > 0) {
            <div class="flex flex-wrap gap-1 pt-1">
              @for (tag of node.tags; track tag) {
                <span class="chip">{{ tag }}</span>
              }
            </div>
          }
        </div>
      } @else if (trace.focus(); as focus) {
        <div class="border-b border-line px-2 py-2">
          <p class="break-all font-mono text-xs text-ink">{{ focus }}</p>
          <p class="pt-1 text-2xs text-ink-subtle">Entry focus — click a node in the trace for detail.</p>
        </div>
      } @else {
        <p class="border-b border-line px-2 py-3 text-2xs text-ink-subtle">
          Select an entry, node, or insight to inspect.
        </p>
      }
    }

    <!-- LLM context -->
    <button type="button" class="section-h border-b border-line" (click)="toggle('llm')">
      <span class="text-2xs">{{ open('llm') ? '▾' : '▸' }}</span> LLM context
      <span class="flex-1"></span>
      @if (trace.markdown()) {
        <span class="tabular-nums text-2xs text-ink-subtle">≈{{ tokenEstimate() }} tok</span>
        <button
          type="button"
          class="chip"
          [class.active]="copied()"
          (click)="copy($event)"
          title="Copy LLM context"
        >
          {{ copied() ? 'copied' : 'copy' }}
        </button>
      }
    </button>
    @if (open('llm')) {
      @if (trace.markdown(); as markdown) {
        <pre
          class="prose-zone max-h-96 overflow-y-auto whitespace-pre-wrap break-words border-b border-line px-2 py-2 font-mono text-2xs"
        >{{ markdown }}</pre>
      } @else {
        <p class="border-b border-line px-2 py-3 text-2xs text-ink-subtle">
          Trace something to build LLM-ready context.
        </p>
      }
    }

    <!-- Trail -->
    <button type="button" class="section-h border-b border-line" (click)="toggle('trail')">
      <span class="text-2xs">{{ open('trail') ? '▾' : '▸' }}</span> Trail
      <span class="flex-1"></span>
      @if (trail.pinCount() > 0) {
        <span class="chip active tabular-nums">◈ {{ trail.pinCount() }}</span>
      }
    </button>
    @if (open('trail')) {
      @for (step of trail.breadcrumb(); track step.ts; let i = $index) {
        <div
          class="list-row"
          role="button"
          tabindex="0"
          [class.selected]="i === trail.cursor()"
          (click)="jump(i)"
          (keydown.enter)="jump(i)"
          (keydown.space)="jump(i); $event.preventDefault()"
        >
          <span class="shrink-0 text-2xs text-ink-subtle">{{ stepGlyph(step) }}</span>
          <span class="min-w-0 flex-1 truncate font-mono text-xs" [title]="step.title">{{ step.title }}</span>
          <button
            type="button"
            class="shrink-0 text-2xs"
            [class.text-accent]="trail.isPinned(step)"
            [class.text-ink-subtle]="!trail.isPinned(step)"
            (click)="pin(step, $event)"
            title="Pin to export pack (p)"
          >
            ◈
          </button>
        </div>
      } @empty {
        <p class="px-2 py-3 text-2xs text-ink-subtle">
          Your exploration path collects here — pins seed the export pack.
        </p>
      }
    }
  `,
})
export class Inspector {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  protected readonly trail = inject(TrailStore);

  /** Emitted when the user jumps the trail — parent re-traces the restored step. */
  readonly restore = output<TrailStep>();

  private readonly collapsed = signal<ReadonlySet<SectionId>>(new Set());
  protected readonly copied = signal(false);

  protected readonly tokenEstimate = computed(() => Math.round(this.trace.markdown().length / 4));

  protected open(id: SectionId): boolean {
    return !this.collapsed().has(id);
  }

  protected toggle(id: SectionId): void {
    this.collapsed.update((set) => {
      const next = new Set(set);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  protected jump(index: number): void {
    const step = this.trail.jumpTo(index);
    if (step) this.restore.emit(step);
  }

  protected pin(step: TrailStep, event: Event): void {
    event.stopPropagation();
    this.trail.togglePin(step);
  }

  protected copy(event: Event): void {
    event.stopPropagation();
    void navigator.clipboard?.writeText(this.trace.markdown()).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 1500);
    });
  }

  protected stepGlyph(step: TrailStep): string {
    switch (step.kind) {
      case 'entry':
        return '⌂';
      case 'node':
        return '·';
      case 'reroot':
        return '↳';
      case 'insight':
        return '⚑';
    }
  }
}
