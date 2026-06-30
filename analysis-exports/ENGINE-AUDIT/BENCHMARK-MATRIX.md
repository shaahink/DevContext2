# Benchmark Matrix — "go-to for any .NET repo"

The scoreboard. Each row is a real repo of a distinct archetype; each column is one of the three
at-a-glance questions plus the dive-in. We drive every cell to ✅. New archetypes (bottom) extend coverage —
the tool isn't "go-to for any .NET repo" until the common shapes are all green.

Legend: ✅ good · ⚠️ partial/misleading · ❌ wrong/empty · — n/a. IDs (W#) link to `WORKITEMS.md`.

## Tier 1 — captured this audit (`analysis-repos/`, outputs in `../<repo>/`)

| Repo | Archetype | (1) What is this? | (2) Entry points | (3) Trace/focus dive-in | Open gaps |
|------|-----------|-------------------|------------------|-------------------------|-----------|
| **Serilog** | Library | ✅ clean library surface (facade/builder/abstractions ranked) | — (library) | ✅ `--focus Log` → 153-line trace | — |
| **Ocelot** | API gateway | ✅ Gateway + ROUTES section | ✅ HTTP (3) with targets | ✅ `POST /configuration` trace | — |
| **Files** | Desktop (WinUI 3) | ✅ MAP Files (App) + UI(69) entries | ✅ MainPage, App, [RelayCommand] etc. | ⚠️ App entry trace is shallow | — |
| **aspnetcore** | Framework / monorepo | ✅ topology capped at 50 + disclosure | ✅ W1: 518→10 | ✅ L2: no test DbContext | — |

## Tier 2 — existing eval repos (`eval-repos/`, registered in `eval-repos.json`)

| Repo | Archetype | At-a-glance | Dive-in | Notes |
|------|-----------|-------------|---------|-------|
| **eShop** | Microservices + CQRS/Aspire | ✅ | ✅ flagship trace (`POST /api/orders/` → command → handler → raises → outbox) | `/draft` command-from-param needs P3 semantic |
| **TodoApi** | Minimal API | ✅ | ✅ `POST /todos/` → `TodoDbContext` | clean |
| **VerticalSlice** | FastEndpoints + Clean Arch | ✅ | ✅ `POST /Products` → CreateEndpoint | |
| **AutoMapper** | Library | ⚠️ style "NLayer" | — | archetype-D surface map aspirational |
| **DntSite** | Blazor SSR + API controllers | ✅ controllers/EF | ✅ | good controller-app coverage |
| **CleanArchitecture / FluentValidation / Polly / CommunityToolkit / catalog** | mixed (CQRS / lib / lib / toolkit / controller) | ✅ substrate | — | JSON-substrate evals only |

## Tier 3 — archetype coverage gaps (🔬 add repos + expectations to be truly "any .NET repo")

Each is a common shape we have **no benchmark for**. Adding one = clone a small canonical repo, register it in
`eval-repos.json` + `eval/expectations/`, capture Map+Trace, and record the gap here.

| Archetype | Why it matters | Candidate repo |
|-----------|----------------|----------------|
| **Console / CLI tool** | `Main`-rooted apps; System.CommandLine verbs as entry points | a System.CommandLine sample, or **this CLI itself** |
| **Worker / BackgroundService** | hosted-service entry ladder rung; queue consumers | a `dotnet new worker` + MassTransit sample |
| **gRPC service** | service methods as entry points (not HTTP routes) | grpc-dotnet examples |
| **Blazor (WASM & Server)** | component lifecycle / `@page` routes as entries | a Blazor sample app |
| **MAUI / Avalonia** | cross-platform desktop (validates W5 beyond WinUI) | an Avalonia sample |
| **Classic MVC (Razor views)** | controller→view actions, no service call (validates **W8**) | a Razor MVC sample |
| **Serverless (Azure Functions / Lambda)** | `[Function]`/handler attributes as entries | an Azure Functions isolated sample |
| **Class library / NuGet (non-AutoMapper)** | second library data point for the surface engine | Humanizer / Newtonsoft |

**Success bar:** for each Tier-1/2/3 row, the Map answers (1) and (2) without noise, and at least one
`--focus` produces a non-empty, faithful trace for (3).
