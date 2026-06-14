Slicing from BlogPost, call graph on.
Analyzing project...

## DevContext ΓÇö Slice on project

**Architecture**: ControllerBased (80% confidence)
**Signals**: controllers ┬╖ minimal-apis ┬╖ efcore
**Projects**: 3 ΓÇö DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: debug | **Tokens**: ~2434 (budget 8000) | **Types**: 25 in output

---
## Entry points

### `BlogPost` (Class, Presentation)
> `DntSite.Web.Features.Posts.Entities.BlogPost` ΓÇö 
C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Posts\Entities\BlogPo
st.cs

**Extends**: `BaseInteractiveEntity<BlogPost, BlogPostVisitor, BlogPostBookmark,
BlogPostReaction, BlogPostTag
    , BlogPostComment, BlogPostCommentVisitor, BlogPostCommentBookmark, 
BlogPostCommentReaction, BlogPostUserFile,
    BlogPostUserFileVisitor>`

## Endpoints

No endpoints detected.

## Call graph

```text
**DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.Equals**
Γö£ΓöÇ `string.Equals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:117)`
Γö£ΓöÇ `string.Equals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:116)`
Γö£ΓöÇ `y.GetType` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:111)`
Γö£ΓöÇ `x.GetType` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:111)`
Γö£ΓöÇ `DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.ReferenceEquals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:96)`
Γö£ΓöÇ `DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.nameof` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:76)`
ΓööΓöÇ `DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.Equals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:73)`

```
## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `BaseEntity` | ΓÇö | Id |

**31 EF Core migrations found.**

## Background workers

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

---
*Generated in 39.3ms | 1289 types (25 active, 1264 pruned) | Compression: 
TrivialMemberCompressor(ΓêÆ9%) ┬╖ StructuralDeduplicator(ΓêÆ15%) | Schema v1.1*

analyzed 1336 files ┬╖ 25 types kept of 1289 ┬╖ 2440/8000 tokens ┬╖ 18.9s stage2 
├ù2.0 stage3 ├ù2.2

                         Stage Timing                          
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Stage                   Γöé    Time Γöé Bar                     Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé DiscoveryAndCacheWarmup Γöé   160ms Γöé                         Γöé
Γöé GenericExtraction       Γöé  7314ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê         Γöé
Γöé SignalSealing           Γöé     2ms Γöé                         Γöé
Γöé SpecificExtraction      Γöé 11191ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê Γöé
Γöé Scoring                 Γöé   110ms Γöé                         Γöé
Γöé Compression             Γöé    37ms Γöé                         Γöé
Γöé Total                   Γöé 18883ms Γöé                         Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                                   Extractors                                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Name                    Γöé    Time Γöé +Types Γöé +Dets Γöé Status                  Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé CallGraphExtractor      Γöé 11187ms Γöé      0 Γöé   115 Γöé ran                     Γöé
Γöé IndirectWiringDetector  Γöé  8941ms Γöé      0 Γöé   115 Γöé ran                     Γöé
Γöé SyntaxStructureExtracto Γöé  7310ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé r                       Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé DiRegistrationExtractor Γöé  7305ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé EndpointExtractor       Γöé  2528ms Γöé      0 Γöé   106 Γöé ran                     Γöé
Γöé EfCoreExtractor         Γöé  1102ms Γöé      0 Γöé   105 Γöé ran                     Γöé
Γöé InMemoryEventBusExtract Γöé   718ms Γöé      0 Γöé    80 Γöé ran                     Γöé
Γöé or                      Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé ControllerActionExtract Γöé   654ms Γöé      0 Γöé    73 Γöé ran                     Γöé
Γöé or                      Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé ProgramCsFlowExtractor  Γöé   252ms Γöé      0 Γöé    39 Γöé ran                     Γöé
Γöé FileTreeExtractor       Γöé   105ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé DependencyExtractor     Γöé    28ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé LayerClassifier         Γöé    28ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé ProjectStructure        Γöé    25ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé SolutionDiscovery       Γöé    22ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé AntiPatternDetector     Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé AspireExtractor         Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs aspire            Γöé
Γöé EventBusExtractor       Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs masstransit or    Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé nservicebus             Γöé
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
Γöé   Time   Γöé       19073ms       Γöé
Γöé  Tokens  Γöé ~2440 (budget 8000) Γöé
Γöé Version  Γöé v1.0.5-preview.0.42 Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
