import { computed, effect, inject, Injectable, signal } from '@angular/core';

import type { AnalysisSummary, GraphFacetsResponse, MapResponse, StatsResponse } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { OperationController } from '../core/activity/operation-controller';
import { DevContextApi, type NeighborDirection } from '../data-access/devcontext-api';
import { type AnalysisStatus, type EdgeVm, type EntryGroupVm, type NodeDetailVm, type TraceNodeVm } from '../models/view-models';

export type TraceDetail = 'signature' | 'salient' | 'full';

export interface ProgressVm {
  readonly stage: string;
  readonly percent: number;
  readonly message: string;
}

export interface LogLine {
  readonly stage: string;
  readonly message: string;
  readonly percent: number;
  readonly timestamp: number;
}

/** Everything SessionStore used to hold as its own private signals — now one tab's slice. */
export interface TabSessionSlice {
  readonly status: AnalysisStatus;
  readonly error: string | null;
  readonly handle: string | null;
  readonly summary: AnalysisSummary | null;
  readonly mapResponse: MapResponse | null;
  readonly mapMarkdown: string;
  readonly graphFacets: GraphFacetsResponse | null;
  readonly entryGroups: readonly EntryGroupVm[];
  readonly stats: StatsResponse | null;
  readonly statsError: string | null;
  readonly statsLoading: boolean;
  readonly progress: ProgressVm;
  readonly consoleLog: readonly LogLine[];
}

/** Everything TraceStore used to hold as its own private signals — now one tab's slice. */
export interface TabTraceSlice {
  readonly focus: string | null;
  readonly depth: number;
  readonly detail: TraceDetail;
  readonly error: string | null;
  readonly loading: boolean;
  readonly found: boolean;
  readonly tree: TraceNodeVm | null;
  readonly markdown: string;
  readonly touched: readonly string[];
  readonly emitted: readonly string[];
  readonly selectedNodeId: string | null;
  readonly nodeDetail: NodeDetailVm | null;
  readonly neighbors: readonly EdgeVm[];
  /** Direction of `neighbors` (Stage's Node altitude toggle, proposal §2). */
  readonly neighborDirection: NeighborDirection;
}

export interface TabState {
  readonly id: string;
  readonly path: string;
  readonly label: string;
  readonly session: TabSessionSlice;
  readonly trace: TabTraceSlice;
  /** Last view route while this tab was active — restored when switching back to it. */
  readonly route: string;
  /** Owns the in-flight analyze() for this tab. Cancelling one tab's controller must never
   * touch another tab's — that's the whole point of concurrent per-tab analysis. */
  readonly controller: OperationController;
}

export const DEFAULT_SESSION_SLICE: TabSessionSlice = {
  status: 'idle',
  error: null,
  handle: null,
  summary: null,
  mapResponse: null,
  mapMarkdown: '',
  graphFacets: null,
  entryGroups: [],
  stats: null,
  statsError: null,
  statsLoading: false,
  progress: { stage: '', percent: 0, message: '' },
  consoleLog: [],
};

export const DEFAULT_TRACE_SLICE: TabTraceSlice = {
  focus: null,
  depth: 6,
  detail: 'salient',
  error: null,
  loading: false,
  found: true,
  tree: null,
  markdown: '',
  touched: [],
  emitted: [],
  selectedNodeId: null,
  nodeDetail: null,
  neighbors: [],
  neighborDirection: 'out',
};

/**
 * Holds up to MAX_TABS independent repo sessions (I10). SessionStore/TraceStore are facades over
 * `activeTab()` — components never touch this store directly except the tab strip and app-shell.
 *
 * Race rule: any async completion (analyze progress/result, trace response, stats fetch) must write
 * via `updateTab`/`updateSession`/`updateTrace` using a tabId CAPTURED when the operation started,
 * never `activeId()` re-read at completion time — otherwise a background tab's result can bleed into
 * whatever tab the user has since switched to.
 */
const STORAGE_KEY = 'devcontext-workspace';

interface PersistedTab {
  readonly path: string;
  readonly label: string;
  readonly route: string;
}

interface PersistedWorkspace {
  readonly tabs: readonly PersistedTab[];
  readonly activeIndex: number;
}

@Injectable({ providedIn: 'root' })
export class WorkspaceStore {
  static readonly MAX_TABS = 6;

  private readonly api = inject(DevContextApi);

  private readonly _tabs = signal<TabState[]>([]);
  private readonly _activeId = signal<string | null>(null);
  /** Most-recently-active tab ids, front = current. Drives Ctrl+Tab/Ctrl+Shift+Tab (GAP-T5). */
  private readonly _mru = signal<readonly string[]>([]);

  readonly tabs = this._tabs.asReadonly();
  readonly activeId = this._activeId.asReadonly();
  readonly mru = this._mru.asReadonly();
  readonly activeTab = computed(() => this._tabs().find((t) => t.id === this._activeId()) ?? null);
  readonly atCap = computed(() => this._tabs().length >= WorkspaceStore.MAX_TABS);

  constructor() {
    this.restore();
    // Persist path/label/route (never session/trace data or the handle) after every change, so a
    // restart reopens the same tabs as idle placeholders — never auto-re-analyzing all of them.
    effect(() => this.persist(this._tabs(), this._activeId()));
  }

  /** Creates a new idle tab, activates it, and returns its id. No-op (returns the active id
   * unchanged) if already at the tab cap. */
  createTab(path = '', label = 'New tab'): string {
    const existing = this._tabs();
    if (existing.length >= WorkspaceStore.MAX_TABS) return this._activeId() ?? existing[0]?.id ?? '';

    const id = crypto.randomUUID();
    const tab: TabState = {
      id,
      path,
      label,
      session: DEFAULT_SESSION_SLICE,
      trace: DEFAULT_TRACE_SLICE,
      route: '/',
      controller: new OperationController(),
    };
    this._tabs.set([...existing, tab]);
    this._activeId.set(id);
    this.pushMru(id);
    return id;
  }

  /** Closes a tab, cancelling its in-flight operation and freeing its server-side snapshot (if any).
   * Activates the neighbor that slid into its place (or the previous one) if the closed tab was active. */
  closeTab(id: string): void {
    const list = this._tabs();
    const idx = list.findIndex((t) => t.id === id);
    if (idx === -1) return;

    const closing = list[idx];
    closing.controller.cancel();
    if (closing.session.handle) void this.api.closeSession(closing.session.handle).catch(() => undefined);

    const next = list.filter((t) => t.id !== id);
    this._tabs.set(next);
    this._mru.update((mru) => mru.filter((mid) => mid !== id));

    if (this._activeId() === id) {
      const neighbor = next[idx] ?? next[idx - 1] ?? null;
      this._activeId.set(neighbor?.id ?? null);
      if (neighbor) this.pushMru(neighbor.id);
    }
  }

  setActive(id: string): void {
    if (!this._tabs().some((t) => t.id === id)) return;
    this._activeId.set(id);
    this.pushMru(id);
  }

  /** Unshifts `id` to the front of the MRU list, deduping. */
  private pushMru(id: string): void {
    this._mru.update((mru) => [id, ...mru.filter((mid) => mid !== id)]);
  }

  tabById(id: string): TabState | null {
    return this._tabs().find((t) => t.id === id) ?? null;
  }

  updateTab(id: string, updater: (tab: TabState) => TabState): void {
    this._tabs.update((list) => list.map((t) => (t.id === id ? updater(t) : t)));
  }

  updateSession(id: string, updater: (s: TabSessionSlice) => TabSessionSlice): void {
    this.updateTab(id, (t) => ({ ...t, session: updater(t.session) }));
  }

  updateTrace(id: string, updater: (s: TabTraceSlice) => TabTraceSlice): void {
    this.updateTab(id, (t) => ({ ...t, trace: updater(t.trace) }));
  }

  setRoute(id: string, route: string): void {
    this.updateTab(id, (t) => (t.route === route ? t : { ...t, route }));
  }

  setPathLabel(id: string, path: string, label: string): void {
    this.updateTab(id, (t) => ({ ...t, path, label }));
  }

  private restore(): void {
    const parsed = this.readPersisted();
    if (!parsed?.tabs?.length) return;

    const restored: TabState[] = parsed.tabs.slice(0, WorkspaceStore.MAX_TABS).map((t) => ({
      id: crypto.randomUUID(),
      path: t.path,
      label: t.label,
      session: DEFAULT_SESSION_SLICE,
      trace: DEFAULT_TRACE_SLICE,
      route: t.route || '/',
      controller: new OperationController(),
    }));
    this._tabs.set(restored);

    const activeIdx = Math.min(Math.max(parsed.activeIndex, 0), restored.length - 1);
    this._activeId.set(restored[activeIdx]?.id ?? restored[0]?.id ?? null);
  }

  private persist(tabs: readonly TabState[], activeId: string | null): void {
    const activeIndex = Math.max(0, tabs.findIndex((t) => t.id === activeId));
    const payload: PersistedWorkspace = {
      tabs: tabs.map((t) => ({ path: t.path, label: t.label, route: t.route })),
      activeIndex,
    };
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
    } catch {
      /* quota exceeded – drop */
    }
  }

  private readPersisted(): PersistedWorkspace | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as PersistedWorkspace) : null;
    } catch {
      return null;
    }
  }
}
