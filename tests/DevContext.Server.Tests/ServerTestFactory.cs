using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.Testing;

namespace DevContext.Server.Tests;

/// <summary>
/// Shared test host for the gRPC server tests (Tapestry T0.1a).
///
/// The host runs IN-PROCESS via <see cref="WebApplicationFactory{TEntryPoint}"/> (a Kestrel-less
/// TestServer), so a passing run spawns no external <c>DevContext.Server</c> process — verified
/// 2026-07-15: the full 14-test suite leaves zero server processes behind and testhost exits in ~4s.
/// This teardown is the orphan-proofing safety net: if a future test path (or an interrupted run)
/// ever leaves a <c>DevContext.Server</c> apphost process alive, disposing the fixture kills it so it
/// cannot lock <c>bin/</c> for the next build.
///
/// It targets ONLY the apphost-named process (<c>DevContext.Server.exe</c>), never <c>dotnet</c>, so
/// a developer's separately-launched dev server — which start-dev-bg.ps1 runs as
/// <c>dotnet &lt;dll&gt;</c> — is never touched by a test run.
/// </summary>
public sealed class ServerTestFactory : WebApplicationFactory<Program>
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        foreach (var proc in Process.GetProcessesByName("DevContext.Server"))
        {
            try
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit(2000);
            }
            catch
            {
                // Already gone, or access denied — best-effort cleanup only.
            }
            finally
            {
                proc.Dispose();
            }
        }
    }
}
