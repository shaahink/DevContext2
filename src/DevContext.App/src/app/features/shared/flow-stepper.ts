import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import type { FlowStat } from '../../state/atlas.store';
import { KIND_LABELS } from '../../models/view-models';

@Component({
  selector: 'app-flow-stepper',
  imports: [RouterLink],
  template: `
    @if (flows().length > 0) {
      <div class="space-y-4">
        @for (flow of flows(); track flow.focus) {
          <a
            class="stepper-strip"
            [routerLink]="['/explore']"
            [queryParams]="{ focus: flow.focus }"
          >
            <div class="flex items-center gap-1.5">
              <!-- entry chip -->
              <span class="step-chip bg-accent-dim text-accent-ink">{{ flow.title }}</span>
              <!-- stats -->
              <span class="flex items-center gap-1 text-2xs text-ink-subtle">
                <span>{{ flow.nodeCount }} steps</span>
                <span>&middot;</span>
                <span>{{ flow.maxDepth }} deep</span>
                @if (flow.boundaryCrossings > 0) {
                  <span>&middot;</span>
                  <span class="text-warn">{{ flow.boundaryCrossings }} cross-service</span>
                }
                @if (flow.dataTouches > 0) {
                  <span>&middot;</span>
                  <span>{{ flow.dataTouches }} data</span>
                }
                <span>&middot;</span>
                <span class="text-ink-muted">{{ KIND_LABELS[flow.kind] ?? flow.kind }}</span>
              </span>
            </div>
          </a>
        }
      </div>
    } @else {
      <p class="py-4 text-center text-xs text-ink-subtle">No flows indexed yet — start background indexing from the Explore page.</p>
    }
  `,
  styles: `
    .stepper-strip {
      display: block;
      padding: 8px 12px;
      border-radius: 6px;
      border: 1px solid var(--vibe-line);
      background: var(--vibe-surface);
      text-decoration: none;
      transition: border-color 0.15s, background 0.15s;
    }
    .stepper-strip:hover {
      border-color: var(--vibe-accent);
      background: var(--vibe-surface-2);
    }
    .step-chip {
      display: inline-flex;
      align-items: center;
      padding: 2px 8px;
      border-radius: 4px;
      font-family: 'JetBrains Mono', monospace;
      font-size: 11px;
      font-weight: 500;
    }
  `,
})
export class FlowStepper {
  readonly flows = input.required<readonly FlowStat[]>();
  protected readonly KIND_LABELS = KIND_LABELS;
}
