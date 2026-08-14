# Agent Probe — does the MCP make a code-browsing agent more effective?

> Pre-registration. Written before any run. The point of writing it first is that the
> decision rule (§5) cannot be chosen after seeing the numbers.
>
> **This file is never edited to match what happened.** Where execution departed from it, the
> departure is recorded in [`DEVIATIONS.md`](DEVIATIONS.md) — read the two together. Six
> deviations are on record as of the P2 pilot; none touches §5, §3.1, the questions or the keys.
>
> Lineage: this is the fresh-agent A/B that `IDEAL-OUTPUT-TARGET.md` §7 specified and
> `docs/dev/archive/reports/probe-kit.md` scoped, and that
> `docs/dev/reports/phase1-member-origin-reprobe.md` §4 explicitly deferred as
> "a human-in-the-loop step". The standing verdict on record is **"primer, not
> accelerator."** This design exists to replace that verdict with a measurement.

---

## 0. Amendment log

A pre-registration may be **amended**, in writing, before the run it governs — that is the
opposite of editing it to match what happened. Each row below was written on the date shown,
before any run of the study it governs, and each is reproduced inline in its own section with
the same marker so a reader who lands mid-file cannot miss it. The pilot (P1/P2, 54 runs,
`eval-results/agent-probe/RESULTS.md`) was run under the **unamended** text; nothing here is
retrofitted onto its numbers.

| # | Date | Section | What changed | Why, in one line |
|---|---|---|---|---|
| **A1** | 2026-08-14 | §3.2, §6.1 | The unseen repo is named, built by a committed mechanical renamer, and pinned | §6.1 called the mitigation "not optional" and the pilot shipped without it |
| **A2** | 2026-08-14 | §3.3 | The question mix is pinned per repo, including which repo carries E and which carries F | The pilot's set was one-per-class (DEVIATIONS D7); "half weight alternating" was never resolved to an assignment, so it could be chosen after seeing numbers |
| **A3** | 2026-08-14 | §4.2, §5, §9 | Non-inferiority becomes a **single pooled** endpoint, bootstrapped over **questions** (reps aggregated first), margin **−0.10** | The −0.05 per-repo bar is unreachable at 30 pairs, and the run-level pairing was pseudo-replicated — anti-conservative in exactly the direction a non-inferiority test must not lean |
| **A4** | 2026-08-14 | §7 | κ on 20% of the full set, with an explicit rule for a degenerate marginal and a minimum-n gate | At pilot n the κ gate could only return "perfect" or "unevaluable"; a degenerate marginal has no κ at all |
| **A5** | 2026-08-14 | §3.1, §9, §10 | A fourth arm, **BI** (B-instructed), is added | Unprompted adoption is the purist's bar; instructed use is the product's actual deployment shape |

Source: `eval-results/agent-probe/RESULTS.md` §10 (the pilot's own list of what the full run
needs) and `docs/dev/research/DEEP-EVAL-2026-08-13.md` §4 W3.8. Two of RESULTS §10's own
figures did not survive re-derivation; see the note inside §4.2.

---

## 1. What already exists, and the gap this fills

| Existing | What it measures | What it can't tell you |
|---|---|---|
| `eval/expectations/*.json` (~30 repos) | Does the engine produce the right map/trace/entries? | Nothing about agents |
| `eval/graph-truth.ps1` + matrix | Is the graph correct per defect class? | Nothing about agents |
| `eval/mcp-qa/run.js` | Does a **scripted** call sequence answer 11 QA questions, and does the checkout question fit in ≤3 calls / ≤2k tokens? | The calls are hand-written. It tests the *tool contract*, not whether an agent left to its own devices is better off |
| `eval/mcp-qa/dogfood.js` | Per-call `ms`, `chars`, `tokens` into `call-log.jsonl` + raw responses | It's a transport for a human driving the tools, not an experiment |

Every one of those measures **DevContext**. None measures **an agent using DevContext**.
The product claim — "let AI agents query your codebase instead of grepping it" (README) —
has never been tested against the thing it claims to beat.

That is the gap: **there is no grep baseline anywhere in the repo.** The one place the
comparison is named at all is the informal "ripgrep test" in `PRODUCT-DIRECTION.md` §9,
which is a bar for *usefulness*, not a controlled comparison.

---

## 2. The question, stated so it can come out "no"

> For a fresh agent answering questions about an unfamiliar .NET repository, does
> having the DevContext MCP server available reduce the cost of reaching a correct
> answer, without reducing how often it is correct?

Three ways this can fail, all of which the design must be able to detect:

- **Null** — the MCP arm costs the same and is right as often. The tools are neutral; the token cost of 22 tool schemas is a pure tax.
- **Primer** — the MCP arm is *more correct* but costs the same or more. That is the verdict already on record. Valuable, but it is not the claim on the README.
- **Harmful** — the MCP arm is cheaper but *less correct*, because a confident wrong graph answer stops the agent looking. This is the outcome that matters most and the one a naive "did it get cheaper?" experiment would miss.

---

## 3. Design

### 3.1 Arms

Three arms, same model, same prompt, same repo, fresh session each time.

| Arm | Tools available | Tests |
|---|---|---|
| **G** (grep baseline / control) | `Read`, `Grep`, `Glob`, `Bash(git *)` — **no MCP** | What an agent does today |
| **M** (MCP only) | `mcp__devcontext__*` — **no file tools** | Is the MCP *sufficient*? Isolates the graph from the file system |
| **B** (both / treatment) | Everything in G plus everything in M | The real-world configuration |
| **BI** (B, instructed) | Byte-identical to B — same tools, same deny list | The **deployed** configuration: B plus the one line every real install ships |

> **Amended 2026-08-14 (A1.1) — arm BI added.** The pilot measured *unprompted* adoption and
> found it near zero (`mcp_call_share` 0.015). That is a real finding about whether an agent
> reaches for the tools on its own, and it is **not** the question a shipped product answers,
> because a shipped product ships a `CLAUDE.md` line telling the agent the tools exist. Running
> only B conflates "agents won't reach for it" with "agents won't benefit from it"; running B
> and BI separates them, and the separation is the single most decision-relevant number the
> full run can produce.
>
> **Mechanics, pinned so BI differs from B in exactly one byte-range.** BI's argv is
> `armArgs("B")` with one substitution: the `--system-prompt` value becomes
> `system.txt + "\n\n" + system-instructed.txt`. Tool sets, deny lists, `--add-dir`,
> `--mcp-config`, model, budget cap and isolation are identical to B — the harness builds BI
> by delegating to B's own branch, so the two cannot drift apart. §6.3's `--bare` skips
> `CLAUDE.md` auto-discovery, so the instruction **cannot** ride in a repo file and be read;
> it must ride in the system prompt, and that is the honest analogue of the deployed line.
> The instruction text is committed at `eval/agent-probe/system-instructed.txt` and is pinned
> as of this amendment — changing it after a run is a new deviation, not an edit.
>
> **What BI costs and what it buys.** It is a fourth arm at every cell: §9's run count goes
> 360 → 480 and §10's estimate moves with it. BI is **not** in the primary contrast. The
> primary endpoint stays B-vs-G (§4.1/§4.2) so the pre-registered decision rule is unchanged;
> BI-vs-G is reported as a **secondary, pre-registered** contrast under the same analysis, and
> BI-vs-B is the adoption delta attributable to the instruction alone.

Arm **M** is not decoration. Without it, arm B's result is unattributable: if B ≈ G, you
cannot distinguish "the tools don't help" from "the agent never called them". M forces
the tools to carry the whole task, and the M-vs-B gap tells you how much of B's behaviour
is actually MCP-driven. Record `mcp_call_share` (MCP calls ÷ all tool calls) in arm B as a
manipulation check: **if `mcp_call_share` < 0.2 across arm B, the B-vs-G comparison is not
a test of the MCP and must be reported as such, not as a null result.**

> **Amended 2026-08-14 (A1.1) — the two branches out of the manipulation check, fixed in
> advance.** The pilot failed this check (0.015). Before the full study is funded, the check
> is re-run on its own: **arm B alone, one repo (eShop), 18 runs, prompt and system text
> unchanged**, against the tool surface as revised by the trust pack (curated catalog,
> described tools). Exactly one of these fires, and which one is decided by the number, not by
> a later reading of it:
>
> - **≥ 0.2 — proceed.** The full study runs as specified. B stays the primary treatment arm.
> - **< 0.2 — the honest fallback.** This is a **product finding**, not a failed measurement:
>   with a curated, described surface an agent still does not reach for the tools unprompted.
>   Its consequences are all three of: (a) the primary contrast becomes **M-vs-G** and the
>   study reports *sufficiency* ("can the graph alone answer these?") rather than
>   *augmentation*; (b) **BI** is promoted from secondary to the arm that carries the product
>   claim, because instructed use is then the only configuration in which the tools are used
>   at all; (c) arm B is retained and reported as the measurement of unprompted adoption,
>   which is the finding.
>
> The floor stays 0.2 in both branches. Nothing below it is re-described as a pass.

### 3.2 Repositories

Four, chosen for *shape* and for one specific confound (§6.1).

| Repo | Shape | Why |
|---|---|---|
| `eval-repos/eShop` @ `9b4f9434` | CQRS / MediatR / integration events, 7 services | The hard case: cross-project indirection grep genuinely can't follow |
| `eval-repos/TodoApi` @ `307a1ead` | Minimal API, small | Floor check — on a repo this small, grep should win. If the MCP wins here too, suspect the design |
| `eval-repos/FluentValidation` @ `94397908` | Library, no entry points | Library surface, a different question shape entirely |
| **One private/unseen repo** | Anything | **Load-bearing.** See §6.1 |

> **Amended 2026-08-14 (A1.1) — the unseen repo is named, built and pinned.** The pilot ran
> with three famous repos and none of this, which §6.1 calls "not optional"; the row above was
> a promise, and a promise is not a repo. It is now a tree:
>
> | | |
> |---|---|
> | Source | `https://github.com/ardalis/CleanArchitecture.git` @ `74624fb0e45454c471b5ca00b13acbab9263cbf3` (present here as the `eval-repos/VerticalSlice` submodule) |
> | Scope | the **main solution only** — `src/`, `tests/`, `.aspire/` and the root build files. The upstream tree bundles three independent solutions and that breaks the rename; see below |
> | Builder | `eval/agent-probe/rename-repo.mjs`, seed `20260814`, `--ns Clean.Architecture` |
> | Output | `eval/agent-probe/unseen/Driewie/` — committed, **132 files, 10 projects** (6 `src`, 4 `tests`) |
> | Pin | `treeSha256` in `eval/agent-probe/unseen-repo.manifest.json`; the sealed map is `unseen-repo.rename-map.json` |
> | Probe id | `Driewie` — `questions/Driewie.json`, authored by hand against the **renamed** tree |
>
> **Why a public repo renamed rather than a private one.** A private repo cannot be committed,
> cannot be re-derived by a reader, and cannot be checked by anyone who did not have it. A
> deterministic rename of a pinned public tree is reproducible from two committed files, and
> reproducibility is what makes the number defensible. Same seed, same source, byte-identical
> output — the manifest carries the tree hash so that is checkable rather than claimed.
>
> **Why one solution and not the whole tree — a defect the build caught and reading would not
> have.** Upstream ships three solutions side by side. `LoggingBehavior` and `ServiceConfig` are
> *declared* in two of them and arrive from the `Ardalis.SharedKernel` **package** in the third.
> A repo-wide sweep of "what this repo declares" therefore renamed those names at their use
> sites in the main solution, where they belong to a package — `CS0246`, twice, on a tree that
> had already reported "Build succeeded" once under a different bug. The extract is scoped to
> one solution, which removes the collision class rather than patching around it.
>
> **What the rename does and does not cover**, so nobody has to infer it from the code:
> namespaces (as dotted prefixes), declared type names, and the file and directory names that
> carry them — **85 of 92 declared types**, 2 domain namespace segments, **965 identifier
> substitutions across 121 renamed paths**. **Members are not renamed**: a text pass cannot rename a method
> that implements a package's interface without breaking the build, and a tree that does not
> compile is a worse repo than a contaminated one. Architectural vocabulary is deliberately
> **kept** — `Core`, `UseCases`, `Repository`, `CommandHandler` — because it is what *both*
> arms reason with; stripping it would handicap grep and the graph together, which is a
> different experiment, not a cleaner one. What moves is the domain noun, which is what carries
> a memorised answer.
>
> **Residual, measured and stated rather than assumed away.** The builder scans its own output
> and records what survives: `Clean` × 1 — the English word, in `// Clean up environment
> variable`. Not scanned, because it cannot be removed: the package list still reads
> `Ardalis.GuardClauses`, `Ardalis.Result`, `Ardalis.Specification`, `Ardalis.SharedKernel`,
> `Ardalis.SmartEnum`, `NimblePros.Metronome` and three more. Renaming a package id fails
> restore, so the tree's **provenance stays guessable** even though none of its symbols do.
> That is the honest limit of a text rename, and it is not what §6.1 is defending against:
> contamination hurts by letting the model answer **without searching**, and no memorised
> answer survives when every symbol it names is gone.
>
> **Verified by building, against the unmodified tree as a control.** Both are built with
> `-p:NuGetAudit=false` — upstream turns NuGet advisories into errors and 17 fire on both trees
> from package CVEs that predate this work. The control (`Clean.Architecture.slnx`, unmodified)
> is **0 errors / 4 warnings**. The renamed tree must match it; the evidence file records the
> comparison. "It builds" is the claim, and it is the claim that caught both defects above.
>
> **Two run-time conditions, pre-registered here because they are easy to get wrong.**
> (a) The tree is **staged outside this repository** before any run — an agent whose transcript
> shows `.../eval/agent-probe/unseen/...` has been told it is in a probe fixture, and could read
> DevContext's own source besides. (b) `questions/Driewie.json` is authored against the renamed
> tree, by hand, **before any run**, and its keys name renamed symbols only.

### 3.3 Task set

Per repo, 6 questions across the classes below (24 items total). Each is a **question with a
written answer key**, produced by reading the repo by hand *before any run*, and stored as
`eval/agent-probe/questions/<repo>.json`.

| Class | n/repo | Example (eShop) | Why it's in the set |
|---|---|---|---|
| **A. Orientation** | 1 | "What kind of system is this and what are its entry points?" | Cheap, high-frequency, where `overview` should shine |
| **B. Indirection** | 2 | "`POST /basket/checkout` — trace it to where the order is persisted." | The core claim. Send → handler → domain event → EF is exactly what grep can't follow |
| **C. Impact** | 1 | "What breaks if I change `CheckoutBasketCommandHandler`?" | Set-valued answer; scoring is recall/precision against the key, not 0/1 |
| **D. Attribution trap** | 1 | "Does `CatalogApi.CreateItem` publish `ProductPriceChangedIntegrationEvent`?" | **True answer: no** — `UpdateItem` does. An agent reading the file sees the event in the class and can mis-attribute. This is the exact defect Phase 1 fixed (`phase1-member-origin-reprobe.md`), so the graph is *known* to be right here |
| **E. Negative control** | ½ | "Which handlers consume `<event that nothing consumes>`?" | True answer is "none". Detects fabrication in **both** arms |
| **F. grep-favouring control** | ½ | "Which files contain the literal string `X`?" | If the MCP arm wins here, something is wrong with the harness — this is a sanity check on the design, not on the product |

Classes E and F are what keep this honest. A design with only classes B and C is built to
produce a win.

> **Amended 2026-08-14 (A1.1) — the mix is resolved to an assignment, per repo.** The `n/repo`
> column above says "E ½, F ½" and never said *which* repo carries which. A half-weight class
> is not runnable: you cannot ask half a question. The pilot resolved it by carrying **both**
> (DEVIATIONS **D7**) — which is how the set became one-per-class, halving class B, the only
> class the MCP is designed to win. Leaving the assignment open would let a later session pick
> it after seeing numbers, which is the one thing this file exists to prevent. It is therefore
> pinned here, before the run:
>
> | Repo | A | B | C | D | E | F | total |
> |---|---|---|---|---|---|---|---|
> | **unseen** (renamed, §3.2 — reported first) | 1 | **2** | 1 | 1 | 1 | — | 6 |
> | `eShop` | 1 | **2** | 1 | 1 | — | 1 | 6 |
> | `TodoApi` | 1 | **2** | 1 | 1 | 1 | — | 6 |
> | `FluentValidation` | 1 | **2** | 1 | 1 | — | 1 | 6 |
> | **study** | 4 | **8** | 4 | 4 | 2 | 2 | **24** |
>
> E and F each land twice in four repos — "half weight", made concrete. The alternation starts
> on the unseen repo with **E** (fabrication control), because that is the repo where a model
> that cannot recall the answer is most likely to invent one, and it is the repo §6.1 requires
> be reported first.
>
> **Consequences that must be executed before the run, not after.** Each of the three existing
> question files gains a **second class-B question**, and drops the one of E/F this table does
> not assign it. Authored the way §3.3's selection discipline already requires: written from
> the repo by hand, with the answer key, **before any run**, and committed. The dropped items
> are not deleted — they stay in the file marked `"inSet": false`, because they are the pilot's
> record and `grade.mjs --check` must still resolve them.
>
> **Direction of effect, stated because it favours the treatment.** Relative to the pilot this
> restores class B from 1 to 2 and halves the controls. DEVIATIONS D7 records that the pilot's
> composition ran *against* the treatment; correcting it therefore moves *toward* it. That is
> not a thumb on the scale — it is §3.3 as written before any number existed — but the full
> run's write-up must state it in the same breath as the headline, and must not compare its
> composition-corrected result to the pilot's as if the two sets were the same sample.

**Selection discipline:** the questions are written from the repo, not from the tool
catalogue. Do not write a question because `seam` answers it well. Write the questions a
new engineer actually asks in week one, then see which tool covers them.

---

## 4. Metrics

Everything below comes out of a single `claude -p --output-format json` result object.

### 4.1 Primary endpoint

**Cost to a correct answer**, paired by question:

```
cost_ratio(q) = median(cost_B(q, reps)) / median(cost_G(q, reps))
```

reported as the median of `log2(cost_ratio)` across questions, with a bootstrap 95% CI.
Log-ratio because token distributions are heavy-tailed and a mean over raw dollars is
dominated by one rabbit-holing run.

`cost` is `total_cost_usd` from the JSON result. **Trap:** on a subscription/OAuth account
`total_cost_usd` comes back `0`. Verify it is non-zero on the pilot's first run; if it is
zero, compute cost from `usage` + `modelUsage` at published rates (Opus 5 `$5 / $25` per
MTok; Sonnet 5 `$3 / $15`, intro `$2 / $10` through 2026-08-31; Haiku 4.5 `$1 / $5`) and
price `cache_creation_input_tokens` at 1.25× input and `cache_read_input_tokens` at 0.1×.

### 4.2 Co-primary: correctness

Per-item score against the answer key (§7). Arm B must be **non-inferior** to arm G:
the lower bound of the 90% CI on `(accuracy_B − accuracy_G)` must exceed `−0.05`.

A cost win with a correctness loss is not a win. This is the single most important line
in the document.

> **Amended 2026-08-14 (A1.1) — the endpoint's unit, method and margin are re-specified.**
> Every number below is re-derived, not quoted: `node eval/agent-probe/analyse.mjs --ni-power`
> prints both surfaces and is the artifact this amendment stands on.
>
> **Two defects in the text above, both found by computing it rather than reading it.**
>
> *Defect 1 — the pairs were pseudo-replicated.* `paired2x2` pairs on `(question, rep)`: 18
> pairs from 6 questions in the pilot, 120 from 24 questions in the full run. Reps of the same
> question are five draws on one question, not five independent questions — §9 already knows
> this and resamples **questions** for the cost endpoint. Inflating n by 5× narrows the
> interval, and a narrower interval makes non-inferiority **easier** to declare. That is
> anti-conservative in the one direction a non-inferiority test must never lean.
>
> *Defect 2 — the reachability table in RESULTS §10.4 is a special case, mislabelled.* Its
> column reads "90% lower at a perfect tie … the best 90% lower bound each n can produce". At a
> perfect tie (`b = c = 0`) Newcombe method 10 collapses to the *asymmetry* of the Wilson
> interval, so the bound depends entirely on the accuracy level, and RESULTS computed it at
> accuracy = 1.0 — the **worst** perfect tie, not the best:
>
> | pairs | acc 1.00 | acc 0.90 | acc 0.80 | acc 0.70 | acc 0.50 |
> |---|---|---|---|---|---|
> | 18 | −0.131 | −0.102 | −0.073 | −0.058 | **0.000** |
> | 30 | −0.083 | −0.066 | −0.050 | −0.033 | **0.000** |
> | 120 | −0.022 | −0.018 | −0.013 | −0.009 | **0.000** |
>
> At accuracy 0.5 a perfectly concordant pair set returns a lower bound of exactly **0.000 at
> any n** — the interval has zero width. So "no data could clear it at n = 30" is false as
> stated, and the alternative reading is worse: the method hands back a degenerate interval
> whose width is driven by where the accuracy sits, not by the evidence. Neither reading is a
> basis for a release gate. **The instrument is replaced, not re-tuned.**
>
> **The endpoint, as it now stands.**
>
> 1. **Unit of inference is the question**, matching §4.1. Reps are aggregated first: for each
>    (question, arm), `acc = correct / reps` ∈ {0, 0.2, …, 1.0}. Δ(q) = `acc_B(q) − acc_G(q)`.
> 2. **Method**: the 90% lower bound is the 5th percentile of 10,000 bootstrap resamples of
>    `mean(Δ)` over the 24 questions, seed `20260814` — the same resampling unit, count and
>    seed discipline §9 already fixes for cost. No Wilson, no Newcombe, no 2×2.
> 3. **One pooled endpoint, not four.** Non-inferiority is evaluated **once**, over all 24
>    questions. §6.1's per-repo reporting survives in full as *description* — accuracy and cost
>    per repo, unseen repo first — but a per-repo non-inferiority **test** is withdrawn, because
>    six questions cannot support one (below).
> 4. **Margin −0.10**, superseding −0.05.
>
> **Why −0.10 and not −0.05, and why not −0.15.** Measured, at 24 questions and 5 reps:
>
> | scenario | 90% lower, 24 questions | 90% lower, 6 questions (one repo) |
> |---|---|---|
> | perfect tie, every Δ = 0 | 0.000 | 0.000 |
> | B loses 2 of 5 reps on **1** question | −0.050 | −0.200 |
> | B loses 2 of 5 reps on **2** questions | −0.067 | −0.267 |
> | 1 question −2/5, 1 question +2/5 (net zero) | −0.033 | −0.133 |
> | 2 questions −2/5, 2 questions +2/5 (net zero) | −0.050 | −0.200 |
> | B wrong on **all 5 reps** of 1 question | −0.125 | −0.500 |
>
> A −0.05 margin is failed by *one* question on which arm B loses two reps out of five, and by
> a net-zero exchange of two wins for two losses. That is rep-level noise, not a correctness
> regression, and a bar that cannot survive it reports "Regression" on a tie. A −0.10 margin
> absorbs that and still **fails** the case the bar exists to catch: one whole question where
> arm B is wrong every time (−0.125). −0.15 would swallow that case too, and is rejected for
> it. −0.10 is the tightest margin that is both reachable and falsifiable at this n; it was
> chosen from the design's structure, before the study's data exists.
>
> **The per-repo test is withdrawn on arithmetic, not on convenience.** At six questions, one
> question losing two reps gives −0.200. No margin that would pass that is a bar anyone should
> put in a README. Four repos of six questions cannot each carry a non-inferiority test; the
> study has one, pooled.
>
> **Net effect on strictness, stated plainly.** The margin is loosened 2× (−0.05 → −0.10) and
> the effective sample is tightened 5× (120 pseudo-pairs → 24 questions). Those move in
> opposite directions and the amendment claims no free lunch: it claims the instrument now
> measures the thing at the unit the design already declared, at a margin the unit can reach.
> The **cost** endpoint (§4.1) and the **decision rule's** cost threshold are untouched.

### 4.3 Secondary

| Metric | Source | Why |
|---|---|---|
| `num_turns`, tool-call count | result JSON / transcript | Latency proxy, and the closest thing to the existing "≤3 calls" gate |
| `duration_ms` | result JSON | Wall clock, the thing a user actually feels |
| `fabrication_rate` | grader flag | Asserted a file/symbol/edge that does not exist. Counted separately from wrong-but-honest |
| `mcp_call_share` | transcript, arm B | Manipulation check (§3.1) |
| `citation_accuracy` | grader | Do the cited `file:line` refs resolve? Cheap, mechanical, and it catches plausible-sounding wrong answers |
| **Tool-schema tax** | §4.4 | The MCP's fixed cost, which the treatment must earn back |

### 4.4 The tool-schema tax — measure it, don't hide it

22 tool descriptions enter the system prompt in arms M and B. The descriptions in
`docs/product/mcp-reference.md` are long (the `neighbors` and `trace` entries run to
paragraphs). That is a real, unavoidable cost of the treatment and it must appear in the
headline number.

Measure it directly: run the same trivial prompt (`"reply with the word ok"`) in arm G and
arm B and record turn-1 `input_tokens + cache_creation_input_tokens`. The delta is the tax.
Report it as an absolute token count and as a percentage of median run cost.

It is paid once per session as a cache write (1.25×) and then read at ~0.1× per turn, so
it hurts short runs most — which is exactly where class A and F questions live. Do not
average it away.

### 4.5 Not measured, and why

- **Cold `analyze` time.** A first analysis of a large repo takes minutes. Amortising it into per-question cost would be dishonest in either direction. Instead: **warm every arm before the clock starts** (analyse once, confirm `cached: true`), report cold analysis cost separately as a one-off setup number, and state plainly that the headline figure is the warm case.

---

## 5. Decision rule (fixed before running)

Mapped onto the repo's own vocabulary from `ACCEPTANCE.md`:

| Result | Verdict |
|---|---|
| Correctness non-inferior **and** median `log2(cost_ratio)` CI upper bound < `−0.32` (≥20% cheaper) | **Accelerator.** The README claim is earned. Ship the number |
| Correctness improves (CI lower bound on Δaccuracy > `+0.05`) but cost not reduced | **Primer.** Unchanged from the 2026-06 verdict. Reposition the product on accuracy, not token savings |
| Neither CI excludes zero | **Null.** The tools are a tax. Either the tool descriptions need work or the surface is wrong |
| Correctness non-inferiority fails | **Regression.** Stop and find out which questions; a confident wrong graph answer is worse than no tool |

The 20% threshold is not arbitrary: below it the result is swamped by the tool-schema tax
on short questions and is not a claim anyone should put on a CV or a README.

> **Amended 2026-08-14 (A1.1).** The four verdicts, the cost threshold and the `+0.05`
> improvement threshold are **unchanged**. Two riders only:
>
> - Wherever this table says "non-inferior" or "non-inferiority", read §4.2 as amended: the
>   pooled, question-level, bootstrapped endpoint at margin **−0.10**. The `+0.05` figure in
>   the *Primer* row is a superiority threshold on the same quantity and is left where it is.
> - The rule is evaluated **once**, on the pooled study. If the §3.1 manipulation check fires
>   its fallback branch, the arm in the primary contrast changes (B → M) and this table is read
>   with that substitution; the thresholds do not move.

---

## 6. Confounds, and what is done about each

### 6.1 Pretraining contamination — the one that could invalidate everything

eShop, TodoApi, and FluentValidation are famous public repos. The model may have memorised
them. That inflates arm G (grep + recall beats grep alone) and *understates* the treatment.
It could equally cut the other way if recall is stale and wrong.

**Mitigation, and it is not optional:** at least one repo in the set must be one the model
cannot have seen — a private repo, or a public one with a mechanical identifier rename
(types, methods, namespaces, routes) applied before the run. **The headline number is
reported per repo, and the unseen repo's number is reported first.** If the effect exists
only on the famous repos, the honest conclusion is that the experiment measured recall.

### 6.2 Prompt leakage in the tool descriptions

The MCP tool descriptions name domain concepts — "MediatR handlers", "seams",
"integration events". Some of the treatment effect is the *descriptions* teaching the agent
what to look for, independent of the graph. This is legitimately part of the product, but
it must be stated in the write-up rather than discovered by a reader. Arm M vs arm B partly
separates it.

### 6.3 Ambient context bleeding into the runs

`CLAUDE.md`, hooks, auto-memory, project settings, and installed plugins all differ from
machine to machine and would silently change behaviour between arms.

**Control:** every run uses `--bare` (skips hooks, LSP, plugin sync, auto-memory, and
`CLAUDE.md` auto-discovery), `--strict-mcp-config` (only the MCP config passed on the
command line), and `--setting-sources` empty. The system prompt is supplied explicitly and
is byte-identical across arms.

### 6.4 Grader bias

**Blind grading.** The graded artifact is the agent's **final answer text only** — never the
transcript, because the tool calls reveal the arm. Arm labels are stripped and items are
shuffled before grading.

### 6.5 Stochasticity

`n = 5` repetitions per (question × arm), fresh session each. Report medians, not means.
Question order randomised; arms run interleaved, not blocked, so a mid-experiment change in
API latency or model routing hits all arms equally.

### 6.6 Runaway runs

A rabbit-holing baseline run would distort the mean and, worse, could be quietly excluded
later. **Cap every run at `--max-budget-usd 2.00`.** A run that hits the cap is scored
**incorrect with cost = cap** (right-censored), never dropped. Censoring rate per arm is
reported; if it differs by more than 10 points between arms, the medians are not comparable
and the write-up says so.

---

## 7. Grading protocol

Two passes, both mechanical where possible.

**Pass 1 — deterministic.** Answer keys carry `must_mention` (symbols/files that must
appear) and `must_not_mention` (the trap answers, e.g. `ProductPriceChangedIntegrationEvent`
for class D). Class E items score correct only on an explicit "none". Citation refs are
resolved against the repo at the pinned SHA by script. No model involved.

**Pass 2 — LLM judge**, for the parts a string match can't settle (class B trace ordering,
class C set overlap, explanation quality).

- Model: `claude-opus-5`, fresh session per item, `--bare`, effort `high`.
- Input: question + answer key + the anonymised final answer. **Nothing else.**
- Output: structured — `{correct: bool, fabricated: bool, missing: [], extra: []}`.
- **Validation:** a human grades a 20% stratified sample. Compute Cohen's κ against the judge. **If κ < 0.8, the judge's scores are discarded and the whole set is graded by hand.** A judge nobody checked is a random number generator with good manners.

> **Amended 2026-08-14 (A1.1) — the validation is made applicable, and its consequence is
> kept.** The line above is unrunnable as written whenever the judge's verdicts are lopsided,
> which is the normal case: on the pilot's 11-item sample the judge marked 10 of 11 correct and
> `fabricated: false` on **all** 11. Against a 10:1 marginal one human disagreement drops κ to
> 0.62 and a disagreement on the single minority item drops it to 0.00; against an 11:0
> marginal κ has no value at all (`1 − pe = 0`, which `kappa.mjs` correctly reports as
> *undefined* rather than as 1.0). A statistic that returns "0.00" and "undefined" for a judge
> that agreed on 10 or 11 of 11 is measuring prevalence, not the judge. **The 0.8 bar and its
> consequence are not moved.** What follows makes the bar land on something.
>
> **Sample.** 20% of the judged set = **96 of 480** items (`ceil(0.20 × n)`), stratified over
> the repo × arm × class cells at equal inclusion probability per run, seed `20260814`, copied
> to arm-free filenames with the mapping sealed until grading is complete — the protocol the
> pilot already executed at `results/r1.2-human-sample/`, at a size where it can mean something.
>
> **The `correct` verdict.** The judge is accepted only if **all three** hold:
>
> 1. **Raw agreement ≥ 0.90** over the 96 items — equivalently PABAK ≥ 0.80, which is what a
>    prevalence-adjusted reading of the same 0.8 bar resolves to.
> 2. **κ ≥ 0.8**, *when κ is applicable*: the judge's marginal on the sample is no more extreme
>    than **90:10** and neither marginal is degenerate. When it is more extreme, κ is reported
>    for the record and rule 2 is **replaced** by the minority-class check below — replaced,
>    not dropped.
> 3. **Every disagreement is adjudicated in writing by the human grader and published** with
>    the item. A disagreement that nobody explains is a disagreement nobody can weigh.
>
> **Minority-class check (the replacement for rule 2, and the stricter test of the two).** κ at
> an extreme marginal exists to catch a judge that is lenient on the rare verdict. Test that
> directly instead of hoping a random 20% samples it: take up to **20** further items the judge
> placed in the **minority** class across the whole set (all of them if fewer than 20), grade
> them by hand, and require **≥ 0.80 agreement within that enriched set**. This is deliberately
> the hardest place for a bad judge to survive, and its result is reported separately from the
> 96-item figure — an enriched sample's agreement rate is not comparable to a random one's, and
> the write-up must not pool them.
>
> **The `fabricated` verdict — the degenerate marginal, handled by name.** A study in which
> nothing is fabricated *should* produce an all-`false` column, and κ on it is undefined by
> construction. No threshold is applied to it, and "undefined" is **never** reported as 1.0 or
> as a pass. Instead: **every** item that *either* rater marks `fabricated: true` is adjudicated
> by hand and published, and the fabrication endpoint itself rests on pass 1's deterministic
> `must_not_mention` and class-E "none" checks, which involve no model at all. If both raters
> mark zero fabrications across the whole set, the write-up states "no fabrication observed;
> κ undefined by construction" and claims nothing further from it.
>
> **The consequence is unchanged.** Any failure above discards the judge's scores and the whole
> set is graded by hand — 480 items, and that is a schedule decision to be taken before the
> study is funded, not discovered after it.

---

## 8. Harness

Build on what exists. `eval/mcp-qa/dogfood.js` already has the per-call token logging; the
new part is driving a *real agent* rather than a scripted call list. The runner is
`claude -p` in headless mode — it reports usage and cost directly and enforces the arm
boundary through its own permission system, so the isolation is not something the harness
has to fake.

```powershell
# Arm G — grep baseline. No MCP server is even configured.
claude -p --bare --output-format json `
  --model claude-opus-5 `
  --add-dir C:\Code\DevContext2\eval-repos\eShop `
  --allowedTools "Read,Grep,Glob" `
  --disallowedTools "Edit,Write,WebFetch,WebSearch" `
  --max-budget-usd 2.00 `
  --system-prompt-file eval\agent-probe\system.txt `
  "$question"

# Arm M — MCP only. --strict-mcp-config keeps any user-level MCP config out.
claude -p --bare --output-format json `
  --model claude-opus-5 `
  --mcp-config eval\agent-probe\mcp.json --strict-mcp-config `
  --allowedTools "mcp__devcontext" `
  --disallowedTools "Read,Grep,Glob,Bash,Edit,Write" `
  --max-budget-usd 2.00 `
  --system-prompt-file eval\agent-probe\system.txt `
  "$question"

# Arm B — both.
claude -p --bare --output-format json `
  --model claude-opus-5 `
  --add-dir C:\Code\DevContext2\eval-repos\eShop `
  --mcp-config eval\agent-probe\mcp.json --strict-mcp-config `
  --allowedTools "Read,Grep,Glob,mcp__devcontext" `
  --disallowedTools "Edit,Write,WebFetch,WebSearch" `
  --max-budget-usd 2.00 `
  --system-prompt-file eval\agent-probe\system.txt `
  "$question"
```

Per run, persist: the full result JSON, the `--output-format stream-json` transcript (for
tool-call counting and `mcp_call_share`), and a row in `runs.jsonl` keyed
`(repo, question_id, arm, rep, model)`.

**Pre-flight assertions, run before every batch — a batch that fails any of these is void:**

1. `analyze` returns `cached: true` for every repo in every arm (warm, per §4.5).
2. Arm G's transcript contains **zero** `mcp__` tool calls.
3. Arm M's transcript contains **zero** `Read`/`Grep`/`Glob` calls.
4. `total_cost_usd` is non-zero on the first run (§4.1).
5. The DevContext build under test is a recorded git SHA, pinned for the whole experiment.

---

## 9. Sample size and analysis

- Pilot: 1 repo (eShop) × 6 questions × 3 arms × 3 reps = **54 runs**.
- Full: 4 repos × 6 questions × 3 arms × 5 reps = **360 runs**.

Analysis is paired by question. Bootstrap (10,000 resamples over questions) the median
`log2(cost_ratio)`; Wilcoxon signed-rank as a distribution-free companion. **No t-tests on
raw token counts** — the distribution is not normal and one censored run would carry the
result.

Accuracy differences: Wilson score interval on the paired difference.

> **Amended 2026-08-14 (A1.1).**
>
> - **Full run is 480 runs, not 360**: 4 repos × 6 questions × **4 arms** (G, M, B, BI) × 5
>   reps. The pilot's 54 stands as run.
> - **Accuracy differences are no longer a Wilson/Newcombe interval on run-level pairs.** Per
>   §4.2 as amended, reps are aggregated to a per-question accuracy and the 90% interval is a
>   10,000-resample bootstrap **over the 24 questions**, seed `20260814` — the same unit and
>   machinery this section already fixes for cost. The paired 2×2 and its exact McNemar stay in
>   `analyse.mjs` as *descriptive* output; they are no longer the endpoint.
> - The pilot's published analysis (`results/a1.2-analysis.md`) is **not** recomputed under the
>   new method. It was run under the unamended design and is read against the unamended text.

---

## 10. Cost and time

Rough, from the run shape (60–150k input tokens with caching, 2–5k output):

| Phase | Runs | Estimated |
|---|---|---|
| Pilot | 54 | ~$25–40 |
| Full | 360 | ~$150–250 |
| Judging | ~450 items | ~$15 |

Comparable to the Conductor run's `$224.20`, and reported the same way — actual spend
recorded, not estimated, in the write-up.

> **Amended 2026-08-14 (A1.1) — re-estimated from the pilot's *actual* spend, not from the
> guess above.** The pilot recorded **$23.67 for 54 runs** = `$0.438`/run, and **$3.48 for 54
> judge items** = `$0.064`/item (RESULTS §8). Applied forward:
>
> | Phase | Runs / items | Estimate at the pilot's realised unit cost |
> |---|---|---|
> | Adoption gate (§3.1, arm B alone on eShop) | 18 | **~$8** |
> | Full study, four arms | 480 | **~$210** |
> | Judging | 480 | **~$31** |
> | | | **~$250**, band $200–330 |
>
> The band's upper end allows for BI running longer than B — an instructed agent that actually
> calls the tools does more work per run than one that ignores them, and the pilot has no
> measurement of that cell. Actual spend is reported, per the paragraph above; this estimate is
> for the funding decision only, and the §6.6 per-run cap of `$2.00` is unchanged.

**Run the pilot first and stop.** Its job is not to produce a result; it is to find out
whether the harness is measuring anything: does `mcp_call_share` clear 0.2, does
`total_cost_usd` populate, do the arm-isolation assertions hold, is the between-rep variance
small enough that n=5 can resolve a 20% effect. Read those four numbers, fix what is broken,
then commit to the full run.

---

## 11. Extensions (only after the main result lands)

- **Model × arm.** Repeat arms G and B on `claude-sonnet-5`. The commercially interesting claim is not "Opus gets cheaper" but *"the MCP lets a cheaper model match a more expensive one's grep-only accuracy"* — a 3× cost story rather than a 20% one. This is the strongest possible finding and the reason to keep the harness parameterised by model from day one.
- **Implementation task.** The original probe-kit task ("add a per-line discount to orders") as a secondary endpoint, scored by does-it-build plus a rubric. Expensive to grade; worth it only once the Q&A result is in.
- **Per-tool attribution.** Which of the 22 tools appear in winning traces. Feeds the folding decisions that already removed `flow`, `insights`, and `interesting_points`.

---

## 12. Outputs

- `eval-results/<date>/agent-probe/RESULTS.md` — the numbers, per repo, unseen repo first.
- `runs.jsonl` + raw result JSON — so any number in the write-up can be re-derived.
- A one-line verdict in `PRODUCT-DIRECTION.md` §9 replacing "primer, not accelerator" with whatever this actually finds, including if that is "still a primer".
