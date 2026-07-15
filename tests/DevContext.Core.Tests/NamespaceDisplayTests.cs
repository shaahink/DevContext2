using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>T2.7 (audit A7) — the synthetic "global" namespace keeps a stable graph identity but must
/// never reach the user as a group label; it falls back namespace → project/top-folder.</summary>
public sealed class NamespaceDisplayTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("global", true)]
    [InlineData("Ordering.API", false)]
    [InlineData("globalization", false)] // not the sentinel — a real namespace that merely starts with "global"
    public void IsGlobal_only_matches_the_sentinel_or_empty(string? ns, bool expected)
        => Assert.Equal(expected, NamespaceDisplay.IsGlobal(ns));

    [Fact]
    public void Label_returns_the_namespace_when_it_is_real()
        => Assert.Equal("Ordering.API", NamespaceDisplay.Label("Ordering.API", "fallback"));

    [Fact]
    public void Label_falls_back_when_global_and_never_returns_global()
    {
        Assert.Equal("Apis", NamespaceDisplay.Label("global", "Apis"));
        Assert.Equal("app", NamespaceDisplay.Label("global", null));   // no fallback → "app", never "global"
        Assert.Equal("app", NamespaceDisplay.Label("", "   "));
    }

    [Fact]
    public void FolderLabel_returns_the_immediate_containing_folder()
    {
        Assert.Equal("Apis", NamespaceDisplay.FolderLabel(@"C:\repo\src\Ordering.API\Apis\OrdersApi.cs"));
        Assert.Equal("Apis", NamespaceDisplay.FolderLabel("/repo/src/Ordering.API/Apis/OrdersApi.cs"));
        Assert.Null(NamespaceDisplay.FolderLabel(null));
    }
}
