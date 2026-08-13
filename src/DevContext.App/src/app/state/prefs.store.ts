import { Injectable, signal } from '@angular/core';

export interface Prefs {
  readonly schemaVersion: 1;
  readonly defaultDepth: number;
  readonly defaultDetail: 'salient' | 'signature' | 'full';
  readonly useRoslyn: boolean;
  readonly autoCleanup: boolean;
  /** Inspector dock level on the Workbench (0 = hidden, 3 = focus mode). Proposal §8.2. */
  readonly dockLevel: number;
  /** N1.1 — Context Studio shaping. Cards are session state and die with the handle; how you
   * like your packs shaped is a preference and must survive the tab switch that used to keep
   * stale cards alive and the reload that used to reset the budget to 4000. */
  readonly studioBudget: number;
  readonly studioIntent: StudioIntent;
  readonly studioFormat: StudioFormat;
}

/** Mirrors ContextIntent / OutputFormat in the Studio's scope-picker (kept structural rather
 * than imported so the root prefs store does not depend on a feature module). */
export type StudioIntent = 'trace' | 'explain' | 'review';
export type StudioFormat = 'markdown' | 'plain' | 'json';

/**
 * N2.2 — THE pack budget default, and the app's statement of the number the engine already uses:
 * ContextPackBuilder.DefaultBudgetTokens, the MCP get_context signature and this all read 8000.
 *
 * It was 4000 here alone, which meant the Studio opened at half the ceiling an agent gets for the
 * same pack and said nothing about it — the same pipeline answering two sizes depending on which
 * face you asked. The likely origin is TracePolicy.DefaultBudgetTokens (4000), which budgets one
 * TRACE rather than a whole pack; both constants now document what they budget.
 *
 * A value already in localStorage is left alone on purpose: since N1.1 the slider position is a
 * remembered CHOICE, and 4000 is one of its stops. This changes what the Studio opens with when
 * nobody has chosen, which is what "default" means.
 */
export const DEFAULT_STUDIO_BUDGET = 8000;

const STORAGE_KEY = 'devcontext-prefs';

const DEFAULTS: Prefs = {
  schemaVersion: 1,
  defaultDepth: 6,
  defaultDetail: 'salient',
  useRoslyn: true,
  autoCleanup: true,
  dockLevel: 2,
  studioBudget: DEFAULT_STUDIO_BUDGET,
  studioIntent: 'trace',
  studioFormat: 'markdown',
};

/**
 * Persisted user preferences (Analysis defaults, appearance, etc.).
 * Writes to localStorage under a schema-versioned key. Read by the landing
 * "Analyze" button and the Settings→Analysis tab.
 */
@Injectable({ providedIn: 'root' })
export class PrefsStore {
  private readonly _prefs = signal<Prefs>(this.load());

  readonly prefs = this._prefs.asReadonly();

  readonly defaultDepth = () => this._prefs().defaultDepth;
  readonly defaultDetail = () => this._prefs().defaultDetail;
  readonly useRoslyn = () => this._prefs().useRoslyn;
  readonly autoCleanup = () => this._prefs().autoCleanup;
  readonly dockLevel = () => this._prefs().dockLevel;
  readonly studioBudget = () => this._prefs().studioBudget;
  readonly studioIntent = () => this._prefs().studioIntent;
  readonly studioFormat = () => this._prefs().studioFormat;

  setDepth(d: number): void {
    this.update({ defaultDepth: clamp(d, 1, 10) });
  }

  setDetail(d: 'salient' | 'signature' | 'full'): void {
    this.update({ defaultDetail: d });
  }

  setUseRoslyn(v: boolean): void {
    this.update({ useRoslyn: v });
  }

  setAutoCleanup(v: boolean): void {
    this.update({ autoCleanup: v });
  }

  setDockLevel(level: number): void {
    this.update({ dockLevel: clamp(level, 0, 3) });
  }

  /** N1.1 — Studio shaping, persisted. Clamped to the budget panel's own slider range so a
   * hand-edited localStorage cannot put the Studio in a state its UI cannot represent. */
  setStudioBudget(tokens: number): void {
    this.update({ studioBudget: clamp(Math.round(tokens), 500, 32000) });
  }

  setStudioIntent(intent: StudioIntent): void {
    this.update({ studioIntent: intent });
  }

  setStudioFormat(format: StudioFormat): void {
    this.update({ studioFormat: format });
  }

  /** Returns a partial AnalyzeSpec with the user's defaults, ready to merge. */
  analyzeDefaults(): { depth: number; detail: 'salient' | 'signature' | 'full'; noRoslyn: boolean; cleanup: 'auto' | 'keep' } {
    const p = this._prefs();
    return {
      depth: p.defaultDepth,
      detail: p.defaultDetail,
      noRoslyn: !p.useRoslyn,
      cleanup: p.autoCleanup ? 'auto' : 'keep',
    };
  }

  private update(partial: Partial<Omit<Prefs, 'schemaVersion'>>): void {
    this._prefs.update((p) => ({ ...p, ...partial }));
    this.save();
  }

  private load(): Prefs {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return DEFAULTS;
      const parsed = JSON.parse(raw) as Prefs;
      if (parsed.schemaVersion !== 1) return DEFAULTS;
      return { ...DEFAULTS, ...parsed };
    } catch {
      return DEFAULTS;
    }
  }

  private save(): void {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(this._prefs())); }
    catch { /* quota exceeded */ }
  }
}

function clamp(v: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, v));
}
