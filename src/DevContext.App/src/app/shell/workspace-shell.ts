import { Component, effect, HostListener, inject, OnDestroy, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

import { AtlasStore } from '../state/atlas.store';
import { ConnectionStore } from '../state/connection.store';
import { SessionStore } from '../state/session.store';
import { ThemeService } from '../core/theme/theme.service';
import { TICKER_PRIORITY, TickerService, type TickerItem } from '../core/ticker.service';
import { WebviewShortcutsService } from '../core/webview-shortcuts.service';
import { SingleInstanceService } from '../core/single-instance.service';
import { Titlebar } from './titlebar/titlebar';
import { TabStrip } from './tab-strip';
import { OfflineBanner } from './offline-banner';
import { ActivityBar } from './activity-bar';
import { Statusbar } from './statusbar/statusbar';
import { Omnibox } from '../features/omnibox/omnibox';
import { NodeCard } from '../features/node-card/node-card';
import { NodePeek } from '../features/peek/node-peek';

/** Static filler tips (proposal §6 "at most 1-in-4") — posted once at shell startup;
 * `TickerService` persists which ones a user has already seen across sessions. */
const STATUSBAR_TIPS: readonly TickerItem[] = [
  { id: 'tip:ctrl-k', text: 'Ctrl+K opens the omnibox — search entries, nodes, or run a command', priority: TICKER_PRIORITY.tip },
  { id: 'tip:shift-e', text: 'Shift+E opens the full entry audit table', priority: TICKER_PRIORITY.tip },
  { id: 'tip:pin', text: 'Press p to pin a trail step into your export pack', priority: TICKER_PRIORITY.tip },
  { id: 'tip:esc-ladder', text: 'Escape backs out one step at a time: cancel, close, deselect, clear', priority: TICKER_PRIORITY.tip },
];

/** Must match `activity-bar.ts`'s `railItems` route+shortKey pairing exactly — that
 * array is the source of truth (proposal §8.1's 5-icon rail); this is a second,
 * hand-kept copy purely because the rail's items also carry icon/badge/requiresSession
 * concerns that don't belong in a keyboard-shortcut table. The pre-W7 version of this
 * map drifted from the rail during the W4 route cutover (stale /overview,/entries,
 * /trace,/graph,/export; missing 'h' for Home and 'a' for Atlas entirely) — GAP-T2. */
const VIEW_SHORTCUTS: Record<string, string> = {
  h: '/',
  e: '/explore',
  a: '/atlas',
  i: '/insights',
  m: '/mcp',
  s: '/settings',
};

/** Full §8.4 keyboard map (GAP-T2) — every row here corresponds to a real, currently
 * wired handler: `omnibox.ts` (Ctrl+K/P), `tab-strip.ts` (Ctrl+T/W/1-6/Tab), this file
 * (g-prefix nav, ?, Escape-for-help), `workbench-page.ts`'s `onGlobalKey`/`onEscape`
 * (everything else — Ctrl+Shift+L, Ctrl+E, Ctrl+Z/Y, Alt+←→, p, v-prefix, the full
 * Esc-ladder, j/k//,Enter,Shift+E live in `entry-deck.ts`). */
const SHORTCUT_HELP = [
  { keys: 'Ctrl+K / Ctrl+P', desc: 'Omnibox — Tab cycles verbs: Trace · Node · Usages · Copy' },
  { keys: 'Ctrl+T', desc: 'New tab' },
  { keys: 'Ctrl+W', desc: 'Close active tab' },
  { keys: 'Ctrl+1–6', desc: 'Jump to tab' },
  { keys: 'Ctrl+Tab / Ctrl+Shift+Tab', desc: 'Cycle tabs (MRU)' },
  { keys: 'Ctrl+Shift+L', desc: 'Inspector dock toggle (0 ↔ last)' },
  { keys: 'Ctrl+E', desc: 'Export drawer' },
  { keys: 'Ctrl+Z / Ctrl+Y · Alt+←/→', desc: 'Trail undo / redo' },
  { keys: 'Ctrl+R', desc: 'Re-analyze (restores focus)' },
  { keys: 'j / k', desc: 'Scrub the entry deck' },
  { keys: '/', desc: 'Filter the entry deck' },
  { keys: 'Enter', desc: 'Trace the selected entry' },
  { keys: 'Shift+E', desc: 'Open the entry audit table' },
  { keys: 'v t / v g / v s / v n', desc: 'Stage: tree · graph · system · node' },
  { keys: 'p', desc: 'Pin the current selection to the trail' },
  { keys: 'g h/e/a/i/m/s', desc: 'Go to Home / Explore / Atlas / Insights / MCP / Settings' },
  { keys: '?', desc: 'Show this help' },
  { keys: 'Escape', desc: 'Esc-ladder: cancel trace → close overlay → unpin peek → deselect node → clear focus → clear filter' },
];

/**
 * Workspace shell (proposal §8.1) — W1 regrid. Titlebar(30) / TabStrip(32) /
 * OfflineBanner(24, conditional) / main / Statusbar(22), all normal document-flow rows
 * now instead of `fixed`-positioned bars with manual `calc(100vh - ...)` height math.
 *
 * The Trail row (proposal's 22px row between TabStrip and main) stays page-owned inside
 * WorkbenchPage rather than promoted here — TrailBar's `restore` output needs a
 * session-handle-aware re-tracer, which only WorkbenchPage has today. Promoting it here
 * would need that logic duplicated (or hoisted) for every route, which is explicitly a
 * W4 decision per the skeleton HANDOFF, not a W1 one.
 */
@Component({
  selector: 'app-workspace-shell',
  imports: [RouterOutlet, Titlebar, TabStrip, OfflineBanner, ActivityBar, Statusbar, Omnibox, NodeCard, NodePeek],
  template: `
    <app-titlebar />
    <app-tab-strip />
    <app-offline-banner />
    <div class="flex min-h-0 flex-1 overflow-hidden">
      <app-activity-bar />
      <main class="min-w-0 flex-1 overflow-y-auto">
        <router-outlet />
      </main>
    </div>
    <app-statusbar />
    <app-omnibox />
    <app-node-card />
    <app-node-peek />

    @if (helpOpen()) {
      <div class="fixed inset-0 z-[60] flex items-center justify-center" (click)="helpOpen.set(false)" (keydown.escape)="helpOpen.set(false)" role="dialog" tabindex="0">
        <div class="absolute inset-0 bg-base/80 backdrop-blur-sm"></div>
        <div class="overlay-float relative max-h-[70vh] w-[560px] overflow-y-auto" (click)="$event.stopPropagation()" (keydown)="$event.stopPropagation()" tabindex="-1">
          <div class="flex items-center justify-between border-b border-line px-4 py-3">
            <h2 class="text-sm font-semibold text-ink">Keyboard Shortcuts</h2>
            <button class="px-1 text-xs text-ink-muted hover:text-ink" (click)="helpOpen.set(false)" (keydown.enter)="helpOpen.set(false)" (keydown.space)="helpOpen.set(false); $event.preventDefault()">✕</button>
          </div>
          <div class="space-y-2 p-4">
            @for (s of helpItems; track s.keys) {
              <div class="flex items-start justify-between gap-4">
                <span class="text-xs text-ink">{{ s.desc }}</span>
                <span class="kbd shrink-0 whitespace-nowrap">{{ s.keys }}</span>
              </div>
            }
          </div>
        </div>
      </div>
    }

    @if (gPending()) {
      <div class="fixed bottom-12 left-1/2 z-50 -translate-x-1/2 overlay-float px-3 py-1.5 font-mono text-xs text-ink-muted">
        Press a key to navigate (<kbd class="text-accent">?</kbd> for help)
      </div>
    }
  `,
  host: { class: 'flex h-screen flex-col' },
})
export class WorkspaceShell implements OnDestroy {
  private readonly router = inject(Router);
  protected readonly helpItems = SHORTCUT_HELP;
  protected readonly helpOpen = signal(false);

  private gTimer: ReturnType<typeof setTimeout> | null = null;
  protected readonly gPending = signal(false);

  constructor() {
    inject(ConnectionStore).start();
    inject(ThemeService);
    inject(WebviewShortcutsService).start();
    inject(SingleInstanceService).start();

    const session = inject(SessionStore);
    const atlas = inject(AtlasStore);
    const ticker = inject(TickerService);

    for (const tip of STATUSBAR_TIPS) ticker.post(tip);

    // Analysis facts (§6). `replaceGroup` — NOT `dismissAll` + `post` — see that
    // method's doc comment: two separate writes to the same signal in one effect
    // execution reproducibly freezes the tab (found the hard way in this checkpoint).
    effect(() => {
      const s = session.summary();
      const item: TickerItem | null =
        session.ready() && s
          ? {
              id: `active:analysis:${s.label}`,
              text: `${s.label} — ${s.nodes} nodes, ${s.edges} edges, ${s.entries} entries`,
              icon: 'check',
              priority: TICKER_PRIORITY.analysis,
            }
          : null;
      ticker.replaceGroup('active:analysis:', item ? [item] : []);
    });

    // Engine insight headlines (§6).
    effect(() => {
      const items: TickerItem[] = session.insights().slice(0, 5).map((i) => ({
        id: `active:insight:${i.id}`,
        text: i.title,
        icon: 'zap',
        link: '/insights',
        priority: TICKER_PRIORITY.insight,
      }));
      ticker.replaceGroup('active:insight:', items);
    });

    // Atlas discoveries (§6) — only once indexing settles, so partial/still-changing
    // scores don't cycle a "discovery" through the ticker and then quietly change.
    effect(() => {
      const items: TickerItem[] =
        atlas.status() === 'done'
          ? [...atlas.flows()]
              .filter((f) => f.found && f.boundaryCrossings > 0)
              .sort((a, b) => b.score - a.score)
              .slice(0, 3)
              .map((f) => ({
                id: `active:atlas:${f.focus}`,
                text: `${f.title} crosses ${f.boundaryCrossings} boundar${f.boundaryCrossings === 1 ? 'y' : 'ies'}`,
                icon: 'boxes',
                link: `/explore?focus=${encodeURIComponent(f.focus)}`,
                priority: TICKER_PRIORITY.atlas,
              }))
          : [];
      ticker.replaceGroup('active:atlas:', items);
    });
  }

  @HostListener('window:keydown', ['$event'])
  onKeydown(e: KeyboardEvent): void {
    if (e.ctrlKey || e.metaKey || e.altKey) return;
    const active = document.activeElement;
    if (active?.tagName === 'INPUT' || active?.tagName === 'TEXTAREA') return;

    if (e.key === '?') {
      e.preventDefault();
      this.helpOpen.update((v) => !v);
      return;
    }

    if (e.key === 'Escape') {
      this.helpOpen.set(false);
      return;
    }

    if (e.key === 'g') {
      e.preventDefault();
      this.gPending.set(true);
      if (this.gTimer) clearTimeout(this.gTimer);
      this.gTimer = setTimeout(() => this.gPending.set(false), 1500);
      return;
    }

    if (this.gPending()) {
      const route = VIEW_SHORTCUTS[e.key];
      if (route) {
        e.preventDefault();
        void this.router.navigateByUrl(route);
      }
      this.gPending.set(false);
      if (this.gTimer) clearTimeout(this.gTimer);
    }
  }

  ngOnDestroy(): void {
    if (this.gTimer) clearTimeout(this.gTimer);
  }
}
