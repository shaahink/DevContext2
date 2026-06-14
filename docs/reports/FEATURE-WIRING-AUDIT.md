# Feature Wiring Audit — CLI & Desktop (MainViewModel)

> Deep review of every user-facing option exposed from the CLI (`AnalyzeSettings`) and the
> desktop (`MainViewModel` + Razor panels), tracing each one through the processor / extractor /
> pruner / renderer it is supposed to drive. Focus: bugs, wrong wiring, dead paths, edge cases,
> and "does the feature deliver the value it promises".
>
> Date: 2026-06-14 · Branch: `feature/desktop-live-update-gating`
> Scope read: `AnalyzeSettings`, `AnalyzeCommand`, `AnalysisIntentResolver`, `ScenarioRegistry`,
> `Scenario`, `DiscoveryPipeline`, `RenderPlanBuilder`, all 4 pruners, `ServiceRegistration`,
> `MainViewModel`, `AnalysisService`, `SectionSelectionModel`, `OutputViewModel`,
> `SectionViewModel`, `ConfigPanel.razor`, `OutputPanel.razor`, `CallGraphExtractor`,
> `SourceBodyExtractor`, `FocusPointResolver`, `MarkdownRenderer` (dispatch), `SnapshotCache`,
> `PLAN-8`.

---

## 0. How options are *supposed* to flow

```
CLI flag / Desktop control
   └─ IntentInput (Focus, Depth, ExplicitScenario, ExplicitProfile)
        └─ AnalysisIntentResolver.Resolve  →  ResolvedIntent { Scenario, Profile, FocusPoints }
             └─ ExtractionOptions (analyze-time)        ─┐
             └─ DiscoveryContext.ActiveScenario          ├─ DiscoveryPipeline.AnalyzeAsync → AnalysisSnapshot
             └─ SharedAnalysisContext.FocusPoints       ─┘     (extract → seal signals → Stage3 → score → compress)
                  └─ RenderRequest (render-time: Format, MaxTokens, Sections, Provenance, Diagnostics, TokenView)
                       └─ RenderPlanBuilder.Build → RenderPlan (included types, budget, cap, per-type cap)
                            └─ renderer (Markdown/Json/Html)
```

The PLAN-1 split is real and mostly clean: **analyze once → render many**. Budget and the
type cap are enforced **only** at render time in `RenderPlanBuilder` (sort by `FinalScore`,
drop by budget, drop by `MaxSurvivingTypes`). The bugs below are mostly at the *edges* where an
option is declared but its wire is cut, or where two parallel mechanisms drifted apart.

Severity legend: **P1** broken/ misleading for a normal user · **P2** wrong under a reachable
path or latent trap · **P3** cleanliness / minor / docs.

---

## 1. Findings (ranked)

### F1 — P1 — Desktop "Sections" drawer does not filter the output (only the token counter)

There are **two independent section systems** and only one of them is wired to the renderer:

* **System A (the real filter):** `SectionSelectionModel.Sections` (`List<SectionToggle>`) →
  `GetActiveSections()` → `RenderRequest.Sections` → `RenderPlan.Sections` →
  `MarkdownRenderer.ShouldRender(...)` (`MarkdownRenderer.cs:548-551`). This is what actually
  decides which `## sections` appear in the output.
* **System B (the drawer UI):** `SectionSelectionModel.SectionGroups` of `SectionViewModel`
  with `IsIncluded`, built from the rendered `SectionStat`s in `BuildSectionDataFromStat`. This
  is the only section UI the user can actually click (`OutputPanel.razor:142-158`).

Toggling a checkbox in the drawer sets `SectionViewModel.IsIncluded` (`OutputPanel.razor:152`),
which fires `RecalcTokenTotal()` + `OnSectionChanged` → `RebuildLlmViewText()`. But:

* `RebuildLlmViewText()` sets `_output.LlmViewText = _output.RawContent` (`MainViewModel.cs:89-93`)
  — i.e. the **full** content, ignoring `IsIncluded`.
* `BuildSectionDataFromStat` builds each `SectionViewModel` with `FullText = ""`
  (`SectionSelectionModel.cs:168`), so even the old "join included FullText" path
  (`:204-205`) would yield an empty string. Its result is discarded with `_` in
  `RerenderAsync` (`MainViewModel.cs:438`).

**Net effect:** unchecking "DI registrations" (or any section) in the drawer changes the token
tally and the budget bar (`OutputPanel.razor:135-139`) but **does not remove the section** from
the Human view, the LLM view, Copy, Copy-for-LLM, or Save. The budget bar and the
`Analyze (~N tok)` button (`MainViewModel.cs:99`) therefore display a number the produced output
does not honor. This looks like a regression introduced when the LLM view was repointed to raw
content (commit `7aed1b5`, "LLM view shows raw markdown … instead of empty section-built text").

> The only live path into System A, `MainViewModel.SetSectionEnabled` → `OnRenderInputChanged`,
> would correctly re-render with the new section set — but **nothing in any `.razor` calls it.**

### F2 — P1 — There is no live UI to choose real sections or enable Source code

`ConfigPanel.razor` has a `@* ── Sections ── *@` header (`:112`) with **no controls under it**
— the section checkboxes were removed and never replaced. So System A (the real filter) can only
be set by:

* scenario defaults (`ApplyScenarioSectionDefaults`, on scenario switch), and
* persisted settings at startup (`LoadSectionDefaults`).

Consequences:

* The user cannot turn any individual section on/off at runtime (the drawer that *looks* like it
  does is F1).
* The **"Source code"** toggle (`__source__`, hint *"adds full C# bodies, +2k–12k tokens"*) is
  unreachable: it's excluded from `GetActiveSections` (`SectionSelectionModel.cs:96`), it's a
  sentinel so it never appears as a rendered `SectionStat` in the drawer, and `SetSectionEnabled`
  has no caller. The feature can only be switched on by hand-editing `settings.json`.

### F3 — P1/P2 — `--focus` / `--around` are documented "Repeatable" but only the first is used

`AnalyzeSettings.Focus`/`Around` are `string[]` and the help says *"Focus point. Repeatable."*
But `AnalyzeCommand` collapses them to one value:

```csharp
var focusInput = settings.Focus ?? settings.Around;          // AnalyzeCommand.cs:76
var focusText  = focusInput is { Length: > 0 } ? focusInput[0] : null;  // :77
... Focus = focusText ?? settings.Task                        // :81  (single string)
```

`IntentInput.Focus` is a single `string`, so `-f A -f B` silently drops `B`. The rest of the
pipeline (`SharedAnalysisContext.FocusPoints` is an array, every pruner loops over all focus
points) already supports multiple — only the intake is throttled to one. Either honor multiple
focus points or drop "Repeatable" from the help and reject extras.

### F4 — P2 — Endpoint focus (`-f "GET /route"`) is parsed but never resolved to a handler

`AnalysisIntentResolver` recognizes endpoint syntax and emits
`new FocusPoint(FocusKind.Endpoint, "", TypeName: null, …, Route)` and the explanation string
*"Slicing from GET /route — handler resolved after scan."* (`AnalysisIntentResolver.cs:87,108`).
But **no resolution ever happens:**

* `FocusPointResolver.Resolve` only handles `FocusKind.Type`/`Method`; endpoint focus points
  pass through unchanged (`FocusPointResolver.cs:14,29-32`), so `TypeName` stays `null`.
* `ResolveFocusPoints` runs **before** Stage 3 (`DiscoveryPipeline.cs:99` vs Stage 3 at `:103`),
  i.e. before any `EndpointDetection` exists, and there is no second resolution pass afterward.
* `FocusKind.Endpoint` appears nowhere else except the explanation string (verified by grep).

Downstream, a `TypeName == null` focus produces **zero** signal: `PathProximityPruner` has no
resolved file path and skips the name fallback (`PathProximityPruner.cs:17,39`),
`CallReachabilityPruner` builds an empty seed set (`CallReachabilityPruner.cs:21-31`), and
`RenderPlanBuilder` pins nothing (`RenderPlanBuilder.cs:21`). So `-f "GET /api/orders"` only
flips the scenario to deep-dive and toggles the focus-gated render sections on — it does **not**
slice toward the handler, despite the message saying it does. This is a half-wired PLAN-2
headline feature; the explanation actively misleads.

### F5 — P2 — Dead pruner duplicating render logic with a broken score

`TokenBudgetEnforcer` is **not registered** — `ServiceRegistration.cs:28-30` registers only
`PathProximityPruner`, `CallReachabilityPruner`, `PatternRelevancePruner`. So it never runs, and
by consequence:

* `TypeDiscovery.RelevanceScore` is **never assigned** anywhere (only read, inside the dead
  enforcer at `TokenBudgetEnforcer.cs:18,55`).
* `TypeDiscovery.IsPruned` is **never set** to true (only `TokenBudgetEnforcer` set it), so the
  `if (type.IsPruned …) continue;` guards in all six compression strategies are permanently
  false — compression is gated by `IsHardExcluded` alone.

This is currently harmless but it is a **trap**: the file sorts candidates by
`PathProximityScore + RelevanceScore` (an always-zero term) and runs *before* `FinalScore` is
computed (`DiscoveryPipeline.cs:455-467`). If a future maintainer re-registers it "to enforce the
budget", it would set `IsPruned` by a meaningless score, silently removing those types from
**compression** while `RenderPlanBuilder` (which ignores `IsPruned`) still **includes** them —
producing un-formatted / un-truncated bodies in the output. Recommend deleting
`TokenBudgetEnforcer`, `RelevanceScore`, and `IsPruned`, or rewriting the enforcer to call the
exact `RenderPlanBuilder` ordering. (Matches the "MaxSurvivingTypes enforced twice" note in
project memory — it is now enforced once, at render; the analyze-time copy is dead.)

### F6 — P2 — Clone cleanup modes `session` and `24h` are accepted but unimplemented

Both entry points only act on `"auto"`:

* CLI: `if (cleanup == "auto") GitCloneService.Cleanup(...)` (`AnalyzeCommand.cs:210-212`).
* Desktop: `if (... CloneCleanup == "auto") ... Cleanup(...)` (`MainViewModel.cs:377-382`).

`AnalyzeSettings` advertises `auto | session | 24h | keep` and the desktop **defaults to `24h`**
(`MainViewModel.cs:118`, with a "Cache 24h" option in `ConfigPanel.razor:59`). But `session`,
`24h`, and `keep` all behave identically (nothing is ever cleaned, and there is no 24h-expiry or
session-end sweep at the call sites). So the default "Cache 24h" implies a cache lifetime that
isn't enforced. **Verify** in `GitCloneService` whether a deterministic `ClonePath` + age check
exists; from the call sites it does not. Either implement the modes or collapse them to
`auto | keep`.

### F7 — P2 — "Source code" / "Entry points" only render when a focus point exists

`AppendSourceBodies` and `AppendEntryPoints` both early-return on empty focus
(`MarkdownRenderer.cs:280` and `:374`: `if (options.FocusPoints.IsDefaultOrEmpty …) return;`).
Source bodies are *not* gated by `ShouldRender(SectionNames.SourceCode)` at all — they're gated
by focus presence. So enabling Source code (profile→`full`, `SourceBodyExtractor` runs at
`SourceBodyExtractor.cs:24-25`) yields **no source in the output** unless a focus point is also
set. Combined with F2 (endpoint focus = no real focus) and F8 below, the path to actually seeing
source bodies is narrow and undocumented.

### F8 — P2 — Desktop profile is derived from section toggles but is not in the cache key

`DerivedProfile` is computed from the *CallGraph* and *__source__* section toggles
(`SectionSelectionModel.cs:71-81`: source→`full`, callgraph→`debug`, else `focused`) and is an
**analyze-time** input (it gates `CallGraphExtractor`/`SourceBodyExtractor` via `ShouldRun`).
But `AnalysisKey` omits profile entirely:

```csharp
new(ProjectPath, SelectedScenario.Value, Around, NoRoslyn, DryRun, IncludeAntiPatterns) // MainViewModel.cs:102
```

Today this is *masked* because, with no live section UI (F2), profile is a deterministic
function of scenario (overview→focused, deep-dive→debug via its default sections), and scenario
*is* in the key. But it is a latent correctness trap that goes live the moment either (a) the
section UI from F2 is restored, or (b) persisted `LastActiveSections` enables `__source__`/
CallGraph independent of scenario: a `focused`/`debug` snapshot will be served from cache when
`full`/`debug` was requested, so the call graph or source silently goes missing and the
`IsStale` banner never fires (it uses the same key). **Fix this together with F1/F2** — add the
derived profile to `AnalysisKey`, and route the profile-affecting section toggles through
`MarkAnalysisInputsChanged()` (analysis tier), not the render tier. (PLAN-8's input-tier table
classifies *all* section toggles as render-tier, which is the blind spot; PLAN-8 explicitly says
to *flag* such pre-existing render/analyze misclassifications, not fix them in that plan.)

### F9 — P3 — Sections that render unconditionally bypass the section filter

In the main dispatch, `AppendArchitecture`/`AppendSignals`/`AppendProjects`
(`MarkdownRenderer.cs:24-26`), `AppendAntiPatterns` (`:110`) and `AppendEventFlow` (`:114`) are
called **without** a `ShouldRender` guard, unlike every other section (`:31-120`). Anti-patterns
and event flow therefore always appear when their detections exist, and aren't user-toggleable
(neither is in `SectionSelectionModel.Sections`). Architecture/Signals/Projects always render
even if the user selected zero sections. Minor, but it makes the section model inconsistent and
the token accounting harder to reason about.

### F10 — P3 — `--task` (deprecated) silently degrades to a junk focus point

`Focus = focusText ?? settings.Task` (`AnalyzeCommand.cs:81`). A free-text task like
`--task "fix the login bug"` has no `/` and no HTTP verb, so it's parsed by
`ParseTypeOrMethodFocus` as a **bare type name** `"fix the login bug"`
(`AnalysisIntentResolver.cs:152-156`), which never resolves → "type not found" diagnostic +
folder-proximity fallback, and flips the scenario to deep-dive. So a deprecated free-text intent
now produces a misleading focused slice instead of an overview. Prefer ignoring `--task` for
focus (keep only the deprecation warning) or routing it to overview.

### F11 — P3 — `AnalysisService` caches one pipeline across all projects; `rootPath` is unused

`AnalysisService.GetPipeline` memoizes `_cachedPipeline` on first call and ignores subsequent
`rootPath` (`AnalysisService.cs:45-55`). This is safe *only because* `AddDevContextServices`
never uses its `rootPath` parameter (`ServiceRegistration.cs:5` — `rootPath` is dead). It's a
latent footgun: the first person to make a service depend on `rootPath` will get the
first-analyzed project's root for every later project in a desktop session. Remove the dead
parameter or key the pipeline cache by root.

### F12 — P3 — GitHub URL validation fires per keystroke

`ProjectPath` is bound `@bind:event="oninput"` (`ConfigPanel.razor:50`) →
`OnProjectPathChanged` → `ValidateGitHubUrlAsync` (`MainViewModel.cs:120-165`), which issues a
network `ValidateAsync` (git `ls-remote`) on each change. It's throttled by cancellation
(`_validateOp.Cancel()`), so only the settled value completes, but it still spawns a request per
pause while typing a URL. The focus field was already calmed to `onchange` per PLAN-8 Phase 5;
the path field was not. Consider a small debounce or `onchange`.

### F13 — P3 — Render does two full passes per re-render (markdown + html)

`AnalysisService.RenderAsync` renders markdown, then renders **again** as HTML for the Human
view whenever format is markdown (`AnalysisService.cs:176-181`). Render is cheap by design, so
this is acceptable, but every slider tick / provenance toggle pays 2× render. Worth noting if
render ever stops being cheap (e.g. large source bodies).

---

## 2. Per-option wiring matrix

| Option (CLI / Desktop) | Drives | Tier | Status |
|---|---|---|---|
| `PATH` / Project path | root resolve, clone | analyze | OK (path validate per-keystroke, F12) |
| `--focus` / Focus(`Around`) | FocusPoints → pruners, pins, focus-gated sections | analyze | **F3** (only first used), **F4** (endpoint never resolved) |
| `--around` | alias of `--focus` | analyze | OK (same F3) |
| `--depth` | clamps `MaxCallDepth` + `MaxPathDistance` | analyze | OK; desktop doesn't expose it (`AnalysisService.cs:70`) |
| `--scenario` / Mode | scenario (overview/deep-dive), section defaults | analyze | OK (`trace`→deep-dive, `audit`→overview+warn) |
| `--profile` / *(derived)* | extractor gating (Debug→callgraph, Full→source) | analyze | desktop derives from section toggles; **F8** (not in cache key) |
| `--max-tokens` / Token budget | `RenderPlan` budget | render | OK (desktop debounced render) |
| `--format` / Format | renderer selection | render | OK (html accepted but undocumented in help) |
| `--include-provenance` | provenance in output | render | OK |
| `--include-diagnostics` | diagnostics section | render | OK |
| `--include-anti-patterns` | `AntiPatternDetector` + always-rendered section | analyze | OK (renders unconditionally, F9) |
| Sections (desktop drawer) | *should* filter output | render | **F1** (only token math), **F2** (no real UI) |
| `--no-roslyn` / Skip Roslyn | `NullRoslynProvider`, no call graph | analyze | OK |
| `--dry-run` / Dry run | plan-only snapshot | analyze | OK |
| `--cleanup` / cleanup select | clone cleanup | post | **F6** (session/24h/keep all = keep) |
| `--keep` | cleanup=keep | post | OK |
| `--ref` | branch/tag checkout | analyze | OK (desktop uses `repo.Ref`) |
| `--strict` | exit 2 on self-check fail | post | OK (CLI only) |
| `--metrics` / `--stats` | RunReport tables | post | OK (aliases) |
| `--token-view` | per-section token table in output | render | OK (desktop hard-codes `TokenView:false`, `MainViewModel.cs:429`) |
| `--task` | deprecated free-text → focus | analyze | **F10** (degrades to junk focus) |

---

## 3. What's genuinely solid (so we don't "fix" it)

* The PLAN-1 analyze→render split: budget + `MaxSurvivingTypes` cap enforced once, at render
  (`RenderPlanBuilder`), sorted by `FinalScore`. Pins (explicit focus types) are exempt from
  hard-exclusion and counted first.
* Scenario→profile derivation and the `trace`/`audit` aliases.
* Render-tier toggles (format, provenance, diagnostics, max-tokens) correctly re-render without
  re-analysis — this is the working core of the live-update feature.
* `SnapshotCache` LRU itself (move-to-front, evict-at-capacity) is correct; the bug is what the
  **key** contains (F8), not the cache.
* Signal-gated Stage 3 extractors and the extractor validation warnings.

---

## 4. Suggested fix order

1. **F1 + F2 together** (desktop sections): decide one model. Either (a) wire the drawer's
   `IsIncluded` back to System A (`SetSectionEnabled(key)` → re-render) and restore a section
   list in `ConfigPanel`, or (b) drop the drawer's filtering pretense and relabel it a token
   preview. (a) is the honest fix and restores the advertised feature.
2. **F8** (cache key + profile tier) — fold in while touching F1/F2 so section→profile changes
   re-analyze instead of serving a stale snapshot.
3. **F3** (multi-focus intake) and **F4** (endpoint→handler resolution) — both are about the
   focus pipeline; do together.
4. **F6** (cleanup modes), **F5** (delete dead enforcer + dead fields), then the P3 polish.

See `FEATURE-WIRING-AGENT-BRIEF.md` for the executable task list.
