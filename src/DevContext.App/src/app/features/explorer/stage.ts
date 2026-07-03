import { Component, computed, inject, output, signal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { GraphCanvas } from '../../ui/graph-canvas/graph-canvas';
import { TraceNodeComponent } from '../trace/trace-node';

export type StageAltitude = 'system' | 'flow' | 'node';
export type FlowMode = 'tree' | 'graph';

/**
 * Stage (F proposal §2) — one center canvas, three altitudes, never blank:
 *  - system: project topology from MapResponse.topology[] — available the moment
 *    analysis completes, before any trace (kills the blank-graph pain structurally);
 *  - flow: the current trace as Tree or Graph (today's /trace + /graph merged);
 *  - node: one-hop neighborhood of the selected node from GetNeighbors.
 * Loading is content-preserving: previous content dims to 60% under a hairline.
 *
 * TODO(W4): system altitude gets a real GraphCanvas topology builder
 * (`buildFromTopology`) — the list below is the correct data through the wrong lens.
 * TODO(W4): node altitude gets a direction toggle (out | in | usages).
 */
@Component({
  selector: 'app-stage',
  imports: [GraphCanvas, TraceNodeComponent],
  host: { class: 'panel relative flex h-full min-h-0 flex-col' },
  template: `
    @if (trace.loading()) {
      <div class="hairline"></div>
    }

    <div class="flex items-center gap-1 border-b border-line px-2 py-1">
      @for (alt of altitudes; track alt.id) {
        <button
          type="button"
          class="chip"
          [class.active]="altitude() === alt.id"
          [title]="alt.hint"
          (click)="altitude.set(alt.id)"
        >
          {{ alt.label }}
        </button>
      }
      <span class="mx-1 h-4 w-px bg-line"></span>
      @if (altitude() === 'flow') {
        <button type="button" class="chip" [class.active]="flowMode() === 'tree'" (click)="flowMode.set('tree')">
          Tree
        </button>
        <button type="button" class="chip" [class.active]="flowMode() === 'graph'" (click)="flowMode.set('graph')">
          Graph
        </button>
        @if (flowMode() === 'graph') {
          <select
            class="ml-1 bg-transparent text-2xs text-ink-muted focus:outline-none"
            [value]="graphDepth()"
            (change)="onGraphDepth($event)"
            title="Graph depth"
          >
            @for (d of [1, 2, 3, 4]; track d) {
              <option [value]="d">depth {{ d }}</option>
            }
          </select>
        }
      }
      <span class="flex-1"></span>
      @if (trace.focus(); as focus) {
        <span class="truncate font-mono text-2xs text-ink-subtle" [title]="focus">{{ focus }}</span>
      }
    </div>

    <div class="min-h-0 flex-1 overflow-auto transition-opacity" [class.opacity-60]="trace.loading()">
      @switch (altitude()) {
        @case ('system') {
          @if (topology().length > 0) {
            <div class="p-2">
              <p class="mb-2 text-2xs text-ink-subtle">
                Project topology — {{ topology().length }} projects. Click filtering + graph canvas land in W4.
              </p>
              @for (project of topology(); track project.name) {
                <div class="list-row">
                  <span class="font-mono text-xs text-ink">{{ project.name }}</span>
                  @if (project.dependsOn.length > 0) {
                    <span class="truncate text-2xs text-ink-subtle">→ {{ project.dependsOn.join(', ') }}</span>
                  }
                </div>
              }
            </div>
          } @else {
            <div class="flex h-full items-center justify-center text-xs text-ink-subtle">
              Analyze a repo to see its project topology.
            </div>
          }
        }
        @case ('flow') {
          @if (trace.tree(); as tree) {
            @if (flowMode() === 'tree') {
              <div class="p-2">
                <app-trace-node [node]="tree" (nodeSelected)="nodeSelected.emit($event)" />
              </div>
            } @else {
              <app-graph-canvas
                class="block h-full"
                [trace]="tree"
                [maxDepth]="graphDepth()"
                (nodeSelected)="nodeSelected.emit($event)"
              />
            }
          } @else if (!trace.found()) {
            <div class="flex h-full items-center justify-center text-xs text-ink-subtle">
              Trace not found for this focus.
            </div>
          } @else {
            <div class="flex h-full items-center justify-center text-xs text-ink-subtle">
              @if (session.ready()) {
                Select an entry on the left — <span class="kbd mx-1">j</span>/<span class="kbd">k</span> to scrub.
              } @else {
                Analyze a repo first.
              }
            </div>
          }
        }
        @case ('node') {
          @if (trace.selectedNodeId(); as nodeId) {
            <div class="p-2">
              <p class="mb-2 truncate font-mono text-2xs text-ink-subtle" [title]="nodeId">
                {{ nodeId }} — outgoing edges (direction toggle lands in W4)
              </p>
              @for (edge of trace.neighbors(); track edge.to) {
                <div
                  class="list-row"
                  role="button"
                  tabindex="0"
                  (click)="nodeSelected.emit(edge.to)"
                  (keydown.enter)="nodeSelected.emit(edge.to)"
                  (keydown.space)="nodeSelected.emit(edge.to); $event.preventDefault()"
                >
                  <span class="chip shrink-0">{{ edge.kind }}</span>
                  <span class="min-w-0 flex-1 truncate font-mono text-xs">{{ edge.otherTitle || edge.to }}</span>
                  <span
                    class="shrink-0 text-2xs"
                    [class.text-success]="edge.resolution === 'Semantic'"
                    [class.text-warn]="edge.resolution !== 'Semantic'"
                  >
                    {{ edge.resolution === 'Semantic' ? 'verified' : 'approx' }}
                  </span>
                </div>
              } @empty {
                <p class="text-xs text-ink-subtle">No outgoing edges.</p>
              }
            </div>
          } @else {
            <div class="flex h-full items-center justify-center text-xs text-ink-subtle">
              Select a node in a trace to inspect its neighborhood.
            </div>
          }
        }
      }
    </div>
  `,
})
export class Stage {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);

  readonly nodeSelected = output<string>();

  protected readonly altitude = signal<StageAltitude>('flow');
  protected readonly flowMode = signal<FlowMode>('tree');
  protected readonly graphDepth = signal(3);

  protected readonly altitudes: readonly { id: StageAltitude; label: string; hint: string }[] = [
    { id: 'system', label: 'System', hint: 'Project topology (v s)' },
    { id: 'flow', label: 'Flow', hint: 'Current trace (v t / v g)' },
    { id: 'node', label: 'Node', hint: 'Selected node neighborhood (v n)' },
  ];

  protected readonly topology = computed(() => this.session.mapResponse()?.topology ?? []);

  protected onGraphDepth(event: Event): void {
    this.graphDepth.set(Number((event.target as HTMLSelectElement).value));
  }
}
