using Api.Services;

namespace Api.Rpc;

/// <summary>A gRPC service impl (extends the generated <c>GreeterBase</c>). Its RPC delegates to an
/// injected domain service — the callee the seeded Map-mode call graph must resolve as the entry
/// target (T1.1).</summary>
public sealed class GreeterService : Greeter.GreeterBase
{
    private readonly IGreetingService _greetings;

    public GreeterService(IGreetingService greetings) => _greetings = greetings;

    public override Task<HelloReply> SayHello(HelloRequest request)
        => Task.FromResult(new HelloReply(Normalize(_greetings.BuildGreeting(request.Name))));

    // T1.7 — a private helper is NOT a proto RPC: it must never surface as a gRPC entry
    // (mirrors eShop BasketService.MapToCustomerBasket*). Only `public override` RPCs are entries.
    private static string Normalize(string s) => s.Trim();
}
