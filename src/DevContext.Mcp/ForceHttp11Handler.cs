using System.Net;

namespace DevContext.Mcp;

// gRPC-Web always uses HTTP/1.1 — GrpcChannel may attempt HTTP/2 which the server rejects
internal sealed class ForceHttp11Handler : DelegatingHandler
{
    public ForceHttp11Handler(HttpMessageHandler inner) : base(inner) { }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Version = HttpVersion.Version11;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
        return base.SendAsync(request, cancellationToken);
    }
}
