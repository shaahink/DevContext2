import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { KIND_LABELS } from '../../models/view-models';
import { StartHero } from '../home/start-hero';
import { IdentityStrip } from '../home/identity-strip';
import { RunConsole } from '../home/run-console';
import { KindIcon } from '../../ui/kind-icon/kind-icon';

const MAX_TOP_FLOWS = 7;
const MAX_INSIGHTS = 5;

interface InsightRowVm {
  readonly id: string;
  readonly severity: string;
  readonly title: string;
  readonly action?: string;
  readonly actionTarget?: string;
}

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

          @if (needsAttention().length) {
            <div>
              <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-warn">What needs attention</h2>
              <div class="space-y-1">
                @for (i of needsAttention(); track i.id) {
                  <a
                    class="flex items-center gap-2 rounded px-2 py-1 text-xs hover:bg-surface-2 transition-colors"
                    [routerLink]="(i.action && i.action !== 'None') ? ['/explore'] : ['/insights']"
                    [queryParams]="homeActionParams(i.action ?? '', i.actionTarget)"
                  >
                    <span class="chip shrink-0"
                      [class.text-danger]="i.severity === 'warning'"
                      [class.text-warn]="i.severity === 'notable'"
                    >{{ i.severity }}</span>
                    <span class="min-w-0 flex-1 truncate text-ink">{{ i.title }}</span>
                    @if (i.action && i.action !== 'None') {
                      <span class="shrink-0 text-2xs text-accent">{{ actionLabel(i.action) }} &rarr;</span>
                    }
                  </a>
                }
              </div>
            </div>
          }

          @if (goodToKnow().length) {
            <div>
              <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Good to know</h2>
              <div class="space-y-1">
                @for (i of goodToKnow(); track i.id) {
                  <div class="flex items-center gap-2 rounded px-2 py-1 text-xs">
                    <span class="chip shrink-0 text-ink-subtle">{{ i.severity }}</span>
                    <span class="min-w-0 flex-1 truncate text-ink-muted">{{ i.title }}</span>
                  </div>
                }
              </div>
            </div>
          }

          @if (session.insightCount() > (needsAttention().length + goodToKnow().length)) {
            <a routerLink="/insights" class="block text-2xs text-accent hover:underline">
              See all {{ session.insightCount() }} insights &rarr;
            </a>
          }

          <details class="text-xs text-ink-muted">
            <summary class="cursor-pointer hover:text-ink">Engine details</summary>
            <app-run-console />
          </details>
        </div>
      }
    </div>
  `,
})
export class HomePage {
  protected readonly session = inject(SessionStore);
  protected readonly atlas = inject(AtlasStore);
  protected readonly KIND_LABELS = KIND_LABELS;

  protected readonly topFlows = computed(() => {
    const flatEntries = this.session.entryGroups().flatMap((g) => g.entries);
    const ranked = flatEntries
      .filter((e) => e.score !== undefined)
      .sort((a, b) => (b.score ?? 0) - (a.score ?? 0));
    if (ranked.length > 0) return ranked.slice(0, MAX_TOP_FLOWS);
    return flatEntries.slice(0, MAX_TOP_FLOWS);
  });

  private readonly allInsights = computed<readonly InsightRowVm[]>(() => {
    const real: InsightRowVm[] = this.session.insights().map((i) => ({
      id: i.id,
      severity: i.severity,
      title: i.title,
      action: i.action,
      actionTarget: i.actionTarget,
    }));
    const s = this.session.summary();
    if (s && s.entries > 0 && s.entriesWithTarget < s.entries) {
      const unwired = s.entries - s.entriesWithTarget;
      const severity = unwired / s.entries > 0.2 ? 'warning' : 'notable';
      real.unshift({ id: 'unwired-entries', severity, title: `${unwired} of ${s.entries} entries have no resolved target` });
    }
    return real;
  });

  protected readonly needsAttention = computed(() =>
    this.allInsights().filter((i) => i.severity === 'warning' || i.severity === 'notable').slice(0, MAX_INSIGHTS),
  );

  protected readonly goodToKnow = computed(() =>
    this.allInsights().filter((i) => i.severity === 'info').slice(0, MAX_INSIGHTS),
  );

  protected actionLabel(action: string): string {
    switch (action) {
      case 'Focus': return 'Trace';
      case 'Node': return 'Open';
      case 'Filter': return 'Filter';
      default: return action;
    }
  }

  protected homeActionParams(action: string, target?: string): Record<string, string> {
    if (!action || action === 'None' || !target) return {};
    switch (action) {
      case 'Focus': return { focus: target };
      case 'Node': return { focus: target, view: 'node' };
      case 'Filter': return { kind: target };
      default: return target ? { focus: target } : {};
    }
  }
}
