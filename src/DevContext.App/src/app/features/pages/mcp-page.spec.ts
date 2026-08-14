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
  let mcpHandshake: Mock;
  let observeToolCalls: Mock;
  let writeMcpConfig: Mock;
  let listMcpTools: Mock;

  /**
   * N4.1 — the status the server MEASURED. The old page's whole status was one boolean that a
   * mutating read had just made true; these fields are a disk probe, a subscriber count and
   * real agent traffic.
   */
  function status(overrides: Record<string, unknown> = {}) {
    return {
      observerCount: 0,
      binaryFound: true,
      binaryPath: 'C:/app/resources/server/devcontext-mcp.exe',
      binarySource: 'bundle',
      lastAgentCallAtUtcMs: 0,
      lastAgentTool: '',
      agentCallCount: 0,
      // N4.2 — the setup cards arrive WITH the status, composed server-side around the path the
      // probe resolved. The page no longer owns a snippet template, so a stub that omitted these
      // renders no cards at all — which is the point: there is one composer, and it is not here.
      hosts: hostCards('C:/app/resources/server/devcontext-mcp.exe'),
      ...overrides,
    };
  }

  function hostCards(command: string) {
    return [
      { id: 'claude', label: 'Claude Code', relativePath: '.mcp.json', snippet: snippetFor('mcpServers', command) },
      { id: 'cursor', label: 'Cursor', relativePath: '.cursor/mcp.json', snippet: snippetFor('mcpServers', command) },
      { id: 'vscode', label: 'VS Code', relativePath: '.vscode/mcp.json', snippet: snippetFor('servers', command) },
    ];
  }

  function snippetFor(key: string, command: string) {
    return JSON.stringify({ [key]: { devcontext: { command, args: [] } } }, null, 2);
  }

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

  /**
   * N4.3 — the catalog as the server read it off a live tools/list. Shaped like the real thing:
   * a curated menu (T1.2), the unlisted specialists that menu leaves out, and the folded names.
   */
  function catalog(overrides: Record<string, unknown> = {}) {
    return {
      ok: true,
      error: '',
      command: 'C:/app/resources/server/devcontext-mcp.exe',
      elapsedMs: 900,
      tools: [
        {
          name: 'trace', description: 'Follow one entry point through the code that serves it.',
          specialist: false, why: '',
          parameters: [
            { name: 'handle', type: 'string', required: true, description: 'Session handle from analyze.' },
            { name: 'focus', type: 'string', required: true, description: 'Entry point, symbol or node id.' },
            { name: 'depth', type: 'integer', required: false, description: 'How far to follow.' },
          ],
        },
        {
          name: 'read_source', description: 'Read the source of one symbol.',
          specialist: false, why: '', parameters: [],
        },
        {
          name: 'get_context', description: 'Compose an LLM-sized context pack.',
          specialist: false, why: '', parameters: [],
        },
      ],
      specialists: [
        { name: 'tests_for', description: '', specialist: true, why: 'Which tests cover a symbol.', parameters: [] },
        { name: 'node', description: '', specialist: true, why: 'One node by id.', parameters: [] },
      ],
      retired: [
        { retired: 'flow', replacement: 'trace', call: 'trace(handle, focus:"POST /basket/checkout")' },
      ],
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
    getMcpStatus = vi.fn().mockResolvedValue(status());
    listSessions = vi.fn().mockResolvedValue({ sessions: [] });
    // N4.1 — StartMcp/StopMcp no longer exist on the wire. They stay in this stub on purpose:
    // if the page ever grows a mutating call again, "never called" has to keep failing.
    startMcp = vi.fn().mockResolvedValue({ running: true });
    stopMcp = vi.fn().mockResolvedValue({ stopped: true });
    mcpHandshake = vi.fn().mockResolvedValue({
      ok: true, command: 'C:/app/resources/server/devcontext-mcp.exe',
      serverName: 'devcontext', serverVersion: '1.0.0', protocolVersion: '2024-11-05',
      toolCount: 3, toolNames: ['analyze', 'map', 'get_context'], elapsedMs: 812n, error: '',
    });
    observeToolCalls = vi.fn().mockReturnValue({
      async *[Symbol.asyncIterator]() { /* silent stream */ },
    });
    writeMcpConfig = vi.fn().mockResolvedValue({
      path: 'C:/Code/eshop/.mcp.json',
      relativePath: '.mcp.json',
      action: 'created',
      command: 'C:/app/resources/server/devcontext-mcp.exe',
    });
    listMcpTools = vi.fn().mockResolvedValue(catalog());
    TestBed.configureTestingModule({
      providers: [
        { provide: DevContextApi, useValue: { getMcpStatus, writeMcpConfig, listMcpTools } },
        { provide: DEVCONTEXT_CLIENT, useValue: { listSessions, startMcp, stopMcp, mcpHandshake, observeToolCalls } },
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
    getMcpStatus.mockResolvedValue(status({ observerCount: 2 }));
    const { el } = await createPage();

    expect(getMcpStatus).toHaveBeenCalledTimes(1);
    expect(startMcp).not.toHaveBeenCalled(); // the whole defect: reading used to start it
    expect(text(el, 'mcp-status-text')).toContain('2 watcher(s) attached');
  });

  it('an unreachable server is not rendered as "off" (§3.F.14)', async () => {
    getMcpStatus.mockResolvedValue(null);
    const { el } = await createPage();

    expect(text(el, 'mcp-status-error')).toContain('Could not reach');
    // and it must not claim the binary is there — nothing was measured at all
    expect(text(el, 'mcp-status-label')).toContain('not found');
  });

  it('Copy marks the card that was clicked (§3.F.11)', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="copy-snippet-cursor"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'copy-snippet-cursor')).toBe('Copied!');
    expect(text(el, 'copy-snippet-vscode')).toBe('Copy');
    expect(text(el, 'copy-snippet-claude')).toBe('Copy');
  });

  /**
   * N4.2 (audit §4, Room 2 "setup that works") — the snippets used to be a constant in this file
   * with a hard-coded C:/path/to/DevContext2/... in it: a path that read like a real machine's and
   * existed on none. They now arrive with the status read, so what the page shows is what the
   * server resolved and what its write-config button would produce.
   */
  it('host snippets carry the RESOLVED path the server probed, not a page-side placeholder', async () => {
    const { el } = await createPage();
    const snippets = [...el.querySelectorAll('pre')].map((p) => p.textContent ?? '');

    expect(snippets).toHaveLength(3);
    for (const s of snippets) {
      expect(s).not.toContain('"command": "devcontext-mcp"');      // bare command is not on PATH
      expect(s).not.toContain('C:/path/to/');                       // and no invented path
      expect(s).toContain('C:/app/resources/server/devcontext-mcp.exe');
    }
    expect(text(el, 'mcp-setup-note')).not.toContain('ships with the desktop installer');
    // Source "bundle" is the one case where shipping-with-the-app IS true, so the note says it.
    expect(text(el, 'mcp-setup-note')).toContain('ships beside this app');
  });

  it('with no binary found the snippet admits it instead of naming a plausible path', async () => {
    getMcpStatus.mockResolvedValue(status({
      binaryFound: false, binaryPath: '', binarySource: '',
      hosts: hostCards('<absolute path to devcontext-mcp.exe>'),
    }));
    const { el } = await createPage();

    expect(text(el, 'snippet-claude')).toContain('<absolute path to devcontext-mcp.exe>');
    expect(text(el, 'mcp-setup-note')).toContain('dotnet build src/DevContext.Mcp');
    // and nothing offers to write a config naming something that is not there
    expect((el.querySelector('[data-testid="write-config-claude"]') as HTMLButtonElement).disabled).toBe(true);
  });

  it('write-config writes into the SELECTED analyzed repo and reports what the server did', async () => {
    listSessions.mockResolvedValue({ sessions: [session()] });
    const { fixture, el } = await createPage();

    expect(text(el, 'write-config-repo')).toBe('C:/Code/eshop');
    (el.querySelector('[data-testid="write-config-vscode"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(writeMcpConfig).toHaveBeenCalledWith('handle-abcdef123456', 'vscode');
    expect(text(el, 'write-result-vscode')).toBe('Wrote .mcp.json');
  });

  it('write-config repeats the server\'s "unchanged" instead of claiming a write', async () => {
    listSessions.mockResolvedValue({ sessions: [session()] });
    writeMcpConfig.mockResolvedValue({
      path: 'C:/Code/eshop/.mcp.json', relativePath: '.mcp.json', action: 'unchanged',
      command: 'C:/app/resources/server/devcontext-mcp.exe',
    });
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="write-config-claude"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'write-result-claude')).toBe('.mcp.json already points here');
  });

  it('a refused write is shown as a failure, not as a silent no-op', async () => {
    listSessions.mockResolvedValue({ sessions: [session()] });
    writeMcpConfig.mockRejectedValue(new Error('.mcp.json is not valid JSON'));
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="write-config-claude"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'write-result-claude')).toContain('not valid JSON');
    expect(el.querySelector('[data-testid="write-result-claude"]')?.className).toContain('text-danger');
  });

  it('with no analyzed session there is nothing to write into, and the button says so', async () => {
    const { el } = await createPage();

    const button = el.querySelector('[data-testid="write-config-claude"]') as HTMLButtonElement;
    expect(button.disabled).toBe(true);
    expect(button.getAttribute('title')).toContain('Analyze a repo first');
  });

  it('the feed total counts the rows on screen, and rows carry the WIRE time (§3.F.12)', async () => {
    getMcpStatus.mockResolvedValue(status({ observerCount: 1 }));
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

  /**
   * N4.1 (audit §3.D, §4 Room 2) — the page used to PERFORM status: a dot that went green
   * because the gRPC server answered a mutating call, and a Start/Stop button that flipped a
   * global telemetry mute while its label claimed to control an MCP endpoint it never touched.
   * Both are gone. These pin what replaced them: three checks that are only as green as what
   * the server actually found.
   */
  it('the binary probe is rendered as a verdict WITH the path a host must name', async () => {
    const { el } = await createPage();

    expect(text(el, 'mcp-status-label')).toContain('devcontext-mcp found');
    expect(text(el, 'mcp-binary-path')).toBe('C:/app/resources/server/devcontext-mcp.exe');
    expect(text(el, 'mcp-binary-check')).toContain('bundle');
  });

  it('a missing binary says no host config can work — it does not render as idle', async () => {
    getMcpStatus.mockResolvedValue(status({ binaryFound: false, binaryPath: '', binarySource: '' }));
    const { el } = await createPage();

    expect(text(el, 'mcp-status-label')).toContain('not found');
    expect(el.querySelector('[data-testid="mcp-binary-path"]')).toBeNull();
  });

  it('renders whether an AGENT has actually called, not just that the server is up', async () => {
    getMcpStatus.mockResolvedValue(status({
      lastAgentCallAtUtcMs: Date.now() - 120_000, lastAgentTool: 'get_context', agentCallCount: 7,
    }));
    const { el } = await createPage();

    expect(text(el, 'mcp-last-agent-call')).toContain('get_context');
    expect(text(el, 'mcp-last-agent-call')).toContain('2m ago');
    expect(text(el, 'mcp-last-agent-call')).toContain('7 agent call(s)');
  });

  it('says plainly when no agent has ever called', async () => {
    const { el } = await createPage();
    expect(text(el, 'mcp-last-agent-call')).toContain('No agent has called this server yet');
  });

  it('the handshake reports the REAL tool menu the process answered with', async () => {
    const { fixture, el } = await createPage();
    expect(el.querySelector('[data-testid="mcp-handshake-result"]')).toBeNull();

    (el.querySelector('[data-testid="mcp-handshake-run"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(mcpHandshake).toHaveBeenCalledTimes(1);
    expect(text(el, 'mcp-handshake-result')).toContain('tools/list answered');
    expect(text(el, 'mcp-handshake-result')).toContain('protocol 2024-11-05');
    expect(text(el, 'mcp-handshake-tools')).toBe('analyze · map · get_context');
  });

  it('a failed handshake shows the failure instead of a green light', async () => {
    mcpHandshake.mockResolvedValue({
      ok: false, command: 'C:/app/devcontext-mcp.exe', serverName: '', serverVersion: '',
      protocolVersion: '', toolCount: 0, toolNames: [], elapsedMs: 30_000n,
      error: 'devcontext-mcp.exe did not answer within 30s.',
    });
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="mcp-handshake-run"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(text(el, 'mcp-handshake-error')).toContain('did not answer within 30s');
    expect(text(el, 'mcp-handshake-result')).not.toContain('tools/list answered');
  });

  it('nothing on the page mutates MCP state — not opening it, not re-checking it', async () => {
    const { fixture, el } = await createPage();

    (el.querySelector('[data-testid="mcp-status-refresh"]') as HTMLButtonElement).click();
    await new Promise((r) => setTimeout(r, 5));
    fixture.detectChanges();

    expect(getMcpStatus).toHaveBeenCalledTimes(2);
    expect(startMcp).not.toHaveBeenCalled();
    expect(stopMcp).not.toHaveBeenCalled();
    expect(el.querySelector('[data-testid="mcp-toggle"]')).toBeNull(); // the mute is gone
  });

  /**
   * N4.3 (audit §4, Room 2 "the catalog, served"; BUG-BACKLOG #4) — the page used to carry its
   * own literal array of eight tool names. It offered `search`, which the MCP has never exposed
   * (it is `find`), and `insights`, folded into `stats` by G2.1 — and because the failure is a
   * LABEL, nothing errored. These pin that the menu on screen is the one the server just read
   * off a live tools/list, and that there is no second list to drift.
   */
  it('renders the menu the server read off tools/list, with the descriptions T1.1 put there', async () => {
    const { el } = await createPage();

    expect(listMcpTools).toHaveBeenCalledTimes(1);
    expect(text(el, 'catalog-tool-trace')).toBe('trace');
    expect(text(el, 'mcp-catalog')).toContain('Follow one entry point through the code that serves it.');
    // parameters, required-ness and their descriptions all came off the same reply
    expect(text(el, 'mcp-catalog')).toContain('handle*');
    expect(text(el, 'mcp-catalog')).toContain('depth');
    expect(text(el, 'mcp-catalog-source')).toContain('devcontext-mcp.exe');
  });

  it('shows the unlisted specialists and the retired names, both harvested from the envelope', async () => {
    const { el } = await createPage();

    expect(text(el, 'catalog-specialist-tests_for')).toBe('tests_for');
    expect(text(el, 'mcp-specialists')).toContain('Which tests cover a symbol.');
    expect(text(el, 'mcp-retired')).toContain('flow');
    expect(text(el, 'mcp-retired')).toContain('trace');
  });

  it('Try-a-Tool offers the SERVED names — no page-side list, and no phantom "search"', async () => {
    const { el } = await createPage();
    const options = [...el.querySelectorAll<HTMLOptionElement>('#try-tool-select option')];
    const names = options.map((o) => o.value);

    expect(names).toEqual(['trace', 'read_source', 'get_context', 'tests_for', 'node']);
    expect(names).not.toContain('search');   // the drifted label the literal array carried
    expect(names).not.toContain('insights'); // folded into stats by G2.1, still listed here after
    // read_source has no single gRPC RPC behind it, so the option is disabled and says why
    // rather than being quietly dropped — an omission is how the old list read as complete.
    const readSource = options.find((o) => o.value === 'read_source')!;
    expect(readSource.disabled).toBe(true);
    expect(readSource.title).toContain('call it from an agent');
    expect(options.find((o) => o.value === 'trace')!.disabled).toBe(false);
  });

  it('a catalog that could not be read says so instead of rendering an empty menu', async () => {
    listMcpTools.mockResolvedValue(catalog({
      ok: false, error: 'devcontext-mcp.exe did not answer within 30s.',
      tools: [], specialists: [], retired: [],
    }));
    const { el } = await createPage();

    expect(text(el, 'mcp-catalog-error')).toContain('did not answer within 30s');
    expect(el.querySelector('[data-testid="mcp-catalog"]')).toBeNull();
    expect([...el.querySelectorAll('#try-tool-select option')]).toHaveLength(0);
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
