// Agent-probe runner - three arms, headless `claude -p` subprocesses. Pure ASCII on purpose.
//
//   node eval/agent-probe/run-probe.mjs --repo eShop --reps 3 [--arms G,M,B] [--questions id,id]
//   node eval/agent-probe/run-probe.mjs --repo eShop --reps 3 --dry-run
//
// What this file is responsible for, in the order the design cares about:
//
//   1. ARM ISOLATION. Each arm gets a fixed argv (DESIGN.md section 8, verbatim). Arm G is never
//      handed an --mcp-config at all; arm M is denied Read/Grep/Glob/Bash. The claude permission
//      system - not this script - enforces it, and the transcript is kept so P1 can PROVE it.
//   2. SPEND CONTROL. The probe subprocesses are invisible to conductor's cost accumulator, so
//      the only brakes that exist are here: --max-budget-usd on every single invocation, and a
//      hard refusal to plan more than MAX_RUNS_PER_INVOCATION runs at once. Neither is a flag.
//   3. RESUMABILITY. Stage P2 is longer than one session. Every completed run is appended to
//      results/runs.jsonl immediately and any cell already in that file is skipped, so a
//      wind-down costs one run and never the batch.
//   4. HONEST ACCOUNTING. cost/usage/turns/duration are read out of the claude result object.
//      Nothing here computes a token count of its own.

import { readFileSync, writeFileSync, appendFileSync, existsSync, mkdirSync } from "fs";
import { join, dirname, resolve } from "path";
import { fileURLToPath } from "url";
import { execFileSync, spawn } from "child_process";
import { createInterface } from "readline";

const HERE = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = resolve(HERE, "..", "..");
const EVAL_REPOS = join(REPO_ROOT, "eval-repos");
const RESULTS = join(HERE, "results");
const RAW = join(RESULTS, "raw");
const RUNS_JSONL = join(RESULTS, "runs.jsonl");
const SYSTEM_PROMPT_FILE = join(HERE, "system.txt");
const MCP_CONFIG = join(HERE, "mcp.json");
const MCP_EXE = join(REPO_ROOT, "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

// ---- the two brakes. Not configurable, on purpose. --------------------------
// The probe subprocesses spend real money that never reaches conductor's budget, so these are the
// only limits in the system. If you find yourself editing either one, stop and escalate instead.
const MAX_BUDGET_USD = "1.50";
const MAX_RUNS_PER_INVOCATION = 60;

// A subprocess that ignores its own budget cap still has to end. Generous - a class B question on
// eShop legitimately runs minutes - but finite, so one wedged child cannot eat a session.
const RUN_TIMEOUT_MS = 20 * 60 * 1000;

const VALID_ARMS = ["G", "M", "B"];
const DEFAULT_MODEL = "claude-opus-5";

// ---- args -------------------------------------------------------------------

function argOf(name, fallback = null) {
  const i = process.argv.indexOf(`--${name}`);
  if (i === -1) return fallback;
  const v = process.argv[i + 1];
  if (v === undefined || v.startsWith("--")) return fallback;
  return v;
}
const hasFlag = (name) => process.argv.includes(`--${name}`);

const OPT = {
  repo: argOf("repo"),
  arms: (argOf("arms", "G,M,B") || "").split(",").map((s) => s.trim()).filter(Boolean),
  reps: Number(argOf("reps", "1")),
  questions: (argOf("questions", "") || "").split(",").map((s) => s.trim()).filter(Boolean),
  model: argOf("model", DEFAULT_MODEL),
  seed: Number(argOf("seed", "20260811")),
  maxRuns: Number(argOf("max-runs", String(MAX_RUNS_PER_INVOCATION))),
  tag: argOf("tag", ""),
  dryRun: hasFlag("dry-run"),
  skipWarm: hasFlag("skip-warm"),
  allowNoBare: hasFlag("allow-no-bare"),
};

function die(msg) {
  console.error(`\nREFUSED: ${msg}\n`);
  process.exit(2);
}

// ---- isolation mode (DESIGN 6.3) --------------------------------------------
// --bare is what the pre-registration asked for. On an OAuth-only machine it cannot authenticate
// at all: bare mode reads ANTHROPIC_API_KEY or an apiKeyHelper and nothing else, so every run
// comes back "Not logged in". The fallback reproduces the parts of --bare that this experiment
// actually depends on (no user/project/local settings -> no hooks, no ambient permissions; only
// the MCP config passed on the command line; an explicit system prompt), and it is recorded on
// every row so nobody has to take this comment's word for it. It never engages by accident.
const HAVE_API_KEY = Boolean(process.env.ANTHROPIC_API_KEY);
const ISOLATION = HAVE_API_KEY ? "bare" : "no-settings-fallback";

// ---- question sets ----------------------------------------------------------

function loadQuestions(repoName) {
  const path = join(HERE, "questions", `${repoName}.json`);
  if (!existsSync(path)) die(`no question set at ${path}`);
  const doc = JSON.parse(readFileSync(path, "utf8"));

  const repoDir = join(EVAL_REPOS, doc.repo);
  if (!existsSync(repoDir)) die(`eval-repos/${doc.repo} is not cloned`);

  // The keys describe one exact tree. Running against a different one is not a measurement.
  const head = execFileSync("git", ["-C", repoDir, "rev-parse", "HEAD"], { encoding: "utf8" }).trim();
  if (!head.startsWith(doc.sha)) {
    die(`eval-repos/${doc.repo} HEAD is ${head.slice(0, 8)} but the answer keys were written against ${doc.sha}`);
  }
  return { doc, repoDir, head };
}

// ---- deterministic shuffle --------------------------------------------------
// DESIGN 6.5: question order randomised, arms interleaved rather than blocked, so a mid-batch
// change in API latency or model routing hits all three arms equally. Seeded so the order that
// produced a given runs.jsonl can be reconstructed.

function mulberry32(a) {
  return function () {
    a |= 0; a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
function shuffled(items, seed) {
  const rnd = mulberry32(seed);
  const a = items.slice();
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(rnd() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

// ---- resumability -----------------------------------------------------------

const cellKey = (r) => `${r.repo}|${r.questionId}|${r.arm}|${r.rep}|${r.model}`;

function alreadyRecorded() {
  const done = new Set();
  if (!existsSync(RUNS_JSONL)) return done;
  const lines = readFileSync(RUNS_JSONL, "utf8").split("\n").filter((l) => l.trim());
  for (const [i, line] of lines.entries()) {
    let r;
    try { r = JSON.parse(line); }
    catch { die(`results/runs.jsonl line ${i + 1} is not valid JSON - refusing to append to a corrupt ledger`); }
    done.add(cellKey(r));
  }
  return done;
}

// ---- warm the engine (DESIGN 4.5, and it is not optional) -------------------
// A cold analysis takes minutes and would land entirely inside whichever arm ran first. The MCP
// exe is a thin client over a gRPC server that outlives it, so analysing once here leaves every
// later claude-spawned MCP process hitting the same warm session. We assert `cached` on a SECOND
// process precisely because the first one is allowed to be the cold one.

function mcpClient() {
  const proc = spawn(MCP_EXE, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  const pending = new Map();
  let nextId = 1;
  rl.on("line", (line) => {
    let msg;
    try { msg = JSON.parse(line); } catch { return; }
    const w = pending.get(msg.id);
    if (w) { clearTimeout(w.timer); pending.delete(msg.id); w.resolve(msg); }
  });
  proc.stderr.resume();
  return {
    call: (method, params = {}, timeoutMs = 45000) =>
      new Promise((res, rej) => {
        const id = nextId++;
        const timer = setTimeout(() => { pending.delete(id); rej(new Error(`mcp timeout: ${method}`)); }, timeoutMs);
        pending.set(id, { resolve: res, timer });
        proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
      }),
    notify: (method, params = {}) => proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n"),
    close: () => { rl.close(); proc.kill(); },
  };
}

async function analyzeOnce(repoDir, budgetMs) {
  const c = mcpClient();
  try {
    // The handshake does not return until a gRPC server is live behind the MCP, and it starts one
    // if none is listening - that boot is the slowest wait here, so it gets its own budget.
    const init = await c.call("initialize", {
      protocolVersion: "2024-11-05",
      capabilities: {},
      clientInfo: { name: "agent-probe-warm", version: "1" },
    }, 180000);
    if (init.error) throw new Error(`initialize failed: ${JSON.stringify(init.error)}`);
    c.notify("notifications/initialized", {});

    const t0 = Date.now();
    const resp = await c.call("tools/call", { name: "analyze", arguments: { path: repoDir } }, budgetMs);
    const ms = Date.now() - t0;
    if (resp.error) throw new Error(`analyze failed: ${JSON.stringify(resp.error)}`);
    const text = (resp.result?.content || []).map((b) => b.text || "").join("");
    let payload = {};
    try { payload = JSON.parse(text); } catch { payload = { text }; }
    return { cached: payload.cached === true, ms, payload };
  } finally {
    c.close();
  }
}

async function warmRepo(repoDir) {
  if (!existsSync(MCP_EXE)) die(`MCP server not built at ${MCP_EXE} - build src/DevContext.Mcp first`);
  console.log(`warm: analyzing ${repoDir} (cold pass may take minutes)`);
  const cold = await analyzeOnce(repoDir, 15 * 60 * 1000);
  console.log(`warm: pass 1 cached=${cold.cached} ${cold.ms}ms`);
  // Second pass in a FRESH mcp process. This is the assertion that matters: every arm-M/B run
  // spawns its own MCP, so "cached on a second process" is the only proof that they will be warm.
  const warm = await analyzeOnce(repoDir, 5 * 60 * 1000);
  console.log(`warm: pass 2 cached=${warm.cached} ${warm.ms}ms (fresh mcp process)`);
  if (!warm.cached) {
    die("analyze did not report cached:true on a second, fresh MCP process - the arms would not be " +
        "warm and a cold analysis would land entirely inside whichever arm ran first (DESIGN 4.5)");
  }
  return { cold, warm };
}

// ---- the three arms (DESIGN section 8, verbatim) ----------------------------

function armArgs(arm, repoDir) {
  const shared = [
    "-p",
    "--output-format", "stream-json",
    "--verbose",
    "--model", OPT.model,
    "--strict-mcp-config",
    "--max-budget-usd", MAX_BUDGET_USD,
    "--system-prompt", readFileSync(SYSTEM_PROMPT_FILE, "utf8"),
  ];
  if (ISOLATION === "bare") shared.push("--bare");
  else shared.push("--setting-sources", "");

  if (arm === "G") {
    // No --mcp-config at all: the MCP server is not merely denied, it is never configured.
    return [...shared,
      "--add-dir", repoDir,
      "--allowedTools", "Read,Grep,Glob",
      "--disallowedTools", "Edit,Write,WebFetch,WebSearch"];
  }
  if (arm === "M") {
    return [...shared,
      "--mcp-config", MCP_CONFIG,
      "--allowedTools", "mcp__devcontext",
      "--disallowedTools", "Read,Grep,Glob,Bash,Edit,Write"];
  }
  if (arm === "B") {
    return [...shared,
      "--add-dir", repoDir,
      "--mcp-config", MCP_CONFIG,
      "--allowedTools", "Read,Grep,Glob,mcp__devcontext",
      "--disallowedTools", "Edit,Write,WebFetch,WebSearch"];
  }
  die(`unknown arm ${arm}`);
}

function claudeBin() {
  if (process.env.CLAUDE_BIN) return process.env.CLAUDE_BIN;
  const home = process.env.USERPROFILE || process.env.HOME || "";
  const local = join(home, ".local", "bin", process.platform === "win32" ? "claude.exe" : "claude");
  return existsSync(local) ? local : (process.platform === "win32" ? "claude.exe" : "claude");
}

// The child inherits this session's environment, which is itself a claude process. Its
// CLAUDE_CODE_* markers would change the child's behaviour, so they are stripped - identically
// for all three arms, which is what matters.
function childEnv() {
  const env = { ...process.env };
  for (const k of Object.keys(env)) {
    if (k.startsWith("CLAUDE_CODE_") || k === "CLAUDECODE") delete env[k];
  }
  return env;
}

// ---- one run ----------------------------------------------------------------

function slug(s) { return String(s).replace(/[^A-Za-z0-9._-]/g, "_"); }

function spawnRun(cell, repoDir) {
  return new Promise((resolvePromise) => {
    const args = armArgs(cell.arm, repoDir);
    const started = Date.now();
    const child = spawn(claudeBin(), args, {
      cwd: repoDir,
      env: childEnv(),
      stdio: ["pipe", "pipe", "pipe"],
      windowsHide: true,
    });

    const events = [];
    let stdoutBuf = "";
    let stderr = "";
    let timedOut = false;

    const timer = setTimeout(() => { timedOut = true; child.kill(); }, RUN_TIMEOUT_MS);

    child.stdout.on("data", (d) => {
      stdoutBuf += d.toString();
      let nl;
      while ((nl = stdoutBuf.indexOf("\n")) !== -1) {
        const line = stdoutBuf.slice(0, nl).trim();
        stdoutBuf = stdoutBuf.slice(nl + 1);
        if (!line) continue;
        try { events.push(JSON.parse(line)); } catch { /* non-JSON noise */ }
      }
    });
    child.stderr.on("data", (d) => { stderr += d.toString(); });

    child.on("close", (code) => {
      clearTimeout(timer);
      if (stdoutBuf.trim()) { try { events.push(JSON.parse(stdoutBuf.trim())); } catch { /* partial */ } }
      resolvePromise({ events, stderr, exitCode: code, timedOut, wallMs: Date.now() - started, args });
    });

    // The prompt goes on STDIN. As an argv positional it is swallowed by the variadic
    // --allowedTools/--disallowedTools and claude exits saying no input was provided.
    child.stdin.write(cell.prompt);
    child.stdin.end();
  });
}

function toolCallsOf(events) {
  const names = [];
  for (const e of events) {
    const content = e?.message?.content;
    if (!Array.isArray(content)) continue;
    for (const block of content) {
      if (block?.type === "tool_use" && block.name) names.push(block.name);
    }
  }
  return names;
}

function initFacts(events) {
  const init = events.find((e) => e.type === "system" && e.subtype === "init");
  if (!init) return { toolsOffered: null, mcpToolsOffered: null, mcpServers: null };
  const tools = Array.isArray(init.tools) ? init.tools : [];
  return {
    toolsOffered: tools.length,
    mcpToolsOffered: tools.filter((t) => String(t).startsWith("mcp__")).length,
    mcpServers: init.mcp_servers ?? null,
  };
}

// Right-censoring (DESIGN 6.6): a run that hit the cap is scored incorrect at cost = cap, never
// dropped. The raw signals are recorded next to the flag so the call can be re-derived later.
function isCensored(result, timedOut) {
  if (timedOut) return true;
  if (!result) return true;
  if (result.subtype && result.subtype !== "success") return true;
  const reason = String(result.terminal_reason || "");
  if (/budget|max_turns|limit/i.test(reason)) return true;
  if (result.is_error === true) return true;
  return false;
}

async function runCell(cell, repoDir, ordinal, total) {
  const label = `${cell.questionId}/${cell.arm}/rep${cell.rep}`;
  console.log(`[${ordinal}/${total}] ${label} ...`);

  const out = await spawnRun(cell, repoDir);
  const result = out.events.find((e) => e.type === "result") || null;
  const calls = toolCallsOf(out.events);
  const facts = initFacts(out.events);

  const dir = join(RAW, cell.repo);
  mkdirSync(dir, { recursive: true });
  const base = `${slug(cell.questionId)}__${cell.arm}__rep${cell.rep}`;
  const streamPath = join(dir, `${base}.stream.jsonl`);
  const resultPath = join(dir, `${base}.result.json`);
  writeFileSync(streamPath, out.events.map((e) => JSON.stringify(e)).join("\n") + "\n", "utf8");
  writeFileSync(resultPath, JSON.stringify(result ?? { missing: true, stderr: out.stderr.slice(0, 4000) }, null, 2), "utf8");

  const censored = isCensored(result, out.timedOut);
  const row = {
    repo: cell.repo,
    questionId: cell.questionId,
    questionClass: cell.questionClass,
    arm: cell.arm,
    rep: cell.rep,
    model: OPT.model,
    // Read out of the result object. Nothing here recomputes a token count.
    answer: result?.result ?? "",
    toolCalls: calls,
    costUsd: result?.total_cost_usd ?? null,
    usage: result?.usage ?? null,
    modelUsage: result?.modelUsage ?? null,
    numTurns: result?.num_turns ?? null,
    durationMs: result?.duration_ms ?? null,
    durationApiMs: result?.duration_api_ms ?? null,
    censored,
    // Provenance - everything needed to re-derive a row or explain it away.
    maxBudgetUsd: Number(MAX_BUDGET_USD),
    isolation: ISOLATION,
    terminalReason: result?.terminal_reason ?? null,
    subtype: result?.subtype ?? null,
    isError: result?.is_error ?? null,
    permissionDenials: result?.permission_denials ?? null,
    sessionId: result?.session_id ?? null,
    exitCode: out.exitCode,
    timedOut: out.timedOut,
    wallMs: out.wallMs,
    devcontextSha: cell.devcontextSha,
    repoSha: cell.repoSha,
    tag: OPT.tag || null,
    startedAt: new Date().toISOString(),
    ...facts,
    rawStream: `results/raw/${cell.repo}/${base}.stream.jsonl`,
    rawResult: `results/raw/${cell.repo}/${base}.result.json`,
  };

  appendFileSync(RUNS_JSONL, JSON.stringify(row) + "\n", "utf8");
  const mcpShare = calls.length ? (calls.filter((c) => c.startsWith("mcp__")).length / calls.length).toFixed(2) : "-";
  console.log(`      cost=${row.costUsd} turns=${row.numTurns} calls=${calls.length} mcpShare=${mcpShare} censored=${censored}`);
  return row;
}

// ---- main -------------------------------------------------------------------

async function main() {
  if (!OPT.repo) die("--repo <name> is required (a file must exist at questions/<name>.json)");
  for (const a of OPT.arms) if (!VALID_ARMS.includes(a)) die(`--arms contains '${a}', expected some of ${VALID_ARMS.join(",")}`);
  if (!Number.isInteger(OPT.reps) || OPT.reps < 1) die("--reps must be a positive integer");
  if (OPT.maxRuns > MAX_RUNS_PER_INVOCATION) {
    die(`--max-runs ${OPT.maxRuns} exceeds the hard ceiling of ${MAX_RUNS_PER_INVOCATION}. ` +
        "The probe subprocesses are invisible to conductor's budget; this ceiling is the only brake.");
  }
  if (ISOLATION !== "bare" && !OPT.allowNoBare) {
    die("ANTHROPIC_API_KEY is not set, so --bare cannot authenticate (bare mode never reads OAuth " +
        "or the keychain) and every run would come back 'Not logged in'. Either set the key and get " +
        "the pre-registered --bare, or pass --allow-no-bare to run with the documented fallback " +
        "(--setting-sources \"\" + --strict-mcp-config + explicit --system-prompt), which is " +
        "recorded as isolation:'no-settings-fallback' on every row.");
  }

  const { doc, repoDir, head } = loadQuestions(OPT.repo);
  const devcontextSha = execFileSync("git", ["-C", REPO_ROOT, "rev-parse", "HEAD"], { encoding: "utf8" }).trim();

  let questions = doc.questions;
  if (OPT.questions.length) {
    questions = questions.filter((q) => OPT.questions.includes(q.id));
    const missing = OPT.questions.filter((id) => !doc.questions.some((q) => q.id === id));
    if (missing.length) die(`--questions names ids not in the set: ${missing.join(", ")}`);
  }

  // Every arm gets the byte-identical prompt. The repo root is stated because arm M has no cwd and
  // no file tools; withholding it would test whether the MCP can guess a path, not whether it helps.
  const cells = [];
  for (const q of questions) {
    for (const arm of OPT.arms) {
      for (let rep = 1; rep <= OPT.reps; rep++) {
        cells.push({
          repo: doc.repo,
          questionId: q.id,
          questionClass: q.class,
          arm,
          rep,
          model: OPT.model,
          repoSha: head,
          devcontextSha,
          prompt: `Repository root: ${repoDir.replace(/\\/g, "/")}\n\n${q.prompt}`,
        });
      }
    }
  }

  const done = alreadyRecorded();
  const planned = shuffled(cells, OPT.seed).filter((c) => !done.has(cellKey(c)));
  const skipped = cells.length - planned.length;

  console.log(`repo=${doc.repo}@${head.slice(0, 8)} model=${OPT.model} isolation=${ISOLATION} cap=$${MAX_BUDGET_USD}`);
  console.log(`cells=${cells.length} alreadyRecorded=${skipped} planned=${planned.length} seed=${OPT.seed}`);

  if (planned.length > OPT.maxRuns) {
    die(`${planned.length} runs planned but the ceiling is ${OPT.maxRuns}. Narrow with --questions / ` +
        "--arms / --reps and run again; the file is resumable, so a second invocation picks up where " +
        "this one stopped.");
  }
  if (planned.length === 0) { console.log("nothing to do - every cell is already recorded"); return; }

  if (OPT.dryRun) {
    console.log("\n--dry-run: no subprocess is spawned. Planned order:");
    planned.forEach((c, i) => console.log(`  ${String(i + 1).padStart(3)}. ${c.questionId} arm=${c.arm} rep=${c.rep}`));
    console.log("\nargv per arm (system prompt elided):");
    for (const arm of OPT.arms) {
      const shown = armArgs(arm, repoDir).map((a) => (a.includes("\n") ? "<contents of system.txt>" : a));
      console.log(`  ${arm}: ${claudeBin()} ${shown.map((a) => (a === "" ? '""' : a)).join(" ")}`);
    }
    console.log("\nprompt (identical in every arm), first cell:\n---\n" + planned[0].prompt + "\n---");
    return;
  }

  mkdirSync(RAW, { recursive: true });
  if (!OPT.skipWarm) await warmRepo(repoDir);

  let spent = 0;
  for (const [i, cell] of planned.entries()) {
    const row = await runCell(cell, repoDir, i + 1, planned.length);
    spent += row.costUsd || 0;
  }
  console.log(`\ndone: ${planned.length} runs, $${spent.toFixed(4)} recorded in results/runs.jsonl`);
}

main().catch((e) => { console.error(e); process.exit(1); });
