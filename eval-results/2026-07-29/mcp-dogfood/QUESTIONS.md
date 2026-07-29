# G4.1 — Dogfood Task 1: ten questions, written BEFORE the first MCP call

**Repo:** `eval-repos/Hangfire` (357 `.cs` files, 5 projects — `Hangfire.Core`, `Hangfire.SqlServer`,
`Hangfire.AspNetCore`, `Hangfire.NetCore`, `Hangfire.SqlServer.Msmq`). Sanctioned by
`docs/dev/research/R4-MCP-PROPER-TOOL.md` §2 as a non-octet repo.

**Rules of the drive** (R4 §2 Task 1):

- Answers come from MCP tools ONLY. No `Read`, no `Grep`, no file listing, no `git grep` during the drive.
- `read_source` IS an MCP tool and is therefore allowed — but every answer that depends on it is
  flagged, because a tool that only works by handing back source text is grep with extra steps, and
  that distinction is the point of this exercise.
- Every call is logged by the driver (`eval/mcp-qa/dogfood.js`), not by hand.
- Ground truth is established AFTER the drive, in a separate phase, with ordinary file reading. Those
  reads are not in the call log and do not count as fallback.

**Method confound, stated up front:** Hangfire is a well-known OSS project and I carry pretrained
knowledge of it. A call is graded HELPED only when the MCP **response contains** the fact — the raw
dump under `raw/` has to show it. Agreement with what I already suspected is not evidence.

## The questions

| # | Question | Flavour (R4 §2) |
|---|----------|-----------------|
| Q1 | When application code calls `BackgroundJob.Enqueue(...)`, what is the chain that ends in the job being written to storage, and which call actually performs the write? | where does X get persisted |
| Q2 | What takes a job back OFF storage and runs it? Name the server-side components and the order they run in. | what happens on X |
| Q3 | When a job changes state (Enqueued → Processing → Succeeded), who applies the transition, and what gets to observe or veto it? | who consumes event Y |
| Q4 | I want to add a new storage backend (e.g. Postgres). Which types must I implement, and what does the existing SqlServer implementation derive from? | where would I add feature W |
| Q5 | The Hangfire Dashboard is served over HTTP. How does an HTTP request reach a dashboard page — what is the entry surface, and how is it dispatched? | what happens on POST /Z |
| Q6 | Recurring jobs: where is the schedule persisted, and which component fires a recurring job when it comes due? | where does X get persisted / who fires Y |
| Q7 | I want a filter that runs before and after every job execution (e.g. logging). Which interface do I implement, and where is the filter pipeline invoked? | where would I add feature W |
| Q8 | Automatic retry on failure — which component implements it, and what does it hook into? | who consumes event Y |
| Q9 | The client serialises a method call and the server invokes it. What representation carries the method across, and where is it turned back into a real invocation? | the seam |
| Q10 | What is the connection/transaction abstraction over storage, and which in-repo types implement it? | where would I add feature W |

## What "answered" means

Per question, two verdicts, decided at two different times:

- **During the drive** — ANSWERED / PARTIAL / UNANSWERED: could I state the answer from MCP responses alone?
- **After the drive** — CORRECT / WRONG: does this checkout's source agree with what I stated?

R4 §3's bar: **≥8/10 answered correctly with no grep fallback.**
