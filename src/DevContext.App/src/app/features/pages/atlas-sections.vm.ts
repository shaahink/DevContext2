import type { WithheldReason } from '../../ui/withheld/withheld';

/**
 * R3 C-2 — why each Atlas section is empty, as pure functions over the facts the page already has.
 *
 * These live outside the component on purpose. The rule they encode ("a section that withholds
 * must say WHY, and 'nothing was looked at' is not 'nothing was found'") is the checkpoint's
 * subject, so it is unit-tested in the battery rather than only observed through a DOM probe.
 */

/** A section with nothing to show: the reason class plus the sentence the reader gets. */
export interface WithheldNote {
  readonly reason: WithheldReason;
  readonly text: string;
}

/** Flow-index progress, as `AtlasStore.status()` reports it. */
export type FlowIndexStatus = 'idle' | 'indexing' | 'paused' | 'done' | 'cancelled';

/** Everything the five decisions below are allowed to read. One object, so a new section cannot
 * quietly invent a second source for a fact another section already states. */
export interface AtlasSectionFacts {
  /** The repo's own entry-point count — not an archetype guess. */
  readonly entries: number;
  readonly isLibrary: boolean;
  readonly projectCount: number;
  /** Services the Architecture canvas draws (the ServiceMap facet — D-4's one definition). */
  readonly serviceCount: number;
  /** Per-service style rows the Map response carried. */
  readonly serviceStyleCount: number;
  readonly topFlowCount: number;
  readonly flowStatus: FlowIndexStatus;
}

/**
 * Top flows. A flow starts at an entry point, so a repo without one has no flows — that is the
 * archetype, not a gap. Returns null when the section should render its content.
 */
export function topFlowsWithheld(f: AtlasSectionFacts): WithheldNote | null {
  if (f.entries === 0) {
    return {
      reason: 'archetype',
      text: f.isLibrary
        ? 'No entry points — a library exposes a surface, not flows. Its front doors are on the Explore page.'
        : 'No entry points were found in this scope, so there are no flows to rank.',
    };
  }
  if (f.topFlowCount > 0) return null;
  if (f.flowStatus === 'indexing' || f.flowStatus === 'paused') {
    return { reason: 'not-computed', text: 'Indexing flows… the top flows appear as entries are traced.' };
  }
  return f.flowStatus === 'done'
    ? { reason: 'none-found', text: 'No flow could be traced from any entry point in this repo.' }
    : { reason: 'not-computed', text: 'Flows have not been indexed yet — open the Explore page to index them.' };
}

/**
 * Event and queue board. T6.0 S1.8 already made this one honest about indexing state; C-2 gives
 * the same sentences their reason class and stops the library case from claiming a detection ran.
 */
export function eventWiringWithheld(f: AtlasSectionFacts): WithheldNote {
  if (f.entries === 0) {
    return {
      reason: 'archetype',
      text: f.isLibrary
        ? 'No entry points — event wiring does not apply to a pure library surface.'
        : 'No entry points were found in this scope, so no publisher or consumer could be joined.',
    };
  }
  if (f.flowStatus === 'indexing' || f.flowStatus === 'paused') {
    return { reason: 'not-computed', text: 'Indexing flows… event wiring appears as publishers are found.' };
  }
  return f.flowStatus === 'done'
    ? { reason: 'none-found', text: 'No events detected — every indexed flow stays in-process.' }
    : { reason: 'not-computed', text: 'No event wiring data — flows have not been indexed yet.' };
}

/**
 * Data stores — the sharp one. The section's ONLY inputs are the per-service style stacks and the
 * ServiceMap cards, both empty by construction when a repo has no services. It reported that as
 * "No data-store signals detected", which a reader takes as a finding about the repo; the finding
 * was actually that nothing had been examined.
 */
export function dataStoresWithheld(f: AtlasSectionFacts): WithheldNote {
  if (f.serviceCount === 0 && f.serviceStyleCount === 0) {
    return {
      reason: 'archetype',
      text: f.isLibrary
        ? 'Data stores are read per service, and a library has none — whatever runs this library owns its storage.'
        : 'Data stores are read per service, and no service was resolved in this scope.',
    };
  }
  const n = f.serviceCount || f.serviceStyleCount;
  return {
    reason: 'none-found',
    text: `No data-store signals — nothing in the ${n} service${n === 1 ? '' : 's'} names a known store (EF Core, Dapper, Redis, Mongo, SQL Server…).`,
  };
}

/** Per-service breakdown over an empty service set — the caption used to count one anyway. */
export function serviceBreakdownWithheld(f: AtlasSectionFacts): WithheldNote {
  if (!f.isLibrary) {
    return {
      reason: 'none-found',
      text: 'No service — nothing in the analyzed solution is both runnable and a production project.',
    };
  }
  const n = f.projectCount;
  return {
    reason: 'archetype',
    text: `No services — a library is packaged, not run. Its ${n} project${n === 1 ? '' : 's'} ${n === 1 ? 'is' : 'are'} on the Architecture canvas above.`,
  };
}

/**
 * Hub radar. The server ranks a hub by how many indexed flows share it
 * (`FlowIndexBuilder.TopHubDegrees` keeps nodes on more than one flow), so with no flows there can
 * be no hub. The old text told that reader to go index flows — nothing to index, nothing changes.
 */
export function hubRadarWithheld(f: AtlasSectionFacts): WithheldNote {
  if (f.entries === 0) {
    return {
      reason: 'archetype',
      text: 'No hubs — the radar ranks nodes by how many flows pass through them, and a repo with no entry points has no flows.',
    };
  }
  if (f.flowStatus === 'indexing' || f.flowStatus === 'paused') {
    return { reason: 'not-computed', text: 'Indexing flows… a hub appears once a node is shared by more than one flow.' };
  }
  return f.flowStatus === 'done'
    ? { reason: 'none-found', text: 'No hubs — no node appears on more than one indexed flow.' }
    : { reason: 'not-computed', text: 'No hubs yet — flows have not been indexed. Open the Explore page to index them.' };
}
