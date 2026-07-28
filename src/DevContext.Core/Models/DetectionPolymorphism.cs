using System.Text.Json.Serialization.Metadata;

namespace DevContext.Core.Models;

/// <summary>
/// Batch D (R2 §2.D) — the ONE place <see cref="Detection"/>'s runtime polymorphism is declared.
/// <para>There used to be two schemes for the same type hierarchy: a hand-maintained
/// <c>[JsonDerivedType]</c> attribute list on <c>Detection</c> (discriminator <c>"type"</c>, used by the
/// JSON context render) and a reflective modifier in <c>SnapshotPersistence</c> (discriminator
/// <c>"$dtype"</c>, used by the snapshot cache). Two schemes meant a new detection record could
/// round-trip through the cache and be unserializable in the render, or vice versa — the attribute list
/// had already drifted into inconsistent indentation, which is what a list nobody can verify looks like.</para>
/// <para>The reflective one survives because it CANNOT drift: every concrete record deriving from
/// <see cref="Detection"/> in this assembly is registered under its own type name. The wire
/// discriminator is <c>"type"</c> — the render's existing name — so the JSON output contract is
/// byte-identical; the snapshot's <c>"$dtype"</c> changes, which is what the schema version bump covers.</para>
/// </summary>
public static class DetectionPolymorphism
{
    /// <summary>The wire property carrying the detection's type name.</summary>
    public const string DiscriminatorProperty = "type";

    /// <summary>A <see cref="DefaultJsonTypeInfoResolver"/> modifier: registers every concrete
    /// <see cref="Detection"/> subtype in this assembly under its type name. An unknown discriminator on
    /// load throws — the snapshot cache treats that as a miss and re-analyzes.</summary>
    public static void Apply(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Detection)) return;
        var options = new JsonPolymorphismOptions { TypeDiscriminatorPropertyName = DiscriminatorProperty };
        foreach (var t in typeof(Detection).Assembly.GetTypes()
                     .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Detection)))
                     .OrderBy(t => t.Name, StringComparer.Ordinal))
            options.DerivedTypes.Add(new JsonDerivedType(t, t.Name));
        typeInfo.PolymorphismOptions = options;
    }

    /// <summary>A resolver carrying <see cref="Apply"/>. Every serializer that touches a
    /// <see cref="Detection"/> uses this — there is no second way to spell it.</summary>
    public static DefaultJsonTypeInfoResolver Resolver() => new() { Modifiers = { Apply } };
}
