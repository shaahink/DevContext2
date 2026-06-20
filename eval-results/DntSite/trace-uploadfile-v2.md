Slicing from POST /api/UploadFile — handler resolved after scan.
Analyzing project...

TRACE  POST /api/UploadFile
       
C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSite
.Web\Features\UserFiles\Controllers\UploadFileController.cs:42

▸ ENTRY  POST /api/UploadFile  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:42)
   ├─ call UploadFileController.MessagesFilesUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:42)
   │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
   │  │      if (!isSaved)
   │  │      {
   │  │      return BadRequest(new FileUploadResultModel
   │  ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
   │  │      Url = uploadedFileUrl,
   │  │      FileName = file?.FileName ?? fileName
   │  │      });
   │  │  ├─ call EmailsFactoryService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\AdminsEmailsService.cs:61) [verified]
   │  │  │  ├─ call CommonService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Common\Services\EmailsFactoryService.cs:249) [verified]
   │  │  │  │      if (string.IsNullOrWhiteSpace(ip))
   │  │  │  │      {
   │  │  │  └─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\Common\Services\EmailsFactoryService.cs:246) [verified]
   │  │  │         }
   │  │  │         var ip = httpContextAccessor.HttpContext.GetIP();
   │  │  ├─ call CurrentUserService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\AdminsEmailsService.cs:44) [verified]
   │  │  │      => emailsFactoryService.SendEmailToAllAdminsAsync<RecycleEmail, 
RecycleEmailModel>(messageId: "SendRecycle",
   │  │  │      inReplyTo: "", references: "SendRecycle", new RecycleEmailModel
   │  │  │      {
   │  │  │  ├─ call CachedAppSettingsProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\CurrentUserService.cs:101) [verified]
   │  │  │  ├─ call UsersInfoService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\CurrentUserService.cs:69) [verified]
   │  │  │  │      if (httpContext?.IsGetRequest() != false)
   │  │  │  │      {
   │  │  │  │  └─ call CachedAppSettingsProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\UsersInfoService.cs:49) [verified]
   │  │  │  │         return await _users.OrderBy(x => 
x.Id).FirstOrDefaultAsync(x => x.EMail == eMail && x.Id != userId.Value) is
   │  │  │  │         null;
   │  │  │  ├─ call SpidersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\CurrentUserService.cs:48) [verified]
   │  │  │  │      => new()
   │  │  │  │      {
   │  │  │  │      User = await usersService.FindUserAsync(GetCurrentUserId()),
   │  │  │  └─ call UserRolesService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\CurrentUserService.cs:37) [verified]
   │  │  │         }
   │  │  │         public Task<bool> IsCurrentUserSpiderAsync() => 
spidersService.IsSpiderClientAsync(httpContextAccessor.HttpContext);
   │  │  │     ├─ call ApplicationDbContext  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\UserRolesService.cs:95) [verified]
   │  │  │     │      if (inputRoleValues is null || inputRoleValues.Count == 0)
   │  │  │     │      {
   │  │  │     │      user.Roles.Clear();
   │  │  │     ├─ call User  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\UserRolesService.cs:104) [approx]
   │  │  │     │      var newRoleValuesToAdd = 
inputRoleValues.Except(currentUserRoleValues).ToList();
   │  │  │     │      var correspondingDbNewRolesToAdd = await _roles
   │  │  │     ├─ call EmailsFactoryService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\UserRolesService.cs:134) [verified]
   │  │  │     │  (truncated — more edges beyond depth/fan-out)
   │  │  │     └─ call DeviceDetectionService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\UserRolesService.cs:31) [verified]
   │  │  │            var roles = await FindUserRolesAsync(user.Id);
   │  │  │            foreach (var role in roles)
   │  │  └─ call CommonService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserProfiles\Services\AdminsEmailsService.cs:20) [verified]
   │  │         Dest = destUri.ToString(),
   │  │         AdminUrl = adminUrl
   │  │         }, emailSubject: "ارجاع دهنده جدید برای تائید", addIp: true);
   │  └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
   │         });
   │         }
   │         }
   │     └─ call EfDbLoggerProvider  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\DbLogger\Services\EfDbLogger.cs:74) [verified]
   │            });
   │            }
   ├─ call UploadFileController.CommonFilesUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:38)
   │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
   │  │      if (!isSaved)
   │  │      {
   │  │      return BadRequest(new FileUploadResultModel
   │  ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
   │  │      Url = uploadedFileUrl,
   │  │      FileName = file?.FileName ?? fileName
   │  │      });
   │  │  (truncated — more edges beyond depth/fan-out)
   │  └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
   │         });
   │         }
   │         }
   │     (truncated — more edges beyond depth/fan-out)
   ├─ call UploadFileController.FileUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:34)
   │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
   │  │      if (!isSaved)
   │  │      {
   │  │      return BadRequest(new FileUploadResultModel
   │  ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
   │  │      Url = uploadedFileUrl,
   │  │      FileName = file?.FileName ?? fileName
   │  │      });
   │  │  (truncated — more edges beyond depth/fan-out)
   │  └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
   │         });
   │         }
   │         }
   │     (truncated — more edges beyond depth/fan-out)
   ├─ call UploadFileController.CourseFileUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:30)
   │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
   │  │      if (!isSaved)
   │  │      {
   │  │      return BadRequest(new FileUploadResultModel
   │  ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
   │  │      Url = uploadedFileUrl,
   │  │      FileName = file?.FileName ?? fileName
   │  │      });
   │  │  (truncated — more edges beyond depth/fan-out)
   │  └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
   │         });
   │         }
   │         }
   │     (truncated — more edges beyond depth/fan-out)
   ├─ call UploadFileController.CourseImagesUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:26)
   │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
   │  │      if (!isSaved)
   │  │      {
   │  │      return BadRequest(new FileUploadResultModel
   │  ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
   │  │      Url = uploadedFileUrl,
   │  │      FileName = file?.FileName ?? fileName
   │  │      });
   │  │  (truncated — more edges beyond depth/fan-out)
   │  └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
   │         });
   │         }
   │         }
   │     (truncated — more edges beyond depth/fan-out)
   ├─ call UploadFileController.MessagesImagesUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:22)
   │  ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
   │  │      if (!isSaved)
   │  │      {
   │  │      return BadRequest(new FileUploadResultModel
   │  ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
   │  │      Url = uploadedFileUrl,
   │  │      FileName = file?.FileName ?? fileName
   │  │      });
   │  │  (truncated — more edges beyond depth/fan-out)
   │  └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
   │         });
   │         }
   │         }
   │     (truncated — more edges beyond depth/fan-out)
   └─ call UploadFileController.ImageUpload  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:18)
      ├─ call AppFoldersService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:50) [verified]
      │      if (!isSaved)
      │      {
      │      return BadRequest(new FileUploadResultModel
      ├─ call AdminsEmailsService  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:74) [verified]
      │      Url = uploadedFileUrl,
      │      FileName = file?.FileName ?? fileName
      │      });
      │  (truncated — more edges beyond depth/fan-out)
      └─ call EfDbLogger  
(C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default\src\DntSit
e.Web\Features\UserFiles\Controllers\UploadFileController.cs:85) [verified]
             });
             }
             }
         (truncated — more edges beyond depth/fan-out)

analyzed 1336 files · 1452 nodes · 1099 edges · 70 entries · depth 5 · ~3176 
tokens · 72.8s stage2 ×2.0 stage3 ×1.3
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │     DntSite.slnx     │
│   Time   │       72928ms        │
│  Tokens  │ ~3176 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.100 │
╰──────────┴──────────────────────╯
