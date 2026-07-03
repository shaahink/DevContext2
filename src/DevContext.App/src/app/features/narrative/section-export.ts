import { Component, effect, inject, input, output, signal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { DevContextApi } from '../../data-access/devcontext-api';
import { ToastService } from '../../ui/toast/toast';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';

@Component({
  selector: 'app-section-export',
  imports: [Icon, Button],
  template: `
    <div class="fixed inset-0 z-50 flex" [class.hidden]="!open()">
      <div
        class="absolute inset-0 bg-base/80 backdrop-blur-sm"
        role="button"
        tabindex="0"
        aria-label="Close export"
        (click)="emitDismissed()"
        (keydown.enter)="emitDismissed()"
        (keydown.space)="emitDismissed()"
      ></div>
      <div class="relative mx-auto my-6 flex w-full max-w-5xl flex-col overflow-hidden rounded-lg border border-line bg-elevated shadow-2xl">
        <div class="flex items-center gap-3 border-b border-line px-4 py-3">
          <app-icon name="file-text" [size]="16" class="text-accent" />
          <h2 class="text-sm font-semibold text-ink">LLM Context Export</h2>
          <span class="text-xs text-ink-subtle">Structured context for LLMs</span>
          <div class="ml-auto flex items-center gap-2">
            @if (tokenCount() > 0) {
              <span class="text-xs tabular-nums text-ink-muted">{{ fmt(tokenCount()) }} tok</span>
            }
            <app-button variant="secondary" size="sm" (click)="render()" [disabled]="loading()">
              <app-icon [name]="loading() ? 'loader' : 'refresh'" [size]="12" />
              Re-render
            </app-button>
            <app-button variant="secondary" size="sm" (click)="copy()" [disabled]="!content()">
              <app-icon name="copy" [size]="12" />
              Copy
            </app-button>
            <app-button variant="ghost" size="sm" (click)="emitDismissed()">
              <app-icon name="x" [size]="14" />
            </app-button>
          </div>
        </div>

        <div class="flex min-h-0 flex-1">
          @if (sectionData().length) {
            <div class="w-52 shrink-0 overflow-y-auto border-r border-line p-3">
              <p class="mb-2 text-xs font-semibold uppercase tracking-wider text-ink-subtle">Sections</p>
              <div class="space-y-0.5">
                @for (s of sectionData(); track s.key) {
                  <label class="flex cursor-pointer items-center gap-2 rounded px-2 py-1 text-xs hover:bg-surface-2 transition-colors">
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

          <div class="min-h-0 flex-1 overflow-auto p-4">
            @if (renderError()) {
              <div class="flex h-full flex-col items-center justify-center gap-3">
                <span class="text-danger text-xs">{{ renderError() }}</span>
                <button class="rounded bg-surface-2 px-3 py-1.5 text-xs text-ink hover:bg-surface-1" (click)="render()">Retry</button>
              </div>
            } @else if (content()) {
              <pre class="whitespace-pre-wrap font-mono text-sm text-ink leading-relaxed">{{ content() }}</pre>
            } @else if (loading()) {
              <div class="flex h-full items-center justify-center gap-2 text-xs text-ink-muted">
                <app-icon name="loader" [size]="14" class="animate-spin" />
                Rendering…
              </div>
            } @else {
              <div class="flex h-full items-center justify-center text-xs text-ink-subtle">Rendering context…</div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
  host: { class: 'contents' },
})
export class SectionExport {
  readonly open = input(false);
  readonly dismissed = output<void>();

  private readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly toast = inject(ToastService);

  protected readonly content = signal('');
  protected readonly sectionData = signal<{ key: string; tokens: number; enabled: boolean }[]>([]);
  protected readonly tokenCount = signal(0);
  protected readonly loading = signal(false);

  protected readonly renderError = signal<string | null>(null);

  constructor() {
    effect(() => {
      if (this.open() && this.session.handle()) {
        void this.render();
      }
    });
  }

  protected emitDismissed(): void {
    this.dismissed.emit();
  }

  protected async render(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.loading.set(true);
    this.renderError.set(null);
    try {
      const currentToggles = this.sectionData().filter((s) => s.enabled).map((s) => s.key);
      const res = await this.api.render(handle, {
        format: 'markdown',
        sections: currentToggles.length ? currentToggles : undefined,
      });
      this.content.set(res.content);
      this.tokenCount.set(res.estimatedTokens);
      // Preserve user toggles: only add new sections, keep existing enabled state
      const existing = new Map(this.sectionData().map((s) => [s.key, s.enabled]));
      const data = (res.sections ?? []).map((s) => ({ key: s.key, tokens: s.tokens, enabled: existing.get(s.key) ?? true }));
      this.sectionData.set(data);
    } catch {
      this.renderError.set('Render failed — check server connection.');
      this.toast.show('Render failed', 'error');
    } finally {
      this.loading.set(false);
    }
  }

  protected toggleSection(key: string): void {
    this.sectionData.update((data) =>
      data.map((s) => (s.key === key ? { ...s, enabled: !s.enabled } : s)),
    );
    void this.render();
  }

  protected async copy(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.content());
      this.toast.show('Copied to clipboard', 'info');
    } catch {
      this.toast.show('Copy failed', 'error');
    }
  }

  protected fmt(n: number): string {
    if (n >= 1000) return (n / 1000).toFixed(1) + 'K';
    return String(n);
  }
}
