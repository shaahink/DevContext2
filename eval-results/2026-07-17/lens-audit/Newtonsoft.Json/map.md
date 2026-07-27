MAP  Newtonsoft.Json     (2 projects)

STACK  net46;net40;net35;net20;net5.0;net6.0;net8.0;netcoreapp3.1;netcoreapp2.1, net46;net6.0, net6.0, net8.0;net6.0;net45;net40;net35;net20;netstandard1.0;netstandard1.3;netstandard2.0

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         Newtonsoft.Json.TestConsole: CLI [CLI]

TOPOLOGY (depends-on)
   Newtonsoft.Json
   Newtonsoft.Json.TestConsole ── Newtonsoft.Json.Tests

PACKAGES
   Testing:  Moq, NUnit, NUnit3TestAdapter, xunit, xunit.runner.visualstudio
   Other:  Autofac, BenchmarkDotNet, FSharp.Core, Microsoft.CodeAnalysis.NetAnalyzers, Microsoft.CSharp, Microsoft.NET.Test.Sdk, Microsoft.NETCore.App 2.1.30, Microsoft.SourceLink.GitHub … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "<TypeName>")
