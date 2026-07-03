import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { GraphCanvas } from '../../ui/graph-canvas/graph-canvas';
import { ArchitecturePanel } from '../atlas/architecture-panel';

/**
 * Atlas (proposal §2) — architecture: map prose-zone, project topology graph,
 * packages/pipeline (ArchitecturePanel), Event Wiring Board, Hub Radar. Topology
 * graph reuses the exact `graph-canvas` `topology` binding the Workbench's System
 * altitude uses (`features/explorer/stage.ts`).
 *
 * Event Wiring Board + Hub Radar are surfaced now (data's already computed by
 * `AtlasStore` from W3) even though the background indexer that populates them is a
 * W5 item — per AGENTS.md's recorded call to pull this forward since the marginal
 * cost is ~0. They render empty with an explanatory note until W5 wires
 * `AtlasStore.start()` on analysis-ready.
 */
@Component({
  selector: 'app-atlas-page',
  imports: [GraphCanvas, ArchitecturePanel],
  template: `
    <div class="mx-auto max-w-4xl space-y-8 px-5 pb-10 pt-6">
      @if (!session.ready()) {
        <p class="py-8 text-center text-xs text-ink-subtle">Analyze a repo to see its atlas.</p>
      } @else {
        @if (mapMarkdown(); as md) {
          <div class="prose-zone whitespace-pre-wrap">{{ md }}</div>
        }

        <div>
          <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Topology</h2>
          @if (topology().length > 0) {
            <app-graph-canvas
              class="block h-80 rounded-md border border-line"
              [data]="{ mode: 'topology', projects: topology() }"
              (nodeSelected)="onProjectTap($event)"
            />
          } @else {
            <p class="py-8 text-center text-xs text-ink-subtle">No project topology detected.</p>
          }
        </div>

        <div>
          <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Architecture</h2>
          <app-architecture-panel />
        </div>

        <div>
          <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Event Wiring Board</h2>
          @if (eventWiring().length) {
            <div class="overflow-x-auto rounded border border-line">
              <table class="w-full text-left text-xs">
                <thead>
                  <tr class="border-b border-line bg-surface-2 text-2xs uppercase tracking-wider text-ink-muted">
                    <th class="px-2 py-1.5 font-medium">Publisher</th>
                    <th class="px-2 py-1.5 font-medium">Event</th>
                    <th class="px-2 py-1.5 font-medium">Consumer</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-line">
                  @for (w of eventWiring(); track w.event + w.publisherFocus) {
                    <tr class="hover:bg-surface-2">
                      <td class="px-2 py-1.5 font-mono text-ink">{{ w.publisherTitle }}</td>
                      <td class="px-2 py-1.5 text-ink-muted">{{ w.event }}</td>
                      <td class="px-2 py-1.5 font-mono text-ink">
                        @if (w.consumerTitle) {
                          {{ w.consumerTitle }}
                        } @else {
                          <span class="chip text-warn">unmatched</span>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">
              No wiring indexed yet — the background flow indexer arrives in W5.
            </p>
          }
        </div>

        <div>
          <h2 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Hub Radar</h2>
          @if (hubs().length) {
            <div class="space-y-1">
              @for (h of hubs(); track h.nodeId) {
                <div class="list-row justify-between">
                  <span class="min-w-0 flex-1 truncate font-mono text-xs text-ink">{{ h.title }}</span>
                  <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ h.flowCount }} flows</span>
                </div>
              }
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">
              No hubs indexed yet — the background flow indexer arrives in W5.
            </p>
          }
        </div>
      }
    </div>
  `,
})
export class AtlasPage {
  protected readonly session = inject(SessionStore);
  protected readonly atlas = inject(AtlasStore);
  private readonly router = inject(Router);

  protected readonly mapMarkdown = this.session.mapMarkdown;
  protected readonly topology = computed(() => this.session.mapResponse()?.topology ?? []);
  protected readonly eventWiring = this.atlas.eventWiring;
  protected readonly hubs = this.atlas.hubs;

  /** Topology node tap ('' = empty-canvas tap, ignored) — land on Explore's System altitude. */
  protected onProjectTap(name: string): void {
    if (!name) return;
    void this.router.navigate(['/explore'], { queryParams: { view: 'system' } });
  }
}
