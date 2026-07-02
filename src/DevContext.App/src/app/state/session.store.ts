import { computed, inject, Injectable, signal } from '@angular/core';

import type { AnalysisSummary, MapResponse, StatsResponse } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { ActivityService } from '../core/activity/activity.service';
import { DevContextApi, type AnalyzeSpec } from '../data-access/devcontext-api';
import { type AnalysisStatus, type EntryGroupVm, groupEntries } from '../models/view-models';
import { RecentStore } from './recent.store';

export interface ProgressVm {
  readonly stage: string;
  readonly percent: number;
  readonly message: string;
}

@Injectable({ providedIn: 'root' })
export class SessionStore {
  private readonly api = inject(DevContextApi);
  private readonly activity = inject(ActivityService);
  private readonly recentStore = inject(RecentStore);

  private readonly _status = signal<AnalysisStatus>('idle');
  private readonly _error = signal<string | null>(null);
  private readonly _handle = signal<string | null>(null);
  private readonly _summary = signal<AnalysisSummary | null>(null);
  private readonly _mapResponse = signal<MapResponse | null>(null);
  private readonly _mapMarkdown = signal('');
  private readonly _entryGroups = signal<readonly EntryGroupVm[]>([]);
  private readonly _stats = signal<StatsResponse | null>(null);
  private readonly _statsError = signal<string | null>(null);
  private readonly _statsLoading = signal(false);

  readonly status = this._status.asReadonly();
  readonly error = this._error.asReadonly();
  readonly handle = this._handle.asReadonly();
  readonly summary = this._summary.asReadonly();
  readonly mapResponse = this._mapResponse.asReadonly();
  readonly mapMarkdown = this._mapMarkdown.asReadonly();
  readonly entryGroups = this._entryGroups.asReadonly();

  readonly busy = computed(() => this._status() === 'analyzing' || this._status() === 'cloning');
  readonly ready = computed(() => this._status() === 'ready');
  readonly entryCount = computed(() => this._entryGroups().reduce((n, g) => n + g.entries.length, 0));
  readonly stats = this._stats.asReadonly();
  readonly statsError = this._statsError.asReadonly();
  readonly statsLoading = this._statsLoading.asReadonly();
  readonly insights = computed(() => this._stats()?.insights ?? []);
  readonly insightCount = computed(() => this.insights().length);
  lastStats = () => this._stats();

  async analyze(spec: AnalyzeSpec): Promise<void> {
    this.activity.start(isRepoUrl(spec.path) ? 'Cloning…' : 'Analyzing…');

    this._error.set(null);
    this._handle.set(null);
    this._summary.set(null);
    this._mapResponse.set(null);
    this._mapMarkdown.set('');
    this._entryGroups.set([]);
    this._stats.set(null);
    this._statsError.set(null);
    this._statsLoading.set(false);
    this._status.set(isRepoUrl(spec.path) ? 'cloning' : 'analyzing');

    try {
      const ctrl = this.activity.controller;
      const outcome = await ctrl!.run(async (signal) => {
        return await this.api.analyze(
          spec,
          (p) => {
            this.activity.setProgress(p.stage, Math.round(p.percent), p.message);
            this._status.set(p.stage.includes('Clon') ? 'cloning' : 'analyzing');
          },
          signal,
        );
      });

      if (!outcome) {
        this._status.set('idle');
        this.activity.clear();
        return;
      }

      if (!outcome.ok) {
        if (outcome.code === 'Cancelled') {
          this._status.set('idle');
          this.activity.clear();
          return;
        }
        this.fail(outcome.message);
        return;
      }

      this._handle.set(outcome.handle);
      this._summary.set(outcome.summary);

      const [map, entries] = await Promise.all([
        this.api.getMap(outcome.handle),
        this.api.listEntryPoints(outcome.handle),
      ]);
      this._mapResponse.set(map);
      this._mapMarkdown.set(map.markdown);
      this._entryGroups.set(groupEntries(entries.entryPoints));
      this._status.set('ready');
      this.activity.clear();

      this._statsLoading.set(true);
      this.api.getStats(outcome.handle)
        .then(s => { this._stats.set(s); this._statsLoading.set(false); })
        .catch(err => { this._statsError.set(describeError(err)); this._statsLoading.set(false); });

      this.recentStore.add(spec.path, outcome.summary.label);
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        this._status.set('idle');
        this.activity.clear();
        return;
      }
      this.fail(describeError(err));
    }
  }

  cancel(): void {
    this.activity.controller?.cancel();
  }

  refreshStats(): void {
    const h = this._handle();
    if (!h) return;
    this._statsError.set(null);
    this._statsLoading.set(true);
    this.api.getStats(h)
      .then(s => { this._stats.set(s); this._statsLoading.set(false); })
      .catch(err => { this._statsError.set(describeError(err)); this._statsLoading.set(false); });
  }

  private fail(message: string): void {
    this._error.set(message);
    this._status.set('error');
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
