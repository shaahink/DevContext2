import { Component, computed, inject, input, signal } from '@angular/core';
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
    __TAURI__?: unknown;
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
  return typeof window !== 'undefined' && window.__TAURI__ !== undefined;
}

@Component({
  selector: 'app-header',
  imports: [Icon],
  template: `
    <header
      class="fixed top-0 z-40 flex h-11 w-full items-center border-b border-line/50 px-3 transition-all duration-300 select-none shadow-sm"
      [class.bg-base]="!transparent()"
      [class.bg-base/80]="transparent()"
      [class.backdrop-blur-lg]="transparent()"
      [class.shadow-sm]="!transparent()"
      [class.shadow-none]="transparent()"
    >
      <div class="flex items-center gap-3" data-tauri-drag-region style="flex:1; height:100%; display:flex; align-items:center;">
        <span
          class="flex cursor-pointer items-center gap-1.5 rounded font-mono text-sm font-semibold tracking-tight text-ink no-drag"
          (click)="navigateHome()"
          (keydown.enter)="navigateHome()"
          tabindex="0"
          role="button"
        >
          <span class="text-accent">&diams;</span>
          <span>DevContext</span>
        </span>
      </div>

      <div class="flex flex-1 items-center justify-center pointer-events-auto">
        @if (session.busy() || session.ready()) {
          <div class="relative">
            <button
              class="flex items-center gap-1.5 rounded px-2 py-1 text-xs text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors"
              (click)="repoMenuOpen.set(!repoMenuOpen())"
            >
              <app-icon name="folder-open" [size]="12" />
              <span class="max-w-[200px] truncate font-mono">{{ repoLabel() }}</span>
              <svg width="10" height="6" viewBox="0 0 10 6" class="transition-transform" [class.rotate-180]="repoMenuOpen()"><path d="M1 1l4 4 4-4" stroke="currentColor" stroke-width="1.2" fill="none" stroke-linecap="round"/></svg>
            </button>
            @if (repoMenuOpen()) {
              <div class="fixed inset-0 z-30" (click)="repoMenuOpen.set(false)" (keydown.escape)="repoMenuOpen.set(false)" role="dialog" tabindex="0"></div>
              <div class="absolute top-full left-1/2 -translate-x-1/2 mt-1 w-72 rounded border border-line bg-elevated shadow-xl z-40 overflow-hidden" (keydown.escape)="repoMenuOpen.set(false)" tabindex="0">
                <div class="px-3 py-2 border-b border-line">
                  <p class="text-2xs text-ink-subtle uppercase mb-0.5">Current</p>
                  <p class="text-xs font-mono text-ink truncate">{{ repoLabel() }}</p>
                </div>
                @if (recents().length) {
                  <div class="border-b border-line py-1">
                    <p class="px-3 py-1 text-2xs text-ink-subtle uppercase">Recent</p>
                    @for (r of recents(); track r.path) {
                      <button
                        class="flex w-full items-center gap-2 rounded-none px-3 py-1.5 text-xs text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors text-left"
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
                    class="flex w-full items-center gap-2 px-3 py-1.5 text-xs text-accent hover:bg-surface-2 transition-colors text-left"
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
      </div>

      <div class="flex items-center gap-2 pointer-events-auto">
        @if (session.ready()) {
          <button
            class="flex cursor-pointer items-center gap-1 rounded px-2 py-1 text-2xs text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors"
            (click)="newAnalysis()"
            title="New analysis"
          >
            <app-icon name="play" [size]="11" /> New
          </button>
        }
        <span
          class="flex items-center gap-1.5 text-2xs text-ink-muted"
          [class.text-success]="connection.online()"
          [class.text-danger]="connection.checked() && !connection.online()"
          [class.text-ink-muted]="!connection.checked()"
        >
          <span
            class="inline-block h-1.5 w-1.5 rounded-full"
            [class.bg-success]="connection.online()"
            [class.bg-danger]="connection.checked() && !connection.online()"
            [class.bg-ink-muted]="!connection.checked()"
          ></span>
          {{ connection.checked() ? (connection.online() ? 'Connected' : 'Offline') : '...' }}
        </span>

        @if (isTauri()) {
        <div class="flex items-center -mr-1">
          <button class="flex h-7 w-9 cursor-pointer items-center justify-center rounded text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors" (click)="minimize()" title="Minimize">
            <svg width="10" height="1" viewBox="0 0 10 1"><rect width="10" height="1" fill="currentColor"/></svg>
          </button>
          <button class="flex h-7 w-9 cursor-pointer items-center justify-center rounded text-ink-muted hover:bg-surface-2 hover:text-ink transition-colors" (click)="toggleMaximize()" title="Maximize">
            <svg width="10" height="10" viewBox="0 0 10 10"><rect x="1" y="1" width="8" height="8" fill="none" stroke="currentColor" stroke-width="1.2"/></svg>
          </button>
          <button class="flex h-7 w-9 cursor-pointer items-center justify-center rounded text-ink-muted hover:bg-danger hover:text-ink transition-colors" (click)="closeWindow()" title="Close">
            <svg width="10" height="10" viewBox="0 0 10 10"><line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.2"/><line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.2"/></svg>
          </button>
        </div>
        }
      </div>
    </header>
  `,
  host: { class: 'contents' },
})
export class AppHeader {
  readonly transparent = input(false);
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
