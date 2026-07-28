import { describe, expect, it } from 'vitest';

import { commandCounts, indexByTitle, shouldShowCommandSurface, type CommandGroupLike } from './command-surface.vm';

const GITVERSION: readonly CommandGroupLike[] = [
  { project: 'GitVersion.Calculation', entries: [{ title: 'calculate (CalculateCommand)' }] },
  { project: 'GitVersion.Cli', entries: [{ title: 'test (TestCommand)' }] },
  { project: 'GitVersion.Output', entries: [{ title: 'output (OutputCommand)', target: 'OutputCommand.ExecuteAsync' }] },
];

describe('R3 D-D — the CLI landing rule', () => {
  it('lands a CliTool on its commands when the Flow lens has nothing focused', () => {
    expect(shouldShowCommandSurface({ archetype: 'CliTool', lens: 'flow', hasFocus: false, groups: GITVERSION })).toBe(true);
    // The archetype string comes off MapResponse verbatim; the CLI writes it lowercase.
    expect(shouldShowCommandSurface({ archetype: 'clitool', lens: 'flow', hasFocus: false, groups: GITVERSION })).toBe(true);
  });

  it('yields to a focus — a focused command is a trace, which is what D-A decided', () => {
    expect(shouldShowCommandSurface({ archetype: 'CliTool', lens: 'flow', hasFocus: true, groups: GITVERSION })).toBe(false);
  });

  it('leaves the topology lenses alone — they exist to draw the topology', () => {
    for (const lens of ['service', 'layer', 'feature']) {
      expect(shouldShowCommandSurface({ archetype: 'CliTool', lens, hasFocus: false, groups: GITVERSION })).toBe(false);
    }
  });

  it('does not touch any other archetype', () => {
    for (const archetype of ['Microservices', 'App', 'Library', 'Worker']) {
      expect(shouldShowCommandSurface({ archetype, lens: 'flow', hasFocus: false, groups: GITVERSION })).toBe(false);
    }
  });

  it('falls through to the canvas when the engine projected no commands', () => {
    expect(shouldShowCommandSurface({ archetype: 'CliTool', lens: 'flow', hasFocus: false, groups: [] })).toBe(false);
    expect(shouldShowCommandSurface({
      archetype: 'CliTool', lens: 'flow', hasFocus: false,
      groups: [{ project: 'GitVersion.App', entries: [] }],
    })).toBe(false);
  });
});

describe('R3 D-D — command counts', () => {
  it('counts the commands and the ones with no resolved handler', () => {
    expect(commandCounts(GITVERSION)).toEqual({ total: 3, unwired: 2 });
  });

  it('counts nothing without inventing a zero-length group', () => {
    expect(commandCounts([])).toEqual({ total: 0, unwired: 0 });
  });
});

describe('R3 D-D — the focus join', () => {
  it('keeps the first entry for a title, so a click scrubs where the deck would', () => {
    const index = indexByTitle([
      { title: 'calculate (CalculateCommand)', focus: 'CalculateCommand' },
      { title: 'calculate (CalculateCommand)', focus: 'Other' },
    ]);
    expect(index.get('calculate (CalculateCommand)')?.focus).toBe('CalculateCommand');
  });

  it('has no entry for a projected command the entry list never loaded', () => {
    expect(indexByTitle([{ title: 'a' }]).get('b')).toBeUndefined();
  });
});
