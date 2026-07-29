import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { AtlasStore } from '../../state/atlas.store';
import { TraceStore } from '../../state/trace.store';
import { humanizeTfms, projectDisplayName } from '../../core/format';
import { FlowStepper } from '../shared/flow-stepper';
import { ServiceCards, type EntryMix } from '../shared/service-cards';
import { GraphCanvas, type GraphCanvasData } from '../../ui/graph-canvas/graph-canvas';
import { classifyServiceRoles, classifyTransport } from '../../ui/graph-canvas/semantics';
import { KIND_LABELS, NODE_KIND_LABELS } from '../../models/view-models';

@Component({
  selector: 'app-atlas-page',
  imports: [RouterLink, FlowStepper, ServiceCards, GraphCanvas],
  template: `
    <div class="mx-auto max-w-4xl space-y-8 px-5 pb-10 pt-6">
      @if (!session.ready()) {
        <p class="py-8 text-center text-xs text-ink-subtle">Analyze a repo to see its atlas.</p>
      } @else {
        <!-- M6.2: Export buttons (top-right) — clipboard + file download (T6.11) -->
        <div class="flex items-center justify-between">
          <h1 class="text-lg font-bold text-ink">Atlas</h1>
          <div class="flex items-center gap-2">
            <button
              class="flex items-center gap-1.5 rounded-md border border-line bg-surface px-3 py-1.5 text-xs text-ink hover:border-accent hover:bg-surface-2 transition-colors"
              [class.copied]="copied()"
              (click)="copyAtlas()"
            >
              <span class="i-lucide-file-text h-3.5 w-3.5"></span>
              {{ copied() ? 'Copied!' : 'Export one-pager' }}
            </button>
            <button
              class="flex items-center gap-1.5 rounded-md border border-line bg-surface px-3 py-1.5 text-xs text-ink hover:border-accent hover:bg-surface-2 transition-colors"
              data-testid="onepager-download"
              (click)="downloadAtlas()"
            >
              <span class="i-lucide-download h-3.5 w-3.5"></span>
              Download .md
            </button>
          </div>
        </div>

        <!-- T6.7: MAP header as structured chips (was the raw CLI markdown blob — a text
             wall that duplicated the per-service list and shipped ;-joined TFM strings) -->
        <div class="flex flex-wrap items-center gap-x-4 gap-y-2 rounded-lg border border-line bg-surface px-4 py-3 text-xs">
          @if (session.summary(); as s) {
            <span class="font-semibold text-ink">{{ s.label }}</span>
          }
          @if (map()?.archetype; as a) {
            <span class="chip">{{ a }}</span>
          }
          <!-- D4.4 (F1): style suppressed for libraries, exactly as the CLI's Library renderer. -->
          @if (!map()?.isLibrary) {
            @if (map()?.style; as st) {
              <span class="text-ink-muted" [title]="map()?.styleEvidence || ''">
                {{ st }}
                @if (confidenceTier(); as tier) {
                  <span class="text-ink-subtle" [title]="'Style detection confidence: ' + ((map()?.styleConfidence ?? 0) * 100).toFixed(0) + '%'"> &middot; {{ tier }}</span>
                }
              </span>
            }
          }
          @if (stackChips().length) {
            <span class="flex flex-wrap items-center gap-1.5">
              @for (item of stackChips(); track item) {
                <span class="rounded bg-surface-2 px-1.5 py-0.5 font-mono text-2xs text-ink-muted">{{ item }}</span>
              }
            </span>
          }
        </div>

        <!-- ① Layered architecture view (D4.3/L3): the FULL canvas — zoom, legend, level
             toggle, service expansion — not the 280px hero the baseline squeezed 33
             projects into. Double-click a box to open it in Explore. -->
        <div>
          <h2 class="section-h mb-3">Architecture</h2>
          <!-- R3 D-4: this caption used to count PROJECTS while the section below counted SERVICES,
               so the page never said the picture and the list hold the same set. It now leads with
               that one number and accounts for every member of it. -->
          <p class="mb-2 text-2xs text-ink-subtle">
            @if (serviceCount()) {
              <span class="text-ink-muted">{{ serviceCount() }} services</span>
              <span> ({{ serviceRoleSummary() }})</span> &middot;
            }
            {{ topology().length }} projects &middot;
            {{ topology().reduce((n, p) => n + p.dependsOn.length, 0) }} dependency edges
            @if (atlasTransports().length) {
              &middot; {{ atlasTransports().length }} transport links
            }
          </p>
          <app-graph-canvas
            [data]="atlasCanvasData()"
            (nodeActivated)="onProjectTap($event)"
          />
        </div>

        <!-- ② Top flows as stepper strips -->
        <div>
          <h2 class="section-h mb-3">Top flows</h2>
          @if ((session.summary()?.entries ?? 0) === 0) {
            <p class="py-4 text-center text-xs text-ink-subtle">No entry points — a library exposes surface, not flows.</p>
          } @else {
            <app-flow-stepper [flows]="atlasTopFlows()" />
          }
        </div>

        <!-- ③ Event & queue board (D4.3: queue transports join the async seams) -->
        <div>
          <h2 class="section-h mb-3">Event &amp; queue board</h2>
          @if (queueLinks().length) {
            <div class="mb-2 space-y-1">
              @for (q of queueLinks(); track q.key) {
                <div class="flex items-center gap-2 rounded border border-line bg-surface px-3 py-1.5 text-xs" [title]="q.evidence">
                  <span class="font-mono text-ink">{{ q.from }}</span>
                  <span class="chip text-2xs text-warn">{{ q.transport }}</span>
                  <span class="text-ink-subtle">&rarr;</span>
                  <span class="font-mono text-ink">{{ q.to }}</span>
                </div>
              }
            </div>
          }
          @if (eventWiring().length) {
            <div class="overflow-x-auto rounded border border-line">
              <table class="w-full text-left text-xs">
                <thead>
                  <tr class="border-b border-line bg-surface-2 text-2xs uppercase tracking-wider text-ink-muted">
                    <th class="px-3 py-2 font-medium">Publisher</th>
                    <th class="px-3 py-2 font-medium">Event</th>
                    <th class="px-3 py-2 font-medium">Consumer</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-line">
                  @for (w of eventWiring(); track w.event + w.publisherFocus + (w.consumerFocus ?? '')) {
                    <tr class="hover:bg-surface-2">
                      <td class="px-3 py-2 font-mono text-ink">
                        <a class="hover:text-accent hover:underline" [routerLink]="['/explore']" [queryParams]="{ focus: w.publisherFocus }">
                          {{ w.publisherTitle }}
                        </a>
                      </td>
                      <td class="px-3 py-2 text-ink-muted">
                        {{ w.event }}
                        @if (w.approx) {
                          <span class="chip text-warn ml-1 text-2xs" title="Name-match heuristic — the analyzed graph carried no event projection">approx</span>
                        }
                        @if (w.crossService) {
                          <span class="chip ml-1 text-2xs" title="Publisher and consumer live in different services">cross-service</span>
                        }
                      </td>
                      <td class="px-3 py-2 font-mono text-ink">
                        @if (w.consumerTitle && w.consumerFocus) {
                          <a class="hover:text-accent hover:underline" [routerLink]="['/explore']" [queryParams]="{ focus: w.consumerFocus }">
                            {{ w.consumerTitle }}
                          </a>
                        } @else {
                          <span class="chip text-warn text-2xs">unconsumed</span>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else if (!queueLinks().length) {
            <p class="py-4 text-center text-xs text-ink-subtle">{{ eventWiringEmptyText() }}</p>
          }
        </div>

        <!-- ④ Data stores (D4.3/L3) -->
        <div>
          <h2 class="section-h mb-3">Data stores</h2>
          @if (dataStores().length) {
            <div class="space-y-1">
              @for (d of dataStores(); track d.service) {
                <div class="flex items-center gap-2 rounded border border-line bg-surface px-3 py-1.5 text-xs">
                  <span class="min-w-0 flex-1 truncate font-mono text-ink">{{ d.service }}</span>
                  @for (tech of d.stores; track tech) {
                    <span class="chip shrink-0 text-2xs">{{ tech }}</span>
                  }
                </div>
              }
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">No data-store signals detected.</p>
          }
        </div>

        <!-- ⑤ Per-service cards (style + entry mix, D4.3) -->
        <div>
          <h2 class="section-h mb-3">Per-service breakdown</h2>
          <!-- R3 D-4: same set as the canvas above, same count, and every card says how the canvas
               drew it — the reader can now join a card to a box, a frame, or the tray. -->
          <p class="mb-2 text-2xs text-ink-subtle">
            The {{ serviceCount() }} services the Architecture canvas draws &mdash; a service is a
            runnable production project.
          </p>
          <app-service-cards [services]="serviceStyles()" [entryMix]="entryMix()" [roles]="serviceRoles()" />

          <!-- R3 D-4: the boundary the scope pick draws. A repo that declares several solutions is
               analysed one at a time, so some runnable apps are discovered on disk and then never
               drawn, counted or named. dotnet-podcasts keeps two MAUI clients in sibling solutions
               and every Atlas surface was silent about them. They are listed apart from the cards
               above and never called services - the canvas does not draw them. -->
          @if (outsideScopeApps().length) {
            <div class="mt-4 rounded-lg border border-dashed border-line bg-surface px-3 py-2.5">
              <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">
                Not analyzed &mdash; {{ outsideScopeApps().length }} runnable app{{ outsideScopeApps().length === 1 ? '' : 's' }} outside this solution
              </h3>
              <p class="mt-1 text-2xs text-ink-subtle">
                Discovered on disk, listed in another solution. Not services &mdash; switch solutions to analyze them.
              </p>
              <div class="mt-2 space-y-1">
                @for (a of outsideScopeApps(); track a.projectName) {
                  <div class="flex items-center gap-2 text-xs">
                    <span class="min-w-0 flex-1 truncate font-mono text-ink-muted">{{ a.projectName }}</span>
                    <span class="chip shrink-0 text-2xs">{{ a.style }}</span>
                    @for (tech of a.stack; track tech) {
                      <span class="chip shrink-0 text-2xs">{{ tech }}</span>
                    }
                  </div>
                }
              </div>
            </div>
          }
        </div>

        <!-- ⑤ Cross-cutting (behaviors, packages) -->
        <div>
          <h2 class="section-h mb-3">Cross-cutting</h2>
          @if (pipelineBehaviors().length || packages().length) {
            <div class="flex flex-wrap gap-3">
              @if (pipelineBehaviors().length) {
                <div class="flex-1 min-w-48 rounded-lg border border-line bg-surface p-3">
                  <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle mb-2">Pipeline behaviors</h3>
                  <div class="flex flex-wrap gap-1.5">
                    @for (b of pipelineBehaviors(); track b) {
                      <span class="chip text-xs">{{ b }}</span>
                    }
                  </div>
                </div>
              }
              @if (packages().length) {
                <div class="flex-1 min-w-48 rounded-lg border border-line bg-surface p-3">
                  <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle mb-2">Packages</h3>
                  <div class="flex flex-wrap gap-1.5">
                    @for (pg of packages(); track pg.label) {
                      <span class="chip text-xs" [title]="pg.packages.join(', ')">{{ pg.label }} ({{ pg.packages.length }})</span>
                    }
                  </div>
                </div>
              }
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">No cross-cutting data resolved.</p>
          }
        </div>

        <!-- ⑥ Hub radar -->
        <div>
          <h2 class="section-h mb-3">Hub radar</h2>
          <p class="mb-2 text-2xs text-ink-subtle">Nodes appearing in the most distinct flows across the repo.</p>
          @if (hubsWithDegree().length) {
            <div class="space-y-1">
              @for (h of hubsWithDegree(); track h.nodeId) {
                <div
                  class="list-row justify-between"
                  role="button"
                  tabindex="0"
                  (click)="onHubTap(h.nodeId)"
                  (keydown.enter)="onHubTap(h.nodeId)"
                  (keydown.space)="onHubTap(h.nodeId); $event.preventDefault()"
                >
                  <span class="flex min-w-0 flex-1 items-center gap-2">
                    <!-- R3 D-4: say WHAT each row is. Seven of eShop's ten hubs were Service nodes
                         sitting unlabelled beside types, two of them printing the node kind as if it
                         were a namespace. A row now names its kind, and a service row uses the same
                         word the canvas and the breakdown use. -->
                    <span class="chip shrink-0 text-2xs" [title]="hubKindTitle(h.kind)">{{ hubKindLabel(h.kind) }}</span>
                    <span class="min-w-0 truncate font-mono text-xs text-ink">{{ h.title }}</span>
                    @if (h.project && h.kind !== 'Service') {
                      <span class="shrink-0 truncate text-2xs text-ink-subtle">{{ h.project }}</span>
                    }
                  </span>
                  <span class="shrink-0 text-2xs tabular-nums text-ink-subtle">
                    {{ h.flowCount }} flow{{ h.flowCount === 1 ? '' : 's' }}
                    @if (h.degree; as d) {
                      &middot; in {{ d.inDegree }} &middot; out {{ d.outDegree }}
                    }
                  </span>
                </div>
              }
            </div>
          } @else {
            <p class="py-4 text-center text-xs text-ink-subtle">No hubs indexed — index flows from the Explore page.</p>
          }
        </div>
      }
    </div>
  `,
})
export class AtlasPage {
  protected readonly session = inject(SessionStore);
  protected readonly atlas = inject(AtlasStore);
  private readonly trace = inject(TraceStore);
  private readonly router = inject(Router);

  protected readonly map = this.session.mapResponse;
  protected readonly topology = computed(() => this.session.mapResponse()?.topology ?? []);

  /** Words + thresholds mirror MapRenderer.AppendStyle (one definition, T6.8). */
  protected readonly confidenceTier = computed(() => {
    const c = this.map()?.styleConfidence ?? 0;
    if (c <= 0) return null;
    return c >= 0.8 ? 'high' : c >= 0.5 ? 'moderate' : 'low';
  });

  /** Stack chips with `;`-joined TFM lists humanized: "net10.0-android;net10.0-ios;…"
   * reads "net10.0 + MAUI targets" (audit B1 gate: no raw joined TFM strings in the DOM). */
  protected readonly stackChips = computed(() => {
    const seen = new Set<string>();
    const chips: string[] = [];
    for (const item of this.map()?.stack ?? []) {
      const label = humanizeTfms(item);
      if (!seen.has(label)) { seen.add(label); chips.push(label); }
    }
    return chips;
  });
  protected readonly serviceStyles = computed(() => this.session.mapResponse()?.serviceStyles ?? []);
  /**
   * R3 D-4 — runnable apps the analysed solution does NOT contain, from the engine's own list.
   * Deliberately separate from `serviceStyles`: a service is a project the canvas draws, and the
   * canvas does not draw these. Merging the two lists would re-open the defect D-4 closed.
   */
  protected readonly outsideScopeApps = computed(() => this.session.mapResponse()?.outsideScopeApps ?? []);
  protected readonly pipelineBehaviors = computed(() => this.session.mapResponse()?.pipelineBehaviors ?? []);
  protected readonly packages = computed(() => this.session.mapResponse()?.packages ?? []);

  /** D4.3: the atlas diagram is the full canvas fed by the ServiceMap facet — same
   * deterministic geometry as Home/Explore (one map, three sizes). */
  protected readonly atlasTransports = computed(() => this.session.graphFacets()?.serviceMap?.transports ?? []);

  /**
   * R3 D-4 (G6.1) — THE service set for this page. One definition, from the engine's one
   * runnable-and-production list: `ServiceBoundaryInference.RunnableProjects` → `NodeKind.Service`
   * → the ServiceMap facet. The canvas draws it; the breakdown describes it; the hub radar labels
   * its rows with the same word.
   */
  protected readonly serviceCards = computed(() => this.session.graphFacets()?.serviceMap?.services ?? []);
  protected readonly serviceCount = computed(() => this.serviceCards().length);
  /** The same role classification the canvas renders — not a second derivation of it. */
  protected readonly serviceRoles = computed(() =>
    classifyServiceRoles(this.serviceCards(), this.atlasTransports()));
  /** "9 drawn · 1 orchestrator · 2 in no relationship" — every member of the set accounted for. */
  protected readonly serviceRoleSummary = computed(() => {
    let linked = 0, orchestrators = 0, isolated = 0;
    for (const role of this.serviceRoles().values()) {
      if (role === 'orchestrator') orchestrators++;
      else if (role === 'isolated') isolated++;
      else linked++;
    }
    const parts = [`${linked} drawn`];
    if (orchestrators) parts.push(`${orchestrators} orchestrator${orchestrators === 1 ? '' : 's'}`);
    if (isolated) parts.push(`${isolated} in no relationship`);
    return parts.join(' · ');
  });
  protected readonly atlasCanvasData = computed<GraphCanvasData>(() => ({
    mode: 'topology',
    projects: this.topology(),
    services: this.serviceCards(),
    transports: this.atlasTransports(),
  }));

  /** Queue-class transports for the async board (events already have their table). */
  protected readonly queueLinks = computed(() => {
    const seen = new Set<string>();
    const rows: { key: string; from: string; to: string; transport: string; evidence: string }[] = [];
    for (const t of this.atlasTransports()) {
      if (classifyTransport(t.transport).cls !== 'queue') continue;
      const key = `${t.fromService}|${t.toService}|${t.transport}`;
      if (seen.has(key)) continue;
      seen.add(key);
      rows.push({ key, from: t.fromService, to: t.toService, transport: t.transport, evidence: t.evidence ?? '' });
    }
    return rows;
  });

  /** Data stores per service: tech signals from the style stack (EFCore/Redis/…) plus the
   * engine's RoleTags.DataStore mark on the ServiceMap card. Signals only — never invented. */
  private static readonly STORE_TECH = /ef.?core|entity.?framework|npgsql|postgres|sql.?server|sqlite|mysql|mariadb|redis|mongo|cosmos|dynamo|dapper|litedb|ravendb|marten|elastic/i;
  protected readonly dataStores = computed(() => {
    const byService = new Map<string, Set<string>>();
    for (const s of this.serviceStyles()) {
      const techs = s.stack.filter((t) => AtlasPage.STORE_TECH.test(t));
      if (techs.length) byService.set(s.projectName, new Set(techs));
    }
    for (const card of this.serviceCards()) {
      if (!card.stack.includes('datastore')) continue;
      const set = byService.get(card.displayName) ?? new Set<string>();
      if (set.size === 0) set.add('data store');
      byService.set(card.displayName, set);
    }
    return [...byService.entries()]
      .map(([service, stores]) => ({ service, stores: [...stores].sort() }))
      .sort((a, b) => a.service.localeCompare(b.service, 'en'));
  });

  /** Entry mix per project for the service cards (counts by kind, ranked). */
  protected readonly entryMix = computed<EntryMix>(() => {
    const counts = new Map<string, Map<string, number>>();
    for (const g of this.session.entryGroups()) {
      for (const e of g.entries) {
        if (!e.project) continue;
        const per = counts.get(e.project) ?? new Map<string, number>();
        const label = KIND_LABELS[e.kind] ?? e.kind;
        per.set(label, (per.get(label) ?? 0) + 1);
        counts.set(e.project, per);
      }
    }
    const mix = new Map<string, readonly { label: string; count: number }[]>();
    for (const [project, per] of counts) {
      mix.set(project, [...per.entries()]
        .map(([label, count]) => ({ label, count }))
        .sort((a, b) => b.count - a.count));
    }
    return mix;
  });
  protected readonly eventWiring = this.atlas.eventWiring;
  protected readonly hubsWithDegree = this.atlas.hubsWithDegree;
  protected readonly atlasTopFlows = computed(() => this.atlas.topFlows().slice(0, 5));

  /** Honest empty state (T6.0 S1.8): "index flows" was shown even AFTER indexing finished —
   * on a monolith the truthful message is "there are no cross-service events", not an
   * instruction that changes nothing. */
  protected readonly eventWiringEmptyText = computed(() => {
    if ((this.session.summary()?.entries ?? 0) === 0) return 'No entry points — event wiring does not apply to a pure library surface.';
    const status = this.atlas.status();
    if (status === 'indexing' || status === 'paused') return 'Indexing flows… event wiring appears as publishers are found.';
    if (status === 'done') return 'No events detected — every indexed flow stays in-process.';
    return 'No event wiring data — flows have not been indexed yet.';
  });

  protected copied = signal(false);

  protected onProjectTap(name: string): void {
    if (!name) return;
    void this.router.navigate(['/explore'], { queryParams: { view: 'system', project: name } });
  }

  /** R3 D-4: the row's kind in the app's one word for it. An unmapped kind renders verbatim
   * rather than being silently dropped — a missing word is a fact worth seeing. */
  protected hubKindLabel(kind: string): string {
    return NODE_KIND_LABELS[kind] ?? kind.toLowerCase();
  }

  protected hubKindTitle(kind: string): string {
    return kind === 'Service'
      ? 'A service — a runnable production project. The same set the Architecture canvas draws and the per-service breakdown describes.'
      : `Graph node kind: ${kind}`;
  }

  protected onHubTap(nodeId: string): void {
    void this.trace.selectNode(nodeId, 'usages');
    void this.router.navigate(['/explore'], { queryParams: { view: 'node' } });
  }

  /** M6.2: Export one-pager — copies a markdown-like text to clipboard. */
  protected async copyAtlas(): Promise<void> {
    const md = this.buildAtlasMarkdown();
    try {
      await navigator.clipboard.writeText(md);
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      // Fallback for non-https
    }
  }

  /** T6.11 — file download beside the clipboard export (`${repo}-atlas-${date}.md`,
   * matching the Studio's save-name convention). */
  protected downloadAtlas(): void {
    const md = this.buildAtlasMarkdown();
    const repo = (this.session.summary()?.label ?? 'repo').replace(/[^a-z0-9.-]+/gi, '-');
    const date = new Date().toISOString().slice(0, 10);
    const blob = new Blob([md], { type: 'text/markdown' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `${repo}-atlas-${date}.md`;
    a.click();
    URL.revokeObjectURL(a.href);
  }

  private buildAtlasMarkdown(): string {
    const s = this.session.summary();
    const lines: string[] = [];

    lines.push(`# Atlas — ${s?.label ?? 'Repo'}`);
    lines.push('');
    lines.push(`**Archetype:** ${this.session.mapResponse()?.archetype ?? 'unknown'} | **Projects:** ${this.topology().length} | **Entries:** ${s?.entries ?? 0}`);
    lines.push('');

    // Service diagram (text representation). Display names strip only the common solution
    // prefix (T6.8, audit A8) — the old last-segment cut exported "AppHost → API, API, API…".
    lines.push('## Services');
    const allNames = this.topology().map((p) => p.name);
    const dn = (name: string) => projectDisplayName(name, allNames);
    for (const p of this.topology()) {
      const style = this.serviceStyles().find((st) => st.projectName === p.name);
      const deps = p.dependsOn.length > 0 ? ` → ${p.dependsOn.map(dn).join(', ')}` : '';
      lines.push(`- **${dn(p.name)}**${style?.style ? ` (${style.style})` : ''}${deps}`);
      if (style?.stack.length) lines.push(`  Stack: ${style.stack.map((t) => humanizeTfms(t)).join(', ')}`);
    }
    lines.push('');

    // Top flows
    lines.push('## Top Flows');
    for (const f of this.atlasTopFlows()) {
      lines.push(`- **${f.title}** (${f.nodeCount} steps, ${f.maxDepth} deep, ${f.boundaryCrossings} cross-service)`);
    }
    lines.push('');

    // D4.3: the export mirrors the page — transports + data stores ride along.
    const queues = this.queueLinks();
    if (queues.length) {
      lines.push('## Queue Links');
      for (const q of queues) {
        lines.push(`- ${q.from} —[${q.transport}]→ ${q.to}`);
      }
      lines.push('');
    }
    const stores = this.dataStores();
    if (stores.length) {
      lines.push('## Data Stores');
      for (const d of stores) {
        lines.push(`- ${d.service}: ${d.stores.join(', ')}`);
      }
      lines.push('');
    }

    // Event wiring — the server's one T2.6 join (T6.11); cross-service hops labeled.
    const wiring = this.eventWiring();
    if (wiring.length) {
      const crossCount = wiring.filter((w) => w.crossService).length;
      lines.push(`## Event Wiring (${wiring.length} wires${crossCount > 0 ? `, ${crossCount} cross-service` : ''})`);
      for (const w of wiring) {
        lines.push(`- ${w.publisherTitle} → **${w.event}** → ${w.consumerTitle ?? 'unconsumed'}${w.crossService ? ' _(cross-service)_' : ''}`);
      }
      lines.push('');
    }

    // Hub radar
    const hubs = this.hubsWithDegree();
    if (hubs.length) {
      lines.push('## Hub Radar');
      for (const h of hubs) {
        lines.push(`- ${h.title}: ${h.flowCount} flows${h.degree ? ` (in ${h.degree.inDegree}, out ${h.degree.outDegree})` : ''}`);
      }
      lines.push('');
    }

    // Cross-cutting
    const behaviors = this.pipelineBehaviors();
    if (behaviors.length) lines.push(`## Pipeline Behaviors\n${behaviors.map((b) => `- ${b}`).join('\n')}\n`);

    return lines.join('\n');
  }
}
