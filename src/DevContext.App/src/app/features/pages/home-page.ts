import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { projectDisplayName } from '../../core/format';
import { KIND_LABELS } from '../../models/view-models';
import { StartHero } from '../home/start-hero';
import { IdentityStrip } from '../home/identity-strip';
import { RunConsole } from '../home/run-console';
import { KindIcon } from '../../ui/kind-icon/kind-icon';
import { ServiceMapHero } from '../shared/service-map-hero';
import { HomeTiles } from '../shared/home-tiles';
import { OnboardingRow } from '../shared/onboarding-row';

const MAX_TOP_FLOWS = 7;

interface InsightRowVm {
  readonly id: string;
  readonly severity: string;
  readonly title: string;
  readonly action?: string;
  readonly actionTarget?: string;
}

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, StartHero, IdentityStrip, RunConsole, KindIcon, ServiceMapHero, HomeTiles, OnboardingRow],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      @if (!session.busy() && !session.ready()) {
        <app-start-hero />
      } @else if (session.busy()) {
        <app-run-console />
      } @else {
        <div class="space-y-8">
          <app-identity-strip />

          <!-- M6.1: Service map hero — deterministic layout -->
          <div>
            <h2 class="section-h mb-3">{{ heroHeading() }}</h2>
            <app-service-map-hero
              [topology]="topology()"
              [serviceStyles]="serviceStyles()"
            />
          </div>

          <!-- M6.1: Three tiles -->
          <app-home-tiles [topology]="topology()" />

          <!-- Top Flows -->
          @if (topFlows().length) {
            <div>
              <h2 class="section-h mb-3">Top flows</h2>
              <div class="space-y-1">
                @for (e of topFlows(); track e.focus) {
                  <a
                    class="list-row flex items-center gap-2"
                    [routerLink]="['/explore']"
                    [queryParams]="{ focus: e.focus }"
                  >
                    <app-kind-icon [kind]="e.kind" [size]="14" class="text-ink-subtle" />
                    @if (e.httpMethod) {
                      <span class="chip shrink-0">{{ e.httpMethod }}</span>
                    }
                    <span class="min-w-0 flex-1 truncate font-mono text-xs text-ink">{{ e.route || e.title }}</span>
                    @if (e.project) {
                      <span class="chip shrink-0 flex items-center gap-1 text-2xs" [title]="e.project">
                        <span class="inline-block h-2 w-2 rounded-sm" [style.background]="svcColor(e.project)"></span>
                        {{ shortName(e.project) }}
                      </span>
                    }
                    <span class="shrink-0 text-2xs text-ink-subtle">{{ KIND_LABELS[e.kind] ?? e.kind }}</span>
                  </a>
                }
              </div>
            </div>
          }

          <!-- Insights (needs attention) -->
          @if (needsAttention().length) {
            <div>
              <h2 class="section-h mb-2 text-warn">Needs attention</h2>
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

          <!-- M6.1: Onboarding row -->
          <app-onboarding-row />

          @if (session.insightCount() > needsAttention().length) {
            <a routerLink="/insights" class="block text-2xs text-accent hover:underline">
              See all {{ session.insightCount() }} insights &rarr;
            </a>
          }

          <details class="text-xs text-ink-muted">
            <summary class="cursor-pointer hover:text-ink">Run report</summary>
            <app-run-console />
          </details>
        </div>
      }
    </div>
  `,
})
export class HomePage {
  protected readonly session = inject(SessionStore);
  protected readonly KIND_LABELS = KIND_LABELS;

  protected readonly topology = computed(() => this.session.mapResponse()?.topology ?? []);
  protected readonly serviceStyles = computed(() => this.session.mapResponse()?.serviceStyles ?? []);

  /** "How services connect" is microservice copy — on a monolith the hero shows the
   * runnable surfaces (Web + workers + CLI), so say that (T6.1). */
  protected readonly heroHeading = computed(() =>
    /microservice/i.test(this.session.mapResponse()?.archetype ?? '') ? 'How services connect' : 'What runs');

  private readonly svcPalette = ['#8b93ff', '#6cb2eb', '#98c379', '#e5c07b', '#d19a66', '#c678dd', '#56b6c2', '#5ac8fa', '#d16d9e', '#99a0ac'];
  protected svcColor(name: string): string {
    const idx = this.topology().findIndex((p) => p.name === name);
    return this.svcPalette[idx % this.svcPalette.length] ?? this.svcPalette[0];
  }
  /** Common-prefix strip only — never the last dot segment (T6.8, audit A8). The colored
   * square replaced the old tinted chip background, whose ink was unreadable on light
   * service colors (T6.0 S1.7). */
  protected shortName(name: string): string {
    return projectDisplayName(name, this.topology().map((p) => p.name));
  }

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
    this.allInsights().filter((i) => i.severity === 'warning' || i.severity === 'notable').slice(0, 5),
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
