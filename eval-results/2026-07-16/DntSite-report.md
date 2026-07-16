# REPORT
**DntSite**

Style: ControllerBased
_2 projects  ·  70 HttpEndpoint, 24 ScheduledJob  ·  net10.0 + controllers + minimal-apis + efcore_

## Stats

| Metric | Value |
|--------|-------|
| Files | 1342 |
| Projects | 3 |
| Nodes | 3308 |
| Edges | 4364 |
| Entries | 94 |
| With target | 93/94 |
| Deep spine (>=2) | 94/94 (100%) |
| Verified edges | 49% |
| Analyzed in | 138.3s |

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
       DntSite.Web
▸ ENTRY  GET /Feed/GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:110)
   └─ call FeedController.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:110)
          private async Task<WhatsNewFeedChannel> GetLatestChangesAsync()
          => await feedsService.GetLatestChangesAsync(await ShowBriefDescriptionAsync());
      ├─ call IFeedsService  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [approx]
      │      public interface IFeedsService : IScopedService
      │      Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30);
      │      Task<WhatsNewFeedChannel> GetBacklogsAsync(bool showBriefDescription,
      │  └─ di FeedsService [approx]
      │         public class FeedsService(
      │         ICachedAppSettingsProvider appSettingsService,
      │         IProjectsService projectsService,
      ├─ call FeedController.ShowBriefDescriptionAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
      │      private async Task<bool> ShowBriefDescriptionAsync()
      │      var appSetting = await cachedAppSettingsProvider.GetAppSettingsAsync();
      │      return appSetting.ShowRssBriefDescription;
      │  ├─ call ICachedAppSettingsProvider  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [approx]
      │  │      public interface ICachedAppSettingsProvider : ISingletonService
      │  │      Task<AppSetting> GetAppSettingsAsync();
      │  │      Task<(string? SiteRootUri, string? Domain)> GetSiteRootDomainAsync();
      │  │  └─ di CachedAppSettingsProvider [approx]
      │  │         public class CachedAppSettingsProvider(IServiceProvider serviceProvider, ILockerService lockerService)
      │  │         : ICachedAppSettingsProvider
      │  │         private AppSetting? _appSetting;
      │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [verified]
      │         public async Task<AppSetting> GetAppSettingsAsync()
      │         if (_appSetting is not null)
      │         return _appSetting;
      └─ call FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
             public async Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30)
             var result = new List<WhatsNewItemModel>();
             result.AddRange((await GetProjectsNewsAsync(showBriefDescription)).RssItems ?? []);
         (15 more branches omitted beyond fan-out)
         ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:84) [verified]
         │      private static WhatsNewFeedChannel GetFeedChannel(string title,
         │      AppSetting appSetting,
         │      List<WhatsNewItemModel> rssItems)
         ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:80) [verified]
         │      private async Task<AppSetting> GetAppSettingsAsync()
         │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │      BlogName = "DNT",
         │  ├─ call ICachedAppSettingsProvider  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [approx]
         │  │      public interface ICachedAppSettingsProvider : ISingletonService
         │  │      Task<AppSetting> GetAppSettingsAsync();
         │  │      Task<(string? SiteRootUri, string? Domain)> GetSiteRootDomainAsync();
         │  │  (stopped at depth 4; 1 branch omitted)
         │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [verified]
         │         public async Task<AppSetting> GetAppSettingsAsync()
         │         if (_appSetting is not null)
         │         return _appSetting;
         ├─ call GetQuestionsCommentsFeedItemsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:77) [verified]
         │      public async Task<WhatsNewFeedChannel> GetQuestionsCommentsFeedItemsAsync(bool showBriefDescription,
         │      int pageNumber = 0,
         │      int recordsPerPage = 8,
         │  ├─ call IQuestionsCommentsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:94) [approx]
         │  │      public interface IQuestionsCommentsService : IScopedService
         │  │      Task<PagedResultModel<StackExchangeQuestionComment>> GetLastPagedStackExchangeQuestionCommentsOfUserAsync(
         │  │      string name,
         │  │  └─ di QuestionsCommentsService [approx]
         │  │         public class QuestionsCommentsService(
         │  │         IUnitOfWork uow,
         │  │         IStatService statService,
         │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:105) [verified]
         │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │      AppSetting appSetting,
         │  │      List<WhatsNewItemModel> rssItems)
         │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:100) [approx]
         │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:97) [verified]
         │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │      BlogName = "DNT",
         │  │  (stopped at depth 4; 2 branches omitted)
         │  └─ call GetLastPagedStackExchangeQuestionCommentsAsNoTrackingAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:94) [verified]
         │         public Task<PagedResultModel<StackExchangeQuestionComment>>
         │         GetLastPagedStackExchangeQuestionCommentsAsNoTrackingAsync(int pageNumber,
         │         int recordsPerPage = 8,
         │     └─ call DbSet  (src/DntSite.Web/Features/StackExchangeQuestions/Services/QuestionsCommentsService.cs:63) [approx]
         ├─ call GetQuestionsFeedItemsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:76) [verified]
         │      public async Task<WhatsNewFeedChannel> GetQuestionsFeedItemsAsync(bool showBriefDescription,
         │      int pageNumber = 0,
         │      int? userId = null,
         │  ├─ call IQuestionsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:143) [approx]
         │  │      public interface IQuestionsService : IScopedService
         │  │      ValueTask<StackExchangeQuestion?> FindStackExchangeQuestionAsync(int id);
         │  │      Task<List<StackExchangeQuestion>> GetAllPublicStackExchangeQuestionsOfDateAsync(DateTime date);
         │  │  └─ di QuestionsService [approx]
         │  │         public class QuestionsService(
         │  │         IUnitOfWork uow,
         │  │         ITagsService tagsService,
         │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:154) [verified]
         │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │      AppSetting appSetting,
         │  │      List<WhatsNewItemModel> rssItems)
         │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:149) [approx]
         │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:146) [verified]
         │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │      BlogName = "DNT",
         │  │  (stopped at depth 4; 2 branches omitted)
         │  └─ call GetStackExchangeQuestionsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:143) [verified]
         │         public Task<PagedResultModel<StackExchangeQuestion>> GetStackExchangeQuestionsAsync(int pageNumber,
         │         int? userId = null,
         │         int recordsPerPage = 15,
         │     └─ call DbSet  (src/DntSite.Web/Features/StackExchangeQuestions/Services/QuestionsService.cs:121) [approx]
         ├─ call GetBacklogsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:75) [verified]
         │      public async Task<WhatsNewFeedChannel> GetBacklogsAsync(bool showBriefDescription,
         │      int pageNumber = 0,
         │      int? userId = null,
         │  ├─ call IBacklogsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:119) [approx]
         │  │      public interface IBacklogsService : IScopedService
         │  │      ValueTask<Backlog?> FindBacklogAsync(int id);
         │  │      Task<Backlog?> GetFullBacklogAsync(int id, bool showDeletedItems = false);
         │  │  └─ di BacklogsService [approx]
         │  │         public class BacklogsService(
         │  │         IUnitOfWork uow,
         │  │         ITagsService tagsService,
         │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:130) [verified]
         │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │      AppSetting appSetting,
         │  │      List<WhatsNewItemModel> rssItems)
         │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:125) [approx]
         │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:122) [verified]
         │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │      BlogName = "DNT",
         │  │  (stopped at depth 4; 2 branches omitted)
         │  └─ call GetBacklogsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:119) [verified]
         │         public Task<PagedResultModel<Backlog>> GetBacklogsAsync(int pageNumber,
         │         int? userId = null,
         │         int recordsPerPage = 15,
         │     └─ call DbSet  (src/DntSite.Web/Features/Backlogs/Services/BacklogsService.cs:67) [approx]
         ├─ call GetLearningPathsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:74) [verified]
         │      public async Task<WhatsNewFeedChannel> GetLearningPathsAsync(bool showBriefDescription,
         │      int pageNumber = 0,
         │      int? userId = null,
         │  ├─ call ILearningPathService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:166) [approx]
         │  │      public interface ILearningPathService : IScopedService
         │  │      ValueTask<LearningPath?> FindLearningPathAsync(int id);
         │  │      Task<LearningPath?> GetLearningPathAsync(int id, bool showDeletedItems = false);
         │  │  └─ di LearningPathService [approx]
         │  │         public class LearningPathService(
         │  │         IUnitOfWork uow,
         │  │         ITagsService tagsService,
         │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:177) [verified]
         │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │      AppSetting appSetting,
         │  │      List<WhatsNewItemModel> rssItems)
         │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:172) [approx]
         │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:169) [verified]
         │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │      BlogName = "DNT",
         │  │  (stopped at depth 4; 2 branches omitted)
         │  └─ call GetLearningPathsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:166) [verified]
         │         public Task<PagedResultModel<LearningPath>> GetLearningPathsAsync(int pageNumber,
         │         int? userId = null,
         │         int recordsPerPage = 15,
         │     └─ call DbSet  (src/DntSite.Web/Features/RoadMaps/Services/LearningPathService.cs:59) [approx]
         ├─ call GetCourseTopicsRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:73) [verified]
         │      public async Task<WhatsNewFeedChannel> GetCourseTopicsRepliesAsync(bool showBriefDescription,
         │      int count = 15,
         │      bool onlyActives = true)
         │  ├─ call ICourseTopicCommentsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:693) [approx]
         │  │      public interface ICourseTopicCommentsService : IScopedService
         │  │      ValueTask<CourseTopicComment?> FindTopicCommentAsync(int commentId);
         │  │      Task<CourseTopicComment?> FindTopicCommentIncludeParentAsync(int commentId);
         │  │  └─ di CourseTopicCommentsService [approx]
         │  │         public class CourseTopicCommentsService(
         │  │         IUnitOfWork uow,
         │  │         IUserRatingsService userRatingsService,
         │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:701) [verified]
         │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
         │  │      AppSetting appSetting,
         │  │      List<WhatsNewItemModel> rssItems)
         │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:696) [approx]
         │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:694) [verified]
         │  │      private async Task<AppSetting> GetAppSettingsAsync()
         │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
         │  │      BlogName = "DNT",
         │  │  (stopped at depth 4; 2 branches omitted)
         │  └─ call GetLastTopicCommentsIncludePostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:693) [verified]
         │         public Task<List<CourseTopicComment>>
         │         GetLastTopicCommentsIncludePostAndUserAsync(int count, bool onlyActives = true)
         │         => _comments.AsNoTracking()
         │     └─ call DbSet  (src/DntSite.Web/Features/Courses/Services/CourseTopicCommentsService.cs:80) [approx]
         └─ call GetAllCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:72) [verified]
                public async Task<WhatsNewFeedChannel> GetAllCoursesTopicsAsync(bool showBriefDescription)
                var list = await courseTopicsService.GetPagedAllActiveCoursesTopicsAsync();
                var appSetting = await GetAppSettingsAsync();
            ├─ call ICourseTopicsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:182) [approx]
            │      public interface ICourseTopicsService : IScopedService
            │      Task<bool> CanUserAddCourseTopicAsync(CurrentUserModel? user, int courseId);
            │      ValueTask<CourseTopic?> FindCourseTopicAsync(int id);
            │  └─ di CourseTopicsService [approx]
            │         public class CourseTopicsService(
            │         IUnitOfWork uow,
            │         IStatService statService,
            ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:190) [verified]
            │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │      AppSetting appSetting,
            │      List<WhatsNewItemModel> rssItems)
            ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:185) [approx]
            ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:183) [verified]
            │      private async Task<AppSetting> GetAppSettingsAsync()
            │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │      BlogName = "DNT",
            │  (stopped at depth 4; 2 branches omitted)
            └─ call GetPagedAllActiveCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:182) [verified]
                   public Task<List<CourseTopic>> GetPagedAllActiveCoursesTopicsAsync()
                   => _courseTopics.AsNoTracking()
                   .Where(x => !x.IsDeleted && x.Course.IsReadyToPublish)
               └─ call DbSet  (src/DntSite.Web/Features/Courses/Services/CourseTopicsService.cs:78) [approx]
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: GET /Feed/LatestChanges

TRACE  GET /Feed/LatestChanges
       src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89
       DntSite.Web
▸ ENTRY  GET /Feed/LatestChanges  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89)
   └─ call FeedController.LatestChanges  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:89)
          public async Task<IActionResult> LatestChanges()
          => new FeedResult<WhatsNewItemModel>(await GetLatestChangesAsync());
      └─ call FeedController.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:90) [verified]
             private async Task<WhatsNewFeedChannel> GetLatestChangesAsync()
             => await feedsService.GetLatestChangesAsync(await ShowBriefDescriptionAsync());
         ├─ call IFeedsService  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [approx]
         │      public interface IFeedsService : IScopedService
         │      Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30);
         │      Task<WhatsNewFeedChannel> GetBacklogsAsync(bool showBriefDescription,
         │  └─ di FeedsService [approx]
         │         public class FeedsService(
         │         ICachedAppSettingsProvider appSettingsService,
         │         IProjectsService projectsService,
         ├─ call FeedController.ShowBriefDescriptionAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
         │      private async Task<bool> ShowBriefDescriptionAsync()
         │      var appSetting = await cachedAppSettingsProvider.GetAppSettingsAsync();
         │      return appSetting.ShowRssBriefDescription;
         │  ├─ call ICachedAppSettingsProvider  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [approx]
         │  │      public interface ICachedAppSettingsProvider : ISingletonService
         │  │      Task<AppSetting> GetAppSettingsAsync();
         │  │      Task<(string? SiteRootUri, string? Domain)> GetSiteRootDomainAsync();
         │  │  └─ di CachedAppSettingsProvider [approx]
         │  │         public class CachedAppSettingsProvider(IServiceProvider serviceProvider, ILockerService lockerService)
         │  │         : ICachedAppSettingsProvider
         │  │         private AppSetting? _appSetting;
         │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [verified]
         │         public async Task<AppSetting> GetAppSettingsAsync()
         │         if (_appSetting is not null)
         │         return _appSetting;
         └─ call FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
                public async Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30)
                var result = new List<WhatsNewItemModel>();
                result.AddRange((await GetProjectsNewsAsync(showBriefDescription)).RssItems ?? []);
            (15 more branches omitted beyond fan-out)
            ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:84) [verified]
            │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │      AppSetting appSetting,
            │      List<WhatsNewItemModel> rssItems)
            ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:80) [verified]
            │      private async Task<AppSetting> GetAppSettingsAsync()
            │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │      BlogName = "DNT",
            │  ├─ call ICachedAppSettingsProvider  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [approx]
            │  │      public interface ICachedAppSettingsProvider : ISingletonService
            │  │      Task<AppSetting> GetAppSettingsAsync();
            │  │      Task<(string? SiteRootUri, string? Domain)> GetSiteRootDomainAsync();
            │  │  (stopped at depth 5; 1 branch omitted)
            │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [verified]
            │         public async Task<AppSetting> GetAppSettingsAsync()
            │         if (_appSetting is not null)
            │         return _appSetting;
            ├─ call GetQuestionsCommentsFeedItemsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:77) [verified]
            │      public async Task<WhatsNewFeedChannel> GetQuestionsCommentsFeedItemsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int recordsPerPage = 8,
            │  ├─ call IQuestionsCommentsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:94) [approx]
            │  │      public interface IQuestionsCommentsService : IScopedService
            │  │      Task<PagedResultModel<StackExchangeQuestionComment>> GetLastPagedStackExchangeQuestionCommentsOfUserAsync(
            │  │      string name,
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:105) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:100) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:97) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetLastPagedStackExchangeQuestionCommentsAsNoTrackingAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:94) [verified]
            │         public Task<PagedResultModel<StackExchangeQuestionComment>>
            │         GetLastPagedStackExchangeQuestionCommentsAsNoTrackingAsync(int pageNumber,
            │         int recordsPerPage = 8,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetQuestionsFeedItemsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:76) [verified]
            │      public async Task<WhatsNewFeedChannel> GetQuestionsFeedItemsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int? userId = null,
            │  ├─ call IQuestionsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:143) [approx]
            │  │      public interface IQuestionsService : IScopedService
            │  │      ValueTask<StackExchangeQuestion?> FindStackExchangeQuestionAsync(int id);
            │  │      Task<List<StackExchangeQuestion>> GetAllPublicStackExchangeQuestionsOfDateAsync(DateTime date);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:154) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:149) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:146) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetStackExchangeQuestionsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:143) [verified]
            │         public Task<PagedResultModel<StackExchangeQuestion>> GetStackExchangeQuestionsAsync(int pageNumber,
            │         int? userId = null,
            │         int recordsPerPage = 15,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetBacklogsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:75) [verified]
            │      public async Task<WhatsNewFeedChannel> GetBacklogsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int? userId = null,
            │  ├─ call IBacklogsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:119) [approx]
            │  │      public interface IBacklogsService : IScopedService
            │  │      ValueTask<Backlog?> FindBacklogAsync(int id);
            │  │      Task<Backlog?> GetFullBacklogAsync(int id, bool showDeletedItems = false);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:130) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:125) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:122) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetBacklogsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:119) [verified]
            │         public Task<PagedResultModel<Backlog>> GetBacklogsAsync(int pageNumber,
            │         int? userId = null,
            │         int recordsPerPage = 15,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetLearningPathsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:74) [verified]
            │      public async Task<WhatsNewFeedChannel> GetLearningPathsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int? userId = null,
            │  ├─ call ILearningPathService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:166) [approx]
            │  │      public interface ILearningPathService : IScopedService
            │  │      ValueTask<LearningPath?> FindLearningPathAsync(int id);
            │  │      Task<LearningPath?> GetLearningPathAsync(int id, bool showDeletedItems = false);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:177) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:172) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:169) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetLearningPathsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:166) [verified]
            │         public Task<PagedResultModel<LearningPath>> GetLearningPathsAsync(int pageNumber,
            │         int? userId = null,
            │         int recordsPerPage = 15,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetCourseTopicsRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:73) [verified]
            │      public async Task<WhatsNewFeedChannel> GetCourseTopicsRepliesAsync(bool showBriefDescription,
            │      int count = 15,
            │      bool onlyActives = true)
            │  ├─ call ICourseTopicCommentsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:693) [approx]
            │  │      public interface ICourseTopicCommentsService : IScopedService
            │  │      ValueTask<CourseTopicComment?> FindTopicCommentAsync(int commentId);
            │  │      Task<CourseTopicComment?> FindTopicCommentIncludeParentAsync(int commentId);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:701) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:696) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:694) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetLastTopicCommentsIncludePostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:693) [verified]
            │         public Task<List<CourseTopicComment>>
            │         GetLastTopicCommentsIncludePostAndUserAsync(int count, bool onlyActives = true)
            │         => _comments.AsNoTracking()
            │     (stopped at depth 5; 1 branch omitted)
            └─ call GetAllCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:72) [verified]
                   public async Task<WhatsNewFeedChannel> GetAllCoursesTopicsAsync(bool showBriefDescription)
                   var list = await courseTopicsService.GetPagedAllActiveCoursesTopicsAsync();
                   var appSetting = await GetAppSettingsAsync();
               ├─ call ICourseTopicsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:182) [approx]
               │      public interface ICourseTopicsService : IScopedService
               │      Task<bool> CanUserAddCourseTopicAsync(CurrentUserModel? user, int courseId);
               │      ValueTask<CourseTopic?> FindCourseTopicAsync(int id);
               │  (stopped at depth 5; 1 branch omitted)
               ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:190) [verified]
               │      private static WhatsNewFeedChannel GetFeedChannel(string title,
               │      AppSetting appSetting,
               │      List<WhatsNewItemModel> rssItems)
               ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:185) [approx]
               ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:183) [verified]
               │      private async Task<AppSetting> GetAppSettingsAsync()
               │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
               │      BlogName = "DNT",
               │  (stopped at depth 5; 2 branches omitted)
               └─ call GetPagedAllActiveCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:182) [verified]
                      public Task<List<CourseTopic>> GetPagedAllActiveCoursesTopicsAsync()
                      => _courseTopics.AsNoTracking()
                      .Where(x => !x.IsDeleted && x.Course.IsReadyToPublish)
                  (stopped at depth 5; 1 branch omitted)
RESULT   200 OK · failure → 404 Not Found

---

### Trace 3: GET /llms-full.txt

TRACE  GET /llms-full.txt
       src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:106
       DntSite.Web
▸ ENTRY  GET /llms-full.txt  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:106)
   └─ call FeedController.LlmsFull  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:106)
          [Microsoft.AspNetCore.Mvc.Route(template: "/llms-full.txt")]
          public async Task<IActionResult> LlmsFull()
          => new LlmsFullTxtResult<WhatsNewItemModel>(await GetLatestChangesAsync());
      └─ call FeedController.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:108) [verified]
             private async Task<WhatsNewFeedChannel> GetLatestChangesAsync()
             => await feedsService.GetLatestChangesAsync(await ShowBriefDescriptionAsync());
         ├─ call IFeedsService  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [approx]
         │      public interface IFeedsService : IScopedService
         │      Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30);
         │      Task<WhatsNewFeedChannel> GetBacklogsAsync(bool showBriefDescription,
         │  └─ di FeedsService [approx]
         │         public class FeedsService(
         │         ICachedAppSettingsProvider appSettingsService,
         │         IProjectsService projectsService,
         ├─ call FeedController.ShowBriefDescriptionAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
         │      private async Task<bool> ShowBriefDescriptionAsync()
         │      var appSetting = await cachedAppSettingsProvider.GetAppSettingsAsync();
         │      return appSetting.ShowRssBriefDescription;
         │  ├─ call ICachedAppSettingsProvider  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [approx]
         │  │      public interface ICachedAppSettingsProvider : ISingletonService
         │  │      Task<AppSetting> GetAppSettingsAsync();
         │  │      Task<(string? SiteRootUri, string? Domain)> GetSiteRootDomainAsync();
         │  │  └─ di CachedAppSettingsProvider [approx]
         │  │         public class CachedAppSettingsProvider(IServiceProvider serviceProvider, ILockerService lockerService)
         │  │         : ICachedAppSettingsProvider
         │  │         private AppSetting? _appSetting;
         │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:133) [verified]
         │         public async Task<AppSetting> GetAppSettingsAsync()
         │         if (_appSetting is not null)
         │         return _appSetting;
         └─ call FeedsService.GetLatestChangesAsync  (src/DntSite.Web/Features/RssFeeds/Controllers/FeedController.cs:111) [verified]
                public async Task<WhatsNewFeedChannel> GetLatestChangesAsync(bool showBriefDescription, int take = 30)
                var result = new List<WhatsNewItemModel>();
                result.AddRange((await GetProjectsNewsAsync(showBriefDescription)).RssItems ?? []);
            (15 more branches omitted beyond fan-out)
            ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:84) [verified]
            │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │      AppSetting appSetting,
            │      List<WhatsNewItemModel> rssItems)
            ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:80) [verified]
            │      private async Task<AppSetting> GetAppSettingsAsync()
            │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │      BlogName = "DNT",
            │  ├─ call ICachedAppSettingsProvider  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [approx]
            │  │      public interface ICachedAppSettingsProvider : ISingletonService
            │  │      Task<AppSetting> GetAppSettingsAsync();
            │  │      Task<(string? SiteRootUri, string? Domain)> GetSiteRootDomainAsync();
            │  │  (stopped at depth 5; 1 branch omitted)
            │  └─ call CachedAppSettingsProvider.GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:716) [verified]
            │         public async Task<AppSetting> GetAppSettingsAsync()
            │         if (_appSetting is not null)
            │         return _appSetting;
            ├─ call GetQuestionsCommentsFeedItemsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:77) [verified]
            │      public async Task<WhatsNewFeedChannel> GetQuestionsCommentsFeedItemsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int recordsPerPage = 8,
            │  ├─ call IQuestionsCommentsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:94) [approx]
            │  │      public interface IQuestionsCommentsService : IScopedService
            │  │      Task<PagedResultModel<StackExchangeQuestionComment>> GetLastPagedStackExchangeQuestionCommentsOfUserAsync(
            │  │      string name,
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:105) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:100) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:97) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetLastPagedStackExchangeQuestionCommentsAsNoTrackingAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:94) [verified]
            │         public Task<PagedResultModel<StackExchangeQuestionComment>>
            │         GetLastPagedStackExchangeQuestionCommentsAsNoTrackingAsync(int pageNumber,
            │         int recordsPerPage = 8,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetQuestionsFeedItemsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:76) [verified]
            │      public async Task<WhatsNewFeedChannel> GetQuestionsFeedItemsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int? userId = null,
            │  ├─ call IQuestionsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:143) [approx]
            │  │      public interface IQuestionsService : IScopedService
            │  │      ValueTask<StackExchangeQuestion?> FindStackExchangeQuestionAsync(int id);
            │  │      Task<List<StackExchangeQuestion>> GetAllPublicStackExchangeQuestionsOfDateAsync(DateTime date);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:154) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:149) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:146) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetStackExchangeQuestionsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:143) [verified]
            │         public Task<PagedResultModel<StackExchangeQuestion>> GetStackExchangeQuestionsAsync(int pageNumber,
            │         int? userId = null,
            │         int recordsPerPage = 15,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetBacklogsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:75) [verified]
            │      public async Task<WhatsNewFeedChannel> GetBacklogsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int? userId = null,
            │  ├─ call IBacklogsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:119) [approx]
            │  │      public interface IBacklogsService : IScopedService
            │  │      ValueTask<Backlog?> FindBacklogAsync(int id);
            │  │      Task<Backlog?> GetFullBacklogAsync(int id, bool showDeletedItems = false);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:130) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:125) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:122) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetBacklogsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:119) [verified]
            │         public Task<PagedResultModel<Backlog>> GetBacklogsAsync(int pageNumber,
            │         int? userId = null,
            │         int recordsPerPage = 15,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetLearningPathsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:74) [verified]
            │      public async Task<WhatsNewFeedChannel> GetLearningPathsAsync(bool showBriefDescription,
            │      int pageNumber = 0,
            │      int? userId = null,
            │  ├─ call ILearningPathService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:166) [approx]
            │  │      public interface ILearningPathService : IScopedService
            │  │      ValueTask<LearningPath?> FindLearningPathAsync(int id);
            │  │      Task<LearningPath?> GetLearningPathAsync(int id, bool showDeletedItems = false);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:177) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:172) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:169) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetLearningPathsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:166) [verified]
            │         public Task<PagedResultModel<LearningPath>> GetLearningPathsAsync(int pageNumber,
            │         int? userId = null,
            │         int recordsPerPage = 15,
            │     (stopped at depth 5; 1 branch omitted)
            ├─ call GetCourseTopicsRepliesAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:73) [verified]
            │      public async Task<WhatsNewFeedChannel> GetCourseTopicsRepliesAsync(bool showBriefDescription,
            │      int count = 15,
            │      bool onlyActives = true)
            │  ├─ call ICourseTopicCommentsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:693) [approx]
            │  │      public interface ICourseTopicCommentsService : IScopedService
            │  │      ValueTask<CourseTopicComment?> FindTopicCommentAsync(int commentId);
            │  │      Task<CourseTopicComment?> FindTopicCommentIncludeParentAsync(int commentId);
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:701) [verified]
            │  │      private static WhatsNewFeedChannel GetFeedChannel(string title,
            │  │      AppSetting appSetting,
            │  │      List<WhatsNewItemModel> rssItems)
            │  ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:696) [approx]
            │  ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:694) [verified]
            │  │      private async Task<AppSetting> GetAppSettingsAsync()
            │  │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
            │  │      BlogName = "DNT",
            │  │  (stopped at depth 5; 2 branches omitted)
            │  └─ call GetLastTopicCommentsIncludePostAndUserAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:693) [verified]
            │         public Task<List<CourseTopicComment>>
            │         GetLastTopicCommentsIncludePostAndUserAsync(int count, bool onlyActives = true)
            │         => _comments.AsNoTracking()
            │     (stopped at depth 5; 1 branch omitted)
            └─ call GetAllCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:72) [verified]
                   public async Task<WhatsNewFeedChannel> GetAllCoursesTopicsAsync(bool showBriefDescription)
                   var list = await courseTopicsService.GetPagedAllActiveCoursesTopicsAsync();
                   var appSetting = await GetAppSettingsAsync();
               ├─ call ICourseTopicsService  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:182) [approx]
               │      public interface ICourseTopicsService : IScopedService
               │      Task<bool> CanUserAddCourseTopicAsync(CurrentUserModel? user, int courseId);
               │      ValueTask<CourseTopic?> FindCourseTopicAsync(int id);
               │  (stopped at depth 5; 1 branch omitted)
               ├─ call FeedsService.GetFeedChannel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:190) [verified]
               │      private static WhatsNewFeedChannel GetFeedChannel(string title,
               │      AppSetting appSetting,
               │      List<WhatsNewItemModel> rssItems)
               ├─ call BlogPost.MapToWhatsNewItemModel  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:185) [approx]
               ├─ call GetAppSettingsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:183) [verified]
               │      private async Task<AppSetting> GetAppSettingsAsync()
               │      => await appSettingsService.GetAppSettingsAsync() ?? new AppSetting
               │      BlogName = "DNT",
               │  (stopped at depth 5; 2 branches omitted)
               └─ call GetPagedAllActiveCoursesTopicsAsync  (src/DntSite.Web/Features/RssFeeds/Services/FeedsService.cs:182) [verified]
                      public Task<List<CourseTopic>> GetPagedAllActiveCoursesTopicsAsync()
                      => _courseTopics.AsNoTracking()
                      .Where(x => !x.IsDeleted && x.Course.IsReadyToPublish)
                  (stopped at depth 5; 1 branch omitted)
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_6 info · 2 warning_

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
- POST /api/UploadFile/MessagesFilesUpload
- POST /api/UploadFile/CommonFilesUpload
- 62 no auth annotation

### _INFO_: Entry targets resolved 93/94 (98%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 35 Extension · 9 Singleton · 3 Scoped · 1 Bulk (48 total)
*(Wiring)*

### _INFO_: Entry surface: 70 HTTP · 24 scheduled
*(Shape)*

- 70 HTTP
- 24 scheduled

### _INFO_: Most depended-upon: DntSite.Web.Common.BlazorSsr (1 dependents) · DntSite.Web (1 dependents)
*(Topology)*

- DntSite.Web.Common.BlazorSsr (1 dependents)
- DntSite.Web (1 dependents)

### _INFO_: Data map: 9 entities across 2 scopes
*(Data)*

- DntSite.Web (8 entities)
- ApplicationDbContext (1 entities)

### _INFO_: Wiring hubs: DbSet (466) · ApplicationState (298) · IUnitOfWork (250) · IEmailsFactoryService (93) · IFullTextSearchService (89)
*(Wiring)*

- DbSet (466)
- ApplicationState (298)
- IUnitOfWork (250)
- IEmailsFactoryService (93)
- IFullTextSearchService (89)

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
      GET /api/Fts/Search  → FullTextSearchService.FindPagedPosts  (src/DntSite.Web/Features/Searches/Controllers/FtsController.cs:19)
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
   ORM/Data:  EFCoreSecondLevelCacheInterceptor.MemoryCache 5.3.11, Gridify.EntityFramework 2.19.1, Microsoft.EntityFrameworkCore 10.0.9, Microsoft.EntityFrameworkCore.Abstractions 10.0.9, Microsoft.EntityFrameworkCore.Design 10.0.9, Microsoft.EntityFrameworkCore.Relational 10.0.9, Microsoft.EntityFrameworkCore.Sqlite 10.0.9, Microsoft.EntityFrameworkCore.Sqlite.Core 10.0.9 … (10 total)
   Testing:  MSTest.TestAdapter 4.2.3, MSTest.TestFramework 4.2.3
   Utilities:  Humanizer.Core 3.0.10
   Other:  DNTCommon.Web.Core 14.8.3, Lucene.Net 4.8.0-beta00017, Lucene.Net.Analysis.Common 4.8.0-beta00017, Lucene.Net.QueryParser 4.8.0-beta00017, Microsoft.NET.Test.Sdk 18.6.0, Microsoft.TypeScript.MSBuild 6.0.3, Microsoft.Web.LibraryManager.Build 3.0.71, Telegram.Bot 22.10.1

→ drill in:  --focus "<entry>"   (e.g. --focus "GET /Feed/GetLatestChangesAsync")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 477ms |
| GenericExtraction | 9359ms |
| SignalSealing | 0ms |
| SpecificExtraction | 9839ms |
| Compression | 137ms |
| **Total** | **138339ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 9355ms | 1294 | 83 |
| SyntaxStructureExtractor | 9354ms | 1294 | 59 |
| DiRegistrationExtractor | 9351ms | 111 | 59 |
| BlazorEntryExtractor | 5121ms | 0 | 118 |
| CallGraphExtractor | 4711ms | 0 | 0 |
| EndpointExtractor | 3176ms | 0 | 118 |
| BodyFactsExtractor | 2547ms | 0 | 0 |
| EfCoreExtractor | 1045ms | 0 | 117 |
| InMemoryEventBusExtractor | 753ms | 0 | 94 |
| ControllerActionExtractor | 656ms | 0 | 86 |
| IndirectWiringDetector | 632ms | 0 | 85 |
| SourceBodyExtractor | 423ms | 0 | 0 |
| FileTreeExtractor | 305ms | 0 | 0 |
| SolutionDiscovery | 110ms | 0 | 0 |
| ProjectStructure | 58ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 4219 | 2106 |
| ReadsWrites | 16 | 16 |
| Resolves | 128 | 105 |
| EntityRelation | 1 | 1 |

_1342 files · 3 projects_
