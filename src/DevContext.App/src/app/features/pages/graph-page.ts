import { Component } from '@angular/core';

import { SectionGraph } from '../narrative/section-graph';

@Component({
  selector: 'app-graph-page',
  imports: [SectionGraph],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      <app-section-graph />
    </div>
  `,
})
export class GraphPage {}
