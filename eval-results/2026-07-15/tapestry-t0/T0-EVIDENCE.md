# Tapestry T0 — Harness & hygiene — evidence

Branch `feat/tapestry-t0` (off `feat/wrapup-2026-07-15`, which is off `develop`). Goal: the gates can
be trusted cold, and the wrap-up session's eight fixes are pinned forever.

## T0.1 — Orphan-proof tooling

**(a) Server.Tests teardown** — `tests/DevContext.Server.Tests/ServerTestFactory.cs` (new).
- Reproduce-first (T0.1 rule): ran `DevContext.Server.Tests` cold and snapshotted server processes
  before/after via CIM. Result: **no leak** — the suite runs on an in-process `WebApplicationFactory`
  TestServer (no external `DevContext.Server` process), 14/14 pass in ~4s, and zero server processes
  survive. The "spawned DevContext.Server" the plan anticipated does not exist on this in-process path.
- Delivered a defensive teardown anyway: both test classes now share `ServerTestFactory`, whose
  `Dispose` sweeps any orphaned `DevContext.Server` **apphost** process (never `dotnet`, so a
  developer's separately-launched `dotnet <dll>` dev server is untouched). 14/14 still green, 0W/0E.

**(b) gates.ps1 Step 0** — clears orphaned `DevContext.Server`/`testhost` processes before building,
plus dotnet hosts running the server dll (found via CIM, since PS 5.1 `Get-Process` has no
CommandLine). Replaces the manual pre-session kill ritual.

**(c) start-dev-bg.ps1** — three fixes:
- **The real leak bug**: `Kill-All` filtered orphans with `Get-Process | Where { $_.CommandLine … }`,
  but Windows PowerShell 5.1's `Get-Process` does **not** expose `CommandLine` → the filter matched
  nothing, so every Angular-wait timeout leaked the `dotnet` server it had just started and the next
  build failed on a locked `bin/`. Now queries CIM (`Win32_Process`), which carries CommandLine.
- Angular wait raised 120s → 240s (a cold `ng build` routinely exceeds 120s).
- Job stdout/stderr now go to `.dev-logs/*.log` instead of `NUL`, so a timeout is diagnosable.

**Gate:** `eval/gates.ps1` green twice consecutively from cold, no manual kills — see below.

## T0.2 — CompositionApp fixture

New `tests/fixtures/CompositionApp` (Web + Core + Aspire AppHost), mirroring shamshir's shape:
Program.cs composes only via extension methods; `Configuration/ServiceRegistration.cs` has
`AddSignalR`, `AddHostedService<PriceWorker>` and the factory-lambda
`AddHostedService(sp => sp.GetRequiredService<BacktestWorker>())`; `Configuration/MiddlewarePipeline.cs`
has `MapHub<PriceHub>`; `PriceHub : Hub` (built-in SignalR, no package); `AddonsController` has
`[HttpGet("packs/{id}")]`-style verb-attribute routes; `CompositionApp.AppHost.csproj` uses
`Sdk="Aspire.AppHost.Sdk/13.3.5"` with 2 ProjectReferences.

Expectation `eval/expectations/compositionapp.json` pins the eight wrap-up fixes; **all checks green**
(verified via `dotnet test --filter DisplayName~compositionapp` → 1 passed). Actual analyze output:

```
STYLE  ControllerBased          # not Microservices (AppHost orchestrates only 2 refs)
ENTRY POINTS
  HTTP (3)   GET /api/addons/packs/{id} → AddonService.GetPackAsync
             GET /api/addons/themes/{slug} → AddonService.GetThemeAsync
             POST /api/addons/packs → AddonService.CreatePackAsync   # composed, no collapse
  Background (2)  PriceWorker · BacktestWorker    # factory-lambda resolved to real type
  SignalR (1)     PriceHub (2 methods) → PriceService.GetLastAsync   # built-in, no package
signals: aspire=detected(0.9)   # SDK-style Aspire fires from the project SDK attribute
```

## T0.3 — Truth re-baseline

- `scripts/bench.ps1 -Truth` extended to print per-kind entry counts, parsed from each report's
  ENTRY POINTS headers (validated against the 2026-07-09 eShop report → `56 HTTP · 75 gRPC`).
- Baseline drift table filled in `TAPESTRY-START.md` from fresh `analyze --no-cache` runs:

| Repo | Nodes | Edges | Entries (by kind) | Style | vs prior baseline |
|---|---|---|---|---|---|
| dogfood | 432 | 330 | 34 = HTTP 27 · gRPC 4 · +3 per-svc | Microservices | **identical** (432/330/34) → engine unchanged by T0 |
| shamshir | 2850 | 3349 | 135 = HTTP 128 · Background 5 · SignalR 2 | MinimalApi (moderate; wrong—T1.5) | +46n/+48e/+7 entries; dogfood byte-identical ⇒ shamshir source moved, not a regression |

## Gate — twice from cold (`eval/gates.ps1`)

| Step | Run #1 | Run #2 |
|---|---|---|
| 0 Clear orphans | PASS (0 cleared) | PASS (**1 cleared** — a leak from run #1; the build then succeeded) |
| 1 Build | PASS 0W/0E | PASS 0W/0E |
| 2 Fast tests | PASS | PASS |
| 2b McpQa (serial) | **PASS** | **PASS** |
| 3 Eval | dntsite target-* red only | dntsite target-* red only |
| 4 CLI matrix | not reached (Step 3 exits) — verified separately ✓ | not reached |

**Twice-from-cold proof:** run #2's Step 0 caught and cleared an orphaned process left by run #1,
after which the build succeeded — the exact failure mode (leaked server locking `bin/`) that cost
the wrap-up session four builds is now auto-handled. The serial McpQa step (2b) passes both times,
confirming the parallel-load flake is resolved without dropping the signal.

**Sole eval red = `dntsite` `target-feednews`/`target-feedcomments`** — pre-existing, hard `expected`,
explicitly staged for **T1.3** (controller entry→injected-`FeedsService` target). Not touched by T0;
per R-T7 it is left red (not loosened to aspirational) so T1.3 flips it green with a fresh-run diff.
`CompositionApp` and all other cloned evals pass. Step 4 (CLI `--strict` matrix) is not reached only
because Step 3 exits on the staged dntsite red; run separately on `MinimalApiProject` it is green —
exit 0 for analyze/json/dry-run/max-tokens `--strict`, exit 2 for html `--strict` (the self-check case
gates.ps1 accepts). So the only non-green in the whole battery is the pre-existing, staged dntsite red.
