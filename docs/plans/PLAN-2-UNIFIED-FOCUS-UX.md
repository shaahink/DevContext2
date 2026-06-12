# Plan 2 — Focus + Depth: Entry-Point Slicing, No Query Pretense

> Implements P4 (and part of P6) of `docs/DESIGN-PHILOSOPHY.md`. Depends on Plan 1's types
> (`AnalysisSnapshot`, `RenderRequest`); independent of Plan 3.
>
> **Product model (this is the spec):** DevContext is static analysis with smart,
> controllable filtering — NOT a query system. There are exactly two situations:
> 1. **Unknown repo** → orientation map (no starting point exists).
> 2. **Known starting point** → slice from a focus *down the wiring*
>    (endpoint → handler → MediatR → entities/events), with a depth dial.
> Everything else is filtering (sections, budget, noise) — smart defaults first, visual
> adjustment after (Plan 1's lens). There is no natural-language input anywhere: the
> `--task` keyword-inference feature is removed because pseudo-NLU sets query-system
> expectations the tool doesn't honor (P7: genuine).

## Ground rules

- Branch off the Plan 1 result. Build + test per phase. Razor comments are `@* *@`.
- CLI compatibility for one release: `--scenario`, `--profile`, `--around`, `--task` keep
  *parsing* — `--around` aliases `--focus`, scenario/profile become hidden expert overrides,
  `--task` prints a deprecation pointing at `--focus` (and is otherwise ignored).
  `trace`/`audit` remain silent aliases.
- The `__source__` sentinel ≠ a `SectionNames` entry (see Plan 1 ground rules).

## Phase 0 — Recon

Read: `Configuration/IntentInferrer.cs` (to be deleted) + its tests, `ScenarioRegistry.cs`,
`Resolvers/FocusPointParser.cs`, `FocusPointResolver` (locate it), `Models/Detections.cs`
(`EndpointDetection` fields), `StringHelpers`, `AnalyzeCommand` (`ResolveScenarioAndProfile`,
`BuildSharedAnalysis`), `AnalysisService.AnalyzeAsync` (scenario/profile remapping lines),
`MainViewModel` (`ApplyScenarioSectionDefaults`, `SelectedScenario`, `IsTraceMode`,
`DerivedProfile`, the Task/Around properties), `Components/ConfigPanel.razor`.
Grep `IntentInferrer` and `"trace"` / `"audit"` string literals solution-wide; list call sites.

## Phase 1 — `AnalysisIntentResolver` in Core (small and total)

New `src/DevContext.Core/Configuration/AnalysisIntentResolver.cs`:

```csharp
public sealed record IntentInput
{
    public string? Focus { get; init; }             // type | Type:Method | endpoint route text
    public int? Depth { get; init; }                // --depth; null = scenario default
    public string? ExplicitScenario { get; init; }  // hidden expert override
    public string? ExplicitProfile { get; init; }   // hidden expert override
}

public sealed record ResolvedIntent
{
    public required Scenario Scenario { get; init; }       // with Depth applied to Pruning config
    public required ExtractionProfile Profile { get; init; }
    public required ImmutableArray<FocusPoint> FocusPoints { get; init; } // unresolved; pipeline resolves
    public required string Explanation { get; init; }      // one sentence, always shown
    public ImmutableArray<string> Warnings { get; init; } = [];
}
```

Rules — encode exactly, in order; this is the whole derivation:

1. Aliases: `trace`→`deep-dive`, `audit`→`overview` (deprecation warning). Unknown explicit
   scenario → error result.
2. Scenario: `ExplicitScenario` if set; else **focus present → `deep-dive`, absent →
   `overview`**. That is the entire mode logic. No keyword rules.
3. Depth: if `Depth` is set, override `Scenario.Pruning.MaxCallDepth` (and scale
   `MaxPathDistance`: depth ≤ 2 ⇒ 1, else 2) via `with`-clone. Range-check 1–10.
4. Profile: `ExplicitProfile` if set; else `deep-dive` → `Debug` (call graph on),
   `overview` → `Focused`.
5. Focus parsing: try `FocusPointParser` (Type[:Method]) first. If the text contains `/` or
   starts with an HTTP verb (`GET|POST|PUT|PATCH|DELETE `, case-insensitive), classify as
   **endpoint focus**: emit `FocusPoint { Kind = FocusKind.Endpoint, Route = ..., HttpMethod = ... }`
   (new fields/kind — resolved in Phase 2).
6. Explanation: `"Overview map (no focus)."` or
   `"Slicing from OrderService, depth 5, call graph on."` or
   `"Slicing from GET /api/orders → handler resolved after scan."`
   Warning when explicit `deep-dive` has no focus: `"deep-dive without --focus behaves like
   overview — give a starting point"`.

**Delete `IntentInferrer.cs` and its tests** (grep call sites first — `AnalyzeCommand`,
`AnalysisService`, tests). Resolver unit tests: ≥ 10 (aliases, derivation both ways, depth
override + clamping, endpoint classification incl. bare `/orders` and `get /orders`,
explanation snapshots, deep-dive-no-focus warning).

## Phase 2 — Endpoint focus resolution in the pipeline

`FocusPointResolver` runs after Stage 2; endpoint detections appear in Stage 3. So endpoint
focus resolves **after Stage 3** (verify the pipeline still has a sequencing point before
scoring — it does: scoring runs last):

1. Add `FocusKind.Endpoint`. After Stage 3, match endpoint focus points against
   `EndpointDetection`s: exact route match first, then normalized
   (`StringHelpers.NormalizeRoute`), then substring; filter by HTTP verb when given.
   On match: rewrite the focus point to the handler — `TypeName = HandlerType,
   MethodName = HandlerMethod, Kind = Method` — so existing path/graph scoring works
   untouched. Ambiguity (n > 1): take all matches, add Info diagnostic listing them.
   No match: Warning diagnostic with 3 nearest routes (Levenshtein on normalized route).
2. Type/method focus keeps resolving after Stage 2 exactly as today (did-you-mean intact).
3. Tests: route exact/normalized/substring/verb-filtered/ambiguous/missing — ≥ 6, fixture
   style copied from existing extractor tests.

## Phase 3 — CLI adoption

1. Delete `ResolveScenarioAndProfile` from `AnalyzeCommand`; build `IntentInput`, call the
   resolver, print `Explanation` (dim) + `Warnings` (yellow) before the spinner.
2. `AnalyzeSettings`: `--focus` (primary, repeatable like `--around` today; `--around` stays
   as alias — check Spectre's multi-name option support, else keep two properties feeding one
   list), `--depth <N>` (new), `--task` marked `[Obsolete]`-equivalent in help + runtime
   deprecation message, `--scenario`/`--profile` help text suffixed
   `"(advanced — derived from --focus)"`.
3. `ScenariosCommand`: list `overview` and `deep-dive` only (display name "Slice" is clearer
   than "Trace" — decide once, update `DisplayName` in the registry and goldens deliberately),
   each with a one-line "when" (`no starting point` / `requires --focus`).

## Phase 4 — Registry + duplication cleanup

Delete the `"trace"` entry from `ScenarioRegistry` (alias lives in the resolver). Delete the
scenario/profile remapping lines from `AnalysisService` — it builds `IntentInput` and calls
the resolver too (one derivation, two front-ends). Update every `"trace"`/`"audit"` literal
found in Phase 0 recon.

## Phase 5 — Desktop adoption

1. **Remove the Intent/Task text field** from `ConfigPanel.razor` and the `Task` plumbing
   from `MainViewModel`/`AnalysisOptions`/`AppSettings` (keep the `AppSettings.LastTask`
   property reading-tolerant so old settings.json files still deserialize; just stop writing
   it). Grep `Task` bindings in all `.razor` files first — note `Task` is also the BCL type;
   grep for `VM.Task` / `LastTask` / `opts.Task` specifically.
2. **Focus picker** replaces the Entry-point field: one input bound to `Around`/`Focus` with
   autocomplete from the previous snapshot — suggestions = type names + `Type:Method` pairs +
   endpoint routes (`GET /api/orders`), sourced from `_snapshot.Model` (types by `FinalScore`,
   endpoints from detections), capped ~500. Plain Razor dropdown, no JS library.
3. Mode toggle (Overview/Slice) = display of the derived mode, user-overridable (override ⇒
   `ExplicitScenario`). Show `Explanation` under the row (small, dim, monospace).
4. **Noise visibility** (the "visual filter" half of the product model): in the section
   drawer, add an "Excluded types: N — show" disclosure backed by `RenderPlan.Excluded`
   (Plan 1) grouped by reason (`test project`, `budget`, `scenario cap`), each group
   re-includable as a render-input change (re-render only, no re-analysis). If
   `RenderPlan`/`RerenderAsync` from Plan 1 isn't merged yet, stop and report — this phase
   depends on it.
5. Profile stays fully derived (no control). Grep `SelectedProfile|IsProfile|SetProfileCommand`
   across `.razor` + tests before deleting any leftovers.

Desktop tests: focus suggestions populate after analysis (types + routes), mode derives from
focus presence, override sticks, excluded-group re-include triggers re-render not re-analysis,
old settings.json with `LastTask` still loads.

## Phase 6 — Verification

- Full `dotnet test` + `eval/gates.ps1`.
- CLI matrix (record outputs in PR):
  `analyze .` → Overview, explanation printed;
  `analyze . --focus DiscoveryPipeline` → Slice, depth default;
  `analyze . --focus DiscoveryPipeline:RunAsync --depth 3` → depth honored (visible in stats);
  `analyze . --focus "GET /api/orders"` on an eval repo with endpoints → resolves to handler
  (diagnostic shows the resolution);
  `analyze . --around X` → identical to `--focus X`;
  `analyze . --task "anything"` → deprecation message, behaves as no-focus overview;
  `analyze . --scenario trace` → works silently; `--scenario audit` → deprecation;
  `--scenario deep-dive` without focus → warning hint.
- Desktop: picker autocompletes routes after first analysis; excluded-types disclosure
  re-includes without re-analysis.
- Docs: `cli-reference.md` + `desktop-ui.md` rewritten around the two situations
  (map / slice) + filtering; scenario/profile in an "Advanced" footnote. Remove the `--task`
  section from `cli-reference.md` and the `--task` override paragraph from
  `configuration.md`; delete `examples/intent.md` and fix `--task` usages in the other
  `examples/*.md` to `--focus` equivalents (grep `--task` across `docs/` — zero hits outside
  deprecation notes when done).
