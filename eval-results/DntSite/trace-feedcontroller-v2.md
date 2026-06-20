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
          return appSetting.ShowRssBriefDescription;
          }
      (truncated — more edges beyond depth/fan-out)
      ├─ call ProjectsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:274) [verified]
      │      var rssItems = list.Data.Select(item
      │      => 
item.MapToProjectsReleasesWhatsNewItemModel(appSetting.SiteRootUri, 
showBriefDescription))
      │      .ToList();
      ├─ call BlogPost  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:280) [approx]
      │      return GetFeedChannel(title, appSetting, rssItems);
      │      }
      ├─ call ProjectReleasesService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:295) [verified]
      │      var rssItems = list.Data.Select(item
      │      => 
item.MapToProjectsIssuesWhatsNewItemModel(appSetting.SiteRootUri, 
showBriefDescription))
      │      .ToList();
      ├─ call ProjectIssuesService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:316) [verified]
      │      var title = $"فید {WhatsNewItemType.ProjectsIssuesReplies.Value}";
      ├─ call ProjectIssueCommentsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:334) [verified]
      │      return GetFeedChannel(title, appSetting, rssItems);
      │      }
      ├─ call ProjectFaqsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:386) [verified]
      │      var project = await 
projectsService.FindProjectAsync(projectId.Value);
      ├─ call BlogPostDraftsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:259) [verified]
      │      return GetFeedChannel(title, appSetting, rssItems);
      │      }
      ├─ call VoteCommentsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:352) [verified]
      │      return GetFeedChannel(title, appSetting, rssItems);
      │      }
      ├─ call AdvertisementCommentsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:368) [verified]
      │      var title = $"فید {WhatsNewItemType.ProjectsFaqs.Value}";
      ├─ call LearningPathService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:166) [verified]
      │      public async Task<WhatsNewFeedChannel> GetAllCoursesAsync(bool 
showBriefDescription,
      │      int pageNumber = 0,
      ├─ call BacklogsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:119) [verified]
      │      var appSetting = await GetAppSettingsAsync();
      └─ call QuestionsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\RssFeeds\Services\FeedsService.cs:143) [verified]
             var appSetting = await GetAppSettingsAsync();
             var rssItems = list.Data

analyzed 1336 files · 1452 nodes · 154 edges · 70 entries · depth 2 · ~1051 
tokens · 47.2s stage2 ×2.0 stage3 ×1.4
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     DntSite.slnx     │
│   Time   │       47253ms        │
│  Tokens  │ ~1051 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.100 │
╰──────────┴──────────────────────╯
