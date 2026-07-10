using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

public sealed class SemanticLitePopulatorTests
{
    [Theory]
    [InlineData("lib/net10.0/Foo.dll", 100)]
    [InlineData("lib/net9.0/Foo.dll", 90)]
    [InlineData("lib/net8.0/Foo.dll", 80)]
    [InlineData("lib/net7.0/Foo.dll", 70)]
    [InlineData("lib/net6.0/Foo.dll", 60)]
    [InlineData("lib/net5.0/Foo.dll", 50)]
    [InlineData("lib/net11.0/Foo.dll", 110)]
    public void Scores_modern_tfm_by_major(string path, int expected)
    {
        Assert.Equal(expected, SemanticLitePopulator.TfmScore(path));
    }

    [Theory]
    [InlineData("lib/netcoreapp3.1/Foo.dll", 40)]
    [InlineData("lib/netstandard2.1/Foo.dll", 31)]
    [InlineData("lib/netstandard2.0/Foo.dll", 30)]
    [InlineData("lib/netstandard1.0/Foo.dll", 20)]
    [InlineData("lib/net45/Foo.dll", 10)]
    [InlineData("lib/net48/Foo.dll", 10)]
    public void Scores_legacy_tfm_by_fallback(string path, int expected)
    {
        Assert.Equal(expected, SemanticLitePopulator.TfmScore(path));
    }

    [Fact]
    public void Unknown_tfm_scores_minimum()
    {
        Assert.Equal(1, SemanticLitePopulator.TfmScore("lib/nosuch/Foo.dll"));
    }

    [Fact]
    public void Multiple_segments_picks_last_net()
    {
        Assert.Equal(100, SemanticLitePopulator.TfmScore("lib/net8.0/subdir/net10.0/Foo.dll"));
    }

    [Fact]
    public void NetX_without_dot_scores_fallback()
    {
        // net45, net48 etc. have digits but no dot — should fall through to fallback
        Assert.Equal(10, SemanticLitePopulator.TfmScore("lib/net45/Foo.dll"));
    }

    [Fact]
    public void Higher_major_ranks_above_lower()
    {
        var score10 = SemanticLitePopulator.TfmScore("lib/net10.0/a.dll");
        var score9 = SemanticLitePopulator.TfmScore("lib/net9.0/b.dll");
        Assert.True(score10 > score9);
    }

    [Fact]
    public void Multi_digit_minor_is_parsed()
    {
        Assert.Equal(10 * 10 + 10, SemanticLitePopulator.TfmScore("lib/net10.10/Foo.dll"));
    }
}
