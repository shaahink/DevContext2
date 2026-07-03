import { Injectable, inject } from '@angular/core';

import { PrefsStore } from '../state/prefs.store';
import { SessionStore } from '../state/session.store';
import { WorkspaceStore } from '../state/workspace.store';
import type { AnalyzeSpec } from '../data-access/devcontext-api';
import { isTauri } from './tauri-env';

/**
 * Handles the "single-instance-path" event the Rust side emits (`tauri-plugin-single-instance`'s
 * callback, `lib.rs`) when a second launch — e.g. a path argument from Explorer's "Open with…",
 * or a second CLI invocation — is redirected into the already-running instance. Opens the path
 * as a NEW tab (via `WorkspaceStore.createTab`) rather than replacing whatever the user is
 * already looking at, unlike `Titlebar.selectRecent`/`WebviewShortcutsService.reanalyze`, which
 * deliberately reuse the current tab.
 */
@Injectable({ providedIn: 'root' })
export class SingleInstanceService {
  private readonly workspace = inject(WorkspaceStore);
  private readonly session = inject(SessionStore);
  private readonly prefs = inject(PrefsStore);

  private started = false;

  start(): void {
    if (this.started || !isTauri()) return;
    this.started = true;
    void this.listen();
  }

  private async listen(): Promise<void> {
    const { listen } = await import('@tauri-apps/api/event');
    await listen<string>('single-instance-path', (event) => {
      const path = event.payload;
      if (!path) return;
      const label = path.split(/[\\/]/).pop() || path;
      this.workspace.createTab(path, label);
      const defs = this.prefs.analyzeDefaults();
      const spec: AnalyzeSpec = { path, depth: defs.depth, detail: defs.detail, noRoslyn: defs.noRoslyn, cleanup: defs.cleanup };
      void this.session.analyze(spec);
    });
  }
}
