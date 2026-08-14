# R1.2 - human grading sheet (DESIGN section 7 validation)

Fill the two verdict columns and nothing else, then run:

    node eval/agent-probe/kappa.mjs

**Grade from the `item-NN.txt` files only.** Each one is byte-identical to what the
blind judge was given: the question, the verified answer key, the redacted candidate
answer, and the same rubric. Do not open `SEALED-key.json` and do not look at
`judged.jsonl` until every row below is filled - both of those name the arm and the
judge's verdict, and reading either first is the end of the blind.

Write `y` or `n`. Leave `?` and `kappa.mjs` will refuse to run.

| item | correct | fabricated |
|---|---|---|
| item-01 | ? | ? |
| item-02 | ? | ? |
| item-03 | ? | ? |
| item-04 | ? | ? |
| item-05 | ? | ? |
| item-06 | ? | ? |
| item-07 | ? | ? |
| item-08 | ? | ? |
| item-09 | ? | ? |
| item-10 | ? | ? |
| item-11 | ? | ? |

Definitions, copied from the rubric the judge was held to, so both raters are
answering the same question:

- **correct** - the answer substantively answers the question and agrees with the key.
  Judge the substance, not the wording, length or formatting. An answer that reaches
  the key's conclusion by a different route is correct. An answer that hedges into both
  a right and a wrong conclusion is not correct.
- **fabricated** - the answer asserts a file, symbol, method or code path that the key
  contradicts, or states as fact something the key shows to be false. Being incomplete
  is NOT fabrication.
- `[REDACTED]` is an uninformative placeholder inserted to keep the grading blind.
  Never let its presence or absence count for or against an answer.
