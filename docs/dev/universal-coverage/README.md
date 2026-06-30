# universal-coverage

The audit and expansion that made DevContext detect **27 .NET repos** across all major archetypes — apps, libraries, frameworks, and gateways.

## What's here

| File | Purpose |
|------|---------|
| `HANDOVER.md` | **Start here.** Full handover doc — master plan, what was delivered, kernel hygiene rules, resume instructions. |
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
