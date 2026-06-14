Overview map (no focus).
Analyzing project...

## Dry Run Plan
**Root**: C:\Code\DevContext2\_eval-dntsite
**Scenario**: Overview
**Profile**: Focused
**Max tokens**: 8000

### Stage 1 (sequential)
| Status | Name | Description |
|---|---|---|
| Γ£ô | FileTreeExtractor | Walks the file tree and registers all source file 
paths in the cache |
| Γ£ô | SolutionDiscovery | Parses .sln and .slnx files to discover solution 
structure |
| Γ£ô | ProjectStructure | Parses .csproj files to extract project structure info 
|

### Stage 2 (parallel)
| Status | Name | Description |
|---|---|---|
| Γ£ô | DependencyExtractor | Detects NuGet package references and builds project 
dependency graph |
| Γ£ô | LayerClassifier | Classifies each project into an ArchitectureLayer using 
path heuristics and package references |
| Γ£ô | SyntaxStructureExtractor | Walks syntax trees of all .cs files to discover
type declarations and emits controller signal fallback |
| Γ£ô | ProgramCsFlowExtractor | Walks Program.cs files for middleware 
registration order and background worker detection |
| Γ£ô | DiRegistrationExtractor | Cheap syntax matching for 
services.AddSingleton/AddScoped/AddTransient and AddX extension methods |

### Stage 3 (conditional, after signal detection)
| Status | Name | Requires | Description |
|---|---|---|---|
| ? | EndpointExtractor | minimal-apis OR fast-endpoints OR controllers | 
Detects HTTP endpoints: Minimal API Map* calls, FastEndpoints, MVC controllers |
| ? | MediatRExtractor | mediatr | Walks syntax trees to detect MediatR handlers
and marker interfaces |
| ? | ControllerActionExtractor | controllers | Walks syntax trees to detect MVC
controller actions and their route templates |
| ? | CallGraphExtractor | (always runs) | Walks syntax trees using Roslyn to 
build a BFS-depth-limited call graph |
| ? | EfCoreExtractor | efcore | Walks syntax trees to detect EF Core DbContext,
DbSet properties, and entity configurations |
| ? | EventBusExtractor | masstransit OR nservicebus | Walks syntax trees to 
detect message bus consumers and bus registrations |
| ? | SourceBodyExtractor | (always runs) | Populates SourceBody for each 
non-pruned type with its declaration source text |
| ? | AntiPatternDetector | (always runs) | Detects fire-and-forget tasks, 
IServiceScopeFactory, new outside DI, CancellationToken.None, unbounded 
collections |
| ? | InMemoryEventBusExtractor | (always runs) | Detects in-memory event bus 
wiring: IEventBus.Subscribe, IEventBus.PublishAsync, and IEventHandler 
implementations |
| ? | IndirectWiringDetector | (always runs) | Detects indirect wiring patterns 
like Activator.CreateInstance, Castle DynamicProxy, service locator, and 
reflection scanning |
| ? | AspireExtractor | aspire | Walks AppHost project files to detect Aspire 
resource patterns and service relationships |

*Stage 3 extractors run conditionally based on Stage 2 signal results. Use 
--scenario or configure signals in devcontext.json to control which run.*

Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé  Metric  Γöé        Value        Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé Solution Γöé    _eval-dntsite    Γöé
Γöé   Time   Γöé        250ms        Γöé
Γöé  Tokens  Γöé  ~0 (budget 8000)   Γöé
Γöé Version  Γöé v1.0.5-preview.0.42 Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
