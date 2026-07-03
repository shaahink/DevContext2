import { Component, input } from '@angular/core';

/** Shimmer placeholder for first-load content (NodeCard, Inspector sections, deck rows).
 * Content-preserving loading (proposal §5.2) uses this only on FIRST load — refreshes dim
 * existing content + `.hairline` instead of swapping in a skeleton. */
@Component({
  selector: 'app-skeleton',
  template: '',
  host: {
    class: 'skeleton block',
    '[style.width]': 'width()',
    '[style.height]': 'height()',
  },
})
export class Skeleton {
  readonly width = input('100%');
  readonly height = input('0.875rem');
}
