# D5.4 — H2/H3 delivery decision (Windows-only bundle, tag-derived installer version, dry-run)

**Decision (H2): desktop bundle is Windows-only — now ENCODED, not just documented.**
`tauri.conf.json` `bundle.targets` narrowed `"all"` → `["nsis","msi"]`; release.yml header states
the decision and why (no mac/linux hardware to verify bundles); desktop job renamed
`(Windows-only)`; README §Platform support already said the same (H4).

**H3: installer version derives from the release tag.** New
`src/DevContext.App/scripts/set-tauri-version.mjs` (tag → numeric x.y.z; MSI/WiX rejects
prerelease suffixes — the CLI nupkg keeps full semver via MinVer) + a release.yml step before
`tauri build`. Local drive: `v1.4.7-preview.2` → conf `1.4.7`; garbage tag → exit 1.

**Dry-run instrument:** `workflow_dispatch` trigger — uploads the full artifact inventory but the
NuGet push is tag-guarded and the GitHub Release job is `if: ref_type == 'tag'`; a dispatch can
never publish. Dispatch version stamps 0.0.0 (unpublishable marker).

## Dry-run verdicts (branch feat/prism-d5)

- **Run 1 (29665930176): FAILED — a REAL latent release bug caught on the instrument's first
  flight.** `pnpm/action-setup@v4` reads `packageManager` from the repo-root package.json, which
  doesn't exist in this monorepo; ci.yml/eval.yml already carried the `package_json_file` fix but
  release.yml predated it. Any real `v*` tag release would have failed identically at the desktop
  job. Fixed @ 28273d3.
- **Run 2 (29666022448): GREEN.** Desktop ✓ (incl. the H3 version step), CLI pack ✓, GitHub
  Release **skipped** (tag guard proven live).

## Artifact inventory (run 29666022448) — matches the decision exactly

| Artifact | Contents |
|---|---|
| `desktop-installers` (34.4 MB) | `nsis/DevContext_0.0.0_x64-setup.exe` + `msi/DevContext_0.0.0_x64_en-US.msi` — the two narrowed Windows targets, nothing else |
| `nuget-package` (15.6 MB) | CLI .nupkg (MinVer-versioned) |

No mac/linux artifacts; no release created; 0.0.0 stamp confirms the version step executed
(a real tag substitutes its own version through the same step).
