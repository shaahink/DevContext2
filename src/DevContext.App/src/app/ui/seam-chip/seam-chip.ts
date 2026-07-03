import { Component, computed, input } from '@angular/core';

import { seamColor } from '../../models/seam-colors';

/** Renders a trace/graph seam (Entry, Call, Send, Handle, Raise, Consume, Data, Resolve,
 * Pipeline — `SeamKind` on the wire) as a colored `.chip`, per the seam palette (proposal
 * §4.2). Looked up case-insensitively so a caller passing either casing still resolves. */
@Component({
  selector: 'app-seam-chip',
  template: '{{ seam() }}',
  host: {
    class: 'chip font-mono',
    '[style.color]': 'color()',
    '[style.borderColor]': 'borderColor()',
  },
})
export class SeamChip {
  readonly seam = input.required<string>();

  protected readonly color = computed(() => seamColor(this.seam()) ?? 'var(--vibe-ink-muted)');
  protected readonly borderColor = computed(() => {
    const c = seamColor(this.seam());
    return c ? `color-mix(in srgb, ${c} 40%, transparent)` : 'var(--vibe-line)';
  });
}
