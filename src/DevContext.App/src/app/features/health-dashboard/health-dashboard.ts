import { Component, effect, inject, signal } from '@angular/core';

import type { StatsResponse } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { DevContextApi } from '../../data-access/devcontext-api';
import { SessionStore } from '../../state/session.store';
import { Badge } from '../../ui/badge/badge';
import { Button } from '../../ui/button/button';
import { Icon } from '../../ui/icon/icon';
import { Panel } from '../../ui/panel/panel';
import { Sheet } from '../../ui/sheet/sheet';

function scoreColor(v: number, thresholds: [number, number]): 'success' | 'warn' | 'danger' {
  if (v >= thresholds[0]) return 'success';
  if (v >= thresholds[1]) return 'warn';
  return 'danger';
}

@Component({
  selector: 'app-health-dashboard',
  imports: [Sheet, Panel, Icon, Badge, Button],
  template: `
    <app-button variant="ghost" size="sm" (click)="open.set(true)" title="Architecture health">
      <app-icon name="dot" [size]="14" [style.color]="overallHealth()" /> Health
    </app-button>

    <app-sheet [open]="open()" (closed)="open.set(false)">
      <div class="flex h-full flex-col">
        <div class="flex items-center justify-between border-b border-line px-4 py-3">
          <div class="flex items-center gap-2">
            <app-icon name="dot" [size]="15" [style.color]="overallHealth()" />
            <span class="font-medium text-ink">Architecture health</span>
          </div>
          <app-button variant="ghost" size="sm" (click)="open.set(false)">
            <app-icon name="x" [size]="14" />
          </app-button>
        </div>

        <div class="flex-1 overflow-y-auto p-4 space-y-4">
          <app-panel title="Style adherence">
            <div class="p-3 space-y-2 text-xs">
              <div class="flex justify-between">
                <span class="text-ink-muted">Style</span>
                <span class="font-medium text-ink">{{ session.mapResponse()?.style ?? 'Unknown' }}</span>
              </div>
              @if (session.mapResponse()?.styleConfidence; as conf) {
                <div class="flex justify-between">
                  <span class="text-ink-muted">Confidence</span>
                  <app-badge [variant]="scoreColor(conf, [0.7, 0.4])">{{ (conf * 100).toFixed(0) }}%</app-badge>
                </div>
              }
              @if (session.mapResponse()?.styleEvidence) {
                <div class="text-ink-subtle italic">via {{ session.mapResponse()?.styleEvidence }}</div>
              }
            </div>
          </app-panel>

          @if (stats(); as s) {
            <app-panel title="Wiring coverage">
              <div class="p-3 space-y-2 text-xs">
                <div class="flex justify-between">
                  <span class="text-ink-muted">Entries with target</span>
                  <span class="font-mono text-ink">{{ s.graph?.entriesWithTarget ?? 0 }} / {{ s.graph?.entries ?? 0 }}</span>
                </div>
                @if (s.seams.length) {
                  <div class="mt-2 space-y-1">
                    <span class="text-ink-subtle">Edge quality by seam</span>
                    @for (sm of s.seams; track sm.seam) {
                      <div class="flex items-center justify-between text-[11px]">
                        <span class="text-ink">{{ sm.seam }}</span>
                        <span class="font-mono">
                          <span class="text-ink">{{ sm.count - sm.approx }}</span>
                          <span class="text-ink-muted"> verified</span>
                          <span class="text-ink-subtle"> / {{ sm.approx }} approx</span>
                        </span>
                      </div>
                    }
                  </div>
                }
              </div>
            </app-panel>

            <app-panel title="Graph size">
              <div class="p-3 grid grid-cols-3 gap-2 text-xs">
                <div class="text-center">
                  <span class="block font-mono text-lg text-ink">{{ s.graph?.nodes }}</span>
                  <span class="text-ink-subtle">nodes</span>
                </div>
                <div class="text-center">
                  <span class="block font-mono text-lg text-ink">{{ s.graph?.edges }}</span>
                  <span class="text-ink-subtle">edges</span>
                </div>
                <div class="text-center">
                  <span class="block font-mono text-lg text-ink">{{ s.graph?.entries }}</span>
                  <span class="text-ink-subtle">entries</span>
                </div>
              </div>
            </app-panel>

            <app-panel title="Performance">
              <div class="p-3 space-y-1 text-xs">
                <div class="flex justify-between">
                  <span class="text-ink-muted">Total time</span>
                  <span class="font-mono text-ink">{{ s.totalWallMs }} ms</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-ink-muted">Corpus</span>
                  <span class="font-mono text-ink">{{ s.corpus?.csharpFiles ?? 0 }} C# files / {{ s.corpus?.totalFiles ?? 0 }} files</span>
                </div>
              </div>
            </app-panel>
          } @else {
            <div class="flex items-center justify-center py-12 text-xs text-ink-subtle">
              <app-button variant="ghost" size="sm" (click)="load()">Load stats</app-button>
            </div>
          }
        </div>

        <div class="border-t border-line p-4">
          <app-button variant="primary" (click)="load()" class="w-full">
            <app-icon name="refresh" [size]="14" /> Refresh
          </app-button>
        </div>
      </div>
    </app-sheet>
  `,
})
export class HealthDashboard {
  protected readonly session = inject(SessionStore);
  private readonly api = inject(DevContextApi);

  readonly open = signal(false);
  readonly stats = signal<StatsResponse | null>(null);

  readonly overallHealth = signal('#3fb950');

  constructor() {
    effect(() => {
      if (this.open()) void this.load();
    });
  }

  async load(): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    try {
      const s = await this.api.getStats(handle);
      this.stats.set(s);
      const coverage = s.graph ? (s.graph.entriesWithTarget / Math.max(s.graph.entries, 1)) : 0;
      const confidence = this.session.mapResponse()?.styleConfidence ?? 0;
      const avg = (coverage * 0.5 + confidence * 0.3 + (s.seams.length > 0 ? 0.2 : 0));
      this.overallHealth.set(avg >= 0.7 ? '#3fb950' : avg >= 0.4 ? '#d29922' : '#f85149');
    } catch { /* ignore */ }
  }

  protected scoreColor(v: number, thresholds: [number, number]): 'success' | 'warn' | 'danger' {
    return scoreColor(v, thresholds);
  }
}
