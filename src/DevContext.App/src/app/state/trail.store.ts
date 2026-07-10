import { computed, effect, inject, Injectable, signal } from '@angular/core';

import { WorkspaceStore } from './workspace.store';

export type TrailStepKind = 'entry' | 'node' | 'insight' | 'reroot';

export interface TrailStep {
  readonly kind: TrailStepKind;
  /** nodeId for entries/nodes/reroots, insight title for insights. */
  readonly id: string;
  readonly title: string;
  /** The trace focus that reproduces this step — what undo/redo/jump re-traces. Unused
   * for `reroot` steps, which restore via `TraceStore.reroot(id)` instead (client-side,
   * see its doc comment for why a real re-fetch isn't possible). */
  readonly focus: string;
  readonly ts: number;
}

/** M7.3: A group of consecutive trail steps belonging to the same flow (same focus). */
export interface TrailFlowGroup {
  /** Whether this is a group (multiple steps) or a single step rendered solo. */
  readonly grouped: boolean;
  readonly steps: readonly TrailStep[];
  /** Index range of these steps in the original breadcrumb. */
  readonly fromIndex: number;
  readonly toIndex: number;
}

interface TrailSlice {
  readonly steps: readonly TrailStep[];
  /** Index of the CURRENT step in `steps`; -1 = no selection yet. Undo moves it left,
   * redo right. A push while not at the tip truncates the forward branch (like a
   * browser history, not a tree). */
  readonly cursor: number;
  readonly pins: readonly TrailStep[];
}

const EMPTY_SLICE: TrailSlice = { steps: [], cursor: -1, pins: [] };
const STEP_CAP = 50;

function stepKey(s: Pick<TrailStep, 'kind' | 'id'>): string {
  return `${s.kind}:${s.id}`;
}

/**
 * The Trail (proposal §1, §3.8) — one concept serving three jobs: breadcrumb,
 * undo/redo stack, and export seed (pins). Per-tab slices are kept HERE (not in
 * WorkspaceStore) so this lands additively; slices self-garbage-collect when their
 * tab disappears. Facade signals follow the SessionStore/TraceStore pattern: they
 * always reflect the ACTIVE tab.
 *
 * undo()/redo()/jumpTo() move the cursor and return the step to restore — the caller
 * (workbench page / shell) owns re-tracing, because only it has the session handle.
 */
@Injectable({ providedIn: 'root' })
export class TrailStore {
  private readonly workspace = inject(WorkspaceStore);

  private readonly _slices = signal<ReadonlyMap<string, TrailSlice>>(new Map());

  private readonly active = computed(
    () => this._slices().get(this.workspace.activeId() ?? '') ?? EMPTY_SLICE,
  );

  readonly steps = computed(() => this.active().steps);
  readonly cursor = computed(() => this.active().cursor);
  readonly pins = computed(() => this.active().pins);
  readonly pinCount = computed(() => this.pins().length);
  /** Steps from the root up to and including the current one — the breadcrumb. */
  readonly breadcrumb = computed(() => this.steps().slice(0, this.cursor() + 1));
  /** M7.3: Breadcrumb collapsed into flow groups — consecutive steps with the same
   *  `focus` are grouped together with a count. Solo steps render as ungrouped. */
  readonly groupedBreadcrumb = computed<TrailFlowGroup[]>(() => {
    const bc = this.breadcrumb();
    if (bc.length === 0) return [];
    const groups: TrailFlowGroup[] = [];
    let i = 0;
    while (i < bc.length) {
      const start = i;
      const focus = bc[i].focus;
      // Collect consecutive steps with the same focus (reroot steps have empty focus — keep separate)
      while (i < bc.length && bc[i].focus === focus && focus !== '') i++;
      // Reroot steps (empty focus) are always solo
      if (focus === '') {
        while (i < bc.length && bc[i].focus === '') {
          groups.push({ grouped: false, steps: [bc[i]], fromIndex: i, toIndex: i });
          i++;
        }
        continue;
      }
      const count = i - start;
      if (count > 1) {
        groups.push({ grouped: true, steps: bc.slice(start, i), fromIndex: start, toIndex: i - 1 });
      } else {
        groups.push({ grouped: false, steps: [bc[start]], fromIndex: start, toIndex: start });
      }
    }
    return groups;
  });
  readonly current = computed(() => this.steps()[this.cursor()] ?? null);
  readonly canUndo = computed(() => this.cursor() > 0);
  readonly canRedo = computed(() => this.cursor() < this.steps().length - 1);
  readonly hasTrail = computed(() => this.steps().length > 0);

  constructor() {
    // GC: drop slices for tabs that no longer exist (tab closed).
    effect(() => {
      const live = new Set(this.workspace.tabs().map((t) => t.id));
      const slices = this._slices();
      if (![...slices.keys()].some((id) => !live.has(id))) return;
      const next = new Map([...slices].filter(([id]) => live.has(id)));
      this._slices.set(next);
    });
  }

  /** Records a step for the active tab. Consecutive duplicates (same kind+id) are
   * collapsed. Pushing while undone truncates the redo branch. Capped FIFO. */
  push(step: Omit<TrailStep, 'ts'>): void {
    const tabId = this.workspace.activeId();
    if (!tabId) return;

    this.update(tabId, (s) => {
      const current = s.steps[s.cursor];
      if (current && stepKey(current) === stepKey(step)) return s;

      let steps = [...s.steps.slice(0, s.cursor + 1), { ...step, ts: Date.now() }];
      if (steps.length > STEP_CAP) steps = steps.slice(steps.length - STEP_CAP);
      return { ...s, steps, cursor: steps.length - 1 };
    });
  }

  /** Moves back one step; returns the step to restore, or null at the root. */
  undo(): TrailStep | null {
    return this.moveTo((s) => s.cursor - 1);
  }

  /** Moves forward one step; returns the step to restore, or null at the tip. */
  redo(): TrailStep | null {
    return this.moveTo((s) => s.cursor + 1);
  }

  /** Jumps to an absolute index in the trail (breadcrumb click). */
  jumpTo(index: number): TrailStep | null {
    return this.moveTo(() => index);
  }

  togglePin(step: TrailStep): void {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    this.update(tabId, (s) => {
      const key = stepKey(step);
      const without = s.pins.filter((p) => stepKey(p) !== key);
      return { ...s, pins: without.length < s.pins.length ? without : [...s.pins, step] };
    });
  }

  isPinned(step: Pick<TrailStep, 'kind' | 'id'>): boolean {
    const key = stepKey(step);
    return this.pins().some((p) => stepKey(p) === key);
  }

  clearActive(): void {
    const tabId = this.workspace.activeId();
    if (!tabId) return;
    this.update(tabId, () => EMPTY_SLICE);
  }

  private moveTo(target: (s: TrailSlice) => number): TrailStep | null {
    const tabId = this.workspace.activeId();
    if (!tabId) return null;

    const slice = this._slices().get(tabId) ?? EMPTY_SLICE;
    const next = target(slice);
    if (next < 0 || next > slice.steps.length - 1 || next === slice.cursor) return null;

    this.update(tabId, (s) => ({ ...s, cursor: next }));
    return slice.steps[next];
  }

  private update(tabId: string, fn: (s: TrailSlice) => TrailSlice): void {
    this._slices.update((map) => {
      const prev = map.get(tabId) ?? EMPTY_SLICE;
      const next = fn(prev);
      if (next === prev) return map;
      return new Map(map).set(tabId, next);
    });
  }
}
