import { Component, inject } from '@angular/core';
import { ActivityService } from '../core/activity/activity.service';
import { SessionStore } from '../state/session.store';

@Component({
  selector: 'app-status-bar',
  template: `
    <div class="flex h-full items-center gap-2 border-t border-line bg-surface px-3 text-3xs text-ink-muted">
      @if (activity.state() === 'busy') {
        <span class="flex items-center gap-1.5">
          <span class="inline-block h-2 w-2 animate-pulse rounded-full bg-accent"></span>
          {{ activity.label() }}
          @if (activity.percent() > 0) {
            <span class="text-ink-subtle">{{ activity.percent() }}%</span>
          }
        </span>
        <span class="text-ink-subtle">{{ activity.stage() }}</span>
      } @else if (activity.state() === 'error') {
        <span class="text-danger">{{ activity.label() }}</span>
      } @else if (session.status() === 'ready') {
        <span>{{ session.summary()?.label ?? 'Ready' }} &middot; {{ session.entryCount() }} entries</span>
      } @else {
        <span>Ready</span>
      }
    </div>
  `,
  host: { class: 'col-span-3' },
})
export class StatusBar {
  protected readonly activity = inject(ActivityService);
  protected readonly session = inject(SessionStore);
}
