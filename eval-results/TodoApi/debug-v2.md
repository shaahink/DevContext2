## DevContext -- Architecture Overview on TodoApp

**Architecture**: MinimalApi (100% confidence)
**Signals**: minimal-apis
**Projects**: 7 -- Todo.Api, Todo.Api.Tests, TodoApp.AppHost, TodoApp.ServiceDefaults, Todo.Web.Client, Todo.Web.Server, Todo.Web.Shared
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 32 in output

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

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| GET | signin/{provider} | async (string provider, AuthClient client, HttpContext context, IDataProtectionProvider dataProtectionProvider) =>
        {
            // Grab the login information from the external login dance
            var result = await context.AuthenticateAsync(AuthenticationSchemes.ExternalScheme);

            if (result.Succeeded)
            {
                var principal = result.Principal;

                var id = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // TODO: We should have the user pick a user name to complete the external login dance
                // for now we'll prefer the email address
                var name = (principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity?.Name)!;

                // Protect the user id so it for transport
                var protector = dataProtectionProvider.CreateProtector(provider);

                var token = await client.GetOrCreateUserAsync(provider, new() { Username = name, ProviderKey = protector.Protect(id) });

                if (token is not null)
                {
                    // Write the login cookie
                    await SignIn(id, name, token, provider).ExecuteAsync(context);
                }
            }

            // Delete the external cookie
            await context.SignOutAsync(AuthenticationSchemes.ExternalScheme);

            // TODO: Handle the failure somehow

            return Results.Redirect("/");
        }.<lambda> | - |
| GET | login/{provider} | (string provider) =>
        {
            // Trigger the external login flow by issuing a challenge with the provider name.
            // This name maps to the registered authentication scheme names in AuthenticationExtensions.cs
            return Results.Challenge(
                properties: new() { RedirectUri = $"/auth/signin/{provider}" },
                authenticationSchemes: [provider]);
        }.<lambda> | - |
| POST | logout | async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // TODO: Support remote logout
            // If this is an external login then use it
            //var result = await context.AuthenticateAsync();
            //if (result.Properties?.GetExternalProvider() is string providerName)
            //{
            //    await context.SignOutAsync(providerName, new() { RedirectUri = "/" });
            //}
        }.<lambda> | - |
| POST | login | async (UserInfo userInfo, AuthClient client) =>
        {
            // Retrieve the access token give the user info
            var token = await client.GetTokenAsync(userInfo);

            if (token is null)
            {
                return Results.Unauthorized();
            }

            return SignIn(userInfo, token);
        }.<lambda> | - |
| POST | register | async (UserInfo userInfo, AuthClient client) =>
        {
            // Retrieve the access token given the user info
            var token = await client.CreateUserAsync(userInfo);

            if (token is null)
            {
                return Results.Unauthorized();
            }

            return SignIn(userInfo, token);
        }.<lambda> | - |
| POST | /token/{provider} | async Task<Results<Ok<AccessTokenResponse>, SignInHttpResult, ValidationProblem>> (string provider, ExternalUserInfo userInfo, UserManager<TodoUser> userManager, SignInManager<TodoUser> signInManager, IDataProtectionProvider dataProtectionProvider) =>
        {
            var protector = dataProtectionProvider.CreateProtector(provider);

            var providerKey = protector.Unprotect(userInfo.ProviderKey);

            var user = await userManager.FindByLoginAsync(provider, providerKey);

            var result = IdentityResult.Success;

            if (user is null)
            {
                user = new TodoUser() { UserName = userInfo.Username };

                result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, displayName: null));
                }
            }

            if (result.Succeeded)
            {
                var principal = await signInManager.CreateUserPrincipalAsync(user);

                return TypedResults.SignIn(principal);
            }

            return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        }.<lambda> | - |
| DELETE | /{id} | async Task<Results<NotFound, Ok>> (TodoDbContext db, int id, CurrentUser owner) =>
        {
            var rowsAffected = await db.Todos.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteDeleteAsync();

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        }.<lambda> | - |
| PUT | /{id} | async Task<Results<Ok, NotFound, BadRequest>> (TodoDbContext db, int id, TodoItem todo, CurrentUser owner) =>
        {
            if (id != todo.Id)
            {
                return TypedResults.BadRequest();
            }

            var rowsAffected = await db.Todos.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteUpdateAsync(updates =>
                                                updates.SetProperty(t => t.IsComplete, todo.IsComplete)
                                                       .SetProperty(t => t.Title, todo.Title));

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        }.<lambda> | - |
| POST | / | async Task<Created<TodoItem>> (TodoDbContext db, TodoItem newTodo, CurrentUser owner) =>
        {
            var todo = new Todo
            {
                Title = newTodo.Title,
                OwnerId = owner.Id
            };

            db.Todos.Add(todo);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/todos/{todo.Id}", todo.AsTodoItem());
        }.<lambda> | - |
| GET | /{id} | async Task<Results<Ok<TodoItem>, NotFound>> (TodoDbContext db, int id, CurrentUser owner) =>
        {
            return await db.Todos.FindAsync(id) switch
            {
                Todo todo when todo.OwnerId == owner.Id || owner.IsAdmin => TypedResults.Ok(todo.AsTodoItem()),
                _ => TypedResults.NotFound()
            };
        }.<lambda> | - |
| GET | / | async (TodoDbContext db, CurrentUser owner) =>
        {
            return await db.Todos.Where(todo => todo.OwnerId == owner.Id).Select(t => t.AsTodoItem()).AsNoTracking().ToListAsync();
        }.<lambda> | - |

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
    client.BaseAddress = new("http://todoapi");
} |
| Extension | AddHttpForwarderWithServiceDiscovery | ? |
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveWebAssemblyComponents | ? |
| Scoped | TodoClient | TodoClient |
| Extension | AddDataProtection | o => o.ApplicationDiscriminator = "TodoApp" |
| Extension | AddAuthorizationBuilder | ? |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddHttpClient | client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

    // The cookie auth stack detects this header and avoids redirects for unauthenticated
    // requests
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
} |
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

## Related types grouped by layer

- **Api**: TodoApplication, TodoApi, TokenService, TodoApiTests, AuthorizationHandlerExtensions, ClaimsTransformation, TodoDbContext, UsersApi, RateLimitExtensions, TodoDbContextModelSnapshot, ValidationFilterExtensions, CurrentUser, AuthToken, CheckCurrentUserAuthHandler, UserApiTests, ForeignKeyChange, AuthHandler
- **Presentation**: AuthClient, ExternalProviders, AuthenticationExtensions, AuthenticationSchemes, AuthApi, TodoClient
- **Unknown**: HttpAuthenticationStateProvider, UserInfo, Extensions, ExternalUserInfo, TodoMappingExtensions, Extensions, TodoItem, AuthToken, Todo

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Info | CallReachabilityPruner | No method-level focus points; skipping reachability analysis. |
| Info | CallGraphExtractor | Built call graph: 697 edges at depth ≤ 3 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.TodoItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: TodoApi.Migrations.ForeignKeyChange |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: TodoApi.Migrations.RemoveIsAdmin |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: TodoApi.Migrations.Initial |
| Info | ProgramCsFlowExtractor | Orphan pattern: 'AddAuthentication' at line 14 in Program.cs has no corresponding 'UseAuthentication' call |

---
*Generated in 10.2ms | 43 types (32 active, 11 pruned) | Schema v2.0*
