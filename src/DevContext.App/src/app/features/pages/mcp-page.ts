import { Component, inject, signal, type WritableSignal, type OnDestroy, type OnInit, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { copyToClipboard } from '../../core/clipboard';
import { DEVCONTEXT_CLIENT, type DevContextClient } from '../../core/grpc/client';
import { DevContextApi, type McpHostConfig } from '../../data-access/devcontext-api';
import { ToastService } from '../../ui/toast/toast';

function errorText(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
}

/** N0.2 (audit §3.F.12) — render the instant the SERVER stamped on the call. */
function fmtWireTime(timestampUtcMs: bigint | number): string {
  const ms = Number(timestampUtcMs);
  return ms > 0 ? new Date(ms).toLocaleTimeString() : '—';
}

interface ToolCallEntry {
  time: string;
  tool: string;
  repo: string;
  estTokens: number;
  elapsedMs: number;
  /** "ui" (the app's own gRPC-web traffic) or "agent" (MCP sidecar over native gRPC). */
  origin: string;
}

/** N4.1 — one handshake result, flattened for the template (the wire's bigint becomes a number). */
interface HandshakeView {
  ok: boolean;
  command: string;
  serverName: string;
  serverVersion: string;
  protocolVersion: string;
  toolCount: number;
  toolNames: string[];
  elapsedMs: number;
  error: string;
}

interface SessionItem {
  handle: string;
  repo: string;
  ageSeconds: number;
  calls: number;
  nodes: number;
  edges: number;
  entries: number;
  /** R4 item 10 — the ANALYSIS behind this session, which a cache hit makes much older than the session. */
  fromCache: boolean;
  analyzedAt: string;
}

/** N4.2 — what a write-config click did, per host card. */
interface WrittenConfig {
  ok: boolean;
  text: string;
  title: string;
}

@Component({
  selector: 'app-mcp-page',
  imports: [FormsModule],
  template: `
    <div class="mx-auto max-w-5xl px-5 pb-10 pt-6 space-y-6">
      <!-- Status card — N4.1 (audit §4, Room 2: "status that measures").
           What stood here was theater: one dot fed by a MUTATING StartMcp call, so it went
           green whenever the gRPC server was reachable, and a Start/Stop button that flipped a
           global telemetry mute while claiming to control an "MCP endpoint" it never touched.
           Three checks replaced it, and each one is something the server went and looked at:
           is the binary a host would spawn actually on disk, is anyone watching the stream,
           and has an agent ever called. The handshake below proves the rest. -->
      <div class="rounded-lg border border-line bg-surface p-4 space-y-3" data-testid="mcp-status-card">
        <div class="flex items-start justify-between gap-4">
          <div class="min-w-0 space-y-2">
            <!-- Check 1 — the binary a host config has to name. -->
            <div class="flex items-center gap-2 flex-wrap" data-testid="mcp-binary-check">
              <span
                class="flex h-3 w-3 rounded-full shrink-0"
                [class.bg-green]="binaryFound()"
                [class.bg-warn]="!binaryFound()"
                data-testid="mcp-status-dot"
              ></span>
              <span class="text-sm font-medium" data-testid="mcp-status-label">{{ binaryLabel() }}</span>
              @if (binarySource(); as src) {
                <span class="chip text-2xs" [title]="binarySourceHint()">{{ src }}</span>
              }
            </div>
            @if (binaryPath(); as path) {
              <div class="pl-5 font-mono text-2xs text-ink-subtle break-all" data-testid="mcp-binary-path">{{ path }}</div>
            }

            <!-- Check 2 + 3 — watchers, and whether an agent has actually been here. -->
            <div class="pl-5 text-xs text-ink-subtle" data-testid="mcp-status-text">{{ statusText() }}</div>
            <div class="pl-5 text-xs text-ink-subtle" data-testid="mcp-last-agent-call">{{ lastAgentCallText() }}</div>
          </div>

          <div class="flex flex-col items-end gap-1.5 shrink-0">
            <button
              class="rounded border border-line px-3 py-1.5 text-xs font-medium text-ink hover:bg-surface-raised transition-colors"
              [class.opacity-50]="handshakeRunning()"
              [disabled]="handshakeRunning()"
              title="Spawn the executable above and run one real MCP initialize + tools/list round trip"
              data-testid="mcp-handshake-run"
              (click)="runHandshake()"
            >{{ handshakeRunning() ? 'Handshaking…' : 'Test handshake' }}</button>
            <button
              class="text-2xs text-ink-subtle hover:text-ink transition-colors"
              data-testid="mcp-status-refresh"
              (click)="refreshStatus()"
            >Re-check</button>
          </div>
        </div>

        @if (statusError(); as err) {
          <!-- N0.2 (audit §3.F.14) — the status read used to swallow its failure and render a
               grey dot, which is also what a healthy-but-off endpoint looks like. -->
          <div class="text-xs text-danger" data-testid="mcp-status-error">{{ err }}</div>
        }

        @if (handshake(); as hs) {
          <!-- The handshake is the only check here that proves a host config would WORK: it
               spawns the same executable over the same transport and reads the same menu. -->
          <div class="rounded border border-line bg-base p-3 text-xs space-y-1" data-testid="mcp-handshake-result">
            @if (hs.ok) {
              <div class="text-ink">
                <span class="text-green">tools/list answered</span> — {{ hs.serverName }} v{{ hs.serverVersion }},
                protocol {{ hs.protocolVersion }}, {{ hs.toolCount }} tools in {{ hs.elapsedMs }}ms
              </div>
              <div class="font-mono text-2xs text-ink-subtle break-words" data-testid="mcp-handshake-tools">
                {{ hs.toolNames.join(' · ') }}
              </div>
            } @else {
              <div class="text-danger" data-testid="mcp-handshake-error">{{ hs.error }}</div>
              @if (hs.command) {
                <div class="font-mono text-2xs text-ink-subtle break-all">{{ hs.command }}</div>
              }
            }
          </div>
        }
      </div>

      <!-- Host config — N4.2 (audit §4, Room 2: "setup that works").
           The snippets are no longer written here. They arrive with the status read, composed by
           the same server code that writes the config file, around the same path the probe above
           resolved. That is the whole fix: the page used to hard-code a placeholder that read
           like a real machine's path, so a first-run user pasted a config naming an executable
           nobody had. It cannot drift from the writer now, because it IS the writer's output. -->
      <div>
        <div class="flex items-center justify-between mb-2 gap-3">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Host Config</h3>
          @if (writeTarget(); as target) {
            <div class="flex items-center gap-2 text-2xs text-ink-subtle min-w-0">
              <span class="shrink-0">Write into</span>
              @if (sessions().length > 1) {
                <select
                  class="rounded border border-line bg-base px-1.5 py-0.5 text-2xs max-w-xs"
                  data-testid="write-config-repo"
                  [ngModel]="target.handle"
                  (ngModelChange)="configHandle.set($event)"
                >
                  @for (s of sessions(); track s.handle) {
                    <option [value]="s.handle">{{ s.repo }}</option>
                  }
                </select>
              } @else {
                <span class="font-mono truncate" data-testid="write-config-repo">{{ target.repo }}</span>
              }
            </div>
          }
        </div>
        <p class="mb-2 text-2xs text-ink-subtle" data-testid="mcp-setup-note">{{ setupNote() }}</p>
        <div class="grid grid-cols-3 gap-3">
          @for (cfg of hosts(); track cfg.id) {
            <div class="rounded border border-line bg-surface p-3 flex flex-col">
              <div class="flex items-center justify-between mb-1.5">
                <span class="text-xs font-medium">{{ cfg.label }}</span>
                <!-- N0.2 (audit §3.F.11) — copy() used to guess which card was clicked by
                     sniffing the snippet text for the host name, and no snippet contains it,
                     so every copy flipped the VS Code card to "Copied!". The card says which
                     one it is; it does not need to be guessed. -->
                <button
                  class="text-2xs text-ink-subtle hover:text-ink transition-colors"
                  [attr.data-testid]="'copy-snippet-' + cfg.id"
                  (click)="void copy(cfg.id, cfg.snippet)"
                >{{ copied() === cfg.id ? 'Copied!' : 'Copy' }}</button>
              </div>
              <pre class="text-2xs text-ink-subtle overflow-x-auto max-h-24 font-mono"
                   [attr.data-testid]="'snippet-' + cfg.id">{{ cfg.snippet }}</pre>
              <div class="mt-2 pt-2 border-t border-line space-y-1">
                <button
                  class="w-full rounded border border-line px-2 py-1 text-2xs font-medium hover:bg-surface-raised transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                  [disabled]="!canWriteConfig() || writingHost() !== null"
                  [title]="writeButtonTitle(cfg)"
                  [attr.data-testid]="'write-config-' + cfg.id"
                  (click)="void writeConfig(cfg)"
                >{{ writingHost() === cfg.id ? 'Writing…' : 'Write ' + cfg.relativePath }}</button>
                @if (written()[cfg.id]; as result) {
                  <div
                    class="text-2xs break-all"
                    [class.text-ink-subtle]="result.ok"
                    [class.text-danger]="!result.ok"
                    [title]="result.title"
                    [attr.data-testid]="'write-result-' + cfg.id"
                  >{{ result.text }}</div>
                }
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Sessions -->
      <div>
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Sessions</h3>
          <button class="text-2xs text-ink-subtle hover:text-ink" (click)="refreshSessions()">Refresh</button>
        </div>
        @if (sessionsError(); as err) {
          <!-- N0.2 (audit §3.F.14) — the 30s poll used to fail silently, so a dead server and a
               repo with no sessions rendered the same empty table. -->
          <p class="text-xs text-danger py-2" data-testid="sessions-error">{{ err }}</p>
        }
        @if (sessions().length === 0) {
          <p class="text-xs text-ink-subtle py-4 text-center">No active sessions. Analyze a repo first.</p>
        } @else {
          <div class="rounded border border-line bg-surface overflow-hidden">
            <table class="w-full text-xs">
              <thead>
                <tr class="border-b border-line text-ink-subtle">
                  <th class="text-left p-2 font-medium">Repo</th>
                  <th class="text-left p-2 font-medium">Handle</th>
                  <th class="text-right p-2 font-medium" title="How long ago this session opened">Session</th>
                  <!-- N0.2 (audit §3.F.13) — the age column said "Age" and showed the SESSION's
                       age; on a snapshot-cache hit that is a brand-new session serving an
                       analysis from days ago, so the number lied about the data's freshness.
                       from_cache and analyzed_at were already on the wire, mapped, unrendered. -->
                  <th class="text-right p-2 font-medium" title="How old the ANALYSIS behind this session is">Analyzed</th>
                  <th class="text-right p-2 font-medium">Calls</th>
                  <th class="text-right p-2 font-medium">Nodes</th>
                  <th class="text-right p-2 font-medium">Edges</th>
                  <th class="text-right p-2 font-medium">Entries</th>
                  <th class="p-2"></th>
                </tr>
              </thead>
              <tbody>
                @for (s of sessions(); track s.handle) {
                  <tr class="border-b border-line/50 last:border-0 hover:bg-surface-raised">
                    <td class="p-2 truncate max-w-40" [title]="s.repo">{{ s.repo.split(/[\\/]/).pop() }}</td>
                    <!-- T6.10 (audit B9/A15): the page used to SHOW a truncated handle its own
                         TRY-A-TOOL then rejected. Full handle on hover, one-click copy, and a
                         "use" button that prefills the tool form — zero typing. -->
                    <td class="p-2 font-mono text-ink-subtle">
                      <button
                        type="button"
                        class="hover:text-ink transition-colors"
                        [title]="s.handle + ' — click to copy'"
                        data-testid="session-handle-copy"
                        (click)="void copyHandle(s.handle)"
                      >{{ copiedHandle() === s.handle ? 'Copied!' : s.handle.slice(0, 8) + '…' }}</button>
                    </td>
                    <td class="p-2 text-right font-mono tabular-nums">{{ fmtAge(s.ageSeconds) }}</td>
                    <td class="p-2 text-right font-mono tabular-nums" data-testid="session-analyzed" [title]="analysisAgeTitle(s)">
                      {{ analysisAge(s) }}
                    </td>
                    <td class="p-2 text-right">{{ s.calls }}</td>
                    <td class="p-2 text-right">{{ s.nodes }}</td>
                    <td class="p-2 text-right">{{ s.edges }}</td>
                    <td class="p-2 text-right">{{ s.entries }}</td>
                    <td class="p-2 text-right">
                      <button
                        type="button"
                        class="rounded border border-line px-1.5 py-0.5 text-2xs text-ink-muted hover:border-accent hover:text-ink transition-colors"
                        title="Prefill Try-a-Tool with this session"
                        data-testid="session-use"
                        (click)="useSession(s.handle)"
                      >use</button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Live feed — default-filtered to agent traffic (T6.10, audit B9: one page render
           logged 163 UI-origin calls / ~99k tok; agent calls would drown). -->
      <div>
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">
            Live Feed
            @if (visibleEvents().length > 0) {
              <span class="ml-2 font-normal">({{ visibleEvents().length }})</span>
            }
          </h3>
          <div class="flex items-center gap-2">
            <button
              type="button"
              class="chip text-2xs"
              [class.active]="!showUiCalls()"
              data-testid="feed-origin-filter"
              [title]="showUiCalls() ? 'Showing everything, including the app’s own gRPC traffic' : 'UI-origin calls hidden — showing agent traffic only'"
              (click)="showUiCalls.set(!showUiCalls())"
            >{{ showUiCalls() ? 'all origins' : 'agents only' }}</button>
            <!-- N0.2 (audit §3.F.12) — this was a running counter over every event ever seen,
                 including the UI-origin rows the filter hides and the rows that fell off the
                 200-row buffer, so "Total" described nothing on screen. It now sums exactly the
                 visible rows and says so. -->
            <span class="text-2xs text-ink-subtle tabular-nums" data-testid="feed-total"
              title="Sum over the rows shown — switch the origin filter to change it">
              Shown: {{ visibleTokens() }} tok
            </span>
            <button class="text-2xs text-ink-subtle hover:text-ink" (click)="clearEvents()">Clear</button>
          </div>
        </div>
        @if (feedError(); as err) {
          <!-- N0.2 (audit §3.F.14) — the stream's catch-all used to end the feed in silence. -->
          <p class="text-xs text-danger pb-2" data-testid="feed-error">{{ err }}</p>
        }
        @if (visibleEvents().length === 0) {
          <p class="text-xs text-ink-subtle py-4 text-center">
            @if (events().length > 0 && !showUiCalls()) {
              No agent calls yet — {{ events().length }} UI-origin calls hidden.
            } @else {
              No tool calls yet — this page is watching; ask an agent to call a DevContext tool.
            }
          </p>
        } @else {
          <div class="rounded border border-line bg-surface overflow-hidden">
            <div class="max-h-64 overflow-y-auto">
              @for (e of visibleEvents(); track $index) {
                <div class="flex items-center gap-2 px-3 py-1.5 border-b border-line/30 last:border-0 text-2xs hover:bg-surface-raised">
                  <span class="font-mono tabular-nums text-ink-subtle shrink-0 w-12">{{ e.time }}</span>
                  <span class="chip shrink-0 text-2xs" [class.text-accent]="e.origin === 'agent'">{{ e.origin }}</span>
                  <span class="font-medium shrink-0 w-20 truncate">{{ e.tool }}</span>
                  <span class="text-ink-subtle shrink-0 w-16 truncate">{{ e.repo.split(/[\\/]/).pop() }}</span>
                  <span class="font-mono tabular-nums text-ink-subtle">~{{ e.estTokens }}t</span>
                  <span class="font-mono tabular-nums text-ink-subtle">{{ e.elapsedMs }}ms</span>
                </div>
              }
            </div>
          </div>
        }
      </div>

      <!-- Try a tool -->
      <div>
        <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Try a Tool</h3>
        <div class="rounded border border-line bg-surface p-3 space-y-3">
          <div class="flex gap-2 items-end">
            <div class="flex-1">
              <label class="block text-2xs text-ink-subtle mb-1" for="try-tool-select">Tool</label>
              <select
                id="try-tool-select"
                class="w-full rounded border border-line bg-base px-2 py-1.5 text-xs"
                [(ngModel)]="selectedTool"
              >
                @for (t of availableTools; track t) {
                  <option [value]="t">{{ t }}</option>
                }
              </select>
            </div>
            <div class="flex-1">
              <label class="block text-2xs text-ink-subtle mb-1" for="try-handle-input">Handle</label>
              <input
                id="try-handle-input"
                class="w-full rounded border border-line bg-base px-2 py-1.5 text-xs font-mono"
                placeholder="session handle"
                [(ngModel)]="tryHandle"
              />
            </div>
            <div class="flex-1">
              <label class="block text-2xs text-ink-subtle mb-1" for="try-arg-input">Arg</label>
              <input
                id="try-arg-input"
                class="w-full rounded border border-line bg-base px-2 py-1.5 text-xs font-mono"
                placeholder="nodeId / focus"
                [(ngModel)]="tryArg"
              />
            </div>
            <button
              class="rounded bg-accent px-3 py-1.5 text-xs font-medium text-accent-ink hover:brightness-110"
              [class.opacity-50]="!tryHandle"
              [disabled]="!tryHandle"
              (click)="tryTool()"
            >Run</button>
          </div>
          @if (tryResult()) {
            <pre class="text-2xs text-ink-subtle font-mono max-h-40 overflow-auto bg-base rounded p-2 border border-line">{{ tryResult() }}</pre>
          }
        </div>
      </div>
    </div>
  `,
})
export class McpPage implements OnInit, OnDestroy {
  private readonly client: DevContextClient = inject(DEVCONTEXT_CLIENT);
  private readonly api = inject(DevContextApi);
  private readonly toast = inject(ToastService);

  /** N4.1 — the three measurements behind the status card (see the template's header comment).
   * There is no "telemetry streaming" flag any more: forwarding is unconditional server-side,
   * so the honest numbers are who is watching and what an agent actually did. */
  protected readonly observerCount = signal(0);
  protected readonly binaryFound = signal(false);
  protected readonly binaryPath = signal('');
  protected readonly binarySource = signal('');
  protected readonly lastAgentCallAtUtcMs = signal(0);
  protected readonly lastAgentTool = signal('');
  protected readonly agentCallCount = signal(0);
  /** Ticked by the same poll that refreshes status, so "3m ago" keeps meaning it. */
  private readonly nowMs = signal(Date.now());
  protected readonly handshake = signal<HandshakeView | null>(null);
  protected readonly handshakeRunning = signal(false);
  protected readonly copied = signal<string | null>(null);
  protected readonly copiedHandle = signal<string | null>(null);
  protected readonly events: WritableSignal<ToolCallEntry[]> = signal([]);
  protected readonly sessions = signal<SessionItem[]>([]);
  protected readonly tryResult = signal<string | null>(null);
  /** N0.2 (audit §3.F.14) — the three catch-alls (status read, session poll, event stream) now
   * each have a user-visible signal; they used to fail into a UI indistinguishable from idle. */
  protected readonly statusError = signal<string | null>(null);
  protected readonly sessionsError = signal<string | null>(null);
  protected readonly feedError = signal<string | null>(null);
  /** Default OFF: the feed exists to watch AGENTS; the app's own render chatter drowns it. */
  protected readonly showUiCalls = signal(false);
  protected readonly visibleEvents = computed(() =>
    this.showUiCalls() ? this.events() : this.events().filter((e) => e.origin !== 'ui'));
  /** N0.2 (audit §3.F.12) — the tokens of the rows actually on screen. */
  protected readonly visibleTokens = computed(() =>
    this.visibleEvents().reduce((n, e) => n + e.estTokens, 0));

  /** N4.2 — the setup cards, as the server composed them. The page renders; it does not build. */
  protected readonly hosts = signal<readonly McpHostConfig[]>([]);
  /** Which analyzed repo a write-config click targets; empty means "the first session". */
  protected readonly configHandle = signal('');
  protected readonly writingHost = signal<string | null>(null);
  protected readonly written = signal<Record<string, WrittenConfig>>({});
  /**
   * The sandbox probes gRPC RPCs but LABELS them with MCP tool names, so this list drifts from the
   * real menu independently of it. Two rows were already wrong when G2.1 folded the menu: there is
   * no MCP tool called search (it is find), and insights was a second door onto the same GetStats
   * call that stats makes — now folded away. See UnknownToolHandler for the menu itself.
   */
  protected readonly availableTools = ['stats', 'map', 'entrypoints', 'trace', 'node', 'find', 'impact', 'get_context'];
  protected selectedTool = 'stats';
  protected tryHandle = '';
  protected tryArg = '';

  private streamAbort: AbortController | null = null;
  private sessionTimer: ReturnType<typeof setInterval> | null = null;
  /** N4.1 — the one-shot re-read that lets the subscription land before the count is believed. */
  private settleTimer: ReturnType<typeof setTimeout> | null = null;

  /** N4.1 — what the binary probe found, named as a check with a verdict. */
  protected readonly binaryLabel = computed(() =>
    this.binaryFound()
      ? 'devcontext-mcp found — a host can spawn it'
      : 'devcontext-mcp not found — no host config here can work yet');

  protected readonly binarySourceHint = computed(() => {
    switch (this.binarySource()) {
      case 'bundle': return 'Found beside the DevContext server — the copy the desktop ships';
      case 'path': return 'Found on PATH — an installed tool or a manual copy';
      case 'dev-build': return 'Found in this repo’s build output (dotnet build src/DevContext.Mcp)';
      default: return '';
    }
  });

  /** L6.6: Status text reflects live state instead of static toggle label.
   * N0.2 (audit §3.F.9) — and it now describes what was measured. The old copy ("Accepting
   * connections", "Endpoint stopped") described an MCP endpoint this server neither owns nor
   * observes. N4.1 — with the mute gone, the watcher count is the whole story: every tool call
   * this server serves is forwarded to whoever is subscribed. */
  protected readonly statusText = computed(() => {
    const watchers = this.observerCount();
    const live = this.sessions().length;
    const sessions = live > 0 ? `${live} analysis session(s) open.` : 'No analysis sessions open.';
    return `Every tool call this server serves is forwarded — ${watchers} watcher(s) attached. ${sessions}`;
  });

  /**
   * N4.2 — where a written config would go. The config is PROJECT-scoped (see McpConfigWriter for
   * why it is not the user-global file), so it needs an analyzed repo; with none open there is
   * nothing honest to write and the buttons say so.
   */
  protected readonly writeTarget = computed(() => {
    const open = this.sessions();
    if (open.length === 0) return null;
    return open.find((s) => s.handle === this.configHandle()) ?? open[0];
  });

  protected readonly canWriteConfig = computed(() => this.writeTarget() !== null && this.binaryFound());

  /** What the user must actually do next, keyed off what the probe found — not a fixed sentence. */
  protected readonly setupNote = computed(() => {
    if (!this.binaryFound()) {
      return 'No devcontext-mcp on this machine yet — build it (dotnet build src/DevContext.Mcp) or install the '
        + 'desktop bundle, then Re-check. Until then the snippets below carry a placeholder, not a path.';
    }
    const shipped = this.binarySource() === 'bundle'
      ? 'It ships beside this app, so the path below exists on any machine with the installer. '
      : '';
    return `${shipped}The snippets name the executable the probe just resolved — copy one, or let DevContext write `
      + 'the config into the repo for you.';
  });

  protected writeButtonTitle(host: McpHostConfig): string {
    if (!this.binaryFound()) return 'Nothing to point a host at yet — devcontext-mcp was not found';
    const target = this.writeTarget();
    if (!target) return 'Analyze a repo first — the config is written into that repo';
    return `Write ${host.relativePath} into ${target.repo}, merging with anything already there`;
  }

  /** N4.1 — the check the old page could not make at all: has an agent ever actually been here. */
  protected readonly lastAgentCallText = computed(() => {
    const at = this.lastAgentCallAtUtcMs();
    if (at <= 0) return 'No agent has called this server yet — the calls below are the app’s own.';
    const age = this.fmtAge(Math.max(0, Math.round((this.nowMs() - at) / 1000)));
    const tool = this.lastAgentTool() || 'a tool';
    return `Last agent call: ${tool}, ${age} ago — ${this.agentCallCount()} agent call(s) served.`;
  });

  async ngOnInit(): Promise<void> {
    this.refreshSessions();
    this.sessionTimer = setInterval(() => {
      this.refreshSessions();
      // N4.1 — the status numbers age (last agent call, watcher count), so they are polled with
      // the sessions rather than read once at open and left to rot on screen.
      void this.refreshStatus();
    }, 30_000);

    // N4.1 — subscribe first, THEN read: this page is itself a watcher, and a count read before
    // subscribing renders one lower than the truth it is describing.
    this.startStream();
    await this.refreshStatus();
    // MEASURED: subscribing is a network round trip, so the read above still beats it and the
    // card opened saying "0 watcher(s) attached" while this very page was watching. One
    // follow-up read settles it without pretending to know the count locally.
    this.settleTimer = setTimeout(() => { void this.refreshStatus(); }, 1_000);
  }

  ngOnDestroy(): void {
    if (this.sessionTimer) clearInterval(this.sessionTimer);
    if (this.settleTimer) clearTimeout(this.settleTimer);
    this.stopStream();
  }

  /** N0.2 (audit §3.F.9) — a READ. This used to be `startMcp`, so opening the page turned
   * telemetry on for every observer and then reported "active" because of its own call. */
  protected async refreshStatus(): Promise<void> {
    const status = await this.api.getMcpStatus();
    this.nowMs.set(Date.now());
    if (status === null) {
      this.statusError.set('Could not reach the DevContext server to read MCP status.');
      return;
    }
    this.statusError.set(null);
    this.observerCount.set(status.observerCount);
    this.binaryFound.set(status.binaryFound);
    this.binaryPath.set(status.binaryPath);
    this.binarySource.set(status.binarySource);
    this.lastAgentCallAtUtcMs.set(status.lastAgentCallAtUtcMs);
    this.lastAgentTool.set(status.lastAgentTool);
    this.agentCallCount.set(status.agentCallCount);
    this.hosts.set(status.hosts);
  }

  /**
   * N4.2 — the write-config-for-me button. The server owns the whole decision: which file, which
   * key, which command path, and whether anything needed changing. This reports what came back,
   * including "unchanged" — a button that claimed to write every time would be the same species
   * of lie the audit found in the old status dot.
   */
  protected async writeConfig(host: McpHostConfig): Promise<void> {
    const target = this.writeTarget();
    if (!target || this.writingHost() !== null) return;

    this.writingHost.set(host.id);
    try {
      const result = await this.api.writeMcpConfig(target.handle, host.id);
      const text = result.action === 'unchanged'
        ? `${result.relativePath} already points here`
        : `${result.action === 'created' ? 'Wrote' : 'Updated'} ${result.relativePath}`;
      this.record(host.id, { ok: true, text, title: `${result.path}\ncommand: ${result.command}` });
      this.toast.show(`${text} — restart ${host.label} to pick it up`, 'success');
    } catch (err) {
      const message = errorText(err);
      this.record(host.id, { ok: false, text: message, title: message });
      this.toast.show(`Could not write ${host.relativePath}: ${message}`, 'error');
    } finally {
      this.writingHost.set(null);
    }
  }

  private record(hostId: string, result: WrittenConfig): void {
    this.written.update((all) => ({ ...all, [hostId]: result }));
  }

  /**
   * N4.1 — the handshake. Spawns the resolved executable server-side and runs one real
   * initialize + tools/list round trip, so a green result means a host config naming that path
   * would work. Nothing else on this page can claim that.
   */
  protected runHandshake(): void {
    if (this.handshakeRunning()) return;
    this.handshakeRunning.set(true);
    this.client.mcpHandshake({}).then((resp) => {
      this.handshake.set({
        ok: resp.ok,
        command: resp.command,
        serverName: resp.serverName,
        serverVersion: resp.serverVersion,
        protocolVersion: resp.protocolVersion,
        toolCount: resp.toolCount,
        toolNames: resp.toolNames,
        elapsedMs: Number(resp.elapsedMs),
        error: resp.error,
      });
      // A handshake that ran is also a fresh look at the binary — re-read the probe.
      void this.refreshStatus();
    }).catch((err) => {
      this.handshake.set({
        ok: false, command: '', serverName: '', serverVersion: '', protocolVersion: '',
        toolCount: 0, toolNames: [], elapsedMs: 0,
        error: `The server could not run the handshake: ${errorText(err)}`,
      });
    }).finally(() => this.handshakeRunning.set(false));
  }

  /** N0.2 (audit §3.F.11) — the card names itself; the old version sniffed the snippet TEXT for
   * a host name none of them contain, so every copy marked the VS Code card. Through the
   * Tauri-aware helper, and a rejected write is reported instead of leaving a dead button. */
  protected async copy(host: string, text: string): Promise<void> {
    try {
      await copyToClipboard(text);
      this.copied.set(host);
      setTimeout(() => this.copied.set(null), 2000);
    } catch (err) {
      this.toast.show(`Copy failed: ${errorText(err)}`, 'error');
    }
  }

  /** T6.10 — copy the FULL handle (the table shows a truncated one for width). */
  protected async copyHandle(handle: string): Promise<void> {
    try {
      await copyToClipboard(handle);
      this.copiedHandle.set(handle);
      setTimeout(() => this.copiedHandle.set(null), 2000);
    } catch (err) {
      this.toast.show(`Copy failed: ${errorText(err)}`, 'error');
    }
  }

  /** T6.10 — one click prefills TRY-A-TOOL with a live session: zero typing to a working call. */
  protected useSession(handle: string) {
    this.tryHandle = handle;
  }

  protected refreshSessions() {
    this.client.listSessions({}).then((resp) => {
      this.sessionsError.set(null);
      this.sessions.set(
        (resp.sessions || []).map((s) => ({
          handle: s.handle,
          repo: s.repo,
          ageSeconds: Number(s.ageSeconds),
          calls: s.calls,
          nodes: s.nodes,
          edges: s.edges,
          entries: s.entries,
          fromCache: s.fromCache,
          analyzedAt: s.analyzedAt,
        })),
      );
    }).catch((err) => {
      // N0.2 (audit §3.F.14) — a failed poll used to leave the last table on screen with no
      // hint that it had stopped refreshing.
      this.sessionsError.set(`Session list is stale — refresh failed: ${errorText(err)}`);
    });
  }

  protected clearEvents() {
    this.events.set([]);
  }

  protected fmtAge(seconds: number): string {
    if (seconds < 60) return `${seconds}s`;
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m`;
    return `${Math.floor(seconds / 3600)}h`;
  }

  /** N0.2 (audit §3.F.13) — how old the ANALYSIS is, which after a snapshot-cache hit is the
   * number that matters: the session is seconds old, the data behind it can be days old. */
  protected analysisAge(s: SessionItem): string {
    if (!s.analyzedAt) return '—';
    const ms = Date.parse(s.analyzedAt);
    if (Number.isNaN(ms)) return '—';
    const age = this.fmtAge(Math.max(0, Math.round((Date.now() - ms) / 1000)));
    return s.fromCache ? `${age} (cached)` : age;
  }

  protected analysisAgeTitle(s: SessionItem): string {
    if (!s.analyzedAt) return 'The server did not report when this analysis ran';
    return s.fromCache
      ? `Analysis finished ${s.analyzedAt} and was rehydrated from the snapshot cache — the session is younger than the data`
      : `Analysis finished ${s.analyzedAt}`;
  }

  protected tryTool() {
    if (!this.tryHandle) return;
    const tool = this.selectedTool;
    const handle = this.tryHandle;
    const arg = this.tryArg;

    // Map tool to RPC call
    const call = (() => {
      switch (tool) {
        case 'stats': return this.client.getStats({ handle });
        case 'map': return this.client.getMap({ handle });
        case 'entrypoints': return this.client.listEntryPoints({ handle });
        case 'trace': return this.client.getTrace({ handle, focus: arg || handle });
        case 'node': return this.client.getNode({ handle, nodeId: arg });
        case 'find': return this.client.searchNodes({ handle, query: arg });
        case 'impact': return this.client.getImpact({ handle, nodeId: arg, maxDepth: 4 });
        case 'get_context': return this.client.getContext({ handle, focus: arg, budgetTokens: 4000 });
        default: return null;
      }
    })();

    if (!call) return;

    call.then((resp) => {
      this.tryResult.set(JSON.stringify(resp, null, 2));
    }).catch((err) => {
      this.tryResult.set(`Error: ${err.message}`);
    });
  }

  private startStream() {
    this.streamAbort = new AbortController();
    const stream = this.client.observeToolCalls({}, { signal: this.streamAbort.signal });

    (async () => {
      try {
        this.feedError.set(null);
        for await (const evt of stream) {
          const entry: ToolCallEntry = {
            // N0.2 (audit §3.F.12) — the WIRE timestamp. This was the time the browser happened
            // to receive the row, so a reconnect or a buffered burst stamped old calls "now";
            // timestamp_utc_ms has been on the event since M3.3 and nothing read it.
            time: fmtWireTime(evt.timestampUtcMs),
            tool: evt.tool,
            repo: evt.sessionRepo ?? '',
            estTokens: Number(evt.estTokens),
            elapsedMs: Number(evt.elapsedMs),
            origin: evt.origin || 'agent',
          };
          this.events.update((arr) => [entry, ...arr].slice(0, 200));
        }
      } catch (err) {
        // N0.2 (audit §3.F.14) — a dropped stream used to be indistinguishable from a quiet one.
        if (!this.streamAbort?.signal.aborted) {
          this.feedError.set(`Live feed disconnected: ${errorText(err)}`);
        }
      }
    })();
  }

  private stopStream() {
    this.streamAbort?.abort();
    this.streamAbort = null;
  }
}
