import { inject, Injectable, signal } from '@angular/core';

import { DEVCONTEXT_CLIENT } from '../core/grpc/client';
import type {
  AnalysisSummary,
  CloseResponse,
  ContextPackResponse,
  ContextResponse,
  EntryPointsResponse,
  FlowIndexResponse,
  GraphFacetsResponse,
  ImpactResponse,
  InterestingPointsResponse,
  ListSessionsResponse,
  MapResponse,
  NeighborsResponse,
  NodeResponse,
  ProgressEvent,
  ReadSourceResponse,
  RenderResponse,
  SearchResponse,
  StatsResponse,
  TraceResponse,
  VerifyContextResponse,
} from '../core/grpc/gen/devcontext/v1/devcontext_pb';
import { ReadSourceMode } from '../core/grpc/gen/devcontext/v1/devcontext_pb';

export interface AnalyzeSpec {
  readonly path: string;
  readonly focus?: string;
  readonly depth?: number;
  readonly detail?: string;
  readonly noRoslyn?: boolean;
  readonly cleanup?: 'auto' | 'keep';
  /** R3 D-D: which solution to analyze when the repo declares several — a name, a file name, or a
   * repo-relative path, exactly as the CLI's `--sln` takes it. */
  readonly sln?: string;
}

export type AnalyzeOutcome =
  | { readonly ok: true; readonly handle: string; readonly summary: AnalysisSummary }
  | { readonly ok: false; readonly code: string; readonly message: string };

export type NeighborDirection = 'out' | 'in' | 'usages';

@Injectable({ providedIn: 'root' })
export class DevContextApi {
  private readonly client = inject(DEVCONTEXT_CLIENT);

  async analyze(
    spec: AnalyzeSpec,
    onProgress?: (p: ProgressEvent) => void,
    signal?: AbortSignal,
  ): Promise<AnalyzeOutcome> {
    const stream = this.client.analyze(
      {
        path: spec.path,
        focus: spec.focus,
        depth: spec.depth,
        detail: spec.detail,
        noRoslyn: spec.noRoslyn ?? false,
        cleanup: spec.cleanup,
        sln: spec.sln,
      },
      { signal },
    );

    for await (const evt of stream) {
      switch (evt.event.case) {
        case 'progress':
          onProgress?.(evt.event.value);
          break;
        case 'result':
          return { ok: true, handle: evt.event.value.handle, summary: evt.event.value.summary! };
        case 'error':
          return { ok: false, code: evt.event.value.code, message: evt.event.value.message };
        default:
          break;
      }
    }
    return { ok: false, code: 'NoResult', message: 'Analysis ended without a result.' };
  }

  getMap(handle: string): Promise<MapResponse> {
    return this.client.getMap({ handle });
  }

  getGraphFacets(handle: string, maxFlows = 10): Promise<GraphFacetsResponse> {
    return this.client.getGraphFacets({ handle, maxFlows });
  }

  /** T7.4 — the whole flow atlas (per-entry stats + hub degrees) in ONE call, memoized
   * server-side per session. Replaces the client-side ~100-getTrace background indexer. */
  getFlowIndex(handle: string, signal?: AbortSignal): Promise<FlowIndexResponse> {
    return this.client.getFlowIndex({ handle }, { signal });
  }

  listEntryPoints(handle: string): Promise<EntryPointsResponse> {
    return this.client.listEntryPoints({ handle });
  }

  getTrace(handle: string, focus: string, depth: number, detail: string, signal?: AbortSignal): Promise<TraceResponse> {
    return this.client.getTrace({ handle, focus, depth, detail }, { signal });
  }

  getNode(handle: string, nodeId: string, signal?: AbortSignal): Promise<NodeResponse> {
    return this.client.getNode({ handle, nodeId }, { signal });
  }

  getNeighbors(handle: string, nodeId: string, direction: NeighborDirection, signal?: AbortSignal): Promise<NeighborsResponse> {
    return this.client.getNeighbors({ handle, nodeId, direction }, { signal });
  }

  searchNodes(handle: string, query: string, limit: number): Promise<SearchResponse> {
    return this.client.searchNodes({ handle, query, limit });
  }

  getStats(handle: string): Promise<StatsResponse> {
    return this.client.getStats({ handle });
  }

  render(handle: string, options: { focus?: string; depth?: number; detail?: string; format?: string; sections?: string[]; includeDiagnostics?: boolean }): Promise<RenderResponse> {
    return this.client.render({
      handle,
      focus: options.focus,
      depth: options.depth,
      detail: options.detail,
      format: options.format ?? 'markdown',
      sections: options.sections ?? [],
      includeDiagnostics: options.includeDiagnostics ?? false,
    });
  }

  closeSession(handle: string): Promise<CloseResponse> {
    return this.client.closeSession({ handle });
  }

  /** T6.9 — live server sessions (repo path + handle + graph counts), for boot reattach. */
  listSessions(): Promise<ListSessionsResponse> {
    return this.client.listSessions({});
  }

  getImpact(handle: string, nodeId: string, maxDepth?: number): Promise<ImpactResponse> {
    return this.client.getImpact({ handle, nodeId, maxDepth: maxDepth ?? 0 });
  }

  getInterestingPoints(handle: string, archetype?: string): Promise<InterestingPointsResponse> {
    return this.client.getInterestingPoints({ handle, archetype });
  }

  getContext(handle: string, focus: string, options?: { budgetTokens?: number; intent?: 'trace' | 'explain' | 'review' }): Promise<ContextResponse> {
    return this.client.getContext({ handle, focus, budgetTokens: options?.budgetTokens, intent: options?.intent });
  }

  getContextPack(handle: string, cards: { type: string; title: string; entryIds: string[] }[], options?: { budgetTokens?: number; intent?: string }): Promise<ContextPackResponse> {
    return this.client.getContextPack({
      handle,
      cards: cards.map((c) => ({ type: c.type, title: c.title, entryIds: c.entryIds })),
      budgetTokens: options?.budgetTokens ?? 8000,
      intent: options?.intent ?? 'trace',
    });
  }

  /** T5.2 (audit R6) — per-section staleness of a focus's pack vs the disk (T4.5 RPC). */
  verifyContext(handle: string, focus: string, budgetTokens?: number): Promise<VerifyContextResponse> {
    return this.client.verifyContext({ handle, focus, budgetTokens });
  }

  readSource(handle: string, nodeId: string, options?: { mode?: ReadSourceMode; windowLines?: number }): Promise<ReadSourceResponse> {
    return this.client.readSource({
      sessionId: handle,
      nodeId,
      mode: options?.mode ?? ReadSourceMode.MEMBER,
      windowLines: options?.windowLines ?? 0,
    });
  }

  async ping(): Promise<{ ready: boolean; version: string }> {
    try {
      const res = await this.client.ping({});
      return { ready: res.ready, version: res.version };
    } catch {
      return { ready: false, version: '' };
    }
  }

  protected readonly _mcpRunning = signal(false);
  readonly mcpRunning = this._mcpRunning.asReadonly();

  setMcpRunning(running: boolean): void {
    this._mcpRunning.set(running);
  }

  async getMcpStatus(): Promise<boolean> {
    try {
      const resp = await this.client.startMcp({});
      return resp.running;
    } catch {
      return false;
    }
  }
}
