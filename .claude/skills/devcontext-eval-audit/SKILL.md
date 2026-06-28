---
name: devcontext-eval-audit
description: Audit DevContext's output quality against a real .NET repo. Use when asked to evaluate, audit, or sanity-check what DevContext produces for a repo (Map/Trace fidelity), compare against expectations or a recorded baseline, find detection defects, or verify a fix. Codifies the worked DntSite audit.
---

An eval-audit runs DevContext over a real repo, captures the Map + focused Traces, compares them to
the recorded ground truth, and writes a structured findings report. Paths are relative to the repo
root. Shell is **Windows PowerShell 5.1**. The worked example is `eval-results/DntSite/AUDIT.md`.

## Inputs / ground truth

- **Local repos:** `eval-repos/` (TodoApi, VerticalSlice, eShop, AutoMapper, OrchardCore, …) and the
  GitHub clone cache `C:\Users\<you>\AppData\Local\DevContext\repos\<owner>-<repo>-default`.
- **Expectations:** `eval-repos.json` (per repo: expected architecture, endpoint count, signals,
  entry points, workers). The machine eval is `tests/DevContext.Core.Tests` (`Category=Eval`,
  `EvalExpectationTests`, goldens in `tests/goldens/`).
- **North star / assessment:** `docs/IDEAL-OUTPUT-TARGET.md`, `docs/archive/reports/OUTPUT-QUALITY-ASSESSMENT.md`.
- **Aspirational checks (1 remaining):** VerticalSlice `no-dynamic` (FastEndpoints routes `<dynamic>`).
  3 previously aspirational checks flipped to `expected` in `chore/housekeeping-stats`: eShop arch-style,
  eShop aspire-signal, VerticalSlice mediatr-signal.
- **Known gaps:** eShop entry→target (`POST /api/orders/ → CreateOrderCommand`), MessageConsumer Bus
  entries (RabbitMQ `IIntegrationEventHandler`), trace TOUCHES on simple repos.
  See `docs/iterations/housekeeping-stats/HANDOVER.md` for verification guides.

## Capture (CLI, absolute paths, UTF-8)

```powershell
$cli = "src/DevContext.Cli/bin/Debug/net10.0/DevContext.Cli.dll"   # build first: dotnet build DevContext.slnx
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()       # avoid · / box-char mojibake
$r = "C:\abs\path\to\repo"
& dotnet $cli analyze $r                  2>$null | Out-File eval-results/<Repo>/map-v2.md   -Encoding utf8
& dotnet $cli analyze $r --format json    2>$null | Out-File eval-results/<Repo>/map-v2.json -Encoding utf8
& dotnet $cli analyze $r --focus "<Entry>"            2>$null | Out-File eval-results/<Repo>/trace-<x>.md -Encoding utf8
& dotnet $cli analyze $r --focus "<Entry>" --detail signature 2>$null | Out-File … # also salient/full
```

Also run **G1 Hybrid scope** both ways: at the **repo root** (whole-solution) and at a **subfolder /
`.csproj`** (closure) — they can legitimately differ (e.g. architecture style). The three `--detail`
levels (signature/salient/full) are the desktop's call-graph "Detail" control.

## Audit dimensions (compare new output ↔ expectations ↔ a code read)

| Dimension | Check |
|---|---|
| Projects / topology | test projects excluded (G6); count matches scope |
| Architecture style | matches `eval-repos.json` (root vs closure may differ); evidence-driven (not name-substring) |
| Endpoints | count + each `route → Target` (G2); minimal-API lambdas anchor their own node (G5) |
| **Entry groups** (NEW) | Domain handlers (MediatR notifications), Bus consumers (MessageConsumerDetection), Background workers — each as a separate group under ENTRY POINTS |
| **Trace summary** (NEW) | RESULT (HTTP status per verb), NEXT (lifecycle hints from emitted events), TOUCHES (entities reachable from trace), EMITS (deduped events) |
| **PipelineBehaviors** (NEW) | MediatR pipeline shown under CROSS-CUTTING when IPipelineBehavior regs exist (including AddOpenBehavior inside AddMediatR lambdas) |
| Scheduled/hosted workers | surfaced as entries (`AddScheduledTask<T>`/`AddHostedService`) |
| Archetype | App vs Library (`ArchetypeDetector`) — library → PUBLIC SURFACE, not entry inventory |
| Traces | reach the real seams **including deep cross-project ones** (Send→Handler, raises, consumes) |
| Tokens / perf | graph-shaped stats line (`N nodes · M edges · X entries · depth D`); capped packages (G9); wall time reasonable |
| **Stats page** (NEW) | Card grid layout in Desktop: Timing Waterfall, Extractors, Scorer+Token Funnel, Cache+Corpus+Parallelism+Graph |

## Write the report

`eval-results/<Repo>/AUDIT.md`: verdict → methodology → a per-dimension scorecard (✅/❌/⚠ with
evidence: file:line, counts) → each defect with **root cause + suggested fix** → net assessment.
**Preserve any prior baseline files** (don't overwrite the historical pre-Map output) — add `-v2`
captures alongside. If you then fix a defect, add a "Post-fix re-audit" section with the new evidence.

## Fix → re-verify loop

Reproduce → write/extend a failing test (or note the gap a passing test missed — e.g. shallow
substring checks) → fix at the source → re-capture → confirm the scorecard flips → ratchet
`eval/expectations/*.json` + goldens **with review** (`$env:UPDATE_GOLDENS=1`, never blind) → gate
green (Core 269/2-skip · Desktop 64 · build 0-warn).

## Gotchas

- **Absolute paths** to the CLI (relative = GitHub clone attempt). **Rebuild CLI after a Core edit.**
- Capture with `Out-File -Encoding utf8` after setting `[Console]::OutputEncoding`; don't pipe the CLI
  through `Select-Object -First N` (truncates the pipe, corrupts the exit code).
- The legacy `eval-results/<Repo>/*.md` may predate the graph Map/Trace renderer — it won't line-diff;
  compare on **facts** (counts, style, seams), not bytes.
- `TraceQualityTests` asserts only a few substrings — manually verify deep/cross-project trace hops.
