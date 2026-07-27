# D5.3 — determinism thread (http ServiceLink pair-order + [bus] provenance-site flaps)

**Verdict: both flaps killed at their chokepoints. Fresh-analysis A/B byte-identical on both repros.**

## Mechanism (three nondeterminism sources, one class)

The flaps were "same detection-order class" as suspected (D3.4 row), but the class had three legs:

1. **`ConcurrentBag` arrival order** — `DiscoveryModel.Detections`/`CallEdges` were filled by
   `Parallel.ForEachAsync` extractor waves; every first-match anchor pick over them (http
   ServiceLink `Provenance = first matching refit route`, seam citation sites) inherited the
   interleaving. Run 1 of the pre-fix A/B reproduced it live: the `Shopping.Web → YarpApiGateway`
   line flapped position run-to-run.
2. **`ConcurrentDictionary.Values` order** — `model.Types` enumeration order is randomized PER
   PROCESS (string-hash seeding). It fed graph node insertion, `FirstOrDefault` type picks, and
   NameResolver/SymbolTable short-name collision lists (the C5 known-latent).
3. **`FrozenDictionary` layout order** — the frozen `CodeGraph.AllEdges`/`Nodes` enumerated
   hash-layout order, which is also per-process random. This was the surviving leg after fix 1:
   the two `[http]` lines sharing one provenance anchor (`IBasketService.cs:7`) live under
   different from-nodes, and their relative render order followed frozen-dictionary layout.

## Fixes (chokepoints, not per-site patches)

- `SealableBag<T>` (new) replaces `ConcurrentBag` for Detections/CallEdges; the pipeline calls
  `model.SealDeterministicOrder()` once after Stage 3 — canonical sort (file, line, type,
  ToString / field-wise for CallEdge). Persistence re-adds in natural (sealed) order — the old
  reversed re-add compensated ConcurrentBag's LIFO.
- `DiscoveryModel.OrderedTypes` — FQN-ordered cached view; ALL graph-assembly reads of
  `Types.Values` switched to it (uniform rule). NameResolver + SymbolTable also sort their ctor
  input (covers synthetic-graph callers).
- `CodeGraph` captures the builder's insertion order for `Nodes`/`AllEdges`/derived in-edges at
  construction; the frozen dictionaries stay for O(1) lookup only.
- Old snapshots carry pre-fix order: no schema bump needed — the J2 engine-version key (Core MVID
  changed by this very build) already rejects them.

## Proof

Fresh analyses (snapshot cache bypassed via per-run `DEVCONTEXT_CACHE_ROOT`), one binary:

- dogfood (http flap repro): run1 vs run2 **BYTE-IDENTICAL**, run1 vs run3 **BYTE-IDENTICAL**
  (`dogfood-run{1,2,3}.md`)
- bitwarden ([bus] flap repro): run1 vs run2 **BYTE-IDENTICAL** (`bitwarden-run{1,2}.md`)
- Pre-fix run of the same A/B reproduced the dogfood flap on the first try (pair swap at the
  shared anchor), so the instrument is sensitive to the failure.

Unit: `DeterministicOrderTests` 6/6 (sealed order insertion/parallel-independent; call-edge total
order; clear-then-readd order; graph insertion-order capture; NameResolver collision pick
enumeration-independent). Fast suite 612+23 green; loom-guards + truth gate PASS. One test
assertion updated (`Same_route_endpoints_are_disambiguated_not_merged`): with canonical order the
FIRST endpoint in source order keeps the bare title — previously the bag's LIFO arrival picked it.
