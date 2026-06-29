import { computed, inject, Injectable, signal } from '@angular/core';

import type { AnalysisSummary, MapResponse } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
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
  private readonly recentStore = inject(RecentStore);
  private abort: AbortController | null = null;

  private readonly _status = signal<AnalysisStatus>('idle');
  private readonly _progress = signal<ProgressVm>({ stage: '', percent: 0, message: '' });
  private readonly _error = signal<string | null>(null);
  private readonly _handle = signal<string | null>(null);
  private readonly _summary = signal<AnalysisSummary | null>(null);
  private readonly _mapResponse = signal<MapResponse | null>(null);
  private readonly _mapMarkdown = signal('');
  private readonly _entryGroups = signal<readonly EntryGroupVm[]>([]);

  readonly status = this._status.asReadonly();
  readonly progress = this._progress.asReadonly();
  readonly error = this._error.asReadonly();
  readonly handle = this._handle.asReadonly();
  readonly summary = this._summary.asReadonly();
  readonly mapResponse = this._mapResponse.asReadonly();
  readonly mapMarkdown = this._mapMarkdown.asReadonly();
  readonly entryGroups = this._entryGroups.asReadonly();

  readonly busy = computed(() => this._status() === 'analyzing' || this._status() === 'cloning');
  readonly ready = computed(() => this._status() === 'ready');
  readonly entryCount = computed(() => this._entryGroups().reduce((n, g) => n + g.entries.length, 0));

  async analyze(spec: AnalyzeSpec): Promise<void> {
    this.abort?.abort();
    const abort = new AbortController();
    this.abort = abort;

    this._error.set(null);
    this._handle.set(null);
    this._summary.set(null);
    this._mapResponse.set(null);
    this._mapMarkdown.set('');
    this._entryGroups.set([]);
    this._status.set(isRepoUrl(spec.path) ? 'cloning' : 'analyzing');
    this._progress.set({ stage: 'Starting', percent: 0, message: 'Starting…' });

    try {
      const outcome = await this.api.analyze(
        spec,
        (p) => this._progress.set({ stage: p.stage, percent: p.percent, message: p.message }),
        abort.signal,
      );

      if (!outcome.ok) {
        if (outcome.code === 'Cancelled') {
          this._status.set('idle');
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

      this.recentStore.add(spec.path, outcome.summary.label);
    } catch (err) {
      if (abort.signal.aborted) {
        this._status.set('idle');
        return;
      }
      this.fail(describeError(err));
    }
  }

  cancel(): void {
    this.abort?.abort();
  }

  private fail(message: string): void {
    this._error.set(message);
    this._status.set('error');
  }
}

function isRepoUrl(path: string): boolean {
  return /github\.com|^https?:\/\//i.test(path);
}

function describeError(err: unknown): string {
  if (err instanceof Error) return err.message;
  return 'Could not reach the DevContext server. Is it running?';
}
