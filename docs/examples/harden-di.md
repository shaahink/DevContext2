# Example: DI Hardening Audit

**Mode**: Overview — Identify indirect wiring patterns, service locators, reflection activation, and other DI anti-patterns.

**Command**:
```bash
# Check for DI issues across the codebase
devcontext . --scenario overview --profile debug

# Or focus the intent
devcontext . --task "di injection wiring middleware pipeline"
```

---

## What the output looks like

### Indirect Wiring

```markdown
### Indirect wiring

| Kind | Caller | Target |
|------|--------|--------|
| ManualServiceLocator | DntSitePageTitle.AddToSiteUrlsBackgroundQueueAsync | unknown |
| ReflectionActivation | DbSetsConfigs.RegisterAllDerivedEntities | GetTypes |
| ManualServiceLocator | SqLiteServiceCollectionExtensions.UseConfiguredSqLite | unknown |
| ManualServiceLocator | DataSeedersRunner.RunAllDataSeeders | unknown |
```

### DI Registrations

```markdown
### DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Bulk | AutoInjectAllServices | [bulk auto-registration] | ServicesRegistry.cs:23 |
| Singleton | AuditableEntitiesInterceptor | AuditableEntitiesInterceptor | DbContextConfig.cs:32 |
| Singleton | EfExceptionsInterceptor | EfExceptionsInterceptor | DbContextConfig.cs:31 |
| Scoped | AuthenticationStateProvider → IdentityRevalidatingAuthenticationStateProvider | AuthenticationConfig.cs:21 |
| Singleton | IXmlRepository → DataProtectionKeyService | DataProtectionConfig.cs:18 |
```

### Middleware Pipeline

```markdown
### Middleware pipeline

| Type | Kind | Count | Sources |
|------|------|-------|---------|
| UseForwardedHeaders | UseX | 1 | Program.cs |
| UseExceptionHandler | UseX | 1 | Program.cs |
| UseStatusCodePagesWithReExecute | UseX | 1 | Program.cs |
| UseAntiDos | UseX | 1 | Program.cs |
| UseCsp | UseX | 1 | Program.cs |
| UseHttpsRedirection | UseX | 1 | Program.cs |
| UseAuthentication | UseX | 1 | Program.cs |
| UseAuthorization | UseX | 1 | Program.cs |
| UseAntiforgery | UseX | 1 | Program.cs |
| UseOutputCache | UseX | 1 | Program.cs |
| UseRequestTimeouts | UseX | 1 | Program.cs |
```

### Anti-Patterns (when `--include-anti-patterns` is active)

```markdown
### Anti-patterns detected

| Severity | Pattern | Description | File |
|----------|---------|-------------|------|
| high | ServiceLocator | IServiceScopeFactory.CreateScope() | BacktestOrchestrator.cs:117 |
| high | FireAndForget | _ = RunAsync(cfg) task never awaited | BacktestOrchestrator.cs:85 |
```

---

## What to look for

| Pattern | What it means | Risk |
|---------|---------------|------|
| **ManualServiceLocator** | `sp.GetService<T>()` or `sp.GetRequiredService<T>()` is called manually instead of constructor injection | Hidden dependencies, harder to test |
| **ReflectionActivation** | `Activator.CreateInstance` or assembly scanning for types | Bypasses DI container entirely |
| **DynamicProxy** | Castle Proxy or similar runtime proxies | Complex to debug, may hide DI resolution |
| **AutoInjectAllServices** | Bulk auto-registration (e.g., Scrutor scanning) | Hard to audit which services are registered |
| **Missing `UseX` for `AddX`** | E.g., `AddCors` without `UseCors` | Config bug — service registered but middleware not activated |

---

## Profile variations

| Profile flag | What you get extra |
|-------------|--------------------|
| `--profile focused` | Indirect wiring + middleware + DI registrations (default) |
| `--profile debug` | Adds call graph — useful if wiring detection finds issues and you want to trace the call chain |
| `--profile full` | Adds source code bodies — useful for deep audit of the flagged locations |
