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
