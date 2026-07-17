# UI Drive Gate — Baseline (Loom L0.3)

**Date:** 2026-07-16  
**App:** http://localhost:4200 (server :5179 + ng :4200)  
**Screenshots:** `eval-results/2026-07-16/ui/*.png` (10) + `notes.md`  

Headless drive of the real UI. Each assertion targets a confirmed audit §5 defect and is
EXPECTED RED until its owner stage. The gate records reds with owners; it does not
green-wash. Run with `--gate` (armed in L6) to enforce.

## Assertions

| # | Assertion | Result | Audit | Owner | Detail |
|---|-----------|--------|-------|-------|--------|
| A-tabstrip-height | tab strip height >= 30px | PASS | U1 | L6.1 | stripH=32px (want >=30) |
| B-new-preserves-tabs | titlebar New preserves other tabs | PASS | U2 | L6.1 | before=["eshop-microservic…✕","TodoApp.sln✕"] after=["eshop-microservic…✕","TodoApp.sln✕","New tab✕"] lost=[] added=true |
| C-code-pane-nonempty | code pane non-empty on entry selection | PASS | U3 | L6.2 | code length=562 |
| D-context-preset-cards | context studio preset seeds >= 1 card | PASS | U6 | L6.4 | cards=5 |
| E-omitted-list-rendered | tiny-budget pack renders the omitted[] list | PASS | R1 | T5.1 | visible=true text=Omitted (8) tests (Validators for /products): no content for its entries — omitt |
| F-pack-error-retry | failed pack RPC shows card error; retry recovers | PASS | R4 | T5.1 | errorShown=true clearedAfterRetry=true |
| G-repack-on-budget | budget change re-packs: copy@4k != copy@1k, headers match slider | PASS | C1 | T5.6 | 4k=10749ch(hdr4k=true) 1k=7158ch(hdr1k=true) differ=true |
| H-plain-not-markdown | plain copy differs from markdown copy (bytes) | PASS | C1 | T5.6 | plain=6891ch differ=true noHeadings=true |
| I-verification-stale-cycle | verification ledger: fresh -> stale on disk edit -> fresh on restore | PASS | R6 | T5.2 | fresh=true staleAfterEdit=true freshAfterRestore=true |
| J-keyboard-battery | single-key + g-prefix nav, ? help, Ctrl+K omnibox all work | PASS | 37 | T6.5 | {"singleE":"/explore","singleA":"/atlas","gThenH":"/","helpOpen":true,"omniboxOpen":true} |
| K-route-restore-home | fresh boot of / renders Home even after a /settings visit | PASS | 49 | T6.5 | restoreHome=true |
| L-theme-matrix | every declared vibe x theme paints the shell surfaces in that mode | PASS | 38-39 | T6.6 | modern/dark:ok modern/light:ok terminal/dark:ok hacker/dark:ok |
| M-rpc-budget | page-render RPC budget: fresh load <20 UI calls, each SPA nav <=15 | PASS | B11 | T7.4 | fresh=8{"Ping":2,"ListSessions":1,"GetMap":1,"ListEntryPoints":1,"GetGraphFacets":1,"GetFlowIndex":1,"GetStats":1} atlas |

## Red items enumerated (owner stage)

**Gate status:** OK (all reds are expected-red with owners; no regression)
