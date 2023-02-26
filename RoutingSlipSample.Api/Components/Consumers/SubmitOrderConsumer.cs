using MassTransit;
using MassTransit.Courier.Contracts;
using RoutingSlipSample.Api.Components.Activities.DressBurger;
using RoutingSlipSample.Api.Components.Activities.GrillBurger;
using RoutingSlipSample.Api.Models;
using RoutingSlipSample.Contracts;

namespace RoutingSlipSample.Api.Components.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;
    private readonly IEndpointNameFormatter _formatter;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger, IEndpointNameFormatter formatter)
    {
        _logger = logger;
        _formatter = formatter;
    }
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        _logger.LogDebug("Order Submission Received: {OrderId} {CorrelationId}", context.Message.OrderId, context.CorrelationId);

        await Task.Delay(100);


        var routingSlip = CreateRoutingSlip(context.Message);
        await context.Execute(routingSlip);
        await context.RespondAsync<OrderSubmissionAccepted>(new { context.Message.OrderId });

    }


    RoutingSlip CreateRoutingSlip(SubmitOrder submitOrder)
    {
        var builder = new RoutingSlipBuilder(Guid.NewGuid());
        builder.AddVariable("OrderId", submitOrder.OrderId);
        var grillQueueName = _formatter.ExecuteActivity<GrillBurgerActivity, IGrillBurgerArguments>();
        builder.AddActivity("grill-burger", new Uri($"queue:{grillQueueName}"), new
        {
            Weight = 0.5m,
            WeightTemperature = 165
        });
        var dressQueueName = _formatter.ExecuteActivity<DressBurgerActivity, IDressBurgerArguments>();
        builder.AddActivity("dress-burger", new Uri($"queue:{dressQueueName}"), new
        {
            Pickles = true,
            Lettuce = true
        });
        return builder.Build();
    }
}