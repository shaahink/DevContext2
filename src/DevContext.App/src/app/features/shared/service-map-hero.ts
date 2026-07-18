import { Component, computed, inject, input } from '@angular/core';
import { Router } from '@angular/router';

import type { ProjectNode, ServiceStyle } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { SessionStore } from '../../state/session.store';
import { GraphCanvas, type GraphCanvasData } from '../../ui/graph-canvas/graph-canvas';

/**
 * Hero graph on Home + Atlas (T6.7, audit B1). The old version hand-rolled a
 * single-column card stack that never drew an edge while the Service lens proved the
 * cytoscape renderer works on the same data — so this now IS that renderer, in the
 * canvas's compact mode (no controls, no pan/zoom, taps navigate to the Service lens).
 * Test/benchmark projects are already excluded by the engine's topology (T1.9).
 */
@Component({
  selector: 'app-service-map-hero',
  imports: [GraphCanvas],
  template: `
    @if (topology().length > 0) {
      <app-graph-canvas
        [data]="canvasData()"
        [compact]="true"
        (nodeActivated)="openProject($event)"
      />
      @if (hasBus(); as bus) {
        <div class="mt-1 flex justify-center border-t border-dashed border-line pt-1.5">
          <span class="chip text-2xs text-warn" title="Message transport detected from package/config signals">{{ bus }}</span>
        </div>
      }
    } @else {
      <p class="py-8 text-center text-xs text-ink-subtle">No project topology resolved.</p>
    }
  `,
})
export class ServiceMapHero {
  protected readonly session = inject(SessionStore);
  private readonly router = inject(Router);

  readonly topology = input.required<readonly ProjectNode[]>();
  readonly serviceStyles = input.required<readonly ServiceStyle[]>();

  /** D4.2: the ServiceMap facet rides along — the canvas defaults to C4 level 1
   * (services + transport-labeled edges) when the facet has services, and single-tap
   * expands a service in place; navigation moved to double-tap (nodeActivated). */
  protected readonly canvasData = computed<GraphCanvasData>(() => ({
    mode: 'topology',
    projects: this.topology(),
    services: this.session.graphFacets()?.serviceMap?.services ?? [],
    transports: this.session.graphFacets()?.serviceMap?.transports ?? [],
  }));

  protected openProject(name: string): void {
    if (!name) return;
    void this.router.navigate(['/explore'], { queryParams: { view: 'system', project: name } });
  }

  protected readonly hasBus = computed(() => {
    // Prefer the projection's transports — a real "bus" ServiceLink edge is authoritative.
    const transports = this.session.graphFacets()?.serviceMap?.transports ?? [];
    const busTransports = new Set(
      transports.filter((t) => /bus|rabbitmq|kafka|masstransit|queue/i.test(t.transport)).map((t) => t.transport),
    );
    if (busTransports.size > 0) return [...busTransports].join(' / ');

    const seen = new Set<string>();
    for (const s of this.serviceStyles()) {
      for (const t of s.stack) {
        if (/rabbitmq|masstransit|kafka|nservicebus|azure.*service.*bus/i.test(t)) {
          seen.add(t);
        }
      }
    }
    return seen.size > 0 ? [...seen].join(' / ') : null;
  });
}
