// A1.1 - the unseen-repo builder (DESIGN.md sections 3.2 and 6.1). Pure ASCII on purpose.
//
// Produces the study's fourth repository by applying a DETERMINISTIC mechanical identifier
// rename to a pinned public repo. The point is contamination: DESIGN 6.1 calls pretraining
// recall "the one that could invalidate everything", because a model that has memorised a famous
// repo can answer arm G's questions without searching, which inflates the control and understates
// the treatment. A renamed tree breaks *answer recall* - the symbols the memorised answers name
// no longer exist - while leaving both arms looking at exactly the same code.
//
//   node eval/agent-probe/rename-repo.mjs --survey --src <abs dir>
//   node eval/agent-probe/rename-repo.mjs --src <abs dir> --out <abs dir> --seed 20260814
//
// Determinism is the whole contract: same (src tree, seed) in, byte-identical tree out. Nothing
// in here reads the clock, the filesystem order beyond a sort, or Math.random.
//
// WHAT IS RENAMED, and why the line is drawn there:
//   - namespace segments declared by the repo itself
//   - type names declared by the repo itself (class/interface/struct/record/enum)
//   - the file and directory names that carry those identifiers
// WHAT IS NOT:
//   - members (methods, properties, fields). Renaming a member without a semantic model breaks
//     every interface implementation and every override the moment the interface comes from a
//     package. Roslyn could do it; a text pass cannot, and a tree that does not compile is worse
//     than a contaminated one. The type-level rename already breaks recall of the answer keys.
//   - anything reachable from an external `using` - package namespaces and their types stay put,
//     or restore fails.
// The residual is recorded in the manifest and in DESIGN 6.1: package references still name their
// authors, so the tree's *provenance* is guessable even though its *answers* are not.

import { readFileSync, writeFileSync, mkdirSync, readdirSync, statSync, copyFileSync, rmSync, existsSync } from "node:fs";
import { join, dirname, relative, basename, extname, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { createHash } from "node:crypto";

const HERE = dirname(fileURLToPath(import.meta.url));
const argv = process.argv.slice(2);
const argOf = (n, d) => { const i = argv.indexOf(`--${n}`); return i >= 0 && argv[i + 1] ? argv[i + 1] : d; };
const hasFlag = (n) => argv.includes(`--${n}`);
const die = (m) => { console.error(`rename-repo: ${m}`); process.exit(1); };

const SRC = argOf("src", "");
const OUT = argOf("out", "");
const SEED = argOf("seed", "20260814");
const SURVEY = hasFlag("survey");

// --verify recomputes the tree hash over an existing directory and checks it against the committed
// manifest. It reads nothing else, so a reader can check the pin without a source repo, a seed, or
// this tool's opinion of what it did.
if (hasFlag("verify")) {
  const dir = argOf("verify-dir", join(HERE, "unseen", "Driewie"));
  const man = JSON.parse(readFileSync(join(HERE, "unseen-repo.manifest.json"), "utf8"));
  const rows = [];
  (function scan(d) {
    for (const e of readdirSync(d, { withFileTypes: true }).sort((x, y) => (x.name < y.name ? -1 : 1))) {
      if (e.name === "bin" || e.name === "obj") continue;
      const p = join(d, e.name);
      if (e.isDirectory()) scan(p);
      else rows.push([relative(dir, p).split(sep).join("/"), createHash("sha256").update(readFileSync(p)).digest("hex")]);
    }
  })(dir);
  rows.sort((a, b) => (a[0] < b[0] ? -1 : 1));
  const got = createHash("sha256").update(rows.map(([p, h]) => `${p} ${h}`).join("\n")).digest("hex");
  const ok = got === man.treeSha256 && rows.length === man.counts.filesEmitted;
  console.log(`files   expected ${man.counts.filesEmitted}  found ${rows.length}`);
  console.log(`sha256  expected ${man.treeSha256}\n        found    ${got}`);
  console.log(ok ? "VERIFY PASS" : "VERIFY FAIL");
  process.exit(ok ? 0 : 1);
}

if (!SRC) die("--src <absolute dir> is required");
if (!existsSync(SRC)) die(`--src does not exist: ${SRC}`);
if (!SURVEY && !OUT) die("--out <absolute dir> is required unless --survey");

// ---- what is copied at all -----------------------------------------------------------------
// Directories that are build output, VCS metadata, or vendored web assets. Excluding vendored
// css/js keeps the committed tree at code size; none of it is compiled, so the build is unaffected.
// Excluding the repo's own prose is NOT hygiene - a README naming the upstream project would hand
// the model the identity the rename exists to remove.
// `.claude`, `.template.config` and `docs` go too: agent instructions, a template manifest and a
// prose folder each name the upstream project, and a tree that says who it is has not been
// anonymised, only renamed.
const SKIP_DIRS = new Set([".git", "bin", "obj", ".vs", ".idea", ".github", ".claude", ".vscode",
  ".template.config", "docs", "node_modules", "TestResults"]);
const SKIP_FILE_RE = /(\.min\.(js|css)|\.map|\.md|\.png|\.jpg|\.jpeg|\.gif|\.ico|\.svg|\.woff2?|\.ttf|\.eot|\.snk|\.pfx)$/i;

// Files whose CONTENT gets the rename applied. Anything else is copied byte-for-byte.
const TEXT_EXT = new Set([
  ".cs", ".csproj", ".props", ".targets", ".sln", ".slnx", ".json", ".yml", ".yaml",
  ".http", ".razor", ".cshtml", ".config", ".runsettings", ".editorconfig", ".ps1",
  ".sh", ".txt", ".xml", ".sql", ".resx", ".cshtml.cs", ".sqlproj", ".dockerfile",
]);
const isText = (p) => TEXT_EXT.has(extname(p).toLowerCase()) || basename(p).toLowerCase() === "dockerfile";

// ---- deterministic pseudonyms --------------------------------------------------------------
// A seeded hash into a fixed syllable table. Names come out pronounceable but meaningless, which
// is what we want: a meaningless name is unmemorised, and a pronounceable one keeps the tree
// readable so neither arm is handicapped by hex soup.
const ON1 = ["b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "z", "br", "cr", "dr", "fl", "gr", "pl", "st", "tr"];
const VOW = ["a", "e", "i", "o", "u", "ai", "ea", "ie", "oa", "ou"];
const COD = ["", "", "", "l", "n", "r", "s", "m", "k", "t", "d"];

function coin(key, syllables) {
  const d = createHash("sha256").update(`${SEED}|${key}`).digest();
  let out = "";
  for (let i = 0; i < syllables; i++) {
    const on = ON1[d[i * 3] % ON1.length];
    const vo = VOW[d[i * 3 + 1] % VOW.length];
    let co = COD[d[i * 3 + 2] % COD.length];
    // No doubled consonant across the syllable seam - "Talven" reads, "Tallven" does not.
    const nextOn = i + 1 < syllables ? ON1[d[(i + 1) * 3] % ON1.length] : "";
    if (co && nextOn && co === nextOn[0]) co = "";
    out += on + vo + co;
  }
  return out.charAt(0).toUpperCase() + out.slice(1);
}

// Architectural suffixes are PRESERVED. The domain noun is what a model memorises
// (`CreateContributorCommandHandler` -> `CreateFlaeskCommandHandler`); the pattern vocabulary is
// what both arms reason with. Stripping it would degrade grep and the graph together, which is a
// worse experiment, not a purer one.
const SUFFIXES = [
  "CommandHandler", "QueryHandler", "EventHandler", "NotificationHandler", "RequestHandler",
  "Specification", "Configuration", "Repository", "Controller", "Middleware", "Validator",
  "Extensions", "Exception", "Attribute", "Aggregate", "Interface", "Behaviour", "Behavior",
  "Endpoint", "Response", "Provider", "Settings", "Registry", "Factory", "Builder", "Service",
  "Handler", "Command", "Request", "Options", "Context", "Manager", "Adapter", "Wrapper",
  "Filter", "Result", "Record", "Config", "Client", "Worker", "Module", "Mapper", "Query",
  "Event", "Model", "Store", "Tests", "Test", "Dto", "Api", "Db",
];
function splitSuffix(name) {
  for (const s of SUFFIXES) {
    if (name.length > s.length + 2 && name.endsWith(s)) return [name.slice(0, -s.length), s];
  }
  return [name, ""];
}

const used = new Set();
function pseudonym(name) {
  const iface = /^I[A-Z]/.test(name);
  const bare = iface ? name.slice(1) : name;
  const [stem, suffix] = splitSuffix(bare);
  const syll = stem.length <= 6 ? 2 : 3;
  let cand;
  for (let salt = 0; ; salt++) {
    cand = coin(salt === 0 ? stem : `${stem}#${salt}`, syll);
    const full = (iface ? "I" : "") + cand + suffix;
    if (!used.has(full.toLowerCase())) { used.add(full.toLowerCase()); return full; }
    if (salt > 500) die(`could not find a free pseudonym for ${name}`);
  }
}

// ---- walk ----------------------------------------------------------------------------------
function walk(dir, acc = []) {
  for (const e of readdirSync(dir, { withFileTypes: true }).sort((x, y) => (x.name < y.name ? -1 : 1))) {
    if (SKIP_DIRS.has(e.name)) continue;
    const p = join(dir, e.name);
    if (e.isDirectory()) walk(p, acc);
    else if (!SKIP_FILE_RE.test(e.name)) acc.push(p);
  }
  return acc;
}

// ONE SOLUTION, not the whole tree. This matters more than it looks: the source repo ships three
// independent solutions, and a type one of them DECLARES another one gets from a package. A
// repo-wide declaration sweep therefore renamed `LoggingBehavior` and `ServiceConfig` at their
// *use* sites in the main solution, where they come from `NimblePros.SharedKernel` - and the
// build said so, in CS0246, which is why the build is part of this checkpoint and not a nicety.
// `--include` names the top-level directories of the one solution being extracted; root-level
// files (`Directory.Packages.props`, the `.slnx`, `.editorconfig`) always come along.
const INCLUDE = argOf("include", "").split(",").map((s) => s.trim()).filter(Boolean);
const included = (f) => {
  if (!INCLUDE.length) return true;
  const rel = relative(SRC, f).split(sep);
  return rel.length === 1 || INCLUDE.includes(rel[0]);
};

const files = walk(SRC).filter(included);
const csFiles = files.filter((f) => f.toLowerCase().endsWith(".cs"));

// ---- collect what the repo DECLARES --------------------------------------------------------
const NS_DECL = /(?:^|\n)\s*namespace\s+([A-Za-z_][\w.]*)\s*[;{]/g;
const USING = /(?:^|\n)\s*(?:global\s+)?using\s+(?:static\s+)?(?:[A-Za-z_]\w*\s*=\s*)?([A-Za-z_][\w.]*)\s*;/g;
// A type declaration: modifiers, then the keyword, then the name. `record struct X` and
// `record class X` are handled by the optional second keyword.
const TYPE_DECL = /(?:^|\n)[ \t]*(?:\[[^\]]*\][ \t]*\r?\n?[ \t]*)*(?:(?:public|internal|private|protected|sealed|abstract|static|partial|file|new|unsafe|readonly|ref)[ \t]+)*(class|interface|enum|record|struct)[ \t]+(?:(?:class|struct)[ \t]+)?([A-Za-z_]\w*)/g;

const declaredNs = new Set();
const usingNs = new Set();
const declaredTypes = new Map();

for (const f of csFiles) {
  const t = readFileSync(f, "utf8");
  let m;
  NS_DECL.lastIndex = 0; while ((m = NS_DECL.exec(t))) declaredNs.add(m[1]);
  USING.lastIndex = 0; while ((m = USING.exec(t))) usingNs.add(m[1]);
  TYPE_DECL.lastIndex = 0; while ((m = TYPE_DECL.exec(t))) declaredTypes.set(m[2], (declaredTypes.get(m[2]) || 0) + 1);
}

// Ownership is DECLARED BY THE OPERATOR, not inferred, and the tool refuses a prefix the repo
// does not actually declare. Inference was tried and is not safe here: this repo declares
// `Microsoft.Extensions.Hosting` itself (the extension-method namespace hijack), so "the repo
// declares it" does not mean "the repo owns it" - and renaming a segment of `Microsoft.Extensions`
// would break every consumer of the real package. `--survey` prints the declared set so the
// operator can choose; the choice lands in the manifest, so it is auditable rather than clever.
const OWNED = argOf("ns", "").split(",").map((s) => s.trim()).filter(Boolean);
if (!SURVEY) {
  if (!OWNED.length) die("--ns <dotted,prefixes> is required: run --survey first and pass the prefixes the repo owns");
  for (const p of OWNED) {
    if (![...declaredNs].some((d) => d === p || d.startsWith(p + "."))) die(`--ns '${p}' is not a declared namespace prefix in this repo`);
  }
}
const ownsNs = (ns) => OWNED.some((p) => ns === p || ns.startsWith(p + "."));

// A `using` is EXTERNAL unless it sits under an owned prefix. Every segment of an external using
// is off-limits: `Ardalis` in `using Ardalis.Specification;` names a package, and renaming it
// turns a restore into a failure.
const isOwn = (ns) => ownsNs(ns);
const externalSegments = new Set();
for (const u of usingNs) if (!isOwn(u)) for (const seg of u.split(".")) externalSegments.add(seg);
for (const ns of declaredNs) if (!isOwn(ns)) for (const seg of ns.split(".")) externalSegments.add(seg);
for (const seg of ["System", "Microsoft", "Program", "Main", "Startup", "Properties", "Test", "Tests"]) externalSegments.add(seg);

// Architectural vocabulary is PRESERVED, for the same reason the type suffixes are: it is what
// both arms reason with, and a memorised answer is not recoverable from "this repo has a Core
// project". Only the DOMAIN nouns move.
const ARCH_SEGMENTS = new Set([
  "Core", "Web", "Api", "Apis", "Infrastructure", "UseCases", "Domain", "Application", "Modulith",
  "Endpoints", "ApiEndpoints", "Events", "Handlers", "Specifications", "Configurations", "Config",
  "Data", "Migrations", "Queries", "Commands", "Services", "Interfaces", "Extensions", "Models",
  "Dtos", "Features", "Shared", "Common", "Helpers", "Middleware", "Filters", "Validators",
  "Tests", "UnitTests", "IntegrationTests", "FunctionalTests", "AspireTests", "ClassFixtures",
  "Create", "Delete", "Update", "Get", "GetById", "List", "Email", "Localization", "Aggregates",
]);

// Namespace segments under an owned prefix, minus the owned prefix itself (renamed below as a
// dotted unit), minus architecture vocabulary, minus anything an external namespace also claims.
const prefixSegments = new Set(OWNED.flatMap((p) => p.split(".")));
const nsSegments = new Set();
for (const ns of declaredNs) {
  if (!isOwn(ns)) continue;
  for (const seg of ns.split(".")) {
    if (prefixSegments.has(seg) || ARCH_SEGMENTS.has(seg) || externalSegments.has(seg)) continue;
    nsSegments.add(seg);
  }
}

// A declared type is renamable unless an external namespace uses the same simple name (a
// whole-word replace cannot tell the two apart) or it is a name the toolchain is entitled to.
const RESERVED = new Set(["Program", "Startup", "GlobalUsings", "AssemblyInfo", "Error", "Index", "Home", "Shared", "Layout", "App", "Main"]);
const renamableTypes = new Set();
for (const [name, count] of declaredTypes) {
  if (name.length < 3) continue;
  if (RESERVED.has(name)) continue;
  if (externalSegments.has(name)) continue;
  if (ARCH_SEGMENTS.has(name)) continue; // `Create`, `List` - a whole-word replace would hit far more than the type
  if (prefixSegments.has(name)) continue;
  if (nsSegments.has(name)) continue;   // handled once, as a namespace segment
  renamableTypes.add(name);
  void count;
}

if (SURVEY) {
  console.log(`src                 ${SRC}`);
  console.log(`files copied        ${files.length} (${csFiles.length} .cs)`);
  console.log(`declared namespaces ${declaredNs.size}`);
  console.log(`  roots             ${[...new Set([...declaredNs].map((n) => n.split(".")[0]))].sort().join(", ")}`);
  console.log(`renamable ns segs   ${nsSegments.size}: ${[...nsSegments].sort().slice(0, 20).join(", ")}${nsSegments.size > 20 ? " ..." : ""}`);
  console.log(`declared types      ${declaredTypes.size}  renamable ${renamableTypes.size}`);
  console.log(`  held back         ${[...declaredTypes.keys()].filter((n) => !renamableTypes.has(n)).sort().join(", ")}`);
  console.log(`external using roots ${[...new Set([...usingNs].filter((u) => !isOwn(u)).map((u) => u.split(".")[0]))].sort().join(", ")}`);
  if (hasFlag("verbose")) {
    console.log(`\ndeclared namespaces:\n  ${[...declaredNs].sort().join("\n  ")}`);
    console.log(`\nexternal usings:\n  ${[...usingNs].filter((u) => !isOwn(u)).sort().join("\n  ")}`);
  }
  process.exit(0);
}

// ---- build the map -------------------------------------------------------------------------
// Sorted so the map - and therefore the output tree - does not depend on filesystem order.
const map = new Map();
// The owned prefix is renamed as a DOTTED UNIT, not segment by segment. `Clean.Architecture` is
// the repo's single loudest identity tell, and its first segment ("Clean") is too generic to
// replace on its own without collateral. Matching the dotted form also makes the longer prefix
// win: `\bClean\.Architecture\b` cannot fire inside `MinimalClean.Architecture`, because the
// preceding `l` denies the word boundary.
for (const p of [...OWNED].sort()) map.set(p, p.split(".").map((s) => pseudonym(s)).join("."));
for (const seg of [...nsSegments].sort()) map.set(seg, pseudonym(seg));
for (const t of [...renamableTypes].sort()) map.set(t, pseudonym(t));

// The prefix also appears SPACE-joined, in prose the compiler never sees: `s.Title = "Clean
// Architecture API"`. A dotted-form rule does not touch it, and a title string naming the
// upstream project hands back the identity the whole exercise removes.
for (const p of OWNED) map.set(p.split(".").join(" "), map.get(p).split(".").join(" "));
// ...and CONCATENATED, which is how a repo names its own `CleanArchitecture.nuspec`.
for (const p of OWNED) map.set(p.split(".").join(""), map.get(p).split(".").join(""));

// A prefix's first segment on its own: `MinimalClean.nuspec` inside a solution file, `NimblePros`
// in a folder name. Renamable in content too, EXCEPT where the head is an ordinary English word
// that would fire in prose - `Clean` in "Reset database to clean seed state". Those heads are
// listed by the operator and stay path-only.
const PATH_ONLY_HEADS = new Set(argOf("path-only-head", "").split(",").map((s) => s.trim()).filter(Boolean));
for (const p of OWNED) {
  const [srcHead] = p.split("."), [dstHead] = map.get(p).split(".");
  if (!PATH_ONLY_HEADS.has(srcHead) && !map.has(srcHead)) map.set(srcHead, dstHead);
}

// Literal substrings with no identifier structure at all - a comment quoting an upstream issue
// URL. Pinned on the command line and recorded in the manifest, because a silent scrub is a scrub
// nobody can audit.
const REDACT = argOf("redact", "").split(",").map((s) => s.trim()).filter(Boolean)
  .map((pair) => { const i = pair.indexOf("="); if (i < 0) die(`--redact expects from=to, got '${pair}'`); return [pair.slice(0, i), pair.slice(i + 1)]; });

// A repo type may share its simple name with an MSBuild/solution ELEMENT. This one declares a
// `Project` aggregate, and a blind whole-word pass rewrote every `<Project Path=...>` in the
// `.slnx` into `<Jasteakplai Path=...>` - a solution with no projects in it, which msbuild
// reported as "Build succeeded" in 0.33 seconds. The rename is therefore applied with a reduced
// key set inside build files and inside paths (which build files must agree with), and with the
// full set inside C#, where these names are types and nothing else.
const MSBUILD_RESERVED = new Set([
  "Project", "Solution", "Folder", "File", "Configurations", "Platform", "BuildDependency",
  "Properties", "Import", "Target", "Task", "Package", "Reference", "Content", "Compile",
  "None", "Choose", "When", "Otherwise", "ItemGroup", "PropertyGroup", "Output", "Using",
]);
const mkRe = (ks) => new RegExp(`\\b(${ks.map((k) => k.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")).join("|")})\\b`, "g");
const keys = [...map.keys()].sort((a, b) => b.length - a.length || (a < b ? -1 : 1));
const RE = mkRe(keys);
const RE_BUILD = mkRe(keys.filter((k) => !MSBUILD_RESERVED.has(k)));
const BUILD_EXT = new Set([".csproj", ".slnx", ".sln", ".props", ".targets", ".xml", ".config", ".resx", ".runsettings", ".sqlproj", ".nuspec"]);

// Aspire's source generator emits `Projects.Clean_Architecture_Web` - the project name with dots
// turned into underscores, and a trailing `_Web` that denies a word boundary after the prefix.
// It is matched on the LEADING boundary only, which is why it needs its own pass rather than a
// key in the map.
const UNDERSCORE = OWNED.map((p) => [p.split(".").join("_"), map.get(p).split(".").join("_")]);
const US_RE = UNDERSCORE.length
  ? new RegExp(`\\b(${UNDERSCORE.map(([from]) => from.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")).join("|")})`, "g")
  : null;

const applyWith = (re, s) => {
  let out = s.replace(re, (m) => map.get(m) || m);
  if (US_RE) out = out.replace(US_RE, (m) => (UNDERSCORE.find(([f]) => f === m) || [null, m])[1]);
  for (const [from, to] of REDACT) out = out.split(from).join(to);
  return out;
};
const applyText = (s) => applyWith(RE, s);
const applyBuild = (s) => applyWith(RE_BUILD, s);

// Path segments get the same substitution, so `Core/ContributorAggregate/Contributor.cs` follows
// its type. Only the final extension is held back - the stem keeps its dots, because a project
// file is literally named `Clean.Architecture.Core.csproj` and the dotted prefix has to match.
// Directory names carry the prefix's FIRST segment on its own - the source tree has a folder
// literally called `MinimalClean`. That segment is too generic to substitute in code (it would
// fire inside prose and inside package ids), so it is substituted in PATHS only, where the whole
// segment is the name and there is nothing to collide with.
const pathOnly = new Map();
for (const p of OWNED) {
  const [srcHead] = p.split(".");
  const [dstHead] = map.get(p).split(".");
  if (!pathOnly.has(srcHead)) pathOnly.set(srcHead, dstHead);
}

function applyPathSegment(seg) {
  const ext = extname(seg);
  const stem = seg.slice(0, seg.length - ext.length);
  if (pathOnly.has(stem)) return pathOnly.get(stem) + ext;
  return applyBuild(stem) + ext;
}

// ---- emit ----------------------------------------------------------------------------------
if (existsSync(OUT)) rmSync(OUT, { recursive: true, force: true });
mkdirSync(OUT, { recursive: true });

let renamedPaths = 0, rewrittenFiles = 0, copiedFiles = 0, substitutions = 0;
const emitted = [];
for (const f of files) {
  const rel = relative(SRC, f);
  const outRel = rel.split(sep).map(applyPathSegment).join(sep);
  if (outRel !== rel) renamedPaths++;
  const outPath = join(OUT, outRel);
  mkdirSync(dirname(outPath), { recursive: true });
  if (isText(f)) {
    const before = readFileSync(f, "utf8");
    const build = BUILD_EXT.has(extname(f).toLowerCase());
    substitutions += (before.match(build ? RE_BUILD : RE) || []).length;
    const after = build ? applyBuild(before) : applyText(before);
    // Written with LF-agnostic passthrough: whatever the source had is preserved byte-for-byte
    // apart from the substituted identifiers.
    writeFileSync(outPath, after, "utf8");
    rewrittenFiles++;
    emitted.push([outRel, createHash("sha256").update(after).digest("hex")]);
  } else {
    copyFileSync(f, outPath);
    copiedFiles++;
    emitted.push([outRel, createHash("sha256").update(readFileSync(f)).digest("hex")]);
  }
}

// One hash over the whole emitted tree: this is what makes "same seed, same tree" checkable
// rather than asserted.
emitted.sort((a, b) => (a[0] < b[0] ? -1 : 1));
const treeHash = createHash("sha256").update(emitted.map(([p, h]) => `${p.split(sep).join("/")} ${h}`).join("\n")).digest("hex");

// ---- leak scan -----------------------------------------------------------------------------
// The rename is only worth what it removes. Scan the EMITTED tree for every original identifier
// and every segment of every owned prefix, and put the residue in the manifest. A non-zero count
// is not automatically a bug - `Ardalis` and `NimblePros` are package authors and have to stay,
// or restore fails - but it must be visible rather than assumed away.
const leakTokens = [...new Set([...map.keys(), ...OWNED.flatMap((p) => p.split("."))])]
  .filter((t) => !t.includes("."));
const LEAK_RE = mkRe(leakTokens);
// Inside a build file `Project` is an XML element, not the repo's aggregate. Counting those 90
// element names as surviving identity would make the manifest lie in the reassuring direction.
const LEAK_RE_BUILD = mkRe(leakTokens.filter((t) => !MSBUILD_RESERVED.has(t)));
const residual = new Map();
for (const [relPath] of emitted) {
  const abs = join(OUT, relPath);
  if (!isText(abs)) continue;
  const t = readFileSync(abs, "utf8");
  const re = BUILD_EXT.has(extname(abs).toLowerCase()) ? LEAK_RE_BUILD : LEAK_RE;
  let m; re.lastIndex = 0;
  while ((m = re.exec(t))) residual.set(m[1], (residual.get(m[1]) || 0) + 1);
  for (const seg of relPath.split(sep)) {
    const s2 = seg.replace(extname(seg), "");
    if (leakTokens.includes(s2)) residual.set(s2, (residual.get(s2) || 0) + 1);
  }
}

const manifest = {
  tool: "eval/agent-probe/rename-repo.mjs",
  seed: SEED,
  source: { path: SRC.split(sep).join("/") },
  ownedNamespacePrefixes: OWNED,
  pathOnlyHeads: [...PATH_ONLY_HEADS],
  redactions: REDACT.map(([from, to]) => `${from} -> ${to}`),
  excluded: { dirs: [...SKIP_DIRS].sort(), filePattern: SKIP_FILE_RE.source },
  counts: {
    filesEmitted: emitted.length,
    filesRewritten: rewrittenFiles,
    filesCopiedVerbatim: copiedFiles,
    pathsRenamed: renamedPaths,
    identifierSubstitutions: substitutions,
    namespaceSegmentsRenamed: nsSegments.size,
    typesRenamed: renamableTypes.size,
    typesDeclared: declaredTypes.size,
  },
  heldBack: [...declaredTypes.keys()].filter((n) => !renamableTypes.has(n)).sort(),
  residualIdentityHits: Object.fromEntries([...residual].sort((a, b) => b[1] - a[1])),
  treeSha256: treeHash,
};

console.log(JSON.stringify(manifest, null, 2));
console.log(`\nwrote ${emitted.length} files to ${OUT}`);
writeFileSync(join(HERE, "unseen-repo.manifest.json"), JSON.stringify(manifest, null, 2) + "\n", "utf8");
writeFileSync(join(HERE, "unseen-repo.rename-map.json"),
  JSON.stringify(Object.fromEntries([...map].sort((a, b) => (a[0] < b[0] ? -1 : 1))), null, 2) + "\n", "utf8");
console.log(`manifest -> eval/agent-probe/unseen-repo.manifest.json`);
console.log(`map      -> eval/agent-probe/unseen-repo.rename-map.json`);
void statSync;
