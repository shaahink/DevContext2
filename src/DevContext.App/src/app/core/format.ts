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

/** Middle-ellipsis for a route/title in a narrow column: the head names the resource and the
 * tail carries the distinguishing segment, so cutting the middle keeps both ends
 * (`/api/catalog/items/{id:int}` stays distinguishable from `/api/catalog/items`). The full
 * text belongs on [title].
 *
 * R3 D-A (A-4), S7: the budget MUST sit under what the column can actually show, or CSS
 * `truncate` cuts first and this never fires — which is how the audit kept seeing six rows
 * reading `/api/catalog/i…`. S10: lifted out of `entry-deck` because the Context Studio's
 * scope picker had re-grown the identical defect (eleven `/api/catalog/i…` rows), having
 * never received the fix. One rule, one place, so a third list cannot re-invent it.
 * `budget` is the column's character capacity; CSS truncation stays as the backstop. */
export function middleEllipsis(text: string, budget = 34, bias: 'tail' | 'head' = 'tail'): string {
  if (text.length <= budget) return text;
  // S10: WHERE a label distinguishes itself depends on what kind of label it is, and a single
  // split gets one of the two kinds wrong. A route differs at its TAIL
  // (/api/catalog/items vs /api/catalog/items/{id:int}), so the tail must survive. A type or
  // member name differs at its HEAD — eShop's bus consumers are
  // OrderStatusChangedTo{AwaitingValidation,Paid,Shipped,...}IntegrationEventHandler, identical
  // for the last 21 characters, so a tail-biased cut renders several rows as one
  // "OrderStatu...ionEventHandler". Measured in the Studio picker, where two such rows collided.
  const head = bias === 'head'
    ? Math.max(4, budget - 6)
    : Math.min(14, Math.max(4, budget - 16));
  return text.slice(0, head) + '…' + text.slice(-(budget - head - 1));
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

/** The node kinds a canonical id can be prefixed with (`CodeGraph.NodeKind`). Matching against the
 * real set — rather than "everything before the first colon" — keeps `nodeIdLabel` from eating the
 * scheme of an id whose key legitimately contains a colon (EntryPoint:domain:SomeHandler). */
const NODE_KIND_PREFIXES: readonly string[] = ['Type', 'Member', 'EntryPoint', 'Service', 'Message', 'Store'];

/**
 * The ONE sanctioned way a canonical node id reaches the screen (R3 D-4, G6.2).
 *
 * A node's display name is a fact the graph already holds — GraphNode.Title — and every surface
 * should render THAT. But a title is not always on hand: a GetNode RPC still in flight, an edge
 * whose target is not a node. Before this existed, each of those spots invented its own rule, and
 * two of them printed raw metadata:
 *
 *   - the trail carved ids up with split on dot/colon and kept the last two segments, so
 *     Type:Microsoft.Extensions.Logging.ILogger + arity marker read as "Logging.ILogger" + marker
 *     (metadata syntax in a name), while Service:WebApp read as "Service.WebApp" — the node KIND
 *     posing as a namespace. That is the surgery G6.1 removed from the hub radar; it had a second
 *     copy here.
 *   - the node peek and the neighbours header printed the id verbatim, marker and all.
 *
 * The rule: drop the KIND prefix, because a kind is not part of a name; and spell generic arity the
 * way C# spells an unbound generic — ILogger of one, IDictionary of two — never the metadata
 * marker. Nothing else is removed. The namespace stays: dropping segments is what made a Service
 * look like a Type, and it is not this function's job to shorten (the column does that).
 *
 * This is a FALLBACK. Never prefer it to a title the graph gave you — the engine's titles are not
 * derivable from ids (measured: eval-results/2026-07-29/G6/label-mirror-fidelity.txt), so a
 * derivation is a SECOND vocabulary, which is the defect D-4 is about.
 */
export function nodeIdLabel(nodeId: string): string {
  if (!nodeId) return nodeId;
  const colon = nodeId.indexOf(':');
  const key =
    colon > 0 && NODE_KIND_PREFIXES.includes(nodeId.slice(0, colon)) ? nodeId.slice(colon + 1) : nodeId;
  return humanizeArity(key);
}

/** The three tiers an edge/step can be resolved at, mirroring `DevContext.Core.Graph.EdgeTier`. */
export type EdgeTier = 'verified' | 'joined' | 'approx';

/**
 * The app's ONE reading of a wire `resolution` string (V1.1, backlog #25). Mirror of the engine's
 * `EdgeConfidence`: Semantic = verified, Syntactic = approx, everything else (Join — which is also
 * the engine enum's DEFAULT, so it covers every edge no producer labelled) = joined.
 *
 * Before this the app had two readings of its own: the trace node badges tested `=== 'Syntactic'`
 * for approx (right) while the explorer's neighbour list and the canvas tested `!== 'Semantic'`
 * (wrong), so one Join edge was drawn "approx" on one page and unlabelled on another — while the
 * CLI called that same edge "verified". Never test a resolution string inline; call this.
 */
export function edgeTier(resolution: string | null | undefined): EdgeTier {
  if (resolution === 'Semantic') return 'verified';
  if (resolution === 'Syntactic') return 'approx';
  return 'joined';
}

/** Metadata arity marker → C# unbound-generic spelling, everywhere it occurs (a nested chain carries
 * one per generic segment: Outer of 2 . Inner of 1). Arity 0 or a malformed marker is left alone. */
export function humanizeArity(text: string): string {
  if (!text.includes('`')) return text;
  return text.replace(/`(\d+)/g, (whole, digits: string) => {
    const n = Number(digits);
    return n > 0 ? `<${','.repeat(n - 1)}>` : whole;
  });
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
