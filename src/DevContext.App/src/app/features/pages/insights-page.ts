import { Component } from '@angular/core';

import { InsightsView } from '../insights/insights-view';

@Component({
  selector: 'app-insights-page',
  imports: [InsightsView],
  template: `<app-insights-view />`,
})
export class InsightsPage {}
