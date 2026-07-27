using DevContext.Core.Extractors.Generic;

namespace DevContext.Core.Tests;

/// <summary>
/// T1.4 — per-service style inference. Every runnable service should read as what it is; before this the
/// ladder returned "Unknown" for MAUI clients, workers, and Blazor clients, and mislabeled a Blazor
/// storefront that also references YARP as "Gateway [YARP]". The rollup runs after Stage 3 so it can read
/// per-project detections (Blazor @page routes, message consumers) — asserted here on eShop-shaped models.
/// </summary>
public sealed class PerServiceStyleTests
{
    private const string Root = @"C:\repo\src";

    private static PackageReferenceInfo Pkg(string name) => new(name, "1.0.0");

    private static ProjectInfo ExeProj(string name, params PackageReferenceInfo[] pkgs)
        => new(name, $@"{Root}\{name}\{name}.csproj", "C#", ["net10.0"], [], [.. pkgs], "Exe");

    private static string StyleOf(DiscoveryModel model, string project)
    {
        var styles = ArchitectureStyleDetector.DetectPerServiceStyles(model);
        var s = styles.Single(x => x.ProjectName == project);
        return s.Stack.IsDefaultOrEmpty ? s.Style : $"{s.Style} [{string.Join(", ", s.Stack)}]";
    }

    [Fact]
    public void Maui_by_tfm_triple_reads_maui_without_any_package()
    {
        // B2 (Prism D1.2b) — podcasts shape: <UseMaui>true</UseMaui> (SDK-provided) + mobile TFMs,
        // ZERO Microsoft.Maui package references. Read Unknown at audit.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Microsoft.NetConf2021.Maui", $@"{Root}\Mobile\Microsoft.NetConf2021.Maui.csproj",
                    "C#", ["net7.0-android", "net7.0-ios", "net7.0-maccatalyst"], [], [], "Exe"),
            ],
        };

        Assert.Equal("MAUI App [.NET MAUI]", StyleOf(model, "Microsoft.NetConf2021.Maui"));
    }

    [Fact]
    public void Blazor_storefront_outranks_its_yarp_bff_reference()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                ExeProj("WebApp", Pkg("Yarp.ReverseProxy")),
                ExeProj("ApiGateway", Pkg("Yarp.ReverseProxy")),
            ],
        };
        // WebApp owns a Blazor @page route; ApiGateway owns none (a real proxy).
        model.Detections.Add(new EndpointDetection("GET", "/checkout", "Checkout", "<component>", [], [])
        { ExtractorName = "BlazorEntryExtractor", SourceFile = $@"{Root}\WebApp\Pages\Checkout.razor", LineNumber = 1 });

        Assert.Equal("Blazor [Blazor, YARP]", StyleOf(model, "WebApp"));
        Assert.Equal("Gateway [YARP]", StyleOf(model, "ApiGateway"));   // no Blazor pages → still a gateway
    }

    [Fact]
    public void Worker_maui_and_cli_are_not_unknown()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                ExeProj("PaymentProcessor"),                       // web-less consumer host
                ExeProj("ClientApp", Pkg("Microsoft.Maui.Controls")),
                ExeProj("ResearchCli", Pkg("Spectre.Console.Cli")),
            ],
        };
        // PaymentProcessor consumes a message and exposes no HTTP endpoints → Worker Service.
        model.Detections.Add(new MessageConsumerDetection("OrderPaidIntegrationEvent", "PaymentConsumer", "RabbitMQ")
        { ExtractorName = "test", SourceFile = $@"{Root}\PaymentProcessor\PaymentConsumer.cs", LineNumber = 10 });

        Assert.Equal("Worker Service [Worker]", StyleOf(model, "PaymentProcessor"));
        Assert.Equal("MAUI App [.NET MAUI]", StyleOf(model, "ClientApp"));
        Assert.Equal("CLI [CLI]", StyleOf(model, "ResearchCli"));
    }
}
