/**
 * Seam palette (F proposal §4.2) — One-Dark-adjacent hues tuned for Graphite surfaces.
 * Rationale: `Call` is the overwhelmingly common seam so it recedes to neutral;
 * crossing a process/messaging boundary glows warm (Send amber, Raise orange) with
 * the arriving side cool (Consume teal); plumbing (Resolve) stays quiet; roots (Entry)
 * are landmarks so they take the accent itself.
 * Used by trace chips and as Cytoscape node classes — keep keys in sync with
 * graph-canvas styles AND with `SeamKind` in `DevContext.Core/Graph/TraceBuilder.cs`
 * (the wire value is `SeamKind.ToString()` — PascalCase singular: Entry, Call, Send,
 * Handle, Raise, Consume, Data, Resolve, Pipeline. Do not "fix" these to match the
 * proposal doc's prose table, which uses different — wrong — casing/names).
 */
export const SEAM_COLORS: Record<string, string> = {
  Entry: '#8b93ff',
  Handle: '#c678dd',
  Call: '#99a0ac',
  Send: '#e5c07b',
  Raise: '#d19a66',
  Consume: '#56b6c2',
  Data: '#6cb2eb',
  Resolve: '#5ac8fa',
  Pipeline: '#d16d9e',
  CrossService: '#98c379',
};

/** Case-insensitive lookup — defends against a caller passing lowercase (a real bug found
 * in the pre-redesign `trace-node.ts`, which had its own stale lowercase/plural map that
 * never actually matched the wire's PascalCase singular values). */
export function seamColor(seam: string): string | undefined {
  return SEAM_COLORS[seam] ?? SEAM_COLORS[seam.charAt(0).toUpperCase() + seam.slice(1).toLowerCase()];
}
