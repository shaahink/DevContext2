using MediatR;

namespace PatternZoo;

public class RawStringTrapHandler
{
    private readonly IMediator _m;
    public RawStringTrapHandler(IMediator m) => _m = m;

    public Task Run()
    {
        var sql = """
            SELECT * FROM Commands
            WHERE Handler.Send(new FakeCommand()) IS NOT NULL
            """;
        return _m.Send(new RealCommand());
    }
}

public record RealCommand : IRequest<bool>;
public record FakeCommand : IRequest<bool>;
