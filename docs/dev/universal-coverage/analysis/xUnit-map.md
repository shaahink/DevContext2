MAP  xunit     (17 projects)

STACK  net10.0, net45, net452, net472, net472;net48;net481, net472;net8.0, net8.0, net9.0, netstandard2.0, netstandard2.0;net472;net8.0, netstandard2.0;net8.0, netstandard2.1;net472;net48;net481;net9.0;net10.0

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

TOPOLOGY (depends-on)
   xunit.v3.common
   xunit.v3.runner.inproc.console ── xunit.v3.core, xunit.v3.runner.common
   xunit.v3.core ── xunit.v3.common
   xunit.v3.runner.common ── xunit.v3.common
   xunit.v3.assert
   xunit.v3.assert.all-off
   xunit.v3.assert.all-on
   xunit.v3.assert.aot
   xunit.v3.assert.aot.nullable-mixed
   xunit.v3.assert.nullable-mixed
   xunit.v3.msbuildtasks
   xunit.v3.mtp-v1 ── xunit.v3.runner.inproc.console
   xunit.v3.mtp-v2 ── xunit.v3.runner.inproc.console
   xunit.v3.runner.console ── xunit.v3.runner.utility
   xunit.v3.runner.console.x86 ── xunit.v3.runner.utility
   xunit.v3.runner.msbuild ── xunit.v3.runner.utility
   xunit.v3.templates

PACKAGES
   Testing:  Moq, NSubstitute 4.4.0, xunit.abstractions, xunit.analyzers XUNIT_ANALYZERS_VERSION, xunit.assert, xunit.core, xunit.extensions, xunit.runner.utility … (13 total)
   Utilities:  Newtonsoft.Json 13.0.4
   Other:  Bullseye 3.3.0, FSharp.Compiler.Service, McMaster.Extensions.CommandLineUtils 4.1.1, Microsoft.Bcl.AsyncInterfaces, Microsoft.Build.Tasks.Core, Microsoft.NET.Test.Sdk MICROSOFT_NET_TEST_SDK_VERSION, Microsoft.Security.Extensions 1.4.0, Microsoft.Testing.Extensions.Telemetry … (16 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
