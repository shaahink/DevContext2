# A5 — "the server ignores devcontext.json", measured instead of inherited

R4 §2 Task 3 carries a parenthetical: *"Note: server currently ignores devcontext.json — CLI/MCP see
different file sets on this repo; fix or note before this task."* The stage brief told me not to
inherit it. Measured, it is **true in mechanism, wrong about the consequence, and it names the wrong
cause for a divergence that does turn out to be real.**

## The three runs

All at `HEAD 546fb32`, same machine, same session.

| run | reads devcontext.json | files | projects | nodes | edges | entries |
|---|---|---|---|---|---|---|
| CLI from `C:\code\DevContext2` | **yes** | 385 | 8 | **1254** | **1383** | 30 (27 targeted) |
| CLI from `C:\Temp\dc-nocfg` | **no** (none there) | 442 | 29 | **1254** | **1383** | 30 (27 targeted) |
| MCP `analyze("C:/code/DevContext2")` | **no** (server never reads it) | — | 5 | **1260** | **1398** | 30 (27 targeted) |

Artifacts: `cli-analyze-stats.txt`, `cli-analyze-nocfg-stats.txt`, `raw/001-analyze.json`.

## What that says, in order

**1. The server really does ignore it, and the source says why.** `DevContextConfig` is declared in
`src/DevContext.Cli/Services/DevContextConfig.cs` — a **CLI project type**. `AnalyzeCommand.cs:26` and
`QueryCommand.cs:62` load it. Grepping `DevContext.Server` for `DevContextConfig|ExcludePatterns`
returns **no matches**, so the server builds `ExtractionOptions` with
`ExtractionOptions.DefaultExcludePatterns`.

**2. The delta between the two pattern sets is only two entries.** The default set is already
`[.git, bin, obj, .vs, node_modules, .idea, .claude, eval-repos, analysis-repos]`
(`ExtractionOptions.cs:24`). This repo's `devcontext.json` adds exactly **`fixtures`** and
**`goldens`**. The mental picture the note invites — the server swallowing 47 eval repos — is not what
happens; `eval-repos` is excluded by the *default*.

**3. And those two patterns move no nodes and no edges.** Dropping `devcontext.json` moves the file
inventory 385 → 442 and the project count 8 → 29, and leaves the graph **byte-identical at 1254 nodes
/ 1383 edges / 27-of-30 targeted entries**. The reason is Batch C's solution scoping: the twenty-one
extra `.csproj` files are fixture projects outside `DevContext.slnx`, so `SolutionCatalog` already
excludes them from the graph. `devcontext.json`'s `fixtures`/`goldens` entries are, at the graph
level, **redundant with solution scope on this repo**.

> Honest caveat on the file count: ~6 of the 57 extra files are `eval-results/**` artifacts this
> session created between the two CLI runs (`eval-results` is excluded by neither pattern set). The
> project count 8 → 29 is the clean signal, and the node/edge equality is exact.

**4. So the CLI↔MCP divergence that DOES exist is not caused by devcontext.json.** CLI 1254/1383 vs
MCP 1260/1398 — **+6 nodes, +15 edges** — survives with the config-free CLI run producing the *CLI*
number, not the MCP one. Whatever separates the two surfaces is in the CLI-vs-server analysis path,
not in the exclude patterns. R4 §2's parenthetical points at the wrong cause. The divergence is small
(0.5% / 1.1%) but it is a real *two surfaces, one repo, different answers* case, and it is now
isolated: **not the config.**

## The defect this turned up on the way

`DevContextConfig.DefaultPath` (`DevContextConfig.cs:55`) is

```csharp
Path.Combine(Environment.CurrentDirectory, "devcontext.json")
```

— the **current working directory**, not the analysed repo root, and `AnalyzeCommand.cs:26` passes
exactly that. So:

- `devcontext analyze C:\repos\Other` run from `C:\code\DevContext2` applies **DevContext2's**
  exclusions to `Other`; and
- analysing DevContext2 from anywhere else silently ignores its config — which is precisely what run 2
  above demonstrates, with the file count moving by 57.

Neither case says anything. This is the same shape as the MCP defects this drive filed: a real
difference in what was analysed, reported with the confident vocabulary of a complete answer.

## Verdict for the R4 note

Rewrite it as: *the server does not read `devcontext.json` (it is a CLI-only type); on this repo that
costs nothing at the graph level because solution scoping already excludes the fixture projects, but
the CLI and the server still disagree by 6 nodes / 15 edges for a reason that is **not** the config,
and the CLI reads the config from the working directory rather than the repo root.*
