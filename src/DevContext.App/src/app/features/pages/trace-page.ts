import { Component, effect, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
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
export class TracePage {
  private readonly session = inject(SessionStore);
  private readonly traceStore = inject(TraceStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  constructor() {
    this.route.queryParams.subscribe((params) => {
      const focus = params['focus'];
      if (focus && this.session.ready()) {
        const handle = this.session.handle();
        if (handle) {
          void this.traceStore.trace(handle, focus);
        }
      }
    });

    effect(() => {
      const focus = this.traceStore.focus();
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams: focus ? { focus } : {},
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    });
  }
}
