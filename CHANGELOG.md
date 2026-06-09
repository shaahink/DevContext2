# Changelog

All notable changes to DevContext are documented here.

## v2.0.0 (unreleased)

### Added

- **Event flow tracing** — New `InMemoryEventBusExtractor` detects `IEventBus.Subscribe<T>()`, `IEventBus.PublishAsync<T>()`, and `IEventHandler<T>` implementations. New `## Event flow` section in output shows publisher → event → handler → DB relationships for in-memory buses. (InMemoryEventBusExtractor)

- **Anti-pattern detection** — New `AntiPatternDetector` flags 5 patterns: fire-and-forget tasks (`_ = AsyncMethod()`), `IServiceScopeFactory` usage (service locator), `new` outside constructor/DI, `CancellationToken.None`, and unbounded `ConcurrentDictionary`/`ConcurrentQueue` without eviction. Output filtered to exclude test files and mock types. (AntiPatternDetector)

- **Deep call graph with field-type resolution** — CallGraphExtractor now resolves `_field.Method()` to the field's declared type, cross-references DI registrations to find concrete implementations, and chases same-class method calls (`this.DoSomething()`) to the containing type. BFS traversal depth reaches level 5+ on real projects. (CallGraphExtractor)

- **Project dependency tree** — Architecture overview now shows project references as an ASCII tree (`├── └── │`) instead of a flat list. (MarkdownRenderer)

- **Constructor dependency display** — Entry points now show `**Depends on**:` with parameter types and names, plus `**Resolved to**:` cross-referencing each dependency against DI registrations. (MarkdownRenderer, SyntaxStructureExtractor)

- **Source code bodies** — Full type source rendered in output for the entry point and up to 5 call chain types. Unlocked by `--profile full`. (MarkdownRenderer, SourceBodyExtractor)

- **LINQ chain resolution** — Chained calls like `db.Todos.Where(x => ...)` now walk to the root identifier and resolve through the field map. (CallGraphExtractor)

- **DI extension method parsing** — Generic DI extension methods like `AddHostedService<EngineWorker>()` now correctly capture the implementation type (`EngineWorker`) instead of `"?"`. (DiRegistrationExtractor)

- **CancellationToken.None variants** — `default(CancellationToken)` and `new CancellationToken()` are now detected alongside `CancellationToken.None`. (AntiPatternDetector)

### Fixed

- **Constructor extraction** — SyntaxStructureExtractor now captures `ConstructorDeclarationSyntax` (was filtered by `OfType<MethodDeclarationSyntax>`). Constructor parameters now appear in the call graph's field map. (SyntaxStructureExtractor)

- **fqnMap short name collisions** — When two types share a short name (e.g., `Models.Order` and `Dtos.Order`), the resolver now prefers the type matching the caller's namespace. Collisions are logged as diagnostics. (CallGraphExtractor)

- **ArrowExpressionClauseSyntax false positive** — `int Prop => new Foo()` in expression-bodied members is no longer incorrectly flagged as outside DI. (AntiPatternDetector)

- **Empty catch blocks** — DependencyExtractor and AntiPatternDetector empty catches now log diagnostics instead of silently swallowing errors.

- **Cancellation in BFS loops** — Call graph BFS traversal now respects `CancellationToken`. (CallGraphExtractor)

- **Duplicated code removed** — Triplicated `EnumerateSourceFilesAsync` consolidated into shared `ExtractorHelpers`. `ExtractGenericArguments` duplication resolved. (ExtractorHelpers)

- **Dead code removed** — `TypeDiscovery.AdditionalFilePaths` removed (populated but never read).

### Changed

- **Anti-pattern severity normalized** — All fire-and-forget patterns now severity `"high"` (was `"medium"` for `ContinueWith`). `CancellationTokenNone` downgraded to `"low"` in test files.
- **ServiceLocator detection uses AST** — `DetectServiceScopeFactory` now matches on exact member name (`CreateScope`/`CreateAsyncScope`) instead of `string.Contains`.
- **AntiPatternDetection record moved** — From `AntiPatternDetector.cs` to `Models/Detections.cs` for consistency with all other detection types.
- **Test file filtering added** — `DetectServiceScopeFactory` and `DetectCancellationTokenNone` now skip or downgrade findings in test files.

### Architecture

- **11 ADRs** covering all design decisions: Roslyn separation, pipeline stage ordering, signal sealing, typed detection hierarchy, parse-once cache, async-first IO, stable JSON schema, and more.
- **Shared** `ExtractorHelpers` for `EnumerateSourceFilesAsync` and `IsTestFile`.

### Known Limitations

- In-memory event bus `PublishAsync` calls without explicit generic type arguments (<T>) are not captured (requires Roslyn semantic model).
- FastEndpoints call graph edges are not resolved (inheritance-based handlers).
- Partial class fields are invisible across partial files in the call graph.
