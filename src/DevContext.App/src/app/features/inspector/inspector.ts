import { Component, computed, effect, inject, output, signal } from '@angular/core';

import { DevContextApi } from '../../data-access/devcontext-api';
import { AtlasStore } from '../../state/atlas.store';
import { TrailStore, type TrailFlowGroup, type TrailStep } from '../../state/trail.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { ToastService } from '../../ui/toast/toast';
import { Skeleton } from '../../ui/skeleton/skeleton';
import { isTauri } from '../../core/tauri-env';
import { copyToClipboard } from '../../core/clipboard';
import { highlightCSharp } from '../../core/code-highlight';

type SectionId = 'details' | 'code' | 'callstack' | 'insights' | 'trail';

/**
 * Inspector (F proposal §2) — the right panel. Content is driven ENTIRELY by the
 * current selection; sections collapse independently. Details fill instantly from
 * local data (selection echo, §5.2) while RPC-backed sections follow.
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
          @if (node.tags.length > 0 || node.layer || node.feature) {
            <div class="flex flex-wrap gap-1 pt-1">
              @if (node.layer) {
                <span class="chip !bg-vibe-accent/15 !text-vibe-accent" title="Architecture layer">{{ node.layer }}</span>
              }
              @if (node.feature) {
                <span class="chip !bg-vibe-info/15 !text-vibe-info" title="Feature area">{{ node.feature }}</span>
              }
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

    <!-- Code (M7.1) — file path + line, reveal/copy actions. Opens source when available. -->
    <button type="button" class="section-h border-b border-line" (click)="toggle('code')">
      <span class="text-2xs">{{ open('code') ? '▾' : '▸' }}</span> Code
      @if (trace.nodeDetail()?.filePath; as fp) {
        <span class="ml-1 min-w-0 truncate font-mono text-2xs text-ink-subtle">{{ basename(fp) }}</span>
      }
    </button>
    @if (open('code')) {
      @if (trace.nodeDetail(); as node) {
        @if (node.filePath) {
          <div class="space-y-1.5 border-b border-line px-2 py-2">
            <p class="break-all font-mono text-xs text-accent" [title]="node.filePath">
              {{ node.filePath }}
              @if (node.lineNumber) {<span class="tabular-nums text-ink-subtle">:{{ node.lineNumber }}</span>}
            </p>
            <div class="flex flex-wrap gap-1">
              <button type="button" class="chip" [class.active]="codePathCopied()" (click)="copyFilePath(node.filePath)">
                {{ codePathCopied() ? 'copied' : 'copy path' }}
              </button>
              @if (isTauriEnv) {
                <button type="button" class="chip" (click)="revealInExplorer(node.filePath)">
                  reveal in explorer
                </button>
              }
              <button type="button" class="chip" (click)="loadCode(node)">
                {{ codeLoading() ? 'loading…' : 'load source' }}
              </button>
            </div>
            @if (codeContent()) {
              <pre class="code-block max-h-80 overflow-y-auto whitespace-pre border border-line bg-base p-2 font-mono text-2xs leading-relaxed"><code [innerHTML]="highlightedCode()"></code></pre>
            } @else if (codeLoading()) {
              <div class="space-y-1 py-2">
                <app-skeleton />
                <app-skeleton width="80%" />
                <app-skeleton width="60%" />
                <app-skeleton width="90%" />
              </div>
            } @else if (codeError(); as err) {
              <p class="text-2xs text-ink-muted">{{ err }}</p>
            }
          </div>
        } @else {
          <p class="border-b border-line px-2 py-3 text-2xs text-ink-subtle">
            No source file path for this node.
          </p>
        }
      } @else {
        <p class="border-b border-line px-2 py-3 text-2xs text-ink-subtle">
          Select a node to view its source location.
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
      @for (group of trail.groupedBreadcrumb(); track group.fromIndex; let gi = $index) {
        @if (group.grouped) {
          <!-- M7.3: Grouped flow steps — collapsed by default, expand to see individual steps -->
          <div
            class="list-row cursor-pointer"
            role="button"
            tabindex="0"
            [class.selected]="isCursorInGroup(group)"
            (click)="toggleGroup(group.fromIndex)"
            (keydown.enter)="toggleGroup(group.fromIndex)"
            (keydown.space)="toggleGroup(group.fromIndex); $event.preventDefault()"
          >
            <span class="shrink-0 text-2xs text-ink-subtle">
              {{ isGroupExpanded(group.fromIndex) ? '▾' : '▸' }}
            </span>
            <span class="chip active tabular-nums shrink-0">{{ group.steps.length }}</span>
            <span class="min-w-0 flex-1 truncate font-mono text-xs text-ink-muted" [title]="group.steps[0].title">
              {{ group.steps[0].title }}
            </span>
            <span class="shrink-0 text-2xs text-ink-subtle">flow</span>
          </div>
          @if (isGroupExpanded(group.fromIndex)) {
            @for (step of group.steps; track step.ts; let si = $index) {
              <div
                class="list-row pl-6"
                role="button"
                tabindex="0"
                [class.selected]="(group.fromIndex + si) === trail.cursor()"
                (click)="jump(group.fromIndex + si)"
                (keydown.enter)="jump(group.fromIndex + si)"
                (keydown.space)="jump(group.fromIndex + si); $event.preventDefault()"
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
            }
          }
        } @else {
          <!-- Solo step (ungrouped) -->
          @for (step of group.steps; track step.ts) {
          <div
            class="list-row"
            role="button"
            tabindex="0"
            [class.selected]="group.fromIndex === trail.cursor()"
            (click)="jump(group.fromIndex)"
            (keydown.enter)="jump(group.fromIndex)"
            (keydown.space)="jump(group.fromIndex); $event.preventDefault()"
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
          }
        }
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

  /** Code tab (M7.1) — source content loaded via render RPC with full-membership body detail. */
  protected readonly codeContent = signal('');
  protected readonly codeLoading = signal(false);
  protected readonly codeError = signal<string | null>(null);
  protected readonly codePathCopied = signal(false);
  protected readonly highlightedCode = computed(() => highlightCSharp(this.codeContent()));
  private codeNodeId: string | null = null;
  /** M7.3: Which trail groups are expanded (keyed by fromIndex). Collapsed by default. */
  private readonly expandedGroups = signal<ReadonlySet<number>>(new Set());

  constructor() {
    // M7.1: Auto-open Code tab and clear stale content when a node is selected.
    effect(() => {
      const node = this.trace.nodeDetail();
      if (node?.filePath) {
        this.collapsed.update((set) => {
          if (!set.has('code')) return set;
          const next = new Set(set);
          next.delete('code');
          return next;
        });
        // Clear stale code content when node changes
        if (node.id !== this.codeNodeId) {
          this.codeContent.set('');
          this.codeError.set(null);
          this.codeNodeId = node.id;
        }
      }
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

  /** M7.3: Whether the cursor falls within this group's step range. */
  protected isCursorInGroup(group: TrailFlowGroup): boolean {
    const c = this.trail.cursor();
    return c >= group.fromIndex && c <= group.toIndex;
  }

  /** M7.3: Expand/collapse a trail flow group. */
  protected isGroupExpanded(fromIndex: number): boolean {
    return this.expandedGroups().has(fromIndex);
  }

  protected toggleGroup(fromIndex: number): void {
    this.expandedGroups.update((set) => {
      const next = new Set(set);
      if (next.has(fromIndex)) next.delete(fromIndex);
      else next.add(fromIndex);
      return next;
    });
  }

  protected revealInExplorer(filePath: string | undefined): void {
    if (!filePath) return;
    void import('@tauri-apps/plugin-opener')
      .then(({ revealItemInDir }) => revealItemInDir(filePath))
      .catch(() => this.toast.show('Could not reveal file — it may not exist on this machine.', 'error'));
  }

  /** M7.1: Copy file path to clipboard with visual feedback. */
  protected copyFilePath(filePath: string | undefined): void {
    if (!filePath) return;
    void copyToClipboard(filePath).then(() => {
      this.codePathCopied.set(true);
      setTimeout(() => this.codePathCopied.set(false), 2000);
    });
  }

  /** Gap 1: Load raw source code for the selected node via the readSource RPC. */
  protected loadCode(node: { id: string; title: string; filePath?: string }): void {
    const handle = this.session.handle();
    if (!handle) return;
    this.codeLoading.set(true);
    this.codeError.set(null);
    this.codeContent.set('');

    this.api
      .readSource(handle, node.id)
      .then((res) => {
        this.codeContent.set(res.content);
      })
      .catch((err) => {
        const msg = err instanceof Error ? err.message : 'Failed to load source';
        this.codeError.set(msg);
      })
      .finally(() => this.codeLoading.set(false));
  }

  /** M7.1: Extract base filename from a full path. */
  protected basename(path: string): string {
    return path.replace(/^.*[/\\]/, '');
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
