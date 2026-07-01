# Iteration I2 — CLI v2 + kernel wire format (W9 retirement)

> **Status: BLOCKED on I1** · Phase: V4 (pulled forward — the wire contract unblocks I3–I6) · One session.
> Design contract: `FACES-DESIGN.md` §1. Votes already recorded: retire the legacy catalog (option b);
> `query` = in-proc + `--attach`.

## Goal

One wire format owned by the kernel; a CLI whose verbs match the desktop and the future MCP; the dead
token/catalog machinery gone; `cli-reference.md` truthful again.

## Step 0 — Reproduce & inventory (the safety net for a deletion iteration)

1. Gates green.
2. **Consumer inventory before deleting anything** (this is the tricky bit of W9 — the machinery is
   NOT purely dead): grep for `FinalScore`, `RenderPlanBuilder`, `TokenBudgetEnforcer`,
   `PatternRelevancePruner`, `TokenBudget`, `RenderedTokens` across `src/` + `tests/` + `eval/`.
   Known consumers to expect: `Cli/Services/ServiceRegistration.cs` (pruner DI), legacy
   `JsonContextRenderer`/`HtmlContextRenderer`, `RenderPlanBuilder.cs:34` (RoleScore), desktop stats
   funnel, several eval `json-*` checks, `OutputSelfCheck` (verify!). List them in the commit message.

## Step 1 — Kernel JSON contract

New `Rendering/KernelJsonRenderer.cs`: serializes `{ schema: "devcontext/v1", archetype, map: MapModel,
entries: EntryPoint[], insights: [] /* filled by I3 */, stats: { seams, entriesWithTarget },
scope, trace?: Trace }` with `System.Text.Json` source-gen context (the graph records are already
serialization-clean; add `[JsonSerializable]` context entries). `--include-graph` adds nodes/edges.
**Do not hand-shape DTOs** — serialize the kernel records; the contract IS the kernel. Golden test:
snapshot the TodoApi JSON, assert stable field names.

## Step 2 — Migrate eval, then delete

1. Rewrite each eval `json-*` expectation path against the new shape (e.g. `$.archetype`,
   `$.entries[*].kind`) — most already match since MapModel field names carry over; fix the rest.
2. Point `--format json` at `KernelJsonRenderer`; run eval — iterate until green.
3. **Delete:** `TokenBudgetEnforcer`, `PatternRelevancePruner`, `RenderPlanBuilder`'s catalog path,
   `TokenBudget` plumbing in `ExtractionOptions`/pipeline, `--max-tokens`/`--token-view` flags, the
   scorer fields the pruners fed (check `InclusionReason` usage — keep provenance if the narrative
   uses it, delete if catalog-only). HTML: replace `HtmlContextRenderer` with a minimal shell that
   embeds the kernel JSON + the narrative markdown (the desktop export seam) — do not keep the old
   catalog HTML alive.
4. `BudgetIndependenceTests` — the invariant is now vacuous (no budget exists); replace with a test
   asserting `ExtractionOptions` has no token members (locks the door).

## Step 3 — CLI v2 sweep (FACES-DESIGN §1.1)

- Remove `--task`, `--around`, `--cleanup`, `--metrics`, `--include-provenance`,
  `--include-anti-patterns`, `scenarios` command → each leaves a one-release stub printing the pointer
  (Spectre: keep the option, mark `[Description("(removed) use --focus")]`, return exit 0 with message).
- Hide `--scenario`/`--profile` from help (`IsHidden = true`).
- `--stats` gains an optional value: `repo` (default; insights+coverage once I3 lands — until then
  seams+coverage) | `engine` (the old telemetry table).

## Step 4 — `devcontext query`

New `QueryCommand` (branch: `config.AddBranch("query", …)` or single command + `<op>` argument —
**suggested: single command with `<op>` argument**, less Spectre ceremony, ops validated against a
static list). Plumbing: run the same composition root → `AnalysisSnapshot` → `GraphQuery` → op switch →
serialize with the Step-1 context. `--attach <host:port>`: a thin gRPC client over
`proto/devcontext/v1/devcontext.proto` calling the running Server instead (map ops to
`ListEntryPoints/GetMap/GetTrace/GetNode/GetNeighbors/SearchNodes/GetStats`). Tricky bit: **id
round-tripping** — `node`/`neighbors`/`usages` accept the human string and resolve via
`GraphQuery.ResolveNodeId`; print the resolved id in output so the next command can use it exactly.
Also: move the Server's ad-hoc `SearchNodes` logic INTO `GraphQuery.Search` and have the Server call it
(one implementation, audit §7).

## Docs & goldens

- **Rewrite `docs/product/cli-reference.md`** to the v2 surface (this is the iteration where it
  changes most — from here on the rule is: any flag/op change edits cli-reference in the same commit).
- `docs/product/desktop-ui.md`: note the JSON contract + Render unchanged for the app.
- Eval: add a `cli-matrix` entry exercising `query entrypoints` + `query trace` on TodoApi.
- CHANGELOG/README (root): the deprecation table.

## Gate

Gates green (eval fully migrated) · `analyze --format json | jq .archetype` works on TodoApi ·
`query trace "GET /todos"` returns the same hops as `analyze -f` · grep proves the deleted types are
gone · cli-reference matches `--help` output (paste both in the commit).
