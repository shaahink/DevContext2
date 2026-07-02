import { Component, computed, HostListener, inject, signal } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';
import { SessionStore } from '../../state/session.store';
import { ThemeService } from '../../core/theme/theme.service';
import { AppHeader } from '../../shell/header/app-header';
import { AppFooter } from '../../shell/footer/app-footer';
import { ScrollSpy } from '../../shell/scroll-spy/scroll-spy';
import { SectionCard } from '../../ui/section-card/section-card';
import { Icon } from '../../ui/icon/icon';
import { SectionLanding } from './section-landing';
import { SectionIdentity } from './section-identity';
import { SectionEntries } from './section-entries';
import { SectionTrace } from './section-trace';
import { SectionArchitecture } from './section-architecture';
import { SectionGraph } from './section-graph';
import { SectionStats } from './section-stats';
import { SectionExport } from './section-export';
import { SectionConsole } from './section-console';
import { SectionLens } from './section-lens';
import { SectionSettings } from './section-settings';

@Component({
  selector: 'app-narrative-canvas',
  imports: [AppHeader, AppFooter, ScrollSpy, SectionCard, Icon, SectionLanding, SectionIdentity, SectionEntries, SectionTrace, SectionArchitecture, SectionGraph, SectionStats, SectionExport, SectionConsole, SectionLens, SectionSettings],
  template: `
    <app-header [transparent]="!isAtTop()">
      <ng-container analyze />
    </app-header>

    <app-scroll-spy
      [sections]="visibleSections()"
      [active]="activeSection()"
    />

    <main class="mx-auto max-w-4xl px-5 pb-10 pt-12">
      @if (session.error()) {
        <div class="rounded-md border border-danger/30 bg-danger/10 px-4 py-3 text-xs text-ink">
          <div class="flex items-start gap-2">
            <app-icon name="x" [size]="14" class="mt-0.5 shrink-0 text-danger" />
            <div>
              <p class="font-medium text-danger">Analysis failed</p>
              <p class="mt-0.5 text-ink-muted">{{ session.error() }}</p>
            </div>
          </div>
        </div>
      }

      @if (!session.ready()) {
        <app-section-landing />
      }

      @if (session.busy()) {
        <app-section-console />
      }

      @if (session.ready()) {
        <app-section-console />
        <app-section-identity />
        <app-section-entries />
        <app-section-trace />

        <app-section-lens />

        <app-section-architecture />
        <app-section-graph />

        <app-section-stats />

        <app-section-card id="export" title="LLM Context" [last]="true">
          <div class="flex items-center justify-center py-4">
            <button
              class="flex cursor-pointer items-center gap-2 rounded-md border border-line bg-surface px-4 py-2 text-xs text-ink-muted transition-colors hover:border-line-strong hover:text-ink"
              (click)="exportOpen.set(true)"
            >
              <app-icon name="file-text" [size]="14" />
              Open LLM Context Exporter
            </button>
          </div>
        </app-section-card>
      }

      <app-section-settings />
    </main>

    <app-section-export [open]="exportOpen()" (dismissed)="exportOpen.set(false)" />

    <app-footer [transparent]="!isAtBottom()" />
  `,
  host: { class: 'block min-h-screen' },
})
export class NarrativeCanvas {
  protected readonly session = inject(SessionStore);
  private readonly connection = inject(ConnectionStore);

  readonly isAtTop = signal(true);
  readonly isAtBottom = signal(false);
  readonly activeSection = signal('landing');
  readonly visibleSections = computed(() => {
    if (this.session.ready()) {
      return ['landing', 'console', 'identity', 'entries', 'trace', 'lens', 'architecture', 'graph', 'stats', 'export', 'settings'] as const;
    }
    return ['landing', 'console', 'settings'] as const;
  });
  readonly exportOpen = signal(false);

  constructor() {
    this.connection.start();
    inject(ThemeService); // instantiate to set data-vibe/data-theme on <html>
  }

  @HostListener('window:scroll', [])
  onScroll(): void {
    const y = window.scrollY;
    this.isAtTop.set(y < 48);
    const docH = document.documentElement.scrollHeight;
    const winH = window.innerHeight;
    this.isAtBottom.set(y + winH >= docH - 48);

    this.detectActiveSection(y, winH);
  }

  private detectActiveSection(scrollY: number, winH: number): void {
    for (const id of this.visibleSections()) {
      const el = document.getElementById(id);
      if (!el) continue;
      const rect = el.getBoundingClientRect();
      if (rect.top <= winH * 0.4 && rect.bottom >= 0) {
        this.activeSection.set(id);
        return;
      }
    }
  }
}
