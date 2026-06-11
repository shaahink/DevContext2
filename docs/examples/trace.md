# Example: Trace an Endpoint

**Mode**: Trace — Entry-point focused with call graph, handler chain, and source code.

**Command**:
```bash
# Trace a specific controller's RSS feed endpoint
devcontext analyze . --scenario trace --around FeedController:SiteFeed

# Or let the intent auto-select the right mode
devcontext analyze . --task "trace the RSS feed generation"
```

---

## What the output looks like

### Entry Points

```markdown
### `FeedController` (Class, Presentation)
> DntSite.Web.Features.RssFeeds.Controllers.FeedController

**Extends**: `ControllerBase`

**Methods**:
- `Task<IActionResult> SiteFeed()`
- `Task<IActionResult> Posts()`
- `Task<IActionResult> Comments()`
- `Task<IActionResult> News()`
- ...
```

### Call Graph

```markdown
FeedController.SiteFeed
└─ FeedsService.GetFeedChannel (FeedsService.cs:570)
   ├─ FeedsService.IsPrivate (FeedsService.cs:564)
   ├─ BlogPostsService.GetLastBlogPostsIncludeAuthorTagsAsync (BlogPostsService.cs:107)
   │  └─ DbSet.AsNoTracking().Where().Include().OrderByDescending().Take().ToListAsync
   └─ CachedAppSettingsProvider.GetAppSettingsAsync (CachedAppSettingsProvider.cs:23)
      ├─ IServiceProvider.RunScopedServiceAsync
      └─ ILockerService.LockAsync
```

### Endpoints (filtered to nearby controllers)

```markdown
## Endpoints

**DntSite.Web** (41 endpoints near FeedController)

| Method | Route | Handler | Source |
|--------|-------|---------|--------|
| GET | /blog/rss.xml | FeedController.SiteFeed | FeedController.cs:92 |
| GET | /feed/rss | FeedController.SiteFeed | FeedController.cs:92 |
| GET | /rss.xml | FeedController.SiteFeed | FeedController.cs:92 |
| GET | /atom.xml | FeedController.SiteFeed | FeedController.cs:92 |
| GET | /llms.txt | FeedController.LlmsTxt | FeedController.cs:103 |
| GET | /Feed/Posts | FeedController.Posts | FeedController.cs:19 |
| GET | /Feed/Comments | FeedController.Comments | FeedController.cs:33 |
| GET | /ProjectsFeeds/ProjectsNews | ProjectsFeedsController.ProjectsNews | ProjectsFeedsController.cs:17 |
| ... | ... | ... | ... |
```

---

## What this tells the LLM

- **FeedController** has 19 endpoints, 9 of which are alternate routes for `SiteFeed` (RSS, Atom, blog feed)
- **Call graph** traces from `SiteFeed` → `FeedsService` → `BlogPostsService` → database via EF Core
- `CachedAppSettingsProvider` is pulled in through `IServiceProvider.RunScopedServiceAsync` — potential service locator pattern detected
- Endpoints table is **filtered to 41** from the full 70, showing only controllers within the `RssFeeds` feature directory
- The 9 alternate routes (`/blog/rss.xml`, `/feed/rss`, `/rss.xml`, `/atom.xml`, etc.) are all listed because `ExtractActionRoutes` now collects all `[Route]` attributes per action

---

## Profile variations

| Profile flag | What you get extra |
|-------------|--------------------|
| (default) | Entry points, filtered endpoints, DI wiring, data model |
| `--profile debug` | Call graph with BFS traversal up to depth 5 |
| `--profile full` | Source code bodies for entry point + call graph types |
