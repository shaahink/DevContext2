using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// Marks a truth test as pending (skipped) until a named Loom stage unblocks it.
/// Remove this attribute and flip to the normal [Fact] when the underlying fix lands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TruthPendingAttribute : FactAttribute
{
    public TruthPendingAttribute(string stage)
    {
        Skip = $"Truth ratchet: pending fix by {stage}";
    }
}

/// <summary>
/// Marks a truth theory test as pending (skipped) until a named Loom stage unblocks it.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TruthPendingTheoryAttribute : TheoryAttribute
{
    public TruthPendingTheoryAttribute(string stage)
    {
        Skip = $"Truth ratchet: pending fix by {stage}";
    }
}
