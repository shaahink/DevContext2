import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';

import { DevContextApi } from '../data-access/devcontext-api';
import { TrailStore, type TrailStep } from './trail.store';
import { WorkspaceStore } from './workspace.store';

function step(id: string): Omit<TrailStep, 'ts'> {
  return { kind: 'entry', id, title: id, focus: id };
}

function setup() {
  TestBed.configureTestingModule({
    providers: [{ provide: DevContextApi, useValue: { closeSession: () => Promise.resolve({}) } }],
  });
  const workspace = TestBed.inject(WorkspaceStore);
  const trail = TestBed.inject(TrailStore);
  workspace.createTab('repo', 'repo');
  return { workspace, trail };
}

// WorkspaceStore persists tabs to localStorage in an effect and restore()s them in its
// constructor, so a store built by one test can inherit the tabs of the last one. At six tabs
// createTab() refuses and returns null (M1.2 — it used to return the ACTIVE id, which silently
// made two "tabs" the same tab), so an inherited-tab test now fails on a null deref instead of
// on a mystery. Whether the effect has flushed before the next test constructs its store is a
// scheduling detail that differs between machines — this failed on CI while passing locally.
// Clearing storage per test removes the ordering dependency rather than betting on the timing.
beforeEach(() => localStorage.clear());

describe('TrailStore', () => {
  it('pushes steps and walks them with undo/redo', () => {
    const { trail } = setup();
    trail.push(step('a'));
    trail.push(step('b'));
    trail.push(step('c'));
    expect(trail.steps().map((s) => s.id)).toEqual(['a', 'b', 'c']);
    expect(trail.cursor()).toBe(2);

    expect(trail.undo()?.id).toBe('b');
    expect(trail.cursor()).toBe(1);
    expect(trail.undo()?.id).toBe('a');
    expect(trail.canUndo()).toBe(false);
    expect(trail.undo()).toBeNull(); // already at the root

    expect(trail.redo()?.id).toBe('b');
    expect(trail.redo()?.id).toBe('c');
    expect(trail.canRedo()).toBe(false);
    expect(trail.redo()).toBeNull(); // already at the tip
  });

  it('truncates the forward branch when pushing after an undo (browser-history semantics)', () => {
    const { trail } = setup();
    trail.push(step('a'));
    trail.push(step('b'));
    trail.push(step('c'));
    trail.undo(); // cursor -> b

    trail.push(step('d'));
    expect(trail.steps().map((s) => s.id)).toEqual(['a', 'b', 'd']);
    expect(trail.canRedo()).toBe(false);
  });

  it('collapses a push that repeats the current step', () => {
    const { trail } = setup();
    trail.push(step('a'));
    trail.push(step('a'));
    expect(trail.steps().length).toBe(1);
  });

  it('jumpTo moves the cursor to an absolute index (breadcrumb click)', () => {
    const { trail } = setup();
    trail.push(step('a'));
    trail.push(step('b'));
    trail.push(step('c'));
    expect(trail.jumpTo(0)?.id).toBe('a');
    expect(trail.cursor()).toBe(0);
    expect(trail.jumpTo(0)).toBeNull(); // no-op: already there
  });

  it('toggles pins independently of cursor position', () => {
    const { trail } = setup();
    trail.push(step('a'));
    const a = trail.current()!;
    expect(trail.isPinned(a)).toBe(false);
    trail.togglePin(a);
    expect(trail.isPinned(a)).toBe(true);
    expect(trail.pinCount()).toBe(1);
    trail.togglePin(a);
    expect(trail.isPinned(a)).toBe(false);
  });

  it('self-GCs a tab slice when the tab closes', () => {
    const { workspace, trail } = setup();
    const tabId = workspace.activeId()!;
    trail.push(step('a'));
    expect(trail.hasTrail()).toBe(true);

    workspace.closeTab(tabId);
    workspace.createTab('repo2', 'repo2');
    expect(trail.hasTrail()).toBe(false);
  });

  it('keeps separate slices per tab', () => {
    const { workspace, trail } = setup();
    const tabA = workspace.activeId()!;
    trail.push(step('a1'));

    const tabB = workspace.createTab('repoB', 'repoB');
    trail.push(step('b1'));
    expect(trail.steps().map((s) => s.id)).toEqual(['b1']);

    workspace.setActive(tabA);
    expect(trail.steps().map((s) => s.id)).toEqual(['a1']);
    expect(tabA).not.toBe(tabB);
  });
});
