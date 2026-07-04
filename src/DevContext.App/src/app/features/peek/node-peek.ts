import { Component, HostListener, inject } from '@angular/core';
import { Router } from '@angular/router';
import { NodePeekStore } from '../../state/node-peek.store';
import { Skeleton } from '../../ui/skeleton/skeleton';

/**
 * node-peek — the 200ms-hover card (proposal §8.1/§10 W7.1), deliberately lighter than
 * `NodeCard` (real fields only: kind, location, degree — no neighbor lists, no actions).
 * Ctrl pins it open past `mouseleave`; Escape/click-outside/✕ dismiss a pinned peek.
 * Single global instance mounted in `workspace-shell.ts`, positioned near whichever
 * `NodeLink` triggered it via `NodePeekStore.anchor()`.
 */
@Component({
  selector: 'app-node-peek',
  standalone: true,
  imports: [Skeleton],
  template: `
    @if (store.nodeId(); as id) {
      @if (store.anchor(); as pos) {
        <div
          class="overlay-float fixed z-50 w-64 text-xs"
          [style.left.px]="clampedX(pos.x)"
          [style.top.px]="clampedY(pos.y)"
          (click)="$event.stopPropagation()"
          (keydown)="$event.stopPropagation()"
          (mouseenter)="store.cancelHide()"
          (mouseleave)="store.requestHide()"
          tabindex="-1"
        >
          <div class="flex items-center justify-between border-b border-line px-2 py-1">
            <span class="truncate font-mono text-2xs text-ink-muted" [title]="id">{{ id }}</span>
            @if (store.pinned()) {
              <button class="px-1 text-ink-muted hover:text-ink" (click)="store.dismiss()" title="Close">✕</button>
            }
          </div>
          <div class="p-2 space-y-1.5">
            @if (store.loading()) {
              <app-skeleton height="0.875rem" width="70%" />
              <app-skeleton height="0.75rem" width="45%" />
              <app-skeleton height="0.75rem" width="90%" />
            } @else if (store.error()) {
              <p class="text-danger text-2xs">Failed to load node.</p>
            } @else if (store.node(); as n) {
              @if (n.found) {
                <p class="text-sm font-medium text-ink truncate" [title]="n.title">{{ n.title }}</p>
                <p class="text-2xs text-ink-muted">{{ n.kind }}</p>
                @if (n.filePath) {
                  <p class="font-mono text-2xs text-ink-subtle truncate" [title]="n.filePath">{{ n.filePath }}</p>
                }
                <p class="text-2xs text-ink-muted">In {{ n.inDegree ?? 0 }} · Out {{ n.outDegree ?? 0 }}</p>
              } @else {
                <p class="text-2xs text-ink-subtle">Node not found.</p>
              }
            }
          </div>
        </div>
      }
    }
  `,
})
export class NodePeek {
  protected readonly store = inject(NodePeekStore);
  private readonly router = inject(Router);

  clampedX(x: number): number {
    return Math.max(4, Math.min(x, window.innerWidth - 264));
  }

  clampedY(y: number): number {
    return Math.max(4, Math.min(y, window.innerHeight - 120));
  }

  /** `/explore`'s own Esc-ladder (`workbench-page.ts`'s `onEscape`) owns "unpin peek" as
   * one ordered rung (after closing overlays, before deselecting the node) — deferring
   * to it here avoids two independent global listeners racing on the same keypress,
   * where this one firing first would clear `nodeId()` before the ladder ever saw it
   * open, silently skipping straight to deselecting the node. Every other route (Home,
   * Atlas, Insights, Settings) has no ladder of its own, so this is the only handler. */
  @HostListener('window:keydown.escape')
  onEscape(): void {
    if (this.router.url.startsWith('/explore')) return;
    if (this.store.nodeId()) this.store.dismiss();
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.store.pinned()) this.store.dismiss();
  }
}
