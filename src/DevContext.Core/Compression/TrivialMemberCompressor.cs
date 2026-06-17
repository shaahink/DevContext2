using DevContext.Core.Utilities;

namespace DevContext.Core.Compression;

/// <summary>Removes trivial members (parameterless constructors, ToString, Equals, GetHashCode, auto-properties).</summary>
public sealed class TrivialMemberCompressor : ICompressionStrategy
{
    /// <summary>Gets the name of this compression strategy.</summary>
    public string Name => "TrivialMemberCompressor";
    /// <summary>Gets the execution order.</summary>
    public int Order => 10;

    public ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct)
    {
        var tokensBefore = TokenEstimator.Estimate(model);
        var notes = new List<string>();
        var removedCount = 0;

        foreach (var type in model.Types.Values)
        {
            if (type.IsPruned || type.IsHardExcluded) continue;

            var originalMethods = type.Methods;
            var filteredMethods = originalMethods.Where(m => !IsTrivialMethod(m)).ToImmutableArray();
            if (filteredMethods.Length < originalMethods.Length)
            {
                removedCount += originalMethods.Length - filteredMethods.Length;
                notes.Add($"Removed {originalMethods.Length - filteredMethods.Length} trivial methods from '{type.Id}'");
            }

            var originalProperties = type.Properties;
            var filteredProperties = originalProperties.Where(p => !IsAutoProperty(p)).ToImmutableArray();
            if (filteredProperties.Length < originalProperties.Length)
            {
                removedCount += originalProperties.Length - filteredProperties.Length;
                notes.Add($"Removed {originalProperties.Length - filteredProperties.Length} auto-properties from '{type.Id}'");
            }

            type.Methods = filteredMethods;
            type.Properties = filteredProperties;
        }

        var tokensAfter = TokenEstimator.Estimate(model);
        return new ValueTask<CompressionResult>(new CompressionResult(
            Name, tokensBefore, tokensAfter, notes));
    }

    private static bool IsTrivialMethod(MethodSignature method)
    {
        if (method is { Name: ".ctor", ParameterTypes.Length: 0 }) return true;
        if (method.Name is "ToString" or "Equals" or "GetHashCode") return true;

        return false;
    }

    private static bool IsAutoProperty(PropertySignature property)
    {
        return property.HasGetter && property.HasSetter && !property.IsReadOnly;
    }

}
