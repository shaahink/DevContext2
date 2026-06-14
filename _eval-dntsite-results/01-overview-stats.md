Overview map (no focus).
Analyzing project...

## DevContext ΓÇö Overview on project

**Architecture**: ControllerBased (80% confidence)
**Signals**: controllers ┬╖ minimal-apis ┬╖ efcore
**Projects**: 3 ΓÇö DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: focused | **Tokens**: ~3481 (budget 8000) | **Types**: 40 in output

---
## Architecture overview

```text
ΓööΓöÇΓöÇ DntSite.Tests
    ΓööΓöÇΓöÇ DntSite.Web
        ΓööΓöÇΓöÇ DntSite.Web.Common.BlazorSsr
```

## Endpoints

**DntSite.Web** (70 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| GET | /.well-known/change-password | ╬╗ ChangePasswordEndpoint.cs:9 | - | 
ChangePasswordEndpoint.cs:9 |
| POST | /api/UploadFile | UploadFileController.MessagesFilesUpload | - | 
UploadFileController.cs:42 |
| POST | /api/UploadFile | UploadFileController.CommonFilesUpload | - | 
UploadFileController.cs:38 |
| POST | /api/UploadFile | UploadFileController.FileUpload | - | 
UploadFileController.cs:34 |
| POST | /api/UploadFile | UploadFileController.CourseFileUpload | - | 
UploadFileController.cs:30 |
| POST | /api/UploadFile | UploadFileController.CourseImagesUpload | - | 
UploadFileController.cs:26 |
| POST | /api/UploadFile | UploadFileController.MessagesImagesUpload | - | 
UploadFileController.cs:22 |
| POST | /api/UploadFile | UploadFileController.ImageUpload | - | 
UploadFileController.cs:18 |
| GET | /users/EmailToImage/{id:int?} | FileController.EmailToImage | - | 
FileController.cs:60 |
| GET | /File/EmailToImage | FileController.EmailToImage | - | 
FileController.cs:60 |
| GET | /File/CourseImages | FileController.CourseImages | - | 
FileController.cs:55 |
| GET | /File/CourseFiles | FileController.CourseFiles | - | 
FileController.cs:51 |
| GET | /File/CommonFiles | FileController.CommonFiles | Authorize | 
FileController.cs:47 |
| GET | /File/NewsThumb | FileController.NewsThumb | - | FileController.cs:42 |
| GET | /File/Messages | FileController.Messages | - | FileController.cs:39 |
| GET | /File/ProjectFile | FileController.ProjectFile | - | 
FileController.cs:34 |
| GET | /File/UserFile | FileController.UserFile | - | FileController.cs:30 |
| GET | /File/MessagesImages | FileController.MessagesImages | - | 
FileController.cs:27 |
| GET | /File/Image | FileController.Image | - | FileController.cs:22 |
| GET | /File/Avatar | FileController.Avatar | - | FileController.cs:18 |
| GET | /Welcome | WelcomeController.Log | - | WelcomeController.cs:12 |
| GET | /Sitemap/Get | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /sitemap | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /sitemap.xml | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /OpenSearch | OpenSearchController.RenderOpenSearch | - | 
OpenSearchController.cs:13 |
| POST | /api/Fts | FtsController.Log | - | FtsController.cs:48 |
| GET | /api/Fts | FtsController.Search | - | FtsController.cs:19 |
| GET | /ProjectsFeeds/ProjectIssuesReplies/{id:int?} | 
ProjectsFeedsController.ProjectIssuesReplies | - | ProjectsFeedsController.cs:72
|
| GET | /ProjectsFeeds/ProjectIssues/{id:int?} | 
ProjectsFeedsController.ProjectIssues | - | ProjectsFeedsController.cs:59 |
| GET | /ProjectsFeeds/ProjectFiles/{id:int?} | 
ProjectsFeedsController.ProjectFiles | - | ProjectsFeedsController.cs:46 |
| GET | /ProjectsFeeds/ProjectFaqs/{id:int?} | 
ProjectsFeedsController.ProjectFaqs | - | ProjectsFeedsController.cs:33 |
| GET | /ProjectsFeeds/ProjectsFaqs | ProjectsFeedsController.ProjectsFaqs | - |
ProjectsFeedsController.cs:30 |
| GET | /ProjectsFeeds/ProjectsIssuesReplies | 
ProjectsFeedsController.ProjectsIssuesReplies | - | 
ProjectsFeedsController.cs:26 |
| GET | /ProjectsFeeds/ProjectsIssues | ProjectsFeedsController.ProjectsIssues |
- | ProjectsFeedsController.cs:23 |
| GET | /ProjectsFeeds/ProjectsFiles | ProjectsFeedsController.ProjectsFiles | -
| ProjectsFeedsController.cs:20 |
| GET | /ProjectsFeeds/ProjectsNews | ProjectsFeedsController.ProjectsNews | - |
ProjectsFeedsController.cs:17 |
| GET | /ProjectsFeeds/Get | ProjectsFeedsController.Get | - | 
ProjectsFeedsController.cs:15 |
| GET | /ProjectsFeeds/Index | ProjectsFeedsController.Index | - | 
ProjectsFeedsController.cs:13 |
| GET | /Feed/ShowBriefDescriptionAsync | 
FeedController.ShowBriefDescriptionAsync | - | FeedController.cs:131 |
| GET | /Feed/Announcements | FeedController.Announcements | - | 
FeedController.cs:127 |
| GET | /Feed/Surveys | FeedController.Surveys | - | FeedController.cs:124 |
| GET | /Feed/CoursesComments | FeedController.CoursesComments | - | 
FeedController.cs:120 |
| GET | /Feed/CoursesTopics | FeedController.CoursesTopics | - | 
FeedController.cs:116 |
| GET | /Feed/Courses | FeedController.Courses | - | FeedController.cs:113 |
| GET | /Feed/GetLatestChangesAsync | FeedController.GetLatestChangesAsync | - |
FeedController.cs:110 |
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
| GET | /Feed/LatestChanges | FeedController.LatestChanges | - | 
FeedController.cs:89 |
| GET | /Feed/NewsAuthor/{id?} | FeedController.NewsAuthor | - | 
FeedController.cs:77 |
| GET | /Feed/NewsComments | FeedController.NewsComments | - | 
FeedController.cs:73 |
| GET | /Feed/Author/{id?} | FeedController.Author | - | FeedController.cs:61 |
| GET | /Feed/Tag/{id?} | FeedController.Tag | - | FeedController.cs:50 |
| GET | /Feed/News | FeedController.News | - | FeedController.cs:47 |
| GET | /feeds/comments/{name?} | FeedController.UserComments | - | 
FeedController.cs:36 |
| GET | /Feed/Comments | FeedController.Comments | - | FeedController.cs:33 |
| GET | /feeds/posts/{name?} | FeedController.UserPosts | - | 
FeedController.cs:22 |
| POST | /Feed/Posts | FeedController.Posts | - | FeedController.cs:19 |
| GET | /Feed | FeedController.Index | - | FeedController.cs:15 |
| GET | /Feed/Index | FeedController.Index | - | FeedController.cs:15 |
| GET | /Exports/{type}/{name}.pdf | ExportsController.Get | - | 
ExportsController.cs:13 |
| POST | /api/JavaScriptErrorsReport | JavaScriptErrorsReportController.Log | - 
| JavaScriptErrorsReportController.cs:16 |

## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `BaseEntity` | ΓÇö | Id |

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
            var context = 
serviceProvider.GetRequiredService<ApplicationDbContext>();
            SetCascadeOnSaveChanges(context);

            return context;
        } | [factory] | SQLiteServiceCollectionExtensions.cs:14 |
| Extension | AddDbContextPool | AddDbContextPool ΓåÆ (serviceProvider, 
optionsBuilder)
... | SQLiteServiceCollectionExtensions.cs:11 |
| Singleton | _ => configuration | [factory] | SQLiteContextFactory.cs:34 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors ΓåÆ new 
TestHostingEnvironment() | SQLiteContextFactory.cs:28 |
| Scoped | IWebHostEnvironment | IWebHostEnvironment ΓåÆ TestHostingEnvironment | 
SQLiteContextFactory.cs:27 |
| Singleton | IAppFoldersService | IAppFoldersService ΓåÆ AppFoldersService | 
SQLiteContextFactory.cs:26 |
| Singleton | ILoggerFactory | ILoggerFactory ΓåÆ LoggerFactory | 
SQLiteContextFactory.cs:25 |
| Singleton | IHttpContextAccessor | IHttpContextAccessor ΓåÆ HttpContextAccessor 
| SQLiteContextFactory.cs:24 |
| Extension | AddLogging | AddLogging ΓåÆ cfg => cfg.AddSimpleConsole(opts =>
... | SQLiteContextFactory.cs:17 |
| Extension | AddOptions | AddOptions ΓåÆ (AddOptions) | 
SQLiteContextFactory.cs:15 |
| Singleton | ILoggerProvider | ILoggerProvider ΓåÆ EfDbLoggerProvider | 
EfDbLoggerFactoryExtensions.cs:9 |
| Extension | AddOptions | AddOptions ΓåÆ StartupSettingsModel | 
ServicesRegistry.cs:37 |
| Extension | AddCustomizedAuthentication | AddCustomizedAuthentication ΓåÆ 
siteSettings | ServicesRegistry.cs:32 |
| Extension | AddCustomizedControllers | AddCustomizedControllers ΓåÆ 
(AddCustomizedControllers) | ServicesRegistry.cs:31 |
| Extension | AddSchedulers | AddSchedulers ΓåÆ (AddSchedulers) | 
ServicesRegistry.cs:29 |
| Extension | AddDNTCommonWeb | AddDNTCommonWeb ΓåÆ (AddDNTCommonWeb) | 
ServicesRegistry.cs:28 |
| Extension | AddCustomizedDataProtection | AddCustomizedDataProtection ΓåÆ 
siteSettings | ServicesRegistry.cs:27 |
| Extension | AddConfiguredDbContext | AddConfiguredDbContext ΓåÆ siteSettings | 
ServicesRegistry.cs:26 |
| Bulk | AutoInjectAllServices | [bulk auto-registration] | 
ServicesRegistry.cs:23 |
| Extension | AddIPrincipal | AddIPrincipal ΓåÆ (AddIPrincipal) | 
ServicesRegistry.cs:22 |
| Extension | AddHttpContextAccessor | AddHttpContextAccessor ΓåÆ 
(AddHttpContextAccessor) | ServicesRegistry.cs:21 |
| Extension | AddForwardedHeadersOptions | AddForwardedHeadersOptions ΓåÆ 
(AddForwardedHeadersOptions) | ServicesRegistry.cs:19 |
| Extension | AddOptions | AddOptions ΓåÆ configuration | ServicesRegistry.cs:18 |
| Extension | AddDNTScheduler | AddDNTScheduler ΓåÆ options =>
... | SchedulersConfig.cs:18 |
| Extension | AddProblemDetails | AddProblemDetails ΓåÆ (AddProblemDetails) | 
MvcControllersConfig.cs:9 |
| Extension | AddRequestTimeouts | AddRequestTimeouts ΓåÆ options =>
... | MvcControllersConfig.cs:9 |
| Extension | AddLargeFilesUploadSupport | AddLargeFilesUploadSupport ΓåÆ 
(AddLargeFilesUploadSupport) | MvcControllersConfig.cs:9 |
| Extension | AddOutputCache | AddOutputCache ΓåÆ options => { 
options.AddPolicy(AlwaysCachePolicy.Name, Al... | MvcControllersConfig.cs:9 |
| Extension | AddControllers | AddControllers ΓåÆ options =>
... | MvcControllersConfig.cs:9 |
| Extension | AddCustomJsonOptionsForWebApps | AddCustomJsonOptionsForWebApps ΓåÆ 
(AddCustomJsonOptionsForWebApps) | MvcControllersConfig.cs:9 |
| Extension | AddEFSecondLevelCache | AddEFSecondLevelCache ΓåÆ options => 
options.UseMemoryCacheProvider()
... | DbContextConfig.cs:38 |
| Extension | AddEfSecondLevelCacheInterceptor | 
AddEfSecondLevelCacheInterceptor ΓåÆ environment | DbContextConfig.cs:33 |
| Singleton | AuditableEntitiesInterceptor | AuditableEntitiesInterceptor | 
DbContextConfig.cs:32 |
| Singleton | EfExceptionsInterceptor | EfExceptionsInterceptor | 
DbContextConfig.cs:31 |
| Extension | AddConfiguredSqLiteDbContext | AddConfiguredSqLiteDbContext ΓåÆ 
(AddConfiguredSqLiteDbContext) | DbContextConfig.cs:24 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors ΓåÆ environment | 
DbContextConfig.cs:23 |
| Extension | AddDataProtection | AddDataProtection ΓåÆ (AddDataProtection) | 
DataProtectionConfig.cs:29 |
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
| Singleton | IXmlRepository | IXmlRepository ΓåÆ DataProtectionKeyService | 
DataProtectionConfig.cs:18 |
| Extension | AddAuthentication | AddAuthentication ΓåÆ options =>
... | AuthenticationConfig.cs:29 |
| Extension | AddCookie | AddCookie ΓåÆ options =>
... | AuthenticationConfig.cs:29 |
| Extension | AddAuthorization | AddAuthorization ΓåÆ options =>
... | AuthenticationConfig.cs:23 |
| Scoped | AuthenticationStateProvider | AuthenticationStateProvider ΓåÆ 
IdentityRevalidatingAuthenticationStateProvider | AuthenticationConfig.cs:21 |
| Extension | AddCascadingAuthenticationState | AddCascadingAuthenticationState 
ΓåÆ (AddCascadingAuthenticationState) | AuthenticationConfig.cs:20 |
| Extension | AddCustomizedServices | AddCustomizedServices ΓåÆ host | 
Program.cs:24 |
| Extension | AddControllers | AddControllers ΓåÆ (AddControllers) | Program.cs:23
|
| Extension | AddRazorComponents | AddRazorComponents ΓåÆ (AddRazorComponents) | 
Program.cs:22 |
| Extension | AddInteractiveServerComponents | AddInteractiveServerComponents ΓåÆ 
(AddInteractiveServerComponents) | Program.cs:22 |

## Related types

- **Presentation**: ExportsController, FeedController, FileController, 
FtsController, JavaScriptErrorsReportController, OpenSearchController, 
ProjectsFeedsController, SitemapController, UploadFileController, 
WelcomeController, BaseEntity, V2024_04_19_1424, AIDailyNewsBacklogsJob, 
AIDailyNewsJob, DisableInactiveUsersJob, AppFoldersService, 
AuditableEntitiesInterceptor, DataProtectionKeyService, EfDbLoggerProvider, 
EfExceptionsInterceptor, IdentityRevalidatingAuthenticationStateProvider, 
AccountModel, ActivatedAccountModel, Activation, AddAdvertisementForm, 
AddCustomSidebar, AddDailyNewsItemAIBacklogs, AddDailyNewsItemAIBacklogsHelp, 
AddGeneralAdvertisementModel, AddJobOfferAdvertisementForm, 
AddNewProjectAdvertisementModel, AdminsEmailsService, AdminUserDataSeeder, 
AdminUserSeedModel, AdvertisementBookmarkConfig, 
AdvertisementCommentBookmarkConfig, AdvertisementCommentConfig, 
AdvertisementCommentReactionConfig, AdvertisementCommentsService, 
AdvertisementCommentVisitorConfig

---
*Generated in 25.2ms | 1289 types (40 active, 1249 pruned) | Compression: 
TrivialMemberCompressor(ΓêÆ9%) ┬╖ StructuralDeduplicator(ΓêÆ15%) | Schema v1.1*

analyzed 1336 files ┬╖ 40 types kept of 1289 ┬╖ 3710/8000 tokens ┬╖ 22.6s stage2 
├ù2.0 stage3 ├ù2.1

                                Stage Timing                                 
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Stage                   Γöé    Time Γöé Bar                                   Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé DiscoveryAndCacheWarmup Γöé   153ms Γöé                                       Γöé
Γöé GenericExtraction       Γöé 20961ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê Γöé
Γöé SignalSealing           Γöé     1ms Γöé                                       Γöé
Γöé SpecificExtraction      Γöé  1380ms Γöé ΓûêΓûê                                    Γöé
Γöé Scoring                 Γöé    23ms Γöé                                       Γöé
Γöé Compression             Γöé    36ms Γöé                                       Γöé
Γöé Total                   Γöé 22592ms Γöé                                       Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                                   Extractors                                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Name                    Γöé    Time Γöé +Types Γöé +Dets Γöé Status                  Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé SyntaxStructureExtracto Γöé 20959ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé r                       Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé DiRegistrationExtractor Γöé 20959ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé EndpointExtractor       Γöé  1379ms Γöé      0 Γöé   103 Γöé ran                     Γöé
Γöé EfCoreExtractor         Γöé   698ms Γöé      0 Γöé   102 Γöé ran                     Γöé
Γöé InMemoryEventBusExtract Γöé   460ms Γöé      0 Γöé    77 Γöé ran                     Γöé
Γöé or                      Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé ControllerActionExtract Γöé   407ms Γöé      0 Γöé    71 Γöé ran                     Γöé
Γöé or                      Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé ProgramCsFlowExtractor  Γöé   212ms Γöé      0 Γöé    39 Γöé ran                     Γöé
Γöé FileTreeExtractor       Γöé    65ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé ProjectStructure        Γöé    56ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé SolutionDiscovery       Γöé    17ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé DependencyExtractor     Γöé    14ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé LayerClassifier         Γöé    13ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé AntiPatternDetector     Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé AspireExtractor         Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs aspire            Γöé
Γöé CallGraphExtractor      Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé EventBusExtractor       Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs masstransit or    Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé nservicebus             Γöé
Γöé IndirectWiringDetector  Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé MediatRExtractor        Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs mediatr           Γöé
Γöé SourceBodyExtractor     Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                   Scorer Funnel                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Scorer                 Γöé Before Γöé After Γöé Delta Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé PatternRelevancePruner Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γöé CallReachabilityPruner Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γöé PathProximityPruner    Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
cache 0% hit ┬╖ 1336 files ┬╖ 0 projects
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé  Metric  Γöé        Value        Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé Solution Γöé    _eval-dntsite    Γöé
Γöé   Time   Γöé       22807ms       Γöé
Γöé  Tokens  Γöé ~3710 (budget 8000) Γöé
Γöé Version  Γöé v1.0.5-preview.0.42 Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
