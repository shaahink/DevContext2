import { Component, effect, inject, signal } from '@angular/core';

import type { EdgeVm, NodeDetailVm } from '../../models/view-models';
import { ActivityService } from '../../core/activity/activity.service';
import { DevContextApi, type NeighborDirection } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { ViewFrame } from '../../shell/view-frame';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { SearchField } from '../../ui/search-field/search-field';

@Component({
  selector: 'app-browse-view',
  imports: [Badge, Button, SearchField, ViewFrame],
  template: `
    <app-view-frame>
      <div sidebar class="flex flex-col h-full">
        <div class="border-b border-line px-3 py-2">
          <app-search-field [(query)]="query" />
        </div>
      </div>

      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Browse</h2>
        <span class="text-xs text-ink-subtle">{{ results().length }} nodes</span>
      </div>

      @if (session.ready()) {
        <div class="divide-y divide-line">
          @for (node of results(); track node.nodeId) {
            <div
              class="flex items-center gap-3 px-3 py-2 text-xs cursor-pointer hover:bg-surface-2"
              [class.bg-accent/10]="selectedNodeId() === node.nodeId"
              (click)="selectNode(node.nodeId)"
              (keydown.enter)="selectNode(node.nodeId)"
              tabindex="0"
            >
              <div class="min-w-0 flex-1">
                <p class="truncate font-mono text-ink">{{ node.title }}</p>
                <p class="truncate text-ink-subtle">{{ node.nodeId }}</p>
              </div>
              <div class="flex shrink-0 items-center gap-1.5">
                <app-badge variant="accent">{{ node.kind }}</app-badge>
                @for (tag of node.tags.slice(0, 2); track tag) { <app-badge variant="default">{{ tag }}</app-badge> }
              </div>
            </div>
          }
        </div>

        @if (nodeDetail(); as nd) {
          <div class="border-t border-line bg-surface p-4">
            <h3 class="mb-3 text-sm font-semibold text-ink">{{ nd.title }}</h3>
            <div class="grid grid-cols-4 gap-3 text-xs">
              <div><span class="text-ink-subtle">Kind</span><p class="text-ink">{{ nd.kind }}</p></div>
              <div><span class="text-ink-subtle">Out</span><p class="text-ink">{{ nd.outDegree }}</p></div>
              <div><span class="text-ink-subtle">In</span><p class="text-ink">{{ nd.inDegree }}</p></div>
              @if (nd.filePath) { <div><span class="text-ink-subtle">Location</span><p class="truncate font-mono text-ink">{{ nd.filePath }}</p></div> }
            </div>
            @if (nd.tags.length) { <div class="mt-2 flex flex-wrap gap-1">@for (tag of nd.tags; track tag) { <app-badge variant="default">{{ tag }}</app-badge> }</div> }
            <div class="mt-3 flex gap-1">
              @for (dir of neighborDirs; track dir.value) {
                <app-button variant="ghost" size="sm" (click)="loadNeighbors(dir.value)">{{ dir.label }}</app-button>
              }
            </div>
            @if (neighbors().length) {
              <div class="mt-2 max-h-48 overflow-auto space-y-1">
                @for (edge of neighbors(); track edge.from + edge.to) {
                  <div class="rounded border border-line bg-surface-2 px-2 py-1 text-xs">
                    <span class="font-mono text-ink">{{ edge.otherTitle }}</span>
                    <span class="ml-2 text-ink-subtle">{{ edge.kind }}</span>
                    @if (edge.resolution === 'Syntactic') { <app-badge variant="warn" class="ml-1">approx</app-badge> }
                  </div>
                }
              </div>
            }
          </div>
        }
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to browse nodes.</p></div>
      }
    </app-view-frame>
  `,
})
export class BrowseView {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly activity = inject(ActivityService);

  protected readonly query = signal('');
  protected readonly results = signal<{ nodeId: string; title: string; kind: string; tags: readonly string[] }[]>([]);
  protected readonly selectedNodeId = signal<string | null>(null);
  protected readonly nodeDetail = signal<NodeDetailVm | null>(null);
  protected readonly neighbors = signal<readonly EdgeVm[]>([]);
  protected readonly neighborDir = signal<NeighborDirection>('out');
  protected readonly neighborDirs: { label: string; value: NeighborDirection }[] = [
    { label: 'Calls out', value: 'out' }, { label: 'Called by', value: 'in' }, { label: 'Usages', value: 'usages' },
  ];

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const q = this.query();
      const handle = this.session.handle();
      if (!handle) return;
      if (this.debounceTimer) clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => {
        void this.activity.runSecondary('Searching nodes…', () =>
          this.api.searchNodes(handle, q, 50).then((res) => {
            this.results.set(res.nodes.map((n) => ({ nodeId: n.nodeId, title: n.title, kind: n.kind, tags: n.tags })));
          }),
        );
      }, 300);
    });
  }

  protected async selectNode(nodeId: string): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.selectedNodeId.set(nodeId);
    await this.activity.runSecondary('Loading node…', async () => {
      const [node, nbrs] = await Promise.all([
        this.api.getNode(handle, nodeId),
        this.api.getNeighbors(handle, nodeId, this.neighborDir()),
      ]);
      this.nodeDetail.set(node.found ? { id: node.nodeId, title: node.title, kind: node.kind, outDegree: node.outDegree, inDegree: node.inDegree, tags: node.tags, filePath: node.filePath ?? undefined } : null);
      this.neighbors.set(nbrs.edges.map((e) => ({ from: e.from, to: e.to, kind: e.kind, resolution: e.resolution, otherTitle: e.otherTitle, provenance: e.provenance ?? undefined })));
    });
  }

  protected async loadNeighbors(dir: NeighborDirection): Promise<void> {
    const handle = this.session.handle();
    const nodeId = this.selectedNodeId();
    if (!handle || !nodeId) return;
    this.neighborDir.set(dir);
    await this.activity.runSecondary('Loading neighbors…', async () => {
      const res = await this.api.getNeighbors(handle, nodeId, dir);
      this.neighbors.set(res.edges.map((e) => ({ from: e.from, to: e.to, kind: e.kind, resolution: e.resolution, otherTitle: e.otherTitle, provenance: e.provenance ?? undefined })));
    });
  }
}
