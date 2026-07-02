import { Component, input, inject } from '@angular/core';
import { NodeStore } from '../../state/node.store';

@Component({
  selector: 'app-node-link',
  standalone: true,
  template: `
    <button
      class="underline decoration-dotted underline-offset-2 font-mono text-xs text-accent hover:text-ink transition-colors cursor-pointer text-left"
      (click)="open($event)"
      [title]="'Open node card: ' + label()"
    >{{ label() }}</button>
  `,
})
export class NodeLink {
  readonly nodeId = input.required<string>();
  readonly label = input.required<string>();

  private readonly nodeStore = inject(NodeStore);

  open(event: MouseEvent): void {
    // A link click must never bubble into an ancestor's own click handler (e.g. an
    // entries-table row that navigates to Trace on click) — opening the node card
    // is the terminal action.
    event.stopPropagation();
    this.nodeStore.show(this.nodeId());
  }
}
