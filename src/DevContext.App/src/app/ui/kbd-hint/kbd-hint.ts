import { Component } from '@angular/core';

@Component({
  selector: 'app-kbd-hint',
  template: '<ng-content />',
  host: {
    class:
      'inline-flex items-center rounded border border-line bg-surface-2 px-1.5 py-0.5 font-mono text-[11px] text-ink-muted',
  },
})
export class KbdHint {}
