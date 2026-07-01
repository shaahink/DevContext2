# Iteration I7 — Benchmark expansion + the insights audit (the closing loop)

> **Status: BLOCKED on I3 (+ whichever I5 picks shipped)** · Phase: V5.4-adjacent · One session,
> repeatable — this is the iteration you re-run whenever a batch of features lands.

## Goal

Extend the standing benchmark/eval to the shapes the program now claims, run everything, and audit the
*output quality* (are insights/facets non-boring and correct?) — not just perf. Produces the report
that tells us what the next iteration should be.

## Step 1 — Extend the standing suites

Current macro-bench suite: DntSite · TodoApi · VerticalSlice · eShop.Ordering.API · AutoMapper ·
OrchardCore (`benchmarks/`, see `devcontext-bench` skill). Current eval: 28 expectation files.
Add one small canonical repo per newly-claimed shape (clone under `eval-repos/`, register in
`eval-repos.json` + expectations + bench suite):

| Shape | Candidate | Must assert |
|---|---|---|
| Messaging app | a small MassTransit sample service | Bus entries + F3 matrix + `wiring.external-events` |
| CLI tool | `spectreconsole/examples` app or `dotnet-outdated` | CliCommand entries (+F10 tree if picked) |
| Blazor app | `blazor-samples/9.0/BlazorWebAppMovies` (already vendored) | routes + insights quiet-where-appropriate |
| Desktop | a small WPF/MAUI sample | UiEntry + archetype App |
| Gateway consumer | an Ocelot sample gateway | Gateway + ROUTES table |
| Worker/serverless | a Functions isolated-worker sample | trigger entries (+F11 detail if picked) |

Keep them SMALL (bench time budget: full suite ≤ ~10 min). Perf rows go into
`benchmarks/results/baseline.md` — re-baseline deliberately, never silently.

## Step 2 — Run

`eval/gates.ps1` + macro bench (`… -- repos`) over the extended suite. Capture per-repo Map + insights
+ one trace into `docs/dev/go-to-program/audit-runs/<date>/`.

## Step 3 — The insights audit (the human-judgment pass)

For each repo, score the captured output against three questions, in a table in
`audit-runs/<date>/INSIGHTS-AUDIT.md`:

1. **Would a dev say any insight out loud?** (the §3.1 bar) — list the best and the most boring.
2. **Is anything WRONG?** (fabrication severity > boring) — every wrong item becomes a negative
   eval expectation in the same PR.
3. **What's the one missing thing a maintainer of THIS repo would want?** — feeds the next menu pick.

This replaces "push more stats and hope" with a measured loop: features → run → judge → ratchet →
next pick. Re-run this iteration after every 2–3 menu picks.

## Gate

Extended gates green · bench baseline updated with commentary · INSIGHTS-AUDIT.md committed with
at least one ratcheted expectation and one named next-pick recommendation.
