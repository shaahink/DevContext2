using DevContext.Server.Endpoints;

namespace DevContext.Server.Tests;

/// <summary>
/// F5 (Prism D4.5) — ui/agent classification from the PRE-UseGrpcWeb content-type.
/// The stash-then-read mechanics live in Program.cs; the classification truth is here.
/// </summary>
public sealed class OriginTagTests
{
    [Theory]
    [InlineData("application/grpc-web+proto", "ui")]
    [InlineData("application/grpc-web-text", "ui")]
    [InlineData("application/GRPC-WEB+proto", "ui")]
    [InlineData("application/grpc", "agent")]
    [InlineData("", "agent")]
    [InlineData(null, "agent")]
    public void Classifies_by_original_content_type(string? contentType, string expected)
        => Assert.Equal(expected, OriginTag.FromContentType(contentType));
}
