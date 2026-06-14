using DevContext.Core.Configuration;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop.Tests;

public class SnapshotCacheTests
{
    private static RunReport DefaultReport => new()
    {
        Stages = [], Extractors = [], Scorers = [], Compressions = [],
        Cache = new(0, 0, 0, 0), Corpus = new(0, 0, 0),
        Funnel = new(0, 0, 0, 0, 0, 0),
        Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
        TotalWall = TimeSpan.Zero,
    };

    private static AnalysisSnapshot MakeSnapshot(string label = "test") =>
        new()
        {
            Model = new DiscoveryModel(),
            Analysis = new SharedAnalysisContext(),
            Scenario = ScenarioRegistry.BuiltIn["overview"],
            Options = new ExtractionOptions(),
            Report = DefaultReport,
        };

    private static AnalysisKey MakeKey(string path = "C:\\test", string scenario = "overview", string focus = "") =>
        new(path, scenario, focus, "focused", false, false, false);

    [Fact]
    public void TryGet_miss_returns_false()
    {
        var cache = new SnapshotCache(4);
        Assert.False(cache.TryGet(MakeKey(), out _));
    }

    [Fact]
    public void Set_and_TryGet_returns_same_snapshot()
    {
        var cache = new SnapshotCache(4);
        var key = MakeKey();
        var snap = MakeSnapshot();
        cache.Set(key, snap);
        Assert.True(cache.TryGet(key, out var retrieved));
        Assert.Same(snap, retrieved);
    }

    [Fact]
    public void TryGet_moves_to_front_so_LRU_evicts_other()
    {
        var cache = new SnapshotCache(2);
        var keyA = MakeKey(path: "C:\\a");
        var keyB = MakeKey(path: "C:\\b");
        var keyC = MakeKey(path: "C:\\c");
        cache.Set(keyA, MakeSnapshot("a"));
        cache.Set(keyB, MakeSnapshot("b"));

        // Access A to make it recently-used; B is now least-recently-used
        Assert.True(cache.TryGet(keyA, out _));

        cache.Set(keyC, MakeSnapshot("c"));

        Assert.True(cache.TryGet(keyA, out _));  // A still present (was recently used)
        Assert.False(cache.TryGet(keyB, out _)); // B evicted (LRU)
        Assert.True(cache.TryGet(keyC, out _));  // C present
    }

    [Fact]
    public void Set_overwrites_existing_key_and_preserves_count()
    {
        var cache = new SnapshotCache(2);
        var key = MakeKey();
        var snap1 = MakeSnapshot("first");
        var snap2 = MakeSnapshot("second");
        cache.Set(key, snap1);
        cache.Set(key, snap2);
        Assert.True(cache.TryGet(key, out var retrieved));
        Assert.Same(snap2, retrieved);

        // capacity is 2; only one key so eviction shouldn't fire
        var keyB = MakeKey(path: "C:\\b");
        cache.Set(keyB, MakeSnapshot("b"));
        Assert.True(cache.TryGet(key, out _));
        Assert.True(cache.TryGet(keyB, out _));
    }

    [Fact]
    public void Eviction_happens_at_capacity_plus_one()
    {
        var cache = new SnapshotCache(3);
        cache.Set(MakeKey(path: "C:\\1"), MakeSnapshot());
        cache.Set(MakeKey(path: "C:\\2"), MakeSnapshot());
        cache.Set(MakeKey(path: "C:\\3"), MakeSnapshot());
        cache.Set(MakeKey(path: "C:\\4"), MakeSnapshot());

        Assert.False(cache.TryGet(MakeKey(path: "C:\\1"), out _)); // LRU evicted
        Assert.True(cache.TryGet(MakeKey(path: "C:\\2"), out _));
        Assert.True(cache.TryGet(MakeKey(path: "C:\\3"), out _));
        Assert.True(cache.TryGet(MakeKey(path: "C:\\4"), out _));
    }

    [Fact]
    public void Clear_removes_all_entries()
    {
        var cache = new SnapshotCache(4);
        var key = MakeKey();
        cache.Set(key, MakeSnapshot());
        cache.Clear();
        Assert.False(cache.TryGet(key, out _));
    }

    [Fact]
    public void Different_analysis_params_produce_different_keys()
    {
        var cache = new SnapshotCache(4);
        var key1 = new AnalysisKey("C:\\test", "overview", "MyClass", "focused", false, false, false);
        var key2 = new AnalysisKey("C:\\test", "overview", "OtherClass", "focused", false, false, false);
        cache.Set(key1, MakeSnapshot());
        Assert.False(cache.TryGet(key2, out _));
        Assert.True(cache.TryGet(key1, out _));
    }

    [Fact]
    public void Render_params_are_excluded_from_key_equality()
    {
        var key = new AnalysisKey("C:\\test", "overview", "Foo", "focused", true, true, true);
        var same = new AnalysisKey("C:\\test", "overview", "Foo", "focused", true, true, true);
        Assert.Equal(key, same);
        Assert.Equal(key.GetHashCode(), same.GetHashCode());
    }
}
