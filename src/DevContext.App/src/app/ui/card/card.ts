import { Component } from '@angular/core';

@Component({
  selector: 'app-card',
  template: '<ng-content />',
  host: { class: 'block rounded-lg border border-line bg-surface p-3' },
})
export class Card {}
