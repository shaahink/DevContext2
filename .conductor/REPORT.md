# Conductor — DevContext pre-release - desktop agent loop run report

_Updated 2026-08-13 23:16 UTC · branch `feat/pre-release-desktop` · HEAD `951a836`_

**Status:** Idle — stage N2 used all 6 attempts without completing — inspect and `conductor resume` (or `conductor skip`) [2h 39m ago, 20:36:43Z]
**Stage:** M1 — Hygiene + Reader prerequisites (proto/mapper shopping list) · attempts used 1
**Checkpoints:** 9/16 done · **Sessions run:** 16 · **Cost:** $99.7996 (agent $99.6281 + gates $0.1716) · **Tokens:** 1,590,346 in / 633,124 out
**Confirmed phases:** N0, N1, N2
**Pending:** full-battery phase gate for M1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| N0 | Truth batch - no-decision honesty fixes on Studio + MCP page | ██████████ 3/3 | confirmed ✓ |
| N1 | Studio truth pass + pins made real (owner decision 1: IMPLEMENT) | ██████████ 2/2 | confirmed ✓ |
| N2 | Pack convergence - one pipeline, two faces (owner decision 2: FULL) | ██████████ 2/2 | confirmed ✓ |
| M1 | Hygiene + Reader prerequisites (proto/mapper shopping list) | ██████████ 2/2 | gating… |
| N3 | Loop joints - routes into Studio + repo-file hand-off (owner decision 3) | ░░░░░░░░░░ 0/2 | todo |
| N4 | MCP page rebuild - the observation deck (owner decision 4: full deck + ship binary) | ░░░░░░░░░░ 0/3 | todo |
| Z1 | Close-out: docs + backlog + README screenshot sync, full battery, push | ░░░░░░░░░░ 0/2 | todo |

<details> ✅<summary>N0 — Truth batch - no-decision honesty fixes on Studio + MCP page (3/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| N0.1 | Studio truth items: multi-entry merge preserves SourceLocations/Verified/Approx (§3.F.3); allocated_tokens no longer echoes budget (§3.F.4); Studio copy/save use the app's clipboard helper and toasts await outcome (§3.F.7) | ✅ DONE | [`36bf916`](https://github.com/shaahink/DevContext2/commit/36bf916) |
| N0.2 | MCP page truth items: status read no longer calls StartMcp (§3.F.9); snippet paths + copy-label fix (§3.F.10/11); feed totals respect the filter + wire timestamps (§3.F.12); sessions table renders the honesty fields so the shown age stops lying (§3.F.13); dead state deleted (§3.F.14) | ✅ DONE | [`36bf916`](https://github.com/shaahink/DevContext2/commit/36bf916) |
| N0.3 | The §3.F inventory filed into BUG-BACKLOG.md as triaged bugs; spec smoke coverage exists for both pages (the three data-testids referenced by real specs) | ✅ DONE | [`823de02`](https://github.com/shaahink/DevContext2/commit/823de02) |

</details>

<details> ✅<summary>N1 — Studio truth pass + pins made real (owner decision 1: IMPLEMENT) (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| N1.1 | Verified/approx rendered per card; verification ledger verifies the pack actually built (wire item 4, mechanism chosen with a stated reason); state lifecycle fixed (per-tab keying or handle-effect invalidation; budget/intent/format persisted); body toggles wired or deleted | ✅ DONE | [`d57b59d`](https://github.com/shaahink/DevContext2/commit/d57b59d) |
| N1.2 | Pins real end-to-end: `p` pins from Explore; TrailStore.pins() has real readers; pinned steps seed the pack; the three advertising surfaces (inspector, trail bar, ticker) tell the truth | ✅ DONE | [`e448d64`](https://github.com/shaahink/DevContext2/commit/e448d64) |

</details>

<details> ✅<summary>N2 — Pack convergence - one pipeline, two faces (owner decision 2: FULL) (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| N2.1 | BuildMulti adopts the ResolveEntry path (symbol-rooted cards); `usage` joins CardTypeSections; picker gains a Types tab (LibrarySurface list) + D-G row identity (target member + route tail + project) | ✅ DONE | [`104c9d0`](https://github.com/shaahink/DevContext2/commit/104c9d0) |
| N2.2 | Honesty-note parity with get_context (fill-rate note + suggested focuses in the rail); budget default reconciled to one stated number; ACCEPTANCE: a FluentValidation pack composed from types, with usage and verified counts, end to end | ✅ DONE | [`aab7cf3`](https://github.com/shaahink/DevContext2/commit/aab7cf3) |

</details>

<details> ✅<summary>M1 — Hygiene + Reader prerequisites (proto/mapper shopping list) (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| M1.1 | Proto/mapper shopping list: TraceNode structured file_path+line_number; ReadSource file mode (or GetFileSource) with caps; per-file edge overlay query on the wire; ProtoMapper stops dropping MultiImplCount/DiHostCount/TestOnly/OmittedNames | ✅ DONE | [`a95d620`](https://github.com/shaahink/DevContext2/commit/a95d620) |
| M1.2 | Hygiene: MapResponse.stack populated or its three consumers stop rendering it (bug filed either way); Layer/Feature lens slots hidden until data exists; createTab MAX_TABS lie fixed; dock resizer added; high-contrast theme selectable or removed | ✅ DONE | [`7ccbf56`](https://github.com/shaahink/DevContext2/commit/7ccbf56) |

</details>

<details><summary>N3 — Loop joints - routes into Studio + repo-file hand-off (owner decision 3) (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| N3.1 | Send-to-Studio from Explore (selection/trail/pins), Insights cards, and NodeCard; Studio default state = proposed pack from current trail+pins (never opens empty after exploration); archetype preset on fresh sessions | ⬜ TODO | - |
| N3.2 | Save writes `.devcontext/packs/<slug>.md` (gitignored by default) + a copyable point-your-agent-here line for CLAUDE.md; Home's point-your-agent-here routes through Studio | ⬜ TODO | - |

</details>

<details><summary>N4 — MCP page rebuild - the observation deck (owner decision 4: full deck + ship binary) (0/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| N4.1 | Status that measures: binary fs probe; ObserverCount + last-agent-call-at on the wire and rendered; handshake = one real MCP tools/list round-trip shown; Start/Stop killed or renamed to what it does | ⬜ TODO | - |
| N4.2 | Setup that works: devcontext-mcp ships in the Tauri bundle; snippets carry the resolved absolute path; write-config-for-me button per host | ⬜ TODO | - |
| N4.3 | The catalog served: ListTools RPC (kills #4 structurally); page renders the curated described menu agents actually get (requires T1 merged in); feed keyed by MCP tool names (analyze wrapped, args digest, wire timestamps); rows deep-link — trace→Explore, get_context→replay-in-Studio | ⬜ TODO | - |

</details>

<details><summary>Z1 — Close-out: docs + backlog + README screenshot sync, full battery, push (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| Z1.1 | STUDIO-MCP-AUDIT statuses + DECISIONS.md (D-G settled by N2) + BUG-BACKLOG reconciled; PRE-RELEASE-PLAN §3 table updated for this run; full battery green; branch pushed | ⬜ TODO | - |
| Z1.2 | README screenshot sync: docs/screenshots refreshed via the existing capture pipeline (screenshot-readme.mts / capture-readme.mts) against the post-N4 app — at minimum 08-context-studio, 09-export, 10-mcp plus any visibly changed page; README captions updated where the UI changed (agent-story claims untouched — engine Z1 owns those); committed and pushed | ⬜ TODO | - |

</details>

## Sessions

| # | Stage | Kind | Att | Started (UTC) | Dur | Outcome | New DONE | Commits | Gates | Cost | Overhead | Tokens |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | N0 | Deliver | 1 | 08-13 16:16 | 0:35 | Advanced | N0.1 N0.2 | 3 | fast-app:OK · guards:OK | $12.1600 | $0.0273 | 177,163/77,582 |
| 2 | N0 | Deliver | 1 | 08-13 16:56 | 0:15 | Advanced | N0.3 | 2 | fast-app:OK · guards:OK | $4.7577 | $0.0190 | 100,599/38,922 |
| 3 | N0 | Fix | 2 | 08-13 17:21 | 0:31 | Progress |  | 3 | fast-app:OK · guards:OK | $5.9230 | $0.0250 | 112,749/54,823 |
| 4 | N1 | Deliver | 1 | 08-13 18:15 | 0:34 | Advanced | N1.1 | 11 | fast-app:OK · guards:OK | $12.0268 | $0.0288 | 194,917/79,115 |
| 5 | N1 | Deliver | 1 | 08-13 18:54 | 0:22 | Advanced | N1.2 | 3 | fast-app:OK · guards:OK | $8.3882 | $0.0294 | 133,027/48,328 |
| 6 | N2 | Deliver | 1 | 08-13 19:49 | 0:21 | Advanced | N2.1 | 3 | fast-app:OK · fast-engine:OK · guards:OK | $12.2349 | $0.0420 | 186,261/68,901 |
| 7 | N2 | Deliver | 1 | 08-13 20:18 | 0:16 | AgentError |  | 0 | gates green (none configured) | $5.3309 |  | 116,243/35,230 |
| 8 | N2 | Fix | 2 | 08-13 20:34 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 9 | N2 | Deliver | 3 | 08-13 20:35 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 10 | N2 | Deliver | 4 | 08-13 20:35 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 11 | N2 | Deliver | 5 | 08-13 20:35 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 12 | N2 | Deliver | 6 | 08-13 20:36 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 13 | N2 | Deliver | 1 | 08-13 20:44 | 0:23 | Advanced | N2.2 | 3 | gates green (none configured) | $9.0389 |  | 144,356/58,449 |
| 14 | M1 | Deliver | 1 | 08-13 21:48 | 0:35 | Advanced | M1.1 | 15 | gates green (none configured) | $13.9233 |  | 184,289/75,385 |
| 15 | M1 | Deliver | 1 | 08-13 22:23 | 0:35 | Advanced | M1.2 | 2 | gates green (none configured) | $13.2104 |  | 181,392/72,474 |
| 16 | M1 | Fix | 2 | 08-13 23:05 | 0:10 | Progress |  | 2 | gates green (none configured) | $2.6342 |  | 59,350/23,915 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 15 | 135M | 98.4% | $97.18 | 9 | 15M | $10.80 |
| stage N0 | 3 | 29.9M | 98.1% | $22.91 | 3 | 9.96M | $7.64 |
| stage N1 | 2 | 28.4M | 98.4% | $20.47 | 2 | 14.2M | $10.24 |
| stage N2 | 8 | 36.7M | 98.3% | $26.67 | 2 | 18.4M | $13.33 |
| stage M1 | 2 | 40.1M | 98.7% | $27.13 | 2 | 20M | $13.57 |
| 2026-08 | 15 | 135M | 98.4% | $97.18 | 9 | 15M | $10.80 |

_Where the money goes: agent $96.99 (100%) · gate $0.17 (0%) · advisor $0.02 (0%) · blended $0.72/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
08-13 21:18:12  ▪ gate guards pass [session]  (1m24s)
08-13 21:18:23  • session #6 N2 → Advanced · done N2.1 · 3 commit(s)  (29m09s)
08-13 21:18:24  ◆ plan reloaded — v1 · 7 stages · 4 gates
08-13 21:18:34  • session #7 N2 Deliver started (attempt 1/6)
08-13 21:34:57  • session #7 N2 → AgentError  (16m23s)
08-13 21:34:58  • session #8 N2 Fix started (attempt 2/6)
08-13 21:35:12  ■ needs human — advisor: human intervention required
08-13 21:35:18  • session #8 N2 → AgentError  (20.0s)
08-13 21:35:18  • session #9 N2 Deliver started (attempt 3/6)
08-13 21:35:32  ■ needs human — advisor: human intervention required
08-13 21:35:37  • session #9 N2 → AgentError  (19.6s)
08-13 21:35:38  • session #10 N2 Deliver started (attempt 4/6)
08-13 21:35:51  ■ needs human — advisor: human intervention required
08-13 21:35:57  • session #10 N2 → AgentError  (19.5s)
08-13 21:35:57  • session #11 N2 Deliver started (attempt 5/6)
08-13 21:36:11  ■ needs human — advisor: human intervention required
08-13 21:36:17  • session #11 N2 → AgentError  (19.5s)
08-13 21:36:17  • session #12 N2 Deliver started (attempt 6/6)
08-13 21:36:31  ■ needs human — advisor: human intervention required
08-13 21:36:37  • session #12 N2 → AgentError  (19.6s)
08-13 21:36:43  ■ needs human — stage N2 used all 6 attempts without completing — inspect and `conductor resume` (or `conductor skip`)
08-13 21:44:15  • session #13 N2 Deliver started (attempt 1/6)
08-13 22:07:39  • session #13 N2 → Advanced · done N2.2 · 3 commit(s)  (23m24s)
08-13 22:48:23  ▪ gate fast-app pass [phase]  (14m03s)
08-13 22:48:23  ▪ gate fast-engine pass [phase]  (4m34s)
08-13 22:48:23  ▪ gate guards pass [phase]  (3m29s)
08-13 22:48:23  ▪ gate battery pass [phase]  (18m29s)
08-13 22:48:23  ✓ checkpoint N2.1 confirmed
08-13 22:48:23  ✓ checkpoint N2.2 confirmed
08-13 22:48:23  ▸ stage N2 confirmed  (1h59m09s)
08-13 22:48:33  ▸ stage M1 entered — Hygiene + Reader prerequisites (proto/mapper shopping list)
08-13 22:48:33  • session #14 M1 Deliver started (attempt 1/4)
08-13 23:23:58  • session #14 M1 → Advanced · done M1.1 · 15 commit(s)  (35m24s)
08-13 23:23:59  • session #15 M1 Deliver started (attempt 1/4)
08-13 23:59:46  • session #15 M1 → Advanced · done M1.2 · 2 commit(s)  (35m46s)
08-14 00:05:41  ▪ gate fast-app FAIL [phase]  (47.6s)
08-14 00:05:41  ▪ gate fast-engine pass [phase]  (1m51s)
08-14 00:05:42  ▪ gate guards pass [phase]  (1m06s)
08-14 00:05:42  ▪ gate battery FAIL [phase]  (56.6s)
08-14 00:05:51  • session #16 M1 Fix started (attempt 2/4)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 16 · retries 7 (44 %) · overall Alert
⛔ [same-failure-loop] stage N2: 6 consecutive sessions made no progress
⚠ [context-saturation] session #14: 20,388,199 context tokens (≥ 20,000,000)
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/pre-release-desktop
working tree: clean
vs upstream: up to date
```

### Commits by session

- **s3 (N0 Fix)** — 3 commit(s):
  - [`c1da823`](https://github.com/shaahink/DevContext2/commit/c1da823) docs(tracker): N0 phase gate is GREEN - both reds were pre-existing, neither was N0's
  - [`32639c6`](https://github.com/shaahink/DevContext2/commit/32639c6) fix(app-tests): pay the elkjs bootstrap in a hook, not in the first assertion
  - [`5fd9911`](https://github.com/shaahink/DevContext2/commit/5fd9911) fix(gates): Step 3 no longer dies on Get-FileHash under a pwsh-7 parent
- **s4 (N1 Deliver)** — 11 commit(s):
  - [`a6ab4e2`](https://github.com/shaahink/DevContext2/commit/a6ab4e2) Merge remote-tracking branch 'origin/feat/pre-release-desktop' into feat/pre-release-desktop
  - [`b89df90`](https://github.com/shaahink/DevContext2/commit/b89df90) docs(tracker): N1.1 handoff - wire item 4 is decided and shipped, next is N1.2 (pins)
  - [`e3a9bc2`](https://github.com/shaahink/DevContext2/commit/e3a9bc2) feat(studio): per-card verified/approx, the pack's own ledger, handle-keyed cards, real body toggles (N1.1, app half)
  - [`56ebc25`](https://github.com/shaahink/DevContext2/commit/56ebc25) feat(pack): GetContextPack returns the ledger for the pack it built + wires exclude_bodies (N1.1, engine half)
  - [`7599e70`](https://github.com/shaahink/DevContext2/commit/7599e70) chore(conductor): s3 N0 Progress — Idle
  - [`15ffaa5`](https://github.com/shaahink/DevContext2/commit/15ffaa5) chore(conductor): s3 N0 Progress — Idle
  - [`c1da823`](https://github.com/shaahink/DevContext2/commit/c1da823) docs(tracker): N0 phase gate is GREEN - both reds were pre-existing, neither was N0's
  - [`32639c6`](https://github.com/shaahink/DevContext2/commit/32639c6) fix(app-tests): pay the elkjs bootstrap in a hook, not in the first assertion
  - [`5fd9911`](https://github.com/shaahink/DevContext2/commit/5fd9911) fix(gates): Step 3 no longer dies on Get-FileHash under a pwsh-7 parent
  - [`ff9789e`](https://github.com/shaahink/DevContext2/commit/ff9789e) chore(conductor): s2 N0 Advanced — Idle
  - [`d57b59d`](https://github.com/shaahink/DevContext2/commit/d57b59d) chore(conductor): s2 N0 Advanced — Idle
- **s5 (N1 Deliver)** — 3 commit(s):
  - [`4d24d85`](https://github.com/shaahink/DevContext2/commit/4d24d85) docs(tracker): N1.2 handoff - pins are real, N1 closed, next is N2.1
  - [`366cc3a`](https://github.com/shaahink/DevContext2/commit/366cc3a) test(explore): p reports what it pinned + N1.2 evidence and backlog close
  - [`e448d64`](https://github.com/shaahink/DevContext2/commit/e448d64) feat(studio): pins seed the pack for real - TrailStore.pins() gains its reader (N1.2)
- **s6 (N2 Deliver)** — 3 commit(s):
  - [`108301a`](https://github.com/shaahink/DevContext2/commit/108301a) docs(tracker): N2.1 closed - pack convergence evidence + handoff to N2.2
  - [`8c38e0b`](https://github.com/shaahink/DevContext2/commit/8c38e0b) feat(studio): the picker gets a Types tab, and rows that are different rows (N2.1)
  - [`104c9d0`](https://github.com/shaahink/DevContext2/commit/104c9d0) feat(pack): BuildMulti resolves symbols, not just entries - and usage becomes a card (N2.1)
- **s13 (N2 Deliver)** — 3 commit(s):
  - [`2ceb4b3`](https://github.com/shaahink/DevContext2/commit/2ceb4b3) docs(tracker): N2.2 handoff — stage N2 closed, start at N3.1
  - [`7ff60eb`](https://github.com/shaahink/DevContext2/commit/7ff60eb) chore(gates): abort != fail, node_modules preflight, fail-fast step order
  - [`aab7cf3`](https://github.com/shaahink/DevContext2/commit/aab7cf3) feat(studio): N2.2 honesty-note parity + one pack-budget number
- **s14 (M1 Deliver)** — 15 commit(s):
  - [`cc29c12`](https://github.com/shaahink/DevContext2/commit/cc29c12) Merge remote-tracking branch 'origin/feat/pre-release-desktop' into feat/pre-release-desktop
  - [`3a10029`](https://github.com/shaahink/DevContext2/commit/3a10029) feat(wire): M1.1 items 2+3 - two file-addressed reads, both capped and both contained
  - [`3f1db01`](https://github.com/shaahink/DevContext2/commit/3f1db01) feat(wire): M1.1 items 1+4 - the trace node stops lying by omission
  - [`b8d918a`](https://github.com/shaahink/DevContext2/commit/b8d918a) chore(conductor): s13 N2 Advanced — Idle
  - [`69f9004`](https://github.com/shaahink/DevContext2/commit/69f9004) chore(conductor): s13 N2 Advanced — Idle
  - [`2ceb4b3`](https://github.com/shaahink/DevContext2/commit/2ceb4b3) docs(tracker): N2.2 handoff — stage N2 closed, start at N3.1
  - [`7ff60eb`](https://github.com/shaahink/DevContext2/commit/7ff60eb) chore(gates): abort != fail, node_modules preflight, fail-fast step order
  - [`aab7cf3`](https://github.com/shaahink/DevContext2/commit/aab7cf3) feat(studio): N2.2 honesty-note parity + one pack-budget number
  - [`77596b1`](https://github.com/shaahink/DevContext2/commit/77596b1) chore(conductor): s12 N2 AgentError — Idle
  - [`937adf1`](https://github.com/shaahink/DevContext2/commit/937adf1) chore(conductor): s11 N2 AgentError — NeedsHuman
  - [`f8b578c`](https://github.com/shaahink/DevContext2/commit/f8b578c) chore(conductor): s10 N2 AgentError — NeedsHuman
  - [`60720ad`](https://github.com/shaahink/DevContext2/commit/60720ad) chore(conductor): s9 N2 AgentError — NeedsHuman
- **s15 (M1 Deliver)** — 2 commit(s):
  - [`e928f7a`](https://github.com/shaahink/DevContext2/commit/e928f7a) feat(app): the M1.2 hygiene batch - four surfaces stop over-claiming
  - [`7ccbf56`](https://github.com/shaahink/DevContext2/commit/7ccbf56) fix(map): MapResponse.stack stops shipping empty to three readers
- **s16 (M1 Fix)** — 2 commit(s):
  - [`951a836`](https://github.com/shaahink/DevContext2/commit/951a836) docs(tracker): handoff for the M1 gate-red fix + file bug #8
  - [`ad0eaff`](https://github.com/shaahink/DevContext2/commit/ad0eaff) fix(app): the lint error that failed M1 twice

## Last session result

> SESSION-RESULT: I fixed the sole cause of the M1 gate RED — a `@typescript-eslint/prefer-function-type` lint error at `workbench-page.spec.ts:136` in the spec file session #15 added for the M1.2 dock resizer — by rewriting `{ (): number | null }` to `() => number | null`, a type-identical change that weakens nothing (no test deleted or skipped, no `eslint-disable`, no rule downgraded, no gate command softened, no golden touched); I then re-ran the gate's own step 5 in full rather than only the red step, since `pnpm check` short-circuits and had therefore never reached test or build, and it is now green with `pnpm lint` exit code 0, 25 test files / 224 tests passed, and `ng build` clean in 11…

## Tracker handoff

```
Stage M1 gate is GREEN again (ad0eaff). s15's five M1.2 deliverables were sound; the spec file it
ADDED had never been linted, and that one error failed fast-app + battery twice each. Fix =
workbench-page.spec.ts:136, `dockWidthOverride: { (): number | null }` → `() => number | null`, a
type-identical rewrite — no test skipped, no eslint-disable, no rule downgraded. Evidence
eval-results/2026-08-13/M1-gate-red-lint-fix.md: lint exit 0, 25 files / 224 tests, ng build clean
(112s), zero ELIFECYCLE. Also filed bug #8: GitHub-URL dead code re-measured today (3 files, zero
real importers) and left UNDECIDED, as the stage requires. NEXT: N3.1, first TODO on the board.
TRAP, and it will fire on the next new spec: `tsc -p tsconfig.spec.json` CANNOT catch this class —
both spellings are the SAME TYPE — and `ng build` never compiles specs. Only `ng lint` separates
them, so lint any NEWLY ADDED spec before claiming green. prefer-function-type fires only when a
call signature is the literal's SOLE member (line 134's two-member version is legal — why the bad
line looked fine). pnpm check SHORT-CIRCUITS: a lint-red gate never ran test or build, re-run all 3.
Still standing: Assert.Equal(collection-expr, ImmutableArray) never passes (IEquatable by REFERENCE
— .ToArray() the actual); contract-sweep only catches fields with NO readers. ng build ~112-193s.
```
