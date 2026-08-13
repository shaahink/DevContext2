// WHICH ENGINE ANSWERED — the guard that keeps a wire probe from measuring the wrong build.
//
// Measured 2026-08-13 (T1.4). The MCP's endpoint was hardcoded 127.0.0.1:5179 on both sides, and
// ServerShim.EnsureServerRunning REUSES whatever already answers /health there, whichever checkout
// built it. Two conductor runs share this machine, so a probe in THIS repo was served by the OTHER
// repo's DevContext.Server and reported this repo's just-landed fixes as still broken — a red that
// no amount of staring at the diff would have explained.
//
// Two defences, both cheap:
//   1. ISOLATE — every probe spawns its MCP with DEVCONTEXT_ENDPOINT set to its own port, so it
//      neither joins nor disturbs a server another checkout (or a dev's desktop app) is using.
//   2. VERIFY  — /health now reports baseDirectory, pid and startedAt. The probe asserts the answer
//      came from ITS repo, and that the process started AFTER the last build of the dll it is about
//      to measure. That second half is the stale-binary trap, caught by the probe instead of by a
//      confusing red three sessions later.
//
// A probe that cannot confirm both must FAIL, not warn: an unattributed measurement is worse than
// no measurement, because it looks like proof.

const { existsSync, statSync } = require("fs");
const { join, resolve } = require("path");

const REPO_ROOT = resolve(join(__dirname, "..", ".."));

// Not 5179: that is the port a dev's own server and the desktop app use. A probe run must be able
// to run beside them without either side noticing. Override for a second concurrent probe.
const ENDPOINT = process.env.DEVCONTEXT_PROBE_ENDPOINT ?? "http://127.0.0.1:5279";

/** Environment for a spawned MCP: it, and the server it spawns, both read DEVCONTEXT_ENDPOINT. */
function probeEnv(endpoint = ENDPOINT) {
  return { ...process.env, DEVCONTEXT_ENDPOINT: endpoint };
}

function norm(p) {
  return resolve(p).replace(/\\/g, "/").toLowerCase().replace(/\/+$/, "");
}

/**
 * Reads /health and judges it. Returns { ok, detail, health } — never throws.
 * `repoRoot` is the checkout the caller believes it is measuring.
 */
async function verifyServerIdentity(endpoint = ENDPOINT, repoRoot = REPO_ROOT) {
  let health = null;
  try {
    const res = await fetch(`${endpoint}/health`, { signal: AbortSignal.timeout(5000) });
    health = await res.json();
  } catch (e) {
    return { ok: false, detail: `no /health at ${endpoint}: ${e.message}`, health: null };
  }
  if (!health?.baseDirectory) {
    return {
      ok: false, health,
      detail: `/health has no baseDirectory - server predates the T1.4 identity fields`
        + ` (version ${health?.version ?? "?"}); it is NOT this build`,
    };
  }
  const mine = norm(health.baseDirectory).startsWith(norm(repoRoot));
  if (!mine) {
    return { ok: false, health, detail: `served by a FOREIGN build: ${health.baseDirectory} (pid ${health.pid})` };
  }
  // Stale-binary half: the running server predates the dll on disk it is supposed to be running.
  const dll = join(repoRoot, "src", "DevContext.Server", "bin", "Debug", "net10.0", "DevContext.Core.dll");
  const built = existsSync(dll) ? statSync(dll).mtimeMs : null;
  const started = Date.parse(health.startedAt ?? "");
  if (built !== null && Number.isFinite(started) && started < built) {
    return {
      ok: false, health,
      detail: `STALE server (pid ${health.pid}) started ${health.startedAt} but`
        + ` DevContext.Core.dll was built ${new Date(built).toISOString()} - it is running the old engine`,
    };
  }
  return { ok: true, health, detail: `${health.baseDirectory} (pid ${health.pid}, started ${health.startedAt})` };
}

module.exports = { REPO_ROOT, ENDPOINT, probeEnv, verifyServerIdentity };
