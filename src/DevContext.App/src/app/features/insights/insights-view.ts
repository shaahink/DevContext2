import { Component, inject } from '@angular/core';
import { SessionStore } from '../../state/session.store';
import { Card } from '../../ui/card/card';
import { Badge } from '../../ui/badge/badge';

@Component({
  selector: 'app-insights-view',
  standalone: true,
  imports: [Card, Badge],
  template: `
    <div class="flex flex-col h-full p-4 space-y-4 overflow-y-auto">
      <h2 class="text-lg font-semibold text-ink">Insights</h2>
      <p class="text-sm text-ink-muted">Repo-specific findings — what's notable, risky, or wired in interesting ways.</p>

      @if (store.summary(); as s) {
        <div class="grid gap-3">
          @if (s.archetype) {
            <app-card>
              <span class="text-2xs text-ink-muted uppercase">Shape</span>
              <p class="text-sm text-ink mt-1">{{ s.archetype }} · {{ s.projectCount }} projects · {{ s.entryCount }} entries</p>
            </app-card>
          }
          @if (s.entryTargetRatio !== null) {
            <app-card>
              <span class="text-2xs text-ink-muted uppercase">Coverage</span>
              <p class="text-sm text-ink mt-1">{{ s.entryTargetRatio }}% of entries have resolved targets</p>
              <div class="mt-1 h-1 bg-surface-2 rounded-full overflow-hidden">
                <div class="h-full bg-accent rounded-full" [style.width.%]="s.entryTargetRatio"></div>
              </div>
            </app-card>
          }
          @if (s.seamCounts?.length) {
            <app-card>
              <span class="text-2xs text-ink-muted uppercase">Wiring seams</span>
              <div class="flex flex-wrap gap-2 mt-2">
                @for (sc of s.seamCounts; track sc.kind) {
                  <span class="rounded bg-surface-2 px-2 py-0.5 text-xs text-ink">{{ sc.kind }}: {{ sc.count }}</span>
                }
              </div>
            </app-card>
          }
        </div>
      } @else {
        <div class="text-sm text-ink-muted">Analyze a repo to see insights.</div>
      }

      <details class="border-t border-line pt-3">
        <summary class="text-xs text-ink-muted cursor-pointer hover:text-ink">Engine details</summary>
        <div class="mt-2 text-xs text-ink-muted space-y-1">
          @if (store.lastStats(); as stats) {
            <p>Nodes: {{ stats.graphNodeCount ?? '—' }} · Edges: {{ stats.graphEdgeCount ?? '—' }}</p>
            <p>Format: {{ stats.schema }}</p>
          }
        </div>
      </details>
    </div>
  `,
})
export class InsightsView {
  readonly store = inject(SessionStore);
}
