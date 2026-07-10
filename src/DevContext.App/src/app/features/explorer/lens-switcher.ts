import { Component, model, output } from '@angular/core';

export type LensId = 'service' | 'layer' | 'feature' | 'flow';

interface LensDef {
  readonly id: LensId;
  readonly label: string;
  readonly hint: string;
  /** True when the engine has not yet plumbed the data for this lens (D9/M2.4 follow-up). */
  readonly unavailable?: boolean;
}

const LENSES: readonly LensDef[] = [
  { id: 'service', label: 'Service', hint: 'Project topology — runtime grouping (current system view)' },
  { id: 'layer', label: 'Layer', hint: 'Nodes colored by architectural layer (API/App/Domain/Infra)' },
  { id: 'feature', label: 'Feature', hint: 'Nodes colored by feature area (namespace-derived)' },
  { id: 'flow', label: 'Flow', hint: 'Trace tree or graph — current flow view' },
];

/**
 * Lens Switcher (M7.2) — replaces the hardcoded altitude selector with named lenses.
 * Each lens is a named projection of the graph; Service=topology, Flow=trace, and
 * Layer/Feature are structural slots for the D9 facets (engine data not yet in proto).
 * The `lens` model() is lifted to the page so it can be mirrored into URL state and
 * each page can set its own default (Explore→Flow, Atlas→Service).
 *
 * L6.5: Added a visible "Table" toolbar button and global Shift+E shortcut.
 */
@Component({
  selector: 'app-lens-switcher',
  template: `
    @for (lens of lenses; track lens.id) {
      <button
        type="button"
        class="chip shrink-0"
        [class.active]="lensModel() === lens.id"
        [class.opacity-50]="lens.unavailable"
        [title]="lens.hint"
        (click)="select(lens)"
      >
        {{ lens.label }}
      </button>
    }
    <span class="mx-1 h-4 w-px bg-line"></span>
    <button
      type="button"
      class="chip shrink-0 inline-flex items-center gap-1"
      title="Open entry table (Shift+E)"
      (click)="tableRequested.emit()"
    >
      <span class="text-2xs opacity-60">&#9776;</span> Table
    </button>
  `,
  host: { class: 'contents' },
})
export class LensSwitcher {
  readonly lenses = LENSES;
  readonly lensModel = model<LensId>('flow');

  /** L6.5: Emitted when the user clicks the visible Table button. */
  readonly tableRequested = output<void>();

  protected select(lens: LensDef): void {
    if (lens.unavailable) return;
    this.lensModel.set(lens.id);
  }
}
