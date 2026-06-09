# DevContext v2 — Output Quality Assessment

**Date**: 2026-06-06  
**Method**: Reading actual output from 12 scenario runs across 4 benchmark repos  

---

## 1. Overall Structure by Section

### Header block (lines 1-6) — ✅ Good

Shows solution name, architecture style, signals, project count, profile, token usage, type count. **Actionable**: scenario name hardcoded to "Architecture Overview" — should reflect `--scenario` flag.

### Architecture overview (5-20 lines) — ✅ Good

Simple project list per layer. Useful for context. Could add dependency arrows or project type badges.

### Endpoints (15-60 lines) — ✅ Good, but needs polish

**What's useful**: Method, Route, Group, Handler, Auth, Source. Clean table format.

**Issues**:
- IdentityServer controllers show `/Account`, `/Grants` — correct but no indication they're HTML pages not REST APIs
- Lambda handlers show `λ {file}:{line}` — clean compact format ✅
- `/` route from `OpenApi.Extensions.cs:41` redirect is a framework concern, not user code — noise
- eShop: ~44 endpoints is comprehensive but includes internal routes like `GET /` which is an OpenAPI redirect, not an API endpoint

### MediatR Handlers (25 lines on eShop) — ✅ Good

Clean table with Kind, Request, Response, Handler. **Value**: helps understand CQRS flow.

### Non-obvious wiring — ⚠️ Mixed

**Middleware pipeline**: Shows UseX/MapX registrations with per-app ordering. But:
- Duplicate entries: `UseExceptionHandler` at Order 1 from TWO different Program.cs files (WebApp + Webhooks) — mixes middleware from different services into one table
- Order is per-program, not global — misleading when apps are merged

**Indirect wiring** (debug-endpoint/harden-di only): Shows service locator detections. Raw and honest.

**DI registrations**: ⚠️ 50% of eShop output — **REDUNDANT**

Lines 134-315 (181 lines) in the eShop output are DI registrations. Most entries show:
```
| Extension | AddX | ? |
```
`?` means the argument couldn't be statically resolved — these are noise.

Massive delegate bodies appear as inline C#:
```
| Singleton | serviceProvider => { ... 20 lines ... } | serviceProvider => { ... 20 lines ... } |
```
These are **duplicated** — the ServiceType and ImplementationType are identical 20-line lambdas. This is pure noise for an LLM.

**Impact**: 50% of output is useful DI info, 50% is verbose noise.

### Related types / Types by namespace — ⚠️ Adequate

For web projects (eShop, TodoApi): 5-layer grouping is useful. But lines are truncated at 2000 chars.

For library mode (AutoMapper): **Huge improvement** from flat wall. Namespace summary with public type counts and top-10 listing is actionable. 88 lines total.

### Diagnostics — ✅ Good

Compact, shows relevant warnings (CallReachabilityPruner note). Pruning notes list what was removed.

### Footer — ✅ Good

Compression savings visible. Schema version.

---

## 2. Scenario Differentiation — ⚠️ Near Zero

**Problem**: All scenarios produce nearly identical output. The only difference is the Indirect wiring subsection appearing only in `debug-endpoint` and `harden-di`.

| Scenario | Should emphasize | Actually differs |
|---|---|---|
| `architecture` | Projects, signals, layers | — |
| `debug-endpoint` | Call graph, endpoint detail | +Indirect wiring |
| `add-similar-feature` | Entry point, related types | — |
| `modify-middleware` | Middleware pipeline | — |
| `trace-message-flow` | Event bus, message handlers | — |
| `harden-di` | Indirect wiring, service locator | +Indirect wiring |

**Root causes**:
1. `MarkdownRenderer` always emits all sections — ignores scenario `RequiredSections`
2. `MaxSurvivingTypes` per scenario not enforced by pruners
3. TokenBudgetEnforcer uses token budget, not type count limit

---

## 3. Data Quality by Section

| Section | eShop (365 lines) | TodoApi (109 lines) | AutoMapper (88 lines) |
|---|---|---|---|
| Header | ✅ | ✅ | ✅ |
| Architecture | ✅ 24 projects | ✅ 7 projects | ✅ 6 projects |
| Endpoints | **43 rows** — comprehensive but has noise | **11 rows** — clean | N/A |
| Middleware | ⚠️ duplicate entries | ✅ clean | N/A |
| DI registrations | **⚠️ 50% noise** — delegate bodies + `?` | ✅ compact | N/A |
| Related types | ⚠️ truncated at 2K chars | ✅ compact | ✅ **well-structured** |
| Compression | ✅ 12%+1%+26% savings | ✅ 1%+5%+11% | ✅ 21%+1%+27% |

---

## 4. Most Actionable Fixes

| Priority | Fix | Impact | Effort |
|---|---|---|---|
| **P1** | **DI registration output noise** — Filter out `?` entries and truncate delegate bodies. Show "Source: {file}:{line}" instead of raw lambdas. | eShop: removes ~80 lines of noise (cuts output by 20%) | ~1h |
| **P1** | **Scenario header** — Use `ActiveScenario.DisplayName` instead of hardcoded "Architecture Overview" | All scenarios correctly labeled | ~10 min |
| **P1** | **Enforce `MaxSurvivingTypes` per scenario** — Add a pruner pass that respects the scenario's type limit after token budget enforcement | Scenarios actually differ in output volume | ~1h |
| **P2** | **`RequiredSections` enforcement** — Only render sections that match the scenario's requirements | `debug-endpoint` no longer shows DI registrations table, `modify-middleware` hides endpoints, etc. | ~2h |
| **P2** | **Deduplicate middleware** — Merge same-name middleware from different services into one row with count | eShop: deduplicates `UseExceptionHandler` × 2, `UseDefaultOpenApi` × 2 | ~30min |
| **P3** | **Call graph in focused profile** — Add lightweight call graph for Focused profile (not just Debug) | `debug-endpoint` and `trace-message-flow` actually show something useful | ~2h |
| **P3** | **Route source filter** — Hide framework/internal routes (`GET /` OpenAPI redirect) | eShop: removes 1 noisy route | ~15 min |

---

## 5. Summary

**What's working well**:
- Endpoint tables are comprehensive and useful
- MediatR handler detection is correct
- Library-mode namespace summary is a huge improvement
- Compression and pruning are effective
- `--metrics` output is informative

**Biggest usability gap**: Scenarios don't actually change the output in meaningful ways. A user running `--scenario debug-endpoint` gets essentially the same output as `--scenario architecture`. The section-level tailoring (`RequiredSections`) and pruning-level differences (`MaxSurvivingTypes`) are defined in configuration but never enforced by the renderer or pruners.

**Biggest quality gap**: DI registration output is noisy — delegate bodies as inline C# make up 50% of the eShop output with marginal value for LLM consumption.

**Actionable output size reduction**: Filtering DI noise + enforcing scenario sections could reduce eShop output from 365 lines to ~150-200 lines without losing useful information.
