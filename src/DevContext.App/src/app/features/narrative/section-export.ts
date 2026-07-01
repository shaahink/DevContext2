import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SessionStore } from '../../state/session.store';
import { DevContextApi } from '../../data-access/devcontext-api';
import { ToastService } from '../../ui/toast/toast';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';

@Component({
  selector: 'app-section-export',
  imports: [FormsModule, Icon, Button],
  template: `
    <div class="fixed inset-0 z-50 flex" [class.hidden]="!open()">
      <div
        class="absolute inset-0 bg-base/80 backdrop-blur-sm"
        role="button"
        tabindex="0"
        (click)="emitDismissed()"
        (keydown.enter)="emitDismissed()"
        (keydown.space)="emitDismissed()"
      ></div>
      <div class="relative mx-auto my-8 flex w-full max-w-5xl flex-col overflow-hidden rounded-lg border border-line bg-surface shadow-2xl">
        <div class="flex items-center gap-3 border-b border-line px-4 py-3">
          <h2 class="text-sm font-semibold text-ink">LLM Context</h2>
          <span class="text-2xs text-ink-subtle">Export structured context for LLMs</span>
          <div class="ml-auto flex items-center gap-2">
            @if (tokenCount() > 0) {
              <span class="text-2xs tabular-nums text-ink-muted">{{ fmt(tokenCount()) }} tok</span>
            }
            <app-button variant="secondary" size="sm" (click)="render()" [disabled]="loading()">
              <app-icon [name]="loading() ? 'loader' : 'refresh'" [size]="12" />
              Render
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
          @if (sections().length) {
            <div class="w-56 shrink-0 overflow-y-auto border-r border-line p-3">
              <p class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Sections</p>
              <div class="space-y-0.5">
                @for (s of sections(); track s) {
                  <label class="flex cursor-pointer items-center gap-2 rounded px-1.5 py-1 text-2xs hover:bg-surface-2">
                    <input type="checkbox" [checked]="true" class="rounded border-line" disabled />
                    <span class="flex-1 truncate text-ink-muted">{{ s }}</span>
                  </label>
                }
              </div>
            </div>
          }

          <div class="min-h-0 flex-1 overflow-auto p-4">
            @if (content()) {
              <pre class="whitespace-pre-wrap font-mono text-xs text-ink leading-relaxed">{{ content() }}</pre>
            } @else if (loading()) {
              <div class="flex h-full items-center justify-center text-xs text-ink-muted">Rendering&hellip;</div>
            } @else {
              <div class="flex h-full items-center justify-center text-xs text-ink-subtle">Click Render to generate LLM context from the analysis snapshot.</div>
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
  private readonly activity = inject(ActivityService);
  private readonly toast = inject(ToastService);

  protected readonly content = signal('');
  protected readonly sections = signal<string[]>([]);
  protected readonly tokenCount = signal(0);
  protected readonly loading = signal(false);

  protected emitDismissed(): void {
    this.dismissed.emit();
  }

  protected async render(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.loading.set(true);
    try {
      const res = await this.api.render(handle, { format: 'markdown' });
      this.content.set(res.content);
      this.sections.set(res.sections?.map((s) => s.key) ?? []);
      this.tokenCount.set(res.estimatedTokens);
    } catch {
      this.toast.show('Render failed', 'error');
    } finally {
      this.loading.set(false);
    }
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
