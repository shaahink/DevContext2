# REPORT
**MVCCore**

Style: SampleCollection
_2 projects  ·  3 HttpEndpoint  ·  net10.0, net452 + swagger + controllers + desktop-ui + minimal-apis + identity + efcore + razor-pages + grpc + signalr_

## Stats

| Metric | Value |
|--------|-------|
| Files | 5571 |
| Projects | 622 |
| Nodes | 1262 |
| Edges | 717 |
| Entries | 3 |
| With target | 3/3 |
| Deep spine (>=2) | 3/3 (100%) |
| Verified edges | 34% |
| Analyzed in | 87.6s |

## Top Flows

1. **POST /Students** → `StudentsController` *(HttpEndpoint)*
2. **POST /Students** → `StudentsController` *(HttpEndpoint)*
3. **POST /Students** → `StudentsController` *(HttpEndpoint)*

### Trace 1: POST /Students [Edit]

TRACE  POST /Students [Edit]
       aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81
       MVCCore
▸ ENTRY  POST /Students [Edit]  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)
   └─ call StudentsController.Edit  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)
          public async Task<IActionResult> Edit(int? id)
          if (id == null)
          return NotFound();
      ├─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:97) [approx]
      └─ call StudentsController.StudentExists  (aspnetcore/data/ef-mvc/intro/samples/5cu/Controllers/StudentsController.cs:104) [verified]
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: POST /Students [Create]

TRACE  POST /Students [Create]
       aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49
       MVCCore
▸ ENTRY  POST /Students [Create]  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49)
   └─ call StudentsController.Create  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49)
          public IActionResult Create()
          return View();
      └─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:59) [approx]
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 3: POST /Students

TRACE  POST /Students
       aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118
       MVCCore
▸ ENTRY  POST /Students  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
   └─ call StudentsController.DeleteConfirmed  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
          [HttpPost, ActionName("Delete")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> DeleteConfirmed(int id)
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

## Insights

_5 info · 5 notable_

### **NOTABLE**: Auth present via app-wide default policy — 3 endpoints not individually verifiable
*(Risk)*

- POST /Students
- POST /Students
- POST /Students

### **NOTABLE**: Config without defaults: 8 consumed keys have no appsettings default
*(Risk)*

- Authentication:Twitter:ConsumerAPIKey
- Authentication:Twitter:ConsumerSecret
- AzureADManagedIdentityClientId
- DefaultConnection
- MovieContext

### **NOTABLE**: Internal hubs: 1 heavily-referenced internal types
*(Topology)*

- ForwardingLogger (387 refs)

### **NOTABLE**: Extension seats: IAuthorizationHandler (7 impls) · IQuoteService (2 impls) · IEmailSender (2 impls)
*(Wiring)*

- IAuthorizationHandler (7 impls)
- IQuoteService (2 impls)
- IEmailSender (2 impls)

### **NOTABLE**: Multi-implementation interfaces: IAuthorizationHandler (7 impls) · IQuoteService (2 impls) · IEmailSender (2 impls)
*(Wiring)*

- IAuthorizationHandler (7 impls)
- IQuoteService (2 impls)
- IEmailSender (2 impls)

### _INFO_: Public surface: 46 interfaces, 2215 classes (2274 total public types)
*(Shape)*

- 46 interfaces
- 2215 classes

### _INFO_: Entry targets resolved 3/3 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 2080 Extension · 115 Singleton · 87 Transient · 32 Scoped (2314 total)
*(Wiring)*

### _INFO_: Most depended-upon: MySharedApp (2 dependents) · RazorClassLib (2 dependents) · DependentLibrary (1 dependents)
*(Topology)*

- MySharedApp (2 dependents)
- RazorClassLib (2 dependents)
- DependentLibrary (1 dependents)

### _INFO_: Data map: 15 entities across 2 scopes
*(Data)*

- MVCCore (3 entities)
- SchoolContext (1 entities)

LIBRARY  MVCCore     (295 public types)

ENTRY API
   register  MyCustomMiddlewareExtensions.UseMyCustomMiddleware   (MyCustomMiddleware.cs)
   register  RequestCultureMiddlewareExtensions.UseRequestCulture   (RequestCultureMiddlewareExtensions.cs)
   derive    ABCEndpointFilters   (AbcEndpointFilters.cs)
   derive    HostPageModel   (HostPageModel.cs)
   implement IDateTime   (IDateTime.cs)
   derive    Person   (BackgroundQueueService.cs)
   extend    ResultsExtensions   (ResultsExtensions.cs)

ABSTRACTIONS
   ABCEndpointFilters (class)  — 3 implementors
   HostPageModel (class)  — 2 implementors
   IDateTime (interface)  — 2 implementors
   Person (class)  — 2 implementors
   ApiException (class)  — 1 implementor
   GreeterBase (class)  — 1 implementor
   IMessageWriter (interface)  — 1 implementor
   IToDoItemRepository (interface)  — 1 implementor

PUBLIC SURFACE
   APIWithControllers
      Program (class):  Main
      WeatherForecast (class)
   APIWithControllers.Controllers
      WeatherForecastController (class):  Get, WeatherForecastController
   APIforSPA
      Email (record)
      EmailSender (class):  EmailSender, SendEmailAsync
      Program (class):  Main
      WeatherForecast (class)
   AddDefaultIdentity
      StartupRemove (class):  Configure, ConfigureServices, StartupRemove
   AuthorizationMiddlewareResultHandlerSample
      Startup (class):  Configure, ConfigureServices, Startup
   BackgroundQueueService
      BackgroundQueue (class):  BackgroundQueue
      Person (class)
   BindTryParseAPI.Controllers
      WeatherForecastController (class):  Get, GetByLocaleRange, GetByRange
   BindTryParseAPI.Models
      DateRange (class):  Parse, TryParse
      Locale (class):  Locale, Parse, TryParse
      WeatherForecast (class)
      WeatherForecastViewModel (class)
   BindTryParseMVC
      LDR (class):  LocaleDateRange
   BindTryParseMVC.Controllers
      WeatherForecastController (class):  ByRange, ByRangeTP, GenRange, Index, LocalGenRange, RangeByLocale
   BindTryParseMVC.Models
      DateRange (class):  Parse, TryParse
      DateRangeTP (class):  DateRangeTP, TryParse
      ErrorViewModel (class)
      Locale (class):  Locale, Parse, TryParse
      WeatherForecast (class)
      WeatherForecastViewModel (class)
   BookStoreApi.Models
      Book (class)
   ContactManager.Data
      SeedData (class):  Initialize, SeedDB
   ContosoUniversity
      SchoolContext (class):  SchoolContext
      SchoolContextFactory (class):  Create
   ContosoUniversity.Controllers
      HomeController (class):  About, Contact, Error, HomeController, Index, Privacy
      StudentsController (class):  Create, Delete, DeleteConfirmed, Details, Edit, EditPost, Index, StudentsController
   ContosoUniversity.Migrations
      Initial (class):  Down, Up
   ContosoUniversity.Models
      Course (class)
      Enrollment (class)
      ErrorViewModel (class)
      Student (class)
   ControllerDI
      Program (class):  CreateHostBuilder, CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
      Startup1 (class):  Configure, ConfigureServices, Startup1
   ControllerDI.Controllers
      HomeController (class):  About, Error, HomeController, Index, Privacy
      SettingsController (class):  Index, SettingsController
   ControllerDI.Interfaces
      IDateTime (interface)
   ControllerDI.Models
      ErrorViewModel (class)
      SampleWebSettings (class)
   ControllerDI.Services
      SystemDateTime (class)
   Culture
      RequestCultureMiddleware (class):  InvokeAsync, RequestCultureMiddleware
      RequestCultureMiddlewareExtensions (class):  UseRequestCulture
   DependentLibrary
      NoGoodController (class):  Index
         Since the MVC project references this project, this controller ordinarily is discovered and available.
   Filters.EndpointFilters
      ABCEndpointFilters (class):  InvokeAsync
      AEndpointFilter (class):  AEndpointFilter
      BEndpointFilter (class):  BEndpointFilter
      CEndpointFilter (class):  CEndpointFilter
   … and 102 more namespaces (use --format json for the full surface)

CONSUMER PATHS
   wire into DI  →  MyCustomMiddlewareExtensions.UseMyCustomMiddleware(...)
   wire into DI  →  RequestCultureMiddlewareExtensions.UseRequestCulture(...)
   extend  →  derive ABCEndpointFilters
   extend  →  derive HostPageModel
   contract  →  implement IDateTime
   extend  →  derive Person

ENTRY POINTS
   HTTP (3)
      POST /Students  → StudentsController  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
      POST /Students [Create]  → StudentsController  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49)
      POST /Students [Edit]  → StudentsController  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.51.0, Grpc.AspNetCore.HealthChecks 2.42.0-pre1, Microsoft.AspNetCore.All 2.0.9, Microsoft.AspNetCore.App, Microsoft.AspNetCore.Authentication.JwtBearer 7.0.2, Microsoft.AspNetCore.Authentication.Negotiate 6.0.0, Microsoft.AspNetCore.Authentication.OpenIdConnect 9.0.0-rc.1.24452.1, Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore 7.0.0-preview.4.22251.1 … (16 total)
   ORM/Data:  EntityFramework 6.4.4, Microsoft.EntityFrameworkCore.Design 6.0.0-preview.7.21378.4, Microsoft.EntityFrameworkCore.InMemory 8.0.2, Microsoft.EntityFrameworkCore.SQLite 2.2.0, Microsoft.EntityFrameworkCore.SqlServer 10.0.1, Microsoft.EntityFrameworkCore.Tools 10.0.1
   Other:  Google.Protobuf 3.22.0, Microsoft.Extensions.FileProviders.Embedded 3.1.6, Microsoft.Extensions.Logging.Debug 3.1.2, Microsoft.Identity.Web 1.25.10, Microsoft.VisualStudio.Web.CodeGeneration.Design 10.0.1, Rick.Docs.Samples.RouteInfo 1.0.0.4

→ drill in:  --focus "<TypeName>"   (e.g. --focus MyCustomMiddlewareExtensions)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 9468ms |
| GenericExtraction | 23897ms |
| SignalSealing | 0ms |
| SpecificExtraction | 22729ms |
| Compression | 168ms |
| **Total** | **87632ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 23891ms | 2331 | 3867 |
| SyntaxStructureExtractor | 23819ms | 2331 | 2709 |
| DiRegistrationExtractor | 23802ms | 15 | 2316 |
| RazorPagesExtractor | 16939ms | 0 | 2081 |
| ProjectStructure | 6662ms | 0 | 0 |
| CallGraphExtractor | 5461ms | 0 | 0 |
| FileTreeExtractor | 2120ms | 0 | 0 |
| BodyFactsExtractor | 1534ms | 0 | 0 |
| EndpointExtractor | 1370ms | 0 | 2631 |
| BlazorEntryExtractor | 1360ms | 0 | 2631 |
| SolutionDiscovery | 681ms | 0 | 0 |
| DependencyExtractor | 542ms | 0 | 0 |
| IndirectWiringDetector | 429ms | 0 | 2096 |
| EfCoreExtractor | 397ms | 0 | 2073 |
| GrpcServiceExtractor | 369ms | 0 | 2042 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 643 | 418 |
| ReadsWrites | 72 | 53 |
| EntityRelation | 2 | 2 |

_5571 files · 622 projects_
