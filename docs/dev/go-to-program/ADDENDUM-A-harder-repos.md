# Addendum A — Harder repos: EF Core depth, build-system intelligence, extended insights

> Authored 2026-07-02 (pass 3) · **Complements the frozen plan** — extends the I5 facet menu and the
> I3 insight catalog *by addition*; no existing guide is modified. Direction from the user: desktop
> apps are no longer a growth area for analysis investment; the edge is **dealing well with harder,
> data-heavy .NET repos** (EF Core-centric LOB systems, modern build machinery, framework-scale code).

## A1. Priority shift (guidance for whoever picks from the I5 menu)

- **F12 (desktop VM wiring) → LATER.** Detection already shipped (WinForms/WPF/MAUI entries stay
  green); no further investment. Skip it when picking.
- The two new facets below (**F14, F15**) enter the menu at ★ priority, alongside blast radius and
  the message matrix.

## A2. F14 — EF Core depth (the data-layer lens) ★

Most valuable on exactly the repos that are "hard": hundreds of entities, layered DbContexts,
migrations history. `EfCoreExtractor` already yields entities/DbContexts/IsAggregate/keys; extend it
plus one render facet. Gate: `efcore` signal.

| Slice | Detect | Deliver |
|---|---|---|
| Entity relation map | nav properties between known entities (syntax: property whose type/generic-arg is another entity) | DATA facet: per-context entity list with relation counts; `query facet data` returns the relation edges (graph canvas can render them) |
| Global query filters | `HasQueryFilter(` in `OnModelCreating` bodies | insight `ef.query-filters`: "Soft-delete/tenancy filters on N entities" — the invisible-WHERE devs get bitten by |
| Raw SQL sites | `FromSqlRaw/FromSqlInterpolated/ExecuteSqlRaw` call sites | insight `ef.raw-sql`: count + file:line evidence (injection review jump-off) |
| Migrations posture | `Migrations/` folder scan: count, latest name+timestamp prefix | insight `ef.migrations`: "214 migrations, latest 2026-06-12 AddOrderIndex" |
| Transaction boundaries | `BeginTransaction/TransactionScope` + `SaveChanges` overrides + MediatR `TransactionBehavior` (already detected as pipeline) | one Cross-cutting line: where the unit-of-work commits — the thing the eShop probe proved agents miss |
| Query hygiene | `AsNoTracking/AsSplitQuery` usage counts | fold into `ef.query-hygiene` insight (Info) — counts only, no judgment |

**Honesty limits (state in output, don't fake):** relations are declared-shape only (no fluent-API
`HasMany` parsing in v1 — note as follow-up); N+1 detection is NOT attempted (too speculative —
explicitly out). **Eval repo:** add `eShopOnWeb` (small, EF-heavy, aggregates + specs) with
expectations for each slice; DntSite covers the migrations/raw-SQL negative cases.

## A3. F15 — Build & solution intelligence (correct on modern machinery) ★

Modern "hard" repos break naive csproj reading. Two of these are **correctness bugs today**, not
features:

1. **Central Package Management (bug-grade):** with `Directory.Packages.props`,
   `<PackageReference Include="X"/>` carries no Version — verify what `CsprojReader` yields; wire
   `Directory.Packages.props` (nearest-ancestor chain) into `ProjectInfo.PackageReferences` so
   **signals and package groups stay correct on CPM repos**. Fixture: a 2-project CPM solution;
   expectation: versions present, `efcore` signal fires from CPM-declared package.
2. **`Directory.Build.props/targets` chain:** `OutputType`, `TargetFramework(s)`, `IsPackable` may
   live there — the ArchetypeDetector's exe/packable checks silently miss them. Resolve the ancestor
   chain (no full MSBuild eval — property lookup with nearest-wins is enough; document the limit).
3. **Multi-targeting:** `TargetFrameworks` (plural) → Overview STACK shows the matrix
   (`net8.0;netstandard2.0 ×12 projects`); analysis parses each file once (no per-TFM duplication —
   state that honestly in the scope note).
4. **InternalsVisibleTo map** (attribute or csproj item) → one Topology line ("internals shared:
   Core → Tests, Benchmarks") + palette-searchable.
5. **Source generators & analyzers consumed** (`Analyzer` items / `OutputItemType="Analyzer"` refs +
   known packages) → Cross-cutting line + **scope-note stamp: "generated code not analyzed"** when
   present — the honesty flag for CommunityToolkit-style repos whose real surface is generated.

## A4. Extended insight sources (I3's catalog grows by addition; launch-10 unchanged)

| Id | Category | Fires on |
|---|---|---|
| `ef.query-filters` / `ef.raw-sql` / `ef.migrations` / `ef.query-hygiene` | Data | §A2 |
| `async.sync-over-async` | Risk | `.Result`/`.Wait()`/`GetAwaiter().GetResult()` in production bodies (body-scan, span-attributed per I1 rules); evidence = worst 5 sites |
| `api.obsolete-still-used` | Risk | `[Obsolete]` member/type with ≥1 in-graph caller (attributes + InEdges — pure join) |
| `config.secrets-smell` | Risk | `Password=`/`AccountKey=`/`sk-` literals in appsettings*.json (local-only tool ⇒ honest to surface; wording "review", never echo the value) |
| `build.cpm-adopted` / `build.multi-targeting` | Shape | §A3 facts worth saying out loud |
| `test.footprint` | Info | test-project/type counts vs production (data exists in ProjectClassifier) — one line, Info only |

Same contract as I3: pure functions, noise-filtered, negative eval per source, silent when unremarkable.

## A5. Hard-repo eval & bench additions (feeds I7's suite)

Add to the I7 matrix: **eShopOnWeb** (EF depth) · a **CPM fixture** (build intelligence) · keep
HotChocolate as the standing perf ceiling. Acceptance line for this addendum: DevContext on an
EF-heavy LOB repo answers "what's the data model, where are the invisible filters, where do
transactions commit" in one run — that's the edge no generic tool has.
