## DevContext -- Architecture Overview on TodoApp

**Architecture**: MinimalApi (100% confidence)
**Signals**: minimal-apis
**Projects**: 7 -- Todo.Api, Todo.Api.Tests, TodoApp.AppHost, TodoApp.ServiceDefaults, Todo.Web.Client, Todo.Web.Server, Todo.Web.Shared
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 28 in output

---
## Architecture overview

- Todo.Api
- Todo.Api.Tests
- TodoApp.AppHost
- TodoApp.ServiceDefaults
- Todo.Web.Client
- Todo.Web.Server
- Todo.Web.Shared

## Endpoints

| Method | Route | Group | Handler | Auth | Source |
|--------|-------|-------|---------|------|--------|
| GET | /auth/signin/{provider} | /auth | λ AuthApi.cs:64 | - | AuthApi.cs:64 |
| GET | /auth/login/{provider} | /auth | λ AuthApi.cs:55 | - | AuthApi.cs:55 |
| POST | /auth/logout | /auth | λ AuthApi.cs:40 | - | AuthApi.cs:40 |
| POST | /auth/login | /auth | λ AuthApi.cs:27 | - | AuthApi.cs:27 |
| POST | /auth/register | /auth | λ AuthApi.cs:14 | - | AuthApi.cs:14 |
| POST | /users/token/{provider} | /users | λ UsersApi.cs:24 | - | UsersApi.cs:24 |
| DELETE | /todos/{id} | /todos | λ TodoApi.cs:67 | - | TodoApi.cs:67 |
| PUT | /todos/{id} | /todos | λ TodoApi.cs:52 | - | TodoApi.cs:52 |
| POST | /todos/ | /todos | λ TodoApi.cs:38 | - | TodoApi.cs:38 |
| GET | /todos/{id} | /todos | λ TodoApi.cs:29 | - | TodoApi.cs:29 |
| GET | /todos/ | /todos | λ TodoApi.cs:24 | - | TodoApi.cs:24 |

## Non-obvious wiring

### Middleware pipeline

| Order | Type | Kind |
|-------|------|------|
| 1 | UseWebAssemblyDebugging | UseX |
| 1 | UseHttpLogging | UseX |
| 2 | UseHsts | UseX |
| 2 | UseRateLimiter | UseX |
| 3 | UseHttpsRedirection | UseX |
| 4 | UseAntiforgery | UseX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
| Extension | AddOptions | Auth0Constants.AuthenticationScheme |
| Extension | AddHttpContextAccessor | ? |
| Singleton | AuthenticationStateProvider | HttpAuthenticationStateProvider |
| Singleton | ExternalProviders | ExternalProviders |
| Extension | AddAuthentication | CookieAuthenticationDefaults.AuthenticationScheme |
| Extension | AddHttpClient | client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

    // The cookie auth stack detects this header and avoids redirects for unauthenticated
    // requests
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
} |
| Extension | AddHttpClient | client =>
{
    client.BaseAddress = new("http://todoapi");
} |
| Extension | AddHttpForwarderWithServiceDiscovery | ? |
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveWebAssemblyComponents | ? |
| Scoped | TodoClient | TodoClient |
| Extension | AddDataProtection | o => o.ApplicationDiscriminator = "TodoApp" |
| Extension | AddAuthorizationBuilder | ? |
| Extension | AddHttpLogging | o =>
{
    if (builder.Environment.IsDevelopment())
    {
        o.CombineLogs = true;
        o.LoggingFields = HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
    }
} |
| Extension | AddRateLimiting | ? |
| Extension | AddOpenApi | options => options.AddBearerTokenAuthentication() |
| Extension | AddCurrentUser | ? |
| Extension | AddSqlite | connectionString |
| Extension | AddIdentityCore | ? |
| Extension | AddEntityFrameworkStores | ? |
| Extension | AddApiEndpoints | ? |
| Extension | AddAuthorizationBuilder | ? |
| Extension | AddCurrentUserHandler | ? |
| Extension | AddAuthentication | ? |
| Extension | AddBearerToken | IdentityConstants.BearerScheme |
| Extension | AddDataProtection | o => o.ApplicationDiscriminator = "TodoApp" |
| Scoped | IAuthorizationHandler | CheckCurrentUserAuthHandler |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |

## Related types grouped by layer

- **Api**: TodoApi, TokenService, AuthorizationHandlerExtensions, ClaimsTransformation, TodoDbContext, UsersApi, RateLimitExtensions, TodoDbContextModelSnapshot, ValidationFilterExtensions, CheckCurrentUserRequirement, CurrentUser, CheckCurrentUserAuthHandler, ForeignKeyChange
- **Presentation**: AuthClient, ExternalProviders, AuthenticationExtensions, AuthenticationSchemes, AuthApi, TodoClient
- **Unknown**: HttpAuthenticationStateProvider, UserInfo, Extensions, ExternalUserInfo, TodoMappingExtensions, Extensions, TodoItem, AuthToken, Todo

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: TodoApi.Migrations.RemoveIsAdmin |
| Info | CallReachabilityPruner | CallGraph not available; skipping reachability analysis. |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.TodoItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: TodoApi.Migrations.ForeignKeyChange |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: TodoApi.Migrations.Initial |
| Info | ProgramCsFlowExtractor | Orphan pattern: 'AddAuthentication' at line 14 in Program.cs has no corresponding 'UseAuthentication' call |

### Pruning notes

- PatternRelevancePruner: pruned test type 'TodoApi.Tests.TodoApplication'
- PatternRelevancePruner: pruned test type 'TodoApi.Tests.TodoApiTests'
- PatternRelevancePruner: pruned test type 'TodoApi.Tests.DbContextExtensions'
- PatternRelevancePruner: pruned test type 'TodoApi.Tests.AuthToken'
- PatternRelevancePruner: pruned test type 'TodoApi.Tests.UserApiTests'
- PatternRelevancePruner: pruned test type 'TodoApi.Tests.AuthHandler'

---
*Generated in 28.4ms | 43 types (28 active, 15 pruned) | Compression: TrivialMemberCompressor(−1%) · BoilerplateCompressor(−5%) · StructuralDeduplicator(−11%) | Schema v2.0*
