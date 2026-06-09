# DevContext v2 — End-to-End Evaluation Findings

**Generated**: 2026-06-06  
**Method**: 12 scenario runs across 4 benchmark repos, each with `--metrics --include-diagnostics`

---

## 1. Crash Bug Found & Fixed

**Bug**: `SpectreDiscoveryObserver._indent` race condition in parallel Stage 2.  
**Symptom**: `ArgumentOutOfRangeException` on `new string(' ', _indent * 2)` when `_indent` goes negative from concurrent thread increments/decrements.  
**Trigger**: Library-mode repos (AutoMapper) with no web signals that skip Stage 3.  
**Fix**: Split indent into stage-level `_indent` (UI thread) and per-extractor `_extractorDepth` (thread-safe via `Interlocked`). Clamp to `Math.Max(0, depth)` in `WriteLine`.

---

## 2. Scenario System Assessment

### 2.1 Scenario Names Not Reflected in Output

**Evidence**: All 12 runs produce `## DevContext -- Architecture Overview on {Solution}` regardless of `--scenario` flag.

| Scenario | Header Shown | Expected Header |
|---|---|---|
| `architecture` | Architecture Overview | Architecture Overview |
| `debug-endpoint` | Architecture Overview | Debug Endpoint |
| `modify-middleware` | Architecture Overview | Modify Middleware |
| `trace-message-flow` | Architecture Overview | Trace Message Flow |
| `harden-di` | Architecture Overview | Harden DI |

**Root cause**: `MarkdownRenderer.AppendHeader` hardcodes `"Architecture Overview"` instead of reading `model.ActiveScenario.DisplayName`.

### 2.2 RequiredSections Not Enforced

**Evidence**: Each scenario defines `RequiredSections` in `ScenarioRegistry`, but the renderer always produces all sections. All 5 eShop scenarios produce the same sections:
- Architecture overview
- Endpoints
- MediatR Handlers
- Non-obvious wiring
- Related types grouped by layer

**Missing per scenario**:
- `debug-endpoint`: "Call graph" never appears (CallGraphExtractor requires Deep tier, Focused profile only runs Fast)
- `trace-message-flow`: "Domain signals" and "Call graph" never appear
- `modify-middleware`: "Entry" section doesn't exist as a separate section

### 2.3 PruningConfig Per-Scenario Not Enforced

**Evidence**: All eShop scenarios produce 439 surviving types despite `MaxSurvivingTypes` settings:

| Scenario | MaxSurvivingTypes | Actual surviving |
|---|---|---|
| architecture | 30 | 439 |
| debug-endpoint | 20 | 439 |
| modify-middleware | 25 | 439 |
| trace-message-flow | 30 | 439 |
| harden-di | 50 | 439 |

**Root cause**: `TokenBudgetEnforcer` uses token-budget-based pruning, not `MaxSurvivingTypes`. The scenario's `MaxSurvivingTypes` is defined but never read by any pruner. `PatternRelevancePruner` also ignores it. The `PruningConfig` class has the field but nothing enforces it.

---

## 3. Signal Detection Results

| Repo | Signals | Correct? |
|---|---|---|
| **eShop** | controllers(90%), minimal-apis(100%), mediatr(100%), fluentvalidation(100%) | ✅ |
| **TodoApi** | minimal-apis(100%) | ✅ |
| **VerticalSlice** | minimal-apis(80%), fluentvalidation(100%), fast-endpoints(100%) | ✅ (fast-endpoints detected via package ref) |
| **AutoMapper** | automapper(90%) | ✅ NEW — project-reference detection working |

**Observation**: `automapper` signal now fires at 90% confidence via `ProjectReference` scanning (Phase 1.2 fix). Library mode activates because no web signals present.

---

## 4. Endpoint Detection by Scenario

All scenarios produce the same endpoints (pruning doesn't differ):

| Repo | Endpoints | Quality |
|---|---|---|
| eShop | 30 HTTP + 14 controller | ✅ Routes normalized with leading `/api/catalog/...` |
| TodoApi | 11 | ✅ Clean |
| VerticalSlice | 23 (FastEndpoints) | ✅ Some `<dynamic>` routes (expected — non-literal expressions) |
| AutoMapper | 0 | ✅ Library mode — no HTTP endpoints |

---

## 5. Issues to Fix

| Priority | Issue | File(s) | Effort |
|---|---|---|---|
| **P0** | `SpectreDiscoveryObserver._indent` race (fixed) | `SpectreDiscoveryObserver.cs` | ✅ Done |
| **P1** | Scenario display name hardcoded in renderer | `MarkdownRenderer.cs:42` | ~5 min |
| **P1** | `MaxSurvivingTypes` scenario setting never enforced | `TokenBudgetEnforcer.cs`, `PatternRelevancePruner.cs` | ~30 min |
| **P2** | `RequiredSections` defined but never checked by renderer | `MarkdownRenderer.cs`, `DiscoveryPipeline.cs` | ~1h |
| **P2** | "Call graph" section — only produced in Deep/Debug profile, but scenarios assume it appears in Focused | `MarkdownRenderer.cs` | ~15 min |
| **P3** | "Entry" section referenced in scenarios but doesn't exist as separate output section | `ScenarioRegistry.cs`, `MarkdownRenderer.cs` | ~30 min |

---

## 6. Already Working Correctly

- ✅ Shared syntax cache reduces runtime (AutoMapper: 8s → 2.5s)
- ✅ `--metrics` under Composite works (rich extractor table)
- ✅ MapGroup prefixes resolved (eShop: `/api/catalog/items`)
- ✅ Controller convention routes (eShop: `/Account` not `/`)
- ✅ Library-mode namespace summary (AutoMapper: compact, not flat wall)
- ✅ Route normalization with leading `/`
- ✅ Duplicate type deduplication (no warnings)
- ✅ Project-reference signals (AutoMapper signal fires)
- ✅ NServiceBus constant used instead of magic string
