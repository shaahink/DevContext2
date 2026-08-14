# Pre-release master plan — consolidation of the four 2026-08-13 audits

_2026-08-13 · consolidates `DEEP-EVAL-2026-08-13.md` (agent face + probe), `GRAPH-DETECTION-AUDIT-2026-08-13.md`
(kernel), `STUDIO-MCP-AUDIT-2026-08-13.md` (DECIDED — all four owner calls), and
`DESKTOP-PRODUCT-AUDIT-2026-08-13.md` (DRAFT — the Reader awaits owner refinement) into ONE executable
program, encoded as two conductor plans that can run in parallel. This doc is the planDoc both runs read._

---

## 1. What this plan is, in three sentences

Everything the four audits recommend that is **decided and autonomous** is consolidated here into two
conductor runs: an **engine run** (agent-surface trust pack → graph honesty vocabulary → edge completeness →
detection declared-coverage → metric recalibration → the $10 adoption gate) and a **desktop run** (the
"one agent loop, two rooms" rebuild: truth batch → Studio pins+truth → pack convergence → hygiene/Reader
prerequisites → loop joints → MCP page rebuild). Owner-only items (κ grading, NuGet, the $200 full study,
the Reader design refinement) are registered in §6 and deliberately NOT in either run — a conductor run
parked on an owner decision is a dead run. The two runs share one cross-dependency (the desktop's N4
renders the curated catalog the engine's T1 produces), handled by sequencing N4 last.

## 2. Decision ledger (what is already decided; nothing below re-opens these)

| Decision | Where decided | Consequence here |
|---|---|---|
| Pins: IMPLEMENT | STUDIO-MCP §8.1 | Desktop N1 wires pins for real; N3 the affordances |
| Pack convergence: FULL | STUDIO-MCP §8.2 | Desktop N2 (`BuildMulti`→`ResolveEntry`, `usage` card, type/member scope). Settles S11's D-G |
| Hand-off: REPO FILE FIRST | STUDIO-MCP §8.3 | Desktop N3 writes `.devcontext/packs/<slug>.md` + "point your agent here" line; server-registered packs staged AFTER W1 curation freeze (deferred) |
| MCP page: FULL DECK + SHIP BINARY | STUDIO-MCP §8.4 | Desktop N4: real status, write-config, served catalog via `ListTools`, feed in MCP vocabulary, `devcontext-mcp` in the Tauri bundle |
| W-ordering: W2a before W2; W5 parallel; W6 after W2 | GRAPH-DETECTION §11 | Engine stage order V1 → E1 → R1, D1 parallelizable |
| Adoption gate before the full study | DEEP-EVAL §4 / RESULTS §10.1 | Engine A1.2 is the $10 gate; the $200 study stays owner-gated (§6) |
| Reader = owner refinement first | DESKTOP audit header | M1/M2/M3/M6-proper are OUT of both runs; only the proto/mapper prerequisites + hygiene ride (desktop M1 stage) |

## 3. The two runs (stage map, with sources)

### Run A — engine & agent face (`conductor.engine.plan.json`, branch `feat/pre-release-engine`, worktree `C:/Code/DevContext2-engine`)

| Stage | = audit item | Specification lives in | Depends on |
|---|---|---|---|
| **T1** trust pack | DEEP-EVAL W1.1–W1.4 | DEEP-EVAL §4 W1 + BUG-BACKLOG #5/#6/#10/#9/#2 | — |
| **V1** one-vocabulary pack | GRAPH-DETECTION W2a | GRAPH-DETECTION §3.3 (#25, #17, #7-rider, #18) | — |
| **E1** edge completeness | DEEP-EVAL W2 + GRAPH-DETECTION W2b | GRAPH-DETECTION §3.1 + §4.1/4.2 (#11→#12/TextSpan→re-measure #8→#7; dogfood invariant; matrix batch) | V1 (honest currency for acceptance numbers) |
| **D1** detection declared-coverage | GRAPH-DETECTION W5 | GRAPH-DETECTION §8–9 (catalog-reachability instrument → Orleans/hosted/TimedJob/Avalonia → #14/#20/#2 → rung-4 jobs) | — (independent of E1 by design) |
| **R1** metric recalibration | GRAPH-DETECTION W6 | GRAPH-DETECTION §3.3 #22/#23/#24, G10 discipline | E1 (thresholds read the post-E1 Semantic share) |
| **A1** re-probe prep + adoption gate | DEEP-EVAL W3.7 + the §2.3 design debts | RESULTS.md §10, DESIGN.md, DEEP-EVAL §4 W3 | T1 (the gate measures the fixed surface) |
| **Z1** close-out | DEEP-EVAL W4.11 + release-gate table | DEEP-EVAL §4 gate table; this doc §6 | A1 |

### Run B — desktop agent loop (`conductor.desktop.plan.json`, branch `feat/pre-release-desktop`, worktree `C:/Code/DevContext2-desktop`)

| Stage | = audit item | Specification lives in | Depends on |
|---|---|---|---|
| **N0** truth batch | STUDIO-MCP N0 | STUDIO-MCP §3.F (no-decision items) + §5 N0 | — |
| **N1** Studio truth + pins | STUDIO-MCP N1 (pins DECIDED) | STUDIO-MCP §5 N1 | N0 |
| **N2** pack convergence | STUDIO-MCP N2 (FULL) | STUDIO-MCP §5 N2 + §4 wire items 4/5 | N1 |
| **M1** hygiene + Reader prerequisites | DESKTOP M7 + §4 shopping list | DESKTOP §4 (proto items 1–4) + §5 M7 (no-decision subset) | — |
| **N3** loop joints | STUDIO-MCP N3 + hand-off decision 3 | STUDIO-MCP §5 N3 + §4 Room 1 | N1, N2 |
| **N4** MCP page rebuild | STUDIO-MCP N4 (FULL DECK + SHIP) | STUDIO-MCP §5 N4 + §4 Room 2 + wire items 1–3 | **Run A's T1** (curated catalog) — see §4 |
| **Z1** close-out | — | docs/backlog reconciliation | N4 |

Overlap risk between the runs is low by construction: Run A lives in `src/DevContext.Core/Graph*`,
`src/DevContext.Mcp`, `eval/`; Run B lives in `src/DevContext.App`, proto/`DevContext.Contracts`,
`ContextPackBuilder` (Context/, untouched by Run A). Merge order at the end: engine first (it moves
matrix baselines), desktop rebases before its final merge.

## 4. Parallelism & GitHub sync — what conductor actually supports (reviewed 2026-08-13)

- **Within one run, useful parallelism does not exist.** Tier A `analysisLanes` are read-only,
  15-minute, and **uncosted** (`maxConcurrentLanes` doc: "lane spend is not costed — lanes bill against
  your account without moving `maxRunCostUsd`") — a budget-discipline violation for a paid program, so
  both plans declare none. Tier B `mutatingLanes` are parsed and never scheduled (plan-config.md,
  2026-08-06 note). Gate-level `parallel: true` exists but our battery is one script.
- **Across runs, parallelism is proven.** Two engines on one machine never interfered (field log
  2026-07-29: separate `run.db` each; worktrees work as anchors). That is the lever this plan uses:
  Run A and Run B in separate worktrees, launchable together. Shared risks accepted: CPU contention
  during batteries, and a shared subscription rate ceiling (a second `usage limit` backoff on either
  run is the tell — if it recurs, pause one run and finish sequentially).
- **The one cross-run dependency** (N4 needs T1's curated catalog): N4 is last in Run B; its stage
  notes instruct the session to merge `feat/pre-release-engine` into the desktop branch first (recorded
  in the handoff) and, if T1 has not landed yet, deliver N4's curation-independent checkpoints and
  leave the catalog one open. Launching sequentially (engine first) dissolves the issue entirely.
- **GitHub sync: not implemented in conductor.** `C:/Code/conductor/docs/dev/GITHUB-SYNC-DESIGN-2026-08-13.md`
  is a design (one-way Issues/Projects mirror, reader of the event log, off by default) — its own header
  says nothing is implemented, and the authored checkpoint (L6.3, lanes plan) is unlaunched. What exists
  today and both plans use: `report.push: true` — tracker + `.conductor/REPORT.md` are committed and
  pushed each cycle, so the live board is browsable on GitHub at the branch. Telegram push is configured
  for both runs. When conductor ships the Issues mirror, these plans need no change to adopt it.

## 5. Token-efficiency levers encoded in both plans

1. **Custom templates** (`templates/session.md` + `fix.md`, proven over graph-v2 + probe runs): no
   QA-of-previous-session, no pre-session battery, fast mid-session loop only.
2. **`gatePolicy: perPhase`** — fast tier (`gates.ps1 -Scope engine|app`, ~90s) per session; full
   battery only at stage boundaries (the cadence encoded in gates.ps1 since T7.0, eval stamp-cached).
3. **Token ceiling per the corrected field recipe** — 26M × 0.62 (nudge ~16.1M ≈ the nudge that
   completed graph-v2, margin ~9.9M sized to the ≥2.5×-landing-burn rule). **Launch drill re-derives it
   from `doctor`'s `tokens` line** — a ceiling is only valid for the plan it was fitted to.
4. **Model: `claude-opus-5`** — measured on this machine's own runs: ~12% more per unit of work than
   sonnet but ~half the sessions, zero rollovers, zero fix sessions (field log 2026-08-04). Advisor
   pinned to haiku so circuit-breaker consults don't hang a bare REPL.
5. **Strand-doc pointers in every stage's notes** — sessions read ONE spec doc, not the doc pile.
6. **`bg start` rule for child-process work** (probe batches, matrix runs, judge sweeps) — prevents the
   stall-rail kill + resume + attempt burn that nearly cost the probe run its night (field log 2026-08-11).

## 6. Owner-only register (NOT in either run — each would park a paid engine)

| Item | Source | Blocks |
|---|---|---|
| Grade the 11-item κ sample (~1 hr), run `node eval/agent-probe/kappa.mjs` | DEEP-EVAL W3.6; sample in `eval/agent-probe/results/r1.2-human-sample/` | Trusting every correctness number already collected; nothing else |
| NuGet: claim `DevContext.*`, set key, exercise release path | OWNER-TODO §1 | Public install path |
| Authorize + run the full study (~$200; unseen repo first; B-instructed arm) | DEEP-EVAL W3.8; A1.1 writes the amended DESIGN it runs on | Release gate 4 (the agent claim) |
| Reader refinement session (M1/M2/M3/M6 + the §8 open questions incl. Monaco-vs-CodeMirror spike) | DESKTOP audit §7–8 | The next desktop program; M1 stage in Run B ships its prerequisites |
| Merges to develop | standing order | Both runs merge only at owner sign-off |

## 7. Deferred register (named so nothing silently drops)

N5 cockpit depth (STUDIO-MCP §5 — default defer) · server-registered packs `SavePack`/`pack:<name>`
(after W1 curation freeze, per decision 3) · inheritance/implements seam kind (build when the unseen-repo
probe or the Library story demands it) · property-accessor BodyFacts walking (discriminating fixture
first) · #13/#16 snapshot freshness (fold into whichever session next touches the cache) · `Channel<T>`
seam · Kafka/Rebus/CAP consumers + code-first gRPC · legacy renderer parity (blocked on the render-kernel
decision, which the Reader refinement decides) · incremental analysis (unpriced) · GitHub-URL path
delete-or-build (owner call, desktop audit M7).

## 8. Release gates (from DEEP-EVAL §4, tracked in Run A's Z1)

| # | Gate | Owner | Status (recorded by Z1.1, 2026-08-14) |
|---|---|---|---|
| 1 | Ripgrep test passes on the five shapes of PRODUCT-DIRECTION §9 | Z1 records status | **NOT MET — not runnable as specified.** Measured on this box: `eval-repos/` holds `eShop`, `TodoApi`, `VerticalSlice` only, so 2 of the 5 shapes (Minimal API, CQRS/DDD) have a repo; **DntSite** (Controller Web API) has no checkout and **AutoMapper** (Library) is not in `eval-repos.json` at all; the **Blazor** row of §9 still reads "(add one)" — that shape was never assigned a repo. Closing this needs a repo decision, not a measurement pass. |
| 2 | Wire-truth gate green in the battery | T1.4 builds it; battery enforces | **MET.** Step 2c of `eval/gates.ps1`, enforced on every full run: 14/14 tools described, 52/52 params, entry names round-trip into `get_context`/`trace`, every elision names its `budgetTokens` lever, out-of-range enums rejected. Green in the Z1 battery. |
| 3 | Arm-B adoption ≥ 0.2 with described+curated surface | A1.2 (~$10) | **MET.** Median per-run `mcp_call_share` **0.306** vs the pre-registered **0.20** floor (pooled 0.354, mean 0.335); pilot on the 22-tool undescribed surface was 0.015. 18 runs, $6.42, prompt byte-unchanged. → `eval-results/2026-08-14/a1-adoption-gate/A1.2-EVIDENCE.md` |
| 4 | Unseen-repo non-inferiority at a stated cost band | owner-gated full study (§6) | **NOT RUN — owner-gated (~$200), by design.** A1.1 delivered what it runs on: the amended pre-registration (`eval/agent-probe/DESIGN.md`) and a build-verified identifier-renamed unseen repo. The allowed-to-fail branch is therefore the live one — see the Status line below. |
| 5 | README claims ⊆ measurements | Z1 honesty pass | **MET (this pass).** Four claims were false or unmeasured and are now corrected: the MCP surface (22 tools → 14 advertised + 8 unlisted, five places), the graph layout ("Cytoscape dagre" → Cytoscape over ELK layered), "no hallucinated code" (contradicted by BUG-BACKLOG #6's measured phantom member nodes), and the gate-battery block (listed five commands that are not the battery). The agent story is now stated as measured, with the primer-not-accelerator limit named. |

Gate 4 is allowed to fail: the release is then human lens + agent primer with a true README
(DEEP-EVAL §4). What is not on the table is releasing the claim untested a second time.

**Status as of 2026-08-14: that fallback branch is the live one.** Gate 4 has not been run, so the
README claims a primer and explicitly says the accelerator claim is not established. Gates 2, 3 and 5
are met; gate 1 is blocked on repos, not on the engine.

## 9. Launch drill (operator checklist — nothing here is done by the runs themselves)

1. Commit + push this doc, both plans, both trackers on `feat/agent-probe`.
2. `git worktree add C:/Code/DevContext2-engine  -b feat/pre-release-engine  <feat/agent-probe tip>`
   `git worktree add C:/Code/DevContext2-desktop -b feat/pre-release-desktop <feat/agent-probe tip>`
   In each: `git submodule update --init` (eval-repos are needed by gates + probe), and in the desktop
   worktree `pnpm install` under `src/DevContext.App`. Push each branch once with `-u`.
3. Per worktree preflight: `conductor doctor -p <plan>` — the `work` line must read the expected count
   (20 for engine, 15 for desktop); read the `tokens` line and re-derive `maxSessionTokens` if it names
   a floor/median in conflict; `conductor journey -p <plan>` — Model column shows `claude-opus-5`, not
   `(default)`; `conductor run -p <plan> --dry-run` — assert the composed prompt does NOT contain
   "QA THE PREVIOUS SESSION" and has no unresolved `{braces}`.
4. Escalation-token check per tracker: the handoff block must not contain the literal token
   (`awk '/^## Handoff/,/^## Checkpoints/' <tracker> | grep -ci "human:"` → 0).
5. Launch each run DETACHED with stdout+stderr redirected to scratch files
   (`Start-Process conductor -ArgumentList 'run','-p','<plan>','--headless' …`); `--once` first on
   whichever launches first if supervision is wanted. Arm ONE log-tail monitor per run with the
   current filter from the conductor-drive skill (including `token ceiling|rolled over`).
6. Budget: caps are per-process counters (they reset on engine restart — field log 2026-08-11); read
   lifetime spend from `conductor status`/run.db, never from cap arithmetic after any restart.
