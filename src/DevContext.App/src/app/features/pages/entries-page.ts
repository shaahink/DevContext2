import { Component } from '@angular/core';

import { SectionEntries } from '../narrative/section-entries';

@Component({
  selector: 'app-entries-page',
  imports: [SectionEntries],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      <app-section-entries />
    </div>
  `,
})
export class EntriesPage {}
