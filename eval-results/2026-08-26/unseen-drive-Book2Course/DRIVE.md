# Unseen-repo hand drive — Book2Course

**Does the MCP earn its place against `Read`/`Grep` on a repo the engine has never seen?**

2026-08-26. Target: `C:/Code/BookToCourse` @ `c14da997`, a private .NET 10 Aspire app
(`apps/app/Book2Course.slnx` — Api · AppHost · Domain · Pipeline · ServiceDefaults, 487 C# files,
plus 4 test projects). DevContext build under test: `develop` @ `b7b0ab0`, `dotnet build
DevContext.slnx` green (0 warnings).

This is **not** the pre-registered re-probe (`eval/agent-probe/DESIGN.md` §8, still unrun). It is a
hand drive: one operator, five questions fixed in advance, answered twice — once with grep/read
only, once through `devcontext-mcp` over stdio. No blind judge, no reps, no arm isolation. It
settles *where to look*, not *by how much*.

Why this repo: DESIGN wanted an unseen repo and budgeted for "a mechanically identifier-renamed
public repo" as the affordable stand-in. This one is genuinely unseen, is not in any training set,
and its owner can adjudicate ground truth directly. It is also **unusually well commented** — XML
docs carrying decision references (D7, D21, D27, D33, D39, D45) — which is a headwind for the lens,
because grep does well on documented code. Read every "grep won" below with that asterisk.

## The questions, fixed before either arm ran

| # | Class (DESIGN §3.3) | Question |
|---|---|---|
| Q1 | A — orientation | New to this repo: what are the entry points and what shape is it? |
| Q2 | B — indirection | A user uploads a source. What runs, end to end, endpoint → pipeline stage? |
| Q3 | C — impact | If I change `Domain/Runs/Run.cs`, what breaks? |
| Q4 | seam | How does `SourceUploadEndpoints` reach `IngestStage`, hop by hop? |
| Q5 | F — control, grep should win | Where is the job queue's polling interval configured? |

## Scoreboard

| # | grep/read | the lens | winner |
|---|---|---|---|
| Q1 | entry list + Aspire topology in 4 reads | correct shape, **broken `startHere`** (F1) | draw, with a defect |
| Q2 | full chain incl. the async half, ~6 calls | 31 hops in 0.3s **with a DI resolution**, silent on the async half | lens on depth, grep on completeness |
| Q3 | 67 files match `\bRun\b`; 137 match the namespace — **no answer** | **48 affected, grouped by project, each with file:line** | **lens, decisively** |
| Q4 | worked out the queue-mediated path by reading 3 files | `found:false`, twice (F4) | grep |
| Q5 | exact answer: `Pipeline:Queue:Drain`, `IdlePollSeconds` = 2 | "1 keys exist" (F3) | grep, as designed — but see F3 |

`analyze` on the repo **root** found the nested `.slnx` by itself: 45.2s cold, 1315 nodes / 1739
edges / 41 entries / 5 projects. Warm calls thereafter ran 0.0–1.7s. That part simply works.

## What the lens won

**Q3 — impact. This is the finding.** `impact(Type:Book2Course.Domain.Runs.Run, direction:up)`
returned **48 affected symbols in 0.4s**, grouped by project, each with kind, hop distance and
file:line: `ModelCall`, `RunEvent`, `ActivityFeedProjector.RunEntries`, `AccountingRollup.Runs`,
`CourseCreator.CreateAsync`, `OutlineApprovals.ContinueOrOpenAsync`, `OutlineRevisionCaps.SpentAsync`.
Five sampled at random all genuinely reference `Run`. Grep's answer to the same question was 67
files containing the token and 137 touching the namespace — a haystack, not an answer.

**This is the question that scored 0/3 in the pilot.** Class C was arm M's total failure and the
deep eval mapped it onto backlog #7/#8/#11/#12. Those are fixed, and on an unseen repo the question
now answers. That is the single strongest piece of evidence the product has.

**Q2 — `trace` resolves DI.** 31 steps, ~2.1k tokens, every hop file:line-cited, `[approx]` markers
on unverified edges, `4 omitted` declared with the lever that undoes it. The hop grep cannot make:

```
→ Type: IObjectStore   …/Api/Storage/SourceUploads.cs:82
  ◇ Type: S3ObjectStore  …/Api/Storage/StorageModule.cs:27     ← registration, resolved
→ Member: S3ObjectStore.PutAsync
```

Grep hands you the interface and stops. The lens crosses to the implementation and names the
registration site.

**The honesty ledger is real.** `stats` reports the seam breakdown per kind:
`Calls` 1594 total = 650 verified · 44 joined · **900 approx**. So **56% of this repo's call edges
are approximate**, said out loud, unprompted. (The deep eval measured 80% on the engine's own graph
before W2; 56% is a real improvement and still the majority.) `resolve` refuses to guess — `Run`
came back `ambiguous:true` with 8 candidates, each with file:line and in/out degree. `seam` returning
`found:false` said *why*, and was telling the truth.

## What the lens got wrong

### F1 · HIGH · Extension and BCL methods are bound as members of the receiver type

`overview.startHere` — the first line an agent reads — is:

```
Start here: AppDbContext, AppDbContext.ConfigureAwait,
            AppDbContext.Where, AppDbContext.IgnoreQueryFilters
```

`AppDbContext` declares **none** of those (`Api/Data/AppDbContext.cs` — its members are a
constructor, 13 `DbSet` properties, `ConfigureConventions`, `OnModelCreating`). `ConfigureAwait` is
`Task`'s; `Where`/`Select`/`ToListAsync`/`FirstOrDefaultAsync`/`IgnoreQueryFilters` are LINQ/EF
extension methods. The engine mints `Member:…AppDbContext::ConfigureAwait` by binding the call to
the *receiver's* type, then ranks by degree — so the noise sorts to the top (`::ConfigureAwait` 72
connections, `::Where` 53, `::IgnoreQueryFilters` 46) and displaces real starting points.

Not confined to ranking: the same nodes appear inside traces —
`→ Member: SourceUploads.ConfigureAwait [approx]`, `→ Member: S3ObjectStore.ConfigureAwait [approx]`.

Same family as #7 (a METHOD registered as a Type node with 26 BCL references bound to it) and #12
(receiver-type binding), both marked FIXED — this is a **residual of that family on the extension
-method path**. It lands on class A, which the deep eval names as winnable ground.

Invariant worth gating: *no node may be a member of a type that does not declare it.*

### F2 · HIGH · The auth insight misses `MapGroup` and cries wolf on the safest surface

> **FIXED 2026-08-26, re-measured on the same repo.** `auth.anonymous` went **12/39 → 6/39**
> and `web.auth-surface` **27 protected → 33**. The six that remain are genuinely public and
> check out in source: `POST /auth/register`, `POST /auth/login` (you cannot require auth to log
> in), `GET /auth/google` (the OAuth challenge), `GET /` (the banner) and `POST /client-events`
> (the repo's one real `.AllowAnonymous()`). `POST /auth/logout` and `GET /me` carry
> `RequireAuthorization` and are correctly no longer flagged.
>
> **The diagnosis below is right about the symptom and wrong about the cause** — `MapGroup`
> inheritance was already implemented (the E1 work). Two things defeated it, and both had to go:
>
> 1. **The group prefix accepted only a string LITERAL.** `app.MapGroup(GroupPrefix)` with
>    `internal const string GroupPrefix = "/admin"` left `prefix` null, so `ExtractGroupPrefixes`
>    `continue`d and never registered the group variable at all. `ExtractGroupAuth` gates on
>    `groupPrefixes.ContainsKey`, so losing the prefix silently lost the auth with it. Naming your
>    group's prefix with a constant — the idiomatic way to write it — cost the surface its policy.
>    The tell was in the original evidence and I read past it: the routes printed as
>    `GET /accounting`, not `GET /admin/accounting`. The prefix was already missing.
>    Fixed by resolving const references through the project-wide const index that already existed
>    for FastEndpoints routes (G2), bare and qualified.
> 2. **The B3 cross-file index carried the prefix but not the auth.** The policy is declared in
>    `AdminEndpoints.cs`; the routes live in `AdminPipelineEndpoints.cs` / `AdminPromptEndpoints.cs`
>    / `AdminLedgerEndpoints.cs`, reached via `admin.MapAdminLedgerEndpoints()`. Fixing (1) alone
>    moved the routes to `/admin/*` and left the count at 12/39 — a group is one surface, and its
>    authorization had to travel with its prefix. The index value is now
>    `CallerGroupContext(Prefix, Auth)`, under the same all-call-sites-agree rule.
>
> Four tests in `EndpointExtractorTests`, including the exact two-file shape and a guard that a
> non-constant prefix is still never guessed at. Core suite 904/904.



`stats` insight `auth.anonymous`, severity `warning`, confidence 0.69:

> **12/39 endpoints anonymous, incl. 4 POST/PUT/DELETE**
> evidence: `GET /accounting`, `GET /stats`, `GET /pipeline`, `GET /prompts`, `GET /prompts/{key}/versions`

Every endpoint named is in the admin group, and `Api/Endpoints/AdminEndpoints.cs:18`:

```csharp
var admin = app.MapGroup(GroupPrefix)
    .RequireAuthorization(Policies.Admin)
```

They are protected by the **strictest policy in the application**. The detector reads per-endpoint
`.RequireAuthorization()` and does not follow `MapGroup` inheritance. The repo contains exactly one
real `AllowAnonymous` (`ClientEventEndpoints.cs:22`), so the true count is nowhere near 12.

A confident, evidence-bearing, wrong-in-the-dangerous-direction security claim is worse than no
insight. `web.auth-surface` ("11 unannotated of 39") is the same miss wearing a second hat — and
two counters for one fact is the #19/#20 shape again.

### F3 · HIGH · `config` does not know the Options pattern

`config(key:"Pipeline:Queue:Drain")` → `No config key exactly 'Pipeline:Queue:Drain' (1 keys exist)`,
candidates: `["OTEL_EXPORTER_OTLP_ENDPOINT"]`. **One key, in this repo.**

The repo binds config the modern way — `Pipeline/DependencyInjection.cs:73`:

```csharp
services.AddOptions<QueueDrainOptions>()
    .BindConfiguration(QueueDrainOptions.SectionName)   // "Pipeline:Queue:Drain"
```

…with `[Range]`-validated properties (`IdlePollSeconds` = 2, `Workers` = 2, `Enabled`), plus an
env-var contract in `infra/.env.example` and `Storage__*` / `Pipeline__Media__*` wiring in the
AppHost. The detector sees only `IConfiguration` / `GetValue` / `GetSection` call sites.

It propagates: insight `config.missing-defaults` reports "1 consumed keys" off the same blind spot,
so the catalog **under-declares and doesn't say so** — the mirror of the "catalog over-declares"
class in `GRAPH-DETECTION-AUDIT-2026-08-13.md`.

### F4 · HIGH · `seam` cannot cross a transport — the port is a sink, not a bridge

Two of five questions hit this.

```
seam(SourceUploadEndpoints → IngestStage)  →  found:false
seam(BuildCoordinator      → IngestStage)  →  found:false
  "…the walk exhausted everything reachable from each end within 8 hops
   and neither reached the other."
```

The first is *truthful* — upload only stages the file, ingest happens later off the queue. The
second is a **miss**, and `neighbors(IJobQueue, direction:in)` shows exactly why. The join is in the
graph, fully verified:

| from | kind | resolution | provenance |
|---|---|---|---|
| `BuildCoordinator.AdvanceAsync` | Calls | **Semantic** | `Pipeline/Workflow/BuildCoordinator.cs:34` |
| `BuildCoordinator.CancelAsync` | Calls | **Semantic** | `Pipeline/Workflow/BuildCoordinator.cs:61` |
| `JobRunner.RunNextAsync` | Calls | **Semantic** | `Pipeline/Workflow/JobRunner.cs:82` |
| `JobRunner.RunAsync` | Calls | **Semantic** | `Pipeline/Workflow/JobRunner.cs:106` |
| `JobRunner.CompleteAsync` | Calls | **Semantic** | `Pipeline/Workflow/JobRunner.cs:199` |

Producer and consumer both point **into** `IJobQueue`; nothing points **out** of it. So the
transport node has in-degree 8 and out-degree 0 — a sink. No path can route through it, and `seam`
correctly reports no path across a connection that plainly exists.

This is the **handler-join** cell of the graph-truth matrix, and it is not a corner case: it is
every queue-, bus- and outbox-driven .NET app. `seam` is advertised as "the only tool that answers
how does A reach B" — on this architecture it answers it only within a single process hop.

### F5 · LOW · `usages` returns duplicate identical edges

`usages(JobRunner)` → `count: 3`, all three the same caller, same kind, same line
(`QueueDrainService.TurnAsync`, `QueueDrainService.cs:95`). One call site, counted three times.

## The single sentence

**F1, F2 and F3 are one defect: the detector reads the direct spelling and misses the compositional
one** — the extension method instead of the declared member, `MapGroup(...).RequireAuthorization()`
instead of the per-endpoint call, `AddOptions<T>().BindConfiguration()` instead of `GetSection`.
Every one of them under- or mis-declares while sounding certain. F4 is a different shape: the graph
holds the truth and the query cannot route through it.

## What this licenses

- **The impact claim is earned on an unseen repo.** Q3 is a real, checkable win over grep, and it is
  the question the pilot lost. That is the demo to lead with.
- **`trace`'s DI hop and the approx/verified ledger are earned.** Both do something grep cannot.
- **Nothing here licenses "the MCP beats grep".** On five questions it won one decisively, added
  depth on one, and lost three — two of those to defects, not to grep being better.
- **F2 is release-blocking on its own.** A tool that tells an owner their admin surface is anonymous,
  when it is the most protected surface they have, loses the trust the whole lens runs on.
- **The pre-registered re-probe should not be run until F1–F4 are filed and fixed.** The pilot's
  lesson was that it measured a surface with known-broken parts; running the $150–250 study against
  a `startHere` full of `ConfigureAwait` and a `seam` that cannot cross a queue would repeat it.

## Reproducing

Driver: `scratchpad/mcp.js` (stdio JSON-RPC; same shape as `eval/mcp-qa/drive-generic.js`).
Call batches and full transcripts: `calls-analyze.json` / `analyze-out.txt`,
`calls-q.json` / `q-out.txt`, `calls-q2.json` / `q2-out.txt`.
