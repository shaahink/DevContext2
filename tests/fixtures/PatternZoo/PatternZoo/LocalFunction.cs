using MediatR;

namespace PatternZoo;

public class LocalFunctionHandler
{
    private readonly IMediator _m;
    public LocalFunctionHandler(IMediator m) => _m = m;

    public Task Run()
    {
        return Dispatch();

        Task Dispatch() => _m.Send(new LocalCommand());
    }
}

public record LocalCommand : IRequest<bool>;
