import { Component, computed, effect, inject, OnDestroy, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { nodeIdLabel } from '../../core/format';
import { AtlasStore } from '../../state/atlas.store';
import { NodePeekStore } from '../../state/node-peek.store';
import { PrefsStore } from '../../state/prefs.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore, type TrailStep } from '../../state/trail.store';
import { type EntryVm } from '../../models/view-models';
import { TrailBar } from '../../shell/trail-bar';
import { EntryBrowser } from '../entry-browser/entry-browser';
import { EntryDeck } from '../explorer/entry-deck';
import { LibraryWorkbench } from '../library/library-workbench';
import { type LensId } from '../explorer/lens-switcher';
import { Stage, type FlowMode, type StageAltitude } from '../explorer/stage';
import { Inspector } from '../inspector/inspector';
import { TableLens } from '../table-lens/table-lens';
import { ToastService } from '../../ui/toast/toast';

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
 * Dock: Ctrl+Shift+L cycles the three levels; the drag handle between Stage and Inspector
 * (M1.2, closing the W4 remainder) overrides the level's width continuously, clamped 20–70%
 * and persisted. Level 3 (focus) has no handle — there is nothing to its left to resize.
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
  imports: [EntryDeck, Stage, Inspector, TrailBar, RouterLink, TableLens, LibraryWorkbench, EntryBrowser],
  host: {
    class: 'flex h-full min-h-0 flex-col',
    '(window:keydown)': 'onGlobalKey($event)',
  },
  template: `
    <app-trail-bar (restore)="onRestore($event)" />

    @if (session.ready() && isLibrary()) {
      <!-- D4.4 (F1): archetype Library routes Explore to the public-surface browser —
           a library has surface, not entry-point flows. -->
      <app-library-workbench class="min-h-0 flex-1" />
    } @else if (session.ready()) {
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
            [(flowMode)]="stageFlowMode"
            [(lensModel)]="stageLens"
            (nodeSelected)="onNode($event)"
            (retrace)="onRetrace($event)"
            (projectSelected)="projectFilter.set($event)"
            (tableRequested)="browserOpen.set(true)"
            (commandSelected)="onEntry($event)"
          />
        }
        @if (dockLevel() > 0 && dockLevel() < 3) {
          <!--
            W4 remainder (M1.2): the dock drag handle. Ctrl+Shift+L was the only control, so the
            inspector was 30/40/100% or nothing. Keyboard-reachable (Left/Right nudge 2%, Home
            restores the level's width), and it reports its width to a screen reader as a real
            separator rather than a decorative bar.
          -->
          <div
            role="separator"
            aria-orientation="vertical"
            aria-label="Resize inspector"
            [attr.aria-valuenow]="dockWidth()"
            aria-valuemin="20"
            aria-valuemax="70"
            tabindex="0"
            data-testid="dock-resizer"
            class="w-1 shrink-0 cursor-col-resize bg-line transition-colors hover:bg-accent focus-visible:bg-accent focus-visible:outline-none"
            [class.bg-accent]="dockResizing()"
            [title]="'Drag to resize (' + dockWidth() + '%) — double-click to reset'"
            (pointerdown)="onDockResizeStart($event)"
            (dblclick)="resetDockWidth()"
            (keydown)="onDockResizeKey($event)"
          ></div>
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

    <!-- D4.5 (L5): the entry BROWSER is the primary all-entries surface; the raw
         audit table survives as the Shift+E power view. -->
    @if (browserOpen()) {
      <div class="fixed inset-0 z-50 flex flex-col bg-base">
        <app-entry-browser
          [groups]="session.entryGroups()"
          (selectionChange)="onEntry($event)"
          (dismissed)="browserOpen.set(false)"
          (tableRequest)="browserOpen.set(false); tableOpen.set(true)"
        />
      </div>
    }

    @if (tableOpen()) {
      <div class="fixed inset-0 z-50 flex flex-col bg-base">
        <app-table-lens
          [groups]="session.entryGroups()"
          (dismissed)="tableOpen.set(false)"
        />
      </div>
    }

    @if (vPending()) {
      <div class="fixed bottom-12 left-1/2 z-50 -translate-x-1/2 overlay-float px-3 py-1.5 font-mono text-xs text-ink-muted">
        Stage: <kbd class="text-accent">t</kbd>ree · <kbd class="text-accent">g</kbd>raph · <kbd class="text-accent">s</kbd>ystem · <kbd class="text-accent">n</kbd>ode
      </div>
    }
  `,
})
export class WorkbenchPage implements OnDestroy {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  protected readonly trail = inject(TrailStore);
  private readonly atlas = inject(AtlasStore);
  private readonly nodePeek = inject(NodePeekStore);
  private readonly prefs = inject(PrefsStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly dockLevel = signal(this.prefs.dockLevel());
  /** W4 remainder (M1.2) — a width the user dragged to, in % of the workbench. Null means "use the
   * level's own width", so Ctrl+Shift+L keeps its three crisp stops and the drag is an override on
   * top rather than a fourth state to reconcile. */
  protected readonly dockWidthOverride = signal<number | null>(this.prefs.dockWidth());
  protected readonly dockWidth = computed(() =>
    this.dockLevel() === 3 ? DOCK_WIDTHS[3] : (this.dockWidthOverride() ?? DOCK_WIDTHS[this.dockLevel()]));
  /** True while a resize drag is in flight — suppresses the panels' transitions and text selection. */
  protected readonly dockResizing = signal(false);
  /** Set by Stage's System altitude (project click); cleared from the deck's own chip. */
  protected readonly projectFilter = signal<string | null>(null);
  /** Lifted from Stage/EntryDeck's `model()`s so they can mirror into `?view&kind&q`. */
  protected readonly stageAltitude = signal<StageAltitude>('flow');
  protected readonly stageFlowMode = signal<FlowMode>('tree');
  protected readonly stageLens = signal<LensId>('flow');
  protected readonly deckKind = signal<string | null>(null);
  protected readonly deckFilterText = signal('');
  protected readonly tableOpen = signal(false);
  /** D4.5 (L5) — the grouped/ranked entry browser (primary); table = Shift+E power view. */
  protected readonly browserOpen = signal(false);
  /** D4.4 (F1) — Library archetype swaps the whole explore surface for the workbench. */
  protected readonly isLibrary = computed(() => this.session.mapResponse()?.isLibrary ?? false);

  private pendingTrace: ReturnType<typeof setTimeout> | null = null;
  /** Last dock level > 0, so Ctrl+Shift+L toggles 0 ↔ last instead of cycling. */
  private lastVisibleDock = this.dockLevel() > 0 ? this.dockLevel() : 2;
  /** `v` prefix for stage altitude switching (§8.4 "v t/v g/v s/v n"), same 1.5s-window
   * chord pattern as workspace-shell.ts's `g` prefix for view nav. */
  private vTimer: ReturnType<typeof setTimeout> | null = null;
  protected readonly vPending = signal(false);

  constructor() {
    // Background flow indexing starts on analysis-ready (SessionStore.analyze()'s success
    // path), not here — that's what makes Home's Top Flows work without ever visiting
    // /explore. T7.4: the index is ONE memoized server call now, so the old pause-while-
    // a-user-trace-is-in-flight parking (and the whole pause/resume surface) is gone.

    // Read URL state once (deep-link compat, proposal §8.3) — never re-read reactively,
    // since we're the ones writing it below (would otherwise fight the write effect).
    const params = this.route.snapshot.queryParamMap;
    const urlView = params.get('view');
    if (isStageAltitude(urlView)) this.stageAltitude.set(urlView);
    const urlLens = params.get('lens');
    if (isLensId(urlLens)) this.stageLens.set(urlLens);
    else if (urlView === 'system') {
      // T6.2 — lens default per archetype, only when landing straight in the System view
      // (hero/atlas clicks) with no explicit lens: microservices read best grouped by
      // service, layered monoliths by layer, vertical slices by feature. The flow lens
      // stays the default everywhere else — the first-contact trace is the flagship.
      const defaultLens = effect(() => {
        const map = this.session.mapResponse();
        if (!map) return;
        defaultLens.destroy();
        if (this.stageLens() !== 'flow') return; // user already picked one
        if (/microservice/i.test(map.archetype)) this.stageLens.set('service');
        else if (/NLayer|Onion|CleanArchitecture/i.test(map.style)) this.stageLens.set('layer');
        else if (/VerticalSlices/i.test(map.style)) this.stageLens.set('feature');
      });
    }
    const urlKind = params.get('kind');
    if (urlKind) this.deckKind.set(urlKind);
    const urlQuery = params.get('q');
    if (urlQuery) this.deckFilterText.set(urlQuery);
    const urlProject = params.get('project');
    if (urlProject) this.projectFilter.set(urlProject);

    const urlFocus = params.get('focus');
    if (urlFocus) {
      const restoreFocus = effect(() => {
        const handle = this.session.handle();
        if (!handle || !this.session.ready()) return;
        restoreFocus.destroy();
        if (urlView === 'node') {
          this.trace.selectNode(urlFocus);
        } else {
          void this.trace.trace(handle, urlFocus);
        }
      });
    }

    // Mirror state back — replaceUrl so it never grows browser history (same convention
    // as TracePage's existing `?focus`).
    effect(() => {
      const queryParams = {
        focus: this.trace.focus() || null,
        view: this.stageAltitude() === 'flow' ? null : this.stageAltitude(),
        lens: this.stageLens() === 'flow' ? null : this.stageLens(),
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
    if (this.vTimer !== null) clearTimeout(this.vTimer);
  }

  /** N1.2 (audit §3.A) — `p` used to toggle the pin and show NOTHING: the only feedback was a
   * glyph turning accent-coloured in the inspector, which is closed at dock level 0, and with no
   * current step it returned in silence. Pins seed the Studio pack now (context-studio.ts
   * onTrailSeed), so the toast says what was pinned, how many are held, and where they go. */
  protected onPin(): void {
    const current = this.trail.current();
    if (!current) {
      this.toast.show('Nothing to pin — pick an entry or a node first', 'info');
      return;
    }
    const wasPinned = this.trail.isPinned(current);
    this.trail.togglePin(current);
    const count = this.trail.pinCount();
    this.toast.show(
      wasPinned
        ? `Unpinned ${current.title} — ${count} pinned`
        : `Pinned ${current.title} — ${count} pinned, seeding Context Studio's pack`,
      wasPinned ? 'info' : 'success',
    );
  }

  /** Deck scrub — debounced so j/k sweeps commit once, then trace + trail push. */
  protected onEntry(entry: EntryVm): void {
    if (this.pendingTrace !== null) clearTimeout(this.pendingTrace);
    this.pendingTrace = window.setTimeout(() => {
      this.pendingTrace = null;
      const handle = this.session.handle();
      if (!handle) return;
      this.trail.push({ kind: 'entry', id: entry.nodeId, title: entry.title, focus: entry.focus });
      void this.trace.trace(handle, entry.focus).then(() => {
        if (this.trace.found()) void this.trace.selectNode(entry.nodeId);
      });
    }, TRACE_DEBOUNCE_MS);
  }

  /** Stage node click — immediate select + trail push (no debounce: it's deliberate). */
  protected onNode(nodeId: string): void {
    const title = this.crumbTitle(nodeId);
    void this.trace.selectNode(nodeId);
    this.trail.push({
      kind: 'node',
      id: nodeId,
      title,
      focus: this.trace.focus() ?? '',
    });
  }

  /** Double-click on a Flow-altitude graph node — re-root the tree at it (proposal §2),
   * distinct from a plain click which only selects it for the Inspector. Client-side only
   * (see `TraceStore.reroot`'s doc comment for why); a no-op if the node isn't in the
   * currently-loaded tree (stale click). */
  protected onRetrace(nodeId: string): void {
    const title = this.crumbTitle(nodeId);
    if (!this.trace.reroot(nodeId)) return;
    this.trail.push({ kind: 'reroot', id: nodeId, title, focus: '' });
  }

  /** A trail crumb's text. R3 D-4 (G6.2): this used to be `shortNodeTitle(nodeId)` — the node id
   * split on dot/colon, last two segments joined — the same string surgery G6.1 deleted from the
   * hub radar, which printed metadata arity in a name and let the node kind read as a namespace
   * ("Service.WebApp"). The clicked node is in the loaded tree and the tree carries the graph's own
   * title, so read it; `nodeIdLabel` is only for the stale-click case where it is not. */
  private crumbTitle(nodeId: string): string {
    return this.trace.titleFor(nodeId) ?? nodeIdLabel(nodeId);
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

  /** D4.5 (L5) — the deck's "browse all" affordance opens the grouped browser now;
   * the raw table stays on Shift+E (power view). */
  protected onOpenAudit(): void {
    this.browserOpen.set(true);
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
      if (isTypingTarget(event.target)) return;
      event.preventDefault();
      void this.router.navigateByUrl('/context');
      return;
    }
    if (event.shiftKey && !event.ctrlKey && !event.metaKey && !event.altKey && event.key.toLowerCase() === 'e') {
      if (isTypingTarget(event.target)) return;
      event.preventDefault();
      this.tableOpen.set(true);
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
      event.preventDefault();
      this.onPin();
      return;
    }
    if (event.key === 'v' && !event.ctrlKey && !event.metaKey && !event.altKey && !isTypingTarget(event.target)) {
      event.preventDefault();
      this.vPending.set(true);
      if (this.vTimer) clearTimeout(this.vTimer);
      this.vTimer = setTimeout(() => this.vPending.set(false), 1500);
      return;
    }
    if (this.vPending()) {
      this.vPending.set(false);
      if (this.vTimer) clearTimeout(this.vTimer);
      switch (event.key) {
        case 't':
          event.preventDefault();
          this.stageLens.set('flow');
          this.stageAltitude.set('flow');
          this.stageFlowMode.set('tree');
          break;
        case 'g':
          event.preventDefault();
          this.stageLens.set('flow');
          this.stageAltitude.set('flow');
          this.stageFlowMode.set('graph');
          break;
        case 's':
          event.preventDefault();
          this.stageLens.set('service');
          this.stageAltitude.set('system');
          break;
        case 'n':
          event.preventDefault();
          this.stageLens.set('flow');
          this.stageAltitude.set('node');
          break;
      }
    }
  }

  /** Esc-ladder (proposal §8.4): cancel in-flight trace → close overlay → unpin peek →
   * deselect node → clear focus → clear deck filter. Runs unconditionally (not gated on
   * focus) — that's the point of a ladder: Escape always does the highest-priority thing
   * that's currently true, same as VS Code's. The peek rung must come from here rather
   * than `NodePeek`'s own standalone `window:keydown.escape` fallback (which only
   * matters outside the workbench, e.g. Home/Atlas) — otherwise a single Escape press
   * both dismisses the peek AND falls through to deselect the node in the same tick. */
  private onEscape(): void {
    if (this.trace.loading()) {
      this.trace.cancelTrace();
      return;
    }
    if (this.tableOpen()) {
      this.tableOpen.set(false);
      return;
    }
    if (this.browserOpen()) {
      this.browserOpen.set(false);
      return;
    }
    if (this.nodePeek.nodeId()) {
      this.nodePeek.dismiss();
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
    if (level >= 3) {
      this.lastVisibleDock = 3;
      this.dockLevel.set(0);
    } else if (level > 0) {
      this.lastVisibleDock = level;
      this.dockLevel.set(level + 1);
    } else {
      this.dockLevel.set(this.lastVisibleDock || 2);
    }
    // Asking for a level is asking for THAT width — otherwise a stale drag would make
    // Ctrl+Shift+L look broken (the level number changes, the panel doesn't move).
    this.resetDockWidth();
    this.prefs.setDockLevel(this.dockLevel());
  }

  /** Drops back to the current level's own width (double-click, or a level change). */
  protected resetDockWidth(): void {
    this.dockWidthOverride.set(null);
    this.prefs.setDockWidth(null);
  }

  protected onDockResizeStart(event: PointerEvent): void {
    const handle = event.currentTarget as HTMLElement;
    const row = handle.parentElement;
    if (!row) return;
    event.preventDefault();
    handle.setPointerCapture(event.pointerId);
    this.dockResizing.set(true);

    const move = (e: PointerEvent): void => {
      const rect = row.getBoundingClientRect();
      if (rect.width <= 0) return;
      // The inspector is to the RIGHT of the handle, so its width is the distance from the
      // pointer to the row's right edge.
      this.setDockWidth(((rect.right - e.clientX) / rect.width) * 100);
    };
    const up = (): void => {
      handle.removeEventListener('pointermove', move);
      handle.removeEventListener('pointerup', up);
      handle.removeEventListener('pointercancel', up);
      this.dockResizing.set(false);
      this.prefs.setDockWidth(this.dockWidthOverride());
    };
    handle.addEventListener('pointermove', move);
    handle.addEventListener('pointerup', up);
    handle.addEventListener('pointercancel', up);
  }

  protected onDockResizeKey(event: KeyboardEvent): void {
    if (event.key === 'Home') {
      event.preventDefault();
      this.resetDockWidth();
      return;
    }
    const step = event.key === 'ArrowLeft' ? 2 : event.key === 'ArrowRight' ? -2 : 0;
    if (step === 0) return;
    event.preventDefault();
    this.setDockWidth(this.dockWidth() + step);
    this.prefs.setDockWidth(this.dockWidthOverride());
  }

  /** Clamped so a drag can never collapse the inspector to an unclickable sliver or squeeze the
   * deck+stage out — both ends of the range stay usable, which is why 0 and 100 are the LEVELS' job. */
  private setDockWidth(percent: number): void {
    this.dockWidthOverride.set(Math.round(Math.min(70, Math.max(20, percent))));
  }
}

function isStageAltitude(value: string | null): value is StageAltitude {
  return value !== null && (VALID_ALTITUDES as readonly string[]).includes(value);
}

const VALID_LENSES: readonly LensId[] = ['service', 'layer', 'feature', 'flow'];

function isLensId(value: string | null): value is LensId {
  return value !== null && (VALID_LENSES as readonly string[]).includes(value as LensId);
}

function isTypingTarget(target: EventTarget | null): boolean {
  const tag = (target as HTMLElement | null)?.tagName;
  return tag === 'INPUT' || tag === 'TEXTAREA';
}
