MAP  NetPodcast     (13 projects)

STACK  net7.0, net7.0-android;net7.0-ios;net7.0-maccatalyst, net7.0;net7.0-android;net7.0-ios;net7.0-maccatalyst · Minimal APIs · MediatR (CQRS) · EF Core

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api; MediatR with 6 handlers

       per service:
         Microsoft.NetConf2021.Maui: Unknown
         NetPodsMauiBlazor: Unknown
         Podcast.Server: Unknown
         ListenTogether.Hub: Unknown [EF Core]
         Podcast.API: Web API [EF Core]
         Podcast.Ingestion.Worker: Worker Service [Worker, EF Core]
         Podcast.Updater.Worker: Worker Service [Worker, EF Core]

TOPOLOGY (depends-on)
   ListenTogether.Domain
   Podcast.Infrastructure
   ListenTogether.Application ── ListenTogether.Domain
   Podcast.Shared
   ListenTogether.Infrastructure ── ListenTogether.Application, ListenTogether.Domain
   Podcast.Client ── Podcast.Pages
   Podcast.Components
   Podcast.Pages ── Podcast.Components, Podcast.Shared
   ListenTogether.Hub ── ListenTogether.Application, ListenTogether.Domain, ListenTogether.Infrastructure
   Podcast.API ── Podcast.Infrastructure
   Podcast.Ingestion.Worker ── Podcast.Infrastructure
   Podcast.Server ── Podcast.Client, Podcast.Shared
   Podcast.Updater.Worker ── Podcast.Infrastructure

ENTRY POINTS
   HTTP (12)
      DELETE /{id}  → FeedsApi  (src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:13)
      GET /  → ShowClient.CheckLink  (src/Services/Podcasts/Podcast.API/Routes/ShowsApi.cs:13)
      GET / [GetAllCategories]  → CategoriesApi  (src/Services/Podcasts/Podcast.API/Routes/CategoriesApi.cs:7)
      GET / [GetAllFeeds]  → FeedsApi  (src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:8)
      GET /{id}  → ShowsApi  (src/Services/Podcasts/Podcast.API/Routes/ShowsApi.cs:14)
      GET /{id} [GetEpisodeById]  → EpisodesApi  (src/Services/Podcasts/Podcast.API/Routes/EpisodesApi.cs:7)
      GET /Error  → ErrorModel  (src/Web/Server/Pages/Error.cshtml.cs:7)
      GET /landing  → PodcastService.GetShows  (src/Web/Server/Pages/Landing.cshtml.cs:7)
      POST /  → FeedsApi  (src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:7)
      PUT /{id}  → IFeedClient  (src/Services/Podcasts/Podcast.API/Routes/FeedsApi.cs:9)
      GET / [Program:20]  (src/Services/ListenTogether/ListenTogether.Hub/Program.cs:20)
      GET /version  (src/Services/Podcasts/Podcast.API/Program.cs:145)
   Background (1)
      Worker  → Worker  (src/Services/Podcasts/Podcast.Updater.Worker/Program.cs:6)
   UI (10)
      GET /categories  (src/Web/Pages/Pages/CategoriesPage.razor:1)
      GET /category/{id:guid}  (src/Web/Pages/Pages/CategoryPage.razor:1)
      GET /discover  (src/Web/Pages/Pages/DiscoverPage.razor:1)
      GET /listen-later  (src/Web/Pages/Pages/ListenLaterPage.razor:1)
      GET /listen-together  (src/Web/Pages/Pages/ListenTogetherPage.razor:1)
      GET /listen-together/create/{episodeId:guid}  (src/Web/Pages/Pages/ListenTogetherPage.razor:1)
      GET /listen-together/room/{roomCode}  (src/Web/Pages/Pages/ListenTogetherPage.razor:1)
      GET /settings  (src/Web/Pages/Pages/SettingsPage.razor:1)
      GET /show/{id:guid}  (src/Web/Pages/Pages/ShowPage.razor:1)
      GET /subscriptions  (src/Web/Pages/Pages/SubscriptionsPage.razor:1)
   SignalR (1)
      ListenTogetherHub (8 methods: OpenRoom, JoinRoom, LeaveRoom)  → GetRoomsQueryQueryHandler.HandleAsync  (src/Services/ListenTogether/ListenTogether.Hub/Hubs/ListenTogetherHub.cs:7)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer 7.0.0-*, Microsoft.AspNetCore.Components.Web 7.0.0-*, Microsoft.AspNetCore.Components.WebAssembly 7.0.0-*, Microsoft.AspNetCore.Components.WebAssembly.DevServer 7.0.0-*, Microsoft.AspNetCore.Components.WebAssembly.Server 7.0.0-*, Microsoft.AspNetCore.OpenApi 7.0.0-*, Microsoft.AspNetCore.SignalR.Client 7.0.0-*, OpenTelemetry.Exporter.Prometheus.AspNetCore 1.4.0-beta.2 … (10 total)
   ORM/Data:  Microsoft.EntityFrameworkCore 7.0.0-*, Microsoft.EntityFrameworkCore.Design 7.0.0-*, Microsoft.EntityFrameworkCore.Relational 7.0.0-*, Microsoft.EntityFrameworkCore.SqlServer 7.0.0-*, Microsoft.EntityFrameworkCore.Tools 7.0.0-rc.2.22472.11, OpenTelemetry.Instrumentation.EntityFrameworkCore 1.0.0-beta.3
   Logging:  Azure.Monitor.OpenTelemetry.Exporter 1.0.0-beta.4, OpenTelemetry.Exporter.Jaeger 1.4.0-beta.2, OpenTelemetry.Extensions 1.0.0-beta.3, OpenTelemetry.Extensions.Hosting 1.0.0-rc9.8, OpenTelemetry.Instrumentation.EventCounters 1.0.0-alpha.1, OpenTelemetry.Instrumentation.Http 1.0.0-rc9.8, OpenTelemetry.Instrumentation.Process 0.1.0-alpha.1, OpenTelemetry.Instrumentation.Runtime 1.0.0
   Cloud:  Azure.Storage.Queues 12.11.0, Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.14.0
   Other:  Asp.Versioning.Http 6.1.0, CommunityToolkit.Mvvm 8.0.0, Dotnet.ReproducibleBuilds 1.1.1, Microsoft.Extensions.Configuration.Abstractions 7.0.0-*, Microsoft.Extensions.DependencyInjection.Abstractions 7.0.0-*, Microsoft.Extensions.Hosting 7.0.0-*, Microsoft.Extensions.Http 7.0.0-*, Microsoft.Identity.Web 1.25.3 … (23 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "PUT /{id}")
