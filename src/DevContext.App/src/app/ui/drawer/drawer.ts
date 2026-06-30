import { Dialog } from '@angular/cdk/dialog';
import { Component, effect, inject, input, TemplateRef, viewChild } from '@angular/core';

@Component({
  selector: 'app-drawer',
  template: `
    <ng-template #drawerTpl>
      <div class="fixed right-0 top-0 z-40 h-full w-80 border-l border-line bg-surface shadow-xl overflow-y-auto">
        <ng-content />
      </div>
    </ng-template>
  `,
  host: { class: 'contents' },
})
export class Drawer {
  private readonly dialog = inject(Dialog);
  readonly open = input(false);
  private readonly drawerTpl = viewChild<TemplateRef<unknown>>('drawerTpl');
  private ref: { close(): void } | null = null;

  constructor() {
    effect(() => {
      const isOpen = this.open();
      if (isOpen && !this.ref) {
        const tpl = this.drawerTpl();
        if (tpl) {
          this.ref = this.dialog.open(tpl, { hasBackdrop: true, panelClass: '!m-0 !p-0' });
        }
      } else if (!isOpen && this.ref) {
        this.ref.close();
        this.ref = null;
      }
    });
  }
}
