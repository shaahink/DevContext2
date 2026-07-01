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

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _vibe = signal<string>(this.loadVibe());
  private readonly _theme = signal<string>(this.loadTheme());

  readonly vibe = this._vibe.asReadonly();
  readonly theme = this._theme.asReadonly();
  readonly vibeDef = computed<VibeDefinition>(() => getVibe(this._vibe()) ?? VIBES[0]);
  readonly vibes = computed(() => VIBES);

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
      html.setAttribute('data-theme', this._theme());
    });
  }

  setVibe(id: string): void {
    const def = getVibe(id);
    if (!def) return;
    this._vibe.set(id);
    if (!def.themes.includes(this._theme())) {
      this._theme.set(def.defaultTheme);
    }
    try { localStorage.setItem(STORAGE_KEY_VIBE, id); } catch { /* ignore */ }
    try { localStorage.setItem(STORAGE_KEY_THEME, this._theme()); } catch { /* ignore */ }
  }

  setTheme(theme: string): void {
    if (!this.vibeDef().themes.includes(theme)) return;
    this._theme.set(theme);
    try { localStorage.setItem(STORAGE_KEY_THEME, theme); } catch { /* ignore */ }
  }

  private cssVar(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  }

  private loadVibe(): string {
    try { return localStorage.getItem(STORAGE_KEY_VIBE) ?? 'terminal'; }
    catch { return 'terminal'; }
  }

  private loadTheme(): string {
    try {
      const vibe = this.loadVibe();
      const def = getVibe(vibe);
      return localStorage.getItem(STORAGE_KEY_THEME) ?? def?.defaultTheme ?? 'dark';
    } catch { return 'dark'; }
  }
}
