Overview map (no focus).
Analyzing project...

{
  "schemaVersion": "1.1",
  "generatedAt": "2026-06-13T11:16:32.3387565Z",
  "architecture": {
    "style": "ControllerBased",
    "confidence": 0.8
  },
  "signals": [
    {
      "key": "controllers",
      "confidence": 0.9,
      "detected": true
    },
    {
      "key": "efcore",
      "confidence": 1,
      "detected": true
    },
    {
      "key": "minimal-apis",
      "confidence": 0.8,
      "detected": true
    }
  ],
  "projects": {
    "count": 3,
    "names": [
      "DntSite.Tests",
      "DntSite.Web",
      "DntSite.Web.Common.BlazorSsr"
    ]
  },
  "typesSummary": {
    "found": 1289,
    "inOutput": 40,
    "prunedPercent": 96.9
  },
  "detections": [
    {
      "type": "BackgroundWorkerDetection",
      "serviceType": "DNTScheduler",
      "implementationType": "DotNetVersionCheckJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 20,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "CheckAdminsLastVisitJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 23,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "FreeSpaceCheckJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 30,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "WebReadersListJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 37,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "UpdatePublicNewsHttpStatusCodeJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 40,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "UpdateDeletedNewsHttpStatusCodeJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 43,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "SendActivationEmailsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 46,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "DisableInactiveUsersJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 49,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "NewPersianYearEmailsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 52,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "ManageBacklogsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 54,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "BackupDatabaseJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 61,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "BackupDataFolderJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 67,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "HumansTxtJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 70,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "DraftsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 73,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "FullTextSearchWriterJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 80,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "ThumbnailsServiceJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 87,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "ExportToSeparatePdfFilesJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 94,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "ExportToMergedPdfFilesJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 101,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "DeleteOrphansJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 104,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "DailyNewsletterJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 107,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "DailyBirthDatesEmailJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 110,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "EmptyPMsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 113,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "AIDailyNewsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 116,
      "confidence": 1
    },
    {
      "serviceType": "DNTScheduler",
      "implementationType": "AIDailyNewsBacklogsJob",
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 123,
      "confidence": 1
    },
    {
      "type": "DiRegistrationDetection",
      "serviceType": "ILoggerProvider",
      "implementationType": "EfDbLoggerProvider",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\DbLogger\\Ser
vices\\EfDbLoggerFactoryExtensions.cs",
      "lineNumber": 9,
      "confidence": 1
    },
    {
      "serviceType": "AddOptions",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddOptions"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 15,
      "confidence": 0.7
    },
    {
      "serviceType": "AddLogging",
      "implementationType": "cfg =\u003E cfg.AddSimpleConsole(opts =\u003E\r\n
{\r\n                opts.TimestampFormat = 
\u0022yyyy-MM-ddTHH:mm:ss.fffffffZ-\u0022;\r\n                opts.ColorBehavior
= LoggerColorBehavior.Enabled;\r\n            })\r\n            .AddDebug()",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddLogging"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 17,
      "confidence": 0.7
    },
    {
      "serviceType": "IHttpContextAccessor",
      "implementationType": "HttpContextAccessor",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 24,
      "confidence": 1
    },
    {
      "serviceType": "ILoggerFactory",
      "implementationType": "LoggerFactory",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 25,
      "confidence": 1
    },
    {
      "serviceType": "IAppFoldersService",
      "implementationType": "AppFoldersService",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 26,
      "confidence": 1
    },
    {
      "serviceType": "IWebHostEnvironment",
      "implementationType": "TestHostingEnvironment",
      "lifetime": "Scoped",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 27,
      "confidence": 1
    },
    {
      "serviceType": "AddEfCoreInterceptors",
      "implementationType": "new TestHostingEnvironment()",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddEfCoreInterceptors"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 28,
      "confidence": 0.7
    },
    {
      "serviceType": "_ =\u003E configuration",
      "implementationType": "_ =\u003E configuration",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 3,
      "factorySummary": "[factory]",
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteContextFactory.cs",
      "lineNumber": 34,
      "confidence": 1
    },
    {
      "serviceType": "AddDbContextPool",
      "implementationType": "(serviceProvider, optionsBuilder)\r\n            
=\u003E optionsBuilder.UseConfiguredSqLite(serviceProvider)",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddDbContextPool"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteServiceCollectionExtensions.cs",
      "lineNumber": 11,
      "confidence": 0.7
    },
    {
      "serviceType": "serviceProvider =\u003E\r\n        {\r\n            var 
context = 
serviceProvider.GetRequiredService\u003CApplicationDbContext\u003E();\r\n
SetCascadeOnSaveChanges(context);\r\n\r\n            return context;\r\n        
}",
      "implementationType": "serviceProvider =\u003E\r\n        {\r\n
var context = 
serviceProvider.GetRequiredService\u003CApplicationDbContext\u003E();\r\n
SetCascadeOnSaveChanges(context);\r\n\r\n            return context;\r\n        
}",
      "lifetime": "Scoped",
      "extensionsUsed": [],
      "shape": 3,
      "factorySummary": "[factory]",
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\SQLiteServiceCollectionExtensions.cs",
      "lineNumber": 14,
      "confidence": 1
    },
    {
      "serviceType": "AddCascadingAuthenticationState",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCascadingAuthenticationState"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\AuthenticationConfig.cs",
      "lineNumber": 20,
      "confidence": 0.7
    },
    {
      "serviceType": "AuthenticationStateProvider",
      "implementationType": "IdentityRevalidatingAuthenticationStateProvider",
      "lifetime": "Scoped",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\AuthenticationConfig.cs",
      "lineNumber": 21,
      "confidence": 1
    },
    {
      "serviceType": "AddAuthorization",
      "implementationType": "options =\u003E\r\n        {\r\n            
options.AddPolicy(CustomRoles.Admin, policy =\u003E 
policy.RequireRole(CustomRoles.Admin));\r\n            
options.AddPolicy(CustomRoles.User, policy =\u003E 
policy.RequireRole(CustomRoles.User));\r\n        }",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddAuthorization"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\AuthenticationConfig.cs",
      "lineNumber": 23,
      "confidence": 0.7
    },
    {
      "serviceType": "AddAuthentication",
      "implementationType": "options =\u003E\r\n            {\r\n
options.DefaultChallengeScheme = 
CookieAuthenticationDefaults.AuthenticationScheme;\r\n                
options.DefaultSignInScheme = 
CookieAuthenticationDefaults.AuthenticationScheme;\r\n                
options.DefaultAuthenticateScheme = 
CookieAuthenticationDefaults.AuthenticationScheme;\r\n            }",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddAuthentication"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\AuthenticationConfig.cs",
      "lineNumber": 29,
      "confidence": 0.7
    },
    {
      "serviceType": "AddCookie",
      "implementationType": "options =\u003E\r\n            {\r\n
options.LoginPath = UserProfilesRoutingConstants.Login;\r\n                
options.LogoutPath = UserProfilesRoutingConstants.Logout;\r\n                
options.AccessDeniedPath = \u0022/error/403\u0022;\r\n                
options.Cookie.Name = \u0022.dnt.site.cookie\u0022;\r\n                
options.Cookie.HttpOnly = true;\r\n\r\n                
options.Cookie.SecurePolicy = environment.IsDevelopment()\r\n
? CookieSecurePolicy.SameAsRequest\r\n                    : 
CookieSecurePolicy.Always;\r\n\r\n                // A cookie with 
\u0022SameSite=Lax\u0022 will be sent with a same-site request,\r\n
// or a cross-site top-level navigation with a \u0022safe\u0022 HTTP method.\r\n
options.Cookie.SameSite = SameSiteMode.Lax;\r\n\r\n                
options.SlidingExpiration = true;\r\n\r\n                options.ExpireTimeSpan 
=\r\n                    
TimeSpan.FromDays(siteSettings.DataProtectionOptions.LoginCookieExpirationDays);
\r\n\r\n                options.Events = new CookieAuthenticationEvents\r\n
{\r\n                    OnValidatePrincipal = context =\u003E\r\n
{\r\n                        var cookieValidatorService =\r\n
context.HttpContext.RequestServices.GetRequiredService\u003CICookieValidatorServ
ice\u003E();\r\n\r\n                        return 
cookieValidatorService.ValidateAsync(context);\r\n                    }\r\n
};\r\n\r\n                options.CookieManager = new ChunkingCookieManager\r\n
{\r\n                    // Slightly smaller chunk size\r\n                    
ChunkSize = 3000,\r\n                    ThrowForPartialCookies = 
environment.IsDevelopment()\r\n                };\r\n            }",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCookie"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\AuthenticationConfig.cs",
      "lineNumber": 29,
      "confidence": 0.7
    },
    {
      "serviceType": "IXmlRepository",
      "implementationType": "DataProtectionKeyService",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DataProtectionConfig.cs",
      "lineNumber": 18,
      "confidence": 1
    },
    {
      "serviceType": "serviceProvider =\u003E\r\n        {\r\n            return
new ConfigureOptions\u003CKeyManagementOptions\u003E(options =\u003E\r\n
{\r\n                
serviceProvider.RunScopedService\u003CIXmlRepository\u003E(xmlRepository\r\n
=\u003E options.XmlRepository = xmlRepository);\r\n            });\r\n        
}",
      "implementationType": "serviceProvider =\u003E\r\n        {\r\n
return new ConfigureOptions\u003CKeyManagementOptions\u003E(options =\u003E\r\n
{\r\n                
serviceProvider.RunScopedService\u003CIXmlRepository\u003E(xmlRepository\r\n
=\u003E options.XmlRepository = xmlRepository);\r\n            });\r\n        
}",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 3,
      "factorySummary": "[factory: new 
ConfigureOptions\u003CKeyManagementOptions\u003E(options =\u003E\r\n            
{\r\n                
serviceProvider.RunScopedService\u003CIXmlRepository\u003E(xmlRepository\r\n
=\u003E options.XmlRepository = xmlRepository);\r\n            })]",
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DataProtectionConfig.cs",
      "lineNumber": 20,
      "confidence": 1
    },
    {
      "serviceType": "AddDataProtection",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddDataProtection"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DataProtectionConfig.cs",
      "lineNumber": 29,
      "confidence": 0.7
    },
    {
      "serviceType": "AddEfCoreInterceptors",
      "implementationType": "environment",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddEfCoreInterceptors"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DbContextConfig.cs",
      "lineNumber": 23,
      "confidence": 0.7
    },
    {
      "serviceType": "AddConfiguredSqLiteDbContext",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddConfiguredSqLiteDbContext"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DbContextConfig.cs",
      "lineNumber": 24,
      "confidence": 0.7
    },
    {
      "serviceType": "EfExceptionsInterceptor",
      "implementationType": "EfExceptionsInterceptor",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 1,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DbContextConfig.cs",
      "lineNumber": 31,
      "confidence": 1
    },
    {
      "serviceType": "AuditableEntitiesInterceptor",
      "implementationType": "AuditableEntitiesInterceptor",
      "lifetime": "Singleton",
      "extensionsUsed": [],
      "shape": 1,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DbContextConfig.cs",
      "lineNumber": 32,
      "confidence": 1
    },
    {
      "serviceType": "AddEfSecondLevelCacheInterceptor",
      "implementationType": "environment",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddEfSecondLevelCacheInterceptor"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DbContextConfig.cs",
      "lineNumber": 33,
      "confidence": 0.7
    },
    {
      "serviceType": "AddEFSecondLevelCache",
      "implementationType": "options =\u003E 
options.UseMemoryCacheProvider()\r\n            
.ConfigureLogging(environment.IsDevelopment(), args =\u003E\r\n            {\r\n
switch (args.EventId)\r\n                {\r\n                    case 
CacheableLogEventId.CacheHit:\r\n                    case 
CacheableLogEventId.QueryResultCached:\r\n                        break;\r\n
case CacheableLogEventId.QueryResultInvalidated:\r\n                        
args.ServiceProvider.GetRequiredService\u003CILoggerFactory\u003E()\r\n
.CreateLogger(nameof(EFCoreSecondLevelCacheInterceptor))\r\n
.LogWarning(message: \u0022{EventId} -\u003E {Message} -\u003E 
{CommandText}\u0022, args.EventId, args.Message,\r\n
args.CommandText);\r\n\r\n                        break;\r\n                    
case CacheableLogEventId.CachingSkipped:\r\n                    case 
CacheableLogEventId.InvalidationSkipped:\r\n                    case 
CacheableLogEventId.CachingSystemStarted:\r\n                    case 
CacheableLogEventId.CachingError:\r\n                    case 
CacheableLogEventId.QueryResultSuppressed:\r\n                    case 
CacheableLogEventId.CacheDependenciesCalculated:\r\n                    case 
CacheableLogEventId.CachePolicyCalculated:\r\n                        break;\r\n
}\r\n            })\r\n            .UseCacheKeyPrefix(prefix: 
\u0022EF_\u0022)\r\n            
.CacheAllQueriesExceptContainingTypes(CacheExpirationMode.Absolute, 
TimeSpan.FromMinutes(value: 5),\r\n                typeof(AppLogItem), 
typeof(SiteUrl), typeof(SiteReferrer))\r\n            
.SkipCachingCommands(commandText\r\n                =\u003E 
commandText.Contains(value: \u0022NEWID()\u0022, 
StringComparison.OrdinalIgnoreCase))\r\n            
.SkipCacheInvalidationCommands(ShouldIgnoreForAllCommands)\r\n            
.UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(value: 1))",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddEFSecondLevelCache"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\DbContextConfig.cs",
      "lineNumber": 38,
      "confidence": 0.7
    },
    {
      "serviceType": "AddProblemDetails",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddProblemDetails"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\MvcControllersConfig.cs",
      "lineNumber": 9,
      "confidence": 0.7
    },
    {
      "serviceType": "AddRequestTimeouts",
      "implementationType": "options =\u003E\r\n            {\r\n
options.DefaultPolicy = new RequestTimeoutPolicy\r\n                {\r\n
Timeout = TimeSpan.FromMinutes(value: 30),\r\n                    
TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable\r\n                
};\r\n            }",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddRequestTimeouts"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\MvcControllersConfig.cs",
      "lineNumber": 9,
      "confidence": 0.7
    },
    {
      "serviceType": "AddLargeFilesUploadSupport",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddLargeFilesUploadSupport"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\MvcControllersConfig.cs",
      "lineNumber": 9,
      "confidence": 0.7
    },
    {
      "serviceType": "AddOutputCache",
      "implementationType": "options =\u003E { 
options.AddPolicy(AlwaysCachePolicy.Name, AlwaysCachePolicy.Instance); }",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddOutputCache"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\MvcControllersConfig.cs",
      "lineNumber": 9,
      "confidence": 0.7
    },
    {
      "serviceType": "AddControllers",
      "implementationType": "options =\u003E\r\n            {\r\n
options.Filters.Add\u003CApplyCorrectYeKeFilterAttribute\u003E();\r\n
options.Filters.Add\u003CCheckSiteIsActiveActionFilter\u003E();\r\n            
}",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddControllers"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\MvcControllersConfig.cs",
      "lineNumber": 9,
      "confidence": 0.7
    },
    {
      "serviceType": "AddCustomJsonOptionsForWebApps",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCustomJsonOptionsForWebApps"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\MvcControllersConfig.cs",
      "lineNumber": 9,
      "confidence": 0.7
    },
    {
      "serviceType": "AddDNTScheduler",
      "implementationType": "options =\u003E\r\n        {\r\n            
options.AddScheduledTask\u003CDotNetVersionCheckJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Hour: 5, Minute: 30, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CCheckAdminsLastVisitJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Minute % 5 == 0 \u0026\u0026 now.Second == 1;\r\n            
});\r\n\r\n            
options.AddScheduledTask\u003CFreeSpaceCheckJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Hour % 6 == 0 \u0026\u0026 now is { Minute: 10, Second: 1 };\r\n
});\r\n\r\n            
options.AddScheduledTask\u003CWebReadersListJob\u003E(utcNow\r\n                
=\u003E GetNowIranTime(utcNow) is { Hour: 3, Minute: 30, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CUpdatePublicNewsHttpStatusCodeJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Day: 1, Hour: 1, Minute: 1, Second: 1 
});\r\n\r\n            
options.AddScheduledTask\u003CUpdateDeletedNewsHttpStatusCodeJob\u003E(utcNow\r\
n                =\u003E GetNowIranTime(utcNow) is { Day: 2, Hour: 1, Minute: 1,
Second: 1 });\r\n\r\n            
options.AddScheduledTask\u003CSendActivationEmailsJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Hour: 11, Minute: 1, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CDisableInactiveUsersJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Hour: 6, Minute: 1, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CNewPersianYearEmailsJob\u003E(utcNow =\u003E 
GetNowIranTime(utcNow).IsStartOfNewYear());\r\n\r\n            
options.AddScheduledTask\u003CManageBacklogsJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Hour % 2 == 0 \u0026\u0026 now is { Minute: 10, Second: 1 };\r\n
});\r\n\r\n            
options.AddScheduledTask\u003CBackupDatabaseJob\u003E(utcNow\r\n                
=\u003E GetNowIranTime(utcNow) is\r\n                {\r\n                    
DayOfWeek: DayOfWeek.Friday or DayOfWeek.Monday, Hour: 4, Minute: 1, Second: 
1\r\n                });\r\n\r\n            
options.AddScheduledTask\u003CBackupDataFolderJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { DayOfWeek: DayOfWeek.Saturday, Hour: 4, 
Minute: 1, Second: 1 });\r\n\r\n            
options.AddScheduledTask\u003CHumansTxtJob\u003E(utcNow\r\n                
=\u003E GetNowIranTime(utcNow) is { Hour: 3, Minute: 1, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CDraftsJob\u003E(utcNow =\u003E\r\n            
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Minute % 5 == 0 \u0026\u0026 now.Second == 1;\r\n            
});\r\n\r\n            
options.AddScheduledTask\u003CFullTextSearchWriterJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Minute % 5 == 0 \u0026\u0026 now.Second == 1;\r\n            
});\r\n\r\n            
options.AddScheduledTask\u003CThumbnailsServiceJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Minute % 10 == 0 \u0026\u0026 now.Second == 1;\r\n            
});\r\n\r\n            
options.AddScheduledTask\u003CExportToSeparatePdfFilesJob\u003E(utcNow 
=\u003E\r\n            {\r\n                var now = 
GetNowIranTime(utcNow);\r\n\r\n                return now.Minute % 20 == 0 
\u0026\u0026 now.Second == 1;\r\n            });\r\n\r\n            
options.AddScheduledTask\u003CExportToMergedPdfFilesJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Hour: 5, Minute: 30, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CDeleteOrphansJob\u003E(utcNow\r\n                
=\u003E GetNowIranTime(utcNow) is { Hour: 3, Minute: 7, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CDailyNewsletterJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Hour: 0, Minute: 1, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CDailyBirthDatesEmailJob\u003E(utcNow\r\n
=\u003E GetNowIranTime(utcNow) is { Hour: 8, Minute: 59, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CEmptyPMsJob\u003E(utcNow\r\n                
=\u003E GetNowIranTime(utcNow) is { Hour: 3, Minute: 1, Second: 1 });\r\n\r\n
options.AddScheduledTask\u003CAIDailyNewsJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Minute % 15 == 0 \u0026\u0026 now.Second == 1;\r\n            
});\r\n\r\n            
options.AddScheduledTask\u003CAIDailyNewsBacklogsJob\u003E(utcNow =\u003E\r\n
{\r\n                var now = GetNowIranTime(utcNow);\r\n\r\n                
return now.Hour % 2 == 0 \u0026\u0026 now is { Minute: 5, Second: 1 };\r\n
});\r\n        }",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddDNTScheduler"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\SchedulersConfig.cs",
      "lineNumber": 18,
      "confidence": 0.7
    },
    {
      "serviceType": "AddOptions",
      "implementationType": "configuration",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddOptions"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 18,
      "confidence": 0.7
    },
    {
      "serviceType": "AddForwardedHeadersOptions",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddForwardedHeadersOptions"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 19,
      "confidence": 0.7
    },
    {
      "serviceType": "AddHttpContextAccessor",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddHttpContextAccessor"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 21,
      "confidence": 0.7
    },
    {
      "serviceType": "AddIPrincipal",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddIPrincipal"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 22,
      "confidence": 0.7
    },
    {
      "serviceType": "AutoInjectAllServices",
      "implementationType": "*",
      "lifetime": "Bulk",
      "extensionsUsed": [
        "AutoInjectAllServices"
      ],
      "shape": 3,
      "factorySummary": "[bulk auto-registration]",
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 23,
      "confidence": 0.6
    },
    {
      "serviceType": "AddConfiguredDbContext",
      "implementationType": "siteSettings",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddConfiguredDbContext"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 26,
      "confidence": 0.7
    },
    {
      "serviceType": "AddCustomizedDataProtection",
      "implementationType": "siteSettings",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCustomizedDataProtection"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 27,
      "confidence": 0.7
    },
    {
      "serviceType": "AddDNTCommonWeb",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddDNTCommonWeb"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 28,
      "confidence": 0.7
    },
    {
      "serviceType": "AddSchedulers",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddSchedulers"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 29,
      "confidence": 0.7
    },
    {
      "serviceType": "AddCustomizedControllers",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCustomizedControllers"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 31,
      "confidence": 0.7
    },
    {
      "serviceType": "AddCustomizedAuthentication",
      "implementationType": "siteSettings",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCustomizedAuthentication"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 32,
      "confidence": 0.7
    },
    {
      "serviceType": "AddOptions",
      "implementationType": "StartupSettingsModel",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddOptions"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\ServicesConfi
gs\\ServicesRegistry.cs",
      "lineNumber": 37,
      "confidence": 0.7
    },
    {
      "serviceType": "AddRazorComponents",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddRazorComponents"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 22,
      "confidence": 0.7
    },
    {
      "serviceType": "AddInteractiveServerComponents",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddInteractiveServerComponents"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 22,
      "confidence": 0.7
    },
    {
      "serviceType": "AddControllers",
      "implementationType": "?",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddControllers"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 23,
      "confidence": 0.7
    },
    {
      "serviceType": "AddCustomizedServices",
      "implementationType": "host",
      "lifetime": "Extension",
      "extensionsUsed": [
        "AddCustomizedServices"
      ],
      "shape": 0,
      "extractorName": "DiRegistrationExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 24,
      "confidence": 0.7
    },
    {
      "type": "EfEntityDetection",
      "entityType": "V2024_04_19_1424",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240419105626_V2024_04_19_1424.cs",
      "lineNumber": 9,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_05_18_1347",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240518102324_V2024_05_18_1347.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_06_19_2139",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240619181237_V2024_06_19_2139.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_06_25_2320",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240625195346_V2024_06_25_2320.cs",
      "lineNumber": 9,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_06_27_2036",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240627170936_V2024_06_27_2036.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_06_28_1257",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240628093237_V2024_06_28_1257.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_06_30_2030",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240630170350_V2024_06_30_2030.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_07_17_1405",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240717104004_V2024_07_17_1405.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_07_19_2106",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240719174006_V2024_07_19_2106.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_07_19_2211",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240719184542_V2024_07_19_2211.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_07_19_2234",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240719190736_V2024_07_19_2234.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_08_12_1323",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240812095940_V2024_08_12_1323.cs",
      "lineNumber": 9,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_09_28_1204",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240928084100_V2024_09_28_1204.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_09_28_1327",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20240928100200_V2024_09_28_1327.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_10_05_1343",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20241005101813_V2024_10_05_1343.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_10_05_1417",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20241005105231_V2024_10_05_1417.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_10_18_1302",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20241018093847_V2024_10_18_1302.cs",
      "lineNumber": 9,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_10_19_2133",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20241019180846_V2024_10_19_2133.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_10_30_1357",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20241030103214_V2024_10_30_1357.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2024_11_17_1942",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20241117161626_V2024_11_17_1942.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2025_12_31_2127",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20251231180814_V2025_12_31_2127.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_01_11_1104",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260111074206_V2026_01_11_1104.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_01_11_1910",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260111154840_V2026_01_11_1910.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_01_28_2352",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260128203053_V2026_01_28_2352.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_02_03_0009",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260202204731_V2026_02_03_0009.cs",
      "lineNumber": 9,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_03_16_1155",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260316083409_V2026_03_16_1155.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_04_02_1410",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260402104927_V2026_04_02_1410.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_04_21_1348",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260421102757_V2026_04_21_1348.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_05_06_1053",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260506073235_V2026_05_06_1053.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_05_07_1000",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260507063821_V2026_05_07_1000.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "V2026_06_03_1205",
      "dbContextType": "Migrations",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
Migrations\\20260603084131_V2026_06_03_1205.cs",
      "lineNumber": 8,
      "confidence": 0.9
    },
    {
      "entityType": "BaseEntity",
      "dbContextType": "ApplicationDbContext",
      "isAggregate": false,
      "keyProperties": [
        "Id"
      ],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\ApplicationDbContext.cs",
      "lineNumber": 75,
      "confidence": 0.7
    },
    {
      "entityType": "\u003COnModelCreating\u003E",
      "dbContextType": "ApplicationDbContext",
      "isAggregate": false,
      "keyProperties": [],
      "extractorName": "EfCoreExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Persistence\\
UnitOfWork\\ApplicationDbContext.cs",
      "lineNumber": 75,
      "confidence": 0.8
    },
    {
      "type": "EndpointDetection",
      "httpMethod": "POST",
      "routeTemplate": "/api/JavaScriptErrorsReport",
      "handlerType": "JavaScriptErrorsReportController",
      "handlerMethod": "Log",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Common\\Contr
ollers\\JavaScriptErrorsReportController.cs",
      "lineNumber": 16,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Exports/{type}/{name}.pdf",
      "handlerType": "ExportsController",
      "handlerMethod": "Get",
      "authAttributes": [],
      "parameterTypes": [
        "string?",
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Exports\\Cont
rollers\\ExportsController.cs",
      "lineNumber": 13,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed",
      "handlerType": "FeedController",
      "handlerMethod": "Index",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 15,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Index",
      "handlerType": "FeedController",
      "handlerMethod": "Index",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 15,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/Feed/Posts",
      "handlerType": "FeedController",
      "handlerMethod": "Posts",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 19,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/feeds/posts/{name?}",
      "handlerType": "FeedController",
      "handlerMethod": "UserPosts",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 22,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Comments",
      "handlerType": "FeedController",
      "handlerMethod": "Comments",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 33,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/feeds/comments/{name?}",
      "handlerType": "FeedController",
      "handlerMethod": "UserComments",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 36,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/News",
      "handlerType": "FeedController",
      "handlerMethod": "News",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 47,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Tag/{id?}",
      "handlerType": "FeedController",
      "handlerMethod": "Tag",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 50,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Author/{id?}",
      "handlerType": "FeedController",
      "handlerMethod": "Author",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 61,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/NewsComments",
      "handlerType": "FeedController",
      "handlerMethod": "NewsComments",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 73,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/NewsAuthor/{id?}",
      "handlerType": "FeedController",
      "handlerMethod": "NewsAuthor",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 77,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/LatestChanges",
      "handlerType": "FeedController",
      "handlerMethod": "LatestChanges",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 89,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/blog/rss.xml",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/blog/feed",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/feed/atom",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/feed/rss",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/feed.xml",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/rss2.xml",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/rss",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/atom.xml",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/rss.xml",
      "handlerType": "FeedController",
      "handlerMethod": "SiteFeed",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 92,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/llms.txt",
      "handlerType": "FeedController",
      "handlerMethod": "LlmsTxt",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 103,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/llms-full.txt",
      "handlerType": "FeedController",
      "handlerMethod": "LlmsFull",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 106,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/GetLatestChangesAsync",
      "handlerType": "FeedController",
      "handlerMethod": "GetLatestChangesAsync",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 110,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Courses",
      "handlerType": "FeedController",
      "handlerMethod": "Courses",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 113,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/CoursesTopics",
      "handlerType": "FeedController",
      "handlerMethod": "CoursesTopics",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 116,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/CoursesComments",
      "handlerType": "FeedController",
      "handlerMethod": "CoursesComments",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 120,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Surveys",
      "handlerType": "FeedController",
      "handlerMethod": "Surveys",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 124,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/Announcements",
      "handlerType": "FeedController",
      "handlerMethod": "Announcements",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 127,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Feed/ShowBriefDescriptionAsync",
      "handlerType": "FeedController",
      "handlerMethod": "ShowBriefDescriptionAsync",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\FeedController.cs",
      "lineNumber": 131,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/Index",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "Index",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 13,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/Get",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "Get",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 15,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectsNews",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectsNews",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 17,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectsFiles",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectsFiles",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 20,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectsIssues",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectsIssues",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 23,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectsIssuesReplies",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectsIssuesReplies",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 26,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectsFaqs",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectsFaqs",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 30,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectFaqs/{id:int?}",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectFaqs",
      "authAttributes": [],
      "parameterTypes": [
        "int?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 33,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectFiles/{id:int?}",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectFiles",
      "authAttributes": [],
      "parameterTypes": [
        "int?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 46,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectIssues/{id:int?}",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectIssues",
      "authAttributes": [],
      "parameterTypes": [
        "int?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 59,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/ProjectsFeeds/ProjectIssuesReplies/{id:int?}",
      "handlerType": "ProjectsFeedsController",
      "handlerMethod": "ProjectIssuesReplies",
      "authAttributes": [],
      "parameterTypes": [
        "int?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\RssFeeds\\Con
trollers\\ProjectsFeedsController.cs",
      "lineNumber": 72,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/api/Fts",
      "handlerType": "FtsController",
      "handlerMethod": "Search",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Searches\\Con
trollers\\FtsController.cs",
      "lineNumber": 19,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/Fts",
      "handlerType": "FtsController",
      "handlerMethod": "Log",
      "authAttributes": [],
      "parameterTypes": [
        "string?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Searches\\Con
trollers\\FtsController.cs",
      "lineNumber": 48,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/OpenSearch",
      "handlerType": "OpenSearchController",
      "handlerMethod": "RenderOpenSearch",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Seo\\Controll
ers\\OpenSearchController.cs",
      "lineNumber": 13,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Sitemap/Get",
      "handlerType": "SitemapController",
      "handlerMethod": "Get",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Seo\\Controll
ers\\SitemapController.cs",
      "lineNumber": 12,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/sitemap",
      "handlerType": "SitemapController",
      "handlerMethod": "Get",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Seo\\Controll
ers\\SitemapController.cs",
      "lineNumber": 12,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/sitemap.xml",
      "handlerType": "SitemapController",
      "handlerMethod": "Get",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Seo\\Controll
ers\\SitemapController.cs",
      "lineNumber": 12,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/Welcome",
      "handlerType": "WelcomeController",
      "handlerMethod": "Log",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\Stats\\Contro
llers\\WelcomeController.cs",
      "lineNumber": 12,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/Avatar",
      "handlerType": "FileController",
      "handlerMethod": "Avatar",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 18,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/Image",
      "handlerType": "FileController",
      "handlerMethod": "Image",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 22,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/MessagesImages",
      "handlerType": "FileController",
      "handlerMethod": "MessagesImages",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 27,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/UserFile",
      "handlerType": "FileController",
      "handlerMethod": "UserFile",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 30,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/ProjectFile",
      "handlerType": "FileController",
      "handlerMethod": "ProjectFile",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 34,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/Messages",
      "handlerType": "FileController",
      "handlerMethod": "Messages",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 39,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/NewsThumb",
      "handlerType": "FileController",
      "handlerMethod": "NewsThumb",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 42,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/CommonFiles",
      "handlerType": "FileController",
      "handlerMethod": "CommonFiles",
      "authAttributes": [
        "Authorize"
      ],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 47,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/CourseFiles",
      "handlerType": "FileController",
      "handlerMethod": "CourseFiles",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 51,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/CourseImages",
      "handlerType": "FileController",
      "handlerMethod": "CourseImages",
      "authAttributes": [],
      "parameterTypes": [
        "string"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 55,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/users/EmailToImage/{id:int?}",
      "handlerType": "FileController",
      "handlerMethod": "EmailToImage",
      "authAttributes": [],
      "parameterTypes": [
        "int?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 60,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/File/EmailToImage",
      "handlerType": "FileController",
      "handlerMethod": "EmailToImage",
      "authAttributes": [],
      "parameterTypes": [
        "int?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\FileController.cs",
      "lineNumber": 60,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "ImageUpload",
      "authAttributes": [],
      "parameterTypes": [
        "ImageFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 18,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "MessagesImagesUpload",
      "authAttributes": [],
      "parameterTypes": [
        "ImageFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 22,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "CourseImagesUpload",
      "authAttributes": [],
      "parameterTypes": [
        "ImageFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 26,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "CourseFileUpload",
      "authAttributes": [],
      "parameterTypes": [
        "NormalFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 30,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "FileUpload",
      "authAttributes": [],
      "parameterTypes": [
        "NormalFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 34,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "CommonFilesUpload",
      "authAttributes": [],
      "parameterTypes": [
        "NormalFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 38,
      "confidence": 1
    },
    {
      "httpMethod": "POST",
      "routeTemplate": "/api/UploadFile",
      "handlerType": "UploadFileController",
      "handlerMethod": "MessagesFilesUpload",
      "authAttributes": [],
      "parameterTypes": [
        "NormalFileDataModel?"
      ],
      "extractorName": "ControllerActionExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserFiles\\Co
ntrollers\\UploadFileController.cs",
      "lineNumber": 42,
      "confidence": 1
    },
    {
      "httpMethod": "GET",
      "routeTemplate": "/.well-known/change-password",
      "handlerType": "context =\u003E\r\n        {\r\n            // 
\u0060/.well-known/change-password\u0060 address will be called by the 
\u0060Change password\u0060 button of the Chrome.\r\n            // Now our 
Web-API app redirects the user to the \u0060/change-password\u0060 address of 
the Blazor App.\r\n            
context.Response.Redirect(\u0022/change-password\u0022, true);\r\n\r\n
return Task.CompletedTask;\r\n        }",
      "handlerMethod": "\u003Clambda\u003E",
      "authAttributes": [],
      "parameterTypes": [],
      "extractorName": "EndpointExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Features\\UserProfiles\
\Endpoints\\ChangePasswordEndpoint.cs",
      "lineNumber": 9,
      "confidence": 1
    },
    {
      "type": "MiddlewareDetection",
      "middlewareType": "UseForwardedHeaders",
      "pipelineOrder": 1,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 50,
      "confidence": 1
    },
    {
      "middlewareType": "UseExceptionHandler",
      "pipelineOrder": 2,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 53,
      "confidence": 1
    },
    {
      "middlewareType": "UseStatusCodePagesWithReExecute",
      "pipelineOrder": 3,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 54,
      "confidence": 1
    },
    {
      "middlewareType": "UseAntiDos",
      "pipelineOrder": 4,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 56,
      "confidence": 1
    },
    {
      "middlewareType": "UseCsp",
      "pipelineOrder": 5,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 58,
      "confidence": 1
    },
    {
      "middlewareType": "UseHttpsRedirection",
      "pipelineOrder": 6,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 62,
      "confidence": 1
    },
    {
      "middlewareType": "UseAuthentication",
      "pipelineOrder": 7,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 67,
      "confidence": 1
    },
    {
      "middlewareType": "UseAuthorization",
      "pipelineOrder": 8,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 68,
      "confidence": 1
    },
    {
      "middlewareType": "UseAntiforgery",
      "pipelineOrder": 9,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 70,
      "confidence": 1
    },
    {
      "middlewareType": "UseOutputCache",
      "pipelineOrder": 10,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 71,
      "confidence": 1
    },
    {
      "middlewareType": "UseRequestTimeouts",
      "pipelineOrder": 11,
      "kind": 0,
      "extractorName": "ProgramCsFlowExtractor",
      "sourceFile": 
"C:\\Code\\DevContext2\\_eval-dntsite\\src\\DntSite.Web\\Program.cs",
      "lineNumber": 79,
      "confidence": 1
    }
  ],
  "pruningSummary": "PatternRelevancePruner: excluded test-project type 
\u0027DntSite.Tests.RaviAiParserTests\u0027",
  "maxTokens": 3481,
  "runReport": {
    "stages": [
      {
        "stage": "DiscoveryAndCacheWarmup",
        "elapsed": "00:00:00.2033902",
        "ordinal": 1
      },
      {
        "stage": "GenericExtraction",
        "elapsed": "00:00:08.7508231",
        "ordinal": 2
      },
      {
        "stage": "SignalSealing",
        "elapsed": "00:00:00.0016577",
        "ordinal": 3
      },
      {
        "stage": "SpecificExtraction",
        "elapsed": "00:00:01.8518888",
        "ordinal": 4
      },
      {
        "stage": "Scoring",
        "elapsed": "00:00:00.0244726",
        "ordinal": 5
      },
      {
        "stage": "Compression",
        "elapsed": "00:00:00.0420802",
        "ordinal": 6
      }
    ],
    "extractors": [
      {
        "name": "SyntaxStructureExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "GenericExtraction",
        "elapsed": "00:00:08.7429116",
        "typesAdded": 1289,
        "detectionsAdded": 83,
        "skipped": false
      },
      {
        "name": "DiRegistrationExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "GenericExtraction",
        "elapsed": "00:00:08.7303925",
        "typesAdded": 1289,
        "detectionsAdded": 83,
        "skipped": false
      },
      {
        "name": "EndpointExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:01.8497960",
        "typesAdded": 0,
        "detectionsAdded": 103,
        "skipped": false
      },
      {
        "name": "EfCoreExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:01.0305550",
        "typesAdded": 0,
        "detectionsAdded": 102,
        "skipped": false
      },
      {
        "name": "InMemoryEventBusExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00.6487093",
        "typesAdded": 0,
        "detectionsAdded": 73,
        "skipped": false
      },
      {
        "name": "ControllerActionExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00.6022997",
        "typesAdded": 0,
        "detectionsAdded": 71,
        "skipped": false
      },
      {
        "name": "ProgramCsFlowExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "GenericExtraction",
        "elapsed": "00:00:00.2504075",
        "typesAdded": 0,
        "detectionsAdded": 39,
        "skipped": false
      },
      {
        "name": "FileTreeExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "DiscoveryAndCacheWarmup",
        "elapsed": "00:00:00.1079292",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": false
      },
      {
        "name": "ProjectStructure",
        "tier": "Fast",
        "category": "",
        "stage": "DiscoveryAndCacheWarmup",
        "elapsed": "00:00:00.0353059",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": false
      },
      {
        "name": "SolutionDiscovery",
        "tier": "Fast",
        "category": "",
        "stage": "DiscoveryAndCacheWarmup",
        "elapsed": "00:00:00.0306816",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": false
      },
      {
        "name": "DependencyExtractor",
        "tier": "Fast",
        "category": "",
        "stage": "GenericExtraction",
        "elapsed": "00:00:00.0267391",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": false
      },
      {
        "name": "LayerClassifier",
        "tier": "Fast",
        "category": "",
        "stage": "GenericExtraction",
        "elapsed": "00:00:00.0267187",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": false
      },
      {
        "name": "AntiPatternDetector",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "gated by ShouldRun"
      },
      {
        "name": "AspireExtractor",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "signal gate: needs aspire"
      },
      {
        "name": "CallGraphExtractor",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "gated by ShouldRun"
      },
      {
        "name": "EventBusExtractor",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "signal gate: needs masstransit or nservicebus"
      },
      {
        "name": "IndirectWiringDetector",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "gated by ShouldRun"
      },
      {
        "name": "MediatRExtractor",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "signal gate: needs mediatr"
      },
      {
        "name": "SourceBodyExtractor",
        "tier": "",
        "category": "",
        "stage": "SpecificExtraction",
        "elapsed": "00:00:00",
        "typesAdded": 0,
        "detectionsAdded": 0,
        "skipped": true,
        "skipReason": "gated by ShouldRun"
      }
    ],
    "scorers": [
      {
        "name": "PatternRelevancePruner",
        "typesBefore": 1289,
        "typesAfter": 1289
      },
      {
        "name": "CallReachabilityPruner",
        "typesBefore": 1289,
        "typesAfter": 1289
      },
      {
        "name": "PathProximityPruner",
        "typesBefore": 1289,
        "typesAfter": 1289
      }
    ],
    "compressions": [
      {
        "name": "LlmFriendlyFormatter",
        "tokensSaved": 0
      },
      {
        "name": "NamespaceGrouper",
        "tokensSaved": 0
      },
      {
        "name": "StructuralDeduplicator",
        "tokensSaved": 11196
      },
      {
        "name": "BoilerplateCompressor",
        "tokensSaved": 0
      },
      {
        "name": "TrivialMemberCompressor",
        "tokensSaved": 7723
      }
    ],
    "cache": {
      "textHits": 0,
      "textMisses": 1339,
      "syntaxTreeHits": 6743,
      "syntaxTreeMisses": 1338
    },
    "corpus": {
      "totalFiles": 0,
      "cSharpFiles": 1336,
      "projects": 0
    },
    "funnel": {
      "typesDiscovered": 0,
      "typesHardExcluded": 0,
      "typesIncluded": 0,
      "rawEstimatedTokens": 0,
      "renderedEstimatedTokens": 0,
      "budget": 8000
    },
    "parallelism": {
      "stage2Wall": "00:00:08.7508231",
      "stage2CpuSum": "00:00:17.7771694",
      "stage3Wall": "00:00:01.8518888",
      "stage3CpuSum": "00:00:04.1313600"
    },
    "totalWall": "00:00:10.9377694"
  }
}
analyzed 1336 files ┬╖ 40 types kept of 1289 ┬╖ 25084/8000 tokens ┬╖ 10.9s stage2 
├ù2.0 stage3 ├ù2.2

                              Stage Timing                              
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Stage                   Γöé    Time Γöé Bar                              Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé DiscoveryAndCacheWarmup Γöé   203ms Γöé                                  Γöé
Γöé GenericExtraction       Γöé  8751ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûêΓûê Γöé
Γöé SignalSealing           Γöé     2ms Γöé                                  Γöé
Γöé SpecificExtraction      Γöé  1852ms Γöé ΓûêΓûêΓûêΓûêΓûêΓûê                           Γöé
Γöé Scoring                 Γöé    24ms Γöé                                  Γöé
Γöé Compression             Γöé    42ms Γöé                                  Γöé
Γöé Total                   Γöé 10938ms Γöé                                  Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»

                                   Extractors                                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Name                     Γöé   Time Γöé +Types Γöé +Dets Γöé Status                  Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé SyntaxStructureExtractor Γöé 8743ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé DiRegistrationExtractor  Γöé 8730ms Γöé   1289 Γöé    83 Γöé ran                     Γöé
Γöé EndpointExtractor        Γöé 1850ms Γöé      0 Γöé   103 Γöé ran                     Γöé
Γöé EfCoreExtractor          Γöé 1031ms Γöé      0 Γöé   102 Γöé ran                     Γöé
Γöé InMemoryEventBusExtracto Γöé  649ms Γöé      0 Γöé    73 Γöé ran                     Γöé
Γöé r                        Γöé        Γöé        Γöé       Γöé                         Γöé
Γöé ControllerActionExtracto Γöé  602ms Γöé      0 Γöé    71 Γöé ran                     Γöé
Γöé r                        Γöé        Γöé        Γöé       Γöé                         Γöé
Γöé ProgramCsFlowExtractor   Γöé  250ms Γöé      0 Γöé    39 Γöé ran                     Γöé
Γöé FileTreeExtractor        Γöé  108ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé ProjectStructure         Γöé   35ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé SolutionDiscovery        Γöé   31ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé DependencyExtractor      Γöé   27ms Γöé      0 Γöé     0 Γöé ran                     Γöé
Γöé LayerClassifier          Γöé   27ms Γöé      0 Γöé     0 Γöé ran                     Γöé
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

                   Scorer Funnel                   
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé Scorer                 Γöé Before Γöé After Γöé Delta Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé PatternRelevancePruner Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γöé CallReachabilityPruner Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γöé PathProximityPruner    Γöé   1289 Γöé  1289 Γöé    0% Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
cache 0% hit ┬╖ 1336 files ┬╖ 0 projects
Γò¡ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö¼ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò«
Γöé  Metric  Γöé        Value         Γöé
Γö£ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö╝ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöñ
Γöé Solution Γöé    _eval-dntsite     Γöé
Γöé   Time   Γöé       11561ms        Γöé
Γöé  Tokens  Γöé ~25084 (budget 8000) Γöé
Γöé Version  Γöé v1.0.5-preview.0.42  Γöé
Γò░ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓö┤ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓò»
