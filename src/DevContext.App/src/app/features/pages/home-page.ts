import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { KIND_LABELS, type EntryVm } from '../../models/view-models';
import { StartHero } from '../home/start-hero';
import { IdentityStrip } from '../home/identity-strip';
import { RunConsole } from '../home/run-console';
import { KindIcon } from '../../ui/kind-icon/kind-icon';

const SEVERITY_ORDER: Record<string, number> = { warning: 0, notable: 1, info: 2 };
const MAX_TOP_FLOWS = 7;
const MAX_INSIGHTS = 3;

/** Lightweight display shape shared by real (server) insights and the client-synthesized
 * unwired-entries card below — only the fields the template actually reads. */
interface InsightRowVm {
  readonly id: string;
  readonly severity: string;
  readonly title: string;
}

/**
 * Home (proposal §2) — `/` when a session exists: console during analysis, then a
 * card-free digest (identity, Top Flows, insight headlines, run report). The
 * no-session state is `StartHero`. Replaces the old overview-page.ts, which wrapped
 * this content in the now-deleted SectionCard.
 *
 * Top Flows prefers `AtlasStore`'s importance ranking (breadth × boundary crossings,
 * proposal §3.2, `atlas.topFlows()`) once the background indexer (triggered from
 * `SessionStore.analyze()`, W5 checkpoint 1) has produced results, mapped back to the
 * full `EntryVm` by focus for its httpMethod/route display fields — `FlowStat` itself
 * doesn't carry those. Falls back to the flat `session.entryGroups()` list while
 * indexing hasn't found anything yet (empty/loading state, not a bug).
 */
@Component({
  selector: 'app-home-page',
  imports: [RouterLink, StartHero, IdentityStrip, RunConsole, KindIcon],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      @if (!session.busy() && !session.ready()) {
        <app-start-hero />
      } @else if (session.busy()) {
        <app-run-console />
      } @else {
        <div class="space-y-8">
          <app-identity-strip />

          @if (topFlows().length) {
            <div>
              <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Top Flows</h2>
              <div class="space-y-1">
                @for (e of topFlows(); track e.focus) {
                  <a
                    class="list-row flex items-center gap-2"
                    [routerLink]="['/explore']"
                    [queryParams]="{ focus: e.focus }"
                  >
                    <app-kind-icon [kind]="e.kind" [size]="12" class="text-ink-subtle" />
                    @if (e.httpMethod) {
                      <span class="chip shrink-0">{{ e.httpMethod }}</span>
                    }
                    <span class="min-w-0 flex-1 truncate font-mono text-xs text-ink">{{ e.route || e.title }}</span>
                    <span class="shrink-0 text-2xs text-ink-subtle">{{ KIND_LABELS[e.kind] ?? e.kind }}</span>
                  </a>
                }
              </div>
            </div>
          }

          @if (topInsights().length) {
            <div>
              <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Insights</h2>
              <div class="space-y-1">
                @for (i of topInsights(); track i.id) {
                  <div class="flex items-center gap-2 px-2 py-1 text-xs">
                    <span
                      class="chip shrink-0"
                      [class.text-danger]="i.severity === 'warning'"
                      [class.text-warn]="i.severity === 'notable'"
                    >{{ i.severity }}</span>
                    <span class="min-w-0 flex-1 truncate text-ink">{{ i.title }}</span>
                  </div>
                }
                <a routerLink="/insights" class="block px-2 py-1 text-2xs text-accent hover:underline">
                  See all {{ session.insightCount() }} insights &rarr;
                </a>
              </div>
            </div>
          }

          <div>
            <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Run report</h2>
            <app-run-console />
          </div>
        </div>
      }
    </div>
  `,
})
export class HomePage {
  protected readonly session = inject(SessionStore);
  protected readonly atlas = inject(AtlasStore);
  protected readonly KIND_LABELS = KIND_LABELS;

  protected readonly topFlows = computed<readonly EntryVm[]>(() => {
    const flatEntries = this.session.entryGroups().flatMap((g) => g.entries);
    const ranked = this.atlas.topFlows();
    if (ranked.length > 0) {
      const byFocus = new Map(flatEntries.map((e) => [e.focus, e] as const));
      const mapped = ranked.map((f) => byFocus.get(f.focus)).filter((e): e is EntryVm => !!e);
      if (mapped.length > 0) return mapped.slice(0, MAX_TOP_FLOWS);
    }
    return flatEntries.slice(0, MAX_TOP_FLOWS);
  });

  /** §3.6 — a client-synthesized "unwired entries" card, competing for a slot by
   * severity alongside the server's real insights (not a separate list — same
   * "Insights" heading, same "See all" link semantics apply to the real ones). */
  private readonly unwiredInsight = computed<InsightRowVm | null>(() => {
    const s = this.session.summary();
    if (!s || s.entries === 0) return null;
    const unwired = s.entries - s.entriesWithTarget;
    if (unwired <= 0) return null;
    const severity = unwired / s.entries > 0.2 ? 'warning' : 'notable';
    return { id: 'unwired-entries', severity, title: `${unwired} of ${s.entries} entries have no resolved target` };
  });

  protected readonly topInsights = computed<readonly InsightRowVm[]>(() => {
    const real: InsightRowVm[] = this.session.insights().map((i) => ({ id: i.id, severity: i.severity, title: i.title }));
    const synthetic = this.unwiredInsight();
    const all = synthetic ? [synthetic, ...real] : real;
    return all
      .sort((a, b) => (SEVERITY_ORDER[a.severity] ?? 3) - (SEVERITY_ORDER[b.severity] ?? 3))
      .slice(0, MAX_INSIGHTS);
  });
}
