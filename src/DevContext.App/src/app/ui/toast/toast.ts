import { Component, input, signal } from '@angular/core';

export interface ToastMessage {
  readonly id: string;
  readonly text: string;
  readonly kind: 'info' | 'success' | 'error';
}

@Component({
  selector: 'app-toast',
  template: `
    <div class="pointer-events-none fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      @for (msg of messages(); track msg.id) {
        <div
          class="pointer-events-auto flex items-center gap-2 rounded-md px-3 py-2 text-sm shadow-lg"
          [class.bg-accent]="msg.kind === 'info'"
          [class.text-accent-ink]="msg.kind === 'info'"
          [class.bg-success]="msg.kind === 'success'"
          [class.text-accent-ink]="msg.kind === 'success'"
          [class.bg-danger]="msg.kind === 'error'"
          [class.text-accent-ink]="msg.kind === 'error'"
        >
          {{ msg.text }}
        </div>
      }
    </div>
  `,
  host: { class: 'contents' },
})
export class Toast {
  readonly messages = input<readonly ToastMessage[]>([]);
}

import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _messages = signal<readonly ToastMessage[]>([]);
  readonly messages = this._messages.asReadonly();

  show(text: string, kind: 'info' | 'success' | 'error' = 'info'): void {
    const id = crypto.randomUUID();
    this._messages.update((msgs) => [...msgs, { id, text, kind }]);
    setTimeout(() => {
      this._messages.update((msgs) => msgs.filter((m) => m.id !== id));
    }, 3500);
  }
}
