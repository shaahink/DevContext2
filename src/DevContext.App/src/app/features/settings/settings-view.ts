import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ThemeService } from '../../core/theme/theme.service';
import { ConnectionStore } from '../../state/connection.store';
import { PrefsStore } from '../../state/prefs.store';
import { isTauri } from '../../core/tauri-env';
import { StorageService, type StorageSummary } from '../../core/storage.service';
import { formatBytes } from '../../core/format';

type SettingsTab = 'appearance' | 'analysis' | 'storage' | 'server' | 'about';

@Component({
  selector: 'app-settings-view',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="flex h-full">
      <!-- Left tabs -->
      <nav class="w-40 shrink-0 border-r border-line bg-surface p-3 space-y-1">
        @for (tab of tabs; track tab.key) {
          <button class="w-full rounded px-2 py-1.5 text-left text-xs transition-colors"
                  [class.bg-accent/10]="activeTab() === tab.key" [class.text-accent]="activeTab() === tab.key"
                  [class.text-ink-muted]="activeTab() !== tab.key"
                  [class.hover:bg-surface-2]="activeTab() !== tab.key"
                  (click)="selectTab(tab.key)">{{ tab.label }}</button>
        }
      </nav>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-4">
        <!-- Appearance -->
        @if (activeTab() === 'appearance') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">Appearance</h2>
            <div class="space-y-2">
              <p class="text-2xs text-ink-muted uppercase">Theme / Vibe</p>
              <div class="flex flex-wrap gap-2">
                @for (vibe of theme.vibes(); track vibe.id) {
                  <button class="rounded border px-3 py-2 text-xs transition-colors"
                          [class.border-accent]="theme.vibe() === vibe.id"
                          [class.text-accent]="theme.vibe() === vibe.id"
                          [class.border-line]="theme.vibe() !== vibe.id"
                          [class.text-ink-muted]="theme.vibe() !== vibe.id"
                          (click)="theme.setVibe(vibe.id)">
                    <div class="font-medium">{{ vibe.name }}</div>
                  </button>
                }
              </div>
            </div>
          </section>
        }

        <!-- Analysis -->
        @if (activeTab() === 'analysis') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">Analysis Defaults</h2>
            <p class="text-2xs text-ink-subtle">Applied to every new analysis.</p>
            <div class="space-y-3">
              <div>
                <p class="text-2xs text-ink-muted uppercase">Default depth</p>
                <select class="mt-1 rounded border border-line bg-surface-2 px-2 py-1 text-xs text-ink" [(ngModel)]="depthModel" (ngModelChange)="prefs.setDepth(+$event)">
                  @for (d of [1,2,3,4,5,6,7,8,9,10]; track d) { <option [value]="d">{{ d }}</option> }
                </select>
              </div>
              <div>
                <p class="text-2xs text-ink-muted uppercase">Default detail</p>
                <select class="mt-1 rounded border border-line bg-surface-2 px-2 py-1 text-xs text-ink" [(ngModel)]="detailModel" (ngModelChange)="prefs.setDetail($event)">
                  <option value="salient">Salient</option>
                  <option value="signature">Signature</option>
                  <option value="full">Full</option>
                </select>
              </div>
              <div class="flex items-center gap-2">
                <input type="checkbox" [(ngModel)]="roslynModel" (ngModelChange)="prefs.setUseRoslyn($event)" />
                <span class="text-xs text-ink">Use Roslyn semantic tier</span>
              </div>
              <div class="flex items-center gap-2">
                <input type="checkbox" [(ngModel)]="cleanupModel" (ngModelChange)="prefs.setAutoCleanup($event)" />
                <span class="text-xs text-ink">Auto-cleanup clones</span>
              </div>
            </div>
          </section>
        }

        <!-- Storage -->
        @if (activeTab() === 'storage') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">Storage</h2>
            @if (!isTauriEnv) {
              <p class="text-2xs text-ink-subtle">Real file listing is only available in the desktop app.</p>
            }

            <div>
              <div class="flex items-center justify-between">
                <p class="text-2xs text-ink-muted uppercase">Cache — %LOCALAPPDATA%/DevContext/cache</p>
                @if (isTauriEnv) {
                  <span class="flex items-center gap-2">
                    <button class="text-2xs text-ink-subtle hover:text-ink hover:underline" (click)="openInExplorer('cache')">Open in Explorer</button>
                    <button class="text-2xs text-danger hover:underline disabled:opacity-50" [disabled]="storageLoading()" (click)="clearCache()">Clear</button>
                  </span>
                }
              </div>
              @if (cache(); as c) {
                <p class="text-xs font-mono text-ink mt-1">{{ formatBytes(c.totalBytes) }} across {{ c.entries.length }} repo{{ c.entries.length === 1 ? '' : 's' }}</p>
                @for (e of c.entries; track e.name) {
                  <p class="truncate text-2xs font-mono text-ink-subtle">{{ e.name }} — {{ formatBytes(e.bytes) }}</p>
                }
              } @else if (storageLoading()) {
                <p class="text-2xs text-ink-subtle">Scanning…</p>
              }
            </div>

            <div class="border-t border-line pt-3">
              <div class="flex items-center justify-between">
                <p class="mb-1 text-2xs text-ink-muted uppercase">Repos — %LOCALAPPDATA%/DevContext/repos</p>
                @if (isTauriEnv) {
                  <span class="flex items-center gap-2">
                    <button class="text-2xs text-ink-subtle hover:text-ink hover:underline" (click)="openInExplorer('repos')">Open in Explorer</button>
                    <button class="text-2xs text-danger hover:underline disabled:opacity-50" [disabled]="storageLoading()" (click)="clearRepos()">Clear</button>
                  </span>
                }
              </div>
              @if (repos(); as r) {
                <p class="text-xs font-mono text-ink">{{ formatBytes(r.totalBytes) }} across {{ r.entries.length }} clone{{ r.entries.length === 1 ? '' : 's' }}</p>
                @for (e of r.entries; track e.name) {
                  <p class="truncate text-2xs font-mono text-ink-subtle">{{ e.name }} — {{ formatBytes(e.bytes) }}</p>
                }
              } @else if (storageLoading()) {
                <p class="text-2xs text-ink-subtle">Scanning…</p>
              }
            </div>

            <p class="border-t border-line pt-3 text-2xs text-ink-subtle">
              Snapshot cache (SHA256 + git HEAD keyed, LRU) is managed automatically. GitHub
              clones are removed at session end unless "Keep" cleanup is selected.
            </p>
          </section>
        }

        <!-- Server -->
        @if (activeTab() === 'server') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">Server</h2>
            <div>
              <p class="text-2xs text-ink-muted uppercase">Status</p>
              <div class="flex items-center gap-2 mt-1">
                <span class="rounded-full w-2 h-2" [class.bg-success]="conn.online()" [class.bg-danger]="!conn.online()"></span>
                <span class="text-xs text-ink">{{ conn.online() ? 'Connected' : 'Offline' }}</span>
              </div>
            </div>
            <div>
              <p class="text-2xs text-ink-muted uppercase">Port</p>
              <p class="text-xs font-mono text-ink mt-1">5179 (http://127.0.0.1:5179)</p>
            </div>
          </section>
        }

        <!-- About -->
        @if (activeTab() === 'about') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">About DevContext</h2>
            <div class="text-xs text-ink-muted space-y-2">
              <p><span class="text-ink">DevContext</span> — the go-to lens for any .NET repo.</p>
              <p>Engine version: {{ conn.version() || '—' }}</p>
              <p>Everything runs locally. Your code never leaves your machine. No telemetry.</p>
              <div class="border-t border-line pt-2 space-y-1">
                <a class="block text-accent hover:underline" href="https://github.com/shaahink/DevContext2" target="_blank" rel="noopener">GitHub repository</a>
                <a class="block text-accent hover:underline" href="https://github.com/shaahink/DevContext2/issues/new" target="_blank" rel="noopener">Report an issue</a>
                <a class="block text-accent hover:underline" href="https://github.com/shaahink/DevContext2/releases" target="_blank" rel="noopener">Check for updates</a>
              </div>
            </div>
          </section>
        }
      </div>
    </div>
  `,
})
export class SettingsView {
  readonly theme = inject(ThemeService);
  readonly conn = inject(ConnectionStore);
  readonly prefs = inject(PrefsStore);
  private readonly storage = inject(StorageService);

  protected readonly tabs: { key: SettingsTab; label: string }[] = [
    { key: 'appearance', label: 'Appearance' },
    { key: 'analysis', label: 'Analysis' },
    { key: 'storage', label: 'Storage' },
    { key: 'server', label: 'Server' },
    { key: 'about', label: 'About' },
  ];

  protected readonly activeTab = signal<SettingsTab>('appearance');
  protected readonly isTauriEnv = isTauri();
  protected readonly cache = signal<StorageSummary | null>(null);
  protected readonly repos = signal<StorageSummary | null>(null);
  protected readonly storageLoading = signal(false);

  protected depthModel = this.prefs.defaultDepth();
  protected detailModel = this.prefs.defaultDetail();
  protected roslynModel = this.prefs.useRoslyn();
  protected cleanupModel = this.prefs.autoCleanup();

  protected selectTab(tab: SettingsTab): void {
    this.activeTab.set(tab);
    if (tab === 'storage' && this.isTauriEnv) void this.loadStorage();
  }

  protected async clearCache(): Promise<void> {
    await this.storage.clearCache();
    void this.loadStorage();
  }

  protected async clearRepos(): Promise<void> {
    await this.storage.clearRepos();
    void this.loadStorage();
  }

  protected openInExplorer(root: 'cache' | 'repos'): void {
    void this.storage.openInExplorer(root);
  }

  protected formatBytes(bytes: number): string {
    return formatBytes(bytes);
  }

  private async loadStorage(): Promise<void> {
    this.storageLoading.set(true);
    try {
      const [cache, repos] = await Promise.all([this.storage.cacheSummary(), this.storage.reposSummary()]);
      this.cache.set(cache);
      this.repos.set(repos);
    } finally {
      this.storageLoading.set(false);
    }
  }
}
