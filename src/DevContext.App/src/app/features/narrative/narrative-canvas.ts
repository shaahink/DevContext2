import { Component, HostListener, inject, signal } from '@angular/core';

import { ConnectionStore } from '../../state/connection.store';
import { SessionStore } from '../../state/session.store';
import { ThemeService } from '../../core/theme/theme.service';
import { AppHeader } from '../../shell/header/app-header';
import { AppFooter } from '../../shell/footer/app-footer';
import { ScrollSpy } from '../../shell/scroll-spy/scroll-spy';
import { SectionCard } from '../../ui/section-card/section-card';
import { SectionLanding } from './section-landing';
import { SectionIdentity } from './section-identity';
import { SectionEntries } from './section-entries';
import { SectionTrace } from './section-trace';
import { SectionArchitecture } from './section-architecture';

const ALL_SECTIONS = [
  'landing',
  'identity',
  'entries',
  'trace',
  'architecture',
  'graph',
  'stats',
  'export',
  'settings',
] as const;

@Component({
  selector: 'app-narrative-canvas',
  imports: [AppHeader, AppFooter, ScrollSpy, SectionCard, SectionLanding, SectionIdentity, SectionEntries, SectionTrace, SectionArchitecture],
  template: `
    <app-header [transparent]="!isAtTop()">
      <ng-container analyze />
    </app-header>

    <app-scroll-spy
      [sections]="visibleSections()"
      [active]="activeSection()"
    />

    <main class="mx-auto max-w-4xl px-4 pb-8 pt-14">
      @if (!session.ready()) {
        <app-section-landing />
      }

      @if (session.ready()) {
        <app-section-identity />
        <app-section-entries />
        <app-section-trace />
        <app-section-architecture />

        <app-section-card id="graph" title="Graph">
          <p class="text-xs text-ink-subtle">Expand to view the call graph.</p>
        </app-section-card>

        <app-section-card id="stats" title="Pipeline">
          <p class="text-sm text-ink-muted">Loading stats&hellip;</p>
        </app-section-card>

        <app-section-card id="export" title="LLM Context">
          <p class="text-xs text-ink-subtle">Generate structured context for LLMs.</p>
        </app-section-card>
      }

      <app-section-card id="settings" title="Settings" [last]="true">
        <p class="text-sm text-ink-muted">Loading settings&hellip;</p>
      </app-section-card>
    </main>

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
  readonly visibleSections = signal<readonly string[]>(ALL_SECTIONS);

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
    for (const id of ALL_SECTIONS) {
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
