# REPORT
**DntSite**

Style: ControllerBased
_2 projects  ·  70 HttpEndpoint, 24 ScheduledJob  ·  net10.0 + controllers + minimal-apis + efcore_

## Stats

| Metric | Value |
|--------|-------|
| Files | 1342 |
| Projects | 3 |
| Nodes | 4965 |
| Edges | 2160 |
| Entries | 94 |
| With target | 93/94 |
| Verified edges | 86% |
| Analyzed in | 17.9s |

## Top Flows

1. **GET /Feed/GetLatestChangesAsync** → `FeedsService.GetLatestChangesAsync` *(HttpEndpoint)*
2. **GET /Feed/LatestChanges** → `FeedController` *(HttpEndpoint)*
3. **GET /llms-full.txt** → `FeedController` *(HttpEndpoint)*
4. **GET /llms.txt** → `FeedController` *(HttpEndpoint)*
5. **GET /atom.xml** → `FeedController` *(HttpEndpoint)*
6. **GET /blog/feed** → `FeedController` *(HttpEndpoint)*
7. **GET /blog/rss.xml** → `FeedController` *(HttpEndpoint)*
8. **GET /feed.xml** → `FeedController` *(HttpEndpoint)*
9. **GET /feed/atom** → `FeedController` *(HttpEndpoint)*
10. **GET /feed/rss** → `FeedController` *(HttpEndpoint)*

### Trace 1: GET /Feed/GetLatestChangesAsync

TRACE  GET /Feed/GetLatestChangesAsync
       src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:110

▸ ENTRY  GET /Feed/GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:110)
   └─ call FeedController.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:110)
          private async Task<WhatsNewFeedChannel> GetLatestChangesAsync()
          => await feedsService.GetLatestChangesAsync(await ShowBriefDescriptionAsync());
      ├─ call FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
      │      public async Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30)
      │      var result = new List<WhatsNewItemModel>();
      │      result.AddRange((await GetProjectsNewsAsync(showBriefDescription)).RssItems ?? []);
      │  (15 more branches omitted beyond fan-out)
      │  ├─ call FeedsService.GetProjectsNewsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:57) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetProjectsNewsAsync(bool showBriefDescription,
      │  │      int pageNumber = 0,
      │  │      int recordsPerPage = 15,
      │  │  ├─ call ProjectsService.GetPagedProjectItemsIncludeUserAndTagsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:274) [verified]
      │  │  │      public Task<PagedResultModel<Project>> GetPagedProjectItemsIncludeUserAndTagsAsync(int pageNumber,
      │  │  │      int recordsPerPage = 15,
      │  │  │      bool showDeletedItems = false,
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:277) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [verified]
      │  │  │         public async Task<AppSetting> GetAppSettingsAsync()
      │  │  │         if (_appSetting is not null)
      │  │  │         return _appSetting;
      │  │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:280) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:285) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  ├─ call FeedsService.GetProjectsFilesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:58) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetProjectsFilesAsync(bool showBriefDescription,
      │  │      int pageNumber = 0,
      │  │      int recordsPerPage = 15,
      │  │  ├─ call ProjectReleasesService.GetAllProjectsReleasesIncludeProjectsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:295) [verified]
      │  │  │      public Task<PagedResultModel<ProjectRelease>> GetAllProjectsReleasesIncludeProjectsAsync(int pageNumber,
      │  │  │      int recordsPerPage = 15,
      │  │  │      bool showDeletedItems = false,
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:298) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  (stopped at depth 4; 1 branch omitted)
      │  │  ├─ call BlogPost.MapToProjectsReleasesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:301) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:306) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  ├─ call FeedsService.GetProjectsIssuesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:59) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetProjectsIssuesAsync(bool showBriefDescription,
      │  │      int pageNumber = 0,
      │  │      int recordsPerPage = 8,
      │  │  ├─ call ProjectIssuesService.GetLastPagedAllProjectsIssuesAsNoTrackingAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:316) [verified]
      │  │  │      public Task<PagedResultModel<ProjectIssue>> GetLastPagedAllProjectsIssuesAsNoTrackingAsync(int pageNumber,
      │  │  │      int recordsPerPage = 8,
      │  │  │      bool showDeletedItems = false,
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:319) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  (stopped at depth 4; 1 branch omitted)
      │  │  ├─ call BlogPost.MapToProjectsIssuesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:322) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:327) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  ├─ call FeedsService.GetProjectsIssuesRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:60) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetProjectsIssuesRepliesAsync(bool showBriefDescription,
      │  │      int count = 15,
      │  │      bool showDeletedItems = false)
      │  │  ├─ call ProjectIssueCommentsService.GetLastIssueCommentsIncludeBlogPostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:334) [verified]
      │  │  │      public Task<List<ProjectIssueComment>>
      │  │  │      GetLastIssueCommentsIncludeBlogPostAndUserAsync(int count, bool showDeletedItems = false)
      │  │  │      => _projectIssueComments.AsNoTracking()
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:337) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  (stopped at depth 4; 1 branch omitted)
      │  │  ├─ call BlogPost.MapToProjectsIssuesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:340) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:345) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  ├─ call FeedsService.GetProjectsFaqsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:61) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetProjectsFaqsAsync(bool showBriefDescription,
      │  │      int pageNumber = 0,
      │  │      int recordsPerPage = 10,
      │  │  ├─ call ProjectFaqsService.GetAllLastPagedProjectFaqsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:386) [verified]
      │  │  │      public Task<List<ProjectFaq>> GetAllLastPagedProjectFaqsAsync(int pageNumber = 0,
      │  │  │      int recordsPerPage = 10,
      │  │  │      bool showDeletedItems = false)
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:389) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  (stopped at depth 4; 1 branch omitted)
      │  │  ├─ call BlogPost.MapToProjectsFaqsWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:392) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:397) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  ├─ call FeedsService.GetPostsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:62) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetPostsAsync(bool showBriefDescription,
      │  │      int count = 15,
      │  │      bool showDeletedItems = false)
      │  │  ├─ call BlogPostsService.GetLastBlogPostsIncludeAuthorTagsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:561) [verified]
      │  │  │      public Task<List<BlogPost>> GetLastBlogPostsIncludeAuthorTagsAsync(int count, bool showDeletedItems = false)
      │  │  │      => _blogPosts.AsNoTracking()
      │  │  │      .Where(x => x.IsDeleted == showDeletedItems)
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:562) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  (stopped at depth 4; 1 branch omitted)
      │  │  ├─ call FeedsService.IsPrivate  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:564) [verified]
      │  │  │      private static bool IsPrivate(BlogPost item) => item.NumberOfRequiredPoints is > 0;
      │  │  ├─ call BlogPost.MapToPostWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:565) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:570) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  ├─ call FeedsService.GetCommentsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:63) [verified]
      │  │      public async Task<WhatsNewFeedChannel> GetCommentsAsync(bool showBriefDescription,
      │  │      int count = 15,
      │  │      bool showDeletedItems = false)
      │  │  ├─ call BlogCommentsService.GetLastBlogCommentsIncludeBlogPostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:577) [verified]
      │  │  │      public Task<List<BlogPostComment>>
      │  │  │      GetLastBlogCommentsIncludeBlogPostAndUserAsync(int count, bool showDeletedItems = false)
      │  │  │      => _blogComments.AsNoTracking()
      │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:578) [verified]
      │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
      │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │  │  │      BlogName = "DNT",
      │  │  │  (stopped at depth 4; 1 branch omitted)
      │  │  ├─ call FeedsService.IsPrivateComment  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:580) [verified]
      │  │  │      private static bool IsPrivateComment(BlogPostComment item) => item.Parent.NumberOfRequiredPoints is > 0;
      │  │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:581) [approx]
      │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:586) [verified]
      │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
      │  │         AppSetting appSetting,
      │  │         List<WhatsNewItemModel> rssItems)
      │  └─ call FeedsService.GetNewsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:64) [verified]
      │         public async Task<WhatsNewFeedChannel> GetNewsAsync(bool showBriefDescription,
      │         int count = 15,
      │         bool showDeletedItems = false)
      │     ├─ call DailyNewsItemsService.GetLastDailyNewsItemsIncludeUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:593) [verified]
      │     │      public Task<List<DailyNewsItem>> GetLastDailyNewsItemsIncludeUserAsync(int count, bool showDeletedItems = false)
      │     │      => _dailyNewsItem.AsNoTracking()
      │     │      .Where(x => x.IsDeleted == showDeletedItems)
      │     ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:594) [verified]
      │     │      private async Task<AppSetting> GetAppSettingsAsync()
      │     │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
      │     │      BlogName = "DNT",
      │     │  (stopped at depth 4; 1 branch omitted)
      │     ├─ call BlogPost.MapToNewsWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:596) [approx]
      │     ├─ call DailyNewsScreenshotsService.GetNewsThumbImage  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:597) [verified]
      │     │      public string GetNewsThumbImage(DailyNewsItem? item, string siteRootUri)
      │     │      if (item is null || string.IsNullOrWhiteSpace(siteRootUri))
      │     │      return string.Empty;
      │     │  ├─ call DailyNewsScreenshotsService.GetThumbnailImageInfo  (src/DntSite.Web/Features/News/Services/DailyNewsScreenshotsService.cs:109) [verified]
      │     │  │      public (string FileName, string Path) GetThumbnailImageInfo(int id)
      │     │  │      var name = string.Create(CultureInfo.InvariantCulture, $"news-{id}.jpg");
      │     │  │      var path = appFoldersService.ThumbnailsServiceFolderPath.SafePathCombine(name);
      │     │  │  (stopped at depth 5; 1 branch omitted)
      │     │  └─ call File.Exists  (src/DntSite.Web/Features/News/Services/DailyNewsScreenshotsService.cs:111) [approx]
      │     └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:602) [verified]
      │            private static WhatsNewFeedChannel GetFeedChannel(string title,
      │            AppSetting appSetting,
      │            List<WhatsNewItemModel> rssItems)
      └─ call FeedController.ShowBriefDescriptionAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
             private async Task<bool> ShowBriefDescriptionAsync()
             var appSetting = await cachedAppSettingsProvider.GetAppSettingsAsync();
             return appSetting.ShowRssBriefDescription;
         └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [verified]
                public async Task<AppSetting> GetAppSettingsAsync()
                if (_appSetting is not null)
                return _appSetting;
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: GET /Feed/LatestChanges

TRACE  GET /Feed/LatestChanges
       src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89

▸ ENTRY  GET /Feed/LatestChanges  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89)
   └─ call FeedController.LatestChanges  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89)
          public async Task<IActionResult> LatestChanges()
          => new FeedResult<WhatsNewItemModel>(await GetLatestChangesAsync());
      └─ call FeedController.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:90) [verified]
             private async Task<WhatsNewFeedChannel> GetLatestChangesAsync()
             => await feedsService.GetLatestChangesAsync(await ShowBriefDescriptionAsync());
         ├─ call FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
         │      public async Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30)
         │      var result = new List<WhatsNewItemModel>();
         │      result.AddRange((await GetProjectsNewsAsync(showBriefDescription)).RssItems ?? []);
         │  (15 more branches omitted beyond fan-out)
         │  ├─ call FeedsService.GetProjectsNewsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:57) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsNewsAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 15,
         │  │  ├─ call ProjectsService.GetPagedProjectItemsIncludeUserAndTagsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:274) [verified]
         │  │  │      public Task<PagedResultModel<Project>> GetPagedProjectItemsIncludeUserAndTagsAsync(int pageNumber,
         │  │  │      int recordsPerPage = 15,
         │  │  │      bool showDeletedItems = false,
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:277) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:280) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:285) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsFilesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:58) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsFilesAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 15,
         │  │  ├─ call ProjectReleasesService.GetAllProjectsReleasesIncludeProjectsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:295) [verified]
         │  │  │      public Task<PagedResultModel<ProjectRelease>> GetAllProjectsReleasesIncludeProjectsAsync(int pageNumber,
         │  │  │      int recordsPerPage = 15,
         │  │  │      bool showDeletedItems = false,
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:298) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsReleasesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:301) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:306) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsIssuesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:59) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsIssuesAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 8,
         │  │  ├─ call ProjectIssuesService.GetLastPagedAllProjectsIssuesAsNoTrackingAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:316) [verified]
         │  │  │      public Task<PagedResultModel<ProjectIssue>> GetLastPagedAllProjectsIssuesAsNoTrackingAsync(int pageNumber,
         │  │  │      int recordsPerPage = 8,
         │  │  │      bool showDeletedItems = false,
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:319) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsIssuesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:322) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:327) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsIssuesRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:60) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsIssuesRepliesAsync(bool showBriefDescription,
         │  │      int count = 15,
         │  │      bool showDeletedItems = false)
         │  │  ├─ call ProjectIssueCommentsService.GetLastIssueCommentsIncludeBlogPostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:334) [verified]
         │  │  │      public Task<List<ProjectIssueComment>>
         │  │  │      GetLastIssueCommentsIncludeBlogPostAndUserAsync(int count, bool showDeletedItems = false)
         │  │  │      => _projectIssueComments.AsNoTracking()
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:337) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsIssuesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:340) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:345) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsFaqsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:61) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsFaqsAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 10,
         │  │  ├─ call ProjectFaqsService.GetAllLastPagedProjectFaqsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:386) [verified]
         │  │  │      public Task<List<ProjectFaq>> GetAllLastPagedProjectFaqsAsync(int pageNumber = 0,
         │  │  │      int recordsPerPage = 10,
         │  │  │      bool showDeletedItems = false)
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:389) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsFaqsWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:392) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:397) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetPostsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:62) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetPostsAsync(bool showBriefDescription,
         │  │      int count = 15,
         │  │      bool showDeletedItems = false)
         │  │  ├─ call BlogPostsService.GetLastBlogPostsIncludeAuthorTagsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:561) [verified]
         │  │  │      public Task<List<BlogPost>> GetLastBlogPostsIncludeAuthorTagsAsync(int count, bool showDeletedItems = false)
         │  │  │      => _blogPosts.AsNoTracking()
         │  │  │      .Where(x => x.IsDeleted == showDeletedItems)
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:562) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call FeedsService.IsPrivate  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:564) [verified]
         │  │  │      private static bool IsPrivate(BlogPost item) => item.NumberOfRequiredPoints is > 0;
         │  │  ├─ call BlogPost.MapToPostWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:565) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:570) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetCommentsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:63) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetCommentsAsync(bool showBriefDescription,
         │  │      int count = 15,
         │  │      bool showDeletedItems = false)
         │  │  ├─ call BlogCommentsService.GetLastBlogCommentsIncludeBlogPostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:577) [verified]
         │  │  │      public Task<List<BlogPostComment>>
         │  │  │      GetLastBlogCommentsIncludeBlogPostAndUserAsync(int count, bool showDeletedItems = false)
         │  │  │      => _blogComments.AsNoTracking()
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:578) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call FeedsService.IsPrivateComment  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:580) [verified]
         │  │  │      private static bool IsPrivateComment(BlogPostComment item) => item.Parent.NumberOfRequiredPoints is > 0;
         │  │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:581) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:586) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  └─ call FeedsService.GetNewsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:64) [verified]
         │         public async Task<WhatsNewFeedChannel> GetNewsAsync(bool showBriefDescription,
         │         int count = 15,
         │         bool showDeletedItems = false)
         │     ├─ call DailyNewsItemsService.GetLastDailyNewsItemsIncludeUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:593) [verified]
         │     │      public Task<List<DailyNewsItem>> GetLastDailyNewsItemsIncludeUserAsync(int count, bool showDeletedItems = false)
         │     │      => _dailyNewsItem.AsNoTracking()
         │     │      .Where(x => x.IsDeleted == showDeletedItems)
         │     ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:594) [verified]
         │     │      private async Task<AppSetting> GetAppSettingsAsync()
         │     │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │     │      BlogName = "DNT",
         │     │  (stopped at depth 5; 1 branch omitted)
         │     ├─ call BlogPost.MapToNewsWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:596) [approx]
         │     ├─ call DailyNewsScreenshotsService.GetNewsThumbImage  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:597) [verified]
         │     │      public string GetNewsThumbImage(DailyNewsItem? item, string siteRootUri)
         │     │      if (item is null || string.IsNullOrWhiteSpace(siteRootUri))
         │     │      return string.Empty;
         │     │  (stopped at depth 5; 2 branches omitted)
         │     └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:602) [verified]
         │            private static WhatsNewFeedChannel GetFeedChannel(string title,
         │            AppSetting appSetting,
         │            List<WhatsNewItemModel> rssItems)
         └─ call FeedController.ShowBriefDescriptionAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
                private async Task<bool> ShowBriefDescriptionAsync()
                var appSetting = await cachedAppSettingsProvider.GetAppSettingsAsync();
                return appSetting.ShowRssBriefDescription;
            └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [verified]
                   public async Task<AppSetting> GetAppSettingsAsync()
                   if (_appSetting is not null)
                   return _appSetting;
RESULT   200 OK · failure → 404 Not Found

---

### Trace 3: GET /llms-full.txt

TRACE  GET /llms-full.txt
       src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:106

▸ ENTRY  GET /llms-full.txt  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:106)
   └─ call FeedController.LlmsFull  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:106)
          [Microsoft.AspNetCore.Mvc.Route(template: "/llms-full.txt")]
          public async Task<IActionResult> LlmsFull()
          => new LlmsFullTxtResult<WhatsNewItemModel>(await GetLatestChangesAsync());
      └─ call FeedController.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:108) [verified]
             private async Task<WhatsNewFeedChannel> GetLatestChangesAsync()
             => await feedsService.GetLatestChangesAsync(await ShowBriefDescriptionAsync());
         ├─ call FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
         │      public async Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30)
         │      var result = new List<WhatsNewItemModel>();
         │      result.AddRange((await GetProjectsNewsAsync(showBriefDescription)).RssItems ?? []);
         │  (15 more branches omitted beyond fan-out)
         │  ├─ call FeedsService.GetProjectsNewsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:57) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsNewsAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 15,
         │  │  ├─ call ProjectsService.GetPagedProjectItemsIncludeUserAndTagsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:274) [verified]
         │  │  │      public Task<PagedResultModel<Project>> GetPagedProjectItemsIncludeUserAndTagsAsync(int pageNumber,
         │  │  │      int recordsPerPage = 15,
         │  │  │      bool showDeletedItems = false,
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:277) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:280) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:285) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsFilesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:58) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsFilesAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 15,
         │  │  ├─ call ProjectReleasesService.GetAllProjectsReleasesIncludeProjectsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:295) [verified]
         │  │  │      public Task<PagedResultModel<ProjectRelease>> GetAllProjectsReleasesIncludeProjectsAsync(int pageNumber,
         │  │  │      int recordsPerPage = 15,
         │  │  │      bool showDeletedItems = false,
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:298) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsReleasesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:301) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:306) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsIssuesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:59) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsIssuesAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 8,
         │  │  ├─ call ProjectIssuesService.GetLastPagedAllProjectsIssuesAsNoTrackingAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:316) [verified]
         │  │  │      public Task<PagedResultModel<ProjectIssue>> GetLastPagedAllProjectsIssuesAsNoTrackingAsync(int pageNumber,
         │  │  │      int recordsPerPage = 8,
         │  │  │      bool showDeletedItems = false,
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:319) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsIssuesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:322) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:327) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsIssuesRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:60) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsIssuesRepliesAsync(bool showBriefDescription,
         │  │      int count = 15,
         │  │      bool showDeletedItems = false)
         │  │  ├─ call ProjectIssueCommentsService.GetLastIssueCommentsIncludeBlogPostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:334) [verified]
         │  │  │      public Task<List<ProjectIssueComment>>
         │  │  │      GetLastIssueCommentsIncludeBlogPostAndUserAsync(int count, bool showDeletedItems = false)
         │  │  │      => _projectIssueComments.AsNoTracking()
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:337) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsIssuesWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:340) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:345) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetProjectsFaqsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:61) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetProjectsFaqsAsync(bool showBriefDescription,
         │  │      int pageNumber = 0,
         │  │      int recordsPerPage = 10,
         │  │  ├─ call ProjectFaqsService.GetAllLastPagedProjectFaqsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:386) [verified]
         │  │  │      public Task<List<ProjectFaq>> GetAllLastPagedProjectFaqsAsync(int pageNumber = 0,
         │  │  │      int recordsPerPage = 10,
         │  │  │      bool showDeletedItems = false)
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:389) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call BlogPost.MapToProjectsFaqsWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:392) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:397) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetPostsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:62) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetPostsAsync(bool showBriefDescription,
         │  │      int count = 15,
         │  │      bool showDeletedItems = false)
         │  │  ├─ call BlogPostsService.GetLastBlogPostsIncludeAuthorTagsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:561) [verified]
         │  │  │      public Task<List<BlogPost>> GetLastBlogPostsIncludeAuthorTagsAsync(int count, bool showDeletedItems = false)
         │  │  │      => _blogPosts.AsNoTracking()
         │  │  │      .Where(x => x.IsDeleted == showDeletedItems)
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:562) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call FeedsService.IsPrivate  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:564) [verified]
         │  │  │      private static bool IsPrivate(BlogPost item) => item.NumberOfRequiredPoints is > 0;
         │  │  ├─ call BlogPost.MapToPostWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:565) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:570) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  ├─ call FeedsService.GetCommentsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:63) [verified]
         │  │      public async Task<WhatsNewFeedChannel> GetCommentsAsync(bool showBriefDescription,
         │  │      int count = 15,
         │  │      bool showDeletedItems = false)
         │  │  ├─ call BlogCommentsService.GetLastBlogCommentsIncludeBlogPostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:577) [verified]
         │  │  │      public Task<List<BlogPostComment>>
         │  │  │      GetLastBlogCommentsIncludeBlogPostAndUserAsync(int count, bool showDeletedItems = false)
         │  │  │      => _blogComments.AsNoTracking()
         │  │  ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:578) [verified]
         │  │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │  │      BlogName = "DNT",
         │  │  │  (stopped at depth 5; 1 branch omitted)
         │  │  ├─ call FeedsService.IsPrivateComment  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:580) [verified]
         │  │  │      private static bool IsPrivateComment(BlogPostComment item) => item.Parent.NumberOfRequiredPoints is > 0;
         │  │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:581) [approx]
         │  │  └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:586) [verified]
         │  │         private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │         AppSetting appSetting,
         │  │         List<WhatsNewItemModel> rssItems)
         │  └─ call FeedsService.GetNewsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:64) [verified]
         │         public async Task<WhatsNewFeedChannel> GetNewsAsync(bool showBriefDescription,
         │         int count = 15,
         │         bool showDeletedItems = false)
         │     ├─ call DailyNewsItemsService.GetLastDailyNewsItemsIncludeUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:593) [verified]
         │     │      public Task<List<DailyNewsItem>> GetLastDailyNewsItemsIncludeUserAsync(int count, bool showDeletedItems = false)
         │     │      => _dailyNewsItem.AsNoTracking()
         │     │      .Where(x => x.IsDeleted == showDeletedItems)
         │     ├─ call FeedsService.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:594) [verified]
         │     │      private async Task<AppSetting> GetAppSettingsAsync()
         │     │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │     │      BlogName = "DNT",
         │     │  (stopped at depth 5; 1 branch omitted)
         │     ├─ call BlogPost.MapToNewsWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:596) [approx]
         │     ├─ call DailyNewsScreenshotsService.GetNewsThumbImage  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:597) [verified]
         │     │      public string GetNewsThumbImage(DailyNewsItem? item, string siteRootUri)
         │     │      if (item is null || string.IsNullOrWhiteSpace(siteRootUri))
         │     │      return string.Empty;
         │     │  (stopped at depth 5; 2 branches omitted)
         │     └─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:602) [verified]
         │            private static WhatsNewFeedChannel GetFeedChannel(string title,
         │            AppSetting appSetting,
         │            List<WhatsNewItemModel> rssItems)
         └─ call FeedController.ShowBriefDescriptionAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
                private async Task<bool> ShowBriefDescriptionAsync()
                var appSetting = await cachedAppSettingsProvider.GetAppSettingsAsync();
                return appSetting.ShowRssBriefDescription;
            └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [verified]
                   public async Task<AppSetting> GetAppSettingsAsync()
                   if (_appSetting is not null)
                   return _appSetting;
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_6 info · 2 notable · 2 warning_

### **WARNING**: 62/70 endpoints anonymous, incl. 3 POST/PUT/DELETE
*(Risk)*

- GET /users/EmailToImage/{id:int?}
- GET /File/EmailToImage
- GET /File/CourseImages
- GET /File/CourseFiles
- GET /File/NewsThumb

### **WARNING**: Auth surface: 8 protected, 62 unannotated of 70 API endpoints
*(Risk)*

- 8 protected
- POST /api/UploadFile
- POST /api/UploadFile
- 62 no auth annotation

### **NOTABLE**: Internal hubs: 2 heavily-referenced internal types
*(Topology)*

- File (17 refs)
- NoopDisposable (1 refs)

### **NOTABLE**: Extension seats: AddOptions (3 impls) · AddEfCoreInterceptors (2 impls) · AddControllers (2 impls)
*(Wiring)*

- AddOptions (3 impls)
- AddEfCoreInterceptors (2 impls)
- AddControllers (2 impls)

### _INFO_: Entry targets resolved 93/94 (98%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: DI: 35 Extension · 9 Singleton · 3 Scoped · 1 Bulk (48 total)
*(Wiring)*

### _INFO_: Routing surface: 8 routes exposed
*(Shape)*

- POST /api/UploadFile
- POST /api/UploadFile
- POST /api/UploadFile
- POST /api/UploadFile
- POST /api/UploadFile

### _INFO_: Public surface: 107 interfaces, 1167 classes (1279 total public types)
*(Shape)*

- 107 interfaces
- 1167 classes

### _INFO_: Entry surface: 70 HTTP · 24 scheduled
*(Shape)*

- 70 HTTP
- 24 scheduled

### _INFO_: Most depended-upon: DntSite.Web.Common.BlazorSsr (1 dependents) · DntSite.Web (1 dependents)
*(Topology)*

- DntSite.Web.Common.BlazorSsr (1 dependents)
- DntSite.Web (1 dependents)

MAP  DntSite     (2 projects)

STACK  net10.0 · Minimal APIs · Controllers · EF Core

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.9)

       per service:
         DntSite.Web: Web App [EF Core]

TOPOLOGY (depends-on)
   DntSite.Web.Common.BlazorSsr
   DntSite.Web ── DntSite.Web.Common.BlazorSsr

ENTRY POINTS
   HTTP (70)
      GET /api/Fts  → FullTextSearchService.FindPagedPosts  (src/DntSite.Web/Features/Searches/Controllers/FtsController.cs:19)
      GET /atom.xml  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:92)
      GET /blog/feed  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:92)
      GET /blog/rss.xml  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:92)
      GET /Exports/{type}/{name}.pdf  → PdfExportService.GetPhysicalFilePath  (src/DntSite.Web/Features/Exports/Controllers/ExportsController.cs:13)
      GET /Feed  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:15)
      GET /feed.xml  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:92)
      GET /Feed/Announcements  → FeedsService.GetAllAdvertisementsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:127)
      GET /feed/atom  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:92)
      GET /Feed/Author/{id?}  → FeedsService.GetAuthorAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:61)
      GET /Feed/Comments  → FeedsService.GetCommentsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:33)
      GET /Feed/Courses  → FeedsService.GetAllCoursesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:113)
      GET /Feed/CoursesComments  → FeedsService.GetCourseTopicsRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:120)
      GET /Feed/CoursesTopics  → FeedsService.GetAllCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:116)
      GET /Feed/GetLatestChangesAsync  → FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:110)
      GET /Feed/Index  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:15)
      GET /Feed/LatestChanges  → FeedController  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89)
      GET /Feed/News  → FeedsService.GetNewsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:47)
      GET /Feed/NewsAuthor/{id?}  → FeedsService.GetNewsAuthorAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:77)
      GET /Feed/NewsComments  → FeedsService.GetNewsCommentsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:73)
      … and 50 more (http entries — use --focus for a drill-in)
   Scheduled (24)
      AIDailyNewsBacklogsJob  → AIDailyNewsBacklogsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:123)
      AIDailyNewsJob  → AIDailyNewsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:116)
      BackupDatabaseJob  → BackupDatabaseJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:61)
      BackupDataFolderJob  → BackupDataFolderJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:67)
      CheckAdminsLastVisitJob  → CheckAdminsLastVisitJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:23)
      DailyBirthDatesEmailJob  → DailyBirthDatesEmailJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:110)
      DailyNewsletterJob  → DailyNewsletterJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:107)
      DeleteOrphansJob  → DeleteOrphansJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:104)
      DisableInactiveUsersJob  → DisableInactiveUsersJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:49)
      DotNetVersionCheckJob  → DotNetVersionCheckJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:20)
      DraftsJob  → DraftsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:73)
      EmptyPMsJob  → EmptyPMsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:113)
      ExportToMergedPdfFilesJob  → ExportToMergedPdfFilesJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:101)
      ExportToSeparatePdfFilesJob  → ExportToSeparatePdfFilesJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:94)
      FreeSpaceCheckJob  → FreeSpaceCheckJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:30)
      FullTextSearchWriterJob  → FullTextSearchWriterJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:80)
      HumansTxtJob  → HumansTxtJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:70)
      ManageBacklogsJob  → ManageBacklogsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:54)
      NewPersianYearEmailsJob  → NewPersianYearEmailsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:52)
      SendActivationEmailsJob  → SendActivationEmailsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:46)
      … and 4 more (scheduled entries — use --focus for a drill-in)

PACKAGES
   ORM/Data:  EFCoreSecondLevelCacheInterceptor.MemoryCache 5.3.12, Gridify.EntityFramework 2.19.1, Microsoft.EntityFrameworkCore 10.0.9, Microsoft.EntityFrameworkCore.Abstractions 10.0.9, Microsoft.EntityFrameworkCore.Design 10.0.9, Microsoft.EntityFrameworkCore.Relational 10.0.9, Microsoft.EntityFrameworkCore.Sqlite 10.0.9, Microsoft.EntityFrameworkCore.Sqlite.Core 10.0.9 … (10 total)
   Testing:  MSTest.TestAdapter 4.2.3, MSTest.TestFramework 4.2.3
   Utilities:  Humanizer.Core 3.0.10
   Other:  DNTCommon.Web.Core 14.8.5, Lucene.Net 4.8.0-beta00017, Lucene.Net.Analysis.Common 4.8.0-beta00017, Lucene.Net.QueryParser 4.8.0-beta00017, Microsoft.NET.Test.Sdk 18.7.0, Microsoft.TypeScript.MSBuild 6.0.3, Microsoft.Web.LibraryManager.Build 3.0.114, Telegram.Bot 22.10.1

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 412ms |
| GenericExtraction | 8974ms |
| SignalSealing | 0ms |
| SpecificExtraction | 7511ms |
| Compression | 99ms |
| **Total** | **17859ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 8970ms | 1294 | 83 |
| DiRegistrationExtractor | 8968ms | 23 | 83 |
| BlazorEntryExtractor | 4831ms | 0 | 116 |
| EndpointExtractor | 2728ms | 0 | 116 |
| CallGraphExtractor | 2674ms | 0 | 0 |
| EfCoreExtractor | 875ms | 0 | 115 |
| InMemoryEventBusExtractor | 628ms | 0 | 89 |
| ControllerActionExtractor | 579ms | 0 | 84 |
| IndirectWiringDetector | 562ms | 0 | 63 |
| FileTreeExtractor | 258ms | 0 | 0 |
| ProgramCsFlowExtractor | 245ms | 0 | 35 |
| SourceBodyExtractor | 142ms | 0 | 0 |
| ProjectStructure | 85ms | 0 | 0 |
| SolutionDiscovery | 65ms | 0 | 0 |
| DependencyExtractor | 17ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 2012 | 173 |
| ReadsWrites | 19 | 17 |
| Resolves | 128 | 105 |
| EntityRelation | 1 | 1 |

_1342 files · 3 projects_
