import { Component, inject, signal, type WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DEVCONTEXT_CLIENT, type DevContextClient } from '../../core/grpc/client';

interface ToolCallEntry {
  time: string;
  tool: string;
  session: string;
  repo: string;
  bytes: number;
  estTokens: number;
  elapsedMs: number;
}

interface SessionItem {
  handle: string;
  repo: string;
  ageSeconds: number;
  calls: number;
  nodes: number;
  edges: number;
  entries: number;
}

const CONFIG_SNIPPETS: { host: string; snippet: string }[] = [
  {
    host: 'Claude Code',
    snippet: `{
  "mcpServers": {
    "devcontext": {
      "command": "devcontext-mcp",
      "args": []
    }
  }
}`,
  },
  {
    host: 'Cursor',
    snippet: `{
  "mcpServers": {
    "devcontext": {
      "command": "devcontext-mcp"
    }
  }
}`,
  },
  {
    host: 'VS Code',
    snippet: `{
  "inputs": [],
  "servers": {
    "devcontext": {
      "command": "devcontext-mcp"
    }
  }
}`,
  },
];

@Component({
  selector: 'app-mcp-page',
  imports: [FormsModule],
  template: `
    <div class="mx-auto max-w-5xl px-5 pb-10 pt-6 space-y-6">
      <!-- Status card -->
      <div class="rounded-lg border border-line bg-surface p-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <span
              class="flex h-3 w-3 rounded-full shrink-0"
              [class.bg-green]="mcpRunning()"
              [class.bg-ink-subtle]="!mcpRunning()"
            ></span>
            <span class="text-sm font-medium">
              {{ mcpRunning() ? 'MCP Endpoint Active' : 'MCP Endpoint Stopped' }}
            </span>
            @if (observerCount() > 0) {
              <span class="text-xs text-ink-subtle">{{ observerCount() }} observer(s)</span>
            }
          </div>
          <button
            class="rounded px-3 py-1.5 text-xs font-medium border transition-colors"
            [class.border-warn/30]="mcpRunning()"
            [class.text-warn]="mcpRunning()"
            [class.hover:bg-warn/10]="mcpRunning()"
            [class.border-line]="!mcpRunning()"
            [class.text-ink]="!mcpRunning()"
            [class.hover:bg-surface-raised]="!mcpRunning()"
            (click)="toggleMcp()"
          >
            {{ mcpRunning() ? 'Stop' : 'Start' }}
          </button>
        </div>
        <div class="mt-2 text-xs text-ink-subtle">
          {{ mcpRunning() ? 'Agents can connect. Tools are served over stdio ↔ gRPC.' : 'Connections refused. Toggle to allow new sessions.' }}
        </div>
      </div>

      <!-- Config snippets -->
      <div>
        <h3 class="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-subtle">Host Config</h3>
        <div class="grid grid-cols-3 gap-3">
          @for (cfg of configSnippets; track cfg.host) {
            <div class="rounded border border-line bg-surface p-3">
              <div class="flex items-center justify-between mb-1.5">
                <span class="text-xs font-medium">{{ cfg.host }}</span>
                <button
                  class="text-2xs text-ink-subtle hover:text-ink transition-colors"
                  (click)="copy(cfg.snippet)"
                >{{ copied() === cfg.host ? 'Copied!' : 'Copy' }}</button>
              </div>
              <pre class="text-2xs text-ink-subtle overflow-x-auto max-h-24 font-mono">{{ cfg.snippet }}</pre>
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
        @if (sessions().length === 0) {
          <p class="text-xs text-ink-subtle py-4 text-center">No active sessions. Analyze a repo first.</p>
        } @else {
          <div class="rounded border border-line bg-surface overflow-hidden">
            <table class="w-full text-xs">
              <thead>
                <tr class="border-b border-line text-ink-subtle">
                  <th class="text-left p-2 font-medium">Repo</th>
                  <th class="text-left p-2 font-medium">Handle</th>
                  <th class="text-right p-2 font-medium">Age</th>
                  <th class="text-right p-2 font-medium">Calls</th>
                  <th class="text-right p-2 font-medium">Nodes</th>
                </tr>
              </thead>
              <tbody>
                @for (s of sessions(); track s.handle) {
                  <tr class="border-b border-line/50 last:border-0 hover:bg-surface-raised">
                    <td class="p-2 truncate max-w-40" [title]="s.repo">{{ s.repo.split(/[\\/]/).pop() }}</td>
                    <td class="p-2 font-mono text-ink-subtle">{{ s.handle.slice(0, 8) }}</td>
                    <td class="p-2 text-right font-mono tabular-nums">{{ fmtAge(s.ageSeconds) }}</td>
                    <td class="p-2 text-right">{{ s.calls }}</td>
                    <td class="p-2 text-right">{{ s.nodes }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Live feed -->
      <div>
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-2xs font-semibold uppercase tracking-wider text-ink-subtle">
            Live Feed
            @if (events().length > 0) {
              <span class="ml-2 font-normal">({{ events().length }})</span>
            }
          </h3>
          <div class="flex items-center gap-2">
            <span class="text-2xs text-ink-subtle tabular-nums">Total: {{ totalTokens() }} tok</span>
            <button class="text-2xs text-ink-subtle hover:text-ink" (click)="clearEvents()">Clear</button>
          </div>
        </div>
        @if (events().length === 0) {
          <p class="text-xs text-ink-subtle py-4 text-center">No tool calls yet. Start MCP and trigger a tool call.</p>
        } @else {
          <div class="rounded border border-line bg-surface overflow-hidden">
            <div class="max-h-64 overflow-y-auto">
              @for (e of events(); track $index) {
                <div class="flex items-center gap-2 px-3 py-1.5 border-b border-line/30 last:border-0 text-2xs hover:bg-surface-raised">
                  <span class="font-mono tabular-nums text-ink-subtle shrink-0 w-12">{{ e.time }}</span>
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
export class McpPage {
  private readonly client: DevContextClient = inject(DEVCONTEXT_CLIENT);

  protected readonly mcpRunning = signal(false);
  protected readonly observerCount = signal(0);
  protected readonly copied = signal<string | null>(null);
  protected readonly events: WritableSignal<ToolCallEntry[]> = signal([]);
  protected readonly totalTokens = signal(0);
  protected readonly sessions = signal<SessionItem[]>([]);
  protected readonly tryResult = signal<string | null>(null);

  protected readonly configSnippets = CONFIG_SNIPPETS;
  protected readonly availableTools = ['stats', 'map', 'entrypoints', 'trace', 'node', 'search', 'impact', 'insights', 'get_context'];
  protected selectedTool = 'stats';
  protected tryHandle = '';
  protected tryArg = '';

  private streamAbort: AbortController | null = null;

  constructor() {
    this.refreshSessions();
  }

  protected toggleMcp() {
    if (this.mcpRunning()) {
      this.client.stopMcp({}).then(() => {
        this.mcpRunning.set(false);
        this.stopStream();
      }).catch(() => { this.mcpRunning.set(false); });
    } else {
      this.client.startMcp({}).then((resp) => {
        this.mcpRunning.set(resp.running);
        if (resp.running) this.startStream();
      }).catch(() => { /* server unreachable */ });
    }
  }

  protected copy(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      this.copied.set(text.includes('Claude') ? 'Claude Code' : text.includes('Cursor') ? 'Cursor' : 'VS Code');
      setTimeout(() => this.copied.set(null), 2000);
    });
  }

  protected refreshSessions() {
    this.client.listSessions({}).then((resp) => {
      this.sessions.set(
        (resp.sessions || []).map((s) => ({
          handle: s.handle,
          repo: s.repo,
          ageSeconds: Number(s.ageSeconds),
          calls: s.calls,
          nodes: s.nodes,
          edges: s.edges,
          entries: s.entries,
        })),
      );
    }).catch(() => { /* server unreachable, sessions list stays empty */ });
  }

  protected clearEvents() {
    this.events.set([]);
    this.totalTokens.set(0);
  }

  protected fmtAge(seconds: number): string {
    if (seconds < 60) return `${seconds}s`;
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m`;
    return `${Math.floor(seconds / 3600)}h`;
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
        case 'search': return this.client.searchNodes({ handle, query: arg });
        case 'impact': return this.client.getImpact({ handle, nodeId: arg, maxDepth: 4 });
        case 'insights': return this.client.getStats({ handle });
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
        for await (const evt of stream) {
          const entry: ToolCallEntry = {
            time: new Date().toLocaleTimeString(),
            tool: evt.tool,
            session: evt.sessionHandle?.slice(0, 8) ?? '',
            repo: evt.sessionRepo ?? '',
            bytes: Number(evt.bytes),
            estTokens: Number(evt.estTokens),
            elapsedMs: Number(evt.elapsedMs),
          };
          this.events.update((arr) => [entry, ...arr].slice(0, 200));
          this.totalTokens.update((t) => t + entry.estTokens);
        }
      } catch {
        // Stream closed
      }
    })();
  }

  private stopStream() {
    this.streamAbort?.abort();
    this.streamAbort = null;
  }
}
