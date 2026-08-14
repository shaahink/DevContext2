import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';

import type { PackProposal } from '../models/context-card';
import { StudioHandoffStore, STUDIO_ROUTE } from './studio-handoff.store';

function proposal(source = 'the node “OrderService”'): PackProposal {
  return {
    seeds: [{ type: 'usage', title: 'Who uses OrderService', entryIds: ['Type:Acme.OrderService'], estimatedLines: 15 }],
    source,
  };
}

describe('StudioHandoffStore (N3.1)', () => {
  let navigateByUrl: Mock;

  beforeEach(() => {
    navigateByUrl = vi.fn().mockResolvedValue(true);
    TestBed.configureTestingModule({
      providers: [{ provide: Router, useValue: { navigateByUrl } }],
    });
  });

  it('take() reads ONCE — a second walk into the room must not re-seed the same cards', () => {
    const store = TestBed.inject(StudioHandoffStore);
    store.send(proposal());

    expect(store.take()?.seeds).toHaveLength(1);
    expect(store.take()).toBeNull();
    expect(store.pending()).toBeNull();
  });

  it('open() leaves the proposal and navigates to Studio', async () => {
    const store = TestBed.inject(StudioHandoffStore);

    await expect(store.open(proposal())).resolves.toBe(true);

    expect(navigateByUrl).toHaveBeenCalledWith(STUDIO_ROUTE);
    expect(store.pending()?.source).toBe('the node “OrderService”');
  });

  it('reports a refused navigation instead of swallowing it — the sender toasts the truth', async () => {
    navigateByUrl.mockResolvedValue(false);
    const store = TestBed.inject(StudioHandoffStore);

    await expect(store.open(proposal())).resolves.toBe(false);
  });
});
