# REPORT
**MVCCore**

Style: SampleCollection
_2 projects  ·  3 HttpEndpoint  ·  net10.0, net452 + swagger + controllers + desktop-ui + minimal-apis + identity + efcore + razor-pages + grpc + signalr_

## Stats

| Metric | Value |
|--------|-------|
| Files | 5575 |
| Projects | 627 |
| Nodes | 1262 |
| Edges | 718 |
| Entries | 3 |
| With target | 3/3 |
| Verified edges | 34% |
| Analyzed in | 72.8s |

## Top Flows

1. **POST /Students** → `StudentsController` *(HttpEndpoint)*
2. **POST /Students** → `StudentsController` *(HttpEndpoint)*
3. **POST /Students** → `StudentsController` *(HttpEndpoint)*

### Trace 1: POST /Students

TRACE  POST /Students
       aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118
       MVCCore
▸ ENTRY  POST /Students  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
   ├─ call StudentsController.DeleteConfirmed  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
   │      [HttpPost, ActionName("Delete")]
   │      [ValidateAntiForgeryToken]
   │      public async Task<IActionResult> DeleteConfirmed(int id)
   ├─ call StudentsController.Edit  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)
   │      public async Task<IActionResult> Edit(int? id)
   │      if (id == null)
   │      return NotFound();
   │  ├─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:97) [approx]
   │  └─ call StudentsController.StudentExists  (aspnetcore/data/ef-mvc/intro/samples/5cu/Controllers/StudentsController.cs:104) [verified]
   └─ call StudentsController.Create  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49)
          public IActionResult Create()
          return View();
      └─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:59) [approx]
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: POST /Students

TRACE  POST /Students
       aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118
       MVCCore
▸ ENTRY  POST /Students  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
   ├─ call StudentsController.DeleteConfirmed  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
   │      [HttpPost, ActionName("Delete")]
   │      [ValidateAntiForgeryToken]
   │      public async Task<IActionResult> DeleteConfirmed(int id)
   ├─ call StudentsController.Edit  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)
   │      public async Task<IActionResult> Edit(int? id)
   │      if (id == null)
   │      return NotFound();
   │  ├─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:97) [approx]
   │  └─ call StudentsController.StudentExists  (aspnetcore/data/ef-mvc/intro/samples/5cu/Controllers/StudentsController.cs:104) [verified]
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
   ├─ call StudentsController.DeleteConfirmed  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:118)
   │      [HttpPost, ActionName("Delete")]
   │      [ValidateAntiForgeryToken]
   │      public async Task<IActionResult> DeleteConfirmed(int id)
   ├─ call StudentsController.Edit  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)
   │      public async Task<IActionResult> Edit(int? id)
   │      if (id == null)
   │      return NotFound();
   │  ├─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:97) [approx]
   │  └─ call StudentsController.StudentExists  (aspnetcore/data/ef-mvc/intro/samples/5cu/Controllers/StudentsController.cs:104) [verified]
   └─ call StudentsController.Create  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49)
          public IActionResult Create()
          return View();
      └─ call StudentsController.View  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:59) [approx]
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

## Insights

_3 info · 6 notable · 1 warning_

### **WARNING**: Auth surface: 0 protected, 3 unannotated of 3 API endpoints
*(Risk)*

- 3 no auth annotation

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

### **NOTABLE**: ViewModel-View: 191 VMs + 2 Views (0 call edges)
*(Wiring)*

- 191 ViewModels
- 2 Views

### **NOTABLE**: Internal hubs: 3 heavily-referenced internal types
*(Topology)*

- ForwardingLogger (380 refs)
- UserRepository (1 refs)
- ScopedProcessingService (1 refs)

### **NOTABLE**: Extension seats: AddDbContext (79 impls) · AddHttpClient (33 impls) · AddCors (23 impls)
*(Wiring)*

- AddDbContext (79 impls)
- AddHttpClient (33 impls)
- AddCors (23 impls)

### **NOTABLE**: Multi-implementation interfaces: IAuthorizationHandler (7 impls) · IQuoteService (2 impls) · IEmailSender (2 impls)
*(Wiring)*

- IAuthorizationHandler (7 impls)
- IQuoteService (2 impls)
- IEmailSender (2 impls)

### _INFO_: Entry targets resolved 3/3 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 1 feature areas
*(Shape)*

- ContosoUniversity/Controllers (3 entries)

### _INFO_: Routing surface: 3 routes exposed
*(Shape)*

- POST /Students
- POST /Students
- POST /Students

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
   GenericHostSample
      Program (class):  CreateHostBuilder, CreateHostBuilder2, Main
   GrcpServices
      ExamplesService (class):  StreamingBothWays, StreamingFromClient, StreamingFromServer, UnaryCall
      GreeterBase (class):  SayHello
      GreeterService (class):  SayHello
      HelloReply (class)
      HelloRequest (class)
   GrpcServiceHC.Services
      GreeterService (class):  GreeterService, SayHello
   HttpResultInterfaces
      Program (class):  Main
   MVCCore
      Program (class):  CreateHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
   MVCareas
      Program (class):  CreateHostBuilder, CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
      Startup2 (class):  Configure, ConfigureServices, Startup2
      StartupMapAreaRoute (class):  Configure, ConfigureServices, StartupMapAreaRoute
   MVCareas.Areas.Products.Controllers
      HomeController (class):  About, Index
      ManageController (class):  About, Index
   MVCareas.Areas.Services.Controllers
      HomeController (class):  About, Index
   MVCareas.Controllers
      HomeController (class):  About, Error, HomeController, Index, Privacy
   MVCareas.Models
      ErrorViewModel (class)
   Middleware.Example
      IMessageWriter (interface):  Write
      LoggingMessageWriter (class):  LoggingMessageWriter, Write
      MyCustomMiddleware (class):  InvokeAsync, MyCustomMiddleware
      MyCustomMiddlewareExtensions (class):  UseMyCustomMiddleware
      RequestCultureMiddleware (class):  InvokeAsync, RequestCultureMiddleware
      RequestCultureMiddlewareExtensions (class):  UseRequestCulture
   MinAPISeparateFile
      TodoEndpoints (class):  Map
   MinimalAPI
      Program (class):  Main
      WeatherForecast (class)
   MyApp
      Startup (class):  Configure, ConfigureServices, Startup
   MyNamespace
      ApiException (class):  ApiException, ToString
      FileResponse (class):  Dispose, FileResponse
      ProblemDetails (class)
      TodoClient (class):  CreateAsync, DeleteAsync, GetAsync, GetByIdAsync, PrepareRequest, ProcessResponse, TodoClient, UpdateJsonSerializerSettings
      TodoItem (class)
   MyPolicy
      Startup (class):  Configure, ConfigureServices
   MySharedApp.Controllers
      MySharedController (class):  Index, IndexView
   PageFilter
      IndexModel (class):  IndexModel, OnPageHandlerExecutionAsync, OnPageHandlerSelectionAsync
      ProcessUserAgent (class):  ProcessUserAgent, Write
      Program (class):  CreateHostBuilder, CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
      Startup2 (class):  Configure, ConfigureServices, Startup2
      StartupSync (class):  Configure, ConfigureServices, StartupSync
   PageFilter.Filters
      AddHeaderAttribute (class):  AddHeaderAttribute, OnResultExecuting
      SampleAsyncPageFilter (class):  OnPageHandlerExecutionAsync, OnPageHandlerSelectionAsync, SampleAsyncPageFilter
      SamplePageFilter (class):  OnPageHandlerExecuted, OnPageHandlerExecuting, OnPageHandlerSelected, SamplePageFilter
   PageFilter.Movies
      IndexModel (class):  OnGet
      TestModel (class):  OnGet
   PageFilter.Pages
      ErrorModel (class):  ErrorModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   Plugin
      HelloController (class):  Index
   RPareas
      Program (class):  CreateHostBuilder, CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
   RPareas.Areas.Identity
      IdentityHostingStartup (class):  Configure
   RPareas.Areas.Identity.Pages.Account
      InputModel (class)
      LoginModel (class):  LoginModel, OnGetAsync, OnPostAsync
      LogoutModel (class):  LogoutModel, OnGet, OnPost
      RegisterModel (class):  OnGet, OnPostAsync, RegisterModel
   RPareas.Areas.Identity.Pages.Account.Manage
      ManageNavPages (class):  ChangePasswordNavClass, ExternalLoginsNavClass, IndexNavClass, PersonalDataNavClass, TwoFactorAuthenticationNavClass
      PersonalDataModel (class):  OnGet, PersonalDataModel
   RPareas.Areas.Products.Pages
      AboutModel (class):  OnGet
      IndexModel (class):  OnGet
   RPareas.Areas.Services.Pages.Manage
      AboutModel (class):  OnGet
      IndexModel (class):  OnGet
   RPareas.Data
      ApplicationDbContext (class):  ApplicationDbContext
   RPareas.Data.Migrations
      ApplicationDbContextModelSnapshot (class)
      CreateIdentitySchema (class)
   RPareas.Pages
      AboutModel (class):  OnGet
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   RPauth
      Startup (class):  Configure, ConfigureServices, Startup
   RPgoog2
      StartupAccessDeniedPath (class):  Configure, ConfigureServices, StartupAccessDeniedPath
   RazorClassLib.MyFeature.Pages
      Page1Model (class):  OnGet
   RazorPagesContacts
      Program (class):  BuildWebHost, CreateHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
      StartupOnHead (class):  Configure, ConfigureServices, StartupOnHead
      StartupRPoptions (class):  Configure, ConfigureServices, StartupRPoptions
      StartupWithRazorPagesAtContentRoot (class):  Configure, ConfigureServices, StartupWithRazorPagesAtContentRoot
      StartupWithRazorPagesRoot (class):  Configure, ConfigureServices, StartupWithRazorPagesRoot
   RazorPagesContacts.Data
      CustomerDbContext (class):  CustomerDbContext
      RazorPagesContactsContext (class):  RazorPagesContactsContext
   RazorPagesContacts.Migrations
      InitialCreate (class)
      RazorPagesContactsContextModelSnapshot (class)
   RazorPagesContacts.Models
      Customer (class)
   RazorPagesContacts.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet, OnGetAsync, OnPostDeleteAsync
      PrivacyModel (class):  OnGet, OnHead, PrivacyModel
   RazorPagesContacts.Pages.Customers
      Create2Model (class):  Create2Model, OnGet, OnPostAsync
      CreateModel (class):  CreateModel, OnGet, OnPostAsync
      DeleteModel (class):  DeleteModel, OnGetAsync, OnPostAsync
      DetailsModel (class):  DetailsModel, OnGetAsync
      EditModel (class):  EditModel, OnGetAsync, OnPostAsync
      IndexModel (class):  IndexModel, OnGetAsync, OnPostDeleteAsync
   RazorPagesIntro
      Program (class):  BuildWebHost, CreateHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
   RazorPagesIntro.Pages
      ErrorModel (class):  ErrorModel, OnGet
      Index2Model (class):  OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   RazorPagesMovie
      Program (class):  BuildWebHost, CreateHostBuilder, CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
   RazorPagesMovie.Data
      RazorPagesMovieContext (class):  RazorPagesMovieContext
   RazorPagesMovie.Migrations
      Initial (class)
      InitialCreate (class)
      New_DataAnnotations (class)
      Rating (class)
      RazorPagesMovieContextModelSnapshot (class)
   RazorPagesMovie.Models
      Movie (class)
      MovieContext (class):  MovieContext
      RazorPagesMovieContext (class):  RazorPagesMovieContext
      SeedData (class):  Initialize
   RazorPagesMovie.Pages
      AboutModel (class):  OnGet
      ContactModel (class):  OnGet
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   RazorPagesMovie.Pages.Movies
      DeleteModel (class):  DeleteModel, OnGetAsync, OnPostAsync
      DetailsModel (class):  DetailsModel, OnGetAsync
      EditModel (class):  EditModel, OnGetAsync, OnPostAsync
      IndexModel (class):  IndexModel, OnGetAsync
   SampleApp
      CulturedQueryStringValueProviderFactory (class):  CreateValueProviderAsync
   SignalRWebPack.Hubs
      ChatHub (class):  NewMessage
   StartupEnhancement
      StartupEnhancementHostingStartup (class):  Configure
   StartupFilterSample
      Program (class):  BuildWebHost, CreateHostBuilder, Main
      RequestSetOptionsMiddleware (class):  Invoke, RequestSetOptionsMiddleware
      RequestSetOptionsStartupFilter (class):  Configure
   TodoApi
      Program (class):  BuildWebHost, CreateHostBuilder, CreateWebHostBuilder, Main
   TodoApi.EndpointFilters
      TodoIsValidFilter (class):  InvokeAsync, TodoIsValidFilter
      TodoIsValidUcFilter (class):  InvokeAsync
      Utilities (class):  IsValid
   ViewInjectSample.Controllers
      HelperController (class):  Index
      ProfileController (class):  Index
      ToDoController (class):  Index, ToDoController
   ViewInjectSample.Helpers
      MyHtmlHelper (class):  MyHtmlHelper
   ViewInjectSample.Infrastructure
      ToDoItemRepository (class):  List, ToDoItemRepository
   ViewInjectSample.Interfaces
      IToDoItemRepository (interface):  List
   ViewInjectSample.Model
      Profile (class)
      State (class):  State
      ToDoItem (class)
   ViewInjectSample.Model.Services
      ProfileOptionsService (class):  ListColors, ListGenders, ListStates
      StatisticsService (class):  GetAveragePriority, GetCompletedCount, GetCount, StatisticsService
   Web2API.Controllers
      TodoItems1Controller (class):  GetTodoItems, GetTodoItems2, MyDelete, MyDelete2, PutTodoItem
      TodoItems2Controller (class):  GetTodoItems, GetTodoItems2, MyDelete, MyDelete2, PreflightRoute, PutTodoItem
      TodoItemsController (class):  GetTodoItems, MyDelete, PutTodoItem
      ValuesController (class):  Get, GetValues2, Put
      WidgetController (class):  Get
   WebAPI
      HostPageModel (class):  SetHost
      IndexModel (class):  IndexModel, OnGet
      MyGC (class)
      Startup (class):  Configure, ConfigureServices, Startup
      Startup2 (class):  Configure, ConfigureServices, Startup2
      StartupAllowSubdomain (class):  Configure, ConfigureServices, StartupAllowSubdomain
      StartupEndPointBugTest (class):  Configure, ConfigureServices
      StartupTest2 (class):  Configure, ConfigureServices
      TestModel (class):  OnGet, TestModel
      WeatherForecast (class)
   WebAPI.Controllers
      TodoItems1Controller (class):  GetTodoItems, GetTodoItems2, MyDelete, MyDelete2, PutTodoItem
      TodoItems2Controller (class):  GetTodoItems, GetTodoItems2, MyDelete, MyDelete2, PreflightRoute, PutTodoItem
      TodoItemsController (class):  GetTodoItems, MyDelete, PutTodoItem
      ValuesController (class):  Get, GetValues2, Put
      WeatherForecastController (class):  Get, WeatherForecastController
      WidgetController (class):  Get
   WebAPI3
      Startup (class):  Configure, ConfigureServices, Startup
   WebAPIDefault
      Startup (class):  Configure, ConfigureServices
   WebAPIendPt
      Startup (class):  Configure, ConfigureServices
   WebAll
      WeatherForecast (class)
   WebAll.Controllers
      Home2Controller (class):  Error, Home2Controller, Index, Privacy
      WeatherForecastController (class):  Get, WeatherForecastController
   WebAll.Models
      ErrorViewModel (class)
   WebAll.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   WebApp
      CulturedQueryStringValueProviderFactory (class):  CreateValueProviderAsync
      Program (class):  BuildWebHost, CreateWebHostBuilder, Main
   WebApp1
      StartupTwitter (class):  Configure, ConfigureServices, StartupTwitter
   WebApp1.Areas.MyFeature2
      IndexModel (class):  OnGet
   WebApp1.Controllers
      ValuesController (class):  Delete, Get, Post, Put
   WebAppParts
      Program (class):  CreateHostBuilder, CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
      Startup2 (class):  Configure, ConfigureServices, Startup2
      StartupRm (class):  Configure, ConfigureServices, StartupRm
   WebAppParts.Controllers
      HomeController (class):  Error, Index, Privacy
   WebAppParts.Models
      ErrorViewModel (class)
   WebAppRP5
      Startup (class):  Configure, ConfigureServices, Startup
   WebApplication1.Data
      AppDbCntx (class):  AppDbCntx
   WebApplication1.Data.Migrations
      AppDbCntxModelSnapshot (class)
   WebApplication1.Pages
      PrivacyModel (class):  OnGet
   WebApplication2
      Program (class):  CreateWebHostBuilder, Main
      Startup (class):  Configure, ConfigureServices, Startup
   WebApplication2.Data
      ApplicationDbContext (class):  ApplicationDbContext
   WebApplication2.Data.Migrations
      ApplicationDbContextModelSnapshot (class)
      CreateIdentitySchema (class)
   WebApplication2.Pages
      ErrorModel (class):  OnGet
      IndexModel (class):  OnGet
      PrivacyModel (class):  OnGet
   WebApplication4
      Startup (class):  Configure, ConfigureServices, Startup
   WebClaimsPrincipal.Data
      ApplicationDbContext (class):  ApplicationDbContext
   WebClaimsPrincipal.Data.Migrations
      ApplicationDbContextModelSnapshot (class)
      CreateIdentitySchema (class)
   WebClaimsPrincipal.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   WebHTTPS
      Startup (class):  Configure, ConfigureServices, Startup
   WebPS.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   WebRP
      Startup (class):  Configure, ConfigureServices, Startup
   WebRPauth.Data
      ApplicationDbContext (class):  ApplicationDbContext
   WebRPauth.Data.Migrations
      ApplicationDbContextModelSnapshot (class)
      CreateIdentitySchema (class)
   WebRPauth.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   WebRPmapClaims.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   WebRPwinAuth.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
   WebStartup.Middleware
      RequestSetOptionsMiddleware (class):  Invoke, RequestSetOptionsMiddleware
      RequestSetOptionsStartupFilter (class):  Configure
   WebViewInject.Controllers
      HomeController (class):  Error, HomeController, Index2, Privacy
   WebViewInject.Models
      ErrorViewModel (class)
   WebViewInject.Pages
      ErrorModel (class):  ErrorModel, OnGet
      IndexModel (class):  IndexModel, OnGet
      PrivacyModel (class):  OnGet, PrivacyModel
      RPprofileModel (class):  OnGet
   global
      ApplicationDbContext (class):  ApplicationDbContext
      HeartRateRecord (record):  Create
      HtmlResult (class):  ExecuteAsync, HtmlResult
      IndexModel (class):  IndexModel, OnGetAsync
      LDR (class):  LocaleDateRange
      MyAuthorizationMiddlewareResultHandler (class):  HandleAsync, Show404ForForbiddenResult
      MyClaimsTransformation (class):  TransformAsync
      MyUserAgentDetectionLib (class):  DisallowsSameSiteNone
      PrefixKeyVaultSecretManager (class):  GetKey, Load, PrefixKeyVaultSecretManager
      Product (class)
      ResultsExtensions (class):  Html
      SampleAuthorizationMiddlewareResultHandler (class):  HandleAsync
      Show404Requirement (class)
      Startup (class):  Configure, ConfigureServices, Startup
      Startup2 (class):  Configure, ConfigureServices, Startup2
      Tag (class):  TryParse
      TodoController (class):  GetById, TodoController
      TodoDb (class):  TodoDb

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
      POST /Students  → StudentsController  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:81)
      POST /Students  → StudentsController  (aspnetcore/data/entity-framework-6/3.xsample/MVCCore/Controllers/StudentsController.cs:49)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.51.0, Grpc.AspNetCore.HealthChecks 2.42.0-pre1, Microsoft.AspNetCore.All 2.0.9, Microsoft.AspNetCore.App, Microsoft.AspNetCore.Authentication.JwtBearer 7.0.2, Microsoft.AspNetCore.Authentication.Negotiate 6.0.0, Microsoft.AspNetCore.Authentication.OpenIdConnect 9.0.0-rc.1.24452.1, Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore 7.0.0-preview.4.22251.1 … (16 total)
   ORM/Data:  EntityFramework 6.4.4, Microsoft.EntityFrameworkCore.Design 6.0.0-preview.7.21378.4, Microsoft.EntityFrameworkCore.InMemory 8.0.2, Microsoft.EntityFrameworkCore.SQLite 2.2.0, Microsoft.EntityFrameworkCore.SqlServer 10.0.1, Microsoft.EntityFrameworkCore.Tools 10.0.1
   Other:  Google.Protobuf 3.22.0, Microsoft.Extensions.FileProviders.Embedded 3.1.6, Microsoft.Extensions.Logging.Debug 3.1.2, Microsoft.Identity.Web 1.25.10, Microsoft.VisualStudio.Web.CodeGeneration.Design 10.0.1, Rick.Docs.Samples.RouteInfo 1.0.0.4

→ drill in:  --focus "<TypeName>"   (e.g. --focus MyCustomMiddlewareExtensions)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 9364ms |
| GenericExtraction | 16080ms |
| SignalSealing | 0ms |
| SpecificExtraction | 17398ms |
| Compression | 206ms |
| **Total** | **72772ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 16077ms | 2331 | 3755 |
| DiRegistrationExtractor | 16071ms | 15 | 3755 |
| RazorPagesExtractor | 12651ms | 0 | 2312 |
| ProgramCsFlowExtractor | 9829ms | 0 | 1425 |
| ProjectStructure | 5119ms | 0 | 0 |
| CallGraphExtractor | 4440ms | 0 | 0 |
| FileTreeExtractor | 3375ms | 0 | 0 |
| EndpointExtractor | 1153ms | 0 | 2853 |
| BlazorEntryExtractor | 980ms | 0 | 2852 |
| SolutionDiscovery | 867ms | 0 | 0 |
| BodyFactsExtractor | 638ms | 0 | 0 |
| EfCoreExtractor | 423ms | 0 | 2386 |
| DependencyExtractor | 422ms | 0 | 15 |
| ControllerActionExtractor | 375ms | 0 | 2185 |
| SignalRHubExtractor | 355ms | 0 | 2115 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 644 | 419 |
| ReadsWrites | 72 | 53 |
| EntityRelation | 2 | 2 |

_5575 files · 627 projects_
