import { Component, computed, effect, inject, OnDestroy, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AtlasStore } from '../../state/atlas.store';
import { PrefsStore } from '../../state/prefs.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore, type TrailStep } from '../../state/trail.store';
import { WorkspaceStore } from '../../state/workspace.store';
import { type EntryVm } from '../../models/view-models';
import { TrailBar } from '../../shell/trail-bar';
import { EntryDeck } from '../explorer/entry-deck';
import { Stage } from '../explorer/stage';
import { Inspector } from '../inspector/inspector';

const TRACE_DEBOUNCE_MS = 150;
/** Inspector width per dock level (% of the workbench). Level 3 = focus mode. */
const DOCK_WIDTHS = [0, 30, 40, 100] as const;

/**
 * Workbench (F proposal §2) — Entry Deck │ Stage │ Inspector around ONE selection.
 * This page owns what selection MEANS: deck scrubs commit a debounced trace + trail
 * push; stage node clicks select + push; trail restores re-trace WITHOUT pushing.
 *
 * TODO(W4): URL state (?focus&view) read/write; Esc-ladder; audit-table overlay
 *   (Shift+E currently no-ops via openAudit); dock drag handles; move global
 *   shortcuts (Ctrl+Shift+L, Ctrl+Z/Y) into workspace-shell so they work on every
 *   page — window-level here is a stopgap.
 * NOTE: Atlas indexing auto-starts once per snapshot handle (see effect below) and
 *   its progress is read from `atlas.progressLabel()` — surface it in the statusbar
 *   segment in W5.
 */
@Component({
  selector: 'app-workbench-page',
  imports: [EntryDeck, Stage, Inspector, TrailBar, RouterLink],
  host: {
    class: 'flex h-full min-h-0 flex-col',
    '(window:keydown)': 'onGlobalKey($event)',
  },
  template: `
    <app-trail-bar (restore)="onRestore($event)" />

    @if (session.ready()) {
      <div class="flex min-h-0 flex-1">
        @if (dockLevel() < 3) {
          <app-entry-deck
            class="w-64 shrink-0 border-r border-line"
            [groups]="session.entryGroups()"
            [selectedFocus]="trace.focus()"
            [projectFilter]="projectFilter()"
            (selectionChange)="onEntry($event)"
            (openAudit)="onOpenAudit()"
            (projectFilterCleared)="projectFilter.set(null)"
          />
          <app-stage
            class="min-w-0 flex-1"
            (nodeSelected)="onNode($event)"
            (retrace)="onRetrace($event)"
            (projectSelected)="projectFilter.set($event)"
          />
        }
        @if (dockLevel() > 0) {
          <app-inspector
            class="shrink-0 border-l border-line"
            [style.width.%]="dockWidth()"
            (restore)="onRestore($event)"
          />
        }
      </div>
    } @else {
      <div class="flex flex-1 flex-col items-center justify-center gap-2 text-xs text-ink-subtle">
        <p>The Workbench needs an analyzed repo.</p>
        <a routerLink="/" class="text-accent hover:underline">Analyze one on the Home screen →</a>
      </div>
    }
  `,
})
export class WorkbenchPage implements OnDestroy {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  protected readonly trail = inject(TrailStore);
  private readonly atlas = inject(AtlasStore);
  private readonly workspace = inject(WorkspaceStore);
  private readonly prefs = inject(PrefsStore);

  protected readonly dockLevel = signal(this.prefs.dockLevel());
  protected readonly dockWidth = computed(() => DOCK_WIDTHS[this.dockLevel()]);
  /** Set by Stage's System altitude (project click); cleared from the deck's own chip. */
  protected readonly projectFilter = signal<string | null>(null);

  private pendingTrace: ReturnType<typeof setTimeout> | null = null;
  /** Last dock level > 0, so Ctrl+Shift+L toggles 0 ↔ last instead of cycling. */
  private lastVisibleDock = this.dockLevel() > 0 ? this.dockLevel() : 2;
  private atlasStartedFor: string | null = null;

  constructor() {
    // Kick off background flow indexing once per snapshot (§3.1). Captures the tabId
    // NOW — results land in the tab that asked even if the user switches away.
    effect(() => {
      const handle = this.session.handle();
      const tabId = this.workspace.activeId();
      if (!handle || !tabId || !this.session.ready()) return;
      if (this.atlasStartedFor === handle) return;
      this.atlasStartedFor = handle;
      const entries = this.session.entryGroups().flatMap((g) => g.entries);
      this.atlas.start(tabId, handle, entries);
    });

    // User latency beats background indexing: park the atlas while a trace is in flight.
    effect(() => {
      if (this.trace.loading()) this.atlas.pause();
      else this.atlas.resume();
    });
  }

  ngOnDestroy(): void {
    if (this.pendingTrace !== null) clearTimeout(this.pendingTrace);
  }

  /** Deck scrub — debounced so j/k sweeps commit once, then trace + trail push. */
  protected onEntry(entry: EntryVm): void {
    if (this.pendingTrace !== null) clearTimeout(this.pendingTrace);
    this.pendingTrace = setTimeout(() => {
      this.pendingTrace = null;
      const handle = this.session.handle();
      if (!handle) return;
      this.trail.push({ kind: 'entry', id: entry.nodeId, title: entry.title, focus: entry.focus });
      void this.trace.trace(handle, entry.focus);
    }, TRACE_DEBOUNCE_MS);
  }

  /** Stage node click — immediate select + trail push (no debounce: it's deliberate). */
  protected onNode(nodeId: string): void {
    void this.trace.selectNode(nodeId);
    this.trail.push({
      kind: 'node',
      id: nodeId,
      title: shortNodeTitle(nodeId),
      focus: this.trace.focus() ?? '',
    });
  }

  /** Double-click on a Flow-altitude graph node — re-root the tree at it (proposal §2),
   * distinct from a plain click which only selects it for the Inspector. Client-side only
   * (see `TraceStore.reroot`'s doc comment for why); a no-op if the node isn't in the
   * currently-loaded tree (stale click). */
  protected onRetrace(nodeId: string): void {
    if (!this.trace.reroot(nodeId)) return;
    this.trail.push({ kind: 'reroot', id: nodeId, title: shortNodeTitle(nodeId), focus: '' });
  }

  /** Trail undo/redo/jump — restore WITHOUT pushing (that would fork the history). */
  protected onRestore(step: TrailStep): void {
    if (step.kind === 'reroot') {
      this.trace.reroot(step.id);
      return;
    }
    const handle = this.session.handle();
    if (!handle || !step.focus) return;
    if (step.focus !== this.trace.focus()) void this.trace.trace(handle, step.focus);
    if (step.kind === 'node') void this.trace.selectNode(step.id);
  }

  protected onOpenAudit(): void {
    // TODO(W4): open the full sortable audit table overlay (today's section-entries).
  }

  protected onGlobalKey(event: KeyboardEvent): void {
    if (event.ctrlKey && event.shiftKey && event.key.toLowerCase() === 'l') {
      event.preventDefault();
      this.toggleDock();
      return;
    }
    if (event.ctrlKey && !event.shiftKey && event.key.toLowerCase() === 'z') {
      event.preventDefault();
      const step = this.trail.undo();
      if (step) this.onRestore(step);
      return;
    }
    if (event.ctrlKey && (event.key.toLowerCase() === 'y' || (event.shiftKey && event.key.toLowerCase() === 'z'))) {
      event.preventDefault();
      const step = this.trail.redo();
      if (step) this.onRestore(step);
    }
  }

  private toggleDock(): void {
    const level = this.dockLevel();
    if (level > 0) {
      this.lastVisibleDock = level;
      this.dockLevel.set(0);
    } else {
      this.dockLevel.set(this.lastVisibleDock);
    }
    this.prefs.setDockLevel(this.dockLevel());
  }
}

function shortNodeTitle(nodeId: string): string {
  const parts = nodeId.split(/[./:]/).filter(Boolean);
  return parts.length > 1 ? parts.slice(-2).join('.') : nodeId;
}
