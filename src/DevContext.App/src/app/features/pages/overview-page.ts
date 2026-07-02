import { Component, inject } from '@angular/core';

import { SessionStore } from '../../state/session.store';
import { SectionLanding } from '../../features/narrative/section-landing';
import { SectionIdentity } from '../../features/narrative/section-identity';
import { SectionArchitecture } from '../../features/narrative/section-architecture';
import { SectionStats } from '../../features/narrative/section-stats';
import { SectionConsole } from '../../features/narrative/section-console';

@Component({
  selector: 'app-overview-page',
  imports: [SectionLanding, SectionIdentity, SectionArchitecture, SectionStats, SectionConsole],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      @if (!session.ready()) {
        <app-section-landing />
      }
      @if (session.busy()) {
        <app-section-console />
      }
      @if (session.ready()) {
        <app-section-console />
        <app-section-identity />
        <app-section-architecture />
        <app-section-stats />
      }
    </div>
  `,
})
export class OverviewPage {
  protected readonly session = inject(SessionStore);
}
