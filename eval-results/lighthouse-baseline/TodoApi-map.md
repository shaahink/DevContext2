Overview map (no focus).
Analyzing project...

MAP  TodoApp     (6 projects)

STACK  net9.0 ú Minimal APIs ú EF Core

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 6 project(s); no MediatR

TOPOLOGY (depends-on)
   Todo.Web.Shared
   TodoApp.ServiceDefaults
   Todo.Api ÄÄ TodoApp.ServiceDefaults
   Todo.Web.Client ÄÄ Todo.Web.Shared
   Todo.Web.Server ÄÄ Todo.Web.Client, Todo.Web.Shared, TodoApp.ServiceDefaults
   TodoApp.AppHost ÄÄ Todo.Api, Todo.Web.Server

ENTRY POINTS
   HTTP (12)
      DELETE /todos/{id}   TodoApi  (Todo.Api/Todos/TodoApi.cs:67)
      GET /auth/login/{provider}   AuthApi  (Todo.Web/Server/AuthApi.cs:55)
      GET /auth/signin/{provider}   AuthApi  (Todo.Web/Server/AuthApi.cs:64)
      GET /todos/   TodoApi  (Todo.Api/Todos/TodoApi.cs:24)
      GET /todos/{id}   TodoApi  (Todo.Api/Todos/TodoApi.cs:29)
      POST /auth/login   AuthApi  (Todo.Web/Server/AuthApi.cs:27)
      POST /auth/logout   AuthApi  (Todo.Web/Server/AuthApi.cs:40)
      POST /auth/register   AuthApi  (Todo.Web/Server/AuthApi.cs:14)
      POST /todos/   TodoApi  (Todo.Api/Todos/TodoApi.cs:38)
      POST /users/token/{provider}   UsersApi  (Todo.Api/Users/UsersApi.cs:24)
      PUT /todos/{id}   TodoApi  (Todo.Api/Todos/TodoApi.cs:52)
      GET /  (Todo.Web/Server/App.razor:1)

PACKAGES
   Web/API:  AspNet.Security.OAuth.GitHub 9.4.0, Auth0.AspNetCore.Authentication
1.5.0, Microsoft.AspNetCore.Authentication.Google 9.0.8, 
Microsoft.AspNetCore.Authentication.MicrosoftAccount 9.0.8, 
Microsoft.AspNetCore.Components.WebAssembly 9.0.8, 
Microsoft.AspNetCore.Components.WebAssembly.Server 9.0.8, 
Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.8, 
Microsoft.AspNetCore.Mvc.Testing 9.0.8 . (11 total)
   ORM/Data:  Microsoft.EntityFrameworkCore.Design 9.0.8, 
Microsoft.EntityFrameworkCore.InMemory 9.0.8, 
Microsoft.EntityFrameworkCore.Sqlite 9.0.8
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.12.0, 
OpenTelemetry.Extensions.Hosting 1.12.0, OpenTelemetry.Instrumentation.Http 
1.12.0, OpenTelemetry.Instrumentation.Runtime 1.12.0
   Testing:  coverlet.collector 6.0.4, xunit 2.9.3, xunit.runner.visualstudio 
3.1.3
   Other:  Microsoft.Extensions.Http 9.0.8, Microsoft.Extensions.Http.Resilience
9.7.0, Microsoft.Extensions.ServiceDiscovery 9.5.0, 
Microsoft.Extensions.ServiceDiscovery.Yarp 9.5.0, Microsoft.NET.Test.Sdk 
17.14.1, Microsoft.OpenApi 1.6.22, MiniValidation 0.9.2, Yarp.ReverseProxy 2.3.0

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 40 files ú 164 nodes ú 87 edges ú 12 entries ú 11/12 target ú ~635 
tokens ú 3.4s stage2 x2.8 stage3 x1.1

                                    Insights                                    
ÚÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Sev  ³ Category ³ Title                        ³ Evidence                    ³
ÃÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ WARN ³ Risk     ³ 11/12 endpoints anonymous,   ³ GET /, GET                  ³
³      ³          ³ incl. 6 POST/PUT/DELETE      ³ /auth/signin/{provider},    ³
³      ³          ³                              ³ GET /auth/login/{provider}  ³
³ NOTE ³ Wiring   ³ Possible dead code: 5 public ³ ExternalProviders,          ³
³      ³          ³ types with zero inbound      ³ AuthClient,                 ³
³      ³          ³ references                   ³ ExternalUserInfo            ³
³ NOTE ³ Wiring   ³ Multi-implementation         ³ ? (16 impls),               ³
³      ³          ³ interfaces: ? (16 impls) ú   ³ ApplicationDiscriminator =  ³
³      ³          ³ ApplicationDiscriminator =   ³ "TodoApp" (2 impls),        ³
³      ³          ³ "TodoApp" (2 impls) ú        ³ TodoDbContext (2 impls)     ³
³      ³          ³ TodoDbContext (2 impls)      ³                             ³
³ INFO ³ Coverage ³ Entry targets resolved 11/12 ³                             ³
³      ³          ³ (91%) - use --focus for      ³                             ³
³      ³          ³ deeper traces                ³                             ³
³ INFO ³ Wiring   ³ DI: 33 Extension ú 5         ³                             ³
³      ³          ³ Singleton ú 5 Scoped (43     ³                             ³
³      ³          ³ total)                       ³                             ³
³ INFO ³ Topology ³ Most depended-upon: Todo.Api ³ Todo.Api (2 dependents),    ³
³      ³          ³ (2 dependents) ú             ³ Todo.Web.Shared (2          ³
³      ³          ³ Todo.Web.Shared (2           ³ dependents),                ³
³      ³          ³ dependents) ú                ³ TodoApp.ServiceDefaults (1  ³
³      ³          ³ TodoApp.ServiceDefaults (1   ³ dependents)                 ³
³      ³          ³ dependents)                  ³                             ³
ÀÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
                          Stage Timing                           
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Stage                   ³   Time ³ Bar                        ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ DiscoveryAndCacheWarmup ³  208ms ³ ÛÛ                         ³
³ GenericExtraction       ³  359ms ³ ÛÛÛÛ                       ³
³ SignalSealing           ³    2ms ³                            ³
³ SpecificExtraction      ³ 2205ms ³ ÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛ ³
³ Compression             ³   40ms ³                            ³
³ Total                   ³ 3379ms ³                            ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

                                   Extractors                                   
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Name                     ³   Time ³ +Types ³ +Dets ³ Status                  ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ CallGraphExtractor       ³ 2037ms ³      0 ³     0 ³ ran                     ³
³ SyntaxStructureExtractor ³  353ms ³     43 ³    49 ³ ran                     ³
³ DiRegistrationExtractor  ³  343ms ³      0 ³    49 ³ ran                     ³
³ ProgramCsFlowExtractor   ³  266ms ³      0 ³     6 ³ ran                     ³
³ EndpointExtractor        ³  133ms ³      0 ³    16 ³ ran                     ³
³ BlazorEntryExtractor     ³  130ms ³      0 ³    15 ³ ran                     ³
³ EfCoreExtractor          ³   82ms ³      0 ³     5 ³ ran                     ³
³ ProjectStructure         ³   75ms ³      0 ³     0 ³ ran                     ³
³ SourceBodyExtractor      ³   70ms ³      0 ³     0 ³ ran                     ³
³ SolutionDiscovery        ³   58ms ³      0 ³     0 ³ ran                     ³
³ FileTreeExtractor        ³   43ms ³      0 ³     0 ³ ran                     ³
³ InMemoryEventBusExtracto ³   31ms ³      0 ³     1 ³ ran                     ³
³ r                        ³        ³        ³       ³                         ³
³ IndirectWiringDetector   ³   28ms ³      0 ³     0 ³ ran                     ³
³ LayerClassifier          ³   28ms ³      0 ³     0 ³ ran                     ³
³ DependencyExtractor      ³   27ms ³      0 ³     0 ³ ran                     ³
³ AntiPatternDetector      ³    0ms ³      0 ³     0 ³ skipped: gated by       ³
³                          ³        ³        ³       ³ ShouldRun               ³
³ AspireExtractor          ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aspire            ³
³ AwsLambdaExtractor       ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aws-lambda        ³
³ AzureFunctionsExtractor  ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs azure-functions   ³
³ CliCommandExtractor      ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs cli-commands      ³
³ ControllerActionExtracto ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³ r                        ³        ³        ³       ³ needs controllers       ³
³ DesktopEntryExtractor    ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs desktop-ui        ³
³ EventBusExtractor        ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs masstransit or    ³
³                          ³        ³        ³       ³ nservicebus             ³
³ GraphQlResolverExtractor ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs graphql           ³
³ GrpcServiceExtractor     ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs grpc              ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

          Graph Seams           
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄ¿
³ Seam        ³ Edges ³ Approx ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄ´
³ Calls       ³    21 ³      4 ³
³ ReadsWrites ³    45 ³     43 ³
³ Resolves    ³    21 ³      0 ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÙ
164 nodes ú 87 edges ú 11/12 entries  target
cache 80% hit ú 40 files ú 0 projects
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³  Metric  ³        Value         ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ Solution ³     TodoApp.sln      ³
³   Time   ³        4552ms        ³
³  Tokens  ³  ~635 (budget 8000)  ³
³ Version  ³ v1.0.5-preview.0.244 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
