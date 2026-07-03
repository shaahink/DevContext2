import { computed, Injectable, signal } from '@angular/core';

/** Lower number = shown first. */
export type TickerPriority = 0 | 1 | 2 | 3;

export const TICKER_PRIORITY = {
  /** Live analysis facts ("12 projects found") — most timely. */
  analysis: 0 as TickerPriority,
  /** Engine insight headlines ("⚠ Missing auth on POST /orders"). */
  insight: 1 as TickerPriority,
  /** Atlas discoveries ("GET /report crosses 2 boundaries"). */
  atlas: 2 as TickerPriority,
  /** Keyboard tips — filler, rate-limited, shown once ever (persisted). */
  tip: 3 as TickerPriority,
} as const;

export interface TickerItem {
  /** Stable id — used for dedupe and (for tips) the seen-ledger. */
  readonly id: string;
  readonly text: string;
  /** Icon registry name (optional). */
  readonly icon?: string;
  /** Router link target when clicked (optional). */
  readonly link?: string;
  readonly priority: TickerPriority;
}

const ROTATE_MS = 6000;
const SEEN_KEY = 'devcontext-ticker-seen';
/** At most 1 tip per this many rotations (proposal §6: "at most 1-in-4"). */
const TIP_SPACING = 4;

/**
 * StatusBar insight ticker (proposal §6). Sources push items; the service rotates
 * the current one every 6s, highest-priority unseen first, then round-robins.
 * pause() on hover so the user can actually read; tips are persisted as seen so
 * they never nag twice across sessions.
 *
 * Wired from `workspace-shell.ts`'s constructor: analysis facts + insight headlines
 * (via SessionStore), AtlasStore discoveries, and static keyboard tips; the statusbar
 * renders `current()` and calls pause()/resume() on mouseenter/leave.
 */
@Injectable({ providedIn: 'root' })
export class TickerService {
  private readonly _items = signal<readonly TickerItem[]>([]);
  private readonly _current = signal<TickerItem | null>(null);
  private readonly _paused = signal(false);

  private timer: ReturnType<typeof setInterval> | null = null;
  private rotationsSinceTip = TIP_SPACING; // allow a tip immediately if nothing else
  private shownIds = new Set<string>();
  private readonly seenTips = this.loadSeen();

  readonly current = this._current.asReadonly();
  readonly paused = this._paused.asReadonly();
  readonly count = computed(() => this._items().length);

  /** Adds an item (deduped by id; seen tips are dropped). Starts rotation if idle. */
  post(item: TickerItem): void {
    if (item.priority === TICKER_PRIORITY.tip && this.seenTips.has(item.id)) return;
    if (this._items().some((i) => i.id === item.id)) return;

    this._items.update((items) =>
      [...items, item].sort((a, b) => a.priority - b.priority),
    );
    if (this._current() === null) this.rotate();
    this.ensureTimer();
  }

  /** Removes an item (e.g. a stale analysis fact after re-analyze). */
  dismiss(id: string): void {
    this._items.update((items) => items.filter((i) => i.id !== id));
    if (this._current()?.id === id) this.rotate();
  }

  /** Drops every item posted with an id starting with `prefix` (tab scoping). */
  dismissAll(prefix: string): void {
    this._items.update((items) => items.filter((i) => !i.id.startsWith(prefix)));
    if (this._current() && this._current()!.id.startsWith(prefix)) this.rotate();
  }

  /** Atomically replaces every item under `prefix` with a fresh set (0 or more) in a
   * SINGLE `_items` write. Use this instead of `dismissAll(prefix)` immediately
   * followed by `post(...)` from the same call site (e.g. inside an `effect()`) —
   * two separate reads-then-writes of `_items` within one synchronous execution
   * reproducibly freezes the tab: Angular's reactive graph treats the second write as
   * invalidating a dependency the same execution already read, and re-schedules that
   * execution synchronously forever (confirmed by hand via bisection, W5 checkpoint 8 —
   * no exception is thrown, so nothing appears in the console; the JS thread just stops
   * yielding, and even `page.evaluate(() => document.title)` never resolves). Every
   * other effect in this codebase that both reads and writes the same signal defers the
   * write into a microtask (e.g. `AtlasStore`'s degree-cache effect writes inside a
   * `getNode().then()`) — this method is the synchronous equivalent: one write, not two. */
  replaceGroup(prefix: string, items: readonly TickerItem[]): void {
    const keep = items.filter((i) => !(i.priority === TICKER_PRIORITY.tip && this.seenTips.has(i.id)));
    this._items.update((prev) => {
      const rest = prev.filter((i) => !i.id.startsWith(prefix));
      return [...rest, ...keep].sort((a, b) => a.priority - b.priority);
    });
    if (this._current() === null) {
      if (keep.length > 0) this.rotate();
    } else if (keep.length === 0 && this._current()!.id.startsWith(prefix)) {
      this.rotate();
    }
    if (keep.length > 0) this.ensureTimer();
  }

  pause(): void {
    this._paused.set(true);
  }

  resume(): void {
    this._paused.set(false);
  }

  /** Manual cycling (statusbar ‹ › affordance). Works even while paused. */
  next(): void {
    this.rotate();
  }

  private ensureTimer(): void {
    if (this.timer !== null) return;
    this.timer = setInterval(() => {
      if (!this._paused()) this.rotate();
    }, ROTATE_MS);
  }

  private rotate(): void {
    const items = this._items();
    if (items.length === 0) {
      this._current.set(null);
      if (this.timer !== null) {
        clearInterval(this.timer);
        this.timer = null;
      }
      return;
    }

    const isTipAllowed = this.rotationsSinceTip >= TIP_SPACING;
    const eligible = items.filter(
      (i) => i.priority !== TICKER_PRIORITY.tip || isTipAllowed,
    );
    const pool = eligible.length > 0 ? eligible : items;

    // Highest-priority item not yet shown this cycle; when all shown, start over.
    let pick = pool.find((i) => !this.shownIds.has(i.id));
    if (!pick) {
      this.shownIds = new Set();
      pick = pool[0];
    }

    this.shownIds.add(pick.id);
    this._current.set(pick);

    if (pick.priority === TICKER_PRIORITY.tip) {
      this.rotationsSinceTip = 0;
      this.markTipSeen(pick.id);
      this.dismissLater(pick.id); // a tip leaves the pool once delivered
    } else {
      this.rotationsSinceTip++;
    }
  }

  /** Remove after current display slot (keeps it visible for its rotation). */
  private dismissLater(id: string): void {
    this._items.update((items) => items.filter((i) => i.id !== id));
  }

  private markTipSeen(id: string): void {
    this.seenTips.add(id);
    try {
      localStorage.setItem(SEEN_KEY, JSON.stringify([...this.seenTips]));
    } catch {
      /* quota exceeded – drop */
    }
  }

  private loadSeen(): Set<string> {
    try {
      const raw = localStorage.getItem(SEEN_KEY);
      return new Set(raw ? (JSON.parse(raw) as string[]) : []);
    } catch {
      return new Set();
    }
  }
}
