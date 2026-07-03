import { Injectable } from '@angular/core';

import { isTauri } from './tauri-env';

/** One top-level entry under the cache or repos root (a single repo's data). */
export interface StorageEntry {
  name: string;
  bytes: number;
}

export interface StorageSummary {
  entries: StorageEntry[];
  totalBytes: number;
}

const CACHE_ROOT = 'DevContext/cache';
const REPOS_ROOT = 'DevContext/repos';

/**
 * Reads the SAME on-disk locations `DevContext.Core`'s `SnapshotCacheRoot`/`RepoUrl.ClonePath`
 * use (`%LOCALAPPDATA%/DevContext/cache` and `%LOCALAPPDATA%/DevContext/repos` — via
 * `BaseDirectory.LocalData`, the raw OS local-data root, NOT Tauri's app-specific
 * `AppLocalData` subfolder), using `tauri-plugin-fs` scoped to exactly that subtree
 * (`capabilities/default.json`'s `fs:scope`). Settings·Storage (S3) was previously a static
 * stub showing a hardcoded (and wrong — "clones", not "repos") path with no real data; this
 * is the real listing. Sizes are a full recursive walk (no shortcut exists — `stat()` on a
 * directory doesn't report recursive content size), so a very large git clone could take a
 * visible moment; acceptable for a settings page opened occasionally, not a hot path.
 */
@Injectable({ providedIn: 'root' })
export class StorageService {
  async cacheSummary(): Promise<StorageSummary> {
    return this.summarize(CACHE_ROOT);
  }

  async reposSummary(): Promise<StorageSummary> {
    return this.summarize(REPOS_ROOT);
  }

  async clearCache(): Promise<void> {
    await this.clear(CACHE_ROOT);
  }

  async clearRepos(): Promise<void> {
    await this.clear(REPOS_ROOT);
  }

  /** Opens the cache or repos root folder in the OS file explorer (opener plugin). */
  async openInExplorer(root: 'cache' | 'repos'): Promise<void> {
    if (!isTauri()) return;
    const [{ localDataDir, join }, { openPath }] = await Promise.all([
      import('@tauri-apps/api/path'),
      import('@tauri-apps/plugin-opener'),
    ]);
    const base = await localDataDir();
    const sub = root === 'cache' ? CACHE_ROOT : REPOS_ROOT;
    await openPath(await join(base, ...sub.split('/')));
  }

  private async summarize(root: string): Promise<StorageSummary> {
    if (!isTauri()) return { entries: [], totalBytes: 0 };
    const fs = await import('@tauri-apps/plugin-fs');
    let topLevel: Awaited<ReturnType<typeof fs.readDir>>;
    try {
      topLevel = await fs.readDir(root, { baseDir: fs.BaseDirectory.LocalData });
    } catch {
      return { entries: [], totalBytes: 0 }; // root doesn't exist yet — nothing analyzed so far
    }

    const entries: StorageEntry[] = [];
    for (const item of topLevel) {
      if (!item.isDirectory) continue;
      const bytes = await this.dirSize(fs, `${root}/${item.name}`);
      entries.push({ name: item.name, bytes });
    }
    entries.sort((a, b) => b.bytes - a.bytes);
    return { entries, totalBytes: entries.reduce((sum, e) => sum + e.bytes, 0) };
  }

  private async dirSize(fs: typeof import('@tauri-apps/plugin-fs'), path: string): Promise<number> {
    let items: Awaited<ReturnType<typeof fs.readDir>>;
    try {
      items = await fs.readDir(path, { baseDir: fs.BaseDirectory.LocalData });
    } catch {
      return 0;
    }
    let total = 0;
    for (const item of items) {
      const childPath = `${path}/${item.name}`;
      if (item.isDirectory) {
        total += await this.dirSize(fs, childPath);
      } else if (item.isFile) {
        try {
          const info = await fs.stat(childPath, { baseDir: fs.BaseDirectory.LocalData });
          total += info.size;
        } catch {
          // file vanished mid-walk (e.g. another session evicting it) — skip
        }
      }
    }
    return total;
  }

  private async clear(root: string): Promise<void> {
    if (!isTauri()) return;
    const { remove, BaseDirectory } = await import('@tauri-apps/plugin-fs');
    try {
      await remove(root, { baseDir: BaseDirectory.LocalData, recursive: true });
    } catch {
      // nothing to clear
    }
  }
}
