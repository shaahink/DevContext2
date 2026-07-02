import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

import { SectionExport } from '../narrative/section-export';
import { SessionStore } from '../../state/session.store';

@Component({
  selector: 'app-export-page',
  imports: [SectionExport],
  template: `
    <div class="mx-auto max-w-4xl px-5 pb-10 pt-6">
      @if (session.ready()) {
        <app-section-export [open]="true" (dismissed)="onDismiss()" />
      } @else {
        <p class="py-8 text-center text-xs text-ink-subtle">Analyze a repo to export LLM context.</p>
      }
    </div>
  `,
})
export class ExportPage {
  protected readonly session = inject(SessionStore);
  private readonly router = inject(Router);

  protected onDismiss(): void {
    void this.router.navigateByUrl('/overview');
  }
}
