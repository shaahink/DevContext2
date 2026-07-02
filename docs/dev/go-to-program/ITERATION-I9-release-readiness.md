# Iteration I9 — Release readiness (the table stakes)

> **Status: NOT STARTED** · Addendum (complements I1–I7; no existing-doc edits) · Depends on: I4
> (settings view exists), I8 (Storage group) · CI/CD & code-signing explicitly out of scope here.
> This is the "what must a desktop app + CLI of this level have before strangers install it" list.
> Work through it as a checklist; each unchecked box is a small self-contained slice.

## A. Desktop (Angular/Tauri) — general app features

**Identity & about**
- [ ] About panel (Settings → About): app version + git commit, **engine version** (from `Ping`),
      Tauri/Angular versions, license (project OSS license), **third-party notices** (aggregate:
      `npm licenses` + NuGet packages + cargo — generate a NOTICES.md at build, render it),
      links: repo · docs · report-issue (pre-filled GitHub issue with versions).
- [ ] Privacy line, verbatim: *"Everything runs locally. Your code never leaves your machine.
      No telemetry."* (and keep it true.)
- [ ] App icon set + window title `DevContext — <repo label>` per active tab.

**Windowing & lifecycle**
- [ ] Window size/position/maximized persisted & restored (Tauri window-state plugin); min 1024×640.
- [ ] Single-instance (second launch focuses the running window; a path argument opens a new tab —
      pairs with I10).
- [ ] Graceful exit: cancel running analyses, flush prefs, stop the sidecar server.

**Server (sidecar) supervision**
- [ ] Server as Tauri sidecar with supervised lifecycle: crash → header dot red → auto-restart once →
      "Restart engine" action (UI-UX-GUIDELINES §2.1 already wires the status dot).
- [ ] Port conflict: pick a free port, pass to the webview via Tauri state — never a hard-coded port
      failure. Localhost-bind only, loopback asserted.

**Errors, logs, updates**
- [ ] Global error boundary: uncaught UI error → toast + "copy details" (versions + stack);
      `provideBrowserGlobalErrorListeners` is already wired — route it to the toast + log file.
- [ ] Log files: app log (tauri-plugin-log) + server log (Serilog file sink) under
      `%LOCALAPPDATA%/DevContext/logs/`, 7-day rolling; Settings → Server: "Open logs folder".
- [ ] Update check (manual, not auto): About → "Check for updates" hits the GitHub releases API,
      compares semver, links the release page. **VOTE:** no Tauri auto-updater at v1 (signing +
      infra burden); manual check + notice is honest and cheap.
- [ ] First-run experience: empty landing shows a one-liner + "try it on a sample" (analyze a small
      bundled-path suggestion or a well-known GitHub URL) + the ⌘K hint.

**Quality floor (already the house rule — verify before release)**
- [ ] Empty/loading/error triad on every view (UI-UX-GUIDELINES §3) — audit pass.
- [ ] Keyboard map + `?` overlay work; all buttons real buttons; focus visible; reduced-motion.
- [ ] No dead controls, no placeholder zeros (the WPF D1–D11 regression list — re-audit the Angular
      app against those 11 items explicitly).

## B. CLI — polish floor

- [ ] **Exit codes documented & stable** in cli-reference: `0` ok · `1` usage/input · `2` strict
      self-check failure · `3` analysis failure · `4` network/clone. Test asserting each.
- [ ] **stdout/stderr discipline audit:** content (markdown/JSON) → stdout; progress, warnings,
      stamps, diagnostics → stderr. Today's `AnsiConsole.MarkupLine` calls mix them — sweep and fix;
      `devcontext analyze -f X --format json | jq` must never break.
- [ ] `--quiet` (suppress stderr chrome) + `NO_COLOR` / `--no-color` + auto-plain when redirected
      (Spectre profile detection — verify it's on).
- [ ] `--version` prints semver + commit; `version` command matches.
- [ ] Update notice: on run, at most once/day (cached under the I8 storage root), check NuGet for a
      newer `DevContext.Cli`; print one stderr line; `DEVCONTEXT_NO_UPDATE_CHECK=1` disables. Never
      block, 1s timeout, silent on failure.
- [ ] Shell completions: generate static completion scripts (`devcontext completion pwsh|bash|zsh`)
      covering commands + option names (Spectre exposes the model; a small generator command is fine).
- [ ] Help quality pass: every command has one usage example in `--help`; errors suggest the nearest
      valid flag/op ("did you mean --focus?").
- [ ] Config file precedence documented in cli-reference (`devcontext.json` < env < flags) + one test.

## C. Server

- [ ] `Ping` returns engine version + schema version (About + compatibility check need it).
- [ ] Graceful shutdown on SIGTERM (drain in-flight analyze, close sessions).
- [ ] Startup log line states bind address + "local only".

## Docs & gate

`desktop-ui.md` + `cli-reference.md` sections for everything user-visible (same-commit rule).
Gate: the checklist above fully ticked in the PR description, with screenshots for About/Storage/
first-run and a paste of `analyze --format json | jq .archetype` proving the stream discipline.
