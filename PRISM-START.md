# Prism — Phase Tracker (OPEN at D1, 2026-07-17)

**Phase plan:** `docs/dev/briefs/proposal-prism.md` (5 big deliveries + standing QA cadence — read
§1 for the rules, §2 for delivery specs, §3 for the finding→delivery traceability table).
**Audit truth:** `eval-results/2026-07-17/lens-audit/AUDIT.md` (findings A–H) +
`EXPERIENCE-ADDENDUM.md` (I–N). **Predecessor:** `docs/dev/HANDOVER-TAPESTRY.md`.
Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src` · second pole: `C:\code\shamshir`.

Branch scheme: `feat/prism-d<n>`, merged to develop per delivery on `GATE: PASS` + the delivery's
DoD. `feat/prism-d1` is cut from `audit/library-round` (= develop `9ee401a` + the 3 audit/proposal
docs commits) — the audit evidence rides into develop with the D1 merge.

## Handoff (running)

last: 2026-07-17 **D1 delivery session LAUNCHED (orchestrated).** Tracker created; `feat/prism-d1`
cut from `audit/library-round` tip `f28790d`; github-ready worktree pruned; octet SHAs pinned below;
baseline battery **GATE: PASS** on the D1 tip (`prism-d1/gates-d1-open-baseline.txt`, eval re-ran
full 10m36s∥4m15s, fresh stamp written). Operating model revised (owner call): orchestrated visible
sessions, QA deferred to phase end, stacked branch train, single merge.
next: D1 work — harness first (D1.0), then archetype/render honesty (D1.1), entry surfaces (D1.2),
per-service style rungs (D1.3), hygiene riders (D1.4).
gotchas (standing, carried from Tapestry): fast-suite load-flake (1 fail right after dotnet churn,
green when quiet, name uncaptured — watch battery logs); PS 5.1 × UTF-8 em-dashes in detached
scripts (keep battery scripts ASCII); dogfood PRE-EXISTING mods stand — never restore; never
build/test in a worktree while its battery runs; rebuild the CLI after any Core edit;
absolute CLI paths only.

## Operating model (REVISED 2026-07-17 at D1 open — supersedes proposal §1 QA cadence, owner call)

- **Orchestrated phase.** An orchestrator session spawns one visible Claude Code session per
  delivery (models per proposal §1 table), watches via tracker/git/evidence (never transcripts),
  and closes sessions when their delivery is done. **Channel discipline: the orchestrator writes
  `PRISM-INBOX.md` ONLY; delivery sessions write this tracker + code.** Delivery sessions re-read
  `PRISM-INBOX.md` at every checkpoint boundary and treat its entries as orchestrator instructions.
- **QA deferred to phase end (owner call).** NO per-delivery full battery, NO octet re-runs, NO
  QA-back between deliveries. Per-commit cheap gates REMAIN mandatory: `dotnet build` 0w/0e +
  fast tests (`--filter "Category!=Eval"`) + `scripts/loom-guards.ps1`. One massive phase-end QA
  (D5): full battery + octet harness + insight-validity + poles drift diff + clean-clone.
  Exception: a delivery whose DoD *is* an octet claim (D1) runs `eval/lens-audit.ps1 octet` once
  at its close as the DoD proof.
- **Branch train, single merge.** `feat/prism-d1 → d2 → d3 → d4 → d5` stacked (each off the
  previous tip, like Tapestry T4–T8). ONE merge to develop after phase-end QA passes, with owner
  sign-off. develop is not touched mid-phase.
- **When a session's context runs low:** finish the current checkpoint, update the handoff block
  (`D<n> SESSION <k> CLOSED — continue at D<n>.<x>`), commit, stop. The orchestrator chains a
  fresh session from tracker state. When a delivery's checkpoints are all VERIFIED: write
  `D<n> DELIVERY CLOSED` in the handoff, commit, stop.
- Evidence per delivery under `eval-results/<date>/prism-d<n>/`. Truth ratchets only tighten;
  Tapestry poles byte-identical unless a DoD says otherwise; **no new bare `catch` in Core**
  (loom-guards ban lands in D1.0).

## The octet (pinned)

Audit clones live at
`C:\Users\shahi\AppData\Local\Temp\claude\C--code-DevContext2\21fab51e-9c82-4278-8271-a302683a111a\scratchpad\repos\`
(session temp — MAY VANISH; re-clone at these SHAs if gone). D1.0 gives them a stable home +
pins these rows in `eval/README.md`.

| Repo | Pinned SHA | Origin | Intended verdict (aspirational until D1) |
|------|-----------|--------|------------------------------------------|
| Newtonsoft.Json | `4f73e74372445108d2c1bda37b36e6f5e43402e0` | JamesNK/Newtonsoft.Json | Library (aux console ≠ App) |
| refit | `71634f2c5d0845c311b1cf4f4bb512437fe86fb5` | reactiveui/refit | Library (already PASS on CLI) |
| StackExchange.Redis | `0b03ed1d12a6a783873a44cd1f6fad3acf54395f` | StackExchange/StackExchange.Redis | Library (toys/ = aux hosts) |
| wolverine | `7019b7d1b4520f84f90adbc6d407998c85e5e750` | JasperFx/wolverine | Framework-library (SelfNamePatterns) |
| GitVersion | `6476e5c478ec1b56a45914b3af4f6edcfd20deb0` | GitTools/GitVersion | CliTool (new archetype) |
| dotnet-podcasts | `5ee8be2990b81eb681bbd100875c263aaa5ab68a` | microsoft/dotnet-podcasts | App: hub entry + MAUI present, grouped routes |
| ScreenToGif | `27a49c3be69486f2db964290f4f2274e790fb687` | NickeManarin/ScreenToGif | Desktop, MVVM style rung |
| bitwarden-server | `3e79593151787eb94853cb29420530d32f9b543c` | bitwarden/server | App: per-service styles ≤2/17 Unknown, hub entry |

## Delivery table

| Delivery | Theme | Findings | Status |
|----------|-------|----------|--------|
| **D1** | Archetype truth + entry surfaces + style rungs (engine) | A1–A5, B1–B6, C4, C6, E1–E5 | **IN PROGRESS** (opened 2026-07-17) |
| D2 | Graph depth + self-health + insight validity (engine) | C1–C3, C5, I1–I2, J1, J3, G1-dep, D3 | TODO |
| D3 | Cache resurrection + living waterfall + compiler lever | J2, K1–K2, L2, L7, D1–D2, perf lever | TODO |
| D4 | Visual intelligence + library workbench + Studio/nav (app) | F1–F6, L1, L3–L6, M | TODO |
| D5 | MCP polish + cross-platform + final hardening | G1, H1–H3, phase QA | TODO |

## D1 checkpoint table

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED · VERIFIED. Evidence under
`eval-results/2026-07-17/prism-d1/`. A checkpoint without a fresh artifact is not DONE.

### D1.0 — Harness first (it gates everything)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.0a | Octet SHAs pinned in `eval/README.md` + stable clone home | TODO | | |
| D1.0b | Aspirational expectation rows for intended verdicts (table above) | TODO | | |
| D1.0c | `eval/lens-audit.ps1 <repo\|octet>`: timed analyze → captures → MCP drive → FAIL probes (map-tokens ≪ repo size; Unknown+0-entries; sample rows in per-service; wall-time vs baseline) | TODO | | |
| D1.0d | Bare-`catch` ban in `scripts/loom-guards.ps1` (Core; existing swallows grandfathered until D2) | TODO | | |

### D1.1 — Archetype & render honesty (A1–A5, E2)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.1a | A1 transitive aux-exe references (Newtonsoft TestConsole → Tests → lib) | TODO | | |
| D1.1b | A2/A3 `toys`/build-tooling NoiseFilter rungs; holder csproj (`.github`/`docs`/`docker`) excluded everywhere; topology applies per-service filters (E2) | TODO | | |
| D1.1c | A4 catalog self-name audit — SelfNamePatterns wherever nuget id ≠ project names, wolverine first; runnable-service inference honors NoiseFilter unless SamplesAreTheProduct | TODO | | |
| D1.1d | A3/B4 `Archetype.CliTool`: Exe + no web surfaces + PackAsTool/parser evidence → command-surface render; plain `Main()` becomes an entry | TODO | | |
| D1.1e | A5 render backstop — no dead maps: 0 entries + public surface ⇒ library sections; + Main ⇒ console view; harness FAILs any <~400-token map on a >100-file repo | TODO | | |

### D1.2 — Entry surfaces 2026 (B1–B6, C6)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.2a | B1 in-framework SignalR: `MapHub<T>`/`: Hub` evidence, package-free (podcasts ListenTogetherHub + bitwarden NotificationsHub) | TODO | | |
| D1.2b | B2 MAUI catalog descriptor + `UseMaui`/TFM probe; pages/shell as UiEntries | TODO | | |
| D1.2c | B3 MapGroup prefix composition into routes (fixes map, flows, Studio picker, MCP addressing at one locus) | TODO | | |
| D1.2d | B5 queue seams: Storage-Queue/ASB/RabbitMQ senders + hosted consumers as `[approx]` channel edges | TODO | | |
| D1.2e | B6 honest branding — hand-rolled `IRequestHandler` ⇒ "CQRS (hand-rolled)" | TODO | | |
| D1.2f | C6 entry target attribution fidelity (`GET / → ShowClient.CheckLink`; interface-as-target) | TODO | | |

### D1.3 — Per-service style rungs (C4)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.3a | owns-endpoints ⇒ Web API/MVC; owns-hubs ⇒ SignalR host; IdentityServer/OpenIddict ⇒ Identity provider; ViewModel+WPF ⇒ Desktop MVVM; `Api` without dot; in-framework Razor-Pages probe | TODO | | |

### D1.4 — Hygiene riders (E1, E3–E5)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.4a | E3 duplicate-name disambiguation (`Messages` ×6, `AppHost` ×2) | TODO | | |
| D1.4b | E5 TFM summarization (no raw TFM matrices in STACK) | TODO | | |
| D1.4c | E1 skip `Update=`/MSBuild-expression package refs | TODO | | |
| D1.4d | E4 remove the `--profile debug` ghost hint | TODO | | |

### D1 Definition of Done (from proposal §2)
- Octet expectation rows for archetype/style/entries flip aspirational→expected.
- Newtonsoft / SE.Redis / GitVersion / wolverine render real lenses.
- podcasts hub + MAUI present; bitwarden per-service ≤2/17 Unknown; zero bare-`/` grouped routes.
- MediatR-class repos + Tapestry poles byte-identical.
- Per-commit cheap gates green throughout; `eval/lens-audit.ps1 octet` run at close as the DoD
  proof. (Full battery deferred to phase-end QA — operating model above.)

## Baseline drift table (poles — must stay byte-identical through D1)

| Repo | Nodes/Edges/Entries | Style | Note |
|------|--------------------|-------|------|
| dogfood (eshop-microservices) | 439 / 339 / 34 | Microservices (App) | PRE-EXISTING local mods — never restore |
| eShop | 1089 / 837 / 109 | Microservices 0.91 | unchanged since T2.5 |
| shamshir | ~2882 / 3375 / 135 | NLayer 0.6 | live repo; <1.2% drift/session normal |
| TodoApi | 123 / 81 / 12 | MinimalApi | |
| aspire-samples | 68 / 34 / 5 | SampleCollection | T8.2 fix |

Octet "before" snapshot (the thing D1 must flip): Newtonsoft ❌ App/0-entries/19-line map ·
SE.Redis ❌ App/MinimalApi-from-toys · GitVersion ❌ App/0-entries/empty map · wolverine ❌
App/CleanArchitecture-from-samples/80 sample rows · podcasts ⚠ hub+MAUI missing, `GET /` ×5 ·
bitwarden ⚠ 17/17 Unknown, hub missing · ScreenToGif ⚠ style Unknown · refit ✅ (CLI).
Full scorecard: AUDIT.md §Per-repo scorecard.
