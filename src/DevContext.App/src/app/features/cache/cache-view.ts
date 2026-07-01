import { Component, inject } from '@angular/core';

import { RecentStore } from '../../state/recent.store';
import { SessionStore } from '../../state/session.store';
import { ViewFrame } from '../../shell/view-frame';
import { Card } from '../../ui/card/card';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-cache-view',
  imports: [Icon, Card, ViewFrame],
  template: `
    <app-view-frame [showSidebar]="false">
      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <h2 class="text-sm font-semibold text-ink">Cache &amp; Data</h2>
      </div>

      <div class="overflow-auto p-5 space-y-5">
        @if (session.ready()) {
          <app-card>
            <h3 class="text-sm font-medium text-ink">Analysis summary</h3>
            <div class="mt-3 grid grid-cols-3 gap-3 text-xs">
              <div><span class="text-ink-subtle">Projects</span><p class="font-mono text-ink">{{ session.summary()?.projects ?? 0 }}</p></div>
              <div><span class="text-ink-subtle">Nodes</span><p class="font-mono text-ink">{{ session.summary()?.nodes ?? 0 }}</p></div>
              <div><span class="text-ink-subtle">Edges</span><p class="font-mono text-ink">{{ session.summary()?.edges ?? 0 }}</p></div>
              <div><span class="text-ink-subtle">Entries</span><p class="font-mono text-ink">{{ session.summary()?.entries ?? 0 }}</p></div>
              <div><span class="text-ink-subtle">Wired</span><p class="font-mono text-ink">{{ session.summary()?.entriesWithTarget ?? 0 }}</p></div>
              <div><span class="text-ink-subtle">Coverage</span><p class="font-mono text-ink">{{ coverage() }}%</p></div>
            </div>
          </app-card>
        } @else {
          <p class="text-xs text-ink-muted">Analyze a repo to see cache stats.</p>
        }

        <app-card>
          <h3 class="text-sm font-medium text-ink">Recent repositories</h3>
          @if (recents().length) {
            <div class="mt-3 space-y-1.5">
              @for (r of recents(); track r.path) {
                <div class="flex items-center justify-between rounded border border-line bg-surface-2 px-3 py-2">
                  <div class="min-w-0 flex-1"><p class="truncate font-mono text-xs text-ink">{{ r.label }}</p><p class="truncate text-2xs text-ink-subtle">{{ r.path }}</p></div>
                  <button class="ml-2 text-ink-subtle hover:text-danger" (click)="removeRecent(r.path)" title="Remove from recents"><app-icon name="x" [size]="12" /></button>
                </div>
              }
            </div>
          } @else { <p class="mt-3 text-xs text-ink-muted">No recent repos yet.</p> }
        </app-card>

        <app-card>
          <h3 class="text-sm font-medium text-ink">Clone management</h3>
          <p class="mt-0.5 text-xs text-ink-muted">
            Cloned GitHub repos are cleaned up after analysis unless "Keep" is selected in advanced options.
            Only clones created by DevContext are managed — local paths are never touched.
          </p>
        </app-card>
      </div>
    </app-view-frame>
  `,
})
export class CacheView {
  protected readonly session = inject(SessionStore);
  private readonly recentStore = inject(RecentStore);
  protected readonly recents = this.recentStore.recents;

  protected coverage(): string {
    const s = this.session.summary();
    if (!s || s.entries === 0) return '0';
    return Math.round((s.entriesWithTarget / s.entries) * 100).toString();
  }

  protected removeRecent(path: string): void { this.recentStore.remove(path); }
}
