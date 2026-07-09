# Loom — Post-Delivery Workflow

## Phase Structure

Loom L0-L8 (34/34 checkpoints) is **COMPLETE** and delivered. The remaining work is in 3 sequential phases:

| Phase | Sessions | Theme | Protocol |
|-------|----------|-------|----------|
| 1 | 1-9 | Merge + Debt Cleanup | Code + config changes, one item per session |
| 2 | 10-12 | Static Design Review | Read-only audit. Rate checkpoints vs design doc. Write report. |
| 3 | 13 | Final QA Driver | Live app drive. Screenshot everything. Plan bugfixes. |

## Universal Pre-Session Ritual (all phases, ≤5 min)

1. Read `LOOM-START.md` handoff block (≤12 lines, always current).
2. Read your session's item in `conductor-DEBT.md` (Phase 1) or the relevant section of this workflow (Phase 2-3).
3. Run **selective** gate:
   - Engine change → `dotnet build DevContext.slnx` + `dotnet test --filter Category!=Eval`
   - UI change → `pnpm check` (from `src/DevContext.App`)
   - Config/docs change → relevant command only
   - Review/QA (no code change) → skip gates
   - **Never build on red.** If anything fails before you start, fix or write BLOCKED in the handoff.
4. State in the tracker what artifact proves this session done.

## Universal Post-Session Ritual (all phases, ≤15 min)

1. Run the selective gate again — confirm nothing regressed.
2. Produce evidence artifact under `eval-results/<date>/`.
3. Overwrite `LOOM-START.md` handoff block (≤12 lines, never append).
4. Update checkpoint status in LOOM-START.md table.
5. Commit (`fix(debt): <item>` or `docs(review): R<N>` or `qa: ...`). Push.

## Discipline Invariants (unchanged from Loom)

- `scripts/loom-guards.ps1` — keep it green (0 banned patterns).
- Every claim in the tracker names a fresh artifact.
- Scope changes get a `> scope change:` line under the checkpoint row.
- Truth gates and goldens may only be *ratcheted* (loosened never; tightened with a fresh-run diff).
- One commit per session, one checkpoint per commit. Never batch.

---

## Phase 1 Protocol — Debt Cleanup

**Goal:** Resolve 8 deferred debt items + merge the feature branch.

Each item is pre-sized (20-45 min) and pre-gated. The order is simpler-first.

### Per-Session Checklist

1. Read the item's section in `conductor-DEBT.md` (background, files, gate).
2. Read the relevant handover in `.conductor/handovers/L<N>.md` if needed.
3. Fix the root cause, not the symptom (each item has specific guidance).
4. Verify with the item's gate command.
5. Evidence → commit → handoff.

### Session Table

| # | Item | Effort | Gate Command |
|---|------|--------|-------------|
| 1 | L0.5 — Cold-QA B9 denominator + UI boot-liveness | ~20m | `node eval/mcp-qa/run-cold.js` exits clean; dead env → red |
| 2 | L3.5 — TodoApi eval gap triaged | ~25m | `dotnet test --filter "TodoApi"` passes or skips with reason |
| 3 | L5.x — Audit-trap sweep (5 small traps) | ~20m | guards green; build 0w/0e; advisory count stable |
| 4 | Merge feat/loom-l7 → develop (squash per L-stage) | ~30m | `git log --oneline develop` = feature commits only |
| 5 | L0.4 — Truth gate in battery + pending sweep | ~30m | guards include truth; `Skip.IfNot` not `return` |
| 6 | L3.4 — TfmScore net10.0+ | ~35m | dogfood SemanticLite ≤4.0s |
| 7 | L2.5 — Lambda scope pollution + SeamContext dedup | ~40m | multi-lambda test passes |
| 8 | L4.5 — Flow model hardening | ~40m | GetContextPack integration test |
| 9 | L1.6 — SymbolTable member indexing | ~45m | SymbolKind.Member populated; RefSite.FromType deleted |

### Merge Protocol (Session 4)

1. `git checkout develop && git pull`
2. For each L-stage branch in order (loom-l1, loom-l2, loom-l5, loom-l7):
   ```
   git merge --squash feat/loom-l<N>
   git commit -m "feat(loom): L<N> — <stage theme>"
   ```
3. Push develop.
4. Tag: `git tag loom-2026-07-08`
5. Keep `feat/loom-l7` branch as-is (archival).

---

## Phase 2 Protocol — Static Design Review

**Goal:** Read-only audit against the design authority. No code changes.

### Read Order Per Session

| Session | Design Doc | Stage Specs | Handovers |
|---------|-----------|-------------|-----------|
| R1 (10) | `loom-graph-design.md` §0-2 | `proposal-loom.md` §L0-L2 | L0.md, L1.md, L2.md |
| R2 (11) | `loom-graph-design.md` §3-5 | `proposal-loom.md` §L4-L6 | L4.md, L5.md, L6.md |
| R3 (12) | `loom-graph-design.md` §6-8 | `proposal-loom.md` §L7-L8 + cross-cutting | L8.md + system sweep |

### Rating System

| Rating | Meaning |
|--------|---------|
| ✅ CONFORMS | Matches design doc exactly. No gaps. |
| ⚠️ CONFORMS-WITH-FINDINGS | Matches substance. Minor doc/cosmetic gaps. |
| ❌ DEVIATES | Does NOT match design doc. Material gap. |

**Stage verdict** = worst checkpoint rating:
- All ✅ or ⚠️ → **PASS**
- 1 ❌ → **PASS-WITH-FINDINGS**
- ≥2 ❌ → **FAIL**

### Report Template

```markdown
# Design Review: <Scope>

**Date:** <date>
**Reviewer:** opencode/deepseek-v4-pro (autonomous)
**Branch:** develop
**Range reviewed:** <git range>

## Stage: <L-N> — <theme>

| Checkpoint | Design requirement | Delivered | Rating | Evidence |
|------------|-------------------|-----------|--------|----------|
| <id> | <quoted from design doc> | <code + behavior> | ✅/⚠️/❌ | <path> |

**Stage verdict:** ✅ PASS / ⚠️ PASS-WITH-FINDINGS / ❌ FAIL

**Findings:**
1. <description>
```

R3 also checks system-level contracts:
| Contract | Design says | Code says | Rating |
|----------|-------------|-----------|--------|
| Law R1 — no silent winners | Ambiguous edges skipped in traversals | grep GraphQuery | ✅/⚠️/❌ |
| Law R2 — tier monotone | Only upgrade, never downgrade | grep SymbolTable | ✅/⚠️/❌ |
| No Regex in Core/Graph | loom-design §2.3 | Run guards | ✅/⚠️/❌ |
| SymbolId outside Graph2/ | loom-design §1.1 | Run guards | ✅/⚠️/❌ |
| NodeId.ForType stable | proposal §L1.4 | Count advisory | ✅/⚠️/❌ |

### Output

One file per session: `docs/design-reviews/R<N>-<scope>.md`
Gate: Report committed. All checkpoints rated.

---

## Phase 3 Protocol — Final QA Driver

**Goal:** Start the REAL app. Drive every surface. Verify every claim in HANDOVER-LOOM.md.
Plan bugfixes. Fix nothing.

### Checklist

#### CLI
```
dotnet run --project src/DevContext.Cli --no-build -- report <dogfood> -o out.md
```
- [ ] "MAP" section present
- [ ] "TRACE" section present (checkout flow)
- [ ] 436n/338e/34e/6SL/69% within ±5%
- [ ] Screenshot: `eval-results/<date>/ui/cli-report.md`

#### MCP Server
```
dotnet run --project src/DevContext.Mcp
```
- [ ] `node eval/mcp-qa/run.js` → 8/8 pass
- [ ] `node eval/mcp-qa/run-cold.js` → ≥90% actionable
- [ ] Screenshot: `eval-results/<date>/ui/mcp-results.txt`

#### UI (pnpm dev:web, analyze dogfood)
- [ ] Home page: ServiceMapHero with 6 services, no library cards
- [ ] Home: top flows, onboarding row, three tiles
- [ ] Atlas: 6 sections, flow steppers, event wiring, export
- [ ] Explore: 3-pane (deck/canvas/inspector), lens switcher
- [ ] Explore: Code tab shows source via read_source RPC
- [ ] Table: Shift+E shortcut, CDK-virtualized, CSV export
- [ ] MCP page: sessions list, toggle, live feed, try-a-tool console
- [ ] Context Studio: scope tree, preset scaffolds cards, budget panel
- [ ] Settings: Dark/Light/System toggle works
- [ ] Screenshots: `eval-results/<date>/ui/page-*.png` (one per page)

#### Truth Tests
- [ ] `dotnet test --filter "Category=Truth"` → ≥10P/1S
- [ ] No unexpected failures

#### Bench
- [ ] `powershell -File scripts/bench.ps1` → 21/22 OK, 1 skip (DntSite)

#### Loom Guards
- [ ] `powershell -File scripts/loom-guards.ps1` → 0 banned patterns

### Product Claims Verification (HANDOVER-LOOM.md §8)

| Claim | Expected | Verified? | Evidence |
|-------|----------|-----------|----------|
| 1 — Wiring truth v2 | Checkout traces across 3 services, cold | ✅/⚠️/❌ | Screenshot |
| 2 — .NET lens devs enjoy | Tabs 32px, code pane, table lens, studio | ✅/⚠️/❌ | Screenshot |
| 3 — Agent surface cold | ≥90% actionable, ≤3 calls | ✅/⚠️/❌ | MCP QA output |

### Bugfix Planning

For every ❌ DEVIATES or ⚠️ CONFORMS-WITH-FINDINGS found:

| # | Gap | Surface | Effort | Files | Priority |
|---|-----|---------|--------|-------|----------|
| 1 | ... | Home | 30 min | src/... | P1 |

Output: `docs/qa-reports/QA-BUGFIXES.md`

### Outputs

- `docs/qa-reports/QA-FINAL-LOOM.md` — per-surface verdicts, screenshots, claim verification
- `docs/qa-reports/QA-BUGFIXES.md` — sized bugfix plan for next engineer

**Gate:** Both reports committed. At least one screenshot per UI page exists.
All 3 product claims from HANDOVER-LOOM.md §8 either verified or documented.

---

## Rating Taxonomy Reference (Phases 2 & 3)

| Rating | Meaning | Next Action |
|--------|---------|-------------|
| ✅ CONFORMS | Matches design doc. No gaps. | None. |
| ⚠️ CONFORMS-WITH-FINDINGS | Minor doc/cosmetic gaps. | Document findings. No code change. |
| ❌ DEVIATES | Material gap vs design doc. | Report: exact doc section, what delivered, fix estimate (effort + files). |

| Stage Verdict | Condition |
|---------------|-----------|
| ✅ PASS | All checkpoints CONFORMS or CONFORMS-WITH-FINDINGS |
| ⚠️ PASS-WITH-FINDINGS | At most 1 DEVIATES with clear fix path |
| ❌ FAIL | ≥2 DEVIATES or core functionality broken |
