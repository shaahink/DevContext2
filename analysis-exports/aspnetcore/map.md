MAP  AspNetCore     (395 projects)

STACK  net11.0, net472, netstandard2.0 · Minimal APIs · Controllers · EF Core

STYLE  NLayer  (confidence moderate)
       evidence: EF Core + 429 projects; folder roles: Application, Infrastructure, Api, Core

TOPOLOGY (depends-on)
   ApiExplorerWebSite
   ApplicationModelWebSite
   aspnetcoretools ── dotnet-dev-certs, dotnet-user-jwts, dotnet-user-secrets
   AzureAppServicesHostingStartupSample
   AzureAppServicesSample
   BasicTestApp ── LazyTestContentPackage, TestContentPackage
   BasicWebSite
   Benchmarks
   BlazingPizza.Server
   BlazorUnitedApp ── BlazorUnitedApp.Client
   BlazorUnitedApp.Client
   BuildAfterTargetingPack
   Certificate.Optional.Sample
   Certificate.Sample
   ClaimsTransformation
   ClassLibraryWithPortablePdbs
   ClientSample
   CodeGenerator
   Components.TestServer ── BasicTestApp, Components.WasmMinimal, Components.WasmRemoteAuthentication, Microsoft.Extensions.Validation.ValidationsGenerator
   Components.WasmMinimal ── NotReferencedInWasmCodePackage, TestContentPackage
   Components.WasmRemoteAuthentication
   ComponentsApp.App
   ComponentsApp.Server ── ComponentsApp.App
   ControllersFromServicesClassLibrary
   ControllersFromServicesWebSite ── ControllersFromServicesClassLibrary
   CookiePolicySample
   Cookies
   CookieSample
   CookieSessionSample
   CorsMiddlewareWebSite
   CorsWebSite
   CustomAuthorizationFailureResponse
   CustomBasePathApp
   CustomEncryptorSample
   CustomPolicyProvider
   DatabaseErrorPageSample
   DefaultBuilder.SampleApp
   DelegationSite
   DeveloperExceptionPageSample
   dotnet-dev-certs
   dotnet-getdocument
   dotnet-sql-cache
   dotnet-user-jwts
   dotnet-user-secrets
   DynamicSchemes
   EchoApp
   EntityFrameworkCoreSample
   ErrorPageMiddlewareWebSite
   ExceptionHandlerSample
   FilesWebSite
   FormatterWebSite
   GenericHostWebSite
   GenericWebHost
   GetDocument.Insider
   GetDocumentSample
   GlobalizationWasmApp
   HeaderPropagationSample
   HealthChecksSample
   HelixTestRunner
   HostedBlazorWebassemblyApp.Client ── HostedBlazorWebassemblyApp.Shared
   HostedBlazorWebassemblyApp.Server ── HostedBlazorWebassemblyApp.Client, HostedBlazorWebassemblyApp.Shared
   HostedBlazorWebassemblyApp.Shared
   HostedInAspNet.Client
   HostedInAspNet.Server ── CustomBasePathApp, HostedInAspNet.Client
   HostFilteringSample
   HotAddSample
   HtmlGenerationWebSite
   http2cat
   Http2SampleApp
   Http3SampleApp
   HttpAbstractions.SampleApp
   HttpClientApp
   HttpLogging.Sample
   HttpOverridesSample
   HttpsPolicySample
   HttpStress
   Identity.DefaultUI.WebSite
   Identity.ExternalClaims
   IdentitySample.ApiEndpoints
   IdentitySample.DefaultUI
   IdentitySample.Mvc
   IdentitySample.PasskeyConformance
   IdentitySample.PasskeyUI
   IIS.Common.TestLib ── Microsoft.AspNetCore.Server.IntegrationTesting.IIS
   IIS.Microbenchmarks ── IIS.Tests, IISExpress.FunctionalTests, InProcessWebSite, Microsoft.AspNetCore.Server.IntegrationTesting
   IISSample
   InProcessNewShimWebSite
   InProcessWebSite
   IntegrationTestsWebsite
   InteropClient
   InteropWebsite
   IStartupInjectionAssemblyName
   JwtBearerSample
   JwtClientSample
   JwtSample
   Kestrel.SampleApp
   KeyManagementSample
   KeyManagementSimulator
   LargeResponseApp
   LazyTestContentPackage
   LinkabilityChecker
   LocalizationSample
   LocalizationWebsite ── ResourcesClassLibraryNoAttribute, ResourcesClassLibraryWithAttribute
   Logging.W3C.Sample
   Microsoft.AspNetCore
   Microsoft.AspNetCore.Analyzer.Testing
   Microsoft.AspNetCore.Analyzers
   Microsoft.AspNetCore.ANCMSymbols
   Microsoft.AspNetCore.Antiforgery
   Microsoft.AspNetCore.Antiforgery.Microbenchmarks
   Microsoft.AspNetCore.App.Analyzers
   Microsoft.AspNetCore.App.CodeFixes ── Microsoft.AspNetCore.App.Analyzers
   Microsoft.AspNetCore.App.Internal.Assets
   Microsoft.AspNetCore.App.SourceGenerators
   Microsoft.AspNetCore.Authentication
   Microsoft.AspNetCore.Authentication.Abstractions
   Microsoft.AspNetCore.Authentication.BearerToken
   Microsoft.AspNetCore.Authentication.Certificate
   Microsoft.AspNetCore.Authentication.Cookies
   Microsoft.AspNetCore.Authentication.Core
   Microsoft.AspNetCore.Authentication.Facebook
   Microsoft.AspNetCore.Authentication.Google
   Microsoft.AspNetCore.Authentication.JwtBearer
   Microsoft.AspNetCore.Authentication.MicrosoftAccount
   Microsoft.AspNetCore.Authentication.Negotiate
   Microsoft.AspNetCore.Authentication.OAuth
   Microsoft.AspNetCore.Authentication.OpenIdConnect
   Microsoft.AspNetCore.Authentication.Twitter
   Microsoft.AspNetCore.Authentication.WsFederation
   Microsoft.AspNetCore.Authorization
   Microsoft.AspNetCore.Authorization.Policy
   Microsoft.AspNetCore.AzureAppServices.HostingStartup
   Microsoft.AspNetCore.AzureAppServicesIntegration
   Microsoft.AspNetCore.BrowserTesting
   Microsoft.AspNetCore.Components
   Microsoft.AspNetCore.Components.Analyzers
   Microsoft.AspNetCore.Components.Authorization
   Microsoft.AspNetCore.Components.CustomElements
   Microsoft.AspNetCore.Components.Endpoints
   Microsoft.AspNetCore.Components.Forms
   Microsoft.AspNetCore.Components.Media
   Microsoft.AspNetCore.Components.Performance
   Microsoft.AspNetCore.Components.QuickGrid
   Microsoft.AspNetCore.Components.QuickGrid.EntityFrameworkAdapter
   Microsoft.AspNetCore.Components.SdkAnalyzers
   Microsoft.AspNetCore.Components.Server
   Microsoft.AspNetCore.Components.Testing ── Microsoft.AspNetCore.Components.Testing.Generators, Microsoft.AspNetCore.Components.Testing.Tasks
   Microsoft.AspNetCore.Components.Testing.Generators
   Microsoft.AspNetCore.Components.Testing.Tasks
   Microsoft.AspNetCore.Components.Web
   Microsoft.AspNetCore.Components.WebAssembly
   Microsoft.AspNetCore.Components.WebAssembly.Authentication
   Microsoft.AspNetCore.Components.WebAssembly.DevServer
   Microsoft.AspNetCore.Components.WebAssembly.Server
   Microsoft.AspNetCore.Components.WebView
   Microsoft.AspNetCore.Components.WebView.Photino
   Microsoft.AspNetCore.Connections.Abstractions
   Microsoft.AspNetCore.CookiePolicy
   Microsoft.AspNetCore.Cors
   Microsoft.AspNetCore.Cryptography.Internal
   Microsoft.AspNetCore.Cryptography.KeyDerivation
   Microsoft.AspNetCore.DataProtection
   Microsoft.AspNetCore.DataProtection.Abstractions
   Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
   Microsoft.AspNetCore.DataProtection.Extensions
   Microsoft.AspNetCore.DataProtection.MicroBenchmarks
   Microsoft.AspNetCore.DataProtection.StackExchangeRedis
   Microsoft.AspNetCore.DeveloperCertificates.XPlat
   Microsoft.AspNetCore.Diagnostics
   Microsoft.AspNetCore.Diagnostics.Abstractions
   Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
   Microsoft.AspNetCore.Diagnostics.HealthChecks
   Microsoft.AspNetCore.Grpc.JsonTranscoding
   Microsoft.AspNetCore.Grpc.Microbenchmarks
   Microsoft.AspNetCore.Grpc.Swagger
   Microsoft.AspNetCore.HeaderPropagation
   Microsoft.AspNetCore.HostFiltering
   Microsoft.AspNetCore.Hosting
   Microsoft.AspNetCore.Hosting.Abstractions
   Microsoft.AspNetCore.Hosting.Server.Abstractions
   Microsoft.AspNetCore.Hosting.WindowsServices
   Microsoft.AspNetCore.Html.Abstractions
   Microsoft.AspNetCore.Http
   Microsoft.AspNetCore.Http.Abstractions
   Microsoft.AspNetCore.Http.Abstractions.Microbenchmarks
   Microsoft.AspNetCore.Http.Connections
   Microsoft.AspNetCore.Http.Connections.Client
   Microsoft.AspNetCore.Http.Connections.Common
   Microsoft.AspNetCore.Http.Extensions
   Microsoft.AspNetCore.Http.Features
   Microsoft.AspNetCore.Http.Microbenchmarks ── Microsoft.AspNetCore.Http.RequestDelegateGenerator
   Microsoft.AspNetCore.Http.RequestDelegateGenerator
   Microsoft.AspNetCore.Http.Results
   Microsoft.AspNetCore.HttpLogging
   Microsoft.AspNetCore.HttpOverrides
   Microsoft.AspNetCore.HttpsPolicy
   Microsoft.AspNetCore.Identity ── Microsoft.AspNetCore.Http.RequestDelegateGenerator
   Microsoft.AspNetCore.Identity.EntityFrameworkCore
   Microsoft.AspNetCore.Identity.UI
   Microsoft.AspNetCore.InternalTesting
   Microsoft.AspNetCore.JsonPatch
   Microsoft.AspNetCore.JsonPatch.SystemTextJson
   Microsoft.AspNetCore.Localization
   Microsoft.AspNetCore.Localization.Routing
   Microsoft.AspNetCore.Metadata
   Microsoft.AspNetCore.MiddlewareAnalysis
   Microsoft.AspNetCore.Mvc
   Microsoft.AspNetCore.Mvc.Abstractions
   Microsoft.AspNetCore.Mvc.Analyzers
   Microsoft.AspNetCore.Mvc.Api.Analyzers
   Microsoft.AspNetCore.Mvc.ApiExplorer
   Microsoft.AspNetCore.Mvc.Core
   Microsoft.AspNetCore.Mvc.Core.TestCommon
   Microsoft.AspNetCore.Mvc.Cors
   Microsoft.AspNetCore.Mvc.DataAnnotations
   Microsoft.AspNetCore.Mvc.Formatters.Json
   Microsoft.AspNetCore.Mvc.Formatters.Xml
   Microsoft.AspNetCore.Mvc.Localization
   Microsoft.AspNetCore.Mvc.Microbenchmarks ── Microsoft.AspNetCore.Mvc.Views.Microbenchmarks
   Microsoft.AspNetCore.Mvc.NewtonsoftJson
   Microsoft.AspNetCore.Mvc.Razor
   Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
   Microsoft.AspNetCore.Mvc.RazorPages
   Microsoft.AspNetCore.Mvc.TagHelpers
   Microsoft.AspNetCore.Mvc.TestDiagnosticListener
   Microsoft.AspNetCore.Mvc.Testing
   Microsoft.AspNetCore.Mvc.Testing.Tasks
   Microsoft.AspNetCore.Mvc.ViewFeatures
   Microsoft.AspNetCore.Mvc.Views.Microbenchmarks
   Microsoft.AspNetCore.Mvc.Views.TestCommon ── Microsoft.AspNetCore.Mvc.Core.TestCommon
   Microsoft.AspNetCore.OpenApi ── Microsoft.AspNetCore.Http.RequestDelegateGenerator, Microsoft.AspNetCore.OpenApi.SourceGenerators
   Microsoft.AspNetCore.OpenApi.Microbenchmarks ── Microsoft.AspNetCore.OpenApi.Tests
   Microsoft.AspNetCore.OpenApi.SourceGenerators
   Microsoft.AspNetCore.OutputCaching
   Microsoft.AspNetCore.OutputCaching.Microbenchmarks
   Microsoft.AspNetCore.OutputCaching.StackExchangeRedis
   Microsoft.AspNetCore.Owin
   Microsoft.AspNetCore.Owin.Microbenchmarks
   Microsoft.AspNetCore.RateLimiting
   Microsoft.AspNetCore.Razor
   Microsoft.AspNetCore.Razor.Runtime
   Microsoft.AspNetCore.RequestDecompression
   Microsoft.AspNetCore.RequestDecompression.Microbenchmarks
   Microsoft.AspNetCore.ResponseCaching
   Microsoft.AspNetCore.ResponseCaching.Abstractions
   Microsoft.AspNetCore.ResponseCaching.Microbenchmarks
   Microsoft.AspNetCore.ResponseCompression
   Microsoft.AspNetCore.ResponseCompression.Microbenchmarks
   Microsoft.AspNetCore.Rewrite
   Microsoft.AspNetCore.Routing
   Microsoft.AspNetCore.Routing.Abstractions
   Microsoft.AspNetCore.Routing.Microbenchmarks
   Microsoft.AspNetCore.Security.Microbenchmarks
   Microsoft.AspNetCore.Server.HttpSys
   Microsoft.AspNetCore.Server.HttpSys.Microbenchmarks
   Microsoft.AspNetCore.Server.IIS
   Microsoft.AspNetCore.Server.IISIntegration
   Microsoft.AspNetCore.Server.IntegrationTesting
   Microsoft.AspNetCore.Server.IntegrationTesting.IIS ── Microsoft.AspNetCore.Server.IntegrationTesting, TestTasks
   Microsoft.AspNetCore.Server.Kestrel
   Microsoft.AspNetCore.Server.Kestrel.Core
   Microsoft.AspNetCore.Server.Kestrel.Microbenchmarks
   Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes
   Microsoft.AspNetCore.Server.Kestrel.Transport.Quic
   Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets
   Microsoft.AspNetCore.Session
   Microsoft.AspNetCore.SignalR
   Microsoft.AspNetCore.SignalR.Client
   Microsoft.AspNetCore.SignalR.Client.Core
   Microsoft.AspNetCore.SignalR.Client.SourceGenerator
   Microsoft.AspNetCore.SignalR.Common
   Microsoft.AspNetCore.SignalR.Core
   Microsoft.AspNetCore.SignalR.Microbenchmarks ── Microsoft.AspNetCore.SignalR.StackExchangeRedis.Tests
   Microsoft.AspNetCore.SignalR.Protocols.Json
   Microsoft.AspNetCore.SignalR.Protocols.MessagePack
   Microsoft.AspNetCore.SignalR.Protocols.NewtonsoftJson
   Microsoft.AspNetCore.SignalR.StackExchangeRedis
   Microsoft.AspNetCore.SignalR.Tests.Utils
   Microsoft.AspNetCore.SpaProxy
   Microsoft.AspNetCore.SpaServices.Extensions
   Microsoft.AspNetCore.StaticAssets
   Microsoft.AspNetCore.StaticFiles
   Microsoft.AspNetCore.TestHost
   Microsoft.AspNetCore.WebSockets
   Microsoft.AspNetCore.WebSockets.Microbenchmarks
   Microsoft.AspNetCore.WebUtilities
   Microsoft.AspNetCore.WebUtilities.Microbenchmarks
   Microsoft.Authentication.WebAssembly.Msal
   Microsoft.dotnet-openapi
   Microsoft.DotNet.Web.Client.ItemTemplates
   Microsoft.DotNet.Web.ItemTemplates
   Microsoft.DotNet.Web.ProjectTemplates
   Microsoft.Extensions.ApiDescription.Client
   Microsoft.Extensions.ApiDescription.Server
   Microsoft.Extensions.Caching.MicroBenchmarks
   Microsoft.Extensions.Caching.SqlServer
   Microsoft.Extensions.Caching.StackExchangeRedis
   Microsoft.Extensions.Configuration.KeyPerFile
   Microsoft.Extensions.Diagnostics.HealthChecks
   Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions
   Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
   Microsoft.Extensions.Features
   Microsoft.Extensions.FileProviders.Embedded ── Microsoft.Extensions.FileProviders.Embedded.Manifest.Task
   Microsoft.Extensions.FileProviders.Embedded.Manifest.Task
   Microsoft.Extensions.Http.Polly
   Microsoft.Extensions.Identity.Core
   Microsoft.Extensions.Identity.Stores
   Microsoft.Extensions.Localization
   Microsoft.Extensions.Localization.Abstractions
   Microsoft.Extensions.Logging.AzureAppServices
   Microsoft.Extensions.Validation ── Microsoft.Extensions.Validation.ValidationsGenerator
   Microsoft.Extensions.Validation.Localization
   Microsoft.Extensions.Validation.ValidationsGenerator
   Microsoft.Extensions.WebEncoders
   Microsoft.JSInterop
   Microsoft.JSInterop.WebAssembly
   Microsoft.McpServer.ProjectTemplates
   Microsoft.Net.Http.Headers
   Microsoft.Web.Xdt.Extensions
   MiddlewareAnalysisSample
   MinimalFormSample
   MinimalJwtBearerSample
   MinimalOpenIdConnectSample
   MinimalSample ── Microsoft.AspNetCore.Http.RequestDelegateGenerator
   MinimalSampleOwin
   Mvc.RoutingWebSite
   MvcFormSample
   MvcSandbox
   NativeIISSample ── Microsoft.AspNetCore.Server.IntegrationTesting.IIS
   NegotiateAuthSample
   NonDISample
   NotReferencedInWasmCodePackage
   OpenIdConnectSample
   OutputCachingSample
   PathSchemeSelection
   PhotinoTestApp ── BasicTestApp, Microsoft.AspNetCore.Components.WebView.Photino
   PlaintextApp
   QueueSharing
   RateLimitingSample
   RazorBuildWebSite ── RazorBuildWebSite.Views
   RazorBuildWebSite.PrecompiledViews
   RazorBuildWebSite.Views
   RazorPagesClassLibrary
   RazorPagesWebSite ── RazorPagesClassLibrary
   RazorWebSite
   Redis
   RepoTasks
   RequestDecompressionSample
   ResourcesClassLibraryNoAttribute
   ResourcesClassLibraryWithAttribute
   ResponseCachingSample
   ResponseCompressionSample
   ResultsOfTGenerator
   RewriteSample
   RoutingSandbox
   RoutingWebSite
   Sample ── Microsoft.AspNetCore.Http.RequestDelegateGenerator, Microsoft.AspNetCore.OpenApi.SourceGenerators
   SampleStartups
   Sandbox
   SecurityWebSite
   SelfHostServer
   ServerComparison.TestSites
   SessionSample
   SignalR.Client.FunctionalTestApp
   SignalRSamples
   SimpleWebSite
   SimpleWebSiteWithWebApplicationBuilder
   SimpleWebSiteWithWebApplicationBuilderException
   SocialSample
   SocialWeather
   StandaloneApp
   StaticFileSample
   StaticFilesAuth
   StatusCodePagesSample
   SystemdTestApp
   TagHelpersWebSite ── RazorPagesClassLibrary
   TestClient
   TestContentPackage
   TestStartupAssembly1
   TestTasks
   TlsFeaturesObserve
   VersioningWebSite
   Wasm.Performance.ConsoleHost ── Wasm.Performance.TestApp
   Wasm.Performance.Driver ── Wasm.Performance.TestApp
   Wasm.Performance.TestApp
   Wasm.Prerendered.Client
   Wasm.Prerendered.Server ── CustomBasePathApp, Wasm.Prerendered.Client
   WebAppSample ── Microsoft.AspNetCore.App.Analyzers, Microsoft.AspNetCore.App.CodeFixes, Microsoft.AspNetCore.App.CodeFixes
   WebSockets.TestServer
   WebSocketSample
   WebTransportInteractiveSampleApp
   WebTransportSampleApp
   WelcomePageSample
   WsFedSample
   XmlFormattersWebSite

ENTRY POINTS
   HTTP (518)
      POST /Manage/DownloadPersonalData  (src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs:114)
      POST /Manage/LinkExternalLogin  (src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs:94)
      POST /Account/PasskeyRequestOptions  (src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs:78)
      POST /Account/PasskeyCreationOptions  (src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs:53)
      POST /Account/Logout  (src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs:44)
      POST /Account/PerformExternalLogin  (src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs:25)
      GET /api/get/{id}  (src/Http/Routing/test/testassets/RoutingWebSite/UseRouterStartup.cs:45)
      GET /api/get/{id}  (src/Http/Routing/test/testassets/RoutingWebSite/UseRouterStartup.cs:28)
      GET /api/get/{id}  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:161)
      GET /api/DomainWildcard  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:137)
      GET /WithDoubleAsteriskCatchAll/{**path}  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:106)
      GET /WithSingleAsteriskCatchAll/{*path}  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:94)
      GET /withoptionalconstraints/{id:endsWith(_001)?}  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:85)
      GET /withconstraints/{id:endsWith(_001)}  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:76)
      GET /convention  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:66)
      GET /plaintext  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:55)
      GET /  (src/Http/Routing/test/testassets/RoutingWebSite/UseEndpointRoutingStartup.cs:37)
      GET /api/get/{id}  (src/Http/Routing/test/testassets/RoutingSandbox/UseRouterStartup.cs:28)
      GET /attributes  (src/Http/Routing/test/testassets/RoutingSandbox/UseEndpointRoutingStartup.cs:78)
      GET /graph  (src/Http/Routing/test/testassets/RoutingSandbox/UseEndpointRoutingStartup.cs:64)
      GET /plaintext  (src/Http/Routing/test/testassets/RoutingSandbox/UseEndpointRoutingStartup.cs:53)
      GET /  (src/Http/Routing/test/testassets/RoutingSandbox/UseEndpointRoutingStartup.cs:31)
      GET /deployment  (src/SignalR/clients/ts/FunctionalTests/Startup.cs:259)
      GET /clientresult/{id}  (src/SignalR/clients/ts/FunctionalTests/Startup.cs:246)
      GET /generateJwtToken  (src/SignalR/clients/ts/FunctionalTests/Startup.cs:241)
      GET /todos/{id}  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApiAot-CSharp/Program.Main.cs:53)
      GET /todos/  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApiAot-CSharp/Program.Main.cs:51)
      GET /todos/{id}  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApiAot-CSharp/Program.cs:47)
      GET /todos/  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApiAot-CSharp/Program.cs:45)
      GET /weatherforecast  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApi-CSharp/Program.MinimalAPIs.WindowsOrNoAuth.cs:42)
      GET /weatherforecast  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApi-CSharp/Program.MinimalAPIs.OrgOrIndividualB2CAuth.cs:114)
      GET /  (src/ProjectTemplates/Web.ProjectTemplates/content/GrpcService-CSharp/Program.Main.cs:18)
      GET /  (src/ProjectTemplates/Web.ProjectTemplates/content/GrpcService-CSharp/Program.cs:12)
      GET /  (src/ProjectTemplates/Web.ProjectTemplates/content/EmptyWeb-CSharp/Program.Main.cs:10)
      GET /  (src/ProjectTemplates/Web.ProjectTemplates/content/EmptyWeb-CSharp/Program.cs:4)
      GET /  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilderException/Program.cs:6)
      POST /fileupload  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:67)
      POST /accepts-xml  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:65)
      POST /accepts-default  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:64)
      GET /greeting  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:62)
      GET /webroot  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:60)
      GET /environment  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:59)
      GET /problem  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:57)
      GET /many-results  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:47)
      GET /accepted-object  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:45)
      GET /ok-object  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:43)
      GET /json  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:41)
      GET /assert-early  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:39)
      GET /  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:37)
      POST /_ready/{token}  (src/Components/Testing/src/Infrastructure/ServerFixture.cs:88)
      POST /oidc/token  (src/Components/test/testassets/Components.TestServer/RemoteAuthenticationStartup.cs:80)
      GET /oidc/authorize  (src/Components/test/testassets/Components.TestServer/RemoteAuthenticationStartup.cs:61)
      GET /oidc/.well-known/openid-configuration  (src/Components/test/testassets/Components.TestServer/RemoteAuthenticationStartup.cs:49)
      POST /api/antiforgery-form  (src/Components/test/testassets/Components.TestServer/RazorComponentEndpointsStartup.cs:364)
      POST /redirect/nonblazor/post  (src/Components/test/testassets/Components.TestServer/RazorComponentEndpointsStartup.cs:355)
      GET /redirect/nonblazor/get  (src/Components/test/testassets/Components.TestServer/RazorComponentEndpointsStartup.cs:354)
      PUT /  (src/Servers/Kestrel/stress/Program.cs:498)
      POST /duplexSlow  (src/Servers/Kestrel/stress/Program.cs:483)
      POST /duplex  (src/Servers/Kestrel/stress/Program.cs:478)
      POST /  (src/Servers/Kestrel/stress/Program.cs:470)
      GET /parallel-abort  (src/Servers/Kestrel/stress/Program.cs:462)
      GET /abort  (src/Servers/Kestrel/stress/Program.cs:456)
      GET /headers  (src/Servers/Kestrel/stress/Program.cs:436)
      GET /slow  (src/Servers/Kestrel/stress/Program.cs:427)
      GET /  (src/Servers/Kestrel/stress/Program.cs:422)
      POST /manage/info  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:348)
      GET /manage/info  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:336)
      POST /manage/2fa  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:260)
      POST /resetPassword  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:225)
      POST /forgotPassword  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:206)
      POST /resendConfirmationEmail  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:193)
      GET /confirmEmail  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:142)
      POST /refresh  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:122)
      POST /login  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:90)
      POST /register  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:57)
      GET /HtmlGeneration_Customer  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Areas/Customer/Controllers/HtmlGeneration_CustomerController.cs:16)
      GET /[area]/RemoteAttribute_Verify/IsIdAvailable  (src/Mvc/test/WebSites/BasicWebSite/Areas/Area2/Controllers/RemoteAttribute_VerifyController.cs:15)
      GET /[area]/RemoteAttribute_Verify/IsIdAvailable  (src/Mvc/test/WebSites/BasicWebSite/Areas/Area1/Controllers/RemoteAttribute_VerifyController.cs:14)
      GET /[area]/RemoteAttribute_Home/Details  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Areas/Area1/Controllers/RemoteAttribute_HomeController.cs:34)
      POST /[area]/RemoteAttribute_Home/Create  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Areas/Area1/Controllers/RemoteAttribute_HomeController.cs:21)
      GET /[area]/RemoteAttribute_Home/Create  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Areas/Area1/Controllers/RemoteAttribute_HomeController.cs:15)
      GET /Home  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Travel/HomeController.cs:23)
      GET /Order/GetOrder  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Order/OrderController.cs:19)
      GET /[area]/Users  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/UserManagementController.cs:19)
      GET /[area]/LG3/LinkOutsideOfArea/{id?}  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/LG3Controller.cs:34)
      GET /[area]/LG3/LinkInsideOfAreaFail/{id?}  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/LG3Controller.cs:28)
      GET /[area]/LG3/LinkInsideOfArea/{id?}  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/LG3Controller.cs:23)
      GET /[area]/LG3/SomeAction/{id?}  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/LG3Controller.cs:19)
      POST /Fallback  → ControllerBase.Content  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/FallbackController.cs:16)
      POST /Dynamic  → ControllerBase.Content  (src/Mvc/test/WebSites/RoutingWebSite/Areas/Admin/DynamicController.cs:16)
      GET /api/orders/{id?}  (src/Mvc/test/WebSites/BasicWebSite/Controllers/LinkGeneration/OrdersController.cs:17)
      GET /api/orders/{id?}  (src/Mvc/test/WebSites/BasicWebSite/Controllers/LinkGeneration/OrdersController.cs:11)
      GET /ProducesWithMediaTypeSuffixesController/ContactInfo  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContentNegotiation/ProducesWithMediaTypeSuffixesController.cs:12)
      GET /InvalidContentType  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContentNegotiation/InvalidContentTypeController.cs:10)
      GET /FormatFilter  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContentNegotiation/FormatFilterController.cs:20)
      GET /ConsumesAttribute_WithFallbackActionController/CreateProduct  → ControllerBase.Content  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_WithFallbackActionController.cs:25)
      GET /ConsumesAttribute_WithFallbackActionController/CreateProductXml  → ControllerBase.Content  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_WithFallbackActionController.cs:18)
      GET /ConsumesAttribute_WithFallbackActionController/CreateProductJson  → ControllerBase.Content  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_WithFallbackActionController.cs:11)
      GET /ConsumesAttribute_PassThrough/CreateProductMultiple  → ControllerBase.Content  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_PassThroughController.cs:24)
      GET /ConsumesAttribute_PassThrough/CreateProductMultiple  → ControllerBase.Content  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_PassThroughController.cs:18)
      GET /ConsumesAttribute_PassThrough/CreateProduct  → ControllerBase.Content  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_PassThroughController.cs:12)
      GET /ConsumesAttribute_AmbiguousActions/CreateProduct  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_NoFallBackActionController.cs:18)
      GET /ConsumesAttribute_AmbiguousActions/CreateProduct  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_NoFallBackActionController.cs:12)
      GET /ConsumesAttribute_MediaTypeSuffix/CreateProduct  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_MediaTypeSuffix.cs:21)
      GET /ConsumesAttribute_MediaTypeSuffix/CreateProduct  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionConstraints/ConsumesAttribute_MediaTypeSuffix.cs:12)
      DELETE /api/Values  (src/Grpc/JsonTranscoding/test/testassets/Sandbox/Controllers/ValuesController.cs:39)
      PUT /api/Values  (src/Grpc/JsonTranscoding/test/testassets/Sandbox/Controllers/ValuesController.cs:33)
      POST /api/Values  (src/Grpc/JsonTranscoding/test/testassets/Sandbox/Controllers/ValuesController.cs:27)
      GET /api/Values  (src/Grpc/JsonTranscoding/test/testassets/Sandbox/Controllers/ValuesController.cs:20)
      GET /api/Values  (src/Grpc/JsonTranscoding/test/testassets/Sandbox/Controllers/ValuesController.cs:13)
      GET /WeatherForecast  (src/ProjectTemplates/Web.ProjectTemplates/content/WebApi-CSharp/Controllers/WeatherForecastController.cs:93)
      GET /api/XmlApiControllerBase/ActionReturningValidationDetailsWithMetadata  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/XmlApiControllerBase.cs:36)
      GET /api/XmlApiControllerBase/ActionReturningValidationProblem  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/XmlApiControllerBase.cs:32)
      GET /api/XmlApiControllerBase/ActionReturningProblemDetails  → ControllerBase.NotFound  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/XmlApiControllerBase.cs:17)
      GET /api/XmlApiControllerBase/ActionReturningClientErrorStatusCodeResult  → ControllerBase.NotFound  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/XmlApiControllerBase.cs:13)
      POST /SerializableError  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/SerializableErrorController.cs:37)
      POST /SerializableError  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/SerializableErrorController.cs:31)
      GET /SerializableError  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/SerializableErrorController.cs:11)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/XmlFormattersWebSite/Controllers/HomeController.cs:12)
      GET /Tickets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/TicketsController.cs:26)
      GET /Tickets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/TicketsController.cs:20)
      POST /Pets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/PetsController.cs:44)
      POST /Pets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/PetsController.cs:38)
      GET /Pets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/PetsController.cs:32)
      GET /Pets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/PetsController.cs:26)
      GET /Pets  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/PetsController.cs:19)
      DELETE /Movies  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/MoviesController.cs:47)
      PUT /Movies  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/MoviesController.cs:41)
      POST /Movies  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/MoviesController.cs:35)
      GET /Movies  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/MoviesController.cs:29)
      GET /Movies  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/MoviesController.cs:23)
      DELETE /ItemsV2  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsV2Controller.cs:43)
      PUT /ItemsV2  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsV2Controller.cs:37)
      GET /ItemsV2  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsV2Controller.cs:25)
      DELETE /Items/{id}  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsController.cs:45)
      PUT /Items/{id}  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsController.cs:39)
      POST /Items/{id}  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsController.cs:33)
      GET /Items/{id}  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsController.cs:27)
      GET /Items/{id}  (src/Mvc/test/WebSites/VersioningWebSite/Controllers/ItemsController.cs:21)
      POST /Employee  → Controller.View  (src/Mvc/test/WebSites/TagHelpersWebSite/Controllers/EmployeeController.cs:24)
      POST /Login  (src/Mvc/test/WebSites/SecurityWebSite/Controllers/LoginController.cs:31)
      POST /Login  (src/Mvc/test/WebSites/SecurityWebSite/Controllers/LoginController.cs:23)
      POST /Login  (src/Mvc/test/WebSites/SecurityWebSite/Controllers/LoginController.cs:15)
      POST /IgnoreAntiforgery  → ControllerBase.Content  (src/Mvc/test/WebSites/SecurityWebSite/Controllers/IgnoreAntiforgeryController.cs:11)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/SecurityWebSite/Controllers/HomeController.cs:16)
      POST /Antiforgery  → Controller.View  (src/Mvc/test/WebSites/SecurityWebSite/Controllers/AntiforgeryController.cs:11)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:66)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:60)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:54)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:48)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:42)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:36)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:30)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:24)
      GET /Teams  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/TeamController.cs:18)
      GET /Store  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/StoreController.cs:37)
      GET /Store/Store/Shop/Orders  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/StoreController.cs:31)
      GET /Store  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/StoreController.cs:25)
      GET /Store  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/StoreController.cs:18)
      GET /RouteData/RouteData/Attribute  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/RouteDataController.cs:17)
      GET /ParameterTransformer/MyAction  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/ParameterTransformerController.cs:18)
      GET /PageParameter  → ControllerBase.Content  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/PageParameterController.cs:17)
      PUT /Order/Edit/{orderId?}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/OrderController.cs:30)
      POST /Order/Add/{orderId?}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/OrderController.cs:24)
      GET /Order/Add/{orderId?}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/OrderController.cs:18)
      PUT /api/v1/Maps  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/MapsController.cs:39)
      POST /api/v1/Maps  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/MapsController.cs:31)
      GET /api/v1/Maps  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/MapsController.cs:19)
      GET /LinkParser/some-path/{x}/{y}/{z?}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/LinkParserController.cs:40)
      GET /Items/IndexWithArgument/{arg}  → ControllerBase.Ok  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/ItemsController.cs:21)
      GET /Items/IndexWithSelectiveFilter  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/ItemsController.cs:16)
      GET /Items/Index  → ControllerBase.Ok  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/ItemsController.cs:11)
      DELETE /Friends  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/FriendsController.cs:25)
      GET /Friends  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/FriendsController.cs:18)
      GET /{controller:slugify}/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EndpointRoutingController.cs:30)
      GET /{controller:slugify}/{action}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EndpointRoutingController.cs:24)
      GET /{controller}/{action=Index}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EndpointRoutingController.cs:18)
      GET /EndpointName/LinkToAttributeRouted/{path?}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EndpointNameController.cs:19)
      GET /api/Employee/{id}/Salary  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EmployeeController.cs:67)
      DELETE /api/Employee  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EmployeeController.cs:61)
      GET /api/Employee  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EmployeeController.cs:55)
      GET /api/Employee  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EmployeeController.cs:49)
      GET /api/Employee  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/EmployeeController.cs:43)
      GET /DynamicOrder  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/DynamicOrderController.cs:23)
      GET /DynamicOrder  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/DynamicOrderController.cs:17)
      POST /ConsumesAttribute/Json  → ControllerBase.Content  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/ConsumesAttributeController.cs:11)
      GET /api/Company/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/CompanyController.cs:57)
      GET /api/Company/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/CompanyController.cs:49)
      DELETE /api/Company/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/CompanyController.cs:41)
      PUT /api/Company/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/CompanyController.cs:32)
      GET /api/Company/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/CompanyController.cs:22)
      GET /Branches  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/BranchesController.cs:22)
      POST /Banks/Bank/Withdraw/{id}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/BanksController.cs:45)
      GET /Banks/Bank/Deposit/{amount}  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/BanksController.cs:37)
      GET /Banks/Bank/Deposit  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/BanksController.cs:37)
      PATCH /Banks  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/BanksController.cs:26)
      GET /Banks  (src/Mvc/test/WebSites/RoutingWebSite/Controllers/BanksController.cs:17)
      GET /ViewWithPaths  → Controller.View  (src/Mvc/test/WebSites/RazorWebSite/Controllers/ViewWithPathsController.cs:10)
      GET /Redirect  → ControllerBase.RedirectToPage  (src/Mvc/test/WebSites/RazorPagesWebSite/Controllers/RedirectController.cs:10)
      GET /ClientValidationDisabled  → Controller.View  (src/Mvc/test/WebSites/RazorPagesWebSite/Controllers/ClientValidationDisabledController.cs:10)
      GET /AuthorizedAction/Index  → ControllerBase.Ok  (src/Mvc/test/WebSites/RazorPagesWebSite/Controllers/AuthorizedActionController.cs:13)
      POST /UpdateableViews  → UpdateableFileProvider.CancelRazorPages  (src/Mvc/test/WebSites/RazorBuildWebSite/Controllers/UpdateableViewsController.cs:19)
      POST /UpdateableViews  → UpdateableFileProvider.CancelRazorPages  (src/Mvc/test/WebSites/RazorBuildWebSite/Controllers/UpdateableViewsController.cs:12)
      POST /HtmlGeneration_Home  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/HtmlGeneration_HomeController.cs:241)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:85)
      POST /Catalog_CacheTagHelper  → ProductsService.UpdateProducts  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:75)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:66)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:55)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:45)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:37)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:30)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:23)
      GET /Catalog_CacheTagHelper  → Controller.View  (src/Mvc/test/WebSites/HtmlGenerationWebSite/Controllers/Catalog_CacheTagHelperController.cs:12)
      PUT /Testing  → ControllerBase.RedirectToActionPermanentPreserveMethod  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:139)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:129)
      POST /Testing  → RequestCookieCollection.ContainsKey  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:109)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:99)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:75)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:72)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:66)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:53)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:40)
      POST /Testing  → RequestCookieCollection.ContainsKey  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:22)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/GenericHostWebSite/Controllers/TestingController.cs:19)
      POST /XmlSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/XmlSerializerController.cs:54)
      POST /XmlSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/XmlSerializerController.cs:48)
      POST /XmlSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/XmlSerializerController.cs:39)
      POST /XmlSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/XmlSerializerController.cs:29)
      POST /XmlSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/XmlSerializerController.cs:23)
      POST /Validation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/ValidationController.cs:88)
      POST /Validation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/ValidationController.cs:82)
      POST /Validation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/ValidationController.cs:71)
      POST /Validation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/ValidationController.cs:44)
      POST /Validation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/ValidationController.cs:25)
      POST /Validation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/ValidationController.cs:11)
      POST /Unions/EchoEnvelope  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:207)
      POST /Unions/EchoPetWithClassifier  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:204)
      POST /Unions/EchoPet  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:201)
      POST /Unions/EchoNullableIntStringWithClassifier  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:198)
      POST /Unions/EchoNullableIntString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:195)
      POST /Unions/EchoIntShortWithClassifier  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:192)
      POST /Unions/EchoIntShort  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:189)
      POST /Unions/EchoIntStringImplicit  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:186)
      POST /Unions/EchoIntStringWithClassifier  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:183)
      POST /Unions/EchoIntString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:180)
      POST /Unions/EchoBoolString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:177)
      GET /Unions/Envelope  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:163)
      GET /Unions/Nested  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:150)
      GET /Unions/ObjectCase  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:138)
      GET /Unions/UnionWithNullableCase  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:126)
      GET /Unions/NullableWrapper  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:114)
      GET /Unions/AsyncValueTask  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:107)
      GET /Unions/AsyncTask  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:104)
      GET /Unions/PrimitiveCharInt  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:92)
      GET /Unions/PrimitiveDateTimeInt  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:84)
      GET /Unions/PrimitiveGuidInt  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:76)
      GET /Unions/PrimitiveBoolString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:68)
      GET /Unions/PrimitiveDoubleString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:60)
      GET /Unions/PrimitiveDecimalString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:52)
      GET /Unions/PrimitiveLongString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:44)
      GET /Unions/PrimitiveIntString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:36)
      GET /Unions/PrimitiveShortString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:28)
      GET /Unions/PrimitiveByteString  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/UnionsController.cs:20)
      POST /TestApi/PostBookWithNoValidation  → ControllerBase.Ok  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/TestApiController.cs:13)
      GET /SystemTextJsonOutputFormatter/AsyncEnumerable  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/SystemTextJsonOutputFormatterController.cs:22)
      GET /SystemTextJsonOutputFormatter/PolymorphicResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/SystemTextJsonOutputFormatterController.cs:14)
      GET /Stream  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/StreamController.cs:37)
      GET /Stream  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/StreamController.cs:30)
      GET /Stream  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/StreamController.cs:23)
      GET /Stream  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/StreamController.cs:16)
      GET /Stream  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/StreamController.cs:10)
      POST /SerializableError  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/SerializableErrorController.cs:10)
      GET /RespectBrowserAcceptHeader  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/RespectBrowserAcceptHeaderController.cs:10)
      GET /jsonpatch/CreateProduct  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonPatchController.cs:31)
      PATCH /jsonpatch/PatchProduct  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonPatchController.cs:12)
      GET /JsonOutputFormatter/ProblemDetailsResult  → ControllerBase.NotFound  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:62)
      GET /JsonOutputFormatter/PolymorphicResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:54)
      GET /JsonOutputFormatter/LargeObjectResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:46)
      GET /JsonOutputFormatter/DictionaryResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:37)
      GET /JsonOutputFormatter/CollectionModelResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:29)
      GET /JsonOutputFormatter/SimpleModelResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:25)
      GET /JsonOutputFormatter/StringWithNonAsciiContent  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:22)
      GET /JsonOutputFormatter/StringWithUnicodeResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:19)
      GET /JsonOutputFormatter/StringResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:16)
      GET /JsonOutputFormatter/IntResult  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonOutputFormatterController.cs:13)
      POST /JsonFormatter  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonFormatterController.cs:113)
      POST /JsonFormatter  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonFormatterController.cs:88)
      POST /JsonFormatter  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonFormatterController.cs:82)
      POST /JsonFormatter  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonFormatterController.cs:71)
      POST /JsonFormatter  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonFormatterController.cs:60)
      POST /JsonFormatter  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/JsonFormatterController.cs:48)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:58)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:54)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:49)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:45)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:41)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:31)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:25)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:19)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/HomeController.cs:13)
      POST /DoNotRespectBrowserAcceptHeader  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/DoNotRespectBrowserAcceptHeaderController.cs:31)
      GET /DoNotRespectBrowserAcceptHeader  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/DoNotRespectBrowserAcceptHeaderController.cs:20)
      GET /DoNotRespectBrowserAcceptHeader  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/DoNotRespectBrowserAcceptHeaderController.cs:10)
      POST /DataContractSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/DataContractSerializerController.cs:43)
      POST /DataContractSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/DataContractSerializerController.cs:35)
      POST /DataContractSerializer  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/DataContractSerializerController.cs:27)
      GET /{controller}/{action}  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/AsyncEnumerableController.cs:23)
      GET /{controller}/{action}  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/AsyncEnumerableController.cs:16)
      GET /{controller}/{action}  (src/Mvc/test/WebSites/FormatterWebSite/Controllers/AsyncEnumerableController.cs:12)
      POST /UploadFiles  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/FilesWebSite/Controllers/UploadFilesController.cs:24)
      POST /UploadFiles  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/FilesWebSite/Controllers/UploadFilesController.cs:11)
      GET /api/store/ActionWithDifferentCorsPolicy  (src/Mvc/test/WebSites/CorsWebSite/Controllers/StoreController.cs:38)
      GET /api/store/ActionWithCorsDisabled  (src/Mvc/test/WebSites/CorsWebSite/Controllers/StoreController.cs:29)
      GET /api/store/ActionWithCorsSettings  (src/Mvc/test/WebSites/CorsWebSite/Controllers/StoreController.cs:20)
      GET /api/store/ActionUsingControllerCorsSettings  (src/Mvc/test/WebSites/CorsWebSite/Controllers/StoreController.cs:14)
      POST /NonCors/Post  (src/Mvc/test/WebSites/CorsWebSite/Controllers/CustomerController.cs:17)
      GET /NonCors/GetOptions  (src/Mvc/test/WebSites/CorsWebSite/Controllers/CustomerController.cs:11)
      GET /Cors/EditUserComment  (src/Mvc/test/WebSites/CorsWebSite/Controllers/BlogController.cs:31)
      GET /Cors/GetExclusiveContent  (src/Mvc/test/WebSites/CorsWebSite/Controllers/BlogController.cs:24)
      GET /Cors/GetUserComments  (src/Mvc/test/WebSites/CorsWebSite/Controllers/BlogController.cs:18)
      GET /Cors/GetBlogComments  (src/Mvc/test/WebSites/CorsWebSite/Controllers/BlogController.cs:13)
      PUT /Testing  → ControllerBase.RedirectToActionPermanentPreserveMethod  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:153)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:143)
      POST /Testing  → RequestCookieCollection.ContainsKey  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:123)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:113)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:89)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:86)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:80)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:67)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:60)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:54)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:41)
      POST /Testing  → RequestCookieCollection.ContainsKey  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:23)
      GET /Testing  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TestingController.cs:20)
      POST /TempDataProperty  → ControllerBase.RedirectToAction  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataPropertyController.cs:25)
      POST /TempDataProperty  → ControllerBase.RedirectToAction  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataPropertyController.cs:17)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:119)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:113)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:105)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:99)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:92)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:86)
      GET /TempData  → ResponseCookiesWrapper.GrantConsent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/TempDataController.cs:80)
      POST /RequestSizeLimit  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestSizeLimitController.cs:24)
      POST /RequestSizeLimit  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestSizeLimitController.cs:12)
      GET /RequestScopedService/FromProperty  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:52)
      GET /RequestScopedService/FromActionArgument  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:43)
      GET /RequestScopedService/FromViewComponent  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:37)
      GET /RequestScopedService/FromTagHelper  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:31)
      GET /RequestScopedService/FromView  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:25)
      GET /RequestScopedService/FromFilter  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:19)
      GET /RequestScopedService/FromConstraint  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestScopedServiceController.cs:12)
      POST /RequestFormLimits  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestFormLimitsController.cs:46)
      POST /RequestFormLimits  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestFormLimitsController.cs:35)
      POST /RequestFormLimits  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestFormLimitsController.cs:24)
      POST /RequestFormLimits  → ControllerBase.BadRequest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RequestFormLimitsController.cs:12)
      GET /RemoteAttribute_Verify/RemoteAttribute_Verify/IsIdAvailable  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RemoteAttribute_VerifyController.cs:12)
      GET /RemoteAttribute_Home/Details  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RemoteAttribute_HomeController.cs:32)
      POST /RemoteAttribute_Home/Create  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RemoteAttribute_HomeController.cs:20)
      GET /RemoteAttribute_Home/Create  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RemoteAttribute_HomeController.cs:14)
      POST /ReadFromThrowingRequestBody  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ReadFromThrowingRequestBodyController .cs:14)
      POST /ReadFromThrowingRequestBody  → ControllerBase.ValidationProblem  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ReadFromThrowingRequestBodyController .cs:10)
      GET /RazorComponents  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RazorComponentsController.cs:68)
      GET /RazorComponents  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RazorComponentsController.cs:61)
      GET /RazorComponents  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/RazorComponentsController.cs:49)
      GET /PageRoute  (src/Mvc/test/WebSites/BasicWebSite/Controllers/PageRouteController.cs:36)
      GET /PageRoute  (src/Mvc/test/WebSites/BasicWebSite/Controllers/PageRouteController.cs:24)
      POST /NonNullable  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/NonNullableController.cs:16)
      GET /api/NonNullable  (src/Mvc/test/WebSites/BasicWebSite/Controllers/NonNullableApiController.cs:14)
      GET /Home  (src/Mvc/test/WebSites/BasicWebSite/Controllers/HomeController.cs:127)
      POST /Home  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/HomeController.cs:116)
      GET /Home  (src/Mvc/test/WebSites/BasicWebSite/Controllers/HomeController.cs:110)
      GET /Filters/{culture}/Filters/MiddlewareFilterTest  (src/Mvc/test/WebSites/BasicWebSite/Controllers/FiltersController.cs:24)
      POST /Filters  (src/Mvc/test/WebSites/BasicWebSite/Controllers/FiltersController.cs:12)
      GET /DefaultValues/EchoValue_DefaultParameterValue_ForGlobbedPath/{**path}  (src/Mvc/test/WebSites/BasicWebSite/Controllers/DefaultValuesController.cs:31)
      GET /DefaultValues  (src/Mvc/test/WebSites/BasicWebSite/Controllers/DefaultValuesController.cs:23)
      GET /DefaultValues  (src/Mvc/test/WebSites/BasicWebSite/Controllers/DefaultValuesController.cs:17)
      GET /DefaultValues  (src/Mvc/test/WebSites/BasicWebSite/Controllers/DefaultValuesController.cs:11)
      GET /CustomValueProvider  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomValueProviderController.cs:23)
      GET /CustomValueProvider  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomValueProviderController.cs:19)
      GET /CustomValueProvider  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomValueProviderController.cs:15)
      GET /CustomValueProvider  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomValueProviderController.cs:11)
      GET /services  → DefaultCustomService.Process  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomServiceApiController.cs:50)
      GET /services  → DefaultCustomService.Process  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomServiceApiController.cs:40)
      GET /services  → DefaultCustomService.Process  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomServiceApiController.cs:32)
      GET /services  → DefaultCustomService.Process  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomServiceApiController.cs:24)
      GET /services  → DefaultCustomService.Process  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomServiceApiController.cs:18)
      GET /services  → DefaultCustomService.Process  (src/Mvc/test/WebSites/BasicWebSite/Controllers/CustomServiceApiController.cs:12)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:139)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:135)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:117)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:100)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:94)
      POST /contact  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:90)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:86)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:79)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:72)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:66)
      POST /contact  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:55)
      POST /contact  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:51)
      POST /contact  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:48)
      POST /contact  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:40)
      POST /contact  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:33)
      GET /contact  → Results.NoContent  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ContactApiController.cs:21)
      GET /AsyncDisposable  → ControllerBase.Ok  (src/Mvc/test/WebSites/BasicWebSite/Controllers/AsyncDisposableController.cs:23)
      GET /Antiforgery  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/AntiforgeryController.cs:71)
      POST /Antiforgery  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/AntiforgeryController.cs:63)
      POST /Antiforgery  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/AntiforgeryController.cs:44)
      POST /Antiforgery  → Controller.View  (src/Mvc/test/WebSites/BasicWebSite/Controllers/AntiforgeryController.cs:35)
      GET /ActionResultOfT  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionResultOfTController.cs:22)
      GET /ActionResultOfT  (src/Mvc/test/WebSites/BasicWebSite/Controllers/ActionResultOfTController.cs:11)
      GET /License  (src/Mvc/test/WebSites/ApplicationModelWebSite/Controllers/LicenseController.cs:10)
      GET /Home  (src/Mvc/test/WebSites/ApplicationModelWebSite/Controllers/HomeController.cs:37)
      GET /Home  (src/Mvc/test/WebSites/ApplicationModelWebSite/Controllers/HomeController.cs:34)
      GET /Home  (src/Mvc/test/WebSites/ApplicationModelWebSite/Controllers/HomeController.cs:30)
      GET /Home  (src/Mvc/test/WebSites/ApplicationModelWebSite/Controllers/HomeController.cs:22)
      GET /Home  (src/Mvc/test/WebSites/ApplicationModelWebSite/Controllers/HomeController.cs:16)
      GET /ApiExplorerWithTypedResult/GetProduct  → TypedResults.Ok  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerWithTypedResultController.cs:12)
      GET /ApiExplorerVisibilitySetExplicitly  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerVisibilitySetExplicitlyController.cs:18)
      GET /ApiExplorerVisibilitySetExplicitly  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerVisibilitySetExplicitlyController.cs:12)
      GET /ApiExplorerVisibilityEnabledByConvention  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerVisibilityEnabledByConventionController.cs:11)
      GET /ApiExplorerVisibilityDisabledByConvention  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerVisibilityDisabledByConventionController.cs:11)
      GET /ApiExplorerVoid/ActionWithNoExplicitType  → ControllerBase.Ok  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerSystemVoid.cs:16)
      GET /ApiExplorerVoid/ActionWithVoidType  → ControllerBase.Ok  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerSystemVoid.cs:13)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetTaskOfInt  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:79)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetTaskOfProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:73)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetTaskOfDerivedActionResult  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:67)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetTaskOfIActionResult  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:61)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetTaskOfObject  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:55)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetTask  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:49)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetInt  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:43)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetActionResultProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:40)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:34)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetDerivedActionResult  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:28)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetIActionResult  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:22)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetObject  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:16)
      GET /ApiExplorerResponseTypeWithoutAttribute/GetVoid  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithoutAttributeController.cs:11)
      GET /ApiExplorerResponseTypeWithAttribute/UpdateProductWithLimitedResponseContentTypes  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:80)
      GET /ApiExplorerResponseTypeWithAttribute/UpdateProductWithDefaultResponseContentTypes  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:73)
      GET /ApiExplorerResponseTypeWithAttribute/CreateProductWithLimitedResponseContentTypes  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:65)
      GET /ApiExplorerResponseTypeWithAttribute/CreateProductWithDefaultResponseContentTypes  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:58)
      GET /ApiExplorerResponseTypeWithAttribute/GetProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:51)
      GET /ApiExplorerResponseTypeWithAttribute/GetTask  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:44)
      GET /ApiExplorerResponseTypeWithAttribute/GetTaskWithExplicitResponseTypeStatusCode  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:37)
      GET /ApiExplorerResponseTypeWithAttribute/GetIActionResult  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:30)
      GET /ApiExplorerResponseTypeWithAttribute/GetObject  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:23)
      GET /ApiExplorerResponseTypeWithAttribute/GetVoid  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:17)
      GET /ApiExplorerResponseTypeWithAttribute/GetVoidWithExplicitResponseTypeStatusCode  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithAttributeController.cs:11)
      POST /ApiExplorerResponseTypeWithApiConventionController/PostItem  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:41)
      DELETE /ApiExplorerResponseTypeWithApiConventionController/DeleteProductAsync  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:38)
      PUT /ApiExplorerResponseTypeWithApiConventionController/Put  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:35)
      POST /ApiExplorerResponseTypeWithApiConventionController/PostTaskOfProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:32)
      POST /ApiExplorerResponseTypeWithApiConventionController/PostWithProduces  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:28)
      POST /ApiExplorerResponseTypeWithApiConventionController/PostWithConventions  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:22)
      GET /ApiExplorerResponseTypeWithApiConventionController/GetProducts  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:19)
      GET /ApiExplorerResponseTypeWithApiConventionController/GetTaskOfActionResultOfProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:16)
      GET /ApiExplorerResponseTypeWithApiConventionController/GetProduct  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeWithApiConventionController.cs:13)
      GET /ApiExplorerResponseTypeOverrideOnAction  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeOverrideOnActionController.cs:26)
      GET /ApiExplorerResponseTypeOverrideOnAction  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeOverrideOnActionController.cs:18)
      GET /ApiExplorerResponseTypeOverrideOnAction  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseTypeOverrideOnActionController.cs:13)
      GET /ApiExplorerResponseContentTypeOverrideOnAction  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseContentTypeOverrideOnActionController.cs:18)
      GET /ApiExplorerResponseContentTypeOverrideOnAction  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseContentTypeOverrideOnActionController.cs:12)
      GET /ApiExplorerResponseContentType/NoMatch  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseContentTypeController.cs:31)
      GET /ApiExplorerResponseContentType/WildcardMatch  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseContentTypeController.cs:24)
      GET /ApiExplorerResponseContentType/Specific  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseContentTypeController.cs:17)
      GET /ApiExplorerResponseContentType/Unset  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerResponseContentTypeController.cs:11)
      GET /ApiExplorerReload/Reload  → ControllerBase.Ok  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerReloadableController.cs:15)
      GET /ApiExplorerReload/Index  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerReloadableController.cs:11)
      GET /ApiExplorerParameters/IsRequiredParameters  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:37)
      GET /ApiExplorerParameters/DefaultValueParameters  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:33)
      GET /ApiExplorerParameters/ComplexModel  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:29)
      GET /ApiExplorerParameters/SimpleModelFromBody/{id}  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:24)
      GET /ApiExplorerParameters/SimpleModel  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:20)
      GET /ApiExplorerParameters/SimpleParametersWithBinderMetadata  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:16)
      GET /ApiExplorerParameters/SimpleParameters  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerParametersController.cs:12)
      GET /ApiExplorerNameSetExplicitly  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerNameSetExplicitlyController.cs:17)
      GET /ApiExplorerNameSetExplicitly  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerNameSetExplicitlyController.cs:12)
      GET /ApiExplorerNameSetByConvention  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerNameSetByConventionController.cs:11)
      GET /ApiExplorerInboundOutBound  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerInboundOutboundController.cs:15)
      GET /ApiExplorerInboundOutBound  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerInboundOutboundController.cs:10)
      GET /ApiExplorerHttpMethod  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerHttpMethodController.cs:26)
      GET /ApiExplorerHttpMethod  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerHttpMethodController.cs:16)
      GET /ApiExplorerHttpMethod/All  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerHttpMethodController.cs:11)
      GET /ApiExplorerApiController/ProducesWithUnsupportedContentType  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerApiController.cs:32)
      GET /ApiExplorerApiController/ActionWithFormFileCollectionParameter  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerApiController.cs:28)
      GET /ApiExplorerApiController/ActionWithIdSuffixParameter  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerApiController.cs:24)
      GET /ApiExplorerApiController/ActionWithIdParameter  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerApiController.cs:20)
      GET /ApiExplorerApiController/ActionWithSomeParameters  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerApiController.cs:15)
      GET /ApiExplorerApiController/ActionWithoutParameters  → ControllerBase.Ok  (src/Mvc/test/WebSites/ApiExplorerWebSite/Controllers/ApiExplorerApiController.cs:13)
      GET /api/User  → ClaimActionCollection.Where  (src/Components/test/testassets/Components.TestServer/Controllers/UserController.cs:20)
      GET /Reload  → ControllerBase.Ok  (src/Components/test/testassets/Components.TestServer/Controllers/ReloadController.cs:12)
      DELETE /api/Person  (src/Components/test/testassets/Components.TestServer/Controllers/PersonController.cs:48)
      PUT /api/Person  (src/Components/test/testassets/Components.TestServer/Controllers/PersonController.cs:41)
      GET /api/Person  → Request.ToString  (src/Components/test/testassets/Components.TestServer/Controllers/PersonController.cs:34)
      POST /api/Person  (src/Components/test/testassets/Components.TestServer/Controllers/PersonController.cs:24)
      GET /api/Person  → Request.ToString  (src/Components/test/testassets/Components.TestServer/Controllers/PersonController.cs:16)
      GET /api/Greeting/SayHello  (src/Components/test/testassets/Components.TestServer/Controllers/GreetingController.cs:11)
      GET /Download  (src/Components/test/testassets/Components.TestServer/Controllers/DownloadController.cs:12)
      POST /api/Data  → Request.CopyToAsync  (src/Components/test/testassets/Components.TestServer/Controllers/DataController.cs:27)
      GET /api/Data  → ControllerBase.File  (src/Components/test/testassets/Components.TestServer/Controllers/DataController.cs:14)
      GET /Culture/SetCulture  → CookieRequestCultureProvider.MakeCookieValue  (src/Components/test/testassets/Components.TestServer/Controllers/CultureController.cs:13)
      GET /api/Cookie/Increment  → RequestCookieCollection.TryGetValue  (src/Components/test/testassets/Components.TestServer/Controllers/CookieController.cs:22)
      GET /api/Cookie/Reset  (src/Components/test/testassets/Components.TestServer/Controllers/CookieController.cs:16)
      GET /My  (src/Mvc/test/WebSites/SimpleWebSiteWithWebApplicationBuilder/Program.cs:79)
      GET /ErrorPageMiddleware  (src/Mvc/test/WebSites/ErrorPageMiddlewareWebSite/ErrorPageMiddlewareController.cs:32)
      GET /ErrorPageMiddleware  (src/Mvc/test/WebSites/ErrorPageMiddlewareWebSite/ErrorPageMiddlewareController.cs:29)
      GET /ErrorPageMiddleware  (src/Mvc/test/WebSites/ErrorPageMiddlewareWebSite/ErrorPageMiddlewareController.cs:23)
      GET /ErrorPageMiddleware  (src/Mvc/test/WebSites/ErrorPageMiddlewareWebSite/ErrorPageMiddlewareController.cs:17)
      GET /ErrorPageMiddleware  (src/Mvc/test/WebSites/ErrorPageMiddlewareWebSite/ErrorPageMiddlewareController.cs:11)
      GET /AggregateException  → Controller.View  (src/Mvc/test/WebSites/ErrorPageMiddlewareWebSite/AggregateExceptionController.cs:10)
      GET /NotInServices  → Controller.View  (src/Mvc/test/WebSites/ControllersFromServicesWebSite/NotInServicesController.cs:10)
      GET /Another  → Controller.View  (src/Mvc/test/WebSites/ControllersFromServicesWebSite/AnotherController.cs:23)
      GET /Another  → Controller.View  (src/Mvc/test/WebSites/ControllersFromServicesWebSite/AnotherController.cs:17)
      GET /Another  → Controller.View  (src/Mvc/test/WebSites/ControllersFromServicesWebSite/AnotherController.cs:11)
      GET /Nested  (src/Mvc/test/WebSites/ControllersFromServicesClassLibrary/NestedControllerOwner.cs:12)
      GET /Generic  (src/Mvc/test/WebSites/ControllersFromServicesClassLibrary/GenericController.cs:10)
      POST /EmployeeRecords  → ControllerBase.Content  (src/Mvc/test/WebSites/ControllersFromServicesClassLibrary/EmployeeRecords.cs:16)
      PUT /EmployeeRecords  → ControllerBase.Content  (src/Mvc/test/WebSites/ControllersFromServicesClassLibrary/EmployeeRecords.cs:10)
   Background (1)
      Worker  (src/ProjectTemplates/Web.ProjectTemplates/content/Worker-CSharp/Program.cs:4)

PACKAGES
   Web/API:  Microsoft.AspNetCore.AspNetCoreModuleV2 2.2.0, Microsoft.AspNetCore.AzureAppServices.SiteExtension.$(AspNetCoreMajorMinorVersion).x64 %(_ResolvedPackageVersionInfo.PackageVersion), Microsoft.AspNetCore.AzureAppServices.SiteExtension.$(AspNetCoreMajorMinorVersion).x86 %(_ResolvedPackageVersionInfo.PackageVersion), Microsoft.AspNetCore.Hosting 2.2.0, Microsoft.AspNetCore.HttpsPolicy 2.2.0, Microsoft.AspNetCore.ResponseCompression 2.2.0, Microsoft.AspNetCore.Server.IIS 2.2.6, Microsoft.AspNetCore.Server.IISIntegration 2.2.0 … (10 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.Design 2.1.1, Microsoft.EntityFrameworkCore.Sqlite 2.1.1, Microsoft.EntityFrameworkCore.SqlServer 2.1.1, MySqlConnector 0.43.0, Npgsql.EntityFrameworkCore.PostgreSQL 2.1.1.1, Pomelo.EntityFrameworkCore.MySql 2.1.1
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
   Other:  @(PackageReference), $(HostingStartupPackageName), Drop.App, Microsoft.Build.Framework, Microsoft.Build.Tasks.Core, Microsoft.Build.Utilities.Core, Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp … (19 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
