import { Component } from '@angular/core';

import { SectionTrace } from '../narrative/section-trace';
import { SectionLens } from '../narrative/section-lens';

@Component({
  selector: 'app-trace-page',
  imports: [SectionTrace, SectionLens],
  template: `
    <div class="mx-auto max-w-5xl px-5 pb-10 pt-6 space-y-6">
      <app-section-trace />
      <app-section-lens />
    </div>
  `,
})
export class TracePage {}
