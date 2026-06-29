import { Component } from '@angular/core';

@Component({
  selector: 'app-toolbar',
  template: '<ng-content />',
  host: { class: 'flex items-center gap-2 border-b border-line bg-surface px-3 py-1.5' },
})
export class Toolbar {}
