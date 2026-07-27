using System.Xml.Linq;

using DevContext.Core.Resolvers;

namespace DevContext.Core.Tests;

public sealed class CsprojReaderCpmTests
{
    private static readonly string FixtureRoot = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../fixtures/CpmProject"));

    [Fact]
    public void ParsePackageReferencesCpmAware_resolves_versions_from_directory_packages_props()
    {
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");
        var doc = XDocument.Load(csprojPath);

        var packages = CsprojReader.ParsePackageReferencesCpmAware(doc, csprojPath);

        Assert.Equal(3, packages.Length);

        var mediatr = packages.Single(p => p.Name == "MediatR");
        Assert.Equal("12.0.0", mediatr.Version);

        var fv = packages.Single(p => p.Name == "FluentValidation");
        Assert.Equal("11.5.0", fv.Version);

        var hosting = packages.Single(p => p.Name == "Microsoft.Extensions.Hosting");
        Assert.Equal("9.0.0", hosting.Version);
    }

    [Fact]
    public void ParsePackageReferencesCpmAware_returns_empty_version_when_no_cpm_file_exists()
    {
        var csprojXml = """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="SomePkg" />
              </ItemGroup>
            </Project>
            """;
        var doc = XDocument.Parse(csprojXml);
        var csprojPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "test.csproj");

        var packages = CsprojReader.ParsePackageReferencesCpmAware(doc, csprojPath);

        Assert.Single(packages);
        Assert.Equal("", packages[0].Version);
    }

    [Fact]
    public void ParsePackageReferencesCpmAware_inline_version_overrides_cpm()
    {
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");
        var doc = XDocument.Load(csprojPath);

        var packages = CsprojReader.ParsePackageReferencesCpmAware(doc, csprojPath);
        var hosting = packages.Single(p => p.Name == "Microsoft.Extensions.Hosting");

        Assert.Equal("9.0.0", hosting.Version);
    }

    [Fact]
    public void ResolveOutputType_returns_from_directory_build_props_when_not_in_csproj()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
            </Project>
            """);
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");

        var outputType = CsprojReader.ResolveOutputType(csprojDoc, csprojPath);

        Assert.Equal("Exe", outputType);
    }

    [Fact]
    public void ResolveOutputType_ignores_a_conditioned_ancestor_value()
    {
        // Prism D1.2b — xunit's src/Directory.Build.props shape: OutputType=Exe applies ONLY to
        // '.v3.*.tests' projects via <Choose><When Condition=...>. Taking it unconditionally made
        // every xunit CLASSLIB read as an exe, which erased the self-sourced 'testing' signal and
        // flipped the whole repo Library -> App (archetype != reality).
        var dir = Path.Combine(Path.GetTempPath(), "dcx-cond-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "src"));
        File.WriteAllText(Path.Combine(dir, "Directory.Build.props"), """
            <Project>
              <Choose>
                <When Condition=" $(MSBuildProjectName.EndsWith('.tests')) ">
                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                  </PropertyGroup>
                </When>
              </Choose>
            </Project>
            """);
        try
        {
            var classlib = XDocument.Parse("""<Project Sdk="Microsoft.NET.Sdk"></Project>""");
            var resolved = CsprojReader.ResolveOutputType(
                classlib, Path.Combine(dir, "src", "xunit.v3.assert.csproj"));

            Assert.Null(resolved);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ResolveOutputType_returns_csproj_value_when_set()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Library</OutputType>
              </PropertyGroup>
            </Project>
            """);
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");

        var outputType = CsprojReader.ResolveOutputType(csprojDoc, csprojPath);

        Assert.Equal("Library", outputType);
    }

    [Fact]
    public void ResolveOutputType_returns_null_when_unset_everywhere()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
            </Project>
            """);
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "fake.csproj");

        var outputType = CsprojReader.ResolveOutputType(csprojDoc, nonExistentPath);

        Assert.Null(outputType);
    }

    [Fact]
    public void ResolveIsPackable_returns_true_from_directory_build_props()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
            </Project>
            """);
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");

        // Directory.Build.props has IsPackable=false, but csproj doesn't set it
        var isPackable = CsprojReader.ResolveIsPackable(csprojDoc, csprojPath);

        Assert.False(isPackable);
    }

    [Fact]
    public void ResolveIsPackable_returns_false_when_unset_everywhere()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
            </Project>
            """);
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "fake.csproj");

        Assert.False(CsprojReader.ResolveIsPackable(csprojDoc, nonExistentPath));
    }

    [Fact]
    public void ResolveTargetFrameworks_returns_from_csproj()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "fake.csproj");

        var tfms = CsprojReader.ResolveTargetFrameworks(csprojDoc, nonExistentPath);

        Assert.Single(tfms);
        Assert.Equal("net8.0", tfms[0]);
    }

    [Fact]
    public void ResolveTargetFrameworks_returns_from_directory_build_props()
    {
        var csprojDoc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
            </Project>
            """);
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");

        var tfms = CsprojReader.ResolveTargetFrameworks(csprojDoc, csprojPath);

        Assert.Single(tfms);
        Assert.Equal("net10.0", tfms[0]);
    }

    [Fact]
    public void ResolveCpmVersions_returns_expected_packages()
    {
        var csprojPath = Path.Combine(FixtureRoot, "src", "App", "App.csproj");
        var versions = CsprojReader.ResolveCpmVersions(csprojPath);

        Assert.True(versions.ContainsKey("MediatR"));
        Assert.Equal("12.0.0", versions["MediatR"]);
        Assert.True(versions.ContainsKey("FluentValidation"));
        Assert.Equal("11.5.0", versions["FluentValidation"]);
        Assert.True(versions.ContainsKey("Microsoft.Extensions.Hosting"));
        Assert.Equal("10.0.0", versions["Microsoft.Extensions.Hosting"]);
    }

    [Fact]
    public void ResolveCpmVersions_returns_empty_when_no_directory_packages_props()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "fake.csproj");
        var versions = CsprojReader.ResolveCpmVersions(nonExistentPath);

        Assert.Empty(versions);
    }
}
