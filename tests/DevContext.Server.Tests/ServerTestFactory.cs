using System.Diagnostics;

using DevContext.Server.Sessions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

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
    // J2 — every analyze in this host would otherwise SAVE into (and next run LOAD from) the
    // user's real snapshot cache: pollution, plus AnalyzeFlowTests asserts on streamed progress
    // that a cache hit never emits. Redirect THIS host to a fresh temp root.
    //
    // G5 s18: this used to be a DEVCONTEXT_CACHE_ROOT write in the constructor and a write of null
    // in Dispose. Three test classes take this fixture and two more redirect the cache themselves,
    // all in xUnit collections that run concurrently in one process — so the redirect was a race,
    // and the null on Dispose pointed whoever was still running at the developer's REAL cache. The
    // root now travels through DI, where each host owns its own.
    private readonly string _cacheRoot = Path.Combine(
        Path.GetTempPath(), "devcontext-server-tests-cache", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            // Program.cs registers ServerOptions as an instance; re-register the same options with
            // this fixture's root so nothing else the real composition root bound is discarded.
            var registered = services.Last(d => d.ServiceType == typeof(ServerOptions));
            var prior = (ServerOptions)registered.ImplementationInstance!;
            services.Remove(registered);
            services.AddSingleton(prior with { SnapshotCacheRoot = _cacheRoot });
        });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        try
        {
            if (Directory.Exists(_cacheRoot)) Directory.Delete(_cacheRoot, true);
        }
        catch (IOException) { /* temp dir — OS cleans up */ }
        catch (UnauthorizedAccessException) { /* same */ }

        // #39: the sweep used to kill EVERY DevContext.Server on the machine by name — including
        // the one the MCP QA drive (or another checkout's probe) was mid-conversation with. Kill
        // only a process this factory can ATTRIBUTE: its main module lives under THIS repo's tree.
        // The factory's own host is in-process, so anything matched here is a leaked orphan from an
        // interrupted run in this checkout — the only thing this teardown exists to clean. A process
        // whose binary cannot be read (other user, exited) is unattributable and is left alone.
        var repoRoot = FindRepoRoot();
        foreach (var proc in Process.GetProcessesByName("DevContext.Server"))
        {
            try
            {
                var exePath = proc.MainModule?.FileName;
                if (repoRoot is null || exePath is null
                    || !exePath.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase))
                    continue;

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

    /// <summary>The checkout this test assembly was built in — the attribution boundary for the
    /// orphan sweep above. Walks up from the test bin dir to the solution marker.</summary>
    private static string? FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "DevContext.slnx")))
                return dir.FullName;
        return null;
    }
}
