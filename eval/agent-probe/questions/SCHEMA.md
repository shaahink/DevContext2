# Question-set schema

One file per repo: `eval/agent-probe/questions/<repo>.json`. These are **pre-registered ground
truth** — they are written by reading the repo at its pinned SHA, before any probe run, and they
do not change afterwards. If a key turns out to be wrong, fix it and re-run every arm; never fix
a key to match an answer you have already seen.

```jsonc
{
  "repo": "eShop",                 // directory name under eval-repos/
  "sha": "9b4f9434",               // the pinned commit the keys were written against
  "authoredBy": "K1.1",            // checkpoint that produced this file
  "questions": [
    {
      "id": "eshop-b1",
      "class": "B",                // A | B | C | D | E | F  (see below)
      "prompt": "How does POST /basket/checkout reach the point where an order is persisted? Name the handlers in order.",
      "answer": "Prose ground truth, written from the source. This is what a human grader reads.",
      "expectedVerdict": null,     // "yes" | "no" | "none" | null. REQUIRED for class D and E.
      "mustMention": ["CheckoutBasketCommandHandler", "..."],   // every entry must exist in the repo
      "mustNotMention": [],        // terms that would indicate a specific wrong answer; must also exist in the repo
      "evidence": ["src/Basket.API/..."]   // where you verified it, for the human re-check
    }
  ]
}
```

## Classes

| Class | n per repo | What it is | Why it is in the set |
|---|---|---|---|
| **A** | 1 | Orientation — "what is this system, what are its entry points" | High-frequency, cheap; where a one-call brief should win |
| **B** | 2 | Indirection — a flow that crosses projects through send/DI/events | The core claim; grep cannot follow it |
| **C** | 1 | Impact — "what breaks if I change X" | Set-valued; graded on recall and precision against the key, not 0/1 |
| **D** | 1 | Attribution trap — a fact that is true of a *sibling* member, not the one asked about | An agent reading the whole file can mis-attribute. `expectedVerdict` is usually `"no"` |
| **E** | ½ | Negative control — the true answer is "nothing matches" | Fabrication detector. **`mustMention` must be empty; `expectedVerdict` is `"none"`** |
| **F** | ½ | grep-favouring control — find a literal string, a config key | Sanity check on the *design*. If the MCP arm wins here, the harness is wrong |

Classes E and F are not padding. A set containing only B and C is built to produce a win.

## Rules the gate enforces (`node eval/agent-probe/verify.mjs`)

1. Valid JSON; `repo`, `sha`, `questions` present; unique ids.
2. The cloned repo's HEAD matches `sha` — otherwise the keys describe a different tree.
3. **Every `mustMention` entry resolves in the repo** (as a path, or as a literal string found by `git grep`). A key nobody verified is not ground truth.
4. **Every `mustNotMention` entry also resolves.** A trap term that does not exist in the repo is not a trap — the agent could never have said it, so the control tests nothing.
5. Class E carries an empty `mustMention`.
6. `expectedVerdict`, when present, is one of `yes` / `no` / `none`.

## How `mustNotMention` is scored (pre-registered, written at K1.3 before any run)

A `mustNotMention` entry is a **claim the answer asserts**, not a substring. The grader (A1.1)
counts a violation only when the answer *puts the term forward as part of its answer* — as an
affected dependent, a matching file, a consumer of the event. An answer that names the term in
order to **exclude** it ("`CreateOrderDraftCommandHandler` is not affected", "it is not in
`appsettings.json`") is exhibiting exactly the discrimination the trap tests for and scores as
**clean**, not as a violation.

This is written down here rather than left to the grader because a naive substring test would
penalise the most careful answers in every arm, adding noise to the primary endpoint. The rule
is fixed before any run and applies to all three repos. The gate's own check is unaffected —
it only asserts the term *exists in the repo*, which is what makes the trap real.

## Writing a good question

- Write the question a new engineer actually asks in week one, then see which tool happens to cover it. Do **not** write a question because a particular MCP tool answers it well — that is how a rigged set gets built.
- The `answer` field is prose a human can check in under a minute against `evidence`.
- Prefer questions whose answer is stable across the repo's history, so the key does not rot.
- For class D, the strongest form is a fact the engine is *known* to get right after a specific fix — `docs/dev/reports/phase1-member-origin-reprobe.md` documents one (a member-anchored trace shows only that method's wiring, so a sibling's integration event no longer appears on it).
