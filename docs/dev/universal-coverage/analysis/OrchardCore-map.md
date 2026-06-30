MAP  OrchardCore     (202 projects)

STACK  netstandard2.0 · Minimal APIs · Controllers

STYLE  ModularMonolith  (confidence high)
       evidence: 11 module-like sub-projects: orchardcore.module.targets, errors.orchardcoremodules.twoplus, examples.modules.assyattrib.alpha, examples.modules.assyattrib.bravo, examples.modules.assyattrib.charlie, examples.orchardcoremodules.alpha, modulesample, errors.orchardcorethemes.themeandmodule, module.pages, orchardcore.templates.cms.module, orchardcore.templates.mvc.module

TOPOLOGY (depends-on)
   OrchardCore.Module.Targets ── OrchardCore.Abstractions
   OrchardCore.DisplayManagement ── OrchardCore.DisplayManagement.Abstractions, OrchardCore.Infrastructure.Abstractions, OrchardCore.Liquid.Abstractions, OrchardCore.Localization.Abstractions, OrchardCore.Mvc.Core
   OrchardCore.ResourceManagement ── OrchardCore.ResourceManagement.Core
   OrchardCore.Navigation.Core ── OrchardCore.Admin.Abstractions, OrchardCore.DisplayManagement, OrchardCore.Infrastructure.Abstractions
   OrchardCore.Admin.Abstractions ── OrchardCore.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.ContentManagement.Abstractions ── OrchardCore.Data.YesSql.Abstractions
   OrchardCore.Infrastructure.Abstractions ── OrchardCore.Abstractions, OrchardCore.Data.Abstractions, OrchardCore.ResourceManagement.Abstractions
   OrchardCore.Data.Abstractions ── OrchardCore.Abstractions
   OrchardCore.ContentManagement.Display ── OrchardCore.ContentManagement.Abstractions, OrchardCore.DisplayManagement
   OrchardCore.Abstractions
   OrchardCore.ContentTypes.Abstractions ── OrchardCore.ContentManagement.Abstractions, OrchardCore.ContentManagement.Display
   OrchardCore.Deployment.Abstractions ── OrchardCore.DisplayManagement, OrchardCore.Recipes.Abstractions
   OrchardCore.Recipes.Abstractions ── OrchardCore.Data.YesSql.Abstractions
   OrchardCore.Liquid.Abstractions ── OrchardCore.Abstractions
   OrchardCore.ContentManagement ── OrchardCore.ContentManagement.Abstractions, OrchardCore.ContentTypes.Abstractions, OrchardCore.Data.YesSql, OrchardCore.Infrastructure.Abstractions, OrchardCore.Recipes.Abstractions
   OrchardCore.Indexing.Abstractions ── OrchardCore.ContentManagement.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.ContentManagement.GraphQL ── OrchardCore.Apis.GraphQL.Abstractions, OrchardCore.ContentManagement, OrchardCore.ContentManagement.Abstractions, OrchardCore.ContentManagement.Display, OrchardCore.Contents.Core, OrchardCore.ContentTypes.Abstractions
   OrchardCore.Apis.GraphQL.Abstractions ── OrchardCore.DisplayManagement.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.Contents.Core ── OrchardCore.ContentManagement, OrchardCore.DisplayManagement.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.Settings.Core ── OrchardCore.Abstractions, OrchardCore.Deployment.Abstractions
   OrchardCore.Localization.Abstractions ── OrchardCore.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.ContentLocalization.Abstractions ── OrchardCore.Abstractions, OrchardCore.ContentManagement.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.Workflows.Abstractions ── OrchardCore.DisplayManagement, OrchardCore.Liquid.Abstractions
   OrchardCore.Users.Core ── OrchardCore.ContentManagement.GraphQL, OrchardCore.Data.YesSql, OrchardCore.DisplayManagement.Abstractions, OrchardCore.Infrastructure.Abstractions, OrchardCore.Roles.Core, OrchardCore.Users.Abstractions
   OrchardCore.Data.YesSql ── OrchardCore.Abstractions, OrchardCore.Data, OrchardCore.Data.YesSql.Abstractions
   OrchardCore.DisplayManagement.Abstractions
   OrchardCore.DisplayManagement.Liquid ── OrchardCore.DisplayManagement, OrchardCore.DynamicCache.Abstractions, OrchardCore.Liquid.Abstractions, OrchardCore.ResourceManagement.Abstractions
   OrchardCore.ContentPreview.Abstractions
   OrchardCore.Contents.TagHelpers ── OrchardCore.ContentManagement.Abstractions, OrchardCore.DisplayManagement, OrchardCore.DisplayManagement.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.ResourceManagement.Abstractions
   OrchardCore.Shortcodes.Abstractions ── OrchardCore.Infrastructure.Abstractions
   OrchardCore.Data.YesSql.Abstractions ── OrchardCore.Abstractions, OrchardCore.Data.Abstractions
   OrchardCore.Indexing.Core ── OrchardCore.ContentLocalization.Abstractions, OrchardCore.ContentManagement, OrchardCore.ContentPreview.Abstractions, OrchardCore.Deployment.Abstractions, OrchardCore.Indexing.Abstractions, OrchardCore.Infrastructure.Abstractions, OrchardCore.Search.Abstractions
   OrchardCore.MetaWeblog.Abstractions ── OrchardCore.ContentManagement.Abstractions, OrchardCore.XmlRpc.Abstractions
   OrchardCore.Theme.Targets ── OrchardCore.Module.Targets
   OrchardCore.Users.Abstractions ── OrchardCore.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.ContentFields.Core ── OrchardCore.ContentManagement.Abstractions
   OrchardCore.FileStorage.Abstractions
   OrchardCore.Media.Abstractions ── OrchardCore.Abstractions, OrchardCore.ContentManagement.Abstractions, OrchardCore.FileStorage.Abstractions
   OrchardCore.Roles.Core ── OrchardCore.Infrastructure.Abstractions, OrchardCore.Roles.Abstractions
   OrchardCore.Search.Abstractions ── OrchardCore.ContentManagement, OrchardCore.DisplayManagement, OrchardCore.Indexing.Abstractions
   OrchardCore.Setup.Abstractions ── OrchardCore.Abstractions, OrchardCore.Recipes.Abstractions
   OrchardCore.AdminMenu.Abstractions ── OrchardCore.Navigation.Core
   OrchardCore.Deployment.Core ── OrchardCore.Deployment.Abstractions
   OrchardCore.Email.Abstractions ── OrchardCore.Infrastructure.Abstractions
   OrchardCore.Media.Core ── OrchardCore.FileStorage.Abstractions, OrchardCore.Infrastructure.Abstractions, OrchardCore.Media.Abstractions
   OrchardCore.Queries.Abstractions ── OrchardCore.Infrastructure.Abstractions
   OrchardCore.Sitemaps.Abstractions ── OrchardCore.ContentManagement.Abstractions, OrchardCore.DisplayManagement.Abstractions, OrchardCore.Infrastructure.Abstractions
   OrchardCore.AuditTrail.Abstractions ── OrchardCore.ContentManagement.Abstractions, OrchardCore.ContentManagement.Display
   OrchardCore.Email.Core ── OrchardCore.Email.Abstractions, OrchardCore.Infrastructure.Abstractions
   … and 152 more projects (use --focus for a scoped slice)

ENTRY POINTS
   HTTP (281)
      DELETE /api/content/{contentItemId}  → DefaultContentManager.GetAsync  (src/OrchardCore.Modules/OrchardCore.Contents/Endpoints/Api/DeleteEndpoint.cs:16)
      GET /Access  → OpenIdServerService.GetSettingsAsync  (src/OrchardCore.Modules/OrchardCore.OpenId/Controllers/AccessController.cs:298)
      GET /Access  → OpenIdServerService.GetSettingsAsync  (src/OrchardCore.Modules/OrchardCore.OpenId/Controllers/AccessController.cs:52)
      GET /Admin  → AdminController  (src/OrchardCore.Modules/OrchardCore.DataLocalization/Controllers/AdminController.cs:153)
      GET /Admin  → AdminController  (src/OrchardCore.Modules/OrchardCore.DataLocalization/Controllers/AdminController.cs:106)
      GET /Admin  → AdminController  (src/OrchardCore.Modules/OrchardCore.DataLocalization/Controllers/AdminController.cs:88)
      GET /Admin  → AdminController  (src/OrchardCore.Modules/OrchardCore.Cors/Controllers/AdminController.cs:43)
      GET /Admin  → AdminController  (src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/Controllers/AdminController.cs:8)
      GET /api/content/{contentItemId}  → DefaultContentManager.GetAsync  (src/OrchardCore.Modules/OrchardCore.Contents/Endpoints/Api/GetEndpoint.cs:16)
      GET /api/elasticsearch/content  → ElasticsearchApiController  (src/OrchardCore.Modules/OrchardCore.Elasticsearch/Controllers/ElasticsearchApiController.cs:28)
      GET /api/elasticsearch/documents  → ElasticsearchApiController  (src/OrchardCore.Modules/OrchardCore.Elasticsearch/Controllers/ElasticsearchApiController.cs:56)
      GET /api/lucene/content  → LuceneApiController  (src/OrchardCore.Modules/OrchardCore.Lucene/Controllers/LuceneApiController.cs:28)
      GET /api/lucene/documents  → LuceneApiController  (src/OrchardCore.Modules/OrchardCore.Lucene/Controllers/LuceneApiController.cs:56)
      GET /Application  → Notifier.WarningAsync  (src/OrchardCore.Modules/OrchardCore.OpenId/Controllers/ApplicationController.cs:93)
      GET /ChangeEmail  → ChangeEmailController  (src/OrchardCore.Modules/OrchardCore.Users/Controllers/ChangeEmailController.cs:84)
      GET /ChangeEmail  → ChangeEmailController  (src/OrchardCore.Modules/OrchardCore.Users/Controllers/ChangeEmailController.cs:35)
      GET /ContentCulturePicker  → DefaultLocalizationService.GetSupportedCulturesAsync  (src/OrchardCore.Modules/OrchardCore.ContentLocalization/Controllers/ContentCulturePickerController.cs:29)
      GET /Demo/Demo  → DemoController  (src/OrchardCore.Modules/OrchardCore.Demo/Controllers/DemoController.cs:7)
      GET /Demo/Demo/About  → DemoController  (src/OrchardCore.Modules/OrchardCore.Demo/Controllers/DemoController.cs:13)
      GET /Demo/Demo/Contact  → DemoController  (src/OrchardCore.Modules/OrchardCore.Demo/Controllers/DemoController.cs:18)
      … and 261 more (http entries — use --focus for a drill-in)

PACKAGES
   Web/API:  Azure.Extensions.AspNetCore.Configuration.Secrets, Azure.Extensions.AspNetCore.DataProtection.Blobs, Microsoft.AspNetCore.Authentication.Facebook, Microsoft.AspNetCore.Authentication.Google, Microsoft.AspNetCore.Authentication.MicrosoftAccount, Microsoft.AspNetCore.Authentication.OpenIdConnect, Microsoft.AspNetCore.Authentication.Twitter, Microsoft.AspNetCore.Authorization … (16 total)
   Logging:  OrchardCore.Logging.NLog, OrchardCore.Logging.Serilog
   Testing:  Moq, xunit.v3.mtp-v2
   Cloud:  Azure.Communication.Email, Azure.Communication.Sms, Azure.Identity, Azure.Search.Documents, Azure.Storage.Blobs
   Other:  AngleSharp, AnyAscii, AWSSDK.Extensions.NETCore.Setup, AWSSDK.S3, BenchmarkDotNet, Castle.Core, DocumentFormat.OpenXml, Elastic.Clients.Elasticsearch … (64 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
