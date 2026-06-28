# Example Outputs

Curated DevContext analysis outputs for real open-source .NET repositories.
Each example shows what an LLM receives — a token-budgeted, pruned document
with structure overview, endpoint inventory, DI registrations, and more.

## Repos analyzed

| Repo | Source | Style | Type |
|------|--------|-------|------|
| [eShop](https://github.com/dotnet/eShop) | .NET team reference | Microservices (Aspire) | Map + Trace |
| [VerticalSlice](https://github.com/ardalis/CleanArchitecture) | Steve Smith | VerticalSlices (FastEndpoints) | Map |
| [TodoApi](https://github.com/davidfowl/TodoApi) | David Fowler | MinimalApi | Map |
| [AutoMapper](https://github.com/AutoMapper/AutoMapper) | Jimmy Bogard | Library (class library) | Map (library surface) |
| [DntSite](https://github.com/VahidN/DntSite) | Vahid Nasiri | ControllerBased + Workers | Map + Trace |

## File naming

```
<repo>-<scenario>[-<focus>].md        Markdown output (LLM-ready)
<repo>-<scenario>[-<focus>].json      JSON output (machine-readable)
```

## Regenerating

```powershell
# Clone the eval repos (pinned commits):
git clone https://github.com/dotnet/eShop.git eval-repos/eShop
cd eval-repos/eShop; git checkout 9b4f9434f46fdc5c1a6e9e936af2868340cdbc48; cd ../..

# Run DevContext (replace paths with your setup):
dotnet run --project src/DevContext.Cli -- analyze eval-repos/eShop --max-tokens 8000 > docs/examples/eshop-overview.md
dotnet run --project src/DevContext.Cli -- analyze eval-repos/eShop --focus "POST /api/orders/" --depth 3 --max-tokens 8000 > docs/examples/eshop-trace-orders.md
```

## See also

- Full eval results: `eval-results/`
- Evaluation gate: `eval/gates.ps1`
- Expectations (expected counts/signals): `eval/expectations/`
