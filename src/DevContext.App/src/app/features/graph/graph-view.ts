import { Component, computed, inject, signal } from '@angular/core';
import { SessionStore } from '../../state/session.store';
import { DevContextApi } from '../../data-access/devcontext-api';
import { NodeStore } from '../../state/node.store';
import { ViewFrame } from '../../shell/view-frame';
import { Icon } from '../../ui/icon/icon';
import { Badge } from '../../ui/badge/badge';
import { NodeLink } from '../../ui/node-link/node-link';

interface GraphNode {
  id: string;
  title: string;
  kind: string;
  tags: string[];
}
interface GraphEdge {
  from: string;
  to: string;
  kind: string;
  label: string;
}

const SEAM_COLORS: Record<string, string> = {
  Calls: 'text-accent', Sends: 'text-warn', Handles: 'text-success',
  Raises: 'text-warn', Consumes: 'text-success', Data: 'text-ink-subtle',
  DiResolve: 'text-accent', Pipeline: 'text-ink-subtle',
};

@Component({
  selector: 'app-graph-view',
  standalone: true,
  imports: [ViewFrame, Icon, Badge, NodeLink],
  template: `
    <app-view-frame>
      <div sidebar class="flex flex-col h-full p-3 space-y-2">
        <p class="text-2xs font-semibold uppercase text-ink-subtle">Filter</p>
        @for (sk of seamKinds(); track sk) {
          <button class="flex items-center gap-1.5 rounded px-2 py-1 text-xs transition-colors"
                  [class.bg-accent/10]="seamFilter() === sk" [class.text-accent]="seamFilter() === sk"
                  [class.text-ink-muted]="seamFilter() !== sk"
                  [class.hover:bg-surface-2]="seamFilter() !== sk" (click)="seamFilter.set(seamFilter() === sk ? null : sk)">
            <span [class]="SEAM_COLORS[sk] ?? 'text-ink-subtle'">⬤</span> {{ sk }}
          </button>
        }
        <button class="rounded px-2 py-1 text-xs text-ink-muted hover:bg-surface-2" (click)="seamFilter.set(null)">Show all</button>
      </div>

      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Graph</h2>
        <span class="text-xs text-ink-subtle">{{ filteredNodes().length }} nodes · {{ filteredEdges().length }} edges</span>
      </div>

      @if (session.ready()) {
        @if (loading()) {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Loading graph…</p></div>
        } @else if (nodes().length) {
          <div class="overflow-auto p-4">
            <div class="overflow-auto rounded border border-line">
              <table class="w-full text-xs">
                <thead class="sticky top-0 bg-surface">
                  <tr class="border-b border-line text-2xs text-ink-muted uppercase">
                    <th class="p-2 text-left">Node</th>
                    <th class="p-2 text-left">Kind</th>
                    <th class="p-2 text-left">Connections</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-line">
                  @for (node of filteredNodes(); track node.id) {
                    <tr class="hover:bg-surface-2">
                      <td class="p-2"><app-node-link [nodeId]="node.id" [label]="node.title" /></td>
                      <td class="p-2 text-ink-muted text-2xs">{{ node.kind }}</td>
                      <td class="p-2">
                        <div class="flex flex-wrap gap-1">
                          @for (edge of nodeEdges(node.id); track edge.from + '-' + edge.to + '-' + edge.kind) {
                            <span class="rounded bg-surface-2 px-1 py-0.5 text-2xs" [class]="SEAM_COLORS[edge.kind] ?? 'text-ink-subtle'">
                              {{ edge.label }}
                            </span>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        } @else {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Graph data not available for this session.</p></div>
        }
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to explore the graph.</p></div>
      }
    </app-view-frame>
  `,
})
export class GraphView {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly nodeStore = inject(NodeStore);
  protected readonly SEAM_COLORS = SEAM_COLORS;

  protected readonly loading = signal(false);
  protected readonly nodes = signal<GraphNode[]>([]);
  protected readonly edges = signal<GraphEdge[]>([]);
  protected readonly seamFilter = signal<string | null>(null);

  protected readonly seamKinds = computed(() => {
    const kinds = new Set<string>();
    for (const e of this.edges()) kinds.add(e.kind);
    return [...kinds].sort();
  });

  protected readonly filteredEdges = computed(() => {
    const sf = this.seamFilter();
    if (!sf) return this.edges();
    return this.edges().filter(e => e.kind === sf);
  });

  protected readonly filteredNodes = computed(() => {
    const edges = this.filteredEdges();
    const nodeIds = new Set<string>();
    for (const e of edges) { nodeIds.add(e.from); nodeIds.add(e.to); }
    return this.nodes().filter(n => nodeIds.has(n.id));
  });

  protected nodeEdges(nodeId: string): GraphEdge[] {
    return this.filteredEdges().filter(e => e.from === nodeId || e.to === nodeId);
  }

  constructor() {
    this.loadGraph();
  }

  private loadGraph(): void {
    const handle = this.session.handle();
    if (!handle) return;
    this.loading.set(true);

    // Seed from entry nodes
    const groups = this.session.entryGroups();
    const seeds = groups.flatMap(g => g.entries.map(e => e.nodeId)).filter(Boolean);
    const visited = new Set<string>();
    const graphNodes: GraphNode[] = [];
    const graphEdges: GraphEdge[] = [];

    const queue = [...seeds.slice(0, 10)]; // cap seed set
    visited.clear();

    const explore = async (): Promise<void> => {
      for (let i = 0; i < queue.length && visited.size < 30; i++) {
        const nid = queue[i];
        if (visited.has(nid)) continue;
        visited.add(nid);

        try {
          const node = await this.api.getNode(handle, nid);
          if (node.found) {
            graphNodes.push({ id: node.nodeId, title: node.title, kind: node.kind, tags: node.tags });
          }

          const neighbors = await this.api.getNeighbors(handle, nid, 'out');
          for (const e of neighbors.edges.slice(0, 5)) {
            graphEdges.push({ from: e.from, to: e.to, kind: e.kind, label: `${e.kind} → ${e.otherTitle}` });
            if (!visited.has(e.to) && visited.size < 30) queue.push(e.to);
          }
        } catch {
          // skip unreachable nodes
        }
      }
      this.nodes.set(graphNodes);
      this.edges.set(graphEdges);
      this.loading.set(false);
    };

    void explore();
  }
}
