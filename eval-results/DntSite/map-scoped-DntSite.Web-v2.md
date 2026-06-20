Overview map (no focus).
Analyzing project...

MAP  DntSite     (2 projects)

STACK  net10.0 · Minimal APIs · Controllers · EF Core

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, 
MinimalApi=yes(conf=0.8)

TOPOLOGY (depends-on)
   DntSite.Web ── DntSite.Web.Common.BlazorSsr
   DntSite.Web.Common.BlazorSsr

ENTRY POINTS
   HTTP (70)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:42)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:38)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:34)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:30)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:26)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:22)
      POST /api/UploadFile  → UploadFileController  (UploadFileController.cs:18)
      GET /users/EmailToImage/{id:int?}  → FileController  
(FileController.cs:60)
      GET /File/EmailToImage  → FileController  (FileController.cs:60)
      GET /File/CourseImages  → FileController  (FileController.cs:55)
      GET /File/CourseFiles  → FileController  (FileController.cs:51)
      GET /File/CommonFiles  → FileController  (FileController.cs:47)
      GET /File/NewsThumb  → FileController  (FileController.cs:42)
      GET /File/Messages  → FileController  (FileController.cs:39)
      GET /File/ProjectFile  → FileController  (FileController.cs:34)
      GET /File/UserFile  → FileController  (FileController.cs:30)
      GET /File/MessagesImages  → FileController  (FileController.cs:27)
      GET /File/Image  → FileController  (FileController.cs:22)
      GET /File/Avatar  → FileController  (FileController.cs:18)
      GET /Welcome  → WelcomeController  (WelcomeController.cs:12)
      GET /Sitemap/Get  → SitemapController  (SitemapController.cs:12)
      GET /sitemap  → SitemapController  (SitemapController.cs:12)
      GET /sitemap.xml  → SitemapController  (SitemapController.cs:12)
      GET /OpenSearch  → OpenSearchController  (OpenSearchController.cs:13)
      POST /api/Fts  → FtsController  (FtsController.cs:48)
      GET /api/Fts  → FtsController  (FtsController.cs:19)
      GET /ProjectsFeeds/ProjectIssuesReplies/{id:int?}  → 
ProjectsFeedsController  (ProjectsFeedsController.cs:72)
      GET /ProjectsFeeds/ProjectIssues/{id:int?}  → ProjectsFeedsController  
(ProjectsFeedsController.cs:59)
      GET /ProjectsFeeds/ProjectFiles/{id:int?}  → ProjectsFeedsController  
(ProjectsFeedsController.cs:46)
      GET /ProjectsFeeds/ProjectFaqs/{id:int?}  → ProjectsFeedsController  
(ProjectsFeedsController.cs:33)
      GET /ProjectsFeeds/ProjectsFaqs  → ProjectsFeedsController  
(ProjectsFeedsController.cs:30)
      GET /ProjectsFeeds/ProjectsIssuesReplies  → ProjectsFeedsController  
(ProjectsFeedsController.cs:26)
      GET /ProjectsFeeds/ProjectsIssues  → ProjectsFeedsController  
(ProjectsFeedsController.cs:23)
      GET /ProjectsFeeds/ProjectsFiles  → ProjectsFeedsController  
(ProjectsFeedsController.cs:20)
      GET /ProjectsFeeds/ProjectsNews  → ProjectsFeedsController  
(ProjectsFeedsController.cs:17)
      GET /ProjectsFeeds/Get  → ProjectsFeedsController  
(ProjectsFeedsController.cs:15)
      GET /ProjectsFeeds/Index  → ProjectsFeedsController  
(ProjectsFeedsController.cs:13)
      GET /Feed/ShowBriefDescriptionAsync  → FeedController  
(FeedController.cs:131)
      GET /Feed/Announcements  → FeedController  (FeedController.cs:127)
      GET /Feed/Surveys  → FeedController  (FeedController.cs:124)
      GET /Feed/CoursesComments  → FeedController  (FeedController.cs:120)
      GET /Feed/CoursesTopics  → FeedController  (FeedController.cs:116)
      GET /Feed/Courses  → FeedController  (FeedController.cs:113)
      GET /Feed/GetLatestChangesAsync  → FeedController  (FeedController.cs:110)
      GET /llms-full.txt  → FeedController  (FeedController.cs:106)
      GET /llms.txt  → FeedController  (FeedController.cs:103)
      GET /blog/rss.xml  → FeedController  (FeedController.cs:92)
      GET /blog/feed  → FeedController  (FeedController.cs:92)
      GET /feed/atom  → FeedController  (FeedController.cs:92)
      GET /feed/rss  → FeedController  (FeedController.cs:92)
      GET /feed.xml  → FeedController  (FeedController.cs:92)
      GET /rss2.xml  → FeedController  (FeedController.cs:92)
      GET /rss  → FeedController  (FeedController.cs:92)
      GET /atom.xml  → FeedController  (FeedController.cs:92)
      GET /rss.xml  → FeedController  (FeedController.cs:92)
      GET /Feed/LatestChanges  → FeedController  (FeedController.cs:89)
      GET /Feed/NewsAuthor/{id?}  → FeedController  (FeedController.cs:77)
      GET /Feed/NewsComments  → FeedController  (FeedController.cs:73)
      GET /Feed/Author/{id?}  → FeedController  (FeedController.cs:61)
      GET /Feed/Tag/{id?}  → FeedController  (FeedController.cs:50)
      GET /Feed/News  → FeedController  (FeedController.cs:47)
      GET /feeds/comments/{name?}  → FeedController  (FeedController.cs:36)
      GET /Feed/Comments  → FeedController  (FeedController.cs:33)
      GET /feeds/posts/{name?}  → FeedController  (FeedController.cs:22)
      POST /Feed/Posts  → FeedController  (FeedController.cs:19)
      GET /Feed  → FeedController  (FeedController.cs:15)
      GET /Feed/Index  → FeedController  (FeedController.cs:15)
      GET /Exports/{type}/{name}.pdf  → ExportsController  
(ExportsController.cs:13)
      POST /api/JavaScriptErrorsReport  → JavaScriptErrorsReportController  
(JavaScriptErrorsReportController.cs:16)
      GET /.well-known/change-password  (ChangePasswordEndpoint.cs:9)

PACKAGES
   ORM/Data:  EFCoreSecondLevelCacheInterceptor.MemoryCache, 
Gridify.EntityFramework, Microsoft.EntityFrameworkCore, 
Microsoft.EntityFrameworkCore.Abstractions, 
Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Relational, 
Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Sqlite.Core 
… (10 total)
   Utilities:  Humanizer.Core
   Other:  DNTCommon.Web.Core, Lucene.Net, Lucene.Net.Analysis.Common, 
Lucene.Net.QueryParser, Microsoft.TypeScript.MSBuild, 
Microsoft.Web.LibraryManager.Build, Telegram.Bot

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 1335 files · 1452 nodes · 126 edges · 70 entries · ~1595 tokens · 6.5s 
stage2 ×2.0 stage3 ×2.1
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     DntSite.slnx     │
│   Time   │        6540ms        │
│  Tokens  │ ~1595 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.100 │
╰──────────┴──────────────────────╯
