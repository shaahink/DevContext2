using DevContext.Cli.Services;
using DevContext.Core.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevContext.Desktop.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void Di_container_builds_and_resolves_pipeline()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDevContextServices(".");

        var sp = services.BuildServiceProvider();

        var pipeline = sp.GetRequiredService<DiscoveryPipeline>();
        Assert.NotNull(pipeline);
    }
}
