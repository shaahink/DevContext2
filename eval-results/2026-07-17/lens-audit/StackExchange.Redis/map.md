MAP  StackExchange.Redis     (14 projects)

STACK  net10.0, net461;netstandard2.0;net472;net6.0;net8.0;net10.0, net472;net8.0, net481;net8.0;net10.0, net6.0, net8.0, net8.0;net10.0, net8.0;net461;net472, net8.0;net472, net9.0, netstandard2.0, netstandard2.0;net10.0 · Minimal APIs

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 15 project(s); no MediatR

       per service:
         RESPite.Benchmark: Unknown
         StackExchange.Redis.Benchmarks: Unknown
         KestrelRedisServer: Unknown
         OpBench: Unknown
         TestConsole: CLI [CLI]
         TestConsoleBaseline: Unknown

TOPOLOGY (depends-on)
   StackExchange.Redis ── RESPite
   RESPite
   StackExchange.Redis.Server ── RESPite, StackExchange.Redis
   .github
   docker
   docs
   KestrelRedisServer ── StackExchange.Redis.Server
   OpBench ── StackExchange.Redis
   RedisConfigs
   RESPite.Benchmark ── StackExchange.Redis
   StackExchange.Redis.Benchmarks ── RESPite
   StackExchange.Redis.Build
   TestConsole ── StackExchange.Redis
   TestConsoleBaseline

PACKAGES
   Testing:  NSubstitute 5.3.0, xunit.runner.visualstudio 3.1.5, xunit.v3 3.2.2, xunit.v3.runner.console 3.2.2
   Utilities:  Newtonsoft.Json 13.0.4
   Other:  BenchmarkDotNet 0.15.8, GitHubActionsTestLogger 3.0.2, Microsoft.Bcl.AsyncInterfaces 10.0.5, Microsoft.CodeAnalysis.CSharp 5.3.0, Microsoft.Extensions.Logging.Abstractions 10.0.5, Microsoft.NET.Test.Sdk 18.3.0, Microsoft.Testing.Platform 2.1.0, StackExchange.Redis 2.13.17 … (20 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "<TypeName>")
