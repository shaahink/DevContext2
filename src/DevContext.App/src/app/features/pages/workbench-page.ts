import { Component, computed, effect, inject, OnDestroy, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AtlasStore } from '../../state/atlas.store';
import { PrefsStore } from '../../state/prefs.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore, type TrailStep } from '../../state/trail.store';
import { type EntryVm } from '../../models/view-models';
import { TrailBar } from '../../shell/trail-bar';
import { AuditTable } from '../explorer/audit-table';
import { EntryDeck } from '../explorer/entry-deck';
import { Stage, type StageAltitude } from '../explorer/stage';
import { ExportDrawer } from '../export/export-drawer';
import { Inspector } from '../inspector/inspector';

const TRACE_DEBOUNCE_MS = 150;
/** Inspector width per dock level (% of the workbench). Level 3 = focus mode. */
const DOCK_WIDTHS = [0, 30, 40, 100] as const;
const VALID_ALTITUDES: readonly StageAltitude[] = ['system', 'flow', 'node'];

/**
 * Workbench (F proposal §2) — Entry Deck │ Stage │ Inspector around ONE selection.
 * This page owns what selection MEANS: deck scrubs commit a debounced trace + trail
 * push; stage node clicks select + push; trail restores re-trace WITHOUT pushing.
 *
 * URL state (`?focus&view&kind&q`, proposal §8.3) is read once on load (deep-link
 * compat — a restoreFocus effect self-destructs after firing) and mirrored back with
 * `replaceUrl: true` so it never grows browser history, matching TracePage's existing
 * `?focus` convention.
 *
 * TODO(W4 remainder): dock drag handles (Ctrl+Shift+L is the only control today).
 * Global shortcuts (Ctrl+Shift+L, Ctrl+Z/Y, Esc-ladder, p, Alt+←/→) are deliberately
 * kept window-level HERE rather than promoted to workspace-shell: they all act on the
 * Inspector/Trail/Trace, which only exist while this page is mounted, so promoting
 * would need the same logic duplicated for no benefit until other pages grow a Trail.
 * NOTE: Atlas indexing itself starts in `SessionStore.analyze()`'s success path (fires
 *   on analysis-ready, regardless of route) — this page only pauses/resumes it around
 *   user-initiated traces (see effect below). Progress reads from `atlas.progressLabel()`,
 *   surfaced in the statusbar segment.
 */
@Component({
  selector: 'app-workbench-page',
  imports: [EntryDeck, Stage, Inspector, TrailBar, RouterLink, AuditTable, ExportDrawer],
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
            [(activeKind)]="deckKind"
            [(filterText)]="deckFilterText"
            (selectionChange)="onEntry($event)"
            (openAudit)="onOpenAudit()"
            (projectFilterCleared)="projectFilter.set(null)"
          />
          <app-stage
            class="min-w-0 flex-1"
            [(altitude)]="stageAltitude"
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

    <app-audit-table
      [open]="auditOpen()"
      [groups]="session.entryGroups()"
      (selectionChange)="onAuditSelect($event)"
      (dismissed)="auditOpen.set(false)"
    />

    <app-export-drawer
      [open]="exportOpen()"
      (dismissed)="exportOpen.set(false)"
    />
  `,
})
export class WorkbenchPage implements OnDestroy {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  protected readonly trail = inject(TrailStore);
  private readonly atlas = inject(AtlasStore);
  private readonly prefs = inject(PrefsStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly dockLevel = signal(this.prefs.dockLevel());
  protected readonly dockWidth = computed(() => DOCK_WIDTHS[this.dockLevel()]);
  /** Set by Stage's System altitude (project click); cleared from the deck's own chip. */
  protected readonly projectFilter = signal<string | null>(null);
  /** Lifted from Stage/EntryDeck's `model()`s so they can mirror into `?view&kind&q`. */
  protected readonly stageAltitude = signal<StageAltitude>('flow');
  protected readonly deckKind = signal<string | null>(null);
  protected readonly deckFilterText = signal('');
  protected readonly auditOpen = signal(false);
  protected readonly exportOpen = signal(false);

  private pendingTrace: ReturnType<typeof setTimeout> | null = null;
  /** Last dock level > 0, so Ctrl+Shift+L toggles 0 ↔ last instead of cycling. */
  private lastVisibleDock = this.dockLevel() > 0 ? this.dockLevel() : 2;

  constructor() {
    // Background flow indexing itself starts on analysis-ready (SessionStore.analyze()'s
    // success path), not here — that's what makes Home's Top Flows work without ever
    // visiting /explore. This page only cares about pausing it during a user trace.

    // User latency beats background indexing: park the atlas while a trace is in flight.
    effect(() => {
      if (this.trace.loading()) this.atlas.pause();
      else this.atlas.resume();
    });

    // Read URL state once (deep-link compat, proposal §8.3) — never re-read reactively,
    // since we're the ones writing it below (would otherwise fight the write effect).
    const params = this.route.snapshot.queryParamMap;
    const urlView = params.get('view');
    if (isStageAltitude(urlView)) this.stageAltitude.set(urlView);
    const urlKind = params.get('kind');
    if (urlKind) this.deckKind.set(urlKind);
    const urlQuery = params.get('q');
    if (urlQuery) this.deckFilterText.set(urlQuery);

    const urlFocus = params.get('focus');
    if (urlFocus) {
      const restoreFocus = effect(() => {
        const handle = this.session.handle();
        if (!handle || !this.session.ready()) return;
        restoreFocus.destroy();
        void this.trace.trace(handle, urlFocus);
      });
    }

    // Mirror state back — replaceUrl so it never grows browser history (same convention
    // as TracePage's existing `?focus`).
    effect(() => {
      const queryParams = {
        focus: this.trace.focus() || null,
        view: this.stageAltitude() === 'flow' ? null : this.stageAltitude(),
        kind: this.deckKind(),
        q: this.deckFilterText() || null,
      };
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams,
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
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
    this.auditOpen.set(true);
  }

  /** Audit table row "Trace" — same as picking the entry in the deck (trace + trail
   * push), then close the overlay since that's the point of selecting from it. */
  protected onAuditSelect(entry: EntryVm): void {
    this.auditOpen.set(false);
    const handle = this.session.handle();
    if (!handle) return;
    this.trail.push({ kind: 'entry', id: entry.nodeId, title: entry.title, focus: entry.focus });
    void this.trace.trace(handle, entry.focus);
  }

  protected onGlobalKey(event: KeyboardEvent): void {
    if (event.key === 'Escape' && !event.ctrlKey && !event.metaKey && !event.altKey) {
      this.onEscape();
      return;
    }
    if (event.ctrlKey && event.shiftKey && event.key.toLowerCase() === 'l') {
      event.preventDefault();
      this.toggleDock();
      return;
    }
    if (event.ctrlKey && !event.shiftKey && event.key.toLowerCase() === 'e') {
      event.preventDefault();
      this.exportOpen.set(true);
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
      return;
    }
    if (event.altKey && (event.key === 'ArrowLeft' || event.key === 'ArrowRight')) {
      event.preventDefault();
      const step = event.key === 'ArrowLeft' ? this.trail.undo() : this.trail.redo();
      if (step) this.onRestore(step);
      return;
    }
    if (event.key === 'p' && !event.ctrlKey && !event.metaKey && !event.altKey && !isTypingTarget(event.target)) {
      const current = this.trail.current();
      if (current) {
        event.preventDefault();
        this.trail.togglePin(current);
      }
    }
  }

  /** Esc-ladder (proposal §8.4): cancel in-flight trace → close overlay → deselect node
   * → clear focus → clear deck filter. The full spec's "unpin peek" rung (between close
   * overlay and deselect node) is still a TODO — node-peek doesn't exist yet (W7). Runs
   * unconditionally (not gated on focus) — that's the point of a ladder: Escape always
   * does the highest-priority thing that's currently true, same as VS Code's. */
  private onEscape(): void {
    if (this.trace.loading()) {
      this.trace.cancelTrace();
      return;
    }
    if (this.auditOpen()) {
      this.auditOpen.set(false);
      return;
    }
    if (this.exportOpen()) {
      this.exportOpen.set(false);
      return;
    }
    if (this.trace.selectedNodeId()) {
      this.trace.deselectNode();
      return;
    }
    if (this.trace.focus()) {
      this.trace.clear();
      return;
    }
    if (this.deckFilterText() || this.deckKind()) {
      this.deckFilterText.set('');
      this.deckKind.set(null);
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

function isStageAltitude(value: string | null): value is StageAltitude {
  return value !== null && (VALID_ALTITUDES as readonly string[]).includes(value);
}

function isTypingTarget(target: EventTarget | null): boolean {
  const tag = (target as HTMLElement | null)?.tagName;
  return tag === 'INPUT' || tag === 'TEXTAREA';
}
