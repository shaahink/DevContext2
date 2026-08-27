using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Tests;

/// <summary>F1 (#33) integration — fixture models must DECLARE what their source declares.
/// INV-C refuses a Member node of a type whose model entry does not vouch for it (the declares
/// oracle reads <see cref="TypeDiscovery.Methods"/>), so a source-first fixture that hand-builds
/// its <see cref="TypeDiscovery"/> with <c>Methods = []</c> is lying about its own source: every
/// edge landing on those members silently drops, and the test goes red for the wrong reason.
/// This helper reads the declarations off the fixture's ALREADY-PARSED tree — the same truth the
/// real pipeline's SyntaxStructureExtractor records — so zoo-style tests keep their
/// "the source body IS the fixture" authoring shape.</summary>
internal static class TestMethodSignatures
{
    /// <summary>Every method declared by any type in <paramref name="tree"/> (class, record,
    /// interface, struct — local functions are not member declarations and are excluded by
    /// construction), as the <see cref="MethodSignature"/> rows an honest model carries.</summary>
    public static ImmutableArray<MethodSignature> DeclaredIn(SyntaxTree tree)
        => [.. tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Select(m => new MethodSignature(
                m.Identifier.Text,
                m.ReturnType.ToString(),
                [.. m.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? "")],
                [.. m.ParameterList.Parameters.Select(p => p.Identifier.Text)],
                Microsoft.CodeAnalysis.Accessibility.Public,
                m.Modifiers.Any(SyntaxKind.StaticKeyword),
                m.Modifiers.Any(SyntaxKind.AsyncKeyword)))];
}
