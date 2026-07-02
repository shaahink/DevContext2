import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ThemeService } from '../../core/theme/theme.service';
import { ConnectionStore } from '../../state/connection.store';
import { Icon } from '../../ui/icon/icon';
import { Badge } from '../../ui/badge/badge';

type SettingsTab = 'appearance' | 'analysis' | 'storage' | 'server' | 'about';

@Component({
  selector: 'app-settings-view',
  standalone: true,
  imports: [FormsModule, Icon, Badge],
  template: `
    <div class="flex h-full">
      <!-- Left tabs -->
      <nav class="w-40 shrink-0 border-r border-line bg-surface p-3 space-y-1">
        @for (tab of tabs; track tab.key) {
          <button class="w-full rounded px-2 py-1.5 text-left text-xs transition-colors"
                  [class.bg-accent/10]="activeTab() === tab.key" [class.text-accent]="activeTab() === tab.key"
                  [class.text-ink-muted]="activeTab() !== tab.key"
                  [class.hover:bg-surface-2]="activeTab() !== tab.key"
                  (click)="activeTab.set(tab.key)">{{ tab.label }}</button>
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
                @for (vibe of theme.vibes; track vibe.id) {
                  <button class="rounded border px-3 py-2 text-xs transition-colors"
                          [class.border-accent]="theme.activeVibe() === vibe.id"
                          [class.text-accent]="theme.activeVibe() === vibe.id"
                          [class.border-line]="theme.activeVibe() !== vibe.id"
                          [class.text-ink-muted]="theme.activeVibe() !== vibe.id"
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
            <div class="space-y-3">
              <div>
                <p class="text-2xs text-ink-muted uppercase">Default depth</p>
                <select class="mt-1 rounded border border-line bg-surface-2 px-2 py-1 text-xs text-ink" [(ngModel)]="defaultDepth">
                  @for (d of [1,2,3,4,5,6,7,8,9,10]; track d) { <option [value]="d">{{ d }}</option> }
                </select>
              </div>
              <div>
                <p class="text-2xs text-ink-muted uppercase">Default detail</p>
                <select class="mt-1 rounded border border-line bg-surface-2 px-2 py-1 text-xs text-ink" [(ngModel)]="defaultDetail">
                  <option value="salient">Salient</option>
                  <option value="signature">Signature</option>
                  <option value="full">Full</option>
                </select>
              </div>
              <div class="flex items-center gap-2">
                <input type="checkbox" [(ngModel)]="noRoslyn" />
                <span class="text-xs text-ink">Use Roslyn semantic tier</span>
              </div>
            </div>
          </section>
        }

        <!-- Storage (I8) -->
        @if (activeTab() === 'storage') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">Storage</h2>
            <div>
              <p class="text-2xs text-ink-muted uppercase">Cache location</p>
              <p class="text-xs font-mono text-ink mt-1">%LOCALAPPDATA%/DevContext/cache</p>
              <button class="mt-1 text-xs text-accent hover:underline" onclick="alert('Open folder coming with I8 snapshot cache')">Open folder</button>
            </div>
            <div class="border-t border-line pt-3">
              <p class="text-2xs text-ink-muted uppercase mb-1">Clone folder</p>
              <p class="text-xs font-mono text-ink">%LOCALAPPDATA%/DevContext/clones</p>
              <button class="mt-1 text-xs text-accent hover:underline" onclick="alert('Open folder coming with I8 snapshot cache')">Open folder</button>
            </div>
          </section>
        }

        <!-- Server -->
        @if (activeTab() === 'server') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">Server</h2>
            <div>
              <p class="text-2xs text-ink-muted uppercase">Status</p>
              <div class="flex items-center gap-2 mt-1">
                <span class="rounded-full w-2 h-2" [class.bg-green-500]="conn.online()" [class.bg-red-500]="!conn.online()"></span>
                <span class="text-xs text-ink">{{ conn.online() ? 'Connected' : 'Offline' }}</span>
              </div>
            </div>
            <div>
              <p class="text-2xs text-ink-muted uppercase">Port</p>
              <p class="text-xs font-mono text-ink mt-1">5179 (http://127.0.0.1:5179)</p>
            </div>
          </section>
        }

        <!-- About (I9) -->
        @if (activeTab() === 'about') {
          <section class="space-y-4">
            <h2 class="text-sm font-semibold text-ink">About DevContext</h2>
            <div class="text-xs text-ink-muted space-y-2">
              <p><span class="text-ink">DevContext</span> — the go-to lens for any .NET repo.</p>
              <p>Engine version: {{ conn.version() || '—' }}</p>
              <p>Everything runs locally. Your code never leaves your machine. No telemetry.</p>
              <div class="border-t border-line pt-2 space-y-1">
                <a class="block text-accent hover:underline" href="https://github.com/anomalyco/DevContext" target="_blank">GitHub repository</a>
                <a class="block text-accent hover:underline" href="https://github.com/anomalyco/DevContext/issues/new" target="_blank">Report an issue</a>
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

  protected readonly tabs: { key: SettingsTab; label: string }[] = [
    { key: 'appearance', label: 'Appearance' },
    { key: 'analysis', label: 'Analysis' },
    { key: 'storage', label: 'Storage' },
    { key: 'server', label: 'Server' },
    { key: 'about', label: 'About' },
  ];

  protected readonly activeTab = signal<SettingsTab>('appearance');
  protected readonly defaultDepth = signal(6);
  protected readonly defaultDetail = signal('salient');
  protected readonly noRoslyn = signal(false);
}
