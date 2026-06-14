Overview map (no focus).
Analyzing project...

## DevContext ΓÇö Overview on project

**Architecture**: VerticalSlices (85% confidence)
> minimal-apis signal detected (confidence 80%)
> vertical-slice: 21 feature folders with 3+ artifact types each (e.g. Stats(9 
types), Searches(9 types), Common(8 types), News(8 types))
> feature-based: 27 feature directories in single project, 21 are self-contained
slices
**Signals**: controllers ┬╖ minimal-apis ┬╖ efcore
**Projects**: 3 ΓÇö DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: focused | **Tokens**: ~7996 (budget 8000) | **Types**: 58 in output

---
## Architecture overview

```text
ΓööΓöÇΓöÇ DntSite.Tests
    ΓööΓöÇΓöÇ DntSite.Web
        ΓööΓöÇΓöÇ DntSite.Web.Common.BlazorSsr
```

## Endpoints

**DntSite.Web** (70 endpoints)
| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| POST | /api/UploadFile | UploadFileController.MessagesFilesUpload | - | 
UploadFileController.cs:42 |
| POST | /api/UploadFile | UploadFileController.CommonFilesUpload | - | 
UploadFileController.cs:38 |
| POST | /api/UploadFile | UploadFileController.FileUpload | - | 
UploadFileController.cs:34 |
| POST | /api/UploadFile | UploadFileController.CourseFileUpload | - | 
UploadFileController.cs:30 |
| POST | /api/UploadFile | UploadFileController.CourseImagesUpload | - | 
UploadFileController.cs:26 |
| POST | /api/UploadFile | UploadFileController.MessagesImagesUpload | - | 
UploadFileController.cs:22 |
| POST | /api/UploadFile | UploadFileController.ImageUpload | - | 
UploadFileController.cs:18 |
| GET | /users/EmailToImage/{id:int?} | FileController.EmailToImage | - | 
FileController.cs:60 |
| GET | /File/EmailToImage | FileController.EmailToImage | - | 
FileController.cs:60 |
| GET | /File/CourseImages | FileController.CourseImages | - | 
FileController.cs:55 |
| GET | /File/CourseFiles | FileController.CourseFiles | - | 
FileController.cs:51 |
| GET | /File/CommonFiles | FileController.CommonFiles | Authorize | 
FileController.cs:47 |
| GET | /File/NewsThumb | FileController.NewsThumb | - | FileController.cs:42 |
| GET | /File/Messages | FileController.Messages | - | FileController.cs:39 |
| GET | /File/ProjectFile | FileController.ProjectFile | - | 
FileController.cs:34 |
| GET | /File/UserFile | FileController.UserFile | - | FileController.cs:30 |
| GET | /File/MessagesImages | FileController.MessagesImages | - | 
FileController.cs:27 |
| GET | /File/Image | FileController.Image | - | FileController.cs:22 |
| GET | /File/Avatar | FileController.Avatar | - | FileController.cs:18 |
| GET | /Welcome | WelcomeController.Log | - | WelcomeController.cs:12 |
| GET | /Sitemap/Get | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /sitemap | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /sitemap.xml | SitemapController.Get | - | SitemapController.cs:12 |
| GET | /OpenSearch | OpenSearchController.RenderOpenSearch | - | 
OpenSearchController.cs:13 |
| POST | /api/Fts | FtsController.Log | - | FtsController.cs:48 |
| GET | /api/Fts | FtsController.Search | - | FtsController.cs:19 |
| GET | /ProjectsFeeds/ProjectIssuesReplies/{id:int?} | 
ProjectsFeedsController.ProjectIssuesReplies | - | ProjectsFeedsController.cs:72
|
| GET | /ProjectsFeeds/ProjectIssues/{id:int?} | 
ProjectsFeedsController.ProjectIssues | - | ProjectsFeedsController.cs:59 |
| GET | /ProjectsFeeds/ProjectFiles/{id:int?} | 
ProjectsFeedsController.ProjectFiles | - | ProjectsFeedsController.cs:46 |
| GET | /ProjectsFeeds/ProjectFaqs/{id:int?} | 
ProjectsFeedsController.ProjectFaqs | - | ProjectsFeedsController.cs:33 |
| GET | /ProjectsFeeds/ProjectsFaqs | ProjectsFeedsController.ProjectsFaqs | - |
ProjectsFeedsController.cs:30 |
| GET | /ProjectsFeeds/ProjectsIssuesReplies | 
ProjectsFeedsController.ProjectsIssuesReplies | - | 
ProjectsFeedsController.cs:26 |
| GET | /ProjectsFeeds/ProjectsIssues | ProjectsFeedsController.ProjectsIssues |
- | ProjectsFeedsController.cs:23 |
| GET | /ProjectsFeeds/ProjectsFiles | ProjectsFeedsController.ProjectsFiles | -
| ProjectsFeedsController.cs:20 |
| GET | /ProjectsFeeds/ProjectsNews | ProjectsFeedsController.ProjectsNews | - |
ProjectsFeedsController.cs:17 |
| GET | /ProjectsFeeds/Get | ProjectsFeedsController.Get | - | 
ProjectsFeedsController.cs:15 |
| GET | /ProjectsFeeds/Index | ProjectsFeedsController.Index | - | 
ProjectsFeedsController.cs:13 |
| GET | /Feed/ShowBriefDescriptionAsync | 
FeedController.ShowBriefDescriptionAsync | - | FeedController.cs:131 |
| GET | /Feed/Announcements | FeedController.Announcements | - | 
FeedController.cs:127 |
| GET | /Feed/Surveys | FeedController.Surveys | - | FeedController.cs:124 |
| GET | /Feed/CoursesComments | FeedController.CoursesComments | - | 
FeedController.cs:120 |
| GET | /Feed/CoursesTopics | FeedController.CoursesTopics | - | 
FeedController.cs:116 |
| GET | /Feed/Courses | FeedController.Courses | - | FeedController.cs:113 |
| GET | /Feed/GetLatestChangesAsync | FeedController.GetLatestChangesAsync | - |
FeedController.cs:110 |
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
| GET | /Feed/LatestChanges | FeedController.LatestChanges | - | 
FeedController.cs:89 |
| GET | /Feed/NewsAuthor/{id?} | FeedController.NewsAuthor | - | 
FeedController.cs:77 |
| GET | /Feed/NewsComments | FeedController.NewsComments | - | 
FeedController.cs:73 |
| GET | /Feed/Author/{id?} | FeedController.Author | - | FeedController.cs:61 |
| GET | /Feed/Tag/{id?} | FeedController.Tag | - | FeedController.cs:50 |
| GET | /Feed/News | FeedController.News | - | FeedController.cs:47 |
| GET | /feeds/comments/{name?} | FeedController.UserComments | - | 
FeedController.cs:36 |
| GET | /Feed/Comments | FeedController.Comments | - | FeedController.cs:33 |
| GET | /feeds/posts/{name?} | FeedController.UserPosts | - | 
FeedController.cs:22 |
| POST | /Feed/Posts | FeedController.Posts | - | FeedController.cs:19 |
| GET | /Feed | FeedController.Index | - | FeedController.cs:15 |
| GET | /Feed/Index | FeedController.Index | - | FeedController.cs:15 |
| GET | /Exports/{type}/{name}.pdf | ExportsController.Get | - | 
ExportsController.cs:13 |
| POST | /api/JavaScriptErrorsReport | JavaScriptErrorsReportController.Log | - 
| JavaScriptErrorsReportController.cs:16 |
| GET | /.well-known/change-password | ╬╗ ChangePasswordEndpoint.cs:9 | - | 
ChangePasswordEndpoint.cs:9 |

## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `Advertisement` | ΓÇö | Id |
| `AdvertisementBookmark` | ΓÇö | Id |
| `AdvertisementComment` | ΓÇö | Id |
| `AdvertisementCommentBookmark` | ΓÇö | Id |
| `AdvertisementCommentReaction` | ΓÇö | Id |
| `AdvertisementCommentVisitor` | ΓÇö | Id |
| `AdvertisementReaction` | ΓÇö | Id |
| `AdvertisementTag` | ΓÇö | Id |
| `AdvertisementUserFile` | ΓÇö | Id |
| `AdvertisementUserFileVisitor` | ΓÇö | Id |
| `AdvertisementVisitor` | ΓÇö | Id |
| `AppDataProtectionKey` | ΓÇö | Id |
| `AppLogItem` | ΓÇö | Id |
| `AppSetting` | ΓÇö | Id |
| `Backlog` | ΓÇö | Id |
| `BacklogBookmark` | ΓÇö | Id |
| `BacklogComment` | ΓÇö | Id |
| `BacklogCommentBookmark` | ΓÇö | Id |
| `BacklogCommentReaction` | ΓÇö | Id |
| `BacklogCommentVisitor` | ΓÇö | Id |
| `BacklogReaction` | ΓÇö | Id |
| `BacklogTag` | ΓÇö | Id |
| `BacklogUserFile` | ΓÇö | Id |
| `BacklogUserFileVisitor` | ΓÇö | Id |
| `BacklogVisitor` | ΓÇö | Id |
| `BaseAuditedEntity` | ΓÇö | Id |
| `BlogPost` | ΓÇö | Id |
| `BlogPostBookmark` | ΓÇö | Id |
| `BlogPostComment` | ΓÇö | Id |
| `BlogPostCommentBookmark` | ΓÇö | Id |
| `BlogPostCommentReaction` | ΓÇö | Id |
| `BlogPostCommentVisitor` | ΓÇö | Id |
| `BlogPostDraft` | ΓÇö | Id |
| `BlogPostReaction` | ΓÇö | Id |
| `BlogPostTag` | ΓÇö | Id |
| `BlogPostUserFile` | ΓÇö | Id |
| `BlogPostUserFileVisitor` | ΓÇö | Id |
| `BlogPostVisitor` | ΓÇö | Id |
| `Course` | ΓÇö | Id |
| `CourseBookmark` | ΓÇö | Id |
| `CourseComment` | ΓÇö | Id |
| `CourseCommentBookmark` | ΓÇö | Id |
| `CourseCommentReaction` | ΓÇö | Id |
| `CourseCommentVisitor` | ΓÇö | Id |
| `CourseQuestion` | ΓÇö | Id |
| `CourseQuestionBookmark` | ΓÇö | Id |
| `CourseQuestionComment` | ΓÇö | Id |
| `CourseQuestionCommentBookmark` | ΓÇö | Id |
| `CourseQuestionCommentReaction` | ΓÇö | Id |
| `CourseQuestionCommentVisitor` | ΓÇö | Id |
| `CourseQuestionReaction` | ΓÇö | Id |
| `CourseQuestionTag` | ΓÇö | Id |
| `CourseQuestionUserFile` | ΓÇö | Id |
| `CourseQuestionUserFileVisitor` | ΓÇö | Id |
| `CourseQuestionVisitor` | ΓÇö | Id |
| `CourseReaction` | ΓÇö | Id |
| `CourseTag` | ΓÇö | Id |
| `CourseTopic` | ΓÇö | Id |
| `CourseTopicBookmark` | ΓÇö | Id |
| `CourseTopicComment` | ΓÇö | Id |
| `CourseTopicCommentBookmark` | ΓÇö | Id |
| `CourseTopicCommentReaction` | ΓÇö | Id |
| `CourseTopicCommentVisitor` | ΓÇö | Id |
| `CourseTopicReaction` | ΓÇö | Id |
| `CourseTopicTag` | ΓÇö | Id |
| `CourseTopicUserFile` | ΓÇö | Id |
| `CourseTopicUserFileVisitor` | ΓÇö | Id |
| `CourseTopicVisitor` | ΓÇö | Id |
| `CourseUserFile` | ΓÇö | Id |
| `CourseUserFileVisitor` | ΓÇö | Id |
| `CourseVisitor` | ΓÇö | Id |
| `CustomSidebar` | ΓÇö | Id |
| `DailyNewsItem` | ΓÇö | Id |
| `DailyNewsItemAIBacklog` | ΓÇö | Id |
| `DailyNewsItemBookmark` | ΓÇö | Id |
| `DailyNewsItemComment` | ΓÇö | Id |
| `DailyNewsItemCommentBookmark` | ΓÇö | Id |
| `DailyNewsItemCommentReaction` | ΓÇö | Id |
| `DailyNewsItemCommentVisitor` | ΓÇö | Id |
| `DailyNewsItemReaction` | ΓÇö | Id |
| `DailyNewsItemTag` | ΓÇö | Id |
| `DailyNewsItemUserFile` | ΓÇö | Id |
| `DailyNewsItemUserFileVisitor` | ΓÇö | Id |
| `DailyNewsItemVisitor` | ΓÇö | Id |
| `LearningPath` | ΓÇö | Id |
| `LearningPathBookmark` | ΓÇö | Id |
| `LearningPathComment` | ΓÇö | Id |
| `LearningPathCommentBookmark` | ΓÇö | Id |
| `LearningPathCommentReaction` | ΓÇö | Id |
| `LearningPathCommentVisitor` | ΓÇö | Id |
| `LearningPathReaction` | ΓÇö | Id |
| `LearningPathTag` | ΓÇö | Id |
| `LearningPathUserFile` | ΓÇö | Id |
| `LearningPathUserFileVisitor` | ΓÇö | Id |
| `LearningPathVisitor` | ΓÇö | Id |
| `MassEmail` | ΓÇö | Id |
| `ParentBookmarkEntity` | ΓÇö | Id |
| `ParentReactionEntity` | ΓÇö | Id |
| `ParentUserFileEntity` | ΓÇö | Id |
| `ParentVisitorEntity` | ΓÇö | Id |
| `PrivateMessage` | ΓÇö | Id |
| `PrivateMessageBookmark` | ΓÇö | Id |
| `PrivateMessageComment` | ΓÇö | Id |
| `PrivateMessageCommentBookmark` | ΓÇö | Id |
| `PrivateMessageCommentReaction` | ΓÇö | Id |
| `PrivateMessageCommentVisitor` | ΓÇö | Id |
| `PrivateMessageReaction` | ΓÇö | Id |
| `PrivateMessageTag` | ΓÇö | Id |
| `PrivateMessageUserFile` | ΓÇö | Id |
| `PrivateMessageUserFileVisitor` | ΓÇö | Id |
| `PrivateMessageVisitor` | ΓÇö | Id |
| `Project` | ΓÇö | Id |
| `ProjectBookmark` | ΓÇö | Id |
| `ProjectComment` | ΓÇö | Id |
| `ProjectCommentBookmark` | ΓÇö | Id |
| `ProjectCommentReaction` | ΓÇö | Id |
| `ProjectCommentVisitor` | ΓÇö | Id |
| `ProjectFaq` | ΓÇö | Id |
| `ProjectFaqBookmark` | ΓÇö | Id |
| `ProjectFaqComment` | ΓÇö | Id |
| `ProjectFaqCommentBookmark` | ΓÇö | Id |
| `ProjectFaqCommentReaction` | ΓÇö | Id |
| `ProjectFaqCommentVisitor` | ΓÇö | Id |
| `ProjectFaqReaction` | ΓÇö | Id |
| `ProjectFaqTag` | ΓÇö | Id |
| `ProjectFaqUserFile` | ΓÇö | Id |
| `ProjectFaqUserFileVisitor` | ΓÇö | Id |
| `ProjectFaqVisitor` | ΓÇö | Id |
| `ProjectIssue` | ΓÇö | Id |
| `ProjectIssueBookmark` | ΓÇö | Id |
| `ProjectIssueComment` | ΓÇö | Id |
| `ProjectIssueCommentBookmark` | ΓÇö | Id |
| `ProjectIssueCommentReaction` | ΓÇö | Id |
| `ProjectIssueCommentVisitor` | ΓÇö | Id |
| `ProjectIssuePriority` | ΓÇö | Id |
| `ProjectIssueReaction` | ΓÇö | Id |
| `ProjectIssueStatus` | ΓÇö | Id |
| `ProjectIssueTag` | ΓÇö | Id |
| `ProjectIssueType` | ΓÇö | Id |
| `ProjectIssueUserFile` | ΓÇö | Id |
| `ProjectIssueUserFileVisitor` | ΓÇö | Id |
| `ProjectIssueVisitor` | ΓÇö | Id |
| `ProjectReaction` | ΓÇö | Id |
| `ProjectRelease` | ΓÇö | Id |
| `ProjectReleaseBookmark` | ΓÇö | Id |
| `ProjectReleaseComment` | ΓÇö | Id |
| `ProjectReleaseCommentBookmark` | ΓÇö | Id |
| `ProjectReleaseCommentReaction` | ΓÇö | Id |
| `ProjectReleaseCommentVisitor` | ΓÇö | Id |
| `ProjectReleaseReaction` | ΓÇö | Id |
| `ProjectReleaseTag` | ΓÇö | Id |
| `ProjectReleaseUserFile` | ΓÇö | Id |
| `ProjectReleaseUserFileVisitor` | ΓÇö | Id |
| `ProjectReleaseVisitor` | ΓÇö | Id |
| `ProjectTag` | ΓÇö | Id |
| `ProjectUserFile` | ΓÇö | Id |
| `ProjectUserFileVisitor` | ΓÇö | Id |
| `ProjectVisitor` | ΓÇö | Id |
| `Role` | ΓÇö | Id |
| `SearchItem` | ΓÇö | Id |
| `SearchItemBookmark` | ΓÇö | Id |
| `SearchItemComment` | ΓÇö | Id |
| `SearchItemCommentBookmark` | ΓÇö | Id |
| `SearchItemCommentReaction` | ΓÇö | Id |
| `SearchItemCommentVisitor` | ΓÇö | Id |
| `SearchItemReaction` | ΓÇö | Id |
| `SearchItemTag` | ΓÇö | Id |
| `SearchItemUserFile` | ΓÇö | Id |
| `SearchItemUserFileVisitor` | ΓÇö | Id |
| `SearchItemVisitor` | ΓÇö | Id |
| `SiteReferrer` | ΓÇö | Id |
| `SiteReferrer` | ΓÇö | Id |
| `SiteUrl` | ΓÇö | Id |
| `SiteUrl` | ΓÇö | Id |
| `StackExchangeQuestion` | ΓÇö | Id |
| `StackExchangeQuestionBookmark` | ΓÇö | Id |
| `StackExchangeQuestionComment` | ΓÇö | Id |
| `StackExchangeQuestionCommentBookmark` | ΓÇö | Id |
| `StackExchangeQuestionCommentReaction` | ΓÇö | Id |
| `StackExchangeQuestionCommentVisitor` | ΓÇö | Id |
| `StackExchangeQuestionReaction` | ΓÇö | Id |
| `StackExchangeQuestionTag` | ΓÇö | Id |
| `StackExchangeQuestionUserFile` | ΓÇö | Id |
| `StackExchangeQuestionUserFileVisitor` | ΓÇö | Id |
| `StackExchangeQuestionVisitor` | ΓÇö | Id |
| `Survey` | ΓÇö | Id |
| `SurveyBookmark` | ΓÇö | Id |
| `SurveyComment` | ΓÇö | Id |
| `SurveyCommentBookmark` | ΓÇö | Id |
| `SurveyCommentReaction` | ΓÇö | Id |
| `SurveyCommentVisitor` | ΓÇö | Id |
| `SurveyItem` | ΓÇö | Id |
| `SurveyReaction` | ΓÇö | Id |
| `SurveyTag` | ΓÇö | Id |
| `SurveyUserFile` | ΓÇö | Id |
| `SurveyUserFileVisitor` | ΓÇö | Id |
| `SurveyVisitor` | ΓÇö | Id |
| `User` | ΓÇö | Id |
| `UserProfileBookmark` | ΓÇö | Id |
| `UserProfileComment` | ΓÇö | Id |
| `UserProfileCommentBookmark` | ΓÇö | Id |
| `UserProfileCommentReaction` | ΓÇö | Id |
| `UserProfileCommentVisitor` | ΓÇö | Id |
| `UserProfileReaction` | ΓÇö | Id |
| `UserProfileUserFile` | ΓÇö | Id |
| `UserProfileUserFileVisitor` | ΓÇö | Id |
| `UserProfileVisitor` | ΓÇö | Id |
| `UserUsedPassword` | ΓÇö | Id |

**31 EF Core migrations found.**

## Middleware pipeline

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

## DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Scoped | serviceProvider =>
        {
            var context = 
serviceProvider.GetRequiredService<ApplicationDbContext>();
            SetCascadeOnSaveChanges(context);

            return context;
        } | [factory] | SQLiteServiceCollectionExtensions.cs:14 |
| Extension | AddDbContextPool | AddDbContextPool ΓåÆ (serviceProvider, 
optionsBuilder)
... | SQLiteServiceCollectionExtensions.cs:11 |
| Singleton | _ => configuration | [factory] | SQLiteContextFactory.cs:34 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors ΓåÆ new 
TestHostingEnvironment() | SQLiteContextFactory.cs:28 |
| Scoped | IWebHostEnvironment | IWebHostEnvironment ΓåÆ TestHostingEnvironment | 
SQLiteContextFactory.cs:27 |
| Singleton | IAppFoldersService | IAppFoldersService ΓåÆ AppFoldersService | 
SQLiteContextFactory.cs:26 |
| Singleton | ILoggerFactory | ILoggerFactory ΓåÆ LoggerFactory | 
SQLiteContextFactory.cs:25 |
| Singleton | IHttpContextAccessor | IHttpContextAccessor ΓåÆ HttpContextAccessor 
| SQLiteContextFactory.cs:24 |
| Extension | AddLogging | AddLogging ΓåÆ cfg => cfg.AddSimpleConsole(opts =>
... | SQLiteContextFactory.cs:17 |
| Extension | AddOptions | AddOptions ΓåÆ (AddOptions) | 
SQLiteContextFactory.cs:15 |
| Singleton | ILoggerProvider | ILoggerProvider ΓåÆ EfDbLoggerProvider | 
EfDbLoggerFactoryExtensions.cs:9 |
| Extension | AddOptions | AddOptions ΓåÆ StartupSettingsModel | 
ServicesRegistry.cs:37 |
| Extension | AddCustomizedAuthentication | AddCustomizedAuthentication ΓåÆ 
siteSettings | ServicesRegistry.cs:32 |
| Extension | AddCustomizedControllers | AddCustomizedControllers ΓåÆ 
(AddCustomizedControllers) | ServicesRegistry.cs:31 |
| Extension | AddSchedulers | AddSchedulers ΓåÆ (AddSchedulers) | 
ServicesRegistry.cs:29 |
| Extension | AddDNTCommonWeb | AddDNTCommonWeb ΓåÆ (AddDNTCommonWeb) | 
ServicesRegistry.cs:28 |
| Extension | AddCustomizedDataProtection | AddCustomizedDataProtection ΓåÆ 
siteSettings | ServicesRegistry.cs:27 |
| Extension | AddConfiguredDbContext | AddConfiguredDbContext ΓåÆ siteSettings | 
ServicesRegistry.cs:26 |
| Bulk | AutoInjectAllServices | [bulk auto-registration] | 
ServicesRegistry.cs:23 |
| Extension | AddIPrincipal | AddIPrincipal ΓåÆ (AddIPrincipal) | 
ServicesRegistry.cs:22 |
| Extension | AddHttpContextAccessor | AddHttpContextAccessor ΓåÆ 
(AddHttpContextAccessor) | ServicesRegistry.cs:21 |
| Extension | AddForwardedHeadersOptions | AddForwardedHeadersOptions ΓåÆ 
(AddForwardedHeadersOptions) | ServicesRegistry.cs:19 |
| Extension | AddOptions | AddOptions ΓåÆ configuration | ServicesRegistry.cs:18 |
| Extension | AddDNTScheduler | AddDNTScheduler ΓåÆ options =>
... | SchedulersConfig.cs:18 |
| Extension | AddProblemDetails | AddProblemDetails ΓåÆ (AddProblemDetails) | 
MvcControllersConfig.cs:9 |
| Extension | AddRequestTimeouts | AddRequestTimeouts ΓåÆ options =>
... | MvcControllersConfig.cs:9 |
| Extension | AddLargeFilesUploadSupport | AddLargeFilesUploadSupport ΓåÆ 
(AddLargeFilesUploadSupport) | MvcControllersConfig.cs:9 |
| Extension | AddOutputCache | AddOutputCache ΓåÆ options => { 
options.AddPolicy(AlwaysCachePolicy.Name, Al... | MvcControllersConfig.cs:9 |
| Extension | AddControllers | AddControllers ΓåÆ options =>
... | MvcControllersConfig.cs:9 |
| Extension | AddCustomJsonOptionsForWebApps | AddCustomJsonOptionsForWebApps ΓåÆ 
(AddCustomJsonOptionsForWebApps) | MvcControllersConfig.cs:9 |
| Extension | AddEFSecondLevelCache | AddEFSecondLevelCache ΓåÆ options => 
options.UseMemoryCacheProvider()
... | DbContextConfig.cs:38 |
| Extension | AddEfSecondLevelCacheInterceptor | 
AddEfSecondLevelCacheInterceptor ΓåÆ environment | DbContextConfig.cs:33 |
| Singleton | AuditableEntitiesInterceptor | AuditableEntitiesInterceptor | 
DbContextConfig.cs:32 |
| Singleton | EfExceptionsInterceptor | EfExceptionsInterceptor | 
DbContextConfig.cs:31 |
| Extension | AddConfiguredSqLiteDbContext | AddConfiguredSqLiteDbContext ΓåÆ 
(AddConfiguredSqLiteDbContext) | DbContextConfig.cs:24 |
| Extension | AddEfCoreInterceptors | AddEfCoreInterceptors ΓåÆ environment | 
DbContextConfig.cs:23 |
| Extension | AddDataProtection | AddDataProtection ΓåÆ (AddDataProtection) | 
DataProtectionConfig.cs:29 |
| Singleton | serviceProvider =>
        {
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                serviceProvider.RunScopedService<IXmlRepository>(xmlRepository
                    => options.XmlRepository = xmlRepository);
            });
        } | [factory: new ConfigureOptions<KeyManagementOptions>(options =>
            {
                serviceProvider.RunScopedService<IXmlRepository>(xmlRepository
                    => options.XmlRepository = xmlRepository);
            })] | DataProtectionConfig.cs:20 |
| Singleton | IXmlRepository | IXmlRepository ΓåÆ DataProtectionKeyService | 
DataProtectionConfig.cs:18 |
| Extension | AddAuthentication | AddAuthentication ΓåÆ options =>
... | AuthenticationConfig.cs:29 |
| Extension | AddCookie | AddCookie ΓåÆ options =>
... | AuthenticationConfig.cs:29 |
| Extension | AddAuthorization | AddAuthorization ΓåÆ options =>
... | AuthenticationConfig.cs:23 |
| Scoped | AuthenticationStateProvider | AuthenticationStateProvider ΓåÆ 
IdentityRevalidatingAuthenticationStateProvider | AuthenticationConfig.cs:21 |
| Extension | AddCascadingAuthenticationState | AddCascadingAuthenticationState 
ΓåÆ (AddCascadingAuthenticationState) | AuthenticationConfig.cs:20 |
| Extension | AddCustomizedServices | AddCustomizedServices ΓåÆ host | 
Program.cs:24 |
| Extension | AddControllers | AddControllers ΓåÆ (AddControllers) | Program.cs:23
|
| Extension | AddRazorComponents | AddRazorComponents ΓåÆ (AddRazorComponents) | 
Program.cs:22 |
| Extension | AddInteractiveServerComponents | AddInteractiveServerComponents ΓåÆ 
(AddInteractiveServerComponents) | Program.cs:22 |

## Related types

- **Presentation**: ExportsController, FeedController, FileController, 
FtsController, JavaScriptErrorsReportController, OpenSearchController, 
ProjectsFeedsController, SitemapController, UploadFileController, 
WelcomeController, AppSetting, BaseAuditedEntity, BlogPost, BlogPostDraft, 
Course, CourseQuestion, DailyNewsItem, ParentVisitorEntity, PrivateMessage, 
Project, ProjectFaq, ProjectIssue, SiteReferrer, SiteUrl, Survey, SurveyItem, 
User, V2024_04_19_1424, AIDailyNewsBacklogsJob, AIDailyNewsJob, 
DisableInactiveUsersJob, AppFoldersService, AuditableEntitiesInterceptor, 
DataProtectionKeyService, EfDbLoggerProvider, EfExceptionsInterceptor, 
IdentityRevalidatingAuthenticationStateProvider, AccountModel, 
ActivatedAccountModel, Activation, AddAdvertisementForm, AddCustomSidebar, 
AddDailyNewsItemAIBacklogs, AddDailyNewsItemAIBacklogsHelp, 
AddGeneralAdvertisementModel, AddJobOfferAdvertisementForm, 
AddNewProjectAdvertisementModel, AdminsEmailsService, AdminUserDataSeeder, 
AdminUserSeedModel, AdvertisementBookmarkConfig, 
AdvertisementCommentBookmarkConfig, AdvertisementCommentConfig, 
AdvertisementCommentReactionConfig, AdvertisementCommentsService, 
AdvertisementCommentVisitorConfig, AdvertisementConfig, AdvertisementModel

---
*Generated in 72.8ms | 1289 types (58 active, 1231 pruned) | Compression: 
TrivialMemberCompressor(ΓêÆ9%) ┬╖ StructuralDeduplicator(ΓêÆ15%) | Schema v1.1*

> ≡ƒÆí Narrow this output with `--around TypeName` or `--around 
TypeName:MethodName` for focused context.

analyzed 1336 files ┬╖ 58 types kept of 1289 ┬╖ 5763/8000 tokens ┬╖ 8.3s stage2 
├ù2.0 stage3 ├ù2.1

                             Stage Timing                              
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Stage                   Γöé   Time Γöé Bar                              Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé DiscoveryAndCacheWarmup Γöé  133ms Γöé                                  Γöé
Γöé GenericExtraction       Γöé 6710ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê Γöé
Γöé SignalSealing           Γöé    1ms Γöé                                  Γöé
Γöé SpecificExtraction      Γöé 1373ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûê                           Γöé
Γöé Scoring                 Γöé   28ms Γöé                                  Γöé
Γöé Compression             Γöé   39ms Γöé                                  Γöé
Γöé Total                   Γöé 8329ms Γöé                                  Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                                   Extractors                                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Name                     Γöé   Time Γöé +Types Γöé +Dets Γöé Status                  Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé SyntaxStructureExtractor Γöé 6708ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé DiRegistrationExtractor  Γöé 6707ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé EndpointExtractor        Γöé 1372ms Γöé      0 Γöé   310 Γöé ran                     Γöé
Γöé EfCoreExtractor          Γöé  689ms Γöé      0 Γöé   309 Γöé ran                     Γöé
Γöé InMemoryEventBusExtracto Γöé  447ms Γöé      0 Γöé   282 Γöé ran                     Γöé
Γöé r                        Γöé        Γöé        Γöé       Γöé                         Γöé
Γöé ControllerActionExtracto Γöé  403ms Γöé      0 Γöé   278 Γöé ran                     Γöé
Γöé r                        Γöé        Γöé        Γöé       Γöé                         Γöé
Γöé ProgramCsFlowExtractor   Γöé  171ms Γöé      0 Γöé    39 Γöé ran                     Γöé
Γöé FileTreeExtractor        Γöé   68ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé SolutionDiscovery        Γöé   24ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé ProjectStructure         Γöé   21ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé DependencyExtractor      Γöé   20ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé LayerClassifier          Γöé   20ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé AntiPatternDetector      Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé AspireExtractor          Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé needs aspire            Γöé
Γöé CallGraphExtractor       Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé EventBusExtractor        Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé needs masstransit or    Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé nservicebus             Γöé
Γöé IndirectWiringDetector   Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé MediatRExtractor         Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé needs mediatr           Γöé
Γöé SourceBodyExtractor      Γöé    0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                          Γöé        Γöé        Γöé       Γöé ShouldRun               Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                Hard Exclusions                
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Scorer                 Γöé Checked Γöé Excluded Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé PatternRelevancePruner Γöé    1289 Γöé        ΓÇö Γöé
Γöé CallReachabilityPruner Γöé    1289 Γöé        ΓÇö Γöé
Γöé PathProximityPruner    Γöé    1289 Γöé        ΓÇö Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
cache 0% hit ┬╖ 1336 files ┬╖ 3 projects
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé  Metric  Γöé        Value        Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé Solution Γöé    _eval-dntsite    Γöé
Γöé   Time   Γöé       8743ms        Γöé
Γöé  Tokens  Γöé ~5763 (budget 8000) Γöé
Γöé Version  Γöé v1.0.5-preview.0.42 Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
