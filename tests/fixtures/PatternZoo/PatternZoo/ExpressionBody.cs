using MediatR;

namespace PatternZoo;

public class ExpressionHandler
{
    private readonly IMediator _m;
    public ExpressionHandler(IMediator m) => _m = m;
    public Task Run() => _m.Send(new RunCommand());
}

public record RunCommand : IRequest<bool>;
