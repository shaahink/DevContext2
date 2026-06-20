# DevContext performance — 2026-06-20 21:04

Real pipeline (incl. call graph) over the standing eval repos. Median of 3 timed iterations after 1 warm-up. Wall time is the full `AnalyzeAsync`.

| Repo | Mode | Median | Min–Max | Files | Nodes | Edges | Entries | Stage2 | Stage3 | Top extractors (ms) |
|---|---|--:|--:|--:|--:|--:|--:|--:|--:|---|
| DntSite | Map | 7.8s | 6.8–8.8s | 1336 | 1476 | 150 | 94 | 5251ms | 1462ms | SyntaxStructureExtractor 5251ms, DiRegistrationExtractor 5251ms, EndpointExtractor 1462ms |
| DntSite | Trace (FeedController) | 53.9s | 52.2–91.5s | 1336 | 1476 | 178 | 94 | 6798ms | 84508ms | CallGraphExtractor 84508ms, IndirectWiringDetector 13430ms, SyntaxStructureExtractor 6797ms |
| TodoApi | Map | 0.1s | 0.1–0.2s | 40 | 96 | 35 | 11 | 70ms | 17ms | SyntaxStructureExtractor 69ms, DiRegistrationExtractor 68ms, FileTreeExtractor 23ms |
| TodoApi | Trace (POST /todos/) | 0.4s | 0.4–0.4s | 40 | 96 | 38 | 11 | 57ms | 310ms | CallGraphExtractor 309ms, IndirectWiringDetector 63ms, SyntaxStructureExtractor 56ms |
| VerticalSlice | Map | 0.1s | 0.1–0.1s | 76 | 134 | 25 | 5 | 45ms | 49ms | EndpointExtractor 49ms, SyntaxStructureExtractor 45ms, DiRegistrationExtractor 44ms |
| VerticalSlice | Trace (POST /Products) | 0.2s | 0.2–0.2s | 76 | 140 | 58 | 5 | 39ms | 177ms | CallGraphExtractor 177ms, IndirectWiringDetector 43ms, SyntaxStructureExtractor 39ms |
| eShop.Ordering.API | Map | 0.2s | 0.2–0.4s | 140 | 227 | 60 | 8 | 91ms | 24ms | SyntaxStructureExtractor 91ms, DiRegistrationExtractor 90ms, SolutionDiscovery 68ms |
| eShop.Ordering.API | Trace (POST /api/orders/) | 0.7s | 0.7–1.0s | 140 | 236 | 159 | 8 | 66ms | 547ms | CallGraphExtractor 547ms, IndirectWiringDetector 74ms, SyntaxStructureExtractor 66ms |
| AutoMapper | Map | 2.7s | 2.6–3.1s | 500 | 247 | 7 | 0 | 2461ms | 68ms | SyntaxStructureExtractor 2461ms, DiRegistrationExtractor 763ms, InMemoryEventBusExtractor 68ms |
| OrchardCore | Map | 5.2s | 5.2–5.4s | 5146 | 7143 | 1708 | 281 | 3543ms | 776ms | SyntaxStructureExtractor 3543ms, DiRegistrationExtractor 3543ms, EndpointExtractor 775ms |

## Call-graph phase breakdown (Trace runs)

- **DntSite** Trace (FeedController): parse 1ms · compile 0ms · bind 84306ms · bfs 189ms (1336 files)
- **TodoApi** Trace (POST /todos/): parse 0ms · compile 0ms · bind 307ms · bfs 1ms (40 files)
- **VerticalSlice** Trace (POST /Products): parse 0ms · compile 0ms · bind 174ms · bfs 2ms (76 files)
- **eShop.Ordering.API** Trace (POST /api/orders/): parse 0ms · compile 0ms · bind 543ms · bfs 3ms (140 files)
