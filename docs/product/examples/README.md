# Example Outputs

Curated DevContext analysis outputs for real open-source .NET repositories.
Each example shows what an LLM receives — a structurally-bounded (depth · fan-out)
narrative Map/Trace from the kernel, post-Universal-Lens Phases 0–6 (updated 2026-06-28).

## Repos analyzed

| Repo | Source | Style | Type |
|------|--------|-------|------|
| [eShop](https://github.com/dotnet/eShop) | .NET team reference | Microservices (Aspire) | Map + Trace |
| [VerticalSlice](https://github.com/ardalis/CleanArchitecture) | Steve Smith | VerticalSlices (FastEndpoints) | Map |
| [TodoApi](https://github.com/davidfowl/TodoApi) | David Fowler | MinimalApi | Map |
| [AutoMapper](https://github.com/AutoMapper/AutoMapper) | Jimmy Bogard | Library (class library) | Map (library surface) |
| [DntSite](https://github.com/VahidN/DntSite) | Vahid Nasiri | ControllerBased | Map + Trace |
| [ControllerApp](tests/fixtures/ControllerApp) | (fixture) | MinimalApi | Map + Trace |

## File naming

```
<repo>-<scenario>[-<focus>].md        Markdown output (LLM-ready)
<repo>-<scenario>[-<focus>].json      JSON output (machine-readable)
```

## Regenerating

```powershell
# Use absolute paths (single-slash relative paths are parsed as GitHub owner/repo).
$cli = "src/DevContext.Cli/bin/Release/net10.0/DevContext.Cli.dll"
dotnet build src/DevContext.Cli -c Release

# eShop Catalog.API trace (member-anchored, scope-stamped)
dotnet $cli analyze "$pwd\eval-repos\eShop\src\Catalog.API" --focus "CatalogApi:UpdateItem" --depth 5 | Out-File docs/product/examples/eshop-catalog-trace.md -Encoding utf8

# DntSite Feed trace (entry->target resolved, entry-scoped binding)
dotnet $cli analyze "$env:LOCALAPPDATA\DevContext\repos\VahidN-DntSite-default" --focus "GET /Feed/News" --depth 5 | Out-File docs/product/examples/dntsite-trace-feedcontroller.md -Encoding utf8
```

## See also

- Full eval results: `eval-results/`
- Evaluation gate: `eval/gates.ps1`
- Expectations (expected counts/signals): `eval/expectations/`
- Handover / architecture: `docs/dev/HANDOVER.md`
