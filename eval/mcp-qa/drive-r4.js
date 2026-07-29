// R4 (G1) evidence driver — real MCP calls, raw responses dumped to eval-results.
// Usage: node eval/mcp-qa/drive-r4.js <case> [outDir]
// Cases: map-library | map-multisln | getctx-library | glyphs | retarget | envelope
//        | find-kind | analyze-honesty
// Each case prints PASS/FAIL lines and writes the RAW tool responses (the evidence a
// checkpoint claim needs — a real MCP call showing the response shape, not a diff).

const { spawn } = require("child_process");
const { join, resolve } = require("path");
const { createInterface } = require("readline");
const { existsSync, mkdirSync, writeFileSync } = require("fs");

const CASE = process.argv[2];
const OUT_DIR = resolve(process.argv[3] ?? join(__dirname, "..", "..", "eval-results", new Date().toISOString().slice(0, 10), "mcp-r4"));
const REPOS = join(__dirname, "..", "..", "eval-repos");

const MCP_EXE = join(__dirname, "..", "..", "src", "DevContext.Mcp", "bin", "Debug", "net10.0", "devcontext-mcp.exe");

// ---- JSON-RPC over stdio (same transport as run.js) ----
function mcpClient(exePath) {
  const proc = spawn(exePath, [], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true });
  const rl = createInterface({ input: proc.stdout, crlfDelay: Infinity });
  let nextId = 1;
  const pending = new Map();
  rl.on("line", (line) => {
    try {
      const msg = JSON.parse(line);
      if (msg.id !== undefined && pending.has(msg.id)) {
        const waiter = pending.get(msg.id);
        // Clear the timer. An uncleared one holds node's event loop open until it fires, and
        // analyzeRepo's is TEN MINUTES: the retarget case finished its last check and then sat
        // there for the rest of the timer, so the glyphs case behind it never started. Cost this
        // session two runs before the cause was obvious.
        clearTimeout(waiter.timer);
        waiter.resolve(msg);
        pending.delete(msg.id);
      }
    } catch (_) { /* non-JSON line */ }
  });
  proc.stderr.resume();
  function call(method, params = {}, timeoutMs = 45000) {
    return new Promise((resolvep, reject) => {
      const id = nextId++;
      const timer = setTimeout(() => {
        if (pending.has(id)) { pending.delete(id); reject(new Error(`Timeout: ${method}`)); }
      }, timeoutMs);
      pending.set(id, { resolve: resolvep, timer });
      proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", id, method, params }) + "\n");
    });
  }
  function notify(method, params = {}) {
    proc.stdin.write(JSON.stringify({ jsonrpc: "2.0", method, params }) + "\n");
  }
  return { call, notify, close: () => { rl.close(); proc.kill(); } };
}

async function bootstrap(client) {
  // Own budget: the handshake waits for the MCP to cold-start a gRPC server behind it (see run.js).
  const init = await client.call("initialize", {
    protocolVersion: "2024-11-05", capabilities: {},
    clientInfo: { name: "drive-r4", version: "0.0.1" },
  }, 180000);
  if (init.error) throw new Error(`init failed: ${JSON.stringify(init.error)}`);
  client.notify("notifications/initialized", {});
}

function extract(result) {
  const texts = (result?.content ?? []).filter((c) => c.type === "text").map((c) => c.text).join("\n");
  try { return JSON.parse(texts); } catch { return { text: texts }; }
}

// Raw tool call: returns {data, raw} — raw includes protocol-level error shape if any.
async function tool(client, name, args, timeoutMs = 45000) {
  const resp = await client.call("tools/call", { name, arguments: args }, timeoutMs);
  if (resp.error) return { data: null, raw: { protocolError: resp.error } };
  return { data: extract(resp.result), raw: resp.result };
}

// analyze with the long-poll workaround from run.js (analysis can take minutes cold)
async function analyzeRepo(client, repoPath, extra = {}) {
  const p = tool(client, "analyze", { path: repoPath, ...extra }, 600000);
  const started = Date.now();
  const r = await p;
  const elapsedMs = Date.now() - started;
  const handle = r.data?.handle ?? null;
  return { handle, elapsedMs, response: r.data };
}

function dump(name, obj) {
  if (!existsSync(OUT_DIR)) mkdirSync(OUT_DIR, { recursive: true });
  const p = join(OUT_DIR, name);
  writeFileSync(p, JSON.stringify(obj, null, 2), "utf8");
  console.log(`  wrote ${p}`);
}

function check(label, ok, detail = "") {
  console.log(`  ${ok ? "PASS" : "FAIL"}  ${label}${detail ? " — " + detail : ""}`);
  if (!ok) process.exitCode = 1;
}

// ---- cases ----

const CASES = {
  // G1.1 — map returns the structured surface; markdown advertises no CLI flags.
  async "map-library"(client) {
    const { handle, elapsedMs } = await analyzeRepo(client, join(REPOS, "FluentValidation"));
    check("analyze FluentValidation", !!handle, `${(elapsedMs / 1000).toFixed(1)}s`);
    if (!handle) return;
    const { data } = await tool(client, "map", { handle });
    dump("map-library-fluentvalidation.json", data);
    check("isLibrary", data.isLibrary === true, String(data.isLibrary));
    check("structured surface present", !!data.surface, data.surface ? `entryApi=${data.surface.entryApi?.length ?? 0} abstractions=${data.surface.abstractions?.length ?? 0} groups=${data.surface.groups?.length ?? 0}` : "surface missing");
    check("packages structured", Array.isArray(data.packages) && data.packages.length > 0, `${data.packages?.length ?? 0} groups`);
    check("markdown present", typeof data.markdown === "string" && data.markdown.length > 500, `${data.markdown?.length ?? 0} chars`);
    check("markdown has no CLI flags", !/--focus|--sln|--depth|--max-tokens/.test(data.markdown), (data.markdown.match(/--\w+/g) ?? []).join(",") || "clean");
  },

  async "map-multisln"(client) {
    const { handle, elapsedMs } = await analyzeRepo(client, join(REPOS, "GitVersion"));
    check("analyze GitVersion", !!handle, `${(elapsedMs / 1000).toFixed(1)}s`);
    if (!handle) return;
    const { data } = await tool(client, "map", { handle });
    dump("map-multisln-gitversion.json", data);
    check("solutionScope facts present", !!data.solutionScope && data.solutionScope.totalOnDisk > 1,
      data.solutionScope ? `${data.solutionScope.analyzedRelPath} 1-of-${data.solutionScope.totalOnDisk}` : "missing");
    check("markdown has no CLI flags", !/--focus|--sln/.test(data.markdown), (data.markdown.match(/--\w+/g) ?? []).join(",") || "clean");
    const scopeLine = (data.markdown.split("\n").find((l) => l.startsWith("SCOPE")) ?? "");
    console.log(`  scope line: ${scopeLine.trim()}`);
  },

  // G1.2 — does a type/symbol focus root a pack on a library? Before the fix: a TYPE resolved
  // (AbstractValidator 43% fill) but a bare MEMBER did not (RuleFor → "No context could be built"),
  // and no pack carried the inbound direction (InlineValidator 8% fill, one-line trace).
  async "getctx-library"(client) {
    const { handle } = await analyzeRepo(client, join(REPOS, "FluentValidation"));
    check("analyze FluentValidation", !!handle);
    if (!handle) return;
    // IValidator is the pole case: 9 in-edges, 0 out-edges — a trace-shaped pack sees nothing.
    for (const focus of ["AbstractValidator", "InlineValidator", "IValidator", "RuleFor"]) {
      const { data } = await tool(client, "get_context", { handle, focus, budgetTokens: 4000 }, 90000);
      dump(`getctx-library-${focus}.json`, data);
      const sections = (data.sections ?? []).map((s) => s.key);
      const substantive = sections.some((k) => k !== "identity");
      const fill = data.totalTokens ? Math.round((data.totalTokens / 4000) * 100) : 0;
      console.log(`  focus=${focus}: found=${data.error ? "ENVELOPE" : "yes"} sections=[${sections.join(",")}] tokens=${data.totalTokens ?? "-"} fill=${fill}% ${data.error ? "error=" + data.error : ""}`);
      check(`pack for ${focus} is substantive`, substantive, sections.join(",") || "envelope only");
      if (!substantive) continue;
      check(`pack for ${focus} names the symbol it rooted on`,
        /Rooted on symbol: (Type|Member):/.test(data.content ?? ""),
        ((data.content ?? "").match(/Rooted on symbol:.*/) ?? ["(absent)"])[0].slice(0, 90));
    }
    // The inbound half: these two have real in-edges, so they must carry a `usage` section.
    for (const focus of ["InlineValidator", "IValidator"]) {
      const { data } = await tool(client, "get_context", { handle, focus, budgetTokens: 4000 }, 90000);
      const usage = (data.sections ?? []).find((s) => s.key === "usage");
      const rows = ((data.content ?? "").match(/^- `.*(calls it|resolves to it|references it)/gm) ?? []).length;
      check(`${focus} pack carries the inbound direction (usage section)`, !!usage, usage ? `${usage.tokens} tok · ${rows} rows` : "no usage section");
    }
  },

  // G1.2 CANARY — an entry-rooted pack must be UNMOVED: no usage section, no symbol header.
  async "getctx-entry-canary"(client) {
    const { handle } = await analyzeRepo(client, join(REPOS, "TodoApi"));
    check("analyze TodoApi", !!handle);
    if (!handle) return;
    // The focus must be a REAL declared entry, taken from the tool that lists them — a focus that
    // fails to resolve returns an envelope with no sections, and "no usage section" on an empty pack
    // is a vacuous pass. The first run of this canary did exactly that; hence the hard check below.
    const eps = (await tool(client, "entrypoints", { handle, limit: 20 })).data;
    const flat = [];
    for (const v of Object.values(eps ?? {})) {
      if (Array.isArray(v)) flat.push(...v);
      else if (v && typeof v === "object") for (const vv of Object.values(v)) if (Array.isArray(vv)) flat.push(...vv);
    }
    const focus = flat.map((e) => e?.title ?? e?.route).find((t) => typeof t === "string" && t.length > 0);
    check("found a declared entry to root the canary on", !!focus, focus ?? JSON.stringify(eps).slice(0, 160));
    if (!focus) return;
    const { data } = await tool(client, "get_context", { handle, focus, budgetTokens: 4000 }, 90000);
    dump("getctx-entry-canary-todoapi.json", data);
    const sections = (data.sections ?? []).map((s) => s.key);
    console.log(`  entry focus=${focus}: sections=[${sections.join(",")}] tokens=${data.totalTokens ?? "-"}`);
    // Guard against a vacuous pass: an empty pack proves nothing about the canary.
    check("entry pack is a real pack (not an envelope)", sections.length > 1, sections.join(",") || `ENVELOPE: ${data.error ?? "?"}`);
    check("entry pack has NO usage section", !sections.includes("usage"), sections.join(","));
    check("entry pack has NO symbol header", !/Rooted on symbol/.test(data.content ?? ""), "clean");
    check("entry pack omitted[] has no usage line", !(data.omitted ?? []).some((o) => o.startsWith("usage")), (data.omitted ?? []).join(" | ").slice(0, 120));
  },

  // G1.2 probe — what does the graph actually HOLD around a library symbol? (measure, don't assume)
  async "symbol-probe"(client) {
    const { handle } = await analyzeRepo(client, join(REPOS, "FluentValidation"));
    check("analyze FluentValidation", !!handle);
    if (!handle) return;
    const out = {};
    for (const name of ["InlineValidator", "AbstractValidator", "RuleFor", "IValidator"]) {
      const resolved = (await tool(client, "resolve", { handle, query: name })).data;
      const cands = resolved?.candidates ?? resolved?.results ?? [];
      const nodeId = cands[0]?.nodeId ?? cands[0]?.id ?? resolved?.nodeId ?? null;
      const nb = nodeId ? (await tool(client, "neighbors", { handle, nodeId })).data : null;
      const us = nodeId ? (await tool(client, "usages", { handle, nodeId })).data : null;
      out[name] = { resolved, neighbors: nb, usages: us };
      console.log(`  ${name}: nodeId=${nodeId} candidates=${cands.length} nbKeys=${nb ? Object.keys(nb).join("/") : "-"} usKeys=${us ? Object.keys(us).join("/") : "-"}`);
    }
    dump("symbol-probe-fluentvalidation.json", out);
  },

  // G1.3 — seam glyphs vs proto singular names (needs a repo with bus seams: eShop).
  async glyphs(client) {
    const { handle } = await analyzeRepo(client, join(REPOS, "eShop"));
    check("analyze eShop", !!handle);
    if (!handle) return;
    const args = { handle, focus: "POST /api/orders/", budgetTokens: 3000 };
    const { data } = await tool(client, "trace", { ...args, format: "compact" }, 90000);
    dump("glyphs-trace-eshop.json", data);
    // The same trace, structured — this is where the seam NAMES live, so the compact render can be
    // checked against the vocabulary the repo actually produced instead of against a guess.
    const { data: tree } = await tool(client, "trace", args, 90000);
    dump("glyphs-trace-eshop-tree.json", tree);
    const seams = new Set();
    (function walk(n) { if (!n) return; if (n.seam) seams.add(n.seam); (n.children ?? []).forEach(walk); })(tree.root);

    const text = data.text ?? "";
    const fallbacks = (text.match(/^\s*·/gm) ?? []).length;
    const glyphs = [...new Set([...text.matchAll(/^\s*([▼→⇒⬆↓◉⇛≡◇∥·])/gmu)].map((m) => m[1]))];
    console.log(`  seams in the tree: ${[...seams].sort().join(" ")}`);
    console.log(`  glyphs used: ${glyphs.join(" ")} · fallback rows: ${fallbacks}`);
    console.log(`  legend: ${data.legend ?? "(none)"}`);
    // BEFORE (2026-07-29, mcp-r4-g13-before/glyphs-trace-eshop.json): "▼ → · ⇛", 3 fallback rows —
    // and the two that mattered were the MediatR dispatch and its handler at the top of eShop's
    // order flow, so the spine this program cites most read as two anonymous dots. "at least one
    // non-Call glyph" was too weak a bar to catch that: ⇛ alone satisfied it. The bar is zero.
    check("no seam renders the mute fallback", fallbacks === 0,
      `${fallbacks} mute row(s) · glyphs ${glyphs.join(" ")} · seams ${[...seams].sort().join(",")}`);
    check("the trace exercised more than one seam kind", seams.size > 1, [...seams].join(","));
    check("a legend keys the glyphs that were used", typeof data.legend === "string" && data.legend.length > 0,
      data.legend ?? "(none)");
  },

  // G1.3 — handle-less calls must not retarget to a session another client touched.
  async retarget(client) {
    const a = await analyzeRepo(client, join(REPOS, "TodoApi"));
    const b = await analyzeRepo(client, join(REPOS, "FluentValidation"));
    check("two sessions", !!a.handle && !!b.handle, `A=${a.handle?.slice(0, 8)} B=${b.handle?.slice(0, 8)}`);
    if (!a.handle || !b.handle) return;
    // Another client touches A (any explicit-handle call bumps the server's LastAccess).
    await tool(client, "stats", { handle: a.handle });
    // Agent's handle-less call: must go to B (the repo THIS client last analyzed), not A.
    const { data } = await tool(client, "overview", {});
    dump("retarget-overview.json", data);
    const wentTo = (data.handle ?? "").slice(0, 8);
    check("handle-less call sticks to last-analyzed", data.handle === b.handle,
      `went to ${wentTo}, expected B=${b.handle.slice(0, 8)} (A=${a.handle.slice(0, 8)})`);
  },

  // G1.3 — no raw RpcException may leak past the error envelope.
  async envelope(client) {
    const bogus = "no-such-handle-1234";
    const results = {};
    for (const [name, args] of [
      ["status", { handle: bogus }],
      ["entrypoints", { handle: bogus }],
      ["insights", { handle: bogus }],
      ["close_session", { handle: bogus }],
      ["map", { handle: bogus }],
      ["top_flows", { handle: bogus }],
      ["interesting_points", { handle: bogus }],
    ]) {
      const { data, raw } = await tool(client, name, args);
      results[name] = { data, isError: raw?.isError ?? false };
      const enveloped = data && (data.error !== undefined || data.found === false) && !raw?.isError
        && !JSON.stringify(data).includes("RpcException") && !JSON.stringify(data).includes("Grpc.Core");
      check(`${name}(bogus handle) returns envelope`, enveloped,
        raw?.isError ? "MCP isError (raw exception leaked)" : JSON.stringify(data)?.slice(0, 100));
    }
    dump("envelope-bogus-handle.json", results);
  },

  // G1.4 — find(kind:) server-side: total/hasMore must be true over the kind-filtered set.
  async "find-kind"(client) {
    const { handle } = await analyzeRepo(client, join(REPOS, "eShop"));
    check("analyze eShop", !!handle);
    if (!handle) return;
    const all = (await tool(client, "find", { handle, query: "Order", limit: 100 })).data;
    const typed = (await tool(client, "find", { handle, query: "Order", kind: "Type", limit: 5 })).data;
    dump("find-kind-eshop.json", { all, typed });
    const everyTyped = (typed.results ?? []).every((r) => r.kind === "Type");
    check("kind filter applied", everyTyped, (typed.results ?? []).map((r) => r.kind).join(","));
    check("total is the kind-filtered total (not the page)", typeof typed.total === "number" && typed.total >= (typed.results?.length ?? 0), `total=${typed.total} page=${typed.results?.length}`);
    console.log(`  all-kind total=${all.total} · Type total=${typed.total} · hasMore=${typed.hasMore}`);
  },

  // G1.4 — analyze honesty: cached flag + long-run note + summary in the envelope.
  async "analyze-honesty"(client) {
    const cold = await analyzeRepo(client, join(REPOS, "TodoApi"));
    dump("analyze-honesty-first.json", cold.response);
    const again = await analyzeRepo(client, join(REPOS, "TodoApi"));
    dump("analyze-honesty-again.json", again.response);
    check("summary returned", !!cold.response?.summary, JSON.stringify(cold.response?.summary)?.slice(0, 120));
    check("re-analyze reports cached", again.response?.cached === true, `cached=${again.response?.cached} in ${(again.elapsedMs / 1000).toFixed(1)}s`);
    check("note present", typeof cold.response?.note === "string" && cold.response.note.length > 10, cold.response?.note?.slice(0, 80));
  },
};

async function main() {
  if (!CASE || !CASES[CASE]) {
    console.error(`Usage: node eval/mcp-qa/drive-r4.js <${Object.keys(CASES).join("|")}>`);
    process.exit(2);
  }
  if (!existsSync(MCP_EXE)) {
    console.error(`MCP binary missing: ${MCP_EXE} — dotnet build src/DevContext.Mcp first`);
    process.exit(2);
  }
  const client = mcpClient(MCP_EXE);
  try {
    await bootstrap(client);
    console.log(`case: ${CASE}`);
    await CASES[CASE](client);
  } finally {
    client.close();
  }
}

main().catch((e) => { console.error("FATAL:", e.message); process.exit(1); });
