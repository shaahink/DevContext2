---
name: Bug Report
about: Report unexpected output, crashes, or incorrect analysis
title: ''
labels: bug
assignees: ''

---

**DevContext version** (output of `devcontext version` or commit hash):

**Environment**:
- OS: (e.g., Windows 11, macOS 14, Ubuntu 24.04)
- .NET SDK version: (`dotnet --version`)
- Project type: (e.g., ASP.NET Core, class library, Worker Service)

**Reproduction steps**:
1. Run: `devcontext analyze <path> --scenario <name> [other flags]`
2. ...

**Expected output**:
What the tool should show.

**Actual output**:
What the tool shows. Paste the relevant section or attach the full output.

**Additional context**:
- Does the project use specific NuGet packages? (MediatR, EF Core, MassTransit, etc.)
- Are you using `devcontext.json` configuration?
- Does the issue reproduce across multiple projects?
