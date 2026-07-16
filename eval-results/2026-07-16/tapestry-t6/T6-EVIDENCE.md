# T6 — Workbench & pages revamp — evidence

Stage gate: `scripts/t6-feature-drive.mts` (eShop pole; the shamshir pole ran in T6.0) —
**16/16 PASS**, artifacts under `feature-drive/` (screenshots, `results.json`,
`eshop-onepager.md`). Earlier batteries: T6.0 `ui-pages-audit.md` + `shamshir-shots/`;
T6.5/T6.6 keyboard + theme-matrix batteries (committed with 6a73546).

## Checkpoint → proof

| # | What landed | Proof |
|---|-------------|-------|
| T6.0 | Shamshir pole of the 7-page audit (eShop pole 2026-07-15) | `ui-pages-audit.md`, `shamshir-shots/`, 16/16 drive steps |
| T6.1 | Home/Atlas honest on monolith+workers (.claude worktree exclusion, hub-radar noise filter) | commit 2e5da08 |
| T6.2 | Canvas: entry-kind glyphs on trace roots (`[HTTP]`/`[HUB]`/`[WORKER]`…), approx hops DASHED / verified solid (trace + neighbors) | `graph-canvas.ts` KIND_GLYPHS + `edge[?approx]`; visible in `feature-drive/02-trace-hero.png` |
| T6.3 | Insights honesty: Desktop/Library/Gateway sources archetype-GATED in the pipeline (finding 44); tier-first ranking (audit A11 "12% conf" #1); UI tier words (high/moderate/low, % in tooltip); writes-only validation counting; VM-View self-suppresses at 0 edges; internal-hubs ≥3-refs floor; no CLI-flag copy | `InsightHonestyTests.cs` (5); drive `T6.3 tier-words` + `archetype-copy` PASS; `04-insights-eshop.png` |
| T6.4 | Settings→Server shows the LIVE `serverBaseUrl()` + health target (audit B12) | drive `T6.4 live-server-url` PASS; `05-settings-server.png` |
| T6.5/T6.6 | Single-key nav, route-restore guard, keyboard + theme drive batteries | commit 6a73546 |
| T6.7 | Hero graphs draw edges; MAP header chips | commit 296fb39; drive `T6.7` ×2 PASS |
| T6.8 | Display names (296fb39) + repo-relative entry provenance at VM build, deck middle-ellipsis keeps route tails | drive `T6.8 no-abs-paths-explore` = 0 `C:\` fragments |
| T6.9 | Deck sorts wired-and-deep first; START-HERE trace tile = deep checkout flow else deepest request-shaped flow (never a shallow/unwired UI route — eShop's checkout-titled entries are ALL 1-hop client commands); session reattach on boot (`tryAdopt`) — reload does NOT re-analyze; tiles persist on revisit | drive `T6.9 trace-hero-deep` ("Trace PUT /api/catalog/items/{id:int}", ≥3 hops), `reattach-no-reanalyze` (Analyze 1→1), `tiles-on-revisit` |
| T6.10 | ToolCallEvent.origin (ui = gRPC-web content-type, agent = native gRPC); live feed defaults to agents-only; full-handle copy + "use" prefill button; try-a-tool succeeds via the button; host-config PATH note | drive `T6.10` ×3 PASS; `06-mcp-page.png` |
| T6.11 | EventWiringFacet on GraphFacets (the ONE T2.6 join, server-side); board + one-pager render it (approx chip only for facet-less legacy join); cross-service labels; file download beside clipboard export | drive `T6.11` ×4 PASS: 24 services 0 bare-API rows, 15 event rows, cross-service labels, download button; `eshop-onepager.md` |

## Live-drive catches (this wrap-up)

1. **Trace-checkout tile was a dead end on eShop** — every checkout-titled entry is a 1-hop
   MAUI/Blazor client command (`[RelayCommand] CheckoutViewModel.CheckoutAsync` = 2 nodes,
   verified via `query trace`). A title match alone can never satisfy the ≥3-hop gate; the
   tile now demos a deep checkout flow when one exists (dogfood) else the deepest
   request-shaped (HTTP/gRPC) flow, labeled by what it traces.
2. **Pre-existing CLI crash**: `analyze --focus "[RelayCommand] X.Y"` threw
   `Could not find color or style 'RelayCommand'` — `AnalyzeCommand.cs` interpolated the
   resolved-intent explanation into Spectre markup unescaped. Fixed with `Markup.Escape`.
3. **Drive-harness lesson**: `innerText` reflects CSS `text-transform: uppercase` — assert
   case-insensitively. "Entry focus — click a node" is the inspector's NORMAL empty-selection
   hint, not a dead-end signal; depth is asserted by counting nested `app-trace-node`.

## Gates at the tip

- `dotnet build` 0 warn/0 err · fast tests 517P/0F + 14P server · loom-guards PASS
- `pnpm check` green (45 vitest)
- t6-feature-drive 16/16 · full `eval/gates.ps1` battery: see `gates-t6-stage.txt`
