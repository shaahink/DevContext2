using MediatR;

namespace PatternZoo;

public record RecordHandler(IMediator Mediator)
{
    public Task Execute() => Mediator.Send(new RecordCommand());
}

public record RecordCommand : IRequest<bool>;
