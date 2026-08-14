/**
 * Z1.2 — put REAL agent traffic on the MCP page's live feed while a screenshot is being taken.
 *
 * The feed has no backlog: `ObserveToolCalls` streams to whoever is subscribed and replays
 * nothing (DevContextGrpcService.cs — a channel per observer, no history buffer). So a shot of
 * /mcp taken on a quiet machine shows the empty state, and the affordance N4.3 added (a trace row
 * opens in Explore, a get_context row replays in Studio) cannot appear in it. Faking rows would
 * defeat the point of the page, so this drives the real sidecar instead:
 *
 *   devcontext-mcp.exe  --stdio-->  DEVCONTEXT_ENDPOINT=the dev server the app is already on
 *
 * `ServerShim.EnsureServerRunning` reuses whatever answers /health at that endpoint, so the calls
 * land on the same server the browser is watching, tagged `agent` by the origin middleware, and
 * the page renders them live. Same mechanism as `eval/mcp-qa/deep-link-truth.js`, which measures
 * these rows on the wire; this is its screenshot-side twin.
 *
 * Analyze the SAME repo the app has open, or the rows are honest but unopenable (a row whose repo
 * is not the live session offers to adopt it first — true, and not what the README shot is for).
 */

import { spawn } from 'node:child_process';
import { createInterface } from 'node:readline';
import { existsSync } from 'node:fs';
import { join } from 'node:path';

/** JSON-RPC over the sidecar's stdio — the same shape the eval probes use. */
function mcpClient(exePath, endpoint) {
  const proc = spawn(exePath, [], {
    stdio: ['pipe', 'pipe', 'pipe'],
    windowsHide: true,
    env: { ...process.env, DEVCONTEXT_ENDPOINT: endpoint },
  });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  const pending = new Map();
  let nextId = 1;
  rl.on('line', (line) => {
    try {
      const msg = JSON.parse(line);
      const waiter = msg.id !== undefined && pending.get(msg.id);
      if (waiter) {
        clearTimeout(waiter.timer);
        waiter.resolve(msg);
        pending.delete(msg.id);
      }
    } catch {
      /* the sidecar also logs non-JSON lines */
    }
  });
  proc.stderr.resume();

  const call = (method, params = {}, timeoutMs = 60_000) =>
    new Promise((resolve, reject) => {
      const id = nextId++;
      const timer = setTimeout(() => {
        if (pending.delete(id)) reject(new Error(`Timeout: ${method}`));
      }, timeoutMs);
      pending.set(id, { resolve, timer });
      proc.stdin.write(`${JSON.stringify({ jsonrpc: '2.0', id, method, params })}\n`);
    });

  return {
    call,
    notify: (method, params = {}) =>
      proc.stdin.write(`${JSON.stringify({ jsonrpc: '2.0', method, params })}\n`),
    close: async () => {
      rl.close();
      proc.stdin.end();
      await new Promise((res) => {
        const t = setTimeout(res, 5_000);
        proc.once('exit', () => {
          clearTimeout(t);
          res();
        });
      });
      if (proc.exitCode === null) proc.kill();
    },
  };
}

async function tool(client, name, args, timeoutMs = 180_000) {
  const raw = await client.call('tools/call', { name, arguments: args }, timeoutMs);
  const text = (raw.result?.content ?? []).map((c) => c.text ?? '').join('\n');
  try {
    return { text, body: JSON.parse(text) };
  } catch {
    return { text, body: null };
  }
}

/**
 * Runs one small agent session against `endpoint`. Never throws — a failed seed must not lose the
 * screenshot; it returns what it managed and the caller reports it.
 *
 * @returns {Promise<{ ok: boolean, calls: string[], focus: string|null, detail: string }>}
 */
export async function seedAgentCalls({ repoRoot, endpoint, repoPath, log = console.log }) {
  const exe = join(repoRoot, 'src', 'DevContext.Mcp', 'bin', 'Debug', 'net10.0', 'devcontext-mcp.exe');
  const calls = [];
  if (!existsSync(exe)) {
    return { ok: false, calls, focus: null, detail: `MCP exe not built: ${exe}` };
  }

  const client = mcpClient(exe, endpoint);
  try {
    const init = await client.call(
      'initialize',
      {
        protocolVersion: '2024-11-05',
        capabilities: {},
        clientInfo: { name: 'readme-capture', version: '0.0.1' },
      },
      120_000,
    );
    if (init.error) throw new Error(`initialize failed: ${JSON.stringify(init.error)}`);
    client.notify('notifications/initialized', {});

    log('    → sidecar: analyze (the app already analyzed this repo, so this is a cache hit)');
    const analyzed = await tool(client, 'analyze', { path: repoPath }, 300_000);
    const handle = analyzed.body?.handle ?? analyzed.body?.session?.handle;
    if (!handle) throw new Error(`analyze returned no handle: ${analyzed.text.slice(0, 200)}`);
    calls.push('analyze');

    const entries = await tool(client, 'entrypoints', { handle, full: true });
    calls.push('entrypoints');
    const focus = (entries.body?.entries ?? []).find((e) => e.title)?.title ?? null;
    if (!focus) throw new Error('entrypoints returned nothing with a title');
    log(`    → sidecar: trace + get_context on "${focus}"`);

    await tool(client, 'trace', { handle, focus });
    calls.push('trace');
    await tool(client, 'get_context', { handle, focus });
    calls.push('get_context');

    return { ok: true, calls, focus, detail: `${calls.length} agent calls on ${focus}` };
  } catch (err) {
    return { ok: false, calls, focus: null, detail: err?.message ?? String(err) };
  } finally {
    await client.close().catch(() => {});
  }
}
