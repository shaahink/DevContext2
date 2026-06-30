namespace DevContext.Core.Graph;

/// <summary>A reverse-proxy route from a gateway config file (ocelot.json or YARP).</summary>
public sealed record GatewayRoute(
    string UpstreamTemplate,
    string UpstreamMethods,
    string DownstreamTemplate,
    string DownstreamHosts
);
