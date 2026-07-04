using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

public sealed class MultiImplSourceTests
{
    private static readonly CodeGraph EmptyGraph = new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());

    private static DiRegistrationDetection Reg(string serviceType, string implType) => new(
        serviceType, implType, "Scoped", [])
    {
        ExtractorName = "Test",
        SourceFile = "Startup.cs",
        LineNumber = 1,
    };

    private static void RegisterInterface(DiscoveryModel model, string fqn)
    {
        var name = fqn[(fqn.LastIndexOf('.') + 1)..];
        model.Types[fqn] = new TypeDiscovery
        {
            Id = fqn,
            Name = name,
            Namespace = fqn[..fqn.LastIndexOf('.')],
            FilePath = "Types.cs",
            Kind = TypeKind.Interface,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        };
    }

    private static void RegisterClass(DiscoveryModel model, string fqn)
    {
        var name = fqn[(fqn.LastIndexOf('.') + 1)..];
        model.Types[fqn] = new TypeDiscovery
        {
            Id = fqn,
            Name = name,
            Namespace = fqn[..fqn.LastIndexOf('.')],
            FilePath = "Types.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Infrastructure,
        };
    }

    private static DiscoveryModel WithNoiseRegistrations(int count = 8)
    {
        var model = new DiscoveryModel();
        // Pad past the >= 10 DiRegistrationDetection gate with unrelated single-impl registrations.
        for (var i = 0; i < count; i++)
            model.Detections.Add(Reg($"IService{i}", $"Service{i}"));
        return model;
    }

    [Fact]
    public void GroupsByServiceType_NotImplementationType()
    {
        // E2: a concrete class registered twice under different (or the same) service used to headline
        // as a "multi-impl interface" because the old code grouped by ImplementationType ?? ServiceType.
        var model = WithNoiseRegistrations();
        RegisterClass(model, "MyApp.Data.TodoDbContext");
        model.Detections.Add(Reg("MyApp.Data.TodoDbContext", "MyApp.Data.TodoDbContext"));
        model.Detections.Add(Reg("MyApp.Data.TodoDbContext", "MyApp.Data.TodoDbContext"));

        RegisterInterface(model, "MyApp.Services.INotifier");
        model.Detections.Add(Reg("MyApp.Services.INotifier", "MyApp.Services.EmailNotifier"));
        model.Detections.Add(Reg("MyApp.Services.INotifier", "MyApp.Services.SmsNotifier"));

        var insight = new MultiImplSource().Compute(model, EmptyGraph, []).Single();

        Assert.Contains("INotifier (2 impls)", insight.Evidence);
        Assert.DoesNotContain(insight.Evidence, e => e.Contains("TodoDbContext"));
    }

    [Fact]
    public void UnresolvedServiceType_NeverLeaksBareQuestionMark()
    {
        var model = WithNoiseRegistrations();
        model.Detections.Add(Reg("?", "SomeImpl"));
        model.Detections.Add(Reg("?", "OtherImpl"));

        Assert.Empty(new MultiImplSource().Compute(model, EmptyGraph, []));
    }

    [Fact]
    public void DuplicateRegistrationOfSameImpl_CountsOnceAsDistinct()
    {
        var model = WithNoiseRegistrations();
        RegisterInterface(model, "MyApp.Services.IValidator");
        model.Detections.Add(Reg("MyApp.Services.IValidator", "MyApp.Services.DefaultValidator"));
        model.Detections.Add(Reg("MyApp.Services.IValidator", "MyApp.Services.DefaultValidator"));

        Assert.Empty(new MultiImplSource().Compute(model, EmptyGraph, []));
    }

    [Fact]
    public void UnresolvedInRepo_FallsBackToNamingConvention_ForVendorInterface()
    {
        // IEmailSender isn't declared in this repo (it's a vendor/BCL abstraction) — the convention
        // fallback (I + uppercase) must still recognize it as a service abstraction.
        var model = WithNoiseRegistrations();
        model.Detections.Add(Reg("IEmailSender", "SmtpEmailSender"));
        model.Detections.Add(Reg("IEmailSender", "MockEmailSender"));

        var insight = new MultiImplSource().Compute(model, EmptyGraph, []).Single();
        Assert.Contains("IEmailSender (2 impls)", insight.Evidence);
    }

    [Fact]
    public void ConcreteClassNotDeclaredLocally_IsNotTreatedAsAbstraction()
    {
        var model = WithNoiseRegistrations();
        model.Detections.Add(Reg("SomeOptions", "SomeOptions"));
        model.Detections.Add(Reg("SomeOptions", "OtherOptionsShim"));

        Assert.Empty(new MultiImplSource().Compute(model, EmptyGraph, []));
    }
}
