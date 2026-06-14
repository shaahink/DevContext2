using DevContext.Core.Pipeline;

namespace DevContext.Desktop.ViewModels;

public sealed record AnalysisKey(
    string ProjectPath, string Scenario, string Focus, string Profile,
    bool NoRoslyn, bool DryRun, bool IncludeAntiPatterns);

public sealed class SnapshotCache(int capacity = 8)
{
    private readonly LinkedList<(AnalysisKey Key, AnalysisSnapshot Snap)> _lru = new();
    private readonly Dictionary<AnalysisKey, LinkedListNode<(AnalysisKey, AnalysisSnapshot)>> _map = new();

    public bool TryGet(AnalysisKey key, out AnalysisSnapshot snapshot)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _lru.Remove(node);
            _lru.AddFirst(node);
            snapshot = node.Value.Item2;
            return true;
        }
        snapshot = null!;
        return false;
    }

    public void Set(AnalysisKey key, AnalysisSnapshot snapshot)
    {
        if (_map.TryGetValue(key, out var existing)) { _lru.Remove(existing); _map.Remove(key); }
        var node = _lru.AddFirst((key, snapshot));
        _map[key] = node;
        while (_map.Count > capacity)
        {
            var last = _lru.Last!;
            _lru.RemoveLast();
            _map.Remove(last.Value.Item1);
        }
    }

    public void Clear() { _lru.Clear(); _map.Clear(); }
}
