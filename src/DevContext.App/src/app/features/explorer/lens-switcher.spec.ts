import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { LensSwitcher } from './lens-switcher';

/**
 * M1.2 — Layer and Feature colour by facets that are OPTIONAL on the wire
 * (`MapResponse.topology[].layer/.feature`, `ServiceCard.layer/.feature`) and empty for most
 * repos. The chips used to render regardless: clicking Layer repainted every node the same muted
 * grey and Feature drew a legend with nothing in it. A lens the data cannot support is not offered.
 */
describe('LensSwitcher — facet gating (M1.2)', () => {
  function render(facets?: { layer: boolean; feature: boolean }): string[] {
    const fixture = TestBed.createComponent(LensSwitcher);
    if (facets) fixture.componentRef.setInput('facets', facets);
    fixture.detectChanges();
    const host = fixture.nativeElement as HTMLElement;
    return Array.from(host.querySelectorAll<HTMLElement>('button[data-testid^="lens-"]'))
      .map((b) => b.dataset['testid']!.replace('lens-', ''));
  }

  it('offers only the facet-free lenses when the analysis carries neither facet', () => {
    expect(render()).toEqual(['service', 'flow']);
  });

  it('offers Layer only when the analysis carries layers', () => {
    expect(render({ layer: true, feature: false })).toEqual(['service', 'layer', 'flow']);
  });

  it('offers Feature only when the analysis carries features', () => {
    expect(render({ layer: false, feature: true })).toEqual(['service', 'feature', 'flow']);
  });

  it('offers all four when both facets are present', () => {
    expect(render({ layer: true, feature: true })).toEqual(['service', 'layer', 'feature', 'flow']);
  });

  it('defaults to no facets, so a caller that never measured gets the honest subset', () => {
    const fixture = TestBed.createComponent(LensSwitcher);
    expect(fixture.componentInstance.facets()).toEqual({ layer: false, feature: false });
  });
});
