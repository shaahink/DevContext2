# QA-BUGFIXES.md — Bugfix Plan for Next Engineer

**Date:** 2026-07-09  
**Source:** QA Driver session #13 (Phase 3 — Final QA)  
**Branch:** `feat/loom-l7`  

---

## Bugfix Queue (ordered by impact/cost ratio)

| # | Priority | Gap | Surface | Effort | Files |
|---|----------|-----|---------|--------|-------|
| 1 | P0 | LOOM-START.md baseline table stale truth count | Docs | 5 min | `LOOM-START.md:85` |
| 2 | P1 | L2.4 checkout trace DEVIATES — bus-publish seams not walked | Engine | 45-60 min | `GraphBuilder.cs`, trace traversal |
| 3 | P2 | Tab strip height 28px (not ≥30px) | UI | 15 min | `tab-strip.ts`, `styles.css` |
| 4 | P2 | Code pane null content on entry selection | UI + Server | 30 min | `inspector.ts`, `read_source` RPC |
| 5 | P2 | ContextPack server round-trip v0 (client assembles markdown) | Engine + Server | ~1 session | `ContextPackBuilder.cs`, `GraphQueryService.cs` |
| 6 | P3 | MCP page `mcpRunning` initialized to false — shows "Stopped" on revisit | UI | 30 min | `mcp-page.ts`, `mcp.store.ts` |
| 7 | P3 | Inspector insights substring fallback: "Order" matches "OrderService" | Engine | 15 min | `inspector.ts` or insight matching |
| 8 | P3 | L7.1 spine-depth metric never computed | Engine | 20 min | `FlowModel.cs`, `GraphStats.cs` |
| 9 | P4 | bench.ps1 line 263 encoding fix | Script | 5 min | `scripts/bench.ps1:263` |
| 10 | P4 | Perf budget drift: design doc §8 says ≤4s, reality ~6s | Docs | 10 min | `loom-graph-design.md` §8 |

---

## Detailed Bug Descriptions

### 1. LOOM-START.md baseline table stale (P0)

**Symptom:** Line 85 baseline table claims "9P/2F/0S" for truth tests. Reality is 8P/3S.  
**Root cause:** DntSite test flipped from Skip→Pass after repo was cloned, but table wasn't updated with the correct 8P/3S count.  
**Fix:** Update line 85 to reflect "8P/3S (3 [TruthPending] ratchets)".  
**Verification:** Header block line 9 already says "8P/0F/3S" — verify consistency after fix.

### 2. L2.4 checkout trace bus-publish DEVIATES (P1)

**Symptom:** `Dogfood_checkout_flow_traces_cross_service_depth_ge_5` is `[TruthPending("L2")]` — test can't be activated because checkout trace doesn't walk bus-publish seams (BasketCheckoutEvent → Ordering.Application).  
**Root cause:** Trace traversal skips bus-publish edges (ServiceLink kind with bus transport). The seam exists (BusPublishDetector detects it) but trace doesn't follow it.  
**Fix:** Update trace walker to include bus-publish ServiceLinks. Likely in GraphBuilder.cs trace assembly or FlowModel.cs spine construction.  
**Verification:** Remove `[TruthPending("L2")]` from Dogfood_checkout_flow, run truth gate, verify depth ≥5.

### 3. Tab strip height 28px (P2)

**Symptom:** Tab strip measured at 28px. Target is ≥30px (HANDOVER claims 32px).  
**Root cause:** Tailwind `h-8` (32px) class may be overridden or inner padding/font-size reduces effective height.  
**Fix:** Inspect `tab-strip.ts` line 21 for `h-8` class. Check parent container padding. Adjust to ensure minimum 30px measured height.  
**Verification:** Re-run `node scripts/ui-audit-drive.mjs --gate`, confirm stripH ≥30px.

### 4. Code pane null on entry selection (P2)

**Symptom:** UA drive assertion C fails: code pane content length is null after selecting an entry in Explore.  
**Root cause:** Inspector Code tab calls `read_source` RPC which may not be wired for all node types, or the test selects a node that doesn't have source spans.  
**Fix:** Check `read_source` RPC handler in server → verify it handles all node types with source spans. Ensure Inspector falls back gracefully when source unavailable.  
**Verification:** Manual test: select a known node (e.g., Order class) → Code tab shows C# source with PrismJS highlighting.

### 5. ContextPack server round-trip v0 (P2)

**Symptom:** Context Studio Copy/Save assembles markdown client-side. Server has `ContextPackBuilder` but the full pack is client string concatenation.  
**Root cause:** `getContext` RPC returns structured cards, but the final assembly (markdown generation) happens in the UI.  
**Fix:** Move markdown assembly to server. `ContextPackBuilder` should produce the final formatted output. Update `getContext` RPC to return the assembled pack.  
**Verification:** Copy from Context Studio → verify markdown matches server-generated format. Token meter uses server-side counts.

### 6. MCP page mcpRunning false on revisit (P3)

**Symptom:** MCP page shows "Stopped" when navigated back after the server is running.  
**Root cause:** `mcpRunning` signal initialized to `false`. On page revisit, re-initializes without checking the actual MCP server state.  
**Fix:** Initialize `mcpRunning` from the MCP server status (ping on component init). Or persist state in a service.  
**Verification:** Start server → visit MCP page (shows "Running") → navigate away → return to MCP page → still shows "Running".

### 7. Inspector substring false positives (P3)

**Symptom:** Insights section matching uses substring: searching "Order" can match "OrderService" or "CustomerOrder".  
**Root cause:** Insight matching in `inspector.ts` uses simple substring checks rather than symbol-bound matching.  
**Fix:** Match against resolved SymbolIds rather than string comparison. Or add word-boundary matching.  
**Verification:** Select "Order" node → inspector insights only show Order-specific items, not OrderService.

### 8. L7.1 spine-depth metric missing (P3)

**Symptom:** Design spec requires "entries with ≥2-deep spine ≥70%" but no metric collector exists.  
**Root cause:** L7.1 `PlainCallDetector` produces the edges, but no aggregation of spine depth per entry was implemented.  
**Fix:** Add spine-depth counter to `GraphStats`. Walk entries → compute max depth → report count with depth ≥2. Add to report output.  
**Verification:** Run on dogfood, verify ≥70% entries have depth ≥2.

### 9. bench.ps1 encoding (P4)

**Symptom:** bench.ps1 line 263 backtick-n parsing error in PowerShell 5.1.  
**Root cause:** UTF-8 encoding issue with backtick-n escaped newline on the last line. Resolved this session by rewriting with UTF-8 no-BOM.  
**Fix:** Permanent fix: use `[Environment]::NewLine` or remove the special character from the last Write-Host.  
**Verification:** `powershell -File scripts/bench.ps1` exits clean on a fresh clone.

### 10. Perf budget drift (P4)

**Symptom:** Design doc §8 says "Dogfood ≤4s" but post-Loom reality is ~5.6s.  
**Root cause:** Budget was aspirational (Meridian baseline). Loom added Flows, ArchetypeDetector, 6 seam detectors, SemanticLite.  
**Fix:** Update `loom-graph-design.md` §8 to reflect post-Loom budget: Dogfood ≤6s, Tier A only ≤4s.  
**Verification:** Doc updated, budget matches measured performance.

---

## Session Plan

| Session | Items | Estimated Time |
|---------|-------|----------------|
| 1 | #1 (LOOM-START.md fix) + #10 (perf budget doc) | 15 min |
| 2 | #2 (L2.4 checkout trace) | 45-60 min |
| 3 | #3 (tab strip) + #4 (code pane) | 45 min |
| 4 | #6 (MCP mcpRunning) + #7 (substring) + #8 (spine metric) + #9 (bench encoding) | ~1 hr |
| 5 | #5 (ContextPack server round-trip) | ~1 session |

**Total:** ~5 sessions, ~4-5 hours.

---

*End of QA-BUGFIXES.md — 2026-07-09, session #13*
