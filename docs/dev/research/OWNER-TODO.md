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

The tag was pushed from this session. It triggers `release.yml`, which builds on `windows-latest`
and takes a while (Tauri bundling dominates).

- Watch: `gh run list --workflow=release.yml` · `gh run watch <id>`
- Expect: CLI job (build → fast test suite → pack), Desktop job (NSIS + MSI installers, versioned
  from the tag), then a GitHub Release with the `.nupkg` and both installers attached.
- **Verify the installer version reads `1.0.5`, not `0.1.0`.** That was a real defect once
  (Prism H3): the version came from `tauri.conf.json`'s default instead of the tag. The workflow
  now runs `scripts/set-tauri-version.mjs` from the tag, but it is worth one look on a real release.

If the run fails, nothing is published — the Release job only runs for a tag and after both build
jobs succeed. Fix and re-tag; no cleanup needed on nuget.org because nothing was pushed there.

---

## 3. Branch state — what I did and did not do

| Branch | State |
|---|---|
| `feat/graph-v2` | pushed, merged |
| `develop` | **merged and pushed** — the default branch, 121+ commits of graph-v2 |
| `main` | **fast-forwarded to develop and pushed** — it had been 379 commits behind since the WPF era, with nothing unique |
| `v1.0.5` | tagged and pushed |

You asked for the full sync, so develop and main both moved. Note that `main` had been stale since
2026-06 and its old tip (`b633746`) is still reachable from the tag history if you ever need it.

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
