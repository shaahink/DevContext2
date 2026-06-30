MAP  DntSite     (2 projects)

STACK  net10.0 · Minimal APIs · Controllers · EF Core

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.8)

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
      AIDailyNewsBacklogsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:123)
      AIDailyNewsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:116)
      BackupDatabaseJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:61)
      BackupDataFolderJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:67)
      CheckAdminsLastVisitJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:23)
      DailyBirthDatesEmailJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:110)
      DailyNewsletterJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:107)
      DeleteOrphansJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:104)
      DisableInactiveUsersJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:49)
      DotNetVersionCheckJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:20)
      DraftsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:73)
      EmptyPMsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:113)
      ExportToMergedPdfFilesJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:101)
      ExportToSeparatePdfFilesJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:94)
      FreeSpaceCheckJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:30)
      FullTextSearchWriterJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:80)
      HumansTxtJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:70)
      ManageBacklogsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:54)
      NewPersianYearEmailsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:52)
      SendActivationEmailsJob  (src/DntSite.Web/Features/ServicesConfigs/SchedulersConfig.cs:46)
      … and 4 more (scheduled entries — use --focus for a drill-in)

PACKAGES
   ORM/Data:  EFCoreSecondLevelCacheInterceptor.MemoryCache, Gridify.EntityFramework, Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Abstractions, Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Relational, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Sqlite.Core … (10 total)
   Testing:  MSTest.TestAdapter, MSTest.TestFramework
   Utilities:  Humanizer.Core
   Other:  DNTCommon.Web.Core, Lucene.Net, Lucene.Net.Analysis.Common, Lucene.Net.QueryParser, Microsoft.NET.Test.Sdk, Microsoft.TypeScript.MSBuild, Microsoft.Web.LibraryManager.Build, Telegram.Bot

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
