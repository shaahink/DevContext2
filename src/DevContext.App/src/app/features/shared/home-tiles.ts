import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { type EntryGroupVm } from '../../models/view-models';
import type { ProjectNode } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import {
  entryKindCounts,
  namespaceCount,
  namespacesBySize,
  publicTypeCount,
} from '../library/library-surface.vm';

@Component({
  selector: 'app-home-tiles',
  imports: [RouterLink],
  template: `
    <div class="grid grid-cols-3 gap-3">
      @if (isLibrary()) {
        <!-- D4.4 (F1): library home cards carry surface metrics, not entry metrics
             (was "0 entries / No entry data available" on every library repo). -->
        <div class="tile">
          <h3 class="tile-heading">Surface by namespace</h3>
          @if (topNamespaces().length) {
            <div class="space-y-1.5">
              @for (ns of topNamespaces(); track ns.namespace) {
                <div class="flex items-center gap-2 text-xs">
                  <span class="min-w-0 flex-1 truncate text-ink-muted" [title]="ns.namespace">{{ ns.namespace }}</span>
                  <div class="w-20 shrink-0 bg-surface h-2 rounded-full overflow-hidden">
                    <div class="h-full bg-accent" [style.width.%]="ns.pct"></div>
                  </div>
                  <span class="tabular-nums text-2xs text-ink-subtle w-6 text-right">{{ ns.count }}</span>
                </div>
              }
            </div>
            <p class="mt-1.5 text-2xs text-ink-subtle">
              {{ surfaceTypes() }} public types across {{ surfaceNamespaces() }} namespaces
            </p>
          } @else {
            <p class="text-xs text-ink-muted">No public surface detected.</p>
          }
        </div>

        <div class="tile">
          <h3 class="tile-heading">Consumer front doors</h3>
          @if (frontDoors().length) {
            <div class="space-y-1.5">
              @for (fd of frontDoors(); track fd.kind) {
                <div class="flex items-center justify-between text-xs">
                  <span class="chip text-2xs">{{ fd.kind }}</span>
                  <span class="tabular-nums text-2xs text-ink-subtle">{{ fd.count }}</span>
                </div>
              }
            </div>
            <p class="mt-1.5 text-2xs text-ink-subtle">
              <a routerLink="/explore" class="text-accent hover:underline">Browse the library surface →</a>
            </p>
          } @else {
            <p class="text-xs text-ink-muted">No ranked entry API detected.</p>
          }
        </div>
      } @else {
        <!-- Tile 1: Entries by kind per service (stacked bar) -->
        <div class="tile">
          <h3 class="tile-heading">Entries by kind</h3>
          <div class="space-y-1.5">
            @for (g of entryGroups(); track g.kind) {
              <div class="flex items-center gap-2 text-xs">
                <span class="w-16 shrink-0 text-ink-muted">{{ g.label }}</span>
                <div class="flex-1 bg-surface h-2 rounded-full overflow-hidden">
                  @for (seg of g.perService; track seg.service) {
                    <div
                      class="h-full inline-block"
                      [style.width.%]="seg.pct"
                      [style.background]="seg.color"
                      [title]="seg.service + ': ' + seg.count"
                    ></div>
                  }
                </div>
                <span class="tabular-nums text-2xs text-ink-subtle w-5 text-right">{{ g.entries.length }}</span>
              </div>
            }
          </div>
          <p class="mt-1.5 text-2xs text-ink-subtle">
            {{ totalEntries() }} entries across {{ serviceCount() }} {{ projectNoun() }}
          </p>
        </div>

        <!-- Tile 2: Wiring health (% entries with complete flows) -->
        <div class="tile">
          <h3 class="tile-heading">Wiring health</h3>
          @if (wired() !== null) {
            <div class="flex items-baseline gap-1">
              <span class="text-xl font-bold tabular-nums"
                [class.text-success]="wired()! >= 80"
                [class.text-warn]="wired()! >= 50 && wired()! < 80"
                [class.text-danger]="wired()! < 50"
              >{{ wired() }}%</span>
              <span class="text-xs text-ink-muted">entries targeted</span>
            </div>
            <p class="mt-1 text-2xs text-ink-subtle">
              {{ wiredCount() }}/{{ totalEntries() }} entries
              @if (unwiredCount() > 0) {
                have resolved targets —
                <a routerLink="/insights" class="text-accent hover:underline">{{ unwiredCount() }} unwired</a>
              }
            </p>
            <!-- mini bar -->
            <div class="mt-2 h-1.5 rounded-full bg-surface overflow-hidden">
              <div class="h-full rounded-full transition-all"
                [style.width.%]="wired()!"
                [class.bg-success]="wired()! >= 80"
                [class.bg-warn]="wired()! >= 50 && wired()! < 80"
                [class.bg-danger]="wired()! < 50"
              ></div>
            </div>
          } @else {
            <p class="text-xs text-ink-muted">No entry data available.</p>
          }
        </div>
      }

      <!-- Tile 3: Freshness -->
      <div class="tile">
        <h3 class="tile-heading">Freshness</h3>
        @if (summary(); as s) {
          @if (s.elapsedMs > 0) {
            <p class="text-xs text-ink">
              Analyzed in <span class="font-mono tabular-nums">{{ formatElapsed() }}</span>
            </p>
          }
          <p class="text-2xs text-ink-muted">
            {{ s.nodes }} types · {{ s.edges }} edges · {{ s.projects }} projects
          </p>
          @if (s.stale) {
            <span class="chip mt-1.5 text-warn text-2xs">HEAD moved — Re-analyze</span>
          } @else {
            <span class="chip mt-1.5 text-success text-2xs">Current</span>
          }
        } @else {
          <p class="text-xs text-ink-muted">No analysis data.</p>
        }
      </div>
    </div>
  `,
  styles: `
    .tile {
      padding: 12px;
      border-radius: 8px;
      border: 1px solid var(--vibe-line);
      background: var(--vibe-surface);
    }
    .tile-heading {
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--vibe-ink-subtle);
      margin-bottom: 8px;
    }
    .bg-success { background: var(--vibe-success); }
    .bg-warn { background: var(--vibe-warn); }
    .bg-danger { background: var(--vibe-danger); }
    .text-success { color: var(--vibe-success); }
    .text-warn { color: var(--vibe-warn); }
    .text-danger { color: var(--vibe-danger); }
  `,
})
export class HomeTiles {
  protected readonly session = inject(SessionStore);
  protected readonly summary = this.session.summary;

  readonly topology = input.required<readonly ProjectNode[]>();

  protected readonly entryGroups = computed<readonly (EntryGroupVm & { perService: readonly { service: string; count: number; pct: number; color: string }[] })[]>(() => {
    const groups = this.session.entryGroups();
    const svcNames = this.topology().map((p) => p.name);
    const colors = ['#8b93ff', '#6cb2eb', '#c678dd', '#98c379', '#e5c07b', '#d19a66', '#56b6c2', '#5ac8fa', '#d16d9e', '#99a0ac'];

    return groups.map((g) => {
      const perSvc = new Map<string, number>();
      for (const e of g.entries) {
        const svc = e.project ?? 'unknown';
        perSvc.set(svc, (perSvc.get(svc) ?? 0) + 1);
      }
      const total = g.entries.length || 1;
      const perService = [...perSvc.entries()]
        .sort((a, b) => b[1] - a[1])
        .map(([service, count], i) => ({ service: svcNames.find((s) => s === service) ?? service, count, pct: Math.round((count / total) * 100), color: colors[i % colors.length] }));
      return { ...g, perService };
    });
  });

  /** D4.4 (F1) — library repos swap entry-metrics for surface-metrics. */
  protected readonly isLibrary = computed(() => this.session.mapResponse()?.isLibrary ?? false);
  protected readonly surfaceTypes = computed(() => publicTypeCount(this.session.mapResponse()?.surface));
  protected readonly surfaceNamespaces = computed(() => namespaceCount(this.session.mapResponse()?.surface));
  protected readonly topNamespaces = computed(() => {
    const rows = namespacesBySize(this.session.mapResponse()?.surface, 6);
    const max = rows[0]?.count || 1;
    return rows.map((r) => ({ ...r, pct: Math.max(4, Math.round((r.count / max) * 100)) }));
  });
  protected readonly frontDoors = computed(() => entryKindCounts(this.session.mapResponse()?.surface));

  protected readonly totalEntries = computed(() => this.session.entryGroups().reduce((n, g) => n + g.entries.length, 0));
  protected readonly serviceCount = computed(() => this.topology().length);
  /** Same archetype-aware noun as the identity strip (T6.1) — a monolith has projects. */
  protected readonly projectNoun = computed(() =>
    /microservice/i.test(this.session.mapResponse()?.archetype ?? '') ? 'services' : 'projects');
  protected readonly wiredCount = computed(() => this.summary()?.entriesWithTarget ?? 0);
  protected readonly unwiredCount = computed(() => (this.summary()?.entries ?? 0) - (this.summary()?.entriesWithTarget ?? 0));
  protected readonly wired = computed(() => {
    const s = this.summary();
    if (!s || s.entries === 0) return null;
    return Math.round(((s.entriesWithTarget ?? 0) / s.entries) * 100);
  });

  protected readonly formatElapsed = computed(() => {
    const s = this.summary();
    if (!s) return '';
    return this.formatTime(Number(s.elapsedMs));
  });

  protected formatTime(ms: number): string {
    const s = ms / 1000;
    return s < 10 ? s.toFixed(1) + 's' : s < 60 ? s.toFixed(0) + 's' : (s / 60).toFixed(1) + 'm';
  }
}
