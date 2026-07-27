/**
 * D4.2 (F3/M): the canvas's semantic vocabulary — pure mappings from engine facts
 * (ServiceCard.kind, TransportLink.transport tags, stack role tags) to visual classes.
 * Kept DOM-free so the vocabulary is unit-testable and shared across canvas levels.
 */

/** Transport tag → visual class. Tags come from ServiceLink edge tags verbatim
 * (e.g. "http", "bus", "AzureStorageQueue", "grpc") — classify, never invent. */
export type TransportClass = 'HTTP' | 'queue' | 'gRPC' | 'event' | 'other';

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
  if (/http|rest|typed-client/.test(t)) return { label: 'HTTP', cls: 'HTTP' };
  return { label: tag.length > 12 ? tag.slice(0, 11) + '…' : tag, cls: 'other' };
}

/** ServiceCard.kind → box-label glyph (GraphProjections.ClassifyService vocabulary:
 * "Web API" | "Gateway" | "gRPC" | "Service"). Plain services carry no glyph. */
export function serviceKindGlyph(kind: string): string {
  switch (kind) {
    case 'Web API': return 'API';
    case 'Gateway': return 'GW';
    case 'gRPC': return 'RPC';
    default: return '';
  }
}

/** RoleTags.DataStore as surfaced in ServiceCard.stack. */
export function hasDataStore(stack: readonly string[]): boolean {
  return stack.includes('datastore');
}

export function serviceLabel(displayName: string, kind: string, stack: readonly string[], truncate: (s: string) => string): string {
  const glyph = serviceKindGlyph(kind);
  return (glyph ? `[${glyph}] ` : '') + truncate(displayName) + (hasDataStore(stack) ? ' [db]' : '');
}
