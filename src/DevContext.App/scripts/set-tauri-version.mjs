#!/usr/bin/env node
// H3 (Prism D5.4) — installers carry the release tag's version instead of the hardcoded conf
// default. Called by release.yml before `tauri build`: takes the tag (v1.2.3 or v1.2.3-preview.4),
// extracts the numeric major.minor.patch (MSI/WiX rejects prerelease suffixes; the CLI's NuGet
// package keeps the full semver via MinVer), and writes it into src-tauri/tauri.conf.json.
import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const tag = process.argv[2] ?? '';
const m = tag.match(/^v?(\d+\.\d+\.\d+)/);
if (!m) {
  console.error(`set-tauri-version: cannot extract x.y.z from tag "${tag}"`);
  process.exit(1);
}
const version = m[1];
const confPath = join(dirname(fileURLToPath(import.meta.url)), '..', 'src-tauri', 'tauri.conf.json');
const conf = JSON.parse(readFileSync(confPath, 'utf8'));
conf.version = version;
writeFileSync(confPath, JSON.stringify(conf, null, 2) + '\n');
console.log(`set-tauri-version: tauri.conf.json version -> ${version} (from "${tag}")`);
