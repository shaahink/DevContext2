# Git Heartbeat Cleanup — Post-Phase Instructions

**Current heartbeat count per `git log --all --oneline --grep="chore(conductor):"`:** 107 of 762 commits
**HeartbeatMinutes in plan JSON:** 0 (disabled for future sessions as of 2026-07-08)

## When to run

After the CURRENT phase (L5 or whichever phase is running when you read this) confirms green
+ audit passes. The running conductor session should be finished before running these commands.

## Cleanup commands

```powershell
# 1. Identify the phase-start commit (the first commit of the current phase)
$phaseStart = (git log --oneline --grep="stage .* L5" | Select-Object -Last 1).Split(" ")[0]

# 2. Interactive rebase — squash all chore(conductor): commits since phase start
# This opens an editor. Mark all chore(conductor): lines as "squash" (s), keep feat/fix/audit/docs lines as "pick" (p)
git rebase -i $phaseStart^ --committer-date-is-author-date

# 3. Force-push the squashed history
# WARNING: this rewrites public history. Ensure no other sessions are running.
git push --force-with-lease origin feat/loom-l5
```

## Alternative: automated squash (non-interactive)

```powershell
# Squashes ALL chore(conductor): commits since phase start into a single commit
# Keeps feat/fix/audit/docs commits as-is
$phaseStart = (git log --oneline --grep="stage .* L5" | Select-Object -Last 1).Split(" ")[0]
git rebase -i $phaseStart^ --exec "if git log -1 --pretty=%s | Select-String 'chore\(conductor\)'; then git reset --soft HEAD^ && git commit --amend --no-edit; fi" --committer-date-is-author-date
```

## Verification

After cleanup, heartbeat commits should be 0 (or 1 if the final phase-confirm commit is a
`chore(conductor):` commit). Real commits (feat/fix/audit/docs) should be preserved.

```powershell
git log --oneline --grep="chore(conductor):" | Measure-Object | Select-Object Count
# Should be 0 or 1
```
