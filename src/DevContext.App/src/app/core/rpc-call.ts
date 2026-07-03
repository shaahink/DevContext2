/**
 * LatestGate — switchMap semantics for signal stores (proposal §5.1).
 *
 * Problem: rapid re-triggers (j/k scrub in the entry deck, keystrokes in the omnibox)
 * fire overlapping RPCs. Responses can land out of order, so the LAST response to
 * arrive wins the UI even when it belongs to an OLDER request — stale trees flash in.
 *
 * Contract: for a given key, only the most recently started call may deliver a result.
 * Superseded calls are (a) aborted via their AbortSignal so the transport can drop the
 * wire work, and (b) resolved as STALE so callers can silently ignore them — including
 * their errors: a rejection from a superseded call is never the caller's problem.
 *
 * Usage in a store (key includes the tabId — cancellation must never cross tabs):
 *
 *   private readonly gate = new LatestGate();
 *
 *   const res = await this.gate.run(`${tabId}:trace`, (signal) =>
 *     this.api.getTrace(handle, focus, depth, detail),   // thread `signal` in once the
 *   );                                                   // grpc client accepts one
 *   if (res === STALE) return;                           // a newer call owns the UI
 *
 * Note: epoch-dropping works even while the transport ignores the AbortSignal — the
 * signal is an optimization (stop the wire), the epoch is the correctness guarantee.
 */
export const STALE: unique symbol = Symbol('rpc-stale');
export type Stale = typeof STALE;

export function isStale<T>(value: T | Stale): value is Stale {
  return value === STALE;
}

export class LatestGate {
  private readonly epochs = new Map<string, number>();
  private readonly aborts = new Map<string, AbortController>();

  /** Runs `fn` as the newest call for `key`, aborting the previous one. */
  async run<T>(key: string, fn: (signal: AbortSignal) => Promise<T>): Promise<T | Stale> {
    const epoch = (this.epochs.get(key) ?? 0) + 1;
    this.epochs.set(key, epoch);

    this.aborts.get(key)?.abort();
    const controller = new AbortController();
    this.aborts.set(key, controller);

    try {
      const result = await fn(controller.signal);
      return this.epochs.get(key) === epoch ? result : STALE;
    } catch (err) {
      // Errors belong to the caller only while it is still the current call. A
      // superseded call's failure (usually the abort itself) must not surface.
      if (this.epochs.get(key) !== epoch || controller.signal.aborted) return STALE;
      throw err;
    } finally {
      if (this.aborts.get(key) === controller) this.aborts.delete(key);
    }
  }

  /** True if `key` has a call in flight. Drives spinners/hairlines. */
  inFlight(key: string): boolean {
    return this.aborts.has(key);
  }

  /** Aborts the in-flight call for `key` (if any) and invalidates its result. */
  cancel(key: string): void {
    this.aborts.get(key)?.abort();
    this.aborts.delete(key);
    this.epochs.set(key, (this.epochs.get(key) ?? 0) + 1);
  }

  /** Cancels every in-flight call, or only those whose key starts with `prefix`
   *  (pass a tabId prefix when closing a tab). */
  cancelAll(prefix?: string): void {
    for (const key of [...this.aborts.keys()]) {
      if (!prefix || key.startsWith(prefix)) this.cancel(key);
    }
  }
}
