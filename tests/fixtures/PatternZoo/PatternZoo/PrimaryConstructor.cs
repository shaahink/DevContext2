using MediatR;

namespace PatternZoo;

public class PrimaryCtorHandler(IMediator mediator)
{
    public Task Run() => mediator.Send(new PingCommand());
}

public record PingCommand : IRequest<bool>;
