import { Injectable, signal } from '@angular/core';

export interface RecentRepo {
  readonly path: string;
  readonly label: string;
  readonly accessedAt: number;
}

const STORAGE_KEY = 'devcontext-recents';
const MAX_RECENTS = 10;

@Injectable({ providedIn: 'root' })
export class RecentStore {
  private readonly _recents = signal<readonly RecentRepo[]>(this.load());

  readonly recents = this._recents.asReadonly();

  add(path: string, label?: string): void {
    const name = label ?? path.split(/[/\\]/).pop() ?? path;
    const now = Date.now();
    const updated = this._recents()
      .filter((r) => r.path !== path)
      .slice(0, MAX_RECENTS - 1);
    this._recents.set([{ path, label: name, accessedAt: now }, ...updated]);
    this.save();
  }

  remove(path: string): void {
    this._recents.update((list) => list.filter((r) => r.path !== path));
    this.save();
  }

  private load(): readonly RecentRepo[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as RecentRepo[]) : [];
    } catch {
      return [];
    }
  }

  private save(): void {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(this._recents())); }
    catch { /* quota exceeded – drop */ }
  }
}
