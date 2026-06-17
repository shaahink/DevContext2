namespace DevContext.Core.Tests;

public sealed class SharedUtilitiesTests
{
    // ── TokenEstimator ──────────────────────────────────────────────────

    [Fact]
    public void TokenEstimator_EmptyModel_ReturnsOne()
    {
        var model = new DiscoveryModel();
        var tokens = TokenEstimator.Estimate(model);
        Assert.Equal(1, tokens);
    }

    [Fact]
    public void TokenEstimator_CountsTypeNameAndNamespace()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Test", new TypeDiscovery
        {
            Id = "MyApp.Test",
            Name = "MyService",
            Namespace = "MyApp",
            FilePath = "test.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        var tokens = TokenEstimator.Estimate(model);
        Assert.True(tokens > 1);
    }

    [Fact]
    public void TokenEstimator_ExcludesHardExcludedByDefault()
    {
        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "MyApp.Test",
            Name = "Test",
            Namespace = "MyApp",
            FilePath = "test.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        };
        type.IsHardExcluded = true;
        model.Types.TryAdd("MyApp.Test", type);

        var tokens = TokenEstimator.Estimate(model);
        Assert.Equal(1, tokens); // only Math.Max(1, chars/4) = 1, since hard-excluded type is skipped
    }

    [Fact]
    public void TokenEstimator_IncludesHardExcludedWhenFlagIsOff()
    {
        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "MyApp.Test",
            Name = "TestService",
            Namespace = "MyApp",
            FilePath = "test.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        };
        type.IsHardExcluded = true;
        model.Types.TryAdd("MyApp.Test", type);

        var tokensDefault = TokenEstimator.Estimate(model); // excludeHardExcluded=true
        var tokensInclude = TokenEstimator.Estimate(model, excludeHardExcluded: false);

        Assert.Equal(1, tokensDefault);
        Assert.True(tokensInclude > tokensDefault);
    }

    [Fact]
    public void TokenEstimator_SkipsPrunedTypes()
    {
        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "MyApp.Test",
            Name = "TestService",
            Namespace = "MyApp",
            FilePath = "test.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        };
        type.IsPruned = true;
        model.Types.TryAdd("MyApp.Test", type);

        var tokens = TokenEstimator.Estimate(model);
        Assert.Equal(1, tokens);
    }

    [Fact]
    public void TokenEstimator_IncludeSourceBody_AddsChars()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Test", new TypeDiscovery
        {
            Id = "MyApp.Test",
            Name = "Test",
            Namespace = "MyApp",
            FilePath = "test.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = "public class Test { public int Id { get; set; } }",
        });

        var withoutBody = TokenEstimator.Estimate(model, includeSourceBody: false);
        var withBody = TokenEstimator.Estimate(model, includeSourceBody: true);

        Assert.True(withBody > withoutBody);
    }

    // ── GenericArgumentParser ───────────────────────────────────────────

    [Fact]
    public void SplitGenericArgs_SimpleSingleArg()
    {
        var args = GenericArgumentParser.SplitGenericArgs("string");
        Assert.Single(args);
        Assert.Equal("string", args[0]);
    }

    [Fact]
    public void SplitGenericArgs_TwoArgs()
    {
        var args = GenericArgumentParser.SplitGenericArgs("string, int");
        Assert.Equal(2, args.Length);
        Assert.Equal("string", args[0]);
        Assert.Equal("int", args[1]);
    }

    [Fact]
    public void SplitGenericArgs_NestedGenerics()
    {
        var args = GenericArgumentParser.SplitGenericArgs("Task<IEnumerable<T>>, CancellationToken");
        Assert.Equal(2, args.Length);
        Assert.Equal("Task<IEnumerable<T>>", args[0]);
        Assert.Equal("CancellationToken", args[1]);
    }

    [Fact]
    public void SplitGenericArgs_DeeplyNested()
    {
        var args = GenericArgumentParser.SplitGenericArgs(
            "Dictionary<string, List<KeyValuePair<int, string>>>");
        Assert.Single(args);
        Assert.Equal("Dictionary<string, List<KeyValuePair<int, string>>>", args[0]);
    }

    [Fact]
    public void SplitGenericArgs_Empty_ReturnsEmptyArray()
    {
        var args = GenericArgumentParser.SplitGenericArgs("");
        Assert.Empty(args);
    }

    [Fact]
    public void ExtractGenericArguments_NoGenerics_ReturnsEmpty()
    {
        var args = GenericArgumentParser.ExtractGenericArguments("string");
        Assert.Empty(args);
    }

    [Fact]
    public void ExtractGenericArguments_Simple()
    {
        var args = GenericArgumentParser.ExtractGenericArguments("IRequestHandler<int, string>");
        Assert.Equal(2, args.Length);
        Assert.Equal("int", args[0]);
        Assert.Equal("string", args[1]);
    }

    [Fact]
    public void ExtractGenericArguments_Nested()
    {
        var args = GenericArgumentParser.ExtractGenericArguments(
            "INotificationHandler<OrderPlacedEvent>");
        Assert.Single(args);
        Assert.Equal("OrderPlacedEvent", args[0]);
    }

    [Fact]
    public void ExtractGenericBaseName_NoGenerics()
    {
        var name = GenericArgumentParser.ExtractGenericBaseName("string");
        Assert.Equal("string", name);
    }

    [Fact]
    public void ExtractGenericBaseName_WithGenerics()
    {
        var name = GenericArgumentParser.ExtractGenericBaseName("IRequestHandler<int, string>");
        Assert.Equal("IRequestHandler", name);
    }
}
