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
    <nav class="fixed right-3 top-1/2 z-30 -translate-y-1/2">
      <div class="rounded-lg border border-line/60 bg-surface/90 backdrop-blur-sm shadow-md p-1.5 space-y-0.5">
        @for (id of sections(); track id) {
          <button
            class="flex w-full items-center gap-2 rounded-md px-2.5 py-1.5 text-left text-xs font-medium transition-all duration-150"
            [class.bg-accent]="active() === id"
            [class.text-accent-ink]="active() === id"
            [class.shadow-sm]="active() === id"
            [class.text-ink-muted]="active() !== id"
            [class.hover:bg-surface-2]="active() !== id"
            [class.hover:text-ink]="active() !== id"
            [class.bg-surface]="active() !== id"
            (click)="scrollTo(id)"
            [title]="'Scroll to ' + nameFor(id)"
          >
            <span
              class="inline-block h-1.5 w-1.5 shrink-0 rounded-full transition-all duration-200"
              [class.bg-accent-ink]="active() === id"
              [class.bg-ink-subtle]="active() !== id"
              [class.scale-125]="active() === id"
            ></span>
            <span>{{ nameFor(id) }}</span>
          </button>
        }
      </div>
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
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
