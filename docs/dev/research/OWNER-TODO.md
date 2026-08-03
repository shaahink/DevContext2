# Owner TODO — after the graph-v2 close and the v1.0.5 release

_Written 2026-08-02, at the end of the session that closed the autonomous run (22/22) and cut the
release. Everything here needs an account, a credential or a judgement call that is yours._

---

## 1. NuGet — the CLI is not published, and that is now the only reason the README is awkward

**State, verified today:**

| Check | Result |
|---|---|
| `NUGET_API_KEY` secret on the repo | **absent** (`gh secret list` is empty) |
| `DevContext.Cli` on nuget.org | **unclaimed** — `https://www.nuget.org/packages/DevContext.Cli` is a 404 |
| Package metadata (`src/DevContext.Cli/DevContext.Cli.csproj`) | complete — id, MIT licence, description, tags, README, `PackAsTool` with `devcontext` as the command |
| Release workflow | packs the `.nupkg` and attaches it to the GitHub Release; skips the push and now says so in the run log |

So the package is ready and the id is free. Nobody has to change any code.

**To turn it on:**

1. Sign in at <https://www.nuget.org> with the Microsoft account you want to own the package.
2. Reserve the id: **API Keys → Create** — Glob pattern `DevContext.*`, scopes *Push new packages
   and package versions*. (Reserving `DevContext.*` also stops anyone else taking `DevContext.Core`
   etc. later.)
3. `gh secret set NUGET_API_KEY --repo shaahink/DevContext2` and paste the key.
4. Re-run the Release workflow for the tag, or push the next tag. The push step is already correct
   and will now execute — a bug that would have kept it silently skipped was fixed in this session
   (a step's own `env:` block is not visible to that step's own `if:`).
5. Then simplify the README back to `dotnet tool install -g DevContext.Cli` in the two places that
   currently document the `--add-source` workaround (the surfaces table and §Quickstart → CLI).

**Judgement call for you:** the NuGet key is a publish credential on a public repo. If you would
rather not hold it in Actions, the alternative is publishing manually from your machine
(`dotnet nuget push`) and leaving the workflow as-is — the `.nupkg` is attached to every release
either way, so users are never blocked.

---

## 2. The v1.0.5 release — check it landed

**Done — but it caught a real bug on the way out. Read this one.**

The run (`30749883156`) went green on all three jobs and published
<https://github.com/shaahink/DevContext2/releases/tag/v1.0.5>. The Prism H3 concern is clear: the
installers are versioned from the tag (`DevContext_1.0.5_x64-setup.exe`), not `tauri.conf.json`'s
default.

**A green release shipped without its installers.** v1.0.5 was published with only the `.nupkg`
attached, while both installers sat in the run's artifacts — and nothing failed, because attaching
zero files was not an error. The cause: `upload-artifact` roots an artifact at the *least common
ancestor* of its path list. The CLI job passes one glob (`nupkg/*.nupkg`) so its file lands flat;
the desktop job passes two (`bundle/nsis/*.exe` and `bundle/msi/*.msi`) so the artifact keeps
`nsis/` and `msi/` as directories. The release step's flat `artifacts/*.exe` matched neither.

Fixed in `.github/workflows/release.yml`: recursive globs, plus `fail_on_unmatched_files: true` so
a missing installer fails the release instead of shipping quietly. **The fix is not in the v1.0.5
tag** — it landed after it. The two installers were uploaded to v1.0.5 by hand, so that release is
complete; the next tag exercises the fixed path.

This is worth one look on your side, because it is the class of failure this project keeps finding:
the surface reported success while the thing it promised was absent, and the README tells people to
download exactly those installers.

---

## 3. Branch state — what I did and did not do

| Branch | State |
|---|---|
| `feat/graph-v2` | pushed — tip `aec54d7` |
| `develop` | **merged and pushed** — clean fast-forward, gained **141 commits**. Default branch. |
| `main` | **fast-forwarded to develop and pushed** — gained **521 commits**; it had nothing of its own |
| `v1.0.5` | annotated tag at `aec54d7`, pushed — triggered the Release workflow |

You asked for the full sync, so develop and main both moved. `main` had been stranded at
`b633746` since the WPF era (2026-06); that commit is still reachable if you ever need it
(`git log b633746`), and nothing was rewritten to get there — it was a pure fast-forward.

**The full gate battery was run green on the merged tree before any of this** (`eval/gates.ps1`,
exit 0, eval included) — evidence at `eval-results/2026-08-02/wrapup/gates-full.txt`. The
conductor's own final phase gate was also green, but that was on the pre-wrap-up tree.

**Stale branches worth deleting when you're ready** — there are ~20 local branches, several already
merged (`feat/github-ready`, `feat/lighthouse-*`, `feat/library-surface-fv-polly`, `audit/*`,
`chore/housekeeping*`). I left every one of them alone: deleting branches is cheap to do and
annoying to undo, and none of them costs anything by existing. `git branch --merged develop` lists
the safe ones.

---

## 4. The work itself — where to start next

**Read `docs/dev/research/PLAN.md` §2 STATUS first.** It is written to be read cold and now carries
the close-out. Two pointers that matter more than the rest:

- **`docs/dev/research/BUG-BACKLOG.md` — 24 open findings, 7 high.** These are measured and
  evidenced, filed rather than fixed because each was a product decision. This is a better starting
  point than any fresh audit. Read #8 and #11 together — the second refutes the explanation offered
  for the first, which is exactly the kind of thing that gets fixed wrongly if read alone.
- **S11 is the next owner-interactive session**: D-F (insight dedup, engine-side), D-G (Studio),
  D-H. C-2/C-3 and D-3/D-4 were delivered autonomously as G5–G7, so the R3 remainder is smaller
  than the plan text suggests.

**Three product gates have never fired on any measured repo** (backlog #22/#23/#24). Each is now
honest about its own measurement at the call site, but the keep-or-retire decision is yours —
`graph.orphans` in particular is the one claim in the product that gets live code deleted when it
is wrong, and lowering its floor to make it start firing would not be a threshold correction.

---

## 5. One product decision I did not make for you

`WorkspaceStore.createTab()` (`src/DevContext.App/src/app/state/workspace.store.ts:174`) returns the
**active tab's id** when already at `MAX_TABS` (6), instead of creating one:

```ts
if (existing.length >= WorkspaceStore.MAX_TABS) return this._activeId() ?? existing[0]?.id ?? '';
```

The behaviour is documented and deliberate, but the caller gets back a plausible tab id and cannot
tell it apart from success — so anything keyed by that id (the trail slice, for one) silently writes
into the wrong tab. That is what made the CI failure in §2's neighbourhood so confusing to read.

I left it alone: making a silent no-op loud is a product decision, not a test fix. If you want it
changed, the options are returning `null` at the cap, or surfacing "tab limit reached" in the UI.
Worth deciding, because it is the same shape as several entries in `BUG-BACKLOG.md` — a surface
reporting success while doing nothing.

---

## 6. Every workflow action is on a deprecated Node — green today, on borrowed time

The green run at `0bddb3f` surfaced this on **every job in all three workflows**:

> `##[warning]` Node.js 20 is deprecated. The following actions target Node.js 20 but are being
> forced to run on Node.js 24 —
> [changelog](https://github.blog/changelog/2025-09-19-deprecation-of-node-20-on-github-actions-runners/)

Nothing is broken. The runner silently reruns each Node-20 action on Node 24, which is why CI is
green — but that also means every action in this repo is already executing on a runtime its pinned
major was never built against, and the shim is what is holding it up.

**How far behind the pins are** (checked 2026-08-02):

| Action | Pinned | Latest | Used by |
|---|---|---|---|
| `actions/checkout` | v4 | **v7.0.1** | ci, eval, release |
| `actions/setup-dotnet` | v4 | **v6.0.0** | ci, eval, release |
| `actions/setup-node` | v4 | **v7.0.0** | ci, eval, release |
| `actions/upload-artifact` | v4 | **v7.0.1** | release |
| `actions/download-artifact` | v4 | **v8.0.1** | release |
| `pnpm/action-setup` | v4 | **v6.0.9** | ci, eval, release |
| `softprops/action-gh-release` | v2 | **v3.0.2** | release |

**Three of the breaking changes I checked do not apply to us** — worth recording so nobody
re-derives it:

- `setup-dotnet@v5` dropped older .NET SDKs. We pin `10.0.x` (`global.json`: `10.0.300`), the
  newest — unaffected.
- `setup-node@v5` auto-caches when `packageManager` is present, and `v6` narrowed that to npm.
  There is no root `package.json`, and every call site already sets `cache: pnpm` with an explicit
  `cache-dependency-path` — auto-detection never gets a say.
- `checkout@v7` blocks fork-PR checkout for `pull_request_target` / `workflow_run`. `ci.yml` uses
  plain `pull_request` — unaffected.

**The one that needs your call is `release.yml`.** `download-artifact@v5`'s breaking change is
scoped to single downloads *by ID*, and we download by path with `merge-multiple: true` — so the
documented break misses us. But the artifact plumbing in that job is exactly what shipped v1.0.5
without its installers, and the fix at `5100e90` is written against **v4's** least-common-ancestor
rooting behaviour. Bumping upload/download-artifact there re-opens the one thing that has already
failed once in production.

And it cannot be smoke-tested: the `workflow_dispatch` dry-run uploads artifacts but the `release`
job is gated `if: github.ref_type == 'tag'`, so the download-and-attach step — the step that broke —
is unreachable without cutting a real tag.

**Suggested split, if you want it done:**

1. Bump `ci.yml` and `eval.yml` freely. CI verifies them on the next push; the blast radius is a red
   build you can revert.
2. Bump `release.yml` separately, and verify by cutting `v1.0.6` (or a throwaway pre-release tag)
   and confirming the `.nupkg` **and both installers** are attached. Do not batch it with step 1 —
   if the release breaks again you want the bisect to be one commit wide.

No deadline has been announced for removing the Node 20 shim, so this is not urgent. It is logged
because the warning will now appear on every run and is easy to learn to ignore.

## 7. Things I deliberately left alone

- **The 24 backlog items** — all product decisions, none safe to take unilaterally.
- **`conductor.plan.json`** — left in the repo, pointing at the archived tracker. It is the machine
  record of how the run was driven and is worth keeping until the next phase is planned.
- **`.conductor/` run state** (`run.db`, transcripts, ~1 GB of logs) — gitignored and untouched.
  The parts worth keeping are already extracted: `BUG-BACKLOG.md` and the evidence under
  `eval-results/`. Delete the directory whenever you like.
- **`FIELD-NOTES-2026-07-29-devcontext.md`** at `C:/Code/conductor/docs/dev/` — 20+ entries on
  conductor's own limitations, uncommitted, in the *conductor* repo not this one. You asked for it;
  it is not mine to commit there.
- **Stale local branches** — see §3.
