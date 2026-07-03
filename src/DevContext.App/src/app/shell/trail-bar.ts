import { Component, inject, output } from '@angular/core';

import { TrailStore, type TrailStep } from '../state/trail.store';

/**
 * Trail bar (F proposal §2) — 22px breadcrumb + undo/redo above the Workbench.
 * Hidden until the first selection. Each crumb jumps the trail cursor; the parent
 * re-traces the restored step (only it holds the session handle).
 *
 * Lives inside WorkbenchPage for now; W4 may promote it into workspace-shell
 * between the tab strip and the router outlet (per §8.1) once tabs are wired.
 */
@Component({
  selector: 'app-trail-bar',
  template: `
    @if (trail.hasTrail()) {
      <div class="flex h-[22px] items-center gap-1 border-b border-line bg-base px-2 select-none">
        <button
          type="button"
          class="px-1 text-xs"
          [class.text-ink-muted]="trail.canUndo()"
          [class.text-ink-subtle]="!trail.canUndo()"
          [disabled]="!trail.canUndo()"
          (click)="undo()"
          title="Back (Ctrl+Z / Alt+←)"
        >
          ⟲
        </button>
        <button
          type="button"
          class="px-1 text-xs"
          [class.text-ink-muted]="trail.canRedo()"
          [class.text-ink-subtle]="!trail.canRedo()"
          [disabled]="!trail.canRedo()"
          (click)="redo()"
          title="Forward (Ctrl+Y / Alt+→)"
        >
          ⟳
        </button>
        <span class="h-3 w-px bg-line"></span>

        <div class="flex min-w-0 flex-1 items-center gap-1 overflow-hidden">
          @for (step of trail.breadcrumb(); track step.ts; let i = $index; let last = $last) {
            <button
              type="button"
              class="max-w-48 truncate font-mono text-2xs"
              [class.text-ink]="last"
              [class.text-ink-subtle]="!last"
              [class.hover:text-ink]="!last"
              [title]="step.title"
              (click)="jump(i)"
            >
              {{ step.title }}
            </button>
            @if (!last) {
              <span class="shrink-0 text-2xs text-ink-subtle">›</span>
            }
          }
        </div>

        @if (trail.pinCount() > 0) {
          <span class="chip active shrink-0 tabular-nums" title="Pinned steps seed the export pack">
            ◈ {{ trail.pinCount() }}
          </span>
        }
      </div>
    }
  `,
})
export class TrailBar {
  protected readonly trail = inject(TrailStore);

  /** The step the trail moved to — parent re-traces it. */
  readonly restore = output<TrailStep>();

  protected undo(): void {
    const step = this.trail.undo();
    if (step) this.restore.emit(step);
  }

  protected redo(): void {
    const step = this.trail.redo();
    if (step) this.restore.emit(step);
  }

  protected jump(index: number): void {
    const step = this.trail.jumpTo(index);
    if (step) this.restore.emit(step);
  }
}
