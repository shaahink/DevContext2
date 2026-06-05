 # Real Repo Evaluation Report

**Branch**: feature/eval-real-repos-scrappable
**Tool**: DevContext with Spectre.Cli
**Approach**: Shallow clones of 4 repos. Runs with quick/focused/debug profiles + --metrics . Outputs in md per run.

See individual md files in subdirs for raw --format markdown exports with metrics.

## Per-Repo Analysis

### 1. TodoApi (minimalapi)
Raw: eval-results/TodoApi/focused-architecture.md , debug-debug-endpoint.md , dry-run

From output:
- Correctly detects MinimalApi architecture 100%, signal 'minimal-apis'
- Detects some Non-obvious wiring (middleware UseX, DI AddApiEndpoints)
- **No endpoints detected** - major gap. The TodoApi uses extension methods like AddTodoApi or MapGroup in separate TodoApiExtensions. EndpointExtractor only scans for direct app.Map* in Program.cs .
- Types: 43 found, ~32 in output (pruned ~25%)
- Good for small web api.
- Working: signal, architecture, DI/middleware detection, pruning.
- Not: endpoint detection for modern patterns, perhaps more specific for Aspire parts (it has AppHost).
- Missing: support for MapGroup, better handler resolution (lambdas shown as . <lambda> ), integration with FastEndpoints or other if used.

### 2. VerticalSlice (CleanArch)
Raw: eval-results/VerticalSlice/focused-architecture.md etc.

- Detects signals including from FastEndpoints (DI AddFastEndpoints)
- Architecture overview with layers (Infrastructure etc listed in related)
- No endpoints (uses controllers or FastEndpoints, not minimal direct)
- More types survive (97 active)
- Working: layer classification, DI registration extraction, pruning less aggressive on this.
- Not: endpoint/controller detection (ControllerActionExtractor may not have fired or no 'controllers' signal).
- Missing: better support for FastEndpoints or standard MVC controller actions with attributes.

### 3. AutoMapper (library)
Raw: eval-results/AutoMapper/quick-architecture.md , focused, debug

- No web signals (expected, no minimal/mediatr etc).
- Architecture unknown or project.
- No endpoints.
- Handles large codebase: 2713 types found, prunes to 51 (quick) or 95 (focused) .
- Lists many internal/test classes in related types (not ideal for 'library' use case).
- Working: scales to thousands of types, syntax structure, pruning, compression.
- Not: the output includes lots of test/When_ * classes, which are noise for context.
- Missing: better layer or relevance for libraries (perhaps detect 'test' projects and prune more, or namespace based for production code). SourceBody for key mappers if debug/full.

### 4. eShop (large)
Raw: eval-results/eShop/dry-run-quick.txt (full run heavy, used dry-run + excludes)

- Dry run shows it plans many extractors, skips specifics until signals (as expected, stage1 only for dry).
- Large project count (would trigger MaxProjects logic perhaps).
- Working: root resolution for complex Aspire mono repo, file tree on large, project structure parse.
- Issues likely: time for SyntaxStructure on many .cs, Roslyn load for Aspire projects, exclude patterns important (node, wwwroot etc).
- Missing from dry: full run would reveal if AspireExtractor works well on AppHost, if call graph scales.

## Overall Strengths
- Core detection of architecture style, package signals, basic DI and middleware works well.
- Output format is clean and useful (tables, sections).
- Metrics reveal real perf (SyntaxStructure heavy).
- Pruning and token budget functional.
- Spectre.Cli makes UX professional (subcommands, help, validation).

## Overall Gaps / For Next Iteration
- Endpoint detection too naive for real world (MapGroup, extension methods, attribute routes in controllers, FastEndpoints).
- Duplicate work in tree walking extractors (Syntax + Di + ProgramCsFlow) - should share parsed trees or use cache better for 'interesting' nodes.
- For libraries and non-web: too much test/internal noise in output; need better 'production code' filters or layer inference for non-layered.
- eShop scale: needs testing full run with smart excludes, MaxProjects handling, perf tuning.
- Handler resolution: often shows <lambda> or incomplete.
- More signals for modern stacks (FastEndpoints, Aspire resources fully, gRPC, etc).
- Better integration tests/goldens for these patterns.

See the per-run .md for exact exports used in analysis.

This provides data for next iteration of extractors, pruning heuristics, etc.

