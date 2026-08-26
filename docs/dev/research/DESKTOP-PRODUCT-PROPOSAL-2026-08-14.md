# The Reading Room — desktop product proposal, v2

> **DRAFT v2 — for owner decision.** 2026-08-14. Supersedes §4–§7 of
> `DESKTOP-PRODUCT-AUDIT-2026-08-13.md` (its §1–§3 diagnosis stands and is cited as
> evidence). Method: the four 2026-08-13 audits + the pre-release outcome
> (`PRE-RELEASE-PLAN-2026-08-13.md` §3, both runs DONE) + a full source inventory of the
> post-Z1 app + a **live re-drive on `develop` @ `170d304`** (eShop @ `9b4f943`, captures
> and findings in `eval-results/2026-08-14/desktop-redrive/`). Code claims were swept
> today — re-verify file:line before building (house rule).

---

## 0. The brief this answers

The owner's direction for this round, decoded into requirements:

1. **"Load a repo and browse it — not an IDE — browse the code clearly, not file by file
   but flow by flow."** Flow-first *code* browsing is the product's centre. Not a trace
   summary next to a code peephole: the code itself, navigated by flows.
2. **"Human manual context creation."** The human curates context by hand, continuously,
   while reading — not by filling a form in a separate builder afterwards.
3. **"A noise/signal split."** The app must separate what matters from what doesn't — in
   the reading, and in what gets handed to an agent — and the human must be able to move
   the line.
4. **Constraints:** the existing desktop is *evidence, not the source of truth*; and the
   answer must be a product design, not a ranked feature list — the v1 proposal's M1–M7
   move-list framing is explicitly what did not land.

## Verdict in three sentences

The first proposal diagnosed the product correctly ("a report about an analysis, sitting
on a navigable code graph it never lets you touch") and then answered at the wrong
altitude: seven ranked moves that optimize the existing seven-route app, leaving the
report-shaped IA — dashboard pages, a permanent entry deck, code in a rail — standing.
The live re-drive proves the altitude was wrong by experiment: every prerequisite the v1
plan asked for has now shipped (pins, converged packs, FILE-mode reads, the per-file
overlay, honest MCP), and the app still *reads* as a report, because no amount of landed
moves changes what the centre of the screen is. This proposal redesigns the product
around one workspace — **the Reading Room**: a flow spine, the real source following it,
and an answers rail — with a two-vocabulary object model (the engine's *truth*, the
human's *salience*) that makes the noise split and manual curation structural rather
than features.

---

## 1. The ground moved — v1's hedges are obsolete

The v1 proposal was written while the graph was known-lying and Studio was a monument.
Both runs of the pre-release program have since closed (2026-08-14):

| v1 hedge / precondition | State now |
|---|---|
| "Any impact/browse surface ships after or with W2, wearing the withhold" | **W2 = E1, DONE.** #11/#12/#7/#8 fixed, dogfood in-edge invariant in the battery. The impact hedge is retired; `GetImpact` is wrapped in `devcontext-api.ts:240` with **no UI caller** — purely a UI gap now. |
| The §4 proto/mapper shopping list (trace file:line, FILE reads, per-file overlay, honesty fields) | **All shipped** (M1 stage): `TraceNode.file_path/line_number` + `omitted_names/multi_impl_count/di_host_count/test_only` (`devcontext.proto:429-436`), `READ_SOURCE_MODE_FILE` with truth caps (`:1241`), `FileOverlay` with placed/unplaced honesty (`:1271-1292`). The Inspector already calls both (`inspector.ts:581-621`). |
| Studio has no working inputs / no real output | **N0–N4 DONE.** Pins seed packs, packs are symbol-rooted with usage + verified counts, Save writes `.devcontext/packs/<slug>.md` + agent line, MCP page measures instead of performing. |
| Agent story unproven | Adoption gate **CLEARED** (0.306 vs 0.20 floor). The desktop's hand-off surface now has a measured consumer. |

What is still true from v1's ledger: `GetSeam`, `ConfigLookup`, `FindTestsFor` are not
even wrapped in the API facade; `Render`/`GetInterestingPoints`/`GetImpact` are wrapped
with zero UI callers; `SeamRichness`/`EntityTouches` have no readers. The engine's best
answers still don't reach a human.

## 2. What the v1 proposal got right, and where it failed

**Stands (and this proposal builds on it):** the supply/demand ledger method; the
diagnosis pentad (§3.A–E: analysis-vocabulary, Browse never built, trace-as-grep-output,
impact surfaceless, three look-alike canvases); the three-lane Reader sketch; the
honesty guards; the "not an IDE / no sixth artifact" fences.

**Failed, specifically:**

1. **It answered a product question with a priority list.** M1–M7 "ranked by product
   leverage" is a menu. A menu invites picking M7+M4 and deferring the identity change —
   which is exactly what the pre-release run (correctly, per its charter) did. The shape
   of the answer permitted the outcome the owner didn't want.
2. **It kept the IA it indicted.** §3.E names page-thinking as the disease, then M3
   consolidates the pages instead of dissolving them. Seven routes in, seven-ish routes
   out.
3. **It made the Reader an upgrade of Explore** — inheriting the workbench layout whose
   left lane is a permanent 110-row entry deck. The deck occupies the exact lane the
   flow spine needs; a Reader built inside that frame keeps code in a rail forever.
4. **It had no salience model.** §3.C treats the trace's noise as a typography problem
   ("designed snippet, doc-comment stripping"). The live tail of `POST /api/orders/` —
   `LogInformation`, `LogWarning`, `BeginScope`, `PadLeft`, `Substring` ranked as flow
   steps while the real dispatch is one `approx` leaf (re-drive finding 2) — is not a
   typography problem. The tool has no opinion about what matters inside a seam, and no
   way for the human to give it one.
5. **It under-wrote the human's hand.** Curation appeared only as Studio reframing (M5).
   Pins existed; clips, notes, naming, muting — the gestures of *manual* context
   creation — appeared nowhere.

**The experiment that settles it:** every v1 prerequisite landed (N0–N4 + M1), and the
re-drive still reads as a report — trace text soup centre-screen, code in a `max-h-80`
box, the overlay a list under the code instead of a gutter on it, the NodeCard modal
(A-1, decided 2026-07-28) still alive. Incremental moves onto this IA converge to a
better report, not to a reading tool.

---

## 3. The product, from first principles

**Who and when.** A .NET developer in front of a repo they don't fully hold in their
head — day-1 onboarding, a cross-service change, a review, prepping an agent. Their
questions are DEV-PAINS P1–P15: *what is this, how does this flow actually work, what
calls this, what breaks, which tests cover it, feed my agent the right context.*

**The thesis.** An IDE shows files and makes you reconstruct flows in your head. This
engine materializes the flows — with seams, provenance, and honesty tiers — and anchors
every claim to a file:line. So the app's identity is:

> **The reading room for a .NET repository.** You browse *flows*, the way you browse
> chapters — and the text of each chapter is the *actual source*, with the wiring drawn
> in the margin. Reading leaves a trail of what mattered; that trail, curated by hand,
> IS the context you hand to an agent or a teammate.

Three consequences, one per owner requirement:

- *Flow-by-flow browsing* means the *flow library* is the table of contents and the
  *source file* is the page. Files are never the nav; they are the material a flow walks
  through.
- *Manual context creation* means every reading gesture (pin a hop, clip a span, note a
  caveat, name a flow) accumulates into a curated collection — the pack is the
  *by-product of reading*, not a separate authoring task.
- *The noise split* means the product carries **two orthogonal vocabularies** and never
  confuses them: **truth** (verified/approx/omitted — the engine's, already built) and
  **salience** (signal/quiet/muted — defaulted by the engine, owned by the human).

**Fences (unchanged, restated):** not an IDE — no editing, no language server, no file
tree as primary nav, "open in IDE" everywhere; no sixth artifact — everything here is
Trace × Browse × Stats recombined; no agent runner in the app; no prompt-templating IDE.

## 4. The object model

The design in one table. Two new persistent objects (Lens, Collection), everything else
exists today.

| Object | What it is | Lives where |
|---|---|---|
| **Flow** | An entry (or any node, once server re-root lands) + its trace. First-class: rankable, **nameable, hideable, starrable**. | Engine (trace) + Lens (human title/star/hide) |
| **Hop** | One step of a flow: seam verb + node + provenance + truth tier + **salience tier**. | Engine (`TraceNode`) |
| **Truth** | `verified / approx / omitted-N / N-impls / test-only` — the engine's honesty about the wiring. Already on the wire. | Engine. Human cannot edit it. |
| **Salience** | `signal / quiet / muted`. *Quiet* = engine default (logging, BCL string ops, framework glue, test-only bindings). *Muted/marked* = human override, any scope (hop, node, namespace, seam kind, whole flow). | Engine defaults + **Lens** overrides |
| **Lens** | The per-repo human curation layer: mutes, marks, flow names/stars/hides. Versionable, shareable — the team's accumulated "what matters here." | `.devcontext/lens.json` in the analyzed repo (same convention as packs; gitignore = owner call) |
| **Collection** | This session's gathered material: pinned hops, **clipped code spans**, **notes**, starred flows. Feeds the pack proposal. | Per-tab store (exists as trail/pins; grows clips + notes) |
| **Pack** | The hand-off artifact. Gains `clip` and `note` cards; ships the lens's exclusions in its OMITTED ledger ("muted by your lens: …"). | Engine (`ContextPackBuilder`) + `.devcontext/packs/` |

The rendering law that keeps this honest (extends C-3's withhold rule): **salience never
deletes, it folds.** Quiet hops collapse into one expandable row with a count and a
reason ("· 6 quiet hops — logging, string formatting"); muted flows leave a named tray
("14 flows muted by your lens"); packs name every lens exclusion. The truth vocabulary
is never restyled by salience — an `approx` mark looks the same on signal and on quiet.

## 5. The information architecture — three surfaces, five nav items

```
Home  ·  Read  ·  Context  ·  MCP  ·  Settings
```

**Home — the cover page** (keep, reframe). The 3-state front door stays (start hero /
run console / digest — it is good). The digest reframes from dashboard to *cover page
of the flow book*: identity strip + confidence ledger (keep verbatim), the archetype
hero (keep, services only), and the **table of contents** — flows grouped and ranked,
wearing their human names and stars once the Lens has them. "Needs attention" absorbs
the Insights page as a triage block (post D-F dedup; ack/dismiss persisted in the
Lens). START HERE opens the Reading Room.

**Read — the Reading Room** (the centre; replaces `/explore`, `/atlas`, `/insights` as
routes). Full design in §6.

**Context + MCP — the Desk** (keep as just rebuilt; relocate the entry points). The
N0–N4 work is weeks old, measured, and good — this proposal deliberately does **not**
rebuild Studio or the MCP page. What changes is where curation *happens*: in the
Reading Room, as reading gestures. Studio becomes the assembly bench where the
Collection arrives already shaped (its pack-proposal mechanism exists —
`pack-proposal.ts` — it gains clips/notes/lens as inputs). The MCP page stays the
observation deck.

**What dies** (each was decided or diagnosed already; this proposal executes):

- **The NodeCard modal** — A-1, decided 2026-07-28, still alive (re-drive finding 4).
  The Reading Room's rail is the only inspector; neighbour lists move there.
- **Atlas as a route** — it is a printable dossier pretending to be a page (v1 §3.E,
  confirmed live: its first screen duplicates Home). It becomes **Export** on Home:
  one-pager / .md, generated from the same facts (the builder exists,
  `atlas-page.ts:547-620`). Its genuinely-live sections (event board, hub radar) become
  Reading Room rail answers and TOC groupings.
- **Insights as a route** — cards become (a) the Home triage block and (b) rail
  annotations anchored where the evidence lives. Blocked on D-F dedup (engine), which
  S11 already owns.
- **The permanent entry deck** — the 110-row listbox lane becomes the Reading Room's
  **left lane only when no flow is open** (TOC state) and an overlay (`Ctrl+K` /
  "Browse flows") otherwise. The deck's machinery (filters, kinds, j/k) survives inside
  the TOC and overlay; the Entry Browser and Table Lens overlays survive untouched.
- **Dead code** — GitHub store/repo-card/api (zero importers), `ArchitecturePanel`,
  unreferenced UI primitives. Delete; the GitHub-URL *feature* stays in the vision and
  returns on kernel v2 (owner call recorded in §11.8).

**What survives untouched** — the identity strip + confidence ledger + scope row, tabs +
adoption, omnibox, keyboard discipline, vibes, statusbar honesty, withheld idiom,
Studio machinery, MCP deck, zen mode, Shift+E table.

## 6. The Reading Room

The one workspace. Two states, following D-A's own logic (the centre follows focus) to
its conclusion:

**No flow open — the repo state.** Centre: the working topology canvas (the one canvas;
Home's hero is its compact echo). Left: the flow book — grouped, ranked, named,
starred, with muted flows folded into a named tray. Right rail: repo answers (entries
by kind, event board, hubs, triage). This state absorbs what Explore's system altitude,
Atlas, and the deck each half-did.

**Flow open — the reading state:**

```
┌─ SPINE — this flow ────────┬─ THE PAGE — source, following selection ──────┬─ RAIL — answers ─────────┐
│ ▸ Place an order ★         │ OrdersApi.cs · Ordering.API      118–168/185  │ OrdersApi.CreateOrder…   │
│   POST /api/orders/        │                                               │ Ordering.API · :118      │
│                            │  public static async Task<…> CreateOrderAsync(│ [open in IDE] [clip]     │
│ 1 ▶ OrdersApi              │      [FromHeader("x-requestid")] Guid request…│                          │
│     .CreateOrderAsync      │  {                                            │ WHO CALLS THIS      1    │
│ 2   send IdentifiedCommand │      //mask the credit card number            │ TESTS REACHING      3    │
│     ▸ verified             │ ▶    services.Logger.LogInformation(…);       │ CONFIG READ HERE    2    │
│ 3   handled by             │      …                                        │ WHAT BREAKS BELOW  12    │
│     IdentifiedCmdHandler   │ ▌    await mediator.Send(command);            │   entries in 3 services  │
│ 4   crosses 8 services…    │      │ send IdentifiedCommand ── handled in   │ INSIGHTS HERE       1    │
│     ▸ 15 hops · 37 omitted │      │ Ordering.API [verified]  → step 3      │                          │
│ ·   6 quiet hops           │      …                                        │ COLLECTION               │
│     logging · formatting   │  }                                            │ 2 pins · 1 clip · 1 note │
│ 5   raises OrderStarted…   │  · 14 call sites in view · 11 bound           │ [→ Context]              │
└────────────────────────────┴───────────────────────────────────────────────┴──────────────────────────┘
```

**The spine** (left) is the current flow only — numbered hops, seam verbs as the
narrative, the CrossService collapse row kept verbatim, quiet hops folded (§4's law),
truth badges unchanged. The salient-text soup dies: a hop is one designed line (verb +
member + qualifier), because the body now lives one lane to the right (re-drive finding
3 — the soup is redundant the moment code is on screen). `j/k` walk hops; `Enter`
focuses the page; double-click re-roots (server-side once the RPC lands, client-capped
until then).

**The page** (centre) is the real file, scrollable, syntax-lit, opened at the hop's
line — `ReadSource FILE` + `FileOverlay`, both already on the wire and already called
by the Inspector; this moves them from the rail to the centre. The hop's line is
marked; every line the graph knows gets a **gutter mark** (solid = verified, dashed =
approx), expanding on hover/click to the edge's story ("send IdentifiedCommand —
handled in Ordering.API [verified]") with *advance the spine* as the primary action.
The coverage line under the page states the overlay truth the proto already carries:
"14 call sites in view · 11 bound" — unmapped call sites simply have no mark, and the
count says so (v1's honesty guard, kept). Selecting any span offers **clip**; margin
affordance offers **note**. Reading code *with the wiring overlaid* is the one thing
neither grep nor the IDE can do — it is the product's capability made visible, and it
is where "browse the code clearly, flow by flow" is literally satisfied.

**The rail** (right) answers questions about the selection, every row a door with
provenance: who calls this (in-edges — exists), tests reaching it (`FindTestsFor` —
exists, unwrapped), config read here (`ConfigLookup` — exists, unwrapped), what breaks
below (`GetImpact` — wrapped, no caller; **no withhold needed post-E1**, grouped by
service with hops + truth tier), path A→B (`GetSeam`, as an omnibox verb), insights
touching this node, and the Collection strip (pins/clips/notes so far, "→ Context").
The rail replaces the Inspector *and* the NodeCard modal — one inspection surface.

**Keyboard:** today's discipline carries over (`p` pin, `F` zen, `/` filter, `g`
chords); adds `c` clip, `n` note, `m` mute, `[`/`]` prev/next flow (browse flow by
flow, literally).

**Code component:** the current Prism-in-`innerHTML` block cannot carry decorations,
selection events, or virtual scrolling. Recommendation: **CodeMirror 6** (decoration +
gutter APIs are exactly this design; ~vs Monaco: an order of magnitude smaller in the
Tauri bundle, and we need no editing, no LSP). A one-day spike renders one file + one
overlay before committing (§11.5).

## 7. The noise split — how salience actually works

Three layers, cheapest first, each honest:

1. **Engine defaults ("quiet").** A hop is quiet when its callee is logging
   (`ILogger`/`Log*`/`BeginScope`), BCL string/collection ops on a non-domain receiver
   (`PadLeft`, `Substring`, `GetGenericTypeName`), pure framework glue the trace
   already annotates (`WrappedBy` pipeline), or `test_only`. This is a **render tier,
   not a graph change** — computed next to `TracePolicy` so CLI, MCP, and packs
   inherit one salience vocabulary (one kernel, three faces; an agent's pack gets the
   same fold). Quiet is a *default*, visibly foldable, never a deletion.
2. **The human's Lens.** `m` on anything: a hop occurrence, a node everywhere, a
   namespace, a seam kind, a whole flow. Mark (`★`) is the same gesture upward.
   Flow-level curation happens in the TOC: hide the 42 `[RelayCommand] …NavigateToAsync`
   UI entries in one gesture (mute by kind/namespace), and eShop's flow book drops from
   110 rows to the ~25 that tell the story. The Lens ledger (rail section) lists every
   override with an undo — the mirror of the confidence ledger: *that one is the
   engine's honesty about itself; this one is yours.*
3. **Propagation with receipts.** Everything renders through the lens — spine, TOC,
   canvas, packs. A pack built from a lensed flow excludes muted hops and says so in
   OMITTED ("muted by your lens: 6 hops — logging, formatting"); `VerifyContext` and
   the fill note are unaffected because exclusion happens at card shaping, where
   `exclude_bodies` already lives (`ContextCardSpec`). Nothing silently narrows: the
   agent line in the saved pack names the lens file it was shaped by.

Why this is the right split of labor: the engine is *good* at truth and *adequate* at
default salience; only the human knows that `PaymentProcessor` is legacy or that
logging is never the story *in this repo*. The Lens is that knowledge made durable —
and because it lives in the repo, it onboards the next teammate and the next agent
session for free. No other tool in .NET-land accumulates a team's salience map over a
code graph.

## 8. Manual context creation — reading is curating

The gestures, and where each lands:

| Gesture | Where | Becomes |
|---|---|---|
| **Pin** a hop (`p`) | spine/rail | flow-anchored card seed (exists — N1.2) |
| **Clip** a span (`c`, or select-in-page) | the page | a `clip` card: exact lines + file:line + flow context. The most literal "manual context creation" — the human says *these lines*. |
| **Note** (`n`) | any anchor (hop, clip, flow) | a `note` card: human text, anchored, clearly marked **yours — unverified** in pack + verification ledger. |
| **Name / star / hide** a flow | TOC | Lens; names ship in packs ("Flow: *Place an order* — POST /api/orders/") and in the exported dossier |
| **Mute / mark** (`m`/`★`) | anywhere | Lens; §7 |

The Desk (Studio) then does what it already does since N3 — propose, price, verify,
save — but its proposal input order becomes: *handoff > collection (pins+clips+notes) >
lens-starred flows > archetype preset*. The two empty panes of D-G are now three steps
removed: after any reading session, Studio opens holding your actual reading.

**The honesty tension, addressed:** STUDIO-MCP §6 ruled out free-text blocks because
they dissolve the verification story. Notes are narrower: anchored to a provenance the
engine *can* verify, rendered as visibly human-authored, excluded from verified/approx
counters, listed separately in the ledger ("2 notes — yours, not machine-checked").
The pack stays honest by *labeling* the human's voice instead of banning it. An
onboarding pack with zero human voice is also dishonest — about what onboarding needs.

**The exported flow book.** Named, starred, noted flows + the dossier builder = an
onboarding document the repo has never had: generated facts, human curation, receipts
on every claim. This replaces Atlas's reason to exist as a page (P12's answer).

## 9. What this asks of the engine (all small, none blocking a start)

1. **Server-side re-root** — trace from an arbitrary node (v1 shopping-list item 5, the
   only survivor; today's client re-root is depth-capped, `trace.store.ts:125-134`).
2. **Salience tier** — `TraceNode.salience` (`signal|quiet`) + one classifier next to
   `TracePolicy`. (Reading Room v1 can ship with a client-side classifier; move it
   engine-side when packs adopt the fold, so all faces agree.)
3. **Lens plumbing** — read/write `.devcontext/lens.json` (the `SavePackFile` pattern
   generalized); `GetContextPack` accepts exclusions from cards (mechanism =
   `exclude_bodies`'s sibling, `exclude_nodes`).
4. **`clip`/`note` card types** — `ContextPackBuilder` additions; clip = `ReadSource`
   span + provenance; note = passthrough with the unverified marker.
5. **Wire the four unreached RPCs into the facade** — `FindTestsFor`, `ConfigLookup`,
   `GetSeam`, plus a first `GetImpact` caller. Zero engine work; the rail is their
   surface.
6. **File the front-door bug** — analyzing an empty directory silently analyzes an
   ancestor's solution and renders the full ready-state chrome (re-drive finding 1;
   discriminating fixture: empty temp dir inside any git repo).

## 10. Sequencing — five stages, each shippable, none a menu

The order is dependency-driven; the identity change ships **first**, not last — that is
the lesson of v1.

| Stage | Ships | Acceptance (a first-session user, unaided, on eShop) |
|---|---|---|
| **R1 — the Reading Room exists** | Spine │ page │ rail layout; deck → TOC/overlay; code pane = FILE read + hop highlight + gutter marks from FileOverlay; NodeCard modal deleted (A-1); salient-soup replaced by one-line hops. CodeMirror spike precedes. | P2: "where is this route handled?" answered *with the code on screen*, no IDE trip. The `//mask the credit card number` comment is *seen* inside the app. |
| **R2 — the noise split, default tier** | Client-side quiet classifier; fold idiom in spine + TOC; `[`/`]` flow browsing; quiet folds in flow cards' pack text. | The `POST /api/orders/` spine reads as ~9 story hops + one "6 quiet hops" fold; the pack's flow card drops the logging noise and says so. |
| **R3 — the Lens** | `m`/`★`, flow name/star/hide, lens ledger rail section, `.devcontext/lens.json`, TOC through the lens, packs name lens exclusions. | Muting UI-navigation entries takes one gesture; the flow book drops to the ~25 flows that matter; a teammate pulling the repo inherits it. |
| **R4 — the rail answers** | tests-for, config, who-calls, impact (no withhold), `GetSeam` omnibox verb ("path A → B"), insights anchored on nodes. | P3: "what breaks if I change `IdentifiedCommandHandler`?" answered on screen, grouped by service, with truth tiers. P13: "which tests cover this?" answered. |
| **R5 — IA consolidation + curation depth** | Clips + notes (+ card types); Atlas route → Home Export action; Insights route → Home triage (after D-F); dead-code deletion; Home TOC wears lens names. | The nav is five items; an onboarding pack containing a named flow, a clip, and a note round-trips to an agent via the existing save/agent-line path. |

Battery/gate discipline unchanged; each stage carries its evidence run under
`eval-results/` and the loom-guard sweep. Engine items 1–4 (§9) slot alongside R1–R3
without blocking them.

## 11. Decision agenda (each with a recommendation — decide, then R1 starts)

1. **Is the Reading Room the app?** — i.e., `/explore`, `/atlas`, `/insights` collapse
   into Read + Home per §5. *Recommend: yes.* The half-measure (Reader as a better
   Explore) is what v1 proposed and what the re-drive falsified.
2. **Home: keep as cover page, or fold into Read's repo state?** *Recommend: keep* —
   the 3-state front door and the run console are doing real work; one orientation
   surface, one working surface.
3. **Atlas: export action, or keep the route as "dossier preview"?** *Recommend:
   export action.* Its live sections relocate to the rail/TOC; the builder already
   exists.
4. **Insights: fold when D-F dedup lands, or fold now?** *Recommend: fold with D-F* —
   folding three duplicate auth cards into a triage block just relocates the bug.
5. **Code component: CodeMirror 6 vs Monaco vs Prism+virtual-scroll.** *Recommend:
   CodeMirror 6, after a one-day spike* (file render + decoration + gutter widget +
   selection event). Monaco only if the spike fails on something load-bearing.
6. **Lens persistence: repo file vs local store.** *Recommend: `.devcontext/lens.json`
   in the repo* (pack-file convention; gitignored by default with a visible "commit
   your lens to share it" affordance). Local-only would forfeit the team story — the
   Lens's biggest upside.
7. **Notes in packs: allow with the unverified label, or keep packs machine-only?**
   *Recommend: allow, labeled* (§8's honesty rule). This consciously amends
   STUDIO-MCP §6's "no free text" — templates/variables stay banned.
8. **GitHub-URL path: delete the dead store now?** *Recommend: delete now* (zero
   importers), keep the feature in the vision, rebuild on kernel v2. Ends the zombie
   state either way.
9. **Naming.** The route/verb is **Read**; "Explore" retires with its page. The flow
   library is the **flow book** in copy ("Browse flows"), the curation layer is the
   **Lens**, the gathering strip is the **Collection**. *Recommend: adopt* — the
   product finally speaks the developer's verbs (read, mark, clip, hand off).

---

*Companions: `DESKTOP-PRODUCT-AUDIT-2026-08-13.md` (diagnosis, superseded in §4–§7),
`STUDIO-MCP-AUDIT-2026-08-13.md` (the Desk — executed, untouched here),
`PRE-RELEASE-PLAN-2026-08-13.md` (program state this builds on), and
`eval-results/2026-08-14/desktop-redrive/REDRIVE-NOTES.md` (this session's live
evidence, incl. two candidate bug filings). Wireframe brief for the §11 decisions:
<https://claude.ai/code/artifact/4bba9dce-f07f-4254-9fb8-c26ae77086e3>.*
