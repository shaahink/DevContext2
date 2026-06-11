# Example: Architecture Overview of a Multi-Project Solution

**Mode**: Overview — Get a comprehensive structural overview of any .NET solution.

**Command**:
```bash
devcontext analyze . --scenario overview
```

---

## What the output looks like

### Architecture Classification

```markdown
## DevContext — Overview on project

**Architecture**: ControllerBased (80% confidence)
**Signals**: controllers · efcore · minimal-apis
**Projects**: 3 — DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: focused | **Tokens**: ~6,273 (budget 8000) | **Types**: 12 in output
```

### Project Dependency Tree

```markdown
└── DntSite.Tests
    └── DntSite.Web
        └── DntSite.Web.Common.BlazorSsr
```

### Endpoints (70 found)

```markdown
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| GET | /Feed | FeedController.Index | - | FeedController.cs:15 |
| GET | /blog/rss.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /rss.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /atom.xml | FeedController.SiteFeed | - | FeedController.cs:92 |
| GET | /llms.txt | FeedController.LlmsTxt | - | FeedController.cs:103 |
| GET | /llms-full.txt | FeedController.LlmsFull | - | FeedController.cs:106 |
| GET | /ProjectsFeeds/ProjectsNews | ProjectsFeedsController.ProjectsNews | - | ProjectsFeedsController.cs:17 |
| GET | /Exports/{type}/{name}.pdf | ExportsController.Get | - | ExportsController.cs:13 |
| GET | /File/Avatar | FileController.Avatar | - | FileController.cs:18 |
| POST | /UploadFile | UploadFileController.ImageUpload | - | UploadFileController.cs:18 |
| GET | /sitemap.xml | SitemapController.Get | - | SitemapController.cs:12 |
| POST | /Fts | FtsController.Log | - | FtsController.cs:48 |
| GET | /Welcome | WelcomeController.Log | - | WelcomeController.cs:12 |
| ... | ... | ... | ... | ... |
```

### Data Model (EF Core)

```markdown
### ApplicationDbContext

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| BaseEntity | — | Id |
| <OnModelCreating> | — | — |

### Migrations (30 found)

| V2024_04_19_1424 | V2024_05_18_1347 | ... | V2026_06_03_1205 |
```

### Background Workers (24 found)

```markdown
### Background workers

- DotNetVersionCheckJob (HostedService)
- BackupDatabaseJob (HostedService)
- DailyNewsletterJob (HostedService)
- FullTextSearchWriterJob (HostedService)
- DraftsJob (HostedService)
- AIDailyNewsJob (HostedService)
...and 18 more
```

### DI Registrations

```markdown
### DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Bulk | AutoInjectAllServices | [bulk auto-registration] | ServicesRegistry.cs:23 |
| Singleton | IXmlRepository → DataProtectionKeyService | DataProtectionConfig.cs:18 |
| Singleton | AuditableEntitiesInterceptor | DbContextConfig.cs:32 |
| Scoped | AuthenticationStateProvider → IdentityRevalidatingAuthenticationStateProvider | AuthenticationConfig.cs:21 |
| ... | ... | ... | ... |
```

### Middleware Pipeline

```markdown
| Type | Kind | Count | Sources |
|------|------|-------|---------|
| UseForwardedHeaders | UseX | 1 | Program.cs |
| UseExceptionHandler | UseX | 1 | Program.cs |
| UseAntiDos | UseX | 1 | Program.cs |
| UseCsp | UseX | 1 | Program.cs |
| UseHttpsRedirection | UseX | 1 | Program.cs |
| UseAuthentication | UseX | 1 | Program.cs |
| UseAuthorization | UseX | 1 | Program.cs |
| UseAntiforgery | UseX | 1 | Program.cs |
| UseOutputCache | UseX | 1 | Program.cs |
| UseRequestTimeouts | UseX | 1 | Program.cs |
```

---

## What this tells the LLM

- **20 extractors ran**, producing 186 detections from 1,289 types
- All 10 controllers are found with their routes, not just the ones using `[HttpGet]`/`[HttpPost]` attributes
- 24 scheduled jobs (DNTScheduler) are detected and listed
- `AutoInjectAllServices` pattern is recognized as bulk DI registration
- 30 EF Core migrations are grouped together under a single entry
- Middleware pipeline is fully captured in order
- Architecture is correctly identified as `ControllerBased`, not MinimalApi
