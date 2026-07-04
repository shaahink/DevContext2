import { Component, computed, effect, inject, input, output, signal } from '@angular/core';

import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore } from '../../state/trail.store';
import { ToastService } from '../../ui/toast/toast';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';
import { Skeleton } from '../../ui/skeleton/skeleton';
import { copyToClipboard } from '../../core/clipboard';
import { formatCompact } from '../../core/format';

type ExportPreset = 'full' | 'onboarding' | 'flow' | 'trail';

interface SectionToggle {
  key: string;
  tokens: number;
  enabled: boolean;
}

interface PinnedRender {
  title: string;
  content: string;
  tokens: number;
}

const ONBOARDING_SECTIONS = ['Overview', 'Topology', 'Routes', 'Entry points'];

const PRESET_LABELS: Record<ExportPreset, string> = {
  full: 'Full',
  onboarding: 'Onboarding',
  flow: 'Flow Review',
  trail: 'From Trail',
};

/**
 * Export drawer (proposal §2/§3.8) — Ctrl+E overlay from the Workbench.
 * Ports section-export.ts render logic into a right-side drawer with 4 presets:
 *   Full — all map sections
 *   Onboarding — Identity + Architecture + Entries
 *   Flow Review — current trace focus (single focused render)
 *   From Trail — each pinned trail step rendered via Render RPC, concatenated
 *
 * Follows the same overlay pattern as AuditTable (parent-controlled open/dismissed).
 * Map-wide render preserves user section toggles across re-renders (ported from
 * section-export.ts). Content-preserving loading per proposal §5.2: existing
 * content is dimmed on refresh, not cleared; skeleton blocks on first load only.
 */
@Component({
  selector: 'app-export-drawer',
  imports: [Icon, Button, Skeleton],
  template: `
    <div class="fixed inset-0 z-50" [class.hidden]="!open()">
      <!-- Backdrop scrim -->
      <div
        class="absolute inset-0 bg-base/70"
        role="button"
        tabindex="0"
        aria-label="Close export drawer"
        (click)="dismissed.emit()"
        (keydown.enter)="dismissed.emit()"
      ></div>

      <!-- Drawer panel — right side, 480px -->
      <div class="overlay-float absolute right-0 top-0 bottom-0 w-[480px] flex flex-col overflow-hidden">
        <!-- Header -->
        <div class="flex items-center gap-2 border-b border-line px-4 py-3">
          <app-icon name="file-text" [size]="16" class="text-accent shrink-0" />
          <h2 class="text-sm font-semibold text-ink">Export</h2>
          @if (tokenCount() > 0) {
            <span class="text-xs tabular-nums text-ink-muted">{{ fmt(tokenCount()) }} tok</span>
          }
          <span class="flex-1"></span>
          <app-button variant="ghost" size="sm" (click)="dismissed.emit()">
            <app-icon name="x" [size]="14" />
          </app-button>
        </div>

        <!-- Preset chips -->
        <div class="flex flex-wrap items-center gap-1.5 border-b border-line px-4 py-2">
          @for (p of presets; track p) {
            <button
              type="button"
              class="chip"
              [class.active]="activePreset() === p"
              (click)="selectPreset(p)"
            >{{ PRESET_LABELS[p] }}</button>
          }
        </div>

        <!-- Section toggles (map-wide renders only) -->
        @if (sectionData().length > 0 && activePreset() !== 'trail') {
          <div class="border-b border-line px-4 py-2">
            <p class="mb-1.5 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Sections</p>
            <div class="space-y-0.5 max-h-48 overflow-y-auto">
              @for (s of sectionData(); track s.key) {
                <label class="flex cursor-pointer items-center gap-2 rounded px-2 py-1 text-xs hover:bg-surface-2">
                  <input
                    type="checkbox"
                    [checked]="s.enabled"
                    (change)="toggleSection(s.key)"
                    class="rounded border-line accent-accent"
                  />
                  <span class="flex-1 truncate text-ink">{{ s.key }}</span>
                  <span class="text-2xs tabular-nums text-ink-subtle">{{ s.tokens }}</span>
                </label>
              }
            </div>
          </div>
        }

        <!-- Content area -->
        <div class="min-h-0 flex-1 overflow-y-auto p-4">
          @if (renderError()) {
            <div class="flex h-full flex-col items-center justify-center gap-3">
              <span class="text-xs text-danger">{{ renderError() }}</span>
              <app-button variant="secondary" size="sm" (click)="render()">Retry</app-button>
            </div>
          } @else if (activePreset() === 'trail') {
            @if (trailRendering()) {
              <div class="flex h-full items-center justify-center gap-2 text-xs text-ink-muted">
                <app-icon name="loader" [size]="14" class="animate-spin" />
                Rendering {{ trailProgress() }}
              </div>
            } @else if (pinnedContent().length > 0) {
              <pre
                class="whitespace-pre-wrap font-mono text-xs text-ink leading-relaxed"
                [class.opacity-60]="contentPreserved()"
              >@for (pr of pinnedContent(); track pr.title) {## [{{ pr.title }}]

        {{ pr.content }}

        }</pre>
            } @else if (!loading()) {
              <div class="flex h-full flex-col items-center justify-center gap-3 px-4">
                <span class="text-xs text-ink-subtle">No pinned steps yet.</span>
                <span class="text-2xs text-ink-subtle">Press <kbd class="kbd">p</kbd> in the Workbench to pin steps, then build a trail-based context pack here.</span>
              </div>
            }
          } @else if (activePreset() === 'flow' && !trace.focus() && !loading()) {
            <div class="flex h-full flex-col items-center justify-center gap-3 px-4">
              <span class="text-xs text-ink-subtle">No entry selected.</span>
              <span class="text-2xs text-ink-subtle">Select an entry in the Workbench deck first, then come back for the flow review.</span>
            </div>
          } @else if (content()) {
            <pre
              class="whitespace-pre-wrap font-mono text-xs text-ink leading-relaxed"
              [class.opacity-60]="contentPreserved()"
            >{{ content() }}</pre>
          } @else if (loading()) {
            <div class="space-y-3">
              @for (i of [1,2,3,4,5]; track i) {
                <app-skeleton class="block" width="100%" height="12px" />
                <app-skeleton class="block" [width]="(65 + i * 7) + '%'" height="12px" />
                <app-skeleton class="block" width="80%" height="12px" />
                <div class="h-4"></div>
              }
            </div>
          } @else {
            <div class="flex h-full items-center justify-center text-xs text-ink-subtle">Choose a preset to render</div>
          }
        </div>

        <!-- Bottom bar -->
        <div class="flex items-center gap-2 border-t border-line px-4 py-2">
          <app-button variant="secondary" size="sm" (click)="render()" [disabled]="loading()">
            <app-icon [name]="loading() || trailRendering() ? 'loader' : 'refresh'" [size]="12" />
            Re-render
          </app-button>
          <span class="flex-1"></span>
          <app-button variant="secondary" size="sm" (click)="copy()" [disabled]="!effectiveContent()">
            <app-icon name="copy" [size]="12" />
            Copy
          </app-button>
        </div>
      </div>
    </div>
  `,
  host: { class: 'contents' },
})
export class ExportDrawer {
  readonly open = input(false);
  readonly dismissed = output<void>();

  private readonly session = inject(SessionStore);
  protected readonly trace = inject(TraceStore);
  private readonly trail = inject(TrailStore);
  private readonly api = inject(DevContextApi);
  private readonly toast = inject(ToastService);

  protected readonly presets: readonly ExportPreset[] = ['full', 'onboarding', 'flow', 'trail'];
  protected readonly PRESET_LABELS = PRESET_LABELS;

  protected readonly activePreset = signal<ExportPreset>('full');
  protected readonly sectionData = signal<SectionToggle[]>([]);
  protected readonly content = signal('');
  protected readonly pinnedContent = signal<PinnedRender[]>([]);
  protected readonly tokenCount = signal(0);
  protected readonly loading = signal(false);
  protected readonly trailRendering = signal(false);
  protected readonly trailProgress = signal('');
  protected readonly renderError = signal<string | null>(null);
  /** True when this is a refresh (not first load) — dims existing content. */
  protected readonly contentPreserved = signal(false);
  private hasLoaded = false;

  protected readonly effectiveContent = computed(() => {
    if (this.activePreset() === 'trail') {
      return this.pinnedContent().map((pr) => `## [${pr.title}]\n\n${pr.content}`).join('\n\n');
    }
    return this.content();
  });

  constructor() {
    effect(() => {
      if (this.open() && this.session.handle()) {
        void this.render();
      }
    });
  }

  protected selectPreset(preset: ExportPreset): void {
    this.activePreset.set(preset);
    // Apply section toggles for map-wide presets
    if (preset === 'full') {
      this.sectionData.update((d) => d.map((s) => ({ ...s, enabled: true })));
      void this.render();
    } else if (preset === 'onboarding') {
      this.sectionData.update((d) =>
        d.map((s) => ({ ...s, enabled: ONBOARDING_SECTIONS.includes(s.key) })),
      );
      void this.render();
    } else if (preset === 'flow') {
      void this.render();
    } else if (preset === 'trail') {
      void this.render();
    }
  }

  protected async render(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;

    const preset = this.activePreset();

    if (preset === 'trail') {
      await this.renderTrail(handle);
      return;
    }

    // Flow Review: single focused render
    if (preset === 'flow') {
      await this.renderFocused(handle, this.trace.focus());
      return;
    }

    // Full / Onboarding: map-wide render with section filter
    await this.renderMap(handle);
  }

  private async renderMap(handle: string): Promise<void> {
    this.loading.set(true);
    this.renderError.set(null);
    if (this.hasLoaded) this.contentPreserved.set(true);

    try {
      const activeSections = this.sectionData().filter((s) => s.enabled).map((s) => s.key);
      const res = await this.api.render(handle, {
        format: 'markdown',
        sections: activeSections.length ? activeSections : undefined,
      });

      this.content.set(res.content);
      this.tokenCount.set(res.estimatedTokens);
      this.pinnedContent.set([]);

      // Preserve user toggles: only add new sections, keep existing enabled state
      const existing = new Map(this.sectionData().map((s) => [s.key, s.enabled]));
      const data = (res.sections ?? []).map((s) => ({
        key: s.key,
        tokens: s.tokens,
        enabled: existing.get(s.key) ?? true,
      }));
      this.sectionData.set(data);
      this.hasLoaded = true;
      this.contentPreserved.set(false);
    } catch {
      this.renderError.set('Render failed — check server connection.');
      this.toast.show('Render failed', 'error');
    } finally {
      this.loading.set(false);
    }
  }

  private async renderFocused(handle: string, focus: string | null): Promise<void> {
    if (!focus) {
      this.content.set('');
      this.tokenCount.set(0);
      this.pinnedContent.set([]);
      this.renderError.set(null);
      return;
    }

    this.loading.set(true);
    this.renderError.set(null);
    if (this.hasLoaded) this.contentPreserved.set(true);

    try {
      const res = await this.api.render(handle, { focus, format: 'markdown' });
      this.content.set(res.content);
      this.tokenCount.set(res.estimatedTokens);
      this.pinnedContent.set([]);
      this.hasLoaded = true;
      this.contentPreserved.set(false);
    } catch {
      this.renderError.set('Render failed — check server connection.');
      this.toast.show('Render failed', 'error');
    } finally {
      this.loading.set(false);
    }
  }

  private async renderTrail(handle: string): Promise<void> {
    const pins = this.trail.pins();
    if (pins.length === 0) {
      this.content.set('');
      this.tokenCount.set(0);
      this.pinnedContent.set([]);
      this.renderError.set(null);
      return;
    }

    this.trailRendering.set(true);
    this.renderError.set(null);
    this.contentPreserved.set(false);

    const results: PinnedRender[] = [];
    let totalTokens = 0;

    for (let i = 0; i < pins.length; i++) {
      const pin = pins[i];
      this.trailProgress.set(`step ${i + 1} of ${pins.length}`);
      try {
        const res = await this.api.render(handle, { focus: pin.focus, format: 'markdown' });
        results.push({ title: pin.title, content: res.content, tokens: res.estimatedTokens });
        totalTokens += res.estimatedTokens;
      } catch {
        results.push({ title: pin.title, content: `⚠ Render failed for "${pin.title}"`, tokens: 0 });
      }
    }

    this.pinnedContent.set(results);
    this.tokenCount.set(totalTokens);
    this.hasLoaded = true;
    this.trailRendering.set(false);
  }

  protected toggleSection(key: string): void {
    this.sectionData.update((data) =>
      data.map((s) => (s.key === key ? { ...s, enabled: !s.enabled } : s)),
    );
    void this.render();
  }

  protected async copy(): Promise<void> {
    try {
      await copyToClipboard(this.effectiveContent());
      this.toast.show('Copied to clipboard', 'info');
    } catch {
      this.toast.show('Copy failed', 'error');
    }
  }

  protected fmt(n: number): string {
    return formatCompact(n);
  }
}
