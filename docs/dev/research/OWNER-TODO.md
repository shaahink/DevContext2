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

## 5. Things I deliberately left alone

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
