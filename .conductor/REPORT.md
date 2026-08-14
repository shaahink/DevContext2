# Conductor — DevContext pre-release program, run report

_Written 2026-08-14 on the integration of both runs into `develop`._

The pre-release program ran as **two parallel conductor runs** against one plan, on two branches, on
one machine. Both completed. This file is the index of the pair.

A conductor `REPORT.md` is generated per run, so the two branches each carried their own and the
merge could not union them — a report describing "20/20 over 30 sessions" and one describing "16/16
over 25 sessions" have no combined form that is true of either. Each run's full generated report
(stage progress, per-session cost, timeline, gate history, commits by session) is preserved on its
own branch at `.conductor/REPORT.md`, and its `RUN-SUMMARY.md` — rebuilt from `run.db` so it
survives the engine — sits beside it in the worktree.

| | Run A — engine and agent face | Run B — desktop agent loop |
|---|---|---|
| Plan | `conductor.engine.plan.json` | `conductor.desktop.plan.json` |
| Branch | `feat/pre-release-engine` | `feat/pre-release-desktop` |
| Checkpoints | **20/20** | **16/16** |
| Stages confirmed | T1 · V1 · E1 · D1 · R1 · A1 · Z1 | N0 · N1 · N2 · M1 · N3 · N4 · Z1 |
| Sessions | 29 | 25 |
| Cost | $219.85 of $350 | $189.11 of $300 |
| Ended | 12:34 | 12:57 |

## What the pair cost each other

The two runs were **not independent**, and the record should say so plainly:

- **They shared a machine, and their gates were killing each other.** `eval/gates.ps1` step 0 killed
  `DevContext.Server`/`testhost` **by name, machine-wide**, so each run's gate killed the other's
  server mid-question. Both runs went red the same hour on this one cause and both burned a fix
  session on it. Fixed by a machine-wide OS-handle lock (step 0a) landed on both branches —
  `e926593` on engine, `674a030` on desktop, ported rather than cherry-picked because the two copies
  of that file had legitimately diverged. The merged copy on `develop` keeps the desktop's fail-fast
  step order and picks up the engine's step 2c wire-truth gate.
- **The lock covers gates, not delivery sessions.** Run A's Z1 battery still failed once (exit 5)
  because Run B's session #25 launched `pnpm ng serve` mid-battery; the automatic retry passed. A
  session-level resource sits outside the gate's protection.
- **Run B merged Run A's T1 work** (`153c99f`) as an N4.3 precondition, so Run B's batteries were
  exercising engine code too.
- **A machine reboot at 06:30 killed both runs.** The `NEEDS HUMAN` park that followed was an
  artefact of the reboot — two unspawnable sessions read as `AgentError ×2` — not a run defect.

## Integration

Neither branch was a superset of the other, so `develop` took both as a three-way merge. Six files
conflicted; the resolutions worth knowing about are recorded in the merge commit, and the one that
carried real meaning was `docs/dev/research/BUG-BACKLOG.md`, where each run had reconciled its own
half of the backlog and explicitly declined to claim the other's closures. The merged file settles
every item — including `#4`, which Run A recorded as "Run B's" and Run B closed in N4.3.
