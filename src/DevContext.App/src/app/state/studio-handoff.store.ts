import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';

import type { PackProposal } from '../models/context-card';

/** Where Studio lives. One spelling, so a route rename cannot leave a sender pointing at nothing. */
export const STUDIO_ROUTE = '/context';

/**
 * N3.1 (audit §4 / owner decision 3) — the joint between the rooms.
 *
 * The audit's §3.A finding was that "nothing routes in": Explore, Insights and the NodeCard could
 * all name a symbol worth handing to an agent, and none of them could put it in a pack. The only
 * path into Studio was the activity bar, which arrived with empty panes.
 *
 * This is deliberately a ONE-SHOT channel rather than a second copy of the pack state: the sender
 * leaves a proposal, Studio takes it once on the way in, and it is gone. Studio is recreated by the
 * router on every navigation to {@link STUDIO_ROUTE} (there is no RouteReuseStrategy in this app —
 * checked 2026-08-14), so "take it in the constructor" is the whole lifecycle. A signal that stayed
 * set would re-seed the same cards the next time the user walked into the room.
 */
@Injectable({ providedIn: 'root' })
export class StudioHandoffStore {
  private readonly router = inject(Router);

  private readonly _pending = signal<PackProposal | null>(null);

  /** Read-only view — for a sender that wants to know whether it is overwriting something. */
  readonly pending = this._pending.asReadonly();

  /** Leaves a proposal for Studio WITHOUT navigating (the caller is already going there). */
  send(proposal: PackProposal): void {
    this._pending.set(proposal);
  }

  /** Reads and CLEARS. Null when nobody sent anything — the normal case for a plain nav. */
  take(): PackProposal | null {
    const pending = this._pending();
    if (pending) this._pending.set(null);
    return pending;
  }

  /** Send + open Studio. Returns the router's own verdict so a caller can toast the truth rather
   * than assume the navigation happened. */
  open(proposal: PackProposal): Promise<boolean> {
    this.send(proposal);
    return this.router.navigateByUrl(STUDIO_ROUTE);
  }
}
