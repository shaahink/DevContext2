# Independent Session Audit — Meridian Close-Out Verification (2026-07-07)

Independent audit of `docs/dev/HANDOVER-MERIDIAN.md` + hands-on drives of the engine,
the MCP server, and the desktop UI. Everything below was produced by **fresh runs this
session** — no claim is repeated from prior docs without re-verification.

## Verdict

The handover is **procedurally honest but truth-shallow**. Every gate it names is green
and every artifact it cites exists — but the gates assert *presence*, not *truth*. When
driven the way a real user or a cold agent would drive it, the flagship wiring claim does
not reproduce, the MCP surface fails 15/15 calls for an uninitiated agent, and four of the
user-visible surfaces have first-click defects. Meridian built real machinery; the next
phase must make the machinery *true* and *usable*, not add more of it.

## 1. Gate verification (all reproduced fresh)

| Gate | Claimed | Fresh result | Verdict |
|---|---|---|---|
| `dotnet build DevContext.slnx` | 0w 0e | 0w 0e (30.4s) | ✅ |
| `dotnet test` (non-Eval) | green | Core 355✅/3-skip · Server 12✅ · Desktop 64✅ | ✅ |
| `pnpm check` | green | lint+test+build all pass, bundle built | ✅ |
| Dogfood numbers | 493n · 316e · 34 entries · 6 SL | 493 · 316 · 34 · 6 (3.9s) | ✅ exact |
| Commit hashes G1–G7 | listed | `16d3166`, `3be265f`, `bf3a674`, `ba6c59a` all exist | ✅ |
| M9-ext-b (G8–G11) | "this session" | **UNCOMMITTED** — 10 files, +494/−140 in working tree | ⚠ real code, not committed |
| Evidence artifacts | eval-results/2026-07-07/ | 22 reports + AUDIT.md + mcp-qa.md present | ✅ |

G8–G11 code inspected line-by-line: Inspector Insights + Call Stack sections, rAF minimap
throttle, MCP error toasts are all genuinely implemented as described.

## 2. Product-claim re-verification (the important part)

### Claim 1 — Wiring truth: **FAILS fresh reproduction for the named flagship flow**

> "POST /basket/checkout traces endpoint → command → handler → publish → RabbitMQ →
> Ordering consumer → CreateOrder, across three services"

Fresh CLI run (`--focus "POST /basket/checkout"`):

```
▸ ENTRY  POST /basket/checkout
   └─ call <lambda> POST /basket/checkout
RESULT   200 OK / 201 Created
```

**Depth 1.** No send, no handler, no bus hop, no Ordering. TOUCHES and EMITS empty.
Same through MCP `trace` (children: `[]`). The M1.3 evidence was a *different* flow
(DELETE /orders, which does trace deep). Root-cause trail: the lambda's Member node is
keyed `Basket.API.Basket.CheckoutBasket.CheckoutBasketRequest.<lambda> POST /basket/checkout`
— the lambda was attributed to the **request DTO type**, and the Sends edge (from the
`request.Adapt<CheckoutBasketCommand>()` + `sender.Send(command)` pattern) is not anchored
where the trace walks. The M4.G QA "passes" because it hard-codes the focus string and
accepts `found=true` with 2 steps; its own artifact records `cross-service=false`.

### Claim 2 — Agent surface: **misleading as stated**

The 8/8 QA pass is real but scripted: the harness knows the exact tool signatures, the
session-handle threading, and hard-codes `focus: "POST /basket/checkout"` — the natural-
language "questions" are cosmetic. A cold agent driving the MCP (this session, drive #1)
failed **15/15 calls** — see §4. "Beats grep" holds only for an agent that already knows
the answers' shape.

### Claim 3 — One-page repo: **fails the 10-second test on its own dogfood**

The Home/Atlas service hero renders `Basket.API`, `Catalog.API`, `Ordering.API` as three
identical cards labeled "API" (last dot-segment truncation), and renders class libraries
(`BuildingBlocks`, `Domain`, `Application`, `Infrastructure`, `Messaging`) as peer
"services". The Atlas *prose* names services correctly — the diagram labeling is UI-side.
A `24% confidence` stat headlines the identity strip with no explanation. A dev reading
this for 10 seconds learns something *wrong* about the repo.

## 3. Engine findings (fresh bench + code read)

| # | Finding | Evidence | Root cause |
|---|---|---|---|
| E1 | Flagship checkout trace depth-1 | §2 Claim 1 | Sends edge mis-anchored; lambda attributed to DTO type |
| E2 | **Fabricated cross-project wiring** on multi-sample repos | RazorPages report: `POST /Students` "calls" controllers in *different sample projects*; 3 identical top-flows | `NameResolver.Resolve` returns `fqns[0]` on global short-name collision — first match wins, silently |
| E3 | Near-empty graphs outside the CQRS sweet spot | Blazor 360n/**1e**, Serilog 124n/**1e**, CommunityToolkit 251n/4e, RazorPages 4,483n/38e | Seams are MediatR/bus/EF-shaped; plain `Calls` spine requires both endpoints declared+FilePath, and controller→service→data flows without DI tags mostly drop |
| E4 | Blazor WASM sample labeled `Style: Microservices` | Blazor-report.md | Style detector counts signals (aspire, refit…) from 142 doc-sample projects |
| E5 | TOUCHES over-claims | DELETE /orders trace: `TOUCHES Order, Customer, Product, OrderItem` | Trace collects entities via EntityRelation reachability, not actual data access of the traced path |
| E6 | `impact` groups by `"(unknown)"` service | MCP drive #2 | `GraphNode.Project` rarely stamped; service attribution is a per-call afterthought |
| E7 | Bench gate asserts presence, not truth | 22/22 "pass" includes E2/E3/E4 outputs | content assertion = "has sections", no per-repo semantic expectations |
| E8 | **Static mutable state in GraphBuilder** | `private static Dictionary … _eventPublishers` (GraphBuilder.cs:25) | Two concurrent analyses (multi-tab is a supported feature!) corrupt each other's bus ServiceLinks |

## 4. MCP drive findings (hands-on, stdio, both cold and informed)

Drive #1 (cold agent, natural arg guesses): **15/15 calls failed**, every one returning
the opaque string `An error occurred invoking '<tool>'`. No schema hint, no missing-param
message. Calling a **nonexistent tool** (`flow`) returns *empty success*, not an error.

Drive #2 (correct signatures) — per-tool quality:

| Tool | Result | Finding |
|---|---|---|
| `analyze` → handle | ✅ 31 tok | fine; but *every* other tool then requires `handle` threading — no default-session fallback |
| `overview` | ✅ 242 tok, good | best tool in the set |
| `trace` focus="checkout" | `found:false` | focus resolution is exact-ish; NL phrases fail |
| `trace` exact route | depth-1 tree (E1) | flagship broken |
| `resolve "Order"` | 10 candidates, **`Order` aggregate not among them** | substring over FQN — everything under `Ordering.*` matches; no ranking by kind/degree/exactness |
| `usages IBasketRepository` | `count:0` silently | requires exact nodeId; other tools accept short names — inconsistent resolution, silent empty |
| `impact TotallyMadeUpType` | `totalAffected:0` | unknown symbol indistinguishable from "no impact" — agent gets confidently wrong answer |
| `config key=ConnectionStrings` | `totalKeys:0` | repo has them; exact-key filtering broken/no hint |
| `get_context focus="basket checkout"` | `found:false` | same brittle focus resolution |
| `find "discount"` | 668 tok incl. migrations/snapshots | noise not filtered |
| `tests_for` | 0 (best-effort) | honest |
| docs drift | handover lists tool `flow` among "18 tools" | actual: 22 tools, **no `flow`** |

## 5. UI drive findings (Playwright, dogfood analyzed, 20 screenshots in `ui-audit/`)

| # | Finding | Evidence |
|---|---|---|
| U1 | **Tabs are 17px tall, 10.5px font** | measured; VS Code tabs ≈ 35px. User-reported, confirmed |
| U2 | **Titlebar "New" destroys the active tab** (session lost), creates nothing | tabs `[eshop, TodoApp]` → click New → `[eshop]`; `titlebar.ts:182` calls `session.cancel()` + `closeTab(active)` instead of `createTab()` |
| U3 | **Code pane empty** on the default selection | entry selected → "Select a node to view its source location"; no `pre/code` content found (null) even after opening Code section |
| U4 | Inspector Insights chip shows **all 10 repo insights** as if node-scoped | filter is substring-on-evidence (`inspector.ts filteredInsights`) — empty match = show all |
| U5 | Context Studio scope tree: 30/34 entries under a meaningless **"Default"** group | only Discount.Grpc grouped; service grouping doesn't use real service identity |
| U6 | Context Studio preset click produces **no cards**, duplicated flat list | "I'm changing this endpoint" → second list renders, 0 cards, Copy disabled |
| U7 | Table lens unreachable when focus is in a panel; **no visible button** | Shift+E swallowed; keyboard-only discoverability |
| U8 | Home/Atlas service cards: 3× "API" + libraries as services (Claim 3) | screenshots 02/13 |
| U9 | MCP page says "Stopped / Connections refused" while sessions are live in the list | status conflates endpoint toggle with reality |
| U10 | Clone cancel: plumbing exists (gRPC ct flows to clone), but no confirm/cancel affordance on closing a cloning tab | code read |
| U11 | Icon sizes measured 14×14 / 18×18 on Home — consistent; not the core problem (density/labels are) | measured survey |

## 6. GraphBuilder / graph-model architecture assessment

The graph *model* (`CodeGraph.cs`) is sound: immutable, frozen adjacency, in/out edges,
provenance + resolution + confidence on edges, roles-as-tags. **Keep its spirit.**

The *builder* is where the product's ceiling lives:

- **2,202 lines, 18 regex sites**, ~40 private helpers, one class. Every new framework
  touches it.
- **Stringly identity everywhere**: joins via `names.Resolve(shortName, file)` →
  `NodeId.ForType(string)`. On ambiguity: first match (E2). Unresolved names pass through
  as opaque keys, indistinguishable from resolved ones.
- **Regex over raw bodies** for Sends/Raises/data-touch/variable resolution/field types —
  including hand-rolled string-literal stripping (70 lines) and char-offset→line
  provenance estimation. All of this re-implements what a syntax walk gives for free —
  and the codebase *already parses every file with Roslyn* (`BuildMethodSpans` parses
  each type body again, per type!).
- **Projects masquerade as Types**: ServiceLinks do `NodeId.ForType(projectName)` — a
  project becomes a fake Type node with `Layer="Infrastructure"`. There is no Service
  node kind. This is why impact says "(unknown)" and Context Studio groups by "Default".
- **Static mutable `_eventPublishers`** (E8) — cross-build corruption hazard.
- **Cartesian WrappedBy**: every pipeline behavior × every request node = edges that say
  nothing per-flow.
- Suffix/name heuristics (`EndsWith("Command")`, verb-prefix route matching,
  `LayerSegments` sets) are *fine as heuristics* but are unlabeled — they emit at the
  same confidence tier as real joins.
- Duplicated helpers (`StripGenerics` ≡ `RemoveGenerics`), two `g.Build()` calls to
  detect violations then rebuild, dead `ExtractInnerEntityName`.

## 7. What this means

Meridian's direction (one graph, lenses, agent surface) is right and much of the
machinery exists. The failures share ONE root: **identity and wiring are established by
string luck, then consumed as if they were facts.** The fix is not more heuristics — it
is a graph system with typed identity, explicit resolution tiers, structured body facts
instead of regex, services as first-class nodes, and gates that assert named truths.

That is the Loom phase: `docs/dev/briefs/loom-graph-design.md` (the design) and
`docs/dev/briefs/proposal-loom.md` (the plan).

## Appendix — artifacts produced this session

- `eval-results/2026-07-07/dogfood-audit-v2.md` — fresh dogfood CLI report
- `eval-results/2026-07-07/ui-audit/*.png` + `notes.md` — 20 screenshots + measurements
- MCP drive transcripts (temp): cold-agent 15/15 fail; informed drive per-tool results (§4)
- `src/DevContext.App/scripts/ui-audit-drive.mjs` — reusable UI audit drive (committed)
