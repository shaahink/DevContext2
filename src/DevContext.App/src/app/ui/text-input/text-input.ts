import { Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  imports: [FormsModule],
  template: `
    <input
      class="min-w-0 flex-1 rounded-md border border-line bg-base px-3 py-1.5 font-mono text-ink outline-none placeholder:text-ink-subtle focus:border-accent"
      [class]="inputClass()"
      [type]="type()"
      [placeholder]="placeholder()"
      [(ngModel)]="value"
    />
  `,
  host: { class: 'block w-full' },
})
export class AppTextInput {
  readonly value = model('');
  readonly placeholder = input('');
  readonly type = input('text');
  readonly inputClass = input('');
}
