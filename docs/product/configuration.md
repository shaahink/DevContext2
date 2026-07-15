# Configuration Guide

Verified against `src/DevContext.Cli/Services/DevContextConfig.cs`.

## `devcontext.json`

Place a `devcontext.json` at your project root for persistent settings. The CLI loads it from the
current directory; CLI flags override it. Only the fields below are recognised.

```json
{
  "excludePatterns": [".git", "bin", "obj", ".vs", "node_modules", ".idea", "Migrations"],
  "entryPaths": ["src/Api"]
}
```

The repo's own `devcontext.json` is a good minimal example — it sets only `excludePatterns`.

## Fields

| Field | Type | Status | Description |
|-------|------|--------|-------------|
| `excludePatterns` | `string[]` | **active** | File/dir name patterns skipped during the file-tree scan (case-insensitive substring match). |
| `entryPaths` | `string[]` | **active** | Restrict analysis to these directories/files (e.g. `["src/Api"]`). |
| `maxOutputTokens` | `int` | legacy | Validated to 100–100000, but the token budget is retired for Map/Trace — it affects only the legacy JSON/HTML catalog. See note below. |
| `defaultScenario` | `string` | legacy | Validated against the registered scenarios; the scenario/profile model is retired in favour of `--focus`. |
| `defaultProfile` | `string` | legacy | `quick` \| `focused` \| `debug` \| `full`. Legacy — see note. |
| `profiles` | `object` | legacy | Named overrides, each `{ profile, maxOutputTokens, noRoslyn }`. Legacy. |

> **Legacy fields note.** `maxOutputTokens`, `defaultScenario`, `defaultProfile`, and `profiles` are
> still parsed and range/enum-validated (a bad value is an error), but the token-budget + scenario/
> profile model has been retired at the CLI (the matching flags are hidden no-op stubs). For current
> Map/Trace output they have no effect. Prefer `--focus` / `--detail` on the CLI. (Tracked in
> `docs/dev/NOTABLE-FINDINGS.md` — the config schema is mid-migration.)

## Exclude patterns

```json
{
  "excludePatterns": [".git", "bin", "obj", ".vs", "node_modules", ".idea", "Migrations", "wwwroot/lib", "Generated"]
}
```

Matched against file/directory names (case-insensitive substring). Excluding `Migrations`, generated
folders, and vendored assets keeps the Map focused and the analysis fast.

## CLI flags override config

```bash
devcontext analyze .                 # uses devcontext.json in the current dir
devcontext analyze . --no-roslyn     # flag wins over config
```

## Validation

`DevContextConfig.Validate()` rejects an out-of-range `maxOutputTokens`, an unknown `defaultProfile`,
or a `defaultScenario` not in the registered set. An unparseable `devcontext.json` is ignored (treated
as absent) rather than fatal.

## Desktop settings

The desktop app (`DevContext.App`, Angular/Tauri) manages its own appearance/analysis/storage settings
in its Settings page — it does not read `devcontext.json`. (The former WPF desktop's
`%LocalAppData%\DevContext\settings.json` is gone with that app.)
