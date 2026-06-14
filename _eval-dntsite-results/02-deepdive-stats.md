Overview map (no focus).
deep-dive without --focus behaves like overview ΓÇö give a starting point
Analyzing project...

## DevContext ΓÇö Slice on project

**Architecture**: ControllerBased (80% confidence)
**Signals**: controllers ┬╖ minimal-apis ┬╖ efcore
**Projects**: 3 ΓÇö DntSite.Web, DntSite.Web.Common.BlazorSsr, DntSite.Tests
**Profile**: debug | **Tokens**: ~2404 (budget 8000) | **Types**: 25 in output

---
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

## Call graph

```text
**DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.Equals**
Γö£ΓöÇ `string.Equals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:117)`
Γö£ΓöÇ `string.Equals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:116)`
Γö£ΓöÇ `y.GetType` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:111)`
Γö£ΓöÇ `x.GetType` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:111)`
Γö£ΓöÇ `DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.ReferenceEquals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:96)`
Γö£ΓöÇ `DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.nameof` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:76)`
ΓööΓöÇ `DntSite.Web.Common.BlazorSsr.Models.BreadCrumb.Equals` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web.Common.BlazorSsr\Models\Brea
dCrumb.cs:73)`

**DntSite.Web.Features.Courses.EfConfig.CourseTopicCommentConfig.Configure**
Γö£ΓöÇ `builder.HasOne` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:27)`
Γö£ΓöÇ `builder.HasOne(comment => comment.Reply).WithMany` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:27)`
Γö£ΓöÇ `builder.HasOne(comment => comment.Reply)
            .WithMany(entity => entity.Children).HasForeignKey` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:27)`
Γö£ΓöÇ `builder.HasOne(comment => comment.Reply)
            .WithMany(entity => entity.Children)
            .HasForeignKey(comment => comment.ReplyId).IsRequired` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:27)`
Γö£ΓöÇ `builder.HasIndex` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:24)`
Γö£ΓöÇ `builder.HasOne` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:18)`
Γö£ΓöÇ `builder.HasOne(entity => entity.User).WithMany` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:18)`
Γö£ΓöÇ `builder.HasOne(entity => entity.User)
            .WithMany(user => user.CourseTopicComments).HasForeignKey` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:18)`
Γö£ΓöÇ `builder.HasOne(entity => entity.User)
            .WithMany(user => user.CourseTopicComments)
            .HasForeignKey(entity => entity.UserId).IsRequired` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:18)`
Γö£ΓöÇ `builder.HasOne` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:13)`
Γö£ΓöÇ `builder.HasOne(entity => entity.Parent).WithMany` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:13)`
ΓööΓöÇ `builder.HasOne(entity => entity.Parent)
            .WithMany(@base => @base.Comments).HasForeignKey` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Courses\EfConfig\Co
urseTopicCommentConfig.cs:13)`

**DntSite.Web.Features.StackExchangeQuestions.Components.MarkQuestionCommentAsAn
swer.OnValidSubmitAsync**
Γö£ΓöÇ `string.Create` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Components\MarkQuestionCommentAsAnswer.razor.cs:51)`
Γö£ΓöÇ `DntSite.Web.Features.AppConfigs.Components.ApplicationState.NavigateTo` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Components\MarkQuestionCommentAsAnswer.razor.cs:51)`
Γöé  ΓööΓöÇ `NavigationManager.NavigateTo` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Componen
ts\ApplicationState.razor.cs:62)`
Γö£ΓöÇ 
`DntSite.Web.Features.StackExchangeQuestions.Services.QuestionsCommentsService.M
arkQuestionCommentAsAnswerAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Components\MarkQuestionCommentAsAnswer.razor.cs:46)`
Γöé  Γö£ΓöÇ 
`DntSite.Web.Features.Persistence.UnitOfWork.ApplicationDbContext.SaveChangesAsy
nc` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsCommentsService.cs:204)`
Γöé  ΓööΓöÇ 
`DntSite.Web.Features.StackExchangeQuestions.Services.QuestionsCommentsService.F
indStackExchangeQuestionCommentIncludeParentAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsCommentsService.cs:194)`
Γöé     Γö£ΓöÇ `DbSet.Include` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsCommentsService.cs:93)`
Γöé     Γö£ΓöÇ `_questionComments.Include(x => x.Parent).OrderBy` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsCommentsService.cs:93)`
Γöé     ΓööΓöÇ `_questionComments.Include(x => x.Parent).OrderBy(x => 
x.Id).FirstOrDefaultAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsCommentsService.cs:93)`
Γö£ΓöÇ 
`DntSite.Web.Features.StackExchangeQuestions.Services.QuestionsCommentsService.N
otifyQuestionCommentIsApprovedAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Components\MarkQuestionCommentAsAnswer.razor.cs:42)`
Γöé  ΓööΓöÇ 
`DntSite.Web.Features.StackExchangeQuestions.Services.QuestionsEmailsService.Que
stionCommentIsApprovedSendEmailToWritersAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsCommentsService.cs:210)`
Γöé     Γö£ΓöÇ `comment.ToString` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsEmailsService.cs:205)`
Γöé     Γö£ΓöÇ `comment.ToString` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsEmailsService.cs:204)`
Γöé     Γö£ΓöÇ `string.Create` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsEmailsService.cs:198)`
Γöé     Γö£ΓöÇ `string.Create` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsEmailsService.cs:197)`
Γöé     Γö£ΓöÇ 
`DntSite.Web.Features.Common.Services.EmailsFactoryService.SendEmailToIdAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsEmailsService.cs:196)`
Γöé     Γöé  Γö£ΓöÇ `unknown.SendEmailAsync<TLayout, TLayoutModel>` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Common\Services\Ema
ilsFactoryService.cs:159)`
Γöé     Γöé  ΓööΓöÇ `DntSite.Web.Features.Common.Services.CommonService.FindUserAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Common\Services\Ema
ilsFactoryService.cs:150)`
Γöé     Γöé     Γö£ΓöÇ `DbSet.FindAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Common\Services\Com
monService.cs:85)`
Γöé     Γöé     Γö£ΓöÇ `ValueTask.FromResult` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Common\Services\Com
monService.cs:85)`
Γöé     Γöé     Γö£ΓöÇ `DbSet.OrderBy` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Common\Services\Com
monService.cs:82)`
Γöé     Γöé     ΓööΓöÇ `_users.OrderBy(x => x.Id).FirstOrDefaultAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Common\Services\Com
monService.cs:82)`
Γöé     ΓööΓöÇ 
`DntSite.Web.Features.StackExchangeQuestions.Services.QuestionsEmailsService.IsP
ostCommentatorAuthorOfPost` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Services\QuestionsEmailsService.cs:191)`
ΓööΓöÇ 
`DntSite.Web.Features.StackExchangeQuestions.Services.QuestionsCommentsService.M
arkQuestionCommentAsAnswerAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\StackExchangeQuesti
ons\Components\MarkQuestionCommentAsAnswer.razor.cs:40)`

**DntSite.Web.Features.News.Services.DailyNewsPdfExportService.ShouldNotMergeIte
msAsync**
Γö£ΓöÇ `DntSite.Web.Features.Exports.Services.PdfExportService.HasChangedItem` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\News\Services\Daily
NewsPdfExportService.cs:184)`
Γöé  Γö£ΓöÇ `DateTime.ToDateOnly` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:129)`
Γöé  Γö£ΓöÇ `DateTime.AddDays` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:128)`
Γöé  Γö£ΓöÇ `DateTime.UtcNow.AddDays(value: -1).ToDateOnly` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:128)`
Γöé  Γö£ΓöÇ `x.ToDateOnly` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:126)`
Γöé  Γö£ΓöÇ `postFiles.Max` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:126)`
Γöé  Γö£ΓöÇ `postIds.Contains` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:119)`
Γöé  Γö£ΓöÇ `files.Where` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:119)`
Γöé  Γö£ΓöÇ `files.Where(x => postIds.Contains(x.Id)).ToList` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:119)`
Γöé  ΓööΓöÇ 
`DntSite.Web.Features.Exports.Services.PdfExportService.GetAvailableExportedFile
s` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:112)`
Γöé     Γö£ΓöÇ `Path.GetFileNameWithoutExtension` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:99)`
Γöé     Γö£ΓöÇ `Path.GetFileNameWithoutExtension(item.FullName).Split` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:99)`
Γöé     Γö£ΓöÇ `Path.GetFileNameWithoutExtension(item.FullName)
                        .Split(separator: '-', 
StringSplitOptions.RemoveEmptyEntries)[^1].ToInt` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:99)`
Γöé     Γö£ΓöÇ `itemPdfFiles.Select` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:98)`
Γöé     Γö£ΓöÇ `itemPdfFiles.Select(item => (
                    Path.GetFileNameWithoutExtension(item.FullName)
                        .Split(separator: '-', 
StringSplitOptions.RemoveEmptyEntries)[^1]
                        .ToInt(), item)).ToList` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:98)`
Γöé     Γö£ΓöÇ 
`DntSite.Web.Features.Exports.Services.PdfExportService.GetExportsOutputFolder` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:94)`
Γöé     Γöé  Γö£ΓöÇ `path.TryCreateDirectory` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:68)`
Γöé     Γöé  Γö£ΓöÇ `itemType.ToLowerInvariant` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:67)`
Γöé     Γöé  Γö£ΓöÇ 
`DntSite.Web.Features.AppConfigs.Services.AppFoldersService.SafePathCombine` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:67)`
Γöé     Γöé  ΓööΓöÇ `ArgumentNullException.ThrowIfNull` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:65)`
Γöé     ΓööΓöÇ `new DirectoryInfo(GetExportsOutputFolder(itemType)).GetFiles` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:94)`
ΓööΓöÇ 
`DntSite.Web.Features.Exports.Services.PdfExportService.GetExportFileLocationAsy
nc` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\News\Services\Daily
NewsPdfExportService.cs:183)`
   Γö£ΓöÇ `DateTimeOffset.AddDays` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:60)`
   Γö£ΓöÇ 
`DntSite.Web.Features.AppConfigs.Services.AppFoldersService.GetWebRootAppDataFol
derPath` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:56)`
   Γöé  Γö£ΓöÇ `path.CheckDirExists` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\AppFoldersService.cs:101)`
   Γöé  Γö£ΓöÇ `path.SafePathCombine` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\AppFoldersService.cs:98)`
   Γöé  Γö£ΓöÇ `string.SafePathCombine` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\AppFoldersService.cs:94)`
   Γöé  ΓööΓöÇ `ArgumentNullException.ThrowIfNull` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\AppFoldersService.cs:92)`
   Γö£ΓöÇ `outputPdfFilePath.Replace` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:56)`
   Γö£ΓöÇ `siteRootUri.CombineUrl` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:55)`
   Γö£ΓöÇ `new FileInfo(outputPdfFilePath).ToFormattedFileSize` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:53)`
   Γö£ΓöÇ `outputPdfFilePath.FileExists` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:45)`
   Γö£ΓöÇ `outputFolder.SafePathCombine` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:44)`
   Γö£ΓöÇ 
`DntSite.Web.Features.Exports.Services.PdfExportService.GetExportsOutputFolder` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:43)`
   Γö£ΓöÇ `string.Create` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:40)`
   Γö£ΓöÇ `string.Create(CultureInfo.InvariantCulture, 
$"{domain}-{itemType.Name}-{id}.pdf").ToLowerInvariant` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:40)`
   Γö£ΓöÇ 
`DntSite.Web.Features.AppConfigs.Services.CachedAppSettingsProvider.GetSiteRootD
omainAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:38)`
   Γöé  Γö£ΓöÇ `siteRootUri.IsValidUrl` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:38)`
   Γöé  ΓööΓöÇ 
`DntSite.Web.Features.AppConfigs.Services.CachedAppSettingsProvider.GetAppSettin
gsAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:36)`
   Γöé     Γö£ΓöÇ `uow.DbSet` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
   Γöé     Γö£ΓöÇ `uow.DbSet<AppSetting>().AsNoTracking` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
   Γöé     Γö£ΓöÇ `uow.DbSet<AppSetting>().AsNoTracking().OrderBy` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
   Γöé     Γö£ΓöÇ `uow.DbSet<AppSetting>().AsNoTracking().OrderBy(x => 
x.Id).FirstOrDefaultAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
   Γöé     Γö£ΓöÇ `IServiceProvider.RunScopedServiceAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:21)`
   Γöé     Γö£ΓöÇ `TimeSpan.FromSeconds` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:19)`
   Γöé     ΓööΓöÇ `ILockerService.LockAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:19)`
   ΓööΓöÇ `DntSite.Web.Features.Exports.Services.PdfExportService.nameof` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Exports\Services\Pd
fExportService.cs:36)`

**DntSite.Web.Features.RssFeeds.Services.FeedsService.GetProjectFilesAsync**
Γö£ΓöÇ `string.Format` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:468)`
Γö£ΓöÇ `item.MapToProjectReleaseWhatsNewItemModel` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:465)`
Γö£ΓöÇ `list.Select` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:464)`
Γö£ΓöÇ `list.Data.Select(item
                => 
item.MapToProjectReleaseWhatsNewItemModel(appSetting.SiteRootUri, 
showBriefDescription)).ToList` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:464)`
Γö£ΓöÇ `DntSite.Web.Features.RssFeeds.Services.FeedsService.GetAppSettingsAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:462)`
Γöé  ΓööΓöÇ 
`DntSite.Web.Features.AppConfigs.Services.CachedAppSettingsProvider.GetAppSettin
gsAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:716)`
Γöé     Γö£ΓöÇ `uow.DbSet` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
Γöé     Γö£ΓöÇ `uow.DbSet<AppSetting>().AsNoTracking` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
Γöé     Γö£ΓöÇ `uow.DbSet<AppSetting>().AsNoTracking().OrderBy` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
Γöé     Γö£ΓöÇ `uow.DbSet<AppSetting>().AsNoTracking().OrderBy(x => 
x.Id).FirstOrDefaultAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:23)`
Γöé     Γö£ΓöÇ `IServiceProvider.RunScopedServiceAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:21)`
Γöé     Γö£ΓöÇ `TimeSpan.FromSeconds` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:19)`
Γöé     ΓööΓöÇ `ILockerService.LockAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\AppConfigs\Services
\CachedAppSettingsProvider.cs:19)`
Γö£ΓöÇ 
`DntSite.Web.Features.Projects.Services.ProjectReleasesService.GetAllProjectRele
asesAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:459)`
Γöé  Γö£ΓöÇ `query.ApplyQueryablePagingAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:101)`
Γöé  Γö£ΓöÇ `DbSet.Where` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:94)`
Γöé  Γö£ΓöÇ `_projectReleases.Where(x => x.IsDeleted == showDeletedItems && 
x.ProjectId == projectId).Include` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:94)`
Γöé  Γö£ΓöÇ `_projectReleases.Where(x => x.IsDeleted == showDeletedItems && 
x.ProjectId == projectId)
            .Include(x => x.User).Include` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:94)`
Γöé  Γö£ΓöÇ `_projectReleases.Where(x => x.IsDeleted == showDeletedItems && 
x.ProjectId == projectId)
            .Include(x => x.User)
            .Include(x => x.Project).Include` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:94)`
Γöé  Γö£ΓöÇ `_projectReleases.Where(x => x.IsDeleted == showDeletedItems && 
x.ProjectId == projectId)
            .Include(x => x.User)
            .Include(x => x.Project)
            .Include(x => x.Reactions).Include` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:94)`
Γöé  ΓööΓöÇ `_projectReleases.Where(x => x.IsDeleted == showDeletedItems && 
x.ProjectId == projectId)
            .Include(x => x.User)
            .Include(x => x.Project)
            .Include(x => x.Reactions)
            .Include(x => x.Bookmarks).AsNoTracking` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectReleasesService.cs:94)`
ΓööΓöÇ `DntSite.Web.Features.Projects.Services.ProjectsService.FindProjectAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\RssFeeds\Services\F
eedsService.cs:452)`
   ΓööΓöÇ `DbSet.FindAsync` 
`(C:\Code\DevContext2\_eval-dntsite\src\DntSite.Web\Features\Projects\Services\P
rojectsService.cs:43)`

```
## Data model (EF Core)

### `ApplicationDbContext`

| Entity | Aggregate root | Key properties |
|--------|---------------|----------------|
| `BaseEntity` | ΓÇö | Id |

**31 EF Core migrations found.**

## Background workers

- AIDailyNewsBacklogsJob (HostedService)
- AIDailyNewsJob (HostedService)
- EmptyPMsJob (HostedService)
- DailyBirthDatesEmailJob (HostedService)
- DailyNewsletterJob (HostedService)
- DeleteOrphansJob (HostedService)
- ExportToMergedPdfFilesJob (HostedService)
- ExportToSeparatePdfFilesJob (HostedService)
- ThumbnailsServiceJob (HostedService)
- FullTextSearchWriterJob (HostedService)
- DraftsJob (HostedService)
- HumansTxtJob (HostedService)
- BackupDataFolderJob (HostedService)
- BackupDatabaseJob (HostedService)
- ManageBacklogsJob (HostedService)
- NewPersianYearEmailsJob (HostedService)
- DisableInactiveUsersJob (HostedService)
- SendActivationEmailsJob (HostedService)
- UpdateDeletedNewsHttpStatusCodeJob (HostedService)
- UpdatePublicNewsHttpStatusCodeJob (HostedService)
- WebReadersListJob (HostedService)
- FreeSpaceCheckJob (HostedService)
- CheckAdminsLastVisitJob (HostedService)
- DotNetVersionCheckJob (HostedService)

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

---
*Generated in 29.9ms | 1289 types (25 active, 1264 pruned) | Compression: 
TrivialMemberCompressor(ΓêÆ9%) ┬╖ StructuralDeduplicator(ΓêÆ15%) | Schema v1.1*

analyzed 1336 files ┬╖ 25 types kept of 1289 ┬╖ 8774/8000 tokens ┬╖ 21.5s stage2 
├ù2.0 stage3 ├ù2.1

                           Stage Timing                           
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Stage                   Γöé    Time Γöé Bar                        Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé DiscoveryAndCacheWarmup Γöé   138ms Γöé                            Γöé
Γöé GenericExtraction       Γöé  6837ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê               Γöé
Γöé SignalSealing           Γöé     1ms Γöé                            Γöé
Γöé SpecificExtraction      Γöé 14410ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê Γöé
Γöé Scoring                 Γöé    57ms Γöé                            Γöé
Γöé Compression             Γöé    41ms Γöé                            Γöé
Γöé Total                   Γöé 21528ms Γöé                            Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                                   Extractors                                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Name                    Γöé    Time Γöé +Types Γöé +Dets Γöé Status                  Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé CallGraphExtractor      Γöé 14408ms Γöé      0 Γöé   115 Γöé ran                     Γöé
Γöé IndirectWiringDetector  Γöé 10591ms Γöé      0 Γöé   115 Γöé ran                     Γöé
Γöé SyntaxStructureExtracto Γöé  6834ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé r                       Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé DiRegistrationExtractor Γöé  6833ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé EndpointExtractor       Γöé  2779ms Γöé      0 Γöé   106 Γöé ran                     Γöé
Γöé EfCoreExtractor         Γöé  1185ms Γöé      0 Γöé   105 Γöé ran                     Γöé
Γöé InMemoryEventBusExtract Γöé   758ms Γöé      0 Γöé    79 Γöé ran                     Γöé
Γöé or                      Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé ControllerActionExtract Γöé   665ms Γöé      0 Γöé    71 Γöé ran                     Γöé
Γöé or                      Γöé         Γöé        Γöé       Γöé                         Γöé
Γöé ProgramCsFlowExtractor  Γöé   172ms Γöé      0 Γöé    39 Γöé ran                     Γöé
Γöé FileTreeExtractor       Γöé    68ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé ProjectStructure        Γöé    28ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé SolutionDiscovery       Γöé    24ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé DependencyExtractor     Γöé    21ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé LayerClassifier         Γöé    21ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé AntiPatternDetector     Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γöé AspireExtractor         Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs aspire            Γöé
Γöé EventBusExtractor       Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs masstransit or    Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé nservicebus             Γöé
Γöé MediatRExtractor        Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: signal gate:   Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé needs mediatr           Γöé
Γöé SourceBodyExtractor     Γöé     0ms Γöé      0 Γöé     0 Γöé skipped: gated by       Γöé
Γöé                         Γöé         Γöé        Γöé       Γöé ShouldRun               Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                   Scorer Funnel                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Scorer                 Γöé Before Γöé After Γöé Delta Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé PatternRelevancePruner Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γöé CallReachabilityPruner Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γöé PathProximityPruner    Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
cache 0% hit ┬╖ 1336 files ┬╖ 0 projects
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé  Metric  Γöé        Value        Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé Solution Γöé    _eval-dntsite    Γöé
Γöé   Time   Γöé       21934ms       Γöé
Γöé  Tokens  Γöé ~8774 (budget 8000) Γöé
Γöé Version  Γöé v1.0.5-preview.0.42 Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
