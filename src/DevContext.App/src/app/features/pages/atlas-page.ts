import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { TraceStore } from '../../state/trace.store';
import { ServiceMapHero } from '../shared/service-map-hero';
import { FlowStepper } from '../shared/flow-stepper';
import { ServiceCards } from '../shared/service-cards';

@Component({
  selector: 'app-atlas-page',
  imports: [RouterLink, ServiceMapHero, FlowStepper, ServiceCards],
  template: `
    <div class="mx-auto max-w-4xl space-y-8 px-5 pb-10 pt-6">
      @if (!session.ready()) {
        <p class="py-8 text-center text-xs text-ink-subtle">Analyze a repo to see its atlas.</p>
      } @else {
        <!-- M6.2: Export button (top-right) -->
        <div class="flex items-center justify-between">
          <h1 class="text-lg font-bold text-ink">Atlas</h1>
          <button
            class="flex items-center gap-1.5 rounded-md border border-line bg-surface px-3 py-1.5 text-xs text-ink hover:border-accent hover:bg-surface-2 transition-colors"
            [class.copied]="copied()"
            (click)="copyAtlas()"
          >
            <span class="i-lucide-file-text h-3.5 w-3.5"></span>
            {{ copied() ? 'Copied!' : 'Export one-pager' }}
          </button>
        </div>

        <!-- M6.2: Identity paragraph -->
        <div class="prose-zone text-sm text-ink leading-relaxed">
          @if (mapMarkdown(); as md) {
            {{ md.split('\n').slice(0, 15).join('\n') }}
          }
        </div>

        <!-- ① Service diagram (larger) -->
        <div>
          <h2 class="section-h mb-3">Service diagram</h2>
          <p class="mb-2 text-2xs text-ink-subtle">
            {{ topology().length }} projects &middot;
            {{ topology().reduce((n, p) => n + p.dependsOn.length, 0) }} dependency edges
          </p>
          <app-service-map-hero
            [topology]="topology()"
            [serviceStyles]="serviceStyles()"
          />
        </div>

        <!-- ② Top flows as stepper strips -->
        <div>
          <h2 class="section-h mb-3">Top flows</h2>
          <app-flow-stepper [flows]="atlasTopFlows()" />
        </div>

        <!-- ③ Event wiring board -->
        <div>
          <h2 class="section-h mb-3">Event wiring board</h2>
          @if (eventWiring().length) {
            <div class="overflow-x-auto rounded border border-line">
              <table class="w-full text-left text-xs">
                <thead>
                  <tr class="border-b border-line bg-surface-2 text-2xs uppercase tracking-wider text-ink-muted">
                    <th class="px-3 py-2 font-medium">Publisher</th>
                    <th class="px-3 py-2 font-medium">Event</th>
                    <th class="px-3 py-2 font-medium">Consumer</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-line">
                  @for (w of eventWiring(); track w.event + w.publisherFocus) {
                    <tr class="hover:bg-surface-2">
                      <td class="px-3 py-2 font-mono text-ink">
                        <a class="hover:text-accent hover:underline" [routerLink]="['/explore']" [queryParams]="{ focus: w.publisherFocus }">
                          {{ w.publisherTitle }}
                        </a>
                      </td>
                      <td class="px-3 py-2 text-ink-muted">
                        {{ w.event }}
                        <span class="chip text-warn ml-1 text-2xs">approx</span>
                      </td>
                      <td class="px-3 py-2 font-mono text-ink">
                        @if (w.consumerTitle && w.consumerFocus) {
                          <a class="hover:text-accent hover:underline" [routerLink]="['/explore']" [queryParams]="{ focus: w.consumerFocus }">
                            {{ w.consumerTitle }}
                          </a>
                        } @else {
                          <span class="chip text-warn text-2xs">unconsumed</span>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">No event wiring data — index flows from the Explore page.</p>
          }
        </div>

        <!-- ④ Per-service cards -->
        <div>
          <h2 class="section-h mb-3">Per-service breakdown</h2>
          <app-service-cards [services]="serviceStyles()" />
        </div>

        <!-- ⑤ Cross-cutting (behaviors, packages) -->
        <div>
          <h2 class="section-h mb-3">Cross-cutting</h2>
          @if (pipelineBehaviors().length || packages().length) {
            <div class="flex flex-wrap gap-3">
              @if (pipelineBehaviors().length) {
                <div class="flex-1 min-w-48 rounded-lg border border-line bg-surface p-3">
                  <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle mb-2">Pipeline behaviors</h3>
                  <div class="flex flex-wrap gap-1.5">
                    @for (b of pipelineBehaviors(); track b) {
                      <span class="chip text-xs">{{ b }}</span>
                    }
                  </div>
                </div>
              }
              @if (packages().length) {
                <div class="flex-1 min-w-48 rounded-lg border border-line bg-surface p-3">
                  <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle mb-2">Packages</h3>
                  <div class="flex flex-wrap gap-1.5">
                    @for (pg of packages(); track pg.label) {
                      <span class="chip text-xs" [title]="pg.packages.join(', ')">{{ pg.label }} ({{ pg.packages.length }})</span>
                    }
                  </div>
                </div>
              }
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">No cross-cutting data resolved.</p>
          }
        </div>

        <!-- ⑥ Hub radar -->
        <div>
          <h2 class="section-h mb-3">Hub radar</h2>
          <p class="mb-2 text-2xs text-ink-subtle">Nodes appearing in the most distinct flows across the repo.</p>
          @if (hubsWithDegree().length) {
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
            <p class="py-4 text-center text-xs text-ink-subtle">No hubs indexed — index flows from the Explore page.</p>
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
  protected readonly serviceStyles = computed(() => this.session.mapResponse()?.serviceStyles ?? []);
  protected readonly pipelineBehaviors = computed(() => this.session.mapResponse()?.pipelineBehaviors ?? []);
  protected readonly packages = computed(() => this.session.mapResponse()?.packages ?? []);
  protected readonly eventWiring = this.atlas.eventWiring;
  protected readonly hubsWithDegree = this.atlas.hubsWithDegree;
  protected readonly atlasTopFlows = computed(() => this.atlas.topFlows().slice(0, 5));

  protected copied = signal(false);

  protected onProjectTap(name: string): void {
    if (!name) return;
    void this.router.navigate(['/explore'], { queryParams: { view: 'system', project: name } });
  }

  protected onHubTap(nodeId: string): void {
    void this.trace.selectNode(nodeId, 'usages');
    void this.router.navigate(['/explore'], { queryParams: { view: 'node' } });
  }

  /** M6.2: Export one-pager — copies a markdown-like text to clipboard. */
  protected async copyAtlas(): Promise<void> {
    const md = this.buildAtlasMarkdown();
    try {
      await navigator.clipboard.writeText(md);
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      // Fallback for non-https
    }
  }

  private buildAtlasMarkdown(): string {
    const s = this.session.summary();
    const lines: string[] = [];

    lines.push(`# Atlas — ${s?.label ?? 'Repo'}`);
    lines.push('');
    lines.push(`**Archetype:** ${this.session.mapResponse()?.archetype ?? 'unknown'} | **Projects:** ${this.topology().length} | **Entries:** ${s?.entries ?? 0}`);
    lines.push('');

    // Service diagram (text representation)
    lines.push('## Services');
    for (const p of this.topology()) {
      const style = this.serviceStyles().find((st) => st.projectName === p.name);
      const deps = p.dependsOn.length > 0 ? ` → ${p.dependsOn.map((d) => d.split('.').pop()).join(', ')}` : '';
      lines.push(`- **${p.name.split('.').pop() ?? p.name}**${style?.style ? ` (${style.style})` : ''}${deps}`);
      if (style?.stack.length) lines.push(`  Stack: ${style.stack.join(', ')}`);
    }
    lines.push('');

    // Top flows
    lines.push('## Top Flows');
    for (const f of this.atlasTopFlows()) {
      lines.push(`- **${f.title}** (${f.nodeCount} steps, ${f.maxDepth} deep, ${f.boundaryCrossings} cross-service)`);
    }
    lines.push('');

    // Event wiring
    const wiring = this.eventWiring();
    if (wiring.length) {
      lines.push('## Event Wiring');
      for (const w of wiring) {
        lines.push(`- ${w.publisherTitle} → **${w.event}** → ${w.consumerTitle ?? 'unconsumed'}`);
      }
      lines.push('');
    }

    // Hub radar
    const hubs = this.hubsWithDegree();
    if (hubs.length) {
      lines.push('## Hub Radar');
      for (const h of hubs) {
        lines.push(`- ${h.title}: ${h.flowCount} flows${h.degree ? ` (in ${h.degree.inDegree}, out ${h.degree.outDegree})` : ''}`);
      }
      lines.push('');
    }

    // Cross-cutting
    const behaviors = this.pipelineBehaviors();
    if (behaviors.length) lines.push(`## Pipeline Behaviors\n${behaviors.map((b) => `- ${b}`).join('\n')}\n`);

    return lines.join('\n');
  }
}
