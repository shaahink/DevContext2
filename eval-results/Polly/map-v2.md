Overview map (no focus).
Analyzing project...

MAP  Polly     (7 projects)

STACK  net10.0, net10.0;net9.0;net8.0, net6.0;netstandard2.0;net472;net462, 
net8.0;net6.0;netstandard2.0;net472;net462, net8.0;netstandard2.0 · Minimal APIs

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 13 project(s); no MediatR

TOPOLOGY (depends-on)
   Polly ── Polly.Core
   Polly.Benchmarks ── Polly
   Polly.Core
   Polly.Core.Benchmarks ── Polly, Polly.Core, Polly.Extensions, 
Polly.RateLimiting
   Polly.Extensions ── Polly.Core
   Polly.RateLimiting ── Polly.Core
   Polly.Testing ── Polly.Core

PACKAGES
   Testing:  FsCheck.Xunit, NSubstitute, xunit
   Utilities:  Polly.Contrib.WaitAndRetry, Polly.Core, Polly.Extensions, 
Refit.HttpClientFactory, RestSharp
   Other:  Flurl.Http.Signed, Microsoft.Bcl.AsyncInterfaces, 
Microsoft.Bcl.TimeProvider, Microsoft.Extensions.Caching.Memory, 
Microsoft.Extensions.Configuration, Microsoft.Extensions.DependencyInjection, 
Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Logging … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 794 files · 389 nodes · 38 edges · 0 entries · ~280 tokens · 13.1s 
stage2 ×2.1 stage3 ×1.8
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │      Polly.slnx      │
│   Time   │       13224ms        │
│  Tokens  │  ~280 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.134 │
╰──────────┴──────────────────────╯
