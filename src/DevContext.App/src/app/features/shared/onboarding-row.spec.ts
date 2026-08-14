import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { describe, expect, it } from 'vitest';

import { AtlasStore } from '../../state/atlas.store';
import { SessionStore } from '../../state/session.store';
import { OnboardingRow } from './onboarding-row';

/**
 * N3.2 (STUDIO-MCP §5 N3, decision 3) — "Point your agent here" is the app's one promise about
 * handing a repo to an agent, and it used to route to the MCP page: a different question (how do
 * I CONNECT an agent to this server) with no artifact at the end of it. Decision 3 gave the
 * promise a concrete answer — compose in the Studio, Save writes .devcontext/packs/<slug>.md,
 * copy the line into CLAUDE.md — so the tile has to land there. A route is exactly the kind of
 * claim that rots silently, which is why it is pinned rather than eyeballed.
 */
describe('OnboardingRow', () => {
  function render() {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AtlasStore, useValue: { flows: signal([]) } },
        { provide: SessionStore, useValue: { entryGroups: signal([]) } },
      ],
    });
    const fixture = TestBed.createComponent(OnboardingRow);
    fixture.detectChanges();
    return fixture;
  }

  it('routes "Point your agent here" through the Studio hand-off (N3.2)', () => {
    const el: HTMLElement = render().nativeElement;
    const tile = el.querySelector<HTMLAnchorElement>('[data-testid="point-agent-here"]');

    expect(tile).not.toBeNull();
    expect(tile!.textContent?.trim()).toContain('Point your agent here');
    expect(tile!.getAttribute('href')).toBe('/context');
  });

  it('still offers the atlas as the other start-here move', () => {
    const el: HTMLElement = render().nativeElement;
    const hrefs = [...el.querySelectorAll('a')].map((a) => a.getAttribute('href'));

    expect(hrefs).toContain('/atlas');
    // The MCP page is reached as MCP SETUP now, not as the agent hand-off.
    expect(hrefs).not.toContain('/mcp');
  });
});
