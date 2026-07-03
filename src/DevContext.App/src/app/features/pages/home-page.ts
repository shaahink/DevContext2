import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { KIND_LABELS, type EntryVm } from '../../models/view-models';
import { StartHero } from '../home/start-hero';
import { IdentityStrip } from '../home/identity-strip';
import { RunConsole } from '../home/run-console';
import { KindIcon } from '../../ui/kind-icon/kind-icon';

const SEVERITY_ORDER: Record<string, number> = { warning: 0, notable: 1, info: 2 };
const MAX_TOP_FLOWS = 7;
const MAX_INSIGHTS = 3;

/**
 * Home (proposal §2) — `/` when a session exists: console during analysis, then a
 * card-free digest (identity, Top Flows, insight headlines, run report). The
 * no-session state is `StartHero`. Replaces the old overview-page.ts, which wrapped
 * this content in the now-deleted SectionCard.
 *
 * Top Flows shows a plain entry list for now — `AtlasStore`'s importance ranking
 * (breadth × boundary crossings, proposal §3.2) lands in W5 once the background
 * indexer is triggered; until then this is literally `session.entryGroups()`
 * flattened, per the proposal's own W4 note.
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
  protected readonly KIND_LABELS = KIND_LABELS;

  protected readonly topFlows = computed<readonly EntryVm[]>(() =>
    this.session.entryGroups().flatMap((g) => g.entries).slice(0, MAX_TOP_FLOWS),
  );

  protected readonly topInsights = computed(() =>
    [...this.session.insights()]
      .sort((a, b) => (SEVERITY_ORDER[a.severity] ?? 3) - (SEVERITY_ORDER[b.severity] ?? 3))
      .slice(0, MAX_INSIGHTS),
  );
}
