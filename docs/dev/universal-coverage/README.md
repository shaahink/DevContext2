# universal-coverage

The audit and expansion that made DevContext detect **27 .NET repos** across all major archetypes — apps, libraries, frameworks, and gateways.

## What's here

| File | Purpose |
|------|---------|
| `MASTER-PLAN.md` | **Start here for the next work.** Resumable V2 plan — corrected archetype model, the detected≠rendered gap, the Entry Surface Catalog (anti-explosion), prioritized workstreams (WS-A…G), target output specs, eval strategy. Supersedes the order in FINDINGS-AND-PLAN. |
| `HANDOVER.md` | What P0/P3/P1 delivered — kernel hygiene rules, the 10 entry kinds, resume instructions for the *completed* phase. |
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
