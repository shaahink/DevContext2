import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';

@Component({
  selector: 'app-onboarding-row',
  imports: [RouterLink],
  template: `
    <div class="flex flex-wrap items-center gap-3 rounded-lg border border-accent-dim bg-accent/5 px-4 py-3">
      <span class="mr-1 text-2xs font-semibold uppercase tracking-wider text-accent">Start here</span>

      @if (checkoutFocus(); as focus) {
        <a class="btn-onboard" [routerLink]="['/explore']" [queryParams]="{ focus: focus }">
          <span class="i-lucide-git-branch h-3.5 w-3.5"></span>
          Trace checkout
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

      <span class="ml-auto text-2xs text-ink-subtle font-mono">pnpm dev:web &rarr; MCP ready</span>
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

  protected readonly checkoutFocus = computed(() => {
    for (const g of this.session.entryGroups()) {
      const checkout = g.entries.find((e) =>
        /checkout/i.test(e.title ?? '') || /checkout/i.test(e.focus ?? ''),
      );
      if (checkout) return checkout.focus;
    }
    return null;
  });
}
