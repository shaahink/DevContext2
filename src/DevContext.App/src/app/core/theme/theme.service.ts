import { computed, effect, Injectable, signal } from '@angular/core';

import type { VibeDefinition } from './vibe-definition';
import { getVibe, VIBES } from './vibes';

export interface ThemePalette {
  readonly base: string;
  readonly surface: string;
  readonly surface2: string;
  readonly elevated: string;
  readonly line: string;
  readonly lineStrong: string;
  readonly ink: string;
  readonly inkMuted: string;
  readonly inkSubtle: string;
  readonly accent: string;
  readonly accentInk: string;
  readonly success: string;
  readonly warn: string;
  readonly danger: string;
}

const STORAGE_KEY_VIBE = 'devcontext-vibe';
const STORAGE_KEY_THEME = 'devcontext-theme';

/** Sentinel `theme()` value (proposal §4.2 W7.5 "system-follow") — resolved against
 * `prefers-color-scheme` at render time rather than being a CSS `data-theme` value
 * itself (the stylesheet only has concrete selectors, e.g. `[data-theme="dark"]`). */
const SYSTEM = 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _vibe = signal<string>(this.loadVibe());
  private readonly _theme = signal<string>(this.loadTheme());
  private readonly _systemPrefersDark = signal(this.loadSystemPrefersDark());

  readonly vibe = this._vibe.asReadonly();
  /** The user's raw preference — may be `'system'`; use `resolvedTheme` for the value
   * that's actually painted (and for anything reading CSS custom properties). */
  readonly theme = this._theme.asReadonly();
  readonly vibeDef = computed<VibeDefinition>(() => getVibe(this._vibe()) ?? VIBES[0]);
  readonly vibes = computed(() => VIBES);

  /** `system` resolves to `dark`/`light` (whichever the OS reports and the current vibe
   * actually declares) — falls back to the vibe's own default if it only has one theme
   * (e.g. `terminal`, which is dark-only by design, not an oversight). */
  readonly resolvedTheme = computed<string>(() => {
    const requested = this._theme();
    if (requested !== SYSTEM) return requested;
    const def = this.vibeDef();
    const wantDark = this._systemPrefersDark();
    if (wantDark && def.themes.includes('dark')) return 'dark';
    if (!wantDark && def.themes.includes('light')) return 'light';
    return def.defaultTheme;
  });

  readonly palette = computed<ThemePalette>(() => ({
    base: this.cssVar('--vibe-base'),
    surface: this.cssVar('--vibe-surface'),
    surface2: this.cssVar('--vibe-surface-2'),
    elevated: this.cssVar('--vibe-elevated'),
    line: this.cssVar('--vibe-line'),
    lineStrong: this.cssVar('--vibe-line-strong'),
    ink: this.cssVar('--vibe-ink'),
    inkMuted: this.cssVar('--vibe-ink-muted'),
    inkSubtle: this.cssVar('--vibe-ink-subtle'),
    accent: this.cssVar('--vibe-accent'),
    accentInk: this.cssVar('--vibe-accent-ink'),
    success: this.cssVar('--vibe-success'),
    warn: this.cssVar('--vibe-warn'),
    danger: this.cssVar('--vibe-danger'),
  }));

  constructor() {
    effect(() => {
      const html = document.documentElement;
      html.setAttribute('data-vibe', this._vibe());
      html.setAttribute('data-theme', this.resolvedTheme());
    });

    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    mq.addEventListener('change', (e) => this._systemPrefersDark.set(e.matches));
  }

  setVibe(id: string): void {
    const def = getVibe(id);
    if (!def) return;
    this._vibe.set(id);
    if (this._theme() !== SYSTEM && !def.themes.includes(this._theme())) {
      this._theme.set(def.defaultTheme);
    }
    try { localStorage.setItem(STORAGE_KEY_VIBE, id); } catch { /* ignore */ }
    try { localStorage.setItem(STORAGE_KEY_THEME, this._theme()); } catch { /* ignore */ }
  }

  setTheme(theme: string): void {
    if (theme !== SYSTEM && !this.vibeDef().themes.includes(theme)) return;
    this._theme.set(theme);
    try { localStorage.setItem(STORAGE_KEY_THEME, theme); } catch { /* ignore */ }
  }

  private cssVar(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  }

  private loadVibe(): string {
    try { return localStorage.getItem(STORAGE_KEY_VIBE) ?? 'modern'; }
    catch { return 'modern'; }
  }

  private loadTheme(): string {
    try {
      const vibe = this.loadVibe();
      const def = getVibe(vibe);
      return localStorage.getItem(STORAGE_KEY_THEME) ?? def?.defaultTheme ?? 'dark';
    } catch { return 'dark'; }
  }

  private loadSystemPrefersDark(): boolean {
    try { return window.matchMedia('(prefers-color-scheme: dark)').matches; }
    catch { return true; }
  }
}
