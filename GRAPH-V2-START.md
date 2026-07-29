# graph-v2 — autonomous remainder (conductor-driven)

The authority is `docs/dev/research/PLAN.md` (§2 STATUS is the cold-start entry point) and
`docs/dev/research/DECISIONS.md`. This tracker is the checkpoint surface conductor drives; since W1
it is a **generated view** of the work graph in `.conductor/run.db`. Claim a checkpoint with
`conductor task --done <id> --evidence <path>` — hand-editing a row claims nothing.

**Out of scope, deliberately:** R3's remaining decisions — D-F (insight dedup *policy*), D-G (the
Studio design call), D-H. Those are owner-interactive briefs. Conductor never re-plans, so they can
only park it; they stay with the owner in an interactive session.

## Handoff  (overwrite this block, ≤12 lines, no history)
last: a first session ran 15 min on **G1.1** and was **ABORTED mid-flight** by the owner — it never
  reached gates, never committed, and claimed nothing. **Its work is still in the working tree,
  uncommitted**: `src/DevContext.Mcp/DevContextTools.cs` (the G1.1 target),
  `Core/Rendering/MapRenderer.cs`, `Core/Rendering/LibrarySurfaceRenderer.cs`,
  `Core/Models/DiscoveryModel.cs`, six files under `tests/goldens/`, and a new untracked
  `eval/mcp-qa/drive-r4.js` (an evidence driver it wrote). **You are continuing that work, not
  restarting it.** Read the diff FIRST (`git diff`, `git status`), keep what is right, correct what
  is not. Treat the six golden edits as UNVERIFIED — that session called them a legitimate
  fresh-run ratchet move but died before any gate agreed. Verify or revert them; do not inherit them.
stage: **G1 IN PROGRESS** (attempt 1).
gate: not run since those edits. S10 closed green — `eval-results/2026-07-28/gates-s10-close.txt`.
next: **G1.1** — `map` returns the structured Map surface, not 7 scalars + markdown that advertises
  CLI flags the MCP does not have.
trap: rebuild `src/DevContext.Cli` after ANY Core edit; `analyze` takes a POSITIONAL path.

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence = artifact path.

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| G1.1 | `map` returns the structured Map surface (library surface, packages, aggregates, service styles); its markdown stops advertising CLI flags that don't exist over MCP | TODO | | |
| G1.2 | `get_context` accepts type/symbol roots — a library gets a pack instead of nothing | TODO | | |
| G1.3 | Seam glyphs match the proto (singular/plural), handle-less calls stop retargeting across repos, RpcException stops leaking past the error envelope on all five tools | TODO | | |
| G1.4 | `find(kind:)` filters server-side so total/hasMore are true; `analyze` returns an honest long-run note + a `cached` flag | TODO | | |
| G2.1 | Tool menu folded (`flow`→`trace(compact)`, `insights`→`stats`, `interesting_points`→`overview`) and the did-you-mean handler reads the real tool list instead of a second hand-maintained one | TODO | | |
| G2.2 | One trace budget default across MCP / CLI / server, read from `TracePolicy` (Batch E's single source) | TODO | | |
| G3.1 | `seam(from,to)` path-between primitive exists at proto + GraphQuery + tool | TODO | | |
| G3.2 | Kind-filtered `neighbors` ("who WRITES this table", "who SENDS this command") exposed | TODO | | |
| G3.3 | Snapshot-cache truth (`from_cache` / `analyzed_at` / `git_head`) on AnalysisSummary + SessionInfo | TODO | | |
| G4.1 | Dogfood Task 1 — 10 real architecture questions on an unseen repo, MCP tools only, every call logged and graded HELPED / NEUTRAL / HURT | TODO | | |
| G4.2 | Dogfood Tasks 2+3 — a real change made through MCP orientation, and DevContext used on itself | TODO | | |
| G4.3 | `eval-results/<date>/mcp-dogfood/REPORT.md` — call log, grades, ranked "what it lacks", judged against R4 §3's success bar | TODO | | |
| G5.1 | Root cause named, per verb with evidence: why GitVersion's five `ICommand<TSettings>` verbs join no handler | TODO | | |
| G5.2 | The join lands — a CLI verb reaches its handler on the gitversion pole, with the CleanArchitecture canary unmoved | TODO | | |
| G6.1 | One vocabulary for "service" on Atlas — the canvas, the per-service breakdown and Hub radar stop disagreeing about what a service is | TODO | | |
| G6.2 | Raw metadata arity never reaches the UI (no `` Logging.ILogger`1 `` in a rendered surface) | TODO | | |
| G7.1 | C-2 — Atlas's five empty sections on a library either fill or withhold themselves with a stated reason | TODO | | |
| G7.2 | C-3 — the withhold-don't-suppress rule applied consistently wherever a surface has no entries | TODO | | |
| G8.1 | HotChocolate profiled: the phase that does not terminate inside the 600s budget is NAMED, with per-phase timings as evidence | TODO | | |
| G8.2 | Fixed, or recorded as an accepted limitation with the defect class named — R1's exit criterion answered either way. **Not by raising the timeout.** | TODO | | |
| G9.1 | An auxiliary/demo executable stops deciding a packable library's archetype: `CLI` and `MahApps.Metro` read Library, canary poles unmoved | TODO | | |
| G10.1 | Sweep for thresholds calibrated on pre-Batch-A (starved-graph) data; each one re-measured on current data and corrected or justified in a comment that states the measurement | TODO | | |
