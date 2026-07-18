# D3.4 — the "compiler lever", profiled and landed (2026-07-18)

## The instrument came first, and it reversed the proposal's mechanism

Fine-grained timing added INSIDE SemanticLitePopulator.Populate (now permanent, in the
tier-routing diagnostic): `fw` (TPA metadata refs) / `nuget` (assets.json + dll refs) /
`trees` / `create` (CSharpCompilation.Create) / `bind` (UpgradeBodyFacts).

DntSite fresh (`--no-cache --include-diagnostics`), BEFORE any fix:

    populate=79774ms (fw=51 nuget=2 trees=2 create=1 bind=79717)

**"Building the merged compilation" costs 55ms.** The T7.2 claim "DntSite 81.1s is building the
merged compilation" conflated the stage with its dominant substep: the SERIAL demand-set bind.
Persisting the compilation on-disk (the proposal's named mechanism) would have saved ~55ms —
the checkpoint therefore delivered the measured lever instead (tracker row has the verdict).

## Two levers, applied one at a time (same run protocol each time)

1. **Parallel bind by tree** (the CallGraphExtractor P2 pattern: per-task GetSemanticModel,
   never a shared SemanticModel; results land at original indices → order-stable):
   bind 79717 → 46692ms. Upgrade counts byte-identical (376/351/5/7872/390625/298).
2. **Arg-bind demand honesty**: 390,625 of those binds were argument types, but every
   ResolveArgTarget call site reads ONLY `Args[0]` of a dispatch/publish/raise verb
   (MediatRDispatch + BusPublish + DomainEventRaise). The demand gate is the runtime UNION of
   the detectors' own verb catalogs (drift-impossible), binding only `Args[0]`:
   bind 46692 → 26400ms; arg binds 390625 → 0 on DntSite (it has no dispatch calls — the
   honest demand really is zero); all other upgrade counts unchanged.

Files: dntsite-tier-timing-{pre,parallel,gated}.txt

## Output unchanged (the guardrail)

- DntSite map: byte-identical pre/post (diff modulo the timing diagnostic; call-graph
  6610 edges / 1880 verified unchanged).
- bitwarden fresh map: **byte-identical to the session-1 baseline** (prism-d3/d31/bw-analyze-map.md),
  diff modulo CRLF — bw-post-map.md. Run 1 showed the KNOWN pre-existing provenance flap
  ([bus] site cited AzureQueueService.cs:30 vs AzureQueuePushEngine.cs:38 — both true send
  sites of the same channel); run 2 on the SAME binary matched the baseline, confirming the
  D2-known-latent detection-order flap, not a regression. bitwarden arg binds: 6 (real dispatch).
- dogfood pole: 439/339/34, Microservices high, ROUTES, http/via gateway (4), and the E1
  keep-side proof — `[bus] Basket.API → Ordering.Application … raises BasketCheckoutEvent`
  (BusPublish → ResolveArgTarget path) still fires — dogfood-post-map.md.
- eShop: 109 entries, 13 integration events, 8 cross-service (pinned semantics).

## Timings after

- DntSite fresh Map: 96.8s → ~43s (SemanticLite 81.8s → 26.5s stage wall).
- bitwarden fresh: 98.3s cold / 68.5s second cold run (was 151.5s in the session-4 octet).
  Proposal cold target ~3.5min: MET with 2.5min headroom. Warm target rides D3.1 (4.2s snapshot HIT).

## Tests

- SemanticLitePopulatorTests: +3 (demand-verbs union pins the detector catalogs; dispatch arg
  binds Semantic while out-of-demand arg stays untouched; parallel pass is order-preserving and
  repeatable). Fast suite 606+3skip +15 green; McpQa serial 2/2; loom-guards PASS.
