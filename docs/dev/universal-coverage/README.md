# universal-coverage

The audit and expansion that made DevContext detect **27 .NET repos** across all major archetypes — apps, libraries, frameworks, and gateways.

## What's here

| File | Purpose |
|------|---------|
| `HANDOVER-V2.md` | **Current.** What WS-A through WS-F delivered — full V2 handover, architecture guide, performance note, resume instructions. |
| `MASTER-PLAN.md` | Original V2 plan — corrected archetype model, workstream descriptions, output specs. |
| `HANDOVER.md` | What P0/P3/P1 delivered (phase 1 — now superseded by V2). |
| `FINDINGS-AND-PLAN.md` | Raw 27-repo analysis matrix, systemic failure diagnosis, priority-ordered fix plan. |
| `run-all.ps1` | Re-run the batch analysis across all 27 repos (`--Fast` skips the 6 huge ones). |
| `analysis/` | Raw JSON + Markdown outputs from every repo analysis (pre-fix baselines). |

## How to re-analyze

```powershell
# Analyze all non-huge repos (20 repos, ~2 min)
powershell -File docs/dev/universal-coverage/run-all.ps1 --Fast

# Analyze only the 15 new repos
powershell -File docs/dev/universal-coverage/run-all.ps1 --OnlyNew
```
