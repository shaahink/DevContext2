import { Component, input } from '@angular/core';

const SECTION_NAMES: Record<string, string> = {
  landing: 'Home',
  identity: 'Identity',
  entries: 'Entries',
  trace: 'Trace',
  architecture: 'Arch',
  graph: 'Graph',
  stats: 'Stats',
  export: 'Export',
  settings: 'Settings',
};

@Component({
  selector: 'app-scroll-spy',
  template: `
    <nav class="fixed right-2 top-1/2 z-30 -translate-y-1/2 space-y-1.5">
      @for (id of sections(); track id) {
        <button
          class="group flex w-full items-center justify-end gap-1.5 rounded py-1 pl-2 pr-1.5 transition-colors hover:bg-surface/50"
          (click)="scrollTo(id)"
          [title]="nameFor(id)"
        >
          <span
            class="inline text-2xs font-medium transition-opacity duration-200"
            [class.text-accent]="active() === id"
            [class.text-ink-subtle]="active() !== id"
          >{{ nameFor(id) }}</span>
          <span
            class="inline-block h-2.5 w-2.5 rounded-full border transition-all duration-200"
            [class.bg-accent]="active() === id"
            [class.border-accent]="active() === id"
            [class.border-ink-subtle]="active() !== id"
            [class.bg-transparent]="active() !== id"
          ></span>
        </button>
      }
    </nav>
  `,
  host: { class: 'contents' },
})
export class ScrollSpy {
  readonly sections = input.required<readonly string[]>();
  readonly active = input.required<string>();

  nameFor(id: string): string {
    return SECTION_NAMES[id] ?? id;
  }

  scrollTo(id: string): void {
    const el = document.getElementById(id);
    if (el) el.scrollIntoView({ behavior: 'smooth' });
  }
}
