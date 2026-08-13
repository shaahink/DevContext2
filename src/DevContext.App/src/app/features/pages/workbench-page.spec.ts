import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';

import { AtlasStore } from '../../state/atlas.store';
import { NodePeekStore } from '../../state/node-peek.store';
import { PrefsStore } from '../../state/prefs.store';
import { SessionStore } from '../../state/session.store';
import { TraceStore } from '../../state/trace.store';
import { TrailStore, type TrailStep } from '../../state/trail.store';
import { ToastService } from '../../ui/toast/toast';
import { WorkbenchPage } from './workbench-page';

/**
 * N1.2 (audit §3.A) — the `p` shortcut. The component is constructed WITHOUT rendering its
 * template: the key handler acts on the trail store alone, and standing up the whole
 * Deck│Stage│Inspector tree would test three other components' markup instead of this one.
 */
interface WorkbenchTestSurface {
  onGlobalKey(event: KeyboardEvent): void;
}

function step(id: string, title: string): TrailStep {
  return { kind: 'entry', id, title, focus: `${id}.Focus`, ts: 1 };
}

describe('WorkbenchPage — pin shortcut', () => {
  let current: ReturnType<typeof signal<TrailStep | null>>;
  let pinned: ReturnType<typeof signal<readonly TrailStep[]>>;
  let togglePin: Mock;

  beforeEach(() => {
    current = signal<TrailStep | null>(null);
    pinned = signal<readonly TrailStep[]>([]);
    // The real store's toggle, minus the per-tab slice machinery (that is trail.store.spec's job).
    togglePin = vi.fn((s: TrailStep) => {
      pinned.update((ps) => (ps.some((p) => p.id === s.id) ? ps.filter((p) => p.id !== s.id) : [...ps, s]));
    });

    TestBed.configureTestingModule({
      providers: [
        {
          provide: SessionStore,
          useValue: { handle: signal('h1'), ready: signal(true), mapResponse: signal(null), entryGroups: signal([]) },
        },
        { provide: TraceStore, useValue: { focus: signal(''), tree: signal(null), selectedNode: signal(null) } },
        {
          provide: TrailStore,
          useValue: {
            current,
            pinCount: () => pinned().length,
            isPinned: (s: TrailStep) => pinned().some((p) => p.id === s.id),
            togglePin,
            undo: () => null,
            redo: () => null,
            jumpTo: () => null,
          },
        },
        { provide: AtlasStore, useValue: {} },
        { provide: NodePeekStore, useValue: { dismiss: vi.fn(), nodeId: signal(null) } },
        { provide: PrefsStore, useValue: { dockLevel: () => 2, setDockLevel: vi.fn() } },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: { get: () => null } } } },
        { provide: Router, useValue: { navigate: vi.fn().mockResolvedValue(true), navigateByUrl: vi.fn() } },
      ],
    });
  });

  function createPage(): WorkbenchTestSurface {
    return TestBed.runInInjectionContext(() => new WorkbenchPage()) as unknown as WorkbenchTestSurface;
  }

  function pressP(page: WorkbenchTestSurface): void {
    page.onGlobalKey(new KeyboardEvent('keydown', { key: 'p' }));
  }

  it('pins the current step and says where it goes (N1.2)', () => {
    current.set(step('node-checkout', 'POST /checkout'));
    const page = createPage();

    pressP(page);

    expect(togglePin).toHaveBeenCalledOnce();
    expect(TestBed.inject(ToastService).messages().map((m) => [m.text, m.kind])).toEqual([
      ["Pinned POST /checkout — 1 pinned, seeding Context Studio's pack", 'success'],
    ]);
  });

  it('reports the unpin too, with the remaining count (N1.2)', () => {
    const s = step('node-checkout', 'POST /checkout');
    current.set(s);
    pinned.set([s]);
    const page = createPage();

    pressP(page);

    expect(TestBed.inject(ToastService).messages().map((m) => [m.text, m.kind])).toEqual([
      ['Unpinned POST /checkout — 0 pinned', 'info'],
    ]);
  });

  it('says why nothing happened when there is no current step (N1.2)', () => {
    const page = createPage();

    pressP(page);

    expect(togglePin).not.toHaveBeenCalled();
    expect(TestBed.inject(ToastService).messages().map((m) => m.text)).toEqual([
      'Nothing to pin — pick an entry or a node first',
    ]);
  });

  it('leaves `p` alone while typing (N1.2 — regression guard)', () => {
    current.set(step('node-checkout', 'POST /checkout'));
    const page = createPage();
    const input = document.createElement('input');

    const event = new KeyboardEvent('keydown', { key: 'p' });
    Object.defineProperty(event, 'target', { value: input });
    page.onGlobalKey(event);

    expect(togglePin).not.toHaveBeenCalled();
    expect(TestBed.inject(ToastService).messages()).toEqual([]);
  });
});
