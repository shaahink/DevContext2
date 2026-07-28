import { Component, computed, DestroyRef, effect, ElementRef, inject, input, output, signal, viewChild } from '@angular/core';
import cytoscape from 'cytoscape';

import type { ProjectNode, ServiceCard, TransportLink } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import type { EdgeVm, TraceNodeVm } from '../../models/view-models';
import type { LensId } from '../../features/explorer/lens-switcher';
import { ThemeService } from '../../core/theme/theme.service';
import { layoutGraph, nodeWidthForLabel, NODE_HEIGHT, type LayoutNodeIn } from './graph-layout';
import { classifyTransport, serviceLabel, type TransportClass } from './semantics';

/** Minimap only earns its screen space in zen mode, and only once a graph is
 * big enough that the viewport can't already see everything at a glance. */
const MINIMAP_NODE_THRESHOLD = 40;

/** Fit must never balloon a small graph: a 4-node library at zoom 3 reads as a bug
 * (baseline refit hero). Compact heroes sit inside prose, so they clamp harder. */
const MAX_FIT_ZOOM_COMPACT = 1.0;
const MAX_FIT_ZOOM = 1.25;

const LAYER_COLORS: Record<string, string> = {
  'Api': '#4493f8',
  'Application': '#a371f7',
  'Domain': '#3fb950',
  'Infrastructure': '#d29922',
  'Persistence': '#d29922',
  'Contracts': '#39c5cf',
  'Presentation': '#4493f8',
  'Shared': '#8b949e',
  'Core': '#a371f7',
  'Testing': '#f85149',
};

const FEATURE_PALETTE = ['#4493f8', '#3fb950', '#d29922', '#f85149', '#a371f7', '#39c5cf', '#f778ba', '#ffa657', '#79c0ff', '#7ee787', '#d2a8ff', '#ff7b72'];

function hashString(str: string): number {
  let hash = 0;
  for (let i = 0; i < str.length; i++) hash = ((hash << 5) - hash + str.charCodeAt(i)) | 0;
  return Math.abs(hash);
}

/**
 * What the Stage renders on one canvas across its three altitudes (proposal §2, §8.1
 * "graph-canvas upgrades"): a trace tree, a project topology, or a node's one-hop
 * neighborhood. One component, one cytoscape instance, three element builders — the
 * graph is never blank because System topology exists the moment analysis completes.
 *
 * D4.1: cytoscape is the RENDERER only — geometry comes from the deterministic ELK
 * layered layout in graph-layout.ts (positions applied as a preset). Labels live INSIDE
 * fixed-width boxes the layout engine knows about, so labels cannot overlap or clip by
 * construction; a ResizeObserver re-fits on container resize.
 *
 * D4.2 (F3/M): topology mode carries the ServiceMap facet. When services exist the
 * canvas defaults to C4-ish LEVEL 1 — service boxes (kind glyph + [db] store mark) wired
 * by transport-labeled edges (HTTP/queue/gRPC/event; unknown endpoints render as dashed
 * externals) — and a tap EXPANDS a service in place into a compound holding its project
 * + direct dependencies. The all-projects view groups into DDD-layer lanes (compound per
 * layer) when the engine put ≥2 layers on the topology. A chip toggles the two levels.
 */
export type GraphCanvasData =
  | { readonly mode: 'trace'; readonly root: TraceNodeVm; readonly maxDepth: number }
  | {
      readonly mode: 'topology';
      readonly projects: readonly ProjectNode[];
      readonly services?: readonly ServiceCard[];
      readonly transports?: readonly TransportLink[];
    }
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

/** Entry-kind glyphs for trace roots (T6.2) — terminal-style text tags, mirroring
 * KIND_LABELS' vocabulary in canvas-sized form. */
const KIND_GLYPHS: Record<string, string> = {
  HttpEndpoint: 'HTTP', SignalRHub: 'HUB', HostedService: 'WORKER', MessageConsumer: 'BUS',
  DomainEventHandler: 'EVENT', GrpcService: 'RPC', CliCommand: 'CLI', UiEntry: 'UI',
  FunctionEntry: 'FN', GrainMethod: 'GRAIN', PublicApi: 'API',
};

function buildTraceElements(root: TraceNodeVm, maxDepth: number): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [];
  let counter = 0;

  const walk = (node: TraceNodeVm, parentElId: string | null, depth: number): void => {
    if (depth > maxDepth) return;
    const elId = `n${counter++}`;
    const glyph = depth === 0 ? KIND_GLYPHS[node.kind] : undefined;
    els.push({
      data: {
        id: elId,
        nodeId: node.id,
        label: (glyph ? `[${glyph}] ` : '') + truncateLabel(node.title),
        fullLabel: node.title,
        seam: node.seam,
        truncated: node.truncated,
        depth: node.depth,
      },
      classes: depth === 0 ? 'entry' : '',
    } as cytoscape.ElementDefinition);
    if (parentElId !== null) {
      // Resolution tier drawn into the picture (T6.2): a semantic (Roslyn-verified) hop is a
      // solid line, an approximate one dashed — honesty visible without opening the inspector.
      els.push({ data: { id: `${parentElId}->${elId}`, source: parentElId, target: elId, seam: node.seam,
        approx: node.resolution !== 'Semantic' } });
    }
    for (const child of node.children) walk(child, elId, depth + 1);
  };

  walk(root, null, 0);
  return els;
}

/** All-projects altitude: one node per project, edges from `dependsOn`. D4.2: when the
 * engine layered ≥2 of the projects, each layer becomes a labeled compound LANE and the
 * layout orders lanes by dependency flow (Api → Application → Domain reads left-to-right). */
function buildTopologyElements(projects: readonly ProjectNode[]): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [];
  const names = new Set(projects.map((p) => p.name));
  const layers = new Set(projects.map((p) => p.layer).filter((l): l is string => !!l));
  const lanesActive = layers.size >= 2;
  if (lanesActive) {
    for (const layer of [...layers].sort()) {
      els.push({ data: { id: `lane:${layer}`, nodeId: '', label: layer, fullLabel: layer, seam: '', lane: true }, classes: 'lane' });
    }
  }
  for (const p of projects) {
    const parent = lanesActive && p.layer ? `lane:${p.layer}` : undefined;
    els.push({ data: { id: p.name, nodeId: p.name, label: truncateLabel(p.name), fullLabel: p.name, seam: '', truncated: false, depth: 0, layer: p.layer ?? '', feature: p.feature ?? '', parent } });
  }
  for (const p of projects) {
    for (const dep of p.dependsOn) {
      if (!names.has(dep)) continue;
      els.push({ data: { id: `${p.name}->${dep}`, source: p.name, target: dep, seam: '' } });
    }
  }
  return els;
}

/** C4-ish level 1 (D4.2/M): service boxes + transport-labeled edges — what the system IS,
 * not what the csproj graph happens to reference. Endpoints resolve service → project →
 * dashed external (never dropped: a transport into the unknown is a finding, not noise).
 * An expanded service becomes a compound holding its project + direct dependencies. */
function buildServiceLevelElements(
  projects: readonly ProjectNode[],
  services: readonly ServiceCard[],
  transports: readonly TransportLink[],
  expanded: ReadonlySet<string>,
): cytoscape.ElementDefinition[] {
  const els: cytoscape.ElementDefinition[] = [];
  const projByName = new Map(projects.map((p) => [p.name, p]));
  const placed = new Set<string>();

  for (const s of services) {
    const name = s.displayName;
    if (placed.has(name)) continue;
    placed.add(name);
    const proj = projByName.get(name);
    const expandable = !!proj && proj.dependsOn.length > 0;
    const isExpanded = expandable && expanded.has(name);
    els.push({
      data: {
        id: name, nodeId: name, label: serviceLabel(name, s.kind, s.stack, truncateLabel), fullLabel: name,
        seam: '', truncated: false, depth: 0, layer: s.layer ?? '', feature: s.feature ?? '', svc: true, expandable,
      },
      classes: 'svc',
    });
    if (isExpanded && proj) {
      const selfId = `${name}::self`;
      els.push({ data: { id: selfId, nodeId: name, label: truncateLabel(name), fullLabel: name, seam: '', depth: 1, layer: proj.layer ?? '', feature: '', parent: name } });
      for (const dep of proj.dependsOn) {
        const depProj = projByName.get(dep);
        if (!depProj) continue;
        const depId = `${name}::${dep}`;
        els.push({ data: { id: depId, nodeId: dep, label: truncateLabel(dep), fullLabel: dep, seam: '', depth: 1, layer: depProj.layer ?? '', feature: '', parent: name } });
        els.push({ data: { id: `${selfId}->${depId}`, source: selfId, target: depId, seam: '' } });
      }
    }
  }

  const seen = new Set<string>();
  let i = 0;
  for (const t of transports) {
    const key = `${t.fromService}|${t.toService}|${t.transport}`;
    if (seen.has(key) || t.fromService === t.toService) continue;
    seen.add(key);
    for (const ep of [t.fromService, t.toService]) {
      if (placed.has(ep)) continue;
      placed.add(ep);
      const proj = projByName.get(ep);
      els.push({
        data: { id: ep, nodeId: ep, label: truncateLabel(ep), fullLabel: ep, seam: '', truncated: false, depth: 0, layer: proj?.layer ?? '', feature: '' },
        classes: proj ? '' : 'external',
      });
    }
    const vis = classifyTransport(t.transport);
    els.push({ data: { id: `t${i++}:${key}`, source: t.fromService, target: t.toService, seam: '', tlabel: vis.label, tclass: vis.cls } });
  }
  return els;
}

/** Node altitude: the selected node plus its one-hop neighborhood from GetNeighbors.
 * Edges keep their TRUE direction (`from`/`to` — node-card's Called-by/Calls split relies
 * on the same fields), so the layered layout naturally seats callers LEFT of the center
 * and callees RIGHT: the neighborhood reads as a flow instead of a star. */
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
    const other = e.from === centerId ? e.to : (e.from || e.to);
    const otherTitle = e.otherTitle || other;
    if (!seen.has(other)) {
      seen.add(other);
      els.push({ data: { id: other, nodeId: other, label: truncateLabel(otherTitle), fullLabel: otherTitle, seam: e.kind, truncated: false, depth: 1 } });
    }
    const source = e.from === centerId ? centerId : other;
    const target = source === centerId ? other : centerId;
    els.push({ data: { id: `edge${counter++}`, source, target, seam: e.kind,
      approx: e.resolution !== 'Semantic' } });
  }
  return els;
}

/** Degree centrality (in+out edge count) per node, written back onto each node's data.
 * D4.1: degree no longer sizes nodes (box width belongs to the label so the layout knows
 * the true footprint) — it drives border emphasis instead, so hubs still pop. */
function annotateDegree(els: cytoscape.ElementDefinition[]): void {
  const degree = new Map<string, number>();
  for (const el of els) {
    const d = el.data as { source?: string; target?: string };
    if (d.source === undefined || d.target === undefined) continue;
    degree.set(d.source, (degree.get(d.source) ?? 0) + 1);
    degree.set(d.target, (degree.get(d.target) ?? 0) + 1);
  }
  for (const el of els) {
    const d = el.data as { id?: string; source?: string };
    if (d.source !== undefined) continue; // edge, not a node
    (el.data as { degree?: number }).degree = degree.get(d.id ?? '') ?? 0;
  }
}

/** Hubs pop through the border, not the box: sqrt scale, clamped so leaf nodes stay quiet. */
function borderWidthForDegree(degree: number): number {
  return Math.min(3, 1.25 + Math.sqrt(Math.max(0, degree)) * 0.3);
}

@Component({
  selector: 'app-graph-canvas',
  template: `
    <div class="relative h-full w-full">
      <div #cy class="h-full w-full"></div>

      <!-- D4.2: C4 level toggle — only when the ServiceMap facet gave us a services level -->
      @if (levelChipsVisible()) {
        <div class="pointer-events-auto absolute left-3 top-2 z-10 flex items-center gap-0.5 rounded border border-line bg-surface/90 p-0.5 text-2xs backdrop-blur">
          <button
            class="rounded px-1.5 py-0.5 transition-colors"
            [class.bg-surface-2]="effectiveLevel() === 'services'"
            [class.text-ink]="effectiveLevel() === 'services'"
            [class.text-ink-subtle]="effectiveLevel() !== 'services'"
            (click)="setLevel('services')"
            title="C4 level 1 — services + transports; tap a service to expand it"
          >Services</button>
          <button
            class="rounded px-1.5 py-0.5 transition-colors"
            [class.bg-surface-2]="effectiveLevel() === 'projects'"
            [class.text-ink]="effectiveLevel() === 'projects'"
            [class.text-ink-subtle]="effectiveLevel() !== 'projects'"
            (click)="setLevel('projects')"
            title="Every project, grouped into layer lanes when the engine layered them"
          >All projects</button>
        </div>
      }

      <!-- Legend popover -->
      @if (!compact()) {
        <button
          class="pointer-events-auto absolute bottom-3 left-3 z-10 chip text-2xs"
          (click)="legendOpen.set(!legendOpen())"
          title="Legend"
        >Legend</button>
      }
      @if (legendOpen()) {
        <div class="pointer-events-none absolute bottom-9 left-3 z-10 rounded border border-line bg-surface/95 px-3 py-2 text-2xs backdrop-blur shadow-overlay">
          <div class="mb-1 font-semibold uppercase text-ink-subtle">Legend</div>
          <div class="grid grid-cols-3 gap-x-4 gap-y-1">
            @for (item of legendItems(); track item.label) {
              <div class="flex items-center gap-1.5">
                <span class="h-2 w-2 rounded-sm" [style.background-color]="item.color"></span>
                <span class="text-ink-muted">{{ item.label }}</span>
              </div>
            }
          </div>
        </div>
      }

      @if (!compact()) {
        <div class="pointer-events-auto absolute right-2 top-2 z-10 flex items-center gap-1 rounded border border-line bg-surface/90 px-1.5 py-1 backdrop-blur text-2xs">
          <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="zoomIn()" title="Zoom in">+</button>
          <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="zoomOut()" title="Zoom out">−</button>
          <button class="rounded p-1 text-ink-muted hover:bg-surface-2 hover:text-ink" (click)="fitGraph()" title="Fit">⊡</button>
        </div>
      }

      <!-- Minimap: zen mode only, and only once the graph is big enough to need one -->
      @if (!compact() && zenMode() && nodeCount() > minimapThreshold) {
        <canvas
          #minimap
          width="160" height="110"
          class="pointer-events-auto absolute bottom-3 right-3 z-10 cursor-pointer rounded border border-line bg-surface/90 backdrop-blur"
          title="Minimap — click to jump"
          (click)="onMinimapClick($event)"
        ></canvas>
      }
    </div>
  `,
  host: {
    class: 'block w-full relative border border-line bg-surface overflow-hidden',
    // `fill` wins over the fixed heights: embedded on a scrolling page (Home hero, Atlas)
    // the canvas must claim a definite height, but as the Stage's landing surface (R3 D-A)
    // a fixed 500px leaves dead space below the graph on a tall viewport.
    '[style.height]': 'fill() ? "100%" : null',
    '[style.height.px]': 'fill() ? null : compact() ? 280 : 500',
    '[class.rounded-lg]': 'compact()',
  },
})
export class GraphCanvas {
  readonly data = input.required<GraphCanvasData>();
  /** Minimap only renders in zen mode (Stage passes its zenMode signal through). */
  readonly zenMode = input(false);
  /** T6.7 — hero embedding: shorter, no legend/zoom controls/minimap, no user pan/zoom
   * (the page scrolls past it), taps still emitted. */
  readonly compact = input(false);
  /** R3 D-A: fill the host's pane instead of the fixed embed height. Set by Stage, whose
   * canvas IS the pane — everywhere else the canvas sits in a scrolling page and needs a
   * definite height of its own. */
  readonly fill = input(false);
  /** Node ID to highlight (accent ring + pulse). Cleared on null/empty. */
  readonly highlightedNodeId = input<string | null>(null);
  /** M7.2/M9: Lens ID for layer/feature-based coloring on topology nodes. */
  readonly lensId = input<LensId>('service');
  readonly nodeSelected = output<string>();
  readonly nodeActivated = output<string>();

  protected readonly legendOpen = signal(false);
  protected readonly nodeCount = signal(0);
  protected readonly minimapThreshold = MINIMAP_NODE_THRESHOLD;

  /** D4.2 disclosure state. Level override is per-canvas; null = "services when available". */
  private readonly levelOverride = signal<'services' | 'projects' | null>(null);
  private readonly expandedServices = signal<ReadonlySet<string>>(new Set());

  /** Level 1 exists to summarize a MULTI-service system — a single-service facet (e.g. a
   * library whose ServiceMap holds its test console, refit) renders one lonely box, so
   * both the default and the toggle require ≥2 services. */
  protected readonly effectiveLevel = computed<'services' | 'projects'>(() => {
    const override = this.levelOverride();
    if (override) return override;
    const d = this.data();
    return d.mode === 'topology' && (d.services?.length ?? 0) >= 2 ? 'services' : 'projects';
  });

  protected readonly levelChipsVisible = computed(() => {
    const d = this.data();
    return d.mode === 'topology' && (d.services?.length ?? 0) >= 2;
  });

  private readonly container = viewChild<ElementRef<HTMLDivElement>>('cy');
  private readonly minimapCanvas = viewChild<ElementRef<HTMLCanvasElement>>('minimap');
  private readonly theme = inject(ThemeService);
  private cy: cytoscape.Core | null = null;
  /** Guards against a stale async layout landing after a newer render started. */
  private renderSeq = 0;
  private resizeObserver: ResizeObserver | null = null;
  private refitScheduled = false;
  /** Topology identity of the last render — a new repo resets disclosure state. */
  private lastProjectsRef: readonly ProjectNode[] | null = null;

  private seamColors: SeamColors = {
    Entry: '#4493f8', Send: '#a371f7', Handle: '#3fb950', Raise: '#d29922',
    Consume: '#d29922', Data: '#39c5cf', Resolve: '#6b7480', Pipeline: '#a371f7', Call: '#8b949e',
  };

  private transportColors: Record<TransportClass, string> = {
    HTTP: '#4493f8', queue: '#d29922', gRPC: '#a371f7', event: '#ffa657', other: '#8b949e',
  };

  readonly legendItems = signal<{ label: string; color: string }[]>([]);

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      this.resizeObserver?.disconnect();
      this.cy?.destroy();
    });

    effect(() => {
      const p = this.theme.palette();
      this.seamColors = {
        Entry: p.accent, Send: '#a371f7', Handle: p.success, Raise: p.warn,
        Consume: p.warn, Data: '#39c5cf', Resolve: p.inkSubtle, Pipeline: '#a371f7', Call: p.inkMuted,
      };
      this.transportColors = { HTTP: '#4493f8', queue: p.warn, gRPC: '#a371f7', event: '#ffa657', other: p.inkMuted };
      this.updateLegend();
    }, { allowSignalWrites: true });

    effect(() => void this.rebuild(), { allowSignalWrites: true });

    effect(() => {
      void this.lensId();
      void this.effectiveLevel();
      this.updateLegend();
      if (this.cy && this.data() && this.data()?.mode === 'topology') {
        this.cy.style().update();
      }
    }, { allowSignalWrites: true });

    // Node highlight (M7.1): accent ring on the node matching highlightedNodeId.
    effect(() => {
      const id = this.highlightedNodeId();
      const cy = this.cy;
      if (!cy) return;
      cy.nodes().removeClass('highlighted');
      if (id) {
        const node = cy.getElementById(id);
        if (node.length > 0) {
          node.addClass('highlighted');
        }
      }
    });

    // Re-fit when the container changes size — a fit computed for one geometry silently
    // clips in another (the baseline's cut-off hero nodes). rAF-debounced.
    effect(() => {
      const host = this.container()?.nativeElement;
      if (!host || this.resizeObserver) return;
      this.resizeObserver = new ResizeObserver(() => this.scheduleRefit());
      this.resizeObserver.observe(host);
    });
  }

  protected setLevel(level: 'services' | 'projects'): void {
    this.levelOverride.set(level);
  }

  private updateLegend(): void {
    const d = this.data();
    if (d?.mode === 'topology' && this.effectiveLevel() === 'services') {
      const p = this.theme.palette();
      this.legendItems.set([
        { label: 'HTTP', color: this.transportColors.HTTP },
        { label: 'queue', color: this.transportColors.queue },
        { label: 'gRPC', color: this.transportColors.gRPC },
        { label: 'event', color: this.transportColors.event },
        { label: 'external', color: p.inkSubtle },
      ]);
      return;
    }
    const lid = this.lensId();
    if (lid === 'layer') {
      const items: { label: string; color: string }[] = [];
      for (const [key, color] of Object.entries(LAYER_COLORS)) {
        items.push({ label: key, color });
      }
      this.legendItems.set(items);
    } else if (lid === 'feature') {
      this.legendItems.set([]);
    } else {
      const items: { label: string; color: string }[] = [];
      for (const [key, color] of Object.entries(this.seamColors)) {
        if (SEAM_LABELS[key]) items.push({ label: SEAM_LABELS[key], color });
      }
      this.legendItems.set(items);
    }
  }

  private rebuild(): void {
    const host = this.container()?.nativeElement;
    const data = this.data();
    // Disclosure state belongs to ONE repo's topology — reset when the projects identity flips.
    const projectsRef = data?.mode === 'topology' ? data.projects : null;
    if (projectsRef !== this.lastProjectsRef) {
      this.lastProjectsRef = projectsRef;
      if (this.expandedServices().size > 0) this.expandedServices.set(new Set());
      if (this.levelOverride() !== null) this.levelOverride.set(null);
    }
    if (!host || !data) {
      this.cy?.destroy();
      this.cy = null;
      return;
    }
    void this.render(host, data);
  }

  private buildForData(data: GraphCanvasData): cytoscape.ElementDefinition[] {
    switch (data.mode) {
      case 'trace':
        return buildTraceElements(data.root, data.maxDepth);
      case 'topology':
        return this.effectiveLevel() === 'services' && (data.services?.length ?? 0) > 0
          ? buildServiceLevelElements(data.projects, data.services ?? [], data.transports ?? [], this.expandedServices())
          : buildTopologyElements(data.projects);
      case 'neighbors':
        return buildNeighborsElements(data.centerId, data.centerTitle, data.edges);
    }
  }

  private async render(host: HTMLElement, data: GraphCanvasData): Promise<void> {
    const seq = ++this.renderSeq;
    const els = this.buildForData(data);
    annotateDegree(els);

    // Deterministic geometry first (pure, DOM-free), then hand cytoscape a preset.
    // Compound membership (data.parent) becomes one level of ELK hierarchy.
    const nodeDefs = els.filter((el) => (el.data as { source?: string }).source === undefined);
    const childrenByParent = new Map<string, cytoscape.ElementDefinition[]>();
    const topLevel: cytoscape.ElementDefinition[] = [];
    for (const el of nodeDefs) {
      const parent = (el.data as { parent?: string }).parent;
      if (parent) {
        const list = childrenByParent.get(parent) ?? [];
        list.push(el);
        childrenByParent.set(parent, list);
      } else {
        topLevel.push(el);
      }
    }
    const toLayoutNode = (el: cytoscape.ElementDefinition): LayoutNodeIn => {
      const d = el.data as { id: string; label: string };
      const kids = childrenByParent.get(d.id);
      return { id: d.id, label: d.label, children: kids?.map(toLayoutNode) };
    };
    const geometry = await layoutGraph(
      topLevel.map(toLayoutNode),
      els
        .filter((el) => (el.data as { source?: string }).source !== undefined)
        .map((el) => {
          const d = el.data as { id: string; source: string; target: string };
          return { id: d.id, source: d.source, target: d.target };
        }),
      { compact: this.compact() },
    );
    if (seq !== this.renderSeq) return; // a newer render superseded this one

    for (const el of nodeDefs) {
      const id = (el.data as { id: string }).id;
      if (childrenByParent.has(id)) continue; // compound parents: cytoscape derives their box from children
      const g = geometry.get(id);
      if (!g) continue;
      el.position = { x: g.x, y: g.y };
      (el.data as { w?: number; h?: number }).w = g.width;
      (el.data as { h?: number }).h = g.height;
    }

    this.cy?.destroy();
    this.cy = null;

    const p = this.theme.palette();
    const colors = this.seamColors;
    const transportColors = this.transportColors;
    this.nodeCount.set(nodeDefs.length);
    const lensColor = this.lensId();

    const nodeBorderColor = (ele: cytoscape.NodeSingular): string => {
      if (lensColor === 'layer') {
        const l = ele.data('layer') as string;
        return LAYER_COLORS[l] ?? p.inkMuted;
      }
      if (lensColor === 'feature') {
        const f = ele.data('feature') as string;
        if (f) return FEATURE_PALETTE[hashString(f) % FEATURE_PALETTE.length];
        return p.inkMuted;
      }
      return colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted;
    };

    const transportColor = (ele: cytoscape.EdgeSingular): string => {
      const cls = ele.data('tclass') as TransportClass | undefined;
      if (cls) return transportColors[cls] ?? p.inkMuted;
      return colors[ele.data('seam') as keyof SeamColors] ?? p.inkMuted;
    };

    this.cy = cytoscape({
      container: host,
      elements: els,
      wheelSensitivity: 0.3,
      userZoomingEnabled: !this.compact(),
      userPanningEnabled: !this.compact(),
      autoungrabify: this.compact(),
      style: [
        {
          selector: 'node',
          style: {
            'background-color': p.surface2,
            'border-width': (ele: cytoscape.NodeSingular) => borderWidthForDegree(ele.data('degree') as number),
            'border-color': nodeBorderColor,
            label: (ele: cytoscape.NodeSingular) => ele.data('label') as string,
            color: p.ink,
            'font-size': 10,
            'font-family': 'Cascadia Code, JetBrains Mono, Consolas, monospace',
            'text-valign': 'center',
            'text-halign': 'center',
            'text-wrap': 'none',
            width: (ele: cytoscape.NodeSingular) => (ele.data('w') as number) ?? nodeWidthForLabel(ele.data('label') as string),
            height: (ele: cytoscape.NodeSingular) => (ele.data('h') as number) ?? NODE_HEIGHT,
            shape: 'round-rectangle',
          },
        },
        {
          // Compound boxes (expanded service / DDD lane): translucent panel, label above the
          // border inside the vertical headroom ELK reserved for it (COMPOUND_LABEL_PAD).
          selector: 'node:parent',
          style: {
            'background-color': p.surface,
            'background-opacity': 0.4,
            'border-width': 1,
            'border-color': (ele: cytoscape.NodeSingular) => {
              const l = ele.data('layer') as string | undefined;
              if ((ele.data('lane') as boolean) && ele.data('label')) {
                return LAYER_COLORS[ele.data('label') as string] ?? p.inkSubtle;
              }
              return l ? (LAYER_COLORS[l] ?? p.inkSubtle) : p.inkSubtle;
            },
            'text-valign': 'top',
            'text-halign': 'center',
            'font-size': 9,
            'font-weight': 'bold',
            color: p.inkMuted,
            padding: '10px',
          } as unknown as cytoscape.Css.Node,
        },
        {
          selector: 'node.entry',
          style: {
            'border-width': 2.5,
            'border-color': p.accent,
            'font-weight': 'bold',
          },
        },
        {
          // D4.2: transport endpoints outside the analyzed solution — visible, honest, dashed.
          selector: 'node.external',
          style: {
            'border-style': 'dashed',
            'border-color': p.inkSubtle,
            'background-opacity': 0.5,
            color: p.inkMuted,
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
            color: p.surface,
            'border-color': p.accent,
          },
        },
        {
          selector: 'node.highlighted',
          style: {
            'background-color': p.accent,
            color: p.surface,
            'border-color': p.accent,
            'border-width': 3,
          },
        },
        {
          selector: 'edge',
          style: {
            width: 1.2,
            'line-color': transportColor,
            'target-arrow-color': transportColor,
            'target-arrow-shape': 'triangle',
            'arrow-scale': 0.7,
            'curve-style': 'bezier',
            label: '',
          },
        },
        {
          // D4.2: transport-labeled edges — the label rides the line (HTTP/queue/gRPC/event),
          // backed by surface so it stays readable where edges cross.
          selector: 'edge[tlabel]',
          style: {
            width: 1.6,
            label: (ele: cytoscape.EdgeSingular) => ele.data('tlabel') as string,
            'font-size': 8,
            'font-family': 'Cascadia Code, JetBrains Mono, Consolas, monospace',
            color: transportColor,
            'text-background-color': p.surface,
            'text-background-opacity': 0.9,
            'text-background-padding': '2px',
            'text-rotation': 'autorotate',
          },
        },
        {
          // T6.2 — approximate (non-Roslyn-verified) hops are dashed; verified stay solid.
          selector: 'edge[?approx]',
          style: { 'line-style': 'dashed', 'line-dash-pattern': [5, 3] },
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
      layout: { name: 'preset', fit: false } as cytoscape.LayoutOptions,
    });

    this.cy.on('tap', 'node', (e) => {
      const target = e.target as cytoscape.NodeSingular;
      // D4.2 progressive disclosure: tapping a service at level 1 expands/collapses it in
      // place. Selection still fires — hosts navigate on activate (dbltap), not select.
      if ((target.data('svc') as boolean) && (target.data('expandable') as boolean) && this.effectiveLevel() === 'services') {
        const next = new Set(this.expandedServices());
        const id = target.id();
        if (next.has(id)) next.delete(id); else next.add(id);
        this.expandedServices.set(next);
      }
      this.nodeSelected.emit(target.data('nodeId') as string);
    });
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

    this.cy.on('pan', () => this.drawMinimap());
    this.cy.on('zoom', () => this.drawMinimap());

    this.fitAndCenter();
    this.drawMinimap();
  }

  /** Fit with padding, then clamp: small graphs center at natural size instead of
   * ballooning; large graphs shrink until everything (boxes = full label footprint,
   * known to the layout) is inside the viewport. Preset layout is synchronous, so no
   * layoutstop dance — call sites are the initial paint and every container resize. */
  private fitAndCenter(): void {
    const cy = this.cy;
    if (!cy || cy.nodes().length === 0) return;
    cy.fit(undefined, this.compact() ? 16 : 32);
    const maxZoom = this.compact() ? MAX_FIT_ZOOM_COMPACT : MAX_FIT_ZOOM;
    if (cy.zoom() > maxZoom) {
      cy.zoom(maxZoom);
      cy.center();
    }
  }

  private scheduleRefit(): void {
    if (this.refitScheduled) return;
    this.refitScheduled = true;
    requestAnimationFrame(() => {
      this.refitScheduled = false;
      this.cy?.resize();
      this.fitAndCenter();
      this.drawMinimap();
    });
  }

  protected zoomIn(): void {
    this.cy?.zoom(this.cy.zoom() * 1.2);
  }

  protected zoomOut(): void {
    this.cy?.zoom(this.cy.zoom() / 1.2);
  }

  protected fitGraph(): void {
    this.fitAndCenter();
  }

  private minimapScheduled = false;

  /** Draws node positions + the current viewport rectangle into the minimap canvas, in the
   * graph's own model coordinates scaled to fit. No extra cytoscape instance — just the
   * positions the main layout already computed. Throttled with requestAnimationFrame so
   * rapid pan/zoom events don't redraw at 60fps. */
  private drawMinimap(): void {
    if (this.minimapScheduled) return;
    this.minimapScheduled = true;
    requestAnimationFrame(() => {
      this.minimapScheduled = false;
      this.drawMinimapFrame();
    });
  }

  private drawMinimapFrame(): void {
    const cy = this.cy;
    const canvas = this.minimapCanvas()?.nativeElement;
    if (!cy || !canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const w = canvas.width;
    const h = canvas.height;
    ctx.clearRect(0, 0, w, h);

    const bb = cy.elements().boundingBox();
    if (!isFinite(bb.w) || !isFinite(bb.h) || bb.w === 0 || bb.h === 0) return;
    const pad = 6;
    const scale = Math.min((w - pad * 2) / bb.w, (h - pad * 2) / bb.h);
    const toX = (x: number) => pad + (x - bb.x1) * scale;
    const toY = (y: number) => pad + (y - bb.y1) * scale;

    const p = this.theme.palette();
    ctx.fillStyle = p.inkSubtle;
    cy.nodes().forEach((n) => {
      const pos = n.position();
      ctx.globalAlpha = n.hasClass('entry') ? 1 : 0.6;
      ctx.beginPath();
      ctx.arc(toX(pos.x), toY(pos.y), n.hasClass('entry') ? 2.5 : 1.5, 0, Math.PI * 2);
      ctx.fill();
    });
    ctx.globalAlpha = 1;

    const ext = cy.extent();
    ctx.strokeStyle = p.accent;
    ctx.lineWidth = 1.5;
    ctx.strokeRect(toX(ext.x1), toY(ext.y1), (ext.x2 - ext.x1) * scale, (ext.y2 - ext.y1) * scale);

    // Stash the mapping so click-to-jump can invert it without recomputing the bounding box.
    this.minimapMap = { bb, scale, pad };
  }

  private minimapMap: { bb: { x1: number; y1: number }; scale: number; pad: number } | null = null;

  /** Click anywhere on the minimap to recenter the main viewport there. */
  protected onMinimapClick(event: MouseEvent): void {
    const cy = this.cy;
    const map = this.minimapMap;
    const canvas = this.minimapCanvas()?.nativeElement;
    if (!cy || !map || !canvas) return;
    const rect = canvas.getBoundingClientRect();
    const clickX = event.clientX - rect.left;
    const clickY = event.clientY - rect.top;
    const modelX = map.bb.x1 + (clickX - map.pad) / map.scale;
    const modelY = map.bb.y1 + (clickY - map.pad) / map.scale;
    const zoom = cy.zoom();
    const container = cy.container();
    const w = container?.clientWidth ?? 0;
    const h = container?.clientHeight ?? 0;
    cy.pan({ x: w / 2 - modelX * zoom, y: h / 2 - modelY * zoom });
  }
}
