import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { ViewFrame } from '../../shell/view-frame';
import { Badge } from '../../ui/badge/badge';
import { Card } from '../../ui/card/card';
import { Icon } from '../../ui/icon/icon';

@Component({
  selector: 'app-overview-view',
  imports: [Icon, Badge, Card, DecimalPipe, ViewFrame],
  template: `
    <app-view-frame [showSidebar]="false">
      <div header class="flex items-center gap-3 border-b border-line bg-surface px-3 py-2">
        <app-icon name="map" [size]="16" class="text-accent" />
        <h2 class="text-sm font-semibold text-ink">Architecture overview</h2>
        @if (session.mapResponse(); as m) {
          <app-badge variant="accent">{{ m.style }}</app-badge>
          @if (m.isLibrary) {
            <app-badge variant="default">Library</app-badge>
          }
        }
      </div>

      @if (session.ready()) {
        <div class="p-5 space-y-5">
          @if (session.mapResponse(); as m) {
            <div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
              <app-card>
                <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Archetype</h3>
                <div class="mt-2 space-y-1.5">
                  <p class="text-sm text-ink">
                    <span class="font-medium">{{ m.archetype }}</span>
                    @if (m.styleConfidence > 0) {
                      <span class="ml-2 text-ink-muted">{{ (m.styleConfidence * 100) | number:'1.0-0' }}% confidence</span>
                    }
                  </p>
                  @if (m.styleEvidence) {
                    <p class="text-xs text-ink-muted">{{ m.styleEvidence }}</p>
                  }
                </div>
              </app-card>

              @if (m.scopeNote) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Scope</h3>
                  <p class="mt-2 text-xs text-ink-muted">{{ m.scopeNote }}</p>
                </app-card>
              }

              @if (m.topology.length) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Topology</h3>
                  <div class="mt-2 space-y-2">
                    @for (p of m.topology; track p.name) {
                      <div class="rounded border border-line bg-surface-2 p-2">
                        <p class="font-mono text-xs font-medium text-ink">{{ p.name }}</p>
                        @if (p.dependsOn.length) {
                          <div class="mt-1 flex flex-wrap items-center gap-1">
                            <span class="text-2xs text-ink-subtle">depends on</span>
                            @for (d of p.dependsOn; track d) {
                              <span class="rounded bg-base px-1.5 py-0.5 font-mono text-2xs text-ink-muted">{{ d }}</span>
                            }
                          </div>
                        }
                      </div>
                    }
                  </div>
                </app-card>
              }

              @if (m.packages.length) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Packages</h3>
                  <div class="mt-2 space-y-2">
                    @for (pg of m.packages; track pg.label) {
                      <div>
                        <p class="text-xs font-medium text-ink">{{ pg.label }}</p>
                        <p class="text-3xs text-ink-muted">{{ pg.packages.join(', ') }}</p>
                      </div>
                    }
                  </div>
                </app-card>
              }

              @if (m.aggregates.length) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Aggregates</h3>
                  <div class="mt-2 flex flex-wrap gap-1.5">
                    @for (a of m.aggregates; track a) { <app-badge variant="accent">{{ a }}</app-badge> }
                  </div>
                </app-card>
              }

              @if (m.pipelineBehaviors.length) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Pipeline</h3>
                  <div class="mt-2 space-y-1">
                    @for (pb of m.pipelineBehaviors; track pb) { <p class="font-mono text-xs text-ink">{{ pb }}</p> }
                  </div>
                </app-card>
              }

              @if (m.surface?.groups?.length) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Library Surface</h3>
                  <div class="mt-2 space-y-2">
                    @for (g of m.surface.groups; track g.namespace) {
                      <div>
                        <p class="text-xs font-medium text-ink">{{ g.namespace }}</p>
                        <div class="mt-1 flex flex-wrap gap-1">
                          @for (t of g.types; track t.name) {
                            <app-badge variant="default">{{ t.kind }}: {{ t.name }}</app-badge>
                          }
                        </div>
                      </div>
                    }
                  </div>
                </app-card>
              }

              @if (m.surface?.extensionPoints?.length) {
                <app-card>
                  <h3 class="text-xs font-semibold uppercase tracking-wide text-ink-subtle">Extension Points</h3>
                  <div class="mt-2 flex flex-wrap gap-1.5">
                    @for (ep of m.surface.extensionPoints; track ep) { <app-badge variant="accent">{{ ep }}</app-badge> }
                  </div>
                </app-card>
              }
            </div>
          }
        </div>
      } @else {
        <div class="flex h-full items-center justify-center text-ink-muted">
          <p class="text-sm">Analyze a repo to see the architecture overview.</p>
        </div>
      }
    </app-view-frame>
  `,
})
export class OverviewView {
  protected readonly session = inject(SessionStore);
}
