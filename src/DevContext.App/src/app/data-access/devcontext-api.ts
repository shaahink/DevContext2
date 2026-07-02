import { inject, Injectable } from '@angular/core';

import { DEVCONTEXT_CLIENT } from '../core/grpc/client';
import type {
  AnalysisSummary,
  CloseResponse,
  EntryPointsResponse,
  MapResponse,
  NeighborsResponse,
  NodeResponse,
  ProgressEvent,
  RenderResponse,
  SearchResponse,
  StatsResponse,
  TraceResponse,
} from '../core/grpc/gen/devcontext/v1/devcontext_pb';

export interface AnalyzeSpec {
  readonly path: string;
  readonly focus?: string;
  readonly depth?: number;
  readonly detail?: string;
  readonly noRoslyn?: boolean;
  readonly cleanup?: 'auto' | 'keep';
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

  listEntryPoints(handle: string): Promise<EntryPointsResponse> {
    return this.client.listEntryPoints({ handle });
  }

  getTrace(handle: string, focus: string, depth: number, detail: string): Promise<TraceResponse> {
    return this.client.getTrace({ handle, focus, depth, detail });
  }

  getNode(handle: string, nodeId: string): Promise<NodeResponse> {
    return this.client.getNode({ handle, nodeId });
  }

  getNeighbors(handle: string, nodeId: string, direction: NeighborDirection): Promise<NeighborsResponse> {
    return this.client.getNeighbors({ handle, nodeId, direction });
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

  async ping(): Promise<{ ready: boolean; version: string }> {
    try {
      const res = await this.client.ping({});
      return { ready: res.ready, version: res.version };
    } catch {
      return { ready: false, version: '' };
    }
  }
}
