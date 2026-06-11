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

## Endpoints

**DntSite.Web** (70 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| POST | /api/UploadFile | UploadFileController.MessagesFilesUpload | - | UploadFileController.cs:42 |
| POST | /api/UploadFile | UploadFileController.CommonFilesUpload | - | UploadFileController.cs:38 |
| POST | /api/UploadFile | UploadFileController.FileUpload | - | UploadFileController.cs:34 |
| POST | /api/UploadFile | UploadFileController.CourseFileUpload | - | UploadFileController.cs:30 |
| POST | /api/UploadFile | UploadFileController.CourseImagesUpload | - | UploadFileController.cs:26 |
| POST | /api/UploadFile | UploadFileController.MessagesImagesUpload | - | UploadFileController.cs:22 |
| POST | /api/UploadFile | UploadFileController.ImageUpload | - | UploadFileController.cs:18 |
| GET | /users/EmailToImage/{id:int?} | FileController.EmailToImage | - | FileController.cs:60 |
| GET | /File/EmailToImage | FileController.EmailToImage | - | FileController.cs:60 |
| GET | /File/CourseImages | FileController.CourseImages | - | FileController.cs:55 |
| GET | /File/CourseFiles | FileController.CourseFiles | - | FileController.cs:51 |
| GET | /File/CommonFiles | FileController.CommonFiles | Authorize | FileController.cs:47 |
| GET | /File/NewsThumb | FileController.NewsThumb | - | FileController.cs:42 |
| GET | /File/Messages | FileController.Messages | - | FileController.cs:39 |
| GET | /File/ProjectFile | FileController.ProjectFile | - | FileController.cs:34 |
| GET | /File/UserFile | FileController.UserFile | - | FileController.cs:30 |
| GET | /File/MessagesImages | FileController.MessagesImages | - | FileController.cs:27 |
| GET | /File/Image | FileController.Image | - | FileController.cs:22 |
| GET | /File/Avatar | FileController.Avatar | - | FileController.cs:18 |
| GET | /Welcome | WelcomeController.Log | - | WelcomeController.cs:12 |
| GET | /Sitemap/Get | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /sitemap | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /sitemap.xml | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /OpenSearch | OpenSearchController.RenderOpenSearch | - | OpenSearchController.cs:13 |
| POST | /api/Fts | FtsController.Log | - | FtsController.cs:48 |
| GET | /api/Fts | FtsController.Search | - | FtsController.cs:19 |
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
| GET | /Exports/{type}/{name}.pdf | ExportsController.Get | - | ExportsController.cs:13 |
| POST | /api/JavaScriptErrorsReport | JavaScriptErrorsReportController.Log | - | JavaScriptErrorsReportController.cs:16 |
| GET | /.well-known/change-password | λ ChangePasswordEndpoint.cs:9 | - | ChangePasswordEndpoint.cs:9 |

## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `BaseEntity` | — | Id |

**31 EF Core migrations found.**

## Middleware pipeline

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

## DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Scoped | serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            SetCascadeOnSaveChanges(context);

            return context;
        } | [factory] | SQLiteServiceCollectionExtensions.cs:14 |
| Extension | AddDbContextPool | AddDbContextPool → (serviceProvider, optionsBuilder)... | SQLiteServiceCollectionExtensions.cs:11 |
| Singleton | _ => configuration | [factory] | SQLiteContextFactory.cs:34 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors → new TestHostingEnvironment() | SQLiteContextFactory.cs:28 |
| Scoped | IWebHostEnvironment | IWebHostEnvironment → TestHostingEnvironment | SQLiteContextFactory.cs:27 |
| Singleton | IAppFoldersService | IAppFoldersService → AppFoldersService | SQLiteContextFactory.cs:26 |
| Singleton | ILoggerFactory | ILoggerFactory → LoggerFactory | SQLiteContextFactory.cs:25 |
| Singleton | IHttpContextAccessor | IHttpContextAccessor → HttpContextAccessor | SQLiteContextFactory.cs:24 |
| Extension | AddLogging | AddLogging → cfg => cfg.AddSimpleConsole(opts =>... | SQLiteContextFactory.cs:17 |
| Extension | AddOptions | AddOptions → (AddOptions) | SQLiteContextFactory.cs:15 |
| Singleton | ILoggerProvider | ILoggerProvider → EfDbLoggerProvider | EfDbLoggerFactoryExtensions.cs:9 |
| Extension | AddDNTScheduler | AddDNTScheduler → options =>... | SchedulersConfig.cs:18 |
| Extension | AddProblemDetails | AddProblemDetails → (AddProblemDetails) | MvcControllersConfig.cs:9 |
| Extension | AddRequestTimeouts | AddRequestTimeouts → options =>... | MvcControllersConfig.cs:9 |
| Extension | AddLargeFilesUploadSupport | AddLargeFilesUploadSupport → (AddLargeFilesUploadSupport) | MvcControllersConfig.cs:9 |
| Extension | AddOutputCache | AddOutputCache → options => { options.AddPolicy(AlwaysCachePolicy.Name, Al... | MvcControllersConfig.cs:9 |
| Extension | AddControllers | AddControllers → options =>... | MvcControllersConfig.cs:9 |
| Extension | AddCustomJsonOptionsForWebApps | AddCustomJsonOptionsForWebApps → (AddCustomJsonOptionsForWebApps) | MvcControllersConfig.cs:9 |
| Extension | AddOptions | AddOptions → StartupSettingsModel | ServicesRegistry.cs:37 |
| Extension | AddCustomizedAuthentication | AddCustomizedAuthentication → siteSettings | ServicesRegistry.cs:32 |
| Extension | AddCustomizedControllers | AddCustomizedControllers → (AddCustomizedControllers) | ServicesRegistry.cs:31 |
| Extension | AddSchedulers | AddSchedulers → (AddSchedulers) | ServicesRegistry.cs:29 |
| Extension | AddDNTCommonWeb | AddDNTCommonWeb → (AddDNTCommonWeb) | ServicesRegistry.cs:28 |
| Extension | AddCustomizedDataProtection | AddCustomizedDataProtection → siteSettings | ServicesRegistry.cs:27 |
| Extension | AddConfiguredDbContext | AddConfiguredDbContext → siteSettings | ServicesRegistry.cs:26 |
| Bulk | AutoInjectAllServices | [bulk auto-registration] | ServicesRegistry.cs:23 |
| Extension | AddIPrincipal | AddIPrincipal → (AddIPrincipal) | ServicesRegistry.cs:22 |
| Extension | AddHttpContextAccessor | AddHttpContextAccessor → (AddHttpContextAccessor) | ServicesRegistry.cs:21 |
| Extension | AddForwardedHeadersOptions | AddForwardedHeadersOptions → (AddForwardedHeadersOptions) | ServicesRegistry.cs:19 |
| Extension | AddOptions | AddOptions → configuration | ServicesRegistry.cs:18 |
| Extension | AddEFSecondLevelCache | AddEFSecondLevelCache → options => options.UseMemoryCacheProvider()... | DbContextConfig.cs:38 |
| Extension | AddEfSecondLevelCacheInterceptor | AddEfSecondLevelCacheInterceptor → environment | DbContextConfig.cs:33 |
| Singleton | AuditableEntitiesInterceptor | AuditableEntitiesInterceptor | DbContextConfig.cs:32 |
| Singleton | EfExceptionsInterceptor | EfExceptionsInterceptor | DbContextConfig.cs:31 |
| Extension | AddConfiguredSqLiteDbContext | AddConfiguredSqLiteDbContext → (AddConfiguredSqLiteDbContext) | DbContextConfig.cs:24 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors → environment | DbContextConfig.cs:23 |
| Extension | AddDataProtection | AddDataProtection → (AddDataProtection) | DataProtectionConfig.cs:29 |
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
| Extension | AddAuthentication | AddAuthentication → options =>... | AuthenticationConfig.cs:29 |
| Extension | AddCookie | AddCookie → options =>... | AuthenticationConfig.cs:29 |
| Extension | AddAuthorization | AddAuthorization → options =>... | AuthenticationConfig.cs:23 |
| Scoped | AuthenticationStateProvider | AuthenticationStateProvider → IdentityRevalidatingAuthenticationStateProvider | AuthenticationConfig.cs:21 |
| Extension | AddCascadingAuthenticationState | AddCascadingAuthenticationState → (AddCascadingAuthenticationState) | AuthenticationConfig.cs:20 |
| Extension | AddCustomizedServices | AddCustomizedServices → host | Program.cs:24 |
| Extension | AddControllers | AddControllers → (AddControllers) | Program.cs:23 |
| Extension | AddRazorComponents | AddRazorComponents → (AddRazorComponents) | Program.cs:22 |
| Extension | AddInteractiveServerComponents | AddInteractiveServerComponents → (AddInteractiveServerComponents) | Program.cs:22 |

## Related types

- **Presentation**: JavaScriptErrorsReportController, SitemapController, FileController, FtsController, ProjectsFeedsController, UploadFileController, OpenSearchController, WelcomeController, BaseEntity, FeedController, ExportsController, V2024_05_18_1347

---
*Generated in 44.4ms | 1289 types (12 active, 1277 pruned) | Compression: TrivialMemberCompressor(−1%) · StructuralDeduplicator(−51%) | Schema v1.0*
