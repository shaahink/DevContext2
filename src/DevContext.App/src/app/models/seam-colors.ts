/**
 * Seam palette (F proposal §4.2) — One-Dark-adjacent hues tuned for Graphite surfaces.
 * Rationale: `Call` is the overwhelmingly common seam so it recedes to neutral;
 * crossing a process/messaging boundary glows warm (Send amber, Raise orange) with
 * the arriving side cool (Consume teal); plumbing (Resolve) stays quiet.
 * Used by trace chips and as Cytoscape node classes — keep keys in sync with
 * graph-canvas styles.
 */
export const SEAM_COLORS: Record<string, string> = {
  Handle: '#c678dd',
  Call: '#99a0ac',
  Send: '#e5c07b',
  Raise: '#d19a66',
  Consume: '#56b6c2',
  Data: '#6cb2eb',
  Resolve: '#5ac8fa',
  Pipeline: '#d16d9e',
};
