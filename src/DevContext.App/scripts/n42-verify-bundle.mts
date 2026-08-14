/**
 * N4.2 — the half of "ship it" that the dev-server probe cannot show.
 *
 * The live page probe (n42-verify-setup.mts) runs against the DEV server, which lives in
 * src/DevContext.Server/bin/Debug — so its binary probe resolves source "dev-build". That proves
 * the page measures; it does not prove the INSTALLED layout works. This does: it starts the
 * PUBLISHED server out of src-tauri/resources/server (the directory tauri.conf.json's
 * bundle.resources ships, byte-for-byte what an installer lays down) and asks it for MCP status.
 *
 * The claim under test: on an installed machine, with no repo checkout and nothing on PATH, the
 * probe finds the bundled devcontext-mcp beside the server and reports source "bundle" — which is
 * what makes the page's config snippets true for a user who only ran the installer.
 *
 * Usage (no dev services needed — this starts and stops its own server on a spare port):
 *   node --experimental-strip-types src/DevContext.App/scripts/n42-verify-bundle.mts
 * Exits non-zero on any failed check.
 */
import { createClient } from '@connectrpc/connect';
import { createGrpcWebTransport } from '@connectrpc/connect-web';
import { spawn } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

import { DevContextService } from '../src/app/core/grpc/gen/devcontext/v1/devcontext_pb.ts';

const REPO = 'C:/Code/DevContext2-desktop';
const BUNDLE_DIR = resolve(`${REPO}/src/DevContext.App/src-tauri/resources/server`);
const SERVER_EXE = join(BUNDLE_DIR, 'DevContext.Server.exe');
const PORT = 5183;
const OUT = `${REPO}/eval-results/2026-08-14`;

const failures: string[] = [];
function check(name: string, ok: boolean, detail: string) {
  console.log(`${ok ? 'PASS' : 'FAIL'}  ${name} — ${detail}`);
  if (!ok) failures.push(`${name}: ${detail}`);
}

const server = spawn(SERVER_EXE, ['--urls', `http://127.0.0.1:${PORT}`], { stdio: ['ignore', 'pipe', 'pipe'] });
server.stdout.resume();
server.stderr.resume();

try {
  // Wait for it to answer rather than guessing at a start-up time.
  let up = false;
  for (let i = 0; i < 60 && !up; i++) {
    await new Promise((r) => setTimeout(r, 500));
    up = await fetch(`http://127.0.0.1:${PORT}/health`).then((r) => r.ok).catch(() => false);
  }
  check('the published server starts out of the bundle directory', up, `${SERVER_EXE} on :${PORT}`);
  if (!up) throw new Error('the bundled server never answered /health');

  const client = createClient(DevContextService, createGrpcWebTransport({ baseUrl: `http://127.0.0.1:${PORT}` }));
  const status = await client.getMcpStatus({});

  check('the bundled server finds a devcontext-mcp', status.mcpBinaryFound, status.mcpBinaryPath);
  check('and reports it as source "bundle", not dev-build',
    status.mcpBinarySource === 'bundle', `source=${status.mcpBinarySource}`);
  check('the path it found is the copy inside the bundle directory',
    resolve(dirname(status.mcpBinaryPath)).toLowerCase() === BUNDLE_DIR.toLowerCase(),
    status.mcpBinaryPath);

  const commands = status.hosts.map((h) => {
    const parsed = JSON.parse(h.snippet) as Record<string, { devcontext?: { command?: string } }>;
    return (parsed.mcpServers ?? parsed.servers)?.devcontext?.command ?? '';
  });
  check('every host snippet an installed user copies names that bundled path',
    commands.length === 3 && commands.every((c) => c === status.mcpBinaryPath),
    JSON.stringify(commands[0] ?? ''));

  mkdirSync(OUT, { recursive: true });
  writeFileSync(`${OUT}/N4.2-bundle-probe.json`, JSON.stringify({
    bundleDirectory: BUNDLE_DIR, serverExe: SERVER_EXE, port: PORT,
    mcpBinaryFound: status.mcpBinaryFound,
    mcpBinaryPath: status.mcpBinaryPath,
    mcpBinarySource: status.mcpBinarySource,
    hosts: status.hosts.map((h) => ({ id: h.id, relativePath: h.relativePath })),
    snippetCommands: commands,
    failures,
  }, null, 2));
} finally {
  server.kill();
}

console.log(failures.length === 0 ? '\nALL CHECKS PASSED' : `\n${failures.length} CHECK(S) FAILED`);
process.exit(failures.length === 0 ? 0 : 1);
