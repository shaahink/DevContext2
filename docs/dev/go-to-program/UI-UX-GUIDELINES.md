# UI/UX Guidelines — the executing agent's design contract

> Authored 2026-07-02 (pass 3) · Audited against `feat/narrative-canvas` @ `9d504c4`.
> Purpose: an agent picking up I4/I5/I8/I9 UI work should make **no layout/interaction decisions from
> scratch** — the decisions are here. Where something is genuinely open, it's marked OPEN with a default.
> **Supersedes** FACES-DESIGN §2.3's "keep the narrative canvas as the spine / no route proliferation"
> line — the user's direction (2026-07-02): *stop the one-scroll-page vibe*.

---

## 1. Navigation model — workspace, not scroll

The app moves from one scrolling narrative to a **workspace**: left rail + routed views + persistent
context header. The Angular Router is already in place with a single wildcard route
(`app.config.ts` → `narrative-canvas`) — this is a route-table change, not an architecture change.

```
┌──────────────────────────────────────────────────────────────┐
│ HEADER  ◆ DevContext   [repo label ▾ recents]   ⌘K   ● srv   │  ← context header, always visible
│         App · Microservices · scope: full · targets 34/94    │  ← honesty ribbon (when ready)
├───────┬──────────────────────────────────────────────────────┤
│ RAIL  │  ROUTED VIEW                                         │
│ Over  │                                                      │
│ Entr  │   (Overview | Entries | Trace | Graph |              │
│ Trace │    Insights | Export | Settings)                     │
│ Graph │                                                      │
│ Insig │                                                      │
│ Expo  │                                                      │
│ ⚙ Set │                                                      │
├───────┴──────────────────────────────────────────────────────┤
│ FOOTER/STATUS  progress · engine activity · version          │
└──────────────────────────────────────────────────────────────┘
```

**Route table** (all lazy, all preserving state via store — views are projections of stores, so
switching routes never refetches):

| Route | View | Content |
|---|---|---|
| `/` (no session) | Landing | analyze input (path/URL/recents), first-run hint |
| `/overview` | Overview | identity + top-3 insights + topology summary + "start here" (interesting points) — the *only* narrative-ish page, short by design |
| `/entries` | Entries | the full table (spec §4) |
| `/trace` · `/trace?focus=X` | Trace | interactive tree; focus in the query param (deep-linkable) |
| `/graph` | Graph | canvas, seeded per §5 |
| `/insights` | Insights | cards + Engine drawer |
| `/export` | Export | packs + preview |
| `/settings` | Settings | §7 groups |

Rules:
- **Rail** = fixed icons+labels, active state, counts badge on Entries/Insights when ready. Disabled
  (with tooltip "analyze first") until `session.ready()` — except Settings and Landing.
- **State in the URL** for anything shareable/back-button-able: trace focus, entries filter/kind,
  selected node (`?node=`). Angular Router query params, synced to store via effects — one direction
  only (URL → store on nav, store → URL replaceUrl on change) to avoid loops.
- Scroll-spy component: retire (it exists for the one-pager). Section-card stays as the in-view
  container primitive.
- The old narrative canvas remains reachable as the **Export preview** (it IS the LLM document) — not
  a navigation surface.

## 2. Shell & communications wiring (audit result + fixes)

Audited: `session.store.ts` is well-built — `ActivityService` + `OperationController` give
cancellation and progress; errors funnel to `fail()`; recents update on success. Keep the pattern.
Required fixes/wiring:

1. **Connection status is invisible.** `connection.store.ts` exists but the header shows nothing.
   Wire: header dot (green/amber/red) + tooltip (server version, port); `Ping` on boot + on-focus
   re-ping; a red state offers "restart server" (Tauri sidecar) / "retry".
2. **Stats fetch failure is silent** (`catch { this._stats.set(null) }`) — set an
   `_statsError` signal; Insights view shows a retriable inline error, never blank.
3. **One API gateway stays the rule:** all RPC through `DevContextApi`; I4 adds `getNode`,
   `getNeighbors`, `search` there — components never import the gRPC client directly.
4. **Store-per-domain** (session/trace/connection/recent/github) is right; add `node.store`
   (node card + palette state) and `prefs.store` (settings §7, persisted via Tauri store or
   localStorage under one key with schema version).
5. **Progress**: analyze progress renders in footer/status bar AND on the landing button — never a
   full-screen block; the app stays navigable during analysis (Settings must work while analyzing).

## 3. Readability rules — never plain text

The complaint "just showing plain text" is structural: markdown/preformatted dumps for structured
data. Rules:

- **Data gets components, prose gets width limits.** Entries/insights/packages/topology are ALWAYS
  tables/cards/chips — never `<pre>`. Narrative markdown only in Export preview.
- Identifiers (`OrderService`, routes) in mono font, ink color, linkified (NodeLink); prose in the
  UI font, `max-w-prose`, `text-ink-muted`.
- **Progressive disclosure over truncation:** long lists collapse at the cap with "… N more"
  *buttons* (expand in place), mirroring the CLI's structural caps — same numbers, same wording.
- Route chips: method badge (existing) + tokenized route (`/api/{id}` params dimmed).
- file:line always as a subtle trailing affordance (§4 col), click = reveal in editor.
- Empty/loading/error triad is MANDATORY per view: skeleton (not spinner) for loading; empty state
  = one sentence + one action; error = message + retry + "copy details".
- Density: rows `py-1.5`, tables `text-xs`, section headers uppercase `text-2xs` — already the house
  style; keep it, don't shrink further.

## 4. Entries view — the concrete spec (agent implements exactly this)

Current state (`section-entries.ts`) already has: kind chips, text search, table
(Method/Route/Target/Kind), approx badge, click-to-trace. **Keep all of it.** Upgrade:

| Aspect | Spec |
|---|---|
| Columns | Method (badge) · Route/Title (mono, param tokens dimmed) · Target (mono, NodeLink) · Auth (`[anon]` warn badge / policy name, when F1 data present — hide column if absent) · Kind (icon+label) · file:line (subtle, reveal-on-click) |
| Sorting | header click: Route (alpha) · Target (resolved-first, default) · Kind. Persist in URL (`?sort=`) |
| Filter | existing chips + search stay; chips show counts (`HTTP 70`); add "has target" / "approx" quick filters as toggle chips |
| Row actions | hover/focus reveals: **Trace** (existing click stays) · **Node card** · **Copy route** · **Reveal file**. Keyboard: `↑↓` move, `Enter` trace, `n` node card, `c` copy |
| Scale | >150 rows → `@defer` + windowing (CDK virtual scroll); header sticky; count in card subtitle = filtered/total (`34 / 94`) |
| Grouping | OPEN (default: flat table + chips; group-by-kind toggle deferred — chips already do the job) |
| Empty states | not analyzed → CTA to landing; filtered-to-zero → "No entries match — clear filters" button |

## 5. Graph & trace views

- Graph seeds from: current trace focus > selected node > top interesting points (F7) — never the
  whole graph by default. Node click = Node Card (not navigation). Seam-kind filter chips reuse
  SEAM_COLORS. Depth slider stays.
- Trace: tree rows, seam-colored edge chips, `[approx]` amber / `[verified]` green badges,
  expand-node re-queries deeper, breadcrumb of focus history (back = pop). "Send to Export pack" on
  any subtree. Empty trace (resolved but no edges) shows the engine's honest hint verbatim.

## 6. Command palette (Ctrl+K) — spec

Overlay, 560px, single input. Sources merged & sectioned: **Actions** (Analyze…, Re-analyze,
Switch vibe, Open export, Go to <view>) · **Entries** (route + method badge) · **Nodes**
(`SearchNodes`, debounced 150ms, top 8, kind icon) · **Recents**. `Enter` = default verb
(entry→trace, node→node card, action→run); `Tab` cycles verbs shown as chips (Trace · Node ·
Usages · Copy). Esc closes. No results → "search the graph for '<q>'" row. Palette verbs and CLI
`query` ops share names — it's the same vocabulary (FACES-DESIGN §1.1).

## 7. Settings — target structure (today: theme/recents/about only)

Groups (left tabs within Settings view): **Appearance** (vibes/themes — exists) ·
**Analysis** (default depth/detail; excluded dirs editor; "use Roslyn tier" toggle = `--no-roslyn`
inverse; lite mode) · **Storage** (I8: cache location + per-repo cache list with sizes + clear;
clone folder path picker + open-in-explorer + "keep clones" toggle; total disk usage bar) ·
**Server** (port, status, restart, logs folder open) · **About** (I9: version + engine version +
commit, license, third-party notices, check-updates, links). Recents move to Landing; the remove-x
stays.

## 8. Keyboard & polish baseline

`Ctrl+K` palette · `?` shortcut overlay · `g` then `o/e/t/g/i/x/s` view nav · `Esc` closes
sheet/palette. Focus rings visible; all interactive elements are real `<button>`/`<a>`
(the WPF-era D-audit rule); tooltips 300ms delay; `prefers-reduced-motion` respected (scanline/glow
effects off). Window size/position persisted (Tauri), min 1024×640.

## 9. WPF donor checklist (port these behaviors, then the WPF app is done informing us)

Dry-run honesty (show plan, don't render empty output) · focus suggestions datalist (palette covers
it) · per-section visibility toggles (Export packs cover it) · status bar with engine phase
(footer covers it). Nothing else in `DevContext.Desktop` is ahead of the Angular app; do not port
layout.
