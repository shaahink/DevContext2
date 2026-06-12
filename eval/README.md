# Eval Repos

The expectation suite runs against real open-source .NET repositories to validate
DevContext's output quality. Clone these into `eval-repos/` at the pinned commits
below for stable, reproducible checks.

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
```

## Pinned Commits

| Repo | Pinned SHA |
|------|-----------|
| eShop | `9b4f9434f46fdc5c1a6e9e936af2868340cdbc48` |
| TodoApi | `307a1eadbbd77a3004c318f2377e4818bc400af6` |
| VerticalSlice (CleanArchitecture) | `74624fb0e45454c471b5ca00b13acbab9263cbf3` |
| AutoMapper | `b57c206dc7291821e42bdf816a5637a5c1d8cb54` |
