# Agent probe — tracker

The authority is `eval/agent-probe/DESIGN.md`. This file is the checkpoint surface conductor drives;
it is a **generated view** of the work graph in `.conductor/run.db`. Claim with
`conductor task --done <id> --evidence <path>` — hand-editing a row claims nothing.

**What this experiment is:** a pre-registered, three-arm comparison asking whether the DevContext
MCP server makes a fresh agent cheaper and/or more accurate at answering questions about an
unfamiliar .NET repo, versus plain `Read`/`Grep`/`Glob`. The standing verdict on record from
2026-06 is "primer, not accelerator" (`docs/product/PRODUCT-DIRECTION.md` §9). This run replaces
that opinion with a measurement.

**Out of scope, deliberately:**
- The full 360-run experiment. This is the 54-run **pilot**; its job is a directional read plus
  proof that the harness measures anything at all.
- The unseen-repo arm (DESIGN.md §6.1). The pilot runs on eShop, which the model may have seen in
  pretraining — so the pilot number is explicitly **not** contamination-controlled, and the report
  must say so.
- Grading the human-check sample. That is the owner's, by hand, and no kappa may be reported
  without it.

## Handoff  (overwrite this block, 12 lines max, no history)
last: nothing yet — run not started. Plan, gate, templates and question schema authored by the
  driver; no session has run.
stage: **K1 not started** (attempt 0).
gate: `node eval/agent-probe/verify.mjs --tier fast` proven green and proven red before launch;
  see the launch note in `.conductor/WATCH-HANDOFF.md`.
next: **K1.1** — author `eval/agent-probe/questions/eShop.json`: six questions covering classes
  A B C D E F, every answer key verified against the source at the pinned SHA `9b4f9434`.
trap: the gate proves every `mustMention` and `mustNotMention` string actually resolves in the
  cloned repo, so a plausible-but-wrong symbol name fails the stage rather than corrupting the
  experiment silently. Read the source; do not take a symbol from a doc.

## Checkpoints

<!-- THE ESCALATION TOKEN — the word HUMAN followed by a colon — parks the run at NeedsHuman and
     notifies the owner when it appears ANYWHERE in the handoff block above. The match is a plain
     substring, not a line anchor: inside backticks, mid-sentence, or in prose merely DESCRIBING
     the convention parks it just as hard as raising one. That is why this legend spells the token
     out rather than using it, and why it sits BELOW the handoff block. In handoff prose the word
     is "escalation". A row flipping to BLOCKED parks the same way. -->

Status is one of TODO, IN PROGRESS, DONE, BLOCKED. Evidence is an artifact path.

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| K1.1 | `questions/eShop.json` exists with 6 questions covering classes A B C D E F; every key symbol resolves in eval-repos/eShop at 9b4f9434; class D is a real sibling-attribution trap and class E has an empty mustMention | TODO | | |
| K1.2 | `questions/TodoApi.json` exists, same shape, keys verified at 307a1ead | TODO | | |
| K1.3 | `questions/FluentValidation.json` exists, same shape, keys verified at 94397908 | TODO | | |
| H1.1 | `run-probe.mjs` drives all three arms as headless subprocesses, is resumable from `runs.jsonl`, caps each run at $1.50, and refuses more than 60 runs per invocation | TODO | | |
| H1.2 | One real end-to-end run per arm is recorded in `results/runs.jsonl` with answer, toolCalls, costUsd, usage, numTurns, durationMs — and the raw result JSON plus transcript are saved under `results/raw` | TODO | | |
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | TODO | | |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | TODO | | |
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | TODO | | |
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | TODO | | |
| A1.2 | Judge pass complete on anonymised final answers only, plus the paired analysis: median log2 cost ratio with bootstrap CI, accuracy difference with CI, fabrication rate, mcp call share | TODO | | |
| R1.1 | `eval-results/agent-probe/RESULTS.md` states the verdict against the four pre-registered outcomes, with the per-class breakdown and the honest pilot interval | TODO | | |
| R1.2 | Human-check sample (20%, stratified) written to a separate file for the owner; report names exactly what the full run needs to turn this pilot into a defensible number | TODO | | |
