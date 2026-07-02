import { Component, input, inject } from '@angular/core';
import { NodeStore } from '../../state/node.store';

@Component({
  selector: 'app-node-link',
  standalone: true,
  template: `
    <button
      class="underline decoration-dotted underline-offset-2 font-mono text-xs text-accent hover:text-ink transition-colors cursor-pointer text-left"
      (click)="open()"
      [title]="'Open node card: ' + label()"
    >{{ label() }}</button>
  `,
})
export class NodeLink {
  readonly nodeId = input.required<string>();
  readonly label = input.required<string>();

  private readonly nodeStore = inject(NodeStore);

  open(): void {
    this.nodeStore.show(this.nodeId());
  }
}
