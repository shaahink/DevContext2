import { Component, inject, signal } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { Segmented } from '../../ui/segmented/segmented';
import { Spinner } from '../../ui/spinner/spinner';
import { Overview } from '../overview/overview';

const VIEW_OPTIONS = [
  { label: 'Overview', value: 'overview' },
  { label: 'Markdown', value: 'markdown' },
] as const;

@Component({
  selector: 'app-map-panel',
  imports: [Spinner, Segmented, Overview],
  template: `
    @if (session.ready()) {
      <div class="flex h-full flex-col">
        <div class="flex items-center justify-end border-b border-line bg-surface px-3 py-1">
          <app-segmented
            [options]="viewOptions"
            [(selected)]="view"
          />
        </div>
        <div class="min-h-0 flex-1">
          @if (view() === 'overview') {
            <app-overview />
          } @else {
            <div class="h-full overflow-auto p-5">
              <pre class="whitespace-pre-wrap font-mono text-xs leading-relaxed text-ink">{{ session.mapMarkdown() }}</pre>
            </div>
          }
        </div>
      </div>
    } @else if (session.busy()) {
      <div class="flex h-full flex-col items-center justify-center gap-3 text-ink-muted">
        <app-spinner />
        <p class="text-sm">{{ session.progress().message }}</p>
      </div>
    }
  `,
})
export class MapPanel {
  protected readonly session = inject(SessionStore);
  protected readonly viewOptions = VIEW_OPTIONS as unknown as readonly { label: string; value: string }[];
  readonly view = signal('overview');
}
