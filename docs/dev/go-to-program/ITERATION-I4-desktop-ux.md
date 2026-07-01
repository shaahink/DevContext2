# Iteration I4 — Desktop UX: the canvas becomes a browser

> **Status: BLOCKED on I2 (contract) · benefits from I3 (insights)** · Phase: V3/V7 · 1–2 sessions.
> Design contract: `FACES-DESIGN.md` §2.3. Angular app on `feat/narrative-canvas` lineage; coordinate
> with any active desktop agent — this iteration touches `DevContext.App` broadly.

## Goal

Moves 1–3: every name is a node link; Ctrl+K palette; sections get smart. The server RPCs this needs
(`GetNode`, `GetNeighbors`, `SearchNodes`) already exist and have zero UI today.

## Slices (each independently commitable)

1. **Node Card** — `features/node/node-card.ts` in the existing `sheet` component: fetch `GetNode` +
   `GetNeighbors(both)`, group neighbors by seam kind (SEAM_COLORS), "usages" tab = in-edges,
   `file:line` row with reveal-in-editor (`vscode://file/{path}:{line}` with `Tauri shell.open`; fall
   back to OS open). A `NodeLink` component (name + subtle underline) that any section can render.
   **Tricky bit — linkifying rendered narrative:** don't regex the rendered markdown. The engine
   already returns *structured* sections; for entries/trace the app has structured stores. Where only
   markdown exists (architecture), linkify conservatively: match whole words against the entry+node
   title set fetched once per session (a Map cached in the store), not arbitrary identifiers.
2. **Command palette** — overlay on Ctrl+K: query `SearchNodes` (debounced), verbs per hit (`Trace`,
   `Node card`, `Usages`, `Copy id`), plus static actions (analyze path/URL, switch vibe, open export).
   Reuse `search-field` + `sheet`. Landing input = same component in "analyze" mode.
3. **Entries section smartness** — kind chips (counts), text filter, resolved-target-first sort, row
   actions (Trace / Node card / copy route). Auth badges once F1 lands (render `[anon]` from entry
   data when present — degrade gracefully when absent).
4. **Interactive trace** — trace store already holds the tree; add expand-on-node (call `GetTrace`
   with deeper depth anchored at that node, splice results), seam-color edge chips,
   `[verified]/[approx]` badges, provenance tooltip, "add subtree to export pack".
5. **Honesty ribbon** — identity section: archetype · scope note · `N/M targets` · approx% (from
   `GetStats`), always visible.
6. **Insights section** (with I3) + **Engine drawer** for telemetry (keep the stage waterfall there —
   it fits the terminal vibe, just not first).
7. **Export packs** — presets composing existing sections (Onboarding / Trace pack / Review pack) in
   the export overlay; token estimate = client-side `chars/4` (render-time only, no kernel budget).

## Best practices for the agent

- Signals + stores pattern is established (`session.store.ts`) — new state goes in stores, components
  stay dumb. No new global service unless a store can't express it.
- Every slice: `pnpm check` (lint+type) + `pnpm test` + a **screenshot** in the commit (the D1–D11
  lesson: green checks don't catch dead UX; the `run-devcontext/desktop-shot.ps1` helper exists).
- Real disabled states, pointer cursors, no swallowed catches (the WPF-era audit items — don't
  regress them in new components).
- Keep the narrative canvas as the spine — no route proliferation; Node Card/palette are overlays.

## Docs & goldens

`docs/product/desktop-ui.md` rewritten per section (same-commit rule). Server proto untouched except
I3's stats message. Playwright/component tests for palette open + node-card fetch if the harness
exists; otherwise the screenshot protocol.

## Gate

`pnpm check` + app tests green · click-through demo: entry → node card → neighbor → usages → trace →
export pack, recorded as a GIF/screenshot series in the PR · desktop still builds via Tauri.
