import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { DevContextApi } from '../data-access/devcontext-api';
import { SessionStore } from './session.store';
import { WorkspaceStore } from './workspace.store';

describe('WorkspaceStore tab isolation (I10)', () => {
  it('writes analyze() completion into the tab that started it, not whichever tab is active when it resolves', async () => {
    let resolveAnalyze!: (v: unknown) => void;
    const analyze = vi.fn(
      () =>
        new Promise((resolve) => {
          resolveAnalyze = resolve;
        }),
    );

    TestBed.configureTestingModule({
      providers: [
        {
          provide: DevContextApi,
          useValue: {
            analyze,
            getMap: vi.fn().mockResolvedValue({ markdown: '' }),
            listEntryPoints: vi.fn().mockResolvedValue({ entryPoints: [] }),
            getStats: vi.fn().mockResolvedValue({ insights: [] }),
          },
        },
      ],
    });

    const workspace = TestBed.inject(WorkspaceStore);
    const session = TestBed.inject(SessionStore);

    // Start analyze on what becomes tab A (no tabs exist yet, so analyze() creates one).
    const analyzePromise = session.analyze({ path: 'C:\\repoA' });
    const tabA = workspace.activeId();
    expect(tabA).toBeTruthy();
    expect(workspace.tabById(tabA!)?.session.status).toBe('analyzing');

    // The user opens a second tab and switches to it WHILE A is still analyzing — this must not
    // cancel A's in-flight request or redirect its result.
    const tabB = workspace.createTab('C:\\repoB', 'repoB');
    expect(workspace.activeId()).toBe(tabB);
    expect(workspace.tabById(tabA!)?.session.status).toBe('analyzing');

    // A's analyze() resolves now, with the user still parked on B.
    resolveAnalyze({
      ok: true,
      handle: 'handle-A',
      summary: { label: 'repoA', projects: 1, entries: 0, entriesWithTarget: 0 },
    });
    await analyzePromise;

    expect(workspace.tabById(tabA!)?.session.status).toBe('ready');
    expect(workspace.tabById(tabA!)?.session.handle).toBe('handle-A');

    // B must be completely untouched.
    expect(workspace.tabById(tabB)?.session.status).toBe('idle');
    expect(workspace.tabById(tabB)?.session.handle).toBeNull();

    // The user's active tab wasn't yanked back to A by A's completion.
    expect(workspace.activeId()).toBe(tabB);
  });

  it('caps tabs at MAX_TABS and keeps the current active tab when at cap', () => {
    const workspace = TestBed.inject(WorkspaceStore);
    const ids = Array.from({ length: WorkspaceStore.MAX_TABS }, (_, i) => workspace.createTab(`p${i}`, `p${i}`));
    expect(workspace.tabs().length).toBe(WorkspaceStore.MAX_TABS);
    expect(workspace.atCap()).toBe(true);

    const activeBeforeOverflow = workspace.activeId();
    const overflow = workspace.createTab('p-overflow', 'overflow');
    expect(workspace.tabs().length).toBe(WorkspaceStore.MAX_TABS);
    expect(overflow).toBe(activeBeforeOverflow);
    expect(ids).not.toContain(overflow === activeBeforeOverflow ? 'never' : overflow);
  });

  it('activates the neighboring tab when the active tab is closed', () => {
    const workspace = TestBed.inject(WorkspaceStore);
    const a = workspace.createTab('a', 'a');
    const b = workspace.createTab('b', 'b');
    const c = workspace.createTab('c', 'c');
    workspace.setActive(b);

    workspace.closeTab(b);

    expect(workspace.tabs().map((t) => t.id)).toEqual([a, c]);
    expect(workspace.activeId()).toBe(c);
  });
});
