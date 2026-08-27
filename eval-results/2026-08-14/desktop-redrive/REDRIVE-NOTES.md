# Desktop re-drive — 2026-08-14 (proposal-v2 session)

Setup: `develop` @ `170d304`, dev servers via `start-dev-bg.ps1` (web :4200, server :5179),
driven in a real browser at 1600×950. Repo: `eval-repos/eShop` @ `9b4f943` (submodule
initialized during the session — see finding 1). Captures in this folder:

| File | State |
|---|---|
| `01-home.png` | Home ready-state digest, eShop (110 entries · 73/110 wired · 34% verified) |
| `02-explore-trace.png` | Explore, `POST /api/orders/` focused, tree view |
| `03-inspector-code.png` | Inspector Code section, `OrdersApi.CreateOrderAsync` (MEMBER mode, lines 118–168 of 185) |
| `04-studio.png` | Context Studio default state (proposal from top 3 flows, 3 cards, verification fresh) |
| `05-insights.png` | Insights page (the three overlapping auth cards still ship — D-F open) |
| `06-atlas.png` | Atlas (stores drawn, AppHost frame, honest caption, export buttons) |

## Findings (observed live; re-verify before filing/fixing, house rule)

1. **Analyzing an empty directory silently analyzes an ancestor's solution.** With the
   `eval-repos/eShop` submodule uninitialized (empty dir), Analyze on that path produced a
   session labelled `DevContext.slnx` at HEAD `170d304` — the PARENT repo's solution and
   commit — rendered as `app · 0 entries · 0 edges · Current`, with START HERE links, no
   error, no "this folder is empty", in 3.6s. The tab still carried the eShop path as its
   label description. Two defects in one: solution discovery escapes the analyze root
   upward, and a 0-node result renders the full confident ready-state chrome. The
   signature defect class, live on the front door. (Not yet filed in BUG-BACKLOG —
   worth a filing with a discriminating fixture: analyze an empty temp dir nested in any
   git repo.)

2. **The trace tail ranks noise as flow steps.** `POST /api/orders/` renders
   `OrderServices.LogInformation` / `LogWarning` / `BeginScope`,
   `CreateOrderRequest.PadLeft` / `.Substring` / `.GetGenericTypeName` as sibling hops of
   the real dispatch — while the actual `mediator.Send` continuation is one `approx` leaf
   (`OrderServices.Send`) at the bottom. Within-seam salience does not exist; the seam
   ranking (Sends > … > Calls) is the only ordering. This is the sharpest live evidence
   for the noise-split requirement.

3. **Salient text is redundant where it is worst.** Each hop carries 1–3 flattened source
   lines, including raw `/// <summary>` and — for `OrderingContext` — a `/// <remarks>`
   about EF migrations as its story line. Once the Code section is open, the same
   information appears twice: crushed in the tree AND highlighted in the pane. The tree
   needs a designed one-liner; the body belongs to the code pane.

4. **The NodeCard modal is still alive** (decision A-1 recorded "dock; kill the modal",
   deferred in S8, never landed): clicking a node NAME in the tree opens a blocking modal
   with raw absolute path text; clicking the ROW selects into the docked Inspector. Two
   inspection surfaces, one click apart.

5. **Code, at its best, is a peephole with receipts.** The Inspector Code section
   auto-loads MEMBER mode with Prism highlighting, `copy path · load source · whole file`,
   an honest range note, and (after whole-file) the FileOverlay wiring-site list. All of
   M1's Reader prerequisites are live — but rendered inside a `max-h-80` box in a 40% rail,
   and the overlay is a LIST below the code, not a gutter on it.

6. **Pin → Studio proposal did not visibly connect (observed once, NOT verified).**
   Pressed `p` with `OrdersApi.CreateOrderAsync` selected, then opened Studio: the banner
   read "Proposed from this repo's top 3 flows", not from the pin. `pack-proposal.ts`
   orders handoff > pins > trail > preset, so either the keypress never registered as a
   pin (focus target?) or the proposal ignored it. Needs a deliberate re-measure before
   filing anything.

7. **Studio flow cards carry the trace's noise into the pack.** The `/api/orders/` card's
   preview embeds the same tree text (`[Call] ... [approx] (truncated, N omitted)`)
   including the logging/formatting hops from finding 2. Curation today = card add/remove,
   body eye-toggle, budget; there is no gesture to exclude a HOP or mark a span. The
   omitted ledger is honest about budget cuts, but the human cannot say "this is noise."

8. **What is genuinely good and must survive any redesign**: the identity strip +
   confidence ledger + solution-scope row; the CrossService collapse row (8 services
   named, 15 hops, 37 omitted — honest and compact); verified/approx chips everywhere;
   Studio's verification fingerprint + fill note + omitted ledger + save-to-repo with the
   agent line; the honest Atlas caption ("12 services (9 drawn · 1 orchestrator · 2 in no
   relationship)"); tabs/omnibox/keyboard discipline; the withheld idiom.
