import { Component, DestroyRef, effect, ElementRef, inject, input, output, signal, viewChild } from '@angular/core';
import cytoscape from 'cytoscape';
import dagre from 'cytoscape-dagre';

import type { ProjectNode } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import type { EdgeVm, TraceNodeVm } from '../../models/view-models';
import { ThemeService } from '../../core/theme/theme.service';

cytoscape.use(dagre);

/**
 * What the Stage renders on one canvas across its three altitudes (proposal §2, §8.1
 * "graph-canvas upgrades"): a trace tree, a project topology, or a node's one-hop
 * neighborhood. One component, one cytoscape instance, three element builders — the
 * graph is never blank because System topology exists the moment analysis completes.
 */
export type GraphCanvasData =
  | { readonly mode: 'trace'; readonly root: TraceNodeVm; readonly maxDepth: number }
  | { readonly mode: 'topology'; readonly projects: readonly ProjectNode[] }
  | { readonly mode: 'neighbors'; readonly centerId: string; readonly centerTitle: string; readonly edges: readonly EdgeVm[] };

interface SeamColors {
  Entry: string;
  Send: string;
  Handle: string;
  Raise: string;
  Consume: string;
  Data: string;
  Resolve: string;
  Pipeline: string;
  Call: string;
}

const SEAM_LABELS: Record<string, string> = {
  Entry: 'entry', Send: 'send', Handle: 'handle', Raise: 'raise',
  Consume: 'consume', Data: 'data', Resolve: 'resolve', Pipeline: 'pipe', Call: 'call',
};

function truncateLabel(label: string): string {
  return label.length > 40 ? label.slice(0, 38) + '…' : label;
}

function buildTraceElements(root: TraceNodeVm, maxDepth: number): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [];
  let counter = 0;

  const walk = (node: TraceNodeVm, parentElId: string | null, depth: number): void => {
    if (depth > maxDepth) return;
    const elId = `n${counter++}`;
    els.push({
      data: {
        id: elId,
        nodeId: node.id,
        label: truncateLabel(node.title),
        fullLabel: node.title,
        seam: node.seam,
        truncated: node.truncated,
        depth: node.depth,
      },
      classes: depth === 0 ? 'entry' : '',
    } as cytoscape.ElementDefinition);
    if (parentElId !== null) {
      els.push({ data: { id: `${parentElId}->${elId}`, source: parentElId, target: elId, seam: node.seam } });
    }
    for (const child of node.children) walk(child, elId, depth + 1);
  };

  walk(root, null, 0);
  return els;
}

/** System altitude: one node per project, edges from `dependsOn` (proposal §2 — "available
 * the moment analysis completes, before any trace"). Dangling deps (no matching project,
 * e.g. an external package) are dropped rather than crashing cytoscape on a missing target. */
function buildTopologyElements(projects: readonly ProjectNode[]): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [];
  const names = new Set(projects.map((p) => p.name));
  for (const p of projects) {
    els.push({ data: { id: p.name, nodeId: p.name, label: truncateLabel(p.name), fullLabel: p.name, seam: '', truncated: false, depth: 0 } });
  }
  for (const p of projects) {
    for (const dep of p.dependsOn) {
      if (!names.has(dep)) continue;
      els.push({ data: { id: `${p.name}->${dep}`, source: p.name, target: dep, seam: '' } });
    }
  }
  return els;
}

/** Node altitude: the selected node plus its one-hop neighborhood from GetNeighbors.
 * `edge.to`/`edge.otherTitle` already mean "the other side" regardless of query
 * direction (out/in/usages) — see TraceStore.selectNode / Stage's existing list view. */
function buildNeighborsElements(
  centerId: string,
  centerTitle: string,
  edges: readonly EdgeVm[],
): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [
    {
      data: { id: centerId, nodeId: centerId, label: truncateLabel(centerTitle), fullLabel: centerTitle, seam: '', truncated: false, depth: 0 },
      classes: 'entry',
    },
  ];
  const seen = new Set([centerId]);
  let counter = 0;
  for (const e of edges) {
    const otherTitle = e.otherTitle || e.to;
    if (!seen.has(e.to)) {
      seen.add(e.to);
      els.push({ data: { id: e.to, nodeId: e.to, label: truncateLabel(otherTitle), fullLabel: otherTitle, seam: e.kind, truncated: false, depth: 1 } });
    }
    els.push({ data: { id: `edge${counter++}`, source: centerId, target: e.to, seam: e.kind } });
  }
  return els;
}

function buildElements(data: GraphCanvasData): cytoscape.ElementDefinition[] {
  switch (data.mode) {
    case 'trace':
      return buildTraceElements(data.root, data.maxDepth);
    case 'topology':
      return buildTopologyElements(data.projects);
    case 'neighbors':
      return buildNeighborsElements(data.centerId, data.centerTitle, data.edges);
  }
}

@Component({
  selector: 'app-graph-canvas',
  template: `
    <div class="relative h-full w-full">
      <div #cy class="h-full w-full"></div>

      <!-- Legend popover -->
      <button
        class="pointer-events-auto absolute bottom-3 left-3 z-10 chip text-[10px]"
        (click)="legendOpen.set(!legendOpen())"
        title="Legend"
      >Legend</button>
      @if (legendOpen()) {
        <div class="pointer-events-none absolute bottom-9 left-3 z-10 rounded border border-line bg-surface/95 px-3 py-2 text-[10px] backdrop-blur shadow-overlay">
          <div class="mb-1 font-semibold uppercase text-ink-subtle">Legend</div>
          <div class="grid grid-cols-3 gap-x-4 gap-y-1">
            @for (item of legendItems; track item.label) {
              <div class="flex items-center gap-1.5">
                <span class="h-2 w-2 rounded-sm" [style.background-color]="item.color"></span>
                <span class="text-ink-muted">{{ item.label }}</span>
              </div>
            }
          </div>
        </div>
      }

      <div class="pointer-events-auto absolute right-2 top-2 z-10 flex items-center gap-1 rounded border border-line bg-surface/90 px-1.5 py-1 backdrop-blur text-2xs">
        <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="zoomIn()" title="Zoom in">+</button>
        <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="zoomOut()" title="Zoom out">−</button>
        <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="fitGraph()" title="Fit">⊡</button>
      </div>
    </div>
  `,
  host: { class: 'block h-[500px] w-full relative border border-line bg-surface overflow-hidden' },
})
export class GraphCanvas {
  readonly data = input.required<GraphCanvasData>();
  readonly nodeSelected = output<string>();
  readonly nodeActivated = output<string>();

  protected readonly legendOpen = signal(false);

  private readonly container = viewChild<ElementRef<HTMLDivElement>>('cy');
  private readonly theme = inject(ThemeService);
  private cy: cytoscape.Core | null = null;

  private seamColors: SeamColors = {
    Entry: '#4493f8', Send: '#a371f7', Handle: '#3fb950', Raise: '#d29922',
    Consume: '#d29922', Data: '#39c5cf', Resolve: '#6b7480', Pipeline: '#a371f7', Call: '#8b949e',
  };

  readonly legendItems: { label: string; color: string }[] = [];

  constructor() {
    inject(DestroyRef).onDestroy(() => this.cy?.destroy());

    effect(() => {
      const p = this.theme.palette();
      this.seamColors = {
        Entry: p.accent, Send: '#a371f7', Handle: p.success, Raise: p.warn,
        Consume: p.warn, Data: '#39c5cf', Resolve: p.inkSubtle, Pipeline: '#a371f7', Call: p.inkMuted,
      };
      this.updateLegend();
      this.rebuild();
    });

    effect(() => void this.rebuild());
  }

  private updateLegend(): void {
    const items: { label: string; color: string }[] = [];
    for (const [key, color] of Object.entries(this.seamColors)) {
      if (SEAM_LABELS[key]) items.push({ label: SEAM_LABELS[key], color });
    }
    (this.legendItems as { label: string; color: string }[]).splice(0, this.legendItems.length, ...items);
  }

  private rebuild(): void {
    const host = this.container()?.nativeElement;
    const data = this.data();
    if (!host || !data) {
      this.cy?.destroy();
      this.cy = null;
      return;
    }
    this.render(host, data);
  }

  private render(host: HTMLElement, data: GraphCanvasData): void {
    this.cy?.destroy();
    this.cy = null;

    const p = this.theme.palette();
    const colors = this.seamColors;

    this.cy = cytoscape({
      container: host,
      elements: buildElements(data),
      wheelSensitivity: 0.3,
      style: [
        {
          selector: 'node',
          style: {
            'background-color': p.surface2,
            'border-width': 1.5,
            'border-color': (ele: cytoscape.NodeSingular) =>
              colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted,
            label: 'data(label)',
            color: p.ink,
            'font-size': 10,
            'font-family': 'Cascadia Code, JetBrains Mono, Consolas, monospace',
            'text-valign': 'center',
            'text-halign': 'right',
            'text-margin-x': 10,
            'text-wrap': 'wrap',
            'text-max-width': '200px',
            width: 14,
            height: 14,
            shape: 'round-rectangle',
          },
        },
        {
          selector: 'node.entry',
          style: {
            width: 22,
            height: 22,
            'border-width': 2.5,
            'border-color': p.accent,
            'font-weight': 'bold',
            'font-size': 11,
          },
        },
        {
          selector: 'node[?truncated]',
          style: { 'border-style': 'dashed', 'border-opacity': 0.5 },
        },
        {
          selector: 'node.selected',
          style: {
            'background-color': p.accent,
            'border-color': p.accent,
            width: 18,
            height: 18,
          },
        },
        {
          selector: 'node.highlighted',
          style: {
            'background-color': p.accent,
            'border-color': p.accent,
            'border-width': 3,
            width: 20,
            height: 20,
          },
        },
        {
          selector: 'edge',
          style: {
            width: 1.2,
            'line-color': (ele: cytoscape.EdgeSingular) =>
              colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted,
            'target-arrow-color': (ele: cytoscape.EdgeSingular) =>
              colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted,
            'target-arrow-shape': 'triangle',
            'arrow-scale': 0.7,
            'curve-style': 'bezier',
            label: '',
          },
        },
        {
          selector: 'edge.highlighted',
          style: {
            width: 2,
            'line-color': p.accent,
            'target-arrow-color': p.accent,
          },
        },
        {
          selector: '.dimmed',
          style: {
            opacity: 0.15,
            'text-opacity': 0.15,
          },
        },
      ],
      layout: {
        name: 'dagre',
        rankDir: 'LR',
        nodeSep: 60,
        rankSep: 140,
        padding: 40,
        animate: false,
      } as cytoscape.LayoutOptions,
    });

    this.cy.on('tap', 'node', (e) => this.nodeSelected.emit(e.target.data('nodeId') as string));
    this.cy.on('dbltap', 'node', (e) => this.nodeActivated.emit(e.target.data('nodeId') as string));
    this.cy.on('tap', (_evt) => {
      if (_evt.target === this.cy) this.nodeSelected.emit('');
    });

    // Focus dimming: hover a node -> dim non-neighbors
    this.cy.on('mouseover', 'node', (e) => {
      const node = e.target;
      const neighbors = node.neighborhood();
      this.cy?.elements().removeClass('dimmed');
      this.cy?.elements().not(neighbors).not(node).addClass('dimmed');
    });
    this.cy.on('mouseout', 'node', () => {
      this.cy?.elements().removeClass('dimmed');
    });

    this.cy.ready(() => {
      this.cy?.fit(undefined, 50);
    });
  }

  protected zoomIn(): void {
    this.cy?.zoom(this.cy.zoom() * 1.2);
  }

  protected zoomOut(): void {
    this.cy?.zoom(this.cy.zoom() / 1.2);
  }

  protected fitGraph(): void {
    this.cy?.fit(undefined, 50);
  }
}
