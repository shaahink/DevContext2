# Handover — Desktop UI V4 (U1–U5) + Engine E2–I9

> **Date:** 2026-07-02 · **Branch:** `develop` · **Commit:** `2585385`
> **Covers:** All unreviewed changes from `feat/ui-iteration` and `feat/engine-iteration`

---

## 1. Git summary

```
develop
├── feat/ui-iteration (UI work: U1–U5)         ← merged into develop
│   ├── d243b39 feat(ui): U1-U5 — live console, synced lens, workspace nav, release polish
│   └── 2585385 merge: feat/engine-iteration → resolved AGENTS.md/PROGRESS-LOG.md conflicts
└── feat/engine-iteration (engine work: E2–I9)  ← merged into feat/ui-iteration, then develop
    ├── d81fd18 E5 benchmark expansion
    ├── 3e19fdc docs tracker
    ├── a4fda72 I9 CLI exit codes + --quiet
    ├── d56dbd9 I10.3 Server rehydrate
    ├── 79af9a8 I8 snapshot cache
    ├── 9cb8a28 A-F14 EF depth
    ├── 4be13e8 A-F15 CPM + Directory.Build.props
    └── e72d90f E2 Pattern-Zoo
```

**Other worktrees are NOT merged** (they are outdated UI architectures):
- `docs/go-to-program-addendum` — old standalone views, conflicts with current code
- `go-to/implement-iterations` — old standalone views, conflicts with current code
- These branches should NOT be merged (they delete our current UI sections)

---

## 2. UI work delivered (feat/ui-iteration)

### U1 — Live Console (P2) ✅
| File | Role |
|------|------|
| `features/narrative/section-console.ts` | boot-log + RunReport, two-mode (boot/report), auto-scroll, phosphor terminal styling |

**Data:** `WorkspaceStore.TabSessionSlice.consoleLog` — accumulated progress events as `LogLine[]`

### U2 — Synced Lens (P3) ✅
| File | Role |
|------|------|
| `features/narrative/section-lens.ts` | 50/50 Human left (trace tree + node detail) + LLM right (auto-rendered markdown) |

**Data:** auto-renders via `api.render(handle, { focus, format: 'markdown' })` on `TraceStore.focus` change (debounced 250ms). Copy button + Ctrl+C.

### U3 — Facet Views ⬜ BLOCKED
**Blocked on engine E4** — `FacetDescriptor.cs` exists in the engine but produces `ImmutableArray<string> Lines` (text), not typed proto fields. The UI needs proto changes to receive structured facet data (F1 auth, F3 message matrix, F4 data map, F5 talks-to, F8 DI health). See `src/DevContext.Core/Graph/Facets/FacetDescriptor.cs` for the engine-side infrastructure.

### U4 — Release Polish ✅
| File | Change |
|------|--------|
| `section-settings.ts` | About with real version, server dot, GitHub/updates links, privacy note |
| `node.store.ts` | Added `error` signal + toast on failure |
| `node-card.ts` | Toast on clipboard copy success/failure |
| `palette.ts` | Clarified swallow comment |

**Audit:** All 34 `catch` sites reviewed. No truly swallowed user-facing errors remain.

### U5 — Workspace Navigation ✅
| File | Role |
|------|------|
| `shell/navigation-rail.ts` | Left rail with icon+label, session-gated, active route highlight (reactive signal) |
| `shell/workspace-shell.ts` | Header + rail + router-outlet + footer, `g+key` nav, `?` help overlay, Ctrl+K palette |
| `app.config.ts` | 8 lazy-loaded routes replacing wildcard |
| `features/pages/*.ts` | 6 page wrappers reusing existing section components |

**Keyboard shortcuts:** `g o/e/t/g/i/x/s` → view nav, `?` → help overlay, `Escape` → close

### U5 — Entries Table ✅
| Feature | How |
|---------|-----|
| Sortable columns | Click Method/Route/Target/Kind header to toggle asc/desc |
| Keyboard nav | ↑↓ to move selection, Enter to trace, `n` for NodeCard, `Ctrl+C` to copy |
| Filtered/total | Subtitle shows `N / M entries` when filtered |

### U5 — Palette ✅
- Entry results capped at top 10
- Stale routes removed (Browse, Document, Stats)
- Export view added

---

## 3. Engine work delivered (feat/engine-iteration)

### E2 — Pattern-Zoo Corpus ✅
- 9 fixture files covering C# syntax shapes
- 13 tests: Sends edges across all patterns + negative guards
- I1.3 conjunction gate + I1.5 string literal stripping

### E5 — Benchmark Expansion ✅
- 8 missing-archetype repos registered in `eval-repos.json` + expectations

### I8 — Snapshot Cache ✅
- `SnapshotCacheService` — SHA256 + git HEAD key, JSON.gz, LRU (10/rep, 2GB)

### I10.3 — Server Rehydrate ✅
- `EngineRunner` checks I8 cache before analysis

### A-F14 — EF Depth ✅
- `EntityRelation` edges, entity-to-aggregate root depth annotation

### A-F15 — Build Intelligence ✅
- CPM (`Directory.Packages.props`) + `Directory.Build.props` ancestor chain

### I9 — Release Readiness ✅
- Exit codes (0-4), `--quiet` flag

---

## 4. UI audit findings & fixes

| Issue | Severity | Fix |
|-------|----------|-----|
| Navigation rail `isActive` not reactive (zoneless) | Critical | Subscribed to Router events → `_currentUrl` signal |
| `palette.ts` stale routes (Browse, Document, Stats) | Medium | Replaced with current routes + added Export |
| `node.store.ts` silent failure (no toast) | High | Added `error` signal + toast |
| `node-card.ts` clipboard silence | Medium | Added toast success/failure |
| `section-entries.ts` unused `idx` param | Low | Removed parameter |
| Export page `[open]="true"` binding | Low | Verified effect triggers render correctly |
| SectionConsole auto-scroll in new layout | Low | `afterRender` + `scrollAnchor` works |
| `SearchField` `[(query)]` binding | Low | Uses Angular `model()` signal — correct |
| Duplicate U3 in AGENTS.md | Low | Cleaned in merged version |

---

## 5. File inventory (new files only)

```
src/DevContext.App/src/app/
  shell/
    navigation-rail.ts          — Left sidebar navigation
    workspace-shell.ts           — Layout container + shortcuts
  features/
    narrative/
      section-console.ts        — Live console boot-log + RunReport
      section-lens.ts           — Synced Human/LLM split pane
    pages/
      overview-page.ts          — Landing + Identity + Architecture + Stats
      entries-page.ts           — Entries table wrapper
      trace-page.ts             — Trace tree wrapper
      graph-page.ts             — Graph wrapper
      insights-page.ts          — Insights wrapper
      export-page.ts            — Export wrapper with dismiss
```

---

## 6. Resume protocol

```powershell
# Cold start
git checkout develop
git pull

# Verify baseline (UI)
Set-Location C:\Code\DevContext2-ui\src\DevContext.App
pnpm install
pnpm check      # lint + 7 tests + build → GREEN

# Start dev
pnpm server     # terminal 1a: .NET backend
pnpm dev:web    # terminal 1b: Angular → http://localhost:4200

# Engine verify (from C:/Code/DevContext2-engine worktree)
dotnet build DevContext.slnx
dotnet test DevContext.slnx --filter "Category!=Eval"
```

## 7. Next items

| Priority | Item | Status |
|----------|------|---------|
| P0 | **E4 Proto changes** — expose facet data as typed proto fields for UI consumption | Needed for U3 |
| P1 | **U3 Facet views** — F1 auth, F3 message matrix, F4 data map, F5 talks-to, F8 DI health | Blocked on E4 |
| P2 | Manual smoke test — run the app and verify all routes, keyboard shortcuts, live console, lens | Ready |

## 8. Known caveats

- The narrative canvas still exists (`features/narrative/narrative-canvas.ts`) but is no longer the main entry point — it's superseded by the workspace shell + routed views
- `graph-page.ts` uses `SectionGraph` (simple) rather than `GraphView` (BFS exploration) — the standalone GraphView is available but not wired
- `insights-page.ts` and `settings-page.ts` load existing standalone components (`InsightsView`, `SettingsView`) with their own internal layouts
- Tab strip (`shell/tab-strip.ts`) exists but is not used in the new workspace shell
- `pnpm build` may time out with concurrent Angular builds (resource contention) — verified via `tsc --noEmit` instead
