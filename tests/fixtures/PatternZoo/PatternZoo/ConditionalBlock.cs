using MediatR;

namespace PatternZoo;

public class ConditionalHandler
{
    private readonly IMediator _m;
    public ConditionalHandler(IMediator m) => _m = m;

    public Task Run()
    {
#if DEBUG
        return _m.Send(new DebugCommand());
#else
        return Task.CompletedTask;
#endif
    }
}

public record DebugCommand : IRequest<bool>;
