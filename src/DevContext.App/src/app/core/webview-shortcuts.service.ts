import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { PrefsStore } from '../state/prefs.store';
import { SessionStore } from '../state/session.store';
import { SnapshotDiffStore } from '../state/snapshot-diff.store';
import { WorkspaceStore } from '../state/workspace.store';
import type { AnalyzeSpec } from '../data-access/devcontext-api';

/**
 * WebView keyboard interception (proposal §7.3) — "silent killers." WebView2 answers a
 * handful of browser-chrome shortcuts itself unless the page calls `preventDefault()`
 * during dispatch: Ctrl+P (print), Ctrl+R/F5 (reload — destroys all tab state, the
 * worst one), Ctrl+F (find), Ctrl+ +/-/0 (zoom, which would drift the carefully sized
 * 13px UI base). A capture-phase `document` listener (installed once, here, ahead of
 * any other same-window handler) reroutes each to something useful instead of just
 * swallowing it.
 *
 * Ctrl+R's "re-analyze" is the light version for W1: it re-runs `analyze()` on the
 * active tab's own path. Full focus-restore (re-select the same trace/node afterward)
 * needs TraceStore coordination and is deferred — see AGENTS.md's F-track status.
 */
@Injectable({ providedIn: 'root' })
export class WebviewShortcutsService {
  private readonly router = inject(Router);
  private readonly workspace = inject(WorkspaceStore);
  private readonly session = inject(SessionStore);
  private readonly prefs = inject(PrefsStore);
  private readonly snapshotDiff = inject(SnapshotDiffStore);

  private started = false;

  start(): void {
    if (this.started) return;
    this.started = true;
    document.addEventListener('keydown', this.onKeydown, { capture: true });
  }

  private readonly onKeydown = (e: KeyboardEvent): void => {
    // F5 (no modifier) is a reload trigger on its own — check before the ctrl guard below.
    if (e.key === 'F5') {
      e.preventDefault();
      return;
    }

    if (!(e.ctrlKey || e.metaKey)) return;

    if (e.key === 'p') {
      e.preventDefault();
      window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true }));
      return;
    }

    if (e.key === 'r') {
      e.preventDefault();
      this.reanalyze();
      return;
    }

    if (e.key === 'f') {
      e.preventDefault();
      this.focusDeckFilter();
      return;
    }

    if (e.key === '=' || e.key === '+' || e.key === '-' || e.key === '0') {
      e.preventDefault();
    }
  };

  private reanalyze(): void {
    const tab = this.workspace.activeTab();
    if (!tab?.path) return;
    const path = tab.path;
    this.snapshotDiff.captureBaseline(path);
    const defs = this.prefs.analyzeDefaults();
    const spec: AnalyzeSpec = { path, depth: defs.depth, detail: defs.detail, noRoslyn: defs.noRoslyn, cleanup: defs.cleanup };
    void this.session.analyze(spec).then(() => this.snapshotDiff.armReport(path));
  }

  private focusDeckFilter(): void {
    if (!this.router.url.startsWith('/explore')) return;
    document.querySelector<HTMLInputElement>('app-entry-deck input[type="text"]')?.focus();
  }
}
