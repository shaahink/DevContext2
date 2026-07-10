import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import type { ProjectNode, ServiceCard, ServiceStyle } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { SessionStore } from '../../state/session.store';

/** Computed layout node — enriches raw ProjectNode + ServiceStyle with layout position. */
interface HeroNode {
  name: string;
  label: string;
  style: string;
  stack: readonly string[];
  /** 0 = gateway, 1 = core service, 2 = other dependency */
  column: 0 | 1 | 2;
  row: number;
  depsFrom: string[]; // names this node depends on that are ALSO in the hero
}

@Component({
  selector: 'app-service-map-hero',
  imports: [RouterLink],
  template: `
    @if (nodes().length > 0) {
      <div class="hero-map">
        @for (node of nodes(); track node.name) {
          <a
            class="hero-card"
            [class.gateway]="node.column === 0"
            [class.core]="node.column === 1"
            [style.grid-column]="node.column + 1"
            [style.grid-row]="node.row + 1"
            [routerLink]="['/explore']"
            [queryParams]="{ view: 'system', project: node.name }"
          >
            <span class="hero-card-name">{{ node.label }}</span>
            @if (node.style) {
              <span class="chip shrink text-2xs">{{ node.style }}</span>
            }
            @if (node.stack.length > 0) {
              <span class="text-2xs text-ink-subtle truncate">{{ node.stack.slice(0, 2).join(', ') }}</span>
            }
          </a>
        }

        <!-- dependency arrows rendered as SVG overlay -->
        <svg class="hero-arrows" aria-hidden="true">
          @for (edge of layoutEdges(); track edge.from + '→' + edge.to) {
            <path
              [attr.d]="edge.d"
              fill="none"
              stroke="var(--vibe-line-strong)"
              stroke-width="1"
              marker-end="url(#hero-arrow)"
            />
          }
          <defs>
            <marker id="hero-arrow" viewBox="0 0 6 6" refX="6" refY="3" markerWidth="5" markerHeight="5" orient="auto-start-reverse">
              <path d="M 0 0 L 6 3 L 0 6 Z" fill="var(--vibe-line-strong)" />
            </marker>
          </defs>
        </svg>

        <!-- bus/broker bottom rail -->
        @if (hasBus(); as bus) {
          <div class="hero-bus-rail">
            <span class="chip text-2xs text-warn">{{ bus }}</span>
          </div>
        }
      </div>
    } @else {
      <p class="py-8 text-center text-xs text-ink-subtle">No project topology resolved.</p>
    }
  `,
  styles: `
    .hero-map {
      display: grid;
      grid-template-columns: 1fr auto 1fr;
      gap: 12px;
      position: relative;
      padding: 8px 0;
    }
    .hero-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 4px;
      padding: 10px 12px;
      border-radius: 8px;
      border: 1px solid var(--vibe-line);
      background: var(--vibe-surface);
      text-decoration: none;
      min-width: 100px;
      transition: border-color 0.15s, background 0.15s;
    }
    .hero-card:hover {
      border-color: var(--vibe-accent);
      background: var(--vibe-surface-2);
    }
    .hero-card.gateway { border-color: var(--vibe-accent-dim); background: var(--vibe-accent); color: var(--vibe-accent-ink); }
    .hero-card.gateway .chip { background: rgb(255 255 255 / 0.15); border-color: transparent; }
    .hero-card.core { border-color: var(--vibe-line-strong); }
    .hero-card-name {
      font-family: 'JetBrains Mono', monospace;
      font-size: 12px;
      font-weight: 600;
      text-align: center;
      color: var(--vibe-ink);
    }
    .hero-card.gateway .hero-card-name { color: var(--vibe-accent-ink); }
    .hero-arrows {
      position: absolute;
      inset: 0;
      width: 100%;
      height: 100%;
      pointer-events: none;
      overflow: visible;
    }
    .hero-bus-rail {
      grid-column: 1 / -1;
      display: flex;
      justify-content: center;
      padding: 6px;
      border-top: 1px dashed var(--vibe-line);
      margin-top: 4px;
    }
  `,
})
export class ServiceMapHero {
  protected readonly session = inject(SessionStore);
  readonly topology = input.required<readonly ProjectNode[]>();
  readonly serviceStyles = input.required<readonly ServiceStyle[]>();

  protected readonly nodes = computed<readonly HeroNode[]>(() => {
    // L4.3 — prefer the ServiceMap projection (runnables only, full DisplayName, no libraries).
    // The topology fallback stays for pre-facets sessions / repos with no Service nodes.
    const cards = this.session.graphFacets()?.serviceMap?.services ?? [];
    if (cards.length > 0) return this.layoutFromCards(cards);

    const projects = this.topology();
    const styles = this.serviceStyles();
    if (projects.length === 0) return [];
    if (projects.length === 1) {
      return [this.toHeroNode(projects[0], styles, 1, 0)];
    }

    const nameSet = new Set(projects.map((p) => p.name));
    const depsFrom = new Map<string, string[]>();
    for (const p of projects) {
      depsFrom.set(p.name, p.dependsOn.filter((d) => nameSet.has(d)));
    }

    // Classify: gateway, core, dependency
    const gateways: ProjectNode[] = [];
    const cores: ProjectNode[] = [];
    const others: ProjectNode[] = [];
    for (const p of projects) {
      const style = this.findStyle(p.name, styles);
      if (this.isGateway(p.name, style)) gateways.push(p);
      else if (this.isCoreService(p.name, style, p.dependsOn, depsFrom)) cores.push(p);
      else others.push(p);
    }

    // Sort cores by dependency rank (fewer dependents = higher)
    const rank = (p: ProjectNode) => (depsFrom.get(p.name) ?? []).length + p.dependsOn.length;
    cores.sort((a, b) => rank(a) - rank(b));
    others.sort((a, b) => rank(a) - rank(b));

    const result: HeroNode[] = [];
    for (const g of gateways) result.push(this.toHeroNode(g, styles, 0, result.length));
    for (const c of cores) result.push(this.toHeroNode(c, styles, 1, result.reduce((n, r) => n + (r.column === 1 ? 1 : 0), 0)));
    for (const o of others) result.push(this.toHeroNode(o, styles, 2, result.reduce((n, r) => n + (r.column === 2 ? 1 : 0), 0)));
    return result;
  });

  /** Lays out the projection's ServiceCards: gateways left, everything else center.
   * DisplayName is rendered verbatim — no client-side name truncation (L4.3). */
  private layoutFromCards(cards: readonly ServiceCard[]): HeroNode[] {
    const gateways = cards.filter((c) => c.kind === 'Gateway');
    const rest = cards.filter((c) => c.kind !== 'Gateway');
    const result: HeroNode[] = [];
    for (const g of gateways) result.push(this.cardToHeroNode(g, 0, result.length));
    let coreRow = 0;
    for (const c of rest) result.push(this.cardToHeroNode(c, 1, coreRow++));
    return result;
  }

  private cardToHeroNode(c: ServiceCard, col: 0 | 1 | 2, row: number): HeroNode {
    return {
      name: c.name,
      label: c.displayName || c.name,
      style: c.kind === 'Service' ? '' : c.kind,
      stack: c.stack,
      column: col,
      row,
      depsFrom: [],
    };
  }

  /** Pairs that have a real dependency AND both are in the hero nodes. */
  protected readonly layoutEdges = computed<readonly { from: string; to: string; d: string }[]>(() => {
    // Edge arrows are computed using a simple heuristic: draw from right edge of source to left edge of target
    // For now, just return empty — edge rendering needs positions computed post-layout.
    return [];
  });

  protected readonly hasBus = computed(() => {
    // Prefer the projection's transports — a real "bus" ServiceLink edge is authoritative.
    const transports = this.session.graphFacets()?.serviceMap?.transports ?? [];
    const busTransports = new Set(
      transports.filter((t) => /bus|rabbitmq|kafka|masstransit|queue/i.test(t.transport)).map((t) => t.transport),
    );
    if (busTransports.size > 0) return [...busTransports].join(' / ');

    const seen = new Set<string>();
    for (const s of this.serviceStyles()) {
      for (const t of s.stack) {
        if (/rabbitmq|masstransit|kafka|nservicebus|azure.*service.*bus/i.test(t)) {
          seen.add(t);
        }
      }
    }
    return seen.size > 0 ? [...seen].join(' / ') : null;
  });

  private toHeroNode(p: ProjectNode, styles: readonly ServiceStyle[], col: 0 | 1 | 2, row: number): HeroNode {
    const s = this.findStyle(p.name, styles);
    return { name: p.name, label: p.name, style: s.style, stack: s.stack,
      column: col, row, depsFrom: p.dependsOn };
  }

  private findStyle(name: string, styles: readonly ServiceStyle[]): ServiceStyle {
    const found = styles.find((s) => s.projectName === name);
    if (found) return found;
    return { $typeName: 'devcontext.v1.ServiceStyle', projectName: name, style: '', stack: [] } as unknown as ServiceStyle;
  }

  private isGateway(name: string, style: ServiceStyle): boolean {
    const n = name.toLowerCase();
    return /gateway|yarp|proxy|reverse|apigateway/i.test(n)
      || style.style === 'Gateway';
  }

  private isCoreService(name: string, style: ServiceStyle, deps: readonly string[], depsFrom: Map<string, string[]>): boolean {
    const n = name.toLowerCase();
    if (this.isGateway(name, style)) return false;
    // Core = called by at least one other project, or has meaningful dependencies
    const calledBy = depsFrom.get(name) ?? [];
    if (calledBy.length > 0) return true;
    if (deps.length >= 3) return true;
    if (/api$/i.test(n) || /web$/i.test(n)) return true;
    return false;
  }
}
