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

/** "net10.0-android;net10.0-ios;net10.0-maccatalyst" → "net10.0 + MAUI targets" (T6.7,
 * audit B1: raw `;`-joined TFM walls shipped verbatim into the identity strip and Atlas
 * MAP header). Non-TFM strings pass through untouched. */
export function humanizeTfms(item: string): string {
  if (!item.includes(';')) return item;
  const parts = item.split(';').map((s) => s.trim()).filter(Boolean);
  if (parts.length === 0 || !parts.every((p) => /^net\d/.test(p))) return item;
  const bases = [...new Set(parts.map((p) => p.split('-')[0]))];
  const hasPlatform = parts.some((p) => p.includes('-'));
  return bases.join(', ') + (hasPlatform ? ' + MAUI targets' : '');
}

/** Project display name (T6.8, audit A8): strip only the COMMON dotted prefix shared by
 * every project in the repo — never the generic `split('.').pop()` last segment, which
 * rendered Basket.API/Catalog.API/Ordering.API… all as "API" (and corrupted the one-pager
 * into "AppHost → API, API, API"). shamshir: TradingEngine.Web → "Web"; eShop keeps full
 * names because its projects share no common prefix. */
export function projectDisplayName(name: string, allNames: readonly string[]): string {
  const prefix = commonDottedPrefix(allNames);
  if (prefix && name.startsWith(prefix + '.') && name.length > prefix.length + 1) {
    return name.slice(prefix.length + 1);
  }
  return name;
}

function commonDottedPrefix(names: readonly string[]): string {
  const distinct = [...new Set(names)].filter(Boolean);
  if (distinct.length < 2) return '';
  const segs = distinct.map((n) => n.split('.'));
  const first = segs[0];
  let common = 0;
  // keep at least one segment of every name un-stripped
  while (common < first.length - 1 && segs.every((s) => s.length > common + 1 && s[common] === first[common])) {
    common++;
  }
  return first.slice(0, common).join('.');
}

/** Repo-relative display path (T6.8, audit B13): absolute machine paths in the Details
 * rail / Call Stack / Table RESOLUTION are longer, non-portable, and token-expensive.
 * The absolute path stays available on [title] / the copy affordance. `repoRoot` is the
 * analyzed tab's path; a solution-FILE path is reduced to its directory first. */
export function repoRelativePath(filePath: string, repoRoot: string | null | undefined): string {
  if (!filePath || !repoRoot) return filePath;
  let root = repoRoot.replace(/[\\/]+$/, '');
  if (/\.slnx?$/i.test(root)) {
    const cut = Math.max(root.lastIndexOf('/'), root.lastIndexOf('\\'));
    if (cut > 0) root = root.slice(0, cut);
  }
  const norm = (p: string) => p.replace(/[\\/]+/g, '/').toLowerCase();
  const nFile = norm(filePath);
  const nRoot = norm(root) + '/';
  if (!nFile.startsWith(nRoot)) return filePath;
  return filePath.slice(root.length).replace(/^[\\/]+/, '');
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
