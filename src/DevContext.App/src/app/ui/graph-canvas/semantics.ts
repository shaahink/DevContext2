/**
 * D4.2 (F3/M): the canvas's semantic vocabulary — pure mappings from engine facts
 * (ServiceCard.kind, TransportLink.transport tags, stack role tags) to visual classes.
 * Kept DOM-free so the vocabulary is unit-testable and shared across canvas levels.
 *
 * R3 D-B: this is the language the owner decided in S7 — transports are the edge layer, a
 * deployment reference is a weaker claim than traffic and is drawn as one, and a service is named
 * for the entry surface it actually owns.
 */

/** Transport tag → visual class. Tags come from ServiceLink edge tags verbatim
 * (e.g. "http", "bus", "AzureStorageQueue", "grpc") — classify, never invent.
 *
 * `deploy` is deliberately NOT a protocol: an orchestrator reference says A was handed B's address
 * at startup, which is a real dependency but not traffic. D-B gives it its own class so the canvas
 * can draw it recessively instead of letting it compete with calls for the reader's attention. */
export type TransportClass = 'HTTP' | 'queue' | 'gRPC' | 'event' | 'deploy' | 'other';

export interface TransportEdgeVisual {
  /** Short label drawn on the edge. Classified tags get the class name; unknown tags
   * stay verbatim (truncated) — honesty over taxonomy. */
  readonly label: string;
  readonly cls: TransportClass;
}

export function classifyTransport(tag: string): TransportEdgeVisual {
  const t = tag.toLowerCase();
  if (t.includes('grpc')) return { label: 'gRPC', cls: 'gRPC' };
  if (/queue|bus|rabbit|kafka|masstransit|nservicebus|servicebus|sqs/.test(t)) return { label: 'queue', cls: 'queue' };
  if (/event/.test(t)) return { label: 'event', cls: 'event' };
  if (/http|rest|typed-client|refit/.test(t)) return { label: 'HTTP', cls: 'HTTP' };
  // An Aspire AppHost reference is a deployment fact, not a protocol: A is handed B's address at
  // startup. Labelled for what it is rather than guessed into a transport class.
  if (t.includes('aspire')) return { label: 'deployment ref', cls: 'deploy' };
  return { label: tag.length > 12 ? tag.slice(0, 11) + '…' : tag, cls: 'other' };
}

/** True for the classes that represent actual traffic. D-B: only these carry a drawn label, because
 * only these answer "how does work move" — the repeated word `apphost` on nine separate edges was
 * the single largest source of noise on the surface Explore now opens on. */
export function isTraffic(cls: TransportClass): boolean {
  return cls !== 'deploy';
}

/** ServiceCard.kind → box-label glyph. The vocabulary is GraphProjections.ClassifyService's, which
 * R3 D-B re-derived from the entry surfaces a service owns — before that every service classified as
 * "Service" and every glyph here rendered empty. A plain service still carries no glyph: a runnable
 * that owns no nameable surface should say nothing rather than guess. */
export function serviceKindGlyph(kind: string): string {
  switch (kind) {
    case 'Web API': return 'API';
    case 'Gateway': return 'GW';
    case 'gRPC': return 'RPC';
    case 'GraphQL': return 'GQL';
    case 'Realtime': return 'HUB';
    case 'Functions': return 'FN';
    case 'Grains': return 'GRAIN';
    case 'Worker': return 'JOB';
    case 'CLI': return 'CLI';
    case 'UI': return 'UI';
    case 'Library': return 'LIB';
    default: return '';
  }
}

/** RoleTags.DataStore as surfaced in ServiceCard.stack. */
export function hasDataStore(stack: readonly string[]): boolean {
  return stack.includes('datastore');
}

/**
 * D-B: `[db]` is the CODE-level signal (this project touches a datastore type) and a drawn store is
 * the ORCHESTRATOR-level one (the deployment declares this resource). Where both exist they say the
 * same thing twice, so the mark yields to the drawing — it survives only on services whose stores
 * were never declared, which is most repos.
 */
export function serviceLabel(
  displayName: string,
  kind: string,
  stack: readonly string[],
  truncate: (s: string) => string,
  drawnStoreCount = 0,
): string {
  const glyph = serviceKindGlyph(kind);
  const db = drawnStoreCount === 0 && hasDataStore(stack) ? ' [db]' : '';
  return (glyph ? `[${glyph}] ` : '') + truncate(displayName) + db;
}

/** A store's drawn label. The orchestrator's own word for the resource type rides along when it gave
 * one ("eventbus · rabbitmq"); an undeclared type stays unnamed rather than guessed from the name.
 *
 * The type is dropped when the NAME already says it — `catalogdb · database` and `redis · redis`
 * spend a third of the box repeating themselves. Abbreviation counts as saying it: a name ending in
 * "db" has already told the reader it is a database. */
export function storeLabel(name: string, resourceType: string, truncate: (s: string) => string): string {
  // The `[db]` glyph, not the box outline, is what makes a store read as NOT a service. Cytoscape's
  // barrel shape is nearly indistinguishable from a round-rectangle at a node's width-to-height
  // ratio, and the product already says kind in text everywhere else ([API], [JOB], [UI]).
  const body = !resourceType || saysItsOwnType(name, resourceType)
    ? truncate(name)
    : `${truncate(name)} · ${resourceType}`;
  return `[db] ${body}`;
}

/** A service, as far as this module needs to know it — structural, so the vocabulary stays free of
 * the generated proto types and its tests stay free of message constructors. */
export interface ServiceStoreOwner {
  readonly displayName: string;
  readonly stores: readonly { readonly name: string; readonly resourceType: string }[];
}

export interface StoreDeclaration {
  readonly name: string;
  readonly resourceType: string;
  /** The services that named this resource, in engine order. */
  readonly owners: readonly string[];
}

/**
 * R3 D-B's lane tail (S8): every resource a deployment DECLARED, once each.
 *
 * The lane views could not inherit transport-coloured edges — lanes live at the all-projects
 * altitude whose edges are csproj references, and colouring those would invent traffic (the scope
 * correction recorded in DECISIONS.md). A declared store is the other kind of fact: it is not
 * traffic and not a reference, it is the orchestrator saying "this repo runs a Redis", which is
 * equally true at either altitude. So it crosses.
 *
 * Two services naming one resource share one declaration — duplicating it would invent a second
 * Redis — and the first non-empty type wins, because a resource has one type and silence is not a
 * disagreement.
 */
export function declaredStores(services: readonly ServiceStoreOwner[]): readonly StoreDeclaration[] {
  const byName = new Map<string, { name: string; resourceType: string; owners: string[] }>();
  for (const s of services) {
    for (const store of s.stores) {
      const existing = byName.get(store.name);
      if (!existing) {
        byName.set(store.name, { name: store.name, resourceType: store.resourceType, owners: [s.displayName] });
        continue;
      }
      if (!existing.resourceType && store.resourceType) existing.resourceType = store.resourceType;
      if (!existing.owners.includes(s.displayName)) existing.owners.push(s.displayName);
    }
  }
  return [...byName.values()];
}

/**
 * R3 D-4 (G6.1) — a service's ROLE in the picture: `orchestrator` (it declares other services as
 * resources, so the canvas draws it as the frame they sit in), `isolated` (the engine placed it in
 * no transport relationship at all, so the canvas trays it), or `linked` (a drawn box).
 *
 * This lived inside `buildServiceLevelElements`, which meant only the canvas knew it. The
 * per-service breakdown listed the same twelve projects as twelve identical peers, so a reader who
 * saw nine boxes, one frame and a tray of two had no way to join the two surfaces — the third of the
 * three vocabularies D-4 found. Both surfaces now read this one function.
 *
 * Membership is NOT decided here: the services passed in are the engine's one runnable-and-production
 * set (`ServiceBoundaryInference.RunnableProjects` → `NodeKind.Service` → the ServiceMap facet).
 */
export type ServiceRole = 'orchestrator' | 'linked' | 'isolated';

export interface ServiceRoleInput {
  readonly displayName: string;
  readonly orchestrates: readonly string[];
}

export interface TransportPair {
  readonly fromService: string;
  readonly toService: string;
}

export function classifyServiceRoles(
  services: readonly ServiceRoleInput[],
  transports: readonly TransportPair[],
): ReadonlyMap<string, ServiceRole> {
  const related = new Set<string>();
  for (const t of transports) {
    if (t.fromService === t.toService) continue;
    related.add(t.fromService);
    related.add(t.toService);
  }

  // A declared orchestrator expresses MEMBERSHIP, not traffic (D-B, B1). Only a frame with members
  // earns the role — an AppHost whose projects all fell out of scope is just another service.
  const byName = new Set(services.map((s) => s.displayName));
  const claimed = new Set<string>();
  const frames = new Set<string>();
  for (const s of services) {
    if (s.orchestrates.length === 0) continue;
    const members = s.orchestrates.filter(
      (m) => byName.has(m) && m !== s.displayName && !claimed.has(m));
    if (members.length === 0) continue;
    frames.add(s.displayName);
    for (const m of members) claimed.add(m);
  }

  // A repo whose services carry no links at all is an inventory, not a mesh — every box is drawn and
  // nothing is trayed. The tray only earns its place when it separates the unconnected FROM
  // something connected.
  const trayEarnsIts = services.some((s) => related.has(s.displayName));

  const roles = new Map<string, ServiceRole>();
  for (const s of services) {
    const name = s.displayName;
    if (roles.has(name)) continue;
    if (frames.has(name)) roles.set(name, 'orchestrator');
    else if (trayEarnsIts && !related.has(name)) roles.set(name, 'isolated');
    else roles.set(name, 'linked');
  }
  return roles;
}

/** The frame each framed service belongs to, for the canvas's compound parenting. Same rule as
 * {@link classifyServiceRoles} — one pass, so the two cannot drift. */
export function orchestratorMembership(
  services: readonly ServiceRoleInput[],
): ReadonlyMap<string, string> {
  const byName = new Set(services.map((s) => s.displayName));
  const frameOf = new Map<string, string>();
  for (const s of services) {
    if (s.orchestrates.length === 0) continue;
    const members = s.orchestrates.filter(
      (m) => byName.has(m) && m !== s.displayName && !frameOf.has(m));
    if (members.length === 0) continue;
    for (const m of members) frameOf.set(m, s.displayName);
  }
  return frameOf;
}

function saysItsOwnType(name: string, resourceType: string): boolean {
  const n = name.toLowerCase().replace(/[^a-z]/g, '');
  const t = resourceType.toLowerCase().replace(/[^a-z]/g, '');
  if (!t) return true;
  if (n.includes(t)) return true;
  // "catalogdb" vs "database": a short suffix that ABBREVIATES the type. An abbreviation keeps the
  // first letter and drops from the middle ("db" is d…b of database), so it is a subsequence
  // sharing an initial — not a prefix, which is what "db" is not.
  for (let len = 2; len <= 4 && len < n.length; len++) {
    if (isAbbreviation(n.slice(-len), t)) return true;
  }
  return false;
}

function isAbbreviation(short: string, long: string): boolean {
  if (short[0] !== long[0]) return false;
  let i = 0;
  for (const ch of long) {
    if (ch === short[i]) i++;
    if (i === short.length) return true;
  }
  return false;
}
