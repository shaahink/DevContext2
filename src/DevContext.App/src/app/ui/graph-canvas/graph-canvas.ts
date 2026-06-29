import {
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  input,
  output,
  viewChild,
} from '@angular/core';
import cytoscape from 'cytoscape';
import dagre from 'cytoscape-dagre';

import type { TraceNodeVm } from '../../models/view-models';
import { ThemeService } from '../../core/theme/theme.service';

cytoscape.use(dagre);

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
  Entry: '→ entry',
  Send: '⇢ send',
  Handle: '⌲ handle',
  Raise: '↑ raise',
  Consume: '↓ consume',
  Data: '□ data',
  Resolve: '∘ resolve',
  Pipeline: '‖ pipe',
  Call: '→ call',
};

function buildElements(root: TraceNodeVm): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [];
  let counter = 0;

  const walk = (node: TraceNodeVm, parentElId: string | null): void => {
    const elId = `n${counter++}`;
    els.push({
      data: {
        id: elId,
        nodeId: node.id,
        label: node.title,
        seam: node.seam,
        truncated: node.truncated,
        depth: node.depth,
      },
      classes: `seam-${node.seam.toLowerCase()}`,
    } as cytoscape.ElementDefinition);
    if (parentElId !== null) {
      els.push({
        data: {
          id: `${parentElId}->${elId}`,
          source: parentElId,
          target: elId,
          seam: node.seam,
        },
      });
    }
    for (const child of node.children) walk(child, elId);
  };

  walk(root, null);
  return els;
}

@Component({
  selector: 'app-graph-canvas',
  template: `
    <div #cy class="h-full w-full"></div>
    <div class="pointer-events-none absolute bottom-3 left-3 z-10 rounded border border-line bg-surface/90 px-3 py-2 text-[10px] backdrop-blur">
      <div class="mb-1 text-ink-subtle font-semibold uppercase">Legend</div>
      <div class="grid grid-cols-2 gap-x-4 gap-y-1">
        @for (item of legendItems; track item.label) {
          <div class="flex items-center gap-1.5">
            <span class="h-2 w-2 rounded-sm" [style.background-color]="item.color"></span>
            <span class="text-ink-muted">{{ item.label }}</span>
          </div>
        }
      </div>
    </div>
  `,
  host: { class: 'block h-full w-full relative' },
})
export class GraphCanvas {
  readonly trace = input<TraceNodeVm | null>(null);
  readonly selectedNodeId = input<string | null>(null);
  readonly highlightedNodeId = input<string | null>(null);
  readonly nodeSelected = output<string>();

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

    effect(() => this.highlight(this.highlightedNodeId()));
    effect(() => {
      const nid = this.selectedNodeId();
      if (nid !== undefined) this.highlightSelected(nid);
    });
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
    const traceNode = this.trace();
    if (!host || !traceNode) {
      this.cy?.destroy();
      this.cy = null;
      return;
    }
    this.render(host, traceNode);
  }

  private render(host: HTMLElement, traceNode: TraceNodeVm): void {
    this.cy?.destroy();
    this.cy = null;

    const p = this.theme.palette();
    const colors = this.seamColors;

    this.cy = cytoscape({
      container: host,
      elements: buildElements(traceNode),
      wheelSensitivity: 0.3,
      style: [
        {
          selector: 'node',
          style: {
            'background-color': p.surface2,
            'border-width': 2,
            'border-color': (ele: cytoscape.NodeSingular) =>
              colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted,
            label: 'data(label)',
            color: p.ink,
            'font-size': 11,
            'font-family': 'Cascadia Code, JetBrains Mono, Consolas, monospace',
            'text-valign': 'center',
            'text-halign': 'right',
            'text-margin-x': 8,
            'text-wrap': 'wrap',
            'text-max-width': '240px',
            width: 16,
            height: 16,
            shape: 'round-rectangle',
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
            width: 20,
            height: 20,
          },
        },
        {
          selector: 'node.highlighted',
          style: {
            'background-color': p.accent,
            'border-color': p.accent,
            'border-width': 3,
            width: 22,
            height: 22,
          },
        },
        {
          selector: 'edge',
          style: {
            width: 1.5,
            'line-color': (ele: cytoscape.EdgeSingular) =>
              colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted,
            'target-arrow-color': (ele: cytoscape.EdgeSingular) =>
              colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted,
            'target-arrow-shape': 'triangle',
            'arrow-scale': 0.9,
            'curve-style': 'taxi',
            'taxi-direction': 'rightward',
            label: 'data(seam)',
            'font-size': 7,
            color: p.inkSubtle,
            'text-background-color': p.base,
            'text-background-opacity': 0.85,
            'text-background-padding': '2',
          },
        },
        {
          selector: 'edge.highlighted',
          style: {
            width: 2.5,
            'line-color': p.accent,
            'target-arrow-color': p.accent,
          },
        },
      ],
      layout: {
        name: 'dagre',
        rankDir: 'LR',
        nodeSep: 40,
        rankSep: 120,
        padding: 32,
        animate: false,
      } as cytoscape.LayoutOptions,
    });

    this.cy.on('tap', 'node', (e) => this.nodeSelected.emit(e.target.data('nodeId') as string));
    this.cy.on('tap', (_evt) => {
      if (_evt.target === this.cy) this.nodeSelected.emit('');
    });

    this.cy.ready(() => {
      this.cy?.fit(undefined, 48);
      this.highlightSelected(this.selectedNodeId());
    });
  }

  private highlight(nodeId: string | null): void {
    if (!this.cy) return;
    this.cy.nodes().removeClass('highlighted');
    this.cy.edges().removeClass('highlighted');
    if (!nodeId) return;
    const target = this.cy.nodes(`[nodeId = "${nodeId}"]`);
    target.addClass('highlighted');
    target.connectedEdges().addClass('highlighted');
  }

  private highlightSelected(nodeId: string | null): void {
    if (!this.cy) return;
    this.cy.nodes().removeClass('selected');
    if (nodeId) this.cy.nodes(`[nodeId = "${nodeId}"]`).addClass('selected');
  }
}
