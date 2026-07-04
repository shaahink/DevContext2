/**
 * Shared display formatters (proposal §10 W7.3, GAP-C1) — the same "1.2K" compact-count
 * logic used to be copy-pasted, byte-for-byte or near enough, across `inspector.ts`,
 * `export-drawer.ts`, `run-console.ts`, and `repo-card.ts`. Components still expose their
 * own thin `protected` wrapper (templates can only call class members), but the logic
 * itself now lives here once.
 */

/** "1.2K" for 1200+ — the tabular-nums token/count style used everywhere except star
 * counts, which use the conventional lower-case GitHub "k" via the `unit` param. */
export function formatCompact(n: number, unit: 'K' | 'k' = 'K'): string {
  return n >= 1000 ? `${(n / 1000).toFixed(1)}${unit}` : String(n);
}

/** "1.4 MB" — binary (1024-based) units, used by Settings·Storage (S3). */
export function formatBytes(bytes: number): string {
  if (bytes <= 0) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB'];
  const i = Math.min(units.length - 1, Math.floor(Math.log(bytes) / Math.log(1024)));
  return `${(bytes / 1024 ** i).toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

/** "3d ago" / "2mo ago" — coarse relative time for repo cards (GitHub picker, recents). */
export function timeAgo(dateStr: string | null): string {
  if (!dateStr) return '';
  const diff = Date.now() - new Date(dateStr).getTime();
  const days = Math.floor(diff / 86400000);
  if (days < 1) return 'today';
  if (days < 30) return `${days}d ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months}mo ago`;
  return `${Math.floor(months / 12)}y ago`;
}
