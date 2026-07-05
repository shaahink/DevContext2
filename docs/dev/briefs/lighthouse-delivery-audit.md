# Lighthouse Delivery Audit — Independent Verification (2026-07-05)

> Audit of the "L0–L7 DONE" claim on `feat/lighthouse-l2`, run against real evidence:
> the L7 artifacts themselves, a fresh engine run on `run-aspnetcore-microservices`
> (the repo in the user's screenshots, `eshop-microservices.sln`), and a first-ever
> live MCP agent session. Verdict up front: **the kernel hardening (L0–L3) is largely
> real; L5 (MCP) shipped broken and unrun; L7's benchmark audit avoided the app-shaped
> repos it was supposed to verify; and the engine's wiring depth is not sufficient for
> the product's core promise on a mainstream .NET microservices repo.**

## 1. L7 — the audit that audited around the evidence

`eval-results/2026-07-05/AUDIT.md` claims "18/21 FIXED" from a bench of 10 repos. Reality:

- **eShop-report.md and TodoApi-report.md are 3-line stubs** — `_No analysis data
  available._` The two flagship *app* repos produced nothing (stale-cache issue), and
  after the snapshot-versioning fix landed **in the same session**, they were never
  re-run. The scorecard's app-shape verdicts rest on CleanArchitecture + DevContext
  self-analysis only.
- **11 of 22 bench repos deferred** (PowerToys, MassTransit, DntSite, Ocelot, gRPC,
  AzureFunctions, RazorPages, CLI, Blazor, Avalonia, MassTransit-Sample). Of the 8 that
  ran, 6 are libraries — the archetype where the product does the least (0 entries,
  0–12% verified edges). The audit verified where verification was cheapest.
- The recommended "MCP agent transcript" (handover §8.3) was never attempted. Had it
  been, it would have failed immediately — see §3.

## 2. Engine — wiring depth fails on a mainstream microservices repo

Fresh run against `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`
(11 projects: Carter + MediatR + Marten + MassTransit/RabbitMQ + gRPC + YARP + Refit +
Razor Pages — as mainstream as .NET microservices get). 474 nodes / 213 edges / 36 entries.

### 2.1 The crown-jewel flow traces to nothing

`POST /basket/checkout` is the repo's signature flow: endpoint → MediatR command →
handler → MassTransit publish → RabbitMQ → Ordering consumer → CreateOrder → domain
events. DevContext's trace:

```
▸ ENTRY  POST /basket/checkout
   └─ call <lambda> POST /basket/checkout
```

Two nodes. `touchedEntities: []`, `emittedEvents: []`. Root causes, all confirmed in code:

| # | Root cause | Where |
|---|-----------|-------|
| W1 | `MediatRExtractor` matches only literal `IRequestHandler<`/`INotificationHandler<`. This repo (like most) defines `ICommandHandler<,> : IRequestHandler<,>` in BuildingBlocks — **every command/query handler is invisible**. Whole graph has 2 `Handles` edges for ~15 handlers. | `MediatRExtractor.cs:41,107` |
| W2 | `Sends` detection misses `request.Adapt<CheckoutBasketCommand>()` + `Send(command)` (command constructed by Mapster, not `new X()`). Checkout has no Sends edge at all. | `GraphBuilder.AddSends` (body-scan) |
| W3 | Trace stops at the command even when a Sends edge exists (DELETE /basket) because the Handles edge is missing (W1) — nothing to traverse into. | consequence of W1 |
| W4 | **Zero cross-service edges.** No MassTransit publish→consume join (Basket→Ordering via BasketCheckoutEvent), no gRPC client→server join (Basket's `DiscountProtoServiceClient` → Discount.Grpc service), no Refit→YARP→downstream-route join (Shopping.Web → gateway → Catalog/Basket/Ordering). The System graph shows only project references; Shopping.Web, YarpApiGateway, Discount.Grpc float disconnected — for a microservices repo, the *entire architecture story* is missing. | no extractor exists |
| W5 | Cross-project name-collision: trace for Shopping.Web's `Product` walks into **Ordering.Infrastructure's `ApplicationDbContext`** (short-name resolution isn't project-scoped). The false edge is also the *only* result `impact(Product)` returns — inverted truth. | `NameResolver` |
| W6 | `CheckoutBasketCommandHandler`: `inDegree 0, outDegree 0`. `impact()` = "nothing affected". An agent (or dev) concludes it's safe to delete the heart of checkout. | consequence of W1/W2 |

### 2.2 Presentation-layer truth bugs

- **Fake routes**: Razor PageModels render as `GET /ProductDetailModel`, `GET /ErrorModel`
  etc. — fallback `"/" + className` at `RazorPagesExtractor.cs:100`. Real route is
  `/ProductDetail`. These fake routes then pollute Top Flows (#1 ranked!), insight
  evidence, and the auth-surface warning.
- **DI extensions as Bus entry points**: `AddMassTransit → AddMassTransit` and
  `UsingRabbitMq → UsingRabbitMq` listed as Bus entries. 2 of 3 bus entries are noise.
- **Archetype "GATEWAY" for the whole solution** (one YARP project ⇒ everything is a
  gateway). Should be *microservices*. Style "CleanArchitecture (confidence high)"
  claimed solution-wide when only Ordering follows it — per-service style is the truth.
- Report footer: "146 files · **0 projects**".

### 2.3 Insight quality

- "Possible dead code: `GetBasketQueryHandler`" — a live MediatR handler (W1 cascade).
  Trust-destroying false positive of exactly the class L0 was supposed to end.
- "Downstream wiring: 10 target services detected" — evidence includes
  `StoreBasketRequest.Adapt` (a Mapster mapping call), `<lambda> DELETE /orders/{id}`,
  and query types. None are "downstream services".
- "Module map: 8 feature areas" = folder names (`Pages`, `Endpoints`, `Services`), with
  "1 entries" grammar. Not feature areas.
- "Entry targets resolved 35/36 (97%) — use --focus for deeper traces" — CLI copy
  leaked into UI insights (claimed fixed in L6).
- Every insight chip/action in the UI navigates to `/explore?focus=<evidence text>` —
  strings like `"27 no auth annotation"` that can never resolve as a focus.
  **The Insights→Explore connection is structurally broken**, not cosmetically.

## 3. MCP (L5 "DONE") — shipped broken, never run

First-ever live session (JSON-RPC over stdio, driver script + transcript in this
session's artifacts):

1. **The server could not start.** DI crash on launch: `ILogger<McpSessionManager>`
   unresolvable — `AddLogging()` was never registered. Fixed this session
   (`Program.cs`). Nobody had ever launched the shipped binary.
2. **Transport starvation deadlock**: the response to `analyze` does not flush until
   the *next* inbound request arrives. A real agent awaiting the analyze response hangs
   forever. (Fire-and-forget `RunAnalysisAsync` starves the stdio write loop.)
3. **All 18 tools have empty descriptions** — 740 tokens of schema, zero guidance. An
   agent cannot tell `trace` from `get_context` from `impact` without trial and error.
4. **Answer quality on the questions agents actually ask** (measured, ~9.7k tokens for
   a full session):
   - "How does checkout work?" → trace: 2 nodes (§2.1). `get_context(checkout, 8k,
     explain)` → **167 tokens of empty box** (blank identity line, signatures without
     line numbers, empty bodies section).
   - "Who uses the Discount service?" → `usages(DiscountService)` returns its own 4
     gRPC method wrappers. The actual caller (Basket.API) is absent (W4).
   - "What breaks if I change X?" → `impact(CheckoutBasketCommandHandler)` = 0 results
     (dangerously wrong); `impact(Product)` = 1 result and it's the false W5 edge.
   - `node(Product)` silently picks one of 3 `Product` types with no disambiguation.
   - `search(Discount)` returns a DI lambda body as a node title.
5. Token waste everywhere: absolute `C:\Users\...` paths repeated per node, unicode-
   escaped JSON, a meaningless `"confidence":0.21` on every response, no pagination
   (`entrypoints` = 2,225 tokens all-or-nothing).

**Bottom line: today the MCP loses to plain grep on its flagship use case** — an agent
with Read+Grep answers the checkout question correctly in ~3 file reads; DevContext
answers it wrongly (empty impact, dead-end trace) at similar token cost. The graph, not
the protocol plumbing, is the binding constraint — which is why the next phase treats
engine wiring and MCP as one program.

## 4. UI/UX — audit of the four surfaces (against user screenshots + code)

- **Home**: identity "sentence" is a stat concatenation ("gateway · 36 endpoints · 11
  services · 474 types"). "11 services" is project count (BuildingBlocks is not a
  service). "confidence 21%" and "wired 35/36" are presented raw with no meaning a dev
  can act on. No visual anywhere — no service map, no type/endpoint breakdown, no
  onboarding path. Top Flows is polluted by fake Razor routes and ranks DELETEs first.
- **Insights**: see §2.3 — content is weak and the jump wiring is structurally broken.
- **Atlas**: `atlas-page.ts` renders the **raw CLI text dump** (`mapMarkdown` in a
  `whitespace-pre-wrap` div) as the primary content — a terminal report pasted into a
  page, with the topology graph and panels below the fold. Event Wiring Board's empty
  state still says "the background flow indexer arrives in W5" (stale copy from the
  Fable plan). "Repo in one page" is the right idea; this is not that page.
- **Explore**: System view shows 11 boxes with overlapping labels and no runtime edges
  (the interesting wiring doesn't exist in the graph — W4). Trail accumulates every
  visited entry into a long undifferentiated list. Icons at 12px and text at 2xs
  (~10px) — the "squint" problem is a design-token issue, systemic, not per-component.
  Graph↔code: selecting a graph node does not confront you with its code; the
  inspector/details pane stays disconnected from what the canvas shows.

## 5. What *was* delivered honestly

Credit where due — verified this session:

- Snapshot schema versioning (`SnapshotSchema.Version`) is real and works (fresh
  analyze on first MCP run, cache hit on second).
- `intent` modes in `ContextPackBuilder` now genuinely branch (explain/review/trace
  ordering + depth). One cosmetic leftover: `idBudget` ternary with identical branches
  (`ContextPackBuilder.cs:42`).
- E1–E8 fixes hold up in the eshop-ms run: no `?` names, no DbContext targets, no WPF
  ICommand entries, per-method gRPC entries (4 Discount methods), MediatR pipeline
  behaviors surfaced (LoggingBehavior → ValidationBehavior), package/stack chips accurate.
- Analysis speed is good: 4.1s cold for 11 projects / 146 files; snapshot reopen ~3s
  via MCP.
- CleanArchitecture/library bench reports (Serilog, Polly, FluentValidation…) are real
  and their claims check out.

## 6. Fixes applied during this audit

- `src/DevContext.Mcp/Program.cs`: `services.AddLogging(b => b.AddSerilog(...))` —
  MCP server can now start at all.

## 7. Disposition

Every finding above feeds `proposal-meridian.md` (the next mega plan): engine wiring
depth (W1–W6), truth/presentation bugs, MCP transport + feature set derived from the
dogfooding transcript, and the four-surface UI redesign. The L7 "audit" methodology
gap (verify on the shapes that matter, gate on app repos, never score a fix without
re-running its failing artifact) is encoded there as the M0 verification harness.
