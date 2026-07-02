import { Component, computed, effect, HostListener, inject, signal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { DevContextApi } from '../../data-access/devcontext-api';
import { ToastService } from '../../ui/toast/toast';
import { SectionCard } from '../../ui/section-card/section-card';
import { Icon } from '../../ui/icon/icon';
import { Button } from '../../ui/button/button';
import { Badge } from '../../ui/badge/badge';
import { TraceNodeComponent } from '../trace/trace-node';

@Component({
  selector: 'app-section-lens',
  imports: [SectionCard, Icon, Button, Badge, TraceNodeComponent],
  template: `
    <app-section-card id="lens" title="Synced Lens" [subtitle]="subtitle()">
      <div class="lens-split">
        <div class="lens-pane lens-human">
          <h3 class="lens-pane-title">Human</h3>

          @if (traceStore.nodeDetail(); as n) {
            <div class="lens-card">
              <div class="lens-card-title">{{ n.title }}</div>
              <div class="space-y-2 text-xs">
                <div class="flex flex-wrap items-center gap-1.5">
                  <app-badge variant="accent">{{ n.kind }}</app-badge>
                  @if (fileShort(n.filePath); as fp) {
                    <span class="font-mono text-ink-muted">{{ fp }}</span>
                  }
                </div>
                @if (n.tags.length) {
                  <div class="flex flex-wrap gap-1">
                    @for (t of n.tags; track t) {
                      <span class="rounded bg-surface-2 px-1.5 py-0.5 text-2xs text-ink-muted">{{ t }}</span>
                    }
                  </div>
                }
                <div class="flex gap-3 text-ink-subtle">
                  <span>In: {{ n.inDegree }}</span>
                  <span>Out: {{ n.outDegree }}</span>
                </div>
              </div>
            </div>
          }

          @if (traceStore.tree(); as root) {
            <div class="lens-card">
              <div class="lens-card-title">Trace</div>
              <div class="max-h-[50vh] overflow-auto">
                <app-trace-node [node]="root" [depth]="0" />
              </div>
            </div>
          } @else if (traceStore.active() && traceStore.loading()) {
            <div class="lens-placeholder">Tracing…</div>
          } @else if (traceStore.active() && !traceStore.found()) {
            <div class="lens-placeholder">Trace not found for this focus.</div>
          }

          @if (!hasHumanContent()) {
            <div class="lens-placeholder">
              <app-icon name="arrow-right" [size]="14" class="inline text-ink-subtle" />
              Select an entry above to see its trace and LLM context side-by-side.
            </div>
          }
        </div>

        <div class="lens-pane lens-llm">
          <div class="flex items-center justify-between mb-2">
            <h3 class="lens-pane-title">LLM Context</h3>
            <div class="flex items-center gap-1.5">
              @if (tokenEstimate() > 0) {
                <span class="text-2xs tabular-nums text-ink-subtle">{{ fmtK(tokenEstimate()) }} tok</span>
              }
              <app-button variant="ghost" size="sm" (click)="copy()" [disabled]="!renderContent()">
                <app-icon name="copy" [size]="11" />
              </app-button>
            </div>
          </div>

          @if (renderContent()) {
            <pre class="lens-content">{{ renderContent() }}</pre>
          } @else if (renderLoading()) {
            <div class="lens-placeholder">
              <app-icon name="loader" [size]="14" class="inline animate-spin mr-2" />
              Rendering…
            </div>
          } @else if (renderError()) {
            <div class="lens-placeholder text-danger">
              <app-icon name="x" [size]="14" class="inline mr-2" />
              {{ renderError() }}
            </div>
          } @else if (!traceStore.active()) {
            <div class="lens-placeholder">LLM context will render here when you select an entry.</div>
          }
        </div>
      </div>
    </app-section-card>
  `,
  host: { class: 'contents' },
})
export class SectionLens {
  protected readonly session = inject(SessionStore);
  protected readonly traceStore = inject(TraceStore);
  private readonly api = inject(DevContextApi);
  private readonly toast = inject(ToastService);

  protected readonly renderContent = signal('');
  protected readonly tokenEstimate = signal(0);
  protected readonly renderLoading = signal(false);
  protected readonly renderError = signal<string | null>(null);

  private renderTimer: ReturnType<typeof setTimeout> | null = null;
  private renderedFocus: string | null = null;

  protected readonly subtitle = computed(() => {
    const f = this.traceStore.focus();
    if (!f) return 'No selection';
    if (f.length > 60) return f.slice(0, 57) + '…';
    return f;
  });

  protected readonly hasHumanContent = computed(() =>
    this.traceStore.active() || this.traceStore.nodeDetail() !== null,
  );

  constructor() {
    effect(() => {
      const focus = this.traceStore.focus();
      if (!focus || focus === this.renderedFocus) return;
      const handle = this.session.handle();
      if (!handle) return;
      this.debouncedRender(handle, focus);
    });
  }

  @HostListener('window:keydown', ['$event'])
  onKeydown(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key === 'c') {
      const content = this.renderContent();
      if (content && document.activeElement?.tagName !== 'INPUT' && document.activeElement?.tagName !== 'TEXTAREA') {
        e.preventDefault();
        void this.copy();
      }
    }
  }

  protected async copy(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.renderContent());
      this.toast.show('Copied LLM context to clipboard', 'info');
    } catch {
      this.toast.show('Copy failed', 'error');
    }
  }

  protected fileShort(path: string | undefined): string | null {
    if (!path) return null;
    const parts = path.replace(/\\/g, '/').split('/');
    return parts.slice(-2).join('/');
  }

  protected fmtK(n: number): string {
    if (n >= 1000) return (n / 1000).toFixed(1) + 'K';
    return String(n);
  }

  private debouncedRender(handle: string, focus: string): void {
    if (this.renderTimer) clearTimeout(this.renderTimer);
    this.renderTimer = setTimeout(() => void this.render(handle, focus), 250);
  }

  private async render(handle: string, focus: string): Promise<void> {
    this.renderedFocus = focus;
    this.renderLoading.set(true);
    this.renderError.set(null);
    this.renderContent.set('');
    try {
      const d = this.traceStore.detail();
      const res = await this.api.render(handle, { focus, detail: d, format: 'markdown' });
      this.renderContent.set(res.content);
      this.tokenEstimate.set(res.estimatedTokens);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Render failed — is the server running?';
      this.renderError.set(msg);
      this.toast.show(msg, 'error');
    } finally {
      this.renderLoading.set(false);
    }
  }
}
