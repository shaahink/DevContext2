using System.Collections;

namespace DevContext.Core.Models;

/// <summary>Thread-safe append accumulator whose enumeration order becomes deterministic once the
/// pipeline calls <see cref="Seal"/> with a canonical comparer. Parallel extractors append in
/// arrival order, which varies run-to-run (the D5.3 determinism flaps: ServiceLink provenance
/// anchors and seam citation sites are first-match picks over this sequence); sealing once after
/// extraction gives every downstream consumer the same order every run. Replaces
/// <c>ConcurrentBag&lt;T&gt;</c>, whose enumeration order is nondeterministic even after all adds
/// complete.</summary>
public sealed class SealableBag<T> : IEnumerable<T>
{
    private readonly object _gate = new();
    private readonly List<T> _items = [];

    /// <summary>Appends an item (thread-safe).</summary>
    public void Add(T item) { lock (_gate) { _items.Add(item); } }

    /// <summary>Current item count.</summary>
    public int Count { get { lock (_gate) { return _items.Count; } } }

    /// <summary>True when the bag holds no items.</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Removes all items. Re-adds after a clear keep insertion order (single-writer replace,
    /// as in the SemanticLite call-edge upgrade).</summary>
    public void Clear() { lock (_gate) { _items.Clear(); } }

    /// <summary>Sorts the accumulated items into the canonical order. The comparison must impose a
    /// total order over value-distinct items — items left tied may land in either slot.</summary>
    public void Seal(Comparison<T> canonicalOrder) { lock (_gate) { _items.Sort(canonicalOrder); } }

    /// <summary>Enumerates a point-in-time snapshot, in insertion order (canonical order after
    /// <see cref="Seal"/>). Safe against concurrent adds.</summary>
    public IEnumerator<T> GetEnumerator()
    {
        T[] snapshot;
        lock (_gate) { snapshot = [.. _items]; }
        return ((IEnumerable<T>)snapshot).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
