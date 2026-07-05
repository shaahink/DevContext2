import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { TraceStore } from '../../state/trace.store';
import { GraphCanvas } from '../../ui/graph-canvas/graph-canvas';
import { ArchitecturePanel } from '../atlas/architecture-panel';

/**
 * Atlas (proposal §2) — architecture: map prose-zone, project topology graph,
 * packages/pipeline (ArchitecturePanel), Event Wiring Board, Hub Radar. Topology
 * graph reuses the exact `graph-canvas` `topology` binding the Workbench's System
 * altitude uses (`features/explorer/stage.ts`).
 *
 * Event Wiring Board (§3.3): both sides click through to a trace. Publisher/consumer
 * are registered entry focuses (`GetTrace`-resolvable), so a plain `routerLink` +
 * `?focus=` does it — same pattern as Home's Top Flow links, no imperative selectNode
 * needed. The join itself is a heuristic name-match (not a real graph edge), badged
 * `[approx]` per proposal §3.3's own vocabulary.
 *
 * Hub Radar (§3.7): `hubsWithDegree()` enriches the flow-appearance count with a real
 * in/out-degree from `getNode` (see `AtlasStore`'s enrichment effect) once it resolves.
 * Hub node ids are raw internal graph ids, NOT registered entry focuses — `GetTrace`'s
 * focus resolution only accepts the latter (documented elsewhere in this codebase), so
 * click-through here goes through `TraceStore.selectNode()` + `?view=node` instead of
 * a `routerLink` `?focus=`, same pattern as the omnibox's "Node" verb.
 */
@Component({
  selector: 'app-atlas-page',
  imports: [GraphCanvas, ArchitecturePanel, RouterLink],
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
                      <td class="px-2 py-1.5 font-mono text-ink">
                        <a class="hover:text-accent hover:underline" [routerLink]="['/explore']" [queryParams]="{ focus: w.publisherFocus }">
                          {{ w.publisherTitle }}
                        </a>
                      </td>
                      <td class="px-2 py-1.5 text-ink-muted">
                        {{ w.event }}
                        <span class="chip text-warn ml-1" title="Heuristic name-match, not a verified graph edge">approx</span>
                      </td>
                      <td class="px-2 py-1.5 font-mono text-ink">
                        @if (w.consumerTitle && w.consumerFocus) {
                          <a class="hover:text-accent hover:underline" [routerLink]="['/explore']" [queryParams]="{ focus: w.consumerFocus }">
                            {{ w.consumerTitle }}
                          </a>
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
          @if (hubsWithDegree().length) {
            <p class="mb-2 text-2xs text-ink-subtle">Among mapped flows — not a whole-graph ranking (no such RPC exists).</p>
            <div class="space-y-1">
              @for (h of hubsWithDegree(); track h.nodeId) {
                <div
                  class="list-row justify-between"
                  role="button"
                  tabindex="0"
                  (click)="onHubTap(h.nodeId)"
                  (keydown.enter)="onHubTap(h.nodeId)"
                  (keydown.space)="onHubTap(h.nodeId); $event.preventDefault()"
                >
                  <span class="min-w-0 flex-1 truncate font-mono text-xs text-ink">{{ h.title }}</span>
                  <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">
                    {{ h.flowCount }} flow{{ h.flowCount === 1 ? '' : 's' }}
                    @if (h.degree; as d) {
                      &middot; in {{ d.inDegree }} &middot; out {{ d.outDegree }}
                    }
                  </span>
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
  private readonly trace = inject(TraceStore);
  private readonly router = inject(Router);

  protected readonly mapMarkdown = this.session.mapMarkdown;
  protected readonly topology = computed(() => this.session.mapResponse()?.topology ?? []);
  protected readonly eventWiring = this.atlas.eventWiring;
  protected readonly hubsWithDegree = this.atlas.hubsWithDegree;

  /** Topology node tap ('' = empty-canvas tap, ignored) — land on Explore's System altitude. */
  protected onProjectTap(name: string): void {
    if (!name) return;
    void this.router.navigate(['/explore'], { queryParams: { view: 'system', project: name } });
  }

  /** Hub Radar row click — raw node id, not a registered entry focus, so this goes
   * through selectNode (same as the omnibox's "Node" verb) rather than a `?focus=` link. */
  protected onHubTap(nodeId: string): void {
    void this.trace.selectNode(nodeId, 'usages');
    void this.router.navigate(['/explore'], { queryParams: { view: 'node' } });
  }
}
