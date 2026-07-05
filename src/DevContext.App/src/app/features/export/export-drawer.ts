import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';

import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore } from '../../state/trail.store';
import { ToastService } from '../../ui/toast/toast';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';
import { Meter } from '../../ui/meter/meter';
import { Skeleton } from '../../ui/skeleton/skeleton';
import { copyToClipboard } from '../../core/clipboard';
import { formatCompact } from '../../core/format';

type ExportPreset = 'full' | 'onboarding' | 'flow' | 'trail';
type ContextIntent = 'trace' | 'explain' | 'review';

interface SectionToggle {
  key: string;
  tokens: number;
  enabled: boolean;
}

/** A ContextPackBuilder section, ready to render as its own card instead of one undifferentiated
 * blob — this is the "styled pack preview" the proposal's L6 LLM-context-pane item asked for. */
interface ContextSectionVm {
  key: string;
  title: string;
  tokens: number;
  content: string;
  code: boolean;
}

interface TrailGroupVm {
  title: string;
  totalTokens: number;
  sections: ContextSectionVm[];
  omitted: string[];
}

const ONBOARDING_SECTIONS = ['Overview', 'Topology', 'Routes', 'Entry points'];

const PRESET_LABELS: Record<ExportPreset, string> = {
  full: 'Full',
  onboarding: 'Onboarding',
  flow: 'Flow Review',
  trail: 'From Trail',
};

const INTENT_LABELS: Record<ContextIntent, string> = {
  trace: 'Trace',
  explain: 'Explain',
  review: 'Review',
};

const BUDGET_OPTIONS = [2000, 4000, 8000, 16000] as const;

const SECTION_TITLES: Record<string, string> = {
  identity: 'Identity',
  trace: 'Trace skeleton',
  signatures: 'Callee signatures',
  bodies: 'Salient code',
  di_wiring: 'DI wiring',
  entities: 'Touched entities',
};

function sectionTitle(key: string): string {
  const base = key.replace(/ \(trimmed\)$/, '');
  const label = SECTION_TITLES[base] ?? base;
  return key === base ? label : `${label} (trimmed)`;
}

/**
 * Export drawer (proposal §2/§3.8) — Ctrl+E overlay from the Workbench.
 * Two families of preset:
 *   Full / Onboarding — map-wide document renders (Render RPC), section toggles, raw markdown.
 *     These are whole-repo documents; a flat scrollable render is the right shape for them.
 *   Flow Review / From Trail — a budget-priced ContextPackBuilder pack for one (or each pinned)
 *     trace focus, rendered as per-section cards with a real token meter and an intent selector
 *     (trace/explain/review — L5.2), instead of one raw wrapped-ASCII blob (the flaw the L6 UI/UX
 *     audit called out: "the product's crown jewel... looks like an accident").
 *
 * Follows the same overlay pattern as AuditTable (parent-controlled open/dismissed).
 */
@Component({
  selector: 'app-export-drawer',
  imports: [Icon, Button, Meter, Skeleton, NgTemplateOutlet],
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

        <!-- Context pack controls (Flow / Trail): intent + budget -->
        @if (isContextPreset()) {
          <div class="flex flex-wrap items-center gap-1.5 border-b border-line px-4 py-2">
            <span class="text-2xs uppercase tracking-wider text-ink-subtle">Intent</span>
            @for (i of intents; track i) {
              <button
                type="button"
                class="chip text-2xs"
                [class.active]="intent() === i"
                [title]="intentHint(i)"
                (click)="setIntent(i)"
              >{{ INTENT_LABELS[i] }}</button>
            }
            <span class="mx-1 h-4 w-px bg-line"></span>
            <span class="text-2xs uppercase tracking-wider text-ink-subtle">Budget</span>
            <select
              class="bg-transparent text-2xs text-ink-muted focus:outline-none"
              [value]="budgetTokens()"
              (change)="onBudgetChange($event)"
            >
              @for (b of budgetOptions; track b) {
                <option [value]="b">{{ fmt(b) }} tok</option>
              }
            </select>
          </div>
        }

        <!-- Section toggles (map-wide renders only) -->
        @if (sectionData().length > 0 && !isContextPreset()) {
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
            } @else if (trailGroups().length > 0) {
              <div class="space-y-4" [class.opacity-60]="contentPreserved()">
                @for (group of trailGroups(); track group.title) {
                  <div>
                    <div class="mb-1.5 flex items-center gap-2">
                      <span class="text-xs font-semibold text-ink">{{ group.title }}</span>
                      <span class="text-2xs tabular-nums text-ink-subtle">{{ fmt(group.totalTokens) }} tok</span>
                    </div>
                    <div class="space-y-2 border-l-2 border-line pl-3">
                      @for (s of group.sections; track s.key) {
                        <ng-container [ngTemplateOutlet]="sectionCard" [ngTemplateOutletContext]="{ $implicit: s }" />
                      }
                    </div>
                  </div>
                }
              </div>
            } @else if (!loading()) {
              <div class="flex h-full flex-col items-center justify-center gap-3 px-4">
                <span class="text-xs text-ink-subtle">No pinned steps yet.</span>
                <span class="text-2xs text-ink-subtle">Press <kbd class="kbd">p</kbd> in the Workbench to pin steps, then build a trail-based context pack here.</span>
              </div>
            }
          } @else if (activePreset() === 'flow') {
            @if (!trace.focus() && !loading()) {
              <div class="flex h-full flex-col items-center justify-center gap-3 px-4">
                <span class="text-xs text-ink-subtle">No entry selected.</span>
                <span class="text-2xs text-ink-subtle">Select an entry in the Workbench deck first, then come back for the flow review.</span>
              </div>
            } @else if (contextSections().length > 0) {
              <div class="space-y-2" [class.opacity-60]="contentPreserved()">
                @for (s of contextSections(); track s.key) {
                  <ng-container [ngTemplateOutlet]="sectionCard" [ngTemplateOutletContext]="{ $implicit: s }" />
                }
                @if (contextOmitted().length > 0) {
                  <div class="rounded border border-warn/40 bg-warn/10 px-3 py-2 text-2xs text-ink-muted">
                    <p class="mb-1 font-semibold text-warn">Omitted (budget-cut)</p>
                    @for (o of contextOmitted(); track o) {
                      <p>{{ o }}</p>
                    }
                  </div>
                }
              </div>
            } @else if (loading()) {
              <div class="space-y-3">
                @for (i of [1,2,3]; track i) {
                  <app-skeleton class="block" width="100%" height="12px" />
                  <app-skeleton class="block" [width]="(65 + i * 7) + '%'" height="12px" />
                }
              </div>
            }
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

        <!-- One context-pack section: header (title, token count, meter), collapsible, content -->
        <ng-template #sectionCard let-s>
          <div class="rounded border border-line" [class.bg-surface-2]="s.code">
            <button
              type="button"
              class="flex w-full items-center gap-2 px-2.5 py-1.5 text-left hover:bg-surface-2"
              (click)="toggleCollapsed(s.key)"
            >
              <app-icon [name]="isCollapsed(s.key) ? 'chevron-right' : 'chevron-down'" [size]="12" class="text-ink-subtle shrink-0" />
              <span class="flex-1 truncate text-xs font-semibold text-ink">{{ s.title }}</span>
              <app-meter [value]="budgetPct(s.tokens)" variant="accent" class="w-10 shrink-0" />
              <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">{{ s.tokens }}</span>
            </button>
            @if (!isCollapsed(s.key)) {
              <pre class="whitespace-pre-wrap border-t border-line px-2.5 py-2 font-mono text-2xs leading-relaxed text-ink">{{ s.content }}</pre>
            }
          </div>
        </ng-template>

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
  protected readonly intents: readonly ContextIntent[] = ['trace', 'explain', 'review'];
  protected readonly budgetOptions = BUDGET_OPTIONS;
  protected readonly PRESET_LABELS = PRESET_LABELS;
  protected readonly INTENT_LABELS = INTENT_LABELS;

  protected readonly activePreset = signal<ExportPreset>('full');
  protected readonly intent = signal<ContextIntent>('trace');
  protected readonly budgetTokens = signal<number>(8000);
  protected readonly sectionData = signal<SectionToggle[]>([]);
  protected readonly content = signal('');
  protected readonly contextSections = signal<ContextSectionVm[]>([]);
  protected readonly contextOmitted = signal<string[]>([]);
  protected readonly trailGroups = signal<TrailGroupVm[]>([]);
  protected readonly tokenCount = signal(0);
  protected readonly loading = signal(false);
  protected readonly trailRendering = signal(false);
  protected readonly trailProgress = signal('');
  protected readonly renderError = signal<string | null>(null);
  protected readonly collapsed = signal<ReadonlySet<string>>(new Set());
  /** True when this is a refresh (not first load) — dims existing content. */
  protected readonly contentPreserved = signal(false);
  private hasLoaded = false;

  protected readonly isContextPreset = computed(() => this.activePreset() === 'flow' || this.activePreset() === 'trail');

  protected readonly effectiveContent = computed(() => {
    if (this.activePreset() === 'trail') {
      return this.trailGroups()
        .map((g) => `## [${g.title}]\n\n${g.sections.map((s) => `### ${s.title}\n${s.content}`).join('\n\n')}`)
        .join('\n\n');
    }
    if (this.activePreset() === 'flow') {
      return this.contextSections().map((s) => `## ${s.title}\n${s.content}`).join('\n\n');
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
    } else {
      void this.render();
    }
  }

  protected setIntent(i: ContextIntent): void {
    this.intent.set(i);
    void this.render();
  }

  protected intentHint(i: ContextIntent): string {
    switch (i) {
      case 'explain':
        return 'Concepts first: DI wiring, entities, signatures — code trimmed if tight on budget.';
      case 'review':
        return 'Code first: full trace + signatures + salient bodies, deepest trace.';
      default:
        return 'Balanced: trace skeleton, signatures, salient bodies, then wiring/entities.';
    }
  }

  protected onBudgetChange(event: Event): void {
    this.budgetTokens.set(Number((event.target as HTMLSelectElement).value));
    void this.render();
  }

  protected budgetPct(tokens: number): number {
    const budget = this.budgetTokens();
    return budget > 0 ? Math.min(100, (tokens / budget) * 100) : 0;
  }

  protected isCollapsed(key: string): boolean {
    return this.collapsed().has(key);
  }

  protected toggleCollapsed(key: string): void {
    this.collapsed.update((set) => {
      const next = new Set(set);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  }

  protected async render(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;

    const preset = this.activePreset();

    if (preset === 'trail') {
      await this.renderTrail(handle);
      return;
    }

    if (preset === 'flow') {
      await this.renderContextFocused(handle, this.trace.focus());
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
      this.contextSections.set([]);
      this.trailGroups.set([]);

      // Preserve user toggles: only add new sections, keep existing enabled state
      const existing = new Map(this.sectionData().map((s) => [s.key, s.enabled]));
      const data = (res.sections ?? []).map((s) => ({
        key: s.key,
        tokens: s.tokens,
        enabled: existing.get(s.key) ?? true,
      }));
      this.sectionData.set(data);
      this.hasLoaded = true;
    } catch {
      this.renderError.set('Render failed — check server connection.');
      this.toast.show('Render failed', 'error');
    } finally {
      this.contentPreserved.set(false);
      this.loading.set(false);
    }
  }

  private async renderContextFocused(handle: string, focus: string | null): Promise<void> {
    if (!focus) {
      this.contextSections.set([]);
      this.contextOmitted.set([]);
      this.tokenCount.set(0);
      this.trailGroups.set([]);
      this.renderError.set(null);
      return;
    }

    this.loading.set(true);
    this.renderError.set(null);
    if (this.hasLoaded) this.contentPreserved.set(true);

    try {
      const res = await this.api.getContext(handle, focus, { budgetTokens: this.budgetTokens(), intent: this.intent() });
      this.contextSections.set(res.sections.map(toSectionVm));
      this.contextOmitted.set(res.omitted);
      this.tokenCount.set(res.totalTokens);
      this.trailGroups.set([]);
      this.hasLoaded = true;
    } catch {
      this.renderError.set('Render failed — check server connection.');
      this.toast.show('Render failed', 'error');
    } finally {
      this.contentPreserved.set(false);
      this.loading.set(false);
    }
  }

  private async renderTrail(handle: string): Promise<void> {
    const pins = this.trail.pins();
    if (pins.length === 0) {
      this.trailGroups.set([]);
      this.tokenCount.set(0);
      this.renderError.set(null);
      return;
    }

    this.trailRendering.set(true);
    this.renderError.set(null);
    this.contentPreserved.set(false);

    const groups: TrailGroupVm[] = [];
    let totalTokens = 0;

    for (let i = 0; i < pins.length; i++) {
      const pin = pins[i];
      this.trailProgress.set(`step ${i + 1} of ${pins.length}`);
      try {
        const res = await this.api.getContext(handle, pin.focus, { budgetTokens: this.budgetTokens(), intent: this.intent() });
        groups.push({ title: pin.title, totalTokens: res.totalTokens, sections: res.sections.map(toSectionVm), omitted: res.omitted });
        totalTokens += res.totalTokens;
      } catch {
        groups.push({ title: pin.title, totalTokens: 0, sections: [], omitted: [`Render failed for "${pin.title}"`] });
      }
    }

    this.trailGroups.set(groups);
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

function toSectionVm(s: { key: string; tokens: number; content: string }): ContextSectionVm {
  return {
    key: s.key,
    title: sectionTitle(s.key),
    tokens: s.tokens,
    content: s.content,
    code: s.key.startsWith('bodies') || s.key.startsWith('signatures'),
  };
}
