import { Component, input, output } from '@angular/core';

import type { GitHubRepo } from '../../data-access/github-api';
import { Badge } from '../badge/badge';
import { Button } from '../button/button';
import { Icon } from '../icon/icon';

function formatStars(n: number): string {
  if (n >= 1000) return `${(n / 1000).toFixed(1)}k`;
  return String(n);
}

function timeAgo(dateStr: string | null): string {
  if (!dateStr) return '';
  const diff = Date.now() - new Date(dateStr).getTime();
  const days = Math.floor(diff / 86400000);
  if (days < 1) return 'today';
  if (days < 30) return `${days}d ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months}mo ago`;
  return `${Math.floor(months / 12)}y ago`;
}

@Component({
  selector: 'app-repo-card',
  imports: [Badge, Icon, Button],
  template: `
    <div class="flex flex-col rounded-lg border border-line bg-surface p-3 transition-colors hover:border-line-strong hover:bg-surface-2">
      <div class="flex items-start gap-3">
        <img
          [src]="repo().ownerAvatar"
          [alt]="repo().ownerLogin"
          class="mt-0.5 h-8 w-8 shrink-0 rounded-full bg-surface-2"
          loading="lazy"
        />
        <div class="min-w-0 flex-1">
          <div class="flex items-center gap-2">
            <span class="truncate font-mono text-sm font-medium text-ink">{{ repo().fullName }}</span>
            <app-badge variant="accent">{{ formatStars(repo().stargazersCount) }} <span class="text-accent-ink/70">&star;</span></app-badge>
          </div>
          @if (repo().description) {
            <p class="mt-1 line-clamp-2 text-xs leading-relaxed text-ink-muted">{{ repo().description }}</p>
          }
          <div class="mt-2 flex flex-wrap items-center gap-2 text-[10px] text-ink-subtle">
            @if (repo().language) {
              <span class="flex items-center gap-1">
                <span class="h-2 w-2 rounded-full bg-accent"></span>
                {{ repo().language }}
              </span>
            }
            @if (repo().pushedAt) {
              <span>Updated {{ timeAgo(repo().pushedAt) }}</span>
            }
            @if (repo().topics.length) {
              <span class="truncate">{{ repo().topics.slice(0, 4).join(' · ') }}</span>
            }
          </div>
        </div>
      </div>
      <div class="mt-3 flex gap-2">
        <app-button variant="primary" size="sm" (click)="analyze.emit(repo().fullName)">
          <app-icon name="play" [size]="12" /> Analyze
        </app-button>
        <a
          [href]="repo().htmlUrl"
          target="_blank"
          rel="noopener"
          class="inline-flex items-center gap-1 rounded-md px-2 py-1 text-[11px] text-ink-muted hover:text-ink"
        >
          <app-icon name="network" [size]="11" /> GitHub
        </a>
      </div>
    </div>
  `,
})
export class RepoCard {
  readonly repo = input.required<GitHubRepo>();
  readonly analyze = output<string>();

  protected formatStars(n: number): string { return formatStars(n); }
  protected timeAgo(d: string | null): string { return timeAgo(d); }
}
