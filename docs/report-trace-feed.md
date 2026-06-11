## DevContext — Trace on project

**Architecture**: MinimalApi (80% confidence)
**Signals**: controllers · minimal-apis · efcore
**Projects**: 3 — DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 8 in output

---
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

**DntSite.Web** (13 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| POST | /UploadFile | UploadFileController.MessagesFilesUpload | - | UploadFileController.cs:42 |
| POST | /UploadFile | UploadFileController.CommonFilesUpload | - | UploadFileController.cs:38 |
| POST | /UploadFile | UploadFileController.FileUpload | - | UploadFileController.cs:34 |
| POST | /UploadFile | UploadFileController.CourseFileUpload | - | UploadFileController.cs:30 |
| POST | /UploadFile | UploadFileController.CourseImagesUpload | - | UploadFileController.cs:26 |
| POST | /UploadFile | UploadFileController.MessagesImagesUpload | - | UploadFileController.cs:22 |
| POST | /UploadFile | UploadFileController.ImageUpload | - | UploadFileController.cs:18 |
| GET | /Welcome | WelcomeController.Log | - | WelcomeController.cs:12 |
| GET | /OpenSearch | OpenSearchController.RenderOpenSearch | - | OpenSearchController.cs:13 |
| POST | /Fts | FtsController.Log | - | FtsController.cs:48 |
| GET | /Fts | FtsController.Search | - | FtsController.cs:19 |
| POST | /JavaScriptErrorsReport | JavaScriptErrorsReportController.Log | - | JavaScriptErrorsReportController.cs:16 |
| GET | /.well-known/change-password | λ ChangePasswordEndpoint.cs:9 | - | ChangePasswordEndpoint.cs:9 |

## Call graph

Not available in current profile. Re-run with `--profile debug` to enable call graph extraction and BFS reachability analysis from entry points.

## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<OnModelCreating>` | — | — |

### `V2024_04_19_1424`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_05_18_1347`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_06_19_2139`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_06_25_2320`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_06_27_2036`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_06_28_1257`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_06_30_2030`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_07_17_1405`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_07_19_2106`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_07_19_2211`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_07_19_2234`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_08_12_1323`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_09_28_1204`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_09_28_1327`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_10_05_1343`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_10_05_1417`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_10_18_1302`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_10_19_2133`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_10_30_1357`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2024_11_17_1942`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2025_12_31_2127`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_01_11_1104`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_01_11_1910`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_01_28_2352`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_02_03_0009`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_03_16_1155`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_04_02_1410`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_04_21_1348`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_05_06_1053`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_05_07_1000`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

### `V2026_06_03_1205`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `<Migration>` | — | — |

## Non-obvious wiring

### Indirect wiring

| Kind | Caller | Target |
|------|--------|--------|
| ManualServiceLocator | DntSitePageTitle.AddToSiteUrlsBackgroundQueueAsync | unknown |
| ManualServiceLocator | DntSitePageTitle.AddToSiteUrlsBackgroundQueueAsync | unknown |
| ManualServiceLocator | DntSitePageTitle.AddToSiteUrlsBackgroundQueueAsync | unknown |
| ReflectionActivation | DbSetsConfigs.RegisterAllDerivedEntities | GetTypes |
| ManualServiceLocator | SqLiteServiceCollectionExtensions.UseConfiguredSqLite | unknown |
| ManualServiceLocator | SqLiteServiceCollectionExtensions.UseConfiguredSqLite | unknown |
| ManualServiceLocator | SqLiteServiceCollectionExtensions.UseConfiguredSqLite | unknown |
| ManualServiceLocator | SqLiteServiceCollectionExtensions.UseConfiguredSqLite | unknown |
| ManualServiceLocator | SqLiteServiceCollectionExtensions.AddConfiguredSqLiteDbContext | unknown |
| ManualServiceLocator | DataSeedersRunner.RunAllDataSeeders | unknown |
| ManualServiceLocator | EfDbLoggerProvider.AddLogItem | unknown |
| ManualServiceLocator | EfDbLogger.Log | unknown |

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
| Singleton | ILoggerProvider | ILoggerProvider → EfDbLoggerProvider | EfDbLoggerFactoryExtensions.cs:9 |

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Info | CallReachabilityPruner | CallGraph not available; skipping reachability analysis. |
| Info | Pipeline | Scenario 'Trace' benefits from call graph. Re-run with '--profile debug' to enable call graph. |
| Info | SolutionDiscovery | No .sln file found |

### Pruning notes

- PatternRelevancePruner: pruned test type 'DntSite.Tests.RaviAiParserTests'

---
*Generated in 29.6ms | 1289 types (8 active, 1281 pruned) | Schema v2.0.0*
