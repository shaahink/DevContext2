# AGENTS.md — DevContext monorepo

Branch `feat/loom-l7`. Monorepo: engine + server + CLI (C#), desktop app (Angular + Rust/Tauri).

## Cold start
1. Read `LOOM-START.md` — the current stage card (branch, checkpoints, key files, gate).
2. `docs/dev/briefs/loom-graph-design.md` — the design authority (MANDATORY before touching graph code).
3. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.
4. `docs/dev/briefs/meridian-agent-playbook.md` — quality bar, anti-patterns, run/test instructions.
5. `docs/dev/HANDOVER-LOOM.md` — Loom close-out: architecture, benchmarks, known gaps.

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + vitest + build
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server

# From C:/Code/DevContext2-ui
dotnet build DevContext.slnx                         # 0 warnings
dotnet test  DevContext.slnx --filter "Category!=Eval"
powershell -File scripts/loom-guards.ps1              # zero banned patterns
```

## Loom rituals (inherited from proposal-loom.md §1 — keep alive for next phase)

### Pre-session ritual (≤10 min)
1. Read `LOOM-START.md` handoff block + stage section + design doc sections cited by your stage.
2. Run the gate battery: `dotnet build` (0w/0e) · `dotnet test --filter Category!=Eval` · `pnpm check`.
   **If anything is red before you start, fix or record — never build on red.**
3. State in the tracker, in one line, what artifact will prove your stage done.

### Post-session ritual (≤15 min)
1. Re-run the gate battery + the truth gates your stage touches.
2. Produce the evidence artifact (fresh run output under `eval-results/<date>/`).
3. Update `LOOM-START.md`: handoff block (overwrite, ≤10 lines), checkpoint row with commit hash + artifact path.
4. Commit per checkpoint, push. Never merge unasked.

### Discipline invariants
- The design doc's §9 prohibitions are hard rules. `scripts/loom-guards.ps1` — keep it green.
- Every claim in the tracker names a fresh artifact.
- Scope changes get a `> scope change:` line under the checkpoint row.
- **Tests policy:** unit tests that pin *internal string mechanics* may be deleted when their subject dies.
  Truth gates and goldens may only be *ratcheted* (loosened never; tightened with a fresh-run diff).

## Hard rules
- `pnpm check` green before every commit. `dotnet build` green for engine changes.
- Docs move with code in the same commit.
- Append `docs/dev/go-to-program/PROGRESS-LOG.md` after every session.
- Do not write new C# extractors — reform in place.
- Commit before starting work, push after finishing.

## Work items
- **Loom L0–L7** ✅ — Truth harness, identity spine, BodyFacts + seam detectors, semantic-lite, flows + projections, MCP v2, workbench repair, repo-shape coverage.
- **Loom L8** ✅ — Close-out: gate battery, truth tests fixed (7P/4S), HANDOVER-LOOM.md, AGENTS.md rituals.
- **Meridian M0–M9** ✅ — see `docs/dev/HANDOVER-MERIDIAN.md`.
- **Lighthouse L0–L7** ✅ — see `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.
- **conductor-DEBT.md** ⬜ — L0.4–L5.x: 8 items (SymbolTable member indexing, BodyFacts scoping, TfmScore, Flow hardening, audit sweep).

## AI agent process management

Agents MUST NOT run foreground-blocking servers (`pnpm dev:web`, `pnpm server`, `concurrently`).
Instead use the background launcher:

```powershell
# Start both .NET server + Angular dev server in background PowerShell Jobs:
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1

# Check status:
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Status

# Kill all:
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill
```

### Screenshot capture (Playwright)

Use `scripts/capture-readme.mts` with `--no-spawn` (services must be running already):

```powershell
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1
node --experimental-strip-types src/DevContext.App/scripts/capture-readme.mts --no-spawn
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill
```

### Anti-patterns — NEVER do these

| Anti-pattern | Why it hangs the agent |
|---|---|
| `pnpm dev:web` or `concurrently` in foreground | Blocks the terminal forever — the command never returns |
| `spawn()` with `detached: true` + `unref()` inside a Node script | **Does NOT work in practice** — child processes get re-attached to the shell and block on stdio. Node's `child_process` detached mode is unreliable on Windows for keeping the parent responsive. |
| `page.goto(..., { waitUntil: 'networkidle' })` | Hangs forever on pages with live connections (MCP, WebSocket feeds, SSE streams). Use `domcontentloaded` instead. |
| Per-shot timeouts > 90s | One hanging shot blocks all remaining captures. Every `capture()` must have a sub-timeout via `Promise.race`. |
| `page.waitForSelector(selector, { timeout: 300_000 })` | No progress logging — the agent sees zero output for 5 minutes and kills the session. Use a polling loop with heartbeat logging every 10s. |
| Silent waits > 15s | Always log "still waiting for X (Ns elapsed)" so the agent can see progress. |

### Correct Playwright patterns (from `capture-readme.mts`)

1. **Per-shot hard timeout:** wrap each capture in `Promise.race([fn(), timeout(90s)])`
2. **Never skip** — catch errors per-shot, take a last-resort screenshot, log the error, continue to next shot
3. **Progress tally:** `[ok/total] screenshot-name ✓/✗` after every shot
4. **Heartbeat:** every 10s during long waits: `⌛ still waiting: selector-name (10s tick)`
5. **Navigation:** prefer `domcontentloaded` + explicit `sleep(2000)`; never `networkidle`

## Resume protocol (post-Loom)
```powershell
git -C C:/Code/DevContext2-ui checkout feat/loom-l7
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read LOOM-START.md for handoff + checkpoint state
# Read docs/dev/briefs/loom-graph-design.md
# Read docs/dev/HANDOVER-LOOM.md
```

## Next session — Conductor Debt Resolution or Next Phase

1. **conductor-DEBT.md** — 8 items, sized small-medium, fully gated. Start from L0.4 (truth gate auto-enforcement).
2. **eShop TraceQuality investigation** — 5 failing tests on non-CQRS repo.
3. **EvalExpectationTests verticalslice** — expectations out of sync.

Baseline (L8): 436 nodes · 338 edges · 34 entries · 6 ServiceLinks · verified 69% · Analyzed ~5.6s.
