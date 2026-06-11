## DevContext — Overview on project

**Architecture**: NLayer (80% confidence)
**Signals**: controllers · minimal-apis · efcore
**Projects**: 3 — DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 10 in output

---
## Architecture overview

└── DntSite.Tests
    └── DntSite.Web
        └── DntSite.Web.Common.BlazorSsr

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

## Related types grouped by layer

- **Presentation**: AffectedColumn, WriteLearningPathsHelp, JavaScriptErrorsReportController, FtsController, UploadFileController, OpenSearchController, EfDbLoggerProvider, WelcomeController, BaseEntity, V2024_04_19_1424

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Info | CallReachabilityPruner | CallGraph not available; skipping reachability analysis. |
| Info | EfCoreExtractor | ApplicationDbContext uses ApplyConfigurationsFromAssembly(typeof(AppDataProtectionKeyConfig).Assembly) for entity discovery. |
| Info | SolutionDiscovery | No .sln file found |

### Pruning notes

- PatternRelevancePruner: pruned test type 'DntSite.Tests.RaviAiParserTests'
- TokenBudgetEnforcer: kept 95 types (1193 pruned for budget 7500)
- TokenBudgetEnforcer: capped at 40 types (scenario limit)

---
*Generated in 31.7ms | 1289 types (10 active, 1279 pruned) | Compression: TrivialMemberCompressor(−2%) · StructuralDeduplicator(−69%) | Schema v2.0.0*
