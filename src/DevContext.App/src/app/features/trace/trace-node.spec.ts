import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import type { TraceNodeVm } from '../../models/view-models';
import { TraceNodeComponent } from './trace-node';

/**
 * M1.1 item 4 — the trace node had no spec at all, which is how four honesty annotations the
 * CLI has rendered since Batch E / I1.6 / C5 / T2.1 sat in Core, rode nothing, and left the GUI
 * showing a resolve step as if it were the only binding and a cut subtree as a bare count.
 * These pin the RENDER, at CLI parity (Rendering/TraceRenderer.cs) — not the pixels.
 */
function tn(init: Partial<TraceNodeVm> & { id: string; title: string }): TraceNodeVm {
  return {
    kind: 'Method',
    seam: 'Call',
    depth: 0,
    resolution: 'Semantic',
    truncated: false,
    omitted: 0,
    omittedNames: [],
    multiImplCount: 0,
    diHostCount: 0,
    testOnly: false,
    tags: [],
    children: [],
    ...init,
  };
}

function render(node: TraceNodeVm): string {
  const fixture = TestBed.createComponent(TraceNodeComponent);
  fixture.componentRef.setInput('node', node);
  fixture.detectChanges();
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

describe('TraceNodeComponent — honesty annotations (M1.1)', () => {
  it('names the omitted branches, not just the count', () => {
    const text = render(
      tn({
        id: 'n1',
        title: 'OrdersApi.Create',
        truncated: true,
        omitted: 2,
        omittedNames: ['PricingService.Handle', 'AuditService.Handle'],
      }),
    );
    expect(text).toContain('2 omitted: PricingService.Handle, AuditService.Handle');
    expect(text).not.toContain('…'); // names.length === omitted — nothing left unnamed
  });

  it('ellipsis when the engine capped the name list below the omitted count', () => {
    const text = render(
      tn({ id: 'n1', title: 'A', truncated: true, omitted: 9, omittedNames: ['X', 'Y', 'Z', 'W'] }),
    );
    expect(text).toContain('9 omitted: X, Y, Z, W, …');
  });

  it('falls back to the bare count when the engine named nobody', () => {
    const text = render(tn({ id: 'n1', title: 'A', truncated: true, omitted: 3 }));
    expect(text).toContain('3 omitted');
    expect(text).not.toContain(':');
  });

  it('renders the multi-impl / multi-host / test-only DI annotations', () => {
    const text = render(
      tn({ id: 'n1', title: 'IOrderRepo', seam: 'Resolve', multiImplCount: 3, diHostCount: 2, testOnly: true }),
    );
    expect(text).toContain('3 impls');
    expect(text).toContain('2 hosts');
    expect(text).toContain('test-only');
  });

  it('says nothing when there is nothing to say — a count of 1 impl is not an annotation', () => {
    const text = render(tn({ id: 'n1', title: 'IOrderRepo', multiImplCount: 1, diHostCount: 1 }));
    expect(text).not.toContain('impls');
    expect(text).not.toContain('hosts');
    expect(text).not.toContain('test-only');
  });
});
