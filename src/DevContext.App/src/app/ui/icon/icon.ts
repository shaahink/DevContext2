import { Component, effect, ElementRef, inject, input } from '@angular/core';
import {
  Activity,
  ArrowRight,
  ArrowUp,
  Boxes,
  Check,
  ChevronDown,
  ChevronRight,
  Circle,
  CircleDot,
  Code,
  Copy,
  createElement,
  Database,
  Download,
  FileText,
  FolderOpen,
  Globe,
  Info,
  type IconNode,
  Laptop,
  Layers,
  Loader,
  Map as MapIcon,
  Moon,
  Network,
  Palette,
  Play,
  Plug,
  RotateCw,
  Search,
  Settings,
  Square,
  Sun,
  TriangleAlert,
  Webhook,
  X,
  Zap,
} from 'lucide';

const REGISTRY: Record<string, IconNode> = {
  activity: Activity,
  'alert-triangle': TriangleAlert,
  'arrow-right': ArrowRight,
  'arrow-up': ArrowUp,
  boxes: Boxes,
  check: Check,
  'chevron-down': ChevronDown,
  'chevron-right': ChevronRight,
  circle: Circle,
  code: Code,
  copy: Copy,
  database: Database,
  dot: CircleDot,
  download: Download,
  'file-text': FileText,
  'folder-open': FolderOpen,
  globe: Globe,
  info: Info,
  laptop: Laptop,
  layers: Layers,
  loader: Loader,
  map: MapIcon,
  moon: Moon,
  network: Network,
  palette: Palette,
  play: Play,
  plug: Plug,
  refresh: RotateCw,
  search: Search,
  settings: Settings,
  square: Square,
  sun: Sun,
  webhook: Webhook,
  x: X,
  zap: Zap,
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
