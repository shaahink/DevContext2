import { Component, computed, effect, HostListener, inject } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { CommandPaletteService } from '../../ui/command-palette/command-palette';
import { ToastService } from '../../ui/toast/toast';
import { DevContextApi } from '../../data-access/devcontext-api';
import { EntriesPanel } from '../entries-panel/entries-panel';
import { HealthDashboard } from '../health-dashboard/health-dashboard';
import { Launcher } from '../launcher/launcher';
import { LlmExport } from '../llm-export/llm-export';
import { MapPanel } from '../map-panel/map-panel';
import { Shortcuts } from '../shortcuts/shortcuts';
import { SourceBar } from '../source-bar/source-bar';
import { StatsDrawer } from '../stats-drawer/stats-drawer';
import { TracePanel } from '../trace-panel/trace-panel';
import { VibeSwitcher } from '../vibe-switcher/vibe-switcher';

@Component({
  selector: 'app-workspace',
  imports: [SourceBar, EntriesPanel, MapPanel, TracePanel, Launcher, LlmExport, StatsDrawer, HealthDashboard, Shortcuts, VibeSwitcher],
  template: `
    <div class="flex h-screen flex-col bg-base text-ink">
      <app-source-bar />

      @if (session.ready() || session.busy()) {
        <div class="grid min-h-0 flex-1" style="grid-template-columns: 280px 1fr">
          <app-entries-panel />
          <div class="min-w-0">
            @if (trace.active()) {
              <app-trace-panel />
            } @else {
              <app-map-panel />
            }
          </div>
        </div>
      } @else {
        <div class="min-h-0 flex-1 overflow-auto">
          <app-launcher (pickRepo)="onAnalyze($event)" />
        </div>
      }

      <div class="flex items-center gap-2 border-t border-line bg-surface-2 px-3 py-1 text-[11px] text-ink-muted">
        <span>{{ statusText() }}</span>
        @if (session.ready()) {
          <span>{{ session.entryCount() }} entry points</span>
          <app-health-dashboard />
          <app-llm-export />
          <app-stats-drawer />
        }
        <div class="ml-auto flex items-center gap-1">
          <app-vibe-switcher />
          <app-shortcuts />
        </div>
      </div>
    </div>
  `,
})
export class Workspace {
  protected readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  private readonly connection = inject(ConnectionStore);
  private readonly cmd = inject(CommandPaletteService);
  private readonly api = inject(DevContextApi);
  private readonly toast = inject(ToastService);
  private prevHandle: string | null = null;

  constructor() {
    this.connection.start();

    effect(() => {
      const handle = this.session.handle();
      if (handle !== this.prevHandle) {
        this.prevHandle = handle;
        this.trace.clear();
      }
    });

    effect(() => {
      const e = this.session.error();
      if (e) this.toast.show(e, 'error');
    });

    this.cmd.setSearch(async (q) => {
      const h = this.session.handle();
      if (!h || !q.trim()) return [];
      try {
        const res = await this.api.searchNodes(h, q, 10);
        return res.nodes.map((n) => ({
          id: `node-${n.nodeId}`,
          label: n.title,
          detail: `${n.kind}`,
          badge: n.tags.slice(0, 3).join(', '),
          action: () => {
            this.trace.selectNode(n.nodeId);
          },
        }));
      } catch {
        return [];
      }
    });

    this.cmd.register({
      id: 'nav',
      placeholder: 'Search commands, entries, nodes…',
      items: computed(() => [
        {
          id: 'cmd-map',
          label: 'Overview map',
          detail: 'Show architecture map',
          action: () => this.trace.clear(),
        },
        ...(this.session.ready()
          ? this.session.entryGroups().flatMap((g) =>
              g.entries.map((e) => ({
                id: `entry-${e.nodeId}`,
                label: e.route || e.title,
                detail: `Trace ${e.kind}`,
                action: () => {
                  const h = this.session.handle();
                  if (h) void this.trace.trace(h, e.focus);
                },
              })),
            )
          : []),
        {
          id: 'cmd-analyze',
          label: 'Analyze repo…',
          detail: 'Open a .NET repo',
          action: () => {
            const input = document.querySelector<HTMLInputElement>('app-source-bar input');
            input?.focus();
          },
        },
      ]),
    });
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      this.cmd.toggle();
    }
  }

  protected onAnalyze(path: string): void {
    void this.session.analyze({ path });
  }

  statusText(): string {
    switch (this.session.status()) {
      case 'cloning': return 'Cloning…';
      case 'analyzing': return 'Analyzing…';
      case 'error': return 'Error';
      case 'ready': return this.trace.active() ? `Tracing ${this.trace.focus()}` : 'Map';
      default: return 'Ready';
    }
  }
}
