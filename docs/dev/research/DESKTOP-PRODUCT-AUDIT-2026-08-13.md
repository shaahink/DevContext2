# Desktop app — product & feature-design audit

> **DRAFT — for owner discussion, not yet a plan.** 2026-08-13. Written on `feat/agent-probe`
> after the deep eval (`DEEP-EVAL-2026-08-13.md`) and graph/detection audit
> (`GRAPH-DETECTION-AUDIT-2026-08-13.md`); this is the third leg — the **desktop product**.
> Method: full app inventory (routes/components/stores read from source), full engine
> supply-side inventory (what the kernel computes vs what reaches the wire vs what the app
> renders), doc-trail harvest (every prior UI audit + DECISIONS.md + PRODUCT-DIRECTION.md),
> and a screen-read of the post-S10 captures (`eval-results/2026-07-28/r3-current-state/*-s10`
> — the shipped v1.0.5 state; re-drive before building, per standing rule).
> Code claims marked **[audit 2026-08-13]** were swept today by subagents — re-verify
> file:line before fixing (house rule).

---

## Verdict in three sentences

The desktop app is a well-crafted, honest **report about an analysis** — and the product it
sits on is a **navigable, source-anchored code graph**, which the app almost never lets the
user touch: of the five locked artifacts (Entry points · Topology · Trace · Stats · **Browse**),
Browse was never built, code itself appears only as a peephole (`ReadSource(MEMBER)` in a
side-panel `<pre>`) and as flattened text soup inside trace rows, and the engine's best
answers — impact, seam paths, tests-for, config bindings, layer violations — are implemented
server-side and **never called by the app at all**. The R3 redesign (S6–S10) fixed what the
pages *say*; it did not change what the product *is*: a dashboard vocabulary of
entries/wired%/nodes/edges instead of the developer's vocabulary of "how does this work,
what calls this, what breaks if I change it, show me the code." The cut-through move is to
rebuild the centre of the app around **one code-anchored flow workspace** — the trace and
the actual source on one surface, every hop clickable, every claim carrying its file:line —
and to demote the three look-alike dashboards to what they really are: an orientation
strip, a printable dossier, and a triage list that all deep-link into that workspace.

---

## 1. What the app is today (inventory, compressed)

7 routes + shell. Full component-level inventory lives in the agent sweep; this is the
product-eye summary. Keep-list from the 07-27 audit still stands (identity strip, freshness,
Shift+E table, Studio machinery, MCP page, library front doors, honest empty-state tone).

| Surface | What it answers today | Read |
|---|---|---|
| **Home** (3-state) | "What is this repo?" — identity strip + scope switcher + archetype-shaped hero (canvas / front doors / command surface) + 3 stat tiles + top flows + START HERE | The strongest page. But it answers with *analysis accounting* (109 entries · 64/109 wired · 8% verified) before it answers with *meaning*. |
| **Explore** | "Let me work" — EntryDeck │ Stage (system/flow/node altitudes, Tree/Graph) │ Inspector (Details/Code/Insights/Call Stack/Trail) | The workspace. The trace tree is the product's crown jewel and it renders as **annotated grep output** (see §3.C). Code is a peephole. |
| **Atlas** | "Architecture dossier" — canvas again + top flows again + event board + stores + per-service + cross-cutting + hub radar; md export | A printable report pretending to be a live page; its first screen ~duplicates Home. Its export is the right idea (see M5). |
| **Insights** | "What's notable/risky" — severity cards with evidence chips + Trace-it links | Good content, no triage model; D-F (3 overlapping auth cards) still open; cards route *into* Explore but nothing routes back. |
| **MCP** | "Agent plumbing" — status, host configs, sessions, live tool feed, try-a-tool | Genuinely good and unique. Hand-kept tool list drifts (bug #4). |
| **Context Studio** | "Turn exploration into an agent pack" — scope picker, cards, budget, live preview, verification | The product's best differentiated feature (07-27 verdict) and its best loop (trail→pins→pack). Still opens on two empty panes (D-G), entries-only scope, dead for libraries. |
| **Settings** | prefs, storage, server, vibes | Fine. |

Shell: 6-tab multi-repo workspace, omnibox (Ctrl+K, verbs Trace/Node/Usages/Impact/Copy),
NodeCard sheet, hover peek, trail with undo/redo + pins (Explore-only), ticker, three vibes.
State: clean per-tab signal stores; server sessions adopted on boot.

**Dead or hollow, in the shipped app** [audit 2026-08-13]:
- **GitHub-URL path is dead code** (`state/github.store.ts`, `ui/repo-card`, `data-access/github-api.ts`
  — zero importers) while "paste a GitHub URL" is in the v1 vision (PRODUCT-DIRECTION §5).
- **Layer & Feature lenses are hollow slots** — selectable chips that only recolor borders;
  engine data "not yet in proto" (D9 comment in `lens-switcher.ts`).
- **`MapResponse.stack` (proto field 13) is never populated** by `ProtoMapper`, yet the
  identity strip, the Atlas chip header, and MCP `overview` all read it — three consumers
  rendering an always-empty field. The S8 dead-by-construction class, in the direction the
  S9 contract sweep does not look (field has readers, lacks a writer). **New find — file as a bug.**
- Inspector dock has no resizer (TODO in `workbench-page.ts`); trail/pins unreachable
  outside Explore; `TraceStore.reroot` is client-side only, capped at fetched depth;
  `createTab()` at MAX_TABS returns the active tab's id (OWNER-TODO §5).

---

## 2. The supply/demand ledger — what the engine can answer vs what the app asks

The heart of this audit. Left column = capability that exists in Core **today** (file paths
in the engine inventory); right = what the desktop does with it.

| Engine capability (exists, tested) | Wire | Desktop today |
|---|---|---|
| **Trace tree** with per-step seam, provenance file:line, resolution tier, salient lines, pipeline wrap, truncation names | `GetTrace` (partial: see below) | Flow tree — the one deep feature |
| **Trace honesty annotations**: `MultiImplCount`, `DiHostCount`, `TestOnly`, `OmittedNames[]` | **dropped by ProtoMapper** | CLI shows them; app *cannot* |
| **`GetSeam(from,to)`** — shortest wiring paths between ANY two nodes, hop-by-hop with file:line | ✅ RPC exists | **Never called** |
| **`GetImpact`** (up/down, by node or by files), `BlastRadius`, `NodesInFiles` | ✅ RPC (BlastRadius: no RPC) | Omnibox verb + "Reached by N flows" line. **No surface.** |
| **`FindTestsFor`** — test methods reaching a node, with distance + file:line | ✅ RPC exists | **Never called** |
| **`ConfigLookup`** — config-key → usage sites with file:line | ✅ RPC exists | **Never called** |
| **`ReadSource`** — member span or ±N window, language-tagged | ✅ RPC | MEMBER mode only, one node, `max-h-80` peephole; WINDOW never used |
| **Source anchors on everything** — every node (FilePath+LineNumber+SourceBody), every edge (Provenance), every entry/seam-hop/impact-row/test-ref/config-binding | mostly ✅ (TraceNode carries only the provenance *string*) | `file:line` shown as copyable text; **never used to show code** |
| **`LayerViolations`** (from→to layer, edge, provenance) | ❌ not on wire | **Invisible everywhere** — computed, persisted, unread |
| **Gateway `Routes`** (ocelot/YARP up/downstream) | ❌ not mapped to proto | Invisible; Gateway archetype has no view at all |
| **Full detections inventory** (DI registrations w/ lifetimes+shapes, middleware **with pipeline order**, EF entities w/ keys, anti-patterns, indirect wiring, Refit/gRPC client method routes) | ❌ no RPC | Invisible in the modern path (10 oldest types render in legacy CLI renderers only) |
| **`TypeDiscovery`** — method signatures, XML docs, properties, base types, interfaces | indirect (LibrarySurface, packs) | No type-detail view; NodeCard shows degree + tags only |
| **`ArchitectureSignals`** (38, each w/ confidence + evidence) | ❌ (kernel JSON only) | Invisible — the "why did you classify this repo this way" story |
| **EventWiring** incl. cross-service pairs, orphans, per-participant provenance | partial (`EventWireRow` booleans) | Atlas table (good) — pairs/provenance dropped |
| **Flow index** (per-flow verified %, touches, emits, hubs) | ✅ | Home/Atlas ranking + hub radar (good) |
| **Context packs** with per-section provenance + staleness verification | ✅ | Studio (good) — entries-only scope |
| **Entry scores** (`Score`, `Reach`, `CrossProjects`) | ✅ | deck sort (good); `SeamRichness`/`EntityTouches` have **zero readers anywhere** |

Two readings of this table:

1. **The app under-consumes its own kernel.** Three whole RPCs unused, the richest trace
   fields dropped at the mapper, the graph's source anchors used as copyable strings instead
   of as *doors into code*. The 07-02 FACES diagnosis is still literally true: *"the kernel
   underneath is a navigable graph — the UI never lets you touch it."* I11 made names
   clickable; it did not change what clicking *reaches* (a card with numbers on it).
2. **What the engine cannot honestly answer yet is also visible here** — impact/usages
   ride on a graph with the #11/#12/#8 edge holes and no inheritance edge kind, so any
   impact/browse surface ships **after or with W2**, wearing the honesty vocabulary
   (verified/approx, "N sites not bound"). This audit's proposals are sequenced against that.

---

## 3. Product diagnosis — five structural findings

### A. The app speaks analysis, not development

Its nouns are entries, wired %, verified %, nodes, edges, snapshots. The developer's
questions (DEV-PAINS P1–P15) are: *what is this? how does X work? what calls this? what
breaks if I change it? is this endpoint secured? who publishes this event? which tests
cover this? feed my agent the right context.* Today the app answers P1 (Home) and half of
P2 (trace, poorly rendered), P5 partially (auth padlocks + insight), P11 (Studio). P3 — the
fear moment, "what breaks" — has **no surface**. P7 has a table on Atlas. "Which tests
cover this" — the engine answers it; the app doesn't ask. The redesign should be measured
by which of those questions a first-session user can answer *without being taught*.

### B. Browse was never built — and code is the missing material

Of PRODUCT-DIRECTION §3's five artifacts, Browse ("navigate the structure and jump
around") is the one with no surface. What shipped instead: a NodeCard modal (kill decision
A-1 still open), a Neighbors list at `node` altitude, and a Code section that renders one
member's body in a 320px-tall box after a manual "load source" click. Meanwhile **every
node and every edge carries a file:line** — the engine is a machine for anchoring
architecture claims to source, and the UI treats source as a citation format instead of a
navigable medium. The user never *sees the code the claims are about* unless they leave
for their IDE — at which point the app has lost the session.

### C. The trace — the crown jewel — reads as annotated grep output

Look at the eShop `POST /api/orders/` capture: each row is a seam chip + node link +
1–3 lines of *flattened* source text — XML doc comments inline ("/// <summary> ///
Provides a base implementation…"), signatures crushed together ("public class
IdentifiedCommand<T, R> : IRequest<R> where T : IRequest<R> public T Command { get; }").
The story (HTTP → command → handler → events → consumers across services) is structurally
there and *narratively* buried. Three separate causes, all fixable:
1. `Salient[]` is raw first-lines-of-callee, not a designed snippet (no code typography,
   no doc-comment stripping, no signature formatting).
2. The tree is the only altitude — there is no "story" reading (the A-2 collapse row was
   the first move in this direction and it worked).
3. The code the step points at is never on screen — the reader can't verify or deepen a
   step without a side-trip, so the trace stays *text about code* instead of *a path
   through code*. (Also: the proto drops `MultiImplCount`/`TestOnly`/`OmittedNames`, so
   the tree is less honest than the CLI's markdown of the same trace.)

### D. Impact has machinery and no surface

`GetImpact` (node or files→entries, up/down, hops), `Seam`, inverse edges, `tests_for` —
the engine's whole reverse-navigation suite reaches the desktop as one omnibox verb and
one line of Inspector text. The probe priced this question (class C, the graph's reason to
exist) and the backlog gates it (#11/#12/#8 + no inheritance edge). The right product
posture: **build the impact surface as W2's UI face** — same acceptance question ("does
the app resolve the true impact set for the probe's class-C question on eShop"), shipping
the honesty vocabulary on every row, and *withholding* (C-3 style, with the reason) until
the graph under it stops lying.

### E. Three pages draw the same picture; the loops that differentiate are hidden

Home hero, Explore system altitude, and Atlas §1 render the same services canvas at three
sizes. No page owns topology; none progresses it (overview → focus → detail). Meanwhile
the genuinely differentiated loops — trail → pins → Studio pack; insight → trace; ticker
surfacing discoveries — are discoverable only via a status-bar hint rotation. IA cost:
page-thinking (the 07-03 brief's diagnosis) survived the redesign; each capability is
still a route. The chrome is good; the *center of gravity* is missing.

---

## 4. The cut-through concept — one code-anchored flow workspace

> The owner's instinct ("live code browsing integrated with the way we interpret a flow")
> and this audit's findings point at the same object. Working name: **the Reader**.
> This is a direction to refine together, not a finished spec. It stays inside the
> five-artifact contract: it is Trace × Browse, not a sixth artifact.

**The thesis:** DevContext's unique capability is that it can put *the wiring* and *the
source* on one surface with receipts. An IDE shows files and makes you reconstruct flows;
the CLI/MCP show flows without the files; the Reader shows **the flow through the files**.
Nobody else in .NET-land owns that view.

**The shape (three lanes, one selection):**

```
┌─ Flow spine ─────────┬─ Code, following the selection ────────┬─ Context rail ──┐
│ ▶ POST /api/orders    │ OrdersApi.cs:47        [verified]      │ Node facts      │
│  1 OrdersApi          │                                        │ in 2 · out 5    │
│    .CreateOrderAsync  │   public static async Task<...>        │ file:line       │
│  2 send ─ Identified  │   CreateOrderAsync(...)                │ [open in IDE]   │
│    Command            │   {                                    │                 │
│  3 handle ─ Identified│ →   await mediator.Send(              │ Who calls this  │
│    CommandHandler     │       new IdentifiedCommand(...));     │ Tests: 3        │
│  ▸ crosses 5 services │       ▲ sends IdentifiedCommand        │ Impact: 12      │
│  4 raise ─ OrderStarted│      handled in Ordering.API ───────▶ │ Config keys: 2  │
│  …                    │                                        │ Insights here: 1│
└──────────────────────┴────────────────────────────────────────┴─────────────────┘
```

- **Left — the flow spine.** The trace, rendered as a *story*, not a dump: numbered hops,
  designed typography, seam verbs as the narrative ("send → handled in → raises → consumed
  by"), the A-2 collapse idiom for the mesh, truncation with names (`OmittedNames` — once
  the proto carries it). Selection is the cursor (the I11 rule, kept).
- **Middle — the code pane, following the selection.** Selecting a hop opens the real file
  at the provenance line (not a 20-line snippet — the file, scrollable), with:
  - the **hop's line highlighted**, and
  - **seam gutter annotations**: every line that is a call-site the graph knows gets its
    edge rendered in the margin ("→ sends CreateOrderCommand · handled in Ordering.API
    [verified]"), clickable to advance the spine. Reading code *with the wiring overlaid*
    is the product's capability made visible — the exact thing grep and the IDE can't do.
  - Escape hatch on every claim: the annotation IS the provenance.
- **Right — the context rail.** The current Inspector, re-pointed at questions: who calls
  this (in-edges), which tests reach it (`FindTestsFor`), what config it reads
  (`ConfigLookup`), what breaks below it (impact teaser → M2), insights touching this
  node, and the trail/pin loop. Every row is an answer, not a stat.

**Why this is the cut-through and not a tweak:** it changes what the product *is* — from
"a report you read before opening your IDE" to "the place you actually read the codebase."
It gives the trace its missing material (code), gives Browse its missing surface (the code
pane + rail are the browser), makes the honesty idiom experiential (verified = solid
gutter mark, approx = dashed), and it is the human twin of what `get_context` serves
agents — one mental model across faces.

**What it needs from the engine/proto (the shopping list — all small, none blocked):**
1. `TraceNode` gains structured `file_path` + `line_number` (today: provenance string only).
2. `ReadSource` file mode (or `GetFileSource(path)`) with byte/line bounds — WINDOW exists,
   a whole-file read with a cap is the missing spelling.
3. A per-file edge overlay query: "edges whose provenance falls in this file" —
   `GraphQuery` can already answer it (`NodesInFiles` exists); needs an RPC.
4. ProtoMapper stops dropping `MultiImplCount` / `DiHostCount` / `TestOnly` / `OmittedNames`.
5. (Later, for spine re-rooting) server-side re-root: trace from an arbitrary node — today
   the client fakes it within fetched depth.

**Honesty guards it must wear:** annotations only where edges exist (no "this file is
fully mapped" implication — an unmapped call site simply has no mark, and the rail says
"N call sites in view · M bound"); the #11/#12 era means gutter coverage will visibly
improve as W2 lands — that is a feature (the UI makes engine progress legible), not a bug
to hide.

---

## 5. The other moves (each stands alone; ranked by product leverage)

**M1 — The Reader** (§4). The centre of the redesign; everything below deep-links into it.

**M2 — Impact: give P3 a surface.** A node/entry/file-set → "what reaches this / what
this reaches", grouped by service, each row with hops + confidence + file:line, exportable
as a Studio pack (the "Change-impact pack" preset finally means something). Ships with W2,
shares its acceptance test, wears withhold-with-reason until then. This is also the
sharpest agent-story demo: the same question the probe asked, answered on screen.

**M3 — IA consolidation: one picture, three altitudes.** Home keeps the canvas as *hero*
(orientation); Explore's system altitude becomes the *working* canvas (same component,
richer lenses); Atlas stops re-drawing it live and becomes what its buttons already claim:
**the exportable dossier** (Export one-pager / Download .md), generated from the same
facts, plus the sections that make sense only on paper. Net: the "three canvases" cost
disappears without deleting anything users know. (Alternative worth debating: fold Atlas
into Home as a "print/report" action and free a nav slot for Impact.)

**M4 — Browse affordances everywhere names appear.** The Reader is the destination; the
graph's cheap wins arrive earlier: NodeCard → docked Neighbours section (closes A-1),
"how does A reach B" (GetSeam) as an omnibox verb ("path OrdersApi → EventBus"), tests-for
and config chips on the rail, click any `file:line` anywhere → Reader at that line. Each
is one existing RPC away.

**M5 — Studio as the agent hand-off centre (D-G, reframed).** Studio's problem isn't the
empty panes; it's that it is framed as a *builder* when its job is a *hand-off*. Default
state = the current trail/pins as a proposed pack (never empty after any exploration);
scope picker gains nodes/types (not just entries — unlocks libraries); picker rows get
their identity from the target member + route tail (the D-G data already exists); "Point
your agent here" on Home routes *through* Studio, not past it to /mcp. The MCP live feed
gains "replay this call as a pack" — the human watches what the agent asked and can hand
it better context. This is the desktop's role in the agent story W1/W3 are earning.

**M6 — Insights become triage, and every claim becomes a door.** D-F dedup at the engine
(one auth fact, one card), severity sections become a work queue (ack/dismiss per repo,
persisted locally), every evidence chip deep-links into the Reader anchored at the
evidence line. Plus the unsurfaced honesty content this audit found: LayerViolations
(architecture-drift card), middleware pipeline order (a "request pipeline" strip on
Atlas/dossier), gateway Routes (the missing Gateway archetype view), ArchitectureSignals
(the "why I classified it this way" disclosure on the identity strip — the trust story in
one click).

**M7 — Hygiene batch (do first, cheap, mostly deletions).** Kill or finish the hollow
surfaces: Layer/Feature lens slots (hide until D9 data exists), dead GitHub store/repo-card
(delete or actually build the URL path it was for — owner call: it IS in the v1 vision),
`MapResponse.stack` (populate or stop rendering — new bug), MCP page tool list served from
the server (#4), unselectable high-contrast theme, `createTab()` MAX_TABS lie (OWNER-TODO
§5), dock resizer TODO. Each is small; together they remove the "confident surface over
absent substance" instances the app itself still carries.

---

## 6. What NOT to do

- **Not an IDE.** No editing, no file tree as primary nav, no language server. The Reader
  is flow-anchored reading; the file tree is at most a rail affordance. The moment we
  rebuild Solution Explorer we've lost the slot (the IDE already owns it).
- **No sixth artifact.** Everything above is Trace/Browse/Topology/Stats/Entry-points
  recombined. (PLAN §6: a sixth needs owner override — none proposed.)
- **No impact UI before its graph.** M2 ships with W2 or wears the withhold. Principle 2
  applies to features, not just copy.
- **Don't grow the menu.** The MCP lesson (22 undescribed tools = catalog soup on the
  agent face) applies to nav items and toggles. Every move above consolidates or deepens;
  only Impact adds a top-level concept, and it can live inside Explore.
- **Don't re-render markdown as the UI.** The Reader renders *source + structured
  overlays*, not the CLI's prose. (UI-UX-GUIDELINES §3 still binding: data gets
  components.)

---

## 7. Sequencing sketch (for discussion only — not the plan)

1. **M7 hygiene** + the proto/mapper shopping list (§4) — days, unblocks everything,
   zero design risk.
2. **Reader v1** — code pane follows trace selection, file view + hop highlight, rail
   re-pointed (tests/config/usages). No gutter overlay yet. This alone changes the
   product's category.
3. **Reader v2** — seam gutter annotations + omnibox "path A→B" (GetSeam) + trail/pins
   everywhere.
4. **M3 IA consolidation** + **M6 insights/triage** (+ dossier export absorbing Atlas).
5. **M2 Impact** — lands with W2 (engine) as one release-gate story.
6. **M5 Studio/agent hand-off** — pairs naturally with W1/W3 (post-fix MCP surface).

Open interactions with the standing program: W2a/W5 (engine) run independently; S11's
D-F/D-G fold into M6/M5 rather than being decided separately; the render-kernel decision
(deferred since R3) is exactly the Reader's render layer — decide it there.

---

## 8. Open questions for the owner (the refinement agenda)

1. **Is the Reader the centre?** If yes, Explore *becomes* it (flow spine replaces the
   tree; code pane replaces the canvas as the default focused view; canvas stays as the
   system altitude). If no — which half matters more: code-following-selection, or gutter
   wiring overlays?
2. **Atlas: page or export?** (M3 has two spellings — keep the page as dossier-preview,
   or fold it into Home + a Report action.)
3. **GitHub-URL path: delete the dead code or build the feature?** It's in the v1 vision
   and it's a discovery surface; but it rides on clone/index infra (kernel v2). A call
   either way ends the zombie state.
4. **Impact timing: UI-with-W2, or UI-behind-a-flag earlier** to pressure-test the design
   on the honest subset (entries→targets are already trustworthy)?
5. **How far does the desktop go into the agent story?** M5's "replay agent call as pack"
   is one step from "the desktop is the agent's cockpit" — attractive, but it competes
   with the human-lens identity. Where's the line?
6. **Monaco vs CodeMirror vs Prism-plus-virtual-scroll for the code pane?** (Tauri bundle
   size vs selection/decoration APIs vs a hand-rolled gutter. Needs a spike; the choice
   shapes Reader v2's overlay work.)
7. **Naming.** "Explore/Atlas/Insights" survive? Or does the Reader deserve the front
   door ("Read" as the primary verb of the product)?

---

*Companion docs: `DEEP-EVAL-2026-08-13.md` (agent face), `GRAPH-DETECTION-AUDIT-2026-08-13.md`
(kernel). The three together are the pre-release master-plan inputs. This one is DRAFT
until the owner pass.*
