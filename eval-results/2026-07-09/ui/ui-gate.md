# UI Drive Gate — Baseline (Loom L0.3)

**Date:** 2026-07-09  
**App:** http://localhost:4200 (server :5179 + ng :4200)  
**Screenshots:** `eval-results/2026-07-09/ui/*.png` (10) + `notes.md`  

Headless drive of the real UI. Each assertion targets a confirmed audit §5 defect and is
EXPECTED RED until its owner stage. The gate records reds with owners; it does not
green-wash. Run with `--gate` (armed in L6) to enforce.

## Assertions

| # | Assertion | Result | Audit | Owner | Detail |
|---|-----------|--------|-------|-------|--------|
| A-tabstrip-height | tab strip height >= 30px | RED (until L6) | U1 | L6.1 | stripH=nullpx (want >=30) |
| B-new-preserves-tabs | titlebar New preserves other tabs | RED (until L6) | U2 | L6.1 | New button not exercised (no session/button) |
| C-code-pane-nonempty | code pane non-empty on entry selection | RED (until L6) | U3 | L6.2 | code length=null |
| D-context-preset-cards | context studio preset seeds >= 1 card | RED (until L6) | U6 | L6.4 | cards=null |

## Red items enumerated (owner stage)
- **A-tabstrip-height** — tab strip height >= 30px → **L6.1** (audit U1)
- **B-new-preserves-tabs** — titlebar New preserves other tabs → **L6.1** (audit U2)
- **C-code-pane-nonempty** — code pane non-empty on entry selection → **L6.2** (audit U3)
- **D-context-preset-cards** — context studio preset seeds >= 1 card → **L6.4** (audit U6)

**Gate status:** OK (all reds are expected-red with owners; no regression)
