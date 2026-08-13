import { Component, computed, input, model, output } from '@angular/core';

export type LensId = 'service' | 'layer' | 'feature' | 'flow';

interface LensDef {
  readonly id: LensId;
  readonly label: string;
  readonly hint: string;
  /** M1.2 — this lens colours by a FACET the analysis may or may not have found. When it did not,
   * the chip is not rendered at all: a lens that recolours nothing into one flat grey is a
   * confident surface over absent substance, which is exactly what this batch removes. */
  readonly facet?: 'layer' | 'feature';
}

const LENSES: readonly LensDef[] = [
  { id: 'service', label: 'Service', hint: 'Project topology — runtime grouping (current system view)' },
  { id: 'layer', label: 'Layer', hint: 'Nodes colored by architectural layer (API/App/Domain/Infra)', facet: 'layer' },
  { id: 'feature', label: 'Feature', hint: 'Nodes colored by feature area (namespace-derived)', facet: 'feature' },
  { id: 'flow', label: 'Flow', hint: 'Trace tree or graph — current flow view' },
];

/**
 * Lens Switcher (M7.2) — replaces the hardcoded altitude selector with named lenses.
 * Each lens is a named projection of the graph; Service=topology, Flow=trace, and
 * Layer/Feature colour by the per-project facets the wire carries on
 * `MapResponse.topology[].layer` / `.feature` and `ServiceCard.layer` / `.feature`.
 * The `lens` model() is lifted to the page so it can be mirrored into URL state and
 * each page can set its own default (Explore→Flow, Atlas→Service).
 *
 * M1.2: those two facets are OPTIONAL on the wire and empty for most repos, and the
 * chips used to render regardless — clicking one repainted every node the same muted
 * grey and, for Feature, drew a legend with nothing in it. The chips now appear only
 * when {@link facets} says the current analysis actually carries that facet, so the
 * toolbar stops advertising a view the data cannot support. This is not the D9 work
 * (richer engine-side facets); it is the honesty gate in front of it.
 *
 * L6.5: Added a visible "Table" toolbar button and global Shift+E shortcut.
 */
@Component({
  selector: 'app-lens-switcher',
  template: `
    @for (lens of lenses(); track lens.id) {
      <button
        type="button"
        class="chip shrink-0"
        [class.active]="lensModel() === lens.id"
        [attr.data-testid]="'lens-' + lens.id"
        [title]="lens.hint"
        (click)="lensModel.set(lens.id)"
      >
        {{ lens.label }}
      </button>
    }
    <span class="mx-1 h-4 w-px bg-line"></span>
    <!-- D4.5 (L5): opens the grouped entry browser (raw table = Shift+E power view). -->
    <button
      type="button"
      class="chip shrink-0 inline-flex items-center gap-1"
      title="Browse all entries — grouped by service (raw table: Shift+E)"
      (click)="tableRequested.emit()"
    >
      <span class="text-2xs opacity-60">&#9776;</span> Entries
    </button>
  `,
  host: { class: 'contents' },
})
export class LensSwitcher {
  /** Which facets the CURRENT analysis carries. Defaults to neither, so a caller that does not
   * measure gets the honest subset rather than the optimistic one. */
  readonly facets = input<{ readonly layer: boolean; readonly feature: boolean }>({ layer: false, feature: false });

  readonly lensModel = model<LensId>('flow');

  readonly lenses = computed<readonly LensDef[]>(() => {
    const have = this.facets();
    return LENSES.filter((l) => !l.facet || have[l.facet]);
  });

  /** L6.5: Emitted when the user clicks the visible Table button. */
  readonly tableRequested = output<void>();
}
