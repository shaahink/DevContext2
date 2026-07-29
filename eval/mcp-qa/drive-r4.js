// R4 (G1) evidence driver — real MCP calls, raw responses dumped to eval-results.
// Usage: node eval/mcp-qa/drive-r4.js <case> [outDir]
// Cases: map-library | map-multisln | getctx-library | glyphs | retarget | envelope
//        | find-kind | analyze-honesty | menu | trace-budget
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
      ["stats", { handle: bogus }],
      ["close_session", { handle: bogus }],
      ["map", { handle: bogus }],
      ["top_flows", { handle: bogus }],
      ["overview", { handle: bogus }],
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
  //
  // The FIRST version of this case passed on the broken before-state, the same way the glyphs case
  // did: it asked "is total >= the page" (22 >= 5, true, and meaningless). What actually distinguishes
  // a true total from a windowed one is that a true total DOES NOT MOVE WITH THE PAGE SIZE. Before
  // the fix, find("Order", limit:100) reported total=120 — i.e. limit+20, the MCP's own fetch
  // window — and the kind-filtered total was "Types among the first 25 matches".
  async "find-kind"(client) {
    const { handle } = await analyzeRepo(client, join(REPOS, "eShop"));
    check("analyze eShop", !!handle);
    if (!handle) return;
    const find = async (args) => (await tool(client, "find", { handle, query: "Order", ...args })).data;

    const all = await find({ limit: 100 });          // the unfiltered canary — kept identical to the
    const allSmall = await find({ limit: 5 });       // before-state call so the pages can be diffed
    const typed = await find({ kind: "Type", limit: 5 });
    const typedAll = await find({ kind: "Type", limit: 1000 });
    const bogusKind = await find({ kind: "NoSuchKind", limit: 5 });
    const page2 = await find({ limit: 5, cursor: 5 });
    dump("find-kind-eshop.json", { all, allSmall, typed, typedAll, bogusKind, page2 });

    console.log(`  total@limit100=${all.total} · total@limit5=${allSmall.total} · Type total=${typed.total} · typedAll page=${typedAll.results?.length} · hasMore=${typed.hasMore}`);

    // 1. The invariant. Two page sizes, one repo, one query — one answer.
    check("total does not move with the page size", all.total === allSmall.total,
      `limit100 -> ${all.total} · limit5 -> ${allSmall.total}`);
    // 2. ...and it is not simply the fetch window in disguise (before: 100+20 === 120).
    check("total is not the fetch window", all.total !== 120 || allSmall.total !== 25,
      `${all.total} vs limit+20=120`);
    // 3. The kind filter runs above the truncation: its total counts every match of that kind.
    check("kind filter applied", (typed.results ?? []).every((r) => r.kind === "Type"),
      (typed.results ?? []).map((r) => r.kind).join(","));
    check("kind total is the whole kind-filtered set", typed.total === (typedAll.results?.length ?? -1),
      `total=${typed.total} · everything-in-one-page=${typedAll.results?.length}`);
    check("kind total does not move with the page size", typed.total === typedAll.total,
      `limit5 -> ${typed.total} · limit1000 -> ${typedAll.total}`);
    check("filtering can only narrow", typed.total <= all.total, `${typed.total} <= ${all.total}`);
    // 4. hasMore is derived from that total, on both sides of the boundary.
    check("hasMore true when the page is short of the total", typed.hasMore === (5 < typed.total),
      `hasMore=${typed.hasMore} page=5 total=${typed.total}`);
    check("hasMore false when one page holds everything", typedAll.hasMore === false,
      `hasMore=${typedAll.hasMore} of ${typedAll.total}`);
    // 5. Paging over the honest total returns different rows, not a re-served first page.
    const ids1 = (allSmall.results ?? []).map((r) => r.nodeId);
    const ids2 = (page2.results ?? []).map((r) => r.nodeId);
    check("cursor pages forward", ids2.length > 0 && ids2.every((id) => !ids1.includes(id)),
      `page1=${ids1.length} page2=${ids2.length} overlap=${ids2.filter((id) => ids1.includes(id)).length}`);
    // 6. An unknown kind is a true zero, not an error and not an unfiltered fallback.
    check("an unknown kind matches nothing", (bogusKind.results ?? []).length === 0 && !!bogusKind.error,
      `error=${bogusKind.error ?? "(none)"} results=${bogusKind.results?.length ?? 0}`);
  },

  // G1.4 — analyze honesty: cached flag + long-run note + summary in the envelope.
  // Before the fix analyze returned {handle,status,hint} and nothing else — the same six words
  // whether the call took 8 minutes or 0 ms, with the summary the server had already computed
  // thrown away at DevContextTools.cs:203.
  async "analyze-honesty"(client) {
    const cold = await analyzeRepo(client, join(REPOS, "TodoApi"));
    dump("analyze-honesty-first.json", cold.response);
    const again = await analyzeRepo(client, join(REPOS, "TodoApi"));
    dump("analyze-honesty-again.json", again.response);
    const s = cold.response?.summary;
    check("summary returned", !!s, JSON.stringify(s)?.slice(0, 140));
    check("summary carries a real graph, not zeros", (s?.nodes ?? 0) > 0 && (s?.edges ?? 0) > 0,
      `nodes=${s?.nodes} edges=${s?.edges} entries=${s?.entries} projects=${s?.projects}`);
    // AnalysisSummary.archetype was assigned nowhere before this checkpoint — it shipped "" on
    // every analyze the server ever answered. Reading it back is how that stays fixed.
    check("summary names the archetype", typeof s?.archetype === "string" && s.archetype.length > 0,
      `archetype=${JSON.stringify(s?.archetype)}`);
    check("note present", typeof cold.response?.note === "string" && cold.response.note.length > 10,
      cold.response?.note?.slice(0, 90));
    check("cold call answers the cached question at all", typeof cold.response?.cached === "boolean",
      `cached=${cold.response?.cached} in ${(cold.elapsedMs / 1000).toFixed(1)}s`);
    check("re-analyze reports cached", again.response?.cached === true,
      `cached=${again.response?.cached} in ${(again.elapsedMs / 1000).toFixed(1)}s`);
    // Corroborate the flag against the clock rather than taking its word: a call that reused an
    // analysis cannot also have spent minutes making one.
    check("cached agrees with the clock", again.response?.cached !== true || again.elapsedMs < 5000,
      `cached=${again.response?.cached} elapsed=${again.elapsedMs}ms`);
    check("the note changes when the answer does", cold.response?.note !== again.response?.note
      || cold.response?.cached === again.response?.cached,
      `cold.cached=${cold.response?.cached} again.cached=${again.response?.cached}`);
  },
  // G2.1 (R4 §1 item 11) — the folded menu, and a did-you-mean handler that reads the REAL list.
  //
  // The discriminating check here is an EQUALITY, not a spot check. The hand-maintained array in
  // UnknownToolHandler was CORRECT at 24 names, so "does availableTools contain `map`" passes on
  // the broken state; what makes it wrong is the fold itself. So: availableTools (as a set) must
  // equal tools/list (as a set). Run this case with the fold landed and the hand list still in
  // place and it goes red on three names — that is the red this case exists to have seen.
  async menu(client) {
    const listed = await client.call("tools/list", {}, 45000);
    const real = (listed.result?.tools ?? []).map((t) => t.name).sort();
    const { data: unknown } = await tool(client, "no_such_tool_xyz", {});
    dump("menu-toolslist.json", { count: real.length, tools: real });
    dump("menu-didyoumean.json", unknown);

    const advertised = [...(unknown?.availableTools ?? [])].sort();
    console.log(`  tools/list (${real.length}): ${real.join(" ")}`);
    console.log(`  availableTools (${advertised.length}): ${advertised.join(" ")}`);

    const folded = ["flow", "insights", "interesting_points"];
    check("the three folded tools are gone from tools/list",
      folded.every((f) => !real.includes(f)),
      folded.filter((f) => real.includes(f)).join(",") || "none present");
    check("the menu is the folded size", real.length === 21, `${real.length} tools`);
    // THE drift invariant.
    check("did-you-mean advertises exactly the real tool list",
      advertised.length === real.length && advertised.every((n, i) => n === real[i]),
      `only-in-didyoumean=[${advertised.filter((n) => !real.includes(n)).join(",")}] ` +
      `only-in-tools/list=[${real.filter((n) => !advertised.includes(n)).join(",")}]`);
    // A retired name must teach its replacement, not merely fail. `flow` is the trap: it is a
    // SUBSTRING of `top_flows`, so the nearest-name heuristic answers the wrong tool with total
    // confidence.
    for (const [retired, want] of [["flow", "trace"], ["insights", "stats"], ["interesting_points", "overview"]]) {
      const { data } = await tool(client, retired, {});
      check(`${retired} names its replacement (${want})`,
        typeof data?.hint === "string" && data.hint.includes(want),
        data?.hint ?? JSON.stringify(data)?.slice(0, 120));
    }

    // Nothing may be lost in the fold: every field the three tools returned must still reach an
    // agent through the tool that absorbed it.
    const { handle } = await analyzeRepo(client, join(REPOS, "TodoApi"));
    check("analyze TodoApi", !!handle);
    if (!handle) return;

    const { data: ov } = await tool(client, "overview", { handle });
    dump("menu-overview.json", ov);
    const pts = ov?.startHere ?? [];
    check("overview absorbed interesting_points (addressable, not just titles)",
      pts.length > 0 && pts.every((p) => typeof p.nodeId === "string" && p.nodeId.length > 0),
      `${pts.length} points · keys=${Object.keys(pts[0] ?? {}).join(",")}`);

    const { data: st } = await tool(client, "stats", { handle });
    dump("menu-stats.json", st);
    check("stats absorbed insights (with the confidence insights() carried)",
      Array.isArray(st?.insights) && st.insights.length > 0
      && st.insights.every((i) => typeof i.confidence === "number"),
      `${st?.insights?.length ?? 0} insights · keys=${Object.keys(st?.insights?.[0] ?? {}).join(",")}`);

    const top = (await tool(client, "top_flows", { handle })).data;
    const focus = (top?.topFlows ?? [])[0];
    const focusArg = focus?.route && focus?.httpMethod ? `${focus.httpMethod} ${focus.route}` : focus?.title;
    check("found an entry to trace", !!focusArg, focusArg ?? JSON.stringify(top)?.slice(0, 120));
    if (!focusArg) return;
    const { data: cf } = await tool(client, "trace", { handle, focus: focusArg, format: "compact" }, 90000);
    dump("menu-trace-compact.json", cf);
    check("trace(compact) absorbed flow's counters",
      typeof cf?.steps === "number" && typeof cf?.touches === "number" && typeof cf?.emits === "number",
      `steps=${cf?.steps} touches=${cf?.touches} emits=${cf?.emits}`);
    check("trace(compact) still renders the compact text",
      typeof cf?.text === "string" && cf.text.startsWith("Entry: "),
      (cf?.text ?? "").split("\n")[0]?.slice(0, 80) ?? "(no text)");
    // No reply may point at a surface that no longer exists (R4 §3's bar).
    const blob = JSON.stringify({ cf, ov, st });
    check("no reply points at a folded tool",
      !/\bflow\(|\binsights\(|\binteresting_points\(/.test(blob),
      (blob.match(/\b(flow|insights|interesting_points)\(/g) ?? []).join(",") || "clean");
  },
  // G2.2 (R4 §1 item 12) — ONE trace budget default across MCP / CLI / server.
  //
  // The discriminating check is CROSS-SURFACE: the same focus, traced with NO dials on either
  // side, must come back the same size. Before the fix the MCP shaped to its own 4000-token
  // literal while `query --op trace` ran unbudgeted, so the two surfaces answered the same
  // question with different trees. A per-surface check ("did I get a trace?") passes on both.
  //
  // The second check is the anti-vacuity guard, and it matters more than it looks: the cheap way
  // to make two surfaces agree is to stop budgeting altogether. So the full tree must still be
  // BIGGER than the defaulted one — the default has to actually cut something for this pole to
  // be a test of a budget rather than a test of its absence.
  async "trace-budget"(client) {
    const repo = join(REPOS, "eShop");
    const focus = "POST /api/orders/";
    const { handle, elapsedMs } = await analyzeRepo(client, repo);
    check("analyze eShop", !!handle, `${(elapsedMs / 1000).toFixed(1)}s`);
    if (!handle) return;

    const steps = (n) => { let c = 0; (function walk(x) { if (!x) return; (x.children ?? []).forEach((ch) => { c++; walk(ch); }); })(n); return c; };
    const mcp = async (args) => (await tool(client, "trace", { handle, focus, ...args }, 120000)).data;

    const { execFileSync } = require("child_process");
    const CLI = join(__dirname, "..", "..", "src", "DevContext.Cli", "bin", "Debug", "net10.0", "DevContext.Cli.exe");
    const cliTrace = (extra) => {
      try {
        const out = execFileSync(CLI, ["query", "trace", "--path", repo, "--focus", focus, ...extra],
          { encoding: "utf8", maxBuffer: 64 * 1024 * 1024, timeout: 900000 });
        return JSON.parse(out.slice(out.indexOf("{")));
      } catch (e) {
        console.log(`  CLI trace failed: ${String(e.message).slice(0, 200)}`);
        return null;
      }
    };

    // MEASURED FIRST, and it moved the checks: on eShop's order entry the 4000-token default cuts
    // NOTHING at any depth the walker reaches — budgetTokens 0 and 4000 return the identical tree
    // at depth 6 (59 steps), 9 and 12 (72). So "the budget bit" is not observable on this pole and
    // asserting it would be theatre. What IS observable here is the other half of the same defect:
    // the MCP restated the policy's DEPTH as its own literal on every call, so the server never saw
    // an unspecified depth and TracePolicy.ElasticDepth — which fires only when the caller left the
    // depth to the server — has never run on any request the product has served.
    const probe = [];
    for (const d of [6, 9, 12]) {
      const f = await mcp({ depth: d, budgetTokens: 0 });
      const shaped = await mcp({ depth: d, budgetTokens: 4000 });
      probe.push({ depth: d, full: steps(f?.root), shaped4000: steps(shaped?.root) });
    }
    console.log(`  budget-bite probe: ${probe.map((p) => `d${p.depth} full=${p.full} @4000=${p.shaped4000}`).join(" · ")}`);

    const dflt = await mcp({});                              // NO dials — the policy decides both
    const atPolicyDepth = await mcp({ depth: 6 });           // the depth MCP used to hard-code
    const tiny = await mcp({ budgetTokens: 300 });           // a budget small enough to bite
    const fullTree = await mcp({ budgetTokens: 0 });         // 0 = the whole thing, every surface
    const cli = cliTrace([]);                                // other transport, no dials either
    // The elastic rule needs BOTH halves of its condition: the walk hit the depth limit AND the
    // result left budget to spare. At the policy budget this pole fails the second half (the tree
    // already spends most of 4000), so the rule correctly declines — which is why the pair below
    // states a roomy budget and leaves only the DEPTH unspecified. That is the dial whose absence
    // could never reach the engine before.
    const roomyElastic = await mcp({ budgetTokens: 20000 });
    const roomyFixed = await mcp({ budgetTokens: 20000, depth: 6 });

    const sDflt = steps(dflt?.root), sAt6 = steps(atPolicyDepth?.root);
    const sTiny = steps(tiny?.root), sFull = steps(fullTree?.root), sCli = steps(cli?.root);
    const sElastic = steps(roomyElastic?.root), sFixed = steps(roomyFixed?.root);
    dump("trace-budget-eshop.json", {
      focus, probe,
      mcpNoDials: { steps: sDflt, budgetTokens: dflt?.budgetTokens, budgetSource: dflt?.budgetSource, omitted: dflt?.omitted, hint: dflt?.hint },
      mcpDepth6: { steps: sAt6 },
      mcpBudget300: { steps: sTiny, budgetSource: tiny?.budgetSource },
      mcpFull: { steps: sFull },
      cliNoDials: { steps: sCli, found: cli?.found },
      elastic: { roomyNoDepth: sElastic, roomyDepth6: sFixed },
    });
    console.log(`  steps — mcp(no dials)=${sDflt} · mcp(depth:6)=${sAt6} · mcp(budget:300)=${sTiny} · mcp(full)=${sFull} · cli(no dials)=${sCli}`);
    console.log(`  elastic — budget20000 no depth=${sElastic} · budget20000 depth:6=${sFixed}`);

    // 1. Anti-vacuity: shaping must be capable of cutting, or every check below is about nothing.
    check("a budget can cut this trace at all", sTiny < sFull, `budget300=${sTiny} full=${sFull}`);
    // 2. THE revival. TracePolicy.ElasticDepth deepens a walk the caller did not pin — and until
    //    this checkpoint it could not run at all, because the MCP assigned `depth` on every call and
    //    a set proto3 optional is a stated dial. With the depth left alone and room in the budget it
    //    now reaches past 6. A stated depth still gets exactly 6: an explicit dial is not a
    //    suggestion (Batch E's rule), and that half is the canary.
    check("an unspecified depth lets the policy deepen the walk", sElastic > sFixed,
      `noDepth=${sElastic} depth6=${sFixed}`);
    check("a STATED depth is still honoured exactly", sFixed === sAt6,
      `roomy@6=${sFixed} default@6=${sAt6}`);
    // 3. Cross-surface: two transports, neither naming a dial, one answer. This is what fails if
    //    only one side is fixed.
    check("MCP and CLI agree when neither names a dial", sDflt === sCli,
      `mcp=${sDflt} cli=${sCli}`);
    // 4. The reply says WHOSE budget shaped it instead of quoting a number it never sent.
    check("the reply names the source of the budget it was shaped by",
      dflt?.budgetSource === "server trace policy" && tiny?.budgetSource === "caller",
      `noDials=${dflt?.budgetSource} stated=${tiny?.budgetSource}`);
    // Evidence line, not a check: at the POLICY budget this pole does not deepen, because the tree
    // already spends most of it. Printed so the next reader sees the rule declining rather than
    // wondering whether it ran.
    console.log(`  elastic at the policy budget: no dials=${sDflt} vs depth:6=${sAt6}`
      + ` (equal is correct here — the tree already uses most of the default budget)`);
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
