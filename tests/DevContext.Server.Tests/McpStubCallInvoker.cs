using Grpc.Core;

namespace DevContext.Server.Tests;

/// <summary>
/// A <see cref="CallInvoker"/> that answers every gRPC call from a script keyed on the method name.
///
/// The generated <c>DevContextServiceClient</c> takes a CallInvoker, so ONE of these covers every
/// RPC the MCP proxy can make — including RPCs added later. That matters: the defect these tests
/// pin (a tool that lets a raw RpcException past its error envelope) is a per-method omission, and
/// a per-method stub would have the same drift problem as the code it is testing.
/// </summary>
internal sealed class McpStubCallInvoker : CallInvoker
{
    private readonly Func<string, object?> _respond;
    private readonly Action<string, object>? _observe;

    /// <param name="respond">
    /// Called with the bare RPC name (e.g. "GetStats"). Return a response message to answer with,
    /// null to answer with an empty message of the right type, or throw to fail the call.
    /// </param>
    /// <param name="observe">
    /// Called with the RPC name and the REQUEST message before responding. Some defects are on the
    /// outbound side — R4 item 6 was a filter the MCP applied to the response instead of putting it
    /// on the request — and those are only visible from here.
    /// </param>
    internal McpStubCallInvoker(Func<string, object?> respond, Action<string, object>? observe = null)
    {
        _respond = respond;
        _observe = observe;
    }

    /// <summary>Every call fails with the given status — the "server is unreachable" shape.</summary>
    internal static McpStubCallInvoker FailAll(StatusCode code, string detail)
        => new(_ => throw new RpcException(new Status(code, detail)));

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        => Respond<TResponse>(method.Name, request);

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
    {
        Task<TResponse> result;
        try { result = Task.FromResult(Respond<TResponse>(method.Name, request)); }
        catch (RpcException ex) { result = Task.FromException<TResponse>(ex); }
        return new AsyncUnaryCall<TResponse>(
            result, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
    {
        IAsyncStreamReader<TResponse> reader;
        try { reader = new OneShotReader<TResponse>(Respond<TResponse>(method.Name, request)); }
        catch (RpcException ex) { reader = new OneShotReader<TResponse>(ex); }
        return new AsyncServerStreamingCall<TResponse>(
            reader, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options)
        => throw new NotSupportedException("No client-streaming RPC on this service.");

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options)
        => throw new NotSupportedException("No duplex-streaming RPC on this service.");

    private TResponse Respond<TResponse>(string method, object request) where TResponse : class
    {
        _observe?.Invoke(method, request);
        return _respond(method) as TResponse ?? Activator.CreateInstance<TResponse>();
    }

    /// <summary>A stream that yields one message, or fails on the first read.</summary>
    private sealed class OneShotReader<T> : IAsyncStreamReader<T>
    {
        private readonly T? _one;
        private readonly RpcException? _failure;
        private bool _read;

        internal OneShotReader(T one) => _one = one;
        internal OneShotReader(RpcException failure) => _failure = failure;

        public T Current => _one!;

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (_failure is not null) return Task.FromException<bool>(_failure);
            if (_read) return Task.FromResult(false);
            _read = true;
            return Task.FromResult(true);
        }
    }
}
