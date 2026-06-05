using MediatR;
using Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

app.MapGet("/orders", async (IMediator mediator) =>
{
    var orders = await mediator.Send(new GetOrdersQuery());
    return Results.Ok(orders);
});

app.MapPost("/orders", async (CreateOrderCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/orders/{id}", id);
});

app.Run();

public record GetOrdersQuery() : IRequest<List<Order>>;
public record CreateOrderCommand(string ProductName, int Quantity) : IRequest<int>;
