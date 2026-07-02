# Iteration I10 — Multi-tab workspace (browse several repos at once)

> **Status: NOT STARTED** · Addendum (new user requirement, 2026-07-02) · Depends on: I4 (workspace
> nav shell) · **strongly wants I8** (snapshot rehydration is what makes >3 tabs memory-honest;
> a reduced v1 without I8 is defined below) · Complements UI-UX-GUIDELINES — this doc is that spec's
> missing "tabs" chapter; it does not modify the frozen docs.

## Goal

Up to **6 tabs**, each an independent repo/folder session, VS Code-grade tab ergonomics, minimal UI,
and a memory story that doesn't melt the machine when someone opens OrchardCore in three tabs.

## 1. The model

```ts
// state/workspace.store.ts
interface TabState {
  readonly id: string;                  // uuid, session-local — never in the URL
  readonly path: string;                // what the user analyzed (dir / sln / URL)
  readonly label: string;               // short repo label (summary.label once ready)
  readonly session: TabSessionSlice;    // exactly today's SessionStore fields
  readonly trace: TabTraceSlice;        // today's TraceStore fields
  readonly node: TabNodeSlice;          // node-card/palette selection (I4)
  readonly view: { route: string; params: Record<string,string>; scroll: number };
}
WorkspaceStore { tabs: signal<TabState[]>; activeId: signal<string>; /* cap = prefs.maxTabs (6) */ }
```

**Migration with minimal churn (the tricky bit — do it exactly like this):** keep `SessionStore` /
`TraceStore` as injectable **facades with their current public signal APIs**, re-implemented as
computed projections of `workspace.active`. Components don't change at all in the first commit; only
the store internals move. Then new tab-aware behavior lands behind the facades.

**Race gotcha (write into the owning tab, not the active one):** `analyze()` captures its `tabId` at
call time; every completion/progress/error callback writes via
`workspace.update(tabId, fn)` — an analysis finishing while the user is on another tab must update
*its own* tab (badge), never bleed into the active one. Same for trace fetches. Add a unit test:
start analyze on tab A, switch to tab B, complete → A.status === 'ready', B untouched.

**Scoping rules:** per-tab = session/trace/node/route/scroll + activity (progress, cancel). Global =
theme, prefs, connection, recents, palette overlay (palette acts on the active tab). `ActivityService`
becomes a per-tab registry; the footer projects the **active** tab's operation and shows a subtle
"N background" chip when other tabs are busy (click = jump to that tab).

## 2. Memory strategy (why 6 is safe)

Client side is trivial (per-tab VMs are KBs–low MBs). The cost is **server-side snapshots** (types +
source bodies + graph; large repos run to hundreds of MB). Strategy:

- Server gains `MaxLiveSessions` (default **3**). LRU eviction of the *heavy* model; with **I8**, the
  queryable parts are already on disk — an evicted handle **rehydrates** on next query (serves
  map/entries/insights/graph/node/search instantly; a profile-changing operation triggers transparent
  re-analysis with tab-scoped progress: "waking this repo…").
- Tab cap (6) = live cap (3) + rehydratable headroom. Both numbers in Settings → Storage, with a
  one-line explanation ("live analyses use memory; background tabs park to disk").
- **Reduced v1 (if I10 lands before I8):** cap tabs at `MaxLiveSessions` (3) and show "close a tab to
  open another"; a handle the server dropped (`NotFound` on any RPC) triggers an automatic re-analyze
  of that tab with progress — define this NotFound→reanalyze recovery in `DevContextApi` regardless,
  it also covers server restarts.
- Session close: closing a tab calls the existing `CloseSession` RPC (frees the snapshot immediately).

## 3. Tab strip UX (VS Code-grade, house-minimal)

- **Placement:** a 32px strip directly under the header row (header keeps logo · ⌘K · status dot;
  the honesty ribbon moves INTO each tab's Overview — it's per-repo state).
- **Tab anatomy:** `◆ label × ` — mono label (truncate 18ch, full path in tooltip); left dot =
  state: none (idle) · spinner (analyzing/cloning) · red (error) · nothing when ready; close ×
  visible on hover + active tab. Active = underline accent + ink text (match segmented-control
  styling, zero radius in terminal vibe).
- **`+` button** at the strip end → new tab with the Landing view (path box focused). Disabled at
  cap with tooltip "Tab limit (6) — close one or raise it in Settings".
- **Interactions:** click switch · middle-click close · `Ctrl+T` new · `Ctrl+W` close active ·
  `Ctrl+Tab`/`Ctrl+Shift+Tab` MRU cycle · `Ctrl+1..6` jump · closing the active tab activates the
  MRU neighbor. Drag-reorder: **OPEN, default NO** for v1 (cheap to add later; VS Code parity not
  required day one).
- **One tab open = strip still visible** (stable layout; it's also the "+" affordance). Single-scroll
  era's scroll-spy stays retired; tabs + rail are the whole navigation now.
- Duplicate-path guard: analyzing a path already open in another tab **switches to that tab**
  (offer "open anyway" in a toast for the rare intentional double).

## 4. URL & persistence

- URL always reflects the **active tab's** view (`/entries?kind=http`…, per UI-UX-GUIDELINES §1);
  switching tabs rewrites the URL (`replaceUrl: true`). Tab identity itself never appears in the URL.
- Persist across restarts (prefs store): `[{path, label, viewRoute}]` + activeIndex. Restore as
  **idle tabs** (label + path, no analysis) — first activation analyzes lazily (instant under I8's
  cache). **Never** auto-analyze all restored tabs on boot (startup storm).
- Single-instance handoff (I9): a second launch with a path argument opens it as a new tab in the
  running window.

## 5. Server work summary

`MaxLiveSessions` + LRU + (with I8) rehydrate path · `CloseSession` already exists ·
`Ping`/`GetStorageInfo` expose live/parked session counts for the Settings view · NotFound→reanalyze
contract documented in the proto comments.

## Docs & gate

`desktop-ui.md` tabs chapter + updated screenshots; `UI-UX-GUIDELINES` is NOT edited (frozen) — this
doc carries the spec. Gate: 3 repos open simultaneously (TodoApi, DntSite, eShop) — switch is
instant, per-tab progress isolated (start eShop analyze, browse TodoApi while it runs), close frees
the session (server log), restart restores idle tabs, cap enforced; the tab-race unit test green;
screenshots of the strip in all three vibes.
