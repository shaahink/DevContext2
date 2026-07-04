import { Injectable, effect, inject, signal } from '@angular/core';
import { AtlasStore } from './atlas.store';
import { SessionStore } from './session.store';
import { WorkspaceStore } from './workspace.store';
import { TICKER_PRIORITY, TickerService } from '../core/ticker.service';
import type { EntryGroupVm } from '../models/view-models';

const DIFF_LABELS: Record<string, { readonly singular: string; readonly plural: string }> = {
  HttpEndpoint: { singular: 'endpoint', plural: 'endpoints' },
  MessageConsumer: { singular: 'consumer', plural: 'consumers' },
  HostedService: { singular: 'hosted service', plural: 'hosted services' },
  ScheduledJob: { singular: 'scheduled job', plural: 'scheduled jobs' },
  DomainEventHandler: { singular: 'domain event handler', plural: 'domain event handlers' },
  PublicApi: { singular: 'public API', plural: 'public APIs' },
};

interface Baseline {
  readonly path: string;
  readonly kindCounts: ReadonlyMap<string, number>;
  readonly verifiedPct: number | null;
}

function countByKind(groups: readonly EntryGroupVm[]): ReadonlyMap<string, number> {
  return new Map(groups.map((g) => [g.kind, g.entries.length]));
}

function formatDelta(kind: string, delta: number): string {
  const labels = DIFF_LABELS[kind] ?? { singular: kind, plural: `${kind}s` };
  const label = Math.abs(delta) === 1 ? labels.singular : labels.plural;
  return `${delta > 0 ? '+' : '−'}${Math.abs(delta)} ${label}`;
}

/** Builds the §3.9 stretch summary line, e.g. "+3 endpoints, −1 consumer, wired
 * 87→91%" — null if literally nothing changed (trust principle: no diff to show
 * beats a fabricated "no changes" line dressed up as data). */
function buildSummary(
  before: ReadonlyMap<string, number>,
  after: ReadonlyMap<string, number>,
  verifiedBefore: number | null,
  verifiedAfter: number | null,
): string | null {
  const kinds = new Set([...before.keys(), ...after.keys()]);
  const parts: string[] = [];
  for (const kind of kinds) {
    const delta = (after.get(kind) ?? 0) - (before.get(kind) ?? 0);
    if (delta !== 0) parts.push(formatDelta(kind, delta));
  }
  if (verifiedBefore !== null && verifiedAfter !== null && verifiedBefore !== verifiedAfter) {
    parts.push(`wired ${verifiedBefore}→${verifiedAfter}%`);
  }
  return parts.length > 0 ? parts.join(', ') : null;
}

/**
 * Snapshot diff (proposal §3.9, stretch, W7 checkpoint 7) — re-analyzing the SAME path
 * (Ctrl+R) captures a before/after comparison of entryGroups (by kind) and the Flow
 * Atlas's repo-wide confidence (§3.5), posted as one ticker item. Zero new engine calls:
 * purely a client-side diff of data this app already fetches on every analyze.
 *
 * The "after" confidence reading deliberately waits for `AtlasStore.status() === 'done'`
 * rather than reading it the instant `analyze()` resolves — atlas indexing is a
 * background process that starts fresh on every analyze, so an early read would compare
 * a fully-settled "before" percentage against a half-indexed "after" one (e.g. "wired
 * 87→12%" for a repo that's actually still catching up, not regressing).
 */
@Injectable({ providedIn: 'root' })
export class SnapshotDiffStore {
  private readonly session = inject(SessionStore);
  private readonly atlas = inject(AtlasStore);
  private readonly workspace = inject(WorkspaceStore);
  private readonly ticker = inject(TickerService);

  private baseline: Baseline | null = null;
  private readonly pendingPath = signal<string | null>(null);

  constructor() {
    effect(() => {
      const path = this.pendingPath();
      if (path && this.atlas.status() === 'done') {
        this.pendingPath.set(null);
        this.reportDiff(path);
      }
    });
  }

  /** Call right before re-triggering `analyze()` on an already-ready tab's own path. */
  captureBaseline(path: string): void {
    this.baseline = this.session.ready()
      ? { path, kindCounts: countByKind(this.session.entryGroups()), verifiedPct: this.atlas.overallVerifiedPct() }
      : null;
  }

  /** Call once the re-analyze's `analyze()` promise has resolved — arms the
   * atlas-done watcher above. No-ops if there's no matching baseline (a fresh analyze
   * of a different path, not a re-analyze, never had one captured). */
  armReport(path: string): void {
    this.pendingPath.set(this.baseline?.path === path ? path : null);
  }

  private reportDiff(path: string): void {
    const base = this.baseline;
    this.baseline = null;
    if (!base || base.path !== path) return;
    // The active tab may have changed while atlas indexing was still catching up —
    // don't attribute another tab's data to this diff.
    if (this.workspace.activeTab()?.path !== path) return;

    const summary = buildSummary(base.kindCounts, countByKind(this.session.entryGroups()), base.verifiedPct, this.atlas.overallVerifiedPct());
    if (!summary) return;
    this.ticker.post({ id: `active:diff:${Date.now()}`, text: summary, icon: 'refresh', priority: TICKER_PRIORITY.analysis });
  }
}
