import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ActivityService } from '../../core/activity/activity.service';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { ViewFrame } from '../../shell/view-frame';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';
import { ToastService } from '../../ui/toast/toast';

interface SectionToggle { key: string; tokens: number; included: boolean; }

@Component({
  selector: 'app-document-view',
  imports: [Icon, Button, FormsModule, ViewFrame],
  template: `
    <app-view-frame>
      <div sidebar class="flex flex-col h-full p-3 space-y-1">
        <p class="mb-2 text-2xs font-semibold uppercase text-ink-subtle">Sections</p>
        @if (sections().length) {
          @for (s of sections(); track s.key) {
            <label class="flex items-center gap-2 rounded px-2 py-1 text-xs text-ink-muted hover:bg-surface-2 cursor-pointer">
              <input type="checkbox" [checked]="s.included" (change)="toggleSection($index)" class="rounded border-line" />
              <span class="flex-1 truncate">{{ s.key }}</span>
              <span class="text-ink-subtle">{{ s.tokens }}</span>
            </label>
          }
        } @else {
          <p class="text-xs text-ink-subtle">Click Render to load sections.</p>
        }
      </div>

      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Document / Export</h2>
        <div class="ml-auto flex items-center gap-2">
          <span class="text-xs text-ink-muted">{{ totalTokens() }} tokens @if (budget() > 0) { <span class="text-ink-subtle">/ {{ budget() }}</span> }</span>
          <input type="range" min="0" max="10000" step="500" class="h-4 w-24" [(ngModel)]="budget" />
          <app-button variant="ghost" size="sm" (click)="renderDoc()" [disabled]="loading()"><app-icon name="refresh" [size]="12" /> Render</app-button>
          <app-button variant="ghost" size="sm" (click)="copyContent()" [disabled]="loading()"><app-icon name="code" [size]="12" /> Copy</app-button>
        </div>
      </div>

      @if (session.ready()) {
        @if (content()) {
          <pre class="whitespace-pre-wrap p-5 font-mono text-xs leading-relaxed text-ink">{{ content() }}</pre>
        } @else {
          <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Click Render to load the document.</p></div>
        }
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted"><p class="text-sm">Analyze a repo to view the document.</p></div>
      }
    </app-view-frame>
  `,
})
export class DocumentView {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);
  private readonly activity = inject(ActivityService);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(false);
  protected readonly sections = signal<SectionToggle[]>([]);
  protected readonly content = signal('');
  protected readonly format = signal('markdown');
  protected readonly budget = signal(4000);

  protected totalTokens(): number { return this.sections().filter((s) => s.included).reduce((n, s) => n + s.tokens, 0); }
  protected toggleSection(index: number): void { this.sections.update((list) => list.map((s, i) => (i === index ? { ...s, included: !s.included } : s))); }

  protected async renderDoc(): Promise<void> {
    const handle = this.session.handle();
    if (!handle || this.loading()) return;
    this.loading.set(true);
    await this.activity.runSecondary('Rendering…', async () => {
      const res = await this.api.render(handle, { format: this.format() });
      this.content.set(res.content);
      this.sections.set(res.sections.map((s) => ({ key: s.key, tokens: s.tokens, included: true })));
    });
    this.loading.set(false);
  }

  protected async copyContent(): Promise<void> {
    const text = this.content();
    if (!text || this.loading()) return;
    this.loading.set(true);
    try {
      await navigator.clipboard.writeText(text);
      this.toast.show('Copied to clipboard', 'success');
    } catch {
      this.toast.show('Clipboard unavailable', 'error');
    }
    this.loading.set(false);
  }
}
