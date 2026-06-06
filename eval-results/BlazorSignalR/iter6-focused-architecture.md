## DevContext -- Architecture Overview on BlazorSignalRApp

**Architecture**: MinimalApi (80% confidence)
**Signals**: minimal-apis
**Projects**: 2 -- BlazorSignalRApp, BlazorSignalRApp.Client
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 1 in output

---
## Architecture overview

- BlazorSignalRApp
- BlazorSignalRApp.Client

## Endpoints

No endpoints detected.

## Non-obvious wiring

### Middleware pipeline

| Type | Kind | Count | Sources |
|------|------|-------|---------|
| MapHub | MapX | 1 | Program.cs |
| UseAntiforgery | UseX | 1 | Program.cs |
| UseHttpsRedirection | UseX | 1 | Program.cs |
| UseStatusCodePagesWithReExecute | UseX | 1 | Program.cs |
| UseHsts | UseX | 1 | Program.cs |
| UseExceptionHandler | UseX | 1 | Program.cs |
| UseWebAssemblyDebugging | UseX | 1 | Program.cs |
| UseResponseCompression | UseX | 1 | Program.cs |

### DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Extension | AddResponseCompression | opts =>... | Program.cs:14 |
| Extension | AddSignalR | (AddSignalR) | Program.cs:12 |
| Extension | AddRazorComponents | (AddRazorComponents) | Program.cs:9 |
| Extension | AddInteractiveWebAssemblyComponents | (AddInteractiveWebAssemblyComponents) | Program.cs:9 |

## Related types grouped by layer

- **Unknown**: ChatHub

---
*Generated in 15.0ms | 1 types (1 active, 0 pruned) | Schema v2.0*
