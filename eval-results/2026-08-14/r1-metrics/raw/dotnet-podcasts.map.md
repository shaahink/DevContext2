MAP  NetPodcast     (13 projects)
SCOPE  analyzed NetPodcast.sln - 1 of 5 solutions in this repo; analyze another 
by naming its solution - style/topology are local to this slice, not the whole 
system
       not analyzed - 2 runnable apps outside this solution:
         Microsoft.NetConf2021.Maui: MAUI App [.NET MAUI]
         NetPodsMauiBlazor: MAUI App [.NET MAUI]

STACK  net7.0, net7.0-android +2 more TFMs ú Minimal APIs ú CQRS (hand-rolled 
mediator) ú EF Core

STYLE  Microservices  (confidence high)
       evidence: 3 runnable web services with message bus (Azure.Storage.Queues)

       per service:
         Podcast.Server: Razor Pages [Razor]
         ListenTogether.Hub: SignalR host [SignalR]
         Podcast.API: Web API [EF Core]
         Podcast.Ingestion.Worker: Worker Service [Worker, EF Core]
         Podcast.Updater.Worker: Worker Service [Worker, EF Core]

TOPOLOGY (depends-on)
   ListenTogether.Domain
   Podcast.Infrastructure
   ListenTogether.Application ÄÄ ListenTogether.Domain
   Podcast.Shared
   ListenTogether.Infrastructure ÄÄ ListenTogether.Application, 
ListenTogether.Domain
   Podcast.Client ÄÄ Podcast.Pages
   Podcast.Components
   Podcast.Pages ÄÄ Podcast.Components, Podcast.Shared
   ListenTogether.Hub ÄÄ ListenTogether.Application, ListenTogether.Domain, 
ListenTogether.Infrastructure
   Podcast.API ÄÄ Podcast.Infrastructure
   Podcast.Ingestion.Worker ÄÄ Podcast.Infrastructure
   Podcast.Server ÄÄ Podcast.Client, Podcast.Shared
   Podcast.Updater.Worker ÄÄ Podcast.Infrastructure

CROSS-SERVICE
  bus (1)
    [bus] Podcast.API  Podcast.Ingestion.Worker  
(C:\Code\eval-poles\dotnet-podcasts\src\Services\Podcasts\Podcast.API\Routes\Fee
dsApi.cs:49 raises feed-queue [AzureStorageQueue])

EVENT WIRING  (1 integration events, 1 cross-service)
  feed-queue [AzureStorageQueue]: Podcast.API  Podcast.Ingestion.Worker

ENTRY POINTS
   HTTP (12)
      DELETE /feeds/{id}   PodcastDbContext  
(src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:13)
      GET /categories/   PodcastDbContext  
(src/Services/Podcasts/Podcast.API/Routes/CategoriesApi.cs:7)
      GET /episodes/{id}   PodcastDbContext  
(src/Services/Podcasts/Podcast.API/Routes/EpisodesApi.cs:7)
      GET /Error   ErrorModel  (src/Web/Server/Pages/Error.cshtml.cs:7)
      GET /feeds/   PodcastDbContext  
(src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:8)
      GET /landing   PodcastService.GetShows  
(src/Web/Server/Pages/Landing.cshtml.cs:7)
      GET /shows/   ShowClient.CheckLink  
(src/Services/Podcasts/Podcast.API/Routes/ShowsApi.cs:13)
      GET /shows/{id}   PodcastDbContext  
(src/Services/Podcasts/Podcast.API/Routes/ShowsApi.cs:14)
      POST /feeds/   FeedsApi  
(src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:7)
      PUT /feeds/{id}   FeedClient.AddFeedAsync  
(src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:9)
      GET /  (src/Services/ListenTogether/ListenTogether.Hub/Program.cs:20)
      GET /version  (src/Services/Podcasts/Podcast.API/Program.cs:145)
   Background (1)
      Worker  (src/Services/Podcasts/Podcast.Ingestion.Worker/Program.cs:20)
   UI (10)
      GET /categories   PodcastService.GetCategories  
(src/Web/Pages/Pages/CategoriesPage.razor:1)
      GET /category/{id:guid}   PodcastService.GetCategories  
(src/Web/Pages/Pages/CategoryPage.razor:1)
      GET /discover   PodcastService.GetShows  
(src/Web/Pages/Pages/DiscoverPage.razor:1)
      GET /listen-later   ListenLaterPage  
(src/Web/Pages/Pages/ListenLaterPage.razor:1)
      GET /listen-together   ListenTogetherPage  
(src/Web/Pages/Pages/ListenTogetherPage.razor:1)
      GET /listen-together/create/{episodeId:guid}   ListenTogetherPage  
(src/Web/Pages/Pages/ListenTogetherPage.razor:1)
      GET /listen-together/room/{roomCode}   ListenTogetherPage  
(src/Web/Pages/Pages/ListenTogetherPage.razor:1)
      GET /settings   ThemeInterop.GetThemeAsync  
(src/Web/Pages/Pages/SettingsPage.razor:1)
      GET /show/{id:guid}   PodcastService.GetShow  
(src/Web/Pages/Pages/ShowPage.razor:1)
      GET /subscriptions   SubscriptionsPage  
(src/Web/Pages/Pages/SubscriptionsPage.razor:1)
   SignalR (1)
      ListenTogetherHub (8 methods: OpenRoom, JoinRoom, LeaveRoom)   
IRequestHandler.HandleAsync  
(src/Services/ListenTogether/ListenTogether.Hub/Hubs/ListenTogetherHub.cs:7)
   Orleans (1)
      RoomGrain : Grain (4 methods: JoinRoom, LeaveRoom, SetRoom)  
(src/Services/ListenTogether/ListenTogether.Application/Grains/RoomGrain.cs:9)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer 7.0.0-*, 
Microsoft.AspNetCore.Components.Web 7.0.0-*, 
Microsoft.AspNetCore.Components.WebAssembly 7.0.0-*, 
Microsoft.AspNetCore.Components.WebAssembly.DevServer 7.0.0-*, 
Microsoft.AspNetCore.Components.WebAssembly.Server 7.0.0-*, 
Microsoft.AspNetCore.OpenApi 7.0.0-*, Microsoft.AspNetCore.SignalR.Client 
7.0.0-*, OpenTelemetry.Exporter.Prometheus.AspNetCore 1.4.0-beta.2 . (10 total)
   ORM/Data:  Microsoft.EntityFrameworkCore 7.0.0-*, 
Microsoft.EntityFrameworkCore.Design 7.0.0-*, 
Microsoft.EntityFrameworkCore.Relational 7.0.0-*, 
Microsoft.EntityFrameworkCore.SqlServer 7.0.0-*, 
Microsoft.EntityFrameworkCore.Tools 7.0.0-rc.2.22472.11, 
OpenTelemetry.Instrumentation.EntityFrameworkCore 1.0.0-beta.3
   Logging:  Azure.Monitor.OpenTelemetry.Exporter 1.0.0-beta.4, 
OpenTelemetry.Exporter.Jaeger 1.4.0-beta.2, OpenTelemetry.Extensions 
1.0.0-beta.3, OpenTelemetry.Extensions.Hosting 1.0.0-rc9.8, 
OpenTelemetry.Instrumentation.EventCounters 1.0.0-alpha.1, 
OpenTelemetry.Instrumentation.Http 1.0.0-rc9.8, 
OpenTelemetry.Instrumentation.Process 0.1.0-alpha.1, 
OpenTelemetry.Instrumentation.Runtime 1.0.0
   Cloud:  Azure.Storage.Queues 12.11.0, 
Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.14.0
   Other:  Asp.Versioning.Http 6.1.0, CommunityToolkit.Mvvm 8.0.0, 
Dotnet.ReproducibleBuilds 1.1.1, Microsoft.Extensions.Configuration.Abstractions
7.0.0-*, Microsoft.Extensions.DependencyInjection.Abstractions 7.0.0-*, 
Microsoft.Extensions.Hosting 7.0.0-*, Microsoft.Extensions.Http 7.0.0-*, 
Microsoft.Identity.Web 1.25.3 . (23 total)

 drill in:  trace a focused entry   (e.g. trace "PUT /feeds/{id}")

