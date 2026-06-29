import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import {
  Component,
  computed,
  effect,
  inject,
  Injectable,
  input,
  model,
  output,
  signal,
  type Signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-command-palette-overlay',
  template: `
    <div
      class="flex flex-col rounded-lg border border-line bg-elevated shadow-2xl"
      style="width: 560px; max-height: 360px"
    >
      <div class="flex items-center gap-2 border-b border-line px-3 py-2">
        <svg class="h-4 w-4 shrink-0 text-ink-subtle" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" />
        </svg>
        <input
          #input
          class="flex-1 bg-transparent text-sm text-ink outline-none placeholder:text-ink-subtle"
          [placeholder]="placeholder()"
          [value]="query()"
          (input)="onQuery($event)"
          (keydown)="onKey($event)"
        />
        @if (searching()) {
          <div class="flex items-center text-ink-subtle">
            <svg class="h-3.5 w-3.5 animate-spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
              <circle cx="12" cy="12" r="10" stroke-opacity="0.25" /><path d="M12 2a10 10 0 0 1 10 10" />
            </svg>
          </div>
        }
      </div>
      <div class="min-h-0 flex-1 overflow-y-auto p-1">
        @for (item of allItems(); track item.id; let idx = $index) {
          <button
            (click)="select(item)"
            (mouseenter)="activeIndex.set(idx)"
            class="flex w-full items-center gap-3 rounded px-3 py-2 text-left text-xs hover:bg-surface-2"
            [class.bg-surface-2]="idx === activeIndex()"
          >
            <span class="text-ink">{{ item.label }}</span>
            @if (item.detail) {
              <span class="truncate text-ink-subtle">{{ item.detail }}</span>
            }
            @if (item.badge) {
              <span class="ml-auto rounded bg-surface-2 px-1 py-0.5 text-[9px] text-ink-muted">{{ item.badge }}</span>
            }
          </button>
        } @empty {
          @if (searching()) {
            <p class="px-3 py-6 text-center text-xs text-ink-subtle">Searching…</p>
          } @else {
            <p class="px-3 py-6 text-center text-xs text-ink-subtle">No results. Try searching for types, methods, or entries.</p>
          }
        }
      </div>
      <div class="flex items-center gap-3 border-t border-line px-3 py-1.5 text-[10px] text-ink-subtle">
        <span class="flex items-center gap-1">
          <kbd class="rounded border border-line bg-surface-2 px-1 py-0.5 font-mono">&uarr;&darr;</kbd> navigate
        </span>
        <span class="flex items-center gap-1">
          <kbd class="rounded border border-line bg-surface-2 px-1 py-0.5 font-mono">Enter</kbd> select
        </span>
        <span class="flex items-center gap-1">
          <kbd class="rounded border border-line bg-surface-2 px-1 py-0.5 font-mono">Esc</kbd> close
        </span>
      </div>
    </div>
  `,
})
export class CommandPaletteOverlay {
  readonly query = model('');
  readonly items = input<readonly PaletteItem[]>([]);
  readonly searchFn = input<((q: string) => Promise<readonly PaletteItem[]>) | null>(null);
  readonly placeholder = input('Search…');
  readonly selected = output<PaletteItem>();
  readonly dismissed = output<void>();

  readonly activeIndex = signal(0);
  readonly remoteResults = signal<readonly PaletteItem[]>([]);
  readonly searching = signal(false);
  private searchId = 0;

  readonly localFiltered = computed(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) return this.items();
    return this.items().filter(
      (i) =>
        i.label.toLowerCase().includes(q) ||
        (i.detail?.toLowerCase().includes(q) ?? false),
    );
  });

  readonly allItems = computed(() => {
    const local = this.localFiltered();
    const remote = this.remoteResults();
    if (!remote.length) return local;
    const seen = new Set(local.map((i) => i.id));
    return [...local, ...remote.filter((r) => !seen.has(r.id))];
  });

  constructor() {
    effect(() => {
      void this.allItems();
      this.activeIndex.set(0);
    });

    effect(() => {
      const fn = this.searchFn();
      const q = this.query().trim();
      if (!fn || !q) {
        this.searchId++;
        this.remoteResults.set([]);
        return;
      }
      const id = ++this.searchId;
      this.searching.set(true);
      const timer = setTimeout(async () => {
        try {
          const results = await fn(q);
          if (id === this.searchId) this.remoteResults.set(results);
        } catch {
          if (id === this.searchId) this.remoteResults.set([]);
        } finally {
          if (id === this.searchId) this.searching.set(false);
        }
      }, 250);
      return () => clearTimeout(timer);
    });
  }

  protected onQuery(e: Event): void {
    this.query.set((e.target as HTMLInputElement).value);
  }

  protected onKey(e: KeyboardEvent): void {
    if (e.key === 'Enter') {
      e.preventDefault();
      const item = this.allItems()[this.activeIndex()];
      if (item) this.selected.emit(item);
      return;
    }
    if (e.key === 'Escape') {
      e.preventDefault();
      this.dismissed.emit();
      return;
    }
    const max = this.allItems().length - 1;
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      this.activeIndex.set(Math.min(this.activeIndex() + 1, max));
    }
    if (e.key === 'ArrowUp') {
      e.preventDefault();
      this.activeIndex.set(Math.max(this.activeIndex() - 1, 0));
    }
  }

  protected select(item: PaletteItem): void {
    this.selected.emit(item);
  }
}

export interface PaletteItem {
  readonly id: string;
  readonly label: string;
  readonly detail?: string;
  readonly badge?: string;
  readonly action: () => void;
}

export interface PaletteSection {
  readonly id: string;
  readonly placeholder: string;
  readonly items: Signal<readonly PaletteItem[]>;
}

@Injectable({ providedIn: 'root' })
export class CommandPaletteService {
  private readonly overlay = inject(Overlay);
  private readonly router = inject(Router);
  private sections = signal<readonly PaletteSection[]>([]);
  private overlayRef: OverlayRef | null = null;
  private searchFn: ((q: string) => Promise<readonly PaletteItem[]>) | null = null;

  constructor() {
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.dismiss());
  }

  register(section: PaletteSection): void {
    this.sections.update((s) => [...s, section]);
  }

  unregister(id: string): void {
    this.sections.update((s) => s.filter((x) => x.id !== id));
  }

  setSearch(fn: (q: string) => Promise<readonly PaletteItem[]>): void {
    this.searchFn = fn;
  }

  toggle(): void {
    if (this.overlayRef) {
      this.dismiss();
    } else {
      this.open();
    }
  }

  open(): void {
    if (this.overlayRef) return;
    const position = this.overlay
      .position()
      .global()
      .centerHorizontally()
      .top('20vh');

    this.overlayRef = this.overlay.create({
      positionStrategy: position,
      hasBackdrop: true,
    });
    this.overlayRef.backdropClick().subscribe(() => this.dismiss());
    this.overlayRef.keydownEvents().subscribe((e) => {
      if (e.key === 'Escape') this.dismiss();
    });

    const allItems = computed(() =>
      this.sections().flatMap((s) => s.items()),
    );

    const portal = new ComponentPortal(CommandPaletteOverlay);
    const ref = this.overlayRef.attach(portal);
    ref.setInput('items', allItems);
    ref.setInput('searchFn', this.searchFn);
    ref.setInput('placeholder', 'Search commands, entries, nodes…');
    ref.instance.selected.subscribe((item) => {
      item.action();
      this.dismiss();
    });
    ref.instance.dismissed.subscribe(() => this.dismiss());
  }

  dismiss(): void {
    if (this.overlayRef) {
      this.overlayRef.dispose();
      this.overlayRef = null;
    }
  }
}
