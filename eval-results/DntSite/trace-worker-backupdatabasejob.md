Slicing from BackupDatabaseJob, call graph on.
Analyzing project...

TRACE  BackupDatabaseJob
       
C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSite
.Web\Features\ServicesConfigs\SchedulersConfig.cs:61

▸ ENTRY  BackupDatabaseJob  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\ServicesConfigs\SchedulersConfig.cs:61)
   └─ call BackupDatabaseJob  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\ServicesConfigs\SchedulersConfig.cs:61)
      ├─ call WebSiteBackupService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\ScheduledTasks\BackupDatabaseJob.cs:18) [verified]
      │  ├─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\WebSiteBackupService.cs:105) [verified]
      │  │      return false;
      │  │      }
      │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\WebSiteBackupService.cs:114) [approx]
      │  │      var dbBackupFileName = 
string.Create(CultureInfo.InvariantCulture, $"db.backup.{NameSalt}.sqlite");
      │  │      return 
appFoldersService.BackupFolderPath.SafePathCombine(dbBackupFileName)!.Replace(ol
dValue: "'",
      │  ├─ call TelegramUploadBackupService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\WebSiteBackupService.cs:129) [verified]
      │  │      dbBackupFilePath, dbBackupFilePath.GetFileName(), 
telegramPartsInfo, cancellationToken);
      │  │      await Task.Delay(_delay, cancellationToken);
      │  │  ├─ call CachedAppSettingsProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\TelegramUploadBackupService.cs:145) 
[verified]
      │  │  │      }
      │  │  │      return null;
      │  │  ├─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\TelegramUploadBackupService.cs:153) 
[verified]
      │  │  │      if (telegramEPubGroup.IsActive && 
!telegramEPubGroup.AccessToken.IsEmpty() &&
      │  │  │      !telegramEPubGroup.ChatId.IsEmpty())
      │  │  ├─ call File  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\TelegramUploadBackupService.cs:196) [approx]
      │  │  │      await Task.Delay(_delay, cancellationToken);
      │  │  │      }
      │  │  ├─ call EmailsFactoryService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\TelegramUploadBackupService.cs:225) 
[verified]
      │  │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\TelegramUploadBackupService.cs:38) [verified]
      │  │  │      {
      │  │  │      parts?.Parts.DeleteParts(logger);
      │  │  │      }
      │  │  └─ call PartsInfo  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\TelegramUploadBackupService.cs:46) [verified]
      │  │         password: zipPassword, logger: logger, cancellationToken: 
cancellationToken);
      │  │         if (partPaths?.Count == 0)
      │  ├─ call BaleUploadBackupService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\WebSiteBackupService.cs:132) [verified]
      │  │      dbBackupFilePath.TryDeleteFile(logger);
      │  │      telegramPartsInfo?.Parts.DeleteParts(logger);
      │  │  ├─ call CachedAppSettingsProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\BaleUploadBackupService.cs:137) [verified]
      │  │  │      if (logger.IsEnabled(LogLevel.Critical))
      │  │  │      {
      │  │  │      logger.LogCritical(message: "`BaleBackupGroup` is not active 
or set.");
      │  │  ├─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\BaleUploadBackupService.cs:144) [verified]
      │  │  │      private async Task<TelegramBackupGroup?> 
GetBaleEPubGroupAsync()
      │  │  │      {
      │  │  ├─ call EmailsFactoryService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\BaleUploadBackupService.cs:228) [verified]
      │  │  │      private void LogBaleErrors(BaleApiResponseStatus? status)
      │  │  │      {
      │  │  │      if (IsFailed(status) && logger.IsEnabled(LogLevel.Error))
      │  │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\BaleUploadBackupService.cs:34) [verified]
      │  │  │      var useProvidedParts = parts.UseProvidedParts(zipPassword);
      │  │  └─ call PartsInfo  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\BaleUploadBackupService.cs:42) [verified]
      │  │         var partPaths = useProvidedParts ? parts?.Parts :
      │  │         isFolder ? await 
path.ZipAndSplitFolderToMultiplePartsAsync(tempDirectory, maxPartSizeMB,
      │  │         outputFileName, password: zipPassword, logger: logger, 
cancellationToken: cancellationToken) :
      │  └─ call OnlineSqliteBackupService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\WebSiteBackupService.cs:33) [verified]
      │         }
      │         }
      │         catch (Exception ex)
      │     └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\Services\OnlineSqliteBackupService.cs:44) [verified]
      │            }
      │            public async Task<bool> ValidateSqliteBackupAsync(string 
dbBackupFilePath,
      └─ call EPubExportService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\SiteBackup\ScheduledTasks\BackupDatabaseJob.cs:19) [verified]
         ├─ call EPubTocItems  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:350) [approx]
         │      html.AppendLine(value: "<div class='mb-3'>");
         │      html.AppendLine(value: "<ul class='list-group'>");
         ├─ call EPubExportHtmlProviderService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:357) [verified]
         │      htmlProviderService.AddHeader(html, listItem.Item.Title);
         │      foreach (var subItem in 
listItem.SubItems.OrderBy(ePubContentItem => ePubContentItem.Id))
         │  └─ call PdfExportService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportHtmlProviderService.cs:14) [verified]
         │         : $"""
         │         <div class='container-fluid min-vh-100 d-flex flex-column'>
         │         <div class='row flex-grow-1'>
         ├─ call EPubExportDocsInfoService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:221) [verified]
         │      bodyNode.InnerHtml;
         │      content = htmlProviderService.ApplyHtmlPageTemplate(title, 
bodyHtml, sideBar);
         ├─ call File  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:138) [approx]
         │      .Replace(oldValue: "font-size: 10pt;", newValue: "font-size: 
inherit;",
         │      StringComparison.OrdinalIgnoreCase));
         ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:143) [approx]
         │      epub.AddStylesheetData(epubPath: "vs.min.css",
         │      
File.ReadAllText(appFoldersService.ExportsAssetsFolder.SafePathCombine("vs.min.c
ss")!));
         ├─ call EPubExportDataProviderService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:95) [verified]
         │      fixLocalUrls: false, domain, sideBar, cancellationToken);
         │      await CreateItemsListContentAsync(epub, title: "نویسندگان",
         ├─ call CachedAppSettingsProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:28) [verified]
         │      return;
         │      }
         ├─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:32) [verified]
         │      if (tocItems is { ArticlesCount: 0, NewsCount: 0 })
         │      {
         └─ call WebSiteBackupService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Exports\Services\EPubExportService.cs:55) [verified]
                string baseUrl,
                string domain,
                CancellationToken cancellationToken)
            (truncated — more edges beyond depth/fan-out)

analyzed 1336 files · 1476 nodes · 179 edges · 94 entries · depth 4 · ~2406 
tokens · 38.3s stage2 ×2.0 stage3 ×1.3
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     DntSite.slnx     │
│   Time   │       38314ms        │
│  Tokens  │ ~2406 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.100 │
╰──────────┴──────────────────────╯
