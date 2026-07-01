# Iteration I5 — Facet menu (pick-any features over existing data)

> **Status: BLOCKED on I1 (trust) + I2 (contract)** · Phase: V2/V3 · Menu: each item ≈ ⅓–1 session,
> any order, pick by appetite. Audit context: `ENGINE-VALUE-AUDIT.md` §4, `DEV-PAINS.md`.

## Prereq slice (do once, first pick pays for it)

**FacetCatalog** (voted in PROGRAM-PLAN): `Graph/Facets/FacetDescriptor.cs` —
`(Id, GateSignal/Archetype predicate, SectionKey, Cap, Compute: snapshot → FacetResult)`.
`MapRenderer`/`LibrarySurfaceRenderer` enumerate active facets into their section list;
`GraphQuery.Facet(id)`; kernel JSON `facets: {}`. One facet = one file + one eval check.
Anti-soup rule enforced by a shared test: a facet must NOT render when its gate is absent.

## The menu (★ = suggested first picks, per DEV-PAINS)

| Pick | Facet | Gate | Notes / tricky bit |
|---|---|---|---|
| ★ | **F13 Blast radius** (`query usages --transitive`, P3) | any | BFS over in-edges depth≤4, collect entry roots; render "reached by: 3 HTTP, 1 job". Tricky: cycles — visited set; cap frontier 500. Not a Map facet — a query op + palette verb. Cheapest wow in the program. |
| ★ | **F3 Message matrix** (P7) | Bus entries exist | per message type: producers (Sends/Raises origins) → consumers (Handles/Consumes). `?` for consumed-never-produced. Render as its own section for messaging repos ONLY. |
| | **F1 Auth surface** (P5) | HTTP entries | entry-line `[anon]`/`[authorize:policy]` + Overview digest; insight already ships in I3 — this is the render half. Verify controller-level attr inheritance in `EndpointExtractor` first. |
| | **F2 Middleware pipeline** | web app | one Cross-cutting line from `MiddlewareDetection` ordered by `PipelineOrder`; dedupe UseX seen in multiple Program.cs (multi-project: group by project). |
| | **F4 Data map** (needs the DntSite EfCore gap fix first — do that as its Step 0) | efcore signal | entity ← entry-kind counts via ReadsWrites aggregation. |
| | **F5 Talks-to** (P8) | any | `AddHttpClient<T>` / gRPC clients / Refit interfaces → `TALKS TO` topology group. Tricky: base addresses live in config — join `GetSection("X")` literals when present, else render the client name alone (honest). |
| | **F6 Config surface** (P6) | any | `Configure<T>` + section literals → "CONFIG" line-group. |
| | **F7 Interesting points** | any | `GraphQuery.InterestingPoints(archetype)` per audit §3 composition; feeds palette + landing "start here". |
| | **F8 DI health render** | any | lifetime histogram in stats; captive-dependency stretch (singleton impl ctor param resolves to scoped-registered service — join two detections, mark "possible"). |
| | **F10 CLI command tree** | cli-commands signal | walk `[CommandOption]`/`[CommandArgument]` on settings types (syntax already parsed); render tree under CLI entries. |
| | **F11 Trigger detail** | functions signal | carry binding attr + literal onto the entry title. |
| | **F9 Aspire topology** | aspire signal | AppHost references → SERVICES group. |
| | **F12 Desktop VM wiring** | desktop-ui signal | biggest delta: DataContext/x:DataType join → UiEntry target = VM; VM `[RelayCommand]`s as sub-entries. Do last. |

## Per-pick checklist (the contract)

1. Descriptor + compute function (+ extractor delta only if the table says so).
2. Eval: positive render check on its exemplar repo + the shared negative (absent gate ⇒ absent facet).
3. `GraphQuery.Facet`/op + kernel JSON — desktop/MCP get it free.
4. `cli-reference.md` / `desktop-ui.md` / `INSIGHTS-REFERENCE.md` same-commit updates.
5. Bench spot-check if an extractor was added (no Map-time regression on OrchardCore).

## Gate (per pick, not per iteration)

Gates green + the pick's checklist. The menu never blocks the tracker — record picks in README table.
