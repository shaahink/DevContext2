import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Atlas stub (proposal W1.6) — the real page (map prose, topology graph, packages,
 * pipeline, Event Wiring Board, Hub Radar) is a W4/W5 build. This exists now so the
 * route resolves and the redesign's shape is visible, per "all regions exist from day
 * one — placeholders where needed." Not linked from the activity bar yet (same
 * unlinked-by-design status as `/explore` until the W4 cutover).
 */
@Component({
  selector: 'app-atlas-page',
  imports: [RouterLink],
  template: `
    <div class="flex h-full flex-col items-center justify-center gap-2 text-center text-xs text-ink-subtle">
      <p>Atlas is under construction (proposal §2, W4/W5).</p>
      <p>
        Architecture and stats live on
        <a routerLink="/overview" class="text-accent hover:underline">Overview</a>
        for now.
      </p>
    </div>
  `,
})
export class AtlasPage {}
