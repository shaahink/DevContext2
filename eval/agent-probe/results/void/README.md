# Voided runs — kept, not deleted

Nothing in this directory may be used in an analysis. It is here because a run that was paid for
and then thrown away has to be visible; a quietly deleted run is indistinguishable from a run that
was deleted because its number was inconvenient.

## `runs-void-h1.2-preisolation.jsonl` — 3 runs, eShop `eshop-a1`, arms G/M/B, rep 1, $2.82

Recorded 2026-08-11 by the first version of `run-probe.mjs`, which built each arm from
`DESIGN.md` §8's argv on the assumption that `--allowedTools` restricts the tool set. **It does
not.** `--allowedTools` is an auto-approve list; a tool that appears in neither `--allowedTools`
nor `--disallowedTools` is still offered to the model and still runs.

What that cost, read out of the transcripts now in `raw/`:

| Arm | What leaked |
|---|---|
| G | Executed 3 `Bash` calls (3 more were refused only for containing shell expansions — not for being `Bash`) |
| M | Executed the **subagent** tool once — the subagent reported back that "all reads were `cat`/`ls` only", i.e. the MCP-only arm read the filesystem — plus 5 `Monitor` calls, and `Monitor` runs bash |
| B | Executed the deferred-tool loader, so its tool set was not fixed either |

Arm M having filesystem access is not a blemish on those three runs, it is the negation of what
arm M exists to test (`DESIGN.md` §3.1). The runs are void as measurements.

They are still evidence of two things, which is why they are kept: that `--allowedTools` does not
isolate, and that `total_cost_usd` populates on this account (§4.1's trap — the three runs cost
$0.700, $0.686 and $1.438).

**Remedy applied,** per the standing rule that a harness fault is re-run in *all three* arms and
never only in the one that looked wrong: each arm is now defined by an exhaustive
`--disallowedTools` list covering the whole tool universe minus that arm's tools, every run records
`offeredOutsideArm` / `calledOutsideArm` / `isolationOk`, the runner aborts the batch on the first
breach, and `verify.mjs` fails on any row carrying a breach. All three cells were re-run from
scratch under the fixed harness.
