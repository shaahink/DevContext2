import { Component, DestroyRef, ElementRef, input, inject } from '@angular/core';
import { NodeStore } from '../../state/node.store';
import { NodePeekStore } from '../../state/node-peek.store';

const HOVER_DELAY_MS = 200;

@Component({
  selector: 'app-node-link',
  standalone: true,
  template: `
    <button
      class="underline decoration-dotted underline-offset-2 font-mono text-xs text-accent hover:text-ink transition-colors cursor-pointer text-left"
      (click)="open($event)"
      (mouseenter)="onMouseEnter()"
      (mouseleave)="onMouseLeave()"
      [title]="'Open node card: ' + label()"
    >{{ label() }}</button>
  `,
})
export class NodeLink {
  readonly nodeId = input.required<string>();
  readonly label = input.required<string>();

  private readonly nodeStore = inject(NodeStore);
  private readonly peekStore = inject(NodePeekStore);
  private readonly el = inject(ElementRef<HTMLElement>);

  private hoverTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clearHoverTimer());
  }

  open(event: MouseEvent): void {
    // A link click must never bubble into an ancestor's own click handler (e.g. an
    // entries-table row that navigates to Trace on click) — opening the node card
    // is the terminal action.
    event.stopPropagation();
    this.clearHoverTimer();
    this.peekStore.dismiss();
    this.nodeStore.show(this.nodeId());
  }

  onMouseEnter(): void {
    this.clearHoverTimer();
    this.peekStore.cancelHide();
    this.hoverTimer = setTimeout(() => {
      const rect = this.el.nativeElement.getBoundingClientRect();
      void this.peekStore.show(this.nodeId(), { x: rect.left, y: rect.bottom + 4 });
    }, HOVER_DELAY_MS);
  }

  onMouseLeave(): void {
    this.clearHoverTimer();
    if (this.peekStore.nodeId() === this.nodeId()) this.peekStore.requestHide();
  }

  private clearHoverTimer(): void {
    if (this.hoverTimer !== null) {
      clearTimeout(this.hoverTimer);
      this.hoverTimer = null;
    }
  }
}
