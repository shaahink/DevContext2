import { Component, DestroyRef, effect, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';

import { SessionStore } from '../state/session.store';
import { type TabState, WorkspaceStore } from '../state/workspace.store';

/**
 * The 32px multi-tab strip (I10) — sits under the header, to the right of the icon rail. Up to
 * WorkspaceStore.MAX_TABS independent repo sessions; switching rewrites the URL to that tab's last
 * route (replaceUrl, tab identity itself never appears in the URL).
 *
 * Reduced v1 (documented in ITERATION-I10-workspace-tabs.md §2): no server-side MaxLiveSessions/LRU
 * rehydration yet (that needs I8's snapshot cache, not built) — the tab cap alone keeps this safe at
 * small scale. No drag-reorder (spec marks it optional for v1).
 */
@Component({
  selector: 'app-tab-strip',
  imports: [],
  template: `
    <div class="relative flex items-stretch border-b border-line bg-surface" style="height:32px;min-height:32px">
      @if (confirmCloseId()) {
        <div class="absolute inset-0 z-10 flex items-center gap-2 px-3 text-xs bg-surface" tabindex="-1" (keydown.escape)="cancelConfirmClose()">
          <span class="text-ink">Cancel analysis of <span class="font-mono text-ink">{{ confirmCloseLabel() }}</span>?</span>
          <button type="button" class="rounded px-1.5 py-0.5 text-xs text-danger hover:bg-danger/10" (click)="confirmCloseTab()">Cancel analysis</button>
          <button type="button" class="rounded px-1.5 py-0.5 text-xs text-ink-muted hover:bg-hover hover:text-ink" (click)="cancelConfirmClose()">Keep</button>
        </div>
      }
      <div class="flex flex-1 items-stretch overflow-x-auto">
        @for (tab of workspace.tabs(); track tab.id) {
          <div
            class="group flex min-w-0 max-w-48 cursor-pointer items-center gap-1.5 border-r border-line px-2.5 text-xs transition-colors"
            [class.bg-surface-2]="tab.id === workspace.activeId()"
            [class.text-ink]="tab.id === workspace.activeId()"
            [class.text-ink-subtle]="tab.id !== workspace.activeId()"
            [class.hover:bg-surface-2]="tab.id !== workspace.activeId()"
            [class.border-b-2]="tab.id === workspace.activeId()"
            [class.border-b-accent]="tab.id === workspace.activeId()"
            [title]="tab.path || tab.label"
            role="tab"
            tabindex="0"
            [attr.aria-selected]="tab.id === workspace.activeId()"
            (click)="switchTo(tab.id)"
            (keydown.enter)="switchTo(tab.id)"
            (auxclick)="onAuxClick(tab.id, $event)"
          >
            @if (dotClass(tab); as dc) {
              <span class="h-1.5 w-1.5 shrink-0 rounded-full" [class]="dc"></span>
            }
            <span class="min-w-0 flex-1 truncate font-mono">{{ shortLabel(tab.label) }}</span>
            <button
              type="button"
              class="shrink-0 rounded px-1 text-ink-subtle opacity-0 hover:bg-surface hover:text-ink group-hover:opacity-100"
              [class.opacity-100]="tab.id === workspace.activeId()"
              (click)="closeTab(tab.id, $event)"
              [title]="'Close ' + tab.label"
            >✕</button>
          </div>
        }
      </div>
      <button
        class="shrink-0 border-l border-line px-3 text-xs text-ink-subtle transition-colors hover:bg-surface-2 hover:text-ink disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent"
        [disabled]="workspace.atCap()"
        [title]="workspace.atCap() ? ('Tab limit (' + maxTabs + ') — close one to open another') : 'New tab (Ctrl+T)'"
        (click)="newTab()"
      >+</button>
    </div>
  `,
})
export class TabStrip {
  protected readonly workspace = inject(WorkspaceStore);
  protected readonly maxTabs = WorkspaceStore.MAX_TABS;
  private readonly router = inject(Router);
  private readonly session = inject(SessionStore);

  constructor() {
    const sub = this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe((e) => {
      const activeId = this.workspace.activeId();
      if (activeId) this.workspace.setRoute(activeId, e.urlAfterRedirects);
    });
    inject(DestroyRef).onDestroy(() => sub.unsubscribe());

    // On boot, restored tabs land on screen idle (I10.4 — never auto-analyze all of them). If the
    // one that was active when the app last closed carries a path, jump straight to its remembered
    // route and lazily re-analyze it (below) — the OTHER restored tabs stay untouched until clicked.
    const initialActive = this.workspace.activeTab();
    if (initialActive?.path && this.router.url === '/') {
      void this.router.navigateByUrl(initialActive.route || '/', { replaceUrl: true });
    }

    // The one reactive rule behind "first activation analyzes lazily": whenever the active tab is
    // idle with a remembered path but no handle yet (a restored tab, or one just switched into),
    // kick off its analysis automatically.
    effect(() => {
      const tab = this.workspace.activeTab();
      if (tab && tab.session.status === 'idle' && tab.path && !tab.session.handle) {
        void this.session.analyze({ path: tab.path });
      }
    });
  }

  @HostListener('window:keydown', ['$event'])
  onGlobalKey(e: KeyboardEvent): void {
    if (!(e.ctrlKey || e.metaKey)) return;

    if (e.key === 'Tab') {
      // MRU cycle (GAP-T5) — mru[0] is always the current tab, so the "next" tab to
      // cycle TO is mru[1]; Shift+Tab goes the other way, to the least-recently-used.
      const mru = this.workspace.mru();
      if (mru.length < 2) return;
      e.preventDefault();
      const targetId = e.shiftKey ? mru[mru.length - 1] : mru[1];
      this.switchTo(targetId);
      return;
    }

    if (e.key === 't') {
      e.preventDefault();
      this.newTab();
    } else if (e.key === 'w') {
      const activeId = this.workspace.activeId();
      if (activeId) {
        e.preventDefault();
        this.closeTab(activeId);
      }
    } else if (/^[1-6]$/.test(e.key)) {
      const idx = Number(e.key) - 1;
      const tab = this.workspace.tabs()[idx];
      if (tab) {
        e.preventDefault();
        this.switchTo(tab.id);
      }
    }
  }

  protected switchTo(id: string): void {
    if (id === this.workspace.activeId()) return;
    this.workspace.setActive(id);
    const tab = this.workspace.tabById(id);
    void this.router.navigateByUrl(tab?.route || '/', { replaceUrl: true });
  }

  protected onAuxClick(id: string, event: MouseEvent): void {
    if (event.button === 1) this.closeTab(id, event); // middle-click closes
  }

  protected readonly confirmCloseId = signal<string | null>(null);

  protected confirmCloseLabel(): string {
    const id = this.confirmCloseId();
    if (!id) return '';
    return this.workspace.tabById(id)?.label || '';
  }

  protected confirmCloseTab(): void {
    const id = this.confirmCloseId();
    this.confirmCloseId.set(null);
    if (id) this.executeCloseTab(id);
  }

  protected cancelConfirmClose(): void {
    this.confirmCloseId.set(null);
  }

  protected closeTab(id: string, event?: Event): void {
    event?.stopPropagation();
    const tab = this.workspace.tabById(id);
    if (tab && (tab.session.status === 'analyzing' || tab.session.status === 'cloning')) {
      this.confirmCloseId.set(id);
      return;
    }
    this.executeCloseTab(id);
  }

  private executeCloseTab(id: string): void {
    const wasActive = this.workspace.activeId() === id;
    this.workspace.closeTab(id);
    if (wasActive) {
      const next = this.workspace.activeTab();
      void this.router.navigateByUrl(next?.route || '/', { replaceUrl: true });
    }
  }

  protected newTab(): void {
    if (this.workspace.atCap()) return;
    this.workspace.createTab('', 'New tab');
    void this.router.navigateByUrl('/');
  }

  protected shortLabel(label: string): string {
    const last = label.split(/[\\/]/).pop() || label;
    return last.length > 18 ? last.slice(0, 17) + '…' : last;
  }

  protected dotClass(tab: TabState): string | null {
    const status = tab.session.status;
    if (status === 'analyzing' || status === 'cloning') return 'bg-accent animate-pulse';
    if (status === 'error') return 'bg-danger';
    return null;
  }
}
