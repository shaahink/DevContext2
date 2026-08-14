# Context Studio + MCP page — feature-design audit → plan

> **DECIDED 2026-08-13 — §8's four calls made by the owner (all four: the recommended
> option); ready for master-plan consolidation.** Written 2026-08-13. Written on
> `feat/agent-probe` as the second desktop iteration — the deep pass on the two surfaces the
> first desktop audit (`DESKTOP-PRODUCT-AUDIT-2026-08-13.md`) deliberately compressed: **Context
> Studio** and the **MCP page**. Method: full source inventory of both features (three subagent
> sweeps: Studio, MCP page, engine/MCP supply side), screen-reads of the shipped captures
> (`eval-results/2026-07-28/r3-current-state/*-s10/40-context.png`, `2026-07-27/ui-feature-audit/
> evidence/eshop-studio-composed.png`, `2026-07-29/G7/g72-withhold-sweep-after-eshop-mcp.png`),
> and the doc trail (DEEP-EVAL W1/W3, BUG-BACKLOG #4/#5, DECISIONS D-G, PRODUCT-DIRECTION §5/§7,
> mcp-reference.md). Code claims marked **[audit 2026-08-13]** were swept today — re-verify
> file:line before fixing (house rule). This is not released software; the plan below is deep
> surgery, not iteration.

> **STATUS PASS 2026-08-14 (pre-release run B, stage Z1.1).** The plan in §5 was executed as
> conductor run `8faf849d` on `feat/pre-release-desktop`: **N0, N1, N2, N3 and N4 are all closed**
> (14 checkpoints), N5 stays deferred by the §5 default. Every §3.F item below and every move in
> §5 now carries its outcome and the commit that settles it — read those markers, not the prose
> around them, for current state. The three §3.F items that were still open when the backlog was
> written (8, 15, and the tool-list half of 12) closed in N2.1 and N4.3. Statuses were
> re-measured from the code on 2026-08-14, not copied from the sessions' own claims.

---

## Verdict in three sentences

These two pages are the desktop's half of the agent story — Studio makes context *for* agents,
the MCP page connects and watches them — and both are currently **monuments rather than
bridges**: Studio is a genuinely well-built pack assembler with no working inputs (the
pins-seed-the-pack loop is advertised on three surfaces and implemented on none; not one button
anywhere in the app sends anything *to* Studio) and no real output (clipboard/file only — no
hand-off an agent can consume), while the MCP page performs statuses it does not measure (its
"status check" is a mutating `StartMcp` call, its Stop button mutes a telemetry stream, its
setup snippets point at a binary the installer never ships) and speaks a different language
from the agents it claims to observe (gRPC method names in the feed, a hand-kept 8-of-22 tool
list, a try-a-tool that never touches the MCP surface). The signature defect class of the whole
program — **a confident surface over absent substance** — is denser on these two pages than
anywhere else in the product, which matters doubly because they are the exact surfaces W1/W3
(the trust pack and the re-probe) will stand on. The cut-through is to rebuild them as **one
agent loop with two rooms** — Studio as the hand-off desk (explore → compose → hand to agent),
the MCP page as the observation deck (what the agent asked, in the agent's vocabulary, with
setup that actually works) — and to converge the human pack and the agent pack onto the one
pipeline the kernel already almost has.

---

## 1. What the two surfaces are today (inventory, compressed)

Full inventories live in the three agent sweeps; product-eye summary here.

### Context Studio (`/context`)

Three panes: scope picker (entries grouped by service) │ composition (cards, drag-reorder,
per-card copy) │ budget rail (slider 1k–16k, per-card meters, intent, format, verification
ledger, Copy/Save). The loop: select entries → "Add N to context" seeds 9 cards (one per card
type) → debounced `GetContextPack` → live preview + omitted ledger + auto-verify → Copy/Save.
The machinery praise from 07-27 stands: server-priced meters, an honest OMITTED list, per-file
staleness verification with a Re-analyze escape.

What the inventory adds [audit 2026-08-13]:

- **Ways in: three, all chrome.** Rail icon, `c`/`g c`, `Ctrl+E` from Workbench. A grep for
  `'/context'` across the app returns only chrome registrations. **No content surface routes
  to Studio** — not Explore, not Insights cards, not NodeCard, not the MCP page, not even the
  global omnibox's command list.
- **Ways out: two, both inert to agents.** Copy (raw `navigator.clipboard`, bypassing the
  app's own WebView2-safe `clipboard.ts` helper, success toast fired unconditionally) and Save
  (browser blob download, same unconditional toast, no Tauri save dialog).
- **Scope: entries only.** Cards carry entry nodeIds; there is no node/type/file/project
  scoping. Selection keys on `entry.focus`, which collides for same-named handlers across
  projects (`view-models.ts:153`). Picker rows show method + 26-char ellipsized route + kind
  glyph — project, file:line, target, score all exist on `EntryVm` and are dropped (the D-G
  identity finding, confirmed on the eShop capture: ten identical `GET /api/catalog/i…` rows).
- **Zero-entry repos: a dead end with honest prose.** The C-3 fix made the withheld sentence
  true ("a library is scoped by its public surface… Pick types from the omnibox above",
  `scope-picker.ts:75-82`) — but the omnibox it points at iterates `entryGroups()` only
  (`:395-412`), so on the repos that need it the instruction is unfollowable. Studio remains
  dead for the library archetype, now politely.
- **State: none.** No store, not per-tab, not persisted; navigating away destroys the cards.
  Tab-switch swaps the session under a card list built from the other tab's nodeIds; re-analyze
  issues a new handle and **nothing invalidates or re-packs** — the stale pack, meters, and
  verification stay on screen, exportable. The verification panel's own Re-analyze button leads
  straight into this state.

### MCP page (`/mcp`)

One 465-line component, five sections: status card → host config (Claude Code / Cursor /
VS Code snippets) → sessions table (30s poll) → live feed (server stream, 200-row client
buffer, "agents only" default filter) → try-a-tool (8-tool dropdown, one free-text arg, raw
JSON result). Inbound links: Home's "Point your agent here", the statusbar server chip, the
rail. **Outbound links: zero** — no `Router`, no `<a>`, in the whole page.

---

## 2. The supply/demand ledger

Same exercise as the desktop audit §2 — what the kernel/server can answer vs what these pages
ask. [audit 2026-08-13]

| Supply (exists, tested) | Wire | Studio / MCP page today |
|---|---|---|
| `ContextPackBuilder.Build(focus)` — **symbol-rooted** packs: bare type/member names resolve via `ResolveEntry`; includes the `usage` section (inbound edges) | ✅ `GetContext` | **Never called by Studio.** Only the MCP `get_context` tool uses it — the agent gets a capability the human can't reach |
| `ContextPackBuilder.BuildMulti(cards)` — entry-rooted only (`FindEntry`), 9 card types | ✅ `GetContextPack` | Studio's one pack call. `usage` has no card type → unreachable in any pack (`ContextPackBuilder.cs:494-505`) |
| `SectionAllocation.Verified/Approx` — the trust split, per section | ✅ on wire | Mapped into the card VM, **rendered nowhere** (JSON export only). The Studio never shows the honesty numbers the engine computes |
| Per-section `SourceLocations` (capped 20) | ✅ | Provenance chips — **but** the multi-entry merge (`ContextPackBuilder.cs:575-582`) rebuilds merged sections without locations/verified/approx, so the **primary path** ("Add N to context") ships cards with no provenance and 0/0 trust counters |
| `get_context` honesty layer: fill-rate note, suggested focuses, thin-pack rejection (`DevContextTools.cs:1620-1655`) | MCP-only | **The agent's pack is more honest than the human's** — Studio renders server bytes with none of this |
| `VerifyContext` | ✅ | Called N times per repack (one per entry, full budget, intent-null) against a rebuild of **all** sections — the ledger routinely verifies sections that are not in the pack, at a budget the pack didn't use, in an order the user didn't pick |
| `Render` (sections[], detail=signature/salient/full, depth, format) | ✅ | Never called by Studio — a richer shaping surface than anything the budget rail exposes |
| MCP catalog: 22 tools, each with a written one-line XML summary | ❌ summaries never reach the wire (bug #5); ❌ no `ListTools` RPC (bug #4 root) | Page shows **no catalog at all**; try-a-tool hand-keeps 8 names and probes gRPC, not MCP |
| `SessionInfo`: `commit_sha`, `token_total`, `status`, `from_cache`, `analyzed_at` (the last two exist precisely because `age_seconds` lies after rehydrate) | ✅ | Table renders repo/handle/age/calls/nodes; `edges`/`entries` mapped-never-rendered; the honesty fields unused — the page shows the lying age |
| `ToolCallEvent`: `bytes`, `timestamp_utc_ms`, `session_handle` | ✅ | Feed renders tool/origin/tokens/ms with **client arrival-time** stamps; the rest mapped-never-rendered |
| `McpObservabilityService.ObserverCount` — how many agents are attached | ❌ not on wire | The one number that would make "MCP Endpoint Active" mean something |
| Tool-call history / per-tool aggregates | ❌ nothing stored server-side (pub/sub only; `StopMcp` drops events globally) | 200-row in-memory buffer, gone on navigation |
| MCP process logs (`%LOCALAPPDATA%\DevContext\logs\mcp-*.log`) | n/a | Never surfaced — `mcp-reference.md:23` claims the page shows "a log feed"; it shows the gRPC telemetry stream instead |

Reading: **the same under-consumption diagnosis as the desktop audit, plus a new one — the two
consumers of the pack kernel diverged.** `Build` and `BuildMulti` share `BuildSections` but
differ in resolver (symbol-rooted vs entries-only), section set (`usage`), budget semantics
(whole-budget vs reach-proportional), envelope, and honesty post-processing. "One kernel, three
faces" is broken *inside a single feature*: the human face and the agent face of the very same
artifact — the context pack — take different code paths and tell different truths.

---

## 3. Product diagnosis — six structural findings

### A. The advertised loop does not exist

The product's story for Studio — told by the inspector ("Pin to export pack (p)"), the trail
bar ("Pinned steps seed the export pack"), the ticker ("Press p to pin a trail step into your
export pack"), and the 07-27 audit's own verdict ("its best loop: trail→pins→pack") — is that
exploration accumulates into a pack. **`TrailStore.pins()` has zero readers outside the store
and its spec** [audit 2026-08-13]. The "From current trail" button reads raw `steps()`, keeps
entry-kind steps only, and silently no-ops when there are none. Pinning changes nothing about
any pack, ever. This is the program's defect class at its purest: three surfaces confidently
describing a mechanism that was never wired — and it explains D-G's "opens on two empty panes"
more deeply than an empty-state complaint: Studio opens empty because *nothing in the app can
fill it except the picker inside it*.

### B. Two pack pipelines, one product claim — and the human gets the lesser one

§2's ledger, stated as product: the agent asking `get_context` gets symbol-rooted resolution,
the `usage` section, a fill-rate honesty note, and suggested focuses; the human in Studio gets
entries-only scope, no usage card, no fill note, invisible verified/approx, and — on the
primary multi-entry path — a server merge that silently strips provenance and trust counters.
Meanwhile the verification ledger checks a *different pack* than the one on screen (all
sections, full budget, trace order). None of these are design decisions; they are drift. The
pack is the product's most differentiated artifact and it has two half-truths instead of one
truth.

### C. Entries-only scope in a symbol-rooted product

D-G's open decision ("type-rooted packs UI") understates what the inventory shows: the kernel
*already* builds symbol-rooted packs — `Build` resolves bare type/member names today, and
`usage` (who calls this) only exists on that path. Studio's entries-only scope is not a missing
feature, it is a refusal to call the better half of its own builder. The consequences chain:
libraries dead (the C-3 sentence is honest but the omnibox it points to cannot answer), the
five `OrderStatusChangedTo*EventHandler`s indistinguishable, "Change-impact pack" a preset name
with no impact semantics. Fixing the scope model *is* the D-G decision, and it is a convergence
(§4), not new machinery.

### D. The MCP page performs status instead of measuring it

The chain, end to end [audit 2026-08-13]: `getMcpStatus()` **calls `StartMcp`** — a mutating
RPC — so the dot is green whenever the gRPC server is reachable; Start/Stop flips a volatile
bool that **mutes the telemetry stream for every observer** and never touches the actual
`devcontext-mcp` process (which the agent host spawns over stdio, outside the app's control);
a dead server and a "stopped" endpoint render pixel-identically; the header copy "ships with
the desktop installer" is **false** (the Tauri bundle publishes only `resources/server/**`;
`devcontext-mcp` is never published or put on PATH — `mcp-reference.md:37-38` says so
directly); all three host snippets therefore point at a command that does not resolve on a
clean install; and the Copy feedback lands on the wrong card (the label-sniffing `copy()`
matches none of the snippets, so every copy flips the VS Code card to "Copied!"). A first-run
user following this page precisely ends with a config that cannot work and a green light that
told them nothing.

### E. The page and the agents speak different languages

The feed prints **gRPC method names** (`[CallerMemberName]`), not MCP tool names: an agent's
one `overview` call renders as four rows (`GetGraphFacets`/`GetMap`/`GetStats`/
`GetInterestingPoints`); `analyze` — the agent's first, slowest, most important call — is
unwrapped and **never appears at all**; `find` shows as `SearchNodes`. The "Total: N tok"
counter keeps counting rows the filter hides. Try-a-tool probes gRPC directly with different
defaults than the MCP tools (budget 4000 vs 8000, no intent; unbounded `entrypoints` vs the
tool's top-15; blank-arg `trace` traces the session handle *string*), so it cannot reproduce
what an agent experiences — the envelopes, hints, and resolvers that W1 is hardening are
exactly the layer it bypasses. And the page offers no tool catalog at all, while the server's
22 summaries exist as compile-time XML that reaches neither agents (bug #5) nor this page
(bug #4, no `ListTools`). For an agent the tool list *is* the UX; for the human watching the
agent, this page is the UX — and today they see different worlds.

### F. The truth-defect inventory (file as bugs)

New confident-surface instances found today, each small, each the S8/G-series class. **All
sixteen are now closed** — the marker after each is the checkpoint and commit that settles it
(re-measured 2026-08-14, Z1.1); the backlog number is the id under which the item was filed in
`BUG-BACKLOG.md`:

1. Pins advertised, never read (§A) — or the flag finding of this audit.
   **FIXED N1.2 · `e448d64` (#26)** — `TrailStore.pins()` gained its reader; pins beat the raw trail.
2. Body toggles ("Hide code bodies", "All bodies shown") are **cosmetic** — never sent on the
   wire, never filter preview/copy/save; the eye icon and opacity are the entire feature.
   **FIXED N1.1 · `56ebc25`+`e3a9bc2` (#27)** — WIRED, not deleted: `ContextCardSpec.exclude_bodies`.
3. Multi-entry section merge drops `SourceLocations`/`Verified`/`Approx`
   (`ContextPackBuilder.cs:575-582`) — primary-path cards lose provenance.
   **FIXED N0.1 · `36bf916`**, and the same merge's `Joined` counter fixed after the engine merge
   in `f7e382b` — the trust counters are summed, not dropped.
4. `allocated_tokens` ≡ budget (`:647`) — the preview header prints one number under two labels.
   **FIXED N0.1 · `36bf916`** — allocated is measured, not echoed.
5. Verification ledger verifies a pack that was never built (§B); `checkedAt` stored, never
   rendered; N RPCs per repack.
   **FIXED N1.1 · `56ebc25` (#28)** — verification moved INTO `GetContextPack`'s response.
6. Studio cards survive tab-switch and re-analyze with dead handles/nodeIds; no invalidation.
   **FIXED N1.1 · `e3a9bc2` (#29)** — handle-effect invalidation; shaping persisted as a preference.
7. Studio copy paths bypass `clipboard.ts`; Copy/Save toasts fire before/regardless of outcome.
   **FIXED N0.1 · `36bf916`** — one clipboard helper, toasts await the outcome.
8. Zero-entry empty states instruct an omnibox that cannot comply (C-3 half-fix).
   **FIXED N2.1 · `8c38e0b` (#30)** — the empty state now names the Types tab, which exists and
   carries the public surface (`scope-picker.ts` archetype notes).
9. `getMcpStatus` = `StartMcp` (mutating status); Stop = global telemetry mute mislabeled.
   **FIXED N0.2 · `98c5067`**, deepened in N4.1 · `7c6b6be` (binary probe + observer count +
   handshake) and `7f78edc` (the ui/agent origin tag was wrong, so no agent call was ever counted).
10. "ships with the desktop installer" false; snippets point at non-resolving command.
    **FIXED N0.2 · `98c5067`** (claim withdrawn) then **made TRUE in N4.2 · `d48a122`** —
    `devcontext-mcp` ships in the Tauri bundle and the snippets carry the resolved absolute path.
11. Copy-label sniffing always marks VS Code copied.
    **FIXED N0.2 · `98c5067`** — per-host `copy-snippet-<host>` state.
12. Feed: gRPC names, `analyze` invisible, Total counts hidden rows, client-side timestamps.
    **FIXED N0.2 · `98c5067`** (totals follow the filter, wire timestamps) + **N4.3 · `fbb929b`**
    (the feed speaks MCP tool names and `analyze` is no longer invisible).
13. Sessions: `edges`/`entries` mapped-unrendered; `from_cache`/`analyzed_at` unused so the
    shown age lies after rehydrate (the wire comment says exactly this).
    **FIXED N0.2 · `98c5067`** — both columns render; a "(cached)" analysis-age cell explains the rehydrate.
14. Dead state: `mcpStateSynced`, `DevContextApi._mcpRunning` (no consumers), feed `session`/
    `bytes` fields; three silent catch-alls (poll, stream, status) with no user-visible signal.
    **FIXED N0.2 · `98c5067`** — dead fields deleted, the three failures surface.
15. `usage` section unreachable from any card type; `"client-only type"` omission branch
    unreachable; docs claim a log feed the page doesn't have.
    **FIXED N2.1 · `104c9d0` (#31)** — `CardTypeSections["usage"]` exists, so the section the
    builder has produced since G1.2 is reachable from the Studio. The `"client-only type"`
    omission branch survives as a **wire-facing guard**: no app card type can reach it, but
    `ContextCardSpec.type` is a free string on the wire, so any gRPC/MCP client can. The
    log-feed claim is gone from `docs/product/mcp-reference.md` (it now says the rolling MCP
    logs have no surface tailing them, and describes the tool-call feed that does exist).
16. Neither page has a spec file or any e2e coverage; the three `data-testid`s are unreferenced.
    **FIXED N0.2 + N0.3** (`mcp-page.spec.ts`, `context-studio.spec.ts`) and extended by every
    stage since — `scope-picker.spec.ts`, `workbench-page.spec.ts`, `pack-proposal.spec.ts`.

---

## 4. The cut-through concept — one agent loop, two rooms

> Companion to the desktop audit's Reader (M1): the Reader is where the *human* reads the
> codebase; these two pages are where the human **equips and supervises the agent**. Same
> product, second seat. Everything below stays inside the five-artifact contract — packs are
> Trace/Browse/Stats recombined; nothing new is invented.

**The thesis.** DevContext's agent story (deep-eval §3) is: a *primer* today, an accelerator
once W2 earns it. The desktop's job in that story is the loop no CLI can offer: **watch what
the agent asks → see where its context was thin → compose better context → hand it off → watch
again.** Studio and the MCP page are the two rooms of that loop, and every joint between them
is currently severed (pins→pack dead, nothing routes in, clipboard-only out, feed rows inert,
`get_context` ≠ Studio pack).

**Room 1 — Studio, the hand-off desk.**

- **Never opens empty after exploration.** Default state = the current trail (and pins, once
  real) rendered as a *proposed* pack — the D-G empty panes become a suggestion the user edits.
  On a fresh session: the archetype preset (app → top flows; library → top public types once
  scope converges) as the proposal.
- **Scope = anything the graph can resolve.** Cards carry focus strings, resolved by the same
  `ResolveEntry` path `get_context` uses (§5 N2). Entries, types, members; the picker gains a
  second tab ("Types" — the LibrarySurface list) and the D-G row identity fix (target member +
  route tail + project). Libraries come alive; `usage` becomes a card.
- **Truth on every card.** Verified/approx chips per section (the wire already carries them);
  merge bug fixed so provenance survives the primary path; the fill-rate note and suggested
  focuses from the MCP path rendered in the rail; verification checks the pack actually built.
- **A real hand-off, not a clipboard.** Save gains a repo-file convention —
  `.devcontext/packs/<slug>.md` (gitignored by default, owner call) — plus a "point your agent
  here" affordance: copy a one-line instruction for CLAUDE.md/agent prompt referencing the pack
  file, or (stronger, decision Q3) register the pack on the server session so `get_context`
  serves the human-composed pack to the agent by name. That last step closes the loop
  literally: the human's composition becomes the agent's context, one artifact, two faces.

**Room 2 — the MCP page, the observation deck.**

- **Status that measures.** Three real checks replace the theater: binary present (Tauri fs
  probe of the shipped/`PATH` location), agents attached (`ObserverCount` → wire), last agent
  call age. Start/Stop either controls something real or is renamed to what it is (feed mute)
  — recommendation: kill it; a mute that drops events globally for all observers is a trap.
- **Setup that works.** Ship `devcontext-mcp` in the bundle (or as a `dotnet tool` — owner
  call, OWNER-TODO §1 adjacency), snippets carrying the **resolved absolute path**, a
  "write config for me" button per host, and a handshake test that runs one real MCP
  `tools/list` round-trip and shows the result.
- **The catalog, served — one source of truth.** A `ListTools` RPC (or the page reading the
  MCP process directly) renders the same curated, described menu agents get after W1 —
  names, descriptions, the retired-alias table. Bug #4 dies structurally; the page becomes
  the human mirror of `tools/list`, which is exactly what a supervisor needs to see.
- **The feed in the agent's vocabulary.** Rows keyed by MCP tool name (wrap at the tool layer
  or map server-side), `analyze` included, args summary restored (revive `args_digest` or a
  truncated args string — decision), wire timestamps, totals per visible filter. Rows become
  doors: a `trace` row deep-links into Explore at that focus; a `get_context` row opens
  **"replay in Studio"** — the pack the agent got, loaded as cards, ready to improve (M5's
  idea, now concrete). That single affordance is the loop's return joint: watch → improve →
  hand off.

**What it needs from the server/wire (the shopping list — all small):**
1. `ListTools` RPC (name, description, params digest, curated/retired flag) — or Ping gains it.
2. `ObserverCount` + last-agent-call-at on the wire (extend `McpStatusResponse`).
3. Feed events carry the MCP tool name + args summary (tool-layer wrap), `analyze` wrapped.
4. `VerifyContextRequest` gains cards/intent (verify what was built) — or verification moves
   server-side into `GetContextPack`'s response.
5. `BuildMulti` → `ResolveEntry` (symbol-rooted cards) + `usage` in `CardTypeSections` + the
   merge fix (§3.F.3) + fill-note parity.
6. Pack registration on the session (Q3, if chosen): `SavePack(handle, name, cards)` +
   `get_context(focus: "pack:<name>")`.

---

## 5. The moves (ranked; N0 first, then by decision-dependency)

> **Outcome 2026-08-14 (Z1.1).** N0–N4 all closed in run `8faf849d`; N5 deferred by its own
> default. Per-move status markers are at the end of each paragraph.

**N0 — the truth batch (no decisions, mostly small, do first).** §3.F items 3,4,7,9,10,11,12,
13,14,15 minus the ones needing a design call: merge-bug fix, allocated line, clipboard helper
+ awaited toasts, honest status semantics (split status from StartMcp even before the full
Room-2 work), snippet paths + copy-label fix, feed totals/timestamps, sessions honesty fields,
dead-state deletion, spec smoke coverage for both pages. Each would have been red under the
wire-truth gate W1.4 proposes; several *are* W1.4 items on the desktop side.
**DONE — N0.1 `36bf916` · N0.2 `98c5067` · N0.3 (specs).** Ten items closed here; the six that
needed a decision or a bigger surface went to N1/N2/N4 and are closed there.

**N1 — Studio truth pass (one decision: pins).** Render verified/approx; align verification
with the built pack (wire item 4); state lifecycle (per-tab key or handle-effect invalidation
+ persist budget/intent/format in prefs); body toggles wired to the wire or deleted; pins made
real (read by trail-seed, "p" adds to pack when Studio is the context) **or** the pin idiom
deleted from all three advertising surfaces — Q1.
**DONE — N1.1 `56ebc25`+`e3a9bc2` · N1.2 `e448d64`+`366cc3a`.** Q1 answered IMPLEMENT (§8.1);
verification moved into the pack response rather than the verify RPC growing fields; body toggles
wired rather than deleted.

**N2 — pack convergence (the D-G decision, upgraded).** `BuildMulti` adopts `ResolveEntry`;
`usage` card; type/member scope in the picker (second tab + row identity); honesty-note parity
with `get_context`; budget default reconciled (UI 4000 vs everywhere-else 8000 — pick one,
state it). Acceptance: a FluentValidation pack composed from types, with usage and verified
counts, end to end. This is the item that makes Studio *the* human twin of `get_context`
before the re-probe measures the agent twin.
**DONE — N2.1 `104c9d0`+`8c38e0b` · N2.2 `e769246`.** The acceptance was met on a real clone
(`eval-repos/FluentValidation` @ `94397908`): a type-composed pack with usage and verified counts,
recorded in `eval-results/2026-08-13/N2.2-honesty-parity.md`. The budget number was reconciled to
one stated value — `ContextPackBuilder.DefaultBudgetTokens = 8000` for a PACK; `TracePolicy`'s
4000 stays, and says out loud that it budgets a single trace. **This settles S11's D-G**
(`DECISIONS.md` §D-G).

**N3 — the loop joints.** Send-to-Studio from Explore (selection/trail/pins), Insights cards,
NodeCard; Studio proposed-pack default state; Home's "Point your agent here" routes through
Studio's hand-off (M5's reframe); MCP feed "replay in Studio". Ships after N1/N2 (needs real
pins + scope).
**DONE — N3.1 `f427027` · N3.2 `6efcef6`+`032c9b8`.** Hand-off decision 3 (REPO FILE FIRST)
shipped as the `SavePackFile` RPC writing `.devcontext/packs/<slug>.md` plus the copyable
point-your-agent-here line; the MCP feed's "replay in Studio" half landed later, in N4.3.

**N4 — MCP page rebuild (Room 2).** Status that measures, setup that works (including the
ship-the-binary decision — Q4), served catalog (after W1's curation so the menu is the curated
one), feed vocabulary + deep links. The page's redesign is deliberately *behind* W1: it should
render W1's curated, described catalog, not memorialize the pre-W1 soup.
**DONE — N4.1 `7c6b6be`+`7f78edc` · N4.2 `d48a122`+`7486a73` · N4.3 `6c2501e`+`fbb929b`+`a4896f2`+`c41f489`.**
The W1 dependency was honoured literally: the engine run's T1 catalog was merged onto this branch
(`153c99f`) before the page rendered it. `ListMcpTools` serves the curated menu (14 advertised, 8
specialists, measured live), so bug #4 dies structurally; the feed carries `primary_arg` and its
rows deep-link — trace→Explore, get_context→replay-in-Studio — proven on a real sidecar 10/10 by
`eval/mcp-qa/deep-link-truth.js`, now battery step 2c.

**N5 — optional cockpit depth (decide scope, default: defer).** Server-side call history +
per-tool aggregates (p50/p95, calls/tool), args capture, session origin split. Real value for
the W3 study era; also real scope creep. Deferred unless the owner wants the desktop to be the
probe's dashboard.
**DEFERRED as written** — recorded in `PRE-RELEASE-PLAN-2026-08-13.md` §7's deferred register.
N4.1's observer count and last-agent-call-at give the page the two liveness facts it lacked;
per-tool aggregates and args capture remain unbuilt.

---

## 6. What NOT to do

- **No agent runner in the app.** The desktop equips and observes agents; it does not host
  them. The moment we embed a chat loop we compete with the hosts we integrate with.
- **No prompt-engineering IDE.** Studio composes *graph-derived* context with receipts; free-
  text blocks, templates, and variable interpolation are out of scope (and would dissolve the
  verification story).
- **Don't render the catalog before W1 curates it.** Mirroring 22 undescribed tools onto the
  page reproduces catalog soup for humans. The page ships the curated menu or none.
- **No fake liveness.** Every status light on the MCP page must trace to a measurement
  (process, observer count, handshake). Anything else repeats §3.D.
- **Don't grow the pack format.** The hand-off is markdown with provenance — the format agents
  and humans already share. No bespoke pack DSL.

---

## 7. Sequencing against the standing program

- **N0 lands with W1** — same trust-pack spirit, desktop side; several items are literally the
  wire-truth gate applied to these pages. Nothing blocks the $10 adoption gate.
- **N2 before/with W3's re-probe** if Q2 chooses convergence: the re-probe's `get_context`
  and the Studio should exercise one pipeline so the measurement covers both faces.
- **N4 strictly after W1's curation** (menu + descriptions are its raw material). The
  `ListTools` RPC is shared infrastructure: it also kills bug #4 for good.
- **S11's D-G is absorbed** by N1+N2 — and as of N2.2 it is SETTLED, not merely absorbed;
  `DECISIONS.md` §D-G records the decision and its commits. D-F (insights dedup) stays where it is (M6). The
  desktop audit's M5 is superseded by §4/N3 of this document.
- **Master plan:** this is the fourth input (agent face · kernel · desktop product · this).
  Consolidation happens after the owner's assessment pass, per the standing instruction.

---

## 8. Owner decisions — DECIDED 2026-08-13, ALL FOUR SHIPPED 2026-08-14

1. **Pins: IMPLEMENT.** Pins seed the pack for real; `p` works from Explore; Studio's default
   state becomes the proposed pack. (N1 carries the wiring, N3 the affordances.)
2. **Pack convergence: FULL.** `BuildMulti` adopts `ResolveEntry`; type/member-scoped cards;
   `usage` card; honesty-note parity. This settles S11's D-G. Acceptance: a FluentValidation
   pack composed from types, with usage and verified counts, end to end.
3. **Hand-off: REPO FILE FIRST.** Save writes `.devcontext/packs/<slug>.md` + a copyable
   "point your agent here" line for CLAUDE.md. Server-registered packs (`SavePack` +
   `get_context(pack:<name>)`) staged second, after W1's curation freeze.
4. **MCP page: FULL DECK + SHIP IN INSTALLER.** Real status (binary probe, observer count,
   handshake), write-config buttons, server-served catalog via `ListTools` after W1 curation,
   feed in MCP vocabulary with replay-in-Studio. `devcontext-mcp` ships in the Tauri bundle —
   the page's existing claim becomes true instead of false.

---

*Companion docs: `DEEP-EVAL-2026-08-13.md` (agent face), `GRAPH-DETECTION-AUDIT-2026-08-13.md`
(kernel), `DESKTOP-PRODUCT-AUDIT-2026-08-13.md` (desktop product, first pass). The owner pass
happened on 2026-08-13 (§8), so this is no longer a draft; its §3.F items were filed into
`BUG-BACKLOG.md` by N0.3 and every one of them is now closed — see the status markers above and
the FIXED-in-N* sections at the foot of the backlog. **This document is now a record, not a
plan**: the only unexecuted move in it is N5, deferred on purpose.*
