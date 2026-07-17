---
name: devcontext-eval-audit
description: Audit DevContext's output quality against a real .NET repo. Use when asked to evaluate, audit, or sanity-check what DevContext produces for a repo (Map/Trace fidelity), compare against expectations or a recorded baseline, find detection defects, or verify a fix. Codifies the worked DntSite audit.
---

An eval-audit runs DevContext over a real repo, captures the Map + focused Traces, compares them to
the recorded ground truth, and writes a structured findings report. Paths are relative to the repo
root. Shell is **Windows PowerShell 5.1**. Worked examples: `eval-results/DntSite/AUDIT.md`
(single-repo) and `eval-results/2026-07-17/lens-audit/AUDIT.md` (the unseen-octet round — the
richer template for phase-level audits).

## Unseen-repo round protocol (proven 2026-07-17)

For "are we the lens yet"-class audits, seen fixtures only re-confirm yesterday's blind spots.
Instead:

1. **Pick unseen repos** — never in `eval-repos/`, the clone cache, or prior audits — spanning repo
   SHAPES (classic library, source-gen library, client library, framework+samples, CLI tool,
   multi-surface app, WPF/MAUI desktop, large multi-service app). Shallow-clone at HEAD into the
   scratchpad; record SHAs.
2. **Drive all four surfaces per repo in one session** — CLI map + traces, MCP transcript
   (`eval/mcp-qa/drive-generic.js <repo> <out>` — 22-tool session with token counts), desktop UI
   screenshots, then a ground-truth code read. The highest-value findings are CROSS-SURFACE DIFFS
   (e.g. CLI renders a full library lens while the UI shows an empty entry dashboard).
3. **Judge against "what would an honest lens say"**, not expectations files. Verify claims in the
   output against the repo's code (a style evidence line naming MediatR ⇒ grep the csproj refs).
4. **Automatic FAIL probes:** map token count ≪ repo size (a 945-file repo rendering ~209 tokens);
   `STYLE Unknown` + 0 entries on any repo with a public surface; per-service tables listing
   sample/test/build-infra projects; wall time vs recorded baseline.
5. Time every analyze (stdout stats line carries wall seconds); one sequential timed batch first,
   traces second (each CLI `--focus` re-analyzes — budget for it on big repos).

## Inputs / ground truth

- **Local repos:** `eval-repos/` (TodoApi, VerticalSlice, eShop, AutoMapper, OrchardCore, …) and the
  GitHub clone cache `C:\Users\<you>\AppData\Local\DevContext\repos\<owner>-<repo>-default`.
- **Expectations:** `eval-repos.json` (per repo: expected architecture, endpoint count, signals,
  entry points, workers). The machine eval is `tests/DevContext.Core.Tests` (`Category=Eval`,
  `EvalExpectationTests`, goldens in `tests/goldens/`).
- **North star / assessment:** `docs/product/IDEAL-OUTPUT-TARGET.md`, `docs/dev/archive/reports/OUTPUT-QUALITY-ASSESSMENT.md`.
- **Aspirational checks (1 remaining):** VerticalSlice `no-dynamic` (FastEndpoints routes `<dynamic>`).
  3 previously aspirational checks flipped to `expected` in `chore/housekeeping-stats`: eShop arch-style,
  eShop aspire-signal, VerticalSlice mediatr-signal.
- **Known gaps (post-Phases 0–6):** DntSite TOUCHES (entities registered via `RegisterAllDerivedEntities`
  reflection — subtype expansion applied in Iteration 6 but local-variable receiver resolution still
  drops some `Calls` edges); semantic Sends/Raises (body-scan stays `[approx]` — deferred semantic tier);
  persistent graph cache (Phase 9). eShop CQRS entry→target works; controllers 34/94 on DntSite.
  See `docs/dev/HANDOVER.md` for the full status + `docs/dev/audit/` for the original findings.

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
green (`dotnet test DevContext.slnx --filter "Category!=Eval"` · build 0-warn).

## Gotchas

- **Absolute paths** to the CLI (relative = GitHub clone attempt). **Rebuild CLI after a Core edit.**
- Capture with `Out-File -Encoding utf8` after setting `[Console]::OutputEncoding`; don't pipe the CLI
  through `Select-Object -First N` (truncates the pipe, corrupts the exit code).
- The legacy `eval-results/<Repo>/*.md` may predate the graph Map/Trace renderer — it won't line-diff;
  compare on **facts** (counts, style, seams), not bytes.
- `TraceQualityTests` asserts only a few substrings — manually verify deep/cross-project trace hops.
- **UI drive legs (Playwright):** use `waitUntil: 'domcontentloaded'` — the MCP page holds a live
  feed and never reaches `networkidle`. A second repo needs an explicit top-bar **New** click, or the
  drive silently re-tours the previous session (screenshots look plausible but are the wrong repo).
  Launch node drives from PowerShell, not bash — MSYS mangles `cmd /d /c` flags into drive paths.
- **MCP drive:** spawn `src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe` over stdio JSON-RPC
  (`eval/mcp-qa/drive-generic.js` is the generic driver); log per-call token estimates — budget
  anomalies (a 6000-budget pack returning ~160 tokens) are findings, not noise.
