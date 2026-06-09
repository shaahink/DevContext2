# Security Policy

## Supported Versions

DevContext is a static analysis tool that never makes network calls, never
executes code, and never sends data outside your machine. It reads your
source files and produces structured Markdown or JSON output locally.

| Version | Supported |
|---|---|
| 2.x | ✅ |
| < 2.0 | ❌ |

## Reporting a Vulnerability

DevContext reads code from your local filesystem. If you find a scenario
where DevContext could read or expose unintended files, or produce output
that leaks sensitive information, please open a security advisory on
GitHub or email the maintainer directly.

We will acknowledge receipt within 48 hours and provide an assessment
timeline.
