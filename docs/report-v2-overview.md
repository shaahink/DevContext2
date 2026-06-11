## DevContext — Overview on project

**Architecture**: NLayer (80% confidence)
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

- **Presentation**: JavaScriptErrorsReportController, SitemapController, FileController, FtsController, ProjectsFeedsController, UploadFileController, OpenSearchController, WelcomeController, BaseEntity, FeedController, ExportsController, V2024_05_18_1347

---
*Generated in 27.9ms | 1289 types (12 active, 1277 pruned) | Compression: TrivialMemberCompressor(−1%) · StructuralDeduplicator(−51%) | Schema v2.0.0*
