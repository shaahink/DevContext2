import { Component, effect, ElementRef, inject, input } from '@angular/core';
import {
  ArrowRight,
  Boxes,
  CircleDot,
  Code,
  createElement,
  Database,
  FolderOpen,
  type IconNode,
  Loader,
  Map as MapIcon,
  Network,
  Play,
  Plug,
  RotateCw,
  Search,
  Square,
  Webhook,
  X,
} from 'lucide';

const REGISTRY: Record<string, IconNode> = {
  'arrow-right': ArrowRight,
  boxes: Boxes,
  code: Code,
  database: Database,
  dot: CircleDot,
  'folder-open': FolderOpen,
  loader: Loader,
  map: MapIcon,
  network: Network,
  play: Play,
  plug: Plug,
  refresh: RotateCw,
  search: Search,
  square: Square,
  webhook: Webhook,
  x: X,
};

/** Renders a lucide icon by name. Framework-agnostic icon source wrapped once, so the rest of the
 * app references icons by a stable name and never touches the icon library directly. */
@Component({
  selector: 'app-icon',
  template: '',
  host: { class: 'inline-flex shrink-0 items-center justify-center' },
})
export class Icon {
  readonly name = input.required<string>();
  readonly size = input(16);
  readonly strokeWidth = input(2);

  private readonly host: ElementRef<HTMLElement> = inject(ElementRef);

  constructor() {
    effect(() => {
      const node = REGISTRY[this.name()];
      const el = this.host.nativeElement;
      el.replaceChildren();
      if (!node) return;
      const svg = createElement(node, {
        width: String(this.size()),
        height: String(this.size()),
        'stroke-width': String(this.strokeWidth()),
      });
      el.appendChild(svg);
    });
  }
}
