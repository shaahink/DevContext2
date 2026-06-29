# Desktop UI Guide

The DevContext Desktop app wraps the same engine as the CLI in an interactive WPF + BlazorWebView
shell: a left config panel, a right output panel with Human / LLM / Stats tabs, and a right-side
section drawer that filters both views in sync.

---

## Layout

```
┌── ConfigPanel ──────┬── OutputPanel ───────────────────────┐
│                      │  [Human] [LLM] [Stats]   [Sections]  │
│  Source              │                                      │
│  Focus   [Map/Trace] │  MAP  MyApp  (12 projects)           │
│  Depth   (trace)     │  STACK  net10.0 · Minimal APIs …     │
│  Detail  (trace)     │  TOPOLOGY (depends-on) …             │
│  Output              │  ENTRY POINTS …                      │
│   Format             │                                      │
│   Advanced           │                                      │
│                      │                                      │
│  [ Analyze ]         │                                      │
└──────────────────────┴──────────────────────────────────────┘
```

---

## ConfigPanel (left)

### Source

A local path (`.sln`, `.csproj`, or directory) or a GitHub URL (`github.com/user/repo`). Recent
paths appear as clickable chips. For a GitHub URL a cleanup selector appears: **Auto-clean on exit**
(default) or **Keep permanently**. A cloned repo is reused for the whole session — changing options
or the focus re-analyzes the existing clone instead of re-cloning — and is deleted on app close when
"Auto-clean on exit" is selected.

### Focus  — one control, mode is derived

A single field accepting a `TypeName`, `Type:Method`, or `GET /api/route`. The badge next to the
label shows the **derived mode**:

- **Empty → Map.** Whole-codebase architecture map (style, stack, topology, entry points, packages).
- **Set → Trace.** The call stack from that entry, down the wiring.

After an analysis the field doubles as an **entry picker**: a dropdown lists the entry points found
in the graph (HTTP, bus consumers, hosted services, …). Pick one to trace it; clear the field to go
back to the Map. Changing the focus re-analyzes (the trace needs the call graph, which only the
deep-dive/Debug profile builds) — debounced so typing doesn't spawn a run per keystroke. This is the
exact same `--focus` behavior as the CLI.

### Depth & Detail  (Trace only)

Shown only when a focus is set:

- **Depth** (1–10, default 6) — hops to follow from the entry.
- **Detail** — `Signature` (names only), `Salient` (key body lines), `Full` (method slice).

Both are render-time dials: changing them re-renders the existing analysis without re-analyzing.

### Output

- **Format** — Markdown (Map/Trace narrative) or JSON (structured; preview only in the LLM tab).
- **Advanced**:
  - *Include diagnostics* — appends graph + call-graph diagnostics under a Map/Trace.
  - *Skip Roslyn* — faster; weaker semantic call resolution.
  - *Dry run* — plan only; lists which extractors would run.
  - *Include provenance* / *Include anti-pattern detection* — only shown when the legacy catalog
    output is active (no graph); they don't affect the Map/Trace narrative.

The token-budget slider is intentionally hidden in the current build: the budget doesn't constrain
the Map/Trace narrative, so it would be a dead control. Section-level token accounting is still
shown in the section drawer (see below).

---

## OutputPanel (right)

### Tabs

- **Human** — styled HTML rendering of the output.
- **LLM** — the raw markdown, ready to paste into any LLM.
- **Stats** — the RunReport (stage timing, extractors, cache/corpus).

Both Human and LLM render the **same sections** — just HTML vs raw markdown.

### Sections drawer

The **Sections** button (top-right, with a badge counting hidden sections) opens a right-side drawer
listing every section of the current output — the Map/Trace narrative blocks (Overview, Topology,
Entry points, Cross-cutting, Packages, …) or, in catalog mode, the legacy tables. Each row shows its
token contribution. Toggling a section hides it in **both** the Human and LLM views at once; the DOM
stays intact (no scroll jump), so it's instant. A budget bar at the top of the drawer shows selected
tokens against the budget.

### Toolbar

- **?** — what-is-this overlay.
- **Copy** — copies the Human view.
- **Save** — downloads `.md` / `.json`.
- **Copy LLM** — copies the LLM view.

---

## Status & logging

The status line shows estimated tokens and elapsed time. The Analyze button is disabled until a
source path is filled. DevContext Desktop logs to `%LocalAppData%\DevContext\`:

| File | Contents |
|------|----------|
| `devcontext.log` | General logs (rolling daily, 7 days retained) |
| `crash.log` | Error-level entries — crashes with full stack traces |

Global exception handlers are registered for the WPF dispatcher, AppDomain, and unobserved tasks.
