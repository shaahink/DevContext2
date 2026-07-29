# G4.2 Task 2 — the change, declared BEFORE the drive

**Repo:** `eval-repos/Hangfire` @ `333bd8e` (same pole as G4.1, so orientation cost is comparable).
**Rule:** orientation through MCP tools ONLY — no grep, no `Read` to find *where*. `Read`/`Edit` are
allowed only once I am writing the final code, which is what R4 §2 Task 2 specifies.

## The change

A background-job filter that logs how long each job took to run: `LogJobDurationAttribute`, an
`IServerFilter` that starts a timer before the job executes, and on completion logs the elapsed
milliseconds together with the job id and the method that ran.

Why this and not something invented: Hangfire is a `Library` archetype with **0 entry points**, so
"add an endpoint" (R4 §2's first suggestion) does not exist here. Its actual extension point — the
one the product is designed around, and the one G4.1's Q7 asked about — is the filter pipeline. A
duration logger is the smallest change that must be *correct against this repo's own conventions*
rather than against general .NET knowledge.

## The six facts I must obtain, declared now

Each is graded after the drive as **TOOL** (a tool asserted it), **INFERRED** (I assembled it from
names/snippets the tool returned), or **MISSED** (I had to get it some other way).

| # | fact I need before I can write a line |
|---|---|
| F1 | Which project, folder and **namespace** the server-side filter contracts live in |
| F2 | The exact interface to implement and its **method signatures** (names + parameter types) |
| F3 | Whether this repo's filters derive from a **base class**, and which |
| F4 | How state is carried from the before-hook to the after-hook (a job filter needs a stopwatch to survive between two calls) |
| F5 | How this repo **logs** — Hangfire vendors its own abstraction rather than using `Microsoft.Extensions.Logging`, and an agent that assumes `ILogger<T>` writes code that does not compile |
| F6 | A concrete **existing filter to model the file on**, named with a `file:line` |

## Acceptance

1. The new file compiles: `dotnet build` on the touched project, **exit code captured**, not eyeballed.
2. Every MCP call logged by `eval/mcp-qa/dogfood.js` and graded HELPED / NEUTRAL / HURT.
3. A grep control for the same six facts, counted in calls and tokens, so "did MCP shorten the path"
   is answered with numbers.
4. `eval-repos/Hangfire` is returned to a clean working tree — it is a graph-truth matrix pole.

## Declared confound

Same as G4.1: I have prior knowledge of Hangfire. It is stated here so the reader can discount it.
The F-grades are the defence — a fact only counts as **TOOL** if it is present in a raw response in
this directory, and F5 is deliberately chosen as the one where general .NET knowledge gives the
*wrong* answer.
