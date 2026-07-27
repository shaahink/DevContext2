# R4 — MCP: fixes, then a real dogfood drive

> Owner direction: land the MCP fixes first, then run a FULL drive where the MCP is *actually used*
> to develop or read something in a real session — and judge honestly: did it help? If not, why?
> What does it lack? How does it become a proper tool?

## 1. Fix list (pre-dogfood — from the 2026-07-27 consumption audit; re-verify [audit] refs)

**Correctness / honesty**
1. `map` returns 7 scalars + markdown and DROPS the structured surface (library surface, packages,
   aggregates, service styles) — the markdown then tells agents to run CLI flags that don't exist
   over MCP [audit: `DevContextTools.cs:543-553`; VERIFIED in d51 newtonsoft transcript]. Return the
   structured Map.
2. `get_context` is structurally entry-only — libraries get nothing, GitVersion fills 2%. Add
   type/symbol-rooted packs (shared engine work with R2 §3 / R3 D-C).
3. Compact-trace seam glyphs dead-match plurals ("Sends" vs proto "Send") — every seam renders the
   fallback `·` [audit: `DevContextTools.cs:782-792`]. One-line class of fix.
4. Handle-less calls target the most-recently-ACCESSED session globally — the desktop app touching
   repo A silently retargets the agent's next call from repo B [audit: `DevContextTools.cs:36-41` +
   `AnalysisSessionManager` LastAccess bump]. Scope per-client or remember last-analyzed handle.
5. Raw RpcException leaks past the error envelope on status/entrypoints/insights/close_session/
   list_sessions [audit: respective lines in `DevContextTools.cs`].
6. `find(kind:…)` filters client-side AFTER truncation → false total/hasMore [audit: `:1047-1074`].
   Add `kind` to SearchRequest (server-side).
7. `analyze` blocks silently for minutes discarding Progress events — return at least the CLI's
   honest "first analysis can take minutes; snapshotted" note + `cached` flag in the envelope.

**Missing primitives (the agent-first gaps)**
8. `seam(from,to)` — path-between query; doesn't exist at any layer. Highest-value new primitive.
9. Kind-filtered `neighbors` ("who WRITES this table", "who SENDS this command") — GraphQuery
   supports it; proto/tools don't expose it. 3 proto fields.
10. Snapshot-cache truth (from_cache/analyzed_at/git_head) in AnalysisSummary/SessionInfo.

**Menu hygiene (do during fixes, cheap)**
11. Fold `flow`→`trace(format:compact)`, `insights`→`stats`, `interesting_points`→`overview`;
    24 tools → ~20; reflect tool list for the did-you-mean handler (second hard-coded list drifts).
12. Divergent defaults: trace budget 4000 (MCP) vs unlimited (CLI query) vs shaped (server) —
    one default policy (pairs with R2 Batch E one-trace-contract).

## 2. The dogfood protocol (the real test)

Setup: MCP connected in a fresh Claude Code session (config from the app's MCP page), server warm.
Tasks — REAL work, not lens-audit probes; suggested mix:

- **Task 1 (read/understand)**: in an unseen repo (pick from eval-repos NOT in the octet — e.g.
  Hangfire or OrchardCore), answer 10 genuine architecture questions using ONLY MCP tools (no
  Read/Grep): "where does X get persisted", "who consumes event Y", "what happens on POST /Z",
  "where would I add feature W". Log every tool call.
- **Task 2 (develop)**: implement a small real change in that repo (add an endpoint that touches an
  existing service, or a new event consumer) using MCP for orientation and Read/Edit only for the
  final code. Did MCP shorten the path vs plain grep?
- **Task 3 (DevContext on itself)**: use the MCP against C:\code\DevContext2 for a real maintenance
  question. (Note: server currently ignores devcontext.json — CLI/MCP see different file sets on
  this repo; fix or note before this task.)

Scoring — per tool call, grade: HELPED (answer came from it) / NEUTRAL (redundant with cheaper
call) / HURT (wrong, misleading, or wasted budget). Per task: minutes + calls + would-grep-have-won.
End artifact: `eval-results/<date>/mcp-dogfood/REPORT.md` with the call log, grades, and a ranked
"what it lacks" list. The lens-audit P6 harness (zero-silent-breach probes) stays as the regression
net; this protocol measures USEFULNESS, which P6 does not.

## 3. Success bar (what "proper tool" means here)

- An agent doing Task 1 answers ≥8/10 questions correctly WITHOUT falling back to grep, in fewer
  tokens than a grep-based session (compare the live-feed token total vs a control run).
- No tool returns content that instructs the agent to use another surface (the CLI-flags-in-MCP class).
- Every dead-end reply (0 results, low fill) names a next step that WORKS over MCP.

## 4. Parallelism note

Fixes 1–7 + 11–12 are MCP/server-local and independent of R2 Batches A–C — this strand can run in
parallel with R1/R2 in a separate session/worktree. Primitives 8–10 touch proto+GraphQuery: small,
but coordinate with R2 Batch E to avoid contract churn.
