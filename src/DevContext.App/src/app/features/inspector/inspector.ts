import { Component, computed, DestroyRef, effect, inject, output, signal } from '@angular/core';

import { DevContextApi } from '../../data-access/devcontext-api';
import { AtlasStore } from '../../state/atlas.store';
import { TrailStore, type TrailStep } from '../../state/trail.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { Skeleton } from '../../ui/skeleton/skeleton';
import { ToastService } from '../../ui/toast/toast';
import { isTauri } from '../../core/tauri-env';
import { copyToClipboard } from '../../core/clipboard';
import { formatCompact } from '../../core/format';

type SectionId = 'details' | 'callstack' | 'insights' | 'llm' | 'trail';

const RENDER_DEBOUNCE_MS = 250;

/**
 * Inspector (F proposal §2) — the right panel. Content is driven ENTIRELY by the
 * current selection; sections collapse independently. Details fill instantly from
 * local data (selection echo, §5.2) while RPC-backed sections follow.
 *
 * LLM context renders via the Render RPC (250ms debounce, real token count from
 * `estimatedTokens`) — migrated from section-lens.ts, which this supersedes.
 *
 * TODO(W4 remainder): Call stack hosts a compact ProgressiveTraceTree at depth 2.
 * TODO(W5): Insights section filters stats().insights by the current selection.
 */
@Component({
  selector: 'app-inspector',
  imports: [Skeleton],
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
            <p class="flex items-start gap-1.5 break-all font-mono text-2xs text-ink-subtle" [title]="node.filePath">
              <span class="min-w-0 flex-1">{{ node.filePath }}</span>
              @if (node.lineNumber) {<span class="shrink-0 tabular-nums">:{{ node.lineNumber }}</span>}
              @if (isTauriEnv) {
                <button type="button" class="shrink-0 text-ink-subtle hover:text-ink hover:underline" (click)="revealInExplorer(node.filePath)" title="Reveal in Explorer">reveal</button>
              }
            </p>
          }
          <p class="text-2xs tabular-nums text-ink-muted">in {{ node.inDegree }} · out {{ node.outDegree }}</p>
          @if (node.tags.length > 0) {
            <div class="flex flex-wrap gap-1 pt-1">
              @for (tag of node.tags; track tag) {
                <span class="chip">{{ tag }}</span>
              }
            </div>
          }
          @if (reachedBy(); as r) {
            <p class="pt-1 text-2xs text-ink-muted">
              Reached by <span class="tabular-nums text-ink">{{ r.count }}</span> flow{{ r.count === 1 ? '' : 's' }}
              @if (r.incomplete) {
                <span class="text-ink-subtle"> &middot; atlas indexing, may be incomplete</span>
              }
            </p>
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

    <!-- LLM context (Render RPC, 250ms debounce — proposal §2/§5.1). A plain div, not a
         button, because the row also hosts the Copy button — nested <button>s are
         invalid HTML and Angular's DOM renderer won't sanitize that away (it never
         parses HTML text, so the browser never gets a chance to auto-correct it). -->
    <div class="section-h border-b border-line">
      <button type="button" class="flex min-w-0 flex-1 items-center gap-1" (click)="toggle('llm')">
        <span class="text-2xs">{{ open('llm') ? '▾' : '▸' }}</span> LLM context
      </button>
      @if (renderContent()) {
        <span class="tabular-nums text-2xs text-ink-subtle" [class.animate-pulse]="renderLoading()">
          ≈{{ fmtK(tokenEstimate()) }} tok
        </span>
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
    </div>
    @if (open('llm')) {
      @if (renderContent(); as markdown) {
        <pre
          class="prose-zone max-h-96 overflow-y-auto whitespace-pre-wrap break-words border-b border-line px-2 py-2 font-mono text-2xs transition-opacity"
          [class.opacity-60]="renderLoading()"
        >{{ markdown }}</pre>
      } @else if (renderLoading()) {
        <div class="space-y-1.5 border-b border-line px-2 py-2">
          <app-skeleton />
          <app-skeleton width="80%" />
          <app-skeleton width="60%" />
        </div>
      } @else if (renderError(); as err) {
        <div class="border-b border-line px-2 py-3 text-2xs text-danger">
          {{ err }} —
          <button type="button" class="text-accent hover:underline" (click)="render()">Retry</button>
        </div>
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
  private readonly atlas = inject(AtlasStore);
  private readonly api = inject(DevContextApi);
  private readonly toast = inject(ToastService);

  protected readonly isTauriEnv = isTauri();

  /** §3.4 impact lens. Null (not 0) when no node is selected — `count` can legitimately
   * be 0, so this can't be an `@if` truthiness check on a bare number. */
  protected readonly reachedBy = computed<{ count: number; incomplete: boolean } | null>(() => {
    const nodeId = this.trace.selectedNodeId();
    if (!nodeId) return null;
    return { count: this.atlas.reachedBy(nodeId).length, incomplete: this.atlas.status() !== 'done' };
  });

  /** Emitted when the user jumps the trail — parent re-traces the restored step. */
  readonly restore = output<TrailStep>();

  private readonly collapsed = signal<ReadonlySet<SectionId>>(new Set());
  protected readonly copied = signal(false);

  protected readonly renderContent = signal('');
  protected readonly tokenEstimate = signal(0);
  protected readonly renderLoading = signal(false);
  protected readonly renderError = signal<string | null>(null);

  private renderTimer: ReturnType<typeof setTimeout> | null = null;
  private renderedFocus: string | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      if (this.renderTimer) clearTimeout(this.renderTimer);
    });

    effect(() => {
      const focus = this.trace.focus();
      if (!focus) {
        this.renderedFocus = null;
        this.renderContent.set('');
        this.renderError.set(null);
        return;
      }
      if (focus === this.renderedFocus) return;
      const handle = this.session.handle();
      if (!handle) return;
      this.debouncedRender(handle, focus);
    });
  }

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
    void copyToClipboard(this.renderContent()).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 1500);
    });
  }

  protected revealInExplorer(filePath: string | undefined): void {
    if (!filePath) return;
    void import('@tauri-apps/plugin-opener')
      .then(({ revealItemInDir }) => revealItemInDir(filePath))
      .catch(() => this.toast.show('Could not reveal file — it may not exist on this machine.', 'error'));
  }

  protected fmtK(n: number): string {
    return formatCompact(n);
  }

  /** Manual retry (error state) — bypasses the "already rendered this focus" guard. */
  protected render(): void {
    const handle = this.session.handle();
    const focus = this.trace.focus();
    if (!handle || !focus) return;
    void this.doRender(handle, focus);
  }

  private debouncedRender(handle: string, focus: string): void {
    if (this.renderTimer) clearTimeout(this.renderTimer);
    this.renderTimer = setTimeout(() => void this.doRender(handle, focus), RENDER_DEBOUNCE_MS);
  }

  private async doRender(handle: string, focus: string): Promise<void> {
    this.renderLoading.set(true);
    this.renderError.set(null);
    try {
      const res = await this.api.render(handle, { focus, detail: this.trace.detail(), format: 'markdown' });
      this.renderedFocus = focus;
      this.renderContent.set(res.content);
      this.tokenEstimate.set(res.estimatedTokens);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Render failed — is the server running?';
      this.renderError.set(msg);
      this.toast.show(msg, 'error');
    } finally {
      this.renderLoading.set(false);
    }
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
      default:
        return '·';
    }
  }
}
