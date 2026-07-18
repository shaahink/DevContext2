import { Component, computed, input } from '@angular/core';

import type { ServiceStyle } from '../../core/grpc/gen/devcontext/v1/devcontext_pb';
import { projectDisplayName } from '../../core/format';

/** D4.3 (L3): per-service entry mix, computed by AtlasPage from entryGroups —
 * `projectName → [{ label: 'HTTP', count: 12 }, …]`. */
export type EntryMix = ReadonlyMap<string, readonly { label: string; count: number }[]>;

@Component({
  selector: 'app-service-cards',
  template: `
    @if (services().length > 0) {
      <div class="grid grid-cols-2 gap-3">
        @for (svc of services(); track svc.projectName) {
          <div class="service-card">
            <div class="flex items-center justify-between mb-2">
              <span class="font-mono text-xs font-semibold text-ink" [title]="svc.projectName">{{ displayName(svc.projectName) }}</span>
              @if (svc.style) {
                <span class="chip text-2xs">{{ svc.style }}</span>
              }
            </div>
            @if (mixFor(svc.projectName); as mix) {
              <div class="mb-1.5 flex flex-wrap gap-x-3 gap-y-0.5">
                @for (m of mix; track m.label) {
                  <span class="text-2xs tabular-nums text-ink-muted">{{ m.count }} {{ m.label }}</span>
                }
              </div>
            }
            @if (svc.stack.length > 0) {
              <div class="flex flex-wrap gap-1">
                @for (tag of svc.stack; track tag) {
                  <span class="chip text-2xs text-ink-muted bg-surface-2">{{ tag }}</span>
                }
              </div>
            } @else if (!mixFor(svc.projectName)) {
              <span class="text-2xs text-ink-subtle">No stack signals detected</span>
            }
          </div>
        }
      </div>
    } @else {
      <p class="py-4 text-center text-xs text-ink-subtle">No service styles resolved.</p>
    }
  `,
  styles: `
    .service-card {
      padding: 12px;
      border-radius: 8px;
      border: 1px solid var(--vibe-line);
      background: var(--vibe-surface);
    }
  `,
})
export class ServiceCards {
  readonly services = input.required<readonly ServiceStyle[]>();
  /** D4.3: entry mix per project (style + entry mix ARE the card, proposal §2-D4 L3). */
  readonly entryMix = input<EntryMix | null>(null);

  private readonly allNames = computed(() => this.services().map((s) => s.projectName));

  protected mixFor(projectName: string): readonly { label: string; count: number }[] | null {
    const mix = this.entryMix()?.get(projectName);
    return mix && mix.length > 0 ? mix : null;
  }

  /** Common-prefix strip only — never the last dot segment (T6.8, audit A8). */
  protected displayName(name: string): string {
    return projectDisplayName(name, this.allNames());
  }
}
