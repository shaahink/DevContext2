# Prism Phase — Final Report & Handover

> **Read this first** if you're picking the project up cold. Branch chain
> `feat/prism-d1…d5` (stacked; the d5 tip contains the whole train) — closes the "Prism"
> track (`docs/dev/briefs/proposal-prism.md`), D1 through D5, 2026-07-17 → 2026-07-19.
> Successor to `HANDOVER-TAPESTRY.md`. Checkpoint-level detail lives in `PRISM-START.md`
> (tracker tables + handoff log); this doc is the phase-level map.

## 1. What Prism Was

The 2026-07-17 library-round audit (`eval-results/2026-07-17/lens-audit/AUDIT.md` findings
A–H + `EXPERIENCE-ADDENDUM.md` I–N) found 4/8 octet repos FAILING the lens: archetypes not
matching reality, entry surfaces missing, dead caches, silent MCP breaches, a UI that
under-told the engine's story, and a Windows-only truth nobody had written down. Prism
worked the findings as five big deliveries under full-autonomy orchestration — archetype
truth (D1), graph depth + insight honesty (D2), performance + cache reality (D3), the app
telling the engine's story (D4), MCP honesty + cross-platform + phase QA (D5).

## 2. Delivery Map (what landed where)

| Delivery | Theme | Key evidence (all under `eval-results/`) |
|----------|-------|------------------------------------------|
| D1 | Archetype truth + entry surfaces + style rungs — caller-prefix route index, queue seams, CQRS branding, per-service rungs, typed-client fidelity, dup-name disambiguation, MCP `map` markdown fidelity (D1.5: field 1 was DROPPED — a 1400-node library read as ~60 tokens over MCP) | `2026-07-17/prism-d1/` — octet 8/8, cohort 44/44 |
| D2 | Graph depth + insights — Blazor `@code` virtualization into the call graph, GraphQuery type rollups, DI provenance ranking, J1 zero bare swallows, J3 per-extractor failure columns, I1 orphans honesty (+I1-fix: libraries make NO dead-code claims), D3-budget honesty | `2026-07-18/prism-d2/` — octet 8/8 after I1-fix, P7's first real catch |
| D3 | Performance + cache truth — J2 snapshot cache resurrection (dirty-fingerprint keys, awaited saves), warm bitwarden 4.2s, compiler-lever FALSIFIED by measurement (merged compilation costs 55ms, not 81s — the real lever was the serial demand-set bind: parallel per-tree + arg-bind demand gate, DntSite Map 125.5→34.5s), K1/K2 stats surfaces | `2026-07-18/prism-d3/` — bench PERF-2026-07-18-1346, edges/entries identical |
| D4 | App tells the engine's story — screenshot-gate harness, ELK deterministic canvas (design pass on Fable), semantic transports/lanes/disclosure, atlas, library workbench (THE proto item), Studio nav + F4/F5 mechanisms, D3-carved UI-lite (freshness, waterfall, stats timeline) | `2026-07-18/prism-d4/` — gate 16/16 ×5 re-runs, d44/d45/d46 probes now permanent instruments |
| D5 | Honest to agents, shipped everywhere — G1 get_context fill honesty + suggested focuses, H1 cross-OS CI (5 real off-Windows engine bugs), J2 engine-version key, THE determinism thread, laden-server host release, H2/H3 Windows-only release decision + tag-derived installer version + dry-run instrument | `2026-07-18/prism-d5/` — this delivery |

## 3. D5 Close-out — This Session (2026-07-19)

| # | What | Evidence |
|---|------|----------|
| D5.1 | G1 MCP `get_context` honesty: `fillNote` (budget-cut vs content-exhausted) + `suggestedFocuses`; UiEntry packs pull page members; lens-drive navigation probes = the "zero silent breaches" instrument | `prism-d5/d51/` |
| D5.2 | H1 cross-OS CI: engine matrix windows/ubuntu/macos GREEN — first dispatch shook out FIVE real off-Windows engine bugs (sln/csproj `\` normalization, `Split` overload trap, `:P0` culture percent, PathText string-pure algebra, FakeFileSystem '/'-canonical) | run 29663154138; README §Platform support |
| D5.3 | Riders: J2 engine-version snapshot key (Core MVID — retires bump discipline); determinism thread (3-leg class killed at chokepoints: SealableBag + seal, OrderedTypes, CodeGraph insertion-order capture; call-site order is the call-edge canon — the battery caught the callee-name-order regression); laden-server (EngineHostCache released with last session — the unbounded tree-pinning leak behind "unresponsive after ~36 analyses") | `prism-d5/d53-determinism/EVIDENCE.md`, `LADEN-SERVER.md` |
| D5.4 | H2/H3: Windows-only bundle ENCODED (tauri targets nsis+msi, release.yml claim); installer version from release tag; workflow_dispatch dry-run — caught a real latent release bug (pnpm monorepo path) on first flight; inventory = exactly nsis+msi+nupkg | `prism-d5/d54-release/EVIDENCE.md` |
| D5.5 | PHASE QA, with TWO real catches: battery run 1 caught the call-edge canon flipping a POST target (source order is semantic → call-site canon); clean-clone run 1 caught TraceQualityTests running against empty gitlink dirs (the T8.3 class). Final: full battery GATE: PASS unqualified (stamp written) · octet LENS-AUDIT: PASS 8/8 (P6 MCP drives green = DoD zero silent breaches; P7 quiet) · clean-clone battery GATE: PASS every step | `prism-d5/d55-qa/EVIDENCE.md`, `gates-d55-full-run2.txt`, `octet-d55-proof.txt`, `gates-d55-cleanclone-run2.txt` |
| D5.6 | This doc + tracker close + merge prep (single branch-train merge to develop, owner sign-off pending — cross-OS CI green at the branch tip; develop CI rides the merge) | tracker close block |

## 4. Architecture Deltas (post-Tapestry → post-Prism)

What a returning agent must know beyond `HANDOVER-TAPESTRY.md` §4:

- **Snapshot cache is REAL and self-keying** (`Core/Analysis/SnapshotCacheService` +
  `SnapshotPersistence`): dirty-fingerprint version keys + analysis-flavor suffix + the
  D5.3 **engine-version key (Core MVID)** — a rebuilt engine auto-rejects foreign
  snapshots, so the schema-bump discipline is retired. `--no-cache` on query; `--dry-run`
  bypasses cache reads; `DEVCONTEXT_CACHE_ROOT` redirects the root (tests use it).
- **Determinism is a construction property now** (D5.3): extractor output orders are
  sealed post-extraction (`DiscoveryModel.SealDeterministicOrder` — detections by
  file/line, call edges by CALL SITE because source order is semantic for the
  primary-call pick), all graph-assembly type reads go through `OrderedTypes`, and
  `CodeGraph` enumerates in builder insertion order (frozen dictionaries are lookup-only).
  Fresh A/B byte-identical on dogfood + bitwarden. Don't reintroduce
  `ConcurrentBag`/`.Types.Values`/frozen-order enumeration into anchor picks.
- **The server's memory is bounded** (D5.3): `EngineHostCache` releases a root's host
  (ServiceProvider + parsed-tree cache) when its last session closes; the host cache is
  bounded by the session cap (5). Warm re-opens ride the snapshot cache.
- **Cross-OS is CI-verified for the engine** (D5.2): windows/ubuntu/macos legs in
  `ci.yml`. Path handling: parse-time '/' normalization at the sln/csproj chokepoints +
  `PathText` string-pure algebra — never `IsPathRooted`/`GetFullPath` over drive-style
  text, never `Split('\\','/',options)` (the overload trap), never `:P0` (culture).
- **Release is decision-encoded** (D5.4): Windows-only bundles (tauri.conf targets
  nsis+msi), installer version stamped from the tag by
  `src/DevContext.App/scripts/set-tauri-version.mjs`, and `workflow_dispatch` on
  release.yml is a safe full dry-run (publish steps are tag-guarded).
- **MCP tells the truth about fill** (D5.1): `get_context` low fill carries `fillNote` +
  `suggestedFocuses` from GetGraphFacets; libraries skip get_context honestly.

## 5. Known-latent / Next Threads

- eShop deep-trace render stops at the IntegrationEventLogEF send seam (pre-existing,
  observed since D3; recorded at phase QA — a render-depth thread, not a graph defect).
- Snapshot-cache HIT/location has no app-facing proto field (D4.6 carve) — candidate for
  a future proto rider.
- Desktop bundles for mac/linux: unscheduled (no hardware to verify); the engine itself
  is CI-verified cross-OS.
- Truth-ratchet pending skips (3 pre-existing) ride until their fixtures materialize.
- `analyze --format html --strict` exit 2 = pre-existing allowed state (self-check
  failures), noted by every gate since D2.

## 6. How to Verify This Phase (the short battery)

```powershell
powershell -File eval/gates.ps1              # FULL battery — the citable gate
powershell -File eval/lens-audit.ps1 octet   # the 8-repo lens proof (P1–P7)
```
