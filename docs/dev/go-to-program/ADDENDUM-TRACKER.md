# Addendum Tracker — pass-3 extension of the frozen plan

> Authored 2026-07-02 · The base plan (README tracker + I1–I7 + FACES-DESIGN + DEV-PAINS +
> UI-UX-GUIDELINES) is **frozen and in execution** (`go-to/implement-iterations`, executor's
> `UNIFIED-TRACKER.md`). This file EXTENDS the tracker — merge its rows into README/UNIFIED-TRACKER
> whenever the executor next touches them; until then this is the authority for I8+.

## New iteration rows

| # | Guide | Depends on | Tier | Status |
|---|---|---|---|---|
| I8 | [Caching & storage](ITERATION-I8-caching-storage.md) — repo-hash snapshot cache, clone consolidation, Settings→Storage | I2 | **CORE** (unblocks I10 + instant re-opens) | NOT STARTED |
| I9 | [Release readiness](ITERATION-I9-release-readiness.md) — about/updates/logs/errors table stakes, CLI polish floor | I4, I8 | **CORE** (pre-release gate) | NOT STARTED |
| I10 | [Workspace tabs](ITERATION-I10-workspace-tabs.md) — up to 6 repos, VS Code-grade strip, memory-honest | I4 (+I8 for full cap) | **CORE** (user-requested) | NOT STARTED |
| A | [Harder repos](ADDENDUM-A-harder-repos.md) — F14 EF depth ★, F15 build intelligence ★ (2 bug-grade items), extended insights, F12→LATER | I1, I2 | MENU additions (F14/F15 at ★) | NOT STARTED |

**Updated CORE spine:** I1 → I2 → I3 → I4 → **I8 → I10 → I9** (+ I6 MCP anywhere after I2;
I5/A picks interleave; I7 closes each batch and now includes A5's repos). Rationale: I8 before I10
(tabs need parked-session rehydration), I9 last (release gate audits everything shipped before it).
Note: F15's CPM/Directory.Build.props items are **bug-grade** — an executor may justifiably pull
them into any early iteration.

## Requirements traceability (audit of all user directives, sessions 2026-07-02)

| Directive (paraphrased) | Where baked |
|---|---|
| Audit engine vs go-to goal; per-repo-type value; entry/common/interesting per type | ENGINE-VALUE-AUDIT §2–4 |
| Smarter/wider wiring; harden string-matching & regex, no overfit | AUDIT §5 · I1 |
| Kernel hygiene as we grow | AUDIT §6 · I2 (W9) · FacetCatalog (I5) · DispatchSeamCatalog (I1) |
| Features backed by existing data, consolidated in UI+CLI | AUDIT §4 F1–F12 · I5 · FACES-DESIGN |
| Worktree + branch; votes documented; phased plan, must-first vs pick-later | PROGRAM-PLAN (votes recap) · README tiers · this tracker |
| MCP exposure; desktop+CLI now, web later, one engine | I6 · FACES-DESIGN §1 (one wire contract) |
| Structure + best practices + tricky-bit implementations for DeepSeek | ITERATION-I1…I10 guides (Step 0, code sketches, pitfalls, gates) |
| Check CLI signature | FACES-DESIGN §1 (26 flags classified, v2 surface) · I2 |
| Check old WPF + Angular; UI/UX ideas; evolve Angular; unify with CLI | FACES-DESIGN §2 · UI-UX-GUIDELINES (incl. §9 WPF donor checklist) · I4 |
| Dev-pains audit → features | DEV-PAINS (15 pains, tiered) |
| Stats boring → insights | FACES-DESIGN §3 · I3 · extended set in ADDENDUM-A §A4 |
| cli-reference/desktop-reference updated as we build; goldens unified | README maintenance protocol (docs-with-code, golden ratchet) — restated in every guide's gate |
| Benchmark check + final iteration: new repos, run, find insights, audit | I7 (+A5 additions) |
| Release table stakes (settings, about, general features; not CI/CD) | **I9** |
| More interesting info from the graph as it grows | ADDENDUM-A §A2/A4 (EF depth, async hygiene, obsolete-usage, secrets-smell…) |
| Caching: per repo hash/commit + internal; clone folder consolidated in settings, user in control | **I8** |
| Enough UI/UX impl notes that the agent doesn't decide alone? | UI-UX-GUIDELINES (nav model, entries spec §4, palette §6, settings §7, states §3, keyboard §8) + I10 §3 tab spec |
| "We don't develop desktop apps any more" (analysis investment) | ADDENDUM-A §A1: F12 → LATER |
| Edge = harder repos (.NET/EF Core etc.) | ADDENDUM-A: F14 ★, F15 ★ (incl. 2 correctness bugs), eShopOnWeb + CPM fixtures |
| Angular shell/communications wired properly; smooth nav; readability; stop one-scroll | UI-UX-GUIDELINES §1–3 (wiring audit + fixes; workspace routes supersede single scroll) |
| Entries filter/table review + bake suggested features | UI-UX-GUIDELINES §4 (concrete spec over the existing chips/search/table) |
| Multi-tab workspace, 5–6 tabs, memory-efficient, VS Code-grade, minimal | **I10** |
| Don't modify the in-progress plan; add complementary docs on a fresh worktree/branch | this branch `docs/go-to-program-addendum` (worktree `DevContext2-addendum`) — zero edits to frozen files |

No unbaked directives found in the message audit. The only intentionally-deferred asks remain the
LATER tier: snapshot diff (P9), tests lens (P13), huge-repo scoping (V5.3), web face.
