# Dev Pains → Features

> Authored 2026-07-02. The demand-side audit: which real developer pains can DevContext solve with the
> graph it already builds, which need small additions, and which are named-but-deferred. Tiers:
> **CORE** (must, ordered — bugs/trust/foundations) · **MENU** (independent features, pick by appetite) ·
> **LATER** (needs infrastructure we've deferred).

| # | Dev pain (the moment it hurts) | What we have | What's missing | Feature (→ iteration) | Tier |
|---|---|---|---|---|---|
| P1 | "New repo, day 1 — what IS this and where do I start?" | Map + entries + archetype: the core product, works | interesting-points for non-obvious shapes; first-screen insights | Insights on Overview (I3), InterestingPoints (I5/F7) | **CORE** |
| P2 | "Where is this route/message handled?" | graph + `SearchNodes` RPC | no UI/CLI reach | Command palette (I4) + `query search` (I2) | **CORE** |
| P3 | "If I change this, what breaks?" — the fear moment | inverse edges, `find_usages` | transitive walk + entry roots | **Blast radius**: `query usages --transitive` = BFS over in-edges (depth-capped), report which ENTRIES reach it — "this touches 3 endpoints + 1 job". Cheap, high-wow (I5/F13) | MENU (early pick) |
| P4 | "I can't trust my mental model of the wiring" (MediatR/events invisible in IDE) | the flagship trace | §5.1 bug fabricates cross-method edges | Trust fixes (I1) — *prerequisite for everything above* | **CORE #1** |
| P5 | "Is this endpoint even secured?" (review, pentest prep) | `AuthAttributes` captured, unrendered | render + insight | Auth surface F1 + `auth.anonymous` insight (I3) | **CORE** |
| P6 | "What config/env does this need to run locally?" (onboarding blocker #1 in LOB apps) | `Configure<T>` DI detections | section-name literal join | Config surface F6 (I5) | MENU |
| P7 | "Who publishes the event I consume / does anyone consume what I publish?" | Sends/Raises/Consumes edges | the join rendered | Message matrix F3 (I5) — *the* messaging-repo feature | MENU (early pick) |
| P8 | "What does this service call — what dies when the network does?" | typed-client DI hints, refit signal | small extractor delta | Talks-to F5 (I5) | MENU |
| P9 | "What changed structurally in this branch/PR?" | nothing persisted | snapshot persistence + diff | Snapshot diff — diff entries/insights/edges between two analyses. Killer review feature, **needs V5 index** | LATER |
| P10 | "Is this class dead? Can I delete it?" | InEdges + DI + IndirectWiring detections | careful composition (reflection honesty) | `graph.orphans` insight (I3), *likely-dead* wording | MENU |
| P11 | "Feed my AI the right context, not the whole repo" | LLM export, narrative | packs + MCP | Export packs (I4), MCP server (I6) | **CORE** (MCP is the distribution bet) |
| P12 | "Onboard a teammate without a 2-hour call" | Map/export | curated pack | Onboarding export pack (I4) | MENU |
| P13 | "Which tests exercise this handler?" | tests excluded from graph by design | a tests-lens (opt-in edges test→prod) | Tests lens — new noise-filter mode + `coverage.tests` insight | LATER |
| P14 | "The DI container is a mystery — lifetimes, duplicates, captives" | rich `DiRegistrationDetection` | analysis + render | DI health F8 + `di.lifetimes`/`wiring.multi-impl` insights (I3); captive-dependency check as stretch | MENU |
| P15 | "This monster repo times out / lies about scope" | scope stamp, W1 filters | solution picker UX, per-area maps | Huge-repo scoping (V5.3) | LATER |

**Reading the tiers:** CORE = I1 → I2 → I3 → I4 (+I6) in order — they are dependency-ordered
(trust → wire contract → insights → UI that surfaces them). MENU items are deliberately independent:
each is one facet/insight/query op with its own eval check, pickable in any order inside I5. My
suggested first two MENU picks: **P3 blast radius** (cheapest wow-per-line in the whole program) and
**P7 message matrix** (differentiating; no other tool answers it for .NET).
