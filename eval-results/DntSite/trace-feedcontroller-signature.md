Slicing from FeedController, call graph on.
Analyzing project...

TRACE  FeedController
       
C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSite
.Web\Features\RssFeeds\Controllers\FeedController.cs

▸ ENTRY  FeedController  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Controllers\FeedController.cs)
   ├─ call CachedAppSettingsProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Controllers\FeedController.cs:133) [verified]
   └─ call FeedsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Controllers\FeedController.cs:129) [verified]
      (truncated — more edges beyond depth/fan-out)
      ├─ call ProjectsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:274) [verified]
      ├─ call BlogPost  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:280) [approx]
      ├─ call ProjectReleasesService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:295) [verified]
      ├─ call ProjectIssuesService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:316) [verified]
      ├─ call ProjectIssueCommentsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:334) [verified]
      ├─ call ProjectFaqsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:386) [verified]
      ├─ call BlogPostDraftsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:259) [verified]
      ├─ call VoteCommentsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:352) [verified]
      ├─ call AdvertisementCommentsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:368) [verified]
      ├─ call LearningPathService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:166) [verified]
      ├─ call BacklogsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:119) [verified]
      └─ call QuestionsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:143) [verified]

analyzed 1336 files · 1476 nodes · 178 edges · 94 entries · depth 2 · ~728 
tokens · 35.8s stage2 ×2.0 stage3 ×1.3
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     DntSite.slnx     │
│   Time   │       35827ms        │
│  Tokens  │  ~728 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.100 │
╰──────────┴──────────────────────╯
