# Desktop UI Guide

The DevContext Desktop app provides interactive analysis with live progress, section toggles, and token budget controls.

---

## Layout

```
┌── ConfigPanel ──────┬── OutputPanel ───────────────────────┐
│                      │                                      │
│  Source              │  [Human View] [LLM View]             │
│  Intent              │                                      │
│  Mode                │  ## Architecture overview            │
│  Entry point         │  └── Web ── Application ── Domain   │
│  Sections            │                                      │
│  Token budget        │  ## Endpoints                       │
│  Symbol focus        │  | GET | /Feed | FeedController ... │
│  Output              │                                      │
│                      │  ## Data model (EF Core)            │
│  [ Analyze ]         │  ...                                │
│                      │                                      │
├──────────────────────┼──────────────────────────────────────┤
│  Status bar          │  ~6,273 tokens · 15.1s              │
└──────────────────────┴──────────────────────────────────────┘
```

---

## ConfigPanel (Left Sidebar)

### Source

Enter a local path (`.sln`, `.csproj`, or directory) or a GitHub URL (`github.com/user/repo`). Recent paths appear as clickable chips below the input.

When a GitHub URL is entered, the tool validates the repo and shows a cleanup option:
- **Cache 24h** — Keep clone for 24 hours (default)
- **Auto-clean** — Delete after analysis
- **Keep for session** — Keep until app closes
- **Keep permanently** — Never auto-delete

### Intent (optional)

Free-text description of what you want to analyze. Fills in Mode and Section checkboxes automatically:

| You type | Auto-selects |
|----------|-------------|
| `"trace the order handler"` | Trace mode + Call graph section |
| `"architecture overview"` | Overview mode + Architecture section |
| `"di wiring"` | Overview + DI/Wiring section + Debug profile |

Leave empty to manually choose Mode and Sections.

### Mode

Two toggle buttons:

- **Overview** — Whole-codebase architecture map. Broad pruning. Shows architecture overview, endpoints, entities, DI/wiring, related types.
- **Trace** — Entry-point focused. Tighter pruning. Requires an entry point for best results. Shows call graph, handler chains, event flow.

### Entry Point (Trace mode only)

The type or method to trace from. Prominently displayed in Trace mode, hidden in Overview mode.

| Format | Example |
|--------|---------|
| `ControllerName` | `FeedController` |
| `ControllerName:Action` | `FeedController:Posts` |
| `Namespace.Class:Method` | `MyApp.Services.OrderService:Create` |

Leave empty to scan all entry points.

### Sections

Nine checkboxes that control which sections appear in the output. Your profile is **auto-derived** from what you select:

| Checkbox | What it does | Profile effect |
|----------|-------------|----------------|
| Architecture overview | Project dependency tree, signals | — |
| Endpoints | HTTP endpoint table with routes, auth, source | — |
| MediatR Handlers | Command/query handler list | — |
| Data model | EF Core entities, migrations summary | — |
| DI registrations | Service registration table with lifetimes, types, sources | — |
| Background workers | HostedService / scheduled job listing | — |
| Middleware pipeline | Ordered HTTP middleware pipeline | — |
| Indirect wiring | Service locator, reflection activation, dynamic proxies | — |
| Call graph | BFS call tree from entry points | → **Debug** profile (+Roslyn, slower) |
| Message consumers | Event bus / in-memory consumers | — |
| Related types | Surviving types by layer | — |
| Source code | Full C# bodies for entry point + call chain types | → **Full** profile (+2k-12k tokens) |

### Token Budget

Slider + number input (500–50,000). Controls the `MaxOutputTokens` sent to the pruning stage. Lower values = more aggressive pruning.

### Symbol Focus (Overview mode only)

Optional entry point even in Overview mode. Shows as a secondary field. Same format as Entry Point.

### Output

- **Format** — Markdown (default) or JSON
- **Advanced** (collapsed by default):
  - Include provenance — shows why each type was included
  - Include diagnostics — shows pruning notes, warnings
  - Skip Roslyn — faster, no call graph or deep syntax analysis
  - Dry run — plan only, shows which extractors would run
  - Include anti-pattern detection — finds FireAndForget, ServiceLocator, etc.

---

## OutputPanel (Right Side)

### Tabs

- **Human View** — Rendered markdown output (readable preview)
- **LLM View** — Raw text ready to paste into any LLM. Sections can be toggled via checkboxes in the left panel.

### Section Drawer (LLM View)

Click the sections toggle to show/hide individual output sections. Each section shows its token contribution. Sections can be individually included/excluded to adjust the token budget.

### Toolbar

- **Copy** — Copies the current view to clipboard
- **Copy LLM** — Copies the LLM-optimized text
- **Save** — Downloads the output as `.md` or `.json`

### Token Budget Bar

A color-coded progress bar showing selected tokens vs budget:
- **Green** — Under budget
- **Yellow** — >= 85% of budget
- **Red** — Over budget

---

## Status Bar

Shows estimated token count and elapsed time after analysis completes.

---

## Keyboard Shortcuts

- **F5** — Run analysis (when source path is filled)
- The Analyze button is disabled when the source path is empty.

---

## Logging & Crash Recovery

DevContext Desktop logs to `%LocalAppData%\DevContext\`:

| File | Contents |
|------|----------|
| `devcontext.log` | General logs (rolling daily, 7 days retained) |
| `crash.log` | Error-level entries only — captures crashes with full stack traces |

Global exception handlers are registered for WPF dispatcher, AppDomain, and unobserved task exceptions.
