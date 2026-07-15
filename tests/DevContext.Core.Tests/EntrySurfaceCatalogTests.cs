using DevContext.Core.Graph.EntrySurfaces;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

/// <summary>
/// T1.5 — the architecture-style catalog must not carry false signals. An OpenAPI package
/// (Microsoft.AspNetCore.OpenApi / Swashbuckle) documents an API regardless of whether it is built
/// with minimal APIs or controllers, so it must NOT map to the minimal-apis signal — that flagged
/// controllers apps (which also document with OpenAPI) as MinimalApi at confidence 1.0, out-ranking
/// their real controllers (shamshir read "MinimalApi" instead of "NLayer").
/// </summary>
public sealed class EntrySurfaceCatalogTests
{
    [Fact]
    public void OpenApi_package_is_not_a_minimal_apis_signal()
    {
        foreach (var descriptor in EntrySurfaceCatalog.All)
        {
            if (descriptor.SignalKey != ArchitectureSignals.Keys.MinimalApis) continue;
            Assert.DoesNotContain(descriptor.Packages, p =>
                p.Contains("OpenApi", StringComparison.OrdinalIgnoreCase)
                || p.Contains("Swashbuckle", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void MinimalApis_still_triggers_from_the_web_sdk_hint()
    {
        // The Web SDK hint is load-bearing — EndpointExtractor only runs when the minimal-apis signal
        // is present — so it must remain the trigger even after the OpenApi package was removed.
        var minimal = EntrySurfaceCatalog.All.Single(d => d.SignalKey == ArchitectureSignals.Keys.MinimalApis);
        Assert.Contains("Microsoft.NET.Sdk.Web", minimal.SdkHints);
    }
}
