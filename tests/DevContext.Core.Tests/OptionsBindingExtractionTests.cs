namespace DevContext.Core.Tests;

/// <summary>
/// F3 (BUG-BACKLOG #34, 2026-08-26 unseen drive) — the config catalog was blind to the Options
/// pattern: <c>services.AddOptions&lt;QueueDrainOptions&gt;().BindConfiguration(QueueDrainOptions.SectionName)</c>
/// yielded zero keys, so <c>config</c> found ONE key in a 487-file repo. These pin the extraction-time
/// detection in <see cref="DiRegistrationExtractor"/>: the chained binding call is read and its section
/// argument resolved exactly the way F2 resolved group prefixes — literal, <c>nameof</c>, bare const on
/// the enclosing type, or qualified <c>Type.Member</c> through the project-wide const index — and a
/// computed expression is NEVER guessed.
/// </summary>
public sealed class OptionsBindingExtractionTests
{
    [Fact]
    public async Task AddOptions_BindConfiguration_Literal_YieldsSectionKey()
    {
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/Pipeline/DependencyInjection.cs",
            """
            public static class DependencyInjection
            {
                public static IServiceCollection AddPipeline(this IServiceCollection services)
                {
                    services.AddOptions<QueueDrainOptions>()
                        .BindConfiguration("Pipeline:Queue:Drain")
                        .ValidateDataAnnotations();
                    return services;
                }
            }
            """);

        var binding = Assert.Single(model.Detections.OfType<OptionsBindingDetection>());
        Assert.Equal("QueueDrainOptions", binding.OptionsType);
        Assert.Equal("Pipeline:Queue:Drain", binding.SectionKey);
        Assert.Equal("BindConfiguration", binding.BindingMethod);
    }

    [Fact]
    public async Task AddOptions_BindConfiguration_QualifiedConst_CrossFile_Resolves()
    {
        // THE Book2Course shape (DRIVE.md F3): the section name is a const on the options type,
        // declared in a DIFFERENT file from the composition-root registration. The const index is
        // project-wide, so "QueueDrainOptions.SectionName" resolves across the file boundary.
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:/repo/src/Pipeline/QueueDrainOptions.cs",
            """
            public sealed class QueueDrainOptions
            {
                public const string SectionName = "Pipeline:Queue:Drain";
                public int IdlePollSeconds { get; set; } = 2;
                public int Workers { get; set; } = 2;
            }
            """);
        fs.AddFile(@"C:/repo/src/Pipeline/DependencyInjection.cs",
            """
            public static class DependencyInjection
            {
                public static IServiceCollection AddPipeline(this IServiceCollection services)
                {
                    services.AddOptions<QueueDrainOptions>()
                        .BindConfiguration(QueueDrainOptions.SectionName)
                        .ValidateDataAnnotations()
                        .ValidateOnStart();
                    return services;
                }
            }
            """);

        var model = await RunExtractorOnFilesAsync(fs);

        var binding = Assert.Single(model.Detections.OfType<OptionsBindingDetection>());
        Assert.Equal("QueueDrainOptions", binding.OptionsType);
        Assert.Equal("Pipeline:Queue:Drain", binding.SectionKey);
        Assert.EndsWith("DependencyInjection.cs", binding.SourceFile);
        // The registration statement's own line — what config() will cite as the binding site.
        Assert.Equal(5, binding.LineNumber);
    }

    [Fact]
    public async Task AddOptions_BindConfiguration_BareConstOnEnclosingType_Resolves()
    {
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/Api/Startup.cs",
            """
            public static class Startup
            {
                private const string DrainSection = "Pipeline:Queue:Drain";

                public static void Wire(IServiceCollection services)
                {
                    services.AddOptions<QueueDrainOptions>().BindConfiguration(DrainSection);
                }
            }
            """);

        var binding = Assert.Single(model.Detections.OfType<OptionsBindingDetection>());
        Assert.Equal("Pipeline:Queue:Drain", binding.SectionKey);
    }

    [Fact]
    public async Task AddOptions_BindConfiguration_ComputedValue_IsNeverGuessed()
    {
        // The F2 negative, mirrored: a section name built by an expression nobody evaluated must
        // contribute NOTHING rather than a guess.
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/Api/Startup.cs",
            """
            public static class Startup
            {
                public static void Wire(IServiceCollection services)
                {
                    services.AddOptions<QueueDrainOptions>().BindConfiguration(BuildSectionName());
                    services.AddOptions<MediaOptions>().BindConfiguration("Prefix:" + Suffix());
                }

                private static string BuildSectionName() => "Pipeline:Queue:Drain";
                private static string Suffix() => "Media";
            }
            """);

        Assert.Empty(model.Detections.OfType<OptionsBindingDetection>());
    }

    [Fact]
    public async Task AddOptions_BindConfiguration_Nameof_Resolves()
    {
        // The eShop idiom: AddOptions<BackgroundTaskOptions>().BindConfiguration(nameof(BackgroundTaskOptions)).
        // nameof is a compile-time constant readable from syntax alone — resolving it is not a guess.
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/OrderProcessor/Extensions.cs",
            """
            public static class Extensions
            {
                public static void AddApplicationServices(this IHostApplicationBuilder builder)
                {
                    builder.Services.AddOptions<BackgroundTaskOptions>()
                        .BindConfiguration(nameof(BackgroundTaskOptions));
                }
            }
            """);

        var binding = Assert.Single(model.Detections.OfType<OptionsBindingDetection>());
        Assert.Equal("BackgroundTaskOptions", binding.SectionKey);
    }

    [Fact]
    public async Task AddOptions_Bind_GetSection_Const_Resolves()
    {
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/Api/Startup.cs",
            """
            public static class Startup
            {
                private const string Section = "Storage:S3";

                public static void Wire(IServiceCollection services, IConfiguration configuration)
                {
                    services.AddOptions<S3Options>().Bind(configuration.GetSection(Section));
                }
            }
            """);

        var binding = Assert.Single(model.Detections.OfType<OptionsBindingDetection>());
        Assert.Equal("S3Options", binding.OptionsType);
        Assert.Equal("Storage:S3", binding.SectionKey);
        Assert.Equal("Bind", binding.BindingMethod);
    }

    [Fact]
    public async Task Configure_WithGetSection_Resolves()
    {
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/Api/Startup.cs",
            """
            public static class Startup
            {
                public static void Wire(IServiceCollection services, IConfiguration configuration)
                {
                    services.Configure<MediaOptions>(configuration.GetSection("Pipeline:Media"));
                }
            }
            """);

        var binding = Assert.Single(model.Detections.OfType<OptionsBindingDetection>());
        Assert.Equal("MediaOptions", binding.OptionsType);
        Assert.Equal("Pipeline:Media", binding.SectionKey);
        Assert.Equal("Configure", binding.BindingMethod);
    }

    [Fact]
    public async Task Configure_WithLambda_YieldsNoBinding()
    {
        // Configure<T>(o => ...) binds no configuration section — it must not produce a key.
        var model = await RunExtractorOnSourceAsync(
            @"C:/repo/src/Api/Startup.cs",
            """
            public static class Startup
            {
                public static void Wire(IServiceCollection services)
                {
                    services.Configure<CookiePolicyOptions>(o => o.CheckConsentNeeded = ctx => true);
                }
            }
            """);

        Assert.Empty(model.Detections.OfType<OptionsBindingDetection>());
    }

    private static async Task<DiscoveryModel> RunExtractorOnSourceAsync(string fileName, string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(fileName, source);
        return await RunExtractorOnFilesAsync(fs);
    }

    private static async Task<DiscoveryModel> RunExtractorOnFilesAsync(FakeFileSystem fs)
    {
        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:/repo");
        var (ctx, _) = builder.BuildWithRecording();

        var allFiles = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("", "*", SearchOption.AllDirectories))
            allFiles.Add(f);
        ctx.Analysis.AllSourceFiles = allFiles;

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None);
        return model;
    }
}
