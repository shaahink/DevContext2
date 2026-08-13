import { describe, expect, it } from 'vitest';

import { rangeNote } from './inspector';

/**
 * M1.1 item 2 — the Code pane can now ask for a WHOLE FILE, and the server caps it. A cap the
 * caller cannot see is a cap that lies: showing 2000 lines of a 5231-line file and saying nothing
 * is the same defect class as a trace that cuts six branches and prints only a count. This pins
 * that the note is driven by the SERVER's `truncated` flag, never re-derived from the numbers.
 */
describe('rangeNote (M1.1)', () => {
  it('says "whole file" with the size when nothing was cut', () => {
    expect(rangeNote({ startLine: 1, endLine: 42, totalLines: 42, truncated: false })).toBe('whole file · 42 lines');
  });

  it('states the returned range against the file size when only part came back', () => {
    expect(rangeNote({ startLine: 1, endLine: 2000, totalLines: 5231, truncated: true })).toBe('lines 1–2000 of 5231');
  });

  it('reports the same way for a member window — it states what came back, it does not guess why', () => {
    expect(rangeNote({ startLine: 10, endLine: 40, totalLines: 200, truncated: true })).toBe('lines 10–40 of 200');
  });

  it('says nothing when the server reported no size (error paths return totalLines 0)', () => {
    expect(rangeNote({ startLine: 0, endLine: 0, totalLines: 0, truncated: false })).toBeNull();
  });
});
