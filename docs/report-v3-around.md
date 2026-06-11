## DevContext — Overview on project

**Architecture**: ControllerBased (80% confidence)
**Signals**: controllers · minimal-apis · efcore
**Projects**: 3 — DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 12 in output

---
## Architecture overview

└── DntSite.Tests
    └── DntSite.Web
        └── DntSite.Web.Common.BlazorSsr

## Entry points

### `FeedController` (Class, Presentation)
> `DntSite.Web.Features.RssFeeds.Controllers.FeedController` — C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSite.Web\Features\RssFeeds\Controllers\FeedController.cs

**Extends**: `ControllerBase`

**Methods**:
- `Task<IActionResult> Index()`
- `Task<IActionResult> Posts()`
- `Task<IActionResult> UserPosts(string? name)`
- `Task<IActionResult> Comments()`
- `Task<IActionResult> UserComments(string? name)`
- `Task<IActionResult> News()`
- `Task<IActionResult> Tag(string? id)`
- `Task<IActionResult> Author(string? id)`

## Endpoints

**DntSite.Web** (41 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| GET | /ProjectsFeeds/ProjectIssuesReplies/{id:int?} | ProjectsFeedsController.ProjectIssuesReplies | - | ProjectsFeedsController.cs:72 |
| GET | /ProjectsFeeds/ProjectIssues/{id:int?} | ProjectsFeedsController.ProjectIssues | - | ProjectsFeedsController.cs:59 |
| GET | /ProjectsFeeds/ProjectFiles/{id:int?} | ProjectsFeedsController.ProjectFiles | - | ProjectsFeedsController.cs:46 |
| GET | /ProjectsFeeds/ProjectFaqs/{id:int?} | ProjectsFeedsController.ProjectFaqs | - | ProjectsFeedsController.cs:33 |
| GET | /ProjectsFeeds/ProjectsFaqs | ProjectsFeedsController.ProjectsFaqs | - | ProjectsFeedsController.cs:30 |
| GET | /ProjectsFeeds/ProjectsIssuesReplies | ProjectsFeedsController.ProjectsIssuesReplies | - | ProjectsFeedsController.cs:26 |
| GET | /ProjectsFeeds/ProjectsIssues | ProjectsFeedsController.ProjectsIssues | - | ProjectsFeedsController.cs:23 |
| GET | /ProjectsFeeds/ProjectsFiles | ProjectsFeedsController.ProjectsFiles | - | ProjectsFeedsController.cs:20 |
| GET | /ProjectsFeeds/ProjectsNews | ProjectsFeedsController.ProjectsNews | - | ProjectsFeedsController.cs:17 |
| GET | /ProjectsFeeds/Get | ProjectsFeedsController.Get | - | ProjectsFeedsController.cs:15 |
| GET | /ProjectsFeeds/Index | ProjectsFeedsController.Index | - | ProjectsFeedsController.cs:13 |
| GET | /Feed/ShowBriefDescriptionAsync | FeedController.ShowBriefDescriptionAsync | - | FeedController.cs:131 |
| GET | /Feed/Announcements | FeedController.Announcements | - | FeedController.cs:127 |
| GET | /Feed/Surveys | FeedController.Surveys | - | FeedController.cs:124 |
| GET | /Feed/CoursesComments | FeedController.CoursesComments | - | FeedController.cs:120 |
| GET | /Feed/CoursesTopics | FeedController.CoursesTopics | - | FeedController.cs:116 |
| GET | /Feed/Courses | FeedController.Courses | - | FeedController.cs:113 |
| GET | /Feed/GetLatestChangesAsync | FeedController.GetLatestChangesAsync | - | FeedController.cs:110 |
| GET | /llms-full.txt | FeedController.LlmsFull | - | FeedController.cs:106 |
| GET | /llms.txt | FeedController.LlmsTxt | - | FeedController.cs:103 |
| GET | /blog/rss.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /blog/feed | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /feed/atom | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /feed/rss | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /feed.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /rss2.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /rss | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /atom.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /rss.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /Feed/LatestChanges | FeedController.LatestChanges | - | FeedController.cs:89 |
| GET | /Feed/NewsAuthor/{id?} | FeedController.NewsAuthor | - | FeedController.cs:77 |
| GET | /Feed/NewsComments | FeedController.NewsComments | - | FeedController.cs:73 |
| GET | /Feed/Author/{id?} | FeedController.Author | - | FeedController.cs:61 |
| GET | /Feed/Tag/{id?} | FeedController.Tag | - | FeedController.cs:50 |
| GET | /Feed/News | FeedController.News | - | FeedController.cs:47 |
| GET | /feeds/comments/{name?} | FeedController.UserComments | - | FeedController.cs:36 |
| GET | /Feed/Comments | FeedController.Comments | - | FeedController.cs:33 |
| GET | /feeds/posts/{name?} | FeedController.UserPosts | - | FeedController.cs:22 |
| POST | /Feed/Posts | FeedController.Posts | - | FeedController.cs:19 |
| GET | /Feed | FeedController.Index | - | FeedController.cs:15 |
| GET | /Feed/Index | FeedController.Index | - | FeedController.cs:15 |

## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<OnModelCreating>` | — | — |
| `BaseEntity` | — | Id |

### `Migrations`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `V2024_04_19_1424` | — | — |
| `V2024_05_18_1347` | — | — |
| `V2024_06_19_2139` | — | — |
| `V2024_06_25_2320` | — | — |
| `V2024_06_27_2036` | — | — |
| `V2024_06_28_1257` | — | — |
| `V2024_06_30_2030` | — | — |
| `V2024_07_17_1405` | — | — |
| `V2024_07_19_2106` | — | — |
| `V2024_07_19_2211` | — | — |
| `V2024_07_19_2234` | — | — |
| `V2024_08_12_1323` | — | — |
| `V2024_09_28_1204` | — | — |
| `V2024_09_28_1327` | — | — |
| `V2024_10_05_1343` | — | — |
| `V2024_10_05_1417` | — | — |
| `V2024_10_18_1302` | — | — |
| `V2024_10_19_2133` | — | — |
| `V2024_10_30_1357` | — | — |
| `V2024_11_17_1942` | — | — |
| `V2025_12_31_2127` | — | — |
| `V2026_01_11_1104` | — | — |
| `V2026_01_11_1910` | — | — |
| `V2026_01_28_2352` | — | — |
| `V2026_02_03_0009` | — | — |
| `V2026_03_16_1155` | — | — |
| `V2026_04_02_1410` | — | — |
| `V2026_04_21_1348` | — | — |
| `V2026_05_06_1053` | — | — |
| `V2026_05_07_1000` | — | — |
| `V2026_06_03_1205` | — | — |

## Non-obvious wiring

### Background workers

- AIDailyNewsBacklogsJob (HostedService)
- AIDailyNewsJob (HostedService)
- EmptyPMsJob (HostedService)
- DailyBirthDatesEmailJob (HostedService)
- DailyNewsletterJob (HostedService)
- DeleteOrphansJob (HostedService)
- ExportToMergedPdfFilesJob (HostedService)
- ExportToSeparatePdfFilesJob (HostedService)
- ThumbnailsServiceJob (HostedService)
- FullTextSearchWriterJob (HostedService)
- DraftsJob (HostedService)
- HumansTxtJob (HostedService)
- BackupDataFolderJob (HostedService)
- BackupDatabaseJob (HostedService)
- ManageBacklogsJob (HostedService)
- NewPersianYearEmailsJob (HostedService)
- DisableInactiveUsersJob (HostedService)
- SendActivationEmailsJob (HostedService)
- UpdateDeletedNewsHttpStatusCodeJob (HostedService)
- UpdatePublicNewsHttpStatusCodeJob (HostedService)
- WebReadersListJob (HostedService)
- FreeSpaceCheckJob (HostedService)
- CheckAdminsLastVisitJob (HostedService)
- DotNetVersionCheckJob (HostedService)

### Middleware pipeline

| Type | Kind | Count | Sources |
|------|------|-------|---------|
| UseRequestTimeouts | UseX | 1 | Program.cs |
| UseOutputCache | UseX | 1 | Program.cs |
| UseAntiforgery | UseX | 1 | Program.cs |
| UseAuthorization | UseX | 1 | Program.cs |
| UseAuthentication | UseX | 1 | Program.cs |
| UseHttpsRedirection | UseX | 1 | Program.cs |
| UseCsp | UseX | 1 | Program.cs |
| UseAntiDos | UseX | 1 | Program.cs |
| UseStatusCodePagesWithReExecute | UseX | 1 | Program.cs |
| UseExceptionHandler | UseX | 1 | Program.cs |
| UseForwardedHeaders | UseX | 1 | Program.cs |

### DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Scoped | serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            SetCascadeOnSaveChanges(context);

            return context;
        } | [factory] | SQLiteServiceCollectionExtensions.cs:14 |
| Extension | AddDbContextPool | AddDbContextPool → (serviceProvider, optionsBuilder)
            => optionsBuilder.UseConfiguredSqLite(serviceProvider) | SQLiteServiceCollectionExtensions.cs:11 |
| Singleton | _ => configuration | [factory] | SQLiteContextFactory.cs:34 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors → new TestHostingEnvironment() | SQLiteContextFactory.cs:28 |
| Scoped | IWebHostEnvironment | IWebHostEnvironment → TestHostingEnvironment | SQLiteContextFactory.cs:27 |
| Singleton | IAppFoldersService | IAppFoldersService → AppFoldersService | SQLiteContextFactory.cs:26 |
| Singleton | ILoggerFactory | ILoggerFactory → LoggerFactory | SQLiteContextFactory.cs:25 |
| Singleton | IHttpContextAccessor | IHttpContextAccessor → HttpContextAccessor | SQLiteContextFactory.cs:24 |
| Extension | AddLogging | AddLogging → cfg => cfg.AddSimpleConsole(opts =>
            {
                opts.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ-";
                opts.ColorBehavior = LoggerColorBehavior.Enabled;
            })
            .AddDebug() | SQLiteContextFactory.cs:17 |
| Extension | AddOptions | AddOptions → ? | SQLiteContextFactory.cs:15 |
| Extension | AddOptions | AddOptions → StartupSettingsModel | ServicesRegistry.cs:37 |
| Extension | AddCustomizedAuthentication | AddCustomizedAuthentication → siteSettings | ServicesRegistry.cs:32 |
| Extension | AddCustomizedControllers | AddCustomizedControllers → ? | ServicesRegistry.cs:31 |
| Extension | AddSchedulers | AddSchedulers → ? | ServicesRegistry.cs:29 |
| Extension | AddDNTCommonWeb | AddDNTCommonWeb → ? | ServicesRegistry.cs:28 |
| Extension | AddCustomizedDataProtection | AddCustomizedDataProtection → siteSettings | ServicesRegistry.cs:27 |
| Extension | AddConfiguredDbContext | AddConfiguredDbContext → siteSettings | ServicesRegistry.cs:26 |
| Bulk | AutoInjectAllServices | [bulk auto-registration] | ServicesRegistry.cs:23 |
| Extension | AddIPrincipal | AddIPrincipal → ? | ServicesRegistry.cs:22 |
| Extension | AddHttpContextAccessor | AddHttpContextAccessor → ? | ServicesRegistry.cs:21 |
| Extension | AddForwardedHeadersOptions | AddForwardedHeadersOptions → ? | ServicesRegistry.cs:19 |
| Extension | AddOptions | AddOptions → configuration | ServicesRegistry.cs:18 |
| Extension | AddDNTScheduler | AddDNTScheduler → options =>
        {
            options.AddScheduledTask<DotNetVersionCheckJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 5, Minute: 30, Second: 1 });

            options.AddScheduledTask<CheckAdminsLastVisitJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Minute % 5 == 0 && now.Second == 1;
            });

            options.AddScheduledTask<FreeSpaceCheckJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Hour % 6 == 0 && now is { Minute: 10, Second: 1 };
            });

            options.AddScheduledTask<WebReadersListJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 3, Minute: 30, Second: 1 });

            options.AddScheduledTask<UpdatePublicNewsHttpStatusCodeJob>(utcNow
                => GetNowIranTime(utcNow) is { Day: 1, Hour: 1, Minute: 1, Second: 1 });

            options.AddScheduledTask<UpdateDeletedNewsHttpStatusCodeJob>(utcNow
                => GetNowIranTime(utcNow) is { Day: 2, Hour: 1, Minute: 1, Second: 1 });

            options.AddScheduledTask<SendActivationEmailsJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 11, Minute: 1, Second: 1 });

            options.AddScheduledTask<DisableInactiveUsersJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 6, Minute: 1, Second: 1 });

            options.AddScheduledTask<NewPersianYearEmailsJob>(utcNow => GetNowIranTime(utcNow).IsStartOfNewYear());

            options.AddScheduledTask<ManageBacklogsJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Hour % 2 == 0 && now is { Minute: 10, Second: 1 };
            });

            options.AddScheduledTask<BackupDatabaseJob>(utcNow
                => GetNowIranTime(utcNow) is
                {
                    DayOfWeek: DayOfWeek.Friday or DayOfWeek.Monday, Hour: 4, Minute: 1, Second: 1
                });

            options.AddScheduledTask<BackupDataFolderJob>(utcNow
                => GetNowIranTime(utcNow) is { DayOfWeek: DayOfWeek.Saturday, Hour: 4, Minute: 1, Second: 1 });

            options.AddScheduledTask<HumansTxtJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 3, Minute: 1, Second: 1 });

            options.AddScheduledTask<DraftsJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Minute % 5 == 0 && now.Second == 1;
            });

            options.AddScheduledTask<FullTextSearchWriterJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Minute % 5 == 0 && now.Second == 1;
            });

            options.AddScheduledTask<ThumbnailsServiceJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Minute % 10 == 0 && now.Second == 1;
            });

            options.AddScheduledTask<ExportToSeparatePdfFilesJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Minute % 20 == 0 && now.Second == 1;
            });

            options.AddScheduledTask<ExportToMergedPdfFilesJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 5, Minute: 30, Second: 1 });

            options.AddScheduledTask<DeleteOrphansJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 3, Minute: 7, Second: 1 });

            options.AddScheduledTask<DailyNewsletterJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 0, Minute: 1, Second: 1 });

            options.AddScheduledTask<DailyBirthDatesEmailJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 8, Minute: 59, Second: 1 });

            options.AddScheduledTask<EmptyPMsJob>(utcNow
                => GetNowIranTime(utcNow) is { Hour: 3, Minute: 1, Second: 1 });

            options.AddScheduledTask<AIDailyNewsJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Minute % 15 == 0 && now.Second == 1;
            });

            options.AddScheduledTask<AIDailyNewsBacklogsJob>(utcNow =>
            {
                var now = GetNowIranTime(utcNow);

                return now.Hour % 2 == 0 && now is { Minute: 5, Second: 1 };
            });
        } | SchedulersConfig.cs:18 |
| Extension | AddProblemDetails | AddProblemDetails → ? | MvcControllersConfig.cs:9 |
| Extension | AddRequestTimeouts | AddRequestTimeouts → options =>
            {
                options.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromMinutes(value: 30),
                    TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable
                };
            } | MvcControllersConfig.cs:9 |
| Extension | AddLargeFilesUploadSupport | AddLargeFilesUploadSupport → ? | MvcControllersConfig.cs:9 |
| Extension | AddOutputCache | AddOutputCache → options => { options.AddPolicy(AlwaysCachePolicy.Name, AlwaysCachePolicy.Instance); } | MvcControllersConfig.cs:9 |
| Extension | AddControllers | AddControllers → options =>
            {
                options.Filters.Add<ApplyCorrectYeKeFilterAttribute>();
                options.Filters.Add<CheckSiteIsActiveActionFilter>();
            } | MvcControllersConfig.cs:9 |
| Extension | AddCustomJsonOptionsForWebApps | AddCustomJsonOptionsForWebApps → ? | MvcControllersConfig.cs:9 |
| Extension | AddEFSecondLevelCache | AddEFSecondLevelCache → options => options.UseMemoryCacheProvider()
            .ConfigureLogging(environment.IsDevelopment(), args =>
            {
                switch (args.EventId)
                {
                    case CacheableLogEventId.CacheHit:
                    case CacheableLogEventId.QueryResultCached:
                        break;
                    case CacheableLogEventId.QueryResultInvalidated:
                        args.ServiceProvider.GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(EFCoreSecondLevelCacheInterceptor))
                            .LogWarning(message: "{EventId} -> {Message} -> {CommandText}", args.EventId, args.Message,
                                args.CommandText);

                        break;
                    case CacheableLogEventId.CachingSkipped:
                    case CacheableLogEventId.InvalidationSkipped:
                    case CacheableLogEventId.CachingSystemStarted:
                    case CacheableLogEventId.CachingError:
                    case CacheableLogEventId.QueryResultSuppressed:
                    case CacheableLogEventId.CacheDependenciesCalculated:
                    case CacheableLogEventId.CachePolicyCalculated:
                        break;
                }
            })
            .UseCacheKeyPrefix(prefix: "EF_")
            .CacheAllQueriesExceptContainingTypes(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(value: 5),
                typeof(AppLogItem), typeof(SiteUrl), typeof(SiteReferrer))
            .SkipCachingCommands(commandText
                => commandText.Contains(value: "NEWID()", StringComparison.OrdinalIgnoreCase))
            .SkipCacheInvalidationCommands(ShouldIgnoreForAllCommands)
            .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(value: 1)) | DbContextConfig.cs:38 |
| Extension | AddEfSecondLevelCacheInterceptor | AddEfSecondLevelCacheInterceptor → environment | DbContextConfig.cs:33 |
| Singleton | AuditableEntitiesInterceptor | AuditableEntitiesInterceptor | DbContextConfig.cs:32 |
| Singleton | EfExceptionsInterceptor | EfExceptionsInterceptor | DbContextConfig.cs:31 |
| Extension | AddConfiguredSqLiteDbContext | AddConfiguredSqLiteDbContext → ? | DbContextConfig.cs:24 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors → environment | DbContextConfig.cs:23 |
| Extension | AddDataProtection | AddDataProtection → ? | DataProtectionConfig.cs:29 |
| Singleton | serviceProvider =>
        {
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                serviceProvider.RunScopedService<IXmlRepository>(xmlRepository
                    => options.XmlRepository = xmlRepository);
            });
        } | [factory: new ConfigureOptions<KeyManagementOptions>(options =>
            {
                serviceProvider.RunScopedService<IXmlRepository>(xmlRepository
                    => options.XmlRepository = xmlRepository);
            })] | DataProtectionConfig.cs:20 |
| Singleton | IXmlRepository | IXmlRepository → DataProtectionKeyService | DataProtectionConfig.cs:18 |
| Extension | AddAuthentication | AddAuthentication → options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            } | AuthenticationConfig.cs:29 |
| Extension | AddCookie | AddCookie → options =>
            {
                options.LoginPath = UserProfilesRoutingConstants.Login;
                options.LogoutPath = UserProfilesRoutingConstants.Logout;
                options.AccessDeniedPath = "/error/403";
                options.Cookie.Name = ".dnt.site.cookie";
                options.Cookie.HttpOnly = true;

                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;

                // A cookie with "SameSite=Lax" will be sent with a same-site request,
                // or a cross-site top-level navigation with a "safe" HTTP method.
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.SlidingExpiration = true;

                options.ExpireTimeSpan =
                    TimeSpan.FromDays(siteSettings.DataProtectionOptions.LoginCookieExpirationDays);

                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        var cookieValidatorService =
                            context.HttpContext.RequestServices.GetRequiredService<ICookieValidatorService>();

                        return cookieValidatorService.ValidateAsync(context);
                    }
                };

                options.CookieManager = new ChunkingCookieManager
                {
                    // Slightly smaller chunk size
                    ChunkSize = 3000,
                    ThrowForPartialCookies = environment.IsDevelopment()
                };
            } | AuthenticationConfig.cs:29 |
| Extension | AddAuthorization | AddAuthorization → options =>
        {
            options.AddPolicy(CustomRoles.Admin, policy => policy.RequireRole(CustomRoles.Admin));
            options.AddPolicy(CustomRoles.User, policy => policy.RequireRole(CustomRoles.User));
        } | AuthenticationConfig.cs:23 |
| Scoped | AuthenticationStateProvider | AuthenticationStateProvider → IdentityRevalidatingAuthenticationStateProvider | AuthenticationConfig.cs:21 |
| Extension | AddCascadingAuthenticationState | AddCascadingAuthenticationState → ? | AuthenticationConfig.cs:20 |
| Extension | AddCustomizedServices | AddCustomizedServices → host | Program.cs:24 |
| Extension | AddControllers | AddControllers → ? | Program.cs:23 |
| Extension | AddRazorComponents | AddRazorComponents → ? | Program.cs:22 |
| Extension | AddInteractiveServerComponents | AddInteractiveServerComponents → ? | Program.cs:22 |
| Singleton | ILoggerProvider | ILoggerProvider → EfDbLoggerProvider | EfDbLoggerFactoryExtensions.cs:9 |

## Related types grouped by layer

- **Presentation**: JavaScriptErrorsReportController, SitemapController, FileController, FtsController, ProjectsFeedsController, UploadFileController, OpenSearchController, WelcomeController, BaseEntity, FeedController, ExportsController, V2024_05_18_1347

---
*Generated in 45.3ms | 1289 types (12 active, 1277 pruned) | Compression: TrivialMemberCompressor(−1%) · StructuralDeduplicator(−51%) | Schema v2.0.0*
