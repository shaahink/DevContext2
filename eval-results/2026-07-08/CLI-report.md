# REPORT
**System.CommandLine**

Style: Unknown
_2 projects  ·  1 CliCommand  ·  net472 + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 178 |
| Projects | 10 |
| Nodes | 892 |
| Edges | 1172 |
| Entries | 1 |
| With target | 1/1 |
| Verified edges | 87% |
| Analyzed in | 9.9s |

## Top Flows

1. **RootCommand —settings object** → `RootCommand` *(CliCommand)*

### Trace 1: RootCommand —settings object

TRACE  RootCommand —settings object
       src/System.CommandLine/RootCommand.cs:21
       System.CommandLine
▸ ENTRY  RootCommand —settings object  (src/System.CommandLine/RootCommand.cs:21)
   └─ call RootCommand  (src/System.CommandLine/RootCommand.cs:21)
          /// <summary>
          /// Represents the main action that the application performs.
          /// </summary>

---

## Insights

_5 info · 1 notable_

### **NOTABLE**: Most depended-upon: System.CommandLine (4 dependents) · dotnet-suggest (1 dependents) · System.CommandLine.Tests (1 dependents)
*(Topology)*

- System.CommandLine (4 dependents)
- dotnet-suggest (1 dependents)
- System.CommandLine.Tests (1 dependents)

### _INFO_: Command tree: 1 CLI commands, 1 top-level groups
*(Shape)*

- RootCommand (1 commands)

### _INFO_: Parameter inventory: ~1.0 params per command (avg)
*(Data)*

- 1 commands

### _INFO_: Entry targets resolved 1/1 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Public surface: 2 interfaces, 139 classes (142 total public types)
*(Shape)*

- 2 interfaces
- 139 classes

### _INFO_: Wiring hubs: RootCommand (270) · Command (261) · ParseResult (132) · HelpBuilder (89) · IReadOnlyList (64)
*(Wiring)*

- RootCommand (270)
- Command (261)
- ParseResult (132)
- HelpBuilder (89)
- IReadOnlyList (64)

MAP  System.CommandLine     (2 projects)

STACK  net472

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         System.CommandLine.Benchmarks: Unknown
         dotnet-suggest: Unknown

TOPOLOGY (depends-on)
   System.CommandLine
   dotnet-suggest ── System.CommandLine

ENTRY POINTS
   CLI (1)
      RootCommand —settings object  → RootCommand  (src/System.CommandLine/RootCommand.cs:21)

PACKAGES
   Utilities:  Newtonsoft.Json 13.0.3
   Other:  ApprovalTests 7.0.0, AwesomeAssertions 8.1.0, BenchmarkDotNet 0.13.1, Drop.App, Microsoft.Bcl.Memory 9.0.6, Microsoft.CodeAnalysis.CSharp.Scripting 4.0.1, Microsoft.DotNet.IBCMerge, Microsoft.DotNet.PlatformAbstractions 3.1.6 … (13 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 413ms |
| GenericExtraction | 1890ms |
| SignalSealing | 0ms |
| SpecificExtraction | 584ms |
| Compression | 62ms |
| **Total** | **9873ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 1886ms | 197 | 0 |
| DiRegistrationExtractor | 1869ms | 197 | 0 |
| ProgramCsFlowExtractor | 512ms | 0 | 0 |
| SourceBodyExtractor | 364ms | 0 | 0 |
| BodyFactsExtractor | 330ms | 0 | 0 |
| CallGraphExtractor | 276ms | 0 | 0 |
| ProjectStructure | 239ms | 0 | 0 |
| CliCommandExtractor | 198ms | 0 | 4 |
| InMemoryEventBusExtractor | 184ms | 0 | 4 |
| FileTreeExtractor | 93ms | 0 | 0 |
| SolutionDiscovery | 73ms | 0 | 0 |
| IndirectWiringDetector | 62ms | 0 | 4 |
| DependencyExtractor | 46ms | 0 | 0 |
| LayerClassifier | 25ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 1171 | 152 |
| Resolves | 1 | 1 |

_178 files · 10 projects_
