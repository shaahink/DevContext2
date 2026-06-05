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
        var tokensBefore = EstimateTotalTokens(model);
        var notes = new List<string>();
        var removedCount = 0;

        foreach (var type in model.Types.Values)
        {
            if (type.IsPruned) continue;

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

        var tokensAfter = EstimateTotalTokens(model);
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

    private static int EstimateTotalTokens(DiscoveryModel model)
    {
        var chars = 0;
        foreach (var type in model.Types.Values)
        {
            if (type.IsPruned) continue;
            chars += type.Name?.Length ?? 0;
            chars += type.Namespace?.Length ?? 0;
            chars += type.Methods.Sum(m => m.Name.Length + m.ReturnType.Length);
            chars += type.Properties.Sum(p => p.Name.Length + p.PropertyType.Length);
            chars += type.BaseTypes.Sum(b => b.Length);
            chars += type.ImplementedInterfaces.Sum(i => i.Length);
        }

        return Math.Max(1, chars / 4);
    }
}
