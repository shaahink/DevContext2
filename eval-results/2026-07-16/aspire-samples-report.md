# REPORT
**Metrics**

Style: SampleCollection
_4 projects  ·  net10.0, net10.0-windows + orleans + controllers + desktop-ui + minimal-apis + razor-pages + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 151 |
| Projects | 52 |
| Nodes | 58 |
| Edges | 31 |
| Entries | 0 |
| With target | 0/0 |
| Verified edges | 52% |
| Analyzed in | 5.1s |

## Top Flows

_No entries found._

## Insights

_3 info · 3 notable_

### **NOTABLE**: Config without defaults: 2 consumed keys have no appsettings default
*(Risk)*

- OTEL_EXPORTER_OTLP_ENDPOINT
- WORKER_RUN_CONTINUOUSLY

### **NOTABLE**: Internal hubs: 2 heavily-referenced internal types
*(Topology)*

- CatalogDbInitializer (5 refs)
- UserAgentGrain (3 refs)

### **NOTABLE**: External event contracts: 2 consumed but never produced internally
*(Wiring)*

- BeforeStartEvent
- InitializeResourceEvent

### _INFO_: Public surface: 5 interfaces, 89 classes (110 total public types)
*(Shape)*

- 5 interfaces
- 89 classes

### _INFO_: DI: 156 Extension · 9 Singleton · 3 Scoped · 1 Transient (169 total)
*(Wiring)*

### _INFO_: Most depended-upon: *.AppHost (1 dependents)
*(Topology)*

- *.AppHost (1 dependents)

LIBRARY  Metrics     (0 public types)

→ drill in:  --focus "<TypeName>"   (e.g. --focus TypeName)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 235ms |
| GenericExtraction | 295ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1550ms |
| Compression | 30ms |
| **Total** | **5081ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1219ms | 0 | 0 |
| EndpointExtractor | 304ms | 0 | 188 |
| SyntaxStructureExtractor | 292ms | 130 | 229 |
| DiRegistrationExtractor | 288ms | 0 | 229 |
| ProgramCsFlowExtractor | 288ms | 0 | 60 |
| BlazorEntryExtractor | 126ms | 0 | 152 |
| RazorPagesExtractor | 117ms | 0 | 139 |
| FileTreeExtractor | 114ms | 0 | 0 |
| SourceBodyExtractor | 96ms | 0 | 0 |
| AspireExtractor | 79ms | 0 | 100 |
| ControllerActionExtractor | 75ms | 0 | 100 |
| InMemoryEventBusExtractor | 74ms | 0 | 100 |
| DesktopEntryExtractor | 69ms | 0 | 42 |
| OrleansGrainExtractor | 67ms | 0 | 100 |
| ProjectStructure | 66ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 25 | 15 |
| Resolves | 6 | 0 |

_151 files · 52 projects_
