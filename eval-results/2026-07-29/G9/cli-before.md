CLI TOOL  System.CommandLine     (2 projects)
       not analyzed — 3 runnable apps outside this solution:
         EndToEndTestApp: Unknown
         NativeAOT: Unknown
         Trimming: Unknown

STACK  net472, netstandard2.0

STYLE  NotApplicable  (confidence low)
       evidence: not applicable to a clitool

       per service:
         dotnet-suggest: Unknown

COMMAND SURFACE
   dotnet-suggest [Infrastructure] (1)
      dotnet-suggest (Main)
   System.CommandLine [Shared] (1)
      RootCommand —settings object

TOPOLOGY (depends-on)
   System.CommandLine
   dotnet-suggest ── System.CommandLine

ENTRY POINTS
   CLI (2)
      dotnet-suggest (Main)  (src/System.CommandLine.Suggest/Program.cs:14)
      RootCommand —settings object  (src/System.CommandLine/RootCommand.cs:21)

PACKAGES
   Utilities:  Newtonsoft.Json 13.0.3
   Other:  ApprovalTests 7.0.0, AwesomeAssertions 8.1.0, BenchmarkDotNet 0.13.1, Drop.App, Microsoft.Bcl.Memory 9.0.6, Microsoft.CodeAnalysis.CSharp.Scripting 4.0.1, Microsoft.DotNet.IBCMerge, Microsoft.DotNet.PlatformAbstractions 3.1.6 … (13 total)

→ drill in:  trace a focused entry   (e.g. trace "RootCommand —settings object")
