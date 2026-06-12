# Plan 2 — Unified Focus + Depth UX

> Implements P4 (and part of P6) of `docs/DESIGN-PHILOSOPHY.md`. Depends on Plan 1's types
> (`AnalysisSnapshot`, `RenderRequest`) but can be executed before Plan 3.
> Goal: the user expresses **Focus** (optional: a type/method, or free text that names one)
> and **Depth** (overview ↔ deep). Scenario, profile, sections, and pruning config are
> *derived*, and the derivation is always shown. One resolver in Core, used by both CLI and
> desktop — today's duplicated remapping logic disappears.

## Ground rules

- Branch off the Plan 1 result. Build + test per phase. Razor comments are `@* *@`.
- Backward compatibility: every existing CLI flag keeps working this release. `--scenario`,
  `--profile`, `--around`, `--task` are not removed — they become inputs to the resolver.
  `trace`/`audit` remain silent aliases. Deprecation messaging only, no breakage.
- The `__source__` sentinel ≠ a `SectionNames` entry (see Plan 1 ground rules).

## Phase 0 — Recon

Read: `src/DevContext.Core/Configuration/IntentInferrer.cs`, `ScenarioRegistry.cs`,
`src/DevContext.Core/Resolvers/FocusPointParser.cs`, `FocusPointResolver` (in Pipeline or
Resolvers — locate it), `StringHelpers.LevenshteinDistance`,
`AnalyzeCommand.ResolveScenarioAndProfile` + `BuildSharedAnalysis`,
`AnalysisService.AnalyzeAsync` lines that remap scenario/profile,
`MainViewModel` scenario/section-defaults members (`ApplyScenarioSectionDefaults`,
`SelectedScenario`, `IsTraceMode`, `DerivedProfile`), `Components/ConfigPanel.razor`.

## Phase 1 — `AnalysisIntentResolver` in Core

New file `src/DevContext.Core/Configuration/AnalysisIntentResolver.cs`:

```csharp
public sealed record IntentInput
{
    public string? Task { get; init; }          // free text ("why does checkout 500?")
    public string? Focus { get; init; }         // explicit TypeName[:Method] (from --around / UI picker)
    public string? ExplicitScenario { get; init; }  // expert override (--scenario)
    public string? ExplicitProfile { get; init; }   // expert override (--profile)
}

public sealed record ResolvedIntent
{
    public required Scenario Scenario { get; init; }
    public required ExtractionProfile Profile { get; init; }
    public required ImmutableArray<FocusPoint> FocusPoints { get; init; } // unresolved; pipeline resolves
    public required ImmutableArray<string> FocusCandidateNames { get; init; } // mined from Task text
    public required string Explanation { get; init; }   // human sentence, always shown
    public ImmutableArray<string> Warnings { get; init; } = [];
}

public static class AnalysisIntentResolver
{
    public static ResolvedIntent Resolve(IntentInput input, IFileSystem fs);
}
```

Resolution rules, in order (encode exactly; these are the product spec):

1. **Aliases:** `trace` → `deep-dive`, `audit` → `overview` (warning: "'audit' is deprecated,
   using overview"). Unknown scenario → error surface (return is fine; callers decide exit).
2. **Focus collection:** explicit `Focus` parses via `FocusPointParser`. Additionally, mine
   `Task` text for focus candidates: tokens matching `[A-Z][A-Za-z0-9_]{2,}` (PascalCase
   identifiers), minus a stop-list of common words (`I`, `The`, `Why`, HTTP verbs, …). These go
   into `FocusCandidateNames` only — they are *resolved against the model after Stage 2* (see
   Phase 2), not trusted blindly.
3. **Depth/scenario:** `ExplicitScenario` wins if set. Else: any focus (explicit or mined) →
   `deep-dive`; else `IntentInferrer` keyword rules on `Task` (keep the existing rule table —
   move it inside the resolver, keep `IntentInferrer.Infer` as a delegating shim for tests);
   else `overview`.
4. **Profile:** `ExplicitProfile` wins. Else derive: `deep-dive` → `Debug` (call graph is the
   point); `overview` → `Focused`. (`Full` is only ever explicit or section-derived in the UI.)
5. **Explanation:** one sentence, e.g.
   `Interpreted as: Trace around OrderService (from task text), call graph on, ~25 types max.`
   or `Interpreted as: Overview (no focus given).` Include a hint when it matters:
   deep-dive without any focus → warning `"deep-dive without a focus behaves like overview —
   add --around TypeName or name a type in --task"`.

Unit tests (new file in `DevContext.Core.Tests`): one test per rule above plus alias cases,
PascalCase mining cases (positive + stop-list), and explanation snapshot cases. ≥ 12 tests.

## Phase 2 — Task-derived focus resolution in the pipeline

`DiscoveryPipeline` already resolves `UnresolvedFocusPoints` after Stage 2 via
`FocusPointResolver.Resolve` with Levenshtein "did you mean". Extend:

1. `SharedAnalysisContext` gains `ImmutableArray<string> FocusCandidateNames`.
2. After Stage 2 (same place focus points resolve today): for each candidate name, exact-match
   against `model.Types` (then case-insensitive, then Levenshtein ≤ 2). Matches become real
   `FocusPoint`s appended to `Analysis.FocusPoints`; non-matches produce an Info diagnostic
   (`"task mentioned 'OrderServce' — no matching type; nearest: OrderService"`), not a warning
   spam (task text legitimately contains non-type PascalCase words).
3. If candidates produced ≥ 1 focus point and scenario was inferred as `overview` *only because
   no focus was known at resolve time*, do **not** silently flip scenario mid-run — instead add
   diagnostic `"task names OrderService — consider --scenario deep-dive"`. (Keeps the run
   deterministic w.r.t. the printed Explanation.)

## Phase 3 — CLI adoption

`AnalyzeCommand`:

1. Delete `ResolveScenarioAndProfile`; build `IntentInput` from settings
   (`Task`, first `--around` stays `Focus`-equivalent — pass all `--around` values through
   `FocusPointParser` as today and the *resolver* only handles the scenario/profile/derivation
   logic) and call `AnalysisIntentResolver.Resolve`.
2. Print `resolved.Explanation` before the status spinner (dim style:
   `AnsiConsole.MarkupLine($"[dim]{...}[/]")`), and each `resolved.Warnings` in yellow.
3. Add `--focus` as the documented alias of `--around` in `AnalyzeSettings` (Spectre supports
   multiple option names on one property: `[CommandOption("--focus|--around")]` — verify
   against the existing attribute usage and keep `--around` listed in help as deprecated).
4. Help text (`AnalyzeSettings` descriptions): `--scenario`/`--profile` get
   `"(advanced — usually derived from --focus/--task)"` suffixes.
5. `ScenariosCommand`: drop `trace` from the listing (it's an alias), show `deep-dive` with
   display name `Trace` and a `"requires a focus point"` note.

## Phase 4 — Registry cleanup

`ScenarioRegistry`: delete the duplicated `"trace"` entry (the alias now lives in the resolver).
Grep for `"trace"` string literals across the solution first — tests, desktop scenario lists,
docs — and update each. `AnalysisService` deletes its own remapping lines (`audit`→`overview`,
`trace`→`deep-dive`) and calls the resolver instead — this removes the CLI/desktop duplication.

## Phase 5 — Desktop adoption

`ConfigPanel.razor` + `MainViewModel`:

1. The Intent text field and Entry-point field merge conceptually into a **question row**:
   one text input "What do you want to know?" (binds to `Task`) and a **focus picker** input
   (binds to `Around`) with autocomplete: after any analysis, `_snapshot.Model.Types` provides
   type names; expose `ImmutableArray<string> KnownTypeNames` on the VM (top ~500 by
   `FinalScore`, alphabetical) and wire a simple `<datalist>`-style suggestion dropdown in
   Razor (no JS library — keep it dependency-free).
2. Mode toggle (Overview/Trace) stays, but becomes a *display of the derived depth* that the
   user can override: when the resolver derives a different value than the toggle shows,
   update the toggle and show the Explanation line under the question row (small, dim,
   monospace — this is the P3 transparency string). VM: replace scenario-resolution code with
   `AnalysisIntentResolver.Resolve` and bind `IntentExplanation` (new `[ObservableProperty]`).
3. Section checkboxes remain the expert layer (unchanged behavior; with Plan 1 they're
   render-side anyway). Profile remains fully derived (no UI control) — if any
   `SelectedProfile`/`IsProfileX`/`SetProfileCommand` members still exist, grep all `.razor` +
   tests before removing.
4. Settings: `AppSettings.LastScenario`/`LastProfile` keep loading (back-compat with existing
   settings.json on disk) but saving writes the derived values.

Desktop tests: update VM tests for the new derivation path; add tests:
"task naming a type flips mode to Trace and sets explanation",
"explicit mode override sticks", "focus autocomplete list populates after analysis".

## Phase 6 — Verification

- Unit: resolver tests green; full `dotnet test`.
- CLI matrix (manual, record outputs in PR):
  `analyze .` → Overview, explanation printed;
  `analyze . --task "why does DiscoveryPipeline throw"` → Trace around DiscoveryPipeline;
  `analyze . --around DiscoveryPipeline` → Trace;
  `analyze . --scenario trace` → works, no `trace` in `scenarios` listing;
  `analyze . --scenario audit` → deprecation warning, runs overview;
  `analyze . --scenario deep-dive` (no focus) → warning hint.
- Desktop: question box drives mode + explanation; autocomplete appears after first analysis.
- Docs: update `docs/cli-reference.md` and `docs/desktop-ui.md` to the Focus+Depth vocabulary
  (lead with `--task`/`--focus`; scenario/profile in an "Advanced" subsection).
