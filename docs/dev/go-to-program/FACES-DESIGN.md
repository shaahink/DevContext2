# Faces Design — CLI v2 · Desktop UX · Insights

> Authored 2026-07-02 · Base: `develop` @ `7228d1e` (+ Angular observations from `feat/narrative-canvas` @ `9d504c4`).
> The design contract for the user-facing work in iterations I2–I4 (see `README.md` tracker).
> Principle throughout: **CLI, desktop, and MCP are the same verbs over the same `GraphQuery` kernel.**
> A feature that can't be expressed as a query op doesn't ship in any face.

---

## 1. CLI signature audit (current: `analyze | init | scenarios | version`)

`AnalyzeSettings.cs` carries 26 options. Classified:

| Flag | Verdict | Why |
|---|---|---|
| `[PATH]`, `-f/--focus`, `--depth`, `--detail` | **KEEP** — the core dial set | matches the product model (map ↔ trace derived from focus) |
| `--format`, `-o/--output`, `--strict`, `--verbose`, `--trace`(logging), `--dry-run` | **KEEP** | standard tool hygiene |
| `--no-roslyn`, `--lite` | **KEEP** | honest speed dials; document in perf terms |
| `--repo`, `--ref`, `--keep` | **KEEP** | GitHub path already works (clone → analyze) |
| `--cleanup` | **REMOVE** (fold into `--keep`) | two flags, one bit; `--cleanup` only honors auto/keep anyway |
| `-t/--task` | **REMOVE** (already deprecated) | pseudo-NLU removed by decision 2026-06-12 |
| `-a/--around` | **REMOVE** | pure alias of `--focus`; aliases breed drift |
| `-s/--scenario`, `-p/--profile` | **HIDE** (keep parsing, drop from help) | derived from focus since PLAN-2; expert override only |
| `--max-tokens`, `--token-view` | **REMOVE with W9** (I2) | token machinery leaves the kernel; narrative never read them |
| `--include-provenance`, `--include-anti-patterns` | **REMOVE with W9 / fold into insights** | catalog-only toggles; anti-patterns become an insight source (I3) |
| `--metrics` | **REMOVE** (alias of `--stats`) | one name per concept |
| `--stats` | **RESHAPE** (I3) | today prints engine telemetry; becomes insights + coverage, telemetry behind `--stats engine` |
| `--include-map` | **KEEP** | legitimate compose dial |
| `scenarios` command | **REMOVE** | scenario is derived; the command documents a concept we retired |

**Deprecation policy (public dotnet tool):** removed flags keep parsing for one minor release and print
a one-line pointer (`--around → use --focus`), then go hard-error. `scenarios` prints the pointer and
exits 0. Do the whole sweep in ONE release (I2) so users learn one new shape, not four.

### 1.1 The v2 surface

```
devcontext analyze <path|.sln|url> [-f focus]... [--depth N] [--detail level]
                   [--format md|json] [-o file] [--stats [repo|engine]]
                   [--lite] [--no-roslyn] [--ref branch] [--keep] [--strict] [--dry-run]

devcontext query <op> <args> [--format json|md] [<path>]
        ops: entrypoints [kind] · map · trace <focus> [--depth] · stats · insights
             node <id|name> · neighbors <id> [--in|--out] [--kind seam] · usages <id>
             search <text> [--kind] · facet <name>          # facets land in I5

devcontext serve [--grpc [port]] [--mcp]        # hosts DevContext.Server; --mcp = stdio MCP (I6)
devcontext init · version
```

**`query` session model — options:** (a) each `query` re-analyzes (correct, slow); (b) `query --attach
localhost:PORT` hits a running `serve` (the desktop's server — session reuse for free); (c) wait for the
persistent index (V5). **VOTE: (a)+(b) in I2** — (a) is trivial (same composition root), (b) is ~a thin
gRPC client over the proto that already exists (`proto/devcontext/v1/devcontext.proto`), and together
they make the CLI scriptable now; (c) later upgrades both transparently. `analyze` stays the porcelain
(narrative markdown); `query` is the plumbing (JSON-first; `md` renders the single fragment).

**JSON contract:** after W9 (I2), `--format json` and every `query` op serialize the SAME records the
kernel owns (`MapModel`, `EntryPoint[]`, `Trace`, `NodeDetail`, `EdgeRef[]`, `SeamStat[]`, `Insight[]`).
One wire shape for CLI/desktop/MCP/web — the `devcontext_pb.ts` messages should be regenerated from
these, not parallel-maintained.

---

## 2. Desktop: from WPF to the Angular canvas — audit and direction

### 2.1 What exists

- **Old WPF (`DevContext.Desktop`, BlazorWebView):** proven analyze-once/re-render-many; focus-driven
  single control; per-section HTML toggling; honest dry-run; stats page. UX ceiling: native shell,
  no linkable navigation, one-shot renders. Treat as the *feature checklist donor*, not a base.
- **Angular/Tauri (`DevContext.App` + `DevContext.Server`, gRPC):** single-scroll narrative canvas —
  landing → identity → entries → trace → architecture → graph(canvas) → stats → export → settings;
  scroll-spy; vibe system (terminal/hacker/modern); recents + GitHub stores; LLM-export overlay.
  **Crucially the server already exposes the query kernel** (`ListEntryPoints · GetMap · GetTrace ·
  GetNode · GetNeighbors · SearchNodes · GetStats · Render`) — but the UI only consumes the narrative
  render + entry list + stats. `GetNode`/`GetNeighbors`/`SearchNodes` have **zero UI**.

### 2.2 The gap in one sentence

The canvas is a beautiful *report*; the kernel underneath is a *navigable graph* — the UI never lets
you touch it. "Blunt / lacks smartness" is exactly this: every name on screen is dead text.

### 2.3 Direction — three moves, in order

**Move 1 — Everything is a node (the browse artifact, I4 core).**
Every type/member/route/entry name anywhere in the canvas is a link. Click → **Node Card** (slide-over
`sheet` — component exists): declaration, role tags, file:line (+ "reveal in editor" via `vscode://file/`
fallback OS open), in/out degree, neighbors grouped by seam kind both directions, "usages" = in-edges.
Every neighbor is again a link. Powered 100% by existing RPCs (`GetNode`, `GetNeighbors`). This one move
converts the report into a browser and is the single highest-leverage UX change available.

**Move 2 — Command palette (Ctrl+K).**
One input: fuzzy search over nodes/entries/routes (`SearchNodes` RPC exists) + verbs (`trace <hit>`,
`usages <hit>`, `open file`, `switch vibe`, `export section`). The palette IS the CLI vocabulary — same
verbs as `devcontext query` (§1.1), which keeps the faces unified and teaches both at once. Landing
state gets the same input as "analyze anything" (path / GitHub URL / recents).

**Move 3 — Sections get smart, not more numerous.** Per existing section:

| Section | Today | Upgrade |
|---|---|---|
| Identity | name/style/archetype | + **honesty ribbon**: scope note, entry→target coverage `N/M`, approx-share — the trust surface, always visible |
| Entries | flat list | kind chips w/ counts, text filter, auth badges (F1), sort by resolved-target first; row actions: Trace · Node Card · copy route |
| Trace | rendered text | interactive tree: expandable hops (re-query deeper on expand), seam-colored edges (SEAM_COLORS exists), `[verified]/[approx]` visible, provenance popover, "add to export pack" per subtree |
| Architecture | topology text | project graph w/ fan-in ranking; gateway ROUTES table when Gateway |
| Graph canvas | depth slider, zoom | seed from focus or interesting-points; role-tag coloring; click node → Node Card (Move 1); seam-kind filter chips |
| Stats ("Pipeline") | stage bars, extractor table, cache/tokens | **becomes Insights** (§3); engine telemetry collapses into an "Engine" drawer — keep the terminal-vibe waterfall there for the nerd appeal, off the main path |
| Export | one overlay | **packs**: Onboarding (identity+entries+topology), Trace pack (current trace+touched nodes), Review pack (insights+coverage); per-pack token estimate (render-time count only — no kernel budget) |

**Non-goals for I4:** multi-session tabs, snapshot diff (needs persistence — see DEV-PAINS P9),
web deployment (Server+JSON contract is the seam; nothing extra to build now).

---

## 3. Insights — the fix for "stats feel boring and boilerplate"

### 3.1 Diagnosis

`section-stats.ts` + `--stats` today show **engine telemetry**: stage waterfall, per-extractor timings,
cache hit-rate, token funnel. That's *about us*, and it's the same on every repo — definitionally
boring. The seam table (`GraphStats`) is the only repo-shaped row, and it reads as QA numbers.

**An insight = a fact about THIS repo + why it matters + a jump-off.** Test for every candidate:
*would a dev say this out loud to a teammate?* ("Did you know 12 of our endpoints are anonymous?" —
yes. "Stage 2 took 3.7s" — no.)

### 3.2 The shape (kernel-owned, all faces consume)

```csharp
// src/DevContext.Core/Insights/Insight.cs
public sealed record Insight(
    string Id,                 // stable, e.g. "auth.anonymous-endpoints"
    InsightCategory Category,  // Shape | Risk | Wiring | Data | Topology | Coverage
    Severity Severity,         // Info | Notable | Warning
    string Title,              // "12 of 70 endpoints allow anonymous access"
    ImmutableArray<string> Evidence,   // concrete items, capped, each linkable (node ids / file:line)
    string? JumpOff);          // "query entrypoints --kind http | filter [anon]" / focus suggestion

public interface IInsightSource   // catalog-registered, same hygiene as extractors
{
    string Id { get; }
    IEnumerable<Insight> Compute(AnalysisSnapshot snapshot); // pure, cheap, post-graph
}
```

`InsightsBuilder` runs all sources after `GraphAssembly`, ranks (Severity desc, then per-category cap 3,
global cap ~10), stores on the snapshot. Faces: CLI `--stats` prints them first (telemetry behind
`--stats engine`); desktop Insights section renders cards with linkable evidence (Move 1);
`GraphQuery.Insights()` + MCP `insights` tool return the records.

### 3.3 Launch set (v1 sources — all computable from data we already hold)

| Id | Category | Example output | Data |
|---|---|---|---|
| `shape.entry-mix` | Shape | "Half API, half background machine: 70 HTTP · 24 scheduled jobs" | entry kinds |
| `auth.anonymous` | Risk | "12/70 endpoints anonymous, incl. 3 POST" | `EndpointDetection.AuthAttributes` |
| `wiring.hubs` | Topology | "FeedsService is the hub — 31 in-edges (next: 12)" | `InEdges` degree outlier |
| `wiring.multi-impl` | Wiring | "IPaymentGateway has 2 registered impls" | DI detections |
| `wiring.external-events` | Wiring | "5 events consumed but never produced here — external contracts" | Sends/Raises × Consumes |
| `data.busiest-aggregate` | Data | "Order raises 6 domain events — busiest aggregate" | Raises grouped |
| `di.lifetimes` | Wiring | "142 registrations: 89 scoped · 40 singleton · 13 transient; 6 self-registered concretes" | `DiRegistrationDetection` |
| `topology.chokepoint` | Topology | "18 of 24 projects depend on Common" | project refs |
| `coverage.honesty` | Coverage | "Entry targets resolved 34/94 · 28% of edges approx — deeper profile available" | `GraphStats` |
| `graph.orphans` | Notable | "7 public services with no in-edges, not DI-registered, not entries — likely dead" | InEdges + DI + NoiseFilter (**must** exclude reflection targets via IndirectWiring detections — say "likely", cap at high confidence) |

Rules: every insight must be **noise-filtered** (production code only), **deduplicated against the Map**
(don't restate "this is Microservices"), **honest** (orphans say *likely*; approx edges counted), and
**linkable** (evidence = node ids the faces can resolve). Sources that fire on nothing stay silent —
an empty Insights section on a tiny repo is correct, never pad it.

### 3.4 Why this also fixes the Map's first screen

The top-3 insights join the Map Overview (one line each) — so the CLI's first screen answers "what is
this + what's notable", which is the go-to promise in its purest form.
