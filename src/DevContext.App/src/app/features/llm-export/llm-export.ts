import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { Icon } from '../../ui/icon/icon';
import { Panel } from '../../ui/panel/panel';
import { Sheet } from '../../ui/sheet/sheet';
import { Button } from '../../ui/button/button';

@Component({
  selector: 'app-llm-export',
  imports: [Sheet, Panel, Icon, Button, FormsModule],
  template: `
    <app-button variant="ghost" size="sm" (click)="open.set(true)" title="Export for LLM">
      <app-icon name="code" [size]="14" /> Export
    </app-button>

    <app-sheet [open]="open()" (closed)="open.set(false)">
      <div class="flex h-full flex-col">
        <div class="flex items-center justify-between border-b border-line px-4 py-3">
          <div class="flex items-center gap-2">
            <app-icon name="code" [size]="15" class="text-accent" />
            <span class="font-medium text-ink">LLM export</span>
          </div>
          <app-button variant="ghost" size="sm" (click)="open.set(false)">
            <app-icon name="x" [size]="14" />
          </app-button>
        </div>

        <div class="flex-1 overflow-y-auto p-4 space-y-4">
          <app-panel title="Format">
            <div class="p-3 flex gap-2">
              <app-button
                [variant]="format() === 'markdown' ? 'primary' : 'secondary'"
                [size]="'sm'"
                (click)="format.set('markdown')"
              >Markdown</app-button>
              <app-button
                [variant]="format() === 'json' ? 'primary' : 'secondary'"
                [size]="'sm'"
                (click)="format.set('json')"
              >JSON</app-button>
            </div>
          </app-panel>

          <app-panel title="Sections">
            <div class="p-3 flex flex-wrap gap-1.5">
              @for (s of sectionsWithTokens(); track s.key) {
                <app-button
                  [variant]="isSectionEnabled(s.key) ? 'primary' : 'secondary'"
                  [size]="'sm'"
                  (click)="toggleSection(s.key)"
                >
                  {{ s.key }} <span class="text-ink-subtle text-[10px]">{{ s.tokens }}t</span>
                </app-button>
              }
            </div>
          </app-panel>

          @if (error()) {
            <div class="rounded bg-danger/10 p-3 text-xs text-danger">{{ error() }}</div>
          }

          @if (exportedContent()) {
            <app-panel title="Export">
              <pre class="whitespace-pre-wrap font-mono text-xs leading-relaxed text-ink p-3 max-h-64 overflow-auto">{{ exportedContent() }}</pre>
            </app-panel>
          }
        </div>

        <div class="border-t border-line p-4 flex gap-2">
          <app-button variant="primary" (click)="generate()" class="flex-1">
            <app-icon name="play" [size]="14" /> Generate
          </app-button>
          <app-button variant="secondary" (click)="copy()" [disabled]="exportedContent() === ''">
            <app-icon name="code" [size]="14" /> Copy
          </app-button>
        </div>
      </div>
    </app-sheet>
  `,
})
export class LlmExport {
  private readonly api = inject(DevContextApi);
  private readonly session = inject(SessionStore);
  private readonly trace = inject(TraceStore);

  readonly open = signal(false);
  readonly format = signal('markdown');
  readonly enabledSections = signal<Set<string>>(new Set());
  readonly sectionsWithTokens = signal<{ key: string; tokens: number }[]>([]);
  readonly exportedContent = signal('');
  readonly estimatedTokens = signal(0);
  readonly error = signal<string | null>(null);

  generate(): void {
    this.error.set(null);
    const handle = this.session.handle();
    if (!handle) return;
    const focus = this.trace.focus() ?? undefined;

    this.api.render(handle, {
      focus,
      format: this.format(),
      sections: [...this.enabledSections()],
      includeDiagnostics: false,
    }).then((res) => {
      this.exportedContent.set(res.content);
      this.estimatedTokens.set(res.estimatedTokens);
      this.sectionsWithTokens.set(res.sections.map((s) => ({ key: s.key, tokens: s.tokens })));
      for (const s of res.sections) {
        this.enabledSections.update((set) => {
          const n = new Set(set);
          n.add(s.key);
          return n;
        });
      }
    }).catch((err) => {
      this.error.set(String(err));
    });
  }

  isSectionEnabled(key: string): boolean {
    return this.enabledSections().has(key);
  }

  toggleSection(key: string): void {
    this.enabledSections.update((set) => {
      const n = new Set(set);
      if (n.has(key)) n.delete(key);
      else n.add(key);
      return n;
    });
  }

  async copy(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.exportedContent());
    } catch { /* ignore */ }
  }
}
