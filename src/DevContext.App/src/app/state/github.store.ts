import { Injectable, signal } from '@angular/core';

import type { GitHubRepo } from '../data-access/github-api';
import { searchRepos } from '../data-access/github-api';

const CURATED: readonly string[] = [
  'dotnet/runtime',
  'dotnet/aspnetcore',
  'dotnet/efcore',
  'dnf/dotnet-core',
  'JasonTaylor/CleanArchitecture',
];

@Injectable({ providedIn: 'root' })
export class GitHubStore {
  private readonly _query = signal('');
  private readonly _results = signal<readonly GitHubRepo[]>([]);
  private readonly _totalCount = signal(0);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _page = signal(1);
  private readonly _hasMore = signal(false);
  private abort: AbortController | null = null;

  readonly query = this._query.asReadonly();
  readonly results = this._results.asReadonly();
  readonly totalCount = this._totalCount.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly hasMore = this._hasMore.asReadonly();

  private githubToken: string | null = null;

  constructor() {
    try { this.githubToken = localStorage.getItem('devcontext-github-token'); } catch { /* ignore */ }
  }

  setToken(token: string | null): void {
    this.githubToken = token;
    try { if (token) localStorage.setItem('devcontext-github-token', token); else localStorage.removeItem('devcontext-github-token'); }
    catch { /* ignore */ }
  }

  async loadCurated(signal?: AbortSignal): Promise<void> {
    this._loading.set(true);
    this._error.set(null);
    this._query.set('');
    try {
      const repos: GitHubRepo[] = [];
      for (const fullName of CURATED) {
        if (signal?.aborted) return;
        try {
          const results = await this.fetchRepo(fullName, signal);
          if (results.length > 0) repos.push(results[0]);
        } catch { /* skip repo that fails */ }
      }
      this._results.set(repos);
      this._totalCount.set(repos.length);
      this._hasMore.set(false);
      this._page.set(1);
    } catch (err) {
      this._error.set(err instanceof Error ? err.message : 'Failed to load repos');
    } finally {
      this._loading.set(false);
    }
  }

  async search(query: string): Promise<void> {
    this.abort?.abort();
    const abort = new AbortController();
    this.abort = abort;

    this._query.set(query);
    this._page.set(1);
    this._loading.set(true);
    this._error.set(null);

    if (!query.trim()) {
      await this.loadCurated(abort.signal);
      return;
    }

    try {
      const result = await searchRepos(query, 1, 20, abort.signal);
      if (abort.signal.aborted) return;
      this._results.set(result.items);
      this._totalCount.set(result.totalCount);
      this._hasMore.set(result.items.length < result.totalCount);
    } catch (err) {
      if (abort.signal.aborted) return;
      this._error.set(err instanceof Error ? err.message : 'Search failed');
    } finally {
      if (!abort.signal.aborted) this._loading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    if (this._loading()) return;
    const nextPage = this._page() + 1;
    this._page.set(nextPage);
    this._loading.set(true);

    try {
      const result = await searchRepos(this._query(), nextPage, 20);
      this._results.update((prev) => [...prev, ...result.items]);
      this._hasMore.set(this._results().length < result.totalCount);
    } catch (err) {
      this._error.set(err instanceof Error ? err.message : 'Failed to load more');
    } finally {
      this._loading.set(false);
    }
  }

  cancel(): void {
    this.abort?.abort();
  }

  private async fetchRepo(fullName: string, signal?: AbortSignal): Promise<GitHubRepo[]> {
    const url = `https://api.github.com/repos/${fullName}`;
    const res = await fetch(url, {
      headers: { Accept: 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28' },
      signal,
    });
    if (!res.ok) throw new Error(`Failed to fetch ${fullName}`);
    const item = await res.json() as {
      id: number; full_name: string; html_url: string; clone_url: string;
      description: string | null; stargazers_count: number; language: string | null;
      topics: string[]; pushed_at: string | null;
      owner: { avatar_url: string; login: string };
    };
    return [{
      id: item.id, fullName: item.full_name, htmlUrl: item.html_url,
      cloneUrl: item.clone_url, description: item.description,
      stargazersCount: item.stargazers_count, language: item.language,
      topics: item.topics ?? [], pushedAt: item.pushed_at,
      ownerAvatar: item.owner.avatar_url, ownerLogin: item.owner.login,
    }];
  }
}
