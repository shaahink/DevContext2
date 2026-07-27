# Eval Repos

The expectation suite runs against real open-source .NET repositories to validate
DevContext's output quality. Clone these into `eval-repos/` at the pinned commits
below for stable, reproducible checks.

Two cohorts live here:

- **Expectation cohort** (below) — the original pinned repos driven by
  `eval/expectations/*.json` in the gate battery's eval step.
- **The octet** (§Octet) — 8 repo *shapes* graduated from the 2026-07-17 unseen-lens
  audit (`eval-results/2026-07-17/lens-audit/AUDIT.md`). Pinned for the Prism phase;
  driven by `eval/lens-audit.ps1 <repo|octet>` and by aspirational expectation rows
  that flip to expected as Prism deliveries land.

## Setup

```powershell
mkdir eval-repos -Force | Out-Null

# eShop — reference microservices app
git clone https://github.com/dotnet/eShop.git eval-repos/eShop
pushd eval-repos/eShop; git checkout 9b4f9434f46fdc5c1a6e9e936af2868340cdbc48; popd

# TodoApi — David Fowler's Minimal API example
git clone https://github.com/davidfowl/TodoApi.git eval-repos/TodoApi
pushd eval-repos/TodoApi; git checkout 307a1eadbbd77a3004c318f2377e4818bc400af6; popd

# CleanArchitecture (VerticalSlice) — Steve Smith's template
git clone https://github.com/ardalis/CleanArchitecture.git eval-repos/VerticalSlice
pushd eval-repos/VerticalSlice; git checkout 74624fb0e45454c471b5ca00b13acbab9263cbf3; popd

# AutoMapper — object-mapping library
git clone https://github.com/AutoMapper/AutoMapper.git eval-repos/AutoMapper
pushd eval-repos/AutoMapper; git checkout b57c206dc7291821e42bdf816a5637a5c1d8cb54; popd

# FluentValidation — validation library (abstract-seat + extension-DI + fluent DSL)
git clone https://github.com/FluentValidation/FluentValidation.git eval-repos/FluentValidation
pushd eval-repos/FluentValidation; git checkout 943979089b55664ceb8390547ea1eb84ee99252a; popd

# Polly — resilience library (fluent builder + strategy options)
git clone https://github.com/App-vNext/Polly.git eval-repos/Polly
pushd eval-repos/Polly; git checkout 7a1d10f47e2ec667ceada49deb6bdd9a765753bd; popd

# CommunityToolkit.Mvvm — MVVM Toolkit (source generators + analyzers + marker attributes)
git clone https://github.com/CommunityToolkit/dotnet.git eval-repos/CommunityToolkit.Mvvm
pushd eval-repos/CommunityToolkit.Mvvm; git checkout b135626dd54d33b8f05f2ff31591592c004aa848; popd

# MediatR — mediator library (defines IRequestHandler/INotificationHandler/IPipelineBehavior; AddMediatR DI)
git clone https://github.com/jbogard/MediatR.git eval-repos/MediatR
pushd eval-repos/MediatR; git checkout 1fd25f5beb40aafd6859d9225a37d0c4f5062cfa; popd
```

## Pinned Commits

| Repo | Pinned SHA |
|------|-----------|
| eShop | `9b4f9434f46fdc5c1a6e9e936af2868340cdbc48` |
| TodoApi | `307a1eadbbd77a3004c318f2377e4818bc400af6` |
| VerticalSlice (CleanArchitecture) | `74624fb0e45454c471b5ca00b13acbab9263cbf3` |
| AutoMapper | `b57c206dc7291821e42bdf816a5637a5c1d8cb54` |
| FluentValidation | `943979089b55664ceb8390547ea1eb84ee99252a` |
| Polly | `7a1d10f47e2ec667ceada49deb6bdd9a765753bd` |
| CommunityToolkit.Mvvm | `b135626dd54d33b8f05f2ff31591592c004aa848` |
| MediatR | `1fd25f5beb40aafd6859d9225a37d0c4f5062cfa` |

## Octet (Prism phase — unseen-audit graduates, pinned 2026-07-17)

8 repo shapes spanning the ".NET repo" space, audited blind on `audit/library-round`
and pinned as the Prism regression corpus. Clones live in `eval-repos/<name>` like the
expectation cohort. Intended verdicts start as `"status": "aspirational"` expectation
rows and flip to `expected` as Prism D1 lands (see `PRISM-START.md`).

| Repo | Pinned SHA | Origin | Shape / intended verdict |
|------|-----------|--------|--------------------------|
| Newtonsoft.Json | `4f73e74372445108d2c1bda37b36e6f5e43402e0` | JamesNK/Newtonsoft.Json | Library (aux console ≠ App) |
| refit | `71634f2c5d0845c311b1cf4f4bb512437fe86fb5` | reactiveui/refit | Library (source-gen; CLI already PASS) |
| StackExchange.Redis | `0b03ed1d12a6a783873a44cd1f6fad3acf54395f` | StackExchange/StackExchange.Redis | Library (`toys/` = aux hosts) |
| wolverine | `7019b7d1b4520f84f90adbc6d407998c85e5e750` | JasperFx/wolverine | Framework-library (SelfNamePatterns) |
| GitVersion | `6476e5c478ec1b56a45914b3af4f6edcfd20deb0` | GitTools/GitVersion | CliTool (command-surface render) |
| dotnet-podcasts | `5ee8be2990b81eb681bbd100875c263aaa5ab68a` | microsoft/dotnet-podcasts | App: hub entry + MAUI present, grouped routes |
| ScreenToGif | `27a49c3be69486f2db964290f4f2274e790fb687` | NickeManarin/ScreenToGif | Desktop, MVVM style rung |
| bitwarden-server | `3e79593151787eb94853cb29420530d32f9b543c` | bitwarden/server | App: per-service styles ≤2/17 Unknown, hub entry |

Re-clone a missing octet repo at its pinned SHA (shallow — some are large):

```powershell
# Example: bitwarden-server. Substitute <name>, <origin>, <sha> from the table.
mkdir eval-repos/bitwarden-server; pushd eval-repos/bitwarden-server
git init; git remote add origin https://github.com/bitwarden/server.git
git fetch --depth 1 origin 3e79593151787eb94853cb29420530d32f9b543c
git checkout FETCH_HEAD
popd
```
