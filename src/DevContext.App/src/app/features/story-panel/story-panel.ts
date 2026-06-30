import { Component, computed, input, output } from '@angular/core';

import type { TraceNodeVm } from '../../models/view-models';
import { Badge } from '../../ui/badge/badge';
import { Icon } from '../../ui/icon/icon';

interface StoryStep {
  readonly depth: number;
  readonly nodeId: string;
  readonly seam: string;
  readonly title: string;
  readonly fileHint?: string;
  readonly salient?: string;
  readonly truncated: boolean;
  readonly omitted: number;
}

function flatten(root: TraceNodeVm): StoryStep[] {
  const steps: StoryStep[] = [];
  const walk = (node: TraceNodeVm, depth: number): void => {
    const fileHint = node.provenance ?? undefined;
    steps.push({
      depth,
      nodeId: node.id,
      seam: node.seam,
      title: node.title,
      fileHint,
      salient: node.salient || undefined,
      truncated: node.truncated,
      omitted: node.omitted,
    });
    for (const child of node.children) walk(child, depth + 1);
  };
  walk(root, 0);
  return steps;
}

const SEAM_SEVERITY: Record<string, 'default' | 'accent' | 'success' | 'warn'> = {
  Entry: 'accent',
  Send: 'accent',
  Handle: 'success',
  Raise: 'warn',
  Consume: 'warn',
  Data: 'accent',
  Resolve: 'default',
  Pipeline: 'accent',
  Call: 'default',
};

@Component({
  selector: 'app-story-panel',
  imports: [Badge, Icon],
  template: `
    <div class="flex h-full flex-col overflow-hidden">
      <div class="flex items-center gap-2 border-b border-line px-3 py-1.5 text-xs">
        <app-icon name="code" [size]="13" class="text-accent" />
        <span class="font-medium text-ink">Trace story</span>
        @if (touched().length) {
          <span class="text-ink-subtle">
            &middot; touches <span class="font-mono text-ink">{{ touched().join(', ') }}</span>
          </span>
        }
        @if (emitted().length) {
          <span class="text-ink-subtle">
            &middot; emits <span class="font-mono text-ink">{{ emitted().join(', ') }}</span>
          </span>
        }
      </div>
      <div class="min-h-0 flex-1 overflow-y-auto p-2">
        @for (step of steps(); track step.nodeId; let i = $index) {
          <button
            (click)="highlighted.emit(step.nodeId)"
            (mouseenter)="highlighted.emit(step.nodeId)"
            (mouseleave)="highlighted.emit(null)"
            class="mb-3 block w-full text-left"
            [class.opacity-50]="step.truncated"
          >
            <div class="flex items-center gap-2">
              <app-badge [variant]="severity(step.seam)">{{ step.seam }}</app-badge>
              <span class="text-xs font-mono text-ink">{{ step.title }}</span>
            </div>
            @if (step.fileHint) {
              <div class="mt-0.5 truncate pl-1 font-mono text-[10px] text-ink-subtle">{{ step.fileHint }}</div>
            }
            @if (step.salient) {
              <div class="mt-1 rounded border border-line bg-base p-1.5 font-mono text-[10px] leading-relaxed text-ink-muted">{{ step.salient }}</div>
            }
            @if (step.truncated) {
              <div class="mt-1 text-[10px] text-ink-subtle italic">{{ step.omitted }} more steps omitted</div>
            }
          </button>
        } @empty {
          <p class="p-4 text-center text-xs text-ink-subtle">Select an entry to trace its wiring.</p>
        }
      </div>
    </div>
  `,
})
export class StoryPanel {
  readonly root = input<TraceNodeVm | null>(null);
  readonly touched = input<readonly string[]>([]);
  readonly emitted = input<readonly string[]>([]);
  readonly highlighted = output<string | null>();

  readonly steps = computed<StoryStep[]>(() => {
    const r = this.root();
    return r ? flatten(r) : [];
  });

  severity(seam: string): 'default' | 'accent' | 'success' | 'warn' {
    return SEAM_SEVERITY[seam] ?? 'default';
  }
}
