namespace Api.Rpc;

// Hand-stub of the code protoc would generate from greet.proto, so the fixture is self-contained.
// The gRPC detector keys on a base type whose name ends in "Base" — the real generated shape.
public static class Greeter
{
    public abstract class GreeterBase
    {
        public virtual Task<HelloReply> SayHello(HelloRequest request)
            => Task.FromResult(new HelloReply(string.Empty));
    }
}

public sealed record HelloRequest(string Name);
public sealed record HelloReply(string Message);
