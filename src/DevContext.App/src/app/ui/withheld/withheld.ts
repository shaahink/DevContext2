import { Component, input } from '@angular/core';

/**
 * Why a surface has nothing to show. Three classes, and they are NOT interchangeable — the
 * whole point of R3 C-2 is that "nothing was looked at" and "nothing was found" are different
 * sentences, and Atlas was saying the wrong one.
 *
 * - `archetype`     — the subject does not exist for this kind of repo. A library has no
 *                     services, so it has no per-service data stores; that is not a gap.
 * - `none-found`    — the subject exists, was examined, and nothing matched.
 * - `not-computed`  — the subject exists but the data has not been produced yet (indexing).
 */
export type WithheldReason = 'archetype' | 'none-found' | 'not-computed';

/**
 * A section that withholds itself, saying why — the S9 shape, made reusable.
 *
 * S9 found the whole Confidence Ledger suppressed on every entry-less repo (i.e. every library):
 * 169 edges with a computed verified/approximate split that no reader could reach, because the
 * panel that held them was gated away. The fix was NOT to hide better — it was to show the panel
 * and have the two entry-dependent rows withhold themselves. That is the rule this component
 * carries: keep the heading, keep the shape of the page, and state the reason in place of the
 * content.
 *
 * The data attributes are the machine-readable half. A surface is checkable by
 * `scripts/g71-atlas-empty-sections.mts` only because a withheld notice is structurally distinct
 * from content — a text grep would pass on a page that had simply deleted the section, which is
 * the defect S9 named.
 */
@Component({
  selector: 'app-withheld',
  template: `{{ text() }}`,
  host: {
    class: 'block py-4 text-center text-xs text-ink-subtle',
    'data-withheld': '',
    '[attr.data-reason]': 'reason()',
  },
})
export class Withheld {
  readonly reason = input.required<WithheldReason>();
  /** One sentence, in the section's own words, naming why it is empty on THIS repo. */
  readonly text = input.required<string>();
}
