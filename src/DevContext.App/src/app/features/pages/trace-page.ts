import { Component } from '@angular/core';

import { SectionTrace } from '../narrative/section-trace';

@Component({
  selector: 'app-trace-page',
  imports: [SectionTrace],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      <app-section-trace />
    </div>
  `,
})
export class TracePage {}
