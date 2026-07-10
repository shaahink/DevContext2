# UI Drive Gate — Baseline (Loom L0.3)

**Date:** 2026-07-10  
**App:** http://localhost:4200 (server :5179 + ng :4200)  
**Screenshots:** `eval-results/2026-07-10/ui/*.png` (10) + `notes.md`  

Headless drive of the real UI. Each assertion targets a confirmed audit §5 defect and is
EXPECTED RED until its owner stage. The gate records reds with owners; it does not
green-wash. Run with `--gate` (armed in L6) to enforce.

## Assertions

| # | Assertion | Result | Audit | Owner | Detail |
|---|-----------|--------|-------|-------|--------|
| A-tabstrip-height | tab strip height >= 30px | PASS | U1 | L6.1 | stripH=32px (want >=30) |
| B-new-preserves-tabs | titlebar New preserves other tabs | PASS | U2 | L6.1 | before=["eshop-microservic…✕","TodoApp.sln✕"] after=["eshop-microservic…✕","TodoApp.sln✕","New tab✕"] lost=[] added=true |
| C-code-pane-nonempty | code pane non-empty on entry selection | PASS | U3 | L6.2 | code length=44 |
| D-context-preset-cards | context studio preset seeds >= 1 card | PASS | U6 | L6.4 | cards=5 |

## Red items enumerated (owner stage)

**Gate status:** OK (all reds are expected-red with owners; no regression)
