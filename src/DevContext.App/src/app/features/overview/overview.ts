import { Component, inject } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { Badge } from '../../ui/badge/badge';
import { Icon } from '../../ui/icon/icon';
import { Panel } from '../../ui/panel/panel';
import { Spinner } from '../../ui/spinner/spinner';

@Component({
  selector: 'app-overview',
  imports: [Icon, Badge, Panel, Spinner],
  template: `
    <div class="h-full overflow-auto p-5 space-y-4">
      <div class="flex items-center gap-3">
        <app-icon name="map" [size]="18" class="text-accent" />
        <h2 class="text-sm font-semibold text-ink">Architecture overview</h2>
        @if (session.mapResponse(); as m) {
          <app-badge variant="accent">{{ m.style }}</app-badge>
          @if (m.isLibrary) {
            <app-badge variant="default">library</app-badge>
          }
          @if (m.scopeNote) {
            <app-badge variant="warn" [title]="m.scopeNote">scoped</app-badge>
          }
        }
      </div>

      @if (session.mapResponse(); as m) {
        <div class="grid grid-cols-2 gap-4">
          @if (m.topology.length) {
            <app-panel title="Topology">
              <ul class="p-3 space-y-1 text-xs">
                @for (p of m.topology; track p.name) {
                  <li class="flex items-center gap-2 font-mono text-ink">
                    <app-icon name="boxes" [size]="12" class="text-ink-subtle" />
                    {{ p.name }}
                    @if (p.dependsOn.length) {
                      <span class="text-ink-subtle">&rarr;</span>
                      <span class="text-ink-muted">{{ p.dependsOn.join(', ') }}</span>
                    }
                  </li>
                }
              </ul>
            </app-panel>
          }

          @if (m.packages.length) {
            <app-panel title="Packages">
              <ul class="p-3 space-y-1 text-xs">
                @for (pg of m.packages; track pg.label) {
                  <li class="flex items-center gap-2">
                    <span class="font-medium text-ink">{{ pg.label }}</span>
                    <span class="text-ink-muted">{{ pg.packages.join(', ') }}</span>
                  </li>
                }
              </ul>
            </app-panel>
          }

          @if (m.aggregates.length) {
            <app-panel title="Aggregates">
              <div class="p-3 flex flex-wrap gap-1.5">
                @for (a of m.aggregates; track a) {
                  <app-badge variant="accent">{{ a }}</app-badge>
                }
              </div>
            </app-panel>
          }

          @if (m.pipelineBehaviors.length) {
            <app-panel title="Pipeline">
              <ul class="p-3 space-y-1 text-xs">
                @for (pb of m.pipelineBehaviors; track pb) {
                  <li class="font-mono text-ink">{{ pb }}</li>
                }
              </ul>
            </app-panel>
          }

          <app-panel title="Markdown">
            <pre class="whitespace-pre-wrap font-mono text-xs leading-relaxed text-ink p-3 max-h-96 overflow-auto">{{ session.mapMarkdown() }}</pre>
          </app-panel>
        </div>
      } @else {
        <div class="flex items-center justify-center py-12">
          <app-spinner />
        </div>
      }
    </div>
  `,
})
export class Overview {
  protected readonly session = inject(SessionStore);
}
