// A1.2 - pass 2 of the grading protocol (DESIGN section 7). Pure ASCII on purpose.
//
// The judge is an LLM, so the only thing that makes its scores worth anything is what it is NOT
// allowed to see. Four controls, all of them mechanical:
//
//   1. BLIND TO THE ARM. The judge never sees the transcript - tool calls name the arm outright.
//      It sees question + answer key + the final answer with arm-identifying vocabulary redacted,
//      and nothing else. `--anonymise-only` runs the redactor and the leak scan with no subprocess,
//      which is how the redaction list was iterated to zero BEFORE the first judge call.
//   2. BLIND TO THE REPO. Zero tools: the entire non-MCP tool universe is denied and no
//      --mcp-config is passed, so it cannot go and look. It grades against the key or not at all.
//   3. FRESH SESSION PER ITEM. One `claude -p` subprocess per run, no shared context, so item 40
//      cannot be graded in the light of item 39.
//   4. SAME BRAKES AS THE PROBE. --max-budget-usd on every invocation and a ceiling on how many
//      subprocesses one invocation may start. A judge sweep is 54 invisible claude processes.
//
// Resumable: appends to results/judged.jsonl as each item lands and skips cells already recorded.
//
//   node eval/agent-probe/judge.mjs --anonymise-only     # redact + leak scan, no subprocess
//   node eval/agent-probe/judge.mjs                      # judge every unrecorded item
//   node eval/agent-probe/judge.mjs --limit 2            # judge two, for a smoke test

import { readFileSync, writeFileSync, appendFileSync, existsSync, mkdirSync, readdirSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { spawn } from "node:child_process";

const HERE = dirname(fileURLToPath(import.meta.url));
const RESULTS = join(HERE, "results");
const RUNS = join(RESULTS, "runs.jsonl");
const OUT = join(RESULTS, "judged.jsonl");
const PROMPT_DIR = join(RESULTS, "judge-prompts");
const SCAN_REPORT = join(RESULTS, "a1.2-leak-scan.md");
const SCRATCH = join(RESULTS, "judge-cwd");

const argv = process.argv.slice(2);
const has = (n) => argv.includes(`--${n}`);
const argOf = (n, d) => { const i = argv.indexOf(`--${n}`); return i >= 0 && argv[i + 1] ? argv[i + 1] : d; };
const REPO = argOf("repo", "eShop");
const MODEL = argOf("model", "claude-opus-5");
const EFFORT = argOf("effort", "high");
const LIMIT = Number(argOf("limit", "60"));
const ANON_ONLY = has("anonymise-only");
const MAX_BUDGET_USD = "1.50";

// Same ceiling as the probe runner, same reason: these subprocesses are invisible to the
// conductor's cost accumulator, so the per-invocation cap and this ceiling are the only brakes.
const SPAWN_CEILING = 60;
if (!Number.isFinite(LIMIT) || LIMIT < 1 || LIMIT > SPAWN_CEILING) {
  console.error(`REFUSING: --limit must be 1..${SPAWN_CEILING} (got ${argOf("limit", "60")}).`);
  process.exit(2);
}

// ---- redaction --------------------------------------------------------------------------------
//
// Pre-registered before the first judge call (ledger, stage A1). The load-bearing property is that
// BOTH arms' vocabulary is redacted, not just the treatment's: stripping "overview" while leaving
// "grep" would hand the judge the arm on a plate in the other direction.

const TOK = "[REDACTED]";

const MCP_TOOLS = ["analyze", "config", "entrypoints", "find", "get_context", "impact", "map",
  "neighbors", "node", "overview", "read_source", "resolve", "seam", "stats", "tests_for",
  "top_flows", "trace", "usages"];
const NATIVE_TOOLS = ["Read", "Grep", "Glob", "Bash", "Edit", "Write", "Task", "WebFetch"];
const ALL_TOOLS = [...MCP_TOOLS, ...NATIVE_TOOLS];

const rx = (s, f = "g") => new RegExp(s, f);
const alt = (xs) => xs.map((s) => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")).join("|");

// A heading whose whole section is about HOW the answer was found, not what the answer is.
// Deliberately narrow. A first draft carried a bare `process` alternative and it deleted the
// *answer* to the class-A question ("## Processes that start when the solution runs") in all three
// arms - a redactor that eats substance biases the endpoint it is supposed to protect. Every
// alternative here must be unambiguously about the act of looking.
const METHOD_HEADING = /^(#{1,6})\s*(?:\*\*)?\s*(methodology|method used|how i |how this was (?:found|determined)|tools? used|tooling|investigation|search (?:method|strategy|path)|evidence trail|verification (?:method|steps)|provenance|what i did|steps taken)/i;

// A sentence dropped WHOLE, and the bar for that is high: it must open with a method marker, so
// the sentence is method narration end to end. An earlier draft dropped any sentence CONTAINING a
// method phrase and took real answer content with it - "Three files contain the literal string
// `GracePeriodTime` (confirmed by ...)" is the answer to the class-F question and a trailing
// method clause is no reason to delete it. Method vocabulary inside a substantive sentence is
// neutralised in place by the rewrites below instead.
const METHOD_SENTENCE = [
  /^\s*(?:[-*]\s*)?(?:\*\*|_)?\s*(?:method|methodology|how i (?:established|found|verified|determined|did) this|how this was (?:found|determined)|verification method|provenance|tooling|search method|what i did)\b\s*:?/i,
];

// A line inside a fence that is a shell invocation, i.e. the arm's fingerprint verbatim.
const SHELL_LINE = /^\s*(?:\$\s+)?(?:cd|ls|cat|head|tail|sed|awk|grep|rg|find|dir|type|Select-String|Get-ChildItem|Get-Content|git\s+(?:grep|ls-files))\b/;

// Ordered token rewrites. Each entry is [name, regex, replacement].
const REWRITES = [
  ["mcp-identifier", /mcp__[A-Za-z0-9_]+/g, TOK],
  ["product-name", /\bdev\s*context\b/gi, TOK],
  ["fenced-tool-name", rx("`\\s*(?:" + alt(ALL_TOOLS) + ")\\s*(?:\\([^`)]*\\))?\\s*`", "g"), TOK],
  ["tool-reference", rx("\\b(?:the\\s+)?(?:" + alt(ALL_TOOLS) + ")\\s+(?:tool|call|command|server|results?|output)\\b", "gi"), TOK],
  ["tool-invocation", rx("\\b(?:" + alt(MCP_TOOLS) + ")\\((?=[^)]{0,80}\\))", "g"), TOK + "("],
  // The tool need not be named to give the arm away: "the tool reported ..." is already a tell,
  // and it is the form an answer reaches for when it is paraphrasing a structured result.
  ["generic-tool-noun", /\b(?:the|this|that|a|an|its|one)\s+(?:mcp\s+)?(?:tool|server|index|indexer|analyzer|analyser)\b/gi, TOK],
  ["search-vocabulary", /\b(ripgrep|grep|globbing|glob(?:bed|bing)?|git\s+grep|git\s+ls-files|Select-String|Get-ChildItem|full[- ]text search)\b/gi, TOK],
  // Method vocabulary neutralised IN PLACE, so a substantive sentence survives with its facts
  // intact and only the how-I-looked part removed.
  ["method-verb", /\bI\s+(?:ran|used|called|grepped|searched|invoked|queried|executed|scanned|traced|crawled|listed|inspected|walked)\b/gi, "I " + TOK],
  ["method-object", /\b(?:analy[sz]ed\s+symbol|symbol\s+graph|content\s+search|text\s+search|recursive\s+search|repository-wide\s+(?:content\s+)?search|the\s+analysis\s+(?:covered|shows|showed|reports|reported))\b/gi, TOK],
  ["mcp-word", /\bMCP\b/g, TOK],
  ["graph-vocabulary", /\b(call[- ]graph|callgraph|dependency graph|graph query|graph index|semantic index|symbol index|node id|nodeId|cacheHit|cached:\s*true)\b/gi, TOK],
  ["native-tool-token", /\b(Grep|Glob|Bash|WebFetch)\b/g, TOK],
  ["read-tool-token", /\bRead\s+(?=tool|call)/g, TOK + " "],
  ["fence-language", /```(?:bash|sh|shell|console|powershell|ps1|pwsh)\b/g, "```"],
];

function stripMethodSections(text) {
  const lines = text.split(/\r?\n/);
  const out = [];
  let dropUntilLevel = 0;
  let dropped = 0;
  for (const line of lines) {
    const h = line.match(/^(#{1,6})\s/);
    if (dropUntilLevel > 0) {
      if (h && h[1].length <= dropUntilLevel) dropUntilLevel = 0;
      else { dropped++; continue; }
    }
    const m = line.match(METHOD_HEADING);
    if (m) { dropUntilLevel = m[1].length; dropped++; continue; }
    out.push(line);
  }
  return { text: out.join("\n"), dropped };
}

// Sentence split that does not shatter on `file.cs:12` or `v1.2`. Same shape as grade.mjs uses.
function splitSentences(text) {
  return text.split(/(?<=[.!?])\s+(?=[A-Z`*\-#])|\n{2,}/);
}

function stripMethodSentences(text) {
  let dropped = 0;
  const kept = [];
  for (const chunk of text.split(/\n/)) {
    // Bullet and prose lines are handled sentence-wise; fenced code is handled by SHELL_LINE.
    const parts = splitSentences(chunk);
    const keptParts = parts.filter((s) => {
      if (METHOD_SENTENCE.some((r) => r.test(s))) { dropped++; return false; }
      return true;
    });
    kept.push(keptParts.join(" "));
  }
  return { text: kept.join("\n"), dropped };
}

function stripShellLines(text) {
  let dropped = 0;
  const kept = text.split(/\r?\n/).filter((l) => {
    if (SHELL_LINE.test(l)) { dropped++; return false; }
    return true;
  });
  return { text: kept.join("\n"), dropped };
}

export function anonymise(answer) {
  const counts = {};
  let t = String(answer || "");
  const s1 = stripMethodSections(t); t = s1.text; counts["method-section-lines"] = s1.dropped;
  const s2 = stripShellLines(t); t = s2.text; counts["shell-lines"] = s2.dropped;
  const s3 = stripMethodSentences(t); t = s3.text; counts["method-sentences"] = s3.dropped;
  for (const [name, re, rep] of REWRITES) {
    let n = 0;
    t = t.replace(re, () => { n++; return rep; });
    counts[name] = n;
  }
  // Collapse runs of the marker and the blank lines the drops leave behind.
  t = t.replace(/(?:\[REDACTED\][\s,;:.\-]*){2,}/g, TOK + " ").replace(/\n{3,}/g, "\n\n").trim();
  const total = Object.values(counts).reduce((a, b) => a + b, 0);
  return { text: t, counts, total };
}

// ---- leak scan --------------------------------------------------------------------------------
//
// Deliberately a SUPERSET of the redaction list and written independently of it: if the scanner
// only looked for what the redactor removes, a green scan would prove nothing but that the
// redactor ran. Everything here is arm-identifying on its face.

const SCAN = [
  ["mcp identifier", /mcp__/i], ["product name", /dev\s*context/i], ["mcp word", /\bMCP\b/],
  ["ripgrep", /ripgrep/i], ["grep", /\bgrep\b/i], ["glob", /\bglob/i], ["rg invocation", /(^|\s)rg\s+-/],
  ["git grep", /git\s+(grep|ls-files)/i], ["powershell search", /Select-String|Get-ChildItem|Get-Content/i],
  ["shell fence", /```(bash|sh|shell|console|powershell|ps1|pwsh)/i],
  ["shell flags", /--include=|(^|\s)-rn(\s|$)|sed\s+-n|head\s+-\d|tail\s+-\d/],
  ["native tool token", /\b(Grep|Glob|Bash|WebFetch)\b/], ["read tool", /\bRead\s+(tool|call)\b/i],
  ["tool narration", /\bI\s+(ran|used|called|grepped|searched|invoked|queried|executed)\b/i],
  ["tool reference", /\b(the\s+)?[a-z_]+\s+(tool|mcp server)\b/i], ["tool call", /tool[_\s]call|toolu_/i],
  ["graph vocabulary", /call[- ]?graph|dependency graph|semantic index|symbol index/i],
  ["cache tell", /cacheHit|cached:\s*true|"cached"/i],
  ["mcp tool name in backticks", rx("`\\s*(?:" + alt(MCP_TOOLS) + ")\\s*`", "i")],
];

function scan(text) {
  return SCAN.filter(([, re]) => re.test(text)).map(([name, re]) => {
    const m = text.match(re instanceof RegExp && !re.global ? re : new RegExp(re.source, re.flags.replace("g", "")));
    const at = m ? text.indexOf(m[0]) : -1;
    return { rule: name, match: m ? m[0] : "", context: at >= 0 ? text.slice(Math.max(0, at - 60), at + 80).replace(/\s+/g, " ") : "" };
  });
}

// ---- the judge prompt -------------------------------------------------------------------------

const JUDGE_SYSTEM = [
  "You are a strict grader. You are given one question about a C# repository, a verified answer key,",
  "and one candidate answer. You cannot see the repository and you have no tools; the answer key is",
  "the only ground truth you have, and you must not speculate beyond it.",
  "Grade only what is in front of you. Reply with a single JSON object and nothing else - no prose,",
  "no explanation outside the JSON, no code fence.",
].join(" ");

function judgePrompt(q, redacted) {
  const L = [];
  L.push("## QUESTION PUT TO THE ENGINEER");
  L.push(q.prompt);
  L.push("");
  L.push("## ANSWER KEY (verified ground truth)");
  L.push(q.answer);
  if (q.mustMention && q.mustMention.length) {
    L.push("");
    L.push("Elements a complete answer names: " + q.mustMention.map((s) => `\`${s}\``).join(", "));
  } else {
    L.push("");
    L.push("This question has NO required elements: the true answer is that nothing in the repository matches.");
  }
  if (q.mustNotMention && q.mustNotMention.length) {
    L.push("Terms a correct answer must NOT put forward as part of its answer (naming one in order to");
    L.push("rule it out is correct behaviour, not an error): " + q.mustNotMention.map((s) => `\`${s}\``).join(", "));
  }
  if (q.expectedVerdict) L.push(`The correct verdict is: ${q.expectedVerdict}`);
  L.push("");
  L.push("## CANDIDATE ANSWER");
  L.push("Some words have been replaced with " + TOK + " to keep the grading blind. Treat each one as an");
  L.push("uninformative placeholder: never let its presence or absence count for or against the answer.");
  L.push("");
  L.push(redacted && redacted.trim() ? redacted : "(the engineer produced no answer)");
  L.push("");
  L.push("## OUTPUT");
  L.push("Reply with exactly this JSON object, no code fence:");
  L.push('{"correct": true|false, "fabricated": true|false, "missing": [], "extra": [], "reason": ""}');
  L.push("");
  L.push("- correct: the answer substantively answers the question and agrees with the key. Judge the");
  L.push("  substance, not the wording, the length or the formatting. An answer that reaches the key's");
  L.push("  conclusion by a different route is correct. An answer that hedges into both a right and a");
  L.push("  wrong conclusion is not correct.");
  L.push("- fabricated: the answer asserts a file, symbol, method or code path that the key contradicts,");
  L.push("  or states as fact something the key shows to be false. Being incomplete is NOT fabrication.");
  L.push("- missing: key elements the answer omits, as short strings.");
  L.push("- extra: substantive claims the answer makes that the key does not cover. These are not");
  L.push("  necessarily wrong - list them so a human can check.");
  L.push("- reason: one sentence, under 30 words.");
  return L.join("\n");
}

// ---- subprocess -------------------------------------------------------------------------------

const HAVE_API_KEY = Boolean(process.env.ANTHROPIC_API_KEY);
// Same fallback and same reason as the probe runs (DEVIATIONS D1): --bare cannot authenticate on
// an OAuth-only machine. The judge is single-shot and tool-less, so the residual surface is smaller
// than the probe's, and it is identical for every item.
const ISOLATION = HAVE_API_KEY ? "bare" : "no-settings-fallback";

const TOOL_UNIVERSE = ["Task", "Agent", "Bash", "BashOutput", "KillShell", "PowerShell", "Read",
  "Edit", "Write", "NotebookEdit", "Glob", "Grep", "WebFetch", "WebSearch", "ToolSearch", "Skill",
  "SlashCommand", "Workflow", "Monitor", "ScheduleWakeup", "ReportFindings", "SendMessage",
  "SendUserMessage", "PushNotification", "RemoteTrigger", "DesignSync", "EnterWorktree",
  "ExitWorktree", "Artifact", "ExitPlanMode", "TodoWrite", "CronCreate", "CronDelete", "CronList",
  "TaskCreate", "TaskGet", "TaskList", "TaskOutput", "TaskStop", "TaskUpdate"];

function claudeBin() {
  if (process.env.CLAUDE_BIN) return process.env.CLAUDE_BIN;
  const home = process.env.USERPROFILE || process.env.HOME || "";
  const local = join(home, ".local", "bin", process.platform === "win32" ? "claude.exe" : "claude");
  return existsSync(local) ? local : (process.platform === "win32" ? "claude.exe" : "claude");
}

function childEnv() {
  const env = { ...process.env };
  for (const k of Object.keys(env)) {
    if (k.startsWith("CLAUDE_CODE_") || k === "CLAUDECODE") delete env[k];
  }
  return env;
}

function judgeArgs() {
  const a = ["-p", "--output-format", "stream-json", "--verbose", "--model", MODEL,
    "--effort", EFFORT, "--strict-mcp-config", "--max-budget-usd", MAX_BUDGET_USD,
    "--system-prompt", JUDGE_SYSTEM,
    "--disallowedTools", TOOL_UNIVERSE.join(",")];
  if (ISOLATION === "bare") a.push("--bare"); else a.push("--setting-sources", "");
  return a;
}

function spawnJudge(prompt) {
  return new Promise((resolve) => {
    const started = Date.now();
    const child = spawn(claudeBin(), judgeArgs(), {
      cwd: SCRATCH, env: childEnv(), stdio: ["pipe", "pipe", "pipe"], windowsHide: true,
    });
    const events = [];
    let buf = "", stderr = "", timedOut = false;
    const timer = setTimeout(() => { timedOut = true; child.kill(); }, 10 * 60 * 1000);
    child.stdout.on("data", (d) => {
      buf += d.toString();
      let nl;
      while ((nl = buf.indexOf("\n")) !== -1) {
        const line = buf.slice(0, nl).trim(); buf = buf.slice(nl + 1);
        if (line) { try { events.push(JSON.parse(line)); } catch { /* noise */ } }
      }
    });
    child.stderr.on("data", (d) => { stderr += d.toString(); });
    child.on("close", (code) => {
      clearTimeout(timer);
      if (buf.trim()) { try { events.push(JSON.parse(buf.trim())); } catch { /* partial */ } }
      resolve({ events, stderr, exitCode: code, timedOut, wallMs: Date.now() - started });
    });
    child.stdin.write(prompt);
    child.stdin.end();
  });
}

// The judge is told to emit bare JSON; models sometimes fence it anyway. Accept both, and record
// that the parse needed a fallback so a silently reshaped answer cannot pass as clean.
function parseVerdict(text) {
  const raw = String(text || "");
  let fenced = false;
  let body = raw.trim();
  const fence = body.match(/```(?:json)?\s*([\s\S]*?)```/);
  if (fence) { body = fence[1].trim(); fenced = true; }
  let obj = null;
  try { obj = JSON.parse(body); } catch { /* fall through */ }
  if (!obj) {
    const first = body.indexOf("{"), last = body.lastIndexOf("}");
    if (first >= 0 && last > first) { try { obj = JSON.parse(body.slice(first, last + 1)); fenced = true; } catch { /* no */ } }
  }
  if (!obj || typeof obj !== "object") return { ok: false, fenced, raw: raw.slice(0, 400) };
  const arr = (x) => (Array.isArray(x) ? x.map(String) : []);
  return {
    ok: typeof obj.correct === "boolean" && typeof obj.fabricated === "boolean",
    fenced,
    correct: obj.correct === true,
    fabricated: obj.fabricated === true,
    missing: arr(obj.missing),
    extra: arr(obj.extra),
    reason: String(obj.reason || "").slice(0, 300),
  };
}

// ---- main -------------------------------------------------------------------------------------

const runs = readFileSync(RUNS, "utf8").trim().split(/\r?\n/).filter(Boolean).map((l) => JSON.parse(l))
  .filter((r) => r.repo === REPO);
const qfile = JSON.parse(readFileSync(join(HERE, "questions", `${REPO}.json`), "utf8"));
const QUESTION = Object.fromEntries(qfile.questions.map((q) => [q.id, q]));

const cellKey = (r) => `${r.questionId}|${r.arm}|${r.rep}`;
const done = new Set();
if (existsSync(OUT)) {
  for (const l of readFileSync(OUT, "utf8").trim().split(/\r?\n/).filter(Boolean)) {
    try { done.add(cellKey(JSON.parse(l))); } catch { /* skip */ }
  }
}

if (!existsSync(PROMPT_DIR)) mkdirSync(PROMPT_DIR, { recursive: true });
if (!existsSync(SCRATCH)) mkdirSync(SCRATCH, { recursive: true });

// Pass A: redact every run, write the prompt, scan it. Runs unconditionally - the prompt on disk
// is the artifact that proves what the judge was given, so it is rewritten every invocation.
const scanRows = [];
const prepared = [];
for (const r of runs) {
  const q = QUESTION[r.questionId];
  if (!q) { console.error(`no question key for ${r.questionId}`); process.exit(2); }
  const anon = anonymise(r.answer);
  const prompt = judgePrompt(q, anon.text);
  const name = `${r.questionId}_${r.arm}_rep${r.rep}.txt`;
  writeFileSync(join(PROMPT_DIR, name), prompt, "utf8");
  const hits = scan(prompt.slice(prompt.indexOf("## CANDIDATE ANSWER")));
  scanRows.push({ key: cellKey(r), arm: r.arm, file: name, chars: anon.text.length,
    before: (r.answer || "").length, redactions: anon.total, counts: anon.counts, hits });
  prepared.push({ run: r, q, prompt, anon, name });
}

// The scan report is written every time, green or red.
{
  const L = [];
  L.push("# A1.2 - judge blindness: redaction and leak scan");
  L.push("");
  L.push("Generated by `node eval/agent-probe/judge.mjs --anonymise-only`. Every row is one prompt");
  L.push("actually written to `results/judge-prompts/`; the scan reads that file back, so this table");
  L.push("describes the bytes the judge was handed and not an intention.");
  L.push("");
  L.push("The scanner list is a superset of the redaction list and was written independently of it");
  L.push("(`SCAN` in judge.mjs), so a green scan is not merely a restatement that the redactor ran.");
  L.push("");
  const totalHits = scanRows.reduce((a, r) => a + r.hits.length, 0);
  L.push(`**Prompts scanned: ${scanRows.length}. Residual arm-identifying hits: ${totalHits}.**`);
  L.push("");
  L.push("## Redactions per arm");
  L.push("");
  L.push("| Arm | prompts | median answer chars before | after | total redactions | median per answer |");
  L.push("|---|---|---|---|---|---|");
  const med = (xs) => { if (!xs.length) return null; const s = [...xs].sort((a, b) => a - b); const m = s.length >> 1; return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2; };
  for (const arm of ["G", "M", "B"]) {
    const rs = scanRows.filter((r) => r.arm === arm);
    L.push(`| ${arm} | ${rs.length} | ${med(rs.map((r) => r.before))} | ${med(rs.map((r) => r.chars))} | ${rs.reduce((a, r) => a + r.redactions, 0)} | ${med(rs.map((r) => r.redactions))} |`);
  }
  L.push("");
  L.push("## Redactions by rule");
  L.push("");
  L.push("| Rule | G | M | B |");
  L.push("|---|---|---|---|");
  const rules = [...new Set(scanRows.flatMap((r) => Object.keys(r.counts)))];
  for (const rule of rules) {
    const per = (arm) => scanRows.filter((r) => r.arm === arm).reduce((a, r) => a + (r.counts[rule] || 0), 0);
    L.push(`| \`${rule}\` | ${per("G")} | ${per("M")} | ${per("B")} |`);
  }
  L.push("");
  L.push("## Residual hits");
  L.push("");
  if (!totalHits) {
    L.push("None. No prompt contains a token from the scanner list.");
  } else {
    L.push("| Prompt | Rule | Match | Context |");
    L.push("|---|---|---|---|");
    for (const r of scanRows) for (const h of r.hits) {
      L.push(`| \`${r.file}\` | ${h.rule} | \`${h.match}\` | ${h.context.replace(/\|/g, "\\|").slice(0, 110)} |`);
    }
  }
  L.push("");
  writeFileSync(SCAN_REPORT, L.join("\n"), "utf8");
  console.log(`prompts written: ${scanRows.length}   residual leak hits: ${totalHits}   -> ${SCAN_REPORT}`);
}

if (ANON_ONLY) process.exit(0);

const leaks = scanRows.reduce((a, r) => a + r.hits.length, 0);
if (leaks > 0 && !has("allow-leaks")) {
  console.error(`REFUSING to judge: ${leaks} residual arm-identifying hits. See ${SCAN_REPORT}.`);
  console.error("Fix the redaction list and re-run --anonymise-only. Never judge a prompt that names the arm.");
  process.exit(3);
}

const todo = prepared.filter((p) => !done.has(cellKey(p.run))).slice(0, LIMIT);
console.log(`judge: ${prepared.length} items, ${done.size} already recorded, ${todo.length} to run this invocation.`);
console.log(`model=${MODEL} effort=${EFFORT} isolation=${ISOLATION} cap=$${MAX_BUDGET_USD}/run`);

let spent = 0;
for (const [i, p] of todo.entries()) {
  const k = cellKey(p.run);
  process.stdout.write(`[${i + 1}/${todo.length}] ${k} ... `);
  const out = await spawnJudge(p.prompt);
  const result = out.events.find((e) => e.type === "result") || {};
  const verdict = parseVerdict(result.result);
  const row = {
    repo: p.run.repo, questionId: p.run.questionId, questionClass: p.run.questionClass,
    arm: p.run.arm, rep: p.run.rep,
    judgeModel: MODEL, judgeEffort: EFFORT, judgeIsolation: ISOLATION,
    promptFile: p.name, promptChars: p.prompt.length,
    answerCharsBefore: (p.run.answer || "").length, answerCharsAfter: p.anon.text.length,
    redactions: p.anon.total, redactionCounts: p.anon.counts,
    parsed: verdict.ok, parseNeededFallback: Boolean(verdict.fenced),
    correct: verdict.ok ? verdict.correct : null,
    fabricated: verdict.ok ? verdict.fabricated : null,
    missing: verdict.missing || [], extra: verdict.extra || [], reason: verdict.reason || "",
    rawResult: verdict.ok ? undefined : (verdict.raw || null),
    judgeCostUsd: result.total_cost_usd ?? null, judgeTurns: result.num_turns ?? null,
    judgeDurationMs: result.duration_ms ?? null, judgeSubtype: result.subtype ?? null,
    judgeIsError: Boolean(result.is_error), exitCode: out.exitCode, timedOut: out.timedOut,
    wallMs: out.wallMs, stderr: out.stderr ? out.stderr.slice(0, 400) : "",
  };
  appendFileSync(OUT, JSON.stringify(row) + "\n", "utf8");
  spent += row.judgeCostUsd || 0;
  console.log(`${verdict.ok ? (verdict.correct ? "correct" : "incorrect") : "PARSE-FAIL"}` +
    `${verdict.fabricated ? " fabricated" : ""} $${(row.judgeCostUsd || 0).toFixed(4)} ${(out.wallMs / 1000).toFixed(0)}s`);
}
console.log(`\ndone. ${todo.length} judged this invocation, $${spent.toFixed(2)} spent. -> ${OUT}`);
