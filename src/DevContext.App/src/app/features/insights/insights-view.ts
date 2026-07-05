import { Component, computed, inject } from '@angular/core';
import { SessionStore } from '../../state/session.store';
import { RouterLink } from '@angular/router';

const SEVERITY_CLASS: Record<string, string> = {
  warning: 'border-danger',
  notable: 'border-warn',
  info: 'border-accent',
};

const SEVERITY_LABEL_CLASS: Record<string, string> = {
  warning: 'bg-danger/10 text-danger',
  notable: 'bg-warn/10 text-warn',
  info: 'bg-accent/10 text-accent',
};

const IMPACT_GROUPS: Record<string, string> = {
  warning: 'Act on this',
  notable: 'Act on this',
  info: 'Know this',
};

interface InsightGroup {
  impact: string;
  insights: {
    id: string; title: string; severity: string; severityClass: string;
    detail: string; evidence: string[];
    confidence: number; confidenceBasis?: string;
    whyItMatters?: string; action: string; actionTarget?: string;
  }[];
}

@Component({
  selector: 'app-insights-view',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="flex flex-col h-full p-4 space-y-4 overflow-y-auto">
      <h2 class="text-lg font-semibold text-ink">Insights</h2>
      <p class="text-sm text-ink-muted">Repo-specific findings — what's notable, risky, or wired in interesting ways.</p>

      <!-- Loading -->
      @if (store.busy() || store.statsLoading()) {
        <div class="space-y-3">
          <div class="h-16 rounded bg-surface-2 animate-pulse"></div>
          <div class="h-16 rounded bg-surface-2 animate-pulse"></div>
          <div class="h-10 rounded bg-surface-2 animate-pulse"></div>
        </div>
      }
      <!-- Error -->
      @else if (store.statsError(); as err) {
        <div class="flex items-center gap-3 rounded border border-danger/30 bg-danger/10 px-3 py-2.5 text-sm">
          <span class="text-danger">Failed to load stats.</span>
          <span class="text-ink-muted text-xs">{{ err }}</span>
          <button class="ml-auto text-xs text-accent hover:underline" (click)="retryStats()">Retry</button>
        </div>
      }
      <!-- Empty — not analyzed -->
      @else if (!store.ready()) {
        <div class="text-sm text-ink-muted">
          <p>Analyze a repo to see insights.</p>
          <a routerLink="/" class="text-accent text-xs hover:underline">Go to overview</a>
        </div>
      }
      <!-- Empty — analyzed, zero insights -->
      @else if (groups().length === 0) {
        <div class="text-sm text-ink-muted">No notable findings for this repo.</div>
      }
      <!-- Loaded -->
      @else {
        <div class="space-y-3">
          @for (group of groups(); track group.impact) {
            <div>
              <span class="text-2xs text-ink-muted uppercase tracking-wider">{{ group.impact }}</span>
              <div class="mt-1.5 space-y-2">
                @for (insight of group.insights; track insight.id) {
                  <div class="rounded border border-line bg-surface-1 p-3">
                    <div class="border-l-2 pl-3" [class]="insight.severityClass">
                      <div class="flex items-center gap-2">
                        <span class="text-xs font-semibold text-ink">{{ insight.title }}</span>
                        <span class="rounded px-1.5 py-px text-2xs" [class]="severityLabelClass(insight.severity)">{{ insight.severity }}</span>
                        @if (insight.confidence > 0) {
                          <span class="text-2xs tabular-nums text-ink-subtle" [title]="insight.confidenceBasis ?? ''">{{ (insight.confidence * 100).toFixed(0) }}% conf</span>
                        }
                      </div>
                      @if (insight.whyItMatters) {
                        <p class="mt-1 text-2xs text-ink-muted italic">{{ insight.whyItMatters }}</p>
                      }
                      @if (insight.detail) {
                        <p class="mt-1 text-2xs text-ink-muted">{{ insight.detail }}</p>
                      }
                      @if (insight.evidence.length) {
                        <div class="mt-1.5 flex flex-wrap gap-1">
                          @for (ev of insight.evidence; track ev) {
                            <a
                              class="rounded bg-surface-2 px-1.5 py-0.5 text-2xs text-ink-muted hover:bg-surface-3 hover:text-accent transition-colors"
                              [routerLink]="['/explore']"
                              [queryParams]="{ focus: ev }"
                            >{{ ev }}</a>
                          }
                        </div>
                      }
                      @if (insight.action !== 'None' && insight.actionTarget) {
                        <a class="mt-1.5 inline-block text-2xs text-accent hover:underline"
                           [routerLink]="['/explore']"
                           [queryParams]="{ focus: insight.actionTarget }">
                          {{ insight.action === 'Trace' ? 'Trace it →' : insight.action === 'Usages' ? 'See usages →' : 'Export →' }}
                        </a>
                      }
                    </div>
                  </div>
                }
              </div>
            </div>
          }
        </div>
      }

      <!-- Coverage bar -->
      @if (store.stats(); as s) {
        <div class="border-t border-line pt-3">
          <span class="text-2xs text-ink-muted uppercase">Coverage</span>
          @if (s.graph; as g) {
            @if (g.entries > 0) {
              <p class="text-sm text-ink mt-1">{{ g.entriesWithTarget }}/{{ g.entries }} entries have resolved targets</p>
              <div class="mt-1 h-1 bg-surface-2 rounded-full overflow-hidden">
                <div class="h-full bg-accent rounded-full" [style.width.%]="coveragePct()"></div>
              </div>
            }
          }
        </div>
      }

      <!-- Engine details -->
      @if (store.stats(); as s) {
        <details class="border-t border-line pt-3">
          <summary class="text-xs text-ink-muted cursor-pointer hover:text-ink">Engine details</summary>
          <div class="mt-2 text-xs text-ink-muted space-y-1">
            @if (s.graph; as g) {
              <p>Nodes: {{ g.nodes }} · Edges: {{ g.edges }} · Entries: {{ g.entries }}</p>
            }
            @if (s.totalWallMs) {
              <p>Analysis time: {{ s.totalWallMs }}ms</p>
            }
          </div>
        </details>
      }
    </div>
  `,
})
export class InsightsView {
  readonly store = inject(SessionStore);

  readonly groups = computed(() => {
    const list = this.store.insights();
    if (!list.length) return [] as InsightGroup[];

    const map = new Map<string, {
      id: string; title: string; severity: string; severityClass: string;
      detail: string; evidence: string[];
      confidence: number; confidenceBasis?: string;
      whyItMatters?: string; action: string; actionTarget?: string;
    }[]>();
    for (const i of list) {
      const impact = IMPACT_GROUPS[i.severity] ?? 'Know this';
      if (!map.has(impact)) map.set(impact, []);
      map.get(impact)!.push({
        id: i.id,
        title: i.title,
        severity: i.severity,
        severityClass: SEVERITY_CLASS[i.severity] ?? SEVERITY_CLASS['info'],
        detail: i.detail,
        evidence: [...new Set(i.evidence)],
        confidence: i.confidence,
        confidenceBasis: i.confidenceBasis,
        whyItMatters: i.whyItMatters,
        action: i.action,
        actionTarget: i.actionTarget,
      });
    }
    return [...map.entries()].map(([impact, insights]) => ({ impact, insights }));
  });

  readonly coveragePct = computed(() => {
    const g = this.store.stats()?.graph;
    if (!g || !g.entries) return 0;
    return Math.round(((g.entriesWithTarget ?? 0) / g.entries) * 100);
  });

  severityLabelClass(severity: string): string {
    return SEVERITY_LABEL_CLASS[severity] ?? SEVERITY_LABEL_CLASS['info'];
  }

  retryStats(): void {
    this.store.refreshStats();
  }
}
