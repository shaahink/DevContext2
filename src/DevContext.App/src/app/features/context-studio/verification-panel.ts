import { Component, input, output } from '@angular/core';

import { Icon } from '../../ui/icon/icon';

/** T5.2 (audit R6) — merged verification across every focus the current cards reference. */
export interface PackVerification {
  readonly anyStale: boolean;
  readonly analyzedGitHead: string;
  readonly currentGitHead: string;
  readonly checkedAt: number;
  readonly sections: readonly SectionVerificationVm[];
}

export interface SectionVerificationVm {
  readonly key: string;
  readonly stale: boolean;
  readonly filesChecked: number;
  readonly changed: readonly { file: string; status: string; lineDelta: number }[];
}

/** T5.2 (audit R6, §5.3 sketch) — the per-section accuracy/staleness ledger. The Studio
 * stops silently serving a pack the disk has moved past: every section says whether the
 * files it cites still match the analyze-time fingerprints (T4.5 server half). */
@Component({
  selector: 'app-verification-panel',
  imports: [Icon],
  template: `
    @if (verification(); as v) {
      <div class="mt-3 border-t border-line pt-2" data-testid="verification-panel">
        <div class="mb-1 flex items-center justify-between">
          <h3 class="flex items-center gap-1 text-2xs font-semibold uppercase tracking-wider"
            [class.text-warn]="v.anyStale"
            [class.text-ink-muted]="!v.anyStale">
            <app-icon [name]="v.anyStale ? 'alert-triangle' : 'check'" [size]="12" />
            Verification
          </h3>
          <button
            type="button"
            class="rounded p-0.5 text-ink-subtle hover:bg-hover hover:text-ink transition-colors disabled:opacity-40"
            data-testid="verification-refresh"
            title="Rebuild the pack and re-check every file it cites against the disk"
            [disabled]="verifying()"
            (click)="refreshRequest.emit()"
          >
            <app-icon name="refresh" [size]="12" [class.animate-spin]="verifying()" />
          </button>
        </div>

        <!-- N1.1 (backlog #28) — checkedAt was set by the Studio and declared here, and the
             template never rendered it: a freshness verdict with no indication of WHEN it was
             taken. A ledger that cannot say when it looked is not evidence. -->
        <p class="mb-1 text-2xs text-ink-subtle tabular-nums" data-testid="verification-checked-at">
          Checked {{ checkedAtLabel(v.checkedAt) }} · covers this pack's sections
        </p>

        @if (v.anyStale) {
          <p class="mb-1 text-2xs text-warn" data-testid="verification-stale">
            Stale — the repo changed since this analysis. Sections below may not match the disk.
          </p>
        } @else {
          <p class="mb-1 text-2xs text-success" data-testid="verification-fresh">
            Fresh — every cited file matches its analyze-time fingerprint.
          </p>
        }

        <ul class="space-y-0.5">
          @for (s of v.sections; track s.key) {
            <li class="text-2xs leading-snug">
              <span class="inline-flex items-center gap-1"
                [class.text-warn]="s.stale"
                [class.text-ink-subtle]="!s.stale">
                <app-icon [name]="s.stale ? 'alert-triangle' : 'check'" [size]="10" />
                {{ s.key }} · {{ s.filesChecked }} file{{ s.filesChecked !== 1 ? 's' : '' }}
              </span>
              @for (d of s.changed; track d.file) {
                <div class="pl-4 font-mono text-2xs text-warn" [title]="d.file">
                  {{ shortFile(d.file) }} — {{ d.status }}{{ d.lineDelta !== 0 ? ' (' + (d.lineDelta > 0 ? '+' : '') + d.lineDelta + ' lines)' : '' }}
                </div>
              }
            </li>
          }
        </ul>

        @if (v.analyzedGitHead && v.currentGitHead && v.analyzedGitHead !== v.currentGitHead) {
          <p class="mt-1 font-mono text-2xs text-warn">
            HEAD {{ v.analyzedGitHead.slice(0, 7) }} → {{ v.currentGitHead.slice(0, 7) }}
          </p>
        }

        @if (v.anyStale) {
          <button
            type="button"
            class="mt-1.5 w-full rounded border border-warn/40 px-2 py-1 text-2xs font-medium text-warn hover:bg-warn/10 transition-colors"
            data-testid="verification-reanalyze"
            (click)="reanalyzeRequest.emit()"
          >
            Re-analyze
          </button>
        }
      </div>
    }
  `,
})
export class VerificationPanel {
  readonly verification = input<PackVerification | null>(null);
  readonly verifying = input(false);

  readonly refreshRequest = output<void>();
  readonly reanalyzeRequest = output<void>();

  /** N1.1 — wall-clock time the ledger was taken; the pack is rebuilt often enough that a
   * relative "2 minutes ago" would need a ticking clock to stay true. */
  protected checkedAtLabel(epochMs: number): string {
    return new Date(epochMs).toLocaleTimeString(undefined, { hour12: false });
  }

  protected shortFile(file: string): string {
    // Keep the tail (filename + one parent) readable in the narrow rail; full path on [title].
    const parts = file.replace(/\\/g, '/').split('/');
    return parts.length <= 2 ? file : parts.slice(-2).join('/');
  }
}
