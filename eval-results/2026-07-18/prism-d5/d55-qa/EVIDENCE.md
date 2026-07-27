# D5.5 — PHASE QA (full battery + octet + poles + clean-clone)

## Full gate battery

- **Run 1 (`gates-d55-full.txt`): GATE: FAIL at eval — a REAL catch.** The D5.3 canonical
  call-edge order (callee-name-led) flipped ControllerApp's POST target to the alphabetically
  first collaborator (`AuditService.RecordAsync` over `ProductService.CreateAsync`): the entry
  "primary call" pick keeps the FIRST service callee, so within-member SOURCE order is semantic.
  Fixed @ 208ed50 — call-edge canon now leads with the call site (file, then NUMERIC line);
  ControllerApp restored, dogfood A/B still byte-identical, +1 unit pin
  (`Call_edge_canonical_order_is_source_order_not_callee_name_order`).
- **Run 2 (`gates-d55-full-run2.txt`): GATE: PASS unqualified, eval stamp WRITTEN** — build
  0w/0e, fast suite, McpQa serial, full eval BOTH hosts green (poles ride the cohort), CLI
  matrix, CLI query ops, pnpm check.

## Octet lens-audit (P1–P7) — `octet-d55-proof.txt`

**LENS-AUDIT: PASS (8/8)**, all archetypes intended, P6 MCP-drive navigation probes green
octet-wide (the D5 DoD "zero empty navigations / silent breaches" instrument), P7
insight-validity quiet:

| repo | wall | archetype (got/intended) | style |
|---|---|---|---|
| Newtonsoft.Json | 22.1s | Library/Library | Unknown |
| refit | 14.2s | Library/Library | ControllerBased |
| StackExchange.Redis | 22.1s | Library/Library | MinimalApi |
| wolverine | 95.6s | Library/Library | CleanArchitecture |
| GitVersion | 16.2s | CliTool/CliTool | Unknown |
| dotnet-podcasts | 10.2s | App/App | CleanArchitecture |
| ScreenToGif | 14.1s | Desktop/Desktop | DesktopMvvm |
| bitwarden-server | 173.2s | App/App (cap 522s) | Microservices |

## Poles

Ride the eval cohort inside the battery (both hosts green, run 2). The D5.3 determinism A/B
additionally byte-diffed dogfood ×3 and bitwarden ×2 fresh — all identical
(`d53-determinism/EVIDENCE.md`).

## Clean-clone battery — `gates-d55-cleanclone*.txt`

Fresh `git clone --branch feat/prism-d5` into `C:\code\DevContext2-cc`,
`pnpm install --frozen-lockfile` (568 packages), then the FULL battery from the clone
(no eval stamp — eval runs fresh).

- **Run 1 (at 208ed50): GATE: FAIL at eval — a REAL harness catch.** A fresh clone materializes
  eval-repos gitlinks as EMPTY directories (the T8.3 finding), and `TraceQualityTests` still
  guarded with bare `Directory.Exists` — two tests RAN against an empty `eval-repos/TodoApi` /
  `eval-repos/eShop` and failed the gate instead of skipping. (`EvalExpectationTests` and
  `TruthExpectationTests` already carried the exists-AND-non-empty guard and skipped correctly —
  this class was the one T8.3 missed.) Fixed @ 24ce626: `RepoAvailable` = exists AND non-empty,
  all 7 guard sites; 11/11 green against the REAL repos in the main tree (the guard change skips
  nothing where content exists).
- **Run 2 (at 24ce626): GATE: PASS on every step** — build, fast suite, McpQa serial, eval fresh
  (stamp written in the clone), CLI matrix, query ops, pnpm check (`gates-d55-cleanclone-run2.txt`).
  Cleanup gotcha (3rd occurrence this phase, same class): an orphaned `DevContext.Server` from the
  clone's McpQa held the directory — killed by path filter, clone removed.

## Known-latent observations recorded (phase-QA duty)

- eShop deep-trace render stops at the IntegrationEventLogEF send seam — pre-existing since D3,
  render-depth thread, not a graph defect (byte-identical guardrail held through D3.4).
- `analyze --format html --strict` exit 2 = pre-existing allowed state (self-check failures),
  noted by every gate since D2, unchanged.
- 3 pre-existing truth-ratchet skips (pending fixture materialization) — unchanged.
