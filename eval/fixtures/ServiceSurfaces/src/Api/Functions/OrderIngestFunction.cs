using Api.Services;

using Microsoft.Azure.Functions.Worker;

namespace Api.Functions;

/// <summary>An Azure Functions worker entry. The trigger method delegates to an injected domain
/// service — the callee the seeded Map-mode call graph must reach (T1.1).</summary>
public sealed class OrderIngestFunction
{
    private readonly IOrderIngestService _orders;

    public OrderIngestFunction(IOrderIngestService orders) => _orders = orders;

    [Function("IngestOrder")]
    public Task Run([QueueTrigger("orders")] string payload)
        => _orders.IngestOrder(payload);
}
