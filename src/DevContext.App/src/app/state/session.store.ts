import { computed, inject, Injectable } from '@angular/core';
import { create } from '@bufbuild/protobuf';

import { ActivityService } from '../core/activity/activity.service';
import { AnalysisSummarySchema } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi, type AnalyzeSpec } from '../data-access/devcontext-api';
import { type AnalysisStatus, groupEntries } from '../models/view-models';
import { AtlasStore } from './atlas.store';
import { RecentStore } from './recent.store';
import { DEFAULT_SESSION_SLICE, type LogLine, WorkspaceStore } from './workspace.store';

export type { ProgressVm } from './workspace.store';

/**
 * Facade over the ACTIVE tab's session slice in WorkspaceStore (I10). Public signal API is
 * unchanged from the pre-tabs version on purpose — every component that injects SessionStore
 * keeps working without modification; only the storage moved.
 *
 * analyze() captures its owning tabId once at call time and every progress/completion callback
 * writes into that specific tab — never into whatever tab happens to be active when the callback
 * fires. That's what makes "start eShop analyze, browse TodoApi while it runs" safe.
 */
@Injectable({ providedIn: 'root' })
export class SessionStore {
  private readonly api = inject(DevContextApi);
  private readonly activity = inject(ActivityService);
  private readonly recentStore = inject(RecentStore);
  private readonly workspace = inject(WorkspaceStore);
  private readonly atlas = inject(AtlasStore);

  private readonly activeSession = computed(() => this.workspace.activeTab()?.session ?? DEFAULT_SESSION_SLICE);

  readonly status = computed(() => this.activeSession().status);
  readonly error = computed(() => this.activeSession().error);
  readonly handle = computed(() => this.activeSession().handle);
  readonly summary = computed(() => this.activeSession().summary);
  readonly mapResponse = computed(() => this.activeSession().mapResponse);
  readonly mapMarkdown = computed(() => this.activeSession().mapMarkdown);
  readonly graphFacets = computed(() => this.activeSession().graphFacets);
  readonly entryGroups = computed(() => this.activeSession().entryGroups);
  readonly stats = computed(() => this.activeSession().stats);
  readonly statsError = computed(() => this.activeSession().statsError);
  readonly statsLoading = computed(() => this.activeSession().statsLoading);
  readonly progress = computed(() => this.activeSession().progress);
  readonly consoleLog = computed(() => this.activeSession().consoleLog);

  readonly busy = computed(() => this.status() === 'analyzing' || this.status() === 'cloning');
  readonly ready = computed(() => this.status() === 'ready');
  readonly entryCount = computed(() => this.entryGroups().reduce((n, g) => n + g.entries.length, 0));
  readonly insights = computed(() => this.stats()?.insights ?? []);
  readonly insightCount = computed(() => this.insights().length);
  readonly confidenceLedger = computed(() => this.stats()?.confidenceLedger);
  lastStats = () => this.stats();

  async analyze(spec: AnalyzeSpec): Promise<void> {
    const openElsewhere = this.workspace
      .tabs()
      .find((t) => t.path === spec.path && t.id !== this.workspace.activeId() && (t.session.handle !== null || t.session.status === 'analyzing' || t.session.status === 'cloning'));
    if (openElsewhere) {
      this.workspace.setActive(openElsewhere.id);
      return;
    }

    const reusingTab = this.workspace.activeId() !== null;
    const tabId: string = this.workspace.activeId() ?? this.workspace.createTab(spec.path, spec.path);
    if (reusingTab) this.workspace.setPathLabel(tabId, spec.path, spec.path);

    const controller = this.workspace.tabById(tabId)?.controller;
    if (!controller) return;

    this.activity.start(isRepoUrl(spec.path) ? 'Cloning…' : 'Analyzing…');
    this.workspace.updateSession(tabId, () => ({
      ...DEFAULT_SESSION_SLICE,
      status: isRepoUrl(spec.path) ? 'cloning' : 'analyzing',
    }));

    try {
      const outcome = await controller.run(async (signal) => {
        return await this.api.analyze(
          spec,
          (p) => {
            const percent = Math.round(p.percent);
            this.activity.setProgress(p.stage, percent, p.message);
            const status: AnalysisStatus = p.stage.includes('Clon') ? 'cloning' : 'analyzing';
            const line: LogLine = { stage: p.stage, message: p.message, percent, timestamp: Date.now() };
            this.workspace.updateSession(tabId, (s) => ({
              ...s,
              status,
              progress: { stage: p.stage, percent, message: p.message },
              consoleLog: [...s.consoleLog, line],
            }));
          },
          signal,
        );
      });

      if (!outcome) {
        this.workspace.updateSession(tabId, (s) => ({ ...s, status: 'idle' }));
        this.activity.clear();
        return;
      }

      if (!outcome.ok) {
        if (outcome.code === 'Cancelled') {
          this.workspace.updateSession(tabId, (s) => ({ ...s, status: 'idle' }));
          this.activity.clear();
          return;
        }
        this.fail(tabId, outcome.message);
        return;
      }

      this.workspace.updateSession(tabId, (s) => ({ ...s, handle: outcome.handle, summary: outcome.summary }));
      this.workspace.setPathLabel(tabId, spec.path, outcome.summary.label);

      const [map, entries] = await Promise.all([
        this.api.getMap(outcome.handle),
        this.api.listEntryPoints(outcome.handle),
      ]);
      const entryGroups = groupEntries(entries.entryPoints, spec.path);
      this.workspace.updateSession(tabId, (s) => ({
        ...s,
        mapResponse: map,
        mapMarkdown: map.markdown,
        entryGroups,
        status: 'ready',
      }));
      this.activity.clear();

      // L4.3 — service map + flow list come from the graph projections (one truth), fetched
      // once here; Home hero and Atlas read graphFacets instead of re-deriving client-side.
      this.api
        .getGraphFacets(outcome.handle)
        .then((facets) => this.workspace.updateSession(tabId, (s) => ({ ...s, graphFacets: facets })))
        .catch(() => { /* facets are additive; hero degrades to topology if unavailable */ });

      // Kick off background flow indexing (§3.1) on analysis-ready, regardless of which
      // page the user is on — Home's Top Flows needs this without a detour through /explore.
      this.atlas.start(tabId, outcome.handle, entryGroups.flatMap((g) => g.entries));

      this.workspace.updateSession(tabId, (s) => ({ ...s, statsLoading: true }));
      this.api
        .getStats(outcome.handle)
        .then((stats) => this.workspace.updateSession(tabId, (s) => ({ ...s, stats, statsLoading: false })))
        .catch((err) =>
          this.workspace.updateSession(tabId, (s) => ({ ...s, statsError: describeError(err), statsLoading: false })),
        );

      this.recentStore.add(spec.path, outcome.summary.label);
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        this.workspace.updateSession(tabId, (s) => ({ ...s, status: 'idle' }));
        this.activity.clear();
        return;
      }
      this.fail(tabId, describeError(err));
    }
  }

  /** T6.9 (audit B4) — adopt the server's live session for a repo instead of re-analyzing.
   * The server keeps sessions across client restarts (the MCP page lists them); before this,
   * every new browser context re-ran the full analyze (+~100 GetTrace of flow re-indexing,
   * measured in the T6.0 drive). Returns false when no live session matches — caller falls
   * back to analyze(). */
  async tryAdopt(path: string): Promise<boolean> {
    const tabId = this.workspace.activeId();
    if (!tabId || !path) return false;
    try {
      const live = await this.api.listSessions();
      const norm = (p: string) => p.replace(/[\\/]+/g, '/').replace(/\/+$/, '').toLowerCase();
      const match = live.sessions.find((s) => norm(s.repo) === norm(path) && s.nodes > 0);
      if (!match) return false;

      const handle = match.handle;
      const [map, entries] = await Promise.all([
        this.api.getMap(handle),
        this.api.listEntryPoints(handle),
      ]);
      const entryGroups = groupEntries(entries.entryPoints, path);
      const flat = entryGroups.flatMap((g) => g.entries);
      const summary = create(AnalysisSummarySchema, {
        // F4 (D4.5) — adopt converges on the same identity as fresh analyze: the scored
        // solution name (MapResponse.solution_name), directory basename only as fallback.
        // Pre-D4.5 adopt used the basename while analyze used engine.Label — the audit's
        // "home says Refit.slnx, tab says refit" split.
        label: map.solutionName || (path.replace(/[\\/]+$/, '').split(/[\\/]/).pop() ?? path),
        projects: map.projectCount,
        nodes: match.nodes,
        edges: match.edges,
        entries: match.entries,
        entriesWithTarget: flat.filter((e) => !!e.target).length,
        elapsedMs: 0n,
        archetype: map.archetype,
        isLibrary: map.isLibrary,
      });

      this.workspace.updateSession(tabId, (s) => ({
        ...s,
        handle,
        summary,
        mapResponse: map,
        mapMarkdown: map.markdown,
        entryGroups,
        status: 'ready',
        error: null,
      }));
      this.workspace.setPathLabel(tabId, path, summary.label);

      this.api
        .getGraphFacets(handle)
        .then((facets) => this.workspace.updateSession(tabId, (s) => ({ ...s, graphFacets: facets })))
        .catch(() => { /* additive */ });
      this.atlas.start(tabId, handle, flat);
      this.workspace.updateSession(tabId, (s) => ({ ...s, statsLoading: true }));
      this.api
        .getStats(handle)
        .then((stats) => this.workspace.updateSession(tabId, (s) => ({ ...s, stats, statsLoading: false })))
        .catch((err) =>
          this.workspace.updateSession(tabId, (s) => ({ ...s, statsError: describeError(err), statsLoading: false })));
      return true;
    } catch {
      return false; // server unreachable or listing failed — analyze() is the fallback
    }
  }

  cancel(): void {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    this.workspace.tabById(tabId)?.controller.cancel();
  }

  /** Re-triggers analysis for the active tab using its current path — used by the
   * "Repo moved ahead — Re-analyze?" chip (L1.2 staleness probe). */
  reAnalyze(): void {
    const tab = this.workspace.activeTab();
    if (!tab || !tab.path) return;
    void this.analyze({ path: tab.path });
  }

  refreshStats(): void {
    const tabId = this.workspace.activeId();
    const h = this.handle();
    if (!tabId || !h) return;
    this.workspace.updateSession(tabId, (s) => ({ ...s, statsError: null, statsLoading: true }));
    this.api
      .getStats(h)
      .then((stats) => this.workspace.updateSession(tabId, (s) => ({ ...s, stats, statsLoading: false })))
      .catch((err) =>
        this.workspace.updateSession(tabId, (s) => ({ ...s, statsError: describeError(err), statsLoading: false })),
      );
  }

  private fail(tabId: string, message: string): void {
    this.workspace.updateSession(tabId, (s) => ({ ...s, error: message, status: 'error' }));
    this.activity.setError(message);
  }
}

function isRepoUrl(path: string): boolean {
  return /github\.com|^https?:\/\//i.test(path);
}

function describeError(err: unknown): string {
  if (err instanceof Error) return err.message;
  return 'Could not reach the DevContext server. Is it running?';
}
