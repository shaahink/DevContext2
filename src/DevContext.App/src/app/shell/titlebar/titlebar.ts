import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ConnectionStore } from '../../state/connection.store';
import { SessionStore } from '../../state/session.store';
import { WorkspaceStore } from '../../state/workspace.store';
import { RecentStore } from '../../state/recent.store';
import { PrefsStore } from '../../state/prefs.store';
import type { AnalyzeSpec } from '../../data-access/devcontext-api';
import { Icon } from '../../ui/icon/icon';

declare global {
  interface Window {
    __TAURI_INTERNALS__?: unknown;
  }
}

let tauriWindowApi: {
  getCurrentWindow(): { minimize(): Promise<void>; maximize(): Promise<void>; unmaximize(): Promise<void>; isMaximized(): Promise<boolean>; close(): Promise<void> };
} | null = null;

async function loadTauriWindowApi(): Promise<typeof tauriWindowApi> {
  if (tauriWindowApi) return tauriWindowApi;
  try {
    tauriWindowApi = await import('@tauri-apps/api/window');
    return tauriWindowApi;
  } catch {
    return null;
  }
}

function isTauri(): boolean {
  return typeof window !== 'undefined' && window.__TAURI_INTERNALS__ !== undefined;
}

/**
 * TitleBar (proposal §8.1, 30px) — renamed/restyled from the old 44px app-header. Solid
 * `bg-base`, no blur/shadow (proposal §4.4: resting chrome never floats). Drag-region
 * hygiene (§7.2): the three top-level strips (brand, search/repo, status/window-controls)
 * carry `data-tauri-drag-region`; Tauri only starts a drag when the mousedown's exact
 * event target is the tagged element itself (not a descendant), so real buttons/inputs
 * stay fully clickable and only each strip's bare flex background is draggable.
 *
 * The title bar's search field is a TRIGGER that dispatches the same Ctrl+K keydown
 * `Omnibox` listens for globally, so no direct coupling to Omnibox is needed here. The
 * repo-label dropdown (recents + New analysis) is preserved from the old header for a
 * one-click jump to a specific past repo without opening the omnibox first.
 */
@Component({
  selector: 'app-titlebar',
  imports: [Icon],
  template: `
    <header class="flex h-[30px] w-full shrink-0 items-center border-b border-line bg-base px-2 select-none">
      <div class="flex h-full items-center gap-1.5" data-tauri-drag-region>
        <span
          class="flex cursor-pointer items-center gap-1.5 px-1 font-mono text-xs font-semibold tracking-tight text-ink"
          (click)="navigateHome()"
          (keydown.enter)="navigateHome()"
          tabindex="0"
          role="button"
        >
          <span class="text-accent">&diams;</span>
          <span>DevContext</span>
        </span>
      </div>

      <div class="flex flex-1 items-center justify-center gap-1 px-2" data-tauri-drag-region>
        @if (session.busy() || session.ready()) {
          <div class="relative">
            <button
              type="button"
              class="flex items-center gap-1.5 rounded-sm px-2 py-0.5 text-2xs text-ink-muted transition-colors hover:bg-hover hover:text-ink"
              (click)="repoMenuOpen.set(!repoMenuOpen())"
            >
              <app-icon name="folder-open" [size]="11" />
              <span class="max-w-[180px] truncate font-mono">{{ repoLabel() }}</span>
              <svg width="9" height="5" viewBox="0 0 10 6" class="transition-transform" [class.rotate-180]="repoMenuOpen()"><path d="M1 1l4 4 4-4" stroke="currentColor" stroke-width="1.2" fill="none" stroke-linecap="round"/></svg>
            </button>
            @if (repoMenuOpen()) {
              <div class="fixed inset-0 z-30" (click)="repoMenuOpen.set(false)" (keydown.escape)="repoMenuOpen.set(false)" role="dialog" tabindex="0"></div>
              <div class="overlay-float absolute left-1/2 top-full z-40 mt-1 w-72 -translate-x-1/2 overflow-hidden" (keydown.escape)="repoMenuOpen.set(false)" tabindex="0">
                <div class="border-b border-line px-3 py-2">
                  <p class="mb-0.5 text-2xs uppercase text-ink-subtle">Current</p>
                  <p class="truncate font-mono text-xs text-ink">{{ repoLabel() }}</p>
                </div>
                @if (recents().length) {
                  <div class="border-b border-line py-1">
                    <p class="px-3 py-1 text-2xs uppercase text-ink-subtle">Recent</p>
                    @for (r of recents(); track r.path) {
                      <button
                        type="button"
                        class="flex w-full items-center gap-2 px-3 py-1.5 text-left text-xs text-ink-muted transition-colors hover:bg-hover hover:text-ink"
                        (click)="selectRecent(r.path)"
                      >
                        <app-icon name="folder-open" [size]="11" />
                        <span class="truncate font-mono">{{ r.label }}</span>
                      </button>
                    }
                  </div>
                }
                <div class="py-1">
                  <button
                    type="button"
                    class="flex w-full items-center gap-2 px-3 py-1.5 text-left text-xs text-accent transition-colors hover:bg-hover"
                    (click)="newAnalysis()"
                  >
                    <app-icon name="play" [size]="11" />
                    New analysis…
                  </button>
                </div>
              </div>
            }
          </div>
        }
        <button
          type="button"
          class="flex min-w-0 max-w-64 flex-1 items-center gap-1.5 rounded-sm border border-line px-2 py-0.5 text-2xs text-ink-subtle transition-colors hover:border-line-strong hover:text-ink-muted"
          (click)="openOmnibox()"
          title="Search or jump (Ctrl+K)"
        >
          <app-icon name="search" [size]="11" class="shrink-0" />
          <span class="min-w-0 flex-1 truncate text-left">search or jump…</span>
          <span class="kbd shrink-0">Ctrl+K</span>
        </button>
      </div>

      <div class="flex h-full items-center gap-2" data-tauri-drag-region>
        @if (session.ready()) {
          <button
            type="button"
            class="flex cursor-pointer items-center gap-1 rounded-sm px-1.5 py-0.5 text-2xs text-ink-muted transition-colors hover:bg-hover hover:text-ink"
            (click)="newAnalysis()"
            title="New analysis"
          >
            <app-icon name="play" [size]="10" /> New
          </button>
        }
        <span
          class="flex items-center gap-1 text-2xs"
          [class.text-success]="connection.online()"
          [class.text-danger]="connection.checked() && !connection.online()"
          [class.text-ink-subtle]="!connection.checked()"
          [title]="connection.checked() ? (connection.online() ? 'Connected' : 'Offline') : 'Checking…'"
        >
          <span
            class="inline-block h-1.5 w-1.5 rounded-full"
            [class.bg-success]="connection.online()"
            [class.bg-danger]="connection.checked() && !connection.online()"
            [class.bg-ink-subtle]="!connection.checked()"
          ></span>
        </span>

        @if (isTauri()) {
          <div class="flex items-center">
            <button type="button" class="flex h-[30px] w-8 cursor-pointer items-center justify-center text-ink-muted transition-colors hover:bg-hover hover:text-ink" (click)="minimize()" title="Minimize">
              <svg width="10" height="1" viewBox="0 0 10 1"><rect width="10" height="1" fill="currentColor"/></svg>
            </button>
            <button type="button" class="flex h-[30px] w-8 cursor-pointer items-center justify-center text-ink-muted transition-colors hover:bg-hover hover:text-ink" (click)="toggleMaximize()" title="Maximize">
              <svg width="10" height="10" viewBox="0 0 10 10"><rect x="1" y="1" width="8" height="8" fill="none" stroke="currentColor" stroke-width="1.2"/></svg>
            </button>
            <button type="button" class="flex h-[30px] w-8 cursor-pointer items-center justify-center text-ink-muted transition-colors hover:bg-danger hover:text-ink" (click)="closeWindow()" title="Close">
              <svg width="10" height="10" viewBox="0 0 10 10"><line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.2"/><line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.2"/></svg>
            </button>
          </div>
        }
      </div>
    </header>
  `,
  host: { class: 'contents' },
})
export class Titlebar {
  protected readonly connection = inject(ConnectionStore);
  protected readonly session = inject(SessionStore);
  private readonly workspace = inject(WorkspaceStore);
  private readonly recentStore = inject(RecentStore);
  private readonly prefs = inject(PrefsStore);
  private readonly router = inject(Router);

  protected readonly recents = this.recentStore.recents;
  protected readonly repoMenuOpen = signal(false);
  protected readonly repoLabel = computed(() => {
    const s = this.session.summary();
    if (s?.label) return s.label;
    return 'Analyzing…';
  });

  navigateHome(): void {
    void this.router.navigateByUrl('/');
  }

  protected newAnalysis(): void {
    this.repoMenuOpen.set(false);
    this.session.cancel();
    const tabId = this.workspace.activeId();
    if (tabId) this.workspace.closeTab(tabId);
    void this.router.navigateByUrl('/');
  }

  protected selectRecent(path: string): void {
    this.repoMenuOpen.set(false);
    this.session.cancel();
    const tabId = this.workspace.activeId();
    if (tabId) this.workspace.closeTab(tabId);
    const defs = this.prefs.analyzeDefaults();
    const spec: AnalyzeSpec = { path, depth: defs.depth, detail: defs.detail, noRoslyn: defs.noRoslyn, cleanup: defs.cleanup };
    void this.session.analyze(spec);
  }

  /** Dispatches the same Ctrl+K keydown `Omnibox` listens for globally. */
  protected openOmnibox(): void {
    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true }));
  }

  protected isTauri = isTauri;

  protected async minimize(): Promise<void> {
    const api = await loadTauriWindowApi();
    if (api) await api.getCurrentWindow().minimize();
  }

  protected async toggleMaximize(): Promise<void> {
    const api = await loadTauriWindowApi();
    if (!api) return;
    const win = api.getCurrentWindow();
    if (await win.isMaximized()) await win.unmaximize();
    else await win.maximize();
  }

  protected async closeWindow(): Promise<void> {
    const api = await loadTauriWindowApi();
    if (api) await api.getCurrentWindow().close();
  }
}
