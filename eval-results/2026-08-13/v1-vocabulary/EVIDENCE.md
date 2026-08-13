# V1.1 — ONE verified-edge definition (backlog #25)

Session 5, stage V1, 2026-08-13. Strand: `docs/dev/research/GRAPH-DETECTION-AUDIT-2026-08-13.md`
§3.3. Acceptance was declared in the conductor ledger BEFORE the first edit.

## The defect, re-verified before editing (house rule: the audit's file:line claims are dated)

The engine shipped **two** answers to "is this edge verified?", and both reached a user:

| Surface | Its reading | File:line as found |
|---|---|---|
| MCP `stats` tool | `verified = count - approx` → **Join counts as verified** | `src/DevContext.Mcp/DevContextTools.cs:605` |
| CLI `query stats` | `verified = s.Count - s.Approx` | `src/DevContext.Cli/Commands/QueryCommand.cs:358` |
| Report "Verified edges %" | `(totalEdges - approx) / totalEdges` | `src/DevContext.Core/Rendering/ReportRenderer.cs:160` |
| `GraphOrphansSource` | `Resolution.Semantic` only | `src/DevContext.Core/Insights/GraphOrphansSource.cs:63` |
| `FlowIndexBuilder` | `Resolution.Semantic` only | `src/DevContext.Core/Graph/FlowIndexBuilder.cs:116` |
| `ConfidenceLedger` | verified = Semantic; approx = `Syntactic \|\| Confidence < 1.0f` — a **fourth** spelling, and one an edge can satisfy while also being verified | `src/DevContext.Core/Insights/ConfidenceLedger.cs:38-46` |
| Desktop explorer | `edge.resolution === 'Semantic' ? 'verified' : 'approx'` — the same edge the CLI calls verified is drawn **approx** | `src/DevContext.App/src/app/features/explorer/stage.ts:246` |
| Pack provenance footer | `if (Syntactic) Approx++; else Verified++` | `ContextPackBuilder.cs:1061` |

`Resolution.Join` is the enum's **default value** (`CodeGraph.cs:112-120`, `GraphEdge.Resolution`
initialiser at `:175`), so "Join counts as verified" means *every edge no producer labelled at all*
was reported as Roslyn-verified.

The app also disagreed with **itself**: `trace-node.ts` tested `=== 'Syntactic'` for its approx badge
(right) while `stage.ts`, `graph-canvas.ts` (×2) and `filterApproxTree` tested `!== 'Semantic'`
(wrong), so a Join edge was labelled "approx" on one page and left unmarked on another.

## The one definition

`src/DevContext.Core/Graph/EdgeConfidence.cs` — `EdgeTier { Verified, Joined, Approximate }`, an
exhaustive, mutually-exclusive partition of `Resolution`:

* **Verified** = `Semantic` — a Roslyn `SemanticModel` resolved the symbol.
* **Joined** = `Join` — two detections joined; also the unlabelled default. Never "verified".
* **Approximate** = `Syntactic` — a syntax/string heuristic.

`Confidence` is a **separate axis and is deliberately not folded in**: it is a producer-set 0..1
scalar (a semantically-resolved seam target ships at 0.95), so mixing it into the tier — which is
what `ConfidenceLedger` did — puts one edge in two buckets at once.

The app mirrors it in exactly one function: `edgeTier()` in `src/DevContext.App/src/app/core/format.ts`.

Both halves are now enforced mechanically: **loom-guards rule 9** fails the gate on any
`== Resolution.X` verdict outside `EdgeConfidence.cs` and any `resolution === 'Semantic'`-style
verdict outside `format.ts` (assignments stay legal; a comment quoting the dead shape is ignored).

## Measurement — before vs after, same source tree, same commands

Method (`run-compare.ps1` + `capture.ps1`, both in this folder, re-runnable): a detached worktree at
`HEAD` (`C:/Code/DevContext2-v11-before`) builds the **pre-change** CLI; both binaries then run the
same four reads against **that one pristine tree**, so every difference is attributable to the change.

### The graph did not move — zero blast radius

    before-graphdump.json  vs  after-graphdump.json   →  BYTE-IDENTICAL (410,142 bytes)

1267 nodes / 1396 edges, every `resolution` field unchanged. No edge was reclassified: this
checkpoint changed **what the word means when a surface prints it**, not what the engine detects.

### The numbers that moved (`query stats`, DevContext's own repo)

| Seam | Total | verified BEFORE | verified AFTER | joined | approx |
|---|---|---|---|---|---|
| Calls | 1379 | 268 | **238** | 30 | 1111 |
| Resolves | 16 | 14 | **0** | 14 | 2 |
| ServiceLink | 1 | 1 | **0** | 1 | 0 |

**45 of the 283 edges the engine called Roslyn-verified (16%) were not** — and two of the three
seams were 100% mislabelled. `Resolves` and `ServiceLink` are *entirely* detection joins, which is
correct and unsurprising for DI resolution; what was wrong was calling them verified.

This is the currency E1 will report its acceptance in: engine-own Calls = 1111 approx / 1379
(the E1 baseline note of 1103/1383 is the same measurement at an earlier commit).

Raw artifacts: `before-stats.json`, `after-stats.json`, `before-report.md`, `after-report.md`,
`before-analyze.json`, `after-analyze.json`, `before-graphdump.json`, `after-graphdump.json`.

## Gates run

| Gate | Result |
|---|---|
| `dotnet build DevContext.slnx` | 0 warnings / 0 errors |
| `dotnet test DevContext.slnx --filter "Category!=Eval"` | 737+108 passed; 1 known-flake (`McpQaGateTests`, backlog **#1** — false red on the first run after a Core change; passes on re-run, verified) |
| `scripts/loom-guards.ps1` | PASS, incl. new rule 9 + truth gate 0 failures |
| `eval/contract-sweep.ps1` | PASS — 506 fields, 0 NEW unread (the two new `SeamStat` fields are read by the MCP) |
| `src/DevContext.App` `pnpm check` | exit 0 — lint clean, 159/159 tests, production build |

## Test that pins it

`tests/DevContext.Core.Tests/NumberReconciliationTests.One_definition_of_a_verified_edge_across_every_counting_surface`
builds a 3-edge graph — one Semantic, one added with **no `Resolution` at all**, one Syntactic — and
pins: the partition; that `GraphStats` and `ConfidenceLedger` return the same three counts; that
`Count - Approx` reads **2** verified on that graph while the truth is **1** (the retired inference,
stated so it cannot come back); and that a 0.95-confidence Semantic edge is still verified.

`ContextPackBuilderTests.Sections_carry_provenance_footer_and_structured_tiers` went **red on the
fix** and is the second measurement: every step that fixture walks is Join-resolved, so the pack
section it inspects was being reported to the agent as fully verified with nothing semantically
resolved in it. It now pins `Verified == 0 && Joined > 0`.

## Not done here (deliberate, follow-ups)

* `SectionProvenance.Joined` is on the pack's **text** footer and the C# `SectionAllocation`, but
  not on the proto `SectionProvenanceInfo` — the desktop composition view still sees two of the
  three tiers. Filed rather than fixed: the proto pair needs an app reader to satisfy contract-sweep.
* `ConfidenceLedger.ApproxEdgePct` no longer counts `Confidence < 1.0` edges. The low-confidence
  population is still on every edge and is now reportable on its own axis; no surface reports it yet.
