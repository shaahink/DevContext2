import { Injectable, signal } from '@angular/core';

export type Theme = 'graphite' | 'paper' | 'system';

export interface Prefs {
  readonly schemaVersion: 1;
  readonly defaultDepth: number;
  readonly defaultDetail: 'salient' | 'signature' | 'full';
  readonly useRoslyn: boolean;
  readonly autoCleanup: boolean;
  /** Inspector dock level on the Workbench (0 = hidden, 3 = focus mode). Proposal §8.2. */
  readonly dockLevel: number;
  /** Not yet applied to the DOM — ThemeService (W0 finish) will read this. Proposal §4.2/§8.2. */
  readonly theme: Theme;
}

const STORAGE_KEY = 'devcontext-prefs';

const DEFAULTS: Prefs = {
  schemaVersion: 1,
  defaultDepth: 6,
  defaultDetail: 'salient',
  useRoslyn: true,
  autoCleanup: true,
  dockLevel: 2,
  theme: 'graphite',
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
  readonly theme = () => this._prefs().theme;

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

  setTheme(theme: Theme): void {
    this.update({ theme });
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
