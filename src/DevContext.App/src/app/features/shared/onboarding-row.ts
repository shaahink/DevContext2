import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AtlasStore } from '../../state/atlas.store';
import { SessionStore } from '../../state/session.store';
import { pickHeroFlow } from '../../core/flow-ranking';

@Component({
  selector: 'app-onboarding-row',
  imports: [RouterLink],
  template: `
    <div class="flex flex-wrap items-center gap-3 rounded-lg border border-accent-dim bg-accent/5 px-4 py-3">
      <span class="mr-1 text-2xs font-semibold uppercase tracking-wider text-accent">Start here</span>

      @if (heroFlow(); as hero) {
        <a class="btn-onboard" data-testid="trace-hero" [routerLink]="['/explore']" [queryParams]="{ focus: hero.focus }" [title]="'Trace ' + hero.label">
          <span class="i-lucide-git-branch h-3.5 w-3.5"></span>
          Trace {{ shortLabel(hero.label) }}
        </a>
      }

      <a class="btn-onboard" routerLink="/atlas">
        <span class="i-lucide-boxes h-3.5 w-3.5"></span>
        Open atlas
      </a>

      <a class="btn-onboard" routerLink="/mcp">
        <span class="i-lucide-terminal h-3.5 w-3.5"></span>
        Point your agent here
      </a>
    </div>
  `,
  styles: `
    .btn-onboard {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 10px;
      border-radius: 6px;
      border: 1px solid var(--vibe-line);
      background: var(--vibe-surface);
      color: var(--vibe-ink);
      font-size: 12px;
      text-decoration: none;
      transition: border-color 0.15s, background 0.15s;
    }
    .btn-onboard:hover {
      border-color: var(--vibe-accent);
      background: var(--vibe-surface-2);
    }
  `,
})
export class OnboardingRow {
  protected readonly session = inject(SessionStore);
  private readonly atlas = inject(AtlasStore);

  /** The START-HERE trace tile demos the engine's BEST flow (T6.9, audit B2/B3): a deep
   * checkout flow when the repo has one (the classic demo), else the repo's deepest indexed
   * flow — labeled by what it actually traces. The first-match version landed on WebApp's
   * unwired Blazor `GET /Checkout`; on eShop EVERY checkout-titled entry is a 1-hop client
   * command (CheckoutViewModel.CheckoutAsync = 2 nodes), so a title match alone can't
   * deliver the ≥3-hop gate. While the atlas is still indexing, fall back to a wired
   * NON-UI checkout entry (UI commands stop at the view-model) or show no tile — a tile
   * that appears a few seconds late beats one that opens on a dead end. The old
   * `pnpm dev:web → MCP ready` developer leakage is gone — the agent tile links to the
   * MCP page, which carries the real host configs. */
  protected readonly heroFlow = computed<{ focus: string; label: string } | null>(() => {
    const flows = this.atlas.flows().filter((f) => f.found && f.nodeCount > 1);
    if (flows.length > 0) {
      // R3 D-E (E-2): one ranking rule, the same one Home's and Atlas's Top flows use.
      //
      // This used to prefer ANY flow whose title matched /checkout/i and reached 4 nodes, ahead of
      // the request-shaped preference below it — safe when it was written, because the comment
      // above recorded that every checkout-titled entry on eShop was a 1-hop client command that
      // could not clear the gate. Batches A–E made that false: the resolver got honest, the MAUI
      // `CheckoutViewModel.CheckoutAsync` now reaches four nodes, and the special case started
      // beating the very preference it was written to protect — the tile offered a mobile
      // view-model command as the way into a twelve-service backend. A threshold calibrated on a
      // starved graph inverts when the graph stops being starved; the fix is to rank by what a
      // flow IS, not by what its title says.
      //
      // G10.1: the 4 itself was the last of that calibration and is gone too — it filtered BEFORE
      // the band rule ran, so it could delete the whole request-shaped band first. See pickHeroFlow.
      const best = pickHeroFlow(flows);
      if (best) return { focus: best.focus, label: best.title };
    }

    let best: { focus: string; label: string; score: number } | null = null;
    for (const g of this.session.entryGroups()) {
      if (g.kind === 'UiEntry') continue;
      for (const e of g.entries) {
        if (!/checkout/i.test(e.title ?? '') && !/checkout/i.test(e.focus ?? '')) continue;
        if (!e.target) continue; // never an unwired route
        const score = e.score ?? 0;
        if (!best || score > best.score) best = { focus: e.focus, label: e.title, score };
      }
    }
    return best ? { focus: best.focus, label: best.label } : null;
  });

  /** Tile-sized flow name — middle-ellipsis keeps the distinguishing route tail (T6.8). */
  protected shortLabel(label: string): string {
    if (label.length <= 34) return label;
    return label.slice(0, 16) + '…' + label.slice(-16);
  }
}
