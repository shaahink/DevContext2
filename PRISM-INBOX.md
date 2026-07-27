# PRISM-INBOX — orchestrator → delivery-session channel

**Protocol:** the active delivery session RE-READS this file at every checkpoint boundary (before
starting the next checkpoint) and treats entries as orchestrator instructions. Newest entries at
the top, dated. Only the orchestrator writes this file; delivery sessions write the tracker
(`PRISM-START.md` handoff block + checkpoint rows) and code. Acknowledge an entry by naming it in
your next tracker handoff update.

---

## 2026-07-17 — Standing orders for D1 (session 1)

You are the Prism D1 delivery session: **archetype truth + entry surfaces + style rungs (engine)**.
Model context: you were launched by an orchestrator; the human owner may also be watching your
window. Baseline on your branch tip is green (full battery GATE: PASS).

1. **Read first:** `PRISM-START.md` (your checkpoint table D1.0–D1.4 + operating model + octet
   pins), `docs/dev/briefs/proposal-prism.md` §2-D1, audit findings A/B/C4/C6/E in
   `eval-results/2026-07-17/lens-audit/AUDIT.md`, `AGENTS.md` (hard rules + T-rules). The
   dev-pipeline skill (`.claude/skills/dev-pipeline/SKILL.md`) covers build/test mechanics.
2. **Work order:** D1.0 (harness — it gates everything) → D1.1 → D1.2 → D1.3 → D1.4, top to
   bottom. Branch `feat/prism-d1` is already checked out in this directory.
3. **Per checkpoint:** implement → cheap gates (`dotnet build` 0w/0e + fast tests + loom-guards)
   → commit (docs + tracker row update in the SAME commit) → re-read this file.
4. **NO full battery, NO octet re-runs mid-delivery** (phase QA is deferred — operating model in
   the tracker). Exception: `eval/lens-audit.ps1 octet` ONCE at delivery close as the DoD proof.
5. **Octet clones:** copy them from the scratchpad path in the tracker to a stable home as D1.0a
   (or re-clone at the pinned SHAs if the scratchpad is gone).
6. **Context low?** Finish the current checkpoint, update the tracker handoff
   (`D1 SESSION 1 CLOSED — continue at D1.x`), commit, stop. Don't start a checkpoint you can't
   finish.
7. **All D1 checkpoints VERIFIED?** Run the DoD proof (point 4), update the drift table's octet
   row, write `D1 DELIVERY CLOSED` in the tracker handoff, commit, stop.
8. Poles stay byte-identical (dogfood/eShop/shamshir/TodoApi/aspire-samples — table in tracker).
   Rebuild the CLI after every Core edit. Absolute CLI paths only. Never restore dogfood's
   pre-existing local mods.
