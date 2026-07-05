# REPORT
**System.CommandLine**

Style: Unknown
_2 projects  ·  1 CliCommand  ·  net472 + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 178 |
| Projects | 10 |
| Nodes | 97 |
| Edges | 2 |
| Entries | 1 |
| With target | 1/1 |
| Verified edges | 50% |
| Analyzed in | 3.1s |

## Top Flows

1. **RootCommand —settings object** → `RootCommand` *(CliCommand)*

### Trace 1: RootCommand —settings object

TRACE  RootCommand —settings object
       src/System.CommandLine/RootCommand.cs:21

▸ ENTRY  RootCommand —settings object  (src/System.CommandLine/RootCommand.cs:21)
   └─ call RootCommand  (src/System.CommandLine/RootCommand.cs:21)
          /// <summary>
          /// Represents the main action that the application performs.
          /// </summary>

---

## Insights

_4 info · 2 notable_

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- ParseError
- PathExtensions
- SymbolNode
- TypeExtensions
- Perf_Suggestions

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

MAP  System.CommandLine     (2 projects)

STACK  net472

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

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
| DiscoveryAndCacheWarmup | 344ms |
| GenericExtraction | 1671ms |
| SignalSealing | 0ms |
| SpecificExtraction | 343ms |
| Compression | 127ms |
| **Total** | **3080ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 1665ms | 197 | 0 |
| DiRegistrationExtractor | 1662ms | 0 | 0 |
| ProgramCsFlowExtractor | 473ms | 0 | 0 |
| ProjectStructure | 244ms | 0 | 0 |
| SourceBodyExtractor | 174ms | 0 | 0 |
| CliCommandExtractor | 164ms | 0 | 4 |
| CallGraphExtractor | 138ms | 0 | 0 |
| InMemoryEventBusExtractor | 130ms | 0 | 4 |
| SolutionDiscovery | 49ms | 0 | 0 |
| IndirectWiringDetector | 46ms | 0 | 4 |
| FileTreeExtractor | 45ms | 0 | 0 |
| DependencyExtractor | 30ms | 0 | 0 |
| LayerClassifier | 29ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 1 | 0 |
| Resolves | 1 | 1 |

_178 files · 0 projects_
