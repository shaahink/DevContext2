import { Component, inject, signal, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { NodeStore } from '../../state/node.store';
import { DevContextApi } from '../../data-access/devcontext-api';
import { Router } from '@angular/router';

interface PaletteItem {
  label: string;
  sub?: string;
  action: () => void;
  section: string;
}

@Component({
  selector: 'app-palette',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (open()) {
      <div class="fixed inset-0 z-50" (click)="close()" (keydown.escape)="close()" tabindex="0" role="dialog" aria-modal="true">
        <div class="absolute inset-0 bg-base/70"></div>
        <div class="absolute top-[15%] left-1/2 -translate-x-1/2 w-[560px] max-h-[400px] bg-surface border border-line rounded-lg shadow-2xl overflow-hidden"
             (click)="$event.stopPropagation()" (keydown)="$event.stopPropagation()" tabindex="-1">
          <input class="w-full border-b border-line bg-transparent px-4 py-3 text-sm text-ink outline-none placeholder:text-ink-subtle"
                 placeholder="Search entries, nodes, or type a command..."
                 [ngModel]="query()" (ngModelChange)="onQuery($event)"
                 (keydown)="onKey($event)" />
          <div class="overflow-y-auto max-h-[340px]">
            @for (item of filtered(); track item.label) {
              <button class="flex w-full items-center justify-between px-4 py-2 text-left hover:bg-surface-2"
                      [class.bg-surface-2]="selectedIndex() === $index"
                      (click)="item.action(); close()" (mouseenter)="selectedIndex.set($index)">
                <span class="text-sm text-ink">{{ item.label }}
                  @if (item.sub) { <span class="text-xs text-ink-muted ml-2">{{ item.sub }}</span> }
                </span>
                <span class="text-2xs text-ink-subtle">{{ item.section }}</span>
              </button>
            } @empty {
              <div class="px-4 py-3 text-sm text-ink-muted">No results</div>
            }
          </div>
        </div>
      </div>
    }
  `,
})
export class Palette {
  private api = inject(DevContextApi);
  private session = inject(SessionStore);
  private traceStore = inject(TraceStore);
  private nodeStore = inject(NodeStore);
  private router = inject(Router);

  readonly open = signal(false);
  readonly query = signal('');
  readonly selectedIndex = signal(0);
  readonly searchResults = signal<string[]>([]);

  private actions: PaletteItem[] = [];

  @HostListener('window:keydown', ['$event'])
  onGlobalKey(e: KeyboardEvent) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      this.open.set(true);
      this.query.set('');
      this.selectedIndex.set(0);
      this.buildItems();
    }
  }

  close(): void { this.open.set(false); }

  onQuery(val: string): void {
    this.query.set(val);
    this.selectedIndex.set(0);
    this.buildItems();
    const h = this.session.handle();
    if (h && val.length >= 2) {
      this.api.searchNodes(h, val, 8).then(r => {
        this.searchResults.set(r.results?.map(n => n.nodeId) ?? []);
        this.buildItems();
      }).catch(() => { /* swallow */ });
    }
  }

  onKey(e: KeyboardEvent): void {
    if (e.key === 'ArrowDown') { e.preventDefault(); this.selectedIndex.update(i => Math.min(i + 1, this.filtered().length - 1)); }
    else if (e.key === 'ArrowUp') { e.preventDefault(); this.selectedIndex.update(i => Math.max(i - 1, 0)); }
    else if (e.key === 'Enter') { const item = this.filtered()[this.selectedIndex()]; if (item) { item.action(); this.close(); } }
  }

  private buildItems(): void {
    const q = this.query().toLowerCase();
    const items: PaletteItem[] = [];

    items.push({ label: 'Analyze repo...', sub: 'Local path or URL', section: 'Action', action: () => { this.router.navigate(['/']); } });
    items.push({ label: 'Go to Trace', section: 'View', action: () => { this.router.navigate(['/trace']); } });
    items.push({ label: 'Go to Entries', section: 'View', action: () => { this.router.navigate(['/entries']); } });
    items.push({ label: 'Go to Overview', section: 'View', action: () => { this.router.navigate(['/overview']); } });
    items.push({ label: 'Go to Insights', section: 'View', action: () => { this.router.navigate(['/insights']); } });
    items.push({ label: 'Go to Graph', section: 'View', action: () => { this.router.navigate(['/graph']); } });
    items.push({ label: 'Go to Browse', section: 'View', action: () => { this.router.navigate(['/browse']); } });
    items.push({ label: 'Go to Document', section: 'View', action: () => { this.router.navigate(['/document']); } });
    items.push({ label: 'Go to Settings', section: 'View', action: () => { this.router.navigate(['/settings']); } });
    items.push({ label: 'Go to Stats', section: 'View', action: () => { this.router.navigate(['/stats']); } });

    const entries = this.session.entryGroups();
    for (const g of entries ?? []) {
      for (const e of g.entries) {
        if (!q || e.title.toLowerCase().includes(q) || e.focus.toLowerCase().includes(q))
          items.push({ label: e.title, sub: e.focus, section: g.label, action: () => { this.traceStore.trace(this.session.handle()!, e.focus); this.router.navigate(['/trace']); } });
      }
    }

    for (const nid of this.searchResults()) {
      items.push({ label: nid, section: 'Node', action: () => { this.nodeStore.show(nid); } });
    }

    this.actions = items;
  }

  filtered(): PaletteItem[] {
    return this.actions;
  }
}
