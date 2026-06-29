import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Icon } from './icon';

@Component({
  imports: [Icon],
  template: '<app-icon name="play" />',
})
class HostComponent {}

describe('Icon', () => {
  it('renders an SVG for a known icon name', async () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const svg = (fixture.nativeElement as HTMLElement).querySelector('svg');
    expect(svg).not.toBeNull();
  });
});
