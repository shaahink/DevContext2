## DevContext -- Architecture Overview on TodoApp

**Architecture**: MinimalApi (100% confidence)
**Signals**: minimal-apis
**Projects**: 7 -- Todo.Api, Todo.Api.Tests, TodoApp.AppHost, TodoApp.ServiceDefaults, Todo.Web.Client, Todo.Web.Server, Todo.Web.Shared
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 32 in output

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

No endpoints detected.

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
| Extension | AddHttpClient | client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

    // The cookie auth stack detects this header and avoids redirects for unauthenticated
    // requests
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
} |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddOptions | Auth0Constants.AuthenticationScheme |
| Extension | AddHttpContextAccessor | ? |
| Singleton | AuthenticationStateProvider | HttpAuthenticationStateProvider |
| Singleton | ExternalProviders | ExternalProviders |
| Extension | AddAuthentication | CookieAuthenticationDefaults.AuthenticationScheme |
| Scoped | IAuthorizationHandler | CheckCurrentUserAuthHandler |
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

## Related types grouped by layer

- **Api**: TodoApplication, TodoApi, TokenService, TodoApiTests, AuthorizationHandlerExtensions, ClaimsTransformation, TodoDbContext, UsersApi, RateLimitExtensions, TodoDbContextModelSnapshot, ValidationFilterExtensions, CurrentUser, AuthToken, CheckCurrentUserAuthHandler, UserApiTests, ForeignKeyChange, AuthHandler
- **Presentation**: AuthClient, ExternalProviders, AuthenticationExtensions, AuthenticationSchemes, AuthApi, TodoClient
- **Unknown**: HttpAuthenticationStateProvider, UserInfo, Extensions, ExternalUserInfo, TodoMappingExtensions, Extensions, TodoItem, AuthToken, Todo

---
*Generated in 21.0ms | 43 types (32 active, 11 pruned) | Schema v2.0*
