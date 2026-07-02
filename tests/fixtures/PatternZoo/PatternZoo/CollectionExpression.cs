using MediatR;

namespace PatternZoo;

public class CollectionExpressionHandler
{
    private readonly IMediator _m;
    public CollectionExpressionHandler(IMediator m) => _m = m;

    public Task Run()
    {
        string[] tags = ["urgent", "batch"];
        return _m.Send(new CollectionCommand());
    }
}

public record CollectionCommand : IRequest<bool>;
