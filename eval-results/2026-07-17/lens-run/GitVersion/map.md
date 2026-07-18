CLI TOOL  GitVersion     (11 projects)

STACK  net10.0, net9.0 +2 more TFMs

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         GitVersion.Cli: CLI [CLI]
         GitVersion.Core.Tester: Unknown
         GitVersion.App: Unknown
         GitVersion.Schema: Unknown

COMMAND SURFACE
   GitVersion.App [Infrastructure] (1)
      GitVersion.App (Main)

TOPOLOGY (depends-on)
   GitVersion.BuildAgents ── GitVersion.Core
   GitVersion.LibGit2Sharp ── GitVersion.Core
   GitVersion.App ── GitVersion.BuildAgents, GitVersion.Configuration, GitVersion.Core, GitVersion.LibGit2Sharp, GitVersion.Output
   GitVersion.Configuration (new-cli)
   GitVersion.Configuration (src) ── GitVersion.Core
   GitVersion.Core (new-cli)
   GitVersion.Core (src)
   GitVersion.MsBuild ── GitVersion.BuildAgents, GitVersion.Configuration, GitVersion.Core, GitVersion.Output
   GitVersion.Output (new-cli)
   GitVersion.Output (src) ── GitVersion.Core
   GitVersion.Schema ── GitVersion.Configuration, GitVersion.Core, GitVersion.Output

ENTRY POINTS
   CLI (1)
      GitVersion.App (Main)  (src/GitVersion.App/Program.cs:3)

PACKAGES
   Logging:  Serilog.Extensions.Logging 10.0.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0, Serilog.Sinks.Map 2.0.0
   Testing:  Cake.Coverlet 6.0.1, NUnit 4.6.1, NUnit.Analyzers 4.14.0, NUnit3TestAdapter 6.2.0, Shouldly 4.3.0, xunit.assert 2.9.3
   Utilities:  Polly 8.7.0
   Other:  Buildalyzer 8.0.0, Cake.Codecov 6.0.0, Cake.Frosting.Git 5.0.1, Cake.Http 5.1.0, Cake.Json 7.0.1, Cake.Npx 1.7.0, Cake.Wyam 2.2.14, JsonSchema.Net.Generation 7.3.10 … (38 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "GitVersion.App (Main)")
