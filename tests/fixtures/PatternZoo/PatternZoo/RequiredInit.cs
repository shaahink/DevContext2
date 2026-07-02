using MediatR;

namespace PatternZoo;

public class RequiredInitHandler
{
    public required IMediator Mediator { get; init; }

    public Task Run() => Mediator.Send(new InitCommand());
}

public record InitCommand : IRequest<bool>;
