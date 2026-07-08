import { Component, computed, ElementRef, HostListener, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { isStale, LatestGate } from '../../core/rpc-call';
import { type AnalyzeSpec, DevContextApi } from '../../data-access/devcontext-api';
import { KIND_ICONS } from '../../models/view-models';
import { AtlasStore } from '../../state/atlas.store';
import { PrefsStore } from '../../state/prefs.store';
import { RecentStore } from '../../state/recent.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { WorkspaceStore } from '../../state/workspace.store';
import { KindIcon } from '../../ui/kind-icon/kind-icon';
import { ToastService } from '../../ui/toast/toast';
import { copyToClipboard } from '../../core/clipboard';

type Verb = 'trace' | 'node' | 'usages' | 'impact' | 'copy';
const VERBS: readonly Verb[] = ['trace', 'node', 'usages', 'impact', 'copy'];
const VERB_LABELS: Record<Verb, string> = { trace: 'Trace', node: 'Node', usages: 'Usages', impact: 'Impact', copy: 'Copy' };
const SEARCH_DEBOUNCE_MS = 150;
const SECTION_ORDER = ['Action', 'Recents', 'Entries', 'Nodes'] as const;

interface OmniItem {
  readonly label: string;
  readonly sub?: string;
  readonly section: (typeof SECTION_ORDER)[number];
  readonly icon: string;
  /** Present => this item is a graph node/entry and supports verb-cycling. */
  readonly nodeId?: string;
  /** Trace focus string (entries use `GET /route` form; nodes trace by their own id). */
  readonly focus?: string;
  /** Action/Recents items run themselves directly — verbs don't apply to them. */
  readonly run?: () => void;
}

/**
 * Omnibox (proposal §2/§8.4) — the single search surface, replacing the old command
 * Palette. Ctrl+K/Ctrl+P open it. Sections merge Actions · Recents · Entries · Nodes
 * (§6); Tab cycles a verb (Trace · Node · Usages · Copy, §8.4) applied to whichever
 * entry/node row is selected. Static sections (Action/Recents) are one computed that
 * only depends on session/recents — never rebuilt per keystroke (GAP-C2); Entries/Nodes
 * are separate computeds that do depend on the query.
 */
@Component({
  selector: 'app-omnibox',
  imports: [FormsModule, KindIcon],
  template: `
    @if (open()) {
      <div
        class="fixed inset-0 z-50"
        (click)="close()"
        (keydown.escape)="close()"
        tabindex="0"
        role="dialog"
        aria-modal="true"
      >
        <div class="absolute inset-0 bg-base/70"></div>
        <div
          class="overlay-float absolute left-1/2 top-[15%] w-[560px] max-h-[420px] -translate-x-1/2 overflow-hidden"
          (click)="$event.stopPropagation()"
          (keydown)="$event.stopPropagation()"
          tabindex="-1"
        >
          <input
            #searchInput
            class="w-full border-b border-line bg-transparent px-4 py-3 text-sm text-ink outline-none placeholder:text-ink-subtle"
            placeholder="Search entries, nodes, or type a command…"
            [ngModel]="query()"
            (ngModelChange)="onQuery($event)"
            (keydown)="onKey($event)"
          />

          @if (selected(); as sel) {
            @if (sel.nodeId) {
              <div class="flex items-center gap-1 border-b border-line px-3 py-1.5">
                @for (v of verbs; track v; let i = $index) {
                  <span class="chip" [class.active]="verbIndex() === i">{{ verbLabel(v) }}</span>
                }
                <span class="ml-auto text-2xs text-ink-subtle"><span class="kbd">Tab</span> cycles</span>
              </div>
            }
          }

          <div class="max-h-[340px] overflow-y-auto py-1">
            @for (section of sections(); track section.name) {
              <p class="px-4 pt-2 pb-1 text-2xs uppercase tracking-wider text-ink-subtle">{{ section.name }}</p>
              @for (item of section.items; track item.label + (item.nodeId ?? '')) {
                <button
                  type="button"
                  class="flex w-full items-center gap-2 px-4 py-1.5 text-left hover:bg-hover"
                  [class.bg-hover]="isSelected(item)"
                  (click)="execute(item)"
                  (mouseenter)="hover(item)"
                >
                  <app-kind-icon [kind]="item.icon" [size]="14" class="shrink-0 text-ink-subtle" />
                  <span class="min-w-0 flex-1 truncate text-xs text-ink">{{ item.label }}</span>
                  @if (item.sub) {
                    <span class="max-w-[40%] shrink-0 truncate font-mono text-2xs text-ink-subtle">{{ item.sub }}</span>
                  }
                </button>
              }
            } @empty {
              <div class="px-4 py-6 text-center text-xs text-ink-subtle">
                @if (query().trim()) {
                  search the graph for '{{ query().trim() }}'
                } @else {
                  Type to search, or pick an action.
                }
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
})
export class Omnibox {
  private readonly api = inject(DevContextApi);
  private readonly session = inject(SessionStore);
  private readonly traceStore = inject(TraceStore);
  private readonly atlasStore = inject(AtlasStore);
  private readonly workspace = inject(WorkspaceStore);
  private readonly recentStore = inject(RecentStore);
  private readonly prefs = inject(PrefsStore);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly verbs = VERBS;

  readonly open = signal(false);
  readonly query = signal('');
  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');
  private readonly selectedIndex = signal(0);
  protected readonly verbIndex = signal(0);
  private readonly searchResults = signal<readonly { nodeId: string; title: string; kind: string }[]>([]);

  private readonly searchGate = new LatestGate();
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  /** Static: only depends on recents (never rebuilt per keystroke — GAP-C2). */
  private readonly staticItems = computed<readonly OmniItem[]>(() => {
    const items: OmniItem[] = [
      { label: 'Analyze repo…', sub: 'Local path or URL', section: 'Action', icon: 'play', run: () => this.router.navigate(['/']) },
      { label: 'Go to Workbench', section: 'Action', icon: 'layers', run: () => this.router.navigate(['/explore']) },
      { label: 'Go to Atlas', section: 'Action', icon: 'boxes', run: () => this.router.navigate(['/atlas']) },
      { label: 'Go to Insights', section: 'Action', icon: 'zap', run: () => this.router.navigate(['/insights']) },
      { label: 'Go to Settings', section: 'Action', icon: 'settings', run: () => this.router.navigate(['/settings']) },
    ];
    for (const r of this.recentStore.recents().slice(0, 5)) {
      items.push({ label: r.label, sub: r.path, section: 'Recents', icon: 'folder-open', run: () => this.openRecent(r.path) });
    }
    return items;
  });

  private readonly entryItems = computed<readonly OmniItem[]>(() => {
    const q = this.query().trim().toLowerCase();
    const items: OmniItem[] = [];
    for (const g of this.session.entryGroups()) {
      for (const e of g.entries) {
        if (items.length >= 10) return items;
        if (q && !e.title.toLowerCase().includes(q) && !e.focus.toLowerCase().includes(q)) continue;
        items.push({ label: e.title, sub: e.focus, section: 'Entries', icon: e.kind, nodeId: e.nodeId, focus: e.focus });
      }
    }
    return items;
  });

  private readonly nodeItems = computed<readonly OmniItem[]>(() =>
    this.searchResults().map((n) => ({
      label: n.title || n.nodeId,
      sub: n.nodeId,
      section: 'Nodes',
      icon: KIND_ICONS[n.kind] ? n.kind : 'code',
      nodeId: n.nodeId,
      focus: n.nodeId,
    })),
  );

  /** Sectioned for the template, in a fixed order, empty sections dropped. Static items are
   * filtered by the query here (cheap: at most a handful of rows) — the expensive part
   * (building the list from recents) stays memoized in `staticItems` (GAP-C2). */
  protected readonly sections = computed<readonly { name: string; items: readonly OmniItem[] }[]>(() => {
    const q = this.query().trim().toLowerCase();
    const statics = q ? this.staticItems().filter((i) => i.label.toLowerCase().includes(q)) : this.staticItems();
    const all = [...statics, ...this.entryItems(), ...this.nodeItems()];
    return SECTION_ORDER.map((name) => ({ name, items: all.filter((i) => i.section === name) })).filter(
      (s) => s.items.length > 0,
    );
  });

  private readonly flat = computed<readonly OmniItem[]>(() => this.sections().flatMap((s) => s.items));

  protected readonly selected = computed<OmniItem | null>(() => this.flat()[this.selectedIndex()] ?? null);

  @HostListener('window:keydown', ['$event'])
  onGlobalKey(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'p')) {
      e.preventDefault();
      this.open.set(true);
      this.query.set('');
      this.selectedIndex.set(0);
      this.verbIndex.set(0);
      this.searchResults.set([]);
      // The input doesn't exist in the DOM until the `@if (open())` block renders —
      // defer the focus one macrotask so the overlay is up first. Without this the
      // overlay opens but keystrokes land wherever focus already was (or nowhere).
      setTimeout(() => this.searchInput()?.nativeElement.focus());
    }
  }

  close(): void {
    this.open.set(false);
    if (this.searchTimer !== null) clearTimeout(this.searchTimer);
  }

  onQuery(val: string): void {
    this.query.set(val);
    this.selectedIndex.set(0);

    if (this.searchTimer !== null) clearTimeout(this.searchTimer);
    const handle = this.session.handle();
    if (!handle || val.trim().length < 2) {
      this.searchResults.set([]);
      return;
    }
    this.searchTimer = setTimeout(() => {
      this.searchTimer = null;
      void this.runSearch(handle, val.trim());
    }, SEARCH_DEBOUNCE_MS);
  }

  private async runSearch(handle: string, query: string): Promise<void> {
    try {
      const res = await this.searchGate.run('search', () => this.api.searchNodes(handle, query, 8));
      if (isStale(res)) return;
      this.searchResults.set(res.nodes.map((n) => ({ nodeId: n.nodeId, title: n.title, kind: n.kind })));
    } catch {
      /* non-critical: search suggestions are supplementary, silent failure is OK */
    }
  }

  onKey(e: KeyboardEvent): void {
    switch (e.key) {
      case 'Escape':
        // The panel wrapper stops keydown propagation (so global shortcuts don't leak
        // through while typing), which would otherwise swallow the overlay's own
        // (keydown.escape) before it bubbles — handle it right here instead.
        this.close();
        break;
      case 'ArrowDown':
        e.preventDefault();
        this.selectedIndex.update((i) => Math.min(i + 1, this.flat().length - 1));
        break;
      case 'ArrowUp':
        e.preventDefault();
        this.selectedIndex.update((i) => Math.max(i - 1, 0));
        break;
      case 'Tab':
        e.preventDefault();
        this.verbIndex.update((i) => (i + 1) % VERBS.length);
        break;
      case 'Enter': {
        const item = this.selected();
        if (item) this.execute(item);
        break;
      }
    }
  }

  protected isSelected(item: OmniItem): boolean {
    return this.selected() === item;
  }

  protected hover(item: OmniItem): void {
    const idx = this.flat().indexOf(item);
    if (idx !== -1) this.selectedIndex.set(idx);
  }

  protected verbLabel(v: Verb): string {
    return VERB_LABELS[v];
  }

  protected execute(item: OmniItem): void {
    if (item.run) {
      item.run();
      this.close();
      return;
    }
    if (!item.nodeId) return;

    const verb = VERBS[this.verbIndex() % VERBS.length];
    switch (verb) {
      case 'trace': {
        const handle = this.session.handle();
        if (handle) void this.traceStore.trace(handle, item.focus ?? item.nodeId);
        void this.router.navigate(['/explore']);
        break;
      }
      case 'node':
        void this.traceStore.selectNode(item.nodeId, 'out');
        void this.router.navigate(['/explore'], { queryParams: { view: 'node' } });
        break;
      case 'usages':
        void this.traceStore.selectNode(item.nodeId, 'usages');
        void this.router.navigate(['/explore'], { queryParams: { view: 'node' } });
        break;
      case 'impact': {
        // §3.4 — lands on the same Inspector line the persistent lens shows, plus an
        // instant toast so the count doesn't wait on the Details section to render.
        void this.traceStore.selectNode(item.nodeId, 'usages');
        void this.router.navigate(['/explore'], { queryParams: { view: 'node' } });
        const count = this.atlasStore.reachedBy(item.nodeId).length;
        this.toast.show(`Reached by ${count} flow${count === 1 ? '' : 's'}`, 'info');
        break;
      }
      case 'copy':
        void copyToClipboard(item.nodeId);
        break;
    }
    this.close();
  }

  /** Jump straight to a specific past repo (what the titlebar's recents dropdown does —
   * the old Palette couldn't, GAP-B3). */
  private openRecent(path: string): void {
    const label = path.split(/[\\/]/).pop() || path;
    this.workspace.createTab(path, label);
    const defs = this.prefs.analyzeDefaults();
    const spec: AnalyzeSpec = { path, depth: defs.depth, detail: defs.detail, noRoslyn: defs.noRoslyn, cleanup: defs.cleanup };
    void this.session.analyze(spec);
  }
}
