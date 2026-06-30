# Output Contract — what each artifact must contain to be "go-to for any .NET repo"

**Purpose.** The gradeable spec the benchmark matrix scores against. For every repo, the next agent fills the
scorecard at the bottom: does the actual Map/Trace output answer the three at-a-glance questions for that
archetype? A cell is **PASS** only if the *required* info is present, correct, and noise-free.

This **extends, does not replace**, the canonical contract in the main repo:
- `docs/product/ACCEPTANCE.md` — acceptable output per artifact (§1 entries · §2 topology/Map · §3 trace · §4 stats) + the phase ratchet. **Canonical.**
- `docs/product/IDEAL-OUTPUT-TARGET.md` — the hand-built *shape* of a great Map/Trace/library surface.
- `docs/product/PRODUCT-DIRECTION.md` §4 — the entry-point ladder (which entry kinds exist per archetype).

What this doc adds: (a) reorganizes the requirements by the **three north-star questions × archetype** (the
lens the user actually applies), and (b) specifies the archetypes ACCEPTANCE.md doesn't yet cover —
**Desktop** and **API-Gateway** — plus the **library trace-on-demand** and **framework/monorepo-scale** cases.

---

## The three questions (every archetype must answer all three)

> **Q1 "What is this?"** · **Q2 "How do I use it / where do I start?"** · **Q3 "How does any part work?"**

### Q1 — What is this?  *(the Map preamble — universal, all archetypes)*
The Map header block MUST contain, and be correct:
- **Identity**: `<ARCHETYPE>  <name>  (<scale>)` — archetype label right (App / Library / Desktop / Gateway /
  Microservices / Worker / CLI…), scale = project count or scope stamp.
- **STACK**: runtime TFMs (**no `$(MSBuildVar)` noise**) · web framework · CQRS/mediator · data · messaging ·
  validation — only what's actually present.
- **STYLE**: primary style + confidence + one line of **evidence**; **scope-stamped** when a project closure
  (`(5-project closure of 24)`) — never claim system-level style from one service.
- **TOPOLOGY**: project depends-on graph, **readable** (capped/ranked on large repos — see W4).
- *Fail conditions:* wrong archetype (Files→Library), unscoped style claim, `$(` in STACK, 395-line topology dump.

### Q2 — How do I use it / where do I start?  *(archetype-specific — the entry surface)*
The single most important section. Requirements differ by archetype (below). **Cross-cutting hard rule (W1):**
the entry/surface list is **production-only** — zero test-asset, sample, stress, functional-test, or template
sources; zero infra pseudo-entries (`GET /` from Scalar/OpenAPI/ServiceDefaults).

### Q3 — How does any part work?  *(focus/trace dive-in)*
A user picks any entry/type and gets a faithful walk **down the wiring**. Must satisfy ACCEPTANCE §3:
correct (no sibling fabrication; `[verified]` = method-scoped), complete (call·send·handle·raise·consume·
data·di·pipeline; TOUCHES via Calls), honest cuts (`stopped at depth N; K omitted`), structurally bounded
(depth·fan-out·framework-stop — **not** token). Per-archetype dive-in expectations below.
- *Fail conditions:* 4-line "ENTRY only" trace (library/desktop today), sibling-method edge bleed, silent truncation.

---

## Per-archetype required content

### A — Web app (controllers / minimal API / CQRS-DDD / microservices)  *(eShop, TodoApi, VerticalSlice, DntSite)*
- **Q1:** App; web framework + (if present) MediatR/EF/messaging in STACK; layered/clean/microservices style honest.
- **Q2 — ENTRY POINTS** grouped by kind (HTTP · Bus · Domain · Background · Scheduled), each with `file:line`
  and a `→ target` (action/command/handler) where resolvable; honest `N/M → target` coverage. Microservices:
  also the **cross-service integration-event bus** (who publishes/consumes). Cross-cutting (MediatR pipeline,
  aggregates) surfaced once.
- **Q3:** endpoint trace matches IDEAL-OUTPUT-TARGET §2 — send→handler→raises→consumes→data, pipeline once,
  RESULT/TOUCHES/EMITS/NEXT. *(Status: PASS on eShop/TodoApi/VerticalSlice.)*

### D — Library / NuGet / framework  *(Serilog, FluentValidation, Polly, AutoMapper, CommunityToolkit)*
- **Q1:** Library; `(<N> public types)`; runtime-only packages (test/bench/sample deps excluded).
- **Q2 — surface map** (IDEAL §4): **ENTRY API** ranked register→build→derive/implement→extend with `///`
  summaries · **ABSTRACTIONS** by implementor count · **PUBLIC SURFACE** by namespace, `*.Internal` demoted ·
  **CONSUMER PATHS**. Source-gen libs lead with marker attributes + a GENERATORS section.
- **Q3 — trace on demand** (off by default): focusing a public type/method produces a **non-empty** walk of
  that type's own call wiring (member-origin). *(Status: ❌ today — `--focus Log` → 4 lines. Fixed by W3.)*

### E — Desktop app (WinUI / WPF / Avalonia / MAUI)  *(Files)*  — **NEW, not in ACCEPTANCE yet**
- **Q1:** **Desktop App** (not Library); UI framework in STACK (WinUI/WPF/Avalonia); main exe identified.
- **Q2 — UI entry points:** app startup (`App.OnLaunched`/`OnStartup`/`Main`), windows/pages/views, and
  command handlers (`[RelayCommand]`/`ICommand.Execute`) — the user-triggerable surface. *(Status: ❌ today —
  none detected; classified Library. Fixed by W5.)*
- **Q3:** focusing a window/command traces into its ViewModel methods and services. *(Needs W3 + W5.)*

### G — API gateway / reverse proxy (Ocelot / YARP)  — **NEW, not in ACCEPTANCE yet**
- **Q1:** **Gateway**; reverse-proxy framework in STACK; product solution scoped (not the samples — W6).
- **Q2 — ROUTES/CLUSTERS from config** (`ocelot.json Routes[]`, YARP `ReverseProxy:Routes`) + the middleware
  pipeline + any admin/management controllers. The dynamic route table **is** the surface; the few MVC admin
  endpoints are secondary. *(Status: ❌ today — only 3 admin controllers shown, gateway nature hidden. W7.)*
- **Q3:** trace an admin endpoint (works today) and/or a route→downstream cluster mapping.

### W/C — Worker · CLI · gRPC · Blazor · serverless  *(coverage gaps — Tier 3)*
- **Q1:** archetype recognized (Worker / Console / gRPC service / Blazor / Functions).
- **Q2 — entries per the ladder:** hosted-service `ExecuteAsync` + queue consumers (worker); `Main` +
  command verbs (CLI); service methods (gRPC); `@page`/component lifecycle (Blazor); `[Function]`/trigger
  (serverless).
- **Q3:** trace one entry into its handler/service. *(Status: 🔬 no benchmark yet — add per Phase E.)*

---

## Benchmark scorecard (fill per repo; drives `BENCHMARK-MATRIX.md`)

For each repo capture Map (+ one Trace) and mark each cell **PASS / GAP(W#) / FAIL**, with a one-line reason.

| Repo | Archetype | Q1 what-is-this | Q2 entries/surface | Q3 dive-in | Noise-free (W1) |
|------|-----------|-----------------|--------------------|------------|-----------------|
| Serilog | Library | PASS | PASS | **PASS (W3)** 153-line trace, Logger.Write→MessageTemplateProcessor… | PASS |
| Ocelot | Gateway | **GAP W7** (reads as admin app, routes hidden) | **PASS (W6+W8)** 3/3→target, product-scoped | PASS (admin ep) | PASS |
| Files | Desktop | **FAIL W5** (→Library) | **FAIL W5** (no entries) | **FAIL W3/W5** | PASS |
| aspnetcore | Framework | GAP W4 (topology dump) | **PASS (W1)** 518→10 | **PASS (L2)** | **PASS (W1)** |
| eShop / TodoApi / VerticalSlice | Web app | PASS | PASS | PASS | PASS |
| AutoMapper / FluentValidation / Polly | Library | PASS* | PASS | **PASS (W3)** | PASS |

\*AutoMapper style still "NLayer" (archetype-D surface aspirational). Update this table as items land; a row is
**done** when every cell is PASS.

**Definition of done for "go-to for any .NET repo":** every Tier-1/2/3 row in `BENCHMARK-MATRIX.md` has all
five columns PASS, and each archetype's acceptance check is `expected` (not aspirational) and green in
`eval/gates.ps1`.
