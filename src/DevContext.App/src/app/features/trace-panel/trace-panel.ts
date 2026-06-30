import { Component, effect, inject, signal } from '@angular/core';

import { type TraceDetail, TraceStore } from '../../state/trace.store';
import { GraphCanvas } from '../../ui/graph-canvas/graph-canvas';
import { Icon } from '../../ui/icon/icon';
import { Segmented } from '../../ui/segmented/segmented';
import { Spinner } from '../../ui/spinner/spinner';
import { EntityCrossRef } from '../entity-cross-ref/entity-cross-ref';
import { ImpactPanel } from '../impact-panel/impact-panel';
import { StoryPanel } from '../story-panel/story-panel';

const DETAIL_OPTIONS = [
  { label: 'Signature', value: 'signature' },
  { label: 'Salient', value: 'salient' },
  { label: 'Full', value: 'full' },
] as const;

type RightTab = 'story' | 'impact' | 'entities';

@Component({
  selector: 'app-trace-panel',
  imports: [Icon, GraphCanvas, Segmented, Spinner, StoryPanel, ImpactPanel, EntityCrossRef],
  template: `
    <div class="flex h-full flex-col">
      <div class="flex flex-wrap items-center gap-4 border-b border-line bg-surface px-3 py-1.5 text-xs">
        <div class="flex items-center gap-2">
          <app-icon name="network" [size]="14" class="text-accent" />
          <span class="font-mono text-ink">{{ trace.focus() }}</span>
        </div>
        <div class="flex items-center gap-2 text-ink-muted">
          <span>Depth</span>
          <input type="range" min="1" max="10" [value]="trace.depth()" (change)="onDepth($event)" class="accent-accent" />
          <span class="w-4 text-ink">{{ trace.depth() }}</span>
        </div>
        <app-segmented
          [options]="detailOptions"
          [(selected)]="detail"
          (selectedChange)="setDetail($event)"
        />
        @if (trace.loading()) {
          <app-spinner />
        }
        @if (trace.error()) {
          <span class="text-danger">{{ trace.error() }}</span>
        }
      </div>

      <div class="flex min-h-0 flex-1">
        <div class="relative min-w-0 flex-1 bg-base">
          @if (trace.found() && trace.tree()) {
            <app-graph-canvas
              [trace]="trace.tree()"
              [selectedNodeId]="trace.selectedNodeId()"
              [highlightedNodeId]="highlightedNodeId()"
              (nodeSelected)="trace.selectNode($event)"
            />
          } @else {
            <div class="flex h-full items-center justify-center px-6 text-center text-sm text-ink-subtle">
              @if (trace.loading()) {
                <app-spinner />
              } @else if (trace.error()) {
                <span class="text-danger">{{ trace.error() }}</span>
              } @else {
                No traceable wiring from this entry.
              }
            </div>
          }
        </div>

        <div class="flex w-80 shrink-0 flex-col border-l border-line bg-surface">
          <div class="flex border-b border-line">
            @for (t of rightTabs; track t.id) {
              <button
                class="flex-1 py-1.5 text-[10px] font-medium transition-colors"
                [class.bg-surface-2]="rightTab() === t.id"
                [class.text-accent]="rightTab() === t.id"
                [class.text-ink-muted]="rightTab() !== t.id"
                [class.hover:text-ink]="rightTab() !== t.id"
                (click)="rightTab.set(t.id)"
              >
                {{ t.label }}
              </button>
            }
          </div>
          <div class="min-h-0 flex-1 flex flex-col overflow-hidden">
            @switch (rightTab()) {
              @case ('story') {
                <app-story-panel
                  [root]="trace.tree()"
                  [touched]="trace.touched()"
                  [emitted]="trace.emitted()"
                  (highlighted)="highlightedNodeId.set($event)"
                />
              }
              @case ('impact') {
                <app-impact-panel />
              }
              @case ('entities') {
                <app-entity-cross-ref />
              }
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class TracePanel {
  protected readonly trace = inject(TraceStore);
  protected readonly detailOptions = DETAIL_OPTIONS as unknown as readonly { label: string; value: string }[];
  protected readonly detail = signal(this.trace.detail());
  highlightedNodeId = signal<string | null>(null);
  rightTab = signal<RightTab>('story');

  readonly rightTabs: readonly { id: RightTab; label: string }[] = [
    { id: 'story', label: 'Story' },
    { id: 'impact', label: 'Impact' },
    { id: 'entities', label: 'Entities' },
  ];

  constructor() {
    effect(() => {
      this.detail.set(this.trace.detail());
    });
  }

  onDepth(e: Event): void {
    void this.trace.setDepth(+(e.target as HTMLInputElement).value);
  }

  setDetail(d: string): void {
    void this.trace.setDetail(d as TraceDetail);
  }
}
