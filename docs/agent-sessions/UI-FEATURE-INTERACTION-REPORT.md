# DevContext — UI Feature & Profile Interaction Report

**Date**: 2026-06-11

---

## 1. Three Scenarios — What They Do

| Scenario | Sections Produced | Pruning | Best For |
|---|---|---|---|
| **overview** | Architecture overview, Endpoints, MediatR Handlers, Data Model, Non-obvious wiring, Related types | Max 40 types, path distance 2 | New team members, codebase overview, feature planning, PR context |
| **deep-dive** | Endpoints, Call graph, MediatR Handlers, Data Model, Message consumers, Non-obvious wiring | Max 25 types, path distance 1, call depth 5 (tighter focus, deeper graph) | Debugging an endpoint, tracing event flow, finding anti-patterns |
| **audit** | Architecture overview, Non-obvious wiring, Related types | Max 50 types, pattern boost enabled | DI hardening, wiring audit, middleware inspection, service locator hunting |

---

## 2. Three Profiles — What They Gate

Profiles control **only two things**: call graph extraction and source code extraction. Everything else (endpoint detection, DI analysis, anti-patterns, signals) runs identically at all profiles.

| Profile | Call Graph | Source Code | Token Budget Impact | When to Use |
|---|---|---|---|---|
| **focused** (default) | ❌ Skipped | ❌ Skipped | Fastest — minimal overhead | Quick overview, CI runs, first pass |
| **debug** | ✅ BFS tree depth 5 | ❌ Skipped | Add 400–2,000 tokens | Debugging flow, finding anti-patterns, understanding handler chains |
| **full** | ✅ BFS tree depth 5 | ✅ Full C# source bodies | Add 2,000–12,000 tokens | Preparing rich LLM context, deep code review |

**Note**: Anti-pattern detection is a **separate toggle** (opt-in, disabled by default). It's not gated by profile. When enabled, it adds ~2,000–6,000 tokens to the output.

---

## 3. How Each Scenario × Profile Combination Behaves

### 3a. overview (architecture + add-similar-feature merged)

| Profile | What You Get | Missing vs Higher Profile |
|---|---|---|
| **focused** | 6 sections: Architecture tree, Endpoints table, MediatR Handlers, Data model, DI registrations + middleware, Related types by layer | Call graph, source code |
| **debug** | Focused + Call graph section (BFS from entry points) | Source code |
| **full** | Debug + Source code (full C# for entry point + 5 call chain types) | Nothing |

**Default flow**: `overview + focused` → gets a comprehensive architecture picture in ~3,000–5,000 tokens. Good for 80% of use cases.

### 3b. deep-dive (debug-endpoint + trace-message-flow merged)

| Profile | What You Get | Missing vs Higher Profile |
|---|---|---|
| **focused** | 6 sections: Endpoints, MediatR Handlers, Data model, Message consumers, DI registrations + middleware, Indirect wiring | Call graph, source code |
| **debug** | Focused + Call graph (enables `--around Symbol:Method` tracing) | Source code |
| **full** | Debug + Source code | Nothing |

**Default flow**: `deep-dive + debug` → traces a specific handler through BFS depth-5, shows anti-patterns on each node. Best when debugging with `--around Controller:Action`.

### 3c. audit (modify-middleware + harden-di merged)

| Profile | What You Get | Missing vs Higher Profile |
|---|---|---|
| **focused** | 3 sections: Architecture overview, DI registrations + middleware + indirect wiring, Related types | Call graph, source code |
| **debug** | Focused + Call graph | Source code |
| **full** | Debug + Source code | Nothing |

**Default flow**: `audit + focused` → scans for service locators, reflection activation, captive dependencies. Call graph is less relevant for this scenario (it's about DI structure, not code flow).

---

## 4. UI Flow — What Happens When You Click "Analyse"

```
Phase 1: Source resolution
  ├── Local path → ProjectRootResolver (finds .sln, .csproj, or folder)
  └── GitHub URL → GitCloneService (clone → validate → local path)

Phase 2: Engine runs (all profiles)
  ├── Stage 1: File tree, solution, project structure
  ├── Stage 2: Parallel extraction (dependencies, syntax, DI, middleware)
  ├── Signal sealing
  ├── Stage 3: Specific extractors (endpoints, handlers, entities)
  ├── (Call graph — Debug/Full only)
  ├── (Source bodies — Full only)
  └── (Anti-patterns — only if toggled on)

Phase 3: Pruning + Compression
  ├── Prune types by path proximity, call reachability, pattern relevance
  ├── Token budget enforcement (MaxSurvivingTypes per scenario)
  └── Compression (trivial members, boilerplate, dedup, grouping)

Phase 4: Render
  ├── Markdown or JSON
  └── Token accounting (if --token-view)

Phase 5: Desktop post-processing
  ├── PopulateSections (split by ## headers)
  ├── Section grouping (API/Architecture/Data/Analysis/Debug)
  └── Budget bar, Human/LLM view toggles
```

---

## 5. Token Budget Per Scenario × Profile (Real Numbers from OrchardCore)

| Scenario | Profile | Tokens | Active Types | Time |
|---|---|---|---|---|
| overview | focused | 9,368 | 30 | 38.8ms |
| deep-dive | debug | 14,239 | 20 | 26.5ms |
| overview | full | 9,341 | 25 | 31.0ms |
| audit | focused | 11,020 | 46 | 21.6ms |

**With anti-patterns disabled (default)**: All runs under 4,000 tokens. Anti-patterns alone consumed 6,262 tokens (43–67%) in the OrchardCore test — hence opt-in.

---

## 6. Why 3 Profiles? (Answer to Your Question)

The 3 profiles are **orthogonal** — each adds exactly one capability:

```
focused (base) → +Call Graph = debug → +Source Code = full
```

Users don't need to understand "token budgets" or "BFS depth" to pick. The question is simple:

1. **"I just need an overview"** → `focused`
2. **"I need to trace a handler flow"** → `debug`
3. **"I need full source code for an LLM"** → `full`

The profiles don't interact with scenarios — they add capabilities *on top of* whatever scenario you picked. So `overview + full` gives you architecture + source code. `audit + focused` gives you DI audit without call graphs. `deep-dive + debug` gives you endpoint tracing with BFS trees.

---

## 7. What's Missing from the UI (User Confusion Points)

| Issue | Impact |
|---|---|
| Profile names don't explain themselves | "Focused" means base extraction. "Debug" doesn't mean developer tools — it means call graph. Users don't know this |
| Scenario names are abstract | "Overview" is clear. "Deep Dive" is vague. "Audit" is clear. But no tooltips or hover text explain the 6 sections each produces |
| Anti-pattern toggle is hidden in Advanced | Users don't discover it. Should be more prominent |
| No preview of token cost per combination | User has to run to see how many tokens each scenario+profile uses |
| Budget bar shows totals but not per-section breakdown | Per the original report's recommendation |

---

## 8. Recommendation

**Keep 3 profiles × 3 scenarios** — the matrix is small (9 combinations) and each adds value. The fix isn't removal — it's **better labeling**:

| Current Label | Suggested Label | Tooltip |
|---|---|---|
| **focused** | Standard | Fast extraction with pruning. No call graph or source code. |
| **debug** | With Call Graph | Adds BFS call tree from entry points. Use to trace handler flow. |
| **full** | With Source Code | Adds full C# source bodies. Best for preparing LLM context. |
