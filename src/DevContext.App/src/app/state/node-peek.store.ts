import { Injectable, inject, signal } from '@angular/core';
import { SessionStore } from './session.store';
import { DevContextApi } from '../data-access/devcontext-api';
import { NodeResponse } from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { isStale, LatestGate } from '../core/rpc-call';

export interface PeekAnchor { readonly x: number; readonly y: number }

/**
 * NodePeekStore — the 200ms-hover card (proposal §8.1 `features/peek/node-peek.ts`),
 * deliberately separate from `NodeStore`/`NodeCard` (the click-through full sheet).
 * Single global instance: only one peek is ever visible, so a single `LatestGate` key
 * ('peek') is enough — sweeping the mouse across many `NodeLink`s must not let an
 * earlier, slower `getNode` resolve after a later hover's result already landed.
 */
@Injectable({ providedIn: 'root' })
export class NodePeekStore {
  private readonly api = inject(DevContextApi);
  private readonly session = inject(SessionStore);
  private readonly gate = new LatestGate();

  readonly nodeId = signal<string | null>(null);
  readonly anchor = signal<PeekAnchor | null>(null);
  readonly pinned = signal(false);
  readonly node = signal<NodeResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal(false);

  /** Grace period so the pointer can travel from the trigger `NodeLink` down into the
   * peek card itself (there's a few px of gap) without the card vanishing mid-transit. */
  private static readonly HIDE_GRACE_MS = 150;
  private hideTimer: ReturnType<typeof setTimeout> | null = null;

  /** Live "is Ctrl currently held" flag — a real OS key-repeat isn't guaranteed to fire
   * a fresh `keydown` on every tick, so `show()` reads this directly instead of relying
   * on a `keydown` racing the hover-delay timer. */
  private ctrlDown = false;

  constructor() {
    window.addEventListener('keydown', (e) => {
      if (e.key !== 'Control') return;
      this.ctrlDown = true;
      // Sticky pin (proposal §10 W7.1 "Ctrl to pin"): pressing Ctrl at any point while a
      // peek is already showing keeps it open past `mouseleave` until dismissed.
      if (this.nodeId() && !this.pinned()) this.pinned.set(true);
    });
    window.addEventListener('keyup', (e) => {
      if (e.key === 'Control') this.ctrlDown = false;
    });
  }

  async show(nodeId: string, anchor: PeekAnchor): Promise<void> {
    const handle = this.session.handle();
    if (!handle) return;
    this.clearHideTimer();
    if (this.nodeId() === nodeId) {
      // Already showing (or loading) this node — just follow the cursor, don't refetch.
      this.anchor.set(anchor);
      return;
    }
    this.nodeId.set(nodeId);
    this.anchor.set(anchor);
    // Held from before the hover-delay elapsed (e.g. Ctrl-hover) pins immediately.
    this.pinned.set(this.ctrlDown);
    this.node.set(null);
    this.error.set(false);
    this.loading.set(true);

    try {
      const res = await this.gate.run('peek', (signal) => this.api.getNode(handle, nodeId, signal));
      if (isStale(res)) return;
      this.node.set(res);
      this.loading.set(false);
    } catch {
      this.error.set(true);
      this.loading.set(false);
    }
  }

  /** Called on `mouseleave` from either the trigger `NodeLink` or the peek card itself.
   * No-ops while pinned. Snapshots the node it means to close — if a different hover
   * has since taken over (nodeId changed), this timer becomes a no-op instead of
   * killing the newer peek out from under it. */
  requestHide(): void {
    if (this.pinned()) return;
    const target = this.nodeId();
    this.clearHideTimer();
    this.hideTimer = setTimeout(() => {
      if (this.nodeId() === target) this.dismiss();
    }, NodePeekStore.HIDE_GRACE_MS);
  }

  /** Called on `mouseenter` from either the trigger or the peek card — cancels a
   * pending `requestHide()` so crossing the gap between them doesn't flicker. */
  cancelHide(): void {
    this.clearHideTimer();
  }

  /** Force-close regardless of pin state (✕ button, Escape, click-outside). */
  dismiss(): void {
    this.clearHideTimer();
    this.gate.cancel('peek');
    this.nodeId.set(null);
    this.anchor.set(null);
    this.pinned.set(false);
    this.node.set(null);
    this.error.set(false);
    this.loading.set(false);
  }

  private clearHideTimer(): void {
    if (this.hideTimer !== null) {
      clearTimeout(this.hideTimer);
      this.hideTimer = null;
    }
  }
}
