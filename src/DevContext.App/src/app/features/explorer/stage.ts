import { Component, computed, DestroyRef, effect, inject, model, output, signal } from '@angular/core';
import { NgClass } from '@angular/common';

import type { NeighborDirection } from '../../data-access/devcontext-api';
import { filterApproxTree, type TraceNodeVm } from '../../models/view-models';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { GraphCanvas, type GraphCanvasData } from '../../ui/graph-canvas/graph-canvas';
import { Meter, type MeterVariant } from '../../ui/meter/meter';
import { TraceNodeComponent } from '../trace/trace-node';
import { LensSwitcher, type LensId } from './lens-switcher';

export type StageAltitude = 'system' | 'flow' | 'node';
export type FlowMode = 'tree' | 'graph';
export type NodeViewMode = 'list' | 'graph';

const DIRECTIONS: readonly { id: NeighborDirection; label: string; hint: string }[] = [
  { id: 'out', label: 'Out', hint: 'What this node calls' },
  { id: 'in', label: 'In', hint: 'Direct graph callers' },
  { id: 'usages', label: 'Usages', hint: 'Resolved usages across the codebase' },
];

/**
 * Stage (F proposal §2) — one center canvas, three altitudes, never blank:
 *  - system: project topology from MapResponse.topology[] — available the moment
 *    analysis completes, before any trace (kills the blank-graph pain structurally);
 *  - flow: the current trace as Tree or Graph (today's /trace + /graph merged);
 *  - node: one-hop neighborhood of the selected node from GetNeighbors, direction
 *    toggle (out/in/usages), List (resolution detail) or Graph view.
 * Loading is content-preserving: previous content dims to 60% under a hairline.
 *
 * Double-click on any graph node, any altitude, re-traces from it (`retrace` output).
 * Single-click on a System project node filters the deck to that project instead of
 * selecting a "node" (projects aren't traceable entries) — `projectSelected` output.
 */
@Component({
  selector: 'app-stage',
  imports: [GraphCanvas, TraceNodeComponent, Meter, NgClass, LensSwitcher],
  host: { class: 'panel relative flex h-full min-h-0 flex-col' },
  template: `
    <div
      [ngClass]="zenMode() ? 'fixed inset-0 z-50 flex flex-col bg-base' : 'contents'"
      (keydown.escape)="zenMode.set(false)"
      tabindex="0"
    >
      @if (trace.loading()) {
        <div class="hairline"></div>
      }

      <div class="flex items-center gap-1 border-b border-line px-2 py-1" (dblclick)="zenMode.set(!zenMode())" title="Double-click for zen mode">
        <button
          type="button"
          class="chip shrink-0"
          [class.active]="zenMode()"
          (click)="zenMode.set(!zenMode()); $event.stopPropagation()"
          title="Zen mode (F)"
        >&#9641;</button>

        <!-- M7.2: Lens switcher — replaces old altitude buttons. L6.5: + Table button. -->
        <app-lens-switcher [(lensModel)]="lensModel" (tableRequested)="tableRequested.emit()" />

        @if (lensModel() === 'service' || lensModel() === 'layer' || lensModel() === 'feature') {
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
        }
      @if (lensModel() === 'flow') {
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
        @if (flowMode() === 'tree') {
          <button
            type="button"
            class="chip"
            [class.active]="approxOnly()"
            (click)="approxOnly.set(!approxOnly())"
            title="Show only approx-resolved nodes (and their ancestors, so they stay reachable from the root)"
          >
            approx only
          </button>
        }
      }
      @if (altitude() === 'node') {
        @for (dir of directions; track dir.id) {
          <button type="button" class="chip" [class.active]="trace.neighborDirection() === dir.id" [title]="dir.hint" (click)="onDirection(dir.id)">
            {{ dir.label }}
          </button>
        }
        <span class="mx-1 h-4 w-px bg-line"></span>
        <button type="button" class="chip" [class.active]="nodeViewMode() === 'list'" (click)="nodeViewMode.set('list')">
          List
        </button>
        <button type="button" class="chip" [class.active]="nodeViewMode() === 'graph'" (click)="nodeViewMode.set('graph')">
          Graph
        </button>
      }
      }
      <span class="flex-1"></span>
      @if (altitude() === 'flow' && verifiedPct() !== null) {
        <div class="flex items-center gap-1.5" title="Verified vs approx resolution across this trace">
          <app-meter [value]="verifiedPct()!" [variant]="meterVariant(verifiedPct()!)" class="w-12" />
          <span class="tabular-nums text-2xs text-ink-subtle">{{ verifiedPct() }}%</span>
        </div>
      }
      @if (trace.focus(); as focus) {
        <span class="truncate font-mono text-2xs text-ink-subtle" [title]="focus">{{ focus }}</span>
      }
    </div>

    <div class="min-h-0 flex-1 overflow-auto transition-opacity" [class.opacity-60]="trace.loading()">
      @switch (altitude()) {
        @case ('system') {
          @if (topology().length > 0) {
              <app-graph-canvas
                class="block h-full"
                [data]="topologyData()"
                [highlightedNodeId]="highlightedNodeId()"
                [zenMode]="zenMode()"
                [lensId]="lensModel()"
                (nodeSelected)="onProjectTap($event)"
              />
          } @else {
            <div class="flex h-full items-center justify-center text-xs text-ink-subtle">
              Analyze a repo to see its project topology.
            </div>
          }
        }
        @case ('flow') {
          @if (trace.tree(); as rawTree) {
            @if (flowMode() === 'tree') {
              @if (displayedTree(); as tree) {
                <div class="p-2">
                  <app-trace-node [node]="tree" (nodeSelected)="nodeSelected.emit($event)" />
                </div>
              } @else {
                <div class="flex h-full items-center justify-center text-xs text-ink-subtle">
                  Nothing approx in this trace &mdash; fully verified.
                </div>
              }
            } @else {
              <app-graph-canvas
                class="block h-full"
                [data]="{ mode: 'trace', root: rawTree, maxDepth: graphDepth() }"
                [highlightedNodeId]="highlightedNodeId()"
                [zenMode]="zenMode()"
                [lensId]="'flow'"
                (nodeSelected)="onFlowTap($event)"
                (nodeActivated)="retrace.emit($event)"
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
            @if (nodeViewMode() === 'graph') {
              <app-graph-canvas
                class="block h-full"
                [data]="{ mode: 'neighbors', centerId: nodeId, centerTitle: trace.nodeDetail()?.title ?? nodeId, edges: trace.neighbors() }"
                [highlightedNodeId]="highlightedNodeId()"
                [zenMode]="zenMode()"
                [lensId]="'flow'"
                (nodeSelected)="onNodeTap($event)"
                (nodeActivated)="retrace.emit($event)"
              />
            } @else {
              <div class="p-2">
                <p class="mb-2 truncate font-mono text-2xs text-ink-subtle" [title]="nodeId">{{ nodeId }}</p>
                @for (edge of trace.neighbors(); track edge.to) {
                  <div
                    class="list-row"
                    role="button"
                    tabindex="0"
                    (click)="onNodeTap(edge.to)"
                    (dblclick)="retrace.emit(edge.to)"
                    (keydown.enter)="onNodeTap(edge.to)"
                    (keydown.space)="onNodeTap(edge.to); $event.preventDefault()"
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
                  <p class="text-xs text-ink-subtle">No {{ trace.neighborDirection() }} edges.</p>
                }
              </div>
            }
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
  protected readonly zenMode = signal(false);

  readonly nodeSelected = output<string>();
  /** Double-click anywhere on the canvas — parent re-traces from this node (proposal §2). */
  readonly retrace = output<string>();
  /** System altitude project click — parent filters the Entry Deck to it. */
  readonly projectSelected = output<string>();
  /** L6.5: Visible Table lens button clicked. */
  readonly tableRequested = output<void>();

  /** `model()` so the Workbench can lift it into `?view` URL state (proposal §8.3). */
  readonly altitude = model<StageAltitude>('flow');
  /** `model()` so the Workbench's `v t`/`v g` shortcuts (§8.4) can drive it directly. */
  readonly flowMode = model<FlowMode>('tree');

  /** M7.2: Lens model — lifted to WorkbenchPage so each page owns its default.
   *  Service/flow are live; layer/feature are structural slots (engine data pending). */
  readonly lensModel = model<LensId>('flow');

  protected readonly graphDepth = signal(3);
  protected readonly nodeViewMode = signal<NodeViewMode>('list');
  protected readonly directions = DIRECTIONS;

  /** §3.5 Confidence Ledger. Tree-mode only — graph mode always shows the raw tree,
   * scope decision (the canvas has no per-node badge system to filter against today). */
  protected readonly approxOnly = signal(false);
  protected readonly displayedTree = computed<TraceNodeVm | null>(() => {
    const tree = this.trace.tree();
    if (!tree) return null;
    return this.approxOnly() ? filterApproxTree(tree) : tree;
  });

  /** Walks the currently loaded (unfiltered) tree — independent of AtlasStore's indexer,
   * so this works instantly for ANY trace, not just ones the background indexer reached. */
  protected readonly verifiedPct = computed<number | null>(() => {
    const tree = this.trace.tree();
    if (!tree) return null;
    let total = 0;
    let verified = 0;
    const stack: TraceNodeVm[] = [tree];
    while (stack.length > 0) {
      const node = stack.pop()!;
      total++;
      if (node.resolution === 'Semantic') verified++;
      for (const child of node.children) stack.push(child);
    }
    return total > 0 ? Math.round((verified / total) * 100) : null;
  });

  protected readonly altitudes: readonly { id: StageAltitude; label: string; hint: string }[] = [
    { id: 'system', label: 'System', hint: 'Project topology (v s)' },
    { id: 'flow', label: 'Flow', hint: 'Current trace (v t / v g)' },
    { id: 'node', label: 'Node', hint: 'Selected node neighborhood (v n)' },
  ];

  /** M7.1: Highlight the selected node in graph views with an accent ring. */
  protected readonly highlightedNodeId = computed(() => this.trace.selectedNodeId());

  protected readonly topology = computed(() => this.session.mapResponse()?.topology ?? []);

  /** D4.2: System altitude carries the ServiceMap facet so the canvas can render C4
   * level 1 (services + transport-labeled edges) and expand services in place. */
  protected readonly topologyData = computed<GraphCanvasData>(() => ({
    mode: 'topology',
    projects: this.topology(),
    services: this.session.graphFacets()?.serviceMap?.services ?? [],
    transports: this.session.graphFacets()?.serviceMap?.transports ?? [],
  }));

  protected onGraphDepth(event: Event): void {
    this.graphDepth.set(Number((event.target as HTMLSelectElement).value));
  }

  protected onDirection(direction: NeighborDirection): void {
    const nodeId = this.trace.selectedNodeId();
    if (nodeId) void this.trace.selectNode(nodeId, direction);
  }

  /** System altitude: '' means "tapped empty canvas" (GraphCanvas's deselect signal) — ignored. */
  protected onProjectTap(name: string): void {
    if (name) this.projectSelected.emit(name);
  }

  protected onFlowTap(nodeId: string): void {
    if (nodeId) this.nodeSelected.emit(nodeId);
  }

  protected onNodeTap(nodeId: string): void {
    if (nodeId) this.nodeSelected.emit(nodeId);
  }

  constructor() {
    const onKey = (event: KeyboardEvent): void => {
      if (event.key === 'F' && !event.ctrlKey && !event.metaKey && !event.altKey) {
        const tag = (event.target as HTMLElement | null)?.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA') return;
        event.preventDefault();
        this.zenMode.update((z) => !z);
      }
    };
    // Window-level F key for zen mode toggle
    window.addEventListener('keydown', onKey);
    inject(DestroyRef).onDestroy(() => window.removeEventListener('keydown', onKey));

    // M7.2: Lens → altitude derivation. Service/layer/feature → system, flow → flow.
    effect(() => {
      const lens = this.lensModel();
      if (lens === 'flow') {
        this.altitude.set('flow');
      } else {
        this.altitude.set('system');
      }
    });
  }

  protected meterVariant(pct: number): MeterVariant {
    if (pct >= 80) return 'success';
    if (pct >= 50) return 'accent';
    return 'warn';
  }
}
