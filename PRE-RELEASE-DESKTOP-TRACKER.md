# DevContext pre-release - desktop agent loop — Phase Tracker

**Plan:** DevContext pre-release - desktop agent loop | **Branch:** `feat/pre-release-desktop` | **Plan doc:** docs/dev/research/PRE-RELEASE-PLAN-2026-08-13.md

## Handoff (overwrite this block, ≤12 lines, no history)

Fresh program. Nothing delivered yet. Start at N0.1. All four owner decisions for this
run are already made (STUDIO-MCP-AUDIT §8: pins IMPLEMENT, convergence FULL, hand-off
REPO FILE FIRST, MCP page FULL DECK + SHIP BINARY) — do not re-open them. N4 has a
precondition: merge feat/pre-release-engine (T1's curated catalog) into this branch
first; if T1 has not landed, deliver N4.1–N4.2 and leave N4.3 open.

## Baseline numbers

| Metric | Value |
|---|---|
| Total checkpoints | 15 |
| Done | 0 |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · DONE ✓ (confirmed) · BLOCKED · SKIPPED. Evidence = artifact path produced by a run
this phase (a code path is not evidence). Agent claims are marked DONE; engine confirms as DONE ✓.
Escalation legend: to escalate to the owner, put a line starting with the word HUMAN followed by a colon
into the handoff block — only when escalating right now, never as prose.

### N0 — truth batch (no decisions)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N0.1 | Studio truth items: multi-entry merge preserves SourceLocations/Verified/Approx (§3.F.3); allocated_tokens no longer echoes budget (§3.F.4); Studio copy/save use the app's clipboard helper and toasts await outcome (§3.F.7) | TODO | | |
| N0.2 | MCP page truth items: status read no longer calls StartMcp (§3.F.9); snippet paths + copy-label fix (§3.F.10/11); feed totals respect the filter + wire timestamps (§3.F.12); sessions table renders the honesty fields so the shown age stops lying (§3.F.13); dead state deleted (§3.F.14) | TODO | | |
| N0.3 | The §3.F inventory filed into BUG-BACKLOG.md as triaged bugs; spec smoke coverage exists for both pages (the three data-testids referenced by real specs) | TODO | | |

### N1 — Studio truth pass + pins (decision 1: IMPLEMENT)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N1.1 | Verified/approx rendered per card; verification ledger verifies the pack actually built (wire item 4, mechanism chosen with a stated reason); state lifecycle fixed (per-tab keying or handle-effect invalidation; budget/intent/format persisted); body toggles wired or deleted | TODO | | |
| N1.2 | Pins real end-to-end: `p` pins from Explore; TrailStore.pins() has real readers; pinned steps seed the pack; the three advertising surfaces (inspector, trail bar, ticker) tell the truth | TODO | | |

### N2 — pack convergence (decision 2: FULL)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N2.1 | BuildMulti adopts the ResolveEntry path (symbol-rooted cards); `usage` joins CardTypeSections; picker gains a Types tab (LibrarySurface list) + D-G row identity (target member + route tail + project) | TODO | | |
| N2.2 | Honesty-note parity with get_context (fill-rate note + suggested focuses in the rail); budget default reconciled to one stated number; ACCEPTANCE: a FluentValidation pack composed from types, with usage and verified counts, end to end | TODO | | |

### M1 — hygiene + Reader prerequisites

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| M1.1 | Proto/mapper shopping list: TraceNode structured file_path+line_number; ReadSource file mode (or GetFileSource) with caps; per-file edge overlay query on the wire; ProtoMapper stops dropping MultiImplCount/DiHostCount/TestOnly/OmittedNames | TODO | | |
| M1.2 | Hygiene: MapResponse.stack populated or its three consumers stop rendering it (bug filed either way); Layer/Feature lens slots hidden until data exists; createTab MAX_TABS lie fixed; dock resizer added; high-contrast theme selectable or removed | TODO | | |

### N3 — loop joints (decision 3: repo file first)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N3.1 | Send-to-Studio from Explore (selection/trail/pins), Insights cards, and NodeCard; Studio default state = proposed pack from current trail+pins (never opens empty after exploration); archetype preset on fresh sessions | TODO | | |
| N3.2 | Save writes `.devcontext/packs/<slug>.md` (gitignored by default) + a copyable point-your-agent-here line for CLAUDE.md; Home's point-your-agent-here routes through Studio | TODO | | |

### N4 — MCP page rebuild (decision 4: full deck + ship binary)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| N4.1 | Status that measures: binary fs probe; ObserverCount + last-agent-call-at on the wire and rendered; handshake = one real MCP tools/list round-trip shown; Start/Stop killed or renamed to what it does | TODO | | |
| N4.2 | Setup that works: devcontext-mcp ships in the Tauri bundle; snippets carry the resolved absolute path; write-config-for-me button per host | TODO | | |
| N4.3 | The catalog served: ListTools RPC (kills #4 structurally); page renders the curated described menu agents actually get (requires T1 merged in); feed keyed by MCP tool names (analyze wrapped, args digest, wire timestamps); rows deep-link — trace→Explore, get_context→replay-in-Studio | TODO | | |

### Z1 — close-out

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| Z1.1 | STUDIO-MCP-AUDIT statuses + DECISIONS.md (D-G settled by N2) + BUG-BACKLOG reconciled; PRE-RELEASE-PLAN §3 table updated for this run; full battery green; branch pushed | TODO | | |
