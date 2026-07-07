using DevContext.Core.Graph;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;

using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Tests.Graph2;

/// <summary>L2.2 — seam detectors over BodyFacts, exercised with verbatim dogfood snippets
/// (run-aspnetcore-microservices). The headline is <see cref="Checkout_spine_seams_detected_end_to_end"/>:
/// it proves the E1 flow (<c>request.Adapt&lt;CheckoutBasketCommand&gt;()</c> + <c>sender.Send(command)</c>,
/// <c>publishEndpoint.Publish(eventMessage)</c>) is detected by construction — the foundation L2.4 needs.</summary>
public sealed class SeamDetectorTests
{
    private static ImmutableArray<BodyFacts> Facts(string code, string project = "Test.Proj")
        => BodyFactExtractor.Extract(CSharpSyntaxTree.ParseText(code), "/src/Test.cs", project);

    private static List<SeamMatch> Detect(ISeamDetector detector, string code, SeamContext? ctx = null)
        => Facts(code).SelectMany(b => detector.Detect(b, ctx ?? SeamContext.Empty)).ToList();

    // ── Verbatim dogfood snippets (structure preserved from the real source) ─────────────────

    private const string CheckoutEndpoint = """
        namespace Basket.API.Basket.CheckoutBasket;
        public record CheckoutBasketRequest(BasketCheckoutDto BasketCheckoutDto);
        public record CheckoutBasketResponse(bool IsSuccess);
        public class CheckoutBasketEndpoints : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ISender sender) =>
                {
                    var command = request.Adapt<CheckoutBasketCommand>();
                    var result = await sender.Send(command);
                    var response = result.Adapt<CheckoutBasketResponse>();
                    return Results.Ok(response);
                });
            }
        }
        """;

    private const string CheckoutHandler = """
        namespace Basket.API.Basket.CheckoutBasket;
        public class CheckoutBasketCommandHandler
            (IBasketRepository repository, IPublishEndpoint publishEndpoint)
            : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
        {
            public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
            {
                var basket = await repository.GetBasket(command.BasketCheckoutDto.UserName, cancellationToken);
                if (basket == null) { return new CheckoutBasketResult(false); }
                var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
                eventMessage.TotalPrice = basket.TotalPrice;
                await publishEndpoint.Publish(eventMessage, cancellationToken);
                await repository.DeleteBasket(command.BasketCheckoutDto.UserName, cancellationToken);
                return new CheckoutBasketResult(true);
            }
        }
        """;

    private const string CheckoutConsumer = """
        namespace Ordering.Application.Orders.EventHandlers.Integration;
        public class BasketCheckoutEventHandler
            (ISender sender, ILogger<BasketCheckoutEventHandler> logger)
            : IConsumer<BasketCheckoutEvent>
        {
            public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
            {
                logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);
                var command = MapToCreateOrderCommand(context.Message);
                await sender.Send(command);
            }
            private CreateOrderCommand MapToCreateOrderCommand(BasketCheckoutEvent message)
            {
                return new CreateOrderCommand(new OrderDto());
            }
        }
        """;

    // ── Per-detector fixtures ────────────────────────────────────────────────────────────────

    [Fact]
    public void MediatRDispatch_detects_send_of_adapted_command()
    {
        var seams = Detect(new MediatRDispatchDetector(), CheckoutEndpoint);
        var send = Assert.Single(seams);
        Assert.Equal(EdgeKind.Sends, send.Kind);
        Assert.Equal("CheckoutBasketCommand", send.Target.Text);
        Assert.Equal("MediatRDispatch", send.DetectorId);
        Assert.Contains(":", send.Provenance); // file:line anchored
    }

    [Fact]
    public void MediatRDispatch_does_not_fire_for_bus_publish_receiver()
    {
        // publishEndpoint.Publish(...) is a MassTransit bus receiver — MediatR must not claim it.
        var seams = Detect(new MediatRDispatchDetector(), CheckoutHandler);
        Assert.Empty(seams);
    }

    [Fact]
    public void BusPublish_detects_publish_of_integration_event()
    {
        var seams = Detect(new BusPublishDetector(), CheckoutHandler);
        var raise = Assert.Single(seams);
        Assert.Equal(EdgeKind.Raises, raise.Kind);
        Assert.Equal("BasketCheckoutEvent", raise.Target.Text);
        Assert.Equal("BusPublish", raise.DetectorId);
    }

    [Fact]
    public void IntegrationEventCreation_detects_adapted_event_message()
    {
        var ctx = new SeamContext { IntegrationEventTypes = ["BasketCheckoutEvent"] };
        var seams = Detect(new IntegrationEventCreationDetector(), CheckoutHandler, ctx);
        Assert.Contains(seams, s => s.Target.Text == "BasketCheckoutEvent" && s.Kind == EdgeKind.Raises);
    }

    [Fact]
    public void DomainEventRaise_detects_add_domain_event()
    {
        // Verbatim eShop Order aggregate pattern.
        const string aggregate = """
            namespace Ordering.Domain.Models;
            public class Order
            {
                public static Order Create(OrderId id)
                {
                    var order = new Order();
                    order.AddDomainEvent(new OrderCreatedEvent(order));
                    return order;
                }
                public void AddDomainEvent(IDomainEvent domainEvent) { }
            }
            """;
        var seams = Detect(new DomainEventRaiseDetector(), aggregate);
        var raise = Assert.Single(seams);
        Assert.Equal(EdgeKind.Raises, raise.Kind);
        Assert.Equal("OrderCreatedEvent", raise.Target.Text);
    }

    [Fact]
    public void EntityTouch_detects_reference_to_known_entity()
    {
        // Verbatim CreateOrderHandler — references the Order entity (Order.Create / dbContext.Orders).
        const string handler = """
            namespace Ordering.Application.Orders.Commands.CreateOrder;
            public class CreateOrderHandler(IApplicationDbContext dbContext)
                : ICommandHandler<CreateOrderCommand, CreateOrderResult>
            {
                public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
                {
                    var order = CreateNewOrder(command.Order);
                    dbContext.Orders.Add(order);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return new CreateOrderResult(order.Id.Value);
                }
                private static Order CreateNewOrder(OrderDto orderDto) => Order.Create(OrderId.Of(System.Guid.NewGuid()));
            }
            """;
        var ctx = new SeamContext { KnownEntities = ["Order"] };
        var seams = Detect(new EntityTouchDetector(), handler, ctx);
        Assert.Contains(seams, s => s.Kind == EdgeKind.ReadsWrites && s.Target.Text == "Order");
    }

    // ── Headline: the checkout spine, detected by construction (L2.4 foundation) ─────────────

    [Fact]
    public void Checkout_spine_seams_detected_end_to_end()
    {
        var detectors = new ISeamDetector[]
        {
            new MediatRDispatchDetector(),
            new BusPublishDetector(),
            new IntegrationEventCreationDetector(),
            new DomainEventRaiseDetector(),
            new EntityTouchDetector(),
        };
        var ctx = new SeamContext { IntegrationEventTypes = ["BasketCheckoutEvent"] };

        var seams = new List<SeamMatch>();
        foreach (var code in new[] { CheckoutEndpoint, CheckoutHandler, CheckoutConsumer })
            foreach (var body in Facts(code))
                foreach (var d in detectors)
                    seams.AddRange(d.Detect(body, ctx));

        // 1) Endpoint dispatches the checkout command (E1 pattern: Adapt<T> + sender.Send).
        Assert.Contains(seams, s => s.Kind == EdgeKind.Sends && s.Target.Text == "CheckoutBasketCommand");
        // 2) Handler publishes the integration event onto the bus (cross-service hop origin).
        Assert.Contains(seams, s => s.Kind == EdgeKind.Raises && s.Target.Text == "BasketCheckoutEvent"
            && s.DetectorId == "BusPublish");
        // 3) Consumer dispatches the create-order command (resolved via same-type method return type).
        Assert.Contains(seams, s => s.Kind == EdgeKind.Sends && s.Target.Text == "CreateOrderCommand");
    }

    [Fact]
    public void Detectors_leave_target_unresolved_when_not_statically_obvious()
    {
        // The dispatched value comes from an undeclared factory call — its type is not a Tier-A fact,
        // so no seam is fabricated (Law R1: no silent winners; TierB may resolve it later).
        const string code = """
            namespace N; class C { void M(ISender sender) { sender.Send(GetCommand()); } }
            """;
        var seams = Detect(new MediatRDispatchDetector(), code);
        Assert.Empty(seams);
    }

    [Fact]
    public void Detector_failure_is_isolated_and_does_not_crash_assembly()
    {
        var model = new DiscoveryModel
        {
            Projects = [new("Scratch", @"C:\repo\Scratch\Scratch.csproj", "C#", ["net10.0"], [], [])],
        };
        var filePath = @"C:\repo\Scratch\Handler.cs";
        model.Types.TryAdd("Scratch.Handler", new TypeDiscovery
        {
            Id = "Scratch.Handler",
            Name = "Handler",
            Namespace = "Scratch",
            FilePath = filePath,
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = "public class Handler { public void Run(IMediator m) { m.Send(new Ping()); } }",
        });
        var scope = SolutionScope.FromModel(model);

        var wrappedSource = "namespace Scratch { public class Handler { public void Run(IMediator m) { m.Send(new Ping()); } } }";
        var tree = CSharpSyntaxTree.ParseText(wrappedSource, new CSharpParseOptions().WithPreprocessorSymbols("DEBUG"), filePath);
        var facts = BodyFactExtractor.Extract(tree, filePath, "Scratch");

        var builder = new GraphBuilder(new SyntacticSymbolResolver(), new NoiseFilter(new ProjectClassifier(model.Projects)));
        var (result, _) = builder.Build(model, scope, facts);
        Assert.True(result.NodeCount > 0);
    }

    [Fact]
    public void Auto_extract_fallback_handles_stripped_type_body()
    {
        // When BodyFacts are not pre-extracted (null), the auto-extract path wraps
        // a SourceBody that has no class/struct/record declaration. The triple-brace
        // fix (L2 audit) ensures the synthetic source is valid C#.
        var model = new DiscoveryModel
        {
            Projects = [new("Scratch", @"C:\repo\Scratch\Scratch.csproj", "C#", ["net10.0"], [], [])],
        };
        var filePath = @"C:\repo\Scratch\Handler.cs";
        model.Types.TryAdd("Scratch.Handler", new TypeDiscovery
        {
            Id = "Scratch.Handler",
            Name = "Handler",
            Namespace = "Scratch",
            FilePath = filePath,
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            // Body has no class/struct/record keyword — simulates the stripped-body path.
            SourceBody = "public void Run() { var c = new CreateOrderCommand(1); }",
        });

        var scope = SolutionScope.FromModel(model);
        var builder = new GraphBuilder(new SyntacticSymbolResolver(), new NoiseFilter(new ProjectClassifier(model.Projects)));
        var (result, _) = builder.Build(model, scope, bodyFacts: null);
        Assert.True(result.NodeCount > 0);
    }
}
