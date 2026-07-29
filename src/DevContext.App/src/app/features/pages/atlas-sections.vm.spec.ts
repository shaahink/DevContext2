import { describe, expect, it } from 'vitest';

import {
  type AtlasSectionFacts,
  dataStoresWithheld,
  eventWiringWithheld,
  hubRadarWithheld,
  serviceBreakdownWithheld,
  topFlowsWithheld,
} from './atlas-sections.vm';

/**
 * R3 C-2 — Atlas's empty sections on a library either fill or withhold themselves WITH A REASON.
 *
 * The measurement these pin (eval-results/2026-07-29/G7/g71-atlas-sections-before.txt):
 * FluentValidation's Atlas rendered 7 sections and 5 were empty. Two already said why; three did
 * not, and each was a distinct defect — an unactionable instruction, an empty set described as a
 * set, and a "nothing found" over inputs that were empty by construction.
 */

const LIBRARY: AtlasSectionFacts = {
  entries: 0,
  isLibrary: true,
  projectCount: 2,
  serviceCount: 0,
  serviceStyleCount: 0,
  topFlowCount: 0,
  flowStatus: 'done',
};

/** GitVersion's shape: a CLI tool — entries, services, no stores, no multi-flow hub. */
const CLI_TOOL: AtlasSectionFacts = {
  entries: 1,
  isLibrary: false,
  projectCount: 11,
  serviceCount: 4,
  serviceStyleCount: 4,
  topFlowCount: 1,
  flowStatus: 'done',
};

describe('C-2 — a withheld Atlas section states why', () => {
  it('never issues an instruction a reader with no entry points can act on', () => {
    // The Hub radar said "index flows from the Explore page" on a repo with nothing to index.
    const notes = [
      topFlowsWithheld(LIBRARY),
      eventWiringWithheld(LIBRARY),
      dataStoresWithheld(LIBRARY),
      serviceBreakdownWithheld(LIBRARY),
      hubRadarWithheld(LIBRARY),
    ];
    for (const note of notes) {
      expect(note).not.toBeNull();
      expect(note!.text).not.toMatch(/index flows/i);
    }
  });

  it('every withheld note is a real sentence, not a status word', () => {
    const notes = [
      topFlowsWithheld(LIBRARY)!,
      eventWiringWithheld(LIBRARY),
      dataStoresWithheld(LIBRARY),
      serviceBreakdownWithheld(LIBRARY),
      hubRadarWithheld(LIBRARY),
    ];
    for (const note of notes) {
      expect(note.text.length).toBeGreaterThanOrEqual(20);
      expect(note.reason).toMatch(/^(archetype|none-found|not-computed)$/);
    }
  });

  it('a library withholds by ARCHETYPE, not by "nothing found"', () => {
    // The distinction is the checkpoint: these sections have no subject on a library, so calling
    // them "none found" would report a property of the repo that was never measured.
    expect(topFlowsWithheld(LIBRARY)!.reason).toBe('archetype');
    expect(eventWiringWithheld(LIBRARY).reason).toBe('archetype');
    expect(dataStoresWithheld(LIBRARY).reason).toBe('archetype');
    expect(serviceBreakdownWithheld(LIBRARY).reason).toBe('archetype');
    expect(hubRadarWithheld(LIBRARY).reason).toBe('archetype');
  });

  it('data stores name the per-service input rather than claiming a scan found nothing', () => {
    expect(dataStoresWithheld(LIBRARY).text).toMatch(/per service/i);
    expect(dataStoresWithheld(LIBRARY).text).not.toMatch(/no data-store signals detected/i);
    // With services present it IS a scan, and the count of what was scanned is stated.
    const scanned = dataStoresWithheld(CLI_TOOL);
    expect(scanned.reason).toBe('none-found');
    expect(scanned.text).toContain('4 services');
  });

  it('the per-service breakdown never describes an empty set as a set', () => {
    const note = serviceBreakdownWithheld(LIBRARY);
    expect(note.text).not.toMatch(/\b0 services\b/);
    expect(note.text).toContain('2 projects');
  });

  it('a repo WITH entry points keeps the instruction it can act on', () => {
    const idle = hubRadarWithheld({ ...CLI_TOOL, flowStatus: 'idle' });
    expect(idle.reason).toBe('not-computed');
    expect(idle.text).toMatch(/index/i);
    // …and once the index has run, says what it looked for instead.
    const done = hubRadarWithheld(CLI_TOOL);
    expect(done.reason).toBe('none-found');
    expect(done.text).toMatch(/more than one indexed flow/i);
  });

  it('an entry-less repo that is NOT a library gets its own sentence, not the library one', () => {
    const unknown = { ...LIBRARY, isLibrary: false };
    expect(topFlowsWithheld(unknown)!.text).not.toMatch(/a library/i);
    expect(topFlowsWithheld(unknown)!.reason).toBe('archetype');
    expect(dataStoresWithheld(unknown).text).not.toMatch(/a library/i);
  });

  it('top flows fills when there are flows to rank', () => {
    expect(topFlowsWithheld(CLI_TOOL)).toBeNull();
  });

  it('indexing is reported as not-computed, never as a finding', () => {
    const indexing = { ...CLI_TOOL, topFlowCount: 0, flowStatus: 'indexing' as const };
    expect(topFlowsWithheld(indexing)!.reason).toBe('not-computed');
    expect(eventWiringWithheld(indexing).reason).toBe('not-computed');
    expect(hubRadarWithheld(indexing).reason).toBe('not-computed');
  });

  it('singular counts read as singular', () => {
    const one = { ...LIBRARY, projectCount: 1 };
    expect(serviceBreakdownWithheld(one).text).toContain('1 project is');
    const oneService = { ...CLI_TOOL, serviceCount: 1, serviceStyleCount: 1 };
    expect(dataStoresWithheld(oneService).text).toContain('1 service ');
  });
});
