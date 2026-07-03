import { Component, computed, input } from '@angular/core';

import { KIND_ICONS } from '../../models/view-models';
import { Icon } from '../icon/icon';

/** Icon for an entry-point kind (HttpEndpoint, MessageConsumer, ...), looked up from the
 * shared `KIND_ICONS` registry so entry deck, palette, and omnibox agree on one glyph
 * per kind instead of each picking their own. */
@Component({
  selector: 'app-kind-icon',
  imports: [Icon],
  template: '<app-icon [name]="iconName()" [size]="size()" />',
  host: { class: 'inline-flex shrink-0' },
})
export class KindIcon {
  readonly kind = input.required<string>();
  readonly size = input(12);

  protected readonly iconName = computed(() => KIND_ICONS[this.kind()] ?? 'code');
}
