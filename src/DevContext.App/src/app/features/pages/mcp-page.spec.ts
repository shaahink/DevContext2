import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';

import { DEVCONTEXT_CLIENT } from '../../core/grpc/client';
import { DevContextApi } from '../../data-access/devcontext-api';
import { McpPage } from './mcp-page';

/**
 * N0.2/N0.3 (audit §3.F.9-14, §3.F.16) — the MCP page had no spec file at all, which is how a
 * status read that MUTATED the thing it measured, a copy button that always marked the third
 * card, a "Total" that counted hidden rows, and a session age that lied after a cache hit all
 * shipped together. These pin the honest behaviour, not the pixels.
 */
describe('McpPage', () => {
  let getMcpStatus: Mock;
  let listSessions: Mock;
  let startMcp: Mock;
  let stopMcp: Mock;
  let observeToolCalls: Mock;

  function session(overrides: Record<string, unknown> = {}) {
    return {
      handle: 'handle-abcdef123456',
      repo: 'C:/Code/eshop',
      ageSeconds: 12n,
      calls: 3,
      nodes: 900,
      edges: 1800,
      entries: 24,
      fromCache: false,
      analyzedAt: '',
      ...overrides,
    };
  }

  /** One event as the server streams it — note the WIRE timestamp. */
  function evt(overrides: Record<string, unknown> = {}) {
    return {
      tool: 'GetMap',
      sessionHandle: 'handle-abcdef123456',
      sessionRepo: 'C:/Code/eshop',
      bytes: 100n,
      estTokens: 500n,
      elapsedMs: 12n,
      timestampUtcMs: BigInt(Date.UTC(2026, 7, 13, 9, 30, 0)),
      origin: 'agent',
      ...overrides,
    };
  }

  beforeEach(() => {
    getMcpStatus = vi.fn().mockResolvedValue({ telemetryStreaming: false, observerCount: 0 });
    listSessions = vi.fn().mockResolvedValue({ sessions: [] });
    startMcp = vi.fn().mockResolvedValue({ running: true });
    stopMcp = vi.fn().mockResolvedValue({ stopped: true });
    observeToolCalls = vi.fn().mockReturnValue({
      async *[Symbol.asyncIterator]() { /* silent stream */ },
    });
    TestBed.configureTestingModule({
      providers: [
        { provide: DevContextApi, useValue: { getMcpStatus } },
        { provide: DEVCONTEXT_CLIENT, useValue: { listSessions, startMcp, stopMcp, observeToolCalls } },
      ],
    });
  });

  async function createPage() {
    const fixture = TestBed.createComponent(McpPage);
    fixture.detectChanges();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement };
  }

  function text(el: HTMLElement, testid: string): string {
    return el.querySelector(`[data-testid="${testid}"]`)?.textContent?.trim() ?? '';
  }

  it('reads status WITHOUT starting anything (§3.F.9)', async () => {
    getMcpStatus.mockResolvedValue({ telemetryStreaming: true, observerCount: 2 });
    const { el } = await createPage();

    expect(getMcpStatus).toHaveBeenCalledTimes(1);
    expect(startMcp).not.toHaveBeenCalled(); // the whole defect: reading used to start it
    expect(text(el, 'mcp-status-label')).toBe('Tool-call telemetry streaming');
    expect(text(el, 'mcp-status-text')).toContain('2 watcher(s) attached');
  });

  it('an unreachable server is not rendered as "off" (§3.F.14)', async () => {
    getMcpStatus.mockResolvedValue(null);
    const { el } = await createPage();

    expect(text(el, 'mcp-status-error')).toContain('Could not reach');
    expect(text(el, 'mcp-status-label')).toBe('Tool-call telemetry off');
  });

  it('Copy marks the card that was clicked (§3.F.11)', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="copy-snippet-Cursor"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'copy-snippet-Cursor')).toBe('Copied!');
    expect(text(el, 'copy-snippet-VS Code')).toBe('Copy');
    expect(text(el, 'copy-snippet-Claude Code')).toBe('Copy');
  });

  it('host snippets do not promise a command that will not resolve (§3.F.10)', async () => {
    const { el } = await createPage();
    const snippets = [...el.querySelectorAll('pre')].map((p) => p.textContent ?? '');

    expect(snippets).toHaveLength(3);
    for (const s of snippets) {
      expect(s).not.toContain('"command": "devcontext-mcp"'); // bare command is not on PATH
      expect(s).toContain('devcontext-mcp.exe');
    }
    expect(text(el, 'mcp-setup-note')).not.toContain('ships with the desktop installer');
  });

  it('the feed total counts the rows on screen, and rows carry the WIRE time (§3.F.12)', async () => {
    getMcpStatus.mockResolvedValue({ telemetryStreaming: true, observerCount: 1 });
    observeToolCalls.mockReturnValue({
      async *[Symbol.asyncIterator]() {
        yield evt({ estTokens: 500n, origin: 'agent' });
        yield evt({ estTokens: 9000n, origin: 'ui' });
      },
    });
    const { fixture, el } = await createPage();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    // "agents only" is the default filter — the 9000-token UI row must not be counted
    expect(text(el, 'feed-total')).toBe('Shown: 500 tok');

    const wireTime = new Date(Date.UTC(2026, 7, 13, 9, 30, 0)).toLocaleTimeString();
    expect(el.textContent).toContain(wireTime);

    // flipping the filter changes the total, because it describes what is visible
    (el.querySelector('[data-testid="feed-origin-filter"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(text(el, 'feed-total')).toBe('Shown: 9500 tok');
  });

  it('sessions render the analysis age, not just the session age (§3.F.13)', async () => {
    const analyzedAt = new Date(Date.now() - 7200_000).toISOString(); // 2h ago
    listSessions.mockResolvedValue({ sessions: [session({ fromCache: true, analyzedAt })] });
    const { el } = await createPage();

    const cells = [...el.querySelectorAll('tbody td')].map((c) => c.textContent?.trim() ?? '');
    expect(cells).toContain('1800'); // edges — mapped but never rendered before
    expect(cells).toContain('24');   // entries — same
    expect(text(el, 'session-analyzed')).toBe('2h (cached)');
    expect(el.querySelector('[data-testid="session-analyzed"]')?.getAttribute('title'))
      .toContain('rehydrated from the snapshot cache');
  });

  it('a failed session poll says so instead of showing a frozen table (§3.F.14)', async () => {
    listSessions.mockRejectedValue(new Error('server gone'));
    const { el } = await createPage();

    expect(text(el, 'sessions-error')).toContain('server gone');
  });

  /**
   * N0.3 (§3.F.16) — the two remaining unreferenced data-testids on this page. Both are the
   * §3.F.7 class on the MCP side: a control whose confirmation must follow the OUTCOME, and a
   * button whose whole promise is that it hands the FULL handle on, not the truncated one shown.
   */
  it('copying a handle copies the full one and confirms only after the write resolves', async () => {
    let resolveWrite!: () => void;
    const writeText = vi.fn().mockReturnValue(new Promise<void>((r) => { resolveWrite = r; }));
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    listSessions.mockResolvedValue({ sessions: [session()] });
    const { fixture, el } = await createPage();

    expect(text(el, 'session-handle-copy')).toBe('handle-a…'); // truncated for width

    (el.querySelector('[data-testid="session-handle-copy"]') as HTMLButtonElement).click();
    await Promise.resolve();
    fixture.detectChanges();
    expect(text(el, 'session-handle-copy')).toBe('handle-a…'); // still pending — no premature "Copied!"

    resolveWrite();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(writeText).toHaveBeenCalledWith('handle-abcdef123456'); // the FULL handle, not the shown one
    expect(text(el, 'session-handle-copy')).toBe('Copied!');
  });

  it('a rejected clipboard write never claims the handle was copied', async () => {
    const writeText = vi.fn().mockRejectedValue(new Error('denied'));
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    listSessions.mockResolvedValue({ sessions: [session()] });
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="session-handle-copy"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'session-handle-copy')).not.toBe('Copied!');
  });

  it('the toggle is the ONLY thing that mutates, and a refused start stays off (§3.F.9)', async () => {
    const { fixture, el } = await createPage();
    expect(startMcp).not.toHaveBeenCalled();

    startMcp.mockRejectedValueOnce(new Error('refused'));
    (el.querySelector('[data-testid="mcp-toggle"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(startMcp).toHaveBeenCalledTimes(1);
    expect(text(el, 'mcp-status-label')).toBe('Tool-call telemetry off'); // a refusal is not "streaming"
    expect(text(el, 'mcp-toggle')).toBe('Start');

    (el.querySelector('[data-testid="mcp-toggle"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'mcp-status-label')).toBe('Tool-call telemetry streaming');
    expect(text(el, 'mcp-toggle')).toBe('Stop');
  });

  it('"use" prefills Try-a-Tool with the live handle and enables Run', async () => {
    listSessions.mockResolvedValue({ sessions: [session()] });
    const { fixture, el } = await createPage();

    const run = [...el.querySelectorAll('button')].find((b) => b.textContent?.trim() === 'Run') as HTMLButtonElement;
    expect(run.disabled).toBe(true); // nothing to run against yet

    (el.querySelector('[data-testid="session-use"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect((el.querySelector('#try-handle-input') as HTMLInputElement).value).toBe('handle-abcdef123456');
    expect(run.disabled).toBe(false);
  });
});
